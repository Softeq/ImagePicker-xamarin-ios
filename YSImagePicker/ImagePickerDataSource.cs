using System;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Photos;
using UIKit;
using YSImagePicker.Public;
using YSImagePicker.Views;

namespace YSImagePicker
{
    ///
    /// Datasource for a collection view that is used by Image Picker VC.
    ///
    public class ImagePickerDataSource : UICollectionViewDataSource
    {
        public LayoutModel LayoutModel;
        public CellRegistrator CellRegistrator;
        public readonly ImagePickerAssetModel AssetsModel;

        public ImagePickerDataSource(ImagePickerAssetModel assetsModel)
        {
            AssetsModel = assetsModel;
            LayoutModel = new LayoutModel(new LayoutConfiguration());
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return LayoutModel.NumberOfItems((int) section);
        }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return LayoutModel.NumberOfSections;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (CellRegistrator == null)
            {
                throw new Exception("cells registrator must be set at this moment");
            }

            switch (indexPath.Section)
            {
                case 0:
                    return GetActionCell(collectionView, indexPath);
                case 1:
                    return GetCameraCell(collectionView, indexPath);
                case 2:
                    return GetAssetCell(collectionView, indexPath);
                default: throw new Exception("only 3 sections are supported");
            }
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