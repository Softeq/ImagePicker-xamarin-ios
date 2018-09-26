using System;
using Foundation;
using UIKit;

namespace YSImagePicker.Views.CustomControls
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

        public StationaryButton(IntPtr intPtr) : base(intPtr)
        {

        }

        [Export("awakeFromNib")]
        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            UpdateTint();
        }

        public virtual void SelectionDidChange(bool animated)
        {
            UpdateTint();
        }

        private void SetSelected(bool value, bool animated)
        {
            if (Selected != value)
            {
                Selected = value;
                SelectionDidChange(animated);
            }
        }

        private void UpdateTint()
        {
            TintColor = Selected ? SelectedTintColor : UnselectedTintColor;
        }
    }
}