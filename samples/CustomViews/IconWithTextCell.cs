using System;
using Foundation;
using UIKit;

namespace Softeq.ImagePicker.Sample.CustomViews;

[Register(nameof(IconWithTextCell))]
public partial class IconWithTextCell : UICollectionViewCell
{
    public UILabel Label => TitleLabel;
    public UIImageView ImageView => InternalImageView;

    public IconWithTextCell(IntPtr handle) : base(handle)
    {
    }
}