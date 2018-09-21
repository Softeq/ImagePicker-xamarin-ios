using Foundation;
using UIKit;
using YSImagePicker.Public;

namespace YSImagePicker.Views
{
    public partial class ActionCell : UICollectionViewCell
    {
        [Export("awakeFromNib")]
        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            ImageView.BackgroundColor = UIColor.Clear;
        }

        public void Update(int index, LayoutConfiguration layoutConfiguration)
        {
            var layoutModel = new LayoutModel(layoutConfiguration, 0);
            var actionCount = layoutModel.NumberOfItems(layoutConfiguration.SectionIndexForActions);

            TitleLabel.TextColor = UIColor.Black;

            switch (index)
            {
                case 0:
                    TitleLabel.Text = "Camera";
                    ImageView.Image = UIImage.FromBundle("button-camera");
                    break;
                case 1:
                    TitleLabel.Text = "Photos";
                    ImageView.Image = UIImage.FromBundle("button-photo-library");
                    break;
            }

            var isFirst = index == 0;
            var isLast = index == actionCount - 1;

            switch (layoutConfiguration.ScrollDirection)
            {
                case UICollectionViewScrollDirection.Horizontal:
                    TopOffset.Constant = isFirst ? 10 : 5;
                    BottomOffset.Constant = isLast ? 10 : 5;
                    LeadingOffset.Constant = 5;
                    TrailingOffset.Constant = 5;
                    break;
                case UICollectionViewScrollDirection.Vertical:
                    TopOffset.Constant = 5;
                    BottomOffset.Constant = 5;
                    LeadingOffset.Constant = isFirst ? 10 : 5;
                    TrailingOffset.Constant = isLast ? 10 : 5;
                    break;
            }
        }
    }
}
