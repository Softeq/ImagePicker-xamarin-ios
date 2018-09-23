using YSImagePicker.Public;

namespace YSImagePicker
{
    public class ImagePickerSelectionPolicy
    {
        ///
        /// Helper class that determines which cells are selected, multiple selected or
        /// highlighted.
        ///
        /// We allow selecting only asset items, action items are only highlighted,
        /// camera item is untouched.
        ///
        public bool ShouldSelectItem(int section, LayoutConfiguration layoutConfiguration)
        {
            switch (section)
            {
                case 0:
                case 1:
                    return false;
                default:
                    return true;
            }
        }

        public bool ShouldHighlightItem(int section,LayoutConfiguration layoutConfiguration)
        {
            switch (section)
            {
                case 1:
                    return false;
                default:
                    return true;
            }
        }
    }
}