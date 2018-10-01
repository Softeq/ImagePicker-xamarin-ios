using CoreGraphics;
using UIKit;

namespace Softeq.ImagePicker.Views.CustomControls
{
    public sealed class CheckView : UIImageView
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

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            _foregroundView.Frame = Bounds;
        }
    }
}