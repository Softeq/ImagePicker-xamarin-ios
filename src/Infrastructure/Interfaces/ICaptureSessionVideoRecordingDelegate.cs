using Foundation;
using Softeq.ImagePicker.Media.Capture;

namespace Softeq.ImagePicker.Infrastructure.Interfaces
{
    /// <summary>
    /// Groups a method that informs a delegate about progress and state of photo capturing.
    /// </summary>
    public interface ICaptureSessionVideoRecordingDelegate
    {
        /// <summary>
        /// called when video file recording output is added to the session
        /// </summary>
        /// <param name="session">Session.</param>
        void DidBecomeReadyForVideoRecording(VideoCaptureSession session);

        /// <summary>
        /// called when recording started
        /// </summary>
        /// <param name="session">Session.</param>
        void DidStartVideoRecording(VideoCaptureSession session);

        /// <summary>
        /// called when cancel recording as a result of calling `cancelVideoRecording` func.
        /// </summary>
        /// <param name="session">Session.</param>
        void DidCancelVideoRecording(VideoCaptureSession session);

        /// <summary>
        /// called when a recording was successfully finished
        /// </summary>
        /// <param name="session">Session.</param>
        /// <param name="videoUrl">Video URL.</param>
        void DidFinishVideoRecording(VideoCaptureSession session, NSUrl videoUrl);

        /// <summary>
        /// called when a recording was finished prematurely due to a system interruption 
        /// (empty disk, app put on bg, etc). Video is however saved on provided URL or in assets library if turned on.
        /// </summary>
        /// <param name="session">Session.</param>
        /// <param name="videoUrl">Video URL.</param>
        /// <param name="reason">Reason.</param>
        void DidInterruptVideoRecording(VideoCaptureSession session, NSUrl videoUrl, NSError reason);

        /// <summary>
        /// called when a recording failed
        /// </summary>
        /// <param name="session">Session.</param>
        /// <param name="error">Error.</param>
        void DidFailVideoRecording(VideoCaptureSession session, NSError error);
    }
}