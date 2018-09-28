using System;
using YSImagePicker.Media.Capture;
using YSImagePicker.Media.Delegates;
using YSImagePicker.Public;

namespace YSImagePicker.Media
{
    public static class CaptureFactory
    {
        public static CaptureSession Create(Func<CameraCollectionViewCell> getCameraCellFunc,
            ImagePickerControllerDelegate imagePickerDelegate, CameraMode mode)
        {
            var captureSessionDelegate = new CaptureSessionDelegate(getCameraCellFunc);

            CaptureSession session;
            switch (mode)
            {
                case CameraMode.Photo:
                case CameraMode.PhotoAndLivePhoto:
                    session = new CaptureSession(captureSessionDelegate,
                        new SessionPhotoCapturingDelegate(getCameraCellFunc, imagePickerDelegate));
                    break;
                case CameraMode.PhotoAndVideo:
                    session = new CaptureSession(captureSessionDelegate,
                        new CaptureSessionVideoRecordingDelegate(getCameraCellFunc));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            session.PresetConfiguration = mode.CaptureSessionPresetConfiguration();

            return session;
        }

        private static SessionPresetConfiguration CaptureSessionPresetConfiguration(this CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.Photo:
                    return SessionPresetConfiguration.Photos;
                case CameraMode.PhotoAndLivePhoto:
                    return SessionPresetConfiguration.LivePhotos;
                case CameraMode.PhotoAndVideo:
                    return SessionPresetConfiguration.Videos;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}