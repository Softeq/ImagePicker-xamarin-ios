using System;
using AVFoundation;
using Foundation;
using UIKit;
using YSImagePicker.Interfaces;
using YSImagePicker.Media.Capture;
using YSImagePicker.Public;
using YSImagePicker.Public.Delegates;

namespace YSImagePicker.Media.Delegates
{
    public class SessionPhotoCapturingDelegate : ISessionPhotoCapturingDelegate
    {
        private readonly Func<CameraCollectionViewCell> _getCameraCellFunc;
        private readonly ImagePickerControllerDelegate _imagePickerControllerDelegate;

        public SessionPhotoCapturingDelegate(Func<CameraCollectionViewCell> getCameraCellFunc,
            ImagePickerControllerDelegate @delegate)
        {
            _getCameraCellFunc = getCameraCellFunc;
            _imagePickerControllerDelegate = @delegate;
        }

        public void WillCapturePhotoWith(PhotoCaptureSession session, AVCapturePhotoSettings settings)
        {
            Console.WriteLine($"will capture photo {settings.UniqueID}");
        }

        public void DidCapturePhotoData(PhotoCaptureSession session, NSData didCapturePhotoData,
            AVCapturePhotoSettings settings)
        {
            Console.WriteLine($"did capture photo {settings.UniqueID}");
            _imagePickerControllerDelegate?.DidTake(UIImage.LoadFromData(didCapturePhotoData));
        }

        public void DidFailCapturingPhotoWith(PhotoCaptureSession session, NSError error)
        {
            Console.WriteLine($"did fail capturing: {error}");
        }

        public void CaptureSessionDidChangeNumberOfProcessingLivePhotos(PhotoCaptureSession session)
        {
            var cameraCell = _getCameraCellFunc?.Invoke();

            if (cameraCell == null)
            {
                return;
            }

            var count = session.InProgressLivePhotoCapturesCount;
            cameraCell.UpdateLivePhotoStatus(count > 0, true);
        }
    }
}