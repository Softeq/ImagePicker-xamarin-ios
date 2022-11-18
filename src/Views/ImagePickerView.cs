namespace Softeq.ImagePicker.Views;

public partial class ImagePickerView : UIView
{
    public UICollectionView UICollectionView => CollectionView;

    protected internal ImagePickerView(IntPtr handle) : base(handle)
    {
    }
}