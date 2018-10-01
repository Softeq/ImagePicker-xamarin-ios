// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//

using Foundation;

namespace Softeq.ImagePicker.Views
{
    [Register ("ActionCell")]
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
            if (BottomOffset != null) {
                BottomOffset.Dispose ();
                BottomOffset = null;
            }

            if (ImageView != null) {
                ImageView.Dispose ();
                ImageView = null;
            }

            if (LeadingOffset != null) {
                LeadingOffset.Dispose ();
                LeadingOffset = null;
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
        }
    }
}