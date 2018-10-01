using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using YSImagePicker.Interfaces;
using YSImagePicker.Media;

namespace YSImagePicker.Public
{
    [Register("CameraCollectionViewCell")]
    public class CameraCollectionViewCell : UICollectionViewCell
    {
        private AVAuthorizationStatus? _authorizationStatus;
        public readonly AVPreviewView PreviewView = new AVPreviewView(CGRect.Empty) {BackgroundColor = UIColor.Black};

        private readonly UIImageView _imageView = new UIImageView(CGRect.Empty)
            {ContentMode = UIViewContentMode.ScaleAspectFill};

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

        ///
        /// The cell can have multiple visual states based on authorization status. Use
        /// `updateCameraAuthorizationStatus()` func to update UI.
        ///
        public AVAuthorizationStatus? AuthorizationStatus
        {
            get => _authorizationStatus;
            set
            {
                _authorizationStatus = value;
                UpdateCameraAuthorizationStatus();
            }
        }

        ///
        /// Called each time an authorization status to camera is changed. Update your
        /// cell's UI based on current value of `authorizationStatus` property.
        ///
        public void UpdateCameraAuthorizationStatus()
        {
        }

        ///
        /// If live photos are enabled this method is called each time user captures
        /// a live photo. Override this method to update UI based on live view status.
        ///
        /// - parameter isProcessing: If there is at least 1 live photo being processed/captured
        /// - parameter shouldAnimate: If the UI change should be animated or not.
        ///
        public virtual void UpdateLivePhotoStatus(bool isProcessing, bool shouldAnimate)
        {
        }

        ///
        /// If video recording is enabled this method is called each time user starts or stops
        /// a recording. Override this method to update UI based on recording status.
        ///
        /// - parameter isRecording: If video is recording or not
        /// - parameter shouldAnimate: If the UI change should be animated or not.
        ///
        public virtual void UpdateRecordingVideoStatus(bool isRecording, bool shouldAnimate)
        {
        }

        public virtual void VideoRecodingDidBecomeReady()
        {
        }

        ///
        /// Flips camera from front/rear or rear/front. Flip is always supplemented with
        /// an flip animation.
        ///
        /// - parameter completion: A block is called as soon as camera is changed.
        ///
        public void FlipCamera(Action completion = null)
        {
            Delegate?.FlipCamera(completion);
        }

        ///
        /// Takes a picture
        ///
        public void TakePicture()
        {
            Delegate?.TakePicture();
        }

        ///
        /// Takes a live photo. Please note that live photos must be enabled when configuring Image Picker.
        ///
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
}