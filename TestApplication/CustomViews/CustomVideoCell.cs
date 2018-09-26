using System;
using Foundation;
using UIKit;
using YSImagePicker.Views;

namespace TestApplication.CustomViews
{
    [Register("CustomVideoCell")]
    public partial class CustomVideoCell : ImagePickerAssetCell
    {
        public override UIImageView ImageView => InternalImageView;
        public UILabel Label => InternalLabel;
        
        public CustomVideoCell(IntPtr handle) : base(handle)
        {
        }
    }
}