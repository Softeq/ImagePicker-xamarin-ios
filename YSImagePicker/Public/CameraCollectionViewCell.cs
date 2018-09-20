using System;
using System.Runtime.CompilerServices;
using AVFoundation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using YSImagePicker.Media;
using YSImagePicker.Public;

namespace YSImagePicker.Public
{
    public interface CameraCollectionViewCellDelegate
    {
        void TakePicture();
        void TakeLivePhoto();
        void StartVideoRecording();
        void StopVideoRecording();
        void FlipCamera(Action action);
    }
    
    public class CameraCollectionViewCell : UICollectionViewCell
    {
        private AVAuthorizationStatus? _authorizationStatus;
        public AVPreviewView PreviewView => new AVPreviewView(CGRect.Empty) {BackgroundColor = UIColor.Black};
        public UIImageView ImageView => new UIImageView(CGRect.Empty) {ContentMode = UIViewContentMode.ScaleAspectFill};
        public UIVisualEffectView BlurView { get; set; }
        public bool IsVisualEffectViewUsedForBlurring { get; set; }
        public CameraCollectionViewCellDelegate Delegate { get; set; }

        public CameraCollectionViewCell(CGRect frame) : base(frame)
        {
            BackgroundView = PreviewView;
            PreviewView.AddSubview(ImageView);
        }

        public CameraCollectionViewCell(NSCoder aDecoder) : base(aDecoder)
        {
            BackgroundView = PreviewView;
            PreviewView.AddSubview(ImageView);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            ImageView.Frame = PreviewView.Bounds;
            if (BlurView != null)
            {
                BlurView.Frame = PreviewView.Bounds;
            }
        }

        ///
        /// The cell can have multiple visual states based on autorization status. Use
        /// `updateCameraAuthorizationStatus()` func to udate UI.
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
        public virtual void UpdateCameraAuthorizationStatus()
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

        public void BlurIfNeeded(UIImage blurImage, bool animated, Action completion)
        {
            UIView view;

            if (IsVisualEffectViewUsedForBlurring == false)
            {
                if (ImageView.Image != null)
                {
                    return;
                }

                ImageView.Image = blurImage;
                view = ImageView;
            }
            else
            {
                if (BlurView == null)
                {
                    BlurView = new UIVisualEffectView(UIBlurEffect.FromStyle(UIBlurEffectStyle.Light));
                    PreviewView.AddSubview(BlurView);
                }

                view = BlurView;
                view.Frame = PreviewView.Bounds;
            }

            view.Alpha = 0;
            if (animated == false)
            {
                view.Alpha = 1;
                completion?.Invoke();
            }
            else
            {
                Animate(0.1, 0, UIViewAnimationOptions.AllowAnimatedContent, () => { view.Alpha = 1; },
                    completion);
            }
        }

        public void UnblurIfNeeded(UIImage unblurImage, bool animated, Action completion)
        {
            Action animationBlock = null;
            Action animationCompletionBlock = null;

            if (IsVisualEffectViewUsedForBlurring == false)
            {
                if (ImageView.Image == null)
                {
                    return;
                }

                if (unblurImage != null)
                {
                    ImageView.Image = unblurImage;
                }

                animationBlock = () => ImageView.Alpha = 0;

                animationCompletionBlock = () =>
                {
                    ImageView.Image = null;
                    completion?.Invoke();
                };
            }
            else
            {
                animationBlock = () =>
                {
                    if (BlurView != null)
                    {
                        BlurView.Alpha = 0;
                    }
                };

                animationCompletionBlock = () => completion?.Invoke();
            }

            if (animated == false)
            {
                animationBlock();
                animationCompletionBlock();
            }
            else
            {
                Animate(0.1, 0, UIViewAnimationOptions.AllowAnimatedContent, animationBlock,
                    animationCompletionBlock);
            }
        }

        ///
        /// When user taps a camera cell this method is called and the result is
        /// used when determining whether the tap should take a photo or not. This
        /// is used when user taps on a button so the button is triggered not the touch.
        ///
        public bool TouchIsCaptureEffective(CGPoint point)
        {
            return Bounds.Contains(point) && HitTest(point, null).Equals(ContentView);
        }
    }
}