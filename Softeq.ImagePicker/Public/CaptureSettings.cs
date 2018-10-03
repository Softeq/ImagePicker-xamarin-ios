using Softeq.ImagePicker.Infrastructure.Enums;

namespace Softeq.ImagePicker.Public
{
    public class CaptureSettings
    {
        /// <summary>
        /// Capture session uses this preset when configuring. Select a preset of
        /// media types you wish to support.
        /// Currently you can not change preset at runtime
        /// </summary>
        public CameraMode CameraMode;

        /// <summary>
        /// Return true if captured photos will be saved to photo library. Image picker
        /// will prompt user with request for permissions when needed. Default value is false
        /// for photos. Live photos and videos are always true.
        /// Please note, that at current implementation this applies to photos only. For
        /// live photos and videos this is always true.
        /// </summary>
        public bool SavesCapturedPhotosToPhotoLibrary;

        public bool SavesCapturedLivePhotosToPhotoLibrary = true;
        public bool SavesCapturedVideosToPhotoLibrary = true;
    }
}