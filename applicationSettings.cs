using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace cameraControllerHID
{
    public class applicationSettings
    {
        // Variables used to store the application settings.
        private double _joystickSensitivity = 15;
        private double _presetRuntime = 5;
        private System.Windows.Point _windowLocation;

        // Properties used to access the application settings variables.
        public double joystickSensitivity
        {
            get { return _joystickSensitivity; }
            set { _joystickSensitivity = value; }
        }

        public double presetRuntime
        {
            get { return _presetRuntime; }
            set { _presetRuntime = value; }
        }

        public System.Windows.Point windowLocation
        {
            get { return _windowLocation; }
            set { _windowLocation = value; }
        }

        // Serializes the class to the config file
        // if any of the settings have changed.
        public bool SaveAppSettings()
        {
            StreamWriter myWriter = null;
            XmlSerializer mySerializer = null;
            try
            {
                // Create an XmlSerializer for the 
                // ApplicationSettings type.
                mySerializer = new XmlSerializer(typeof(applicationSettings));
                myWriter =  new StreamWriter(Application.LocalUserAppDataPath + @"\cameraController.config", false);
                // Serialize this instance of the ApplicationSettings 
                // class to the config file.
                mySerializer.Serialize(myWriter, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // If the FileStream is open, close it.
                if (myWriter != null)
                {
                    myWriter.Close();
                }
            }

            return true;
        }

        // Deserializes the class from the config file.
        public bool LoadAppSettings()
        {
            XmlSerializer mySerializer = null;
            FileStream myFileStream = null;
            bool fileExists = false;

            try
            {
                // Create an XmlSerializer for the ApplicationSettings type.
                mySerializer = new XmlSerializer(typeof(applicationSettings));
                FileInfo fi = new FileInfo(Application.LocalUserAppDataPath + @"\cameraController.config");
                // If the config file exists, open it.
                if (fi.Exists)
                {
                    myFileStream = fi.OpenRead();
                    // Create a new instance of the ApplicationSettings by
                    // deserializing the config file.
                    applicationSettings myAppSettings =
                      (applicationSettings)mySerializer.Deserialize(
                       myFileStream);
                    // Assign the property values to this instance of 
                    // the ApplicationSettings class.
                    this._windowLocation = myAppSettings.windowLocation;
                    this._joystickSensitivity = myAppSettings.joystickSensitivity;
                    this._presetRuntime = myAppSettings.presetRuntime;
                    fileExists = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // If the FileStream is open, close it.
                if (myFileStream != null)
                {
                    myFileStream.Close();
                }
            }

            return fileExists;
        }
    }
}
