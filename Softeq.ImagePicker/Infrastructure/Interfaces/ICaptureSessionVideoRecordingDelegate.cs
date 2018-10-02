using Foundation;
using Softeq.ImagePicker.Media.Capture;

namespace Softeq.ImagePicker.Infrastructure.Interfaces
{
    /// Groups a method that informs a delegate about progress and state of photo capturing.
    public interface ICaptureSessionVideoRecordingDelegate
    {
        ///called when video file recording output is added to the session
        void DidBecomeReadyForVideoRecording(VideoCaptureSession session);

        ///called when recording started
        void DidStartVideoRecording(VideoCaptureSession session);

        ///called when cancel recording as a result of calling `cancelVideoRecording` func.
        void DidCancelVideoRecording(VideoCaptureSession session);

        ///called when a recording was successfully finished
        void DidFinishVideoRecording(VideoCaptureSession session, NSUrl videoUrl);

        ///called when a recording was finished prematurely due to a system interruption
        ///(empty disk, app put on bg, etc). Video is however saved on provided URL or in
        ///assets library if turned on.
        void DidInterruptVideoRecording(VideoCaptureSession session, NSUrl videoUrl, NSError reason);

        ///called when a recording failed
        void DidFailVideoRecording(VideoCaptureSession session, NSError error);
    }
}