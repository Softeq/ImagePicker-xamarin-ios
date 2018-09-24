using System;
using System.Collections.Generic;
using System.Linq;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;
using YSImagePicker.Public;

namespace TestApplication
{
    public enum SelectorArgument
    {
        IndexPath,
        None
    }

    public enum CameraItemConfig
    {
        Enabled = 0,
        Disabled = 1
    }

    public enum AssetsSource
    {
        RecentlyAdded = 0,
        OnlyVideos = 1,
        OnlySelfies = 2
    }

    public class CellData
    {
        public string Title { get; }
        public Action<NSIndexPath> Selector { get; }
        public SelectorArgument SelectorArgument { get; } = SelectorArgument.IndexPath;
        public Action<UITableViewCell, ViewController> ConfigBlock { get; }

        public CellData(string title, Action<NSIndexPath> selector, Action<UITableViewCell, ViewController> configBlock)
        {
            Title = title;
            Selector = selector;
            ConfigBlock = configBlock;
        }
    }

    public partial class ViewController : UITableViewController
    {
        private readonly List<CellData[]> _cellsData;
        readonly ImagePickerControllerTest _imagePickerControllerTest = new ImagePickerControllerTest();

        ImagePickerControllerDataSourceTest _imagePickerControllerDataSourceTest =
            new ImagePickerControllerDataSourceTest();

        private readonly Dictionary<string, string> _sectionsData = new Dictionary<string, string>
        {
            {"Presentation", null},
            {"Action Items", null},
            {"Camera Item", null},
            {"Assets Source", null},
            {"Asset Items in a row", null},
            {"Capture mode", null},
            {
                "Save Assets",
                "Assets will be saved to Photo Library. This applies to photos only. Live photos and videos are always saved."
            }
        };

        private void TogglePresentationMode(NSIndexPath indexPath)
        {
            _presentsModally = indexPath.Row == 1;
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

        //default configuration values
        private bool _presentsModally;
        private int _numberOfActionItems = 2;
        private CameraItemConfig _cameraConfig = CameraItemConfig.Enabled;
        private AssetsSource _assetsSource = AssetsSource.RecentlyAdded;
        private int _assetItemsInRow = 2;
        private CameraMode _captureMode = CameraMode.PhotoAndLivePhoto;
        private bool _savesCapturedAssets;

        protected ViewController(IntPtr handle) : base(handle)
        {
            _cellsData = new List<CellData[]>
            {
                new[]
                {
                    new CellData("As input view", TogglePresentationMode, (cell, controller) =>
                        cell.Accessory = controller._presentsModally
                            ? UITableViewCellAccessory.None
                            : UITableViewCellAccessory.Checkmark),
                    new CellData("Modally", TogglePresentationMode, (cell, controller) =>
                        cell.Accessory = controller._presentsModally
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellData("Disabled", SetNumberOfActionItems, (cell, controller) =>
                        cell.Accessory = controller._numberOfActionItems == 0
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellData("One item", SetNumberOfActionItems, (cell, controller) =>
                        cell.Accessory = controller._numberOfActionItems == 1
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellData("Two items (default)", SetNumberOfActionItems,
                        (cell, controller) =>
                            cell.Accessory = controller._numberOfActionItems == 2
                                ? UITableViewCellAccessory.Checkmark
                                : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellData("Enabled (default)", ConfigCameraItem, (cell, controller) =>
                        cell.Accessory = controller._cameraConfig == CameraItemConfig.Enabled
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellData("Disabled", ConfigCameraItem, (cell, controller) =>
                        cell.Accessory = controller._cameraConfig == CameraItemConfig.Disabled
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellData("Camera Roll (default)", ConfigAssetsSource,
                        (cell, controller) =>
                            cell.Accessory = controller._assetsSource == AssetsSource.RecentlyAdded
                                ? UITableViewCellAccessory.Checkmark
                                : UITableViewCellAccessory.None),
                    new CellData("Only videos", ConfigAssetsSource, (cell, controller) =>
                        cell.Accessory = controller._assetsSource == AssetsSource.OnlyVideos
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellData("Only selfies", ConfigAssetsSource, (cell, controller) =>
                        cell.Accessory = controller._assetsSource == AssetsSource.OnlySelfies
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellData("One", ConfigAssetItemsInRow, (cell, controller) =>
                        cell.Accessory = controller._assetItemsInRow == 1
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellData("Two (default)", ConfigAssetItemsInRow, (cell, controller) =>
                        cell.Accessory = controller._assetItemsInRow == 2
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellData("Three", ConfigAssetItemsInRow, (cell, controller) =>
                        cell.Accessory = controller._assetItemsInRow == 3
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellData("Only Photos (default)", ConfigCaptureMode, (cell, controller) =>
                        cell.Accessory = controller._captureMode == CameraMode.Photo
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None),
                    new CellData("Photos and Live Photos", ConfigCaptureMode,
                        (cell, controller) =>
                            cell.Accessory = controller._captureMode == CameraMode.PhotoAndLivePhoto
                                ? UITableViewCellAccessory.Checkmark
                                : UITableViewCellAccessory.None),
                    new CellData("Photos and Videos", ConfigCaptureMode, (cell, controller) =>
                        cell.Accessory = controller._captureMode == CameraMode.PhotoAndVideo
                            ? UITableViewCellAccessory.Checkmark
                            : UITableViewCellAccessory.None)
                },
                new[]
                {
                    new CellData("Don't save (default)", ConfigSavesCapturedAssets,
                        (cell, controller) =>
                            cell.Accessory = controller._savesCapturedAssets
                                ? UITableViewCellAccessory.Checkmark
                                : UITableViewCellAccessory.None),
                    new CellData("Save", ConfigSavesCapturedAssets, (cell, controller) =>
                        cell.Accessory = controller._savesCapturedAssets
                            ? UITableViewCellAccessory.None
                            : UITableViewCellAccessory.Checkmark),
                }
            };
            // Note: this .ctor should not contain any initialization Console.WriteLineic.
        }

        public UIButton CreatePresentButton()
        {
            var button = new UIButton(UIButtonType.Custom);
            var bottomAdjustment = 0f;
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                bottomAdjustment = (float) TableView.AdjustedContentInset.Bottom;
            }

            var frame = button.Frame;
            frame.Size = new CGSize(0, 44 + bottomAdjustment);
            button.Frame = frame;

            var buttonContentEdgeInsets = button.ContentEdgeInsets;
            buttonContentEdgeInsets.Bottom = bottomAdjustment / 2;
            button.ContentEdgeInsets = buttonContentEdgeInsets;
            button.BackgroundColor = UIColor.FromRGB(208 / 255f, 2 / 255f, 27 / 255f);
            button.SetTitle("Present", UIControlState.Normal);
            button.SetTitle("Dismiss", UIControlState.Selected);
            button.AddTarget(PresentButtonTapped, UIControlEvent.TouchUpInside);
            return button;
        }

        private void PresentButtonTapped(object sender, EventArgs e)
        {
            PresentButton.Selected = !PresentButton.Selected;

            if (PresentButton.Selected)
            {
                // create new instance
                var imagePicker = new ImagePickerController
                {
                    Delegate = _imagePickerControllerTest,
                    DataSource = _imagePickerControllerDataSourceTest
                };

                // set action items
                switch (_numberOfActionItems)
                {
                    case 1:
                        imagePicker.LayoutConfiguration.ShowsFirstActionItem = true;
                        imagePicker.LayoutConfiguration.ShowsSecondActionItem = false;
                        break;
                    //if you wish to register your own action cell register it here,
                    //it can by any UICollectionViewCell
                    //imagePicker.cellRegistrator.register(nib: UINib(nibName: "IconWithTextCell", bundle: nil), forActionItemAt: 0)
                    case 2:
                        imagePicker.LayoutConfiguration.ShowsFirstActionItem = true;
                        imagePicker.LayoutConfiguration.ShowsSecondActionItem = true;
                        break;
                    //if you wish to register your own action cell register it here,
                    //it can by any UICollectionViewCell
                    //imagePicker.cellRegistrator.registerNibForActionItems(UINib(nibName: "IconWithTextCell", bundle: nil))
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
//                        imagePicker.CellRegistrator.Register(UINib.FromName("CustomVideoCell", null),
//                            PHAssetMediaType.Video);
                        imagePicker.AssetsFetchResultBlock = () =>
                        {
                            var collection = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.SmartAlbum,
                                PHAssetCollectionSubtype.SmartAlbumVideos, null).firstObject;
                            if (collection == null)
                            {
                                return
                                    null; //you can return nil if you did not find desired fetch result, default fetch result will be used.
                            }

                            return null;
                            //return PHAsset.FetchAssets();
                        };
                        break;
                    case AssetsSource.OnlySelfies:
                        //registering custom image cell to demonstrate how to use custom cells
                        //please note that custom asset cells must conform to  ImagePickerAssetCell protocol
                        //imagePicker.CellRegistrator.RegisterNibForAssetItems(UINib.FromName("CustomImageCell", null));
                        imagePicker.AssetsFetchResultBlock = () =>
                        {
                            var collection = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.SmartAlbum,
                                PHAssetCollectionSubtype.SmartAlbumSelfPortraits, null).firstObject;

                            if (collection == null)
                            {
                                return null;
                            }

                            return null;
                            //return PHAsset.FetchAssets(collection, null);
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
                    //imagePicker.cellRegistrator.registerNibForCameraItem(UINib(nibName: "CustomNibName", bundle: nil))
                    case CameraMode.PhotoAndLivePhoto:
                        imagePicker.CaptureSettings.CameraMode = CameraMode.PhotoAndLivePhoto;
                        break;
                    //if you wish to use your own cell for photo and live photo register it here:
                    //please note that custom cell must sublcass `CameraCollectionViewCell`.
                    //imagePicker.cellRegistrator.registerNibForCameraItem(UINib(nibName: "CustomNibName", bundle: nil))
                    case CameraMode.PhotoAndVideo:
                        imagePicker.CaptureSettings.CameraMode = CameraMode.PhotoAndVideo;
                        //if you wish to use your own cell for photo and video register it here:
                        //please note that custom cell must sublcass `CameraCollectionViewCell`.
                        //imagePicker.cellRegistrator.registerNibForCameraItem(UINib(nibName: "CustomNibName", bundle: nil))
                        break;
                }

                // save capture assets to photo library?
                imagePicker.CaptureSettings.SavesCapturedPhotosToPhotoLibrary = _savesCapturedAssets;

                // presentation
                // before we present VC we can ask for authorization to photo library,
                // if we dont do it now, Image Picker will ask for it automatically
                // after it's presented.
                PHPhotoLibrary.RequestAuthorization(handler =>
                {
                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        // we can present VC regardless of status because we support
                        // non granted states in Image Picker. Please check `ImagePickerControllerDataSource`
                        // for more info.
                        if (_presentsModally)
                        {
                            imagePicker.LayoutConfiguration.ScrollDirection = UICollectionViewScrollDirection.Vertical;
                            PresentPickerModally(imagePicker);
                        }
                        else
                        {
                            imagePicker.LayoutConfiguration.ScrollDirection = UICollectionViewScrollDirection.Horizontal;
                            PresentPickerAsInputView(imagePicker);
                        }
                    });
                });
            }
            else
            {
                UpdateNavigationItem(0);
                CurrentInputView = null;
                ReloadInputViews();
            }
        }

        public void PresentPickerAsInputView(ImagePickerController vc)
        {
            //if you want to present view as input view, you have to set flexible height
            //to adopt natural keyboard height or just set an layout constraint height
            //for specific height.
            vc.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            CurrentInputView = vc.View;

            ReloadInputViews();
        }

        public void PresentPickerModally(ImagePickerController vc)
        {
            vc.NavigationItem.LeftBarButtonItem = new UIBarButtonItem("Dismiss", UIBarButtonItemStyle.Done, this, null);
            var nc = new UINavigationController(vc);
            PresentViewController(nc, true, DismissPresentedImagePicker);
        }

        public void DismissPresentedImagePicker()
        {
            UpdateNavigationItem(0);
            PresentButton.Selected = false;
            NavigationController?.VisibleViewController?.DismissViewController(true, null);
        }

        private void UpdateNavigationItem(int selectedCount)
        {
            if (selectedCount == 0)
            {
                if (NavigationController?.VisibleViewController?.NavigationItem != null)
                {
                    NavigationController.VisibleViewController.NavigationItem.RightBarButtonItem = null;
                }
            }
            else
            {
                var title = $"Items ({selectedCount})";
                if (NavigationController.VisibleViewController.NavigationItem != null)
                {
                    NavigationController.VisibleViewController.NavigationItem.RightBarButtonItem =
                        new UIBarButtonItem(title, UIBarButtonItemStyle.Plain, null, null);
                }
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            CreatePresentButton();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            PresentButton = CreatePresentButton();

            NavigationItem.Title = "Image Picker";
            TableView.RegisterClassForCellReuse(typeof(UITableViewCell), "cellId");
            TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.None;
        }

        public override bool CanBecomeFirstResponder => true;

        public override bool ResignFirstResponder()
        {
            var result = base.ResignFirstResponder();

            if (result)
            {
                CurrentInputView = null;
            }

            return result;
        }

        private UIView CurrentInputView { get; set; }

        public override UIView InputView => CurrentInputView;

        public override UIView InputAccessoryView => PresentButton;

        private UIButton PresentButton { get; set; }

        public override nint NumberOfSections(UITableView tableView)
        {
            return _cellsData.Count;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _cellsData[(int) section].Length;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("cellId", indexPath);
            cell.TextLabel.Text = _cellsData[indexPath.Section][indexPath.Row].Title;

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // deselect
            tableView.DeselectRow(indexPath, animated: true);

            // perform selector
            var selector = _cellsData[indexPath.Section][indexPath.Row].Selector;
            var argumentType = _cellsData[indexPath.Section][indexPath.Row].SelectorArgument;
            if (argumentType == SelectorArgument.IndexPath)
            {
                selector?.Invoke(indexPath);
            }

            // update checks in section
            UncheckCellsInSection(indexPath);
        }

        public void UncheckCellsInSection(NSIndexPath indexPath)
        {
            foreach (var path in TableView.IndexPathsForVisibleRows.Where(path => path.Section == indexPath.Section))
            {
                var cell = TableView.CellAt(path);
                cell.Accessory = path.Equals(indexPath)
                    ? UITableViewCellAccessory.Checkmark
                    : UITableViewCellAccessory.None;
            }
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return _sectionsData.Keys.Skip((int) section).First();
        }

        public override string TitleForFooter(UITableView tableView, nint section)
        {
            return _sectionsData.Keys.Skip((int) section).First();
        }
    }

    public class ImagePickerControllerTest : ImagePickerControllerDelegate
    {
    }

    public class ImagePickerControllerDataSourceTest : ImagePickerControllerDataSource
    {
        public override UIView ImagePicker(ImagePickerController controller, PHAuthorizationStatus status)
        {
            var infoLabel = new UILabel(CGRect.Empty)
            {
                BackgroundColor = UIColor.Green, TextAlignment = UITextAlignment.Center, Lines = 0
            };
            switch (status)
            {
                case PHAuthorizationStatus.Restricted:
                    infoLabel.Text = "Access is restricted\n\nPlease open Settings app and update privacy settings.";
                    break;
                case PHAuthorizationStatus.Denied:
                    infoLabel.Text =
                        "Access is denied by user\n\nPlease open Settings app and update privacy settings.";
                    break;
            }

            return infoLabel;
        }
    }
}