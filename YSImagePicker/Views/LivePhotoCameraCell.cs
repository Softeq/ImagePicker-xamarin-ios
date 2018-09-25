using System;
using Foundation;
using UIKit;
using YSImagePicker.Public;

namespace YSImagePicker.Views
{
    public partial class LivePhotoCameraCell : CameraCollectionViewCell
    {
        public LivePhotoCameraCell(IntPtr handle) : base(handle)
        {
        }

        [Export("awakeFromNib")]
        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            LiveIndicator.Alpha = 0;
            LiveIndicator.TintColor = UIColor.FromRGBA(245 / 255f, 203 / 255f, 47 / 255f, 1);

            EnableLivePhotoButton.UnselectedTintColor = UIColor.White;
            EnableLivePhotoButton.SelectedTintColor =
                UIColor.FromRGBA(245 / 255f, 203 / 255f, 47 / 255f, 1);
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