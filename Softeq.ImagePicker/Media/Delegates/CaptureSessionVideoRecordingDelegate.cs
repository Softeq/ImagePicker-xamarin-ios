using System;
using Foundation;
using Softeq.ImagePicker.Interfaces;
using Softeq.ImagePicker.Media.Capture;
using Softeq.ImagePicker.Public;

namespace Softeq.ImagePicker.Media.Delegates
{
    public class CaptureSessionVideoRecordingDelegate : ICaptureSessionVideoRecordingDelegate
    {
        private readonly Func<CameraCollectionViewCell> _getCameraCellFunc;

        public CaptureSessionVideoRecordingDelegate(Func<CameraCollectionViewCell> getCameraCellFunc)
        {
            _getCameraCellFunc = getCameraCellFunc;
        }

        public void DidBecomeReadyForVideoRecording(VideoCaptureSession session)
        {
            Console.WriteLine("ready for video recording");
            _getCameraCellFunc.Invoke()?.VideoRecodingDidBecomeReady();
        }

        public void DidStartVideoRecording(VideoCaptureSession session)
        {
            Console.WriteLine("did start video recording");
            UpdateCameraCellRecordingStatusIfNeeded(true, true);
        }

        public void DidCancelVideoRecording(VideoCaptureSession session)
        {
            Console.WriteLine("did cancel video recording");
            UpdateCameraCellRecordingStatusIfNeeded(false, true);
        }

        public void DidFinishVideoRecording(VideoCaptureSession session, NSUrl videoUrl)
        {
            Console.WriteLine("did finish video recording");
            UpdateCameraCellRecordingStatusIfNeeded(false, true);
        }

        public void DidInterruptVideoRecording(VideoCaptureSession session, NSUrl videoUrl, NSError reason)
        {
            Console.WriteLine($"did interrupt video recording, reason: {reason}");
            UpdateCameraCellRecordingStatusIfNeeded(false, true);
        }

        public void DidFailVideoRecording(VideoCaptureSession session, NSError error)
        {
            Console.WriteLine("did fail video recording");
            UpdateCameraCellRecordingStatusIfNeeded(false, true);
        }

        private void UpdateCameraCellRecordingStatusIfNeeded(bool isRecording, bool animated)
        {
            _getCameraCellFunc.Invoke()?.UpdateRecordingVideoStatus(isRecording, animated);
        }
    }
}