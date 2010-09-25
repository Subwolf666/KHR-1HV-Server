using System;
using System.Collections.Generic;
using System.Text;
using RoBoIO_DotNet;

namespace Server
{
    public static class AD7918
    {
        private static Logging Log = new Logging();
        private static string Module = "AD7918.cs";
        private static bool _connected = false;

        // Method
        //
        public static bool Init()
        {
            if (!SPI.Connected)
                SPI.Init();

            Log.Module = Module;
            if (!_connected)
            {
                if (RoBoIO.ad7918_InitializeMCH(RoBoIO.AD7918_USECHANNEL0 +
                        RoBoIO.AD7918_USECHANNEL1 +
                        RoBoIO.AD7918_USECHANNEL2 +
                        RoBoIO.AD7918_USECHANNEL3 +
                        RoBoIO.AD7918_USECHANNEL4 +
                        RoBoIO.AD7918_USECHANNEL5 +
                        RoBoIO.AD7918_USECHANNEL6 +
                        RoBoIO.AD7918_USECHANNEL7,
                        RoBoIO.AD7918MODE_RANGE_2VREF,
                        RoBoIO.AD7918MODE_CODING_1023))
                {
                    _connected = true;
                    Log.WriteLineSucces("Opening: AD7918 lib");
                    return true;
                }
                _connected = false;
                Log.WriteLineFail("Opening: AD7918 lib");
                Log.WriteLineError(string.Format("AD7918 lib fails to open ({0})", RoBoIO_DotNet.RoBoIO.roboio_GetErrMsg()));
                return false;
            }
            Log.WriteLineMessage("Opening: AD7918 lib...already open");
            return true;
        }

        // Method
        //
        public static void Close()
        {
            RoBoIO.ad7918_CloseMCH();
            _connected = false;
            Log.WriteLineSucces("Closing AD7918 lib");
        }

        // Property
        //
        public static bool Connected
        {
            get { return _connected; }
        }
    }
}
