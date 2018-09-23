using System;
using CoreGraphics;
using Foundation;
using UIKit;
using YSImagePicker.Public;
using YSImagePicker.Views;

namespace YSImagePicker
{
    public interface IImagePickerDelegateDelegate
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

    /// Informs a delegate what is going on in ImagePickerDelegate
    public class ImagePickerDelegateDelegate : IImagePickerDelegateDelegate
    {
        /// Called when user selects one of action items
        public virtual void DidSelectActionItemAt(ImagePickerDelegate imagePickerDelegate, int index)
        {
        }

        /// Called when user selects one of asset items
        public virtual void DidSelectAssetItemAt(ImagePickerDelegate imagePickerDelegate, int index)
        {
        }

        /// Called when user deselects one of selected asset items
        public virtual void DidDeselectAssetItemAt(ImagePickerDelegate imagePickerDelegate, int index)
        {
        }

        /// Called when action item is about to be displayed
        public virtual void
            WillDisplayActionCell(ImagePickerDelegate imagePickerDelegate, UICollectionViewCell cell, int index)
        {
        }

        /// Called when camera item is about to be displayed
        public virtual void WillDisplayCameraCell(ImagePickerDelegate imagePickerDelegate,
            CameraCollectionViewCell cell)
        {
        }

        /// Called when camera item ended displaying
        public virtual void DidEndDisplayingCameraCell(ImagePickerDelegate imagePickerDelegate,
            CameraCollectionViewCell cell)
        {
        }

        public void WillDisplayAssetCell(ImagePickerDelegate imagePickerDelegate, ImagePickerAssetCell cell, int index)
        {
        }

//func imagePicker(delegate: ImagePickerDelegate, didEndDisplayingAssetCell cell: ImagePickerAssetCell)
        public virtual void DidScroll(ImagePickerDelegate imagePickerDelegate, UIScrollView scrollView)
        {
        }
    }

    public class ImagePickerDelegate : UICollectionViewDelegateFlowLayout
    {
        public ImagePickerLayout Layout;
    
        public IImagePickerDelegateDelegate Delegate;

        private ImagePickerSelectionPolicy selectionPolicy = new ImagePickerSelectionPolicy();

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout,
            NSIndexPath indexPath)
        {
            return Layout?.CollectionView(collectionView, layout, indexPath) ?? CGSize.Empty;
        }

        public override UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout,
            nint section)
        {
            return Layout?.CollectionView(collectionView, layout, (int) section) ?? UIEdgeInsets.Zero;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (indexPath.Section == Layout?.Configuration.SectionIndexForAssets)
            {
                Delegate?.DidSelectAssetItemAt(this, indexPath.Row);
            }
        }

        public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (indexPath.Section == Layout?.Configuration.SectionIndexForAssets)
            {
                Delegate?.DidDeselectAssetItemAt(this, indexPath.Row);
            }
        }

        public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (Layout?.Configuration == null)
            {
                return false;
            }

            return selectionPolicy.ShouldSelectItem(indexPath.Section, Layout.Configuration);
        }

        public override bool ShouldHighlightItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (Layout?.Configuration == null)
            {
                return false;
            }

            return selectionPolicy.ShouldHighlightItem(indexPath.Section, Layout.Configuration);
        }

        public override void ItemHighlighted(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (indexPath.Section == Layout?.Configuration.SectionIndexForActions)
            {
                Delegate?.DidSelectActionItemAt(this, indexPath.Row);
            }
        }

        public override void WillDisplayCell(UICollectionView collectionView, UICollectionViewCell cell,
            NSIndexPath indexPath)
        {
            if (Layout?.Configuration == null)
            {
                return;
            }

            switch (indexPath.Section)
            {
                case var section when section == Layout.Configuration.SectionIndexForActions:
                    Delegate?.WillDisplayActionCell(this, cell, indexPath.Row);
                    break;
                case var section when section == Layout.Configuration.SectionIndexForCamera:
                    Delegate?.WillDisplayCameraCell(this, cell as CameraCollectionViewCell);
                    break;
                case var section when section == Layout.Configuration.SectionIndexForAssets:
                    Delegate?.WillDisplayAssetCell(this, cell as ImagePickerAssetCell, indexPath.Row);
                    break;
                default: throw new Exception("index path not supported");
            }
        }

        public override void CellDisplayingEnded(UICollectionView collectionView, UICollectionViewCell cell,
            NSIndexPath indexPath)
        {
            if (Layout?.Configuration == null)
            {
                return;
            }

            switch (indexPath.Section)
            {
                case var section when section == Layout.Configuration.SectionIndexForCamera:
                    Delegate?.DidEndDisplayingCameraCell(this, cell as CameraCollectionViewCell);
                    break;
                case var section when section == Layout.Configuration.SectionIndexForActions ||
                                      section == Layout.Configuration.SectionIndexForAssets:
                    break;
                default: throw new Exception("index path not supported");
            }
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            Delegate?.DidScroll(this, scrollView);
        }
    }
}