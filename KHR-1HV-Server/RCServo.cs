using System;
using System.Collections.Generic;
using System.Text;
using RoBoIO_DotNet;

namespace Server
{
    public static class RCServo
    {
        private static Logging Log = new Logging();
        private static string Module = "RCServo.cs";
        private static bool _connected = false;
        private static int servo_idx = 1;
        private static uint[] servo = new uint[2]
        {
            RoBoIO.RCSERVO_SERVO_DEFAULT,
            RoBoIO.RCSERVO_KONDO_KRS788,
        };
        private static string[] servo_name = new string[2]
        {
            "Default Servo",
            "KONDO KRS-788",
        };

        //public static bool Init(UInt32 channels)
        public static bool Init()
        {
            Log.Module = Module;
            if (!_connected)
            {
                RoBoIO.roboio_SetRBVer(Server.MainIni.RoboardVersion);
                Log.WriteLineSucces(string.Format("Set Roboard Version: {0}", Server.MainIni.RoboardVersionText));
            }

            if (!_connected)
            {
                for (int i = 0; i < StaticUtilities.numberOfServos; i++)
                {
                    // The servo is KONDO_KRS788 and plugged on
                    RoBoIO.rcservo_SetServo(i, RoBoIO.RCSERVO_KONDO_KRS788);
                    // Set the servo state
                    RoBoIO.rcservo_SetServoParams1(i, 10000, 700, 2300);
                    // The servo has feedback functionality
                    RoBoIO.rcservo_SetServoType(i, RoBoIO.RCSERVO_SV_FEEDBACK, RoBoIO.RCSERVO_FB_SAFEMODE);
                }

                UInt32 channels = 0;
                for (int j = 0; j < StaticUtilities.numberOfServos; j++)
                {
                    if (MainIni.ChannelFunction[j] == 1)
                        channels += Convert.ToUInt32(Math.Pow(2, j));
                }
                Log.WriteLineMessage(string.Format("Channels : {0:X}", channels));
                if (RoBoIO.rcservo_Initialize((UInt32)channels) == true)
                {
                    Log.WriteLineSucces(string.Format("Opening: RCSERVO lib (for {0})", servo_name[servo_idx]));
                    RoBoIO.rcservo_EnterPlayMode();
                    RoBoIO.rcservo_EnableMPOS();
                    RoBoIO.rcservo_SetFPS(MainIni.FPS);
                    _connected = true;
                    return true;
                }
                Log.WriteLineFail(string.Format("Opening: RCSERVO lib (for {0})", servo_name[servo_idx]));
                _connected = false;
                Log.WriteLineError(string.Format("RCSERVO lib fails to initialize ({0})", RoBoIO_DotNet.RoBoIO.roboio_GetErrMsg()));
                return false;
            }
            Log.WriteLineMessage(string.Format("Opening: RCSERVO lib (for {0})...already open", servo_name[servo_idx]));
            return true;
        }

        public static void Close()
        {
	        RoBoIO.rcservo_Close();
            _connected = false;
            Log.WriteLineSucces("Closing RCSERVO lib");
        }

        public static string CPU()
        {
            string message = string.Empty;

            switch (RoBoIO.io_CpuID())
            {
                case RoBoIO.CPU_UNSUPPORTED:
                    message = "CPU Unsupported";
                    break;
                case RoBoIO.CPU_VORTEX86DX_1:
                    message = "CPU VORTEX86DX1";
                    break;
                case RoBoIO.CPU_VORTEX86DX_2:
                    message = "CPU VORTEX86DX2";
                    break;
                case RoBoIO.CPU_VORTEX86DX_3:
                    message = "CPU VORTEX86DX3";
                    break;
                case RoBoIO.CPU_VORTEX86SX:
                    message = "CPU VORTEX86SX";
                    break;
                default:
                    message = "CPU";
                    break;
            }
            return message;
        }

        public static string Version()
        {
            string message = string.Empty;

            switch (RoBoIO.roboio_GetRBVer())
            {
                case RoBoIO.RB_100:
                    message = "RB-100";
                    break;
                case RoBoIO.RB_100b1:
                    message = "RB-100b1";
                    break;
                case RoBoIO.RB_100b2:
                    message = "RB-100b2";
                    break;
                case RoBoIO.RB_110:
                    message = "RB-110";
                    break;
                case RoBoIO.RB_111:
                    message = "RB-111";
                    break;
                default:
                    message = "No version";
                    break;
            }
            return message;
        }

        // Property
        //
        public static bool Connected
        {
            get { return _connected; }
            set { _connected = value; }
        }
    }
}
