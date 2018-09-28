namespace YSImagePicker.Public
{
    public class CaptureSettings
    {
        ///
        /// Capture session uses this preset when configuring. Select a preset of
        /// media types you wish to support.
        ///
        /// - note: currently you can not change preset at runtime
        ///
        public CameraMode CameraMode;

        ///
        /// Return true if captured photos will be saved to photo library. Image picker
        /// will prompt user with request for permissions when needed. Default value is false
        /// for photos. Live photos and videos are always true.
        ///
        /// - note: please note, that at current implementation this applies to photos only. For
        /// live photos and videos this is always true.
        ///
        public bool SavesCapturedPhotosToPhotoLibrary;

        public bool SavesCapturedLivePhotosToPhotoLibrary = true;
        public bool SavesCapturedVideosToPhotoLibrary = true;
    }
}