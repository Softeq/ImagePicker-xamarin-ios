using System;

namespace Softeq.ImagePicker.Infrastructure.Interfaces
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