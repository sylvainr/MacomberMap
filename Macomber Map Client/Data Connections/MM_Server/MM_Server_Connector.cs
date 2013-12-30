using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Macomber_Map.Data_Elements;
using System.Drawing;
using System.Data;
using Macomber_Map.User_Interface_Components;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using MM_Communication.Data_Integration;
using System.Diagnostics;
using System.Reflection;
using Macomber_Map.User_Interface_Components.OneLines;

namespace Macomber_Map.Data_Connections
{
    /// <summary>
    /// This class is responsible for connecting to the CIM server
    /// 
    /// Open Source note: most of the code associated in this class is commented or removed, as it provides the connectivity information to the Macomber Map server, which will be released an integrated in Phase II. 
    /// Some code snippets are provided as example.
    /// </summary>
    public class MM_Server_Connector
    {
        #region Variable declarations
        /// <summary>Login debug mode - infinite time for the server to process login</summary>
        public bool LoginDebugMode = false;

        /// <summary>The server is attempting to connect to the server, don't spawn new instances</summary>
        public bool ConnectionInProgress = false;

        /// <summary>The operatorships the user has</summary>
        public int[] UserOperatorships;

        /// <summary>The list of companies the operator has usership for, based on their permissions</summary>
        public List<MM_Company> OperatorshipCompanies = new List<MM_Company>();

        /// <summary>Whether the user is ERCOT/superuser who can see all equipment and states</summary>
        public bool IsSuperUser = false;
       
        /// <summary>Our last EMS heartbeat</summary>
        public DateTime LastEMSHeartbeat = DateTime.Now;

        /// <summary>When our last data were received from the server</summary>
        public DateTime LastDataReceived = DateTime.Now;

        /// <summary>The name of our EMS console</summary>
        public String EMSConsoleName;

        /// <summary>Our flag for model load completion</summary>
        private bool ModelLoadCompleted = false;

        /// <summary>Our collection of active one-lines</summary>
        public Dictionary<Guid, OneLine_Viewer> ActiveOneLines = new Dictionary<Guid, OneLine_Viewer>();

        /// <summary>Our collection of active control panels</summary>
        public Dictionary<Guid, MM_ControlPanel> ActiveControlPanels = new Dictionary<Guid, MM_ControlPanel>();

        /// <summary>Whether MM is in study or real-time mode</summary>
        public bool StudyMode = false;

        /// <summary>The version number of our CIM file.</summary>
        public string CIMVersion;

        /// <summary>The flag for our CIM version completion</summary>
        public bool HaveCIMVersion = false;

        /// <summary>The base configuration element</summary>
        public XmlElement BaseElement;

        /// <summary>The name of our connector</summary>
        public String Name;

        /// <summary>The name/IP address of our primary server</summary>
        public String Primary;

        /// <summary>The TCP/IP port for our primary server</summary>
        public int PrimaryPort;

        /// <summary>The name/IP address of our secondary server</summary>
        public String Secondary;

        /// <summary>Our one-line mapping and control panel data</summary>
        public XmlElement OneLineMappings;

        /// <summary>The TCP/IP port for our primary server</summary>
        public int SecondaryPort;
            
        /// <summary>The user name associated with the connection</summary>
        private String UserName;

        /// <summary>The password associated with the connection</summary>
        private String Password;

        /// <summary>The connection counter to resolve multi-connect issues</summary>
        private int ConnectionAttempt = 0;

        /// <summary>The data source associated with the connection</summary>
        private String DataSource;

        /// <summary>When a command is started</summary>
        private DateTime CommandStart;

        /// <summary>Track whether a command is being sent</summary>
        private bool CommandRunning = false;

        /// <summary>Our login message</summary>
        private string LoginMessage = "";

        /// <summary>Whether to export the network model for quick loading next time.</summary>
        private bool ExportModel = false;
        
        /// <summary>Whether we're ready for an update</summary>
        private bool readyToUpdate = false;

        /// <summary>Our collection of reserves</summary>
        private Dictionary<MM_Communication.Elements.MM_Element.enumAttributes, MM_Reserve> Reserves = new Dictionary<MM_Communication.Elements.MM_Element.enumAttributes, MM_Reserve>();

        /// <summary>Our collection of SystemWide data</summary>
        private Dictionary<String, Object> SystemWideData = new Dictionary<string, object>();

        /// <summary>Whether the user is verified against the study system.</summary>
        private bool StudyVerified = false;

        /// <summary>Whether the user is verified against the real-time or simulator systems.</summary>
        private bool RTVerified = false;

        /// <summary>Whether the user has access to studies</summary>
        public  bool StudiesEnabled = true;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new CIM Server connector
        /// </summary>
        /// <param name="xCIMServer"></param>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <param name="ConnectionAttempt">The connection attempt number</param>
        /// <param name="DataSource">The data source for our connector</param>
        public MM_Server_Connector(XmlElement xCIMServer, string UserName, string Password, string DataSource, int ConnectionAttempt)
        {
            this.BaseElement = xCIMServer;
            this.UserName = UserName;
            this.Password = Password;
            this.DataSource = DataSource;
            this.ConnectionAttempt = ConnectionAttempt;

            //Read our configuration XML with server connectivity information, and start our management thread
            MM_Serializable.ReadXml(xCIMServer, this, false);
            //ThreadPool.QueueUserWorkItem(new WaitCallback(MessageHandler));
        }


        #endregion

        /// <summary>
        /// Load in our infromation on one-lines
        /// </summary>
        public void LoadOneLineMappings()
        {
            //SendCommand(TCP_Message.enumMacomberMapCodes.Get_OneLine_Mappings);
            //while (OneLineMappings == null)
            //{
            //    Application.DoEvents();
            //    Thread.Sleep(250);
            //}
        }

        /// <summary>
        /// Validate our CIM model, and load the most current one if not available.
        /// </summary>
        public void ValidateCIMModel()
        {
            //if (Data_Integration.NetworkSource.Application == "StateEstimator")
            //{
            //    SendCommand(TCP_Message.enumMacomberMapCodes.CIMVersion);
            //    while (!HaveCIMVersion)
            //    {
            //        Thread.Sleep(100);
            //        Application.DoEvents();
            //    }

            //    if (string.IsNullOrEmpty(CIMVersion))
            //        throw new InvalidOperationException("The server reported an unknown CIM version.");

            //    //Now that we have our CIM file, make sure we can access it.
            //    String TargetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MacomberMap\\Models");
            //    if (!Directory.Exists(TargetDirectory))
            //        Directory.CreateDirectory(TargetDirectory);
            //    ExportModel = !File.Exists(Path.Combine(TargetDirectory, CIMVersion + ".xml"));
            //    if (!ExportModel)
            //    {
            //        XmlDocument xDoc = new XmlDocument();
            //        xDoc.Load(Path.Combine(TargetDirectory, CIMVersion + ".xml"));
            //        foreach (XmlElement xElem in xDoc.DocumentElement.ChildNodes)
            //            try
            //            {
            //                MM_Element.CreateElement(xElem, "", true);
            //            }
            //            catch (Exception ex)
            //            {
            //                Program.LogError(ex);
            //            }
            //    }
            //}
            //else  //StudySystem
                
            //{
            //    // Request Model
            //    StudyMode = true;
            //    SendCommand(TCP_Message.enumMacomberMapCodes.CIMModelOnly);
            //    while (!ModelLoadCompleted)
            //    {
            //        Thread.Sleep(100);
            //        Application.DoEvents();
            //    }

            //}


           

            // SendCommand(TCP_Message.enumMacomberMapCodes.RequestValue);
        }

        /// <summary>
        /// Initiate our StudySystem retrieval
        /// </summary>
        public void RetrieveStudyData()
        {
            ModelLoadCompleted = false;
            //SendCommand(TCP_Message.enumMacomberMapCodes.RequestStudySystemValues);
        }



        /// <summary>
        /// Initiate our initial state + model retrieval
        /// </summary>
        public void RetrieveData()
        {
            ModelLoadCompleted = false;
            if (Data_Integration.NetworkSource.Application == "StateEstimator")
            {
                //SendCommand(ExportModel ? TCP_Message.enumMacomberMapCodes.RequestStateEstimatorAllValuesWithModel : TCP_Message.enumMacomberMapCodes.RequestStateEstimatorAllValues);
                //while (!ModelLoadCompleted)
                //    Application.DoEvents(); //mn//
            }
            else
            {
                
                //String FileName =null;
                //foreach (XmlElement xE in MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/MMServer[@Name='" + Data_Integration.MMServer.Name + "']").ChildNodes)
                //    if (xE.HasAttribute("Application") && xE.Attributes["Application"].Value == "StudySystem" && xE.HasAttribute("Filename") && !String.IsNullOrEmpty(xE.Attributes["Filename"].Value))
                //        FileName = xE.Attributes["Filename"].Value;
                //if (!String.IsNullOrEmpty(FileName) && File.Exists(FileName))
                //{
                //    XmlDocument xDoc = new XmlDocument();
                //    xDoc.Load(FileName);
                //    foreach (XmlElement xElem in xDoc.DocumentElement.ChildNodes)
                //        try
                //        {
                //            MM_Element.CreateElement(xElem, "", true);
                //        }
                //        catch (Exception ex)
                //        {
                //            Program.LogError(ex);
                //        }
                //}
                //else
                //{
                //}

                
                //SendCommand(TCP_Message.enumMacomberMapCodes.RequestStudySystemValues);

            }

            //Now, wait for our last message indicating the data were received           

            //If we need to, let's write out our model.
            if (ExportModel && false)
                using (XmlTextWriter xW = new XmlTextWriter(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MacomberMap\\Models"), CIMVersion + ".xml"), new UTF8Encoding(false)))
                {
                    xW.Formatting = Formatting.Indented;
                    xW.WriteStartDocument();
                    xW.WriteStartElement("CIMModel");
                    xW.WriteAttributeString("WrittenOn", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Unspecified));
                    foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                        Elem.WriteXML(xW);
                    xW.WriteEndElement();
                    xW.WriteEndDocument();
                }
            ExportModel = false;
            
        }



        /// <summary>
        /// Locate a substation's county by its latitude/longitude
        /// </summary>
        /// <param name="LatLng"></param>
        /// <returns></returns>
        public MM_Boundary LookupCounty(PointF LatLng)
        {
            foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                if (Bound.Name != "STATE" && LatLng.X >= Bound.Min.X && LatLng.Y >= Bound.Min.Y && LatLng.X <= Bound.Max.X && LatLng.Y <= Bound.Max.Y && Bound.HitTest(LatLng))
                    return Bound;
            return null;
        }

        /// <summary>Connect to the CIM Server</summary>
        public void Initialize()
        {
            //Create our instance to our client       
            foreach (XmlElement xConnector in BaseElement.ChildNodes)
                if (xConnector.Name == "Connection")
                {
                    string addr = xConnector.Attributes["Target"].Value;
                    int port = XmlConvert.ToInt32(xConnector.Attributes["Port"].Value);

                    //TODO: Initialize the client, and assign our handlers
                    /*
                    TCP_Connection NewClient1 = new TCP_Connection(addr, port);
                    NewClient1.ConnectionStatusChanged += NewClient_ConnectionStatusChanged;
                    NewClient1.OnMessageReceived += NewClient_OnMsgReceived;
                    NewClient1.OnMessageSent += NewClient_OnMessageSent;
                    NewClient1.ServerConnectionLost += NewClient_ServerConnectionLost;
                    Clients.Add(NewClient1);
                    if (xConnector.HasAttribute("Primary") && XmlConvert.ToBoolean(xConnector.Attributes["Primary"].Value))
                        ActiveClient = NewClient1;
                     */
                }
        }

        /// <summary>
        /// Retrieve the collection of real-time data
        /// </summary>
        public void RetrieveRealTimeData()
        {
            //SendCommand(TCP_Message.enumMacomberMapCodes.RequestStateEstimatorAllValues);
            //SendCommand(TCP_Message.enumMacomberMapCodes.RequestValue);
        }


        #region Communications handling
        /// <summary>
        /// Handle the loss of a server connection
        /// </summary>
        /// 
        private void NewClient_ServerConnectionLost(object state)
        {
            //Make sure we have the right to be connected
            if (Data_Integration.MMServer == null)
                return;
/*
            //Make sure we don't try multiple reconnections
            if (ConnectionInProgress)
                return;
            ConnectionInProgress = true;

            //Find our next client
            int NextClient = (Clients.IndexOf(ActiveClient) + 1) % Clients.Count;
            try
            {
                ActiveClient.Disconnect();
                Thread.Sleep(3000); 
                
                //Sleep is to allow for an actual disconnect to occur before trying again, 
                //in case it trys to hit a server that is already registered with this client and has not processed a 
                //disconnect yet, time will have to be measured and adjusted further.  
                //Server as of 20130606 can not handle multi-connections from the same client properly.
                //Should be replaced by advanced handshake routine, which should resolve server handeling issues as well. -mn
                
                if(Data_Integration.Permissions.runDevOptions)                 
                    ActiveClient.Disconnect(); //incase the first one is not recieved, as it seems on busy io path you can miss one which can lead to server issues. -mn | This needs to be tested more both in and out in itest environment under load
                
                Thread.Sleep(2000); //Set for 5 seconds total wait from first disconnect with two seconds here, as max during testing was 4.2 seconds for server processing -mn
            }
            catch (Exception)
            { }

            //Keep trying to activate a client
            while (true)
            {
                if (ConnectionAttempt != Data_Integration.MMServer.ConnectionAttempt) //ConnectionAttempt
                    return;
                String RemoteTarget = Clients[NextClient].hostEndPoint.Address.ToString();
                int RemotePort = Clients[NextClient].hostEndPoint.Port;
                Clients[NextClient].Dispose();
                ActiveClient = Clients[NextClient] = new TCP_Connection(RemoteTarget, RemotePort);
                ActiveClient.ConnectionStatusChanged += NewClient_ConnectionStatusChanged;
                ActiveClient.OnMessageReceived += NewClient_OnMsgReceived;
                ActiveClient.OnMessageSent += NewClient_OnMessageSent;
                ActiveClient.ServerConnectionLost += NewClient_ServerConnectionLost;
                State = ConnectionState.Connecting;

                try
                {
                    Program.LogError("Attempting to connect to " + ActiveClient.hostEndPoint.ToString());
                    ActiveClient.Connect();
                    while (State == ConnectionState.Connecting)
                    {
                        Application.DoEvents();
                        Thread.Sleep(250);
                    }
                    if (State == ConnectionState.Open)
                    {
                        MessageQueue.Clear();
                        Program.LogError("Reconnected to " + ActiveClient.hostEndPoint.ToString() + ". Commencing login.");
                        CommandRunning = false;
                        LogIn();
                        Program.LogError("Login completed with " + ActiveClient.hostEndPoint.ToString() + ".");

                        if (Data_Integration.NetworkSource.Application == "StateEstimator")
                        {
                            SendCommand(TCP_Message.enumMacomberMapCodes.RequestStateEstimatorAllValues);

                            SendCommand(TCP_Message.enumMacomberMapCodes.RequestValue);
                        }
                        else
                        {
                            SendCommand(TCP_Message.enumMacomberMapCodes.RequestStudySystemValues);
                        }

                        LastEMSHeartbeat = DateTime.Now;

                        //Send out our request for one-lines/control panels that are active
                        foreach (KeyValuePair<Guid, OneLine_Viewer> kvp in ActiveOneLines)
                            SendCommand(TCP_Message.enumMacomberMapCodes.MMOneLine, kvp.Value.BaseElement.Name, kvp.Key);
                        foreach (KeyValuePair<Guid, MM_ControlPanel> kvp in ActiveControlPanels)
                            SendCommand(TCP_Message.enumMacomberMapCodes.MM_ControlPanel, kvp.Value.OutMessage);

                        ConnectionInProgress = false;
                        return;
                    }
                    else
                    {
                        //if (ActiveClient.connected)
                        //Was If statement above, however now always send just incase we think were not connected and the server thinks we are, then wait two seconds for server to process disconnect for this case. 20130610 -mn
                        ActiveClient.Disconnect(); Thread.Sleep(2000);
                        Program.LogError("Failure connecting to " + ActiveClient.hostEndPoint.ToString() + ". State: " + State.ToString());
                    }

                }
                catch (Exception ex)
                {
                    if (ActiveClient.Connected)
                        ActiveClient.Disconnect();
                    Program.LogError("Failure connecting to " + ActiveClient.hostEndPoint.ToString() + ": " + ex.Message);
                    if (ex.Message.Contains("Invalid User"))
                        return;

                }
                //Attempt to connect to the next client
                
                NextClient = (NextClient + 1) % Clients.Count;
            }
            */
        }

        /// <summary>
        /// Assign a boolean value if possible
        /// </summary>
        /// <param name="NewValue"></param>
        /// <param name="TargetBool"></param>
        private void AssignBoolean(Object NewValue, ref bool TargetBool)
        {
            if (NewValue is bool)
                TargetBool = (bool)NewValue;
            else
                TargetBool = Convert.ToBoolean(NewValue);
        }



        /// <summary>
        /// Assign a value to a particular item. If it's a dbNull, assign to float.NaN.
        /// </summary>
        /// <param name="NewValue">The value to write</param>
        /// <param name="TargetFloat">The target variable for the value</param>
        private void AssignSingle(Object NewValue, ref float TargetFloat)
        {
            if (NewValue is Single)
                TargetFloat = (Single)NewValue;
            else if (NewValue is int)
                TargetFloat = Convert.ToSingle(NewValue);
            else
                TargetFloat = float.NaN;
        }

        /// <summary>
        /// Handle incoming system-wide data after the MM server has sent all values
        /// </summary>
        private void CompletedSystemWide()
        {
            try
            {
                float HSLT = float.NaN, Gen = float.NaN, HASLT = float.NaN;

                AssignSingle(SystemWideData["LD"], ref Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Load]);
                AssignSingle(SystemWideData["FHZ"], ref Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Frequency]);
                AssignSingle(SystemWideData["ACE"], ref Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.ACE]);
                AssignSingle(SystemWideData["HASLT"], ref HASLT);
                AssignSingle(SystemWideData["HSLT"], ref HSLT);
                AssignSingle(SystemWideData["GEN"], ref Gen);

                //Update our generation, and calculate our normal and emergency capacity
                Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Gen] = Gen;
                if (!float.IsNaN(HSLT) && !float.IsNaN(Gen))
                    Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.GenEmrgCapacity] = HSLT - Gen;
                if (!float.IsNaN(HASLT) && !float.IsNaN(Gen))
                    Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.GenCapacity] = HASLT - Gen;

                //Determine our ACE status and color accordingly based on flags set in the EMS
                if (Convert.ToBoolean(SystemWideData["DBAND"]))
                    Data_Integration.ACEColor = System.Drawing.Color.DarkGray;
                else if (Convert.ToBoolean(SystemWideData["REGUL"]) || Convert.ToBoolean(SystemWideData["REGULI"]))
                    Data_Integration.ACEColor = System.Drawing.Color.White;
                else if (Convert.ToBoolean(SystemWideData["ASS"]))
                    Data_Integration.ACEColor = System.Drawing.Color.Yellow;
                else if (Convert.ToBoolean(SystemWideData["EME"]))
                    Data_Integration.ACEColor = System.Drawing.Color.Red;

                //Update our physical responsive and load responsive values (note - these terminologies are in place from Zonal to Nodal transition
                AssignSingle(SystemWideData["PRCT"], ref Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.LoadRes]);
                AssignSingle(SystemWideData["PRCT"], ref Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.PRC]);

                //Update all of the reserve values
                lock (Data_Integration.Reserves)
                    foreach (MM_Reserve Reserve in new List<MM_Reserve>(Data_Integration.Reserves.Values))
                        Reserve.UpdateValue(SystemWideData[Reserve.EMSValue]);
                Data_Integration.AddSystemValues(Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Wind], Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Gen], DateTime.Now);// Convert.ToDateTime(SystemWideData["TAGCLAST"]));
           
            }
            catch (Exception ex)
            {
                Program.LogError("Error in SystemWide handler: " + ex.Message);
            }
        }

        /// <summary>
        /// Connect to our Macomber Map server
        /// </summary>
        public void Connect()
        {
        }
        
        /// <summary>
        /// Close the connection to our server
        /// </summary>
        public void Close(int option)
        {   
        }
        #endregion

        /// <summary>
        /// Log in to the MM server
        /// </summary>
        public void LogIn()
        {
        }

     
        /// <summary>Load the boundaries</summary>
        public void LoadBoundaries() { throw new InvalidOperationException(); }

        /// <summary>Load an element by TEID</summary>
        /// <param name="TEID">The TEID of the element</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Element LoadElement(Int32 TEID, bool AddIfNew) { throw new InvalidOperationException(); }

        /// <summary>Load an element by TEID and element type</summary>
        /// <param name="TEID">The TEID of the element</param>
        /// <param name="ElementType">The element type</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Element LoadElement(Int32 TEID, MM_Element_Type ElementType, bool AddIfNew) { throw new InvalidOperationException(); }

        /// <summary>
        /// Locate all elements that are within the bounds of a substation (for lasso purposes)
        /// </summary>
        /// <param name="Substations">The collection of substations to search</param>
        /// <returns>A collection of elements </returns>
        public MM_Element[] LocateElements(params String[] Substations) { throw new InvalidOperationException(); }

        /// <summary>
        /// Locate all elements that are within the bounds of a substation (for lasso purposes)
        /// </summary>
        /// <param name="Substations">The collection of substations to search</param>
        /// <returns>A collection of elements </returns>
        public MM_Element[] LocateElements(params Int32[] Substations) { throw new InvalidOperationException(); }

        /// <summary>
        /// Update the state of the current object
        /// </summary>
        public ConnectionState State = ConnectionState.Connecting;

        /// <summary>
        /// Load an element by substation name and element name
        /// </summary>
        /// <param name="SubstationName">The name of the substation</param>
        /// <param name="ElementName">The name of the element</param>
        /// <param name="ElementType">The element type</param>
        /// <returns></returns>
        public MM_Element LoadElement(String SubstationName, String ElementName, MM_Element_Type ElementType) { throw new InvalidOperationException(); }


        /// <summary>Load the collection of companies</summary>
        public void LoadCompanies() { throw new InvalidOperationException(); }

        /// <summary>Load KV levels</summary>
        public void LoadKVLevels() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of element types</summary>
        public void LoadElementTypes() { throw new InvalidOperationException(); }

        /// <summary>
        /// Load our collection of reserves
        /// </summary>
        public void LoadReserves()
        {

            while (!readyToUpdate)
            {
                Application.DoEvents();
            }
            int TargetIndex;
            foreach (XmlElement xElem in MM_Repository.xConfiguration.SelectNodes("//Reserve"))

                if ((TargetIndex = Array.IndexOf(Enum.GetNames(typeof(MM_Communication.Elements.MM_Element.enumAttributes)), xElem.Attributes["EMSValue"].Value)) != -1)
                {
                    MM_Communication.Elements.MM_Element.enumAttributes Attr = (MM_Communication.Elements.MM_Element.enumAttributes)Enum.GetValues(typeof(MM_Communication.Elements.MM_Element.enumAttributes)).GetValue(TargetIndex);
                    MM_Reserve NewReserve = new MM_Reserve(xElem, true);
                    Data_Integration.Reserves.Add(xElem.Attributes["Category"].Value + "-" + xElem.Attributes["Name"].Value, NewReserve);
                    Reserves.Add(Attr, NewReserve);
                }
        }

        /// <summary>Load in our collection of blackstart corridors</summary>
        public void LoadBlackstartCorridors() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of lines</summary>
        public void LoadLines() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of load and weather zones</summary>
        public void LoadZones() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of notes</summary>
        public void LoadNotes()
        {
        }

        /// <summary>Load the collection of substations</summary>
        public void LoadSubstations() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of transformers</summary>
        public void LoadTransformers() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of units</summary>
        public void LoadUnits() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of loads</summary>
        public void LoadLoads() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of DCTies</summary>
        public void LoadDCTies() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of loads</summary>
        public void LoadElectricalBuses() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of busbar sections</summary>
        public void LoadBusbarSections() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of capacitors and reactors</summary>
        public void LoadShuntCompensators() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of contingency definitions</summary>
        public void LoadContingencyDefinitions() { throw new InvalidOperationException(); }

        /// <summary>Load the collection of remedial action schemes</summary>
        public void LoadRemedialActionSchemes() { throw new InvalidOperationException(); }

        private Dictionary<Guid, String> OneLineQueue = new Dictionary<Guid, string>();


        /// <summary>
        /// Load a one-line control panel
        /// </summary>
        /// <param name="Panel"></param>
        /// <param name="Elem"></param>
        /// <param name="SourceApplication"></param>
        /// <param name="xConfig"></param>
        /// <param name="QueryGUID"></param>
        public void LoadOneLineControlPanel(MM_ControlPanel Panel, MM_OneLine_Element Elem, XmlElement xConfig, String SourceApplication, Guid QueryGUID)
        {
            lock (ActiveControlPanels)
                ActiveControlPanels.Add(QueryGUID, Panel);
            try
            {
                //Build our collection of value updates
                List<String> ValueUpdates = new List<string>();
                String OutUpdate = xConfig.ParentNode.Attributes["Values"].Value.Replace("{Station}", Elem.BaseElement.Substation.Name.ToUpper()).Replace("{Name}", Elem.BaseElement.Name.ToUpper());
                if (OutUpdate.Contains("{XFName}"))
                    OutUpdate = OutUpdate.Replace("{XFName}", Elem.xElement.Attributes["BaseElement.TransformerName"].Value);
                ValueUpdates.Add(OutUpdate);
               
                //Add in any additional data sources
                foreach (XmlElement xSubElem in xConfig.SelectNodes("AdditionalDataSource"))
                    ValueUpdates.Add(xSubElem.Attributes["Values"].Value.Replace("{Station}", Elem.BaseElement.Substation.Name.ToUpper()).Replace("{Name}", Elem.BaseElement.Name.ToUpper()));
                
                //Send our panel's outgoing message, and send the command
                Panel.OutMessage = new object[] { String.Join("|",ValueUpdates.ToArray()), QueryGUID, xConfig.Attributes["Title"].Value, SourceApplication, (MM_Communication.Elements.MM_Element.enumElementType)Enum.Parse(typeof(MM_Communication.Elements.MM_Element.enumElementType), xConfig.ParentNode.ParentNode.Attributes["Name"].Value, true), Elem.TEID};                
                //SendCommand(TCP_Message.enumMacomberMapCodes.MM_ControlPanel, Panel.OutMessage);
            }
            catch (Exception ex)
            {
                Program.MessageBox("Error loading control panel " + Panel.Text + " for " + Elem.ToString() + " : " + ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }



        /// <summary>
        /// Send a request for a Boolean toggle
        /// </summary>
        /// <param name="PanelGuid"></param>
        /// <param name="Substring"></param>
        /// <param name="ColumnName"></param>
        /// <param name="ColumnDescription"></param>
        /// <param name="NewValue"></param>
        /// <param name="Substation"></param>
        /// <param name="ElemType"></param>
        /// <param name="ElemName"></param>
        /// <param name="DataSourceName"></param>
        public void ToggleBoolean(Guid PanelGuid, int Substring, String ColumnName, String ColumnDescription, bool NewValue, String Substation, String ElemType, String ElemName, string DataSourceName)
        {
            //SendCommand(TCP_Message.enumMacomberMapCodes.ToggleBoolean, PanelGuid, Substring, ColumnName, NewValue, ColumnDescription, Substation, ElemType, ElemName,DataSourceName);
        }


        /// <summary>
        /// Send a message to our server
        /// </summary>
        /// <param name="PanelGuid"></param>
        /// <param name="Substring"></param>
        /// <param name="Message"></param>
        /// <param name="MessageDescription"></param>
        /// <param name="Substation"></param>
        /// <param name="ElemType"></param>
        /// <param name="ElemName"></param>
        /// <param name="OldValue"></param>
        /// <param name="NewValue"></param>
        /// <param name="DataSourceName"></param>
        public void SendMessage(Guid PanelGuid, int Substring, String Message, String MessageDescription, String Substation, String ElemType, String ElemName, String OldValue, String NewValue, String DataSourceName)
        {
            //SendCommand(TCP_Message.enumMacomberMapCodes.SendMessage, PanelGuid, Substring, Message, MessageDescription, Substation, ElemType, ElemName, OldValue, NewValue,DataSourceName);
        }




        /// <summary>
        /// Update our value in control panel
        /// </summary>
        /// <param name="PanelGuid"></param>
        /// <param name="Substring"></param>
        /// <param name="ColumnName"></param>
        /// <param name="ColumnDescription"></param>
        /// <param name="UpdatedValue"></param>
        /// <param name="OldValue"></param>
        /// <param name="Substation"></param>
        /// <param name="ElemType"></param>
        /// <param name="ElemName"></param>    
        /// <param name="DataSourceName"></param>
        public void UpdateValue(Guid PanelGuid, int Substring, String ColumnName, String ColumnDescription, String UpdatedValue, String OldValue, String Substation, String ElemType, String ElemName, String DataSourceName)
        {
            //SendCommand(TCP_Message.enumMacomberMapCodes.UpdateValue, PanelGuid, Substring, ColumnName, UpdatedValue, ColumnDescription, Substation, ElemType, ElemName, OldValue,DataSourceName);
        }


        /// <summary>
        /// Load a one-line XML
        /// </summary>
        /// <param name="OneLineName">The name of the substation or element</param>
        /// <param name="OLView">The one-line viewer</param>
        /// <param name="OneLineGuid">The GUID of the one-line</param>
        /// <returns>The XML document containing the one-line configuration</returns>        
        public XmlDocument LoadOneLine(String OneLineName, OneLine_Viewer OLView, out Guid OneLineGuid)
        {
            //Register to handle our one-line 
            OneLineGuid = Guid.NewGuid();
            lock (ActiveOneLines)
                ActiveOneLines.Add(OneLineGuid, OLView);

            //SendCommand(TCP_Message.enumMacomberMapCodes.MMOneLine, OneLineName, OneLineGuid);
            String FoundXml;
            while (!OneLineQueue.TryGetValue(OneLineGuid, out FoundXml))
            {
                Thread.Sleep(500);
                Application.DoEvents();
            }
            XmlDocument OutDoc = new XmlDocument();
            OutDoc.LoadXml(FoundXml);
            return OutDoc;
        }

        /// <summary>
        /// Load a one-line XML from a file
        /// </summary>
        /// <param name="OneLineName"></param>
        /// <param name="OLView"></param>
        /// <param name="OneLineGuid"></param>
        /// <returns></returns>
        public static XmlDocument LoadOneLineFromFile(String OneLineName, OneLine_Viewer OLView, out Guid OneLineGuid)
        {
            //Load a one-line from an XML file
            OneLineGuid = Guid.NewGuid();
            XmlDocument OutDoc = new XmlDocument();
            if (File.Exists(Path.Combine(Data_Integration.OneLineDirectory, OneLineName + ".MM_OneLine")))
                OutDoc.Load(Path.Combine(Data_Integration.OneLineDirectory, OneLineName + ".MM_OneLine"));
            else
                using (OpenFileDialog oFd = new OpenFileDialog())
                {
                    oFd.Title = "Open a one-line for " + OneLineName;
                    oFd.Filter = "One-line for " + OneLineName + " (" + OneLineName + ".MM_OneLine)|" + OneLineName + ".MM_OneLine";
                    if (oFd.ShowDialog() == DialogResult.OK)
                    {
                        OutDoc.Load(oFd.FileName);
                        Data_Integration.OneLineDirectory = Path.GetDirectoryName(oFd.FileName);
                    }
                    else
                        throw new InvalidOperationException("Unable to open a one-line for " + OneLineName);

                }
            return OutDoc;
        }

        /// <summary>
        /// Shut down a one-line query
        /// </summary>
        /// <param name="OneLineGuid"></param>
        public void ShutdownOneLine(Guid OneLineGuid)
        {
            lock (ActiveOneLines)
                ActiveOneLines.Remove(OneLineGuid);
            //SendCommand(TCP_Message.enumMacomberMapCodes.Query_Shutdown, OneLineGuid);
        }

   


        /// <summary>
        /// Upload a note to the CIM server
        /// </summary>
        /// <param name="Elem">The element to be updated</param>
        /// <param name="Author">The author of the note</param>
        /// <param name="NoteText">The text of the note</param>
        public void UploadNote(MM_Element Elem, string Author, string NoteText) 
        { 
        }
    }
}