using Softeq.ImagePicker.Infrastructure;
using Softeq.ImagePicker.Infrastructure.Interfaces;
using Softeq.ImagePicker.Public;
using Softeq.ImagePicker.Views;

namespace Softeq.ImagePicker;

public class ImagePickerDelegate : UICollectionViewDelegateFlowLayout
{
    private readonly IImagePickerDelegate _delegate;

    public ImagePickerLayout Layout { get; }

    public ImagePickerDelegate(ImagePickerLayout layout, IImagePickerDelegate imagePickerDelegate = null)
    {
        Layout = layout;
        _delegate = imagePickerDelegate;
    }

    public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout,
        NSIndexPath indexPath)
    {
        return Layout.CollectionView(collectionView, layout, indexPath);
    }

    public override UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout,
        nint section)
    {
        return Layout.CollectionView(collectionView, layout, (int)section);
    }

    public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
    {
        if (indexPath.Section == Layout.Configuration.SectionIndexForAssets)
        {
            _delegate?.DidSelectAssetItemAt(indexPath.Row);
        }
    }

    public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
    {
        if (indexPath.Section == Layout.Configuration.SectionIndexForAssets)
        {
            _delegate?.DidDeselectAssetItemAt(indexPath.Row);
        }
    }

    public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
    {
        return ShouldSelectItem(indexPath.Section, Layout.Configuration);
    }

    public override bool ShouldHighlightItem(UICollectionView collectionView, NSIndexPath indexPath)
    {
        return ShouldHighlightItem(indexPath.Section, Layout.Configuration);
    }

    public override void ItemHighlighted(UICollectionView collectionView, NSIndexPath indexPath)
    {
        if (indexPath.Section == Layout.Configuration.SectionIndexForActions)
        {
            _delegate?.DidSelectActionItemAt(indexPath.Row);
        }
    }

    public override void WillDisplayCell(UICollectionView collectionView, UICollectionViewCell cell,
        NSIndexPath indexPath)
    {
        switch (indexPath.Section)
        {
            case var section when section == Layout.Configuration.SectionIndexForActions:
                _delegate?.WillDisplayActionCell(cell, indexPath.Row);
                break;
            case var section when section == Layout.Configuration.SectionIndexForCamera:
                _delegate?.WillDisplayCameraCell(cell as CameraCollectionViewCell);
                break;
            case var section when section == Layout.Configuration.SectionIndexForAssets:
                _delegate?.WillDisplayAssetCell(cell as ImagePickerAssetCell, indexPath.Row);
                break;
            default: throw new ImagePickerException("index path not supported");
        }
    }

    public override void CellDisplayingEnded(UICollectionView collectionView, UICollectionViewCell cell,
        NSIndexPath indexPath)
    {
        switch (indexPath.Section)
        {
            case var section when section == Layout.Configuration.SectionIndexForCamera:
                _delegate?.DidEndDisplayingCameraCell(cell as CameraCollectionViewCell);
                break;
            case var section when section == Layout.Configuration.SectionIndexForActions ||
                                  section == Layout.Configuration.SectionIndexForAssets:
                break;
            default: throw new ImagePickerException("index path not supported");
        }
    }

    public override void Scrolled(UIScrollView scrollView)
    {
        _delegate?.DidScroll(scrollView);
    }

    /// <summary>
    /// We allow selecting only asset items, action items are only highlighted,
    /// camera item is untouched.
    /// </summary>
    private static bool ShouldSelectItem(int section, LayoutConfiguration layoutConfiguration)
    {
        if (layoutConfiguration.SectionIndexForActions == section ||
            layoutConfiguration.SectionIndexForCamera == section)
        {
            return false;
        }

        return true;
    }

    private static bool ShouldHighlightItem(int section, LayoutConfiguration layoutConfiguration)
    {
        return layoutConfiguration.SectionIndexForCamera != section;
    }
}