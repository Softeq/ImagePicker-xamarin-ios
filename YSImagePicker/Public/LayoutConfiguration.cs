using System;
using UIKit;

namespace YSImagePicker.Public
{
    public class LayoutConfiguration
    {
        public bool ShowsFirstActionItem = true;
        public bool ShowsSecondActionItem = true;

        public bool ShowsCameraItem = true;

        public bool ShowsAssetItems = true;

        ///
        /// Scroll and layout direction
        ///
        public UICollectionViewScrollDirection ScrollDirection = UICollectionViewScrollDirection.Horizontal;

        ///
        /// Defines how many image assets will be in a row. Must be > 0
        ///
        public int NumberOfAssetItemsInRow = 2;

        ///
        /// Spacing between items within a section
        ///
        public nfloat InteritemSpacing = 1;

        ///
        /// Spacing between actions section and camera section
        ///
        public nfloat ActionSectionSpacing = 1;

        ///
        /// Spacing between camera section and assets section
        ///
        public nfloat CameraSectionSpacing = 10;

        public bool HasAnyAction()
        {
            return ShowsFirstActionItem || ShowsSecondActionItem;
        }

        public int SectionIndexForActions = 0;

        public int SectionIndexForCamera = 1;

        public int SectionIndexForAssets = 2;

        public static LayoutConfiguration Default()
        {
            return new LayoutConfiguration();
        }
    }
}