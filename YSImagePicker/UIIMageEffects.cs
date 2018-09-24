using System.Drawing;
using CoreImage;
using UIKit;

namespace YSImagePicker
{
    public static class UIIMageEffects
    {
        public static UIImage Blur(this UIImage image, float blurRadius = 6f)
        {
            if (image != null)
            {
                // Create a new blurred image.
                var imageToBlur = new CIImage(image);
                var blur = new CIGaussianBlur {Image = imageToBlur, Radius = blurRadius};

                var blurImage = blur.OutputImage;
                var context = CIContext.FromOptions(new CIContextOptions {UseSoftwareRenderer = false});
                var cgImage = context.CreateCGImage(blurImage,
                    new RectangleF(new PointF(0, 0), new SizeF((float) image.Size.Width, (float) image.Size.Height)));
                var newImage = UIImage.FromImage(cgImage);

                // Clean up
                imageToBlur.Dispose();
                context.Dispose();
                blur.Dispose();
                blurImage.Dispose();
                cgImage.Dispose();

                return newImage;
            }

            return null;
        }
    }
}