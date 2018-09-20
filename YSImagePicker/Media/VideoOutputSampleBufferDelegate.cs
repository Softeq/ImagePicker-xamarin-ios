using AVFoundation;
using CoreFoundation;
using CoreImage;
using CoreMedia;
using Foundation;
using UIKit;

namespace YSImagePicker.Media
{
    public class VideoOutputSampleBufferDelegate : AVCaptureVideoDataOutputSampleBufferDelegate
    {
        public DispatchQueue ProcessQueue =
            new DispatchQueue(label: "eu.inloop.video-output-sample-buffer-delegate.queue");

        public UIImage LatestImage => ImageRepresentation;

        private CMSampleBuffer _latestSampleBuffer = null;

        private static CIContext context = new CIContext(new CIContextOptions()
        {
            UseSoftwareRenderer = false
        });

        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer,
            AVCaptureConnection connection)
        {
            _latestSampleBuffer = sampleBuffer;
        }

        ///
        /// Converts Sample Buffer to UIImage with backing CGImage. This conversion
        /// is expensive, use it lazily.
        ///
        private UIImage ImageRepresentation
        {
            get
            {
                var pixelBuffer = _latestSampleBuffer.GetImageBuffer();

                if (pixelBuffer == null)
                {
                    return null;
                }

                var ciImage = CIImage.FromImageBuffer(pixelBuffer);

                // downscale image
                var filter = CIFilter.FromName("CILanczosScaleTransform");
                filter.SetValueForKey(ciImage, new NSString("inputImage"));
                filter.SetValueForKey(FromObject(0.25), new NSString("inputScale"));
                filter.SetValueForKey(FromObject(1.0), new NSString("inputAspectRatio"));

                var resizedCiImage = filter.ValueForKey(new NSString("outputImage")) as CIImage;

                // TODO: consider using CIFilter also for bluring and saturating

                // we need to convert CIImage to CGImage because we are using Apples blurring
                // functions (see UIImage+ImageEffects.h) and it requires UIImage with
                // backed CGImage. This conversion is very expensive, use it only
                // when you really need it

                var cgImage = context.CreateCGImage(resizedCiImage, resizedCiImage.Extent);

                if (cgImage != null)
                {
                    return UIImage.FromImage(cgImage);
                }

                return null;
            }
        }
    }
}