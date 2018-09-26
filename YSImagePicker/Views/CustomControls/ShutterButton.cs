using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace YSImagePicker.Views.CustomControls
{
    [Register("ShutterButton")]
    public class ShutterButton : UIButton
    {
        private readonly nfloat _outerBorderWidth = 3;
        private readonly nfloat _innerBorderWidth = 1.5f;
        private readonly nfloat _pressDepthFactor = 0.9f;
        private bool _highlighted;

        public override bool Highlighted
        {
            get => _highlighted;
            set
            {
                _highlighted = value;
                SetInnerLayer(Highlighted, true);
            }
        }

        private nfloat InnerCircleLayerInset => _outerBorderWidth + _innerBorderWidth;

        private readonly CALayer _outerCircleLayer;
        private readonly CALayer _innerCircleLayer;

        public ShutterButton(IntPtr handle):base(handle)
        {
            _outerCircleLayer = new CALayer();
            _innerCircleLayer = new CALayer();
            BackgroundColor = UIColor.Clear;
            Layer.AddSublayer(_outerCircleLayer);
            Layer.AddSublayer(_innerCircleLayer);

            CATransaction.DisableActions = true;

            _outerCircleLayer.BackgroundColor = UIColor.Clear.CGColor;
            _outerCircleLayer.CornerRadius = Bounds.Width / 2;
            _outerCircleLayer.BorderWidth = _outerBorderWidth;
            _outerCircleLayer.BorderColor = TintColor.CGColor;

            _innerCircleLayer.BackgroundColor = TintColor.CGColor;

            CATransaction.Commit();
        }
        
        public ShutterButton(NSCoder aDecoder) : base(aDecoder)
        {
            _outerCircleLayer = new CALayer();
            _innerCircleLayer = new CALayer();
            BackgroundColor = UIColor.Clear;
            Layer.AddSublayer(_outerCircleLayer);
            Layer.AddSublayer(_innerCircleLayer);

            CATransaction.DisableActions = true;

            _outerCircleLayer.BackgroundColor = UIColor.Clear.CGColor;
            _outerCircleLayer.CornerRadius = Bounds.Width / 2;
            _outerCircleLayer.BorderWidth = _outerBorderWidth;
            _outerCircleLayer.BorderColor = TintColor.CGColor;

            _innerCircleLayer.BackgroundColor = TintColor.CGColor;

            CATransaction.Commit();
        }

        public void SetInnerLayer(bool tapped, bool animated)
        {
            if (animated)
            {
                var animation = new CABasicAnimation {KeyPath = "transform.scale"};

                if (tapped)
                {
                    animation.From =
                        _innerCircleLayer.PresentationLayer.ValueForKeyPath(new NSString("transform.scale"));
                    animation.To = FromObject(_pressDepthFactor);
                    animation.Duration = 0.25;
                }
                else
                {
                    animation.From = FromObject(_pressDepthFactor);
                    animation.To = FromObject(1.0);
                    animation.Duration = 0.25;
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
                if (tapped)
                {
                    _innerCircleLayer.SetValueForKeyPath(FromObject(_pressDepthFactor), new NSString("transform.scale"));
                }
                else
                {
                    _innerCircleLayer.SetValueForKeyPath(FromObject(1), new NSString("transform.scale"));
                }

                CATransaction.Commit();
            }
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
    }
}