namespace Softeq.ImagePicker.Infrastructure;

public class ImagePickerException : Exception
{
    public ImagePickerException(string errorMessage) : base(errorMessage)
    {
    }
}