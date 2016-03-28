using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService.Service
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides notifications to Production Support on the status of the service, and to the console user
    /// </summary>
    public static class MM_Notification
    {
        /// <summary>Our log file</summary>
        public static StreamWriter sLog;

        /// <summary>Our lock object for our display</summary>
        private static object LogLockObject = new object();

        static MM_Notification()
        {
            bool AddHeader = !File.Exists(Properties.Settings.Default.LogFileLocation);
            sLog = new StreamWriter(Properties.Settings.Default.LogFileLocation, true, new UTF8Encoding(false)) { AutoFlush = true };
                 if (AddHeader)
                     sLog.WriteLine("<html><head><style>body {background-color:black}</style><title>Macomber Map Server Log</title></head><body>");
        }

        /// <summary>
        /// Notify Prod support on the status of an event.
        /// </summary>
        /// <param name="SubjectLine"></param>
        /// <param name="Content"></param>
        public static void Notify(String SubjectLine, String Content)
        { }

        #region Console/debugging functions
        /// <summary>
        /// Write out a line to the console, if the console is available.
        /// </summary>
        /// <param name="Color"></param>
        /// <param name="Line"></param>
        /// <param name="Parameters"></param>
        public static void WriteLine(ConsoleColor Color, string Line, params object[] Parameters)
        {
            String OutLine = String.Format(Line, Parameters);
            lock (LogLockObject)
            {
                sLog.WriteLine("<div style=\"color:" + Color.ToString() + "\">" + OutLine + "</div><br/>");
                if (Environment.UserInteractive)
                {
                    Console.ForegroundColor = Color;
                    Console.WriteLine(OutLine);
                }
            }
        }
        #endregion
    }
}
