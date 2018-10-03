// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using Foundation;

namespace Softeq.ImagePicker.Sample.CustomViews
{
    partial class IconWithTextCell
	{
		[Outlet]
		UIKit.NSLayoutConstraint BottomOffset { get; set; }

		[Outlet]
		UIKit.UIImageView InternalImageView { get; set; }

		[Outlet] public UIKit.UILabel TitleLabel { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint TopOffset { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (InternalImageView != null) {
				InternalImageView.Dispose ();
				InternalImageView = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (TopOffset != null) {
				TopOffset.Dispose ();
				TopOffset = null;
			}

			if (BottomOffset != null) {
				BottomOffset.Dispose ();
				BottomOffset = null;
			}
		}
	}
}
