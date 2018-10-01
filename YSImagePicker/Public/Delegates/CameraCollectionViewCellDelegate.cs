using System;
using UIKit;
using YSImagePicker.Enums;
using YSImagePicker.Interfaces;
using YSImagePicker.Media.Capture;

namespace YSImagePicker.Public.Delegates
{
    public class CameraCollectionViewCellDelegate : ICameraCollectionViewCellDelegate
    {
        private readonly Func<CameraCollectionViewCell> _getCameraCellFunc;
        private readonly CaptureSession _captureSession;
        private readonly CaptureSettings _captureSettings;

        public CameraCollectionViewCellDelegate(Func<CameraCollectionViewCell> getCameraCellFunc,
            CaptureSession captureSession, CaptureSettings captureSettings)
        {
            _getCameraCellFunc = getCameraCellFunc;
            _captureSession = captureSession;
            _captureSettings = captureSettings;
        }

        public void TakePicture()
        {
            _captureSession.PhotoCaptureSession.CapturePhoto(LivePhotoMode.Off,
                _captureSettings.SavesCapturedPhotosToPhotoLibrary);
        }

        public void TakeLivePhoto()
        {
            _captureSession.PhotoCaptureSession.CapturePhoto(LivePhotoMode.On,
                _captureSettings.SavesCapturedLivePhotosToPhotoLibrary);
        }

        public void StartVideoRecording()
        {
            _captureSession.VideoCaptureSession?.StartVideoRecording(_captureSettings
                .SavesCapturedVideosToPhotoLibrary);
        }

        public void StopVideoRecording()
        {
            _captureSession.VideoCaptureSession?.StopVideoRecording();
        }

        public void FlipCamera(Action completion)
        {
            if (_captureSession == null)
            {
                return;
            }

            var cameraCell = _getCameraCellFunc.Invoke();
            if (cameraCell == null)
            {
                _captureSession.ChangeCamera(completion);
                return;
            }

            // 1. blur cell
            cameraCell.BlurIfNeeded(true, () =>
            {
                {
                    // 2. flip camera
                    _captureSession.ChangeCamera(() =>
                    {
                        UIView.Transition(cameraCell.PreviewView, 0.25,
                            UIViewAnimationOptions.TransitionFlipFromLeft | UIViewAnimationOptions.AllowAnimatedContent,
                            null, () => { cameraCell.UnblurIfNeeded(true, completion); });
                    });
                }
            });
        }
    }
}