using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Photos;
using UIKit;
using YSImagePicker.Public.Delegates;
using YSImagePicker.Views;

namespace YSImagePicker.Public
{
    public partial class ImagePickerController
    {
        ///
        /// Use this object to configure layout of action, camera and asset items.
        ///
        public LayoutConfiguration LayoutConfiguration = new LayoutConfiguration().Default();

        ///
        /// Use this to register a cell classes or nibs for each item types
        ///
        public CellRegistrator CellRegistrator = new CellRegistrator();

        ///
        /// Use these settings to configure how the capturing should behave
        ///
        public CaptureSettings CaptureSettings { get; } = new CaptureSettings();

        ///
        /// Get informed about user interaction and changes
        ///
        public ImagePickerControllerDelegate Delegate;

        ///
        /// Provide additional data when requested by Image Picker
        ///
        public ImagePickerControllerDataSource DataSource;

        ///
        /// A collection view that is used for displaying content.
        ///
        public UICollectionView CollectionView => ImagePickerView.UICollectionView;

        private ImagePickerView ImagePickerView => View as ImagePickerView;

        ///
        /// Programatically select asset.
        ///
        public void SelectAsset(int index, bool animated, UICollectionViewScrollPosition scrollPosition)
        {
            var path = NSIndexPath.FromItemSection(index, LayoutConfiguration.SectionIndexForAssets);
            CollectionView.SelectItem(path, animated, scrollPosition);
        }

        ///
        /// Programatically deselect asset.
        ///
        public void DeselectAsset(int index, bool animated)
        {
            var path = NSIndexPath.FromItemSection(index, LayoutConfiguration.SectionIndexForAssets);
            CollectionView.DeselectItem(path, animated);
        }

        ///
        /// Programatically deselect all selected assets.
        ///
        public void DeselectAllAssets(bool animated)
        {
            var items = CollectionView.GetIndexPathsForSelectedItems();
            if (items == null)
            {
                return;
            }

            foreach (var selectedPath in items)
            {
                CollectionView.DeselectItem(selectedPath, animated);
            }
        }

        ///
        /// Access all currently selected images
        ///
        public IReadOnlyList<PHAsset> SelectedAssets =>
            CollectionView.GetIndexPathsForSelectedItems().Select(x => Asset(x.Row)).ToList();

        ///
        /// Returns an array of assets at index set. An exception will be thrown if it fails
        ///
        public PHAsset[] Assets(NSIndexSet indexes)
        {
            if (_collectionViewDataSource.AssetsModel.FetchResult == null)
            {
                throw new Exception($"Accessing assets at indexes {indexes} failed");
            }

            return _collectionViewDataSource.AssetsModel.FetchResult.ObjectsAt<PHAsset>(indexes);
        }

        ///
        /// Returns an asset at index. If there is no asset at the index, an exception will be thrown.
        ///
        public PHAsset Asset(int index)
        {
            if (_collectionViewDataSource.AssetsModel.FetchResult == null)
            {
                throw new Exception($"Accessing asset at index {index} failed");
            }

            return (PHAsset) _collectionViewDataSource.AssetsModel.FetchResult.ElementAt(index);
        }

        ///
        /// Fetch result of assets that will be used for picking.
        ///
        /// If you leave this nil or return nil from the block, assets from recently
        /// added smart album will be used.
        ///
        public Func<PHFetchResult> AssetsFetchResultBlock;

        ///
        /// Instance appearance proxy object. Use this object to set appearance
        /// for this particular instance of Image Picker.
        ///
        public Appearance Appearance { get; } = new Appearance();
    }
}