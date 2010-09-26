using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Server
{
    public static class XBox360
    {
        //Module Name
        private static string Module = "XBox360.cs";
        private static Logging Log = new Logging();
        private static bool _connected = false;

        private static GamePadState controllerState;
        private static PlayerIndex playerIndex = PlayerIndex.One; // Keeps track of the current controller;
        private static int controllerButton = 65535;
        private static int controllerThumbsticksX1 = 50;
        private static int controllerThumbsticksY1 = 50;
        private static int controllerThumbsticksX2 = 50;
        private static int controllerThumbsticksY2 = 50;
        private static int controllerTriggerLeft = 0;
        private static int controllerTriggerRight = 0;

        public static bool Init()
        {
            Log.Module = Module;
            if (!_connected)
            {
                controllerState = GamePad.GetState(playerIndex);
                if (controllerState.IsConnected)
                {
                    _connected = true;
                    Log.WriteLineSucces("Opening: XBox360 lib");
                    return true;
                }
                _connected = false;
                Log.WriteLineFail("Opening: XBox360 lib");
                return false;
            }
            Log.WriteLineMessage("Opening: XBox360 lib...already open");
            return false;
        }

        public static bool Close()
        {
            if (_connected)
            {
                _connected = false;
                Log.WriteLineSucces("Closing: XBox360 lib");
                return true;
            }
            else
            {
                Log.WriteLineFail("Closing: XBox360 lib");
                return false;
            }
        }

        // Property
        //
        public static int Buttons
        {
            get
            {
                return controllerButton;
            }
        }

        // Property
        //
        public static int ThumbsticksX1
        {
            get
            {
                return controllerThumbsticksX1;
            }
        }

        // Property
        //
        public static int ThumbsticksY1
        {
            get
            {
                return controllerThumbsticksY1;
            }
        }

        // Property
        //
        public static int ThumbsticksX2
        {
            get
            {
                return controllerThumbsticksX2;
            }
        }

        // Property
        //
        public static int ThumbsticksY2
        {
            get
            {
                return controllerThumbsticksY2;
            }
        }

        // Property
        //
        public static int TriggerLeft
        {
            get
            {
                return controllerTriggerLeft;
            }
        }

        // Property
        //
        public static int TriggerRight
        {
            get
            {
                return controllerTriggerRight;
            }
        }

        // Method
        //
        public static void ControllerState()
        {
            controllerState = GamePad.GetState(playerIndex);

            if (controllerState.IsConnected)
            {
                if (!_connected)
                    Init();

                controllerButton = 0;

                if (controllerState.Buttons.A == ButtonState.Pressed)
                    controllerButton = 1;
                if (controllerState.Buttons.B == ButtonState.Pressed)
                    controllerButton += 2;
                if (controllerState.Buttons.X == ButtonState.Pressed)
                    controllerButton += 4;
                if (controllerState.Buttons.Y == ButtonState.Pressed)
                    controllerButton += 8;
                if (controllerState.Buttons.LeftShoulder == ButtonState.Pressed)
                    controllerButton += 16;
                if (controllerState.Buttons.RightShoulder == ButtonState.Pressed)
                    controllerButton += 32;
                if (controllerState.Buttons.Start == ButtonState.Pressed)
                    controllerButton += 64;
                if (controllerState.Buttons.Back == ButtonState.Pressed)
                    controllerButton += 128;
                if (controllerState.Buttons.LeftStick == ButtonState.Pressed)
                    controllerButton += 256;
                if (controllerState.Buttons.RightStick == ButtonState.Pressed)
                    controllerButton += 512;

                if (controllerState.DPad.Up == ButtonState.Pressed)
                    controllerButton += 1024;
                if (controllerState.DPad.Down == ButtonState.Pressed)
                    controllerButton += 2048;
                if (controllerState.DPad.Left == ButtonState.Pressed)
                    controllerButton += 4096;
                if (controllerState.DPad.Right == ButtonState.Pressed)
                    controllerButton += 8192;

                if (controllerButton == 0)
                    controllerButton = 65535;

                controllerThumbsticksX1 = (int)((controllerState.ThumbSticks.Left.X + 1.0f) * 100.0f / 2.0f);
                controllerThumbsticksY1 = (int)((controllerState.ThumbSticks.Left.Y + 1.0f) * 100.0f / 2.0f);

                controllerThumbsticksX2 = (int)((controllerState.ThumbSticks.Right.X + 1.0f) * 100.0f / 2.0f);
                controllerThumbsticksY2 = (int)((controllerState.ThumbSticks.Right.Y + 1.0f) * 100.0f / 2.0f);

                controllerTriggerLeft = (int)(controllerState.Triggers.Left * 100);
                controllerTriggerRight = (int)(controllerState.Triggers.Right * 100);
            }
            else
            {
                _connected = false;
                controllerButton = 65535;
                controllerThumbsticksX1 = 50;
                controllerThumbsticksY1 = 50;
                controllerThumbsticksX2 = 50;
                controllerThumbsticksY2 = 50;
                controllerTriggerLeft = 0;
                controllerTriggerRight = 0;
            }
        }

        // Property
        //
        public static bool Open
        {
            get { return _connected; }
        }
    }
}
