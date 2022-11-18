using System.ComponentModel;

namespace Softeq.ImagePicker.Infrastructure.Extensions;

public static class UIImageExtensions
{
    public static UIImage FromBundle(BundleAssets asset)
    {
        return UIImage.FromBundle(GetDescription(asset));
    }

    static string GetDescription(Enum en)
    {
        var type = en.GetType();

        var memInfo = type.GetMember(en.ToString());

        var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

        return ((DescriptionAttribute)attrs[0]).Description;
    }
}

public enum BundleAssets
{
    [Description("button-camera")] ButtonCamera,
    [Description("button-photo-library")] ButtonPhotoLibrary,
    [Description("icon-check-background")] IconCheckBackground,
    [Description("icon-check")] IconCheck,
    [Description("gradient")] Gradient,
    [Description("icon-badge-livephoto")] IconBadgeLivePhoto,
    [Description("icon-badge-video")] IconBadgeVideo,
}