using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace YSImagePicker.Views
{
    public partial class ImagePickerView : UIView
    {

        public UICollectionView UICollectionView => CollectionView;

        protected internal ImagePickerView(IntPtr handle) : base(handle)
        {
        }
    }
}