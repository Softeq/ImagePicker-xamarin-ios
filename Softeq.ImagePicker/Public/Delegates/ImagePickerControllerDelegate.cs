using Photos;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker.Public.Delegates
{
    ///
    /// Group of methods informing what image picker is currently doing
    ///
    public class ImagePickerControllerDelegate
    {
        ///
        /// Called when user taps on an action item, index is either 0 or 1 depending which was tapped
        ///
        public virtual void DidSelectActionItemAt(ImagePickerController controller, int index)
        {
        }

        ///
        /// Called when user select an asset.
        ///
        public virtual void DidSelectAsset(ImagePickerController controller, PHAsset asset)
        {
        }

        ///
        /// Called when user unselect previously selected asset.
        ///
        public virtual void DidDeselectAsset(ImagePickerController controller, PHAsset asset)
        {
        }

        ///
        /// Called when user takes new photo.
        ///
        public virtual void DidTake(UIImage image)
        {
        }

        ///
        /// Called right before an action item collection view cell is displayed. Use this method
        /// to configure your cell.
        ///
        public virtual void WillDisplayActionItem(ImagePickerController controller, UICollectionViewCell cell,
            int index)
        {
        }

        ///
        /// Called right before an asset item collection view cell is displayed. Use this method
        /// to configure your cell based on asset media type, subtype, etc.
        ///
        public virtual void WillDisplayAssetItem(ImagePickerController controller, ImagePickerAssetCell cell,
            PHAsset asset)
        {
        }
    }
}