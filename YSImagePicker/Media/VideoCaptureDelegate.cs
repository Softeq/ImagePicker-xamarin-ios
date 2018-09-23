using System;
using AVFoundation;
using Foundation;
using Photos;
using UIKit;

namespace YSImagePicker.Media
{
    public class VideoCaptureDelegate : AVCaptureFileOutputRecordingDelegate
    {
        // MARK: Public Methods

        /// set this to false if you don't wish to save video to photo library
        public bool SavesVideoToLibrary = true;

        /// true if user manually requested to cancel recording (stop without saving)
        public bool IsBeingCancelled = false;

        /// if system interrupts recording due to various reasons (empty space, phone call, background, ...)
        public bool RecordingWasInterrupted = false;

        /// non nil if failed or interrupted, nil if cancelled
        private NSError RecordingError { get; set; }

        private nint? _backgroundRecordingId;
        private readonly Action _didStart;
        private readonly Action<VideoCaptureDelegate> _didFinish;
        private readonly Action<VideoCaptureDelegate, NSError> _didFail;

        public VideoCaptureDelegate(Action didStart, Action<VideoCaptureDelegate> didFinish,
            Action<VideoCaptureDelegate, NSError> didFail)
        {
            _didStart = didStart;
            _didFinish = didFinish;
            _didFail = didFail;

            if (UIDevice.CurrentDevice.IsMultitaskingSupported)
            {
                /*
                 Setup background task.
                 This is needed because the `capture(_:, didFinishRecordingToOutputFileAt:, fromConnections:, error:)`
                 callback is not received until AVCam returns to the foreground unless you request background execution time.
                 This also ensures that there will be time to write the file to the photo library when AVCam is backgrounded.
                 To conclude this background execution, endBackgroundTask(_:) is called in
                 `capture(_:, didFinishRecordingToOutputFileAt:, fromConnections:, error:)` after the recorded file has been saved.
                 */
                _backgroundRecordingId = UIApplication.SharedApplication.BeginBackgroundTask(null);
            }
        }


        private void CleanUp(bool deleteFile, bool saveToAssets, NSUrl outputFileUrl)
        {
            void DeleteFileIfNeeded()
            {
                if (deleteFile == false)
                {
                    return;
                }

                var path = outputFileUrl.Path;

                if (!NSFileManager.DefaultManager.FileExists(path))
                {
                    try
                    {
                        NSFileManager.DefaultManager.Remove(path, out _);
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine($"capture session: could not remove recording at url: {outputFileUrl}");
                        Console.WriteLine($"capture session: error: {error}");
                    }
                }
            }

            if (_backgroundRecordingId != null)
            {
                if (_backgroundRecordingId != UIApplication.BackgroundTaskInvalid)
                {
                    UIApplication.SharedApplication.EndBackgroundTask(_backgroundRecordingId.Value);
                }

                _backgroundRecordingId = UIApplication.BackgroundTaskInvalid;
            }

            if (saveToAssets)
            {
                PHPhotoLibrary.RequestAuthorization(status =>
                {
                    {
                        if (status == PHAuthorizationStatus.Authorized)
                        {
                            PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(() =>
                            {
                                var creationRequest = PHAssetCreationRequest.CreationRequestForAsset();
                                var videoResourceOptions = new PHAssetResourceCreationOptions {ShouldMoveFile = true};
                                creationRequest.AddResource(PHAssetResourceType.Video, outputFileUrl,
                                    videoResourceOptions);
                            }, (handler, error) =>
                            {
                                if (error != null)
                                {
                                    Console.WriteLine(
                                        $"capture session: Error occured while saving video to photo library: {error}");
                                    DeleteFileIfNeeded();
                                }
                            });
                        }
                        else
                        {
                            DeleteFileIfNeeded();
                        }
                    }
                });
            }
            else
            {
                DeleteFileIfNeeded();
            }
        }

        public override void DidStartRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl,
            NSObject[] connections)
        {
            _didStart?.Invoke();
        }

        public override void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl,
            NSObject[] connections,
            NSError error)
        {
            if (error != null)
            {
                RecordingError = error;

                Console.WriteLine($"capture session: movie recording failed error: {error}");

                //this can be true even if recording is stopped due to a reason (no disk space, ...) so the video can still be delivered.
                //TODO: Check case when true or always false
                var successfullyFinished =
                    (error.UserInfo[AVErrorKeys.RecordingSuccessfullyFinished] as NSNumber)?.BoolValue;

                if (successfullyFinished.HasValue && successfullyFinished == true)
                {
                    CleanUp(true, SavesVideoToLibrary, outputFileUrl);
                    _didFail.Invoke(this, error);
                }
                else
                {
                    CleanUp(true, false, outputFileUrl);
                    _didFail.Invoke(this, error);
                }
            }
            else if (IsBeingCancelled == true)
            {
                CleanUp(true, false, outputFileUrl);
                _didFinish.Invoke(this);
            }
            else
            {
                CleanUp(true, SavesVideoToLibrary, outputFileUrl);
                _didFinish.Invoke(this);
            }
        }
    }
}