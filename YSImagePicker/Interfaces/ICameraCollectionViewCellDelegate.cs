using System;

namespace YSImagePicker.Interfaces
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