using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.User_Interfaces;
using MacomberMapClient.User_Interfaces.Startup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.SystemInformation;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.Properties;
using SharpDX.Windows;
using CefSharp;
using System.Xml;

namespace MacomberMapClient
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the initial startup functionality of the Macomber Map client.
    /// </summary>
    public static class Program
    {
        /// <summary>Whether to activate the system console</summary>
        public static bool Console = false;

        /// <summary>Our global indicator of when the map started</summary>
        public static bool MapStarted = false;

        /// <summary>Our MM instance</summary>
        public static MacomberMap_Form MM;

        /// <summary>This timer checks the presence of the main form every 15 seconds, if the thread isn't running, kill the application</summary>
        public static System.Threading.Timer MainFormChecker;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(params string[] args)
        {
           // CreateModelXml user password filepath (Build thMM_System_Interfacesis as a console app for this job).
            if (args.Length > 0 && args[0].Equals("CreateModelXml", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    System.Console.Error.WriteLine("Starting XML Model creator client");
                    MM_Server_Interface.LoadModelFromSql = true;
                    MM_System_Interfaces.Headless = true;
                    MM_Repository.OverallDisplay = new MM_Display();
                    MM_Coordinates coord = new MM_Coordinates(-109f, 29f, -86f, 51f, 1f);

                    System.Console.Error.WriteLine("Waiting for server...");
                    while (MM_Server_Interface.MMServers == null || MM_Server_Interface.MMServers.Count == 0)
                        Thread.Sleep(3000);

                    var list = MM_Server_Interface.MMServers.Values.ToList();

                    System.Console.Error.WriteLine("Connecting to server: " + list[0].ServerName);

                    string user = "test";
                    string password = "";
                    string file = "MacomberMap-Combined.MM_Model";

                    if (args.Length > 1)
                    {
                        if (args[1].EndsWith(".MM_Model"))
                            file = args[1];
                        else
                            user = args[1];
                    }
                    if (args.Length > 2)
                    {
                        if (args[2].EndsWith(".MM_Model"))
                            file = args[2];
                        else
                            password = args[2];
                    }
                    if (args.Length > 3)
                    {
                        file = args[3];
                    }
                    Exception exp = new Exception();
                    MM_Server_Interface.TryLogin(list[0].ServerName, list[0].ServerURI, user, password, out exp);
                    MM_Server_Interface.LoadMacomberMapConfiguration();

                    Data_Integration.InitializationComplete = false;
                    System.Console.Error.WriteLine("Starting SQL Model Loader...");
                    SqlModelLoader sqlModelLoader = new SqlModelLoader();
                    sqlModelLoader.ConnectionString = Settings.Default.ModelDatabase;
                    sqlModelLoader.LoadStaticRepository();
                    System.Console.Error.WriteLine("Rolling up Elements...");
                    Data_Integration.UseEstimates = false;
                    Data_Integration.RollUpElementsToSubstation();
                    Data_Integration.CommLoaded = true;
                    Data_Integration.ModelLoadCompletion = DateTime.Now;
                    Data_Integration.NetworkSource = new MM_Data_Source();
                    Data_Integration.NetworkSource.Estimates = false;
                    Data_Integration.NetworkSource.Database = "NETMOM";
                    Data_Integration.NetworkSource.Application = "RTNET";
                    Data_Integration.NetworkSource.BackColor = Color.Blue;
                    Data_Integration.NetworkSource.Telemetry = true;
                    Data_Integration.NetworkSource.Default = true;
                    Data_Integration.NetworkSource.Master = true;
                    Data_Integration.NetworkSource.ViolationApp = "RTCA";
                    Data_Integration.InitializationComplete = true;
                    // Data_Integration.RestartModel(MM_Server_Interface.Client);
                    Data_Integration.SaveXMLData(file, coord);
                    System.Console.WriteLine("Waiting for data...");
                    for (int i = 0; i < 60 * 8; i++)
                    {
                        System.Console.Write(".");
                        Thread.Sleep(1000); // wait for EMS data to flow in.
                    }
                    Data_Integration.SaveXMLData(file, coord);
                }
                catch (Exception ex)
                {
                    System.Console.Error.WriteLine(ex.Message);
                    System.Console.Error.WriteLine(ex.StackTrace);
                    System.Environment.Exit(1);
                }
                System.Environment.Exit(0);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            //   Application.Run(new frmControlTester());
            // return;
            try
            {
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Policies", true);

                if (!key.GetSubKeyNames().Contains("Chromium"))
                    key.CreateSubKey("Chromium");
                key = key.OpenSubKey("Chromium", true);
                if (!key.GetSubKeyNames().Contains("EnabledPlugins"))
                    key.CreateSubKey("EnabledPlugins");
                key = key.OpenSubKey("EnabledPlugins", true);

                if (!key.GetValueNames().Contains("1"))
                {
                    key.SetValue("1", "Silverlight*");
                    key.SetValue("2", "npapi");
                }

            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
            if (Environment.CommandLine.IndexOf("/Test", StringComparison.CurrentCultureIgnoreCase) != -1)
                Application.Run(new frmTest());
            else
                using (MM_Startup_Form Startup = new MM_Startup_Form())
                    if (Startup.ShowDialog(StartNetworkMap) == DialogResult.OK)
                        Application.Run();
                    else
                        Application.Exit();
        }

        /// <summary>
        /// Start the network map in its own thread
        /// </summary>
        /// <param name="state"></param>
        private static void StartNetworkMap(object state)
        {
            object[] State = (object[])state;
            MM = new MacomberMap_Form(State[0] as MM_Coordinates, State[1] as MM_Startup_Form);
            
            ApplicationContext MMContext = new ApplicationContext(MM);
            MMContext.ThreadExit += new EventHandler(MMContext_ThreadExit);
            Application.Run(MMContext);
            
        }

        private static void MMContext_ThreadExit(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// When we have a thread error, try to log and/or report it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            throw new NotImplementedException();
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


    }
}
