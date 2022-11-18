// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using Foundation;
using Softeq.ImagePicker.Views.CustomControls;

namespace Softeq.ImagePicker.Views
{
	[Register ("LivePhotoCameraCell")]
	partial class LivePhotoCameraCell
	{
		[Outlet]
		StationaryButton EnableLivePhotoButton { get; set; }

		[Outlet]
		CarvedLabel LiveIndicator { get; set; }

		[Outlet]
		ShutterButton SnapButton { get; set; }

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
