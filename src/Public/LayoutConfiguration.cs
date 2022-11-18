namespace Softeq.ImagePicker.Public;

public struct LayoutConfiguration
{
    public bool ShowsFirstActionItem;
    public bool ShowsSecondActionItem;
    public string FirstNameOfActionItem;
    public string SecondNameOfActionItem;
    public bool ShowsCameraItem;

    public bool ShowsAssetItems;

    /// <summary>
    /// Scroll and layout direction
    /// </summary>
    public UICollectionViewScrollDirection ScrollDirection;

    /// <summary>
    /// Defines how many image assets will be in a row. Must be > 0
    /// </summary>
    public int NumberOfAssetItemsInRow;

    /// <summary>
    /// Spacing between items within a section
    /// </summary>
    public nfloat InterItemSpacing;

    /// <summary>
    /// Spacing between actions section and camera section
    /// </summary>
    public nfloat ActionSectionSpacing;

    /// <summary>
    /// Spacing between camera section and assets section
    /// </summary>
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