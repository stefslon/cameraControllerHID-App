using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Configuration;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace cameraControllerHID
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // Create an instance of the USB reference device object
        private usbReferenceDevice theReferenceUsbDevice;

        // Create an instance of the joystick device object
        private joystickDevice connectedJoystick;

        // Application (user) settings
        private applicationSettings appSettings;

        public MainWindow()
        {
            InitializeComponent();

            camIcon.Opacity = 0.4;
            camLabel.Opacity = 0.4;
            joyIcon.Opacity = 0.4;
            joyLabel.Opacity = 0.4;

            theReferenceUsbDevice = new usbReferenceDevice(0x16C0, 0x0486, 0x0200);

            // Add a listener for usb events
            theReferenceUsbDevice.usbEvent += new usbReferenceDevice.usbEventsHandler(usbEvent_receiver);

            // Perform an initial search for the target USB device (in case
            // it is already connected as we will not get an event for it)
            theReferenceUsbDevice.findTargetDevice();

            // Connect to joystick device
            connectedJoystick = new joystickDevice();
            connectedJoystick.OnUpdate += new joystickEventHandler(joystick_updated);
            connectedJoystick.OnConnectionEvent += new joystickEventHandler(joystick_connected);
            connectedJoystick.startConnection();

            // Get application settings
            appSettings = new applicationSettings();
            appSettings.LoadAppSettings();
        }

        // Create joystick updated listener
        private void joystick_updated(object source, joystickEventArgs e)
        {
            if (theReferenceUsbDevice.isDeviceAttached)
            {
                double senseValue = 0;
                joySenseSlider.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (System.Threading.ThreadStart)delegate { senseValue = joySenseSlider.Value; }
                );
                theReferenceUsbDevice.run((sbyte)(e.X * senseValue), (sbyte)(e.Y * senseValue));
            }
        }

        // Create joystick connect listener
        private void joystick_connected(object source, joystickEventArgs e)
        {
            if (connectedJoystick.isDeviceAttached)
            {
                enableJoyControls();
                Debug.WriteLine("Yay! Joystick currently attached.");
            }
            else
            {
                disableJoyControls();
                Debug.WriteLine("Ow! Joystick connection lost.");
            }
        }


        // Create a listener for USB events
        private void usbEvent_receiver(object o, EventArgs e)
        {
            // Check the status of the USB device and update the form accordingly
            if (theReferenceUsbDevice.isDeviceAttached)
            {
                // USB Device is currently attached
                Debug.WriteLine("Camera Controller currently attached.");

                theReferenceUsbDevice.cameraName = theReferenceUsbDevice.getName();

                enableCamControls();
                camName.IsEnabled = true;
                camName.Content = theReferenceUsbDevice.cameraName;
            }
            else
            {
                // USB Device is currently unattached
                Debug.WriteLine("Camera Controller currently unattached.");

                theReferenceUsbDevice.cameraName = "";

                disableCamControls();
                try
                {
                    camName.IsEnabled = false;
                    camName.Content = "No Camera Connected";
                }
                catch { }
            }
        }

        private void enableCamControls()
        {
            camIcon.Opacity = 1;
            camLabel.Opacity = 1;

            UpArrow.IsEnabled = true;
            UpRightArrow.IsEnabled = true;
            RightArrow.IsEnabled = true;
            DownRightArrow.IsEnabled = true;
            DownArrow.IsEnabled = true;
            DownLeftArrow.IsEnabled = true;
            LeftArrow.IsEnabled = true;
            UpLeftArrow.IsEnabled = true;
            Stop.IsEnabled = true;
            ReCal.IsEnabled = true;
            SetPresetTime.IsEnabled = true;

            pre1.IsEnabled = true;
            pre2.IsEnabled = true;
            pre3.IsEnabled = true;
            pre4.IsEnabled = true;
            preSet.IsEnabled = true;
        }

        private void disableCamControls()
        {
            try
            {
                camIcon.Opacity = 0.4;
                camLabel.Opacity = 0.4;

                UpArrow.IsEnabled = false;
                UpRightArrow.IsEnabled = false;
                RightArrow.IsEnabled = false;
                DownRightArrow.IsEnabled = false;
                DownArrow.IsEnabled = false;
                DownLeftArrow.IsEnabled = false;
                LeftArrow.IsEnabled = false;
                UpLeftArrow.IsEnabled = false;
                Stop.IsEnabled = false;
                ReCal.IsEnabled = false;
                SetPresetTime.IsEnabled = false;

                pre1.IsEnabled = false;
                pre2.IsEnabled = false;
                pre3.IsEnabled = false;
                pre4.IsEnabled = false;
                preSet.IsEnabled = false;
            }
            catch { }
        }

        private void enableJoyControls()
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(delegate() 
                  {
                      joyIcon.Opacity = 1;
                      joyLabel.Opacity = 1;
                      joyOptions.IsEnabled = true;
                  }));
            }
            catch { }
        }

        private void disableJoyControls()
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(delegate()
                {
                    joyIcon.Opacity = 0.4;
                    joyLabel.Opacity = 0.4;
                    joyOptions.IsEnabled = false;
                }));
            }
            catch { }
        }

        private void clickWheel_Down(object sender, RoutedEventArgs e)
        {
            var objName = ((Button)sender).Name;
            //Debug.WriteLine(((Button)e.OriginalSource).Name);

            sbyte panDir = 0;
            sbyte tiltDir = 0;
            switch (objName)
            {
                case "UpArrow":
                    panDir = 0;
                    tiltDir = +1;
                    break;

                case "UpRightArrow":
                    panDir = +1;
                    tiltDir = +1;
                    break;

                case "RightArrow":
                    panDir = +1;
                    tiltDir = 0;
                    break;

                case "DownRightArrow":
                    panDir = +1;
                    tiltDir = -1;
                    break;

                case "DownArrow":
                    panDir = 0;
                    tiltDir = -1;
                    break;

                case "DownLeftArrow":
                    panDir = -1;
                    tiltDir = -1;
                    break;

                case "LeftArrow":
                    panDir = -1;
                    tiltDir = 0;
                    break;

                case "UpLeftArrow":
                    panDir = -1;
                    tiltDir = +1;
                    break;
            }

            //theReferenceUsbDevice.step(panDir, tiltDir);
            if ((panDir == 0) && (tiltDir == 0))
            {
                theReferenceUsbDevice.stop();
            }
            else
            {
                theReferenceUsbDevice.run((sbyte)(panDir * 5), (sbyte)(tiltDir * 5));
            }
        }

        private void clickWheel_Up(object sender, RoutedEventArgs e)
        {
            theReferenceUsbDevice.stop();
        }

        private async void camName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (theReferenceUsbDevice.isDeviceAttached)
            {
                var newName = await this.ShowInputAsync("", "Rename this camera");

                if (newName == null) //user pressed cancel
                    return;

                if (theReferenceUsbDevice.isDeviceAttached)
                {
                    theReferenceUsbDevice.setName(newName);
                    camName.Content = newName;
                }
            }
        }

        private void preN_Click(object sender, RoutedEventArgs e)
        {
            var objName = ((Button)sender).Name;
            if (!isPresentProgramMode)
            {
                switch (objName)
                {
                    case "pre1":
                        theReferenceUsbDevice.goPreset(1, (ushort)(presetTimeSlider.Value * 1000));
                        break;

                    case "pre2":
                        theReferenceUsbDevice.goPreset(2, (ushort)(presetTimeSlider.Value * 1000));
                        break;

                    case "pre3":
                        theReferenceUsbDevice.goPreset(3, (ushort)(presetTimeSlider.Value * 1000));
                        break;

                    case "pre4":
                        theReferenceUsbDevice.goPreset(4, (ushort)(presetTimeSlider.Value * 1000));
                        break;
                }
            }
            else
            {
                switch (objName)
                {
                    case "pre1": theReferenceUsbDevice.setPreset(1); break;
                    case "pre2": theReferenceUsbDevice.setPreset(2); break;
                    case "pre3": theReferenceUsbDevice.setPreset(3); break;
                    case "pre4": theReferenceUsbDevice.setPreset(4); break;
                }
                preSet_Restore();
            }

        }

        private bool isPresentProgramMode = false;
        private void preSet_Click(object sender, RoutedEventArgs e)
        {
            if (!isPresentProgramMode)
            {
                pre1.Content = "+"; pre1.IsEnabled = true;
                pre2.Content = "+"; pre2.IsEnabled = true;
                pre3.Content = "+"; pre3.IsEnabled = true;
                pre4.Content = "+"; pre4.IsEnabled = true;
                isPresentProgramMode = true;
            }
            else
            {
                preSet_Restore();
            }
        }

        private void preSet_Restore()
        {
            pre1.Content = "1";
            pre2.Content = "2";
            pre3.Content = "3";
            pre4.Content = "4";
            isPresentProgramMode = false;
        }

        private void preN_Program(object sender, MouseButtonEventArgs e)
        {
            var objName = ((Button)sender).Name;
            Debug.WriteLine(e.ClickCount);
            switch (objName)
            {
                case "pre1":
                    Debug.WriteLine("Pre1 Program");
                    break;

                case "pre2":
                    Debug.WriteLine("Pre2 Program");
                    break;

                case "pre3":
                    Debug.WriteLine("Pre3 Program");
                    break;

                case "pre4":
                    Debug.WriteLine("Pre4 Program");
                    break;
            }
        }

        private void ReCal_Click(object sender, RoutedEventArgs e)
        {
            theReferenceUsbDevice.reCalibrate();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            theReferenceUsbDevice.stop();
        }

        private void SetPresetTime_Click(object sender, RoutedEventArgs e)
        {
            presetTimeFlyout.IsOpen = !presetTimeFlyout.IsOpen;
        }

        private void presetTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            presetTime.Content = string.Format("{0:D}s", (int)presetTimeSlider.Value);
        }


        private void joyOpt_Click(object sender, RoutedEventArgs e)
        {
            joyOptionsFlyout.IsOpen = !joyOptionsFlyout.IsOpen;
        }

        private void joySenseSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            joySense.Content = string.Format("{0:D}", (int)joySenseSlider.Value);
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if ((appSettings.windowLocation.X == 0) && (appSettings.windowLocation.Y == 0))
            {
                // Place window in a corner by default
                var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                this.Left = desktopWorkingArea.Right - this.Width;
                this.Top = desktopWorkingArea.Bottom - this.Height;
            }
            else
            {
                this.Left = appSettings.windowLocation.X;
                this.Top = appSettings.windowLocation.Y;
            }

            presetTimeSlider.Value = appSettings.presetRuntime;
            joySenseSlider.Value = appSettings.joystickSensitivity;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            appSettings.windowLocation = new Point(this.Left, this.Top);
            appSettings.presetRuntime = presetTimeSlider.Value;
            appSettings.joystickSensitivity = joySenseSlider.Value;
            appSettings.SaveAppSettings();
        }

    }
}
