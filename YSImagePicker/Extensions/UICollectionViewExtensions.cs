using Foundation;
using UIKit;
using YSImagePicker.Public;
using YSImagePicker.Views;

namespace YSImagePicker.Extensions
{
    public static class UICollectionViewExtensions
    {
        public static CameraCollectionViewCell GetCameraCell(this UICollectionView collectionView,
            LayoutConfiguration layout)
        {
            return collectionView.CellForItem(NSIndexPath.FromItemSection(0, layout.SectionIndexForCamera)) as
                CameraCollectionViewCell;
        }
    }
}