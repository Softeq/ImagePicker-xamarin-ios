using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Photos;
using Softeq.ImagePicker.Infrastructure;
using Softeq.ImagePicker.Infrastructure.Enums;
using Softeq.ImagePicker.Views;
using UIKit;

namespace Softeq.ImagePicker.Public
{
    /// <summary>
    /// Convenient API to register custom cell classes or nibs for each item type.
    /// Supported item types:
    /// 1. action item - register a cell for all items or a different cell for each index.
    /// 2. camera item - register a subclass of `CameraCollectionViewCell` to provide a
    /// 3. asset item - each asset media type (image, video) can have it's own cell
    /// custom camera cell implementation.
    /// </summary>
    public class CellRegistrator
    {
        private const string ActionItemIdentifierPrefix = "eu.inloop.action-item.cell-id";
        public Dictionary<int, (UINib, string)> ActionItemNibsData;
        public Dictionary<int, (Type, string)> ActionItemClassesData;

        //camera item has only 1 cell so no need for identifiers
        public UINib CameraItemNib;
        public UICollectionViewCell CameraItemClass;

        private const string AssetItemIdentifierPrefix = "eu.inloop.asset-item.cell-id";
        public Dictionary<PHAssetMediaType, (UINib, string)> AssetItemNibsData;
        public Dictionary<PHAssetMediaType, (Type, string)> AssetItemClassesData;

        //we use these if there is no asset media type specified
        public UINib AssetItemNib;
        public UICollectionViewCell AssetItemClass;

        public const string CellIdentifierForCameraItem = "eu.inloop.camera-item.cell-id";

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

        public string CellIdentifierForAssetItems => AssetItemIdentifierPrefix;

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

        /// <summary>
        /// Register a cell nib for all action items. Use this method if all action items
        /// have the same cell class.
        /// </summary>
        public void RegisterNibForActionItems(UINib nib)
        {
            Register(nib, int.MaxValue);
        }

        /// <summary>
        /// Register a cell class for all action items. Use this method if all action items
        /// have the same cell class.
        /// </summary>
        public void RegisterCellClassForActionItems(Type cellClass)
        {
            Register(cellClass, int.MaxValue);
        }

        /// <summary>
        /// Register a cell nib for an action item at particular index. Use this method if
        /// you wish to use different cells at each index.
        /// </summary>
        public void Register(UINib nib, int index)
        {
            if (ActionItemNibsData == null)
            {
                ActionItemNibsData = new Dictionary<int, (UINib, string)>();
            }

            var cellIdentifier = ActionItemIdentifierPrefix + index;
            ActionItemNibsData.Add(index, (nib, cellIdentifier));
        }

        /// <summary>
        /// Register a cell class for an action item at particular index. Use this method if
        /// you wish to use different cells at each index.
        /// </summary>
        public void Register(Type cellClass, int index)
        {
            if (ActionItemClassesData == null)
            {
                ActionItemClassesData = new Dictionary<int, (Type, string)>();
            }

            var cellIdentifier = ActionItemIdentifierPrefix + index;
            ActionItemClassesData.Add(index, (cellClass, cellIdentifier));
        }

        /// <summary>
        /// Register a cell class for camera item.
        /// </summary>
        public void RegisterCellClassForCameraItem(CameraCollectionViewCell cellClass)
        {
            CameraItemClass = cellClass;
        }

        /// <summary>
        /// Register a cell nib for camera item.
        /// A cell class must subclass `CameraCollectionViewCell` or an exception
        /// will be thrown.
        /// </summary>
        public void RegisterNibForCameraItem(UINib nib)
        {
            CameraItemNib = nib;
        }

        /// <summary>
        /// Register a cell nib for asset items of specific type (image or video).
        /// Please note, that if you register cell for specific type and your collection view displays
        /// also other types that you did not register an exception will be thrown. Always register cells
        /// for all media types you support.
        /// </summary>
        public void Register(UINib nib, PHAssetMediaType type)
        {
            if (AssetItemNibsData == null)
            {
                AssetItemNibsData = new Dictionary<PHAssetMediaType, (UINib, string)>();
            }

            var cellIdentifier = AssetItemIdentifierPrefix + type.ToString();
            AssetItemNibsData.Add(type, (nib, cellIdentifier));
        }

        /// <summary>
        /// Register a cell class for asset items of specific type (image or video).
        /// Please note, that if you register cell for specific type and your collection view displays
        /// also other types that you did not register an exception will be thrown. Always register cells
        /// for all media types you support.
        /// </summary>
        public void Register(Type cellClass, PHAssetMediaType type)
        {
            if (AssetItemClassesData == null)
            {
                AssetItemClassesData = new Dictionary<PHAssetMediaType, (Type, string)>();
            }

            var cellIdentifier = AssetItemIdentifierPrefix + type;
            AssetItemClassesData.Add(type, (cellClass, cellIdentifier));
        }

        /// <summary>
        /// Register a cell class for all asset items types (image and video).
        /// </summary>
        /// <param name="cellClass">Cell class.</param>
        public void RegisterCellClassForAssetItems(ImagePickerAssetCell cellClass)
        {
            AssetItemClass = cellClass;
        }

        /// <summary>
        /// Register a cell nib for all asset items types (image and video).
        /// Please note that cell's class must conform to `ImagePickerAssetCell` protocol, otherwise an exception will be thrown.
        /// </summary>
        public void RegisterNibForAssetItems(UINib nib)
        {
            AssetItemNib = nib;
        }
    }

    public static class UICollectionExtensions
    {
        /// <summary>
        /// Used by datasource when registering all cells to the collection view. If user
        /// did not register custom cells, this method registers default cells
        /// </summary>
        /// <param name="collectionView">Collection view.</param>
        /// <param name="registrator">Registrator.</param>
        /// <param name="cameraMode">Camera mode.</param>
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
                    UINib.FromName(nameof(ActionCell), NSBundle.FromIdentifier(nameof(ActionCell))),
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
                            CellRegistrator.CellIdentifierForCameraItem);
                        break;
                    case CameraMode.PhotoAndVideo:
                        collectionView.RegisterNibForCell(
                            UINib.FromName(nameof(VideoCameraCell), NSBundle.FromIdentifier(nameof(VideoCameraCell))),
                            CellRegistrator.CellIdentifierForCameraItem);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(cameraMode), cameraMode, null);
                }
            }
            else if (registrator.CameraItemNib != null && registrator.CameraItemClass == null)
            {
                collectionView.RegisterNibForCell(registrator.CameraItemNib, CellRegistrator.CellIdentifierForCameraItem);
            }
            else
            {
                collectionView.RegisterClassForCell(registrator.CameraItemClass.GetType(),
                    CellRegistrator.CellIdentifierForCameraItem);
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

        /// <summary>
        /// Helper func that takes nib,cellId pair and registers them on a collection view
        /// </summary>
        public static void Register(this UICollectionView collectionView, IEnumerable<(UINib, string)> nibsData)
        {
            if (nibsData == null)
            {
                return;
            }

            foreach (var tuple in nibsData)
            {
                collectionView.RegisterNibForCell(tuple.Item1, tuple.Item2);
            }
        }

        /// <summary>
        /// Helper func that takes nib,cellid pair and registers them on a collection view
        /// </summary>
        public static void Register(this UICollectionView collectionView, IEnumerable<(Type, string)> classData)
        {
            if (classData == null)
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