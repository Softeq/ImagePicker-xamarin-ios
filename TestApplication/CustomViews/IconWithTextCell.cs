using System;
using Foundation;
using UIKit;

namespace TestApplication.CustomViews
{
    [Register("IconWithTextCell")]
    public partial class IconWithTextCell : UICollectionViewCell
    {
        public UILabel Label => TitleLabel;
        public UIImageView ImageView => InternalImageView;

        public IconWithTextCell(IntPtr handle) : base(handle)
        {
        }
    }
}