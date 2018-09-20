using Photos;
using UIKit;

namespace YSImagePicker.Operations
{
    public class CollectionViewBatchAnimation
    {
        private UICollectionView CollectionView;
        private int SectionIndex;
        private PHFetchResultChangeDetails Changes;

        public CollectionViewBatchAnimation(UICollectionView collectionView,int sectionIndex,PHFetchResultChangeDetails changes)
        {

            CollectionView = collectionView;
            SectionIndex = sectionIndex;
            Changes = changes;
        }
    
        void Execute() {
            // If we have incremental diffs, animate them in the collection view
            CollectionView.PerformBatchUpdates(()=>{
                
            
                // For indexes to make sense, updates must be in this order:
                // delete, insert, reload, move
                if (Changes.RemovedIndexes!=null && Changes.RemovedIndexes.Count >0)
                {
                    CollectionView.DeleteItems(Changes.RemovedIndexes.map({ IndexPath(item: $0, section: self.sectionIndex) }))
                }
                if let inserted = changes.insertedIndexes, inserted.isEmpty == false {
                    self.collectionView.insertItems(at: inserted.map({ IndexPath(item: $0, section: self.sectionIndex) }))
                }
                if let changed = changes.changedIndexes, changed.isEmpty == false {
                    self.collectionView.reloadItems(at: changed.map({ IndexPath(item: $0, section: self.sectionIndex) }))
                }
                changes.enumerateMoves { fromIndex, toIndex in
                    self.collectionView.moveItem(at: IndexPath(item: fromIndex, section: self.sectionIndex), to: IndexPath(item: toIndex, section: self.sectionIndex))
                }
            }, completion: { finished in
                self.completeOperation()
            })
        }
    }
}