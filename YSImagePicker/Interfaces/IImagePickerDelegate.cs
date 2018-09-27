using UIKit;
using YSImagePicker.Public;
using YSImagePicker.Views;

namespace YSImagePicker.Interfaces
{
    public interface IImagePickerDelegate
    {
        /// Called when user selects one of action items
        void DidSelectActionItemAt(ImagePickerDelegate imagePickerDelegate, int index);

        /// Called when user selects one of asset items
        void DidSelectAssetItemAt(ImagePickerDelegate imagePickerDelegate, int index);

        /// Called when user deselects one of selected asset items
        void DidDeselectAssetItemAt(ImagePickerDelegate imagePickerDelegate, int index);

        /// Called when action item is about to be displayed
        void WillDisplayActionCell(ImagePickerDelegate imagePickerDelegate, UICollectionViewCell cell, int index);

        /// Called when camera item is about to be displayed
        void WillDisplayCameraCell(ImagePickerDelegate imagePickerDelegate, CameraCollectionViewCell cell);

        /// Called when camera item ended displaying
        void DidEndDisplayingCameraCell(ImagePickerDelegate imagePickerDelegate, CameraCollectionViewCell cell);

        void WillDisplayAssetCell(ImagePickerDelegate imagePickerDelegate, ImagePickerAssetCell cell, int index);

        //func imagePicker(delegate: ImagePickerDelegate, didEndDisplayingAssetCell cell: ImagePickerAssetCell)
        void DidScroll(ImagePickerDelegate imagePickerDelegate, UIScrollView scrollView);
    }
}