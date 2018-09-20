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
	partial class ActionCell
	{
		[Outlet]
		UIKit.NSLayoutConstraint BottomOffset { get; set; }

		[Outlet]
		UIKit.UIImageView ImageView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint LeadingOffset { get; set; }

		[Outlet]
		UIKit.UILabel TitleLabel { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint TopOffset { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint TrailingOffset { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ImageView != null) {
				ImageView.Dispose ();
				ImageView = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (TopOffset != null) {
				TopOffset.Dispose ();
				TopOffset = null;
			}

			if (TrailingOffset != null) {
				TrailingOffset.Dispose ();
				TrailingOffset = null;
			}

			if (BottomOffset != null) {
				BottomOffset.Dispose ();
				BottomOffset = null;
			}

			if (LeadingOffset != null) {
				LeadingOffset.Dispose ();
				LeadingOffset = null;
			}
		}
	}
}
