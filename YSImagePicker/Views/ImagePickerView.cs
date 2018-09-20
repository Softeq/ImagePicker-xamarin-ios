using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace YSImagePicker.Views
{
    [Register("ImagePickerView")]
    public partial class ImagePickerView : UICollectionView
    {

        public UICollectionView UICollectionView => CollectionView;
        
        public ImagePickerView(NSCoder coder) : base(coder)
        {
        }

        protected ImagePickerView(NSObjectFlag t) : base(t)
        {
        }

        protected internal ImagePickerView(IntPtr handle) : base(handle)
        {
        }

        public ImagePickerView(CGRect frame, UICollectionViewLayout layout) : base(frame, layout)
        {
        }
    }
}