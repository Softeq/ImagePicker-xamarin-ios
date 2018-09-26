using System;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;

namespace YSImagePicker.Views
{
    ///
    /// A default collection view cell that represents asset item. It supports:
    /// - shows image view of image thumbnail
    /// - icon and duration for videos
    /// - selected icon when isSelected is true
    ///
    [Register("VideoAssetCell")]
    public class VideoAssetCell : AssetCell
    {
        private readonly UILabel _durationLabel;
        private readonly UIImageView _iconView;
        private readonly UIImageView _gradientView;

        public VideoAssetCell(IntPtr handle) : base(handle)
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

            var frame = _gradientView.Frame;
            frame.Size = new CGSize(Bounds.Width, 40);
            frame.Location = new CGPoint(0, Bounds.Height - 40);
            _gradientView.Frame = frame;

            var margin = 5;

            frame = _durationLabel.Frame;
            frame.Size = new CGSize(50, 20);
            frame.Location = new CGPoint(ContentView.Bounds.Width - frame.Size.Width - margin,
                ContentView.Bounds.Height - frame.Size.Height - margin);
            _durationLabel.Frame = frame;

            frame = _iconView.Frame;
            frame.Size = new CGSize(21, 21);
            frame.Location = new CGPoint(margin, ContentView.Bounds.Height - frame.Height - margin);
            _iconView.Frame = frame;
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
}