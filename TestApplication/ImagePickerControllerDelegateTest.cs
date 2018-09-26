using System;
using System.Collections.Generic;
using Foundation;
using ObjCRuntime;
using Photos;
using TestApplication.CustomViews;
using UIKit;
using YSImagePicker.Public;
using YSImagePicker.Views;

namespace TestApplication
{
    public class ImagePickerControllerDelegateTest : ImagePickerControllerDelegate
    {
        public Action<int> DidSelectActionItemAction { get; set; }
        public Action<IReadOnlyList<PHAsset>> DidSelectAssetAction { get; set; }
        public Action<IReadOnlyList<PHAsset>> DidDeselectAssetAction { get; set; }
        public Action<UIImage> DidTakeAssetAction { get; set; }

        public override void DidSelectActionItemAt(ImagePickerController controller, int index)
        {
            DidSelectActionItemAction?.Invoke(index);
        }

        public override void DidSelect(ImagePickerController controller, PHAsset asset)
        {
            DidSelectAssetAction?.Invoke(controller.SelectedAssets);
        }

        public override void DidDeselectAsset(ImagePickerController controller, PHAsset asset)
        {
            DidDeselectAssetAction?.Invoke(controller.SelectedAssets);
        }

        public override void DidTake(ImagePickerController controller, UIImage image)
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
                        iconWithTextCell.ImageView.Image = UIImage.FromBundle("button-camera");
                        break;
                    case 1:
                        iconWithTextCell.TitleLabel.Text = "Photos";
                        iconWithTextCell.ImageView.Image = UIImage.FromBundle("button-photo-library");
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
}