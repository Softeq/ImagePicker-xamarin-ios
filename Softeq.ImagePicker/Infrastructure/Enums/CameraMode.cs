namespace Softeq.ImagePicker.Infrastructure.Enums
{
    public enum CameraMode
    {
        ///
        /// If you support only photos use this preset. Default value.
        ///
        Photo,

        ///
        /// If you know you will use live photos use this preset.
        ///
        PhotoAndLivePhoto,

        /// If you wish to record videos or take photos.
        PhotoAndVideo
    }
}