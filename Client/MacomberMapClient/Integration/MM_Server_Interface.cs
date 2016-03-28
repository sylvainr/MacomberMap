using System.Drawing;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Properties;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Startup;
using MacomberMapCommunications;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.EMS;
using MacomberMapCommunications.WCF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.Integration
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides an interface to the MM Server WCF interface
    /// </summary>
    public static class MM_Server_Interface
    {
        #region Variable declarations
        /// <summary>Our client status, to report to the MM server</summary>
        public static MM_Client_Status ClientStatus = new MM_Client_Status();

        /// <summary>
        /// Should this model load data from SQL?
        /// </summary>
        public static bool LoadModelFromSql = false;

        /// <summary>Our MM WCF interface</summary>
        public static MM_WCF_Interface Client;
        /// <summary>Our server epoch</summary>
        public static DateTime ServerEpoch;

        /// <summary>The name of the current client</summary>
        public static String ClientName
        {
            get
            {
                try
                {
                    return Client.ServerName();
                }
                catch
                {
                    return "SERVER ERROR";
                }
            }
        }

        /// <summary>The user name to our server interface</summary>
        internal static string UserName;

        /// <summary>The password to our server interface</summary>
        internal static string Password;

        /// <summary>Our collection of models</summary>
        public static MM_Database_Model[] ModelCollection;

        /// <summary>Our callback handler</summary>
        public static MM_Server_Callback_Handler CallbackHandler;

        /// <summary>Our cached login information</summary>
        internal static String ServerName;

        /// <summary>The URI for our server instance</summary>
        public static Uri ConnectionURI;

        /// <summary>Our collection of update timestamps</summary>
        public static Dictionary<Type, MM_DataRetrieval_Information> DataRetrievalInformation = new Dictionary<Type, MM_DataRetrieval_Information>();

        /// <summary>The list of client areas</summary>
        public static Dictionary<string, bool> ClientAreas = new Dictionary<string, bool>();
        /// <summary>The QSE user flag</summary>
        public static bool ISQse =false;
        #endregion

        #region Management thread
        /// <summary>
        /// Handle the initailization of our server interface by kicking off a management thread
        /// </summary>
        static MM_Server_Interface()
        {
            StartUdpListener();
            StartPingMacomberMapServers();

            ThreadPool.QueueUserWorkItem(new WaitCallback(ManageServerConnection));
        }
        private static void StartUdpListener()
        {
            Task.Run(async () =>
            {
                using (var udpClient = new UdpClient())
                {
                    udpClient.ExclusiveAddressUse = false;
                    udpClient.Client.SendTimeout = 500;
                    udpClient.Client.ReceiveTimeout = 500;
                    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, Settings.Default.MacomberMapServerBroadcastPort));
                    while (udpClient != null && udpClient.Client != null)
                    {
                        var response = await udpClient.ReceiveAsync();
                        HandleUDPMessage(response);
                    }
                }
            });
        }

        /// <summary>
        /// Manage our server connection
        /// </summary>
        /// <param name="state"></param>
        private static void ManageServerConnection(object state)
        {
            Thread.CurrentThread.Name = "Server connection manager";
            Exception ex;
            while (true)
            {
                if (Client != null && Client.InnerChannel.State == CommunicationState.Opened && DateTime.Now.Second == 0)
                {
                    PingServer();
                    var drInfo = DataRetrievalInformation.ToList();
                    //Now, go through all of our data types. If we need to, perform an update
                    foreach (KeyValuePair<Type, MM_DataRetrieval_Information> kvp in drInfo)
                        if ((DateTime.Now - kvp.Value.LastFullRefresh).TotalSeconds > kvp.Value.FullRefreshTime)
                        {
                            kvp.Value.LastFullRefresh = DateTime.Now;
                            if (!String.IsNullOrEmpty(kvp.Value.RefreshCommand))
                            {
                                MethodInfo FoundMethod = typeof(MM_Server_Interface).GetMethod(kvp.Value.RefreshCommand);
                                if (FoundMethod == null)
                                    MM_System_Interfaces.LogError("Unable to find method to update type {0}", kvp.Value.BaseType.Name);
                                else
                                    try
                                    {
                                        FoundMethod.Invoke(null, null);
                                    }
                                    catch (TargetInvocationException) { }
                                    catch { }
                            }
                        }
                }
                else if (Client != null && (Client.InnerChannel.State == CommunicationState.Faulted || CallbackHandler.Context.State == CommunicationState.Faulted))
                    try
                    {
                        if (TryLoginAgain(ServerName, ConnectionURI, out ex))
                            Data_Integration.RestartModel(Client);
                    }
                    catch { }
                Thread.Sleep(1000);
            }
        }
        #endregion

        #region Server connectivity, listening, and ping


        /// <summary>Our collection of Macomber Map servers</summary>
        public static Dictionary<Uri, MM_Server_Information> MMServers = new Dictionary<Uri, MM_Server_Information>();

        /// <summary>
        /// Ping our Macomber Map servers
        /// </summary>
        internal static void StartPingMacomberMapServers()
        {
            Task.Run(() =>
            {
                var queryUdp = new UdpClient();
                queryUdp.ExclusiveAddressUse = false;
                queryUdp.Client.ReceiveTimeout = 500;
                queryUdp.Client.SendTimeout = 500;
                queryUdp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
                //queryUdp.Client.Bind(new IPEndPoint(IPAddress.Any, Settings.Default.MacomberMapServerQueryPort));

                int CurrentIter = 0;
                //if (Settings.Default.ServersToPing.IndexOf("localhost", StringComparison.CurrentCultureIgnoreCase) < 0)
                //    Settings.Default.ServersToPing += ",localhost";
            while (true)
            {
                //First, once every 10 seconds, send our commands out for manual servers
                if (CurrentIter == 0)
                    foreach (String str in Settings.Default.ServersToPing.Split(','))
                        try
                        {
                            foreach (IPAddress Addr in Dns.GetHostEntry(str).AddressList)
                                if (Addr.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    byte[] OutBytes = new UTF8Encoding(false).GetBytes("Macomber Map Client|" + Application.ProductVersion + "|" + Environment.MachineName + "|");
                                        queryUdp.Send(OutBytes, OutBytes.Length, new IPEndPoint(Addr, Settings.Default.MacomberMapServerQueryPort));

                                        Task.Run(async () =>
                                        {
                                            var response = await queryUdp.ReceiveAsync();
                                            HandleUDPMessage(response);
                                        }).Wait(queryUdp.Client.ReceiveTimeout);
                                    }
                            }
                            catch (SocketException ex)
                            {
                                if (ex.ErrorCode == 10054)
                                {
                                    Debug.WriteLine("Server not available: "+str);
                                }
                        }
                        catch { }

                    //Every 5 seconds, ping our clients
                    if (CurrentIter % 5 == 0)
                        using (Ping p = new Ping())
                        {

                            foreach (MM_Server_Information Server in MMServers.Values.ToArray())
                                try
                                {
                                    var reply = p.Send(Server.ServerURI.Host, 25);
                                    Server.PingTime = reply.RoundtripTime;
                                }
                                catch
                                {
                                    Server.PingTime = -1;
                                }
                            p.Dispose();

                        }
                    //Capture our CPU usage
                    try
                    {
                        long Mem;
                        MM_System_Profiling.MEMORYSTATUSEX MemStatus;
                        ClientStatus.CPUUsage = MM_System_Profiling.GetCPUUsage(out Mem, out MemStatus);
                        ClientStatus.SystemFreeMemory = MemStatus.ullAvailPhys;
                        ClientStatus.SystemTotalMemory = MemStatus.ullTotalPhys;
                        ClientStatus.OpenWindows = Data_Integration.RunningForms.Count;
                        ClientStatus.ApplicationMemoryUse = Mem;

                        //Send CPU status every 2 seconds
                        if (CurrentIter % 2 == 0 && Client != null)
                            Client.ReportUserStatus(ClientStatus);
                    }
                    catch {}

                    //Now, increment our counter
                    if (++CurrentIter == 10)
                    {
                        CurrentIter = 0;
                        if (ServerName != null && Data_Integration.TimeStarted != null && (DateTime.Now - Data_Integration.TimeStarted.Value).TotalMinutes > 3)
                            CheckForData();
                    }
                    //Wait a second
                    Thread.Sleep(1500);
                }
            });
        }

        /// <summary>
        /// Handle an input UDP message
        /// </summary>
        /// <param name="result">UDP Message</param>
        private static void HandleUDPMessage(UdpReceiveResult result) // IPEndPoint remoteEndPoint, string[] splStr)
        {
            if (result == default(UdpReceiveResult)) return;

            var buffer = result.Buffer;
            if (buffer == null || buffer.Length == 0) return;

            string[] splStr = new UTF8Encoding(false).GetString(buffer).Split('|');
            if (string.IsNullOrWhiteSpace(splStr[3]) || splStr[0] == "Macomber Map Client") return;

            Uri ServerUri = new Uri(splStr[3]);

            MM_Server_Information ServerInfo;
            if (!MMServers.TryGetValue(ServerUri, out ServerInfo))
                MMServers.Add(ServerUri, ServerInfo = new MM_Server_Information() { BroadcastEndpoint = result.RemoteEndPoint, ServerURI = ServerUri });
            ServerInfo.Description = splStr.Length == 5 ? "" : splStr[5];
            ServerInfo.UserCount = int.Parse(splStr[4]);
            ServerInfo.ServerName = splStr[2];
            ServerInfo.LastUDP = DateTime.Now;
        }

        #endregion

        volatile static bool checkingForData = false;
        /// <summary>
        /// safty net to try and prevent stale data.
        /// </summary>
        public static void CheckForData()
        {

            foreach (var di in DataRetrievalInformation.Values)
            {
                try
                {
                    if (!checkingForData && di.RefreshCommand != null && ((DateTime.Now - di.LastUpdate).TotalMinutes > 6
                     || (di.ErrorTime > 30 && (DateTime.Now - di.LastUpdate).TotalSeconds > di.ErrorTime)
                     || (di.WarningTime > 5 && (DateTime.Now - di.LastUpdate).TotalSeconds > di.WarningTime)
                     || (di.FullRefreshTime > 5 && (DateTime.Now - di.LastUpdate).TotalSeconds > di.FullRefreshTime)))
                    {
                        checkingForData = true;
                        MethodInfo FoundMethod = typeof(MM_Server_Interface).GetMethod(di.RefreshCommand);

                        if (FoundMethod != null)
                            FoundMethod.Invoke(null, null);
                        checkingForData = false;
                    }
                }
                catch (Exception)
                {
                    //MM_System_Interfaces.LogError(ex);
                    checkingForData = false;
                }

            }
        }


        #region Login
        /// <summary>
        /// Attempt to log in a second time using the existing credentials.
        /// </summary>
        /// <param name="ServerName"></param>
        /// <param name="ConnectionURI"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static bool TryLoginAgain(String ServerName, Uri ConnectionURI, out Exception Error)
        {
            return TryLogin(ServerName, ConnectionURI, UserName, Password, out Error);
        }

        /// <summary>
        /// Attempt to log in to a MM Server
        /// </summary>
        /// <param name="ServerName"></param>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <param name="ConnectionURI"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static bool TryLogin(String ServerName, Uri ConnectionURI, String UserName, String Password, out Exception Error)
        {
            if (Client != null)
            {
                try
                {
                    Client.Dispose();
                }
                catch
                {
                }
                Thread.Sleep(500);
            }
            Client = null;
            CallbackHandler = null;

            try
            {
                MM_Server_Interface.ServerName = ServerName;
                MM_Server_Interface.ConnectionURI = ConnectionURI;
                MM_Server_Interface.UserName = UserName;
                MM_Server_Interface.Password = Password;

                NetTcpBinding tcpBinding = MM_Binding_Configuration.CreateBinding();

                Client = new MM_WCF_Interface(new MM_Server_Callback_Handler().Context, tcpBinding, new EndpointAddress(ConnectionURI.ToString()));
                Client.ChannelFactory.Faulted += ChannelFactory_Faulted;
                Client.ChannelFactory.Opened += ChannelFactory_Opened;
                MM_Server_Interface.CallbackHandler.Context.Faulted += ChannelFactory_Faulted;
                MM_Server_Interface.CallbackHandler.Context.Opened += ChannelFactory_Opened;

                MM_Server_Interface.UserName = UserName;
                MM_Server_Interface.Password = Password;
                MM_Server_Interface.ClientAreas.Clear();
                foreach (String str in Client.HandleUserLogin(UserName, Password))
                    MM_Server_Interface.ClientAreas.Add(str, true);
                if (MM_Server_Interface.ClientAreas.Count == 0)
                {
                    MM_System_Interfaces.LogError("Unable to log in to the Macomber Map Server");
                    Client.InnerChannel.Close();
                    throw new InvalidOperationException("Unable to log in to the Macomber Map server. The username and password combination was invalid, and/or there were no operatorship areas for the client.");
                }
                else if (ClientAreas.ContainsKey("ERCOT"))
                    MM_System_Interfaces.LogError("Logged in to Macomber Map Server, MASTER " + MM_Server_Interface.ClientAreas.Count.ToString("0") + " operatorships.");
                else
                    MM_System_Interfaces.LogError("Logged in to Macomber Map Server, " + MM_Server_Interface.ClientAreas.Count.ToString("0") + " operatorships.");

                Error = null;
                return true;
            }
            catch (Exception ex)
            {
                Error = ex;
                return false;
            }
        }

        /// <summary>
        /// Report a channel factory fault
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ChannelFactory_Faulted(object sender, EventArgs e)
        {
            try
            {
                if (sender != null)
                    ((ICommunicationObject)sender).Abort();
            }
            catch (Exception)
            {
            }
            Exception Err;
            if (TryLogin(ServerName, ConnectionURI, UserName, Password, out Err))
                Data_Integration.RestartModel(Client);

        }

        /// <summary>
        /// Report a channel factory open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ChannelFactory_Opened(object sender, EventArgs e)
        {
            if (sender is InstanceContext)
                MM_System_Interfaces.LogError("MM: Server -> Client connection opened.");
            else
                MM_System_Interfaces.LogError("MM: Client -> Server connection opened.");
        }
        #endregion

        #region Data retrieval activities
        /// <summary>
        /// Ping our server to make sure we have a reliable connection. If not, restart it.
        /// </summary>
        public static void PingServer()
        {
            try
            {
                if (Client.InnerChannel.State == CommunicationState.Opened)
                    Client.Ping();
            }
            catch (Exception ex)
            {
                ChannelFactory_Faulted(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Update a timestamp for our component
        /// </summary>
        /// <param name="InType"></param>
        public static void UpdateTimestamp(Type InType)
        {
            if (DataRetrievalInformation.ContainsKey(InType))
                DataRetrievalInformation[InType].LastUpdate = DateTime.Now;
            else
                DataRetrievalInformation.Add(InType, new MM_DataRetrieval_Information(InType));
        }

        /// <summary>
        /// Load our configuration information
        /// </summary>
        public static void LoadMacomberMapConfiguration()
        {
            DateTime StartTime = DateTime.Now;
            String Config = Client.LoadMacomberMapConfiguration();
            if (MM_Repository.xConfiguration != null)
            {
                MM_Repository.xConfiguration = new XmlDocument();
                MM_Repository.xConfiguration.LoadXml(Config);
                MM_Repository.ReinitializeDisplayParameters();
            }
            else
            {
            MM_Repository.Initialize(Config);
            }
            MM_System_Interfaces.LogError("Applied MM client configuration in " + (DateTime.Now - StartTime).ToString());
        }

        /// <summary>
        /// Load our network model data
        /// </summary>
        public static void LoadNetworkModel()
        {

            DateTime StartTime = DateTime.Now;
            //Load our target model
            LoadModelFromSql = false;

            if (!LoadModelFromSql)
            {
                //ModelCollection = Client.LoadDatabaseModels();
                XmlDocument xDoc = new XmlDocument();
                using (MemoryStream mS = new MemoryStream(Client.LoadModel(null)))
                {
                    using (GZipStream gS = new GZipStream(mS, CompressionMode.Decompress))
                    using (StreamReader sRd = new StreamReader(gS))
                    {
                        xDoc.LoadXml(sRd.ReadToEnd());
                        sRd.Close();
                    }
                    mS.Close();
                }

                MM_System_Interfaces.LogError("Loaded network model file in " + (DateTime.Now - StartTime).ToString());
                MM_System_Interfaces.LogError("Retrieving " + xDoc.DocumentElement.ChildNodes.Count.ToString("#,##0") + " elements.");
                var weakRefXDoc = new WeakReference<XmlDocument>(xDoc, true);
                Data_Integration.LoadXMLData(weakRefXDoc, MM_Startup_Form.Coordinates, null);
                xDoc.LoadXml("<root></root>");
                xDoc.RemoveAll();
                xDoc = null;
            }
            else
            {
                MM_Server_Interface.LoadMacomberMapConfiguration();
                Data_Integration.InitializationComplete = false;
                SqlModelLoader sqlModelLoader = new SqlModelLoader();
                sqlModelLoader.ConnectionString = Settings.Default.ModelDatabase;
                sqlModelLoader.LoadStaticRepository();
                Data_Integration.RollUpElementsToSubstation();
                Data_Integration.CommLoaded = true;
                Data_Integration.ModelLoadCompletion = DateTime.Now;

                MM_Startup_Form.Coordinates.SetTopLeftAndBottomRight(new PointF(-105.59f, 44.54f), new PointF(-86.1f, 29.16f));
                MM_Startup_Form.Coordinates.Center = new PointF(-95.51f, 35.94f);
                MM_Startup_Form.Coordinates.UpdateZoom(10);
            }
            //System.GC.Collect();
            System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced, true);
            Data_Integration.UserName = MM_Server_Interface.UserName;
            MM_System_Interfaces.LogError("Retrieved elements in " + (DateTime.Now - StartTime).ToString());
        }

        /// <summary>
        /// Load in our list of operator commands
        /// </summary>
        public static void LoadOperatorCommands()
        {
            DateTime StartTime = DateTime.Now;
            Data_Integration.HandleEMSCommandClear();
            Data_Integration.HandleEMSCommands(Client.GetEMSCommands());
            MM_System_Interfaces.LogError("Retrieved command history in " + (DateTime.Now - StartTime).ToString());
        }

        /// <summary>
        /// Load our operatorship update data
        /// </summary>
        public static void LoadOperatorshipUpdateData()
        {
            DateTime StartTime = DateTime.Now;
            foreach (MM_Operatorship_Update OperatorshipUpdate in Client.GetOperatorshipUpdates())
                UpdateOperatorshipData(OperatorshipUpdate);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Operatorship_Update));

            MM_System_Interfaces.LogError("Applied operatorship updates in " + (DateTime.Now - StartTime).ToString());
        }

        /// <summary>
        /// Register for Macomber Map updates
        /// </summary>
        public static void RegisterForUpdates()
        {
            Client.RegisterCallback(Application.ProductVersion);
            MM_System_Interfaces.LogError("Registered for MM Server Updates.");
        }

        /// <summary>
        /// Load in a one-line from our server
        /// </summary>
        /// <param name="BaseElement"></param>
        public static MM_OneLine_Data LoadOneLine(MM_Element BaseElement)
        {
            if (BaseElement is MM_Substation)
                return Client.LoadOneLineData(BaseElement.Name, MM_OneLine_Data.enumElementType.Substation);
            else if (BaseElement is MM_Contingency)
                return Client.LoadOneLineData(BaseElement.Name, MM_OneLine_Data.enumElementType.BreakerToBreaker);
            else
                return Client.LoadOneLineData(BaseElement.Name, MM_OneLine_Data.enumElementType.CompanyWide);
        }

        /// <summary>
        /// Handle reading in our full line data
        /// </summary>
        public static void LoadLineData()
        {
            MM_Element FoundElem = null;
            MM_Tie FoundTie;
            foreach (MM_Line_Data InLine in Client.GetLineData())
                if (MM_Repository.TEIDs.TryGetValue(InLine.TEID_Ln, out FoundElem) && FoundElem is MM_Line)
                    UpdateLineData(InLine, FoundElem as MM_Line, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Line_Data));
            foreach (MM_ZeroImpedanceBridge_Data InLine in Client.GetZBRData())
                if (MM_Repository.TEIDs.TryGetValue(InLine.TEID_ZBR, out FoundElem) && FoundElem is MM_Line)
                    UpdateZBRData(InLine, FoundElem as MM_Line, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_ZeroImpedanceBridge_Data));
            foreach (MM_Tie_Data InTie in Client.GetTieData())
            {
                if (MM_Repository.Ties.TryGetValue(InTie.ID_TIE, out FoundTie))
                    UpdateTieData(InTie, FoundTie, false);
                else if (MM_Repository.TEIDs.TryGetValue(InTie.TEID_TIE, out FoundElem))
                    UpdateTieData(InTie, (MM_Tie)FoundElem, false);
            }
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Tie_Data));
            if (Data_Integration.InitializationComplete && Program.MM != null)
            {
                Program.MM.ctlNetworkMap.UpdateLineInformation();
            }
        }

        /// <summary>
        /// Load in our relevant SCADA data
        /// </summary>
        public static void LoadSCADAData()
        {
            UpdateSCADAAnalogData(Client.GetSCADAAnalogData());
            UpdateSCADAStatusData(Client.GetSCADAStatusData());
        }
        static List<MM_Unit> units = null; 
        /// <summary>
        /// Find unit by genmom key
        /// </summary>
        /// <param name="genmomKey">GENMOM key</param>
        /// <returns>unit from repo. null if not found</returns>
        public static List<MM_Unit> FindUnits(String genmomKey)
        {
            List <MM_Unit> rt = new List<MM_Unit>(6);
            try
            {
                if (units == null || units.Count != MM_Repository.Units.Count)
                    units = MM_Repository.Units.Values.ToList();
                int matchIndex = 0;

                for (int i = 0; i < units.Count && (matchIndex == 0 || i < matchIndex+25); i++)
                {
                    if (units[i].GenmomName == genmomKey)
                    {
                        rt.Add(units[i]);
                        matchIndex = i;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return rt;
        }

        /// <summary>
        /// Handle reading in our full unit data
        /// </summary>
        public static void LoadUnitData()
        {
            MM_Element FoundElem;
            foreach (MM_Unit_Data InUnit in Client.GetUnitData())
                if (MM_Repository.TEIDs.TryGetValue(InUnit.TEID_Un, out FoundElem) && FoundElem is MM_Unit)
                    UpdateUnitData(InUnit, (MM_Unit)FoundElem, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Unit_Data));

            foreach (MM_Unit_Simulation_Data InUnit in Client.GetUnitSimulationData())
                if (MM_Repository.TEIDs.TryGetValue(InUnit.TEID_Un, out FoundElem) && FoundElem is MM_Unit)
                    UpdateUnitSimulationData(InUnit, (MM_Unit)FoundElem, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Unit_Simulation_Data));

            foreach (MM_Unit_Gen_Data InUnit in Client.GetUnitGenData())
            {
                var units = FindUnits(InUnit.Key);
                if (units != null && units.Count > 0)
                {
                    foreach (var unit in units)
                    {
                        if (units.Count > 0)
                            unit.MultipleNetmomForEachGenmom = true;
                        UpdateUnitGenData(InUnit, unit, false);
                    }
                }
            }
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Unit_Gen_Data));

            foreach (MM_Unit_Control_Status InUnit in Client.GetUnitControlStatus())
                if (MM_Repository.TEIDs.TryGetValue(InUnit.TEID, out FoundElem) && FoundElem is MM_Unit)
                    UpdateUnitControlStatus(InUnit, (MM_Unit)FoundElem, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Unit_Control_Status));

        }

        public static void LoadFlowgateData()
        {
            UpdateFlowgateData(Client.GetFlowgateData());
        }

        /// <summary>
        /// Load our collection of island data
        /// </summary>
        public static void LoadIslandData()
        {
            foreach (MM_Island_Data Island in Client.GetIslandData())
                UpdateIslandData(Island,true);
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.IslandCount] = MM_Repository.Islands.Count;
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Island_Data));

            foreach (MM_Island_Simulation_Data Island in Client.GetIslandSimulationData())
                UpdateIslandSimulationData(Island,true);
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.IslandCount] = MM_Repository.Islands.Count;
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Island_Simulation_Data));
        }

        /// <summary>
        /// Load our training data
        /// </summary>
        public static void LoadTrainingData()
        {
            MM_Repository.Training = new Data_Elements.Training.MM_Training();
            foreach (MM_Training_Level Level in Client.LoadTrainingLevels())
                MM_Repository.Training.Levels.Add(Level.ID, Level);
        }

        /// <summary>
        /// Load our equipment analog data
        /// </summary>
        public static void LoadAnalogData()
        {
            MM_Element FoundElem;
            List<MM_Element_Type> FoundTypes = new List<MM_Element_Type>();
            foreach (MM_Analog_Measurement Meas in Client.GetAnalogMeasurementData())
                if (MM_Repository.TEIDs.TryGetValue(Meas.TEID, out FoundElem))
                {
                    if (!FoundTypes.Contains(FoundElem.ElemType))
                        FoundTypes.Add(FoundElem.ElemType);
                }
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Analog_Measurement));
        }

        /// <summary>
        /// Handle reading in our full transformer data
        /// </summary>
        public static void LoadTransformerData()
        {
            MM_Element FoundElem;
            foreach (MM_Transformer_Data InXF in Client.GetTransformerData())
                if (MM_Repository.TEIDs.TryGetValue(InXF.TEID_Xf, out FoundElem))
                    if (FoundElem is MM_TransformerWinding)
                        UpdateTransformerData(InXF, FoundElem as MM_TransformerWinding, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Transformer_Data));

            foreach (MM_Transformer_PhaseShifter_Data InXF in Client.GetTransformerPhaseShifterData())
                if (MM_Repository.TEIDs.TryGetValue(InXF.TEID_XF, out FoundElem))
                    if (FoundElem is MM_TransformerWinding)
                        UpdateTransformerPhaseShifterData(InXF, FoundElem as MM_TransformerWinding, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Transformer_PhaseShifter_Data));
        }

        /// <summary>
        /// Handle reading in our full shunt compensator data
        /// </summary>
        public static void LoadShuntCompensatorData()
        {
            MM_Element FoundElem;
            foreach (MM_ShuntCompensator_Data InSC in Client.GetShuntCompensatorData())
                if (MM_Repository.TEIDs.TryGetValue(InSC.TEID_CP, out FoundElem) && FoundElem is MM_ShuntCompensator)
                    UpdateShuntCompensatorData(InSC, FoundElem as MM_ShuntCompensator, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_ShuntCompensator_Data));

            foreach (MM_StaticVarCompensator_Data InSVC in Client.GetStaticVarCompensatorData())
                if (MM_Repository.TEIDs.TryGetValue(InSVC.TEID_SVS, out FoundElem) && FoundElem is MM_StaticVarCompensator)
                    UpdateSVCData(InSVC, FoundElem as MM_StaticVarCompensator, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_StaticVarCompensator_Data));
        }

        /// <summary>
        /// Load our system wide data
        /// </summary>
        public static void LoadSystemWideData()
        {
            foreach (MM_SystemWide_Generation_Data GenData in Client.GetSystemWideGenerationData())
                UpdateSystemWideData(GenData);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_SystemWide_Generation_Data));

            foreach (MM_UnitType_Generation_Data GenData in Client.GetUnitTypeGenerationData())
                UpdateUnitTypeData(GenData);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_UnitType_Generation_Data));

            foreach (MM_Interface_Monitoring_Data Interface in Client.GetInterfaceData())
                UpdateInterfaceData(Interface,true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Interface_Monitoring_Data));

            foreach (MM_Simulation_Time SimulatorTime in Client.GetSimulatorTime())
                UpdateSimulatorTime(SimulatorTime);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Simulation_Time));

            UpdateChartingData(Client.GetChartData());
            UpdateLoadForecastData(Client.GetLoadForecastData());
        }

        /// <summary>
        /// Load our violation and outage data
        /// </summary>
        public static void LoadViolationData()
        {
            LoadBasecaseViolationData();
            LoadContingencyViolationData();
            LoadOutageLineData();
            LoadOutageTransformerData();
            LoadOutageUnitData();
        }

        /// <summary>
        /// Load BaseCase violation data
        /// </summary>
        public static void LoadBasecaseViolationData()
        {
            foreach (MM_Basecase_Violation_Data Basecase in Client.GetBasecaseViolationData())
                Data_Integration.CheckAddViolation(CreateViolation(Basecase));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Basecase_Violation_Data));
        }

        /// <summary>
        /// Load Contingency violation data
        /// </summary>
        public static void LoadContingencyViolationData()
        {
            foreach (MM_Contingency_Violation_Data Contingency in Client.GetContingencyVioltationData())
                Data_Integration.CheckAddViolation(CreateViolation(Contingency));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Contingency_Violation_Data));
        }

        /// <summary>
        /// Load Outage line  data
        /// </summary>
        public static void LoadOutageLineData()
        {
            var lineOutage = Client.GetLineOutageData();

            foreach (MM_Outage_Line_Data LineOutage in lineOutage)
            {

                Data_Integration.CheckAddViolation(CreateViolation(LineOutage));
                
            }
            CheckForOutagedLines();
            var lmps = Client.GetLmpData();
            foreach (var data in lmps)   
                UpdateLmpData(data, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_LMP_Data));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Outage_Line_Data));
        }
        static bool isFirstRun = true;
        /// <summary>
        /// Check for outaged lines and create a FO if there is no planned outage.
        /// </summary>
        public static void CheckForOutagedLines()
        {
            if (!MM_Repository.ViolationTypes.ContainsKey("ForcedOutage"))
                MM_Repository.ViolationTypes.Add("ForcedOutage", new MM_AlarmViolation_Type(22, "ForcedOutage", "FO", true, Color.Firebrick, 2, false));

            if (!MM_Repository.ViolationTypes.ContainsKey("UndocumentedOutage"))
                MM_Repository.ViolationTypes.Add("UndocumentedOutage", new MM_AlarmViolation_Type(22, "UndocumentedOutage", "UDO", true, Color.IndianRed, 2, false));

            // go through outaged lines not in this list and create a FO outage
            foreach (var line in MM_Repository.Lines.Values.ToList())
            {
                if (line.NormallyOpened || line.KVLevel.Nominal <= 69 || line.IsZBR || (line.Substation1 != null && !line.Substation1.IsInternal && line.Substation2 != null && !line.Substation2.IsInternal))
                    continue;
                float lineFlow = line.MVAFlow;
                if (line.Estimated_MW[0] > lineFlow)
                    lineFlow = line.Estimated_MW[0];
                if ((line.LineEnergizationState == line.KVLevel.DeEnergized && line.KVLevel.ShowDeEnergized) || (line.LineEnergizationState == line.KVLevel.PartiallyEnergized && line.KVLevel.ShowPartiallyEnergized))
                {
                    if ((line.Violations == null || line.Violations.Count == 0 || !line.Violations.Any(v => v.Value.Type == MM_Repository.ViolationTypes["PlannedOutage"])) && lineFlow < 8)
                    {
                        var viol = MM_AlarmViolation.CreateViolation(line, "No outage for this line at " + lineFlow.ToString("###0.#") + " MVA", MM_Repository.ViolationTypes["UndocumentedOutage"],
                            DateTime.Now, false, null, lineFlow, lineFlow);
                        viol.ReportedStart = DateTime.Now;
                        viol.Type = MM_Repository.ViolationTypes["UndocumentedOutage"];
                        viol.ReportedEnd = DateTime.Now.AddDays(1);
                        if (!isFirstRun && Data_Integration.TimeStarted != null && (DateTime.Now- Data_Integration.TimeStarted.Value).TotalMinutes > 5)
                            viol.New = true;
                        if (line.Violations == null || line.Violations.Count == 0 || !line.Violations.Any(v => v.Value.Type == MM_Repository.ViolationTypes["UndocumentedOutage"]))
                            Data_Integration.CheckAddViolation(viol);
                    }
                    else //if (line.Violations.Count > 0 &&  line.Violations.Any(v => v.Value.Type == MM_Repository.ViolationTypes["PlannedOutage"]))
                    {
                        var viol = line.Violations.FirstOrDefault(v => v.Value.Type == MM_Repository.ViolationTypes["UndocumentedOutage"]);
                        Data_Integration.RemoveViolation(viol.Value);
                    }
                }
                else if (line.Violations.Any(v => v.Value.Type == MM_Repository.ViolationTypes["UndocumentedOutage"]) || lineFlow > 1)
                {
                    var viol = line.Violations.FirstOrDefault(v => v.Value.Type == MM_Repository.ViolationTypes["UndocumentedOutage"]);
                    if (viol.Value != null)
                        Data_Integration.RemoveViolation(viol.Value);
                }
            }
            isFirstRun = false;
        }

        public static void CheckForOutagedUnits()
        {
            if (!MM_Repository.ViolationTypes.ContainsKey("ForcedOutage"))
                MM_Repository.ViolationTypes.Add("ForcedOutage", new MM_AlarmViolation_Type(22, "ForcedOutage", "FO", true, Color.Firebrick, 2, false));

            if (!MM_Repository.ViolationTypes.ContainsKey("UndocumentedOutage"))
                MM_Repository.ViolationTypes.Add("UndocumentedOutage", new MM_AlarmViolation_Type(22, "UndocumentedOutage", "UDO", true, Color.IndianRed, 2, false));

            foreach (var unit in MM_Repository.Units.Values.ToList())
            {
                if (unit.Substation.IsInternal && unit.Substation.IsMarket && unit.RTGenMW < 1 && unit.DispatchMW > 20 && unit.Estimated_MW < 2 && (unit.Violations == null || unit.Violations.Count == 0 || !unit.Violations.Any(v => v.Value.Type == MM_Repository.ViolationTypes["PlannedOutage"])))
                {
                    var viol = MM_AlarmViolation.CreateViolation(unit, "No outage for this unit at " + unit.RTGenMW.ToString("###0.#") + " MW", MM_Repository.ViolationTypes["UndocumentedOutage"],
                           DateTime.Now, false, null, unit.RTGenMW, unit.RTGenMW);
                    viol.ReportedStart = DateTime.Now;
                    viol.Type = MM_Repository.ViolationTypes["UndocumentedOutage"];
                    viol.ReportedEnd = DateTime.Now.AddDays(1);
                    if (!isFirstRun && Data_Integration.TimeStarted != null && (DateTime.Now - Data_Integration.TimeStarted.Value).TotalMinutes > 5)
                        viol.New = true;
                    if (unit.Violations == null || unit.Violations.Count == 0 || !unit.Violations.Any(v => v.Value.Type == MM_Repository.ViolationTypes["UndocumentedOutage"]))
                        Data_Integration.CheckAddViolation(viol);

                }
                else if (unit.Substation.IsMarket && unit.RTGenMW > 5)
                {
                    var viol = unit.Violations.FirstOrDefault(v => v.Value.Type == MM_Repository.ViolationTypes["UndocumentedOutage"]);
                    if (viol.Value != null)
                        Data_Integration.RemoveViolation(viol.Value);
                }
            }
        }

        /// <summary>
        /// Load Outage Transformer  data
        /// </summary>
        public static void LoadOutageTransformerData()
        {
            foreach (MM_Outage_Transformer_Data TransformerOutage in Client.GetTransformerOutageData())
                Data_Integration.CheckAddViolation(CreateViolation(TransformerOutage));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Outage_Transformer_Data));
        }

        /// <summary>
        /// Load Outage Unit  data
        /// </summary>
        public static void LoadOutageUnitData()
        {
            foreach (MM_Outage_Unit_Data UnitOutage in Client.GetUnitOutageData())
                Data_Integration.CheckAddViolation(CreateViolation(UnitOutage));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Outage_Unit_Data));
            CheckForOutagedUnits();
        }

        /// <summary>
        /// Create a new violation based on data from the MM server
        /// </summary>
        /// <param name="inObj"></param>
        /// <returns></returns>
        public static MM_AlarmViolation CreateViolation(Object inObj)
        {
            MM_AlarmViolation OutViol = new MM_AlarmViolation();

            if (MM_Repository.Lines.Count == 0)
                return OutViol;

            if (inObj is MM_Basecase_Violation_Data)
            {
                MM_Basecase_Violation_Data Basecase = (MM_Basecase_Violation_Data)inObj;
                OutViol.EventTime = Basecase.TSTVIOL_MNOL;
                OutViol.New = Basecase.NEW_MNOL;
                if (Basecase.VIOLTYP_MNOL == "HV")
                    OutViol.Type = MM_Repository.ViolationTypes["BasecaseHighVoltage"];
                else if (Basecase.VIOLTYP_MNOL == "LV")
                    OutViol.Type = MM_Repository.ViolationTypes["BasecaseLowVoltage"];
                else if (Basecase.VIOLTYP_MNOL == "BR")
                    OutViol.Type = MM_Repository.ViolationTypes["BasecaseBranch"];
                MM_Repository.TEIDs.TryGetValue(Basecase.MONELE_TEID, out OutViol.ViolatedElement);
            }
            else if (inObj is MM_Contingency_Violation_Data)
            {
                MM_Contingency_Violation_Data Contingency = (MM_Contingency_Violation_Data)inObj;
                OutViol.EventTime = Contingency.TIMWRST;
                OutViol.New = Contingency.NEW;
                if (Contingency.VIOLTYP == "HV")
                    OutViol.Type = MM_Repository.ViolationTypes["ContingencyHighVoltage"];
                else if (Contingency.VIOLTYP == "LV")
                    OutViol.Type = MM_Repository.ViolationTypes["ContingencyLowVoltage"];
                else if (Contingency.VIOLTYP == "BR")
                    OutViol.Type = MM_Repository.ViolationTypes["ContingencyBranch"];
                else
                    OutViol.Type = MM_Repository.ViolationTypes["ContingencyBranch"]; // TODO: need to add other types (
                OutViol.PreCtgValue = Contingency.PRECTG;
                OutViol.PostCtgValue = Contingency.CURVAL;
                OutViol.ReportedStart = Contingency.TIMIN;
                OutViol.ReportedEnd = Contingency.TIMWRST;
                if (Math.Abs(Contingency.WRSPCT - Contingency.PERCLIM2) < .01)
                    OutViol.Text = string.Format("{2} {0} {1:##0}%", Contingency.MONELM, Contingency.WRSPCT, Contingency.VIOLTYP);
                else
                    OutViol.Text = string.Format("{3} {0} {1:##0}% / {2:##0}% ",  Contingency.MONELM, Contingency.PERCLIM2, Contingency.WRSPCT, Contingency.VIOLTYP);
                if (Contingency.MONELE_TEID < 100000 && Contingency.MONELE_TEID > 0)
                {
                    OutViol.NearBusNumber = Contingency.MONELE_TEID;
                    MM_Bus bus = null;
                    if (MM_Repository.BusNumbers.TryGetValue(OutViol.NearBusNumber, out bus))
                    {
                        OutViol.ViolatedElement = bus;
                        var ce = bus.ConnectedElements;
                        if (!bus.Violations.ContainsKey(OutViol))
                            bus.Violations.Add(OutViol, OutViol);
                        if (ce.Length == 1)
                        {
                            OutViol.ViolatedElement = ce[0];
                            if (!ce[0].Violations.ContainsKey(OutViol))
                                ce[0].Violations.Add(OutViol, OutViol);
                        }
                    }
                }
                MM_Contingency con = null;
                try
                {
                    if (MM_Repository.Contingencies.TryGetValue(Contingency.ID_CTG, out con))
                    {
                        OutViol.ViolatedElement = con;
                        OutViol.Contingency = con;

                        if (con.ConElements.Count > 0 && OutViol.ViolatedElement == null)
                        {
                            MM_Repository.TEIDs.TryGetValue(con.ConElements[0], out OutViol.ViolatedElement);
                        }
                        MM_Element elm = null;
                        if (Contingency.MONELE_TEID > 0)
                        {
                            if (MM_Repository.TEIDs.TryGetValue(Contingency.MONELE_TEID, out elm) && elm is MM_Node && ((MM_Node)elm).ConnectedElements.Count > 0)
                            {
                                OutViol.ViolatedElement = ((MM_Node)elm).ConnectedElements[0];
                                if (!(((MM_Node)elm).ConnectedElements[0].Violations.ContainsKey(OutViol)))
                                    ((MM_Node)elm).ConnectedElements[0].Violations.Add(OutViol, OutViol);
                            }
                        }
                    }
                    if (OutViol.ViolatedElement != null && !OutViol.ViolatedElement.Violations.ContainsKey(OutViol))
                    {
                        OutViol.ViolatedElement.Violations.Add(OutViol, OutViol);
                    }
                }
                catch (Exception ex)
                {
                    
                }

            }
            else if (inObj is MM_Outage_Line_Data)
            {
                MM_Outage_Line_Data LineOutage = (MM_Outage_Line_Data)inObj;
                OutViol.EventTime = LineOutage.TStart_QkLn;
                OutViol.ReportedStart = LineOutage.TmpLst_QkLn;
                OutViol.ReportedEnd = LineOutage.TmpLed_QkLn;
                OutViol.Text = LineOutage.OtTyp3_QkLn + " " + LineOutage.OtTyp2_QkLn;
                

                if (LineOutage.TEID_LN != 0)
                    MM_Repository.TEIDs.TryGetValue(LineOutage.TEID_LN, out OutViol.ViolatedElement);
                else
                {
                    lock (MM_Repository.Lines)
                    {
                        try
                        {
                            var line = MM_Repository.Lines.FirstOrDefault(ln => (ln.Key.Contains(LineOutage.Key2) && ln.Key.Contains(LineOutage.Key3)) && (ln.Key.Contains(LineOutage.Station) || ln.Value.Substation1.Name == LineOutage.Station || ln.Value.Substation2.Name == LineOutage.Station));
                            OutViol.ViolatedElement = line.Value;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                if (LineOutage.OTMW_QkLn == 0)
                    OutViol.Text += " " + OutViol.ReportedStart.ToString("MM/dd HH:mm") + " to " + OutViol.ReportedEnd.ToString("MM/dd HH:mm");
                else
                    OutViol.Text = LineOutage.OTMW_QkLn + " MW";
                
                if (LineOutage.OtTyp1_QkLn == "FO")
                    OutViol.Type = MM_Repository.ViolationTypes["ForcedOutage"];
                else
                    OutViol.Type = MM_Repository.ViolationTypes["PlannedOutage"];
            }
            else if (inObj is MM_Outage_Transformer_Data)
            {
                MM_Outage_Transformer_Data XFOutage = (MM_Outage_Transformer_Data)inObj;
                OutViol.EventTime = XFOutage.TStart_QkXf;
                OutViol.ReportedStart = XFOutage.TmpLst_QkXf;
                OutViol.ReportedEnd = XFOutage.TmpLed_QkXf;

                OutViol.Text = XFOutage.OtTyp3_QkXf + " " + XFOutage.OtTyp2_QkXf;

                if (XFOutage.TEID_Xf != 0)
                    MM_Repository.TEIDs.TryGetValue(XFOutage.TEID_Xf, out OutViol.ViolatedElement);
                else
                {
                    MM_Substation sub = null;
                    MM_Repository.Substations.TryGetValue(XFOutage.Station, out sub);

                    if (sub != null && XFOutage != null && sub.Transformers != null)
                    {
                        var xfm = sub.Transformers.FirstOrDefault(xf => xf.Name.Contains(XFOutage.Key2) && xf.Name.Contains(XFOutage.Key3) && xf.Name.Contains(XFOutage.Key4));
                        if (xfm != null)
                            OutViol.ViolatedElement = xfm;
                    }
                }
                if (XFOutage.OTMW_QkXf == 0)
                    OutViol.Text += " " + OutViol.ReportedStart.ToString("MM/dd HH:mm") + " to " + OutViol.ReportedEnd.ToString("MM/dd HH:mm");
                else
                    OutViol.Text = XFOutage.OTMW_QkXf + " MW";
                
                if (XFOutage.OtTyp1_QkXf == "FO")
                    OutViol.Type = MM_Repository.ViolationTypes["ForcedOutage"];
                else 
                    OutViol.Type = MM_Repository.ViolationTypes["PlannedOutage"];
            }
            else if (inObj is MM_Outage_Unit_Data)
            {
                MM_Outage_Unit_Data UnitOutage = (MM_Outage_Unit_Data)inObj;
                OutViol.EventTime = UnitOutage.TStart_QkUn;
                OutViol.ReportedStart = UnitOutage.TmpLst_QkUn;
                OutViol.ReportedEnd = UnitOutage.TmpLed_QkUn;

                OutViol.Text = UnitOutage.OtTyp3_QkUn + " " + UnitOutage.OtTyp2_QkUn;

                if (UnitOutage.TEID_Un != 0)
                    MM_Repository.TEIDs.TryGetValue(UnitOutage.TEID_Un, out OutViol.ViolatedElement);
                else if (UnitOutage.Key4 != null && UnitOutage.Station != null && MM_Repository.Substations != null)
                {
                    MM_Substation sub = null;
                    MM_Repository.Substations.TryGetValue(UnitOutage.Station, out sub);
                    if (sub != null && sub.Units != null)
                    {
                        MM_Unit unit = sub.Units.FirstOrDefault(un => un.Name != null && un.Name.Contains(UnitOutage.Key4));
                        if (unit != null)
                            OutViol.ViolatedElement = unit;
                    }
                }
                if (UnitOutage.OTMW_QkUn == 0)
                    OutViol.Text += " " + OutViol.ReportedStart.ToString("MM/dd HH:mm") + " to " + OutViol.ReportedEnd.ToString("MM/dd HH:mm");
                else
                    OutViol.Text = UnitOutage.OTMW_QkUn + " MW";
            
                if (UnitOutage.OtTyp1_QkUn == "FO")
                    OutViol.Type = MM_Repository.ViolationTypes["ForcedOutage"];
                else 
                    OutViol.Type = MM_Repository.ViolationTypes["PlannedOutage"];
               
            }
            else
            {
            }

            //Pull out ownership, operatorship and violations as needed
            if (OutViol.ViolatedElement != null)
            {
                OutViol.ElemType = OutViol.ViolatedElement.ElemType;
                OutViol.Owner = OutViol.ViolatedElement.Owner;
                OutViol.Operator = OutViol.ViolatedElement.Operator;
                OutViol.KVLevel = OutViol.ViolatedElement.KVLevel;
            }
            else
                return null;
            return OutViol;
        }

        /// <summary>
        /// Handle reading in our full breaker switch data
        /// </summary>
        public static void LoadBreakerSwitchData()
        {
            MM_Element FoundElem;
            foreach (MM_BreakerSwitch_Data InBreakerSwitch in Client.GetBreakerSwitchData())
                if (MM_Repository.TEIDs.TryGetValue(InBreakerSwitch.TEID_CB, out FoundElem) && FoundElem is MM_Breaker_Switch)
                    UpdateBreakerSwitchData(InBreakerSwitch, (MM_Breaker_Switch)FoundElem, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_BreakerSwitch_Data));

            foreach (MM_Synchroscope_Data InSych in Client.GetSynchroscopeData())
                if (MM_Repository.TEIDs.TryGetValue(InSych.TEID_CB, out FoundElem) && FoundElem is MM_Breaker_Switch)
                    UpdateSynchroscopeData(InSych, (MM_Breaker_Switch)FoundElem, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Synchroscope_Data));

            /* foreach (MM_State_Measurement InMeas in Client.GetStateMeasurementData())
                 if (MM_Repository.TEIDs.TryGetValue(InMeas.TEID_Analog, out FoundElem) && FoundElem is MM_Breaker_Switch)
                     UpdateStateData(InMeas, FoundElem as MM_Breaker_Switch, false);*/
        }

        /// <summary>
        /// Handle reading in our full bus data
        /// </summary>
        public static void LoadBusData()
        {
            MM_Bus FoundBus;
            MM_Element FoundElem = null;
            MM_Element sub = null;

            foreach (MM_Bus_Data InLine in Client.GetBusData())
            {
                MM_Repository.TEIDs.TryGetValue(InLine.TEID_Nd, out FoundElem);

                MM_Repository.TEIDs.TryGetValue(InLine.TEID_St, out sub);

                if (sub == null && FoundElem == null) // can't find this substation in our model.
                    continue;
                if (sub == null)
                    sub = FoundElem.Substation;

                if (!MM_Repository.BusNumbers.TryGetValue(InLine.Bus_Num, out FoundBus))
                {
                    if (FoundElem != null && FoundElem is MM_Node && ((MM_Node)FoundElem).AssociatedBus != null)
                    {
                        MM_Node FoundNode = (MM_Node)FoundElem;
                        FoundNode.AssociatedBus.BusNumber = InLine.Bus_Num;
                        MM_Repository.BusNumbers.Add(InLine.Bus_Num, FoundBus = FoundNode.AssociatedBus);
                    }
                    else
                    {
                        MM_Repository.BusNumbers.Add(InLine.Bus_Num, FoundBus = new MM_Bus(InLine.Bus_Num));
                    }
                }
                //If we have a TEID associated with a node, try and update that one.
                UpdateBusData(InLine, FoundBus, FoundElem, false);
            }
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Bus_Data));
            // remove inactive buses
            try
            {
                var busList = MM_Repository.Busbars.Values.ToList();
                for (int i = 0; i < busList.Count; i++)
                {
                    var bus = busList[i];
                    if (bus.BusNumber <= 0 && bus.Name != null && bus.Substation != null)
                    {
                        if (!MM_Repository.Busbars.Remove(bus.Name))
                            MM_Repository.Busbars.Remove(bus.Substation.Name + "." + bus.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
        }

        /// <summary>
        /// Handle reading in our full load data
        /// </summary>
        public static void LoadLoadData()
        {
            MM_Element FoundElem;
            foreach (MM_Load_Data InLoad in Client.GetLoadData())
                if (MM_Repository.TEIDs.TryGetValue(InLoad.TEID_Ld, out FoundElem) && FoundElem is MM_Load)
                    UpdateLoadData(InLoad, (MM_Load)FoundElem, false);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Load_Data));
        }

        /// <summary>
        /// Load in our collection of notes
        /// </summary>
        public static void LoadNotes()
        {
            Dictionary<int, bool> NotesToRemove = new Dictionary<int, bool>();
            foreach (int Note in MM_Repository.Notes.Keys)
                NotesToRemove.Add(Note, true);

            MM_Note[] CurrentNotes = Client.LoadNotes();
            MM_Note FoundNote;
            foreach (MM_Note Note in CurrentNotes)
            {
                if (!MM_Repository.Notes.TryGetValue(Note.ID, out FoundNote))
                    Data_Integration.HandleNoteEntry(Note);
                else if (!MM_Comparable.IsDataIdentical(Note, FoundNote))
                    Data_Integration.HandleNoteModification(Note);
                NotesToRemove.Remove(Note.ID);
            }

            //Now, check and remove any old notes
            foreach (int NoteToRemove in NotesToRemove.Keys)
                Data_Integration.RemoveNoteEntry(NoteToRemove);
        }
        #endregion

        #region Data updates
        /// <summary>
        /// Update an individual line data point
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="LineData"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateLineData(MM_Line_Data LineData, MM_Line Line, bool TriggerValueChange)
        {
            if (Line.NodeTEIDs[0] == 0)
                Line.NodeTEIDs[0] = LineData.TEID_End1;
            else if (Line.NodeTEIDs[1] == 0)
                Line.NodeTEIDs[1] = LineData.TEID_End1;

            int FirstSub = 1;
            if (Line.NodeTEIDs[0] == LineData.TEID_End1) // to end
                FirstSub = 0;
            else if (Line.NodeTEIDs[1] == LineData.TEID_End1) // from end
                FirstSub = 1;
            else
            { }// Console.WriteLine("Error: Can't find TEID {0} for line {1}/{2}", LineData.TEID_End1, Line.TEID, Line.Name);
            

            MM_Element element = null;
            if (MM_Repository.TEIDs.TryGetValue(LineData.TEID_End1, out element))
            {
                if (element.Substation.Name == Line.Substation1.Name)
                    FirstSub = 0;
                else
                    FirstSub = 1;
            }
            int SecondSub = 1 - FirstSub;
            //UpdateProperty(Line,"IsZBR", false, TriggerValueChange);
            UpdateProperty(Line,"Estimated_MVA",FirstSub, LineData.MVA_End1, TriggerValueChange);
            UpdateProperty(Line,"Estimated_MVA",SecondSub, LineData.MVA_End2, TriggerValueChange);
            UpdateProperty(Line,"Estimated_MVAR",FirstSub, LineData.R_End1, TriggerValueChange);
            UpdateProperty(Line,"Estimated_MVAR",SecondSub, LineData.R_End2, TriggerValueChange);
            UpdateProperty(Line,"Estimated_MW",FirstSub, LineData.W_End1, TriggerValueChange);
            UpdateProperty(Line,"Estimated_MW",SecondSub, LineData.W_End2, TriggerValueChange);
            UpdateProperty(Line,"Telemetered_MW",FirstSub, LineData.WMeas_End1, TriggerValueChange);
            UpdateProperty(Line,"Telemetered_MW",SecondSub, LineData.WMeas_End2, TriggerValueChange);
            UpdateProperty(Line,"Telemetered_MVAR",FirstSub, LineData.RMeas_End1, TriggerValueChange);
            UpdateProperty(Line,"Telemetered_MVAR",SecondSub, LineData.RMeas_End2, TriggerValueChange);
            UpdateProperty(Line,"Telemetered_MVA",FirstSub, ComputeMVA(LineData.WMeas_End1, LineData.RMeas_End1), TriggerValueChange);
            UpdateProperty(Line,"Telemetered_MVA",SecondSub, ComputeMVA(LineData.WMeas_End2, LineData.RMeas_End2), TriggerValueChange);
            UpdateProperty(Line,"NearBusNumber", LineData.End1_Bus, TriggerValueChange);
            UpdateProperty(Line,"FarBusNumber", LineData.End2_Bus, TriggerValueChange);
            UpdateProperty(Line,"Limits",0, LineData.LIMIT1, TriggerValueChange);
            UpdateProperty(Line,"Limits",1, LineData.LIMIT2, TriggerValueChange);
            UpdateProperty(Line,"Limits",2, LineData.LIMIT3, TriggerValueChange);
            //UpdateProperty(Line, "OpenEnd1", LineData.Open_End1, TriggerValueChange);
            //UpdateProperty(Line, "OpenEnd2", LineData.Open_End2, TriggerValueChange);

            //If we're tracking line thermal limits, handle accordingly.
            if (Line.KVLevel.MonitorEquipment && Line.LinePercentage >= Line.KVLevel.ThermalAlert && MM_Repository.ViolationTypes.ContainsKey("ThermalAlert") && !(Line is MM_Tie) && Line.Substation1 != null && Line.Substation1.IsInternal)
                Data_Integration.AddUpdateMonitoringAlert(Line, MM_Repository.ViolationTypes["ThermalAlert"], Line.LinePercentageText);
            else if (Line.KVLevel.MonitorEquipment && Line.LinePercentage >= Line.KVLevel.ThermalWarning && MM_Repository.ViolationTypes.ContainsKey("ThermalWarning") && ((Line.Substation1 != null && Line.Substation1.IsInternal) && (Line.Substation2 != null && Line.Substation2.IsInternal)))
                Data_Integration.AddUpdateMonitoringAlert(Line, MM_Repository.ViolationTypes["ThermalWarning"], Line.LinePercentageText);
            else
                Data_Integration.RemoveMonitoringAlert(Line);
        }

        /// <summary>
        /// Update a SCADA analog value
        /// </summary>
        /// <param name="AnalogData"></param>
        public static void UpdateSCADAAnalogData(MM_Scada_Analog[] AnalogData)
        {
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Scada_Analog));
        }

        /// <summary>
        /// Update Flowgate data
        /// </summary>
        /// <param name="flowgateData"></param>
        public static void UpdateFlowgateData(MM_Flowgate_Data[] flowgateData)
        {
            for (int i = 0; i < flowgateData.Length; i++)
            {
                MM_Contingency con = null;
                if (flowgateData[i].ID != null)
                    MM_Repository.Contingencies.TryGetValue(flowgateData[i].ID, out con);

                if (con != null && flowgateData[i] != null)
                {
                    MM_Flowgate fg = con as MM_Flowgate;
                    UpdateFlowgateData(flowgateData[i], fg, false);
                }
            }
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Flowgate_Data));
        }


        public static void UpdateFlowgateData(MM_Flowgate_Data flowgateData, MM_Flowgate fg, bool TriggerValueChange = false)
        {
            if (fg == null || flowgateData == null)
                return;
            UpdateProperty(fg, "Idc", flowgateData.Idc, TriggerValueChange);
            UpdateProperty(fg, "Lodf", flowgateData.Lodf, TriggerValueChange);
            UpdateProperty(fg, "EmerLimit", flowgateData.EmerLimit, TriggerValueChange);
            UpdateProperty(fg, "NormLimit", flowgateData.NormLimit, TriggerValueChange);
            UpdateProperty(fg, "IROLLimit", flowgateData.IROLLimit, TriggerValueChange);
            UpdateProperty(fg, "PCTGFlow", flowgateData.PCTGFlow, true);
            UpdateProperty(fg, "Reason", flowgateData.Reason, TriggerValueChange);
            UpdateProperty(fg, "Hint", flowgateData.Hint, TriggerValueChange);
            UpdateProperty(fg, "FlowgateType", flowgateData.Type, TriggerValueChange);

        }


        /// <summary>
        /// Update a SCADA status point
        /// </summary>
        /// <param name="StatusData"></param>
        public static void UpdateSCADAStatusData(MM_Scada_Status[] StatusData)
        {
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Scada_Status));
        }

        /// <summary>
        /// Update our load forecast data
        /// </summary>
        /// <param name="LoadForecastData"></param>
        public static void UpdateLoadForecastData(MM_Load_Forecast_Data[] LoadForecastData)
        {
            Data_Integration.LoadForecastData = LoadForecastData;
            Data_Integration.LFDate = DateTime.Now;
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Load_Forecast_Data));
        }

        /// <summary>
        /// Load our load forecast data
        /// </summary>
        public static void LoadLoadForecastData()
        {
            UpdateLoadForecastData(Client.GetLoadForecastData());
        }

        /// <summary>
        /// Load our charting data
        /// </summary>
        public static void LoadChartingData()
        {
            UpdateChartingData(Client.GetChartData());
        }

        /// <summary>
        /// Update our load forecast data
        /// </summary>
        /// <param name="ChartData"></param>
        public static void UpdateChartingData(MM_Chart_Data[] ChartData)
        {
            Data_Integration.ChartData = ChartData;
            Data_Integration.ChartDate = DateTime.Now;
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Chart_Data));
        }

        /// <summary>
        /// Update data for a bus
        /// </summary>
        /// <param name="InBus"></param>
        /// <param name="BusElem"></param>
        /// <param name="TargetElement"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateBusData(MM_Bus_Data InBus, MM_Bus BusElem, MM_Element TargetElement, bool TriggerValueChange)
        {
            if (MM_Repository.Lines == null || MM_Repository.Lines.Count == 0)
                return;

            try
            {
                //First, if we have a node, make sure it's linked up properly.
                if (TargetElement != null)
                    TargetElement.NearBusNumber = InBus.Bus_Num;
                MM_Element FoundStation = null;

                if (TargetElement != null && TargetElement is MM_Node)
                {
                    MM_Node node = (MM_Node)TargetElement;
                    FoundStation = node.Substation;
                    BusElem.Node = node;
                    node.AssociatedBus = BusElem;

                    foreach (var el in node.ConnectedElements)
                    {
                        el.NearBusNumber = InBus.Bus_Num;
                    }
                }
                MM_Element busStation = null;

                if (MM_Repository.TEIDs.TryGetValue(InBus.TEID_St, out busStation) && busStation is MM_Substation)
                {
                    UpdateProperty(BusElem, "Substation", (MM_Substation)busStation, TriggerValueChange);
                    if (busStation != null && !((MM_Substation)busStation).BusbarSections.Any(bs => bs.BusNumber == InBus.Bus_Num)) // do this for the popup menu
                    {
                        if (((MM_Substation)busStation).BusbarSections.Contains(BusElem))
                            ((MM_Substation)busStation).BusbarSections.Remove(BusElem);
                        ((MM_Substation)busStation).BusbarSections.Add(BusElem);
                    }
                    // try and set substation bus to the highest voltage bus data we find on this station
                    //TODO: MEL - unclear what KVLevel assoc w/ substation
                    if (busStation.KVLevel != null)
                    if (float.IsNaN(busStation.NearVoltage) || InBus.NomKv + 1 >= busStation.NearVoltage || InBus.NomKv + 1 >= busStation.KVLevel.Nominal)
                        busStation.NearBusNumber = InBus.Bus_Num;
                }
                else if (FoundStation != null)
                {
                    UpdateProperty(BusElem, "Substation", (MM_Substation)FoundStation, TriggerValueChange);

                    // try and set substation bus to the highest voltage bus data we find on this station
                    if (float.IsNaN(FoundStation.NearVoltage) || InBus.NomKv + 1 >= FoundStation.NearVoltage || InBus.NomKv + 1 >= FoundStation.KVLevel.Nominal)
                        FoundStation.NearBusNumber = InBus.Bus_Num;

                    if (FoundStation != null && !((MM_Substation)FoundStation).BusbarSections.Any(bs => bs.BusNumber == InBus.Bus_Num)) // do this for the popup menu
                    {
                        if (((MM_Substation)FoundStation).BusbarSections.Contains(BusElem))
                            ((MM_Substation)FoundStation).BusbarSections.Remove(BusElem);
                        ((MM_Substation)FoundStation).BusbarSections.Add(BusElem);
                    }
                }


                if (busStation != null && FoundStation != null && busStation.Name != FoundStation.Name)
                {
                    // something not right here. Our MM model doesn't match real-time EMS.
                    MM_System_Interfaces.LogError(new ApplicationException("Model data doesn't match XML model file."));
                    return;
                }

                UpdateProperty(BusElem, "IslandNumber", InBus.Island_ID, TriggerValueChange);
                UpdateProperty(BusElem, "PerUnitVoltage", InBus.V_Pu, TriggerValueChange);
                UpdateProperty(BusElem, "Estimated_Angle", InBus.Angle, TriggerValueChange);
                UpdateProperty(BusElem, "Estimated_kV", InBus.V_Pu * InBus.NomKv, TriggerValueChange);
                BusElem.KVLevel = MM_Repository.FindKVLevel(InBus.NomKv.ToString());

                //BusElem.Substation = (MM_Substation)FoundStation;

                if (BusElem.Name == null && FoundStation != null)
                    BusElem.Name = String.Format("{0}_{1}", BusElem.Substation.Name, BusElem.KVLevel);

                UpdateProperty(BusElem, "Dead", InBus.Dead, TriggerValueChange);
                UpdateProperty(BusElem, "Open", InBus.Open, TriggerValueChange);


                if (MM_Repository.ViolationTypes.ContainsKey("VoltageAlert") && MM_Repository.ViolationTypes.ContainsKey("VoltageWarning"))
                {
                    if (BusElem.KVLevel.MonitorEquipment && !BusElem.Dead && BusElem.PerUnitVoltage != 0 && BusElem.Substation != null && BusElem.Substation.IsInternal && (BusElem.PerUnitVoltage <= BusElem.KVLevel.LowVoltageAlert || BusElem.PerUnitVoltage >= BusElem.KVLevel.HighVoltageAlert))
                        Data_Integration.AddUpdateMonitoringAlert(BusElem, MM_Repository.ViolationTypes["VoltageAlert"], BusElem.VoltageText);
                    else if (BusElem.KVLevel.MonitorEquipment && !BusElem.Dead && (BusElem.PerUnitVoltage <= BusElem.KVLevel.LowVoltageWarning || BusElem.PerUnitVoltage >= BusElem.KVLevel.HighVoltageWarning))
                        Data_Integration.AddUpdateMonitoringAlert(BusElem, MM_Repository.ViolationTypes["VoltageWarning"], BusElem.VoltageText);
                    else
                        Data_Integration.RemoveMonitoringAlert(BusElem);
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
        }

        /// <summary>
        /// Update data for a load point
        /// </summary>
        /// <param name="InLoad"></param>
        /// <param name="LoadElem"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateLoadData(MM_Load_Data InLoad, MM_Load LoadElem, bool TriggerValueChange)
        {
            UpdateProperty(LoadElem,"Estimated_MVAR", InLoad.R_Ld, TriggerValueChange);
            UpdateProperty(LoadElem,"Estimated_MW", InLoad.W_Ld, TriggerValueChange);
            UpdateProperty(LoadElem,"Estimated_MVA", ComputeMVA(InLoad.W_Ld, InLoad.R_Ld), TriggerValueChange);
            UpdateProperty(LoadElem,"Telemetered_MW", InLoad.WMeas_Ld, TriggerValueChange);
            UpdateProperty(LoadElem,"Telemetered_MVAR", InLoad.RMeas_Ld, TriggerValueChange);
            UpdateProperty(LoadElem,"Telemetered_MVA", ComputeMVA(InLoad.WMeas_Ld, InLoad.RMeas_Ld), TriggerValueChange);

            //Update our connected bus, only if we are closed
            UpdateProperty(LoadElem,"NearBusNumber", InLoad.Open_Ld ? -1 : InLoad.Conn_Bus, TriggerValueChange);
            UpdateProperty(LoadElem,"ManualTarget", InLoad.WM_Ld, TriggerValueChange);
            UpdateProperty(LoadElem,"RM", InLoad.RM_Ld, TriggerValueChange);
            UpdateProperty(LoadElem,"RemoveEnabled", InLoad.RMVEnabl_Ld, TriggerValueChange);
            UpdateProperty(LoadElem,"Manual", InLoad.Manual_Ld, TriggerValueChange);
            UpdateProperty(LoadElem,"Removed", InLoad.Remove_Ld, TriggerValueChange);
            UpdateProperty(LoadElem,"WLoad", InLoad.WLoad_LdArea, TriggerValueChange);
            UpdateProperty(LoadElem,"LoadMaximum", InLoad.WMX_Ld, TriggerValueChange);
        }
        #endregion

        /// <summary>
        /// Update data for an island
        /// </summary>
        /// <param name="IslandData"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateIslandData(MM_Island_Data IslandData, bool TriggerValueChange)
        {
            MM_Island FoundIsland;
            if (!MM_Repository.Islands.TryGetValue(IslandData.Isl_Num, out FoundIsland))
            {
                MM_Repository.Islands.Add(IslandData.Isl_Num, FoundIsland = new MM_Island(IslandData.Isl_Num));
                Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.IslandCount] = MM_Repository.Islands.Count;
                Data_Integration.HandleIslandAdd(FoundIsland);
            }            
            UpdateProperty(FoundIsland,"Generation", IslandData.WGen, TriggerValueChange);
            UpdateProperty(FoundIsland,"Load", IslandData.WLoad,TriggerValueChange);
            UpdateProperty(FoundIsland,"MWLosses", IslandData.WLoss,TriggerValueChange);
            UpdateProperty(FoundIsland, "Frequency", IslandData.Frequency, TriggerValueChange);
        }

        /// <summary>
        /// Update data for an island
        /// </summary>
        /// <param name="IslandData"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateIslandSimulationData(MM_Island_Simulation_Data IslandData, bool TriggerValueChange)
        {
            MM_Island FoundIsland;
            if (!MM_Repository.Islands.TryGetValue(IslandData.Isl_Num, out FoundIsland))
            {
                MM_Repository.Islands.Add(IslandData.Isl_Num, FoundIsland = new MM_Island(IslandData.Isl_Num));
                Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.IslandCount] = MM_Repository.Islands.Count;
            }
            UpdateProperty(FoundIsland, "FrequencyControl", IslandData.InProg_Island, TriggerValueChange);
            UpdateProperty(FoundIsland, "ManualTarget", IslandData.TarMan_Island, TriggerValueChange);
        }

        /// <summary>
        /// Update our simulator time
        /// </summary>
        /// <param name="SimulatorTime"></param>
        public static void UpdateSimulatorTime(MM_Simulation_Time SimulatorTime)
        {
            Data_Integration.CurrentTime = SimulatorTime.Simulation_Time.ToLocalTime();
            Data_Integration.SimulatorStatus = SimulatorTime.Status;
        }

        /// <summary>
        /// Update our shunt compensator
        /// </summary>
        /// <param name="InSCData"></param>
        /// <param name="ShuntCompensator"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateShuntCompensatorData(MM_ShuntCompensator_Data InSCData, MM_ShuntCompensator ShuntCompensator, bool TriggerValueChange)
        {
            UpdateProperty(ShuntCompensator,"Estimated_MVAR", InSCData.R_CP,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"Telemetered_MVAR", InSCData.RMeas_CP,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"Nominal_MVAR", InSCData.MRNom_CP,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"Open", InSCData.Open_CP,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"NearBusNumber", InSCData.Conn_Bus,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"FarBusNumber", InSCData.Reg_Bus,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"AVR", InSCData.AVR_CP,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"RemoveEnabled", InSCData.RMVENABL_CP,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"Removed", InSCData.REMOVE_CP,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"Bank", InSCData.Bank_CP,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"ManualTargetMVAR", InSCData.DisTarg_CP,TriggerValueChange);
            UpdateProperty(ShuntCompensator,"ManualTarget", InSCData.VTargMan_CP,TriggerValueChange);
        }

        /// <summary>
        /// Update a breaker's state information
        /// </summary>
        /// <param name="InState"></param>
        /// <param name="InBreaker"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateStateData(MM_State_Measurement InState, MM_Breaker_Switch InBreaker, bool TriggerValueChange)
        {
            UpdateProperty(InBreaker, "BreakerState", (InState.Open_Stat ? MM_Breaker_Switch.BreakerStateEnum.Open : MM_Breaker_Switch.BreakerStateEnum.Closed), TriggerValueChange);
            UpdateProperty(InBreaker, "Good", InState.Good_Stat, TriggerValueChange);
        }

        /// <summary>
        /// Update our unit data
        /// </summary>
        /// <param name="InUnitData"></param>
        /// <param name="InUnit"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateUnitData(MM_Unit_Data InUnitData, MM_Unit InUnit, bool TriggerValueChange)
        {
            UpdateProperty(InUnit,"Estimated_MVAR", InUnitData.R_Un,TriggerValueChange);
            UpdateProperty(InUnit,"Estimated_MW", InUnitData.W_Un,TriggerValueChange);
            UpdateProperty(InUnit,"Estimated_MVA", ComputeMVA(InUnitData.W_Un, InUnitData.R_Un),TriggerValueChange);
            UpdateProperty(InUnit,"Telemetered_MW", InUnitData.WMeas_Un,TriggerValueChange);
            UpdateProperty(InUnit,"Telemetered_MVAR", InUnitData.RMeas_Un,TriggerValueChange);
            UpdateProperty(InUnit,"Telemetered_MVA", ComputeMVA(InUnitData.WMeas_Un, InUnitData.RMeas_Un),TriggerValueChange);
            UpdateProperty(InUnit,"Open", InUnitData.Open_Un,TriggerValueChange);
            UpdateProperty(InUnit,"NoAVR", InUnitData.NoAVR_Un,TriggerValueChange);
            UpdateProperty(InUnit,"NoPSS", InUnitData.NoPSS_Un,TriggerValueChange);
            UpdateProperty(InUnit,"NearBusNumber", InUnitData.Conn_Bus,TriggerValueChange);            
        }


        /// <summary>
        /// Update our unit simulation data
        /// </summary>
        /// <param name="InUnitData"></param>
        /// <param name="InUnit"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateUnitSimulationData(MM_Unit_Simulation_Data InUnitData, MM_Unit InUnit, bool TriggerValueChange)
        {
            UpdateProperty(InUnit, "Frequency", InUnitData.Fhz_Un, TriggerValueChange);
            UpdateProperty(InUnit, "FrequencyControl", InUnitData.FrqCtrl_Un, TriggerValueChange);
			UpdateProperty(InUnit, "Unit_Status", InUnitData.GCPStatus_Un, TriggerValueChange);
            UpdateProperty(InUnit, "RemovedStatus", InUnitData.Remove_Un, TriggerValueChange);
            UpdateProperty(InUnit, "isAGC", InUnitData.AGC_Plc, TriggerValueChange);
            UpdateProperty(InUnit, "isLocal", InUnitData.WLocal_Un, TriggerValueChange);
            UpdateProperty(InUnit, "RampRate", InUnitData.Dwdtloc_Un, TriggerValueChange);
            UpdateProperty(InUnit, "Desired_MW", InUnitData.Wdesloc_Un, TriggerValueChange);
            UpdateProperty(InUnit, "ManVoltageTarg", InUnitData.VTargman_Un, TriggerValueChange);
            UpdateProperty(InUnit, "VoltageTarget", InUnitData.Distarg_Un, TriggerValueChange);
            UpdateProperty(InUnit, "FreqTarget", InUnitData.FrqTarg_SIsl, TriggerValueChange);
            UpdateProperty(InUnit, "FreqToler", InUnitData.FToler_SIsl, TriggerValueChange);

        }

        public static void UpdateLmpData(MM_LMP_Data data, bool TriggerValueChange)
        {
            MM_Substation sub = null;
            try
            {
                MM_Repository.Substations.TryGetValue(data.Station, out sub);
                if (sub != null)
                {
                    if (!string.IsNullOrWhiteSpace(data.PnodeName))
                    {
                        var load = sub.Loads.FirstOrDefault(l => l.Name == data.PnodeName);
                        if (load != null)
                            UpdateProperty(load, "Lmp", data.Lmp, TriggerValueChange);
                           
                    }
                    else if (!string.IsNullOrWhiteSpace(data.ResourceName))
                    {
                        var unit = sub.Units.FirstOrDefault(u => u.MarketResourceName == data.ResourceName);
                        if (unit != null)
                            UpdateProperty(unit, "LMP", data.Lmp, TriggerValueChange);
                    }
                }
                
            }
            catch (Exception)
            {

            }
            
        }

        /// <summary>
        /// Share our unit status with our core system
        /// </summary>
        /// <param name="UnitStatus"></param>
        public static void PushUnitStatus(MM_Unit_Control_Status UnitStatus)
        {
            try
            {
                if (Client.State != CommunicationState.Faulted || Client.State != CommunicationState.Closed || Client.State != CommunicationState.Closing)
            Client.UpdateUnitControlStatusInformation(UnitStatus);
            }
            catch (Exception ex)
            {}
        }

        /// <summary>
        /// Update our unit status pushed from our server
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="InUnit"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateUnitControlStatus(MM_Unit_Control_Status Status, MM_Unit InUnit, bool TriggerValueChange)
        {
            if (!Status.IsOwner)
            {
                InUnit.UnitStatus.StartRamping = Status.StartRamping;
                InUnit.UnitStatus.StartFrequencyControl = Status.StartFrequencyControl;
                InUnit.UnitStatus.CheckedFrequencyControl = Status.CheckedFrequencyControl;
                InUnit.UnitStatus.UnitController = Status.UnitController;
                InUnit.UnitStatus.Online = Status.Online;
                InUnit.UnitStatus.InAGC = Status.InAGC;
                InUnit.UnitStatus.UnitStatus = Status.UnitStatus;
                InUnit.UnitStatus.BaseRPM = Status.BaseRPM;
                InUnit.UnitStatus.BaseVoltage = Status.BaseVoltage;
                InUnit.UnitStatus.OpenTime = Status.OpenTime;
            }
        }


        /// <summary>
        /// Update our unit data
        /// </summary>
        /// <param name="InUnitData"></param>
        /// <param name="InUnit"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateUnitGenData(MM_Unit_Gen_Data InUnitData, MM_Unit InUnit, bool TriggerValueChange)
        {
            try
            {
                UpdateProperty(InUnit, "RegUp", InUnitData.CLREGUP_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "RegDown", InUnitData.CLREGDN_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "EcoMax", InUnitData.ECOMX_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "EcoMin", InUnitData.ECOMN_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "EmerMax", InUnitData.NCEMGMX_UNIT, TriggerValueChange); 
                UpdateProperty(InUnit, "EmerMin", InUnitData.NCEMGMN_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "MosMax", InUnitData.MOSCAPMX_UNIT, TriggerValueChange); 
                UpdateProperty(InUnit, "MosMin", InUnitData.MOSCAPMN_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "SetPoint", InUnitData.SPPSETP_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "RegulationDeployed", InUnitData.REGDEP_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "RampRateUp", InUnitData.MOSRRUP_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "ClearedSpinningCapacity", InUnitData.CLSPINCP_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "DispatchMW", InUnitData.MOSBP_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "SpinningCapacity", InUnitData.RVSP_UNIT, TriggerValueChange); 
                UpdateProperty(InUnit, "RampedSetPoint", InUnitData.SETPTRMP_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "CMode", InUnitData.UDSCMODE_UNIT, TriggerValueChange);
                UpdateProperty(InUnit, "RTGenMW", InUnitData.GEN_UNIT, TriggerValueChange);
				UpdateProperty(InUnit, "HASL", InUnitData.HASL_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "LASL", InUnitData.LASL_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "HSL", InUnitData.HSL_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "LSL", InUnitData.LSL_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "HEL", InUnitData.HEL_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "LEL", InUnitData.LEL_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "HDL", InUnitData.HDL_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "LDL", InUnitData.LDL_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "LMP", InUnitData.LMP_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "Physical_Responsive_Online", InUnitData.PRCGN_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "Physical_Responsive_Sync", InUnitData.PRCHSYN_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "Blackstart_Capacity", InUnitData.RVBLKS_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "RMR_Capacity", InUnitData.RVRMRN_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "Spinning_Capacity", InUnitData.RVSP_UNIT, TriggerValueChange);
	            UpdateProperty(InUnit, "SCED_Basepoint", InUnitData.SCEDBP_UNIT, TriggerValueChange);
                if (InUnit.EcoMax > InUnit.MosMax && !InUnit.MultipleNetmomForEachGenmom)
                    InUnit.MosMax = InUnit.EcoMax;

                if (InUnit.EcoMin < InUnit.MosMin && !InUnit.MultipleNetmomForEachGenmom)
                    InUnit.MosMin = InUnit.EcoMin;

                if (InUnit.MosMax > InUnit.EmerMax && !InUnit.MultipleNetmomForEachGenmom)
                    InUnit.EmerMax = InUnit.MosMax;

                if (float.IsNaN(InUnit.MaxCapacity) || InUnit.MaxCapacity == 0 && !InUnit.MultipleNetmomForEachGenmom)
                    InUnit.MaxCapacity = InUnit.MosMax;
                if ((float.IsNaN(InUnit.MaxCapacity) || InUnit.MaxCapacity == 0) && !InUnit.MultipleNetmomForEachGenmom)
                    InUnit.MaxCapacity = InUnit.EmerMax;
                if (InUnit.RTGenMW > InUnit.MaxCapacity && !InUnit.MultipleNetmomForEachGenmom)
                    InUnit.MaxCapacity = InUnit.RTGenMW;

            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
        }

        /// <summary>
        /// Update our transformer data
        /// </summary>
        /// <param name="InXfData"></param>
        /// <param name="InXFW"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateTransformerPhaseShifterData(MM_Transformer_PhaseShifter_Data InXfData, MM_TransformerWinding InXFW, bool TriggerValueChange)
        {
            UpdateProperty(InXFW.Transformer, "PhaseShifterData", InXfData, TriggerValueChange);
            if (!MM_Repository.TEIDs.ContainsKey(InXfData.TEID_PS))
                MM_Repository.TEIDs.Add(InXfData.TEID_PS, InXFW.Transformer);
        }

        /// <summary>
        /// Update our transformer data
        /// </summary>
        /// <param name="InXfData"></param>
        /// <param name="InXFW"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateTransformerData(MM_Transformer_Data InXfData, MM_TransformerWinding InXFW, bool TriggerValueChange)
        {
            MM_TransformerWinding End1 = null, End2 = null;
            if (InXFW == null || InXFW.Transformer == null)
                return;
            MM_Transformer Transformer = InXFW.Transformer;
            if (InXFW.Transformer.Windings[0].WindingNodeTEID == InXfData.TEID_End1 || InXFW.Transformer.Windings[0].WindingNodeTEID == InXfData.TEID_End1-2300000)
            {
                End1 = InXFW.Transformer.Windings[0];
                End2 = InXFW.Transformer.Windings[1];
            }
            else //if (InXFW.Transformer.Windings[1].WindingNodeTEID == InXfData.TEID_End1)
            {
                End1 = InXFW.Transformer.Windings[1];
                End2 = InXFW.Transformer.Windings[0];
            }
            //else
                //return;

            UpdateProperty(End1,"Estimated_MW", InXfData.W_End1, TriggerValueChange);
            UpdateProperty(End1,"Estimated_MVAR", InXfData.R_End1, TriggerValueChange);
            UpdateProperty(End1,"Estimated_MVA", InXfData.MVA_End1, TriggerValueChange);
            UpdateProperty(End1,"Telemetered_MW", InXfData.WMeas_End1, TriggerValueChange);
            UpdateProperty(End1,"Telemetered_MVAR", InXfData.RMeas_End1, TriggerValueChange);
            UpdateProperty(End1,"Telemetered_MVA", ComputeMVA(InXfData.WMeas_End1, InXfData.RMeas_End1), TriggerValueChange);
            UpdateProperty(End1, "NearBusNumber", InXfData.Open_End1 ? -1 : InXfData.End1_Bus, TriggerValueChange);
            UpdateProperty(End1,"Tap", InXfData.Tap, TriggerValueChange);

            UpdateProperty(End2,"Estimated_MW", InXfData.W_End2, TriggerValueChange);
            UpdateProperty(End2,"Estimated_MVAR", InXfData.R_End2, TriggerValueChange);
            UpdateProperty(End2,"Estimated_MVA", InXfData.MVA_End2, TriggerValueChange);
            UpdateProperty(End2,"Telemetered_MW", InXfData.WMeas_End2, TriggerValueChange);
            UpdateProperty(End2,"Telemetered_MVAR", InXfData.RMeas_End2, TriggerValueChange);
            UpdateProperty(End2,"Telemetered_MVA", ComputeMVA(InXfData.WMeas_End2, InXfData.RMeas_End2), TriggerValueChange);
            UpdateProperty(End2,"Tap", InXfData.ZTap, TriggerValueChange);
            UpdateProperty(End2, "NearBusNumber", InXfData.Open_End2 ? -1 : InXfData.End2_Bus, TriggerValueChange);

            UpdateProperty(Transformer,"Limits",0, InXfData.LIMIT1, TriggerValueChange);
            UpdateProperty(Transformer,"Limits",1, InXfData.LIMIT2, TriggerValueChange);
            UpdateProperty(Transformer,"Limits",2, InXfData.LIMIT3, TriggerValueChange);
            UpdateProperty(Transformer,"TapMin",0, InXfData.TapMin, TriggerValueChange);
            UpdateProperty(Transformer,"TapMin",1, InXfData.ZTapMin, TriggerValueChange);
            UpdateProperty(Transformer,"TapMax",0, InXfData.TapMax, TriggerValueChange);
            UpdateProperty(Transformer,"TapMax",1, InXfData.ZTapMax, TriggerValueChange);

            UpdateProperty(Transformer,"Regulated", InXfData.TEID_REGND != 0, TriggerValueChange);
            UpdateProperty(Transformer,"ManualTarget", InXfData.VTargMan_XF, TriggerValueChange);
            UpdateProperty(Transformer,"TargetDeviation", InXfData.Deviat_XF, TriggerValueChange);
            UpdateProperty(Transformer,"RegulationTargetVoltage", InXfData.DisTarg_XF, TriggerValueChange);
            UpdateProperty(Transformer,"RegulationDeviation", InXfData.DisDev_XF, TriggerValueChange);
            UpdateProperty(Transformer,"OffAVR", InXfData.OffAVR_XF, TriggerValueChange);
            UpdateProperty(Transformer,"WAVRN", InXfData.WAVRN_XF, TriggerValueChange);
            UpdateProperty(Transformer,"WAVR", InXfData.WAVR_XF, TriggerValueChange);
            UpdateProperty(Transformer,"AVR", InXfData.AVR_XF, TriggerValueChange);
            UpdateProperty(Transformer,"MVARating", InXfData.DMM_XF, TriggerValueChange);

            UpdateProperty(Transformer, "NearBusNumber",  End1.NearBusNumber, TriggerValueChange);
            UpdateProperty(Transformer, "FarBusNumber",  End2.NearBusNumber, TriggerValueChange);
        }

        /// <summary>
        /// Update our ZBR data
        /// </summary>
        /// <param name="InZBRData"></param>
        /// <param name="InZBR"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateZBRData(MM_ZeroImpedanceBridge_Data InZBRData, MM_Line InZBR, bool TriggerValueChange)
        {
            try
            {
                int FirstSub = InZBR.Substation1.TEID.Equals(InZBRData.TEID_End1) ? 0 : 1;
                int SecondSub = 1 - FirstSub;
                
                InZBR.IsZBR = true;
                //UpdateProperty(InZBR, "IsZBR", false, TriggerValueChange);
                UpdateProperty(InZBR, "Estimated_MVA", FirstSub, InZBRData.MVA_End1, TriggerValueChange);
                UpdateProperty(InZBR, "Estimated_MVA", SecondSub, InZBRData.MVA_End2, TriggerValueChange);
                UpdateProperty(InZBR, "Estimated_MVAR", FirstSub, InZBRData.R_End1, TriggerValueChange);
                UpdateProperty(InZBR, "Estimated_MVAR", SecondSub, InZBRData.R_End2, TriggerValueChange);
                UpdateProperty(InZBR, "Estimated_MW", FirstSub, InZBRData.W_End1, TriggerValueChange);
                UpdateProperty(InZBR, "Estimated_MW", SecondSub, InZBRData.W_End2, TriggerValueChange);
                UpdateProperty(InZBR, "Telemetered_MW", FirstSub, InZBRData.WMeas_End1, TriggerValueChange);
                UpdateProperty(InZBR, "Telemetered_MW", SecondSub, InZBRData.WMeas_End2, TriggerValueChange);
                UpdateProperty(InZBR, "Telemetered_MVAR", FirstSub, InZBRData.RMeas_End1, TriggerValueChange);
                UpdateProperty(InZBR, "Telemetered_MVAR", SecondSub, InZBRData.RMeas_End2, TriggerValueChange);
                UpdateProperty(InZBR, "NearBusNumber", InZBRData.End1_Bus, TriggerValueChange);
                UpdateProperty(InZBR, "FarBusNumber", InZBRData.End2_Bus, TriggerValueChange);
                UpdateProperty(InZBR, "Telemetered_MVA", FirstSub, ComputeMVA(InZBRData.WMeas_End1, InZBRData.RMeas_End1), TriggerValueChange);
                UpdateProperty(InZBR, "Telemetered_MVA", SecondSub, ComputeMVA(InZBRData.WMeas_End2, InZBRData.RMeas_End2), TriggerValueChange);
                UpdateProperty(InZBR, "Limits", 0, InZBRData.LIMIT1, TriggerValueChange);
                UpdateProperty(InZBR, "Limits", 1, InZBRData.LIMIT2, TriggerValueChange);
                UpdateProperty(InZBR, "Limits", 2, InZBRData.LIMIT3, TriggerValueChange);
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
        }

        /// <summary>
        /// Compute MVA from MW and MVAR, preserving the sign of MW
        /// </summary>
        /// <param name="MW"></param>
        /// <param name="MVAR"></param>
        /// <returns></returns>
        public static float ComputeMVA(float MW, float MVAR)
        {
            return (float)Math.Sign(MW) * (float)Math.Sqrt((MW * MW) + (MVAR * MVAR));
        }

        /// <summary>
        /// Update our system-wide dataa
        /// </summary>
        /// <param name="SystemWideDatum"></param>
        public static void UpdateSystemWideData(MM_SystemWide_Generation_Data SystemWideDatum)
        {
            //Ignore external telemetry
            if (SystemWideDatum.Id_OPA == "EXTERN")
                return;

            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.ACE] = SystemWideDatum.ACE_OPA;
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Frequency] = SystemWideDatum.FHZ_OPA;
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Gen] = SystemWideDatum.Gen_OPA;
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.GenCapacity] = SystemWideDatum.HSLT_OPA;
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.GenEmrgCapacity] = SystemWideDatum.HDLT_OPA;
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Load] = SystemWideDatum.Ld_OPA;
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.LoadRes] = SystemWideDatum.MWDistL_OPA;
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.PRC] = SystemWideDatum.PRCT_OPA;
        }

        /// <summary>
        /// Update our unit type data
        /// </summary>
        /// <param name="UnitTypeDatum"></param>
        public static void UpdateUnitTypeData(MM_UnitType_Generation_Data UnitTypeDatum)
        {
            if (UnitTypeDatum.ID_OPA == "EXTERN")
                return;
            MM_Unit_Type FoundType = MM_Repository.FindGenerationType(UnitTypeDatum.ID_GTY);
            FoundType.Remaining = UnitTypeDatum.RVSP_GTY;
            FoundType.MW = UnitTypeDatum.GEN_GTY;
            if (UnitTypeDatum.ID_GTY == "WIND")
                Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Wind] = UnitTypeDatum.GEN_GTY;
            Data_Integration.RTGenDate = DateTime.Now;
        }

        /// <summary>
        /// Update our SVC data
        /// </summary>
        /// <param name="InSVC"></param>
        /// <param name="FoundSVC"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateSVCData(MM_StaticVarCompensator_Data InSVC, MM_StaticVarCompensator FoundSVC, bool TriggerValueChange)
        {
            UpdateProperty(FoundSVC, "Estimated_MVAR", InSVC.R_SVS, TriggerValueChange);
            UpdateProperty(FoundSVC, "Nominal_MVAR", InSVC.MRNom_SVS, TriggerValueChange);
            UpdateProperty(FoundSVC, "Open", InSVC.Open_SVS, TriggerValueChange);
            UpdateProperty(FoundSVC, "Telemetered_MVAR", InSVC.RMeas_SVS, TriggerValueChange);
            UpdateProperty(FoundSVC, "NearBusNumber", InSVC.Conn_Bus, TriggerValueChange);
            UpdateProperty(FoundSVC, "FarBusNumber", InSVC.Reg_Bus, TriggerValueChange);
            UpdateProperty(FoundSVC, "AVR", InSVC.AVR_SVS, TriggerValueChange);
            UpdateProperty(FoundSVC, "Removed", InSVC.REMOVE_SVS, TriggerValueChange);
            UpdateProperty(FoundSVC, "DisTarg", InSVC.DisTarg_SVS, TriggerValueChange);
            UpdateProperty(FoundSVC, "ManualTarget", InSVC.VTARGMAN_SVS, TriggerValueChange);
            UpdateProperty(FoundSVC, "MaxMVAR", InSVC.RMX_SVS, TriggerValueChange);
            UpdateProperty(FoundSVC, "MinMVAR", InSVC.RMN_SVS, TriggerValueChange);
        }

        /// <summary>
        /// Update a property
        /// </summary>
        /// <param name="Elem"></param>
        /// <param name="Property"></param>
        /// <param name="NewValue"></param>
        /// <param name="TriggerValueChange"></param>
        private static void UpdateProperty(MM_Element Elem, String Property, Object NewValue, bool TriggerValueChange)
        {
            FieldInfo fI;
            PropertyInfo pI;
            if (Elem == null)
                return;
            try
            {
                //First, try our field
                {
                if ((fI = Elem.GetType().GetField(Property)) != null)
                {
                    Object OldValue = fI.GetValue(Elem);

                    if (OldValue == null || (OldValue != null && !OldValue.Equals(NewValue)))
                    {
                        fI.SetValue(Elem, NewValue);
                        if (TriggerValueChange)
                            Elem.TriggerValueChange(Property);
                    }
                }
                else if ((pI = Elem.GetType().GetProperty(Property)) != null)
                {
                    Object OldValue = pI.GetValue(Elem);
                    if (OldValue == null || (OldValue != null && !OldValue.Equals(NewValue)))
                    {
                        pI.SetValue(Elem, NewValue);
                        if (TriggerValueChange)
                            Elem.TriggerValueChange(Property);
                    }
                }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
        }

        /// <summary>
        /// Update a property
        /// </summary>
        /// <param name="Elem"></param>
        /// <param name="Property"></param>
        /// <param name="Index"></param>
        /// <param name="NewValue"></param>
        /// <param name="TriggerValueChange"></param>
        private static void UpdateProperty(MM_Element Elem, String Property, int Index, Object NewValue, bool TriggerValueChange)
        {
            FieldInfo fI;
            PropertyInfo pI;
            Array InArray=null;

            //First, try our field
            if ((fI = Elem.GetType().GetField(Property)) != null)            
                InArray = (Array)fI.GetValue(Elem);
            else if ((pI = Elem.GetType().GetProperty(Property)) != null)
                InArray = (Array)pI.GetValue(Elem);

            if (InArray == null)
            {}
            else if (InArray.GetValue(Index) != null && !InArray.GetValue(Index).Equals(NewValue))
            {
                InArray.SetValue(NewValue, Index);
                if (TriggerValueChange)
                    Elem.TriggerValueChange(Property);            
            }
        }

        /// <summary>
        /// Handle a DC Tie update
        /// </summary>
        /// <param name="TieDatum"></param>
        /// <param name="FoundTie"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateTieData(MM_Tie_Data TieDatum, MM_Tie FoundTie, bool TriggerValueChange)
        {
            FoundTie.MW_Integrated = TieDatum.MW_TIE;
        }

        /// <summary>
        /// Handle a breaker/switch value update
        /// </summary>
        /// <param name="InBreakerSwitch"></param>
        /// <param name="FoundSwitch"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateBreakerSwitchData(MM_BreakerSwitch_Data InBreakerSwitch, MM_Breaker_Switch FoundSwitch, bool TriggerValueChange)
        {
            UpdateProperty(FoundSwitch, "BreakerState", (InBreakerSwitch.Open_CB ? MM_Breaker_Switch.BreakerStateEnum.Open : MM_Breaker_Switch.BreakerStateEnum.Closed), TriggerValueChange);
            UpdateProperty(FoundSwitch,"MinSynch",InBreakerSwitch.MinSync_CB, TriggerValueChange);
            UpdateProperty(FoundSwitch,"MaxSynch",InBreakerSwitch.MaxSync_CB, TriggerValueChange);
            UpdateProperty(FoundSwitch,"NearBusNumber",InBreakerSwitch.Near_BS, TriggerValueChange);
            UpdateProperty(FoundSwitch,"FarBusNumber",InBreakerSwitch.Far_BS, TriggerValueChange);
        }

        /// <summary>
        /// Update our synchroscope data
        /// </summary>
        /// <param name="InSynch"></param>
        /// <param name="FoundSwitch"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateSynchroscopeData(MM_Synchroscope_Data InSynch, MM_Breaker_Switch FoundSwitch, bool TriggerValueChange)
        {
            UpdateProperty(FoundSwitch, "Synchroscope", InSynch, TriggerValueChange);
        }

        /// <summary>
        /// Update our interface
        /// </summary>
        /// <param name="Interface"></param>
        /// <param name="TriggerValueChange"></param>
        public static void UpdateInterfaceData(MM_Interface_Monitoring_Data Interface, bool TriggerValueChange)
        {
            MM_Limit FoundLimit;
            if (!Data_Integration.CSCIROLLimits.TryGetValue(Interface.ID_RTMGRP, out FoundLimit))
                Data_Integration.CSCIROLLimits.Add(Interface.ID_RTMGRP, FoundLimit = new MM_Limit() { Name = Interface.ID_RTMGRP });
            UpdateProperty(FoundLimit, "Current", Interface.RTOTMW_RTMGRP, TriggerValueChange);
            UpdateProperty(FoundLimit, "Max", Interface.LTOTMW_RTMGRP, TriggerValueChange);
        }

        /// <summary>
        /// Attempt to update our operatorship information
        /// </summary>
        /// <param name="OperatorshipUpdate"></param>
        public static void UpdateOperatorshipData(MM_Operatorship_Update OperatorshipUpdate)
        {
            MM_Element FoundElem;
            MM_Company FoundCompany;
            if (MM_Repository.TEIDs.TryGetValue(OperatorshipUpdate.TEID, out FoundElem) && MM_Repository.Companies.TryGetValue(OperatorshipUpdate.Owner, out FoundCompany))
                FoundElem.Operator = FoundCompany;
        }

        /// <summary>
        /// Send a command to our MM Server
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="OldValue"></param>
        public static CheckState SendCommand(String Command, String OldValue)
        {
            if (Data_Integration.SimulatorStatus != MM_Simulation_Time.enumSimulationStatus.Running)
            {
                MessageBox.Show("Unable to send command: Simulator is not currently running.\n\nPlease wait to send your command until the simulator is running again.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return CheckState.Indeterminate;
            }
            try
            {
                if (Client.SendCommand(Command, OldValue))
                    return CheckState.Checked;
                else
                    return CheckState.Unchecked;
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Error sending command {0}: {1}", Command, ex), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return CheckState.Unchecked;
            }
        }
    }
}