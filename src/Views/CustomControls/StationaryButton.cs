namespace Softeq.ImagePicker.Views.CustomControls;

///
/// A button that keeps selected state when selected.
///
[Register(nameof(StationaryButton))]
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
            if (!Highlighted)
            {
                SetSelected(!Selected);
            }
        }
    }

    protected StationaryButton(IntPtr intPtr) : base(intPtr)
    {
    }

    [Export("awakeFromNib")]
    public override void AwakeFromNib()
    {
        base.AwakeFromNib();
        UpdateTint();
    }

    protected virtual void SelectionDidChange(bool animated)
    {
        UpdateTint();
    }

    private void SetSelected(bool value)
    {
        if (Selected != value)
        {
            Selected = value;
            SelectionDidChange(true);
        }
    }

    private void UpdateTint()
    {
        TintColor = Selected ? SelectedTintColor : UnselectedTintColor;
    }
}