using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace YSImagePicker.Views.CustomControls
{
    [Register("ShutterButton")]
    public sealed class ShutterButton : UIButton
    {
        private readonly nfloat _outerBorderWidth = 3;
        private readonly nfloat _innerBorderWidth = 1.5f;
        private readonly nfloat _pressDepthFactor = 0.9f;
        private readonly CALayer _outerCircleLayer;
        private readonly CALayer _innerCircleLayer;
        private nfloat InnerCircleLayerInset => _outerBorderWidth + _innerBorderWidth;
        
        private const string PressAnimationKeyPath = "transform.scale";
        
        public override bool Highlighted
        {
            get => base.Highlighted;
            set
            {
                base.Highlighted = value;
                SetInnerLayer(value, true);
            }
        }

        public ShutterButton(IntPtr handle) : base(handle)
        {
            BackgroundColor = UIColor.Clear;

            _outerCircleLayer = new CALayer
            {
                BackgroundColor = UIColor.Clear.CGColor,
                CornerRadius = Bounds.Width / 2,
                BorderWidth = _outerBorderWidth,
                BorderColor = TintColor.CGColor
            };
            _innerCircleLayer = new CALayer
            {
                BackgroundColor = TintColor.CGColor
            };

            Layer.AddSublayer(_outerCircleLayer);
            Layer.AddSublayer(_innerCircleLayer);

            CATransaction.DisableActions = true;
            CATransaction.Commit();
        }
        
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            CATransaction.DisableActions = true;
            _outerCircleLayer.Frame = Bounds;
            
            _innerCircleLayer.Frame = Bounds.Inset(InnerCircleLayerInset, InnerCircleLayerInset);
            _innerCircleLayer.CornerRadius =
                Bounds.Inset(InnerCircleLayerInset, InnerCircleLayerInset).Width / 2;
            
            CATransaction.Commit();
        }

        private void SetInnerLayer(bool tapped, bool animated)
        {
            if (animated)
            {
                var animation = new CABasicAnimation
                {
                    KeyPath = PressAnimationKeyPath,
                    Duration = 0.25
                };

                if (tapped)
                {
                    animation.From =
                        _innerCircleLayer.PresentationLayer.ValueForKeyPath(new NSString(PressAnimationKeyPath));
                    animation.To = FromObject(_pressDepthFactor);
                }
                else
                {
                    animation.From = FromObject(_pressDepthFactor);
                    animation.To = FromObject(1.0);
                }

                animation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);
                animation.BeginTime = CAAnimation.CurrentMediaTime();
                animation.FillMode = CAFillMode.Forwards;
                animation.RemovedOnCompletion = false;

                _innerCircleLayer.AddAnimation(animation, null);
            }
            else
            {
                CATransaction.DisableActions = true;
                _innerCircleLayer.SetValueForKeyPath(tapped ? FromObject(_pressDepthFactor) : FromObject(1),
                    new NSString(PressAnimationKeyPath));

                CATransaction.Commit();
            }
        }
    }
}