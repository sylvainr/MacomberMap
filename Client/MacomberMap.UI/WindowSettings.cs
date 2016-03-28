using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace MacomberMap.UI
{
    public static class Settings
    {
        public static void LoadWindowSettings(this Form form, string filename)
        {
            string file = System.IO.Path.GetFullPath(filename);

            if (File.Exists(file))
            {
                using (Stream streamReader = File.Open(file, FileMode.Open))
                {

                    WindowSettings loadedSettings = WindowSettings.LoadBinary(streamReader);

                    loadedSettings.SetFormSettings(form);

                    streamReader.Close();
                }
            }
        }

        public static void SaveWindowSettings(this Form form, string filename)
        {
            string path = System.IO.Path.GetDirectoryName(filename);
            string file = System.IO.Path.GetFullPath(filename);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            using (Stream sw = File.Open(file, FileMode.Create))
            {
                WindowSettings settings = new WindowSettings(form);
                settings.SaveBinary(sw);
                sw.Close();
            }
        }

    }
    [Serializable]
    [System.ComponentModel.ToolboxItem(false)]
    public class WindowSettings : ISerializable
    {
        #region Constructors

        public WindowSettings()
        {
        }

        public WindowSettings(Form form)
        {
            this.ReadFormSettings(form);
        }

        public WindowSettings(SerializationInfo info, StreamingContext ctxt)
        {
            this.WindowState = (FormWindowState)info.GetValue("WindowState", typeof(FormWindowState));
            this.Size = (Size)info.GetValue("Size", typeof(Size));
            this.Location = (Point)info.GetValue("Location", typeof(Point));
        }

        #endregion

        #region Properties

        public Point Location { get; set; }

        public Size Size { get; set; }

        public FormWindowState WindowState { get; set; }

        #endregion

        #region Public Methods

        public static WindowSettings LoadBinary(Stream streamReader)
        {
            BinaryFormatter bs = new BinaryFormatter();
            WindowSettings loadedSettings = (WindowSettings)bs.Deserialize(streamReader);
            return loadedSettings;
        }

        public void SaveBinary(Stream stream)
        {
            BinaryFormatter bs = new BinaryFormatter();
            bs.Serialize(stream, this);
        }

        public void CopySettings(WindowSettings settings)
        {
            this.WindowState = settings.WindowState;
            this.Location = settings.Location;
            this.Size = settings.Size;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("WindowState", this.WindowState, typeof(FormWindowState));
            info.AddValue("Size", this.Size, typeof(Size));
            info.AddValue("Location", this.Location, typeof(Point));
        }

        public void ReadFormSettings(Form form)
        {
            this.WindowState = form.WindowState;
            this.Location = form.Location;
            this.Size = form.Size;
        }

        public void SetFormSettings(Form form)
        {

            if (IsOnScreen(this.Location))
                form.Location = this.Location;

            form.WindowState = this.WindowState;
            form.Size = this.Size;
        }

        #endregion

        #region Private Methods

        private static bool IsOnScreen(Rectangle rect)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                if (screen.WorkingArea.Contains(rect))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsOnScreen(Point loc)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                if (screen.WorkingArea.Contains(loc))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
