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
        public LayoutModel LayoutModel = LayoutModel.Empty();
        public CellRegistrator CellRegistrator;
        public readonly ImagePickerAssetModel AssetsModel;

        public ImagePickerDataSource(ImagePickerAssetModel assetsModel)
        {
            AssetsModel = assetsModel;
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

            //TODO: change these hardcoded section numbers to those defined in layoutModel.layoutConfiguration

            switch (indexPath.Section)
            {
                case 0:
                    var identifier = CellRegistrator.CellIdentifier(indexPath.Row);

                    if (identifier == null)
                    {
                        throw new Exception(
                            $"there is an action item at index {indexPath.Row} but no cell is registered.");
                    }

                    return collectionView.DequeueReusableCell(identifier, indexPath) as UICollectionViewCell;

                case 1:
                    var id = CellRegistrator.CellIdentifierForCameraItem;

                    var reusableCell = collectionView.DequeueReusableCell(id, indexPath) as CameraCollectionViewCell;

                    if (reusableCell == null)
                    {
                        throw new Exception(
                            "there is a camera item but no cell class `CameraCollectionViewCell` is registered.");
                    }

                    return reusableCell;

                case 2:
                    var asset = Runtime.GetNSObject<PHAsset>(AssetsModel.FetchResult.ObjectAt(indexPath.Item).Handle);

                    var cellIdentifier = CellRegistrator.CellIdentifier(asset.MediaType);

                    var cell = collectionView.DequeueReusableCell(
                        cellIdentifier ?? CellRegistrator.CellIdentifierForAssetItems, indexPath) as ImagePickerAssetCell;


                    if (cell == null)
                    {
                        throw new Exception($"asset item cell must conform to {nameof(ImagePickerAssetCell)} protocol");
                    }

                    var thumbnailSize = AssetsModel.ThumbnailSize ?? CGSize.Empty;

                    // Request an image for the asset from the PHCachingImageManager.
                    cell.RepresentedAssetIdentifier = asset.LocalIdentifier;
                    AssetsModel.ImageManager.RequestImageForAsset(asset, thumbnailSize, PHImageContentMode.AspectFill,
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

                default: throw new Exception("only 3 sections are supported");
            }
        }
    }
}