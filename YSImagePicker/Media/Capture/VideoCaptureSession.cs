using System;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using Foundation;

namespace YSImagePicker.Media.Capture
{
    public class VideoCaptureSession
    {
        private AVCaptureMovieFileOutput _videoFileOutput;
        private readonly DispatchQueue _sessionQueue = new DispatchQueue("video session queue");
        private AVCaptureDeviceInput _videoDeviceInput;
        private readonly ICaptureSessionVideoRecordingDelegate _videoRecordingDelegate;
        private VideoCaptureDelegate _videoCaptureDelegate;

        public bool IsReadyForVideoRecording => _videoFileOutput != null;
        private NSObject _runtimeErrorNotification;
        public bool IsRecordingVideo => _videoFileOutput?.Recording ?? false;

        private readonly AVCaptureDeviceDiscoverySession _videoDeviceDiscoverySession =
            AVCaptureDeviceDiscoverySession.Create(new[]
                {
                    AVCaptureDeviceType.BuiltInWideAngleCamera,
                    AVCaptureDeviceType.BuiltInDuoCamera
                },
                AVMediaType.Video, AVCaptureDevicePosition.Unspecified);

        public VideoCaptureSession(ICaptureSessionVideoRecordingDelegate videoRecordingDelegate)
        {
            _videoRecordingDelegate = videoRecordingDelegate;
        }

        ///
        /// Configures a session before it can be used, following steps are done:
        /// 1. adds video input
        /// 2. adds video output (for recording videos)
        /// 3. adds audio input (for video recording with audio)
        /// 4. adds photo output (for capturing photos)capture session: trying to record a video but no preview layer is set
        ///
        public SessionSetupResult ConfigureSession(AVCaptureSession session)
        {
            // Choose the back dual camera if available, otherwise default to a wide angle camera.
            if (!TryGetDefaultDevice(out var defaultVideoDevice))
            {
                Console.WriteLine("capture session: could not create capture device");
                return SessionSetupResult.ConfigurationFailed;
            }

            var videoDeviceInput = new AVCaptureDeviceInput(defaultVideoDevice, out var error);

            if (error != null)
            {
                Console.WriteLine($"Error accrued {error}");
            }

            if (session.CanAddInput(videoDeviceInput))
            {
                session.AddInput(videoDeviceInput);
                _videoDeviceInput = videoDeviceInput;
            }
            else
            {
                Console.WriteLine("capture session: could not add video device input to the session");
                return SessionSetupResult.ConfigurationFailed;
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

            return SessionSetupResult.Success;
        }

        public void StartVideoRecording(bool saveToPhotoLibrary)
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

                var recordingDelegate = new VideoCaptureDelegate(
                        () =>
                        {
                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            {
                                _videoRecordingDelegate?.DidStartVideoRecording(this);
                            });
                        }, captureDelegate =>
                        {
                            // we need to remove reference to the delegate so it can be deallocated
                            _sessionQueue.DispatchAsync(() => { _videoCaptureDelegate = null; });

                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            {
                                if (captureDelegate.IsBeingCancelled)
                                {
                                    _videoRecordingDelegate?.DidCancelVideoRecording(this);
                                }
                                else
                                {
                                    _videoRecordingDelegate?.DidFinishVideoRecording(this, outputUrl);
                                }
                            });
                        }, (captureDelegate, error) =>
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
                        })
                { SavesVideoToLibrary = saveToPhotoLibrary };

                _videoFileOutput.StartRecordingToOutputFile(outputUrl, recordingDelegate);

                _videoCaptureDelegate = recordingDelegate;
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
            AVCaptureDevicePosition preferredPosition;
            AVCaptureDeviceType preferredDeviceType;

            _sessionQueue.DispatchAsync(() =>
            {
                var currentVideoDevice = _videoDeviceInput.Device;
                var currentPosition = currentVideoDevice.Position;

                switch (currentPosition)
                {
                    case AVCaptureDevicePosition.Unspecified:
                    case AVCaptureDevicePosition.Front:
                        preferredPosition = AVCaptureDevicePosition.Back;
                        preferredDeviceType = AVCaptureDeviceType.BuiltInDuoCamera;
                        break;
                    case AVCaptureDevicePosition.Back:
                        preferredPosition = AVCaptureDevicePosition.Front;
                        preferredDeviceType = AVCaptureDeviceType.BuiltInWideAngleCamera;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var devices = _videoDeviceDiscoverySession.Devices;

                // First, look for a device with both the preferred position and device type. Otherwise, look for a device with only the preferred position.
                var videoDevice = devices.FirstOrDefault(x =>
                    x.Position == preferredPosition && x.DeviceType == preferredDeviceType);

                if (videoDevice == null)
                {
                    videoDevice = devices.FirstOrDefault(x => x.Position == preferredPosition);
                }

                if (videoDevice == null)
                {
                    return;
                }

                var videoDeviceInput = new AVCaptureDeviceInput(videoDevice, out var error);

                if (error != null)
                {
                    Console.WriteLine($"Error occured while creating video device input: {error}");
                    return;
                }

                // Remove the existing device input first, since using the front and back camera simultaneously is not supported.
                session.RemoveInput(_videoDeviceInput);

                if (session.CanAddInput(videoDeviceInput))
                {
                    session.AddInput(videoDeviceInput);
                    _videoDeviceInput = videoDeviceInput;
                }
                else
                {
                    session.AddInput(_videoDeviceInput);
                }

                var connection = _videoFileOutput?.ConnectionFromMediaType(AVMediaType.Video);
                if (connection != null)
                {
                    if (connection.SupportsVideoStabilization)
                    {
                        connection.PreferredVideoStabilizationMode = AVCaptureVideoStabilizationMode.Auto;
                    }
                }
            });
        }

        public void AddObservers()
        {
            _runtimeErrorNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                AVCaptureSession.RuntimeErrorNotification,
                SessionRuntimeError, _videoDeviceInput.Device);
        }

        public void RemoveObservers()
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_runtimeErrorNotification);
        }

        private static bool TryGetDefaultDevice(out AVCaptureDevice device)
        {
            device = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInDualCamera, AVMediaType.Video,
                AVCaptureDevicePosition.Back);

            if (device != null)
            {
                return true;
            }

            device = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaType.Video,
                AVCaptureDevicePosition.Back);

            if (device != null)
            {
                return true;
            }

            device = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaType.Video,
                AVCaptureDevicePosition.Front);

            if (device != null)
            {
                return true;
            }

            return false;
        }

        private void SessionRuntimeError(NSNotification obj)
        {
            throw new NotImplementedException();
        }
    }
}