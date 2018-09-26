using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using Foundation;
using UIKit;

namespace YSImagePicker.Media
{
    public interface ICaptureSessionPhotoCapturingDelegate
    {
        /// called as soon as the photo was taken, use this to update UI - for example show flash animation or live photo icon
        void WillCapturePhotoWith(CaptureSession session, AVCapturePhotoSettings settings);

        /// called when captured photo is processed and ready for use
        void DidCapturePhotoData(CaptureSession session, NSData didCapturePhotoData,
            AVCapturePhotoSettings settings);

        /// called when captured photo is processed and ready for use
        void DidFailCapturingPhotoWith(CaptureSession session, NSError error);

        /// called when number of processing live photos changed, see inProgressLivePhotoCapturesCount for current count
        void CaptureSessionDidChangeNumberOfProcessingLivePhotos(CaptureSession session);
    }

    /// Groups a method that informs a delegate about progress and state of photo capturing.
    public interface ICaptureSessionVideoRecordingDelegate
    {
        ///called when video file recording output is added to the session
        void DidBecomeReadyForVideoRecording(CaptureSession session);

        ///called when recording started
        void DidStartVideoRecording(CaptureSession session);

        ///called when cancel recording as a result of calling `cancelVideoRecording` func.
        void DidCancelVideoRecording(CaptureSession session);

        ///called when a recording was successfully finished
        void DidFinishVideoRecording(CaptureSession session, NSUrl videoUrl);

        ///called when a recording was finished prematurely due to a system interruption
        ///(empty disk, app put on bg, etc). Video is however saved on provided URL or in
        ///assets library if turned on.
        void DidInterruptVideoRecording(CaptureSession session, NSUrl videoUrl, NSError reason);

        ///called when a recording failed
        void DidFailVideoRecording(CaptureSession session, NSError error);
    }

    /// Groups a method that informs a delegate about progress and state of video recording.
    public interface ICaptureSessionDelegate
    {
        ///called when session is successfully configured and started running
        void CaptureSessionDidResume(CaptureSession session);

        ///called when session is was manually suspended
        void CaptureSessionDidSuspend(CaptureSession session);

        ///capture session was running but did fail due to any AV error reason.
        void DidFail(CaptureSession session, AVError error);

        ///called when creating and configuring session but something failed (e.g. input or output could not be added, etc
        void DidFailConfiguringSession(CaptureSession session);

        ///called when user denied access to video device when prompted
        void CaptureGrantedSession(CaptureSession session, AVAuthorizationStatus status);

        ///Called when user grants access to video device when prompted
        void CaptureFailedSession(CaptureSession session, AVAuthorizationStatus status);

        ///called when session is interrupted due to various reasons, for example when a phone call or user starts an audio using control center, etc.
        void WasInterrupted(CaptureSession session, NSString reason);

        ///called when and interruption is ended and the session was automatically resumed.
        void CaptureSessionInterruptionDidEnd(CaptureSession session);
    }

    public enum SessionSetupResult
    {
        Success,
        NotAuthorized,
        ConfigurationFailed
    }

    public enum GetSessionPresetConfiguration
    {
        Photos,
        LivePhotos,
        Videos
    }

    public enum LivePhotoMode
    {
        On,
        Off
    }

    public class CaptureSession : NSObject
    {
        public ICaptureSessionDelegate Delegate = null;

        public readonly AVCaptureSession Session = new AVCaptureSession();
        private bool _isSessionRunning = false;
        public AVCaptureVideoPreviewLayer PreviewLayer { get; set; }

        public GetSessionPresetConfiguration PresetConfiguration = GetSessionPresetConfiguration.Photos;
        private SessionSetupResult _setupResult = SessionSetupResult.Success;
        private AVCaptureDeviceInput _videoDeviceInput;

        private readonly AVCaptureDeviceDiscoverySession _videoDeviceDiscoverySession =
            AVCaptureDeviceDiscoverySession.Create(new[]
                {
                    AVCaptureDeviceType.BuiltInWideAngleCamera,
                    AVCaptureDeviceType.BuiltInDuoCamera
                },
                AVMediaType.Video, AVCaptureDevicePosition.Unspecified);

        private AVCaptureVideoDataOutput _videoDataOutput;

        /// Communicate with the session and other session objects on this queue.
        /// TODO: Check
        private readonly DispatchQueue _sessionQueue = new DispatchQueue("session queue");

        // MARK: Video Recoding

        public ICaptureSessionVideoRecordingDelegate VideoRecordingDelegate;
        private AVCaptureMovieFileOutput _videoFileOutput;
        private VideoCaptureDelegate _videoCaptureDelegate;

        public bool IsReadyForVideoRecording => _videoFileOutput != null;

        public bool IsRecordingVideo => _videoFileOutput?.Recording ?? false;
        public ICaptureSessionPhotoCapturingDelegate PhotoCapturingDelegate = null;

        // this is provided by argument of capturePhoto()
        //fileprivate var livePhotoMode: LivePhotoMode = .off
        private readonly AVCapturePhotoOutput _photoOutput = new AVCapturePhotoOutput();

        private readonly Dictionary<long, PhotoCaptureDelegate> _inProgressPhotoCaptureDelegates =
            new Dictionary<long, PhotoCaptureDelegate>();

        /// contains number of currently processing live photos
        public int InProgressLivePhotoCapturesCount = 0;

        ///
        /// Set this method to orientation that mathches UI orientation before `prepare()`
        /// method is called. If you need to update orientation when session is running,
        /// use `updateVideoOrientation()` method instead
        ///
        public AVCaptureVideoOrientation VideoOrientation = AVCaptureVideoOrientation.Portrait;

        private readonly IntPtr _sessionRunningObserveContext = IntPtr.Zero;
        private bool _addedObservers = false;
        private NSObject _observeSubjectAreaDidChange;
        private NSObject _subjectAreaDidChangeNotification;
        private NSObject _wasInterruptedNotification;
        private NSObject _interruptionEndedNotification;

        ///
        /// Updates orientation on video outputs
        ///
        public void UpdateVideoOrientation(AVCaptureVideoOrientation newValue)
        {
            VideoOrientation = newValue;

            //we need to change orientation on all outputs
            if (PreviewLayer?.Connection != null)
            {
                PreviewLayer.Connection.VideoOrientation = newValue;
            }
        }

        public void Prepare()
        {
            /*
             Check video authorization status. Video access is required and audio
             access is optional. If audio access is denied, audio is not recorded
             during movie recording.
             */
            var mediaType = AVMediaType.Video;

            switch (AVCaptureDevice.GetAuthorizationStatus(mediaType))
            {
                case AVAuthorizationStatus.Authorized:
                    // The user has previously granted access to the camera.
                    break;

                case AVAuthorizationStatus.NotDetermined:
                    /*
                     The user has not yet been presented with the option to grant
                     video access. We suspend the session queue to delay session
                     setup until the access request has completed.
                     
                     Note that audio access will be implicitly requested when we
                     create an AVCaptureDeviceInput for audio during session setup.
                     */
                    _sessionQueue.Suspend();
                    AVCaptureDevice.RequestAccessForMediaType(AVAuthorizationMediaType.Video, granted =>
                    {
                        if (granted)
                        {
                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            {
                                Console.WriteLine("Tests:5");
                                Delegate?.CaptureGrantedSession(this, AVAuthorizationStatus.Authorized);
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
                Console.WriteLine("Tests:6");
                ConfigureSession();
            });
        }

        public void Resume()
        {
            _sessionQueue.DispatchAsync(() =>
            {
                Console.WriteLine("Tests:7");
                if (_isSessionRunning)
                {
                    Console.WriteLine("capture session: warning - trying to resume already running session");
                    return;
                }

                switch (_setupResult)
                {
                    case SessionSetupResult.Success:
                        // Only setup observers and start the session running if setup succeeded.
                        AddObservers();
                        Session.StartRunning();
                        _isSessionRunning = Session.Running;
                        break;
                    // We are not calling the delegate here explicitly, because we are observing
                    // `running` KVO on session itself.

                    case SessionSetupResult.NotAuthorized:
                        Console.WriteLine("capture session: not authorized");
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:8");
                            var status = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
                            Delegate?.CaptureFailedSession(this, status);
                        });
                        break;
                    case SessionSetupResult.ConfigurationFailed:
                        Console.WriteLine("capture session: configuration failed");
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:9");
                            Delegate?.DidFailConfiguringSession(this);
                        });
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
                Console.WriteLine("Tests:10");
                if (_isSessionRunning != true)
                {
                    Console.WriteLine("capture session: warning - trying to suspend non running session");
                    return;
                }

                Session.StopRunning();
                _isSessionRunning = Session.Running;
                RemoveObservers();
                //we are not calling delegate from here because
                //we are KVOing `isRunning` on session itself so it's called from there
            });
        }

        ///
        /// Configures a session before it can be used, following steps are done:
        /// 1. adds video input
        /// 2. adds video output (for recording videos)
        /// 3. adds audio input (for video recording with audio)
        /// 4. adds photo output (for capturing photos)capture session: trying to record a video but no preview layer is set
        ///
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
                case GetSessionPresetConfiguration.LivePhotos:
                case GetSessionPresetConfiguration.Photos:
                    Session.SessionPreset = AVCaptureSession.PresetPhoto;
                    break;
                case GetSessionPresetConfiguration.Videos:
                    Session.SessionPreset = AVCaptureSession.PresetHigh;
                    break;
            }

            // Choose the back dual camera if available, otherwise default to a wide angle camera.
            if (!TryGetDefaultDevice(out var defaultVideoDevice))
            {
                Console.WriteLine("capture session: could not create capture device");
                _setupResult = SessionSetupResult.ConfigurationFailed;
                Session.CommitConfiguration();
                return;
            }

            var videoDeviceInput = new AVCaptureDeviceInput(defaultVideoDevice, out var error);

            if (error != null)
            {
                Console.WriteLine($"Error accured {error}");
            }

            if (Session.CanAddInput(videoDeviceInput))
            {
                Session.AddInput(videoDeviceInput);

                _videoDeviceInput = videoDeviceInput;

                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    Console.WriteLine("Tests:11");
                    /*
                     Why are we dispatching this to the main queue?
                     Because AVCaptureVideoPreviewLayer is the backing layer for PreviewView and UIView
                     can only be manipulated on the main thread.
                     Note: As an exception to the above rule, it is not necessary to serialize video orientation changes
                     on the AVCaptureVideoPreviewLayer’s connection with other session manipulation.
                     */
                    if (PreviewLayer?.Connection != null)
                    {
                        PreviewLayer.Connection.VideoOrientation = VideoOrientation;
                    }
                });
            }
            else
            {
                Console.WriteLine("capture session: could not add video device input to the session");
                _setupResult = SessionSetupResult.ConfigurationFailed;
                Session.CommitConfiguration();
                return;
            }

            #region test

            // Add movie file output.
            if (PresetConfiguration == GetSessionPresetConfiguration.Videos)
            {
                // A capture session cannot support at the same time:
                // - Live Photo capture and
                // - movie file output
                // - video data output
                // If your capture session includes an AVCaptureMovieFileOutput object, the
                // isLivePhotoCaptureSupported property becomes false.

                Console.WriteLine("capture session: configuring - adding movie file input");

                var movieFileOutput = new AVCaptureMovieFileOutput();
                if (Session.CanAddOutput(movieFileOutput))
                {
                    Session.AddOutput(movieFileOutput);
                    _videoFileOutput = movieFileOutput;

                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        Console.WriteLine("Tests:12");
                        VideoRecordingDelegate?.DidBecomeReadyForVideoRecording(this);
                    });
                }
                else
                {
                    Console.WriteLine("capture session: could not add video output to the session");
                    _setupResult = SessionSetupResult.ConfigurationFailed;
                    Session.CommitConfiguration();
                    return;
                }
            }

            if (PresetConfiguration == GetSessionPresetConfiguration.LivePhotos ||
                PresetConfiguration == GetSessionPresetConfiguration.Videos)
            {
                Console.WriteLine("capture session: configuring - adding audio input");

                // Add audio input, if fails no need to fail whole configuration
                var audioDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Audio);
                var audioDeviceInput = AVCaptureDeviceInput.FromDevice(audioDevice);

                if (Session.CanAddInput(audioDeviceInput))
                {
                    Session.AddInput(audioDeviceInput);
                }
                else
                {
                    Console.WriteLine("capture session: could not add audio device input to the session");
                }
            }
            if (PresetConfiguration == GetSessionPresetConfiguration.LivePhotos ||
                PresetConfiguration == GetSessionPresetConfiguration.Photos ||
                PresetConfiguration == GetSessionPresetConfiguration.Videos)
            {
                // Add photo output.
                Console.WriteLine("capture session: configuring - adding photo output");

                if (Session.CanAddOutput(_photoOutput))
                {
                    Session.AddOutput(_photoOutput);
                    _photoOutput.IsHighResolutionCaptureEnabled = true;

                    //enable live photos only if we intend to use it explicitly
                    if (PresetConfiguration == GetSessionPresetConfiguration.LivePhotos)
                    {
                        _photoOutput.IsLivePhotoCaptureEnabled = _photoOutput.IsLivePhotoCaptureSupported;
                        if (_photoOutput.IsLivePhotoCaptureSupported == false)
                        {
                            Console.WriteLine(
                                "capture session: configuring - requested live photo mode is not supported by the device");
                        }
                    }

                    Console.WriteLine(
                        $"capture session: configuring - live photo mode is {nameof(_photoOutput.IsLivePhotoCaptureEnabled)}");
                }
                else
                {
                    Console.WriteLine("capture session: could not add photo output to the session");
                    _setupResult = SessionSetupResult.ConfigurationFailed;
                    Session.CommitConfiguration();
                    return;
                }
            }

            if (PresetConfiguration != GetSessionPresetConfiguration.Videos)
            {
                // Add video data output - we use this to capture last video sample that is
                // used when blurring video layer - for example when capture session is suspended, changing configuration etc.
                // NOTE: video data output can not be connected at the same time as video file output!
                _videoDataOutput = new AVCaptureVideoDataOutput();
                if (Session.CanAddOutput(_videoDataOutput))
                {
                    Session.AddOutput(_videoDataOutput);
                    _videoDataOutput.AlwaysDiscardsLateVideoFrames = true;
                }
                else
                {
                    Console.WriteLine("capture session: warning - could not add video data output to the session");
                }
            }

            #endregion
            Session.CommitConfiguration();
        }

        private bool TryGetDefaultDevice(out AVCaptureDevice device)
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

        // MARK: KVO and Notifications

        private void AddObservers()
        {
            if (_addedObservers != false)
            {
                return;
            }

            Session.AddObserver(this, "running", NSKeyValueObservingOptions.New, _sessionRunningObserveContext);

            _observeSubjectAreaDidChange = AVCaptureDevice.Notifications.ObserveSubjectAreaDidChange((sender, e) =>
            {
                //let devicePoint = CGPoint(x: 0.5, y: 0.5)
                //focus(with: .autoFocus, exposureMode: .continuousAutoExposure, at: devicePoint, monitorSubjectAreaChange: false)
            });

            _subjectAreaDidChangeNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                AVCaptureDevice.SubjectAreaDidChangeNotification,
                SubjectAreaDidChange, _videoDeviceInput.Device);

            _subjectAreaDidChangeNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                AVCaptureSession.RuntimeErrorNotification,
                SessionRuntimeError, _videoDeviceInput.Device);

            /*
         A session can only run when the app is full screen. It will be interrupted
         in a multi-app layout, introduced in iOS 9, see also the documentation of
         AVCaptureSessionInterruptionReason. Add observers to handle these session
         interruptions and show a preview is paused message. See the documentation
         of AVCaptureSessionWasInterruptedNotification for other interruption reasons.
         */
            _wasInterruptedNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                AVCaptureSession.WasInterruptedNotification,
                SessionWasInterrupted, Session);

            _interruptionEndedNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                AVCaptureSession.InterruptionEndedNotification,
                SessionInterruptionEnded, Session);

            _addedObservers = true;
        }

        private void RemoveObservers()
        {
            if (_addedObservers != true)
            {
                return;
            }

            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_observeSubjectAreaDidChange);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_subjectAreaDidChangeNotification);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_wasInterruptedNotification);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_interruptionEndedNotification);
            Session.RemoveObserver(this, "running", _sessionRunningObserveContext);

            _addedObservers = false;
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (context == _sessionRunningObserveContext)
            {
                var observableChange = new NSObservedChange(change);

                var isSessionRunning = (observableChange.NewValue as NSNumber)?.BoolValue;

                if (isSessionRunning == null)
                {
                    return;
                }

                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    Console.WriteLine("Tests:13");
                    Console.WriteLine($"capture session: is running - ${isSessionRunning}");
                    if (isSessionRunning.Value)
                    {
                        Delegate?.CaptureSessionDidResume(this);
                    }
                    else
                    {
                        Delegate?.CaptureSessionDidSuspend(this);
                    }
                });
            }
            else
            {
                base.ObserveValue(keyPath, ofObject, change, context);
            }
        }

        private void SessionInterruptionEnded(NSNotification notification)
        {
            Console.WriteLine("capture session: interruption ended");

            //this is called automatically when interruption is done and session
            //is automatically resumed. Delegate should know that this happened so
            //the UI can be updated
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                Console.WriteLine("Tests:14");
                Delegate?.CaptureSessionInterruptionDidEnd(this);
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
                    Console.WriteLine("Tests:15");
                    if (_isSessionRunning)
                    {
                        Session.StartRunning();
                        _isSessionRunning = Session.Running;
                    }
                    else
                    {
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:16");
                            Delegate?.DidFail(this, error);
                        });
                    }
                });
            }
            else
            {
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    Console.WriteLine("Tests:17");
                    Delegate?.DidFail(this, error);
                });
            }
        }

        private void SessionWasInterrupted(NSNotification notification)
        {
            /*
             In some scenarios we want to enable the user to resume the session running.
             For example, if music playback is initiated via control center while
             using AVCam, then the user can let AVCam resume
             the session running, which will stop music playback. Note that stopping
             music playback in control center will not automatically resume the session
             running. Also note that it is not always possible to resume, see `resumeInterruptedSession(_:)`.
             */
            if (notification.UserInfo.ContainsKey(AVCaptureSession.InterruptionReasonKey) &&
                !string.IsNullOrEmpty(AVCaptureSession.InterruptionReasonKey))
            {
                Console.WriteLine(
                    $"capture session: session was interrupted with reason {notification.UserInfo[AVCaptureSession.InterruptionReasonKey]}");
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    Console.WriteLine("Tests:18");
                    Delegate?.WasInterrupted(this, AVCaptureSession.InterruptionReasonKey);
                });
            }
            else
            {
                Console.WriteLine("capture session: session was interrupted due to unknown reason");
            }
        }

        public void ChangeCamera(Action completion)
        {
            if (_setupResult != SessionSetupResult.Success)
            {
                Console.WriteLine(
                    "capture session: warning - trying to change camera but capture session setup failed");
                return;
            }

            AVCaptureDevicePosition preferredPosition;
            AVCaptureDeviceType preferredDeviceType;

            _sessionQueue.DispatchAsync(() =>
            {
                Console.WriteLine("Tests:19");
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

                Session.BeginConfiguration();

                // Remove the existing device input first, since using the front and back camera simultaneously is not supported.
                Session.RemoveInput(_videoDeviceInput);

                if (Session.CanAddInput(videoDeviceInput))
                {

                    if (_subjectAreaDidChangeNotification != null)
                    {
                        NSNotificationCenter.DefaultCenter.RemoveObserver(_subjectAreaDidChangeNotification);
                    }
                    _subjectAreaDidChangeNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                           AVCaptureDevice.SubjectAreaDidChangeNotification, SubjectAreaDidChange,
                           _videoDeviceInput.Device);
                    Session.AddInput(videoDeviceInput);
                    _videoDeviceInput = videoDeviceInput;
                }
                else
                {
                    Session.AddInput(_videoDeviceInput);
                }

                var connection = _videoFileOutput?.ConnectionFromMediaType(AVMediaType.Video);
                if (connection != null)
                {
                    if (connection.SupportsVideoStabilization)
                    {
                        connection.PreferredVideoStabilizationMode = AVCaptureVideoStabilizationMode.Auto;
                    }
                }

                /*
                  Set Live Photo capture enabled if it is supported. When changing cameras, the
                  `isLivePhotoCaptureEnabled` property of the AVCapturePhotoOutput gets set to NO when
                  a video device is disconnected from the session. After the new video device is
                  added to the session, re-enable Live Photo capture on the AVCapturePhotoOutput if it is supported.
                  */
                _photoOutput.IsLivePhotoCaptureEnabled =
                    _photoOutput.IsLivePhotoCaptureSupported &&
                    PresetConfiguration == GetSessionPresetConfiguration.LivePhotos;

                Session.CommitConfiguration();

                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    Console.WriteLine("Tests:20");
                    completion?.Invoke();
                });
            });
        }

        public void SubjectAreaDidChange(NSNotification notification)
        {
            //let devicePoint = CGPoint(x: 0.5, y: 0.5)
            //focus(with: .autoFocus, exposureMode: .continuousAutoExposure, at: devicePoint, monitorSubjectAreaChange: false)
        }

        public void CapturePhoto(LivePhotoMode livePhotoMode, bool saveToPhotoLibrary)
        {
            /*
         Retrieve the video preview layer's video orientation on the main queue before
         entering the session queue. We do this to ensure UI elements are accessed on
         the main thread and session configuration is done on the session queue.
         */
            if (PreviewLayer?.Connection?.VideoOrientation == null)
            {
                Console.WriteLine("capture session: warning - trying to capture a photo but no preview layer is set");
                return;
            }

            _sessionQueue.DispatchAsync(() =>
            {
                Console.WriteLine("Tests:21");

                // Capture a JPEG photo with flash set to auto and high resolution photo enabled.
                AVCapturePhotoSettings photoSettings = AVCapturePhotoSettings.Create();

                if (_photoOutput.SupportedFlashModes.Contains(NSNumber.FromInt32((int)AVCaptureFlashMode.Auto)))
                {
                    photoSettings.FlashMode = AVCaptureFlashMode.Auto;
                }
                photoSettings.IsHighResolutionPhotoEnabled = true;

                //TODO: we dont need preview photo, we need thumbnail format, read `previewPhotoFormat` docs
                //photoSettings.embeddedThumbnailPhotoFormat
                //if photoSettings.availablePreviewPhotoPixelFormatTypes.count > 0 {
                //    photoSettings.previewPhotoFormat = [kCVPixelBufferPixelFormatTypeKey as String : photoSettings.availablePreviewPhotoPixelFormatTypes.first!]
                //}

                //TODO: I don't know how it works, need to find out
                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0) &&
                    photoSettings.AvailableEmbeddedThumbnailPhotoCodecTypes.Length > 0)
                {
                    //TODO: specify thumb size somehow, this does crash!
                    //let size = CGSize(width: 200, height: 200)
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
                    if (PresetConfiguration == GetSessionPresetConfiguration.LivePhotos &&
                        _photoOutput.IsLivePhotoCaptureSupported)
                    {
                        var livePhotoMovieFileName = new NSString(new NSUuid().AsString());
                        var livePhotoMovieFilePath = new NSString(System.IO.Path.GetTempPath())
                            .AppendPathComponent(livePhotoMovieFileName).AppendPathExtension(new NSString("mov"));
                        photoSettings.LivePhotoMovieFileUrl =
                            NSUrl.CreateFileUrl(livePhotoMovieFilePath.PathComponents);
                    }
                    else
                    {
                        Console.WriteLine(
                            "capture session: warning - trying to capture live photo but it's not supported by current configuration, capturing regular photo instead");
                    }
                }

                // Use a separate object for the photo capture delegate to isolate each capture life cycle.
                var photoCaptureDelegate = new PhotoCaptureDelegate(photoSettings,
                    () =>
                    {
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:22");
                            PhotoCapturingDelegate?.WillCapturePhotoWith(this, photoSettings);
                        });
                    }, capturing =>
                    {
                        /*
                     Because Live Photo captures can overlap, we need to keep track of the
                     number of in progress Live Photo captures to ensure that the
                     Live Photo label stays visible during these captures.
                     */
                        _sessionQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:23");
                            if (capturing)
                            {
                                InProgressLivePhotoCapturesCount += 1;
                            }
                            else
                            {
                                InProgressLivePhotoCapturesCount -= 1;
                            }

                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            {
                                Console.WriteLine("Tests:24");
                                if (InProgressLivePhotoCapturesCount >= 0)
                                {
                                    PhotoCapturingDelegate?.CaptureSessionDidChangeNumberOfProcessingLivePhotos(
                                        this);
                                }
                                else
                                {
                                    Console.WriteLine(
                                        "capture session: error - in progress live photo capture count is less than 0");
                                }
                            });
                        });
                    }, photoDelegate =>
                    {
                        _sessionQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:25");
                            _inProgressPhotoCaptureDelegates[photoDelegate.RequestedPhotoSettings.UniqueID] =
                                null;
                        });

                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:26");
                            if (photoDelegate.PhotoData != null)
                            {
                                PhotoCapturingDelegate?.DidCapturePhotoData(this, photoDelegate.PhotoData,
                                    photoDelegate.RequestedPhotoSettings);
                            }
                            else if (photoDelegate.ProcessError != null)
                            {
                                PhotoCapturingDelegate?.DidFailCapturingPhotoWith(this, photoDelegate.ProcessError);
                            }
                        });
                    })
                { SavesPhotoToLibrary = saveToPhotoLibrary };


                /*
             The Photo Output keeps a weak reference to the photo capture delegate so
             we store it in an array to maintain a strong reference to this object
             until the capture is completed.
             */
                _inProgressPhotoCaptureDelegates[photoCaptureDelegate.RequestedPhotoSettings.UniqueID] =
                    photoCaptureDelegate;
                _photoOutput.CapturePhoto(photoSettings, photoCaptureDelegate);
            });
        }

        public void StartVideoRecording(bool saveToPhotoLibrary)
        {
            if (_videoFileOutput == null)
            {
                Console.WriteLine("capture session: trying to record a video but no movie file output is set");
                return;
            }

            if (PreviewLayer == null)
            {
                Console.WriteLine("capture session: trying to record a video but no preview layer is set");
                return;
            }

            _sessionQueue.DispatchAsync(() =>
            {
                Console.WriteLine("Tests:27");
                // if already recording do nothing
                if (_videoFileOutput.Recording == true)
                {
                    Console.WriteLine(
                        "capture session: trying to record a video but there is one already being recorded");
                    return;
                }

                // start recording to a temporary file.
                var outputFileName = new NSUuid().AsString();
                VideoCaptureDelegate recordingDelegate;

                var outputUrl = NSFileManager.DefaultManager.GetTemporaryDirectory().Append(outputFileName, false).AppendPathExtension("mov");

                recordingDelegate = new VideoCaptureDelegate(
                    () =>
                    {
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:28");
                            VideoRecordingDelegate?.DidStartVideoRecording(this);
                        });
                    }, captureDelegate =>
                    {
                        // we need to remove reference to the delegate so it can be deallocated
                        _sessionQueue.DispatchAsync(() =>
                    {
                        Console.WriteLine("Tests:29");
                        _videoCaptureDelegate = null;
                    });

                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:30");
                            if (captureDelegate.IsBeingCancelled)
                            {
                                VideoRecordingDelegate?.DidCancelVideoRecording(this);
                            }
                            else
                            {
                                VideoRecordingDelegate?.DidFinishVideoRecording(this, outputUrl);
                            }
                        });
                    }, (captureDelegate, error) =>
                    {
                        // we need to remove reference to the delegate so it can be deallocated
                        _sessionQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:31");
                            _videoCaptureDelegate = null;
                        });

                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:32");
                            if (captureDelegate.RecordingWasInterrupted)
                            {
                                VideoRecordingDelegate?.DidInterruptVideoRecording(this, outputUrl, error);
                            }
                            else
                            {
                                VideoRecordingDelegate?.DidFailVideoRecording(this, error);
                            }
                        });
                    })
                { SavesVideoToLibrary = saveToPhotoLibrary };

                // start recording
                _videoFileOutput.StartRecordingToOutputFile(outputUrl, recordingDelegate);

                _videoCaptureDelegate = recordingDelegate;
            });
        }

        ///
        /// If there is any recording in progres it will be stopped.
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
                Console.WriteLine("Tests:33");
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
    }
}