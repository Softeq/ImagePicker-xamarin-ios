using Softeq.ImagePicker.Public;

namespace Softeq.ImagePicker;

public class LayoutModel
{
    private readonly int[] _sections = { 0, 0, 0 };
    public int NumberOfSections => _sections.Length;

    public LayoutModel(LayoutConfiguration configuration, int assets = 0)
    {
        var actionItems = configuration.ShowsFirstActionItem ? 1 : 0;
        actionItems += configuration.ShowsSecondActionItem ? 1 : 0;
        _sections[configuration.SectionIndexForActions] = actionItems;
        _sections[configuration.SectionIndexForCamera] = configuration.ShowsCameraItem ? 1 : 0;
        _sections[configuration.SectionIndexForAssets] = assets;
    }

    public int NumberOfItems(int section)
    {
        return _sections[section];
    }
}