using System;
using CoreGraphics;
using Foundation;
using Softeq.ImagePicker.Infrastructure;
using Softeq.ImagePicker.Public;
using UIKit;

namespace Softeq.ImagePicker
{
    ///
    /// A helper class that contains all code and logic when doing layout of collection
    /// view cells. This is used solely by collection view's delegate. Typically 
    /// this code should be part of regular subclass of UICollectionViewLayout, however,
    /// since we are using UICollectionViewFlowLayout we have to do this workaround.
    ///
    public class ImagePickerLayout
    {
        public readonly LayoutConfiguration Configuration;

        public ImagePickerLayout(LayoutConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// Returns size for item considering number of rows and scroll direction, if preferredWidthOrHeight is nil, square size is returned
        public CGSize SizeForItem(int numberOfItemsInRow, nfloat? preferredWidthOrHeight,
            UICollectionView collectionView,
            UICollectionViewScrollDirection scrollDirection)
        {
            switch (scrollDirection)
            {
                case UICollectionViewScrollDirection.Horizontal:
                    var itemHeight = collectionView.Frame.Height;
                    itemHeight -= collectionView.ContentInset.Top + collectionView.ContentInset.Bottom;
                    itemHeight -= (numberOfItemsInRow - 1) * Configuration.InterItemSpacing;
                    itemHeight /= numberOfItemsInRow;
                    return new CGSize(preferredWidthOrHeight ?? itemHeight, itemHeight);

                case UICollectionViewScrollDirection.Vertical:
                    var itemWidth = collectionView.Frame.Width;
                    itemWidth -= collectionView.ContentInset.Left + collectionView.ContentInset.Right;
                    itemWidth -= (numberOfItemsInRow - 1) * Configuration.InterItemSpacing;
                    itemWidth /= numberOfItemsInRow;
                    return new CGSize(itemWidth, preferredWidthOrHeight ?? itemWidth);
                default:
                    throw new ArgumentException("Should be invoked only with UICollectionViewScrollDirection");
            }
        }

        public CGSize CollectionView(UICollectionView collectionView, UICollectionViewLayout collectionViewLayout,
            NSIndexPath indexPath)
        {
            if (!(collectionViewLayout is UICollectionViewFlowLayout layout))
            {
                throw new ImagePickerException("currently only UICollectionViewFlowLayout is supported");
            }

            var layoutModel = new LayoutModel(Configuration, 0);

            switch (indexPath.Section)
            {
                case 0:
                    //this will make sure that action item is either square if there are 2 items,
                    //or a recatangle if there is only 1 item
                    //let width = sizeForItem(numberOfItemsInRow: 2, preferredWidthOrHeight: nil, collectionView: collectionView, scrollDirection: layout.scrollDirection).width
                    nfloat ratio = 0.25f;
                    nfloat width = collectionView.Frame.Width * ratio;
                    return SizeForItem(layoutModel.NumberOfItems(Configuration.SectionIndexForActions),
                        width, collectionView, layout.ScrollDirection);

                case 1:
                    //lets keep this ratio so camera item is a nice rectangle

                    var traitCollection = collectionView.TraitCollection;

                    ratio = 160f / 212f;

                    switch (traitCollection.UserInterfaceIdiom)
                    {
                        case var _
                            when traitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Unspecified &&
                                 traitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact:
                        case var _ when traitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular &&
                                        traitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact:
                        case var _ when traitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Compact &&
                                        traitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact:
                            ratio = 1 / ratio;
                            break;
                    }

                    var widthOrHeight = collectionView.Frame.Height * ratio;
                    return SizeForItem(layoutModel.NumberOfItems(Configuration.SectionIndexForCamera),
                        widthOrHeight, collectionView, layout.ScrollDirection);
                case 2:
                    //make sure there is at least 1 item, othewise invalid layout
                    if (Configuration.NumberOfAssetItemsInRow < 0)
                    {
                        throw new ImagePickerException(
                            "invalid layout - numberOfAssetItemsInRow must be > 0, check your layout configuration ");
                    }

                    return SizeForItem(Configuration.NumberOfAssetItemsInRow, null, collectionView,
                        layout.ScrollDirection);
                default:
                    throw new ImagePickerException("unexpected sections count");
            }
        }

        public UIEdgeInsets CollectionView(UICollectionView collectionView, UICollectionViewLayout collectionViewLayout,
            int section)
        {
            if (!(collectionViewLayout is UICollectionViewFlowLayout layout))
            {
                throw new ImagePickerException("currently only UICollectionViewFlowLayout is supported");
            }

            /// helper method that creates edge insets considering scroll direction
            UIEdgeInsets sectionInsets(nfloat inset)
            {
                switch (layout.ScrollDirection)
                {
                    case UICollectionViewScrollDirection.Horizontal:
                        return new UIEdgeInsets(0, 0, 0, inset);
                    case UICollectionViewScrollDirection.Vertical:
                        return new UIEdgeInsets(0, 0, inset, 0);
                    default:
                        throw new ImagePickerException("unexpected enum");
                }
            }

            var layoutModel = new LayoutModel(Configuration, 0);

            switch (section)
            {
                case 0 when layoutModel.NumberOfItems(section) > 0:
                    return sectionInsets(Configuration.ActionSectionSpacing);
                case 1 when layoutModel.NumberOfItems(section) > 0:
                    return sectionInsets(Configuration.CameraSectionSpacing);
                default:
                    return UIEdgeInsets.Zero;
            }
        }
    }
}