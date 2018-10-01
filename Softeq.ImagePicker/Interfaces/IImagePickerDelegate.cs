using Softeq.ImagePicker.Public;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker.Interfaces
{
    public interface IImagePickerDelegate
    {
        /// Called when user selects one of action items
        void DidSelectActionItemAt(int index);

        /// Called when user selects one of asset items
        void DidSelectAssetItemAt(int index);

        /// Called when user deselects one of selected asset items
        void DidDeselectAssetItemAt(int index);

        /// Called when action item is about to be displayed
        void WillDisplayActionCell(UICollectionViewCell cell, int index);

        /// Called when camera item is about to be displayed
        void WillDisplayCameraCell(CameraCollectionViewCell cell);

        /// Called when camera item ended displaying
        void DidEndDisplayingCameraCell(CameraCollectionViewCell cell);

        void WillDisplayAssetCell(ImagePickerAssetCell cell, int index);

        //func imagePicker(delegate: ImagePickerDelegate, didEndDisplayingAssetCell cell: ImagePickerAssetCell)
        void DidScroll(UIScrollView scrollView);
    }
}