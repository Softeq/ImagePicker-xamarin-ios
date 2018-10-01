using System;
using Photos;

namespace Softeq.ImagePicker.Media
{
    public static class PHAssetManager
    {
        public static void PerformChangesWithAuthorization(Action authorizedAction, Action errorAction)
        {
            PHPhotoLibrary.RequestAuthorization(status =>
            {
                if (status == PHAuthorizationStatus.Authorized)
                {
                    PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(authorizedAction, (_, error) =>
                    {
                        if (error != null)
                        {
                            Console.WriteLine(
                                $"capture session: Error occured while saving video or photo library: {error}");
                            errorAction?.Invoke();
                        }
                    });
                }
                else
                {
                    errorAction?.Invoke();
                }
            });
        }
    }
}