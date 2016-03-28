using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MacomberMapIntegratedService.EMS;
using MacomberMapIntegratedService.History;
using System.IO;
using MacomberMapCommunications.Messages.EMS;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Attributes;
using System.Threading;
using MacomberMapIntegratedService.Properties;
using System.ServiceModel;
using System.ServiceModel.Description;
using MacomberMapIntegratedService.WCF;
using System.Net;
using System.Net.Sockets;
using MacomberMapCommunications;
using MacomberMapAdministrationService;
using System.Reflection;
using System.Configuration;
using MacomberMapCommunications.WCF;
using System.Collections.Concurrent;
using MacomberMapIntegratedService.Database;
using System.Data.Common;

namespace MacomberMapIntegratedService.Service
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the static methods for the Macomber Map server
    /// </summary>
    public static class MM_Server
    {
        #region Variable declarations
        /// <summary>Our thread for sending all data</summary>
        private static Thread ClientStressTestingThread = new Thread(new ThreadStart(SendAllDataThread)) { Name = "Data sending thread", IsBackground = true };

        /// <summary>The UDP listner that listens for pings from clients</summary>
        private static UdpClient UdpReceiver;

        /// <summary>The UDP Server that broadcasts out</summary>
        private static UdpClient UdpServer;

        /// <summary>The instance of our MM WCF server</summary>
        private static ServiceHost Instance_MMServer;
        
        /// <summary>The instance of our admin server</summary>
        private static ServiceHost Instance_AdminServer;
      
        /// <summary>Report the start time of our server</summary>
        public static DateTime StartTime;

        /// <summary>Our collection of line data</summary>
        public static MM_Line_Data[] LineData = new MM_Line_Data[0];

        /// <summary>Our collection of loaddata</summary>
        public static MM_Load_Data[] LoadData = new MM_Load_Data[0];

        /// <summary>Our collection of island data</summary>
        public static Dictionary<int, MM_Island_Data> IslandData = new Dictionary<int, MM_Island_Data>();

        /// <summary>Our collection of simulation island data</summary>
        public static Dictionary<int, MM_Island_Simulation_Data> IslandSimulationData = new Dictionary<int, MM_Island_Simulation_Data>();

        /// <summary>Our collection of bus data</summary>
        public static Dictionary<int, MM_Bus_Data> BusData = new Dictionary<int, MM_Bus_Data>();

        /// <summary>Our collection of line outage data data</summary>
        public static Dictionary<int, MM_Outage_Line_Data> LineOutageData = new Dictionary<int, MM_Outage_Line_Data>();

        /// <summary>Our collection of unit outage data data</summary>
        public static Dictionary<int, MM_Outage_Unit_Data> UnitOutageData = new Dictionary<int, MM_Outage_Unit_Data>();

        /// <summary>Our collection of transformer outage data data</summary>
        public static Dictionary<int, MM_Outage_Transformer_Data> TransformerOutageData = new Dictionary<int, MM_Outage_Transformer_Data>();

        /// <summary>Our basecase violation data</summary>
        public static Dictionary<int, MM_Basecase_Violation_Data> BasecaseViolationData = new Dictionary<int, MM_Basecase_Violation_Data>();

        /// <summary>Our contingency violation data</summary>
        public static Dictionary<string, MM_Contingency_Violation_Data> ContingencyViolationData = new Dictionary<string, MM_Contingency_Violation_Data>();

        /// <summary>Our collection of analog measurement data</summary>
        public static MM_Analog_Measurement[] AnalogMeasurementData = new MM_Analog_Measurement[0];

        /// <summary>Our collection of shunt compensator data</summary>
        public static MM_ShuntCompensator_Data[] ShuntCompensatorData = new MM_ShuntCompensator_Data[0];

        /// <summary>Our collection of state measurement data</summary>
        public static MM_State_Measurement[] StateMeasurementData = new MM_State_Measurement[0];

        /// <summary>Our collection of transformer data</summary>
        public static MM_Transformer_Data[] TransformerData = new MM_Transformer_Data[0];

        /// <summary>Our collection of transformer phase shifter data</summary>
        public static MM_Transformer_PhaseShifter_Data[] TransformerPhaseShifterData = new MM_Transformer_PhaseShifter_Data[0];

        /// <summary>Our collection of zero impedance bridge data</summary>
        public static MM_ZeroImpedanceBridge_Data[] ZBRData = new MM_ZeroImpedanceBridge_Data[0];

        /// <summary>Our collection of unit data</summary>
        public static MM_Unit_Data[] UnitData = new MM_Unit_Data[0];

        /// <summary>Our pointer to data collection array</summary>
        public static Dictionary<Type, int> DataCollections = new Dictionary<Type, int>();

        /// <summary>Our array of timestamps that gets passed to the clients for updates</summary>
        public static MM_Data_Collection[] Timestamps = new MM_Data_Collection[0];

        /// <summary>Our lock object for a new timestamp</summary>
        public static object LockObjectForNewTimestamp = new object();

        /// <summary>Our collection of operatorship updates</summary>
        public static Dictionary<int, MM_Operatorship_Update> OperatorshipUpdates = new Dictionary<int, MM_Operatorship_Update>();

        /// <summary>Our latest system information</summary>
        public static MM_System_Information SystemInformation = new MM_System_Information();

        /// <summary>Our breaker/switch data</summary>
        public static MM_BreakerSwitch_Data[] BreakerSwitchData = new MM_BreakerSwitch_Data[0];

        /// <summary>Our chart data</summary>
        public static MM_Chart_Data[] ChartData = new MM_Chart_Data[0];

        /// <summary>Our DC Tie data</summary>
        public static MM_Tie_Data[] TieData = new MM_Tie_Data[0];

        /// <summary>Our SVC data</summary>
        public static MM_StaticVarCompensator_Data[] SVCData = new MM_StaticVarCompensator_Data[0];

        /// <summary>Our Unit-type generation data</summary>
        public static MM_UnitType_Generation_Data[] UnitTypeGenerationData = new MM_UnitType_Generation_Data[0];

        /// <summary>Our system-wide generation data</summary>
        public static MM_SystemWide_Generation_Data[] SystemWideGenerationData = new MM_SystemWide_Generation_Data[0];

        /// <summary>Our unit generation-specific data</summary>
        public static MM_Unit_Gen_Data[] UnitGenData = new MM_Unit_Gen_Data[0];

        /// <summary>Our interface data</summary>
        public static MM_Interface_Monitoring_Data[] InterfaceData = new MM_Interface_Monitoring_Data[0];

        /// <summary>Our LMP data</summary>
        public static MM_LMP_Data[] LmpData = new MM_LMP_Data[0];

        /// <summary>Our flowgate data</summary>
        public static MM_Flowgate_Data[] FlowgateData = new MM_Flowgate_Data[0];

        /// <summary>Our SCADA Status data</summary>
        public static MM_Scada_Status[] SCADAStatuses = new MM_Scada_Status[0];

        /// <summary>Our SCADA analog data</summary>
        public static MM_Scada_Analog[] SCADAAnalogs = new MM_Scada_Analog[0];

        /// <summary>Our load forecast data</summary>
        public static MM_Load_Forecast_Data[] LoadForecastData = new MM_Load_Forecast_Data[0];

        /// <summary>Our unit simulation data</summary>
        public static MM_Unit_Simulation_Data[] UnitSimulationData = new MM_Unit_Simulation_Data[0];

        /// <summary>Our synchroscope data</summary>
        public static MM_Synchroscope_Data[] SynchroscopeData = new MM_Synchroscope_Data[0];

        /// <summary>Our simulator time data</summary>
        public static MM_Simulation_Time[] SimulatorTimeData = new MM_Simulation_Time[0];

        /// <summary>Our unit control data within the simulation</summary>
        public static Dictionary<int, MM_Unit_Control_Status> UnitControlStatusData = new Dictionary<int, MM_Unit_Control_Status>();

        /// <summary>The address of our Macomber Map server</summary>
        public static String MacomberMapServerAddress;

        /// <summary>The name of our application</summary>
        public static string ApplicationName;

        /// <summary>The version of our application</summary>
        public static string ApplicationVersion;

        /// <summary>The URI of our server</summary>
        public static Uri ServerURI;

        /// <summary>The last IP address that posted data to TEDE</summary>
        public static IPAddress LastTEDEAddress = IPAddress.None;

        /// <summary>The last timestamp from TEDE updates</summary>
        public static DateTime LastTEDEUpdate = DateTime.MinValue;

        /// <summary>Our collection of EMS commands</summary>
        public static List<MM_EMS_Command> EMSCommands = new List<MM_EMS_Command>();

        /// <summary>The time stamp for when the server is started</summary>
        public static DateTime ServerEpoch = DateTime.Now;

        /// <summary>The description of our server</summary>
        public static string MacomberMapServerDescription
        {
            get { return Settings.Default.MacomberMapServerDescription; }
            set { 
                
                Settings.Default.MacomberMapServerDescription = value;
                Settings.Default.Properties["MacomberMapServerDescription"].DefaultValue = value;
                
                //Write out our file
                System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
                String FileName = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
                xDoc.Load(FileName);
                xDoc.SelectSingleNode("/configuration/userSettings/MacomberMapIntegratedService.Properties.Settings/setting[@name='MacomberMapServerDescription']/value").InnerText = value;
                xDoc.Save(FileName);
            }
        }
        #endregion

        #region User handling
        /// <summary>Our collection of connected clients</summary>
        private static ConcurrentDictionary<Guid, MM_WCF_Interface> ConnectedUsers = new ConcurrentDictionary<Guid, MM_WCF_Interface>();

        /// <summary>
        /// Return a thread-safe version of our connected users
        /// </summary>
        public static MM_WCF_Interface[] ConnectedUserList
        {
            get
            {
                List<MM_WCF_Interface> Users = new List<MM_WCF_Interface>();
                using (IEnumerator<KeyValuePair<Guid, MM_WCF_Interface>> UserEnum = ConnectedUsers.GetEnumerator())
                    while (UserEnum.MoveNext())
                    {
                        MM_WCF_Interface User = UserEnum.Current.Value;
                        if (User != null)
                            Users.Add(User);
                    }                        
                return Users.ToArray();
            }
        }

        /// <summary>Our list of connected administrators</summary>
        private static ConcurrentDictionary<Guid, MM_Administrator_Types> ConnectedAdministrators = new ConcurrentDictionary<Guid, MM_Administrator_Types>();

        /// <summary>
        /// Return a thread-safe version of our connected users
        /// </summary>
        public static MM_Administrator_Types[] ConnectedAdministatorList
        {
            get
            {
                List<MM_Administrator_Types> Admins = new List<MM_Administrator_Types>(ConnectedAdministrators.Values);
                return Admins.ToArray();
            }
        }

      

        /// <summary>
        /// Add a new administrator to our list of administrators
        /// </summary>
        /// <param name="Admin"></param>
        public static void AddAdministrator(MM_Administrator_Types Admin)
        {
            ConnectedAdministrators.TryAdd(Admin.ConversationGuid, Admin);
        }

        /// <summary>
        /// Remove an administrator from our list
        /// </summary>
        /// <param name="Admin"></param>
        public static void RemoveAdministrator(MM_Administrator_Types Admin)
        {
            MM_Administrator_Types FoundAdmin;
            ConnectedAdministrators.TryRemove(Admin.ConversationGuid, out FoundAdmin);
        }

        /// <summary>
        /// Find a particular administrator
        /// </summary>
        /// <param name="AdminGuid"></param>
        /// <param name="FoundAdmin"></param>
        /// <returns></returns>
        public static bool FindAdministrator(Guid AdminGuid, out MM_Administrator_Types FoundAdmin)
        {
            return ConnectedAdministrators.TryGetValue(AdminGuid, out FoundAdmin);
        }

        /// <summary>
        /// Find a partiuclar user
        /// </summary>
        /// <param name="AdminGuid"></param>
        /// <param name="FoundUser"></param>
        /// <returns></returns>
        public static bool FindUser(Guid AdminGuid, out MM_WCF_Interface FoundUser)
        {
            return ConnectedUsers.TryGetValue(AdminGuid, out FoundUser);
        }

        /// <summary>
        /// Add a new user to our system
        /// </summary>
        /// <param name="User"></param>
        public static void AddUser(MM_WCF_Interface User)
        {
            ConnectedUsers.TryAdd(User.User.UserId, User);
        }

        /// <summary>
        /// Remove a user from the system
        /// </summary>
        /// <param name="User"></param>
        public static void RemoveUser(MM_WCF_Interface User)
        {
            MM_WCF_Interface FoundUser;
            ConnectedUsers.TryRemove(User.User.UserId, out FoundUser);
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Start up the full Macomber Map server
        /// </summary>
        public static void StartServer()
        {
            ApplicationName = Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
            ApplicationVersion = Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            MM_Notification.WriteLine(ConsoleColor.Green, ApplicationName + " " + ApplicationVersion + " starting up.");
            MM_Notification.Notify("SERVER: Macomber Map Service starting", "The Macomber Map Service is starting, ConsoleMode=" + Environment.UserInteractive.ToString());
            StartTime = DateTime.Now;

            //Start our EMS TCP listener            
            MM_EMSReader_TCP.Initialize();
            MM_EMSReader_File.Initialize();

            //Start our database service
            MM_Database_Connector.StartServer();

            //Start our PI interface
            MM_Historic_Reader.StartServer();

            //If we have commands, pull them in
            DateTime BatchDate = DateTime.Now;
            if (File.Exists(Properties.Settings.Default.CommandFileLocation))
                using (StreamReader sRd = new StreamReader(Properties.Settings.Default.CommandFileLocation))
                    MM_EMSReader_File.ProcessStreamRead(sRd, Properties.Settings.Default.CommandFileLocation, typeof(MM_EMS_Command), ref BatchDate);

            //Start our WCF interfaces
            MacomberMapServerAddress = Settings.Default.MacomberMapServerAddress.Split(',')[0].Replace("localhost",Dns.GetHostEntry(IPAddress.Loopback).HostName);
            ServerURI = new Uri(MacomberMapServerAddress);
            Instance_MMServer = StartWCFServer<MM_WCF_Interface>(Settings.Default.MacomberMapServerAddress.Split(','));
            Instance_AdminServer = StartWCFServer<MM_Administrator_Types>(Settings.Default.MacomberMapAdministratorAddress.Split(','));

            ThreadPool.QueueUserWorkItem(new WaitCallback(MeasureSystemState));
            ThreadPool.QueueUserWorkItem(new WaitCallback(SendUDPMessage));
            ThreadPool.QueueUserWorkItem(new WaitCallback(WriteTrainingGameHtmlFile));
            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// Start our WCF server. Thanks to https://social.msdn.microsoft.com/Forums/vstudio/en-US/7195129e-12b7-4dcf-9104-6d6530c33c7f/is-it-possible-to-configure-wcf-service-programmatically-without-using-webconfig?forum=wcf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="BaseAddress"></param>
        private static ServiceHost StartWCFServer<T>(String[] BaseAddress)
        {
            List<Uri> Addresses = new List<Uri>();
            foreach (String str in BaseAddress)
                Addresses.Add(new Uri(str));
            ServiceHost ServerHost = new ServiceHost(typeof(T), Addresses.ToArray());

            //Add our net.TCP binding
            NetTcpBinding TcpBinding = MM_Binding_Configuration.CreateBinding();
            ServiceEndpoint TcpEndpoint = ServerHost.AddServiceEndpoint(typeof(T).GetInterfaces()[0], TcpBinding, Addresses[0]);

            ServiceMetadataBehavior MetadataBehavior = ServerHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (MetadataBehavior != null)
                MetadataBehavior.HttpGetEnabled = MetadataBehavior.HttpsGetEnabled = false;
            else
                ServerHost.Description.Behaviors.Add(new ServiceMetadataBehavior() { HttpGetEnabled = false, HttpsGetEnabled = false });

            //Build our metadata behavior
            /*ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            smb.HttpsGetEnabled = true;            
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            ServerHost.Description.Behaviors.Add(smb);*/
            ServerHost.Open();
            MM_Notification.WriteLine(ConsoleColor.Green, "WCF Server for {0} listening at {1}", typeof(T).Name, Addresses[0].ToString());
            return ServerHost;
        }
        #endregion

        #region Server discovery and status
        /// <summary>
        /// Measure the state of our system
        /// </summary>
        private static void MeasureSystemState(object state)
        {
            while (true)
            {
                long Mem;
                MM_System_Profiling.MEMORYSTATUSEX MemStatus;
                SystemInformation.CPUUsage = MM_System_Profiling.GetCPUUsage(out Mem, out MemStatus);
                SystemInformation.SystemFreeMemory = MemStatus.ullAvailPhys;
                SystemInformation.SystemTotalMemory = MemStatus.ullTotalPhys;
                SystemInformation.ApplicationMemoryUse = Mem;
                SystemInformation.LineCount = LineData.Length;
                SystemInformation.LoadCount = LoadData.Length;
                SystemInformation.BusCount = BusData.Count;
                SystemInformation.LineOutageCount = LineOutageData.Count;
                SystemInformation.TransformerOutageCount = TransformerOutageData.Count;
                SystemInformation.UnitOutageCount = UnitOutageData.Count;
                SystemInformation.UnitCount = UnitData.Length;
                SystemInformation.UnitGenCount = UnitGenData.Length;
                SystemInformation.InterfaceCount = InterfaceData.Length;
                SystemInformation.BasecaseCount = BasecaseViolationData.Count;
                SystemInformation.ContingencyCount = ContingencyViolationData.Count;
                SystemInformation.TransformerCount = TransformerData.Length;
                SystemInformation.PhaseShifterCount = TransformerData.Length;
                SystemInformation.BreakerSwitchCount = BreakerSwitchData.Length;
                SystemInformation.TieCount = TieData.Length;
                SystemInformation.SVCCount = SVCData.Length;
                SystemInformation.UnitTypeCount = UnitTypeGenerationData.Length;
                SystemInformation.SystemCount = SystemWideGenerationData.Length;
                SystemInformation.LMPCount = LmpData.Length;
                SystemInformation.FlowgateCount = FlowgateData.Length;
                SystemInformation.AnalogCount = AnalogMeasurementData.Length;
                SystemInformation.StateCount = StateMeasurementData.Length;
                SystemInformation.SCADAStatusCount = SCADAStatuses.Length;
                SystemInformation.SCADAAnalogCount = SCADAAnalogs.Length;
                SystemInformation.LoadForecastCount = LoadForecastData.Length;
                SystemInformation.UnitSimulationCount = UnitSimulationData.Length;
                SystemInformation.SynchroscopeCount = SynchroscopeData.Length;
                SystemInformation.UnitControlCount = UnitControlStatusData.Count;
                SystemInformation.ShuntCompensatorCount = ShuntCompensatorData.Length;
                SystemInformation.ZBRCount = ZBRData.Length;
                SystemInformation.UserCount = ConnectedUsers.Count; 
                SystemInformation.CommandCount = EMSCommands == null ? -1 : EMSCommands.Count;
                SystemInformation.OperatorshipUpdateCount = OperatorshipUpdates.Count;
                SystemInformation.LastTEDeAddress = LastTEDEAddress;
                SystemInformation.LastTEDeUpdate = LastTEDEUpdate;
                if (SimulatorTimeData.Length > 0)
                {
                    SystemInformation.SimulationTime = SimulatorTimeData[0].Simulation_Time;
                    SystemInformation.SimulationStatus = SimulatorTimeData[0].Status;
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Get the name for our update command
        /// </summary>
        /// <returns></returns>
        private static void SendUpdate<T>(Guid UserId, T[] OutArray)
        {
            //Find our update command
            foreach (object obj in typeof(T).GetCustomAttributes(false))
                if (obj is UpdateCommandAttribute)
                     MM_WCF_Interface.SendMessage(UserId, ((UpdateCommandAttribute)obj).UpdateCommand, OutArray);                    
        }

        /// <summary>
        /// Our test thread for sending all data
        /// </summary>
        private static void SendAllDataThread()
        {
            while (true)
            {
                foreach (MM_WCF_Interface Interface in ConnectedUserList)
                    if (Interface.User.LoggedOnTime != default(DateTime))
                        try
                        {
                            SendUpdate<MM_Line_Data>(Interface.User.UserId, LineData);
                            SendUpdate<MM_Island_Data>(Interface.User.UserId, IslandData.Values.ToArray());
                            SendUpdate<MM_Load_Data>(Interface.User.UserId, LoadData);
                            SendUpdate<MM_Bus_Data>(Interface.User.UserId, BusData.Values.ToArray());
                            SendUpdate<MM_Outage_Line_Data>(Interface.User.UserId, LineOutageData.Values.ToArray());
                            SendUpdate<MM_Outage_Unit_Data>(Interface.User.UserId, UnitOutageData.Values.ToArray());
                            SendUpdate<MM_Outage_Transformer_Data>(Interface.User.UserId, TransformerOutageData.Values.ToArray());
                            SendUpdate<MM_Analog_Measurement>(Interface.User.UserId, AnalogMeasurementData);
                            SendUpdate<MM_ShuntCompensator_Data>(Interface.User.UserId, ShuntCompensatorData);
                            SendUpdate<MM_State_Measurement>(Interface.User.UserId, StateMeasurementData);
                            SendUpdate<MM_Transformer_Data>(Interface.User.UserId, TransformerData);
                            SendUpdate<MM_Transformer_PhaseShifter_Data>(Interface.User.UserId, TransformerPhaseShifterData);
                            SendUpdate<MM_Unit_Data>(Interface.User.UserId, UnitData);
                            SendUpdate<MM_ZeroImpedanceBridge_Data>(Interface.User.UserId, ZBRData);
                            SendUpdate<MM_BreakerSwitch_Data>(Interface.User.UserId, BreakerSwitchData);
                            SendUpdate<MM_Scada_Analog>(Interface.User.UserId, SCADAAnalogs);
                            SendUpdate<MM_Scada_Status>(Interface.User.UserId, SCADAStatuses);
                            SendUpdate<MM_Chart_Data>(Interface.User.UserId, ChartData);
                            SendUpdate<MM_Tie_Data>(Interface.User.UserId, TieData);
                            SendUpdate<MM_StaticVarCompensator_Data>(Interface.User.UserId, SVCData);
                            SendUpdate<MM_SystemWide_Generation_Data>(Interface.User.UserId, SystemWideGenerationData);
                            SendUpdate<MM_UnitType_Generation_Data>(Interface.User.UserId, UnitTypeGenerationData);
                            SendUpdate<MM_Unit_Gen_Data>(Interface.User.UserId, UnitGenData);
                            SendUpdate<MM_Operatorship_Update>(Interface.User.UserId, OperatorshipUpdates.Values.ToArray());
                            SendUpdate<MM_Basecase_Violation_Data>(Interface.User.UserId, BasecaseViolationData.Values.ToArray());
                            SendUpdate<MM_Contingency_Violation_Data>(Interface.User.UserId, ContingencyViolationData.Values.ToArray());
                            SendUpdate<MM_Load_Forecast_Data>(Interface.User.UserId, LoadForecastData);
                            SendUpdate<MM_Interface_Monitoring_Data>(Interface.User.UserId, InterfaceData);
                            SendUpdate<MM_Unit_Simulation_Data>(Interface.User.UserId, UnitSimulationData);
                            SendUpdate<MM_Island_Simulation_Data>(Interface.User.UserId, IslandSimulationData.Values.ToArray());
                            SendUpdate<MM_Synchroscope_Data>(Interface.User.UserId, SynchroscopeData);
                            SendUpdate<MM_Simulation_Time>(Interface.User.UserId, SimulatorTimeData);
                            LastTEDEUpdate = DateTime.Now;
                            Thread.Sleep(1500);
                        }
                        catch (Exception ex)
                        {
                        }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Write out our traning game HTML file
        /// </summary>
        /// <param name="state"></param>
        private static void WriteTrainingGameHtmlFile(object state)
        {
            while (true)
            {
                StringBuilder sB = new StringBuilder();
                using (DbCommand dCmd = MM_Database_Connector.CreateCommand(Settings.Default.TrainingGameQuery, MM_Database_Connector.oConn))
                using (DbDataReader dRd = dCmd.ExecuteReader())
                    while (dRd.Read())
                        sB.Append(ParseLine(Settings.Default.TrainingGameOutputLine, dRd));
                File.WriteAllText(Settings.Default.TrainingGameHtmlFile, Resources.TrainingGameHtmlContent.Replace("{Content}", sB.ToString()));
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="InLine"></param>
        /// <param name="dRd"></param>
        /// <returns></returns>
        private static string ParseLine(String InLine, DbDataReader dRd)
        {
            StringBuilder sB = new StringBuilder();
            int FoundPos = InLine.IndexOf('{');
            int LastPos = 0;

            while (FoundPos != -1)
            {
                if (FoundPos != LastPos)
                    sB.Append(InLine.Substring(LastPos, FoundPos - LastPos));
                int EndBracket = InLine.IndexOf('}', FoundPos); ;

                String ThisParameter = InLine.Substring(FoundPos + 1, EndBracket - FoundPos - 1);
                if (ThisParameter.Contains(':'))
                {
                    Object InVal = dRd[ThisParameter.Split(':')[0]];
                    MethodInfo mI = InVal.GetType().GetMethod("ToString", new Type[] { typeof(string) });
                    sB.Append(mI.Invoke(InVal, new object[] { ThisParameter.Split(':')[1] }));
                }
                else
                    sB.Append(dRd[ThisParameter].ToString());
                LastPos = EndBracket + 1;
                FoundPos = InLine.IndexOf('{', EndBracket);
            }
            if (LastPos != InLine.Length)
                sB.Append(InLine.Substring(LastPos));
            return sB.ToString();
        }

        /// <summary>
        /// Send a broadcast UDP message indicating the MM server is online, and current users available.
        /// </summary>
        /// <param name="state"></param>
        private static void SendUDPMessage(object state)
        {
            UdpReceiver = new UdpClient();
            UdpServer = new UdpClient();

            UdpReceiver.ExclusiveAddressUse = false;
            UdpReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpReceiver.Client.Bind(new IPEndPoint(IPAddress.Any, Settings.Default.MacomberMapServerQueryPort));
            UdpReceiver.BeginReceive(ReceiveUDPMessage, UdpReceiver);

            while (true)
            {
                byte[] OutBytes = new UTF8Encoding(false).GetBytes("Macomber Map Server|" + MM_Server.ApplicationVersion + "|" + Environment.MachineName + "|" + MacomberMapServerAddress + "|" + ConnectedUsers.Count.ToString() + "|" + MacomberMapServerDescription);
                try
                {
                    UdpServer.Send(OutBytes, OutBytes.Length, new IPEndPoint(IPAddress.Broadcast, Settings.Default.MacomberMapServerBroadcastPort));
                }
                catch { }
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Receive a UDP message from a client, and respond to it with our information
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceiveUDPMessage(IAsyncResult ar)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, 0);
            try
            {
                byte[] InLine = UdpReceiver.EndReceive(ar, ref remoteEndPoint);
                String[] splStr = new UTF8Encoding(false).GetString(InLine).Split('|');
            }
            catch { }

            //Receive an incoming UDP message. For right now, all we need to do is respond to it.
            byte[] OutBytes = new UTF8Encoding(false).GetBytes("Macomber Map Server|" + MM_Server.ApplicationVersion + "|" + Environment.MachineName + "|" + MacomberMapServerAddress + "|" + ConnectedUsers.Count.ToString()+"|" + MacomberMapServerDescription);
            try
            {
                if (ar.AsyncState != null)
                    ((UdpClient)ar.AsyncState).Send(OutBytes, OutBytes.Length, remoteEndPoint);
                UdpReceiver.BeginReceive(ReceiveUDPMessage, UdpReceiver);
            }
            catch { }
        }

        /// <summary>
        /// Set our stress test client handler
        /// </summary>
        /// <param name="Active"></param>
        public static void StressTestClients(bool Active)
        {
            if (!Active && (ClientStressTestingThread.ThreadState & ThreadState.Running)==ThreadState.Running)
                ClientStressTestingThread.Suspend();
            else if (Active && (ClientStressTestingThread.ThreadState & ThreadState.Suspended)== ThreadState.Suspended)
                ClientStressTestingThread.Resume();
            else if (Active && (ClientStressTestingThread.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
                ClientStressTestingThread.Start();
        }
        #endregion

        #region Value updates
        /// <summary>
        /// Update our timestamp for a particular item
        /// </summary>
        /// <param name="DataType"></param>
        /// <param name="Rows"></param>
        public static void UpdateTimestamp(Type DataType, int Rows)
        {
            int FoundIndex;
            if (DataCollections.TryGetValue(DataType, out FoundIndex))
                Timestamps[FoundIndex].MarkUpdate(Rows);
            else
                lock (LockObjectForNewTimestamp)
                {
                    //Find the retrieval command for this type.
                    RetrievalCommandAttribute FoundRetr = null;
                    foreach (object obj in DataType.GetCustomAttributes(false))
                        if (obj is RetrievalCommandAttribute)
                            FoundRetr = (RetrievalCommandAttribute)obj;

                    //Determine our new index, and update accordingly.
                    int NewIndex = DataCollections.Count;
                    MM_Data_Collection[] NewColl = new MM_Data_Collection[NewIndex + 1];
                    Array.Copy(Timestamps, 0, NewColl, 0, Timestamps.Length);
                    NewColl[NewIndex] = new MM_Data_Collection() { UpdateMessage = FoundRetr.RetrievalCommand, LastUpdate = DateTime.Now, LastRowCount = Rows };
                    Timestamps = NewColl;
                }
        }
        #endregion       
    }
}
