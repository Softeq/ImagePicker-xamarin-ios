using Softeq.ImagePicker.Public;

namespace Softeq.ImagePicker.Views;

public partial class VideoCameraCell : CameraCollectionViewCell
{
    public VideoCameraCell(IntPtr handle) : base(handle)
    {
    }

    [Export("awakeFromNib")]
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

        Action updates = () => FlipButton.Alpha = isRecording ? 0 : 1;

        if (shouldAnimate)
        {
            Animate(0.25, updates);
        }
        else
        {
            updates.Invoke();
        }
    }

    public override void VideoRecodingDidBecomeReady()
    {
        RecordVideoButton.Enabled = true;
        Animate(0.25, () =>
        {
            RecordVideoButton.Alpha = 1;
        });
    }
}