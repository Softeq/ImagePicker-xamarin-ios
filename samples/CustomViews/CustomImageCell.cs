using System;
using Foundation;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker.Sample.CustomViews;

[Register(nameof(CustomImageCell))]
public partial class CustomImageCell : ImagePickerAssetCell
{

    public UIImageView SubtypeImage => SubtypeImageView;

    public CustomImageCell(IntPtr handle) : base(handle)
    {
    }

    public override UIImageView ImageView => InternalImageView;

    public override bool Selected
    {
        get => base.Selected;
        set
        {
            base.Selected = value;
            SelectedImageView.Hidden = !Selected;
        }
    }

    [Export("awakeFromNib")]
    public override void AwakeFromNib()
    {
        base.AwakeFromNib();

        SubtypeImageView.BackgroundColor = UIColor.Clear;

        SelectedImageView.Hidden = !Selected;
    }
}
