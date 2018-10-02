using AVFoundation;
using Foundation;

namespace Softeq.ImagePicker.Infrastructure.Interfaces
{
    /// <summary>
    /// Groups a method that informs a delegate about progress and state of video recording.
    /// </summary>
    public interface ICaptureSessionDelegate
    {
        /// <summary>
        /// called when session is successfully configured and started running
        /// </summary>
        void CaptureSessionDidResume();

        /// <summary>
        /// called when session is was manually suspended
        /// </summary>
        void CaptureSessionDidSuspend();

        /// <summary>
        /// capture session was running but did fail due to any AV error reason.
        /// </summary>
        /// <param name="error">Error.</param>
        void DidFail(AVError error);

        /// <summary>
        /// called when creating and configuring session but something failed (e.g. input or output could not be added, etc
        /// </summary>
        void DidFailConfiguringSession();

        /// <summary>
        /// called when user denied access to video device when prompted
        /// </summary>
        /// <param name="status">Status.</param>
        void CaptureGrantedSession(AVAuthorizationStatus status);

        /// <summary>
        /// Called when user grants access to video device when prompted
        /// </summary>
        /// <param name="status">Status.</param>
        void CaptureFailedSession(AVAuthorizationStatus status);

        /// <summary>
        /// called when session is interrupted due to various reasons, for example when a phone call or user starts an audio using control center, etc.
        /// </summary>
        /// <param name="reason">Reason.</param>
        void WasInterrupted(NSString reason);

        /// <summary>
        /// called when and interruption is ended and the session was automatically resumed.
        /// </summary>
        void CaptureSessionInterruptionDidEnd();
    }
}