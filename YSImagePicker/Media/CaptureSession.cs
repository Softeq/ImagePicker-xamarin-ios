using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AVFoundation;
using CoreFoundation;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace YSImagePicker.Media
{
    /// Groups a method that informs a delegate about progress and state of photo capturing.
    public class CaptureSessionPhotoCapturingDelegate
    {
        /// called as soon as the photo was taken, use this to update UI - for example show flash animation or live photo icon
        public virtual void CaptureSession(CaptureSession session, AVCapturePhotoSettings settings)
        {
        }

        /// called when captured photo is processed and ready for use
        public virtual void CaptureSession(CaptureSession session, NSData didCapturePhotoData,
            AVCapturePhotoSettings settings)
        {
        }

        /// called when captured photo is processed and ready for use
        public virtual void CaptureSession(CaptureSession session, NSError error)
        {
        }

        /// called when number of processing live photos changed, see inProgressLivePhotoCapturesCount for current count
        public virtual void CaptureSessionDidChangeNumberOfProcessingLivePhotos(CaptureSession session)
        {
        }
    }

    /// Groups a method that informs a delegate about progress and state of video recording.
    public class CaptureSessionVideoRecordingDelegate
    {
        ///called when video file recording output is added to the session
        public virtual void CaptureSessionDidBecomeReadyForVideoRecording(CaptureSession session)
        {
        }

        ///called when recording started
        public virtual void CaptureSessionDidStartVideoRecording(CaptureSession session)
        {
        }

        ///called when cancel recording as a result of calling `cancelVideoRecording` func.
        public virtual void CaptureSessionDidCancelVideoRecording(CaptureSession session)
        {
        }

        ///called when a recording was successfully finished
        public virtual void CaptureSessionDid(CaptureSession session, NSUrl videoUrl)
        {
        }

        ///called when a recording was finished prematurely due to a system interruption
        ///(empty disk, app put on bg, etc). Video is however saved on provided URL or in
        ///assets library if turned on.
        public virtual void CaptureSessionDid(CaptureSession session, NSUrl videoUrl, NSError reason)
        {
        }

        ///called when a recording failed
        public virtual void CaptureSessionDid(CaptureSession session, NSError error)
        {
        }
    }

    public class CaptureSessionDelegate
    {
        ///called when session is successfully configured and started running
        public virtual void CaptureSessionDidResume(CaptureSession session)
        {
        }

        ///called when session is was manually suspended
        public virtual void CaptureSessionDidSuspend(CaptureSession session)
        {
        }

        ///capture session was running but did fail due to any AV error reason.
        public virtual void CaptureSession(CaptureSession session, AVError error)
        {
        }

        ///called when creating and configuring session but something failed (e.g. input or output could not be added, etc
        public virtual void CaptureSessionDidFailConfiguringSession(CaptureSession session)
        {
        }

        ///called when user denied access to video device when prompted
        public virtual void CaptureGrantedSession(CaptureSession session, AVAuthorizationStatus status)
        {
        }

        ///Called when user grants access to video device when prompted
        public virtual void CaptureFailedSession(CaptureSession session, AVAuthorizationStatus status)
        {
        }

        ///called when session is interrupted due to various reasons, for example when a phone call or user starts an audio using control center, etc.
        public virtual void CaptureSession(CaptureSession session, NSString reason)
        {
        }

        ///called when and interruption is ended and the session was automatically resumed.
        public virtual void CaptureSessionInterruptionDidEnd(CaptureSession session)
        {
        }
    }

    public enum SessionSetupResult
    {
        Success,
        NotAuthorized,
        ConfigurationFailed
    }

    public enum SessionPresetConfiguration
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
        public CaptureSessionDelegate Delegate = null;

        public AVCaptureSession Session = new AVCaptureSession();
        public bool IsSessionRunning = false;
        public AVCaptureVideoPreviewLayer PreviewLayer = null;

        private SessionPresetConfiguration _presetConfiguration = SessionPresetConfiguration.Photos;
        private SessionSetupResult _setupResult = SessionSetupResult.Success;
        private AVCaptureDeviceInput _videoDeviceInput;

        private AVCaptureDeviceDiscoverySession _videoDeviceDiscoverySession =
            AVCaptureDeviceDiscoverySession.Create(new[]
                {
                    AVCaptureDeviceType.BuiltInWideAngleCamera,
                    AVCaptureDeviceType.BuiltInDuoCamera
                },
                AVMediaType.Video, AVCaptureDevicePosition.Unspecified);

        private AVCaptureVideoDataOutput _videoDataOutput;

        private VideoOutputSampleBufferDelegate videoOutpuSampleBufferDelegate = new VideoOutputSampleBufferDelegate();

        // returns latest captured image
        public UIImage latestVideoBufferImage => videoOutpuSampleBufferDelegate.LatestImage;

        /// Communicate with the session and other session objects on this queue.
        /// TODO: Check
        private DispatchQueue sessionQueue = new DispatchQueue("session queue");

        // MARK: Video Recoding

        public CaptureSessionVideoRecordingDelegate videoRecordingDelegate;
        public AVCaptureMovieFileOutput videoFileOutput;
        public VideoCaptureDelegate videoCaptureDelegate;

        public bool isReadyForVideoRecording => videoFileOutput != null;

        public bool isRecordingVideo => videoFileOutput?.Recording ?? false;
        public CaptureSessionPhotoCapturingDelegate photoCapturingDelegate = null;

        // this is provided by argument of capturePhoto()
        //fileprivate var livePhotoMode: LivePhotoMode = .off
        private AVCapturePhotoOutput photoOutput = new AVCapturePhotoOutput();

        private Dictionary<long, PhotoCaptureDelegate> inProgressPhotoCaptureDelegates =
            new Dictionary<long, PhotoCaptureDelegate>();

        /// contains number of currently processing live photos
        private int _inProgressLivePhotoCapturesCount = 0;

        ///
        /// Set this method to orientation that mathches UI orientation before `prepare()`
        /// method is called. If you need to update orientation when session is running,
        /// use `updateVideoOrientation()` method instead
        ///
        private AVCaptureVideoOrientation _videoOrientation = AVCaptureVideoOrientation.Portrait;

        private IntPtr sessionRunningObserveContext = IntPtr.Zero;
        private bool addedObservers = false;
        private NSObject _observeSubjectAreaDidChange;
        private NSObject _subjectAreaDidChangeNotification;
        private NSObject _wasInterruptedNotification;
        private NSObject _interruptionEndedNotification;

        ///
        /// Updates orientation on video outputs
        ///
        public void UpdateVideoOrientation(AVCaptureVideoOrientation newValue)
        {
            _videoOrientation = newValue;

            //we need to change orientation on all outputs
            if (PreviewLayer?.Connection != null)
            {
                PreviewLayer.Connection.VideoOrientation = newValue;
            }

            //TODO: we have to update orientation of video data output but it's blinking a bit which is
            //ugly, I have no idea how to fix this
            //note: when I added these 2 updates into a configuration block the lag was even worse

            sessionQueue.DispatchAsync(() =>
            {
                //when device is disconnected also video data output connection orientation is reset, so we need to set to new proper value

                if (_videoDataOutput?.ConnectionFromMediaType(AVMediaType.Video) != null)
                {
                    _videoDataOutput.ConnectionFromMediaType(AVMediaType.Video).VideoOrientation = newValue;
                }
            });
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
                    sessionQueue.Suspend();
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

                        sessionQueue.Resume();
                    });

                    break;
                default:
                    // The user has previously denied access.
                    _setupResult = SessionSetupResult.NotAuthorized;
                    break;
            }

            sessionQueue.DispatchAsync(() => { ConfigureSession(); });
        }

        public void Resume()
        {
            sessionQueue.DispatchAsync(() =>
            {
                if (IsSessionRunning)
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
                        IsSessionRunning = Session.Running;
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
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            Delegate?.CaptureSessionDidFailConfiguringSession(this);
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
            sessionQueue.DispatchAsync(() =>
            {
                if (IsSessionRunning != true)
                {
                    Console.WriteLine("capture session: warning - trying to suspend non running session");
                    return;
                }

                Session.StopRunning();
                IsSessionRunning = Session.Running;
                RemoveObservers();
                //we are not calling delegate from here because
                //we are KVOing `isRunning` on session itself so it's called from there
            });
        }

        ///
        /// Cinfigures a session before it can be used, following steps are done:
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

            switch (_presetConfiguration)
            {
                case SessionPresetConfiguration.LivePhotos:
                case SessionPresetConfiguration.Photos:
                    Session.SessionPreset = AVCaptureSession.PresetPhoto;
                    break;
                case SessionPresetConfiguration.Videos:
                    Session.SessionPreset = AVCaptureSession.PresetHigh;
                    break;
            }

            try
            {
                // Choose the back dual camera if available, otherwise default to a wide angle camera.
                if (!TryGetDefaultDevice(out var defaultVideoDevice))
                {
                    Console.WriteLine("capture session: could not create capture device");
                    _setupResult = SessionSetupResult.ConfigurationFailed;
                    Session.CommitConfiguration();
                    return;
                }

                var videoDeviceInput = AVCaptureDeviceInput.FromDevice(defaultVideoDevice);

                if (Session.CanAddInput(videoDeviceInput))
                {
                    Session.AddInput(videoDeviceInput);

                    _videoDeviceInput = videoDeviceInput;

                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        /*
                         Why are we dispatching this to the main queue?
                         Because AVCaptureVideoPreviewLayer is the backing layer for PreviewView and UIView
                         can only be manipulated on the main thread.
                         Note: As an exception to the above rule, it is not necessary to serialize video orientation changes
                         on the AVCaptureVideoPreviewLayer’s connection with other session manipulation.
                         */
                        if (PreviewLayer?.Connection != null)
                        {
                            PreviewLayer.Connection.VideoOrientation = _videoOrientation;
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
            }
            catch (Exception error)
            {
                Console.WriteLine($"capture session: could not create video device input: {error}");
                _setupResult = SessionSetupResult.ConfigurationFailed;
                Session.CommitConfiguration();
                return;
            }

            // Add movie file output.
            if (_presetConfiguration == SessionPresetConfiguration.Videos)
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
                    videoFileOutput = movieFileOutput;

                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        videoRecordingDelegate?.CaptureSessionDidBecomeReadyForVideoRecording(this);
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

            if (_presetConfiguration == SessionPresetConfiguration.LivePhotos ||
                _presetConfiguration == SessionPresetConfiguration.Videos)
            {
                Console.WriteLine("capture session: configuring - adding audio input");

                // Add audio input, if fails no need to fail whole configuration
                try
                {
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
                catch (Exception error)
                {
                    Console.WriteLine($"capture session: could not create audio device input: {error}");
                }
            }

            if (_presetConfiguration == SessionPresetConfiguration.LivePhotos ||
                _presetConfiguration == SessionPresetConfiguration.Photos ||
                _presetConfiguration == SessionPresetConfiguration.Videos)
            {
                // Add photo output.
                Console.WriteLine("capture session: configuring - adding photo output");

                if (Session.CanAddOutput(photoOutput))
                {
                    Session.AddOutput(photoOutput);
                    photoOutput.IsHighResolutionCaptureEnabled = true;

                    //enable live photos only if we intend to use it explicitly
                    if (_presetConfiguration == SessionPresetConfiguration.LivePhotos)
                    {
                        photoOutput.IsLivePhotoCaptureEnabled = photoOutput.IsLivePhotoCaptureSupported;
                        if (photoOutput.IsLivePhotoCaptureSupported == false)
                        {
                            Console.WriteLine(
                                "capture session: configuring - requested live photo mode is not supported by the device");
                        }
                    }

                    Console.WriteLine(
                        $"capture session: configuring - live photo mode is {nameof(photoOutput.IsLivePhotoCaptureEnabled)}");
                }
                else
                {
                    Console.WriteLine("capture session: could not add photo output to the session");
                    _setupResult = SessionSetupResult.ConfigurationFailed;
                    Session.CommitConfiguration();
                    return;
                }
            }

            if (_presetConfiguration != SessionPresetConfiguration.Videos)
            {
                // Add video data output - we use this to capture last video sample that is
                // used when blurring video layer - for example when capture session is suspended, changing configuration etc.
                // NOTE: video data output can not be connected at the same time as video file output!
                _videoDataOutput = new AVCaptureVideoDataOutput();
                if (Session.CanAddOutput(_videoDataOutput))
                {
                    Session.AddOutput(_videoDataOutput);
                    _videoDataOutput.AlwaysDiscardsLateVideoFrames = true;
                    _videoDataOutput.SetSampleBufferDelegate(videoOutpuSampleBufferDelegate,
                        videoOutpuSampleBufferDelegate.ProcessQueue);

                    var connection = _videoDataOutput.ConnectionFromMediaType(AVMediaType.Video);
                    if (connection != null)
                    {
                        connection.VideoOrientation = _videoOrientation;
                        connection.AutomaticallyAdjustsVideoMirroring = false;
                    }
                }
                else
                {
                    Console.WriteLine("capture session: warning - could not add video data output to the session");
                }
            }

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
            if (addedObservers != false)
            {
                return;
            }

            Session.AddObserver(this, "running", NSKeyValueObservingOptions.New, sessionRunningObserveContext);

            _observeSubjectAreaDidChange = AVCaptureDevice.Notifications.ObserveSubjectAreaDidChange((sender, e) =>
            {
                //let devicePoint = CGPoint(x: 0.5, y: 0.5)
                //focus(with: .autoFocus, exposureMode: .continuousAutoExposure, at: devicePoint, monitorSubjectAreaChange: false)
            });

            _subjectAreaDidChangeNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                AVCaptureDevice.SubjectAreaDidChangeNotification,
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

            addedObservers = true;
        }

        private void RemoveObservers()
        {
            if (addedObservers != true)
            {
                return;
            }

            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
            _observeSubjectAreaDidChange.Dispose();
            _subjectAreaDidChangeNotification.Dispose();
            _wasInterruptedNotification.Dispose();
            _interruptionEndedNotification.Dispose();

            Session.RemoveObserver(this, "running", sessionRunningObserveContext);

            addedObservers = false;
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (context == sessionRunningObserveContext)
            {
                var observableChange = new NSObservedChange(change);

                var isSessionRunning = (observableChange.NewValue as NSNumber)?.BoolValue;

                if (isSessionRunning == null)
                {
                    return;
                }

                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    Console.WriteLine("capture session: is running - ${isSessionRunning}");
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

        public void SessionInterruptionEnded(NSNotification notification)
        {
            Console.WriteLine("capture session: interruption ended");

            //this is called automatically when interruption is done and session
            //is automatically resumed. Delegate should know that this happened so
            //the UI can be updated
            DispatchQueue.MainQueue.DispatchAsync(() => { Delegate?.CaptureSessionInterruptionDidEnd(this); });
        }

        public void SessionRuntimeError(NSNotification notification)
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

            var parsResult = Enum.TryParse<AVError>(errorValue.Code.ToString(), out var error);

            if (parsResult == false)
            {
                return;
            }

            if (error == AVError.MediaServicesWereReset)
            {
                sessionQueue.DispatchAsync(() =>
                {
                    if (IsSessionRunning)
                    {
                        Session.StartRunning();
                        IsSessionRunning = Session.Running;
                    }
                    else
                    {
                        DispatchQueue.MainQueue.DispatchAsync(() => { Delegate?.CaptureSession(this, error); });
                    }
                });
            }
            else
            {
                DispatchQueue.MainQueue.DispatchAsync(() => { Delegate?.CaptureSession(this, error); });
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
                    Delegate?.CaptureSession(this, AVCaptureSession.InterruptionReasonKey);
                });
            }
            else
            {
                Console.WriteLine("capture session: session was interrupted due to unknown reason");
            }
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

            var videoPreviewLayerOrientation = PreviewLayer.Connection.VideoOrientation;

            sessionQueue.DispatchAsync(() =>
            {
                var photoOutputConnection = photoOutput.ConnectionFromMediaType(AVMediaType.Video);
                // Update the photo output's connection to match the video orientation of the video preview layer.
                if (photoOutputConnection != null)
                {
                    photoOutputConnection.VideoOrientation = videoPreviewLayerOrientation;
                }

                // Capture a JPEG photo with flash set to auto and high resolution photo enabled.
                using (var photoSettings = AVCapturePhotoSettings.Create())
                {
                    photoSettings.FlashMode = AVCaptureFlashMode.Auto;
                    photoSettings.IsHighResolutionPhotoEnabled = true;

                    //TODO: we dont need preview photo, we need thumbnail format, read `previewPhotoFormat` docs
                    //photoSettings.embeddedThumbnailPhotoFormat
                    //if photoSettings.availablePreviewPhotoPixelFormatTypes.count > 0 {
                    //    photoSettings.previewPhotoFormat = [kCVPixelBufferPixelFormatTypeKey as String : photoSettings.availablePreviewPhotoPixelFormatTypes.first!]
                    //}

                    //TODO: I dont know how it works, need to find out
                    if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0) &&
                        photoSettings.GetAvailableEmbeddedThumbnailPhotoCodecTypes.Length > 0)
                    {
                        //TODO: specify thumb size somehow, this does crash!
                        //let size = CGSize(width: 200, height: 200)
                        photoSettings.EmbeddedThumbnailPhotoFormat = new NSMutableDictionary
                        {
                            {
                                AVVideo.CodecKey,
                                FromObject(photoSettings.GetAvailableEmbeddedThumbnailPhotoCodecTypes[0])
                            }
                        };
                    }

                    if (livePhotoMode == LivePhotoMode.On)
                    {
                        if (_presetConfiguration == SessionPresetConfiguration.LivePhotos &&
                            photoOutput.IsLivePhotoCaptureSupported)
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
                                photoCapturingDelegate?.CaptureSession(this, photoSettings);
                            });
                        }, capturing =>
                        {
                            /*
                         Because Live Photo captures can overlap, we need to keep track of the
                         number of in progress Live Photo captures to ensure that the
                         Live Photo label stays visible during these captures.
                         */
                            sessionQueue.DispatchAsync(() =>
                            {
                                if (capturing)
                                {
                                    _inProgressLivePhotoCapturesCount += 1;
                                }
                                else
                                {
                                    _inProgressLivePhotoCapturesCount -= 1;
                                }

                                DispatchQueue.MainQueue.DispatchAsync(() =>
                                {
                                    if (_inProgressLivePhotoCapturesCount >= 0)
                                    {
                                        photoCapturingDelegate?.CaptureSessionDidChangeNumberOfProcessingLivePhotos(this);
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
                            sessionQueue.DispatchAsync(() =>
                            {
                                inProgressPhotoCaptureDelegates[photoDelegate.RequestedPhotoSettings.UniqueID] = null;
                            });

                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            {
                                if (photoDelegate.PhotoData != null)
                                {
                                    photoCapturingDelegate?.CaptureSession(this, photoDelegate.PhotoData,
                                        photoDelegate.RequestedPhotoSettings);
                                }
                                else if (photoDelegate.ProcessError != null)
                                {
                                    photoCapturingDelegate?.CaptureSession(this, photoDelegate.ProcessError);
                                }
                            });
                        }) {SavesPhotoToLibrary = saveToPhotoLibrary};


                    /*
                 The Photo Output keeps a weak reference to the photo capture delegate so
                 we store it in an array to maintain a strong reference to this object
                 until the capture is completed.
                 */
                    inProgressPhotoCaptureDelegates[photoCaptureDelegate.RequestedPhotoSettings.UniqueID] =
                        photoCaptureDelegate;
                    photoOutput.CapturePhoto(photoSettings, photoCaptureDelegate);
                }
            });
        }

        public void StartVideoRecording(bool saveToPhotoLibrary)
        {
            if (videoFileOutput == null)
            {
                Console.WriteLine("capture session: trying to record a video but no movie file output is set");
                return;
            }

            if (PreviewLayer == null)
            {
                Console.WriteLine("capture session: trying to record a video but no preview layer is set");
                return;
            }

            /*
             Retrieve the video preview layer's video orientation on the main queue
             before entering the session queue. We do this to ensure UI elements are
             accessed on the main thread and session configuration is done on the session queue.
             */
            var videoPreviewLayerOrientation = PreviewLayer.Connection.VideoOrientation;

            sessionQueue.DispatchAsync(() =>
            {
                // if already recording do nothing
                if (videoFileOutput.Recording == true)
                {
                    Console.WriteLine(
                        "capture session: trying to record a video but there is one already being recorded");
                    return;
                }

                // update the orientation on the movie file output video connection before starting recording.
                var movieFileOutputConnection = videoFileOutput?.ConnectionFromMediaType(AVMediaType.Video);
                if (movieFileOutputConnection != null)
                {
                    movieFileOutputConnection.VideoOrientation = videoPreviewLayerOrientation;
                }

                // start recording to a temporary file.
                var outputFileName = new NSUuid().AsString();
                var outputURL = NSUrl.FromString(System.IO.Path.GetTempPath()).AppendPathExtension(outputFileName)
                    .AppendPathExtension("mov");

                // create a recording delegate
                var recordingDelegate = new VideoCaptureDelegate(
                    () =>
                    {
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            videoRecordingDelegate?.CaptureSessionDidStartVideoRecording(this);
                        });
                    }, captureDelegate =>
                    {
                        // we need to remove reference to the delegate so it can be deallocated
                        sessionQueue.DispatchAsync(() => { videoCaptureDelegate = null; });

                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            if (captureDelegate.IsBeingCancelled)
                            {
                                videoRecordingDelegate?.CaptureSessionDidCancelVideoRecording(this);
                            }
                            else
                            {
                                videoRecordingDelegate?.CaptureSessionDid(this, outputURL);
                            }
                        });
                    }, (captureDelegate, error) =>
                    {
                        // we need to remove reference to the delegate so it can be deallocated
                        sessionQueue.DispatchAsync(() => { videoCaptureDelegate = null; });

                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            if (captureDelegate.RecordingWasInterrupted)
                            {
                                videoRecordingDelegate?.CaptureSessionDid(this, outputURL, error);
                            }
                            else
                            {
                                videoRecordingDelegate?.CaptureSessionDid(this, error);
                            }
                        });
                    });
                recordingDelegate.SavesVideoToLibrary = saveToPhotoLibrary;

                // start recording
                videoFileOutput.StartRecordingToOutputFile(outputURL, recordingDelegate);
                videoCaptureDelegate = recordingDelegate;
            });
        }

        ///
        /// If there is any recording in progres it will be stopped.
        ///
        /// - parameter cancel: if true, recorded file will be deleted and corresponding delegate method will be called.
        ///
        public void StopVideoRecording(bool cancel = false)
        {
            if (videoFileOutput == null)
            {
                Console.WriteLine("capture session: trying to stop a video recording but no movie file output is set");
                return;
            }

            sessionQueue.DispatchAsync(() =>
            {
                if (videoFileOutput.Recording == false)
                {
                    Console.WriteLine(
                        "capture session: trying to stop a video recording but no recording is in progress");
                    return;
                }

                if (videoCaptureDelegate == null)
                {
                    throw new Exception(
                        "capture session: trying to stop a video recording but video capture delegate is nil");
                }

                videoCaptureDelegate.IsBeingCancelled = cancel;
                videoFileOutput.StopRecording();
            });
        }
    }
}