using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class Logging
    {
        private string writeLineMessage = string.Empty;
        private string LevelMsg = string.Empty;
        private SeverityLevel _logLevel = SeverityLevel.INFO;
        private string _Module = string.Empty;

        public enum SeverityLevel
        {
            ERROR,
            WARNING,
            INFO,
            DEBUG
        }

        public Logging()
        {
        }

        // Method
        //
        private void WriteLine(string LogMessage)
        {
            if (StaticUtilities.LOGGING != 0)
            {
                string currentDate = DateTime.Now.ToString("dd/MM/yyyy");
                string currentTime = DateTime.Now.ToString("T");

                switch (_logLevel)
                {
                    case SeverityLevel.ERROR:
                        LevelMsg = "[ERROR]";
                        break;
                    case SeverityLevel.WARNING:
                        LevelMsg = "[WARNING]";
                        break;
                    case SeverityLevel.INFO:
                        LevelMsg = "[INFO]";
                        break;
                    case SeverityLevel.DEBUG:
                        LevelMsg = "[DEBUG]";
                        break;
                    default:
                        LevelMsg = "[NONE]";
                        break;
                }
                // huidige datum en tijd, Level, Module, LogMessage
                writeLineMessage = string.Format("{0} {1} {2}[{3}] => {4}",
                    currentDate, currentTime, LevelMsg, _Module, LogMessage);
                Console.WriteLine(writeLineMessage);
            }
        }

        public void WriteLineMessage(string message)
        {
            _logLevel = SeverityLevel.INFO;
            WriteLine(message);
        }

        public void WriteLineSucces(string message)
        {
            _logLevel = SeverityLevel.INFO;
            WriteLine(string.Format("{0}...succes", message));
        }

        public void WriteLineFail(string message)
        {
            _logLevel = SeverityLevel.WARNING;
            WriteLine(string.Format("{0}...fail", message));
        }

        public void WriteLineError(string message)
        {
            _logLevel = SeverityLevel.ERROR;
            WriteLine(message);
        }
        // Property
        //
        public SeverityLevel LogginLevel
        {
            get { return _logLevel; }
            set { _logLevel = value; }
        }

        // Property
        //
        public string Module
        {
            get { return _Module; }
            set { _Module = value; }
        }
    }
}
