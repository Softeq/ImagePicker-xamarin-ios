// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using YSImagePicker.Views.CustomControls;

namespace YSImagePicker.Views
{
    [Register ("VideoCameraCell")]
    partial class VideoCameraCell
    {
        [Outlet]
        UIKit.UIButton FlipButton { get; set; }


        [Outlet]
        RecordDurationLabel RecordDurationLabel { get; set; }


        [Outlet]
        RecordButton RecordVideoButton { get; set; }


        [Action ("FlipButtonTapped:")]
        partial void FlipButtonTapped (Foundation.NSObject sender);


        [Action ("RecordButtonTapped:")]
        partial void RecordButtonTapped (Foundation.NSObject sender);

        void ReleaseDesignerOutlets ()
        {
            if (FlipButton != null) {
                FlipButton.Dispose ();
                FlipButton = null;
            }

            if (RecordDurationLabel != null) {
                RecordDurationLabel.Dispose ();
                RecordDurationLabel = null;
            }

            if (RecordVideoButton != null) {
                RecordVideoButton.Dispose ();
                RecordVideoButton = null;
            }
        }
    }
}