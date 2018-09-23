using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;

namespace YSImagePicker
{
    public static class Miscellaneous
    {
        public static (IEnumerable<CGRect> added, IEnumerable<CGRect>remove) DifferencesBetweenRects(CGRect old,
            CGRect newValue,
            UICollectionViewScrollDirection scrollDirection)
        {
            switch (scrollDirection)
            {
                case UICollectionViewScrollDirection.Horizontal:
                    return DifferencesBetweenRectsHorizontal(old, newValue);
                case UICollectionViewScrollDirection.Vertical:
                    return DifferencesBetweenRectsVertical(old, newValue);
            }

            throw new Exception("Out of enum");
        }

        public static (IEnumerable<CGRect> added, IEnumerable<CGRect>remove) DifferencesBetweenRectsVertical(CGRect old,
            CGRect newValue)
        {
            if (old.IntersectsWith(newValue))
            {
                var added = new List<CGRect>();
                if (newValue.GetMaxY() > old.GetMaxY())
                {
                    added.Add(new CGRect(newValue.X, old.GetMaxY(), newValue.Width,
                        newValue.GetMaxY() - old.GetMaxY()));
                }

                if (old.GetMinY() > newValue.GetMinY())
                {
                    added.Add(new CGRect(newValue.X, newValue.GetMinY(), newValue.Width,
                        old.GetMinY() - newValue.GetMinY()));
                }

                var removed = new List<CGRect>();
                if (newValue.GetMaxY() < old.GetMaxY())
                {
                    removed.Add(new CGRect(newValue.X, newValue.GetMaxY(), newValue.Width,
                        old.GetMaxY() - newValue.GetMaxY()));
                }

                if (old.GetMinY() < newValue.GetMinY())
                {
                    removed.Add(new CGRect(newValue.X, old.GetMinY(), newValue.Width,
                        newValue.GetMinY() - old.GetMinY()));
                }

                return (added, removed);
            }
            else
            {
                return (Enumerable.Empty<CGRect>(), Enumerable.Empty<CGRect>());
            }
        }

        public static (IEnumerable<CGRect> added, IEnumerable<CGRect>remove) DifferencesBetweenRectsHorizontal(CGRect old,
            CGRect newValue)
        {
            if (old.IntersectsWith(newValue))
            {
                var added = new List<CGRect>();
                if (newValue.GetMaxX() > old.GetMaxX())
                {
                    added.Add(new CGRect(old.GetMaxX(), old.Y, newValue.GetMaxX() - old.GetMaxX(), old.Height));
                }

                if (old.GetMinX() > newValue.GetMinX())
                {
                    added.Add(new CGRect(newValue.GetMinX(), old.Y, old.GetMaxX() - newValue.GetMaxX(), old.Height));
                }

                var removed = new List<CGRect>();
                if (newValue.GetMaxX() < old.GetMaxX())
                {
                    removed.Add(new CGRect(newValue.GetMaxX(), old.Y, old.GetMaxX() - newValue.GetMaxX(), old.Height));
                }

                if (old.GetMinX() < newValue.GetMinX())
                {
                    removed.Add(new CGRect(old.GetMinX(), old.Y, newValue.GetMaxX() - old.GetMaxX(),
                        old.Height));
                }

                return (added, removed);
            }

            return (Enumerable.Empty<CGRect>(), Enumerable.Empty<CGRect>());
        }
        
        public  static List<NSIndexPath> IndexPathsForElements(this UICollectionViewLayout collectionViewLayout, CGRect rect)
        {
            var allLayoutAttributes = collectionViewLayout.LayoutAttributesForElementsInRect(rect);
            return allLayoutAttributes.Select(x=>x.IndexPath).ToList();
        }
    }
}