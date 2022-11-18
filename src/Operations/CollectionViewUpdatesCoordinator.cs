namespace Softeq.ImagePicker.Operations;

public class CollectionViewUpdatesCoordinator
{
    private readonly UICollectionView _сollectionView;

    private readonly NSOperationQueue _serialMainQueue;

    public CollectionViewUpdatesCoordinator(UICollectionView collectionView)
    {
        _serialMainQueue = new NSOperationQueue
        {
            MaxConcurrentOperationCount = 1,
            UnderlyingQueue = DispatchQueue.MainQueue
        };

        _сollectionView = collectionView;
    }

    /// <summary>
    /// Provides opportunity to update collectionView's dataSource in underlying queue.
    /// </summary>
    /// <param name="updates">Updates.</param>
    public void PerformDataSourceUpdate(Action updates)
    {
        _serialMainQueue.AddOperation(updates);
    }

    /// <summary>
    /// Updates collection view.
    /// </summary>
    /// <param name="changes">Changes.</param>
    /// <param name="inSection">In section.</param>
    public void PerformChanges(PHFetchResultChangeDetails changes, int inSection)
    {
        if (changes.HasIncrementalChanges)
        {
            var operation = new CollectionViewBatchAnimation(_сollectionView, inSection, changes);
            _serialMainQueue.AddOperation(operation.Execute);
        }
        else
        {
            _serialMainQueue.AddOperation(_сollectionView.ReloadData);
        }
    }
}