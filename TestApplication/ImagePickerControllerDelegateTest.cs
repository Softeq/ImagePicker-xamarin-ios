using System;
using System.Collections.Generic;
using Photos;
using UIKit;
using YSImagePicker.Public;

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

//        public override void WillDisplayActionItem(ImagePickerController controller, UICollectionViewCell cell, int index)
//        {
////            switch (cell)
////            {
////                case var iconWithTextCell is IconWithTextCell:
////                    iconWithTextCell.TitleLabel.TextColor = UIColor.Black;
//                switch (index) 
//                {
//                    case 0:
//                    iconWithTextCell.titleLabel.text = "Camera"
//                    iconWithTextCell.imageView.image = #imageLiteral(resourceName: "button-camera")
//                    case 1:
//                    iconWithTextCell.titleLabel.text = "Photos"
//                    iconWithTextCell.imageView.image = #imageLiteral(resourceName: "button-photo-library")
//                        default: break
//                }
//            }
    }
}