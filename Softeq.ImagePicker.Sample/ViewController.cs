using System;
using System.Collections.Generic;
using System.Linq;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Photos;
using Softeq.ImagePicker.Public;
using Softeq.ImagePicker.Sample.Models.Enums;
using UIKit;

namespace Softeq.ImagePicker.Sample
{
    public partial class ViewController : UITableViewController
    {
        private readonly ImagePickerConfigurationHandlerClass _imagePickerConfigurationHandlerClass =
            new ImagePickerConfigurationHandlerClass();
        private ImagePickerController _imagePicker;
        private ImagePickerControllerDelegate _imagePickerController;

        private ImagePickerControllerDataSource _imagePickerControllerDataSource =
            new ImagePickerControllerDataSource();

        private UIView _currentInputView;
        private UIButton _presentButton;
        private bool _openedAsInputView = false;

        public override UIView InputView => _currentInputView;

        public override UIView InputAccessoryView => _presentButton;

        protected ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            CreatePresentButton();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _presentButton = CreatePresentButton();

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
                _currentInputView = null;
            }

            return result;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return _imagePickerConfigurationHandlerClass.CellsData.Count;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _imagePickerConfigurationHandlerClass.CellsData[(int)section].Length;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("cellId", indexPath);
            cell.TextLabel.Text =
                _imagePickerConfigurationHandlerClass.CellsData[indexPath.Section][indexPath.Row].Title;

            _imagePickerConfigurationHandlerClass.CellsData[indexPath.Section][indexPath.Row].ConfigBlock?.Invoke(cell);

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // deselect
            tableView.DeselectRow(indexPath, true);

            // perform selector
            var selector = _imagePickerConfigurationHandlerClass.CellsData[indexPath.Section][indexPath.Row].Selector;
            var argumentType = _imagePickerConfigurationHandlerClass.CellsData[indexPath.Section][indexPath.Row]
                .SelectorArgument;
            if (argumentType == SelectorArgument.IndexPath)
            {
                selector?.Invoke(indexPath);
            }

            // update checks in section
            UncheckCellsInSection(indexPath);
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return _imagePickerConfigurationHandlerClass.SectionsData.ElementAt((int)section).GroupTitle;
        }

        public override string TitleForFooter(UITableView tableView, nint section)
        {
            return _imagePickerConfigurationHandlerClass.SectionsData.ElementAt((int)section).GroupDescription;
        }

        private void PresentButtonTapped(object sender, EventArgs e)
        {
            _presentButton.Selected = !_presentButton.Selected;

            if (!_presentButton.Selected)
            {
                UpdateNavigationItem(0);
                _imagePicker.Release();
                _currentInputView = null;
                ReloadInputViews();
                return;
            }

            _imagePicker = _imagePickerConfigurationHandlerClass.CreateImagePicker();


            _imagePickerController = new ImagePickerControllerDelegate()
            {
                DidSelectActionItemAction = DidSelectActionItemAt,
                DidDeselectAssetAction = UpdateSelectedItems,
                DidSelectAssetAction = UpdateSelectedItems

            };

            _imagePicker.Delegate = _imagePickerController;
            _imagePicker.DataSource = _imagePickerControllerDataSource;

            // presentation
            // before we present VC we can ask for authorization to photo library,
            // if we don't do it now, Image Picker will ask for it automatically
            // after it's presented.
            PHPhotoLibrary.RequestAuthorization(handler =>
            {
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    // we can present VC regardless of status because we support
                    // non granted states in Image Picker. Please check `ImagePickerControllerDataSource`
                    // for more info.
                    if (_imagePickerConfigurationHandlerClass.PresentsModally)
                    {
                        _imagePicker.LayoutConfiguration.ScrollDirection = UICollectionViewScrollDirection.Vertical;
                        PresentPickerModally(_imagePicker);
                    }
                    else
                    {
                        _imagePicker.LayoutConfiguration.ScrollDirection =
                            UICollectionViewScrollDirection.Horizontal;
                        PresentPickerAsInputView(_imagePicker);
                    }
                });
            });
        }

        private void PresentPickerAsInputView(ImagePickerController vc)
        {
            //if you want to present view as input view, you have to set flexible height
            //to adopt natural keyboard height or just set an layout constraint height
            //for specific height.
            vc.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            _currentInputView = vc.View;
            _openedAsInputView = true;

            ReloadInputViews();
        }

        private void PresentPickerModally(ImagePickerController vc)
        {
            _openedAsInputView = false;

            vc.NavigationItem.LeftBarButtonItem =
                new UIBarButtonItem("Dismiss", UIBarButtonItemStyle.Done, DismissPresentedImagePicker);
            var nc = new UINavigationController(vc);
            PresentViewController(nc, true, null);
        }

        void DismissPresentedImagePicker(object sender, EventArgs e)
        {
            UpdateNavigationItem(0);
            _imagePicker.Release();
            _presentButton.Selected = false;
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

        public void UncheckCellsInSection(NSIndexPath indexPath)
        {
            foreach (var path in TableView.IndexPathsForVisibleRows.Where(path => path.Section == indexPath.Section))
            {
                TableView.CellAt(path).Accessory = path.Equals(indexPath)
                    ? UITableViewCellAccessory.Checkmark
                    : UITableViewCellAccessory.None;
            }
        }

        private UIButton CreatePresentButton()
        {
            var bottomAdjustment = 0f;

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                bottomAdjustment = (float)TableView.AdjustedContentInset.Bottom;
            }

            var button = new UIButton(UIButtonType.Custom)
            {
                BackgroundColor = UIColor.FromRGB(208 / 255f, 2 / 255f, 27 / 255f),
                ContentEdgeInsets = new UIEdgeInsets(0, 0, bottomAdjustment / 2, 0),
                Frame = new CGRect { Size = new CGSize(0, 44 + bottomAdjustment) }
            };

            button.SetTitle("Present", UIControlState.Normal);
            button.SetTitle("Dismiss", UIControlState.Selected);
            button.AddTarget(PresentButtonTapped, UIControlEvent.TouchUpInside);
            return button;
        }

        private void DidSelectActionItemAt(int index)
        {
            Console.WriteLine($"did select action {index}");

            //before we present system image picker, we must update present button
            //because first responder will be dismissed

            _presentButton.Selected = false;

            if (_openedAsInputView)
            {
                _imagePicker.Release();
            }

            switch (index)
            {
                case 0 when UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera):
                    {
                        var vc = new UIImagePickerController
                        {
                            SourceType = UIImagePickerControllerSourceType.Camera,
                            AllowsEditing = true
                        };
                        var mediaTypes =
                            UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.Camera);
                        if (mediaTypes != null)
                        {
                            vc.MediaTypes = mediaTypes;
                        }

                        NavigationController?.VisibleViewController?.PresentViewController(vc, true, null);
                        break;
                    }
                case 1 when UIImagePickerController.IsSourceTypeAvailable(
                    UIImagePickerControllerSourceType.PhotoLibrary):
                    {
                        var vc = new UIImagePickerController { SourceType = UIImagePickerControllerSourceType.PhotoLibrary };

                        NavigationController?.VisibleViewController?.PresentViewController(vc, true, null);
                        break;
                    }
            }

        }

        private void UpdateSelectedItems(IReadOnlyList<PHAsset> readOnlyList)
        {
            Console.WriteLine($"selected assets: {readOnlyList.Count}");
            UpdateNavigationItem(readOnlyList.Count);
        }
    }
}