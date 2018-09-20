// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace YSImagePicker.Views
{
	partial class VideoCameraCell
	{
		[Outlet]
		UIKit.UIButton FlipButton { get; set; }

		[Outlet]
		YSImagePicker.Views.RecordDurationLabel RecordDurationLabel { get; set; }

		[Outlet]
		YSImagePicker.Views.RecordButton RecordVideoButton { get; set; }

		[Action ("FlipButtonTapped:")]
		partial void FlipButtonTapped (Foundation.NSObject sender);

		[Action ("RecordButtonTapped:")]
		partial void RecordButtonTapped (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (RecordVideoButton != null) {
				RecordVideoButton.Dispose ();
				RecordVideoButton = null;
			}

			if (FlipButton != null) {
				FlipButton.Dispose ();
				FlipButton = null;
			}

			if (RecordDurationLabel != null) {
				RecordDurationLabel.Dispose ();
				RecordDurationLabel = null;
			}
		}
	}
}
