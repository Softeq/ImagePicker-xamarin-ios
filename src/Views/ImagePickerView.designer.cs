// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//

using Foundation;

namespace Softeq.ImagePicker.Views
{
    [Register ("ImagePickerView")]
    partial class ImagePickerView
    {
        [Outlet]
        UIKit.UICollectionView CollectionView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CollectionView != null) {
                CollectionView.Dispose ();
                CollectionView = null;
            }
        }
    }
}