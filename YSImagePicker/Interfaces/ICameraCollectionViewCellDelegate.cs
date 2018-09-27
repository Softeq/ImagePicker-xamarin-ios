using System;

namespace YSImagePicker.Public
{
    public interface ICameraCollectionViewCellDelegate
    {
        void TakePicture();
        void TakeLivePhoto();
        void StartVideoRecording();
        void StopVideoRecording();
        void FlipCamera(Action action);
    }
}