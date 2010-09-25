using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ini;
using RoBoIO_DotNet;

namespace Server
{
    public static class MainIni
    {
        private static Logging Log = new Logging();
        private static string Module = "MainIni.cs";
        private static bool _connected = false;
        private static IniFile main_ini;
        private static string _RoboardVersion;
        private static string _MotionReplay;
        private static string _EnableRemoteControl;
        private static string _PowerUpMotion;
        private static string _LowPowerMotion;
        private static string _LowPowerVoltage;
        private static string _TimeBase;
        private static string _FPS;
        private static string _PA1REF;
        private static string _PA2REF;
        private static string _PA3REF;
        private static string _PA4REF;
        private static string _PA5REF;
        private static string _PA6REF;
        private static int[] _ChannelFunction = new int[StaticUtilities.numberOfServos];
        private static string Filename;

        public static bool Init()
        {
            string applicationFolder = Path.GetDirectoryName(Application.ExecutablePath);

            Filename = "KHR-1HV.ini";
            main_ini = new IniFile(string.Format("{0}\\{1}", applicationFolder, Filename));
            Log.Module = Module;
            if (main_ini.Exists())
            {
                Log.WriteLineSucces(string.Format("Opening: {0}", Filename));
                if (main_ini.Load())
                {
                    Log.WriteLineSucces(string.Format("Loading: {0}", Filename));
                    Read();
                    return true;
                }
                Log.WriteLineFail(string.Format("Loading: {0}", Filename));
                return false;
            }
            else
            {
                Log.WriteLineFail(string.Format("Opening: {0}", Filename));
                IniSection section = new IniSection();
                section.Add("Roboard", "RB100");
                section.Add("MotionReplay", "false");
                section.Add("EnableRemoteControl", "false");
                section.Add("PowerUpMotion", "0");
                section.Add("LowPowerMotion", "0");
                section.Add("LowPowerVoltage", "120");
                section.Add("Timebase", "100");
                section.Add("FPS", "1");
                section.Add("PA1REF", "0");
                section.Add("PA2REF", "0");
                section.Add("PA3REF", "0");
                section.Add("PA4REF", "0");
                section.Add("PA5REF", "0");
                section.Add("PA6REF", "0");
                for (int i = 0; i < StaticUtilities.numberOfServos; i++)
                {
                    string tmp = string.Format("CH{0}", (i + 1));
                    section.Add(tmp, "1");
                }
                main_ini.Add("Main", section);
                Log.WriteLineSucces(string.Format("Creating: {0}", Filename));
                if (Save())
                {
                    Read();
                    return true;
                }
                return false;
            }
        }

        // Method
        //
        public static bool Save()
        {
            if (main_ini.Save())
            {
                Log.WriteLineSucces(string.Format("Saving: {0}", Filename));
                return true;
            }
            Log.WriteLineFail(string.Format("Saving: {0}", Filename));
            return false;
        }

        // Method
        //
        private static bool Read()
        {
            _RoboardVersion = main_ini["Main"]["Roboard"];
            _MotionReplay = main_ini["Main"]["MotionReplay"];
            _EnableRemoteControl = main_ini["Main"]["EnableRemoteControl"];
            _PowerUpMotion = main_ini["Main"]["PowerUpMotion"];
            _LowPowerMotion = main_ini["Main"]["LowPowerMotion"];
            _LowPowerVoltage = main_ini["Main"]["LowPowerVoltage"];
            _TimeBase = main_ini["Main"]["Timebase"];
            _FPS = main_ini["Main"]["FPS"];
            _PA1REF = main_ini["Main"]["PA1REF"];
            _PA2REF = main_ini["Main"]["PA2REF"];
            _PA3REF = main_ini["Main"]["PA3REF"];
            _PA4REF = main_ini["Main"]["PA4REF"];
            _PA5REF = main_ini["Main"]["PA5REF"];
            _PA6REF = main_ini["Main"]["PA6REF"];
            for (int i = 0; i < StaticUtilities.numberOfServos; i++)
            {
                _ChannelFunction[i] = Convert.ToInt32(main_ini["Main"][string.Format("CH{0}", (i + 1))]);
            }
            _connected = true;
            return true;
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

        // Property
        //
        public static int RoboardVersion
        {
            get
            {
                int rbVersion;

                switch (_RoboardVersion)
                {
                    case "RB100":
                        rbVersion = RoBoIO.RB_100;
                        break;
                    case "RB110":
                        rbVersion = RoBoIO.RB_110;
                        break;
                    default:
                        rbVersion = RoBoIO.RB_100;
                        break;
                }
                return rbVersion;
            }
        }

        // Property
        //
        public static string RoboardVersionText
        {
            get { return _RoboardVersion; }
        }

        // Property
        //            
        public static bool MotionReplay
        {
            get
            {
                return Convert.ToBoolean(_MotionReplay);
            }
            set
            {
                _MotionReplay = value.ToString();
                main_ini["Main"]["MotionReplay"] = _MotionReplay;
            }
        }

        // Property
        //
        public static bool EnableRemoteControl
        {
            get
            {
                return Convert.ToBoolean(_EnableRemoteControl);
            }
            set
            {
                _EnableRemoteControl = value.ToString();
                main_ini["Main"]["EnableRemoteControl"] = _EnableRemoteControl;
            }
        }

        // Property
        //
        public static int TimeBase
        {
            get
            {
                return Convert.ToInt32(_TimeBase);
            }
            set
            {
                _TimeBase = Convert.ToString(value);
                main_ini["Main"]["Timebase"] = _TimeBase;
            }
        }

        // Property
        //
        public static int FPS
        {
            get
            {
                return Convert.ToInt32(_FPS);
            }
            set
            {
                _FPS = Convert.ToString(value);
                main_ini["Main"]["FPS"] = _FPS;
            }
        }

        // Property
        //
        public static int PA1Reference
        {
            get
            {
                return Convert.ToInt32(_PA1REF);
            }
            set
            {
                _PA1REF = Convert.ToString(value);
                main_ini["Main"]["PA1REF"] = _PA1REF;
            }
        }

        // Property
        //
        public static int PA2Reference
        {
            get
            {
                return Convert.ToInt32(_PA2REF);
            }
            set
            {
                _PA2REF = Convert.ToString(value);
                main_ini["Main"]["PA2REF"] = _PA2REF;
            }
        }

        // Property
        //
        public static int PA3Reference
        {
            get
            {
                return Convert.ToInt32(_PA3REF);
            }
            set
            {
                _PA3REF = Convert.ToString(value);
                main_ini["Main"]["PA3REF"] = _PA3REF;
            }
        }

        // Property
        //
        public static int PA4Reference
        {
            get
            {
                return Convert.ToInt32(_PA4REF);
            }
            set
            {
                _PA4REF = Convert.ToString(value);
                main_ini["Main"]["PA4REF"] = _PA4REF;
            }
        }

        // Property
        //
        public static int PA5Reference
        {
            get
            {
                return Convert.ToInt32(_PA5REF);
            }
            set
            {
                _PA5REF = Convert.ToString(value);
                main_ini["Main"]["PA5REF"] = _PA5REF;
            }
        }

        // Property
        //
        public static int PA6Reference
        {
            get
            {
                return Convert.ToInt32(_PA6REF);
            }
            set
            {
                _PA6REF = Convert.ToString(value);
                main_ini["Main"]["PA6REF"] = _PA6REF;
            }
        }

        // Property
        //
        public static int[] ChannelFunction
        {
            get
            {
                return _ChannelFunction;
            }
            set
            {
                _ChannelFunction = value;
                for (int i = 0; i < StaticUtilities.numberOfServos; i++)
                {
                    main_ini["Main"][string.Format("CH{0}", (i + 1))] = _ChannelFunction[i].ToString();
                }
                //
            }
        }

            // Property
            //
            public static int LowPowerVoltage
        {
            get
            {
                return Convert.ToInt32(_LowPowerVoltage);
            }
            set
            {
                _LowPowerVoltage = value.ToString();
                main_ini["Main"]["LowPowerVoltage"] = _LowPowerVoltage;
            }
        }

        // Property
        //
        public static int LowPowerMotion
        {
            get
            {
                return Convert.ToInt32(_LowPowerMotion);
            }
            set
            {
                _LowPowerMotion = Convert.ToString(value);
                main_ini["Main"]["LowPowerMotion"] = _LowPowerMotion;
            }
        }

        // Property
        //
        public static int PowerUpMotion
        {
            get
            {
                return Convert.ToInt32(_PowerUpMotion);
            }

            set
            {
                _PowerUpMotion = Convert.ToString(value);
                main_ini["Main"]["PowerUpMotion"] = _PowerUpMotion;
            }
        }
    }
}
