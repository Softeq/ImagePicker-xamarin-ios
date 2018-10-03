using System;
using CoreGraphics;
using Foundation;
using Photos;

namespace Softeq.ImagePicker
{
    public class ImagePickerAssetModel
    {
        private const int FetchLimit = 1000;
        private PHFetchResult _updatedFetchResult;
        private readonly Lazy<PHFetchResult> _defaultFetchResult = new Lazy<PHFetchResult>(FetchAssets);

        public PHCachingImageManager ImageManager { get; }
        public CGSize ThumbnailSize { get; set; }
        public PHFetchResult FetchResult => _updatedFetchResult ?? _defaultFetchResult.Value;

        public ImagePickerAssetModel()
        {
            ImageManager = new PHCachingImageManager();
        }

        public void UpdateFetchResult(PHFetchResult fetchResult)
        {
            _updatedFetchResult = fetchResult;
        }

        private static PHFetchResult FetchAssets()
        {
            const string sortType = "creationDate";

            var assetsOptions = new PHFetchOptions
            {
                SortDescriptors = new[]
                {
                    new NSSortDescriptor(sortType, false)
                },
                FetchLimit = FetchLimit
            };

            return PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.SmartAlbum,
                PHAssetCollectionSubtype.SmartAlbumUserLibrary, null).firstObject is PHAssetCollection assetCollection
                ? PHAsset.FetchAssets(assetCollection, assetsOptions)
                : PHAsset.FetchAssets(assetsOptions);
        }
    }
}