using System;
using Foundation;
using Softeq.ImagePicker.Sample.Models.Enums;
using UIKit;

namespace Softeq.ImagePicker.Sample.Models;

public class CellItemModel
{
    public string Title { get; }
    public Action<NSIndexPath> Selector { get; }
    public Action<UITableViewCell> ConfigBlock { get; }
    public SelectorArgument SelectorArgument { get; set; }

    public CellItemModel(string title, Action<NSIndexPath> selector, Action<UITableViewCell> configBlock)
    {
        Title = title;
        Selector = selector;
        ConfigBlock = configBlock;
    }
}