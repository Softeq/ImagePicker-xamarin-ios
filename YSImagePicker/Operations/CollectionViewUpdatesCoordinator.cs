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
        public void PerformDataSourceUpdate(Action updates)
        {
            serialMainQueue.AddOperation(updates);
        }

        /// Updates collection view.
        public void PerformChanges(PHFetchResultChangeDetails changes,int inSection)
        {
            if (changes.HasIncrementalChanges)
            {
                var operation = new CollectionViewBatchAnimation(СollectionView, inSection, changes);

                serialMainQueue.AddOperation(() => operation.Execute());
            }
            else
            {
                serialMainQueue.AddOperation(() => { СollectionView.ReloadData(); });
            }
        }
    }
}