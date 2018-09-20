using System;
using Foundation;
using ObjCRuntime;
using Photos;
using UIKit;
namespace ImagePicker
{
    //// @interface Appearance : NSObject
    [BaseType(typeof(NSObject))]
    [Protocol] // Add this
    interface Appearance
    {
    }

    // @interface CameraCollectionViewCell : UICollectionViewCell
    [BaseType(typeof(UICollectionViewCell), Name = "_TtC11ImagePicker24CameraCollectionViewCell")]
    [Protocol] // Add this
    interface CameraCollectionViewCell
    {
        // -(void)layoutSubviews;
        [Export("layoutSubviews")]
        void LayoutSubviews();

        // -(void)flipCamera:(void (^ _Nullable)(void))completion;
        [Export("flipCamera:")]
        void FlipCamera([NullAllowed] Action completion);

        // -(void)takePicture;
        [Export("takePicture")]
        void TakePicture();

        // -(void)takeLivePhoto;
        [Export("takeLivePhoto")]
        void TakeLivePhoto();

        // -(void)startVideoRecording;
        [Export("startVideoRecording")]
        void StartVideoRecording();

        // -(void)stopVideoRecording;
        [Export("stopVideoRecording")]
        void StopVideoRecording();
    }

    // @interface CellRegistrator : NSObject
    [BaseType(typeof(NSObject))]
    [Protocol]
    interface CellRegistrator
    {
    }

    [Protocol, Model]
    [BaseType(typeof(NSObject))] // Add this
    interface ImagePickerAssetCell
    {
        [Export("imageView")]
        UIImageView Image { get; set; }

        [NullAllowed, Export("representedAssetIdentifier")]
        string RepresentedAssetIdentifier { get; set; }

        [Export("dispose")]
        void Dispose();
    }

    // @interface ImagePickerController : UIViewController
    [Protocol, Model]
    [BaseType(typeof(NSObject))] // Add this
    interface ImagePickerControllerDelegate
    {
        //func imagePicker(controller: ImagePickerController, didSelectActionItemAt index: Int)
        [Export("controller:index:")]
        void DidSelectActionItem(ImagePickerController imagePicker, int index);

        //func imagePicker(controller: ImagePickerController, didSelect asset: PHAsset)
        [Export("controller:asset:")]
        void DidSelectAsset(ImagePickerController imagePicker, PHAsset index);

        [Export("controller:didDeselect asset:")]
        void DidDeSelectAsset(ImagePickerController imagePicker, PHAsset index);

        [Export("controller:image:")]
        void DidTake(ImagePickerController imagePicker, UIImage image);

        [Export("controller:cell:image:")]
        void WillDisplayActionItem(ImagePickerController controller, UICollectionViewCell cell, int index);

        [Export("controller:cell:asset:")]
        void WillDisplayAssetItem(ImagePickerController controller, ImagePickerAssetCell cell, PHAsset asset);
    }

    // @interface ImagePickerController : UIViewController
    [BaseType(typeof(UIViewController), Name = "_TtC11ImagePicker21ImagePickerController")]
    [Protocol] // Add this
    interface ImagePickerController
    {
        [Wrap("WeakDelegate")]
        [NullAllowed]
        ImagePickerControllerDelegate Delegate { get; set; }

        [NullAllowed, Export("delegate", ArgumentSemantic.Weak)]
        NSObject WeakDelegate { get; set; }

        // -(void)loadView;
        [Export("loadView")]
        void LoadView();

        // -(void)viewDidLoad;
        [Export("viewDidLoad")]
        void ViewDidLoad();

        // -(void)viewWillAppear:(BOOL)animated;
        [Export("viewWillAppear:")]
        void ViewWillAppear(bool animated);

        // -(void)viewWillLayoutSubviews;
        [Export("viewWillLayoutSubviews")]
        void ViewWillLayoutSubviews();
    }

    // @interface ImagePicker_Swift_251 (ImagePickerController) <PHPhotoLibraryChangeObserver>
    [Category]
    [BaseType(typeof(ImagePickerController))]
    interface ImagePickerController_ImagePicker_Swift_251
    {
    }
}