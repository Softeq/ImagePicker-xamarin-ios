// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System;
using Foundation;

namespace YSImagePicker.Views
{
    [Register ("LivePhotoCameraCell")]
    partial class LivePhotoCameraCell
    {
        [Outlet]
        YSImagePicker.Views.StationaryButton EnableLivePhotoButton { get; set; }

        [Outlet]
        YSImagePicker.Views.CarvedLabel LiveIndicator { get; set; }

        [Outlet]
        YSImagePicker.Views.ShutterButton SnapButton { get; set; }

        [Action ("FlipButtonTapped:")]
        partial void FlipButtonTapped (Foundation.NSObject sender);

        [Action ("SnapButtonTapped:")]
        partial void SnapButtonTapped (Foundation.NSObject sender);

        void ReleaseDesignerOutlets ()
        {
            if (EnableLivePhotoButton != null) {
                EnableLivePhotoButton.Dispose ();
                EnableLivePhotoButton = null;
            }

            if (LiveIndicator != null) {
                LiveIndicator.Dispose ();
                LiveIndicator = null;
            }

            if (SnapButton != null) {
                SnapButton.Dispose ();
                SnapButton = null;
            }
        }
    }
}