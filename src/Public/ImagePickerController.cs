using Softeq.ImagePicker.Infrastructure.Extensions;
using Softeq.ImagePicker.Infrastructure.Interfaces;
using Softeq.ImagePicker.Media;
using Softeq.ImagePicker.Media.Capture;
using Softeq.ImagePicker.Operations;
using Softeq.ImagePicker.Public.Delegates;
using Softeq.ImagePicker.Views;

namespace Softeq.ImagePicker.Public;

public partial class ImagePickerController : UIViewController, IPHPhotoLibraryChangeObserver, IImagePickerDelegate
{
    private CollectionViewUpdatesCoordinator _collectionViewCoordinator;
    private CameraCollectionViewCellDelegate _cameraCollectionViewCellDelegate;

    private readonly ImagePickerDataSource _collectionViewDataSource = new(new ImagePickerAssetModel());

    private ImagePickerDelegate _collectionViewDelegate;
    private CaptureSession _captureSession;
    private UIView _overlayView;

    protected virtual UIInterfaceOrientation CurrentOrientation => UIApplication.SharedApplication.StatusBarOrientation;

    public void Release()
    {
        PHPhotoLibrary.SharedPhotoLibrary.UnregisterChangeObserver(this);
        _captureSession?.Suspend();
        Console.WriteLine($"DismissViewController: describing: {nameof(ImagePickerController)}");
    }

    public override void LoadView()
    {
        base.LoadView();
        var nib = UINib.FromName(nameof(ImagePickerView), NSBundle.MainBundle);
        View = nib.Instantiate(null, null)[0] as ImagePickerView;
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        _collectionViewDelegate = new ImagePickerDelegate(new ImagePickerLayout(LayoutConfiguration), this);

        ConfigureCollectionView();

        if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
        {
#pragma warning disable CA1416
            CollectionView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
#pragma warning restore CA1416
        }

        //gesture recognizer to detect taps on a camera cell (selection is disabled)
        View!.AddGestureRecognizer(new UITapGestureRecognizer(TapGestureRecognized)
        {
            CancelsTouchesInView = false
        });

        //connect all remaining objects as needed
        _collectionViewDataSource.CellRegistrator = CellRegistrator;

        //register for photo library updates - this is needed when changing permissions to photo library
        PHPhotoLibrary.SharedPhotoLibrary.RegisterChangeObserver(this);

        //determine auth status and based on that reload UI
#pragma warning disable CA1416
        if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
        {
            ReloadData(PHPhotoLibrary.GetAuthorizationStatus(PHAccessLevel.ReadWrite));
        }
        else
        {
            ReloadData(PHPhotoLibrary.AuthorizationStatus);
        }
#pragma warning restore CA1416

        ConfigureCaptureSession();
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

        _captureSession.UpdateVideoOrientation(GetCaptureVideoOrientation(CurrentOrientation));

        coordinator.AnimateAlongsideTransition(context => { UpdateContentInset(); },
            context => { UpdateItemSize(); });
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
            _collectionViewDataSource.UpdateLayoutModel(new LayoutModel(LayoutConfiguration,
                (int)_collectionViewDataSource.AssetsModel.FetchResult.Count));
        });

        //perform update animations
        _collectionViewCoordinator.PerformChanges(changes, LayoutConfiguration.SectionIndexForAssets);
    }

    public void DidSelectActionItemAt(int index)
    {
        Delegate?.DidSelectActionItemAt(this, index);
    }

    public void DidSelectAssetItemAt(int index)
    {
        Delegate?.DidSelectAsset(this, Asset(index));
    }

    public void DidDeselectAssetItemAt(int index)
    {
        Delegate?.DidDeselectAsset(this, Asset(index));
    }

    public void WillDisplayActionCell(UICollectionViewCell cell, int index)
    {
        if (cell is ActionCell defaultCell)
        {
            defaultCell.Update(index, LayoutConfiguration);
        }

        Delegate?.WillDisplayActionItem(this, cell, index);
    }

    public void WillDisplayCameraCell(CameraCollectionViewCell cell)
    {
        SetupCameraCellIfNeeded(cell);

        if (cell is LivePhotoCameraCell liveCameraCell)
        {
            liveCameraCell.UpdateWithCameraMode(CaptureSettings.CameraMode);
        }

        if (_captureSession.PhotoCaptureSession != null)
        {
            //update live photos
            cell.UpdateLivePhotoStatus(_captureSession.PhotoCaptureSession.InProgressLivePhotoCapturesCount > 0,
                false);
        }

        //update video recording status
        var isRecordingVideo = _captureSession.VideoCaptureSession?.IsRecordingVideo ?? false;
        cell.UpdateRecordingVideoStatus(isRecordingVideo, false);

        //update authorization status if it's changed
        var status = AVCaptureDevice.GetAuthorizationStatus(AVAuthorizationMediaType.Video);
        if (cell.AuthorizationStatus != status)
        {
            cell.AuthorizationStatus = status;
        }

        //resume session only if not recording video
        if (!isRecordingVideo)
        {
            _captureSession.Resume();
        }
    }

    public void DidEndDisplayingCameraCell(CameraCollectionViewCell cell)
    {
        var isRecordingVideo = _captureSession.VideoCaptureSession?.IsRecordingVideo ?? false;

        //suspend session only if not recording video, otherwise the recording would be stopped.
        if (isRecordingVideo == false)
        {
            _captureSession.Suspend();

            DispatchQueue.MainQueue.DispatchAsync(() => cell.BlurIfNeeded(false, null));
        }
    }

    public void WillDisplayAssetCell(ImagePickerAssetCell cell, int index)
    {
        var theAsset = Asset(index);

        //if the cell is default cell provided by Image Picker, it's our responsibility
        //to update the cell with the asset.
        var defaultCell = cell as VideoAssetCell;

        defaultCell?.Update(theAsset);

        Delegate?.WillDisplayAssetItem(this, cell, theAsset);
    }

    public void DidScroll(UIScrollView scrollView)
    {
    }

    private AVCaptureVideoOrientation GetCaptureVideoOrientation(UIInterfaceOrientation orientation)
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
            _cameraCollectionViewCellDelegate.TakePicture();
        }
    }

    private void SetupCameraCellIfNeeded(CameraCollectionViewCell cell)
    {
        if (cell.Delegate != null)
        {
            return;
        }

        cell.Delegate = _cameraCollectionViewCellDelegate;
        cell.PreviewView.Session = _captureSession.Session;
        _captureSession.PreviewLayer = cell.PreviewView.PreviewLayer;

        var config = _captureSession.PresetConfiguration;
        if (config == SessionPresetConfiguration.Videos)
        {
            cell.IsVisualEffectViewUsedForBlurring = true;
        }
    }

    private void ConfigureCollectionView()
    {
        ImagePickerView.BackgroundColor = Appearance.BackgroundColor;
        CollectionView.BackgroundColor = Appearance.BackgroundColor;

        //create animator
        _collectionViewCoordinator = new CollectionViewUpdatesCoordinator(CollectionView);

        //configure flow layout
        var collectionViewLayout = (UICollectionViewFlowLayout)CollectionView.CollectionViewLayout;
        collectionViewLayout.ScrollDirection = LayoutConfiguration.ScrollDirection;
        collectionViewLayout.MinimumInteritemSpacing = LayoutConfiguration.InterItemSpacing;
        collectionViewLayout.MinimumLineSpacing = LayoutConfiguration.InterItemSpacing;

        //finish configuring collection view
        CollectionView.DataSource = _collectionViewDataSource;
        CollectionView.Delegate = _collectionViewDelegate;
        CollectionView.AllowsMultipleSelection = true;
        CollectionView.ShowsVerticalScrollIndicator = false;
        CollectionView.ShowsHorizontalScrollIndicator = false;
        //apply cell registrator to collection view
        CollectionView.Apply(CellRegistrator, CaptureSettings.CameraMode);

        switch (LayoutConfiguration.ScrollDirection)
        {
            case UICollectionViewScrollDirection.Horizontal:
                CollectionView.AlwaysBounceHorizontal = true;
                break;
            case UICollectionViewScrollDirection.Vertical:
                CollectionView.AlwaysBounceVertical = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ConfigureCaptureSession()
    {
        if (!LayoutConfiguration.ShowsCameraItem)
        {
            return;
        }

        _captureSession = CaptureFactory.Create(GetCameraCell, Delegate, CaptureSettings.CameraMode);
        _captureSession.Prepare(GetCaptureVideoOrientation(CurrentOrientation));
        _cameraCollectionViewCellDelegate =
            new CameraCollectionViewCellDelegate(GetCameraCell, _captureSession, CaptureSettings);
    }

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
    }

    private void UpdateContentInset()
    {
        if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
        {
#pragma warning disable CA1416
            CollectionView.ContentInset =
                new UIEdgeInsets(CollectionView.ContentInset.Top, View!.SafeAreaInsets.Left,
                    CollectionView.ContentInset.Bottom, View.SafeAreaInsets.Right);
#pragma warning restore CA1416
        }
    }

    /// Reload collection view layout/data based on authorization status of photo library
    private void ReloadData(PHAuthorizationStatus status)
    {
        switch (status)
        {
            case PHAuthorizationStatus.Authorized:
                _collectionViewDataSource.AssetsModel.UpdateFetchResult(AssetsFetchResultBlock?.Invoke());
                _collectionViewDataSource.UpdateLayoutModel(new LayoutModel(LayoutConfiguration,
                    (int)_collectionViewDataSource.AssetsModel.FetchResult.Count));
                break;
            case PHAuthorizationStatus.Restricted:
            case PHAuthorizationStatus.Denied:
                var view = _overlayView ?? DataSource?.ImagePicker(status);
                if (view != null && !view.Superview.Equals(CollectionView))
                {
                    CollectionView.BackgroundView = view;
                    _overlayView = view;
                }

                break;
            case PHAuthorizationStatus.NotDetermined:
                PHPhotoLibrary.RequestAuthorization(authorizationStatus =>
                {
                    DispatchQueue.MainQueue.DispatchAsync(() => { ReloadData(authorizationStatus); });
                });
                break;
        }
    }
}