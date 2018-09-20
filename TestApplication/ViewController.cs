using System;
using ImagePicker;
using UIKit;

namespace TestApplication
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization Console.WriteLineic.
        }

        public override void ViewDidLoad()
        {

            base.ViewDidLoad();
            var imagePicker = new ImagePickerController();
            imagePicker.Delegate = new TestDelegate();

            var nav = new UINavigationController(new ImagePickerController());
            PresentViewController(nav, true, () => { });
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }
    }

    public class TestDelegate : ImagePickerControllerDelegate
    {

    }
}
