using AVFoundation;
using Foundation;

namespace YSImagePicker.Media
{
    /// Groups a method that informs a delegate about progress and state of video recording.
    public interface ICaptureSessionDelegate
    {
        ///called when session is successfully configured and started running
        void CaptureSessionDidResume();

        ///called when session is was manually suspended
        void CaptureSessionDidSuspend();

        ///capture session was running but did fail due to any AV error reason.
        void DidFail(AVError error);

        ///called when creating and configuring session but something failed (e.g. input or output could not be added, etc
        void DidFailConfiguringSession();

        ///called when user denied access to video device when prompted
        void CaptureGrantedSession(AVAuthorizationStatus status);

        ///Called when user grants access to video device when prompted
        void CaptureFailedSession(AVAuthorizationStatus status);

        ///called when session is interrupted due to various reasons, for example when a phone call or user starts an audio using control center, etc.
        void WasInterrupted(NSString reason);

        ///called when and interruption is ended and the session was automatically resumed.
        void CaptureSessionInterruptionDidEnd();
    }
}