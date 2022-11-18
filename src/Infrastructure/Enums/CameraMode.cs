namespace Softeq.ImagePicker.Infrastructure.Enums;

public enum CameraMode
{
    /// <summary>
    /// If you support only photos use this preset. Default value.
    /// </summary>
    Photo,

    /// <summary>
    /// If you know you will use live photos use this preset.
    /// </summary>
    PhotoAndLivePhoto,

    /// <summary>
    /// If you wish to record videos or take photos.
    /// </summary>
    PhotoAndVideo
}