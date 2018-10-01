using System;
using System.Collections.Generic;
using Foundation;
using Photos;
using UIKit;

namespace YSImagePicker.Operations
{
    public class CollectionViewBatchAnimation
    {
        private readonly UICollectionView _collectionView;
        private readonly int _sectionIndex;
        private readonly PHFetchResultChangeDetails _changes;

        public CollectionViewBatchAnimation(UICollectionView collectionView, int sectionIndex,
            PHFetchResultChangeDetails changes)
        {
            _collectionView = collectionView;
            _sectionIndex = sectionIndex;
            _changes = changes;
        }

        public void Execute()
        {
            // If we have incremental diffs, animate them in the collection view
            _collectionView.PerformBatchUpdates(() =>
            {
                // For indexes to make sense, updates must be in this order:
                // delete, insert, reload, move
                if (_changes.RemovedIndexes != null && _changes.RemovedIndexes.Count > 0)
                {
                    var result = new List<NSIndexPath>();
                    _changes.RemovedIndexes.EnumerateIndexes((nuint idx, ref bool stop) =>
                        result.Add(NSIndexPath.FromItemSection((nint) idx, _sectionIndex)));

                    _collectionView.DeleteItems(result.ToArray());
                }

                if (_changes.InsertedIndexes?.Count > 0)
                {
                    var result = new List<NSIndexPath>();

                    _changes.InsertedIndexes.EnumerateIndexes((nuint idx, ref bool stop) =>
                        result.Add(NSIndexPath.FromItemSection((nint) idx, _sectionIndex)));

                    _collectionView.InsertItems(result.ToArray());
                }

                if (_changes.ChangedIndexes?.Count > 0)
                {
                    var result = new List<NSIndexPath>();
                    _changes.ChangedIndexes.EnumerateIndexes((nuint idx, ref bool stop) =>
                        result.Add(NSIndexPath.FromItemSection((nint) idx, _sectionIndex)));

                    _collectionView.ReloadItems(result.ToArray());
                }

                _changes.EnumerateMoves((fromIndex, toIndex) =>
                {
                    _collectionView.MoveItem(NSIndexPath.FromItemSection((nint) fromIndex, _sectionIndex),
                        NSIndexPath.FromItemSection((nint) toIndex, _sectionIndex));
                });
            }, null);
        }
    }
}