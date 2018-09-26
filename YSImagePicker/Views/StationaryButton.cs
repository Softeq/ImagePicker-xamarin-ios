using System;
using Foundation;
using UIKit;

namespace YSImagePicker.Views
{
    ///
    /// A button that keeps selected state when selected.
    ///
    [Register("StationaryButton")]
    public class StationaryButton : UIButton
    {
        public UIColor UnselectedTintColor;
        public UIColor SelectedTintColor;

        public override bool Highlighted
        {
            get => base.Highlighted;
            set
            {
                base.Highlighted = value;
                if (Highlighted == false)
                {
                    SetSelected(!Selected, true);
                }
            }
        }

        private void SetSelected(bool value, bool animated)
        {
            if (Selected == value)
            {
                return;
            }

            Selected = value;
            SelectionDidChange(animated);
        }

        public StationaryButton(IntPtr intPtr) : base(intPtr)
        {

        }

        [Export("awakeFromNib")]
        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            UpdateTint();
        }

        private void UpdateTint()
        {
            TintColor = Selected ? SelectedTintColor : UnselectedTintColor;
        }

        public virtual void SelectionDidChange(bool animated)
        {
            UpdateTint();
        }
    }
}