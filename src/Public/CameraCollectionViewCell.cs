using Softeq.ImagePicker.Infrastructure.Interfaces;
using Softeq.ImagePicker.Media;

namespace Softeq.ImagePicker.Public;

[Register(nameof(CameraCollectionViewCell))]
public class CameraCollectionViewCell : UICollectionViewCell
{
    private AVAuthorizationStatus? _authorizationStatus;
    public readonly AVPreviewView PreviewView = new AVPreviewView(CGRect.Empty) { BackgroundColor = UIColor.Black };

    private readonly UIImageView _imageView = new UIImageView(CGRect.Empty)
    { ContentMode = UIViewContentMode.ScaleAspectFill };

    private UIVisualEffectView BlurView { get; set; }
    public bool IsVisualEffectViewUsedForBlurring { get; set; }
    public ICameraCollectionViewCellDelegate Delegate { get; set; }

    public CameraCollectionViewCell(IntPtr handle) : base(handle)
    {
        BackgroundView = PreviewView;
        PreviewView.AddSubview(_imageView);
    }

    public override void LayoutSubviews()
    {
        base.LayoutSubviews();

        _imageView.Frame = PreviewView.Bounds;
        if (BlurView != null)
        {
            BlurView.Frame = PreviewView.Bounds;
        }
    }

    /// <summary>
    /// The cell can have multiple visual states based on authorization status. Use
    /// `updateCameraAuthorizationStatus()` func to update UI.
    /// </summary>
    /// <value>The authorization status.</value>
    public AVAuthorizationStatus? AuthorizationStatus
    {
        get => _authorizationStatus;
        set
        {
            _authorizationStatus = value;
            UpdateCameraAuthorizationStatus();
        }
    }

    /// <summary>
    /// Called each time an authorization status to camera is changed. Update your cell's UI based on current value of `authorizationStatus` property.
    /// </summary>
    public void UpdateCameraAuthorizationStatus()
    {
    }

    /// <summary>
    /// If live photos are enabled this method is called each time user captures
    /// a live photo. Override this method to update UI based on live view status.
    /// </summary>
    /// <param name="isProcessing">If there is at least 1 live photo being processed</param>
    /// <param name="shouldAnimate">If the UI change should be animated or not</param>
    public virtual void UpdateLivePhotoStatus(bool isProcessing, bool shouldAnimate)
    {
    }

    /// <summary>
    /// If video recording is enabled this method is called each time user starts or stops
    /// a recording. Override this method to update UI based on recording status.
    /// </summary>
    /// <param name="isRecording">If video is recording or not<c>true</c> is recording.</param>
    /// <param name="shouldAnimate">If the UI change should be animated or not.</param>
    public virtual void UpdateRecordingVideoStatus(bool isRecording, bool shouldAnimate)
    {
    }

    public virtual void VideoRecodingDidBecomeReady()
    {
    }

    /// <summary>
    /// Flips camera from front/rear or rear/front. Flip is always supplemented with an flip animation.
    /// </summary>
    /// <param name="completion">A block is called as soon as camera is changed.</param>
    public void FlipCamera(Action completion = null)
    {
        Delegate?.FlipCamera(completion);
    }

    public void TakePicture()
    {
        Delegate?.TakePicture();
    }

    /// <summary>
    /// Takes a live photo. Please note that live photos must be enabled when configuring Image Picker.
    /// </summary>
    public void TakeLivePhoto()
    {
        Delegate?.TakeLivePhoto();
    }

    public void StartVideoRecording()
    {
        Delegate?.StartVideoRecording();
    }

    public void StopVideoRecording()
    {
        Delegate?.StopVideoRecording();
    }

    public void BlurIfNeeded(bool animated, Action completion)
    {
        if (BlurView == null)
        {
            BlurView = new UIVisualEffectView(UIBlurEffect.FromStyle(UIBlurEffectStyle.Light));
            PreviewView.AddSubview(BlurView);
        }

        BlurView.Frame = PreviewView.Bounds;

        BlurView.Alpha = 0;
        if (animated == false)
        {
            BlurView.Alpha = 1;
            completion?.Invoke();
        }
        else
        {
            Animate(0.2, 0, UIViewAnimationOptions.AllowAnimatedContent, () => BlurView.Alpha = 1,
                completion);
        }
    }

    public void UnblurIfNeeded(bool animated, Action completion)
    {
        Action animationBlock = () =>
        {
            if (BlurView != null)
            {
                BlurView.Alpha = 0;
            }
        };

        if (animated == false)
        {
            animationBlock.Invoke();
            completion?.Invoke();
        }
        else
        {
            Animate(0.2, 0, UIViewAnimationOptions.AllowAnimatedContent, animationBlock, completion);
        }
    }

    public bool TouchIsCaptureEffective(CGPoint point)
    {
        return Bounds.Contains(point) && HitTest(point, null).Equals(ContentView);
    }
}