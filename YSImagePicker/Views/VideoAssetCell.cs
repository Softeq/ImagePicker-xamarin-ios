using System;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;
using YSImagePicker.Extensions;

namespace YSImagePicker.Views
{
    [Register("VideoAssetCell")]
    public sealed class VideoAssetCell : AssetCell
    {
        private readonly UILabel _durationLabel;
        private readonly UIImageView _iconView;
        private readonly UIImageView _gradientView;
        private readonly NSDateComponentsFormatter _durationFormatter;

        public VideoAssetCell(IntPtr handle) : base(handle)
        {
            _durationFormatter = GetDurationFormatter();

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

            _gradientView.Frame = new CGRect(new CGPoint(0, Bounds.Height - 40), new CGSize(Bounds.Width, 40));

            const int margin = 5;

            var frame = CGRect.Empty;
            frame.Size = new CGSize(50, 20);
            frame.Location = new CGPoint(ContentView.Bounds.Width - frame.Size.Width - margin,
                ContentView.Bounds.Height - frame.Size.Height - margin);
            _durationLabel.Frame = frame;

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
                        _gradientView.Image = UIImageExtensions.FromBundle(BundleAssets.Gradient);
                        _iconView.Hidden = false;
                        _durationLabel.Hidden = true;
                        _iconView.Image = UIImageExtensions.FromBundle(BundleAssets.IconBadgeLivePhoto);
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
                        UIImageExtensions.FromBundle(BundleAssets.Gradient)
                            .CreateResizableImage(UIEdgeInsets.Zero, UIImageResizingMode.Stretch);
                    _iconView.Hidden = false;
                    _durationLabel.Hidden = false;
                    _iconView.Image = UIImageExtensions.FromBundle(BundleAssets.IconBadgeVideo);
                    _durationLabel.Text = _durationFormatter.StringFromTimeInterval(asset.Duration);
                    break;
            }
        }

        private NSDateComponentsFormatter GetDurationFormatter()
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