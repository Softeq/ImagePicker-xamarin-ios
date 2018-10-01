using System;
using AVFoundation;
using CoreFoundation;
using Foundation;
using YSImagePicker.Interfaces;

namespace YSImagePicker.Media.Capture
{
    public class CaptureNotificationCenterHandler : NSObject
    {
        private bool _addedObservers;
        private NSObject _wasInterruptedNotification;
        private NSObject _interruptionEndedNotification;
        private readonly IntPtr _sessionRunningObserveContext = IntPtr.Zero;
        private readonly ICaptureSessionDelegate _delegate;

        private const string RunningObserverKeyPath = "running";

        public CaptureNotificationCenterHandler(ICaptureSessionDelegate captureSessionDelegate)
        {
            _delegate = captureSessionDelegate;
        }

        public void AddObservers(AVCaptureSession session)
        {
            if (_addedObservers)
            {
                return;
            }

            session.AddObserver(this, RunningObserverKeyPath, NSKeyValueObservingOptions.New, _sessionRunningObserveContext);

            _wasInterruptedNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                AVCaptureSession.WasInterruptedNotification,
                SessionWasInterrupted, session);
            _interruptionEndedNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                AVCaptureSession.InterruptionEndedNotification,
                SessionInterruptionEnded, session);

            _addedObservers = true;
        }

        public void RemoveObservers(AVCaptureSession session)
        {
            if (_addedObservers != true)
            {
                return;
            }

            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_wasInterruptedNotification);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_interruptionEndedNotification);
            session.RemoveObserver(this, RunningObserverKeyPath, _sessionRunningObserveContext);

            _addedObservers = false;
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
                    _delegate?.WasInterrupted(AVCaptureSession.InterruptionReasonKey);
                });
            }
            else
            {
                Console.WriteLine("capture session: session was interrupted due to unknown reason");
            }
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
                        _delegate?.CaptureSessionDidResume();
                    }
                    else
                    {
                        _delegate?.CaptureSessionDidSuspend();
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
            DispatchQueue.MainQueue.DispatchAsync(() => { _delegate?.CaptureSessionInterruptionDidEnd(); });
        }
    }
}