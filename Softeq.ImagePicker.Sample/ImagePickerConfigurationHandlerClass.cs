using System.Collections.Generic;
using Foundation;
using Photos;
using Softeq.ImagePicker.Enums;
using Softeq.ImagePicker.Public;
using TestApplication.Models;
using TestApplication.Models.Enums;
using UIKit;

namespace TestApplication
{
    public class ImagePickerConfigurationHandlerClass
    {
        private int _numberOfActionItems = 2;
        private CameraItemConfig _cameraConfig = CameraItemConfig.Enabled;
        private AssetsSource _assetsSource = AssetsSource.RecentlyAdded;
        private int _assetItemsInRow = 2;
        private CameraMode _captureMode = CameraMode.PhotoAndLivePhoto;
        private bool _savesCapturedAssets = true;

        public readonly List<(string GroupTitle, string GroupDescription)> SectionsData =
            new List<(string groupTitle, string groupDescription)>
            {
                ("Presentation", string.Empty),
                ("Action Items", string.Empty),
                ("Camera Item", string.Empty),
                ("Assets Source", string.Empty),
                ("Asset Items in a row", string.Empty),
                ("Capture mode", string.Empty),
                (
                    "Save Assets",
                    "Assets will be saved to Photo Library. This applies to photos only. Live photos and videos are always saved."
                )
            };

        public List<CellItemModel[]> CellsData { get; private set; }
        public bool PresentsModally { get; private set; }

        public ImagePickerConfigurationHandlerClass()
        {
            InitializeCellData();
        }

        public ImagePickerController GenerateImagePicker()
        {
            // create new instance
            var imagePicker = new ImagePickerController();

            // set action items
            switch (_numberOfActionItems)
            {
                case 1:
                    imagePicker.LayoutConfiguration.ShowsFirstActionItem = true;
                    imagePicker.LayoutConfiguration.ShowsSecondActionItem = false;
                    //if you wish to register your own action cell register it here,
                    //it can by any UICollectionViewCell
                    imagePicker.CellRegistrator.Register(UINib.FromName("IconWithTextCell", null), 0);
                    break;
                case 2:
                    imagePicker.LayoutConfiguration.ShowsFirstActionItem = true;
                    imagePicker.LayoutConfiguration.ShowsSecondActionItem = true;
                    //if you wish to register your own action cell register it here,
                    //it can by any UICollectionViewCell
                    imagePicker.CellRegistrator.RegisterNibForActionItems(UINib.FromName("IconWithTextCell", null));
                    break;
                default:
                    imagePicker.LayoutConfiguration.ShowsFirstActionItem = false;
                    imagePicker.LayoutConfiguration.ShowsSecondActionItem = false;
                    break;
            }

            // set camera item enabled/disabled
            switch (_cameraConfig)
            {
                case CameraItemConfig.Enabled:
                    imagePicker.LayoutConfiguration.ShowsCameraItem = true;
                    break;
                case CameraItemConfig.Disabled:
                    imagePicker.LayoutConfiguration.ShowsCameraItem = false;
                    break;
            }

            // config assets source
            switch (_assetsSource)
            {
                case AssetsSource.RecentlyAdded:
                    //for recently added we use default fetch result and default asset cell
                    break;
                case AssetsSource.OnlyVideos:
                    //registering custom video cell to demonstrate how to use custom cells
                    //please note that custom asset cells must conform to  ImagePickerAssetCell protocol
                    imagePicker.CellRegistrator.Register(UINib.FromName("CustomVideoCell", null),
                        PHAssetMediaType.Video);
                    imagePicker.AssetsFetchResultBlock = () =>
                    {
                        var collection = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.SmartAlbum,
                            PHAssetCollectionSubtype.SmartAlbumVideos, null).firstObject;
                        if (collection == null)
                        {
                            return
                                null; //you can return null if you did not find desired fetch result, default fetch result will be used.
                        }

                        return PHAsset.FetchAssets((PHAssetCollection) collection, null);
                    };
                    break;
                case AssetsSource.OnlySelfies:
                    //registering custom image cell to demonstrate how to use custom cells
                    //please note that custom asset cells must conform to  ImagePickerAssetCell protocol
                    imagePicker.CellRegistrator.RegisterNibForAssetItems(UINib.FromName("CustomImageCell", null));
                    imagePicker.AssetsFetchResultBlock = () =>
                    {
                        var collection = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.SmartAlbum,
                            PHAssetCollectionSubtype.SmartAlbumSelfPortraits, null).firstObject;

                        if (collection == null)
                        {
                            return null;
                        }

                        return PHAsset.FetchAssets((PHAssetCollection) collection, null);
                    };
                    break;
            }

            // number of items in a row (supported values > 0)
            imagePicker.LayoutConfiguration.NumberOfAssetItemsInRow = _assetItemsInRow;

            // capture mode
            switch (_captureMode)
            {
                case CameraMode.Photo:
                    imagePicker.CaptureSettings.CameraMode = CameraMode.Photo;
                    break;
                //if you wish to use your own cell for capturing photos register it here:
                //please note that custom cell must sublcass `CameraCollectionViewCell`.
                //imagePicker.cellRegistrator.registerNibForCameraItem(UINib(nibName: "CustomNibName", bundle: null))
                case CameraMode.PhotoAndLivePhoto:
                    imagePicker.CaptureSettings.CameraMode = CameraMode.PhotoAndLivePhoto;
                    break;
                //if you wish to use your own cell for photo and live photo register it here:
                //please note that custom cell must sublcass `CameraCollectionViewCell`.
                //imagePicker.cellRegistrator.registerNibForCameraItem(UINib(nibName: "CustomNibName", bundle: null))
                case CameraMode.PhotoAndVideo:
                    imagePicker.CaptureSettings.CameraMode = CameraMode.PhotoAndVideo;
                    //if you wish to use your own cell for photo and video register it here:
                    //please note that custom cell must sublcass `CameraCollectionViewCell`.
                    //imagePicker.cellRegistrator.registerNibForCameraItem(UINib(nibName: "CustomNibName", bundle: null))
                    break;
            }

            // save capture assets to photo library?
            imagePicker.CaptureSettings.SavesCapturedPhotosToPhotoLibrary = _savesCapturedAssets;

            return imagePicker;
        }

        private void TogglePresentationMode(NSIndexPath indexPath)
        {
            PresentsModally = indexPath.Row == 1;
        }

        private void SetNumberOfActionItems(NSIndexPath indexPath)
        {
            _numberOfActionItems = indexPath.Row;
        }

        private void ConfigCameraItem(NSIndexPath indexPath)
        {
            _cameraConfig = (CameraItemConfig) indexPath.Row;
        }

        private void ConfigAssetsSource(NSIndexPath indexPath)
        {
            _assetsSource = (AssetsSource) indexPath.Row;
        }

        private void ConfigAssetItemsInRow(NSIndexPath indexPath)
        {
            _assetItemsInRow = indexPath.Row + 1;
        }

        private void ConfigCaptureMode(NSIndexPath indexPath)
        {
            switch (indexPath.Row)
            {
                case 0:
                    _captureMode = CameraMode.Photo;
                    break;
                case 1:
                    _captureMode = CameraMode.PhotoAndLivePhoto;
                    break;
                case 2:
                    _captureMode = CameraMode.PhotoAndVideo;
                    break;
            }
        }

        private void ConfigSavesCapturedAssets(NSIndexPath indexPath)
        {
            _savesCapturedAssets = indexPath.Row == 1;
        }

        private void InitializeCellData()
        {
            CellsData = new List<CellItemModel[]>
            {
                new[]
                {
                    new CellItemModel("As input view", TogglePresentationMode, cell =>
                        cell.Accessory = PresentsModally
                            ? UITableViewCellAccessory.None
                            : UITableViewCellAccessory.Checkmark),
                    new CellItemModel("Modally", TogglePresentationMode, cell =>
                        cell.Accessory = PresentsModally
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellItemModel("Disabled", SetNumberOfActionItems, cell => cell.Accessory =
                        _numberOfActionItems == 0
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellItemModel("One item", SetNumberOfActionItems, cell =>
                        cell.Accessory = _numberOfActionItems == 1
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellItemModel("Two items (default)", SetNumberOfActionItems,
                        cell =>
                            cell.Accessory = _numberOfActionItems == 2
                                ? UITableViewCellAccessory.Checkmark
                                : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellItemModel("Enabled (default)", ConfigCameraItem, cell =>
                        cell.Accessory = _cameraConfig == CameraItemConfig.Enabled
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellItemModel("Disabled", ConfigCameraItem, cell =>
                        cell.Accessory = _cameraConfig == CameraItemConfig.Disabled
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellItemModel("Camera Roll (default)", ConfigAssetsSource,
                        cell =>
                            cell.Accessory = _assetsSource == AssetsSource.RecentlyAdded
                                ? UITableViewCellAccessory.Checkmark
                                : UITableViewCellAccessory.None),
                    new CellItemModel("Only videos", ConfigAssetsSource, cell =>
                        cell.Accessory = _assetsSource == AssetsSource.OnlyVideos
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellItemModel("Only selfies", ConfigAssetsSource, cell =>
                        cell.Accessory = _assetsSource == AssetsSource.OnlySelfies
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellItemModel("One", ConfigAssetItemsInRow, cell =>
                        cell.Accessory = _assetItemsInRow == 1
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellItemModel("Two (default)", ConfigAssetItemsInRow, cell =>
                        cell.Accessory = _assetItemsInRow == 2
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellItemModel("Three", ConfigAssetItemsInRow, cell =>
                        cell.Accessory = _assetItemsInRow == 3
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellItemModel("Only Photos (default)", ConfigCaptureMode, cell =>
                        cell.Accessory = _captureMode == CameraMode.Photo
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellItemModel("Photos and Live Photos", ConfigCaptureMode,
                        cell =>
                            cell.Accessory = _captureMode == CameraMode.PhotoAndLivePhoto
                                ? UITableViewCellAccessory.Checkmark
                                : UITableViewCellAccessory.None),
                    new CellItemModel("Photos and Videos", ConfigCaptureMode, cell =>
                        cell.Accessory = _captureMode == CameraMode.PhotoAndVideo
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellItemModel("Don't save (default)", ConfigSavesCapturedAssets,
                        cell =>
                            cell.Accessory = _savesCapturedAssets
                                ? UITableViewCellAccessory.Checkmark
                                : UITableViewCellAccessory.None),
                    new CellItemModel("Save", ConfigSavesCapturedAssets, cell =>
                        cell.Accessory = _savesCapturedAssets
                            ? UITableViewCellAccessory.None
                            : UITableViewCellAccessory.Checkmark),
                }
            };
        }
    }
}