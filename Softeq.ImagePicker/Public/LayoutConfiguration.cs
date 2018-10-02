using System;
using UIKit;

namespace Softeq.ImagePicker.Public
{
    public struct LayoutConfiguration
    {
        public bool ShowsFirstActionItem;
        public bool ShowsSecondActionItem;
        public string FirstNameOfActionItem;
        public string SecondNameOfActionItem;
        public bool ShowsCameraItem;

        public bool ShowsAssetItems;

        ///
        /// Scroll and layout direction
        ///
        public UICollectionViewScrollDirection ScrollDirection;

        ///
        /// Defines how many image assets will be in a row. Must be > 0
        ///
        public int NumberOfAssetItemsInRow;

        ///
        /// Spacing between items within a section
        ///
        public nfloat InterItemSpacing;

        ///
        /// Spacing between actions section and camera section
        ///
        public nfloat ActionSectionSpacing;

        ///
        /// Spacing between camera section and assets section
        ///
        public nfloat CameraSectionSpacing;

        public bool HasAnyAction()
        {
            return ShowsFirstActionItem || ShowsSecondActionItem;
        }

        public int SectionIndexForActions;

        public int SectionIndexForCamera;

        public int SectionIndexForAssets;

        public LayoutConfiguration Default()
        {
            ShowsFirstActionItem = true;
            ShowsSecondActionItem = true;
            ShowsCameraItem = true;
            ShowsAssetItems = true;
            ScrollDirection = UICollectionViewScrollDirection.Horizontal;
            NumberOfAssetItemsInRow = 2;
            InterItemSpacing = 1;
            ActionSectionSpacing = 1;
            CameraSectionSpacing = 10;
            SectionIndexForActions = 0;
            SectionIndexForCamera = 1;
            SectionIndexForAssets = 2;
            FirstNameOfActionItem = "Camera";
            SecondNameOfActionItem = "Photos";
            return this;
        }
    }
}