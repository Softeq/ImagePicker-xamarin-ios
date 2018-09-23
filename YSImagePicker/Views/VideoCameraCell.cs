using System;
using CoreGraphics;
using Foundation;
using UIKit;
using YSImagePicker.Public;

namespace YSImagePicker.Views
{
    public partial class VideoCameraCell : CameraCollectionViewCell
    {
        public VideoCameraCell(IntPtr handle) : base(handle)
        {
        }
        
        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            RecordVideoButton.Enabled = false;
            RecordVideoButton.Alpha = 0;
        }

        partial void FlipButtonTapped(NSObject sender)
        {
            FlipCamera();
        }

        partial void RecordButtonTapped(NSObject sender)
        {
            if ((sender as UIButton)?.Selected == true)
            {
                StopVideoRecording();
            }
            else
            {
                StartVideoRecording();
            }
        }

        public override void UpdateRecordingVideoStatus(bool isRecording, bool shouldAnimate)
        {
            RecordVideoButton.Selected = isRecording;

            if (isRecording)
            {
                RecordDurationLabel.Start();
            }
            else
            {
                RecordDurationLabel.Stop();
            }

            Action updates = () => FlipButton.Alpha = isRecording ? 1 : 0;

            if (shouldAnimate)
            {
                Animate(0.25, updates);
            }
            else
            {
                updates.Invoke();
            }
        }
    }
}