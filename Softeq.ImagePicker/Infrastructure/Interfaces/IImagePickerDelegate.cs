using Softeq.ImagePicker.Public;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker.Infrastructure.Interfaces
{
    public interface IImagePickerDelegate
    {
        /// <summary>
        /// Called when user selects one of action items
        /// </summary>
        /// <param name="index">Index.</param>
        void DidSelectActionItemAt(int index);

        /// <summary>
        /// Called when user selects one of asset items
        /// </summary>
        /// <param name="index">Index.</param>
        void DidSelectAssetItemAt(int index);

        /// <summary>
        /// Called when user deselects one of selected asset items
        /// </summary>
        /// <param name="index">Index.</param>
        void DidDeselectAssetItemAt(int index);

        /// <summary>
        /// Called when action item is about to be displayed
        /// </summary>
        /// <param name="cell">Cell.</param>
        /// <param name="index">Index.</param>
        void WillDisplayActionCell(UICollectionViewCell cell, int index);

        /// <summary>
        /// Called when camera item is about to be displayed
        /// </summary>
        /// <param name="cell">Cell.</param>
        void WillDisplayCameraCell(CameraCollectionViewCell cell);

        /// <summary>
        /// Called when camera item ended displaying
        /// </summary>
        /// <param name="cell">Cell.</param>
        void DidEndDisplayingCameraCell(CameraCollectionViewCell cell);

        void WillDisplayAssetCell(ImagePickerAssetCell cell, int index);

        void DidScroll(UIScrollView scrollView);
    }
}