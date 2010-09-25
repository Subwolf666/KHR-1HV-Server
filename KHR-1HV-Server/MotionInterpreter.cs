using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Ini;

namespace Server
{
    public static class MotionInterpreter
    {
        private static Logging Log = new Logging();
        private static string Module = "MotionInterpreter.cs";
        private static IniFile KHR_1HV_Motion;
        private static string FileName = string.Empty;
        private static int Items;
        private static int Links;
        private static int Start;
        public static bool playing = true;
        public static bool pausing = false;

        // Method
        //
        public static void Init()
        {
            Log.Module = Module;
        }

        // Method
        //
        public static void Servos(int[] width, int playtime)
        {
            uint[] _width = new uint[StaticUtilities.numberOfServos];

            for (int i = 0; i < StaticUtilities.numberOfServos; i++)
            {
                int p = Server.Trim.iData[i] + width[i] + StaticUtilities.servoNull;
                if (p <= StaticUtilities.servoMin)
                    p = StaticUtilities.servoMin;
                if (p >= StaticUtilities.servoMax)
                    p = StaticUtilities.servoMax;

                _width[i] = Convert.ToUInt32(p);
            }
            RoBoIO_DotNet.RoBoIO.rcservo_SetAction(_width, Convert.ToUInt32(playtime));
            while (RoBoIO_DotNet.RoBoIO.rcservo_PlayAction() != RoBoIO_DotNet.RoBoIO.RCSERVO_PLAYEND) 
            {
            }
        }

        // Method
        //
        public static void Trim()
        {
            uint playtime = 100; // trim playtime
            uint[] _width = new uint[24];

            for (int i = 0; i < StaticUtilities.numberOfServos; i++)
            {
                _width[i] = Convert.ToUInt32(Server.Trim.iData[i] + StaticUtilities.servoNull);
            }

            RoBoIO_DotNet.RoBoIO.rcservo_SetAction(_width, playtime);
            while (RoBoIO_DotNet.RoBoIO.rcservo_PlayAction() != RoBoIO_DotNet.RoBoIO.RCSERVO_PLAYEND)
            {
            }
        }

        public static bool Play()
        {
            KHR_1HV_Motion = new IniFile(FileName);
            if (!Load())
                return false;

            Items = Convert.ToInt32(KHR_1HV_Motion[StaticUtilities.SectionGraphicalEdit][StaticUtilities.GraphicalEditItems]);
            Links = Convert.ToInt32(KHR_1HV_Motion[StaticUtilities.SectionGraphicalEdit][StaticUtilities.GraphicalEditLinks]);
            Start = Convert.ToInt32(KHR_1HV_Motion[StaticUtilities.SectionGraphicalEdit][StaticUtilities.GraphicalEditStart]);

            Interpreter();

            return true;
        }

        public static bool Stop()
        {
            RoBoIO_DotNet.RoBoIO.rcservo_StopAction();
            playing = false;
            return true;
        }

        public static bool Pause()
        {
            if (pausing)
            {
                RoBoIO_DotNet.RoBoIO.rcservo_PauseAction();
                pausing = true;
            }
            else
            {
                RoBoIO_DotNet.RoBoIO.rcservo_ReleaseAction();
                pausing = false;
            }
            return true;
        }

        // Method
        //
        private static void Interpreter()
        {
            string linknumber = string.Empty;
            string Item = string.Empty;
            string ItemName = string.Empty;
            int ItemType = 0;
            int CmpPrmValue = 0;
            int iLoopCounter = 1;
            string[] saParameter; // var used for the cmp channel
            int[] iParameter = new int[StaticUtilities.numberOfServos];
            int[] iServoChannel = new int[StaticUtilities.numberOfServos];
            uint[] uiMixWidth = new UInt32[StaticUtilities.numberOfServos];
            int iPlaytime = 1;
            int iComparisonRegister = 0;
            int cmpMain = 0;

            playing = true;

            int originnumber = Start;

            while (playing)  
            {
                if (!pausing)
                {
                    // The first item is de Item that Start points to
                    Item = string.Format("Item{0}", originnumber);
                    ItemName = KHR_1HV_Motion[Item][StaticUtilities.ItemName];
                    Log.WriteLineMessage(string.Format("Play Item{0}, Name: {1}", originnumber, ItemName));

                    ItemType = Convert.ToInt32(KHR_1HV_Motion[Item][StaticUtilities.ItemType]);
                    saParameter = KHR_1HV_Motion[Item][StaticUtilities.ItemPrm].Split(',');
                    iPlaytime = Convert.ToInt32(saParameter[0]);

                    cmpMain = 0; // Needed for the jump to link

                    switch (ItemType)
                    {
                        case 0:     // POS
                            for (int ii = 0; ii < StaticUtilities.numberOfServos; ii++)
                            {
                                // convert the current string array Prm to an integer Array
                                iParameter[ii] = Convert.ToInt32(saParameter[ii + 1]);

                                // 1 - 0 <=> Doesn't do anything
                                if (iParameter[ii] == 0)
                                {
                                    // Set the mixwidth to off until something else 
                                    uiMixWidth[ii] = 0; //RoBoIO_DotNet.RoBoIO.RCSERVO_MIXWIDTH_POWEROFF;
                                    iServoChannel[ii] = 0;
                                }
                                // 2 - 1 ~ 16384 ~ 32767 <=> Servo relative angle (Range -16383 ~ 0 ~ +16384)
                                else if ((iParameter[ii] >= 1) && (iParameter[ii] <= 32767))
                                {
                                    uiMixWidth[ii] = 0;
                                    iServoChannel[ii] = iParameter[ii] - 16384;
                                }
                                // 3 - 32768 <=> TTL Output(L)
                                else if (iParameter[ii] == 32768)
                                {
                                    uiMixWidth[ii] = RoBoIO_DotNet.RoBoIO.RCSERVO_MIXWIDTH_POWEROFF;
                                    iServoChannel[ii] = 0;
                                }
                                // 4 - 32769 <=> TTL Output (H)
                                else if (iParameter[ii] == 32770)
                                {
                                    uiMixWidth[ii] = RoBoIO_DotNet.RoBoIO.RCSERVO_MIXWIDTH_POWEROFF;
                                    iServoChannel[ii] = 0;
                                }
                                // 5 - 32770 <=> Free
                                else if (iParameter[ii] == 32770)
                                {
                                    uiMixWidth[ii] = RoBoIO_DotNet.RoBoIO.RCSERVO_MIXWIDTH_POWEROFF;
                                    iServoChannel[ii] = 0;
                                }
                                // 6 - 32771 <=> SET1(PWM only)
                                else if (iParameter[ii] == 32771)
                                {
                                    uiMixWidth[ii] = RoBoIO_DotNet.RoBoIO.RCSERVO_MIXWIDTH_POWEROFF;
                                    iServoChannel[ii] = 0;
                                }
                                // 7 - 32772 <=> SET2(PWM only)
                                else if (iParameter[ii] == 32772)
                                {
                                    uiMixWidth[ii] = RoBoIO_DotNet.RoBoIO.RCSERVO_MIXWIDTH_POWEROFF;
                                    iServoChannel[ii] = 0;
                                }
                                // 8 - 32773 <=> SET3(PWM only)
                                else if (iParameter[ii] == 32773)
                                {
                                    uiMixWidth[ii] = RoBoIO_DotNet.RoBoIO.RCSERVO_MIXWIDTH_POWEROFF;
                                    iServoChannel[ii] = 0;
                                }
                            } // END For loop

                            // Play the motion
                            Servos(iServoChannel, iPlaytime);
                            break;
                        case 1:     // SET
                            // The SET command is usually only 1 Prm, the rest is the same.
                            // That's why there is no for loop here.
                            CmpPrmValue = Convert.ToInt32(saParameter[1]);
                            // 1 - Setup of a Loop counter 0 - 255 <=> 33287 ~ 33542
                            if ((CmpPrmValue >= 33287) && (CmpPrmValue <= 33542))
                            {
                                iLoopCounter = CmpPrmValue - 33287;
                            }
                            // 2 - Setup of the Comparison Register <=> 39942(-1023)~40964(-1), 40965(0)~57348(16383) <=> -1023~16383
                            else if ((CmpPrmValue >= 39942) && (CmpPrmValue <= 57348))
                            {
                                iComparisonRegister = CmpPrmValue - 40965;
                            }
                            // 3 - Calibration of AD1 standard value
                            else if (CmpPrmValue == 65268)
                            {
                                // Perform calibration of the sensor
                            }
                            // 4 - Calibration of AD2 standard value
                            else if (CmpPrmValue == 65269)
                            {
                                // Perform calibration of the sensor
                            }
                            // 5 - Calibration of AD3 standard value
                            else if (CmpPrmValue == 65270)
                            {
                                // Perform calibration of the sensor
                            }
                            // 6 - Jump to scenario
                            else if ((CmpPrmValue >= 57349) && (CmpPrmValue <= 57428))
                            {
                            }
                            // 7 - Call to scenario
                            else if ((CmpPrmValue >= 57434) && (CmpPrmValue <= 57513))
                            {
                            }
                            // 8 - Return from call
                            else if (CmpPrmValue == 57519)
                            {
                            }

                            break;
                        case 2:     // CMP
                            CmpPrmValue = Convert.ToInt32(saParameter[1]);
                            // 1 - Decrement of a loop counter 33543 ~ 33798
                            if ((CmpPrmValue >= 33543) && (CmpPrmValue <= 33978))
                            {
                                if (--iLoopCounter != 0)
                                {
                                    cmpMain = 1; // Find the link with the main key
                                }
                            }
                            // 2 - Comparison of AD1 value with register, if greater then jump
                            //     35846 ~ 36101
                            else if ((CmpPrmValue >= 35846) && (CmpPrmValue <= 36101))
                            {
                                // read a sensor value
                                // which sensor?
                                int sensorvalueAD1 = 100;
                                if (sensorvalueAD1 > iComparisonRegister)
                                {
                                    cmpMain = 1; // Find the link with the main key
                                }
                            }

                            //=====================================================
                            // OMDAT DE VALUE MAX 100 IS KAN ONDERSTAANDE CMP
                            // AANGEPAST WORDEN ZODAT ER OOK RUIMTE IS VOOR DE
                            // LEFT EN RIGHT TRIGGERS.
                            // MAAR DIT MOET EERST IN DE EDITOR GEDAAN WORDEN.
                            //=====================================================
                            // 3 - Comparison of ThumbsticksX1 (PA1) Value with register, if greater then jump
                            //      37382 ~ 37637
                            else if ((CmpPrmValue >= 37382) && (CmpPrmValue <= 37637))
                            {
                                Server.XBox360.ControllerState();
                                if ((Server.XBox360.ThumbsticksX1 - MainIni.PA1Reference) > iComparisonRegister)
                                {
                                    cmpMain = 1;
                                }
                            }
                            // 4 - Comparison of ThumbsticksY1 (PA2) Value with register, if greater then jump
                            else if ((CmpPrmValue >= 37638) && (CmpPrmValue <= 37893))
                            {
                                Server.XBox360.ControllerState();
                                if ((Server.XBox360.ThumbsticksY1 - MainIni.PA2Reference) > iComparisonRegister)
                                {
                                    cmpMain = 1;
                                }
                            }
                            // 5 - Comparison of ThumbsticksY2 (PA3) Value with register, if greater then jump
                            else if ((CmpPrmValue >= 37894) && (CmpPrmValue <= 38149))
                            {
                                Server.XBox360.ControllerState();
                                if ((Server.XBox360.ThumbsticksX2 - MainIni.PA3Reference) > iComparisonRegister)
                                {
                                    cmpMain = 1;
                                }
                            }
                            // 6 - Comparison of ThumbsticksX2 (PA4) Value with register, if greater then jump
                            else if ((CmpPrmValue >= 38150) && (CmpPrmValue <= 38405))
                            {
                                Server.XBox360.ControllerState();
                                if ((Server.XBox360.ThumbsticksY2 - MainIni.PA4Reference) > iComparisonRegister)
                                {
                                    cmpMain = 1;
                                }
                            }
                            // 7 - Comparison of LeftTrigger (PA5) Value with register, if greater then jump
                            // 8 - Comparison of RightTrigger (PA6) Value with register, if greater then jump

                            break;
                        default:
                            break;
                    } // END Switch

                    // if there is a link find it, else stop playing.
                    bool LinkFound = false;
                    // find the next link and check if the main item
                    // is flow wiring or branch wiring
                    for (int j = 0; j < Links; j++)
                    {
                        string linkn = string.Format("{0}{1}", StaticUtilities.SectionLink, j);
                        int linkorg = Convert.ToInt32(KHR_1HV_Motion[linkn][StaticUtilities.LinkOrigin]);
                        int linkmain = Convert.ToInt32(KHR_1HV_Motion[linkn][StaticUtilities.LinkMain]);
                        if ((originnumber == linkorg) && (linkmain == cmpMain))
                        {
                            // make the origin the final
                            originnumber = Convert.ToInt32(KHR_1HV_Motion[linkn][StaticUtilities.LinkFinal]);
                            LinkFound = true;
                            break;
                        }
                    }

                    if (!LinkFound)
                        playing = false;

                    Server.XBox360.ControllerState();
                    if (Server.XBox360.Buttons == 128)
                        playing = false;
                } // END while (!pausing)
            } // END while (playing)
            Log.WriteLineMessage("Playing done");
        }

        // Method
        //
        private static bool Load()
        {
            string message = string.Format("Opening: {0}", FileName);

            // Check if motion exists and if so, load it into KHR_1HV_Motion
            if (KHR_1HV_Motion.Exists())
            {
                if (KHR_1HV_Motion.Load())
                {
                    Log.WriteLineSucces(message);
                    return true;
                }
                Log.WriteLineFail(message);
                return false;
            }
            else
            {
                Log.WriteLineFail(message);
                return false;
            }
        }

        // Property
        //
        public static string Filename
        {
            set { FileName = value; }
        }
    }
}
