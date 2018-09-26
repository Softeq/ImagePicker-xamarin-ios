using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace YSImagePicker.Views.CustomControls
{
    [Register("RecordButton")]
    public sealed class RecordButton : StationaryButton
    {
        private float _outerBorderWidth = 3f;
        private float _innerBorderWidth = 1.5f;
        private float _pressDepthFactor = 0.9f;
        private bool _needsUpdateCircleLayers = true;
        private readonly CALayer _outerCircleLayer;
        private readonly CALayer _innerCircleLayer;
        private LayersState _layersState;

        private float InnerCircleLayerInset => OuterBorderWidth + InnerBorderWidth;

        public float OuterBorderWidth
        {
            get => _outerBorderWidth;
            set
            {
                _outerBorderWidth = value;
                SetNeedsUpdateCircleLayers();
            }
        }

        public float InnerBorderWidth
        {
            get => _innerBorderWidth;
            set
            {
                _innerBorderWidth = value;
                SetNeedsUpdateCircleLayers();
            }
        }

        public float PressDepthFactor
        {
            get => _pressDepthFactor;
            set
            {
                _pressDepthFactor = value;
                SetNeedsUpdateCircleLayers();
            }
        }

        public override bool Highlighted
        {
            get => base.Highlighted;
            set
            {
                if (Selected == false && value != Highlighted && value == true)
                {
                    UpdateCircleLayers(LayersState.Pressed, true);
                }

                base.Highlighted = value;
            }
        }

        public RecordButton(IntPtr handler) : base(handler)
        {
            BackgroundColor = UIColor.Clear;

            _outerCircleLayer = new CALayer
            {
                BackgroundColor = UIColor.Clear.CGColor,
                CornerRadius = Bounds.Width / 2,
                BorderWidth = OuterBorderWidth,
                BorderColor = TintColor.CGColor,
            };
            
            _innerCircleLayer = new CALayer()
            {
                BackgroundColor = UIColor.Red.CGColor
            };

            Layer.AddSublayer(_outerCircleLayer);
            Layer.AddSublayer(_innerCircleLayer);
            
            CATransaction.DisableActions = true;
            CATransaction.Commit();
        }

        protected override void SelectionDidChange(bool animated)
        {
            base.SelectionDidChange(animated);
            
            UpdateCircleLayers(Selected ? LayersState.Recording : LayersState.Initial, animated);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (!_needsUpdateCircleLayers)
            {
                return;
            }
            
            CATransaction.DisableActions = true;
            _outerCircleLayer.Frame = Bounds;
            _innerCircleLayer.Frame = Bounds.Inset(InnerCircleLayerInset, InnerCircleLayerInset);
            _innerCircleLayer.CornerRadius = Bounds.Inset(InnerCircleLayerInset, InnerCircleLayerInset).Width / 2;
            _needsUpdateCircleLayers = false;
            CATransaction.Commit();
        }

        private void SetNeedsUpdateCircleLayers()
        {
            _needsUpdateCircleLayers = true;
            SetNeedsLayout();
        }

        private void UpdateCircleLayers(LayersState state, bool animated)
        {
            if (_layersState == state)
            {
                return;
            }

            _layersState = state;

            switch (_layersState)
            {
                case LayersState.Initial:
                    SetInnerLayer(false, animated);
                    break;
                case LayersState.Pressed:
                    SetInnerLayerPressed(animated);
                    break;
                case LayersState.Recording:
                    SetInnerLayer(true, animated);
                    break;
            }
        }

        private void SetInnerLayerPressed(bool animated)
        {
            if (animated)
            {
                _innerCircleLayer.AddAnimation(TransformAnimation(PressDepthFactor, 0.25), null);
            }
            else
            {
                CATransaction.DisableActions = true;
                _innerCircleLayer.Transform.Scale(PressDepthFactor);

                CATransaction.Commit();
            }
        }

        private void SetInnerLayer(bool recording, bool animated)
        {
            if (recording)
            {
                _innerCircleLayer.AddAnimation(TransformAnimation(0.5f, 0.15), null);
                _innerCircleLayer.CornerRadius = 8;
            }
            else
            {
                _innerCircleLayer.AddAnimation(TransformAnimation(1, 0.25), null);
                _innerCircleLayer.CornerRadius =
                    Bounds.Inset(InnerCircleLayerInset, InnerCircleLayerInset).Width / 2;
            }
        }

        private CAAnimation TransformAnimation(float value, double duration)
        {
            const string keyPath = "transform.scale";

            var animation = new CABasicAnimation
            {
                KeyPath = keyPath,
                From = _innerCircleLayer.PresentationLayer.ValueForKeyPath(new NSString(keyPath)),
                To = FromObject(value),
                Duration = duration,
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut),
                BeginTime = CAAnimation.CurrentMediaTime(),
                FillMode = CAFillMode.Forwards,
                RemovedOnCompletion = false
            };

            return animation;
        }
    }
}