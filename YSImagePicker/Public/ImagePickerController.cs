using System;
using System.Collections.Generic;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;
using YSImagePicker.Media;
using YSImagePicker.Operations;
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
        public virtual void DidSelectActionItemAt(ImagePickerController controller, int index)
        {
        }

        ///
        /// Called when user select an asset.
        ///
        public virtual void DidSelect(ImagePickerController controller, PHAsset asset)
        {
        }

        ///
        /// Called when user unselect previously selected asset.
        ///
        public virtual void DidDeselectAsset(ImagePickerController controller, PHAsset asset)
        {
        }

        ///
        /// Called when user takes new photo.
        ///
        public virtual void DidTake(ImagePickerController controller, UIImage image)
        {
        }

        ///
        /// Called right before an action item collection view cell is displayed. Use this method
        /// to configure your cell.
        ///
        public virtual void WillDisplayActionItem(ImagePickerController controller, UICollectionViewCell cell,
            int index)
        {
        }

        ///
        /// Called right before an asset item collection view cell is displayed. Use this method
        /// to configure your cell based on asset media type, subtype, etc.
        ///
        public virtual void WillDisplayAssetItem(ImagePickerController controller, ImagePickerAssetCell cell,
            PHAsset asset)
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

    public class ImagePickerController : UIViewController, IPHPhotoLibraryChangeObserver, IImagePickerDelegateDelegate,
        ICaptureSessionDelegate, ICaptureSessionPhotoCapturingDelegate, ICaptureSessionVideoRecordingDelegate,
        ICameraCollectionViewCellDelegate
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            PHPhotoLibrary.SharedPhotoLibrary.UnregisterChangeObserver(this);
            _captureSession?.Suspend();
            Console.WriteLine($"deinit: describing: {this}");
        }

        // MARK: Public API

        ///
        /// Use this object to configure layout of action, camera and asset items.
        ///
        public LayoutConfiguration LayoutConfiguration = LayoutConfiguration.Default();

        ///
        /// Use this to register a cell classes or nibs for each item types
        ///
        public CellRegistrator CellRegistrator = new CellRegistrator();

        ///
        /// Use these settings to configure how the capturing should behave
        ///
        public CaptureSettings CaptureSettings = new CaptureSettings();

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
        public UICollectionView CollectionView { get { return ImagePickerView.UICollectionView; } }

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

            return (PHAsset)_collectionViewDataSource.AssetsModel.FetchResult.ElementAt(index);
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

        private readonly ImagePickerDelegate _collectionViewDelegate = new ImagePickerDelegate();

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
                    new UIEdgeInsets(CollectionView.ContentInset.Top, View.SafeAreaInsets.Left, CollectionView.ContentInset.Bottom, View.SafeAreaInsets.Right);
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
                    _collectionViewDataSource.AssetsModel.FetchResult = AssetsFetchResultBlock?.Invoke();
                    _collectionViewDataSource.LayoutModel = new LayoutModel(LayoutConfiguration,
                        (int)_collectionViewDataSource.AssetsModel.FetchResult.Count);
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

        /// Reload camera cell based on authorization status of camera input device (video)
        private void ReloadCameraCell(AVAuthorizationStatus status)
        {
            var cameraCell = GetCameraCell(LayoutConfiguration);

            if (cameraCell == null)
            {
                return;
            }

            cameraCell.AuthorizationStatus = status;
        }

        public CameraCollectionViewCell GetCameraCell(LayoutConfiguration layout)
        {
            return CollectionView.CellForItem(NSIndexPath.FromItemSection(0, layout.SectionIndexForCamera)) as
                CameraCollectionViewCell;
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

            //apply appearance
            var appearance = _instanceAppearanceProxy ?? ClassAppearanceProxy;
            ImagePickerView.BackgroundColor = appearance.BackgroundColor;
            CollectionView.BackgroundColor = appearance.BackgroundColor;

            //create animator
            _collectionViewCoordinator = new CollectionViewUpdatesCoordinator(CollectionView);

            //configure flow layout
            var collectionViewLayout = CollectionView.CollectionViewLayout as UICollectionViewFlowLayout;
            collectionViewLayout.ScrollDirection = LayoutConfiguration.ScrollDirection;
            collectionViewLayout.MinimumInteritemSpacing = LayoutConfiguration.InteritemSpacing;
            collectionViewLayout.MinimumLineSpacing = LayoutConfiguration.InteritemSpacing;

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
            _collectionViewDelegate.Delegate = this;
            _collectionViewDelegate.Layout = new ImagePickerLayout(LayoutConfiguration);

            //register for photo library updates - this is needed when changing permissions to photo library
            //TODO: this is expensive (loading library for the first time)
            PHPhotoLibrary.SharedPhotoLibrary.RegisterChangeObserver(this);

            //determine auth satus and based on that reload UI
            ReloadData(PHPhotoLibrary.AuthorizationStatus);

            //configure capture session
            if (LayoutConfiguration.ShowsCameraItem)
            {
                var session = new CaptureSession();
                _captureSession = session;
                session.PresetConfiguration = CaptureSessionPresetConfiguration(CaptureSettings.CameraMode);
                session.VideoOrientation =
                    GetCaptureVideoOrientation(UIApplication.SharedApplication.StatusBarOrientation);
                session.Delegate = this;
                session.VideoRecordingDelegate = this;
                session.PhotoCapturingDelegate = this;
                session.Prepare();
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

        private void TapGestureRecognized(UIGestureRecognizer sender)
        {
            if (sender.State != UIGestureRecognizerState.Ended)
            {
                return;
            }

            var cameraCell = GetCameraCell(LayoutConfiguration);

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

        public GetSessionPresetConfiguration CaptureSessionPresetConfiguration(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.Photo:
                    return GetSessionPresetConfiguration.Photos;
                case CameraMode.PhotoAndLivePhoto:
                    return GetSessionPresetConfiguration.LivePhotos;
                case CameraMode.PhotoAndVideo:
                    return GetSessionPresetConfiguration.Videos;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
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
                _collectionViewDataSource.AssetsModel.FetchResult = changes.FetchResultAfterChanges;
                ;
                //update layout model because it changed
                _collectionViewDataSource.LayoutModel = new LayoutModel(LayoutConfiguration,
                    (int)_collectionViewDataSource.AssetsModel.FetchResult.Count);
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

                //when using videos preset, we are using different technique for
                //blurring the cell content. If isVisualEffectViewUsedForBlurring is
                //true, then UIVisualEffectView is used for blurring. In other cases
                //we manually blur video data output frame (it's faster). Reason why
                //we have 2 different blurring techniques is that the faster solution
                //can not be used when we have .video preset configuration.
                var config = _captureSession?.PresetConfiguration;
                if (config == GetSessionPresetConfiguration.Videos)
                {
                    cell.IsVisualEffectViewUsedForBlurring = true;
                }
            }

            if (cell is LivePhotoCameraCell liveCameraCell)
            {
                liveCameraCell.UpdateWithCameraMode(CaptureSettings.CameraMode);
            }

            //update live photos
            var inProgressLivePhotos = _captureSession?.InProgressLivePhotoCapturesCount ?? 0;
            cell.UpdateLivePhotoStatus(inProgressLivePhotos > 0, false);

            //update video recording status
            var isRecordingVideo = _captureSession?.IsRecordingVideo ?? false;
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
            var isRecordingVideo = _captureSession?.IsRecordingVideo ?? false;

            //susped session only if not recording video, otherwise the recording would be stopped.
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

        public void CaptureSessionDidResume(CaptureSession session)
        {
            Console.WriteLine("did resume");
            UnblurCellIfNeeded(true);
        }

        public void CaptureSessionDidSuspend(CaptureSession session)
        {
            Console.WriteLine("did suspend");
            BlurCellIfNeeded(true);
        }

        public void DidFail(CaptureSession session, AVError error)
        {
            Console.WriteLine("did fail");
        }

        public void DidFailConfiguringSession(CaptureSession session)
        {
            Console.WriteLine("did fail configuring");
        }

        public void CaptureGrantedSession(CaptureSession session, AVAuthorizationStatus status)
        {
            Console.WriteLine("did grant authorization to camera");
            ReloadCameraCell(status);
        }

        public void CaptureFailedSession(CaptureSession session, AVAuthorizationStatus status)
        {
            Console.WriteLine("did fail authorization to camera");
            ReloadCameraCell(status);
        }

        public void WasInterrupted(CaptureSession session, NSString reason)
        {
            Console.WriteLine("interrupted");
        }

        public void CaptureSessionInterruptionDidEnd(CaptureSession session)
        {
            Console.WriteLine("interruption ended");
        }

        private void BlurCellIfNeeded(bool animated)
        {
            var cameraCell = GetCameraCell(LayoutConfiguration);

            if (cameraCell == null)
            {
                return;
            }

            if (_captureSession == null)
            {
                return;
            }

            cameraCell.BlurIfNeeded(animated, null);
        }

        private void UnblurCellIfNeeded(bool animated)
        {
            GetCameraCell(LayoutConfiguration)?.UnblurIfNeeded(animated, null);
        }

        public void WillCapturePhotoWith(CaptureSession session, AVCapturePhotoSettings settings)
        {
            Console.WriteLine($"will capture photo {settings.UniqueID}");
        }

        public void DidCapturePhotoData(CaptureSession session, NSData didCapturePhotoData,
            AVCapturePhotoSettings settings)
        {
            Console.WriteLine($"did capture photo {settings.UniqueID}");
            Delegate?.DidTake(this, UIImage.LoadFromData(didCapturePhotoData));
        }

        public void DidFailCapturingPhotoWith(CaptureSession session, NSError error)
        {
            Console.WriteLine($"did fail capturing: {error}");
        }

        public void CaptureSessionDidChangeNumberOfProcessingLivePhotos(CaptureSession session)
        {
            var cameraCell = GetCameraCell(LayoutConfiguration);

            if (cameraCell == null)
            {
                return;
            }

            var count = session.InProgressLivePhotoCapturesCount;
            cameraCell.UpdateLivePhotoStatus(count > 0, true);
        }

        public void DidBecomeReadyForVideoRecording(CaptureSession session)
        {
            Console.WriteLine("ready for video recording");
            var cameraCell = GetCameraCell(LayoutConfiguration);

            cameraCell?.VideoRecodingDidBecomeReady();
        }

        public void DidStartVideoRecording(CaptureSession session)
        {
            Console.WriteLine("did start video recording");
            UpdateCameraCellRecordingStatusIfNeeded(true, true);
        }

        public void DidCancelVideoRecording(CaptureSession session)
        {
            Console.WriteLine("did cancel video recording");
            UpdateCameraCellRecordingStatusIfNeeded(false, true);
        }

        public void DidFinishVideoRecording(CaptureSession session, NSUrl videoUrl)
        {
            Console.WriteLine("did finish video recording");
            UpdateCameraCellRecordingStatusIfNeeded(false, true);
        }

        public void DidInterruptVideoRecording(CaptureSession session, NSUrl videoUrl, NSError reason)
        {
            Console.WriteLine($"did interruCameraCollectionViewCellDelegatept video recording, reason: {reason}");
            UpdateCameraCellRecordingStatusIfNeeded(false, true);
        }

        public void DidFailVideoRecording(CaptureSession session, NSError error)
        {
            Console.WriteLine("did fail video recording");
            UpdateCameraCellRecordingStatusIfNeeded(false, true);
        }

        private void UpdateCameraCellRecordingStatusIfNeeded(bool isRecording, bool animated)
        {
            var cameraCell = GetCameraCell(LayoutConfiguration);
            cameraCell?.UpdateRecordingVideoStatus(isRecording, animated);
        }

        public void TakePicture()
        {
            _captureSession?.CapturePhoto(LivePhotoMode.Off, CaptureSettings.SavesCapturedPhotosToPhotoLibrary);
        }

        public void TakeLivePhoto()
        {
            _captureSession?.CapturePhoto(LivePhotoMode.On, CaptureSettings.SavesCapturedLivePhotosToPhotoLibrary);
        }

        public void StartVideoRecording()
        {
            _captureSession?.StartVideoRecording(saveToPhotoLibrary: CaptureSettings.SavesCapturedVideosToPhotoLibrary);
        }

        public void StopVideoRecording()
        {
            _captureSession?.StopVideoRecording(false);
        }

        public void FlipCamera(Action completion)
        {
            if (_captureSession == null)
            {
                return;
            }

            var cameraCell = GetCameraCell(LayoutConfiguration);
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
                            null, () =>
                            {
                                cameraCell.UnblurIfNeeded(true, completion);
                            });
                    });
                }
            });
        }
    }
}