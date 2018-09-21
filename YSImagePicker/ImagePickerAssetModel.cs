using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Photos;
using UIKit;

namespace YSImagePicker
{
    public class ImagePickerAssetModel
    {
        public PHFetchResult FetchResult
        {
            get => userDefinedFetchResult ?? defaultFetchResult;
            set => userDefinedFetchResult = value;
        }

        public readonly PHCachingImageManager ImageManager = new PHCachingImageManager();
        public CGSize? ThumbnailSize;

        /// Tries to access smart album .smartAlbumUserLibrary that should be `Camera Roll` and uses just fetchAssets as fallback
        private PHFetchResult defaultFetchResult
        {
            get
            {
                var assetsOptions = new PHFetchOptions
                {
                    SortDescriptors = new[] {new NSSortDescriptor("creationDate", false)}, FetchLimit = 1000
                };

                var collections = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.SmartAlbum,
                    PHAssetCollectionSubtype.SmartAlbumUserLibrary, null);

                //TODO: Recheck
                if (collections.firstObject != null)
                {
                    //TODO: Should be return PHAsset.fetchAssets(in: cameraRoll, options: assetsOptions)
                    return PHAsset.FetchAssets(assetsOptions);
                }
                else
                {
                    return PHAsset.FetchAssets(assetsOptions);
                }
            }
        }

        private PHFetchResult userDefinedFetchResult;

        //will be use for caching
        private CGRect previousPreheatRect = CGRect.Empty;

        public void UpdateCachedAssets(UICollectionView collectionView)
        {
            // Paradoxly, using this precaching the scrolling of images is more laggy than if there is no precaching
            if (ThumbnailSize == null)
            {
                Console.WriteLine("asset model: update cache assets - thumbnail size is nil");
                return;
            }

            var layout = collectionView.CollectionViewLayout as UICollectionViewFlowLayout;
            if (layout == null)
            {
                Console.WriteLine(
                    "asset model: update cache assets - collection view layout is not flow layout");
                return;
            }

            // The preheat window is twice the height of the visible rect.
            var visibleRect = new CGRect(collectionView.ContentOffset, size: collectionView.Bounds.Size);

            CGRect preheatRect = CGRect.Empty;

            switch (layout.ScrollDirection)
            {
                case UICollectionViewScrollDirection.Vertical:
                    preheatRect = visibleRect.Inset(0, (nfloat) (-0.75 * visibleRect.Height));

                    // Update only if the visible area is significantly different from the last preheated area.
                    var delta = Math.Abs(preheatRect.GetMidY() - previousPreheatRect.GetMidY());
                    if (delta < collectionView.Bounds.Height / 3)
                    {
                        return;
                    }

                    break;
                case UICollectionViewScrollDirection.Horizontal:

                    preheatRect = visibleRect.Inset(dx: -0.75f * visibleRect.Width, dy: 0);

                    // Update only if the visible area is significantly different from the last preheated area.
                    var delta1 = Math.Abs(preheatRect.GetMidX() - previousPreheatRect.GetMidX());
                    if (delta1 < collectionView.Bounds.Width / 3)
                    {
                        return;
                    }

                    break;
            }

            // Compute the assets to start caching and to stop caching.
            var (addedRects, removedRects) =
                Miscellaneous.DifferencesBetweenRects(previousPreheatRect, preheatRect, layout.ScrollDirection);
            
            var addedAssets = addedRects.Select(x => collectionView.CollectionViewLayout.IndexPathsForElements(x))
                .SelectMany(item => item).Select(x => Runtime.GetNSObject<PHAsset>(FetchResult.ObjectAt(x.Item).Handle))
                .ToArray();

            var removedAssets = removedRects.Select(x => collectionView.CollectionViewLayout.IndexPathsForElements(x))
                .SelectMany(item => item).Select(x => Runtime.GetNSObject<PHAsset>(FetchResult.ObjectAt(x.Item).Handle))
                .ToArray();

            // Update the assets the PHCachingImageManager is caching.
            ImageManager.StartCaching(addedAssets, ThumbnailSize.Value, PHImageContentMode.AspectFill, null);

            Console.WriteLine(
                $"asset model: caching, size {ThumbnailSize}, preheat rect {preheatRect}, items {addedAssets.Length}");

            ImageManager.StopCaching(removedAssets, ThumbnailSize.Value, PHImageContentMode.AspectFill, null);
            Console.WriteLine($"asset model: uncaching, preheat rect {preheatRect}, items {removedAssets.Length}");

            // Store the preheat rect to compare against in the future.
            previousPreheatRect = preheatRect;
        }
    }
}