namespace Softeq.ImagePicker.Views;

public abstract class ImagePickerAssetCell : UICollectionViewCell
{
    /// This image view will be used when setting an asset's image
    public abstract UIImageView ImageView { get; }

    /// This is a helper identifier that is used when properly displaying cells asynchronously
    public virtual string RepresentedAssetIdentifier { get; set; }

    protected ImagePickerAssetCell(IntPtr handle) : base(handle)
    {
    }
}