using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Foundation;
using Photos;
using Softeq.ImagePicker.Infrastructure;
using Softeq.ImagePicker.Public.Delegates;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker.Public
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class ImagePickerController
    {
        /// <summary>
        /// Use this object to configure layout of action, camera and asset items.
        /// </summary>
        public LayoutConfiguration LayoutConfiguration = new LayoutConfiguration().Default();

        /// <summary>
        /// Use this to register a cell classes or nibs for each item types
        /// </summary>
        /// <value>The cell registrator.</value>
        public CellRegistrator CellRegistrator { get; } = new CellRegistrator();

        /// <summary>
        /// Use these settings to configure how the capturing should behave
        /// </summary>
        /// <value>The capture settings.</value>
        public CaptureSettings CaptureSettings { get; } = new CaptureSettings();

        /// <summary>
        /// Get informed about user interaction and changes
        /// </summary>
        /// <value>The delegate.</value>
        public ImagePickerControllerDelegate Delegate { get; set; }

        /// <summary>
        /// Provide additional data when requested by Image Picker
        /// </summary>
        public ImagePickerControllerDataSource DataSource;

        /// <summary>
        /// A collection view that is used for displaying content.
        /// </summary>
        /// <value>The collection view.</value>
        public UICollectionView CollectionView => ImagePickerView.UICollectionView;

        public ImagePickerView ImagePickerView => View as ImagePickerView;

        /// <summary>
        /// Fetch result of assets that will be used for picking.
        /// 
        /// If you leave this nil or return nil from the block, assets from recently
        /// added smart album will be used.
        /// </summary>
        public Func<PHFetchResult> AssetsFetchResultBlock;

        /// <summary>
        /// Instance appearance proxy object. Use this object to set appearance
        /// for this particular instance of Image Picker.
        /// </summary>
        /// <value>The appearance.</value>
        public Appearance Appearance { get; } = new Appearance();

        /// <summary>
        /// Programatically select asset.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        /// <param name="scrollPosition">Scroll position.</param>
        public void SelectAsset(int index, bool animated, UICollectionViewScrollPosition scrollPosition)
        {
            var path = NSIndexPath.FromItemSection(index, LayoutConfiguration.SectionIndexForAssets);
            CollectionView.SelectItem(path, animated, scrollPosition);
        }

        /// <summary>
        /// Programatically deselect asset.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        public void DeselectAsset(int index, bool animated)
        {
            var path = NSIndexPath.FromItemSection(index, LayoutConfiguration.SectionIndexForAssets);
            CollectionView.DeselectItem(path, animated);
        }

        /// <summary>
        /// Programatically deselect all selected assets.
        /// </summary>
        /// <param name="animated">If set to <c>true</c> animated.</param>
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

        /// <summary>
        /// Access all currently selected images
        /// </summary>
        /// <value>The selected assets.</value>
        public IReadOnlyList<PHAsset> SelectedAssets =>
            CollectionView.GetIndexPathsForSelectedItems().Select(x => Asset(x.Row)).ToList();

        /// <summary>
        /// Returns an array of assets at index set. An exception will be thrown if it fails
        /// </summary>
        /// <returns>The assets.</returns>
        /// <param name="indexes">Indexes.</param>
        public PHAsset[] Assets(NSIndexSet indexes)
        {
            if (_collectionViewDataSource.AssetsModel.FetchResult == null)
            {
                throw new ImagePickerException($"Accessing assets at indexes {indexes} failed");
            }

            return _collectionViewDataSource.AssetsModel.FetchResult.ObjectsAt<PHAsset>(indexes);
        }

        /// <summary>
        /// Returns an asset at index. If there is no asset at the index, an exception will be thrown.
        /// </summary>
        /// <returns>The asset.</returns>
        /// <param name="index">Index.</param>
        public PHAsset Asset(int index)
        {
            if (_collectionViewDataSource.AssetsModel.FetchResult == null)
            {
                throw new ImagePickerException($"Accessing asset at index {index} failed");
            }

            return (PHAsset)_collectionViewDataSource.AssetsModel.FetchResult.ElementAt(index);
        }
    }
}