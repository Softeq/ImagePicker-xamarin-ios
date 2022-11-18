namespace Softeq.ImagePicker;

public static class Defines
{
    public static class Common
    {
        public const int SecondsInHour = 3600;
        public const int SecondsInMinute = 60;
    }

    //TODO: Move all colors to the ColorSet and up IOS version to 11
    public static class Colors
    {
        public static readonly UIColor OrangeColor = UIColor.FromRGBA(234 / 255f, 53 / 255f, 52 / 255f, 1);
        public static readonly UIColor YellowColor = UIColor.FromRGBA(245 / 255f, 203 / 255f, 47 / 255f, 1);
        public static readonly UIColor GrayColor = UIColor.FromRGBA(208 / 255f, 213 / 255f, 218 / 255f, 1);
    }
}