using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace YSImagePicker.Views.CustomControls
{
    [Register("RecordDurationLabel")]
    public class RecordDurationLabel : UILabel
    {
        private double _backingSeconds = 10000;

        private double BackingSeconds
        {
            get => _backingSeconds;
            set
            {
                _backingSeconds = value;
                UpdateLabel();
            }
        }

        public Lazy<CALayer> IndicatorLayer = new Lazy<CALayer>(() =>
          {
              var layer = new CALayer()
              {
                  MasksToBounds = true,
                  BackgroundColor = UIColor.FromRGBA(234 / 255f, 53 / 255f, 52 / 255f, 1).CGColor,
              };

              var layerFrame = layer.Frame;
              layerFrame.Size = new CGSize(6, 6);
              layer.Frame = layerFrame;
              layer.CornerRadius = layer.Frame.Width / 2;
              layer.Opacity = 0;

              return layer;
          });

        public NSTimer SecondTimer;
        public NSTimer IndicatorTimer;

        public RecordDurationLabel(IntPtr handle) : base(handle)
        {
            CommonInit();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            IndicatorLayer.Value.Position = new CGPoint(-7, Bounds.Height / 2);
        }

        public void Start()
        {
            if (SecondTimer != null)
            {
                return;
            }

            SecondTimer = NSTimer.CreateScheduledTimer(1, true, nsTimer => BackingSeconds++);
            SecondTimer.Tolerance += 1;

            IndicatorTimer = NSTimer.CreateScheduledTimer(1, true, nsTimer => { UpdateIndicator(0.2); });
            IndicatorTimer.Tolerance = 0.1;

            UpdateIndicator(1);
        }

        public void Stop()
        {
            SecondTimer?.Invalidate();
            SecondTimer = null;
            BackingSeconds = 0;
            UpdateLabel();

            IndicatorTimer?.Invalidate();
            IndicatorTimer = null;

            IndicatorLayer.Value.RemoveAllAnimations();
            IndicatorLayer.Value.Opacity = 0;
        }

        private void UpdateLabel()
        {
            //we are not using DateComponentsFormatter because it does not pad zero to hours component
            //so it regurns pattern 0:00:00, we need 00:00:00
            var hours = BackingSeconds / 3600;
            var minutes = BackingSeconds / 60 % 60;
            var seconds = BackingSeconds % 60;

            Text = $"{hours:00}:{minutes:00}:{seconds:00}";
        }

        private void UpdateIndicator(double appearDelay = 0)
        {
            var disappearDelay = 0.25;

            var appear = AppearAnimation(appearDelay);
            var disappear = DisappearAnimation(appear.BeginTime + appear.Duration + disappearDelay);

            var animation = new CAAnimationGroup
            {
                Animations = new[] { appear, disappear },
                Duration = appear.Duration + disappear.Duration + appearDelay + disappearDelay,
                RemovedOnCompletion = true
            };

            IndicatorLayer.Value.AddAnimation(animation, "blinkAnimationKey");
        }

        private void CommonInit()
        {
            Layer.AddSublayer(IndicatorLayer.Value);
            ClipsToBounds = false;
        }

        private CAAnimation AppearAnimation(double delay = 0)
        {
            var appear = new CABasicAnimation
            {
                KeyPath = "opacity",
                From = FromObject(IndicatorLayer.Value.PresentationLayer.Opacity),
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
                KeyPath = "opacity",
                From = FromObject(IndicatorLayer.Value.PresentationLayer?.Opacity),
                To = FromObject(0),
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseIn),
                BeginTime = delay,
                Duration = 0.25
            };

            return disappear;
        }
    }
}