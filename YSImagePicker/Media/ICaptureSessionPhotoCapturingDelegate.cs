using AVFoundation;
using Foundation;

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
}