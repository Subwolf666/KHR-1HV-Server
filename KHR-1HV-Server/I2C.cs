using System;
using System.Collections.Generic;
using System.Text;
using RoBoIO_DotNet;

namespace Server
{
    public static class I2C
    {
        private static Logging Log = new Logging();
        private static string Module = "I2C.cs";
        private const uint i2c_clock = 400000;
        private static bool _connected = false;

        // Method
        //
        public static bool Init()
        {
            Log.Module = Module;
            if (!_connected)
            {
                if (RoBoIO.i2c_Initialize(RoBoIO.I2CIRQ_DISABLE))
                {
                    RoBoIO.i2c0_SetSpeed(RoBoIO.I2CMODE_FAST, i2c_clock);
                    _connected = true;
                    Log.WriteLineSucces("Opening: I2C lib");
                    return true;
                }
                _connected = false;
                Log.WriteLineFail("Opening: I2C lib");
                Log.WriteLineError(string.Format("I2C lib fails to initialize ({0})", RoBoIO_DotNet.RoBoIO.roboio_GetErrMsg()));
                return false;
            }
            Log.WriteLineMessage("Opening: I2C lib...already open");
            return true;
        }

        // Method
        //
        public static void Close()
        {
            RoBoIO.i2c_Close();
            _connected = false;
            Log.WriteLineSucces("Closing I2C lib");
        }

        // Property
        //
        public static bool Connected
        {
            get { return _connected; }
        }
    }
}
