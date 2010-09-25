using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ini;

namespace Server
{
    public static class Table
    {
        private static Logging Log = new Logging();
        public static IniFile MotionTable;
        private static bool _connected = false;
        private static string Filename = "Table.ini";
        private static string Module = "Table.cs";

        // Method
        //
        public static bool Init()
        {
            string applicationFolder = Path.GetDirectoryName(Application.ExecutablePath);
            MotionTable = new IniFile(string.Format("{0}\\{1}", applicationFolder, Filename));
            Log.Module = Module;
            if (MotionTable.Exists())
            {
                Log.WriteLineSucces(string.Format("Opening: {0}", Filename));
                if (MotionTable.Load())
                {
                    _connected = true;
                    Log.WriteLineSucces(string.Format("Loading: {0}", Filename));
                    return true;
                }
                Log.WriteLineFail(string.Format("Loading: {0}", Filename));
                _connected = CreateTable();
                return _connected;
            }
            else
            {
                Log.WriteLineFail(string.Format("Opening: {0}", Filename));
                _connected = CreateTable();
                return _connected;
            }
        }

        // Method
        //
        private static bool CreateTable()
        {
            for (int i = 0; i < StaticUtilities.numberOfMotions; i++)
            {
                IniSection section = new IniSection();
                section.Add("Motion", string.Format("M{0}", (i + 1)));
                section.Add("Filename", string.Empty);
                section.Add("Name", string.Empty);
                section.Add("Count", string.Format("{0}", 0));
                section.Add("Date", "--/--/---- --:--");
                section.Add("Control", string.Format("{0}", 65535));
                MotionTable.Add("Motion" + (i + 1).ToString(), section);
            }
            Log.WriteLineSucces(string.Format("Creating: {0}", Filename));
            return Save();
        }

        // Method
        //
        public static bool Save()
        {
            if (MotionTable.Save())
            {
                Log.WriteLineSucces(string.Format("Saving: {0}", Filename));
                return true;
            }
            Log.WriteLineFail(string.Format("Saving: {0}", Filename));
            return false;
        }

        // Method
        //
        private static bool baseMotion(int motionNumber, string fileName, string name, int count, string currentDateTime, string control)
        {
            string motion = string.Format("Motion{0}", motionNumber);

            MotionTable[motion]["Motion"] = string.Format("M{0}", motionNumber);
            MotionTable[motion]["Filename"] = fileName;
            MotionTable[motion]["Name"] = name;
            MotionTable[motion]["Count"] = string.Format("{0}", count);
            MotionTable[motion]["Date"] = currentDateTime;
            MotionTable[motion]["Control"] = control;

            return true;
        }

        // add new motion to the tableFile
        //
        public static bool addNewMotion(int motionNumber, string fileName, string name, string control)
        {
            string currentDate = DateTime.Now.ToString("dd/MM/yyyy");
            string currentTime = DateTime.Now.ToString("t");

            if (baseMotion(motionNumber, fileName, name, 0, currentDate + " " + currentTime, control))
            {
                Log.WriteLineSucces(string.Format("Adding a new motion to: {0}", Filename));
                return true;
            }
            Log.WriteLineFail(string.Format("Adding a new motion to: {0}", Filename));
            return false;
        }

        // delete existing motion from the tableFile
        //
        public static bool deleteMotion(int motionNumber)
        {
            string motion = string.Format("Motion{0}", motionNumber);
            string fileName = MotionTable[motion]["Filename"];

            if (baseMotion(motionNumber, string.Empty, string.Empty, 0, "--/--/---- --:--", string.Format("{0}", 65535)))
            {
                Log.WriteLineSucces(string.Format("Deleting an existing motion from: {0}", Filename));
                return true;
            }
            Log.WriteLineFail(string.Format("Deleting an existing motion from: {0}", Filename));
            return false;
        }

        // Property
        //
        public static bool Open
        {
            get
            {
                return _connected;
            }
        }
    }
}
