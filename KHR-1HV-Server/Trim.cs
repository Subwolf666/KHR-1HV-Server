using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ini;

namespace Server
{
    public static class Trim
    {
        private static Logging Log = new Logging();
        private static IniFile _Data;
        private static int[] iTrim = new int[StaticUtilities.numberOfServos];
        private static bool _connected = false;
        private static string Filename = "Trim.ini";
        private static string Module = "Trim.cs";

        // Method
        //
        public static bool Init()
        {
            string applicationFolder = Path.GetDirectoryName(Application.ExecutablePath);
            _Data = new IniFile(string.Format("{0}\\{1}", applicationFolder, Filename));
            Log.Module = Module;
            if (_Data.Exists())
            {
                Log.WriteLineSucces(string.Format("Opening: {0}", Filename));
                if (_Data.Load())
                {
                    _connected = true;
                    Log.WriteLineSucces(string.Format("Loading: {0}", Filename));
                    return true;
                }
                Log.WriteLineFail(string.Format("Loading: {0}", Filename));
                _connected = CreateTrim();
                return _connected;
            }
            else
            {
                Log.WriteLineFail(string.Format("Opening: {0}", Filename));
                _connected = CreateTrim();
                return _connected;
            }
        }

        // Method
        //
        private static bool CreateTrim()
        {
            IniSection section = new IniSection();
            section.Add("Prm", "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0");
            _Data.Add("Trim", section);
            Log.WriteLineSucces(string.Format("Creating: {0}", Filename));
            return Save();
        }

        // Method
        //
        public static bool Save()
        {
            string message = string.Format("Saving: {0}", Filename);
            if (_Data.Save())
            {
                Log.WriteLineSucces(message);
                return true;
            }
            Log.WriteLineFail(message);
            return false;
        }

        // Property
        //
        public static string Data
        {
            get
            {
                return _Data["Trim"]["Prm"];
            }
            set
            {
                _Data["Trim"]["Prm"] = value;
            }
        }

        // Property
        public static int[] iData
        {
            get
            {
                string[] saTemp = _Data["Trim"]["Prm"].Split(',');
                for (int i = 0; i < StaticUtilities.numberOfServos; i++)
                    iTrim[i] = Convert.ToInt32(saTemp[i]);
                return iTrim;
            }
        }

        // Property
        //
        public static bool Open
        {
            get
            {
                return _connected;
            }
        }
    }
}
