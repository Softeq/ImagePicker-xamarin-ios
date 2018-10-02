using System;
using AVFoundation;
using CoreMedia;
using Foundation;
using Photos;

namespace Softeq.ImagePicker.Media.Delegates
{
    public class PhotoCaptureDelegate : AVCapturePhotoCaptureDelegate
    {
        private readonly Action _willCapturePhotoAnimation;
        private readonly Action<bool> _capturingLivePhoto;

        private readonly Action<PhotoCaptureDelegate> _completed;
        private NSUrl _livePhotoCompanionMovieUrl;

        public bool ShouldSavePhotoToLibrary { get; set; }
        public NSData PhotoData { get; private set; }
        public AVCapturePhotoSettings RequestedPhotoSettings { get; }
        public NSError ProcessError { get; private set; }

        public PhotoCaptureDelegate(AVCapturePhotoSettings requestedPhotoSettings, Action willCapturePhotoAnimation,
            Action<bool> capturingLivePhoto, Action<PhotoCaptureDelegate> completed)
        {
            RequestedPhotoSettings = requestedPhotoSettings;
            _willCapturePhotoAnimation = willCapturePhotoAnimation;
            _capturingLivePhoto = capturingLivePhoto;
            _completed = completed;

            ShouldSavePhotoToLibrary = true;
        }

        public override void WillBeginCapture(AVCapturePhotoOutput captureOutput,
            AVCaptureResolvedPhotoSettings resolvedSettings)
        {
            if (resolvedSettings.LivePhotoMovieDimensions.Width > 0 &&
                resolvedSettings.LivePhotoMovieDimensions.Height > 0)
            {
                _capturingLivePhoto.Invoke(true);
            }
        }

        public override void WillCapturePhoto(AVCapturePhotoOutput captureOutput,
            AVCaptureResolvedPhotoSettings resolvedSettings)
        {
            _willCapturePhotoAnimation?.Invoke();
        }

        //this method is not called on iOS 11 if method above is implemented
        public override void DidFinishProcessingPhoto(AVCapturePhotoOutput captureOutput,
            CMSampleBuffer photoSampleBuffer,
            CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings,
            AVCaptureBracketedStillImageSettings bracketSettings, NSError error)
        {
            if (photoSampleBuffer != null)
            {
                PhotoData = AVCapturePhotoOutput.GetJpegPhotoDataRepresentation(photoSampleBuffer,
                    previewPhotoSampleBuffer);
            }
            else if (error != null)
            {
                Console.WriteLine($"photo capture delegate: error capturing photo: {error}");
                ProcessError = error;
            }
        }

        public override void DidFinishRecordingLivePhotoMovie(AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl,
            AVCaptureResolvedPhotoSettings resolvedSettings)
        {
            _capturingLivePhoto?.Invoke(false);
        }

        public override void DidFinishProcessingLivePhotoMovie(AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl,
            CMTime duration,
            CMTime photoDisplayTime, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
        {
            if (error != null)
            {
                Console.WriteLine($"photo capture delegate: error processing live photo companion movie: {error}");
                return;
            }

            _livePhotoCompanionMovieUrl = outputFileUrl;
        }

        public override void DidFinishCapture(AVCapturePhotoOutput captureOutput,
            AVCaptureResolvedPhotoSettings resolvedSettings,
            NSError error)
        {
            if (ShouldSaveCaptureResult(error))
            {
                PHAssetManager.PerformChangesWithAuthorization(TryToAddPhotoToLibrary, null, DidFinish);
            }
        }

        private bool ShouldSaveCaptureResult(NSError error)
        {
            if (error != null)
            {
                Console.WriteLine($"photo capture delegate: Error capturing photo: {error}");
                DidFinish();
                return false;
            }

            if (PhotoData == null)
            {
                Console.WriteLine("photo capture delegate: No photo data resource");
                DidFinish();
                return false;
            }

            if (!ShouldSavePhotoToLibrary)
            {
                Console.WriteLine("photo capture delegate: photo did finish without saving to photo library");
                DidFinish();
                return false;
            }

            return true;
        }

        private void TryToAddPhotoToLibrary()
        {
            var creationRequest = PHAssetCreationRequest.CreationRequestForAsset();
            creationRequest.AddResource(PHAssetResourceType.Photo, PhotoData, null);

            if (_livePhotoCompanionMovieUrl == null)
            {
                return;
            }

            var livePhotoCompanionMovieFileResourceOptions = new PHAssetResourceCreationOptions
            {
                ShouldMoveFile = true
            };

            creationRequest.AddResource(PHAssetResourceType.PairedVideo, _livePhotoCompanionMovieUrl,
                livePhotoCompanionMovieFileResourceOptions);
        }

        private void DidFinish()
        {
            if (_livePhotoCompanionMovieUrl?.Path != null &&
                NSFileManager.DefaultManager.FileExists(_livePhotoCompanionMovieUrl.Path))
            {
                NSFileManager.DefaultManager.Remove(_livePhotoCompanionMovieUrl, out var nsError);

                if (nsError != null)
                {
                    Console.WriteLine(
                        $"photo capture delegate: Could not remove file at url: ${_livePhotoCompanionMovieUrl}");
                }
            }

            _completed?.Invoke(this);
        }
    }
}