using System;
using System.Collections.Generic;
using Foundation;
using Photos;
using Softeq.ImagePicker.Infrastructure.Extensions;
using Softeq.ImagePicker.Public;
using Softeq.ImagePicker.Sample.CustomViews;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker.Sample;

public class ImagePickerControllerDelegate : Softeq.ImagePicker.Public.Delegates.ImagePickerControllerDelegate
{
    public Action<int>? DidSelectActionItemAction { get; set; }
    public Action<IReadOnlyList<PHAsset>>? DidSelectAssetAction { get; set; }
    public Action<IReadOnlyList<PHAsset>>? DidDeselectAssetAction { get; set; }
    public Action<UIImage>? DidTakeAssetAction { get; set; }

    public override void DidSelectActionItemAt(ImagePickerController controller, int index)
    {
        DidSelectActionItemAction?.Invoke(index);
    }

    public override void DidSelectAsset(ImagePickerController controller, PHAsset asset)
    {
        DidSelectAssetAction?.Invoke(controller.SelectedAssets);
    }

    public override void DidDeselectAsset(ImagePickerController controller, PHAsset asset)
    {
        DidDeselectAssetAction?.Invoke(controller.SelectedAssets);
    }

    public override void DidTake(UIImage image)
    {
        DidTakeAssetAction?.Invoke(image);
    }

    public override void WillDisplayActionItem(ImagePickerController controller, UICollectionViewCell cell,
        int index)
    {
        if (cell is IconWithTextCell iconWithTextCell)
        {
            iconWithTextCell.TitleLabel.TextColor = UIColor.Black;

            switch (index)
            {
                case 0:
                    iconWithTextCell.TitleLabel.Text = "Camera";
                    iconWithTextCell.ImageView.Image = UIImageExtensions.FromBundle(BundleAssets.ButtonCamera);
                    break;
                case 1:
                    iconWithTextCell.TitleLabel.Text = "Photos";
                    iconWithTextCell.ImageView.Image =
                        UIImageExtensions.FromBundle(BundleAssets.ButtonPhotoLibrary);
                    break;
            }
        }
    }

    public override void WillDisplayAssetItem(ImagePickerController controller, ImagePickerAssetCell cell,
        PHAsset asset)
    {
        switch (cell)
        {
            case var _ when cell is CustomVideoCell videoCell:
                videoCell.Label.Text = GetDurationFormatter().StringFromTimeInterval(asset.Duration);
                break;
            case var _ when cell is CustomImageCell imageCell:
                switch (asset.MediaSubtypes)
                {
                    case PHAssetMediaSubtype.PhotoLive:
                        imageCell.SubtypeImage.Image = UIImage.FromBundle("icon-live");
                        break;
                    case PHAssetMediaSubtype.PhotoPanorama:
                        imageCell.SubtypeImage.Image = UIImage.FromBundle("icon-pano");
                        break;
                    default:
                        {
                            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 2) &&
                                asset.MediaSubtypes == PHAssetMediaSubtype.PhotoDepthEffect)
                            {
                                imageCell.SubtypeImage.Image = UIImage.FromBundle("icon-depth");
                            }

                            break;
                        }
                }

                break;
        }
    }

    private static NSDateComponentsFormatter GetDurationFormatter()
    {
        var formatter = new NSDateComponentsFormatter
        {
            UnitsStyle = NSDateComponentsFormatterUnitsStyle.Positional,
            AllowedUnits = NSCalendarUnit.Minute | NSCalendarUnit.Second,
            ZeroFormattingBehavior = NSDateComponentsFormatterZeroFormattingBehavior.Pad
        };
        return formatter;
    }
}