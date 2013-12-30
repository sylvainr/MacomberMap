using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using Macomber_Map.Data_Connections;
using Macomber_Map.User_Interface_Components;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Macomber_Map.Data_Elements;
using System.Text;
using System.Xml;
using System.Data;
using System.Runtime.InteropServices;
using System.Collections;
using Macomber_Map.User_Interface_Components.NetworkMap;

namespace Macomber_Map
{
    /// <summary>
    /// This class handles the application startup and configuration points
    /// </summary>
    static class Program
    {
        /// <summary>Whether to activate the system console</summary>
        public static bool Console = false;

        /// <summary>Our global indicator of when the map started</summary>
        public static bool MapStarted = false;
        
        /// <summary>The error log</summary>
        public static StreamWriter ErrorLog = null;

        /// <summary>This timer checks the presence of the main form every 15 seconds, if the thread isn't running, kill the application</summary>
        public static System.Threading.Timer MainFormChecker;

        /// <summary>A queue of events</summary>
        public static Queue<String> Events = new Queue<string>();

        /// <summary>Our MM instance</summary>
        public static MacomberMap MM;

        /// <summary>The main entry point for the application.</summary>  
        [MTAThread]
        static void Main(params string[] commands)
        {


           //If we have a command, handle it accordingly.
            if (commands.Length > 0 && Array.IndexOf<String>(commands, "/console") != -1)
                Console = ActivateConsole();

            //Initialize the thread and data integration layer
            Thread.CurrentThread.Name = "Main";

            //Set up our error log
            if (commands.Length > 0 && Array.IndexOf<String>(commands, "/console") != -1)
                Console = ActivateConsole();

            if (commands.Length > 0 && Array.IndexOf<String>(commands, "/log") != -1)
                StartLog();

            foreach (String cmd in commands)
                if (cmd.StartsWith("/command=", StringComparison.CurrentCultureIgnoreCase))
                {
                    String Command = cmd.Substring(cmd.IndexOf('=') + 1);
                    if (MM_Pipe.SendCommand(Command))
                        return;
                    else
                        MM_Pipe.CommandToParse = Command;
                }

            //Run the map
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            //Application.Run(new Macomber_Map.User_Interface_Components.OneLines.MM_OneLine_Synchroscope());
            //return;
            //Initialize our data integration layer and CIM repository, and show the startup form                                             
            MM_Repository.Initialize();
            using (Startup_Form Startup = new Startup_Form())
            {
                //Note: Passing staStateEstimatorwork map to showdialog will automatically kick off the new ui process
                if (Startup.ShowDialog(StaStateEstimatorworkMap) == DialogResult.OK)
                {
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(StaStateEstimatorworkMap), new object[] { Startup.Coordinates, Startup});
                    Application.Run();
                }
                else
                    Application.Exit();
            }
        }


       


        /// <summary>
        /// Handle console-based output data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handle error data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Program_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Start logging
        /// </summary>
        public static void StartLog()
        {
            ErrorLog = new StreamWriter("MacomberMap-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".log", false, Encoding.UTF8);
            ErrorLog.AutoFlush = true;
            //Write our header row
            MemoryStatus Stat;
            GlobalMemoryStatus(out Stat);
            ErrorLog.WriteLine("Macobmer Map started at {0} by {1} on {2}. Full path {3}. OS Version {4}. Processors {5}. Current directory {6}. NET version {7}. Current working set {8}. Physical Memory {9:#,##0}/{10:#,##0}. Page Memory {11:#,##0}/{12:#,##0}. Virtual Memory {13:#,##0}/{14:#,##0}. Memory load {15}%", DateTime.Now, Environment.UserDomainName + "\\" + Environment.UserName, Environment.MachineName, Environment.CurrentDirectory, Environment.OSVersion, Environment.ProcessorCount, Environment.CurrentDirectory, Environment.Version, Environment.WorkingSet, Stat.AvailablePhysical, Stat.TotalPhysical, Stat.AvailablePageFile, Stat.TotalPageFile, Stat.AvailableVirtual, Stat.TotalVirtual, Stat.MemoryLoad);
        }

        /// <summary>
        /// Start the network map in its own thread
        /// </summary>
        /// <param name="state"></param>
        private static void StaStateEstimatorworkMap(object state)
        {
            object[] State = (object[])state;
            MM = new MacomberMap(State[0] as MM_Coordinates, State[1] as Startup_Form);
            ApplicationContext MMContext = new ApplicationContext(MM);
            MMContext.ThreadExit += new EventHandler(MMContext_ThreadExit);
            Application.Run(MMContext);
        }

        /// <summary>
        /// Handle the thread exit by shutting everything down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MMContext_ThreadExit(object sender, EventArgs e)
        {
            LogError("Thread exit.");
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// This is the main checking thread to be run every 15 seconds
        /// </summary>
        /// <param name="MainFormThread"></param>
        public static void ThreadChecker(Object MainFormThread)
        {
            if (MainFormThread == null)
                Process.GetCurrentProcess().Kill();
            else if (MainFormThread is Thread == false)
                Process.GetCurrentProcess().Kill();
            else if (!(MainFormThread as Thread).IsAlive)
                Process.GetCurrentProcess().Kill();

        }

        /// <summary>
        /// Log a system error.
        /// </summary>
        /// <param name="ErrorText">The text of the error</param>
        /// <param name="args">The formatting parameters</param>
        public static void LogError(string ErrorText, params object[] args)
        {
            if (args.Length > 0)
                ErrorText = String.Format(ErrorText, args);
            WriteConsoleLine(ErrorText);
            //Helen
            if (Events.Count > 4000)
                Events.Clear();

            Events.Enqueue(ErrorText);
            if (ErrorLog != null)
                try
                {
                    ErrorLog.WriteLine(ErrorText);
                }
                catch (Exception)
                { }
        }


        /// <summary>
        /// If our console is enabled, write text out to it
        /// </summary>
        /// <param name="ConsoleText">The text to write out to the console</param>
        public static void WriteConsoleLine(String ConsoleText)
        {
            if (Program.Console)
            {
                System.Console.WriteLine(ConsoleText);
            }
            else
                Debug.WriteLine(ConsoleText);
        }

        /// <summary>
        /// Log a system error
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        public static void LogError(Exception ex)
        {
            StringBuilder OutMsg = new StringBuilder();
            int CurTab = 0;
            Exception CurEx = ex;
            do
            {
                OutMsg.AppendLine(new string('\t', CurTab) + "Error in " + CurEx.Source + ": " + CurEx.Message);
                String InLine;
                if (!String.IsNullOrEmpty(CurEx.StackTrace))
                    using (StringReader sRd = new StringReader(CurEx.StackTrace))
                        while (!String.IsNullOrEmpty(InLine = sRd.ReadLine()))
                            OutMsg.AppendLine(new string('\t', CurTab) + InLine);
                CurEx = CurEx.InnerException;
            } while (CurEx != null);
            LogError(OutMsg.ToString());
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                if (ErrorLog != null)
                    ErrorLog.Close();
            }
            finally
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogError(e.Exception);
        }

        /// <summary>
        /// Write out a line's information (including points, distacnce, etc.)
        /// </summary>
        private static void WriteLineInformation()
        {
            StreamWriter sW = new StreamWriter("LatLongInformation.txt");
            sW.WriteLine("Latitude/Longitude estimator");
            sW.WriteLine();

            int TotCountyByteSize = 0;
            foreach (PointF pt in MM_Repository.Counties["STATE"].Coordinates)
                TotCountyByteSize += (pt.X.ToString() + "," + pt.Y.ToString() + ",").Length;
            sW.WriteLine("State boundary: Number of points: {0}, Total byte size: {1}", MM_Repository.Counties["STATE"].Coordinates.Length.ToString("#,###,###,###,##0"), TotCountyByteSize.ToString("#,###,###,###,##0"));

            TotCountyByteSize = 0;
            int TotCountyPoints = 0;
            foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
            {
                TotCountyByteSize += ("\n\t<County Name=\"" + Bound.Name + "\"></County>").Length;
                foreach (PointF pt in Bound.Coordinates)
                {
                    TotCountyByteSize += (pt.X.ToString() + "," + pt.Y.ToString() + ",").Length;
                    TotCountyPoints++;
                }
            }
            sW.WriteLine("Total boundaries: {0} Number of points: {1}, Total byte size: {2}", MM_Repository.Counties.Count.ToString("#,##0"), TotCountyPoints.ToString("#,###,###,###,##0"), TotCountyByteSize.ToString("#,###,###,###,##0"));


            sW.WriteLine();
            sW.WriteLine("Substations: {0}", MM_Repository.Substations.Count.ToString("#,##0"));
            int TotSubLatLng = 0;
            foreach (MM_Substation Sub in MM_Repository.Substations.Values)
                TotSubLatLng += Sub.LatLong.X.ToString().Length + Sub.LatLong.Y.ToString().Length;

            float AvgSubLatLng = 32.76f; // (float)Math.Ceiling((float)TotSubLatLng / (float)MM_Repository.Substations.Count);

            sW.WriteLine("Average number of bytes for latitudes and longitudes of substations: {0}", AvgSubLatLng.ToString("#,##0.0"));
            sW.WriteLine("Total bytes for latitudes and longitudes of substations: {0}", TotSubLatLng.ToString("#,##0"));
            String LatlongHeader = "\t\t<etx:PowerSystemResource.Longitude></etx:PowerSystemResource.Longitude>\n\t\t<etx:PowerSystemResource.Latitude></etx:PowerSystemResource.Latitude>";
            float TotalSubBytes = TotSubLatLng + LatlongHeader.Length * MM_Repository.Substations.Count;
            sW.WriteLine("Total bytes for latitudes and longitudes of substations including XML tag headers: {0}", TotalSubBytes.ToString("#,##0"));
            sW.WriteLine();

            float TotalLineMiles = 0;

            foreach (MM_Line Line in MM_Repository.Lines.Values)
                TotalLineMiles += Line.ConnectedStations[0].DistanceTo(Line.ConnectedStations[1]);


            sW.WriteLine("Lines: {0}", MM_Repository.Lines.Count.ToString("#,##0"));
            sW.WriteLine("Estimated line total miles: {0}", TotalLineMiles.ToString("#,##0.00"));
            float[] MileMarkerCollection = new float[] { 0.1f, 0.25f, 0.5f, 0.75f, 1f, 2f, 4f };
            float[] MarkerSize = new float[MileMarkerCollection.Length];
            float[] MarkerPlusTagSize = new float[MileMarkerCollection.Length];
            String LineLatLongHeader = "\n\t\t<etx:PowerSystemResource.coordinates></etx:PowerSystemResource.coordinates>";
            for (int a = 0; a < MileMarkerCollection.Length; a++)
            {
                MarkerSize[a] = (AvgSubLatLng + 2) * TotalLineMiles / MileMarkerCollection[a];
                MarkerPlusTagSize[a] = MarkerSize[a] + (float)(MM_Repository.Lines.Count * LineLatLongHeader.Length);
            }


            for (int a = 0; a < MileMarkerCollection.Length; a++)
                sW.WriteLine("Estimated number of bytes for line inflection points assuming mile markers at every {0} mile: {1}", MileMarkerCollection[a].ToString("0.00"), MarkerSize[a].ToString("#,##0.0"));

            sW.WriteLine();
            sW.WriteLine("Using additional estimation of line latitude/longitude tag <etx:PowerSystemResource.coordinates>[lat],[long],[lat],[long]</etx:PowerSystemResource.coordinates>");
            for (int a = 0; a < MileMarkerCollection.Length; a++)
                sW.WriteLine("Estimated number of bytes for line inflection points assuming mile markers at every {0} mile: {1}", MileMarkerCollection[a].ToString("0.00"), MarkerPlusTagSize[a].ToString("#,##0.0"));

            sW.WriteLine();
            sW.WriteLine("Total impact to CIM file size of latitude and longitude additions (no counties):");
            for (int a = 0; a < MileMarkerCollection.Length; a++)
                sW.WriteLine("Estimated number of bytes assuming line mile markers at every {0} mile: {1}", MileMarkerCollection[a].ToString("0.00"), (MarkerPlusTagSize[a] + TotalSubBytes).ToString("#,###,###,###"));
            sW.WriteLine();
            sW.WriteLine("Total impact to CIM file size of latitude and longitude additions (including counties):");
            for (int a = 0; a < MileMarkerCollection.Length; a++)
                sW.WriteLine("Estimated number of bytes assuming line mile markers at every {0} mile: {1}", MileMarkerCollection[a].ToString("0.00"), (MarkerPlusTagSize[a] + TotalSubBytes + TotCountyByteSize).ToString("#,###,###,###"));

            sW.Flush();
            sW.Close();
        }

        #region System hook for global memory status
        [DllImport("kernel32.dll")]
        private static extern void GlobalMemoryStatus(out MemoryStatus stat);
        private struct MemoryStatus
        {

            public uint Length; //Length of struct
            public uint MemoryLoad; //Value from 0-100 represents memory usage
            public uint TotalPhysical;
            public uint AvailablePhysical;
            public uint TotalPageFile;
            public uint AvailablePageFile;
            public uint TotalVirtual;
            public uint AvailableVirtual;
        }
        #endregion


        #region Console access
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        /// <summary>
        /// Activate the system console on demand
        /// </summary>
        /// <returns>True</returns>
        private static bool ActivateConsole()
        {
            //If we're running in a command window, capture that.
            int ProcessId;
            GetWindowThreadProcessId(GetForegroundWindow(), out ProcessId);
            Process CurrentProcess = Process.GetProcessById(ProcessId);
            if (CurrentProcess.ProcessName == "cmd")
                AttachConsole(CurrentProcess.Id);
            //If not, create a new console window
            else
                AllocConsole();
            return true;
        }

        #endregion

        #region Input box a-la VB
        /// <summary>
        /// Offer a simple input box, a-la VB
        /// </summary>
        /// <param name="title"></param>
        /// <param name="promptText"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
        #endregion

        private delegate DialogResult SafeMessageBox(String Text, String Caption, MessageBoxButtons buttons, MessageBoxIcon icon);

        /// <summary>
        /// Display a messagebox against the MM thread
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        public static DialogResult MessageBox(String text, String caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Form ActiveForm = Form.ActiveForm;
            if (ActiveForm == null)
                return System.Windows.Forms.MessageBox.Show(text, caption, buttons, icon);
            else if (ActiveForm.InvokeRequired)
                return (DialogResult)ActiveForm.Invoke(new SafeMessageBox(MessageBox), text, caption, buttons, icon);
            else
                return System.Windows.Forms.MessageBox.Show(ActiveForm, text, caption, buttons, icon);            
        }

        /// <summary>
        /// Kick off a messagebox in a separate thread
        /// </summary>
        /// <param name="state"></param>
        public static void MessageBoxInSeparateThread(Object state)
        {
            object[] InVal = (object[])state;            
            using (Form TempForm = new Form())
            {
                TempForm.TopMost = true;
                TempForm.ShowInTaskbar = true;
                System.Windows.Forms.MessageBox.Show(TempForm, (string)InVal[0], (string)InVal[1], (MessageBoxButtons)InVal[2], (MessageBoxIcon)InVal[3]);
            }
        }
    }
}