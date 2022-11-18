using System;
using AVFoundation;

namespace Softeq.ImagePicker.Media.Capture
{
    public class AudioCaptureSession
    {
        public void ConfigureSession(AVCaptureSession session)
        {
            Console.WriteLine("capture session: configuring - adding audio input");

            // Add audio input, if fails no need to fail whole configuration
            var audioDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Audio);
            var audioDeviceInput = AVCaptureDeviceInput.FromDevice(audioDevice);

            if (session.CanAddInput(audioDeviceInput))
            {
                session.AddInput(audioDeviceInput);
            }
            else
            {
                Console.WriteLine("capture session: could not add audio device input to the session");
            }
        }
    }
}