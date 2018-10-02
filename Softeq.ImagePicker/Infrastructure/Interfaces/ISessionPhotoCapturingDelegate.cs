using AVFoundation;
using Foundation;
using Softeq.ImagePicker.Media.Capture;

namespace Softeq.ImagePicker.Infrastructure.Interfaces
{
    public interface ISessionPhotoCapturingDelegate
    {
        /// <summary>
        /// called as soon as the photo was taken, use this to update UI - for example show flash animation or live photo icon
        /// </summary>
        /// <param name="settings">Settings.</param>
        void WillCapturePhotoWith(AVCapturePhotoSettings settings);

        /// <summary>
        /// called when captured photo is processed and ready for use
        /// </summary>
        /// <param name="didCapturePhotoData">Did capture photo data.</param>
        /// <param name="settings">Settings.</param>
        void DidCapturePhotoData(NSData didCapturePhotoData, AVCapturePhotoSettings settings);

        /// <summary>
        /// called when captured photo is processed and ready for use
        /// </summary>
        /// <param name="error">Error.</param>
        void DidFailCapturingPhotoWith(NSError error);

        /// <summary>
        /// called when number of processing live photos changed, see inProgressLivePhotoCapturesCount for current count
        /// </summary>
        /// <param name="session">Session.</param>
        void CaptureSessionDidChangeNumberOfProcessingLivePhotos(PhotoCaptureSession session);
    }
}