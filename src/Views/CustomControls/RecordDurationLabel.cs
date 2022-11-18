using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Softeq.ImagePicker.Views.CustomControls
{
    [Register("RecordDurationLabel")]
    public sealed class RecordDurationLabel : UILabel
    {
        private double _backingSeconds;
        private NSTimer _secondTimer;
        private NSTimer _indicatorTimer;
        private const string AppearDisappearKeyPathString = "opacity";

        private readonly Lazy<CALayer> _indicatorLayer = new Lazy<CALayer>(() =>
        {
            var layer = new CALayer()
            {
                MasksToBounds = true,
                BackgroundColor = Defines.Colors.OrangeColor.CGColor,
            };

            var layerFrame = layer.Frame;
            layerFrame.Size = new CGSize(6, 6);
            layer.Frame = layerFrame;
            layer.CornerRadius = layer.Frame.Width / 2;
            layer.Opacity = 0;

            return layer;
        });

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            _indicatorLayer.Value.Position = new CGPoint(-7, Bounds.Height / 2);
        }

        public void Start()
        {
            if (_secondTimer != null)
            {
                return;
            }

            _secondTimer = NSTimer.CreateScheduledTimer(1, true, _ =>
            {
                ++_backingSeconds;
                UpdateLabel();
            });
            _secondTimer.Tolerance += 1;

            _indicatorTimer = NSTimer.CreateScheduledTimer(1, true, nsTimer => { UpdateIndicator(0.2); });
            _indicatorTimer.Tolerance = 0.1;

            UpdateIndicator(1);
        }

        public void Stop()
        {
            _secondTimer?.Invalidate();
            _secondTimer = null;
            _backingSeconds = 0;
            UpdateLabel();

            _indicatorTimer?.Invalidate();
            _indicatorTimer = null;

            _indicatorLayer.Value.RemoveAllAnimations();
            _indicatorLayer.Value.Opacity = 0;
        }
        
        private RecordDurationLabel(IntPtr handle) : base(handle)
        {
            Layer.AddSublayer(_indicatorLayer.Value);
            ClipsToBounds = false;
        }

        private void UpdateLabel()
        {
            Text =
                $"{_backingSeconds / Defines.Common.SecondsInHour:00}:" +
                $"{_backingSeconds / Defines.Common.SecondsInMinute % Defines.Common.SecondsInMinute:00}:" +
                $"{_backingSeconds % Defines.Common.SecondsInMinute:00}";
        }

        private void UpdateIndicator(double appearDelay = 0)
        {
            const double disappearDelay = 0.25;
            const string animationKey = "blinkAnimationKey";

            var appear = AppearAnimation(appearDelay);
            var disappear = DisappearAnimation(appear.BeginTime + appear.Duration + disappearDelay);

            var animation = new CAAnimationGroup
            {
                Animations = new[] {appear, disappear},
                Duration = appear.Duration + disappear.Duration + appearDelay + disappearDelay,
                RemovedOnCompletion = true
            };

            _indicatorLayer.Value.AddAnimation(animation, animationKey);
        }

        private CAAnimation AppearAnimation(double delay = 0)
        {
            var appear = new CABasicAnimation
            {
                KeyPath = AppearDisappearKeyPathString,
                From = FromObject(_indicatorLayer.Value.PresentationLayer.Opacity),
                To = FromObject(1),
                Duration = 0.15,
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut),
                BeginTime = delay,
                FillMode = CAFillMode.Forwards
            };

            return appear;
        }

        private CAAnimation DisappearAnimation(double delay = 0)
        {
            var disappear = new CABasicAnimation
            {
                KeyPath = AppearDisappearKeyPathString,
                From = FromObject(_indicatorLayer.Value.PresentationLayer?.Opacity),
                To = FromObject(0),
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseIn),
                BeginTime = delay,
                Duration = 0.25
            };

            return disappear;
        }
    }
}