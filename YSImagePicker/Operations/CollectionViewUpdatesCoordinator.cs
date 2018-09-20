using System;
using CoreFoundation;
using Foundation;
using Photos;
using UIKit;

namespace YSImagePicker.Operations
{
    public class CollectionViewUpdatesCoordinator
    {
        private UICollectionView СollectionView;

        private NSOperationQueue serialMainQueue;

        public CollectionViewUpdatesCoordinator(UICollectionView collectionView)
        {
            serialMainQueue = new NSOperationQueue
            {
                MaxConcurrentOperationCount = 1, UnderlyingQueue = DispatchQueue.MainQueue
            };

            СollectionView = collectionView;
        }

        /// Provides opportunuty to update collectionView's dataSource in underlaying queue.
        public void performDataSourceUpdate(Action updates)
        {
            serialMainQueue.AddOperation(updates);
        }

        /// Updates collection view.
        public void PerformChanges<PHAsset>(PHFetchResultChangeDetails changes,int inSection)
        {
            if (changes.HasIncrementalChanges) {
                var operation = CollectionViewBatchAnimation(collectionView: collectionView, sectionIndex: inSection,
                        changes: changes)

                serialMainQueue.addOperation(operation)
            }
            else {
                serialMainQueue.addOperation { [unowned self] in
                    self.collectionView.reloadData()
                }
            }
        }
    }
}