using Softeq.ImagePicker.Public;

namespace Softeq.ImagePicker.Infrastructure.Extensions;

public static class UICollectionViewExtensions
{
    public static CameraCollectionViewCell GetCameraCell(this UICollectionView collectionView,
        LayoutConfiguration layout)
    {
        return collectionView.CellForItem(NSIndexPath.FromItemSection(0, layout.SectionIndexForCamera)) as
            CameraCollectionViewCell;
    }
}