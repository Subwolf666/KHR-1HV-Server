using System;
using System.Collections.Generic;
using System.Text;
using RoBoIO_DotNet;

namespace Server
{
    public static class SPI
    {
        private static Logging Log = new Logging();
        private static string Module = "SPI.cs";
        private static bool _connected = false;

        // Method
        //
        public static bool Init()
        {
            Log.Module = Module;
            if (!_connected)
            {
                if (RoBoIO.spi_Initialize(RoBoIO.SPICLK_21400KHZ))
                {
                    _connected = true;
                    Log.WriteLineSucces("Opening: SPI lib");
                    return true;
                }
                _connected = false;
                Log.WriteLineFail("Opening: SPI lib");
                Log.WriteLineError(string.Format("SPI lib fails to initialize ({0})", RoBoIO_DotNet.RoBoIO.roboio_GetErrMsg()));
                return false;
            }
            Log.WriteLineMessage("Opening: SPI lib...already open");
            return true;
        }

        // Method
        //
        public static void Close()
        {
            RoBoIO.spi_Close();
            _connected = false;
            Log.WriteLineSucces("Closing: SPI lib");
        }

        // Property
        //
        public static bool Connected
        {
            get { return _connected; }
        }
    }
}
