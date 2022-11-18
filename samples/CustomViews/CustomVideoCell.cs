using System;
using Foundation;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker.Sample.CustomViews
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