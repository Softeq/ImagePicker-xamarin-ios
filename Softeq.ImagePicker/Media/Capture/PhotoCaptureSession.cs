using System;
using System.IO;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using Foundation;
using Softeq.ImagePicker.Enums;
using Softeq.ImagePicker.Interfaces;
using Softeq.ImagePicker.Media.Delegates;
using UIKit;

namespace Softeq.ImagePicker.Media.Capture
{
    public class PhotoCaptureSession
    {
        private readonly AVCapturePhotoOutput _photoOutput = new AVCapturePhotoOutput();
        private AVCaptureVideoDataOutput _videoDataOutput;
        private readonly DispatchQueue _sessionQueue;
        private SessionPresetConfiguration _presetConfiguration;
        private readonly VideoDeviceInputManager _videoDeviceInputManager;

        /// contains number of currently processing live photos
        public int InProgressLivePhotoCapturesCount;

        private AudioCaptureSession _audioCaptureSession;

        private readonly ISessionPhotoCapturingDelegate _photoCapturingDelegate;

        public PhotoCaptureSession(Action<NSNotification> sessionRuntimeErrorHandler,
            ISessionPhotoCapturingDelegate photoCapturingDelegate, DispatchQueue queue)
        {
            _photoCapturingDelegate = photoCapturingDelegate;
            _sessionQueue = queue;

            _videoDeviceInputManager = new VideoDeviceInputManager(sessionRuntimeErrorHandler);
        }

        public SessionSetupResult ConfigureSession(AVCaptureSession session,
            SessionPresetConfiguration presetConfiguration)
        {
            _presetConfiguration = presetConfiguration;
            session.SessionPreset = AVCaptureSession.PresetPhoto;

            var inputDeviceConfigureResult = _videoDeviceInputManager.ConfigureVideoDeviceInput(session);

            if (inputDeviceConfigureResult != SessionSetupResult.Success)
            {
                return inputDeviceConfigureResult;
            }

            if (!session.CanAddOutput(_photoOutput))
            {
                Console.WriteLine("capture session: could not add photo output to the session");
                return SessionSetupResult.ConfigurationFailed;
            }

            session.AddOutput(_photoOutput);
            _photoOutput.IsHighResolutionCaptureEnabled = true;

            ConfigureLivePhoto(session);

            _videoDataOutput = new AVCaptureVideoDataOutput();
            if (session.CanAddOutput(_videoDataOutput))
            {
                _videoDataOutput.AlwaysDiscardsLateVideoFrames = true;

                session.AddOutput(_videoDataOutput);
            }
            else
            {
                Console.WriteLine("capture session: warning - could not add video data output to the session");
            }

            return SessionSetupResult.Success;
        }

        public void ChangeCamera(AVCaptureSession session, SessionPresetConfiguration presetConfiguration)
        {
            _videoDeviceInputManager.ConfigureVideoDeviceInput(session);

            _photoOutput.IsLivePhotoCaptureEnabled = _photoOutput.IsLivePhotoCaptureSupported &&
                                                     presetConfiguration == SessionPresetConfiguration.LivePhotos;
        }

        public void CapturePhoto(LivePhotoMode livePhotoMode, bool saveToPhotoLibrary)
        {
            _sessionQueue.DispatchAsync(() =>
            {
                var photoSettings = AVCapturePhotoSettings.Create();

                if (_photoOutput.SupportedFlashModes.Contains(NSNumber.FromInt32((int) AVCaptureFlashMode.Auto)))
                {
                    photoSettings.FlashMode = AVCaptureFlashMode.Auto;
                }

                photoSettings.IsHighResolutionPhotoEnabled = true;

                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0) &&
                    photoSettings.AvailableEmbeddedThumbnailPhotoCodecTypes.Length > 0)
                {
                    photoSettings.EmbeddedThumbnailPhotoFormat = new NSMutableDictionary
                    {
                        {
                            AVVideo.CodecKey,
                            photoSettings.AvailableEmbeddedThumbnailPhotoCodecTypes[0].GetConstant()
                        }
                    };
                }

                if (livePhotoMode == LivePhotoMode.On)
                {
                    if (_presetConfiguration == SessionPresetConfiguration.LivePhotos &&
                        _photoOutput.IsLivePhotoCaptureSupported)
                    {
                        photoSettings.LivePhotoMovieFileUrl =
                            NSUrl.CreateFileUrl(new[] {Path.GetTempPath(), $"{Guid.NewGuid()}.mov"});
                    }
                    else
                    {
                        Console.WriteLine(
                            "capture session: warning - trying to capture live photo but it's not supported by current configuration, capturing regular photo instead");
                    }
                }

                // Use a separate object for the photo capture delegate to isolate each capture life cycle.
                var photoCaptureDelegate = new PhotoCaptureDelegate(photoSettings,
                        () => WillCapturePhotoAnimationAction(photoSettings),
                        CapturingLivePhotoAction, CapturingCompletedAction)
                    {ShouldSavePhotoToLibrary = saveToPhotoLibrary};

                _photoOutput.CapturePhoto(photoSettings, photoCaptureDelegate);
            });
        }

        private void CapturingCompletedAction(PhotoCaptureDelegate photoDelegate)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                if (photoDelegate.PhotoData != null)
                {
                    _photoCapturingDelegate?.DidCapturePhotoData(this, photoDelegate.PhotoData,
                        photoDelegate.RequestedPhotoSettings);
                }
                else if (photoDelegate.ProcessError != null)
                {
                    _photoCapturingDelegate?.DidFailCapturingPhotoWith(this,
                        photoDelegate.ProcessError);
                }
            });
        }

        private void CapturingLivePhotoAction(bool capturing)
        {
            _sessionQueue.DispatchAsync(() =>
            {
                InProgressLivePhotoCapturesCount += capturing ? 1 : -1;

                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    if (InProgressLivePhotoCapturesCount >= 0)
                    {
                        _photoCapturingDelegate?.CaptureSessionDidChangeNumberOfProcessingLivePhotos(this);
                    }
                    else
                    {
                        Console.WriteLine(
                            "capture session: error - in progress live photo capture count is less than 0");
                    }
                });
            });
        }

        private void WillCapturePhotoAnimationAction(AVCapturePhotoSettings photoSettings)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                _photoCapturingDelegate.WillCapturePhotoWith(this, photoSettings);
            });
        }

        private void ConfigureLivePhoto(AVCaptureSession session)
        {
            //enable live photos only if we intend to use it explicitly
            if (_presetConfiguration != SessionPresetConfiguration.LivePhotos)
            {
                return;
            }

            _photoOutput.IsLivePhotoCaptureEnabled = _photoOutput.IsLivePhotoCaptureSupported;
            if (_photoOutput.IsLivePhotoCaptureSupported)
            {
                _audioCaptureSession = new AudioCaptureSession();
                _audioCaptureSession.ConfigureSession(session);
            }
            else
            {
                Console.WriteLine(
                    "capture session: configuring - requested live photo mode is not supported by the device");
            }
        }
    }
}