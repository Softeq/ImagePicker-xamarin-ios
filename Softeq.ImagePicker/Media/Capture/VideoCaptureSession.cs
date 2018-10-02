using System;
using AVFoundation;
using CoreFoundation;
using Foundation;
using Softeq.ImagePicker.Infrastructure.Enums;
using Softeq.ImagePicker.Infrastructure.Interfaces;
using Softeq.ImagePicker.Media.Delegates;

namespace Softeq.ImagePicker.Media.Capture
{
    public class VideoCaptureSession
    {
        private AVCaptureMovieFileOutput _videoFileOutput;
        private readonly ICaptureSessionVideoRecordingDelegate _videoRecordingDelegate;
        private VideoCaptureDelegate _videoCaptureDelegate;
        private AudioCaptureSession _audioCaptureSession;
        private readonly VideoDeviceInputManager _videoDeviceInputManager;
        private readonly DispatchQueue _sessionQueue;

        public bool IsRecordingVideo => _videoFileOutput?.Recording ?? false;

        public VideoCaptureSession(Action<NSNotification> action,
            ICaptureSessionVideoRecordingDelegate videoRecordingDelegate, DispatchQueue queue)
        {
            _videoRecordingDelegate = videoRecordingDelegate;
            _videoDeviceInputManager = new VideoDeviceInputManager(action);
            _sessionQueue = queue;
        }

        public SessionSetupResult ConfigureSession(AVCaptureSession session)
        {
            var inputDeviceConfigureResult = _videoDeviceInputManager.ConfigureVideoDeviceInput(session);

            if (inputDeviceConfigureResult != SessionSetupResult.Success)
            {
                return inputDeviceConfigureResult;
            }

            // Add movie file output.
            Console.WriteLine("capture session: configuring - adding movie file input");

            var movieFileOutput = new AVCaptureMovieFileOutput();
            if (session.CanAddOutput(movieFileOutput))
            {
                session.AddOutput(movieFileOutput);
                _videoFileOutput = movieFileOutput;

                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    _videoRecordingDelegate?.DidBecomeReadyForVideoRecording(this);
                });
            }
            else
            {
                Console.WriteLine("capture session: could not add video output to the session");
                return SessionSetupResult.ConfigurationFailed;
            }

            _audioCaptureSession = new AudioCaptureSession();
            _audioCaptureSession.ConfigureSession(session);

            return SessionSetupResult.Success;
        }

        public void StartVideoRecording(bool shouldSaveVideoToLibrary)
        {
            if (_videoFileOutput == null)
            {
                Console.WriteLine("capture session: trying to record a video but no movie file output is set");
                return;
            }

            _sessionQueue.DispatchAsync(() =>
            {
                // if already recording do nothing
                if (_videoFileOutput.Recording)
                {
                    Console.WriteLine(
                        "capture session: trying to record a video but there is one already being recorded");
                    return;
                }

                // start recording to a temporary file.
                var outputFileName = new NSUuid().AsString();

                var outputUrl = NSFileManager.DefaultManager.GetTemporaryDirectory().Append(outputFileName, false)
                    .AppendPathExtension("mov");

                var recordingDelegate = new VideoCaptureDelegate(DidStartCaptureAction,
                    captureDelegate => DidFinishCaptureAction(captureDelegate, outputUrl),
                    (captureDelegate, error) => DidCaptureFail(captureDelegate, error, outputUrl))
                {
                    ShouldSaveVideoToLibrary = shouldSaveVideoToLibrary
                };

                _videoFileOutput.StartRecordingToOutputFile(outputUrl, recordingDelegate);

                _videoCaptureDelegate = recordingDelegate;
            });
        }

        private void DidCaptureFail(VideoCaptureDelegate captureDelegate, NSError error, NSUrl outputUrl)
        {
            // we need to remove reference to the delegate so it can be deallocated
            _videoCaptureDelegate = null;

            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                if (captureDelegate.RecordingWasInterrupted)
                {
                    _videoRecordingDelegate?.DidInterruptVideoRecording(this, outputUrl, error);
                }
                else
                {
                    _videoRecordingDelegate?.DidFailVideoRecording(this, error);
                }
            });
        }

        private void DidStartCaptureAction()
        {
            DispatchQueue.MainQueue.DispatchAsync(() => { _videoRecordingDelegate?.DidStartVideoRecording(this); });
        }

        private void DidFinishCaptureAction(VideoCaptureDelegate captureDelegate, NSUrl outputUrl)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                if (captureDelegate?.IsBeingCancelled == true)
                {
                    _videoRecordingDelegate?.DidCancelVideoRecording(this);
                }
                else
                {
                    _videoRecordingDelegate?.DidFinishVideoRecording(this, outputUrl);
                }
            });
        }

        ///
        /// If there is any recording in progress it will be stopped.
        ///
        /// - parameter cancel: if true, recorded file will be deleted and corresponding delegate method will be called.
        ///
        public void StopVideoRecording(bool cancel = false)
        {
            if (_videoFileOutput == null)
            {
                Console.WriteLine("capture session: trying to stop a video recording but no movie file output is set");
                return;
            }

            _sessionQueue.DispatchAsync(() =>
            {
                if (_videoFileOutput.Recording == false)
                {
                    Console.WriteLine(
                        "capture session: trying to stop a video recording but no recording is in progress");
                    return;
                }

                if (_videoCaptureDelegate == null)
                {
                    throw new Exception(
                        "capture session: trying to stop a video recording but video capture delegate is nil");
                }

                _videoCaptureDelegate.IsBeingCancelled = cancel;
                _videoFileOutput.StopRecording();
            });
        }

        public void ChangeCamera(AVCaptureSession session)
        {
            _videoDeviceInputManager.ConfigureVideoDeviceInput(session);

            var connection = _videoFileOutput?.ConnectionFromMediaType(AVMediaType.Video);

            if (connection?.SupportsVideoStabilization == true)
            {
                connection.PreferredVideoStabilizationMode = AVCaptureVideoStabilizationMode.Auto;
            }
        }
    }
}