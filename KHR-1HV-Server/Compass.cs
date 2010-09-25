using System;
using System.Collections.Generic;
using System.Text;
using RoBoIO_DotNet;

namespace Server
{
    class Compass
    {
        private static Logging Log = new Logging();
        private static string Module = "Compass.cs";

        private static byte i2c_address = 0x3C; // compass i2c address
																										
        private static string[] _out_title = new string[] { "X-OUT", "Y-OUT", "Z-OUT" };
        
        public static short[] CompassData()
        {
            Log.Module = Module;

            short[] _out_value = new short[] { 0, 0, 0 };
            byte[] data = new byte[2];
            
            if (Server.I2C.Connected == true)
            {
                byte i2c_address_write = (byte)(i2c_address >> 1);
                RoBoIO.i2c0master_StartN(i2c_address_write, (byte)RoBoIO.I2C_WRITE, 2);//write 2 byte
                RoBoIO.i2c0master_WriteN(0x02); //mode register
                RoBoIO.i2c0master_WriteN(0x00); //continue-measureture mode

                Log.WriteLineMessage("Read 3-axis values of Compass...");

                RoBoIO.delay_ms(100);

                RoBoIO.i2c0master_StartN(i2c_address_write, (byte)RoBoIO.I2C_WRITE, 1);
                RoBoIO.i2c0master_SetRestartN((byte)RoBoIO.I2C_READ, 6);
                RoBoIO.i2c0master_WriteN(0x03); //Read from data register (Address : 0x03)

                data[1] = (byte)RoBoIO.i2c0master_ReadN();//X MSB 
                data[0] = (byte)RoBoIO.i2c0master_ReadN();//X LSB 
                _out_value[0] = System.BitConverter.ToInt16(data, 0);
                data[1] = (byte)RoBoIO.i2c0master_ReadN();//X MSB 
                data[0] = (byte)RoBoIO.i2c0master_ReadN();//X LSB 
                _out_value[1] = System.BitConverter.ToInt16(data, 0);
                data[1] = (byte)RoBoIO.i2c0master_ReadN();//X MSB 
                data[0] = (byte)RoBoIO.i2c0master_ReadN();//X LSB 
                _out_value[2] = System.BitConverter.ToInt16(data, 0);
            }
            return _out_value;
        }

        public static string[] out_title
        {
            get { return _out_title; }
        }
    }
}