using System;
using AVFoundation;
using Foundation;
using Photos;
using UIKit;

namespace YSImagePicker.Media
{
    public class VideoCaptureDelegate : AVCaptureFileOutputRecordingDelegate
    {
        private nint? _backgroundRecordingId;
        private readonly Action _didStart;
        private readonly Action<VideoCaptureDelegate> _didFinish;
        private readonly Action<VideoCaptureDelegate, NSError> _didFail;

        /// set this to false if you don't wish to save video to photo library
        public bool ShouldSaveVideoToLibrary = true;

        /// true if user manually requested to cancel recording (stop without saving)
        public bool IsBeingCancelled = false;

        /// if system interrupts recording due to various reasons (empty space, phone call, background, ...)
        public bool RecordingWasInterrupted = false;

        /// non null if failed or interrupted, null if cancelled
        public NSError RecordingError { get; set; }

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
                HandleCaptureResultWithError(error, outputFileUrl);
            }
            else if (IsBeingCancelled)
            {
                CleanUp(false, outputFileUrl);
                _didFinish.Invoke(this);
            }
            else
            {
                CleanUp(ShouldSaveVideoToLibrary, outputFileUrl);
                _didFinish.Invoke(this);
            }
        }

        private void SaveVideoToLibrary(NSUrl outputFileUrl)
        {
            var creationRequest = PHAssetCreationRequest.CreationRequestForAsset();
            var videoResourceOptions = new PHAssetResourceCreationOptions {ShouldMoveFile = true};
            creationRequest.AddResource(PHAssetResourceType.Video, outputFileUrl,
                videoResourceOptions);
        }

        private void HandleCaptureResultWithError(NSError error, NSUrl outputFileUrl)
        {
            RecordingError = error;

            Console.WriteLine($"capture session: movie recording failed error: {error}");

            //this can be true even if recording is stopped due to a reason (no disk space, ...) so the video can still be delivered.
            var successfullyFinished =
                (error.UserInfo[AVErrorKeys.RecordingSuccessfullyFinished] as NSNumber)?.BoolValue;

            if (successfullyFinished == true)
            {
                CleanUp(ShouldSaveVideoToLibrary, outputFileUrl);
                _didFail.Invoke(this, error);
            }
            else
            {
                CleanUp(false, outputFileUrl);
                _didFail.Invoke(this, error);
            }
        }

        private void CleanUp(bool saveToAssets, NSUrl outputFileUrl)
        {
            if (_backgroundRecordingId != null)
            {
                if (_backgroundRecordingId != UIApplication.BackgroundTaskInvalid)
                {
                    UIApplication.SharedApplication.EndBackgroundTask(_backgroundRecordingId.Value);
                }

                _backgroundRecordingId = UIApplication.BackgroundTaskInvalid;
            }

            if (!saveToAssets)
            {
                DeleteFileIfNeeded(outputFileUrl);
                return;
            }

            PHAssetManager.PerformChangesWithAuthorization(() => SaveVideoToLibrary(outputFileUrl),
                () => DeleteFileIfNeeded(outputFileUrl));
        }

        private void DeleteFileIfNeeded(NSUrl outputFileUrl)
        {
            if (NSFileManager.DefaultManager.FileExists(outputFileUrl.Path))
            {
                return;
            }

            NSFileManager.DefaultManager.Remove(outputFileUrl.Path, out var nsError);

            if (nsError != null)
            {
                Console.WriteLine($"capture session: could not remove recording at url: {outputFileUrl}");
                Console.WriteLine($"capture session: error: {nsError}");
            }
        }
    }
}