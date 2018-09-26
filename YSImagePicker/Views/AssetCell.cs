using System;
using CoreGraphics;
using Foundation;
using UIKit;
using YSImagePicker.Views.CustomControls;

namespace YSImagePicker.Views
{
    ///
    /// A default implementation of `ImagePickerAssetCell`. If user does not register
    /// a custom cell, Image Picker will use this one. Also contains
    /// default icon for selected state.
    ///
    [Register("AssetCell")]
    public class AssetCell : ImagePickerAssetCell
    {
        private readonly CheckView _selectedImageView = new CheckView(CGRect.Empty);

        public sealed override UIImageView ImageView { get; } = new UIImageView(CGRect.Empty);

        public override string RepresentedAssetIdentifier { get; set; }

        public override bool Selected
        {
            get => base.Selected;
            set
            {
                base.Selected = value;
                _selectedImageView.Hidden = !base.Selected;

                UpdateState();
            }
        }

        protected AssetCell(IntPtr handle) : base(handle)
        {
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            ImageView.ClipsToBounds = true;
            ContentView.AddSubview(ImageView);

            _selectedImageView.Frame = new CGRect(0, 0, 31, 31);

            ContentView.AddSubview(_selectedImageView);
            _selectedImageView.Hidden = true;
        }

        public override void PrepareForReuse()
        {
            base.PrepareForReuse();

            ImageView.Image = null;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            ImageView.Frame = Bounds;

            const int margin = 5;

            var frame = _selectedImageView.Frame;
            frame.Location = new CGPoint(Bounds.Width - _selectedImageView.Frame.Width - margin, margin);
            _selectedImageView.Frame = frame;
        }

        private void UpdateState()
        {
            if (_selectedImageView.Hidden)
            {
                return;
            }

            _selectedImageView.Image = UIImage.FromBundle("icon-check-background");
            _selectedImageView.ForegroundImage = UIImage.FromBundle("icon-check");
        }
    }
}