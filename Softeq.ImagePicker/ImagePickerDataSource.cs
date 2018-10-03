using System;
using Foundation;
using Photos;
using Softeq.ImagePicker.Infrastructure;
using Softeq.ImagePicker.Public;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker
{
    ///
    /// Datasource for a collection view that is used by Image Picker VC.
    ///
    public class ImagePickerDataSource : UICollectionViewDataSource
    {
        private LayoutModel _layoutModel;
        public CellRegistrator CellRegistrator;
        public ImagePickerAssetModel AssetsModel { get; }

        public ImagePickerDataSource(ImagePickerAssetModel assetsModel)
        {
            AssetsModel = assetsModel;
            _layoutModel = new LayoutModel(new LayoutConfiguration());
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return _layoutModel.NumberOfItems((int) section);
        }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return _layoutModel.NumberOfSections;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (CellRegistrator == null)
            {
                throw new ImagePickerException("cells registrator must be set at this moment");
            }

            switch (indexPath.Section)
            {
                case 0:
                    return GetActionCell(collectionView, indexPath);
                case 1:
                    return GetCameraCell(collectionView, indexPath);
                case 2:
                    return GetAssetCell(collectionView, indexPath);
                default: throw new ImagePickerException("only 3 sections are supported");
            }
        }

        public void UpdateLayoutModel(LayoutModel layoutModel)
        {
            _layoutModel = layoutModel;
        }

        private UICollectionViewCell GetActionCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var identifier = CellRegistrator.CellIdentifier(indexPath.Row);

            if (identifier == null)
            {
                throw new ArgumentException(
                    $"there is an action item at index {indexPath.Row} but no cell is registered.");
            }

            return collectionView.DequeueReusableCell(identifier, indexPath) as UICollectionViewCell;
        }

        private UICollectionViewCell GetCameraCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (collectionView.DequeueReusableCell(CellRegistrator.CellIdentifierForCameraItem, indexPath) is
                CameraCollectionViewCell result)
            {
                return result;
            }

            throw new ArgumentException(
                "there is a camera item but no cell class `CameraCollectionViewCell` is registered.");
        }

        private UICollectionViewCell GetAssetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var asset = (PHAsset) AssetsModel.FetchResult.ObjectAt(indexPath.Item);

            var cellIdentifier = CellRegistrator.CellIdentifier(asset.MediaType) ??
                                 CellRegistrator.CellIdentifierForAssetItems;

            if (!(collectionView.DequeueReusableCell(cellIdentifier, indexPath) is ImagePickerAssetCell cell))
            {
                throw new ArgumentException(
                    $"asset item cell must conform to {nameof(ImagePickerAssetCell)} protocol");
            }

            // Request an image for the asset from the PHCachingImageManager.
            cell.RepresentedAssetIdentifier = asset.LocalIdentifier;

            AssetsModel.ImageManager.RequestImageForAsset(asset, AssetsModel.ThumbnailSize,
                PHImageContentMode.AspectFill,
                null, (image, info) =>
                {
                    // The cell may have been recycled by the time this handler gets called;
                    // set the cell's thumbnail image only if it's still showing the same asset.
                    if (cell.RepresentedAssetIdentifier == asset.LocalIdentifier && image != null)
                    {
                        cell.ImageView.Image = image;
                    }
                });

            return cell;
        }
    }
}