using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using UIKit;

namespace YSImagePicker.Views.CustomControls
{
    [Register("CarvedLabel"), DesignTimeVisible(true)]
    public sealed class CarvedLabel : UIView
    {
        private string _text;
        private UIFont _font;
        private nfloat _cornerRadius = 0f;
        private nfloat _verticalInset = 0f;
        private nfloat _horizontalInset = 0f;
        
        private NSAttributedString AttributedString => new NSAttributedString(Text ?? string.Empty,
            Font ?? UIFont.SystemFontOfSize(12, UIFontWeight.Regular));
        
        [Export("text"), Browsable(true)]
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                InvalidateIntrinsicContentSize();
                SetNeedsDisplay();
            }
        }

        [Export("font"), Browsable(true)]
        public UIFont Font
        {
            get => _font;
            set
            {
                _font = value;
                InvalidateIntrinsicContentSize();
                SetNeedsDisplay();
            }
        }

        [Export("cornerRadius"), Browsable(true)]
        public nfloat CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                SetNeedsDisplay();
            }
        }

        [Export("verticalInset"), Browsable(true)]
        public nfloat VerticalInset
        {
            get => _verticalInset;
            set
            {
                _verticalInset = value;
                InvalidateIntrinsicContentSize();
                SetNeedsDisplay();
            }
        }

        [Export("horizontalInset"), Browsable(true)]
        public nfloat HorizontalInset
        {
            get => _horizontalInset;
            set
            {
                _horizontalInset = value;
                InvalidateIntrinsicContentSize();
                SetNeedsDisplay();
            }
        }

        public override UIColor BackgroundColor => UIColor.Clear;

        public override CGSize IntrinsicContentSize => SizeThatFits(CGSize.Empty);

        public CarvedLabel(IntPtr handle) : base(handle)
        {
            //var _ = BackgroundColor;
            Opaque = false;
        }

        public override void Draw(CGRect rect)
        {
            var color = TintColor;
            color.SetFill();

            var path = UIBezierPath.FromRoundedRect(rect, CornerRadius);
            path.Fill();

            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            var context = UIGraphics.GetCurrentContext();

            var attributedString = AttributedString;
            var stringSize = attributedString.Size;

            var xOrigin = Math.Max(HorizontalInset, (rect.Width - stringSize.Width) / 2);
            var yOrigin = Math.Max(VerticalInset, (rect.Height - stringSize.Height) / 2);

            context.SaveState();
            context.SetBlendMode(CGBlendMode.DestinationOut);
            attributedString.DrawString(new CGPoint(xOrigin, yOrigin));
            context.RestoreState();
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            var stringSize = AttributedString.Size;
            return new CGSize(stringSize.Width + HorizontalInset * 2, stringSize.Height + VerticalInset * 2);
        }
    }
}