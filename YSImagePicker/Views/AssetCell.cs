using System;
using CoreGraphics;
using Foundation;
using UIKit;
using YSImagePicker.Extensions;
using YSImagePicker.Views.CustomControls;

namespace YSImagePicker.Views
{
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
            const int margin = 5;

            base.LayoutSubviews();

            ImageView.Frame = Bounds;

            _selectedImageView.Frame =
                new CGRect(new CGPoint(Bounds.Width - _selectedImageView.Frame.Width - margin, margin),
                    _selectedImageView.Frame.Size);
        }

        private void UpdateState()
        {
            if (_selectedImageView.Hidden)
            {
                return;
            }

            _selectedImageView.Image = UIImageExtensions.FromBundle(BundleAssets.IconCheckBackground);
            _selectedImageView.ForegroundImage = UIImageExtensions.FromBundle(BundleAssets.IconCheck);
        }
    }
}