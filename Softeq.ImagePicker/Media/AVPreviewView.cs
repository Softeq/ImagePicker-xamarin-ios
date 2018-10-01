using AVFoundation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Softeq.ImagePicker.Enums;
using UIKit;

namespace Softeq.ImagePicker.Media
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
                if (PreviewLayer.Session != null && PreviewLayer.Session.Equals(value))
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
        public static Class LayerClass()
        {
            return new Class(typeof(AVCaptureVideoPreviewLayer));
        }

        public AVPreviewView(CGRect frame) : base(frame)
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
}