using System;
using AVFoundation;
using Foundation;
using Photos;
using UIKit;

namespace YSImagePicker.Media
{
    public class VideoCaptureDelegate : AVCaptureFileOutputRecordingDelegate
    {
        public IntPtr Handle { get; }

        // MARK: Public Methods

        /// set this to false if you dont wish to save video to photo library
        public bool SavesVideoToLibrary = true;

        /// true if user manually requested to cancel recording (stop without saving)
        public bool IsBeingCancelled = false;

        /// if system interrupts recording due to various reasons (empty space, phone call, background, ...)
        public bool RecordingWasInterrupted = false;

        /// non nil if failed or interrupted, nil if cancelled
        private NSError recordingError { get; set; }

        private nint? BackgroundRecordingID;
        private readonly Action DidStart;
        private Action<VideoCaptureDelegate> DidFinish;
        private Action<VideoCaptureDelegate, NSError> DidFail;

        public VideoCaptureDelegate(Action didStart, Action<VideoCaptureDelegate> didFinish,
            Action<VideoCaptureDelegate, NSError> didFail)
        {
            DidStart = didStart;
            DidFinish = didFinish;
            DidFail = didFail;

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
                BackgroundRecordingID = UIApplication.SharedApplication.BeginBackgroundTask(null);
            }
        }


        private void CleanUp(bool deleteFile, bool saveToAssets, NSUrl outputFileURL)
        {
            void DeleteFileIfNeeded()
            {
                if (deleteFile == false)
                {
                    return;
                }

                var path = outputFileURL.Path;

                if (!NSFileManager.DefaultManager.FileExists(path))
                {
                    try
                    {
                        NSFileManager.DefaultManager.Remove(path, out _);
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine($"capture session: could not remove recording at url: {outputFileURL}");
                        Console.WriteLine($"capture session: error: {error}");
                    }
                }
            }

            if (BackgroundRecordingID != null)
            {
                if (BackgroundRecordingID != UIApplication.BackgroundTaskInvalid)
                {
                    UIApplication.SharedApplication.EndBackgroundTask(BackgroundRecordingID.Value);
                }

                BackgroundRecordingID = UIApplication.BackgroundTaskInvalid;
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
                                creationRequest.AddResource(PHAssetResourceType.Video, outputFileURL,
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
            DidStart?.Invoke();
        }

        public override void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl,
            NSObject[] connections,
            NSError error)
        {
            if (error != null)
            {
                recordingError = error;

                Console.WriteLine($"capture session: movie recording failed error: {error}");

                //this can be true even if recording is stopped due to a reason (no disk space, ...) so the video can still be delivered.
                //TODO: Check case when true or always false
                var successfullyFinished =
                    (error.UserInfo[AVErrorKeys.RecordingSuccessfullyFinished] as NSNumber)?.BoolValue;

                if (successfullyFinished.HasValue && successfullyFinished == true)
                {
                    CleanUp(true, SavesVideoToLibrary, outputFileUrl);
                    DidFail.Invoke(this, error);
                }
                else
                {
                    CleanUp(true, false, outputFileUrl);
                    DidFail.Invoke(this, error);
                }
            }
            else if (IsBeingCancelled == true)
            {
                CleanUp(true, false, outputFileUrl);
                DidFinish.Invoke(this);
            }
            else
            {
                CleanUp(true, SavesVideoToLibrary, outputFileUrl);
                DidFinish.Invoke(this);
            }
        }
    }
}