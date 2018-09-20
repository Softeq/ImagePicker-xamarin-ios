using YSImagePicker.Public;

namespace YSImagePicker
{
    public class LayoutModel
    {
        private int[] sections = { 0, 0, 0 };

        public LayoutModel(LayoutConfiguration configuration, int assets)
        {
            var actionItems = configuration.ShowsFirstActionItem ? 1 : 0;
            actionItems += configuration.ShowsSecondActionItem ? 1 : 0;
            sections[configuration.SectionIndexForActions] = actionItems;
            sections[configuration.SectionIndexForCamera] = configuration.ShowsCameraItem ? 1 : 0;
            sections[configuration.SectionIndexForAssets] = assets;
        }

        public int NumberOfSections => sections.Length;

        public int NumberOfItems(int section)
        {
            return sections[section];
        }

        public static LayoutModel Empty()
        {
            var emptyConfiguration = new LayoutConfiguration
            {
                ShowsFirstActionItem = false,
                ShowsSecondActionItem = false,
                ShowsCameraItem = false,
                ScrollDirection = UIKit.UICollectionViewScrollDirection.Horizontal,
                NumberOfAssetItemsInRow = 0,
                InteritemSpacing = 0,
                ActionSectionSpacing = 0,
                CameraSectionSpacing = 0
            };

            return new LayoutModel(emptyConfiguration, 0);
        }
    }
}
