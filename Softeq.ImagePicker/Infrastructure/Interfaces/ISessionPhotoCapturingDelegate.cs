using AVFoundation;
using Foundation;
using Softeq.ImagePicker.Media.Capture;

namespace Softeq.ImagePicker.Infrastructure.Interfaces
{
    public interface ISessionPhotoCapturingDelegate
    {
        /// called as soon as the photo was taken, use this to update UI - for example show flash animation or live photo icon
        void WillCapturePhotoWith(AVCapturePhotoSettings settings);

        /// called when captured photo is processed and ready for use
        void DidCapturePhotoData(NSData didCapturePhotoData, AVCapturePhotoSettings settings);

        /// called when captured photo is processed and ready for use
        void DidFailCapturingPhotoWith(NSError error);

        /// called when number of processing live photos changed, see inProgressLivePhotoCapturesCount for current count
        void CaptureSessionDidChangeNumberOfProcessingLivePhotos(PhotoCaptureSession session);
    }
}