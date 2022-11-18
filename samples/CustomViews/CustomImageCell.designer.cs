// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using Foundation;

namespace Softeq.ImagePicker.Sample.CustomViews
{
	partial class CustomImageCell
	{
		[Outlet]
		UIKit.UIImageView InternalImageView { get; set; }

		[Outlet]
		UIKit.UIImageView SelectedImageView { get; set; }

		[Outlet]
		UIKit.UIImageView SubtypeImageView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (InternalImageView != null) {
				InternalImageView.Dispose ();
				InternalImageView = null;
			}

			if (SelectedImageView != null) {
				SelectedImageView.Dispose ();
				SelectedImageView = null;
			}

			if (SubtypeImageView != null) {
				SubtypeImageView.Dispose ();
				SubtypeImageView = null;
			}
		}
	}
}
