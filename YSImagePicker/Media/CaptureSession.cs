using System;
using System.Globalization;
using AVFoundation;
using CoreFoundation;
using Foundation;
using YSImagePicker.Media.Capture;

namespace YSImagePicker.Media
{
    public class CaptureSession : NSObject
    {
        private bool _isSessionRunning;
        private SessionSetupResult _setupResult = SessionSetupResult.Success;
        private readonly DispatchQueue _sessionQueue = new DispatchQueue("session queue");
        private readonly NotificationCenterHandler _notificationCenterHandler;
        public readonly VideoCaptureSession VideoCaptureSession;
        public readonly PhotoCaptureSession PhotoCaptureSession;

        public AVCaptureSession Session { get; } = new AVCaptureSession();
        public AVCaptureVideoPreviewLayer PreviewLayer { get; set; }
        private readonly ICaptureSessionDelegate _captureSessionDelegate;

        public SessionPresetConfiguration PresetConfiguration = SessionPresetConfiguration.Photos;

        public CaptureSession(ICaptureSessionDelegate captureSessionDelegate,
            ICaptureSessionVideoRecordingDelegate videoSessionDelegate)
        {
            _captureSessionDelegate = captureSessionDelegate;

            VideoCaptureSession = new VideoCaptureSession(SessionRuntimeError, videoSessionDelegate, _sessionQueue);
            _notificationCenterHandler = new NotificationCenterHandler(_captureSessionDelegate);
        }

        public CaptureSession(ICaptureSessionDelegate captureSessionDelegate,
            ISessionPhotoCapturingDelegate photoSessionDelegate)
        {
            _captureSessionDelegate = captureSessionDelegate;

            PhotoCaptureSession = new PhotoCaptureSession(SessionRuntimeError, photoSessionDelegate, _sessionQueue);
            _notificationCenterHandler = new NotificationCenterHandler(_captureSessionDelegate);
        }

        public void UpdateVideoOrientation(AVCaptureVideoOrientation newValue)
        {
            if (PreviewLayer?.Connection != null)
            {
                PreviewLayer.Connection.VideoOrientation = newValue;
            }
        }

        public void Prepare(AVCaptureVideoOrientation captureVideoOrientation)
        {
            switch (AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video))
            {
                case AVAuthorizationStatus.Authorized:
                    break;
                case AVAuthorizationStatus.NotDetermined:
                    _sessionQueue.Suspend();
                    AVCaptureDevice.RequestAccessForMediaType(AVAuthorizationMediaType.Video, granted =>
                    {
                        if (granted)
                        {
                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            {
                                _captureSessionDelegate.CaptureGrantedSession(AVAuthorizationStatus.Authorized);
                            });
                        }
                        else
                        {
                            _setupResult = SessionSetupResult.NotAuthorized;
                        }

                        _sessionQueue.Resume();
                    });
                    break;
                default:
                    // The user has previously denied access.
                    _setupResult = SessionSetupResult.NotAuthorized;
                    break;
            }

            _sessionQueue.DispatchAsync(() =>
            {
                ConfigureSession();
                UpdateVideoOrientation(captureVideoOrientation);
            });
        }

        public void Resume()
        {
            _sessionQueue.DispatchAsync(() =>
            {
                if (_isSessionRunning)
                {
                    Console.WriteLine("capture session: warning - trying to resume already running session");
                    return;
                }

                switch (_setupResult)
                {
                    case SessionSetupResult.Success:
                        // Only setup observers and start the session running if setup succeeded.
                        _notificationCenterHandler.AddObservers(Session);
                        Session.StartRunning();
                        _isSessionRunning = Session.Running;
                        break;
                    case SessionSetupResult.NotAuthorized:
                        Console.WriteLine("capture session: not authorized");
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            var status = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
                            _captureSessionDelegate.CaptureFailedSession(status);
                        });
                        break;
                    case SessionSetupResult.ConfigurationFailed:
                        Console.WriteLine("capture session: configuration failed");
                        DispatchQueue.MainQueue.DispatchAsync(_captureSessionDelegate.DidFailConfiguringSession);
                        break;
                }
            });
        }

        public void Suspend()
        {
            if (_setupResult != SessionSetupResult.Success)
            {
                return;
            }

            //we need to capture self in order to postpone deallocation while
            //session is properly stopped and cleaned up
            _sessionQueue.DispatchAsync(() =>
            {
                if (_isSessionRunning != true)
                {
                    Console.WriteLine("capture session: warning - trying to suspend non running session");
                    return;
                }

                Session.StopRunning();
                _isSessionRunning = Session.Running;
                _notificationCenterHandler.RemoveObservers(Session);
            });
        }

        private void ConfigureSession()
        {
            if (_setupResult != SessionSetupResult.Success)
            {
                return;
            }

            Console.WriteLine("capture session: configuring - adding video input");

            Session.BeginConfiguration();

            switch (PresetConfiguration)
            {
                case SessionPresetConfiguration.Photos:
                case SessionPresetConfiguration.LivePhotos:
                    PhotoCaptureSession.ConfigureSession(Session, PresetConfiguration);
                    break;
                case SessionPresetConfiguration.Videos:
                    Session.SessionPreset = AVCaptureSession.PresetHigh;
                    _setupResult = VideoCaptureSession.ConfigureSession(Session);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_setupResult != SessionSetupResult.Success)
            {
                Console.WriteLine("Cannot configure session");
                return;
            }

            Session.CommitConfiguration();
        }

        public void ChangeCamera(Action completion)
        {
            if (_setupResult != SessionSetupResult.Success)
            {
                Console.WriteLine(
                    "capture session: warning - trying to change camera but capture session setup failed");
                return;
            }

            _sessionQueue.DispatchAsync(() =>
            {
                Session.BeginConfiguration();

                switch (PresetConfiguration)
                {
                    case SessionPresetConfiguration.Photos:
                    case SessionPresetConfiguration.LivePhotos:
                        PhotoCaptureSession.ChangeCamera(Session, PresetConfiguration);
                        break;
                    case SessionPresetConfiguration.Videos:
                        VideoCaptureSession.ChangeCamera(Session);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Session.CommitConfiguration();

                DispatchQueue.MainQueue.DispatchAsync(() => { completion?.Invoke(); });
            });
        }

        private void SessionRuntimeError(NSNotification notification)
        {
            if (!(notification.UserInfo?[AVCaptureSession.ErrorKey] is NSError errorValue))
            {
                return;
            }

            Console.WriteLine($"capture session: runtime error: {errorValue}");

            /*
             Automatically try to restart the session running if media services were
             reset and the last start running succeeded. Otherwise, enable the user
             to try to resume the session running.
             */

            var parsResult =
                Enum.TryParse<AVError>(errorValue.Code.ToString(CultureInfo.InvariantCulture), out var error);

            if (parsResult == false)
            {
                return;
            }

            if (error == AVError.MediaServicesWereReset)
            {
                _sessionQueue.DispatchAsync(() =>
                {
                    if (_isSessionRunning)
                    {
                        Session.StartRunning();
                        _isSessionRunning = Session.Running;
                    }
                    else
                    {
                        DispatchQueue.MainQueue.DispatchAsync(() => { _captureSessionDelegate.DidFail(error); });
                    }
                });
            }
            else
            {
                DispatchQueue.MainQueue.DispatchAsync(() => { _captureSessionDelegate.DidFail(error); });
            }
        }
    }
}