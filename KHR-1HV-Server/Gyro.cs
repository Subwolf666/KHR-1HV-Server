using System;
using System.Collections.Generic;
using System.Text;
using RoBoIO_DotNet;

namespace Server
{
    class Gyro
    {
        private static Logging Log = new Logging();
        private static string Module = "Gyro.cs";

        private static byte i2c_address = 0x21; // gyro i2c address
        private static byte high, low = 0;
        private static byte d1, d2;

        private static string[] _out_title = new string[] { "X-OUT", "X45-OUT", "Y-OUT", "Y45-OUT", "Z-OUT", "Z45-OUT", "IDG_Temp", "ISZ_Temp" };

        public static short[] GyroData()
        {
            Log.Module = Module;

            short[] _out_value = new short[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            if (Server.I2C.Connected == true)
            {
                RoBoIO.i2c0master_StartN(i2c_address, (byte)RoBoIO.I2C_WRITE, 2); //AS pin is high
                RoBoIO.i2c0master_WriteN(0x03); //cycle time register
                RoBoIO.i2c0master_WriteN(0x01); //convert time

                Log.WriteLineMessage("Read 3-axis values of Gyro and chip temperature...");
                for (int i = 0; i < 8; i++)
                {
                    high = (byte)((0xF0 & (0x01 << i)) >> 4); //CH5 ~ CH8
                    low = (byte)((0x0F & (0x01 << i)) << 4); //CH1 ~ CH4

                    RoBoIO.i2c0master_StartN(i2c_address, (byte)RoBoIO.I2C_WRITE, 3); //write 3 bytes
                    RoBoIO.i2c0master_WriteN(0x02); //configuration register
                    RoBoIO.i2c0master_WriteN(high);
                    RoBoIO.i2c0master_WriteN((byte)(low + 0x0C));//0x0c : FLTR = 1,ALERT/EN = 1
                    RoBoIO.delay_ms(10);

                    RoBoIO.i2c0master_StartN(i2c_address, (byte)RoBoIO.I2C_WRITE, 1);
                    RoBoIO.i2c0master_SetRestartN((byte)RoBoIO.I2C_READ, 2);
                    RoBoIO.i2c0master_WriteN(0x00); //Read data form Conversion Result Register

                    //Data : 12bits
                    d1 = (byte)RoBoIO.i2c0master_ReadN();
                    d2 = (byte)RoBoIO.i2c0master_ReadN();

                    _out_value[((d1 & 0x70) >> 4)] = ((short)((d1 & 0x0f) * 256 + d2));
                }
            }
            return _out_value;
        }

        public static string[] out_title
        {
            get { return _out_title; }
        }
    }
}
