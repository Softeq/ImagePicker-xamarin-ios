using System;
using CoreGraphics;
using Foundation;
using UIKit;
using YSImagePicker.Public;

namespace YSImagePicker.Views
{
    [Register("LivePhotoCameraCell")]
    public partial class LivePhotoCameraCell : CameraCollectionViewCell
    {
        public LivePhotoCameraCell(CGRect frame) : base(frame)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            LiveIndicator.Alpha = 0;
            LiveIndicator.TintColor = UIColor.FromRGBA(245 / 255, 203 / 255, 47 / 255, 1);

            EnableLivePhotoButton.UnselectedTintColor = UIColor.White;
            EnableLivePhotoButton.SelectedTintColor =
                UIColor.FromRGBA(245 / 255, 203 / 255, 47 / 255, 1);
        }

        partial void SnapButtonTapped(NSObject sender)
        {
            if (EnableLivePhotoButton.Selected)
            {
                TakeLivePhoto();
            }
            else
            {
                TakePicture();
            }
        }

        partial void FlipButtonTapped(NSObject sender)
        {
            FlipCamera();
        }

        public void UpdateWithCameraMode(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.Photo:
                    LiveIndicator.Hidden = true;
                    EnableLivePhotoButton.Hidden = true;
                    break;
                case CameraMode.PhotoAndLivePhoto:
                    LiveIndicator.Hidden = false;
                    EnableLivePhotoButton.Hidden = false;
                    break;
                default:
                    throw new ArgumentException($"Not supported {mode}");
            }
        }

        public override void UpdateLivePhotoStatus(bool isProcessing, bool shouldAnimate)
        {
            Action updates = () => { LiveIndicator.Alpha = isProcessing ? 1 : 0; };

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