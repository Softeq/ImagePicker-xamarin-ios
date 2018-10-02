using System;
using AVFoundation;
using Foundation;
using Softeq.ImagePicker.Infrastructure.Interfaces;
using Softeq.ImagePicker.Public;

namespace Softeq.ImagePicker.Media.Delegates
{
    public class CaptureSessionDelegate : ICaptureSessionDelegate
    {
        private readonly Func<CameraCollectionViewCell> _getCameraCellFunc;

        public CaptureSessionDelegate(Func<CameraCollectionViewCell> getCameraCellFunc)
        {
            _getCameraCellFunc = getCameraCellFunc;
        }

        public void CaptureSessionDidResume()
        {
            Console.WriteLine("did resume");
            UnblurCellIfNeeded(true);
        }

        public void CaptureSessionDidSuspend()
        {
            Console.WriteLine("did suspend");
            BlurCellIfNeeded(true);
        }

        public void DidFail(AVError error)
        {
            Console.WriteLine("did fail");
        }

        public void DidFailConfiguringSession()
        {
            Console.WriteLine("did fail configuring");
        }

        public void CaptureGrantedSession(AVAuthorizationStatus status)
        {
            Console.WriteLine("did grant authorization to camera");
            ReloadCameraCell(status);
        }

        public void CaptureFailedSession(AVAuthorizationStatus status)
        {
            Console.WriteLine("did fail authorization to camera");
            ReloadCameraCell(status);
        }

        public void WasInterrupted(NSString reason)
        {
            Console.WriteLine("interrupted");
        }

        public void CaptureSessionInterruptionDidEnd()
        {
            Console.WriteLine("interruption ended");
        }

        private void ReloadCameraCell(AVAuthorizationStatus status)
        {
            var cameraCell = _getCameraCellFunc.Invoke();

            if (cameraCell == null)
            {
                return;
            }

            cameraCell.AuthorizationStatus = status;
        }

        private void BlurCellIfNeeded(bool animated)
        {
            _getCameraCellFunc.Invoke()?.BlurIfNeeded(animated, null);
        }

        private void UnblurCellIfNeeded(bool animated)
        {
            _getCameraCellFunc.Invoke()?.UnblurIfNeeded(animated, null);
        }
    }
}