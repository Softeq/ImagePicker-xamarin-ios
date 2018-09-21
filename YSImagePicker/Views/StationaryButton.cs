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

        private bool _selected;
        private bool _highlighted;

        public override bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                SetSelected(value, false);
            }
        }

        public override bool Highlighted
        {
            get => _highlighted;
            set
            {
                _highlighted = value;
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

            SelectionDidChange(animated);
        }

        public StationaryButton(IntPtr intPtr):base(intPtr){

        }

        public StationaryButton(NSCoder aDecoder) : base(aDecoder)
        {
        }

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