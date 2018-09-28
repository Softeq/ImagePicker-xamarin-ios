using System;
using System.Collections.Generic;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;
using YSImagePicker.Extensions;
using YSImagePicker.Interfaces;
using YSImagePicker.Media;
using YSImagePicker.Media.Capture;
using YSImagePicker.Models;
using YSImagePicker.Operations;
using YSImagePicker.Views;

namespace YSImagePicker.Public
{
    public class ImagePickerController : UIViewController, IPHPhotoLibraryChangeObserver, IImagePickerDelegate,
        ICameraCollectionViewCellDelegate
    {
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            PHPhotoLibrary.SharedPhotoLibrary.UnregisterChangeObserver(this);
            _captureSession.Suspend();
            Console.WriteLine($"deinit: describing: {this}");
        }

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
        public UICollectionView CollectionView
        {
            get { return ImagePickerView.UICollectionView; }
        }

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
        /// Global appearance proxy object. Use this object to set appearance
        /// for all instances of Image Picker. If you wish to set an appearance
        /// on instances use corresponding instance method.
        ///
        public Appearance AppearanceClassProxy => ClassAppearanceProxy;

        ///
        /// Instance appearance proxy object. Use this object to set appearance
        /// for this particular instance of Image Picker. This has precedence over
        /// global appearance.
        ///
        public Appearance AppearanceInstance()
        {
            if (_instanceAppearanceProxy == null)
            {
                _instanceAppearanceProxy = new Appearance();
            }

            return _instanceAppearanceProxy;
        }

        // MARK: Private Methods

        private CollectionViewUpdatesCoordinator _collectionViewCoordinator;

        private readonly ImagePickerDataSource _collectionViewDataSource =
            new ImagePickerDataSource(new ImagePickerAssetModel());

        private ImagePickerDelegate _collectionViewDelegate;

        private CaptureSession _captureSession;

        private void UpdateItemSize()
        {
            if (_collectionViewDelegate.Layout == null)
            {
                return;
            }

            var itemsInRow = LayoutConfiguration.NumberOfAssetItemsInRow;
            var scrollDirection = LayoutConfiguration.ScrollDirection;
            var cellSize =
                _collectionViewDelegate.Layout.SizeForItem(itemsInRow, null, CollectionView, scrollDirection);
            var scale = UIScreen.MainScreen.Scale;
            var thumbnailSize = new CGSize(cellSize.Width * scale, cellSize.Height * scale);
            _collectionViewDataSource.AssetsModel.ThumbnailSize = thumbnailSize;

            //TODO: we need to purge all image asset caches if item size changed
        }

        private void UpdateContentInset()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                CollectionView.ContentInset =
                    new UIEdgeInsets(CollectionView.ContentInset.Top, View.SafeAreaInsets.Left,
                        CollectionView.ContentInset.Bottom, View.SafeAreaInsets.Right);
            }
        }

        /// View is used when there is a need for an overlay view over whole image picker
        /// view hierarchy. For example when there is no permissions to photo library.
        private UIView _overlayView;

        /// Reload collection view layout/data based on authorization status of photo library
        private void ReloadData(PHAuthorizationStatus status)
        {
            switch (status)
            {
                case PHAuthorizationStatus.Authorized:
                    _collectionViewDataSource.AssetsModel.UpdateFetchResult(AssetsFetchResultBlock?.Invoke());
                    _collectionViewDataSource.LayoutModel = new LayoutModel(LayoutConfiguration,
                        (int) _collectionViewDataSource.AssetsModel.FetchResult.Count);
                    break;
                case PHAuthorizationStatus.Restricted:
                case PHAuthorizationStatus.Denied:
                    var view = _overlayView ?? DataSource?.ImagePicker(this, status);
                    if (view != null && !view.Superview.Equals(CollectionView))
                    {
                        CollectionView.BackgroundView = view;
                        _overlayView = view;
                    }

                    break;
                case PHAuthorizationStatus.NotDetermined:
                    PHPhotoLibrary.RequestAuthorization(authorizationStatus =>
                    {
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            Console.WriteLine("Tests:1");
                            ReloadData(authorizationStatus);
                        });
                    });
                    break;
            }
        }

        ///appearance object for global instances
        public static readonly Appearance ClassAppearanceProxy = new Appearance();

        ///appearance object for an instance
        private Appearance _instanceAppearanceProxy;

        public override void LoadView()
        {
            base.LoadView();
            var nib = UINib.FromName("ImagePickerView", NSBundle.MainBundle);
            View = nib.Instantiate(null, null)[0] as ImagePickerView;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _collectionViewDelegate = new ImagePickerDelegate(new ImagePickerLayout(LayoutConfiguration), this);

            //apply appearance
            var appearance = _instanceAppearanceProxy ?? ClassAppearanceProxy;
            ImagePickerView.BackgroundColor = appearance.BackgroundColor;
            CollectionView.BackgroundColor = appearance.BackgroundColor;

            //create animator
            _collectionViewCoordinator = new CollectionViewUpdatesCoordinator(CollectionView);

            //configure flow layout
            var collectionViewLayout = CollectionView.CollectionViewLayout as UICollectionViewFlowLayout;
            collectionViewLayout.ScrollDirection = LayoutConfiguration.ScrollDirection;
            collectionViewLayout.MinimumInteritemSpacing = LayoutConfiguration.InterItemSpacing;
            collectionViewLayout.MinimumLineSpacing = LayoutConfiguration.InterItemSpacing;

            //finish configuring collection view
            CollectionView.DataSource = _collectionViewDataSource;
            CollectionView.Delegate = _collectionViewDelegate;
            CollectionView.AllowsMultipleSelection = true;
            CollectionView.ShowsVerticalScrollIndicator = false;
            CollectionView.ShowsHorizontalScrollIndicator = false;
            switch (LayoutConfiguration.ScrollDirection)
            {
                case UICollectionViewScrollDirection.Horizontal:
                    CollectionView.AlwaysBounceHorizontal = true;
                    break;
                case UICollectionViewScrollDirection.Vertical:
                    CollectionView.AlwaysBounceVertical = true;
                    break;
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                CollectionView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            }

            //gesture recognizer to detect taps on a camera cell (selection is disabled)
            var recognizer = new UITapGestureRecognizer(TapGestureRecognized)
            {
                CancelsTouchesInView = false
            };

            View.AddGestureRecognizer(recognizer);

            //apply cell registrator to collection view
            CollectionView.Apply(CellRegistrator, CaptureSettings.CameraMode);

            //connect all remaining objects as needed
            _collectionViewDataSource.CellRegistrator = CellRegistrator;

            //register for photo library updates - this is needed when changing permissions to photo library
            //TODO: this is expensive (loading library for the first time)
            PHPhotoLibrary.SharedPhotoLibrary.RegisterChangeObserver(this);

            //determine auth status and based on that reload UI
            ReloadData(PHPhotoLibrary.AuthorizationStatus);

            //configure capture session
            if (LayoutConfiguration.ShowsCameraItem)
            {
                _captureSession = CaptureFactory.Create(GetCameraCell, Delegate, CaptureSettings.CameraMode);
                _captureSession.Prepare(
                    GetCaptureVideoOrientation(UIApplication.SharedApplication.StatusBarOrientation));
            }
        }

        private CameraCollectionViewCell GetCameraCell()
        {
            return CollectionView.GetCameraCell(LayoutConfiguration);
        }

        private void TapGestureRecognized(UIGestureRecognizer sender)
        {
            if (sender.State != UIGestureRecognizerState.Ended)
            {
                return;
            }

            var cameraCell = CollectionView.GetCameraCell(LayoutConfiguration);

            if (cameraCell == null)
            {
                return;
            }

            var point = sender.LocationInView(cameraCell);
            if (cameraCell.TouchIsCaptureEffective(point))
            {
                TakePicture();
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            UpdateItemSize();
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            UpdateContentInset();
            CollectionView.CollectionViewLayout.InvalidateLayout();
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);
            //update capture session with new interface orientation

            _captureSession?.UpdateVideoOrientation(
                GetCaptureVideoOrientation(UIApplication.SharedApplication.StatusBarOrientation));

            coordinator.AnimateAlongsideTransition(context => { UpdateContentInset(); },
                context => { UpdateItemSize(); });
        }

        public AVCaptureVideoOrientation GetCaptureVideoOrientation(UIInterfaceOrientation orientation)
        {
            switch (orientation)
            {
                case UIInterfaceOrientation.Portrait:
                case UIInterfaceOrientation.Unknown:
                    return AVCaptureVideoOrientation.Portrait;
                case UIInterfaceOrientation.PortraitUpsideDown:
                    return AVCaptureVideoOrientation.PortraitUpsideDown;
                case UIInterfaceOrientation.LandscapeRight:
                    return AVCaptureVideoOrientation.LandscapeRight;
                case UIInterfaceOrientation.LandscapeLeft:
                    return AVCaptureVideoOrientation.LandscapeLeft;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
            }
        }

        public void PhotoLibraryDidChange(PHChange changeInstance)
        {
            var fetchResult = _collectionViewDataSource.AssetsModel.FetchResult;
            var changes = changeInstance.GetFetchResultChangeDetails(fetchResult);

            if (fetchResult == null || changes == null)
            {
                return;
            }

            _collectionViewCoordinator.PerformDataSourceUpdate(() =>
            {
                //update old fetch result with these updates
                _collectionViewDataSource.AssetsModel.UpdateFetchResult(changes.FetchResultAfterChanges);

                //update layout model because it changed
                _collectionViewDataSource.LayoutModel = new LayoutModel(LayoutConfiguration,
                    (int) _collectionViewDataSource.AssetsModel.FetchResult.Count);
            });

            //perform update animations
            _collectionViewCoordinator.PerformChanges(changes, LayoutConfiguration.SectionIndexForAssets);
        }

        public void DidSelectActionItemAt(ImagePickerDelegate imagePickerDelegate, int index)
        {
            Delegate?.DidSelectActionItemAt(this, index);
        }

        public void DidSelectAssetItemAt(ImagePickerDelegate imagePickerDelegate, int index)
        {
            Delegate?.DidSelect(this, Asset(index));
        }

        public void DidDeselectAssetItemAt(ImagePickerDelegate imagePickerDelegate, int index)
        {
            Delegate?.DidDeselectAsset(this, Asset(index));
        }

        public void WillDisplayActionCell(ImagePickerDelegate imagePickerDelegate, UICollectionViewCell cell, int index)
        {
            if (cell is ActionCell defaultCell)
            {
                defaultCell.Update(index, LayoutConfiguration);
            }

            Delegate?.WillDisplayActionItem(this, cell, index);
        }

        public void WillDisplayCameraCell(ImagePickerDelegate imagePickerDelegate, CameraCollectionViewCell cell)
        {
            //setup cell if needed
            if (cell.Delegate == null)
            {
                cell.Delegate = this;
                cell.PreviewView.Session = _captureSession?.Session;
                if (_captureSession != null)
                {
                    _captureSession.PreviewLayer = cell.PreviewView.PreviewLayer;
                }

                var config = _captureSession?.PresetConfiguration;
                if (config == SessionPresetConfiguration.Videos)
                {
                    cell.IsVisualEffectViewUsedForBlurring = true;
                }
            }

            if (cell is LivePhotoCameraCell liveCameraCell)
            {
                liveCameraCell.UpdateWithCameraMode(CaptureSettings.CameraMode);
            }
            if (_captureSession.PhotoCaptureSession != null)
            {
                //update live photos
                cell.UpdateLivePhotoStatus(_captureSession.PhotoCaptureSession.InProgressLivePhotoCapturesCount > 0, false);
            }
            //update video recording status
            var isRecordingVideo = _captureSession?.VideoCaptureSession?.IsRecordingVideo ?? false;
            cell.UpdateRecordingVideoStatus(isRecordingVideo, false);

            //update authorization status if it's changed
            var status = AVCaptureDevice.GetAuthorizationStatus(AVAuthorizationMediaType.Video);
            if (cell.AuthorizationStatus != status)
            {
                cell.AuthorizationStatus = status;
            }

            //resume session only if not recording video
            if (isRecordingVideo == false)
            {
                _captureSession?.Resume();
            }
        }

        public void DidEndDisplayingCameraCell(ImagePickerDelegate imagePickerDelegate,
            CameraCollectionViewCell cell)
        {
            var isRecordingVideo = _captureSession?.VideoCaptureSession?.IsRecordingVideo ?? false;

            //suspend session only if not recording video, otherwise the recording would be stopped.
            if (isRecordingVideo == false)
            {
                _captureSession?.Suspend();

                DispatchQueue.MainQueue.DispatchAsync(() => { cell.BlurIfNeeded(false, null); });
            }
        }

        public void WillDisplayAssetCell(ImagePickerDelegate imagePickerDelegate, ImagePickerAssetCell cell,
            int index)
        {
            var theAsset = Asset(index);

            //if the cell is default cell provided by Image Picker, it's our responsibility
            //to update the cell with the asset.
            var defaultCell = cell as VideoAssetCell;

            defaultCell?.Update(theAsset);

            Delegate?.WillDisplayAssetItem(this, cell, theAsset);
        }

        public void DidScroll(ImagePickerDelegate imagePickerDelegate, UIScrollView scrollView)
        {
        }

        public void TakePicture()
        {
            _captureSession?.PhotoCaptureSession.CapturePhoto(LivePhotoMode.Off,
                CaptureSettings.SavesCapturedPhotosToPhotoLibrary);
        }

        public void TakeLivePhoto()
        {
            _captureSession?.PhotoCaptureSession.CapturePhoto(LivePhotoMode.On,
                CaptureSettings.SavesCapturedLivePhotosToPhotoLibrary);
        }

        public void StartVideoRecording()
        {
            _captureSession?.VideoCaptureSession?.StartVideoRecording(CaptureSettings
                .SavesCapturedVideosToPhotoLibrary);
        }

        public void StopVideoRecording()
        {
            _captureSession?.VideoCaptureSession?.StopVideoRecording();
        }

        public void FlipCamera(Action completion)
        {
            if (_captureSession == null)
            {
                return;
            }

            var cameraCell = CollectionView.GetCameraCell(LayoutConfiguration);
            if (cameraCell == null)
            {
                _captureSession.ChangeCamera(completion);
                return;
            }

            // 1. blur cell
            cameraCell.BlurIfNeeded(true, () =>
            {
                {
                    // 2. flip camera
                    _captureSession.ChangeCamera(() =>
                    {
                        UIView.Transition(cameraCell.PreviewView, 0.25,
                            UIViewAnimationOptions.TransitionFlipFromLeft | UIViewAnimationOptions.AllowAnimatedContent,
                            null, () => { cameraCell.UnblurIfNeeded(true, completion); });
                    });
                }
            });
        }
    }
}