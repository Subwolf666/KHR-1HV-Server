using System;
using System.Collections.Generic;
using System.Text;
using RoBoIO_DotNet;

namespace Server
{
    class Accelero
    {
        private static Logging Log = new Logging();
        private static string Module = "Accelero.cs";

        private static byte i2c_address = 0x53; // accelero i2c address

        private static string[] _out_title = new string[] { "X-OUT", "Y-OUT", "Z-OUT" };
     
        public static short[] AcceleroData()
        {
            Log.Module = Module;

            short[] _out_value = new short[] { 0, 0, 0 };
            byte[] data = new byte[2];

            if (Server.I2C.Connected == true)
            {
                RoBoIO.i2c0master_StartN(i2c_address, (byte)RoBoIO.I2C_WRITE, 2);//write 2 byte
                RoBoIO.i2c0master_WriteN(0x2d); //Power_Control register
                RoBoIO.i2c0master_WriteN(0x28); //link and measure mode

                Log.WriteLineMessage("Read 3-axis values of Accelerometer...");

//                RoBoIO.delay_ms(100);
                RoBoIO.delay_ms(50);

                RoBoIO.i2c0master_StartN(i2c_address, (byte)RoBoIO.I2C_WRITE, 2);//write 2 byte
                RoBoIO.i2c0master_WriteN(0x31); //Data_Format register
                RoBoIO.i2c0master_WriteN(0x08); //Full_Resolution

//                RoBoIO.delay_ms(100);
                RoBoIO.delay_ms(50);

                RoBoIO.i2c0master_StartN(i2c_address, (byte)RoBoIO.I2C_WRITE, 2);//write 2 byte
                RoBoIO.i2c0master_WriteN(0x38); //FIFO_Control register
                RoBoIO.i2c0master_WriteN(0x00); //bypass mode

//                RoBoIO.delay_ms(100);
                RoBoIO.delay_ms(50);

                RoBoIO.i2c0master_StartN(i2c_address, (byte)RoBoIO.I2C_WRITE, 1);
                RoBoIO.i2c0master_SetRestartN((byte)RoBoIO.I2C_READ, 6);
                RoBoIO.i2c0master_WriteN(0x32); //Read from X register (Address : 0x32)
                data[0] = (byte)RoBoIO.i2c0master_ReadN();//X LSB 
                data[1] = (byte)RoBoIO.i2c0master_ReadN();//X MSB 
                _out_value[0] = System.BitConverter.ToInt16(data, 0);
                data[0] = (byte)RoBoIO.i2c0master_ReadN();//X LSB 
                data[1] = (byte)RoBoIO.i2c0master_ReadN();//X MSB 
                _out_value[1] = System.BitConverter.ToInt16(data, 0);
                data[0] = (byte)RoBoIO.i2c0master_ReadN();//X LSB 
                data[1] = (byte)RoBoIO.i2c0master_ReadN();//X MSB 
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
	