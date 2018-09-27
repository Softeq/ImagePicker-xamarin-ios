using System;
using CoreGraphics;
using Foundation;
using UIKit;
using YSImagePicker.Public;
using YSImagePicker.Views;

namespace YSImagePicker
{
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
            return Layout.CollectionView(collectionView, layout, (int) section);
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (indexPath.Section == Layout.Configuration.SectionIndexForAssets)
            {
                _delegate?.DidSelectAssetItemAt(this, indexPath.Row);
            }
        }

        public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (indexPath.Section == Layout.Configuration.SectionIndexForAssets)
            {
                _delegate?.DidDeselectAssetItemAt(this, indexPath.Row);
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
                _delegate?.DidSelectActionItemAt(this, indexPath.Row);
            }
        }

        public override void WillDisplayCell(UICollectionView collectionView, UICollectionViewCell cell,
            NSIndexPath indexPath)
        {
            switch (indexPath.Section)
            {
                case var section when section == Layout.Configuration.SectionIndexForActions:
                    _delegate?.WillDisplayActionCell(this, cell, indexPath.Row);
                    break;
                case var section when section == Layout.Configuration.SectionIndexForCamera:
                    _delegate?.WillDisplayCameraCell(this, cell as CameraCollectionViewCell);
                    break;
                case var section when section == Layout.Configuration.SectionIndexForAssets:
                    _delegate?.WillDisplayAssetCell(this, cell as ImagePickerAssetCell, indexPath.Row);
                    break;
                default: throw new Exception("index path not supported");
            }
        }

        public override void CellDisplayingEnded(UICollectionView collectionView, UICollectionViewCell cell,
            NSIndexPath indexPath)
        {
            switch (indexPath.Section)
            {
                case var section when section == Layout.Configuration.SectionIndexForCamera:
                    _delegate?.DidEndDisplayingCameraCell(this, cell as CameraCollectionViewCell);
                    break;
                case var section when section == Layout.Configuration.SectionIndexForActions ||
                                      section == Layout.Configuration.SectionIndexForAssets:
                    break;
                default: throw new Exception("index path not supported");
            }
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            _delegate?.DidScroll(this, scrollView);
        }
        
        ///
        /// We allow selecting only asset items, action items are only highlighted,
        /// camera item is untouched.
        ///
        private bool ShouldSelectItem(int section, LayoutConfiguration layoutConfiguration)
        {
            if (layoutConfiguration.SectionIndexForActions == section ||
                layoutConfiguration.SectionIndexForCamera == section)
            {
                return false;
            }

            return true;
        }

        private bool ShouldHighlightItem(int section, LayoutConfiguration layoutConfiguration)
        {
            if (layoutConfiguration.SectionIndexForCamera == section)
            {
                return false;
            }

            return true;
        }
    }
}