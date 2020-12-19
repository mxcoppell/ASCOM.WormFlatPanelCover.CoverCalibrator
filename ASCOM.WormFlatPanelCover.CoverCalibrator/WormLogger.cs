using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.CompilerServices;

namespace ASCOM.WormFlatPanelCover
{
    class WormLogger
    {
        public enum LoggerOutput { OFF, CONSOLE }
        public static LoggerOutput LoggerType { get; set; }
        private static StreamWriter LogFileWriter { get; set; }

        static WormLogger()
        {
            LoggerType = LoggerOutput.OFF;
        }
        //~Logger() { LogFileWriter.Close(); }
        public static void Log(string message, bool needHeader = false,
            [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            StringBuilder sb = new StringBuilder();
            if (needHeader)
                sb.Append(("(" + memberName + ":" + sourceLineNumber + ") ").PadLeft(30, ' ') + message);
            else
                sb.Append(message);

            if (LoggerType == LoggerOutput.CONSOLE)
            {
                Console.WriteLine(sb.ToString());
            }
        }

    }
}
