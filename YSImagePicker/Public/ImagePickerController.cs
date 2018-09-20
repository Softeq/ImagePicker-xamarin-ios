using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Photos;
using UIKit;
using YSImagePicker.Media;
using YSImagePicker.Views;

namespace YSImagePicker.Public
{
    ///
    /// Group of methods informing what image picker is currently doing
    ///
    public class ImagePickerControllerDelegate
    {
        ///
        /// Called when user taps on an action item, index is either 0 or 1 depending which was tapped
        ///
        public virtual void ImagePicker(ImagePickerController controller, int index)
        {
        }

        ///
        /// Called when user select an asset.
        ///
        public virtual void ImagePicker(ImagePickerController controller, PHAsset asset)
        {
        }

        ///
        /// Called when user unselect previously selected asset.
        ///
        public virtual void ImagePicker(ImagePickerController controller, PHAsset asset)
        {
        }

        ///
        /// Called when user takes new photo.
        ///
        public virtual void ImagePicker(ImagePickerController controller, UIImage image)
        {
        }

        ///
        /// Called when user takes new photo.
        ///
        //TODO:
        //func imagePicker(controller: ImagePickerController, didCaptureVideo url: UIImage)
        //func imagePicker(controller: ImagePickerController, didTake livePhoto: UIImage, videoUrl: UIImage)

        ///
        /// Called right before an action item collection view cell is displayed. Use this method
        /// to configure your cell.
        ///
        public virtual void ImagePicker(ImagePickerController controller, UICollectionViewCell cell, int index)
        {
        }

        ///
        /// Called right before an asset item collection view cell is displayed. Use this method
        /// to configure your cell based on asset media type, subtype, etc.
        ///
        public virtual void ImagePicker(ImagePickerController controller, ImagePickerAssetCell cell, PHAsset asset)
        {
        }
    }

    ///
    /// Image picker may ask for additional resources, implement this protocol to fully support
    /// all features.
    ///
    public abstract class ImagePickerControllerDataSource
    {
        ///
        /// Asks for a view that is placed as overlay view with permissions info
        /// when user did not grant or has restricted access to photo library.
        ///
        public abstract UIView ImagePicker(ImagePickerController controller, PHAuthorizationStatus status);
    }

    public class ImagePickerController : UIViewController
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            PHPhotoLibrary.SharedPhotoLibrary.UnregisterChangeObserver(this);
            captureSession?.suspend();
            Console.WriteLine($"deinit: describing: {this}");
        }

        // MARK: Public API

        ///
        /// Use this object to configure layout of action, camera and asset items.
        ///
        public LayoutConfiguration layoutConfiguration = LayoutConfiguration.Default();

        ///
        /// Use this to register a cell classes or nibs for each item types
        ///
        public CellRegistrator cellRegistrator = new CellRegistrator();

        ///
        /// Use these settings to configure how the capturing should behave
        ///
        public CaptureSettings captureSettings = new CaptureSettings();

        ///
        /// Get informed about user interaction and changes
        ///
        public ImagePickerControllerDelegate Delegate;

        ///
        /// Provide additional data when requested by Image Picker
        ///
        public ImagePickerControllerDataSource dataSource;

        ///
        /// A collection view that is used for displaying content.
        ///
        public UICollectionView collectionView => imagePickerView.UICollectionView;

        private ImagePickerView imagePickerView => View as ImagePickerView;

        ///
        /// Programatically select asset.
        ///
        public void SelectAsset(int index, bool animated, UICollectionViewScrollPosition scrollPosition)
        {
            var path = NSIndexPath.FromItemSection(item: index, section: layoutConfiguration.SectionIndexForAssets);
            collectionView.SelectItem(path, animated: animated, scrollPosition: scrollPosition);
        }

        ///
        /// Programatically deselect asset.
        ///
        public void DeselectAsset(int index, bool animated)
        {
            var path = NSIndexPath.FromItemSection(item: index, section: layoutConfiguration.SectionIndexForAssets);
            collectionView.DeselectItem(path, animated: animated);
        }

        ///
        /// Programatically deselect all selected assets.
        ///
        public void DeselectAllAssets(bool animated)
        {
            var items = collectionView.GetIndexPathsForSelectedItems();
            if (items == null)
            {
                return;
            }

            foreach (var selectedPath in items)
            {
                collectionView.DeselectItem(selectedPath, animated: animated);
            }
        }

        ///
        /// Access all currently selected images
        ///
        public IEnumerable<PHAsset> selectedAssets =>
            collectionView.GetIndexPathsForSelectedItems().Select(x => Asset(x.Row)).ToList();

        ///
        /// Returns an array of assets at index set. An exception will be thrown if it fails
        ///
        public PHAsset Assets(NSIndexSet indexes)
        {
            if (collectionViewDataSource.AssetsModel.FetchResult == null)
            {
                throw new Exception($"Accessing assets at indexes {indexes} failed");
            }

            return collectionViewDataSource.assetsModel.fetchResult.objects(at: indexes);
        }

        ///
        /// Returns an asset at index. If there is no asset at the index, an exception will be thrown.
        ///
        public PHAsset Asset(int index)
        {
            if (collectionViewDataSource.AssetsModel.FetchResult == null)
            {
                throw new Exception($"Accessing asset at index {index} failed");
            }

            return collectionViewDataSource.AssetsModel.FetchResult.ObjectAs(index);
        }

        ///
        /// Fetch result of assets that will be used for picking.
        ///
        /// If you leave this nil or return nil from the block, assets from recently
        /// added smart album will be used.
        ///
        public Func<PHFetchResult> AssetsFetchResultBlock;

        ///
        /// Global appearance proxy object. Use this object to set appearance
        /// for all instances of Image Picker. If you wish to set an appearance
        /// on instances use corresponding instance method.
        ///
        public Appearance AppearanceClassProxy => classAppearanceProxy;

        ///
        /// Instance appearance proxy object. Use this object to set appearance
        /// for this particular instance of Image Picker. This has precedence over
        /// global appearance.
        ///
        public Appearance AppearanceInstance()
        {
            if (instanceAppearanceProxy == null)
            {
                instanceAppearanceProxy = new Appearance();
            }

            return instanceAppearanceProxy;
        }

        // MARK: Private Methods

        private CollectionViewUpdatesCoordinator collectionViewCoordinator;

        private ImagePickerDataSource collectionViewDataSource =
            new ImagePickerDataSource(assetsModel: ImagePickerAssetModel());
        fileprivate var collectionViewDelegate = ImagePickerDelegate()
    
        fileprivate var captureSession: CaptureSession?
    
        private func updateItemSize() {
        
            guard let layout = self.collectionViewDelegate.layout else {
                return
            }
        
            let itemsInRow = layoutConfiguration.numberOfAssetItemsInRow
            let scrollDirection = layoutConfiguration.scrollDirection
            let cellSize = layout.sizeForItem(numberOfItemsInRow: itemsInRow, preferredWidthOrHeight: nil, collectionView: collectionView, scrollDirection: scrollDirection)
            let scale = UIScreen.main.scale
            let thumbnailSize = CGSize(width: cellSize.width * scale, height: cellSize.height * scale)
            self.collectionViewDataSource.assetsModel.thumbnailSize = thumbnailSize
        
            //TODO: we need to purge all image asset caches if item size changed
        }
        
        
    }
}