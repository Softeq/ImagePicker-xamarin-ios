using AVFoundation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace YSImagePicker.Media
{
    ///
    /// A view whose layer is AVCaptureVideoPreviewLayer so it's used for previewing
    /// output from a capture session.
    ///
    public class AVPreviewView : UIView
    {
        private VideoDisplayMode _displayMode = VideoDisplayMode.AspectFill;
        public AVCaptureVideoPreviewLayer PreviewLayer => Layer as AVCaptureVideoPreviewLayer;

        public AVCaptureSession Session
        {
            get => PreviewLayer.Session;
            set
            {
                if (PreviewLayer.Session.Equals(value))
                {
                    return;
                }

                PreviewLayer.Session = value;
            }
        }

        public VideoDisplayMode DisplayMode
        {
            get => _displayMode;
            set
            {
                _displayMode = value;
                ApplyVideoDisplayMode();
            }
        }

        [Export("layerClass")]
        static Class LayerClass()
        {
            return new Class(typeof(AVCaptureVideoPreviewLayer));
        }

        public AVPreviewView(CGRect frame) : base(frame)
        {
            ApplyVideoDisplayMode();
        }

        public AVPreviewView(NSCoder aDecoder) : base(aDecoder)
        {
            ApplyVideoDisplayMode();
        }

        private void ApplyVideoDisplayMode()
        {
            switch (DisplayMode)
            {
                case VideoDisplayMode.AspectFill:
                    PreviewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;
                    break;
                case VideoDisplayMode.AspectFit:
                    PreviewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspect;
                    break;
                case VideoDisplayMode.Resize:
                    PreviewLayer.VideoGravity = AVLayerVideoGravity.Resize;
                    break;
            }
        }
    }

    public enum VideoDisplayMode
    {
        /// Preserve aspect ratio, fit within layer bounds.
        AspectFit,

        /// Preserve aspect ratio, fill view bounds.
        AspectFill,

        ///Stretch to fill layer bounds
        Resize
    }
}