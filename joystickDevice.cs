using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX.DirectInput;

namespace cameraControllerHID
{
    //First we have to define a delegate that acts as a signature for the
    //function that is ultimately called when the event is triggered.
    //You will notice that the second parameter is of MyEventArgs type.
    //This object will contain information about the triggered event.
    public delegate void joystickEventHandler(object source, joystickEventArgs e);

    //This is a class which describes the event to the class that recieves it.
    //An EventArgs class must always derive from System.EventArgs.
    public class joystickEventArgs : EventArgs
    {
        public double X;
        public double Y;
        public joystickEventArgs(double setX, double setY)
        {
            X = setX;
            Y = setY;
        }
    }

    class joystickDevice
    {
        public event joystickEventHandler OnUpdate;
        public event joystickEventHandler OnConnectionEvent;

        private Joystick joystick;
        private System.Threading.Timer pollTimer;
        private System.Threading.Timer connectTimer;

        public double X;
        public double Y;
        public bool isDeviceAttached = false;

        public joystickDevice()
        {
            joystick = null;
        }

        public void startConnection()
        {
            attemptJoystickConnect(null);

            if (joystick==null)
            {
                Debug.WriteLine("No joystick/Gamepad found.");
                connectTimer = new System.Threading.Timer(attemptJoystickConnect, null, 0, 1000);
            }
        }

        private void getJoystickData(object state)
        {
            try
            {
                joystick.Poll();
                JoystickUpdate[] datas = joystick.GetBufferedData();
                foreach (JoystickUpdate joyState in datas)
                {
                    if (joyState.Offset == JoystickOffset.X) X = Math.Max(Math.Min(((double)joyState.Value - 32767.5) / 32767.5, +1), -1);
                    if (joyState.Offset == JoystickOffset.Y) Y = Math.Max(Math.Min(((double)joyState.Value - 32767.5) / 32767.5, +1), -1);
                    if (OnUpdate != null)
                    {
                        OnUpdate(this, new joystickEventArgs(X, Y));
                    }
                }
            }
            catch (SharpDX.SharpDXException ex)
            {
                // Most likely connection was lost, so resume connection timer
                Debug.WriteLine("Joystick connection lost.");
                isDeviceAttached = false;
                if (pollTimer != null)
                {
                    pollTimer.Dispose();
                }

                connectTimer = new System.Threading.Timer(attemptJoystickConnect, null, 0, 1000);

                if (OnConnectionEvent != null)
                {
                    OnConnectionEvent(this, new joystickEventArgs(X, Y));
                }
            }
        }

        private void attemptJoystickConnect(object state)
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                return;
            }
            else
            {
                // Instantiate the joystick
                joystick = new Joystick(directInput, joystickGuid);

                Debug.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

                // Query all suported ForceFeedback effects
                var allEffects = joystick.GetEffects();
                foreach (var effectInfo in allEffects)
                    Debug.WriteLine("Effect available {0}", effectInfo.Name);

                // Set BufferSize in order to use buffered data.
                joystick.Properties.BufferSize = 128;

                // Acquire the joystick
                joystick.Acquire();

                // Poll events from joystick
                pollTimer = new System.Threading.Timer(getJoystickData, null, 0, 20);

                isDeviceAttached = true;

                // Stop connection timer
                if (connectTimer != null)
                {
                    connectTimer.Dispose();
                }
                if (OnConnectionEvent != null)
                {
                    OnConnectionEvent(this, new joystickEventArgs(X, Y));
                }
            }
        }
    }
}
