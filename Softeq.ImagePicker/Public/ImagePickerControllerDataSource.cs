using Photos;
using UIKit;

namespace Softeq.ImagePicker.Public
{
    /// <summary>
    /// Image picker may ask for additional resources, implement this protocol to fully support
    /// all features.
    /// </summary>
    public abstract class ImagePickerControllerDataSource
    {
        /// <summary>
        /// Asks for a view that is placed as overlay view with permissions info
        /// when user did not grant or has restricted access to photo library.
        /// </summary>
        public abstract UIView ImagePicker(PHAuthorizationStatus status);
    }
}