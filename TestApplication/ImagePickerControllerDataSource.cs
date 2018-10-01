using CoreGraphics;
using Photos;
using UIKit;
using YSImagePicker.Public;

namespace TestApplication
{
    public class ImagePickerControllerDataSource : YSImagePicker.Public.ImagePickerControllerDataSource
    {
        public override UIView ImagePicker(PHAuthorizationStatus status)
        {
            var infoLabel = new UILabel(CGRect.Empty)
            {
                BackgroundColor = UIColor.Green, TextAlignment = UITextAlignment.Center, Lines = 0
            };
            switch (status)
            {
                case PHAuthorizationStatus.Restricted:
                    infoLabel.Text = "Access is restricted\n\nPlease open Settings app and update privacy settings.";
                    break;
                case PHAuthorizationStatus.Denied:
                    infoLabel.Text =
                        "Access is denied by user\n\nPlease open Settings app and update privacy settings.";
                    break;
            }

            return infoLabel;
        }
    }
}