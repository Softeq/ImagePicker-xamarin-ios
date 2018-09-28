using Photos;
using UIKit;

namespace YSImagePicker.Public
{
    ///
    /// Image picker may ask for additional resources, implement this protocol to fully support
    /// all features.
    ///
    public abstract class ImagePickerControllerDataSource
    {
        ///
        /// Asks for a view that is placed as overlay view with permissions info
        /// when user did not grant or has restricted access to photo library.
        ///
        public abstract UIView ImagePicker(ImagePickerController controller, PHAuthorizationStatus status);
    }
}