using System;
using System.Linq;
using AVFoundation;
using Foundation;
using Softeq.ImagePicker.Infrastructure.Enums;

namespace Softeq.ImagePicker.Media.Capture
{
    public class VideoDeviceInputManager
    {
        private AVCaptureDeviceInput _videoDeviceInput;
        private NSObject _runtimeErrorNotification;

        private readonly AVCaptureDeviceDiscoverySession _videoDeviceDiscoverySession =
            AVCaptureDeviceDiscoverySession.Create(new[]
                {
                    AVCaptureDeviceType.BuiltInWideAngleCamera,
                    AVCaptureDeviceType.BuiltInDuoCamera
                },
                AVMediaType.Video, AVCaptureDevicePosition.Unspecified);

        private readonly Action<NSNotification> _sessionRuntimeErrorHandler;

        public VideoDeviceInputManager(Action<NSNotification> sessionRuntimeErrorHandler)
        {
            _sessionRuntimeErrorHandler = sessionRuntimeErrorHandler;
        }

        public SessionSetupResult ConfigureVideoDeviceInput(AVCaptureSession session)
        {
            var videoDevice = GetVideoDevice();
           
            if (videoDevice == null)
            {
                Console.WriteLine("capture session: could not create capture device");
                return SessionSetupResult.ConfigurationFailed;
            }

            var videoDeviceInput = new AVCaptureDeviceInput(videoDevice, out var error);

            if (error != null)
            {
                Console.WriteLine($"Error occured while creating video device input: {error}");
                return SessionSetupResult.ConfigurationFailed;
            }

            if (_videoDeviceInput != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_runtimeErrorNotification);
                session.RemoveInput(_videoDeviceInput);
            }

            if (session.CanAddInput(videoDeviceInput))
            {
                session.AddInput(videoDeviceInput);
                _videoDeviceInput = videoDeviceInput;
            }
            else
            {
                session.AddInput(_videoDeviceInput);
            }

            _runtimeErrorNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                AVCaptureSession.RuntimeErrorNotification,
                _sessionRuntimeErrorHandler, _videoDeviceInput.Device);

            return SessionSetupResult.Success;
        }

        private AVCaptureDevice GetVideoDevice()
        {
            if (_videoDeviceInput == null)
            {
                return GetDefaultDevice();
            }

            AVCaptureDevicePosition preferredPosition;
            AVCaptureDeviceType preferredDeviceType;

            var currentVideoDevice = _videoDeviceInput.Device;
            var currentPosition = currentVideoDevice.Position;

            switch (currentPosition)
            {
                case AVCaptureDevicePosition.Unspecified:
                case AVCaptureDevicePosition.Front:
                    preferredPosition = AVCaptureDevicePosition.Back;
                    preferredDeviceType = AVCaptureDeviceType.BuiltInDuoCamera;
                    break;
                case AVCaptureDevicePosition.Back:
                    preferredPosition = AVCaptureDevicePosition.Front;
                    preferredDeviceType = AVCaptureDeviceType.BuiltInWideAngleCamera;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var devices = _videoDeviceDiscoverySession.Devices;

            // First, look for a device with both the preferred position and device type. Otherwise, look for a device with only the preferred position.
            var videoDevice =
                devices.FirstOrDefault(x => x.Position == preferredPosition && x.DeviceType == preferredDeviceType);

            return videoDevice ?? devices.FirstOrDefault(x => x.Position == preferredPosition);
        }

        private static AVCaptureDevice GetDefaultDevice()
        {
            var device = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInDualCamera, AVMediaType.Video,
                AVCaptureDevicePosition.Back);

            if (device != null)
            {
                return device;
            }

            device = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaType.Video,
                AVCaptureDevicePosition.Back);

            if (device != null)
            {
                return device;
            }

            device = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaType.Video,
                AVCaptureDevicePosition.Front);

            return device;
        }
    }
}