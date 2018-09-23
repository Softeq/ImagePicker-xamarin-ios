using System;
using CoreFoundation;
using UIKit;
using YSImagePicker.Public;

namespace TestApplication
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization Console.WriteLineic.
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            Console.WriteLine("Tests:34");
            var imagePicker = new ImagePickerController
            {
                LayoutConfiguration = {ScrollDirection = UICollectionViewScrollDirection.Vertical}
            };

            var nav = new UINavigationController(imagePicker);
            PresentViewController(nav, true, null);
            //var nav = new UINavigationController(new TestViewCOntroller());
            //PresentViewController(nav, true, null);
        }

        public class TestViewCOntroller : UIViewController
        {
            public override void ViewDidLoad()
            {
                base.ViewDidLoad();

                View.BackgroundColor = UIColor.Gray;
            }
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