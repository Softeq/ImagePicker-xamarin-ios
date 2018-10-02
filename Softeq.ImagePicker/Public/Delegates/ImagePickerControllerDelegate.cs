using Photos;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker.Public.Delegates
{
    /// <summary>
    /// Group of methods informing what image picker is currently doing
    /// </summary>
    public class ImagePickerControllerDelegate
    {
        /// <summary>
        /// Called when user taps on an action item, index is either 0 or 1 depending which was tapped
        /// </summary>
        /// <param name="controller">Controller.</param>
        /// <param name="index">Index.</param>
        public virtual void DidSelectActionItemAt(ImagePickerController controller, int index)
        {
        }

        /// <summary>
        /// Called when user select an asset.
        /// </summary>
        /// <param name="controller">Controller.</param>
        /// <param name="asset">Asset.</param>
        public virtual void DidSelectAsset(ImagePickerController controller, PHAsset asset)
        {
        }

        /// <summary>
        /// Called when user unselect previously selected asset.
        /// </summary>
        /// <param name="controller">Controller.</param>
        /// <param name="asset">Asset.</param>
        public virtual void DidDeselectAsset(ImagePickerController controller, PHAsset asset)
        {
        }

        /// <summary>
        /// Called when user takes new photo.
        /// </summary>
        /// <param name="image">Image.</param>
        public virtual void DidTake(UIImage image)
        {
        }

        /// <summary>
        /// Called right before an action item collection view cell is displayed. Use this method
        /// to configure your cell.
        /// </summary>
        /// <param name="controller">Controller.</param>
        /// <param name="cell">Cell.</param>
        /// <param name="index">Index.</param>
        public virtual void WillDisplayActionItem(ImagePickerController controller, UICollectionViewCell cell,
            int index)
        {
        }

        /// <summary>
        /// Called right before an asset item collection view cell is displayed. Use this method
        /// to configure your cell based on asset media type, subtype, etc.
        /// </summary>
        /// <param name="controller">Controller.</param>
        /// <param name="cell">Cell.</param>
        /// <param name="asset">Asset.</param>
        public virtual void WillDisplayAssetItem(ImagePickerController controller, ImagePickerAssetCell cell,
            PHAsset asset)
        {
        }
    }
}