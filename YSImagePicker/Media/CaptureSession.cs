using System;
using System.Globalization;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using Foundation;
using UIKit;
using YSImagePicker.Media.Capture;

namespace YSImagePicker.Media
{
    public class CaptureSession : NSObject
    {
        private bool _isSessionRunning;
        private SessionSetupResult _setupResult = SessionSetupResult.Success;
        private AVCaptureVideoDataOutput _videoDataOutput;
        private readonly AVCapturePhotoOutput _photoOutput = new AVCapturePhotoOutput();
        private readonly IntPtr _sessionRunningObserveContext = IntPtr.Zero;
        private bool _addedObservers;
        private readonly DispatchQueue _sessionQueue = new DispatchQueue("session queue");
        private NSObject _wasInterruptedNotification;
        private NSObject _interruptionEndedNotification;

        public VideoCaptureSession VideoCaptureSession;

        public ICaptureSessionDelegate Delegate = null;

        public readonly AVCaptureSession Session = new AVCaptureSession();
        public AVCaptureVideoPreviewLayer PreviewLayer { get; set; }

        public SessionPresetConfiguration PresetConfiguration = SessionPresetConfiguration.Photos;
        public ICaptureSessionVideoRecordingDelegate VideoRecordingDelegate;


        public ICaptureSessionPhotoCapturingDelegate PhotoCapturingDelegate = null;

        /// contains number of currently processing live photos
        public int InProgressLivePhotoCapturesCount;

        ///
        /// Set this method to orientation that matches UI orientation before `prepare()`
        /// method is called. If you need to update orientation when session is running,
        /// use `updateVideoOrientation()` method instead
        ///
        public AVCaptureVideoOrientation VideoOrientation = AVCaptureVideoOrientation.Portrait;


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
                    _sessionQueue.Suspend();
                    AVCaptureDevice.RequestAccessForMediaType(AVAuthorizationMediaType.Video, granted =>
                    {
                        if (granted)
                        {
                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            {
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

            VideoCaptureSession = new VideoCaptureSession(VideoRecordingDelegate);
            _sessionQueue.DispatchAsync(ConfigureSession);
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
                            var status = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
                            Delegate?.CaptureFailedSession(this, status);
                        });
                        break;
                    case SessionSetupResult.ConfigurationFailed:
                        Console.WriteLine("capture session: configuration failed");
                        DispatchQueue.MainQueue.DispatchAsync(() => { Delegate?.DidFailConfiguringSession(this); });
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
                RemoveObservers();
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
                case SessionPresetConfiguration.LivePhotos:
                case SessionPresetConfiguration.Photos:
                    Session.SessionPreset = AVCaptureSession.PresetPhoto;
                    break;
                case SessionPresetConfiguration.Videos:
                    Session.SessionPreset = AVCaptureSession.PresetHigh;
                    break;
            }

            if (VideoCaptureSession.ConfigureSession(Session) !=
                SessionSetupResult.Success)
            {
                Console.WriteLine("Session for video not configured");
                return;
            }

            if (PresetConfiguration == SessionPresetConfiguration.LivePhotos ||
                PresetConfiguration == SessionPresetConfiguration.Videos)
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

            if (PresetConfiguration == SessionPresetConfiguration.LivePhotos ||
                PresetConfiguration == SessionPresetConfiguration.Photos ||
                PresetConfiguration == SessionPresetConfiguration.Videos)
            {
                // Add photo output.
                Console.WriteLine("capture session: configuring - adding photo output");

                if (Session.CanAddOutput(_photoOutput))
                {
                    Session.AddOutput(_photoOutput);
                    _photoOutput.IsHighResolutionCaptureEnabled = true;

                    //enable live photos only if we intend to use it explicitly
                    if (PresetConfiguration == SessionPresetConfiguration.LivePhotos)
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

            if (PresetConfiguration != SessionPresetConfiguration.Videos)
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

            Session.CommitConfiguration();
        }

        private void AddObservers()
        {
            if (_addedObservers)
            {
                return;
            }

            Session.AddObserver(this, "running", NSKeyValueObservingOptions.New, _sessionRunningObserveContext);
            VideoCaptureSession.AddObservers();
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

            VideoCaptureSession.RemoveObservers();
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
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
            DispatchQueue.MainQueue.DispatchAsync(() => { Delegate?.CaptureSessionInterruptionDidEnd(this); });
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
                Session.BeginConfiguration();
                VideoCaptureSession.ChangeCamera(Session);

                /*
                  Set Live Photo capture enabled if it is supported. When changing cameras, the
                  `isLivePhotoCaptureEnabled` property of the AVCapturePhotoOutput gets set to NO when
                  a video device is disconnected from the session. After the new video device is
                  added to the session, re-enable Live Photo capture on the AVCapturePhotoOutput if it is supported.
                  */
                _photoOutput.IsLivePhotoCaptureEnabled =
                    _photoOutput.IsLivePhotoCaptureSupported &&
                    PresetConfiguration == SessionPresetConfiguration.LivePhotos;

                Session.CommitConfiguration();

                DispatchQueue.MainQueue.DispatchAsync(() => { completion?.Invoke(); });
            });
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
                // Capture a JPEG photo with flash set to auto and high resolution photo enabled.
                AVCapturePhotoSettings photoSettings = AVCapturePhotoSettings.Create();

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
                    if (PresetConfiguration == SessionPresetConfiguration.LivePhotos &&
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
                    {ShouldSavePhotoToLibrary = saveToPhotoLibrary};

                /*
             The Photo Output keeps a weak reference to the photo capture delegate so
             we store it in an array to maintain a strong reference to this object
             until the capture is completed.
             */
                _photoOutput.CapturePhoto(photoSettings, photoCaptureDelegate);
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
                        DispatchQueue.MainQueue.DispatchAsync(() => { Delegate?.DidFail(this, error); });
                    }
                });
            }
            else
            {
                DispatchQueue.MainQueue.DispatchAsync(() => { Delegate?.DidFail(this, error); });
            }
        }
    }
}