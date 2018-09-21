using System;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;

namespace YSImagePicker.Views
{
    ///
    /// Each image picker asset cell must conform to this protocol.
    ///
    public abstract class ImagePickerAssetCell : UICollectionViewCell
    {
        public ImagePickerAssetCell(CGRect frame) : base(frame){}
        
        /// This image view will be used when setting an asset's image
        public abstract UIImageView ImageView { get; }

        /// This is a helper identifier that is used when properly displaying cells asynchronously
        public abstract string RepresentedAssetIdentifier { get; set; }
    }

    ///
    /// A default collection view cell that represents asset item. It supports:
    /// - shows image view of image thumbnail
    /// - icon and duration for videos
    /// - selected icon when isSelected is true
    ///
    public class VideoAssetCell : AssetCell
    {
        private readonly UILabel _durationLabel;
        private readonly UIImageView _iconView;
        private readonly UIImageView _gradientView;

        public VideoAssetCell(CGRect frame) : base(frame)
        {
            _durationLabel = new UILabel(CGRect.Empty);
            _gradientView = new UIImageView(CGRect.Empty);
            _iconView = new UIImageView(CGRect.Empty);

            _gradientView.Hidden = true;

            _iconView.TintColor = UIColor.White;
            _iconView.ContentMode = UIViewContentMode.Center;

            _durationLabel.TextColor = UIColor.White;

            _durationLabel.Font = UIFont.SystemFontOfSize(12, UIFontWeight.Semibold);
            _durationLabel.TextAlignment = UITextAlignment.Right;

            ContentView.AddSubview(_gradientView);
            ContentView.AddSubview(_durationLabel);
            ContentView.AddSubview(_iconView);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            var margin = 5;

            _gradientView.Frame = new CGRect(0, Bounds.Height - 40, Bounds.Width, 40);

            _durationLabel.Frame = new CGRect(ContentView.Bounds.Width - _durationLabel.Frame.Size.Width - margin,
                ContentView.Bounds.Height - _durationLabel.Frame.Size.Height - margin, 50, 20);

            _iconView.Frame = new CGRect(margin, ContentView.Bounds.Height - _iconView.Frame.Height - margin, 21, 21);
        }

        public void Update(PHAsset asset)
        {
            switch (asset.MediaType)
            {
                case PHAssetMediaType.Image:
                    if (asset.MediaSubtypes == PHAssetMediaSubtype.PhotoLive)
                    {
                        _gradientView.Hidden = false;
                        _gradientView.Image = UIImage.FromBundle("gradient");
                        _iconView.Hidden = false;
                        _durationLabel.Hidden = true;
                        _iconView.Image = UIImage.FromBundle("icon-badge-livephoto");
                    }
                    else
                    {
                        _gradientView.Hidden = true;
                        _iconView.Hidden = true;
                        _durationLabel.Hidden = true;
                    }

                    break;
                case PHAssetMediaType.Video:
                    _gradientView.Hidden = false;
                    _gradientView.Image =
                        UIImage.FromBundle("gradient")
                            .CreateResizableImage(UIEdgeInsets.Zero, UIImageResizingMode.Stretch);
                    _iconView.Hidden = false;
                    _durationLabel.Hidden = false;
                    _iconView.Image = UIImage.FromBundle("icon-badge-video");
                    _durationLabel.Text = DurationFormatter().StringFromTimeInterval(asset.Duration);
                    break;
            }
        }

        private NSDateComponentsFormatter DurationFormatter()
        {
            var formatter = new NSDateComponentsFormatter
            {
                UnitsStyle = NSDateComponentsFormatterUnitsStyle.Positional,
                AllowedUnits = NSCalendarUnit.Minute | NSCalendarUnit.Second,
                ZeroFormattingBehavior = NSDateComponentsFormatterZeroFormattingBehavior.Pad
            };
            return formatter;
        }
    }


    ///
    /// A default implementation of `ImagePickerAssetCell`. If user does not register
    /// a custom cell, Image Picker will use this one. Also contains
    /// default icon for selected state.
    ///
    public class AssetCell : ImagePickerAssetCell
    {
        private readonly CheckView _selectedImageView = new CheckView(CGRect.Empty);
        private bool _isSelected;

        public override UIImageView ImageView { get; } = new UIImageView(CGRect.Empty);

        public override string RepresentedAssetIdentifier { get; set; }

        public override bool Selected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                _selectedImageView.Hidden = !_isSelected;

                if (_selectedImageView.Hidden == false)
                {
                    _selectedImageView.Image = UIImage.FromBundle("icon-check-background");
                    _selectedImageView.ForegroundImage = UIImage.FromBundle("icon-check");
                }
            }
        }

        public AssetCell(CGRect frame) : base(frame)
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

            var margin = 5;

            _selectedImageView.Frame = new CGRect(margin, Bounds.Width - _selectedImageView.Frame.Width - margin,
                _selectedImageView.Frame.Width, _selectedImageView.Frame.Height);
        }
    }

    public class CheckView : UIImageView
    {
        private readonly UIImageView _foregroundView = new UIImageView(CGRect.Empty);

        public UIImage ForegroundImage
        {
            get => _foregroundView.Image;
            set => _foregroundView.Image = value;
        }

        public CheckView(CGRect frame) : base(frame)
        {
            AddSubview(_foregroundView);
            ContentMode = UIViewContentMode.Center;
            _foregroundView.ContentMode = UIViewContentMode.Center;
        }
    }
}