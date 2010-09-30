using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using RoBoIO_DotNet;
using Ini;

namespace Server
{
    class Program
    {
        private static Logging Log = new Logging();
        private static string Module = "Program.cs";
        private static bool infinteLoop = true;
        private static string robotFolder;
        private static string motionsFolder;
        private static string audioFolder;
        private static string[] strChannelData = new string[StaticUtilities.numberOfServos];
        private static IniFile khr_1hv_motion;
        private static int Items, Links;
        private static string Name;
        private static string Ctrl;
        private static string fileName;
        private static int selectedMotionIndex;
        private static bool networkBusy;

        static void Main(string[] args)
        {
            Log.Module = Module;

            robotFolder = Path.GetDirectoryName(Application.ExecutablePath);
            motionsFolder = string.Format("{0}\\Motions", robotFolder);
            audioFolder = string.Format("{0}\\Audio", robotFolder);

            networkBusy = false; // There is no network traffic between server and client.

            bool newStart = true; // kan misschien weg.
            Log.WriteLineMessage("==================================");
            Log.WriteLineMessage("Roboard KHR-1HV Server Application");
            Log.WriteLineMessage("==================================");

            Network mainServer = new Network();

            Log.WriteLineMessage("Checking folder structure");
            // Does the Motions folder exists?
            if (!Directory.Exists(motionsFolder))
            {
                Log.WriteLineFail("Motions folder exist");
                // If not lets create one.
                Directory.CreateDirectory(motionsFolder);
                Log.WriteLineSucces("Motions folder created");
                // Delete the Table.ini since all the motions are gone,
                // lets create a new Table.ini
                File.Delete(string.Format("{0}\\Table.ini",robotFolder));
                Log.WriteLineSucces("Deleting existing Table.ini");
            }
            else
                Log.WriteLineSucces("Motions folder exist");

            // Does the Audio folder exists?
            if (!Directory.Exists(audioFolder))
            {
                Log.WriteLineFail("Audio folder exist");
                // If not lets create one
                Directory.CreateDirectory(audioFolder);
                Log.WriteLineSucces("Audio folder created");
            }
            else
                Log.WriteLineSucces("Audio folder exist");

            if (!Server.MainIni.Open)
                Server.MainIni.Init();

            if (!Server.Table.Open)
                Server.Table.Init();

            if (!Server.Trim.Open)
                Server.Trim.Init();

            if (MainIni.EnableRemoteControl)
                if (!Server.XBox360.Open)
                    Server.XBox360.Init();

//            RCServo.Close();
            RCServo.Init();
            Server.I2C.Init();
            Server.SPI.Init();
            Server.AD7918.Init();

            Server.MotionInterpreter.Init();

            // Check if start-up motion is set
            // and play that motion
            //
            if (Server.MainIni.PowerUpMotion != -1)
            {
                Log.WriteLineMessage(string.Format("Play startup motion: {0}", Server.MainIni.PowerUpMotion));
            }
            // remember that the voltage needs to be divided by 10;

            Listen(mainServer); // function call to start listening for connections.

            while (newStart)
            {
                //Network.messageHandler += new Network.NewMessageEventHandler(mainServer_messageHandler);
                infinteLoop = true;
                while (infinteLoop)
                {
                    // networkbusy werkt niet meer door de threading. Dit moet opgelost worden door 
                    // 1-of de networkbusy ook te laten afhangen van playingdone.
                    // in iedergeval moet er in de motioninterpreter een property komen die laat zien dat
                    // de motion playing is of niet.
                    // 2-of in de xbox360controller een stop en start.
                    if (!networkBusy)
                    {
                        // for real time mixing, read the sensor and do the calculations
                        // and apply them to the current servo position with mixwidth as sensorvalue.

                        Server.XBox360.ControllerState();
                        if (Server.XBox360.Open)
                        {
                            //if (Server.XBox360.Buttons == 128)
                            //{
                            //    Server.MotionInterpreter.playing = false;
                            //    //newStart = false;
                            //    break;
                            //}
                            if (Server.XBox360.Buttons == 64)
                            {
                                if (RCServo.Connected)
                                    RCServo.Close();
                                else
                                    RCServo.Init();
                                Thread.Sleep(1000);
                            }
                            for (int i = 0; i < StaticUtilities.numberOfMotions; i++)
                            {
                                // check if the button pressed is linked to a motion in the table and
                                // that the controller state is not 65535 (no motion).
                                if ((Convert.ToInt32(Table.MotionTable["Motion" + (i + 1).ToString()]["Control"]) == Server.XBox360.Buttons) && (Server.XBox360.Buttons != 65535))
                                {
                                    Log.WriteLineMessage(string.Format("Playing motion: {0}, {1}", Table.MotionTable["Motion" + (i + 1).ToString()]["Name"], Table.MotionTable["Motion" + (i + 1).ToString()]["Control"]));
                                    Server.MotionInterpreter.Filename = Server.Table.MotionTable["Motion" + (i + 1).ToString()]["Filename"];
                                    Server.MotionInterpreter.Play();
                                    break;
                                }
                            } // END For
                        } // END If
                    } // END If
                } // END While
            } // END While

            AD7918.Close();
            SPI.Close();
            I2C.Close();
            RCServo.Close();
            mainServer.StopListening();
            Log.WriteLineMessage("Exit");
            Application.Exit();
        }

        //
        //
        public static void mainServer_messageHandler(object sender, NewMessageEventsArgs e)
        {
            networkBusy = true;
            Log.WriteLineMessage(e.NewMessage);
            string sendMsg = string.Empty;
            string[] message;
            string[] Msg;

            message = e.NewMessage.Split(','); // first index contains the command
            // Make a command parser.
            //
            switch(message[0])
            {
                // 
                // Main Menu
                case "Information":
                    sendMsg = string.Empty;
                    sendMsg = string.Format("{0},{1},{2},{3},{4},{5}",
                        RCServo.CPU(), RCServo.Version(), Convert.ToString(RCServo.Connected),
                        Convert.ToString(I2C.Connected),
                        Convert.ToString(AD7918.Connected),
                        Convert.ToString(SPI.Connected));
                    break;
                case "Options":
                    switch (message[1])
                    {
                        case "Read":
                            string tmp = string.Empty;
                            sendMsg = Server.MainIni.MotionReplay.ToString() + "," +
                                      Server.MainIni.EnableRemoteControl.ToString() + "," +
                                      Server.MainIni.PowerUpMotion + "," +
                                      Server.MainIni.LowPowerMotion + "," +
                                      Server.MainIni.LowPowerVoltage.ToString() + "," +
                                      Server.MainIni.TimeBase.ToString();
                            for (int i = 0; i < StaticUtilities.numberOfServos; i++)
                            {
                                tmp += string.Format(",{0}", Server.MainIni.ChannelFunction[i]);
                            }
                            sendMsg = sendMsg + tmp;
                            break;
                        case "Write":
                            Server.MainIni.MotionReplay = Convert.ToBoolean(message[2]);
                            Server.MainIni.EnableRemoteControl = Convert.ToBoolean(message[3]);
                            Server.MainIni.PowerUpMotion = Convert.ToInt32(message[4]);
                            Server.MainIni.LowPowerMotion = Convert.ToInt32(message[5]);
                            Server.MainIni.LowPowerVoltage = Convert.ToInt32(message[6]);
                            Server.MainIni.TimeBase = Convert.ToInt32(message[7]);
                            int[] temp = new int[StaticUtilities.numberOfServos];
                            for (int i = 0; i < StaticUtilities.numberOfServos; i++)
                            {
                                temp[i] = Convert.ToInt32(message[8 + i]);
                            }
                            Server.MainIni.ChannelFunction = temp;
                            Server.MainIni.Save();
                            Server.RCServo.Close();
                            Server.RCServo.Init();

                            if (MainIni.EnableRemoteControl)
                                if (!Server.XBox360.Open)
                                    Server.XBox360.Init();

                            sendMsg = "Ok";
                            break;
                        default:
                            break;
                    }
                    break;
                case "ReadMotionFile":
                    switch(message[1])
                    {
                        case "Open":
                            // Get the selected motion index and check
                            // in the motiontable if the motion exists
                            // return true if the file exist.
                            selectedMotionIndex = Convert.ToInt32(message[2]) + 1;
                            string file = Server.Table.MotionTable["Motion" + selectedMotionIndex.ToString()]["Filename"];
                            if (file != string.Empty)
                            {
                                khr_1hv_motion = new IniFile(file);
                                khr_1hv_motion.Load();
                                sendMsg = "Ok";
                            }
                            else
                            {
                                sendMsg = "No motion to read";
                            }
                            break;
                        case "GraphicalEdit":
                            Msg = new string[8];
                            Msg[0] = khr_1hv_motion["GraphicalEdit"]["Type"];
                            Msg[1] = khr_1hv_motion["GraphicalEdit"]["Width"];
                            Msg[2] = khr_1hv_motion["GraphicalEdit"]["Height"];
                            Msg[3] = khr_1hv_motion["GraphicalEdit"]["Items"];
                            Msg[4] = khr_1hv_motion["GraphicalEdit"]["Links"];
                            Msg[5] = khr_1hv_motion["GraphicalEdit"]["Start"];
                            Msg[6] = khr_1hv_motion["GraphicalEdit"]["Name"];
                            Msg[7] = khr_1hv_motion["GraphicalEdit"]["Ctrl"];
                            Items = 0;
                            Links = 0;
                            sendMsg = string.Join(",", Msg);
                            break;
                        case "Item":
                            Msg = new string[8];
                            string strItems = string.Format("Item{0}", Items++);
                            Msg[0] = khr_1hv_motion[strItems]["Name"];
                            Msg[1] = khr_1hv_motion[strItems]["Width"];
                            Msg[2] = khr_1hv_motion[strItems]["Height"];
                            Msg[3] = khr_1hv_motion[strItems]["Left"];
                            Msg[4] = khr_1hv_motion[strItems]["Top"];
                            Msg[5] = khr_1hv_motion[strItems]["Color"];
                            Msg[6] = khr_1hv_motion[strItems]["Type"];
                            Msg[7] = khr_1hv_motion[strItems]["Prm"];
                            sendMsg = string.Join(",", Msg);
                            break;
                        case "Link":
                            Msg = new string[4];
                            string strLinks = string.Format("Link{0}", Links++);
                            Msg[0] = khr_1hv_motion[strLinks]["Main"];
                            Msg[1] = khr_1hv_motion[strLinks]["Origin"];
                            Msg[2] = khr_1hv_motion[strLinks]["Final"];
                            Msg[3] = khr_1hv_motion[strLinks]["Point"];
                            sendMsg = string.Join(",", Msg);
                            break;
                        default:
                            break;
                    }
                    break;
                case "WriteMotionFile":
                    switch (message[1])
                    {
                        case "Open":
                            // create random filename.
                            fileName = Path.ChangeExtension(Path.GetRandomFileName(), "RMF");
                            fileName = string.Format("{0}\\{1}", motionsFolder, fileName);
                            // get the index for the datatable where the
                            // motion is going to be writen.
                            selectedMotionIndex = Convert.ToInt32(message[2]) + 1;
                            khr_1hv_motion = new IniFile(fileName);
                            sendMsg = "Ok";
                            break;
                        case "GraphicalEdit":
                            IniSection section1 = new IniSection();
                            section1.Add("Type", message[2]);
                            section1.Add("Width", message[3]);
                            section1.Add("Height", message[4]);
                            section1.Add("Items", message[5]);
                            section1.Add("Links", message[6]);
                            section1.Add("Start", message[7]);
                            section1.Add("Name", message[8]);
                            section1.Add("Ctrl", message[9]);
                            khr_1hv_motion.Add("GraphicalEdit", section1);
                            Items = 0;
                            Links = 0;
                            sendMsg = "Ok";
                            break;
                        case "Item":
                            IniSection section2 = new IniSection();
                            section2.Add("Name", message[2]);
                            section2.Add("Width", message[3]);
                            section2.Add("Height", message[4]);
                            section2.Add("Left", message[5]);
                            section2.Add("Top", message[6]);
                            section2.Add("Color", message[7]);
                            section2.Add("Type", message[8]);
                            string strChannel = string.Format("{0}", message[9]);
                            for (int i = 10; i < message.Length; i++)
                            {
                                strChannel = string.Format("{0},{1}", strChannel, message[i]);
                            }
                            section2.Add("Prm", strChannel);
                            string strItems = string.Format("Item{0}", Items++);
                            khr_1hv_motion.Add(strItems, section2);
                            sendMsg = "Ok";
                            break;
                        case "Link":
                            IniSection section3 = new IniSection();
                            section3.Add("Main", message[2]);
                            section3.Add("Origin", message[3]);
                            section3.Add("Final", message[4]);
                            string strPoint = string.Format("{0}", message[5]);
                            for (int i = 6; i < message.Length; i++)
                            {
                                strPoint = string.Format("{0},{1}", strPoint, message[i]);
                            }
                            section3.Add("Point", strPoint);
                            string strLinks = string.Format("Link{0}", Links++);
                            khr_1hv_motion.Add(strLinks, section3);
                            sendMsg = "Ok";
                            break;
                        case "Save":
                            // get the filename of the motion to be deleted
                            string file = Server.Table.MotionTable["Motion" + selectedMotionIndex.ToString()]["Filename"];
                            if (file != string.Empty)
                            {
                                if (File.Exists(file))
                                {
                                    File.Delete(file);
                                    Log.WriteLineSucces(string.Format("Deleting: {0}", Server.Table.MotionTable["Motion" + selectedMotionIndex.ToString()]["Name"]));
                                }
                                else
                                {
                                    Log.WriteLineFail(string.Format("Deleting: {0}", Server.Table.MotionTable["Motion" + selectedMotionIndex.ToString()]["Name"]));
                                }
                            }
                            Name = khr_1hv_motion["GraphicalEdit"]["Name"];
                            Ctrl = khr_1hv_motion["GraphicalEdit"]["Ctrl"];
                            if (!khr_1hv_motion.Save())
                            {
                                Log.WriteLineError(string.Format("Saving: {0}", Name));
                                sendMsg = "NOk";
                            }
                            else
                            {
                                Log.WriteLineSucces(string.Format("Saving: {0}", Name));
                                Server.Table.addNewMotion(selectedMotionIndex, fileName, Name, Ctrl);
                                Server.Table.Save();
                                sendMsg = "Ok";
                            }
                            break;
                    }
                    break;
                case "PlayMotionFile":
                    int strMotion = (int.Parse(message[1]));
                    Log.WriteLineMessage(string.Format("Playing motion: {0}, {1}", Table.MotionTable["Motion" + (strMotion + 1).ToString()]["Name"], Table.MotionTable["Motion" + (strMotion + 1).ToString()]["Control"]));
                    Server.MotionInterpreter.Filename = Server.Table.MotionTable["Motion" + (strMotion + 1).ToString()]["Filename"];
                    Server.MotionInterpreter.Play();
                    //sendMsg = "Ok"; // No need for this.
                    break;
                case "StopMotionFile":
                    Server.MotionInterpreter.Stop();
                    break;
                case "PauseMotionFile":
                    Server.MotionInterpreter.Pause();
                    break;
                case "DataTable":
                    switch (message[1])
                    {
                        case "Open":
                            sendMsg = string.Format("Open,{0},{1}", StaticUtilities.numberOfMotions, StaticUtilities.numberOfDataTableItems);
                            break;
                        case "Get":
                            string[] strTable = new string[StaticUtilities.numberOfDataTableItems];
                            string tmp = string.Format("{0}{1}", StaticUtilities.DataTableMotion, message[2]);
                            strTable[0] = Server.Table.MotionTable[tmp][StaticUtilities.DataTableMotion];
                            strTable[1] = Server.Table.MotionTable[tmp][StaticUtilities.DataTableName];
                            strTable[2] = Server.Table.MotionTable[tmp][StaticUtilities.DataTableCount];
                            strTable[3] = Server.Table.MotionTable[tmp][StaticUtilities.DataTableDate];
                            strTable[4] = Server.Table.MotionTable[tmp][StaticUtilities.DataTableCtrl];
                            sendMsg = string.Format("Get,{0}", string.Join(",", strTable));
                            break;
                        default:
                            break;
                    }
                    break;
                case "DeleteMotionFile":
                    int strMotion1 = int.Parse(message[1]) + 1;
                    string file1 = Server.Table.MotionTable["Motion" + strMotion1.ToString()]["Filename"];
                    Server.Table.deleteMotion(strMotion1);
                    Server.Table.Save();
                    if (file1 != string.Empty)
                    {
                        if (File.Exists(file1))
                        {
                            File.Delete(file1);
                            Log.WriteLineSucces(string.Format("Deleting: {0}", file1));
                        }
                        else
                        {
                            Log.WriteLineFail(string.Format("Deleting: {0}", file1));
                        }
                    }
                    else
                    {
                        Log.WriteLineFail("Deleting: No such file");
                    }
                    // send something back as conformation.
                    sendMsg = "Ok";
                    break;
                case "Servos":
                    switch (message[1])
                    {
                        case "Set":
                            int[] width = new int[StaticUtilities.numberOfServos];
                            Array.Copy(message, 2, strChannelData, 0, 24);
                            for (int q = 0; q < StaticUtilities.numberOfServos; q++)
                            {
                                width[q] = Convert.ToInt32(strChannelData[q]);
                            }
                            Server.MotionInterpreter.Servos(width, 100);
                            break;
                        default:
                            break;
                    }
                    break;
                case "Trim":
                    switch (message[1])
                    {
                        case "Get":
                            sendMsg = Server.Trim.Data;
                            // Set the servo's position to the trim values
                            MotionInterpreter.Trim();
                            break;
                        case "Set":
                            Array.Copy(message, 2, strChannelData, 0, 24);
                            Server.Trim.Data = string.Join(",", strChannelData);
                            sendMsg = "Ok";
                            // misschien als check de trim.data weer terugsturen.
                            MotionInterpreter.Trim();
                            break;
                        case "Stop":
                            Server.Trim.Save();
                            sendMsg = "Ok";
                            break;
                        default:
                            break;
                    }
                    break;
                case "Close":
                case "close":
                    infinteLoop = false;
                    sendMsg = "No Connection";
                    break;
                case "Open":
                    sendMsg = "Connected";
                    break;
                case "XBox360Controller":
                    switch (message[1])
                    {
                        case "Open":
                            if (Server.XBox360.Open)
                                Server.XBox360.ControllerState();
                            sendMsg = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                                Server.XBox360.Open,
                                Server.XBox360.Buttons,
                                Server.XBox360.ThumbsticksX1, Server.XBox360.ThumbsticksY1,
                                Server.XBox360.ThumbsticksX2, Server.XBox360.ThumbsticksY2,
                                Server.XBox360.TriggerLeft, Server.XBox360.TriggerRight,
                                MainIni.PA1Reference, MainIni.PA2Reference, MainIni.PA3Reference, MainIni.PA4Reference,
                                MainIni.PA5Reference, MainIni.PA6Reference);
                            break;
                        case "Read":
                            Server.XBox360.ControllerState();
                            sendMsg = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                                Server.XBox360.Open,
                                Server.XBox360.Buttons,
                                Server.XBox360.ThumbsticksX1, Server.XBox360.ThumbsticksY1,
                                Server.XBox360.ThumbsticksX2, Server.XBox360.ThumbsticksY2,
                                Server.XBox360.TriggerLeft, Server.XBox360.TriggerRight);
                            break;
                        case "Stop":
                            MainIni.PA1Reference = Convert.ToInt32(message[2]);
                            MainIni.PA2Reference = Convert.ToInt32(message[3]);
                            MainIni.PA3Reference = Convert.ToInt32(message[4]);
                            MainIni.PA4Reference = Convert.ToInt32(message[5]);
                            MainIni.PA5Reference = Convert.ToInt32(message[6]);
                            MainIni.PA6Reference = Convert.ToInt32(message[7]);
                            MainIni.Save();
                            //sendMsg = "Ok";
                            break;
                        default:
                            break;
                    }
                    break;
                case "MagnetoData":
                    short[] compass = Compass.CompassData();
                    sendMsg = "MagnetoData," +
                              compass[0].ToString() + "," +
                              compass[1].ToString() + "," +
                              compass[2].ToString();
                    break;
                case "AcceleroData":
                    short[] accelero = Accelero.AcceleroData();
                    sendMsg = "AcceleroData," +
                              accelero[0].ToString() + "," +
                              accelero[1].ToString() + "," +
                              accelero[2].ToString();
                    break;
                case "GyroscopeData":
                    short[] gyro = Gyro.GyroData();
                    sendMsg = "GyroscopeData," +
                              gyro[0].ToString() + "," +
                              gyro[1].ToString() + "," +
                              gyro[2].ToString() + "," +
                              gyro[3].ToString() + "," +
                              gyro[4].ToString() + "," +
                              gyro[5].ToString() + "," +
                              gyro[6].ToString() + "," +
                              gyro[7].ToString();
                    break;
                default:
                    break;
            }
            if (sendMsg != string.Empty)
                Network.SendMessage(sendMsg);
            networkBusy = false;
        }

        static void Listen(Network mainServer)
        {
            // Start listening for connections
            mainServer.StartListening();
            // Show that we started to listen for connections
            Log.WriteLineMessage("Monitoring for connections...");
            Network.messageHandler += new Network.NewMessageEventHandler(mainServer_messageHandler);
        }
    }
}
