using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Photos;
using Softeq.ImagePicker.Infrastructure.Enums;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker.Public
{
    ///
    /// Convenient API to register custom cell classes or nibs for each item type.
    ///
    /// Supported item types:
    /// 1. action item - register a cell for all items or a different cell for each index.
    /// 2. camera item - register a subclass of `CameraCollectionViewCell` to provide a
    /// 3. asset item - each asset media type (image, video) can have it's own cell
    /// custom camera cell implementation.
    ///
    public class CellRegistrator
    {
        // MARK: Private Methods
        private string _actionItemIdentifierPrefix = "eu.inloop.action-item.cell-id";
        public Dictionary<int, (UINib, string)> ActionItemNibsData;
        public Dictionary<int, (Type, string)> ActionItemClassesData;

        //camera item has only 1 cell so no need for identifiers
        public UINib CameraItemNib;
        public UICollectionViewCell CameraItemClass;

        private string _assetItemIdentifierPrefix = "eu.inloop.asset-item.cell-id";
        public Dictionary<PHAssetMediaType, (UINib, string)> AssetItemNibsData;
        public Dictionary<PHAssetMediaType, (Type, string)> AssetItemClassesData;

        //we use these if there is no asset media type specified
        public UINib AssetItemNib;
        public UICollectionViewCell AssetItemClass;

        // MARK: Internal Methods

        public string CellIdentifierForCameraItem = "eu.inloop.camera-item.cell-id";

        //TODO: Check logic
        public string CellIdentifier(int index)
        {
            if (ActionItemNibsData != null && ActionItemNibsData.ContainsKey(index))
            {
                return ActionItemNibsData[index].Item2;
            }

            if (ActionItemClassesData != null && ActionItemClassesData.ContainsKey(index))
            {
                return ActionItemClassesData[index].Item2;
            }

            if (index == int.MaxValue)
            {
                return null;
            }

            //lets see if there is a globally registered cell for all indexes
            return CellIdentifier(int.MaxValue);
        }

        public bool HasUserRegisteredActionCell => ActionItemNibsData?.Any() ?? ActionItemClassesData?.Any() ?? false;

        public string CellIdentifierForAssetItems => _assetItemIdentifierPrefix;

        public string CellIdentifier(PHAssetMediaType type)
        {
            if (AssetItemNibsData != null && AssetItemNibsData.ContainsKey(type))
            {
                return AssetItemNibsData[type].Item2;
            }

            if (AssetItemClassesData != null && AssetItemClassesData.ContainsKey(type))
            {
                return AssetItemClassesData[type].Item2;
            }

            return null;
        }

        // MARK: Public Methods

        ///
        /// Register a cell nib for all action items. Use this method if all action items
        /// have the same cell class.
        ///
        public void RegisterNibForActionItems(UINib nib)
        {
            Register(nib, int.MaxValue);
        }

        ///
        /// Register a cell class for all action items. Use this method if all action items
        /// have the same cell class.
        ///
        public void RegisterCellClassForActionItems(Type cellClass)
        {
            Register(cellClass, int.MaxValue);
        }

        ///
        /// Register a cell nib for an action item at particular index. Use this method if
        /// you wish to use different cells at each index.
        ///
        public void Register(UINib nib, int index)
        {
            if (ActionItemNibsData == null)
            {
                ActionItemNibsData = new Dictionary<int, (UINib, string)>();
            }

            var cellIdentifier = _actionItemIdentifierPrefix + index;
            ActionItemNibsData.Add(index, (nib, cellIdentifier));
        }

        ///
        /// Register a cell class for an action item at particular index. Use this method if
        /// you wish to use different cells at each index.
        ///
        public void Register(Type cellClass, int index)
        {
            if (ActionItemClassesData == null)
            {
                ActionItemClassesData = new Dictionary<int, (Type, string)>();
            }

            var cellIdentifier = _actionItemIdentifierPrefix + index;
            ActionItemClassesData.Add(index, (cellClass, cellIdentifier));
        }

        ///
        /// Register a cell class for camera item.
        ///
        public void RegisterCellClassForCameraItem(CameraCollectionViewCell cellClass)
        {
            CameraItemClass = cellClass;
        }

        ///
        /// Register a cell nib for camera item.
        ///
        /// - note: A cell class must subclass `CameraCollectionViewCell` or an exception
        /// will be thrown.
        ///
        public void RegisterNibForCameraItem(UINib nib)
        {
            CameraItemNib = nib;
        }

        ///
        /// Register a cell nib for asset items of specific type (image or video).
        ///
        /// - note: Please note, that if you register cell for specific type and your collection view displays
        /// also other types that you did not register an exception will be thrown. Always register cells
        /// for all media types you support.
        ///
        public void Register(UINib nib, PHAssetMediaType type)
        {
            if (AssetItemNibsData == null)
            {
                AssetItemNibsData = new Dictionary<PHAssetMediaType, (UINib, string)>();
            }

            var cellIdentifier = _assetItemIdentifierPrefix + type.ToString();
            AssetItemNibsData.Add(type, (nib, cellIdentifier));
        }

        ///
        /// Register a cell class for asset items of specific type (image or video).
        ///
        /// - note: Please note, that if you register cell for specific type and your collection view displays
        /// also other types that you did not register an exception will be thrown. Always register cells
        /// for all media types you support.
        ///
        public void Register(Type cellClass, PHAssetMediaType type)
        {
            if (AssetItemClassesData == null)
            {
                AssetItemClassesData = new Dictionary<PHAssetMediaType, (Type, string)>();
            }

            var cellIdentifier = _assetItemIdentifierPrefix + type;
            AssetItemClassesData.Add(type, (cellClass, cellIdentifier));
        }

        ///
        /// Register a cell class for all asset items types (image and video).
        ///
        public void RegisterCellClassForAssetItems(ImagePickerAssetCell cellClass)
        {
            AssetItemClass = cellClass;
        }

        ///
        /// Register a cell nib for all asset items types (image and video).
        ///
        /// Please note that cell's class must conform to `ImagePickerAssetCell` protocol, otherwise an exception will be thrown.
        ///
        public void RegisterNibForAssetItems(UINib nib)
        {
            AssetItemNib = nib;
        }
    }

    public static class UICollectionExtensions
    {
        ///
        /// Used by datasource when registering all cells to the collection view. If user
        /// did not register custom cells, this method registers default cells
        ///
        public static void Apply(this UICollectionView collectionView, CellRegistrator registrator,
            CameraMode cameraMode)
        {
            //register action items considering type
            //if user did not register any nib or cell, use default action cell
            if (registrator.HasUserRegisteredActionCell == false)
            {
                registrator.RegisterCellClassForActionItems(typeof(ActionCell));
                var identifier = registrator.CellIdentifier(int.MaxValue);

                if (identifier == null)
                {
                    throw new ImagePickerException("Image Picker: unable to register default action item cell");
                }

                collectionView.RegisterNibForCell(
                    UINib.FromName("ActionCell", NSBundle.FromIdentifier(nameof(ActionCell))),
                    identifier);
            }
            else
            {
                collectionView.Register(registrator.ActionItemNibsData?.Values);
                collectionView.Register(registrator.ActionItemClassesData?.Values);
            }

            if (registrator.CameraItemNib == null && registrator.CameraItemClass == null)
            {
                switch (cameraMode)
                {
                    case CameraMode.Photo:
                    case CameraMode.PhotoAndLivePhoto:
                        collectionView.RegisterNibForCell(
                            UINib.FromName(nameof(LivePhotoCameraCell),
                                NSBundle.FromIdentifier(nameof(LivePhotoCameraCell))),
                            registrator.CellIdentifierForCameraItem);
                        break;
                    case CameraMode.PhotoAndVideo:
                        collectionView.RegisterNibForCell(
                            UINib.FromName(nameof(VideoCameraCell), NSBundle.FromIdentifier(nameof(VideoCameraCell))),
                            registrator.CellIdentifierForCameraItem);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(cameraMode), cameraMode, null);
                }
            }
            else if (registrator.CameraItemNib != null && registrator.CameraItemClass == null)
            {
                collectionView.RegisterNibForCell(registrator.CameraItemNib, registrator.CellIdentifierForCameraItem);
            }
            else
            {
                collectionView.RegisterClassForCell(registrator.CameraItemClass.GetType(),
                    registrator.CellIdentifierForCameraItem);
            }

            //register asset items considering type
            collectionView.Register(registrator.AssetItemNibsData?.Values);
            collectionView.Register(registrator.AssetItemClassesData?.Values);

            if (registrator.AssetItemNib == null && registrator.AssetItemClass == null)
            {
                //if user did not register all required classes/nibs - register default cells
                collectionView.RegisterClassForCell(typeof(VideoAssetCell), registrator.CellIdentifierForAssetItems);
                //fatalError("there is not registered cell class nor nib for asset items, please user appropriate register methods on `CellRegistrator`")
            }
            else if (registrator.AssetItemNib != null && registrator.AssetItemClass == null)
            {
                collectionView.RegisterNibForCell(registrator.AssetItemNib, registrator.CellIdentifierForAssetItems);
            }
            else
            {
                collectionView.RegisterClassForCell(registrator.AssetItemClass.GetType(),
                    registrator.CellIdentifierForAssetItems);
            }
        }

        ///
        /// Helper func that takes nib,cellId pair and registers them on a collection view
        ///
        public static void Register(this UICollectionView collectionView, IEnumerable<(UINib, string)> nibsData)
        {
            if (nibsData == null)
            {
                return;
            }

            if (!nibsData.Any())
            {
                return;
            }

            foreach (var tuple in nibsData)
            {
                collectionView.RegisterNibForCell(tuple.Item1, tuple.Item2);
            }
        }

        ///
        /// Helper func that takes nib,cellid pair and registers them on a collection view
        ///
        public static void Register(this UICollectionView collectionView,
            IEnumerable<(Type, string)> classData)
        {
            if (classData == null)
            {
                return;
            }

            if (!classData.Any())
            {
                return;
            }

            foreach (var tuple in classData)
            {
                collectionView.RegisterClassForCell(tuple.Item1, tuple.Item2);
            }
        }
    }
}