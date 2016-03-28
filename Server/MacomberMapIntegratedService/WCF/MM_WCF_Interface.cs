using MacomberMapAdministrationService;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.Communications;
using MacomberMapCommunications.Messages.EMS;
using MacomberMapCommunications.WCF;
using MacomberMapIntegratedService.Database;
using MacomberMapIntegratedService.History;
using MacomberMapIntegratedService.Properties;
using MacomberMapIntegratedService.Service;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService.WCF
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides our WCF interface to our client
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class MM_WCF_Interface : IMM_WCF_Interface
    {
        #region Instance variables
        /// <summary>The GUID for the conversation
        private Guid ConversationGuid = Guid.Empty;

        /// <summary>Our conversation handler</summary>
        private IMM_ConversationMessage_Types ConversationHandler;

        /// <summary>The user associated with this connection</summary>
        public MM_User User;

        /// <summary>The random number generator for our EMS outgoing commands</summary>
        private static Random RandomGenerator = new Random();
        #endregion

        #region Startup/Shutdown
        /// <summary>
        /// Initialize our WCF instance
        /// </summary>
        public MM_WCF_Interface()
        {
            RemoteEndpointMessageProperty EndPoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            String Entry = "";
            try
            {
                Entry = Dns.GetHostEntry(EndPoint.Address).HostName;
            }
            catch
            {
                Entry = "Unknown";
            }

            ConversationGuid = RegisterApplicationStartup(EndPoint.Address, EndPoint.Port, Entry);

            OperationContext.Current.Channel.Faulted += Channel_Closed;
            OperationContext.Current.Channel.Closed += Channel_Closed;
        }

        /// <summary>
        /// Handle our channel closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Channel_Closed(object sender, EventArgs e)
        {
            HandleUserLogout(ConversationGuid);
        }

        /// <summary>
        /// Register our callback against our Guid
        /// </summary>
        /// <param name="VersionNumber"></param>
        /// <returns></returns>
        public bool RegisterCallback(String VersionNumber="Unknown")
        {
            User.ClientVersion = VersionNumber;
            ConversationHandler = OperationContext.Current.GetCallbackChannel<IMM_ConversationMessage_Types>();
            return true;
        }       

        /// <summary>
        /// Register our application startup
        /// </summary>
        /// <returns></returns>
        public bool RegisterApplicationStartup(IPEndPoint Endpoint)
        {
            //Ignore - we're not sending app startup messaging from the server
            return false;
        }

        /// <summary>
        /// Report our ping capabilities
        /// </summary>
        /// <returns></returns>
        public bool Ping()
        {
            return true;
        }
        #endregion

        #region EMS data handling
        /// <summary>
        /// Get our load data
        /// </summary>
        /// <returns></returns>
        public MM_Load_Data[] GetLoadData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.LoadData;
        }

        /// <summary>
        /// Get our SCADA analog data
        /// </summary>
        /// <returns></returns>
        public MM_Scada_Analog[] GetSCADAAnalogData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.SCADAAnalogs;
        }

        /// <summary>
        /// Get our SCADA status data
        /// </summary>
        /// <returns></returns>
        public MM_Scada_Status[] GetSCADAStatusData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.SCADAStatuses;
        }

        public MM_Flowgate_Data[] GetFlowgateData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.FlowgateData;
        }
        /// <summary>
        /// Get our island data
        /// </summary>
        /// <returns></returns>
        public MM_Island_Data[] GetIslandData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.IslandData.Values.ToArray();
        }

        /// <summary>
        /// Get our line data
        /// </summary>
        /// <returns></returns>
        public MM_Line_Data[] GetLineData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.LineData;
        }

        /// <summary>
        /// Report the latest timestamps
        /// </summary>
        /// <returns></returns>
        public MM_Data_Collection[] CheckLatestTimestamps()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.Timestamps;
        }

        /// <summary>
        /// Get our bus data
        /// </summary>
        /// <returns></returns>
        public MM_Bus_Data[] GetBusData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.BusData.Values.ToArray();
        }

        /// <summary>
        /// Get our line outage data
        /// </summary>
        /// <returns></returns>
        public MM_Outage_Line_Data[] GetLineOutageData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.LineOutageData.Values.ToArray();
        }

        /// <summary>
        /// Get our unit outage data
        /// </summary>
        /// <returns></returns>
        public MM_Outage_Unit_Data[] GetUnitOutageData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.UnitOutageData.Values.ToArray();
        }

        /// <summary>
        /// Get our transformer outage data
        /// </summary>
        /// <returns></returns>
        public MM_Outage_Transformer_Data[] GetTransformerOutageData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.TransformerOutageData.Values.ToArray();
        }

        /// <summary>
        /// Return our analog measurement data
        /// </summary>
        /// <returns></returns>
        public MM_Analog_Measurement[] GetAnalogMeasurementData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.AnalogMeasurementData;
        }

        /// <summary>
        /// Return our shunt compensator data
        /// </summary>
        /// <returns></returns>
        public MM_ShuntCompensator_Data[] GetShuntCompensatorData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.ShuntCompensatorData;
        }

        /// <summary>
        /// Return our state measurement data
        /// </summary>
        /// <returns></returns>
        public MM_State_Measurement[] GetStateMeasurementData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.StateMeasurementData;
        }

        /// <summary>
        /// Return our transformer state data
        /// </summary>
        /// <returns></returns>
        public MM_Transformer_Data[] GetTransformerData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.TransformerData;
        }

        /// <summary>
        /// Return our phase shifter data
        /// </summary>
        /// <returns></returns>
        public MM_Transformer_PhaseShifter_Data[] GetTransformerPhaseShifterData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.TransformerPhaseShifterData;
        }

        /// <summary>
        /// Return our unit data
        /// </summary>
        /// <returns></returns>
        public MM_Unit_Data[] GetUnitData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.UnitData;
        }

        /// <summary>
        /// Return our ZBR data
        /// </summary>
        /// <returns></returns>
        public MM_ZeroImpedanceBridge_Data[] GetZBRData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.ZBRData;
        }

        /// <summary>
        /// Return our breaker/switch data
        /// </summary>
        /// <returns></returns>
        public MM_BreakerSwitch_Data[] GetBreakerSwitchData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.BreakerSwitchData;
        }

        /// <summary>
        /// Return our chart data
        /// </summary>
        /// <returns></returns>
        public MM_Chart_Data[] GetChartData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.ChartData;
        }

        /// <summary>
        /// Return our chart data
        /// </summary>
        /// <returns></returns>
        public MM_LMP_Data[] GetLmpData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.LmpData;
        }
        /// Return our DC Tie data
        /// </summary>
        /// <returns></returns>
        public MM_Tie_Data[] GetTieData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.TieData;
        }

        /// <summary>
        /// Return our SVC data
        /// </summary>
        /// <returns></returns>
        public MM_StaticVarCompensator_Data[] GetStaticVarCompensatorData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.SVCData;
        }

        /// <summary>
        /// Return our system-wide generation data
        /// </summary>
        /// <returns></returns>
        public MM_SystemWide_Generation_Data[] GetSystemWideGenerationData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.SystemWideGenerationData;
        }

        /// <summary>
        /// Return our unit-type generation data
        /// </summary>
        /// <returns></returns>
        public MM_UnitType_Generation_Data[] GetUnitTypeGenerationData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.UnitTypeGenerationData;
        }

        /// <summary>
        /// Return our unit generation-specific data
        /// </summary>
        /// <returns></returns>
        public MM_Unit_Gen_Data[] GetUnitGenData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.UnitGenData;
        }

        /// <summary>
        /// Return our basecase violation data
        /// </summary>
        /// <returns></returns>
        public MM_Basecase_Violation_Data[] GetBasecaseViolationData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.BasecaseViolationData.Values.ToArray();
        }

        /// <summary>
        /// Return our contingency violation data
        /// </summary>
        /// <returns></returns>
        public MM_Contingency_Violation_Data[] GetContingencyVioltationData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.ContingencyViolationData.Values.ToArray();
        }

        /// <summary>
        /// Return our interface data
        /// </summary>
        /// <returns></returns>
        public MM_Interface_Monitoring_Data[] GetInterfaceData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.InterfaceData;
        }

        /// <summary>
        /// Return our operatorship updates
        /// </summary>
        /// <returns></returns>
        public MM_Operatorship_Update[] GetOperatorshipUpdates()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.OperatorshipUpdates.Values.ToArray();
        }


        /// <summary>
        /// Return our load forecast data
        /// </summary>
        /// <returns></returns>
        public MM_Load_Forecast_Data[] GetLoadForecastData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.LoadForecastData;
        }

        /// <summary>
        /// Return our unit simulation data
        /// </summary>
        /// <returns></returns>
        public MM_Unit_Simulation_Data[] GetUnitSimulationData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.UnitSimulationData;
        }

        /// <summary>
        /// Return our island simulation data
        /// </summary>
        /// <returns></returns>
        public MM_Island_Simulation_Data[] GetIslandSimulationData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.IslandSimulationData.Values.ToArray();
        }

        /// <summary>
        /// Return our synchroscope data
        /// </summary>
        /// <returns></returns>
        public MM_Synchroscope_Data[] GetSynchroscopeData()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.SynchroscopeData;
        }

        /// <summary>
        /// Return our simulator time
        /// </summary>
        /// <returns></returns>
        public MM_Simulation_Time[] GetSimulatorTime()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.SimulatorTimeData;
        }

        /// <summary>
        /// Report our unit status data
        /// </summary>
        /// <returns></returns>
        public MM_Unit_Control_Status[] GetUnitControlStatus()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.UnitControlStatusData.Values.ToArray();
        }

        /// <summary>
        /// Update our unit control status data
        /// </summary>
        /// <param name="Status"></param>
        /// <returns></returns>
        public bool UpdateUnitControlStatusInformation(MM_Unit_Control_Status Status)
        {
            //First, update our dictionary with our status information
            User.LastReceivedMessage = DateTime.Now;
            MM_EMS_Data_Updater.ProcessUpdate(typeof(MM_Unit_Control_Status), new MM_Unit_Control_Status[] { Status });
            return true;
        }

        /// <summary>
        /// Send a command
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="OldValue"></param>
        /// <returns></returns>
        public bool SendCommand(string Command, String OldValue)
        {
            byte[] OutBytes = new UTF8Encoding(false).GetBytes("#CMD_TYPE,APP,FAM,CMD_NAME,DB,RECORD,FIELD,TEID,VALUE" + Environment.NewLine + Command + Environment.NewLine);
            DateTime SimTime = MM_Server.SimulatorTimeData.Length == 0 ? DateTime.Now : MM_Server.SimulatorTimeData[0].Simulation_Time;

            if (!String.IsNullOrEmpty(Settings.Default.OperatorCommandTCPAddress))
                try
                {
                    using (TcpClient Client = new TcpClient())
                    {
                        Client.Connect(Settings.Default.OperatorCommandTCPAddress, Settings.Default.OperatorCommandTCPPort);
                        using (NetworkStream nS = Client.GetStream())
                            nS.Write(OutBytes, 0, OutBytes.Length);
                        MM_Notification.WriteLine(ConsoleColor.White, "Sent {1} command to TEDE/TCP: by {0} on {2}. SimTime {3}", User.UserName, Command, DateTime.Now, SimTime);
                        MM_Database_Connector.LogCommand(Command, User, "TCP");
                    }
                }
                catch (Exception ex)
                {
                    MM_Notification.WriteLine(ConsoleColor.Red, "Error sending {1} command to TEDE/TCP: by {0} on {2} SimTime {3}: {4} ", User.UserName, Command, DateTime.Now, SimTime, ex);
                    return false;
                }

            if (!String.IsNullOrEmpty(Properties.Settings.Default.OperatorCommandPath))
            {
                String[] splStr = Command.TrimEnd(',').Split(',');
                String TargetFileName = Path.Combine(Properties.Settings.Default.OperatorCommandPath, ((splStr[0].Equals("REFRESH")) ? "EMS_REFRESH_" : "EMS_COMMANDS_") + RandomGenerator.Next().ToString() + ".csv");
                using (FileStream fS = new FileStream(TargetFileName, FileMode.CreateNew, FileAccess.Write))
                    fS.Write(OutBytes, 0, OutBytes.Length);
                MM_Notification.WriteLine(ConsoleColor.White, "Sent {1} command to TEDE/File: by {0} on {2}. SimTime {3}", User.UserName, Command, DateTime.Now, SimTime);
                MM_Database_Connector.LogCommand(Command, User, "File");
            }

            //Now, write out our command
            MM_EMS_Command[] OutCommands = new MM_EMS_Command[] { new MM_EMS_Command(Command, User.UserName, User.MachineName, SimTime, OldValue) };
            try
            {
                List<String> OutLines = new List<string>();
                if (!File.Exists("EMSCommands.csv"))
                    OutLines.Add(MM_EMS_Command.HeaderLine());

                OutLines.Add(OutCommands[0].BuildLine());
                File.AppendAllLines("EMSCommands.csv", OutLines.ToArray());
            }
            catch (Exception ex)
            {
                MM_Notification.WriteLine(ConsoleColor.Red, "Unable to write out command to local log: {0}", ex);
            }

            //Now, send our commands to all administrators
            if (Properties.Settings.Default.ForwardCommandsToAdminClients)
                foreach (MM_Administrator_Types Interface in MM_Server.ConnectedAdministatorList)
                    try
                    {
                        MM_Administrator_Types.SendMessageToConsole(Interface.ConversationGuid, "AddEMSCommands", new object[] { OutCommands });
                    }
                    catch (Exception ex)
                    {
                        MM_Notification.WriteLine(ConsoleColor.Red, "Unable to send {0} to Admin: {1}", "AddEMSCommands", ex);
                    }

            //Now, send out our commands to all clients
            if (Properties.Settings.Default.ForwardCommandsToERCOTClients)
                foreach (MM_WCF_Interface Interface in MM_Server.ConnectedUserList)
                    if (Interface.User.LoggedOnTime != default(DateTime) && Interface.User.ERCOTUser)
                        try
                        {
                            MM_WCF_Interface.SendMessage(Interface.User.UserId, "AddEMSCommands", new object[] { OutCommands });
                        }
                        catch (Exception ex)
                        {
                            MM_Notification.WriteLine(ConsoleColor.Red, "Unable to send {0} to {1}: {2}", "AddEMSCommands", Interface.User.UserName, ex);
                        }
            User.LastCommand = DateTime.Now;
            return true;
        }
        #endregion

        #region PI data handling
        /// <summary>
        /// Initiate a query, and start it
        /// </summary>
        /// <param name="TagSQL"></param>
        /// <param name="EndTime"></param>
        /// <param name="NumPoints"></param>
        /// <param name="StartTime"></param>
        /// <returns></returns>
        public Guid QueryTags(string TagSQL, DateTime StartTime, DateTime EndTime, int NumPoints)
        {
            //First, build our new query
            MM_Historic_Query Query = new MM_Historic_Query();
            Query.Id = Guid.NewGuid();
            Query.State = MM_Historic_Query.enumQueryState.Queued;
            Query.QueryText = TagSQL;
            Query.LastUpdate = DateTime.Now;
            Query.StartTime = StartTime;
            Query.EndTime = EndTime;
            Query.NumPoints = NumPoints;

            //Now, add our query to the list, and trigger the MRE.
            MM_Historic_Reader.Queries.Add(Query.Id, Query);
            MM_Historic_Reader.mreQueryReady.Set();
            User.LastReceivedMessage = DateTime.Now;
            return Query.Id;
        }

        /// <summary>
        /// Check on the status of our query
        /// </summary>
        /// <param name="QueryID"></param>
        /// <returns></returns>
        public MM_Historic_Query.enumQueryState CheckQueryStatus(Guid QueryID)
        {
            MM_Historic_Query FoundQuery;
            User.LastReceivedMessage = DateTime.Now;

            if (MM_Historic_Reader.Queries.TryGetValue(QueryID, out FoundQuery))
                return FoundQuery.State;
            else
                return MM_Historic_Query.enumQueryState.Unknown;
        }

        /// <summary>
        /// Retrieve a query from the queue
        /// </summary>
        /// <param name="QueryID"></param>
        /// <returns></returns>
        public MM_Historic_Query RetrieveQueryResults(Guid QueryID)
        {
            MM_Historic_Query FoundQuery;
            if (MM_Historic_Reader.Queries.TryGetValue(QueryID, out FoundQuery))
            {
                FoundQuery.State = MM_Historic_Query.enumQueryState.Retrieved;
                return FoundQuery;
            }
            else
                return null;
        }
        #endregion

        #region Login activities
        /// <summary>
        /// Check our application version
        /// </summary>
        /// <param name="ClientName"></param>
        /// <returns></returns>
        public Version CheckApplicationVersion(string ClientName = "Macomber Map")
        {
            User.LastReceivedMessage = DateTime.Now;
            return Version.Parse(Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyVersionAttribute>().Version);
        }

        /// <summary>
        /// Handle a user login
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Password"></param>
        /// <param name="UserGuid"></param>
        /// <returns></returns>
        public string[] HandleUserLogin(string Name, string Password)
        {
            MM_Notification.WriteLine(ConsoleColor.Cyan, "User {0} attempting to log in from {1} on {2}", Name, User.IPAddress, DateTime.Now);
            User.UserName = Name;
            if (Settings.Default.AllowTestCredentials && Name == "**MacomberMapTesting**")
            {
                User.LoggedOnTime = DateTime.Now;
                User.ERCOTUser = true;
                return new string[] { "ERCOT" };
            }

            //Validate the user's credentials against active directory (thanks to http://stackoverflow.com/questions/290548/validate-a-username-and-password-against-active-directory)
            bool IsValid = false;
            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain))
                    IsValid = pc.ValidateCredentials(Name, Password);
            }
            catch { }
            if (IsValid && Settings.Default.AcceptAllDomainUsers)
                return new string[] { "ERCOT" };

            if (!IsValid || !File.Exists(Properties.Settings.Default.LoginFileLocation))
            {
                MM_Notification.WriteLine(ConsoleColor.Red, "User {0} log on from {1} failed: invalid credentials or no PERMIT area file.", Name, User.IPAddress);
                return new String[0];
            }

            //Now, check against our user names            
            foreach (String str in File.ReadAllLines(Properties.Settings.Default.LoginFileLocation))
                if (str.StartsWith(Name + ",", StringComparison.CurrentCultureIgnoreCase))
                {
                    User.LoggedOnTime = DateTime.Now;
                    String[] PermissionAreas = str.Substring(str.IndexOf(',') + 1).Split(',');
                    User.ERCOTUser = PermissionAreas.Contains("ERCOT");
                    return PermissionAreas;
                }

            //We didn't find our user, so log out.
            MM_Notification.WriteLine(ConsoleColor.Red, "User {0} log on from {1} failed: not found in PERMIT area file.", Name, User.IPAddress);
            return new string[0];
        }

        /// <summary>
        /// Handle a client first connecting
        /// </summary>
        /// <returns></returns>
        public Guid RegisterApplicationStartup(String IPAddress, int Port, String MachineName)
        {
            MM_Notification.WriteLine(ConsoleColor.Cyan, "New connection from {0} {1} on port {2}", IPAddress, MachineName, Port);
            Guid UserGUID = Guid.NewGuid();
            this.User = new MM_User() { UserId = UserGUID, UserName = "?", LoggedOnTime = DateTime.Now, IPAddress = IPAddress, MachineName = MachineName, Port = Port, LastReceivedMessage = DateTime.Now };
            User.LastReceivedMessage = DateTime.Now;
            MM_Server.AddUser(this);
            return UserGUID;
        }

        /// <summary>
        /// Return a null configuation XML
        /// </summary>
        /// <param name="ClientName"></param>
        /// <returns></returns>
        public string GetConfigurationXml(string ClientName = "Macomber Map")
        {
            User.LastReceivedMessage = DateTime.Now;
            return "<?xml version\"1.0\"?>";
        }

        /// <summary>
        /// Return an application of a particular version
        /// </summary>
        /// <param name="ClientName"></param>
        /// <param name="VersionToRetrieve"></param>
        /// <returns></returns>
        public byte[] GetApplicationVersion(string ClientName = "Macomber Map", Version VersionToRetrieve = default(Version))
        {
            User.LastReceivedMessage = DateTime.Now;
            return new byte[0];
        }
        #endregion

        #region Base activities
        /// <summary>
        /// Report the server name
        /// </summary>
        /// <returns></returns>
        public string ServerName()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.ServerURI.Host + ":" + MM_Server.ServerURI.Port.ToString();
        }

        /// <summary>
        /// Report the server version
        /// </summary>
        /// <returns></returns>
        public string ServerVersion()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Server.ApplicationVersion;
        }

        /// <summary>
        /// Report the server uptime
        /// </summary>
        /// <returns></returns>
        public TimeSpan ServerUptime()
        {
            User.LastReceivedMessage = DateTime.Now;
            return DateTime.Now - MM_Server.StartTime;
        }

        /// <summary>
        /// Handle a user's logout
        /// </summary>
        /// <param name="UserGuid"></param>
        /// <returns></returns>
        public bool HandleUserLogout(Guid UserGuid)
        {
            User.LastReceivedMessage = DateTime.Now;
            MM_Server.RemoveUser(this);
            return true;
        }
        #endregion

        #region Database activities
        #pragma warning disable 0618
        /// <summary>
        /// Report the most recent note date
        /// </summary>
        /// <returns></returns>
        public DateTime GetMostRecentNoteDate()
        {
            User.LastReceivedMessage = DateTime.Now;
            try
            {
                object Resp;
                using (DbCommand oCmd = MM_Database_Connector.CreateCommand("SELECT MAX(CreatedOn) FROM MM_Note", MM_Database_Connector.oConn))
                    Resp = oCmd.ExecuteScalar();
                if (Resp is DateTime)
                    return (DateTime)Resp;
                else
                    return DateTime.MinValue;
            }
            catch { return DateTime.MinValue; }
        }

        /// <summary>
        /// Load in the collection of notes
        /// </summary>
        /// <returns></returns>
        public MM_Note[] LoadNotes()
        {
            User.LastReceivedMessage = DateTime.Now;

            if (MM_Database_Connector.oConn == null || MM_Database_Connector.oConn.State != System.Data.ConnectionState.Open)
                return new MM_Note[0];
            List<MM_Note> OutNotes = new List<MM_Note>();
            using (DbCommand oCmd = MM_Database_Connector.CreateCommand("SELECT * FROM MM_NOTE", MM_Database_Connector.oConn))
            using (DbDataReader oRd = oCmd.ExecuteReader())
                while (oRd.Read())
                    OutNotes.Add(new MM_Note(oRd));
            return OutNotes.ToArray();
        }

        /// <summary>
        /// Upload a note, and return the new ID
        /// </summary>
        /// <param name="Note"></param>
        /// <returns></returns>
        public int UploadNote(MM_Note Note)
        {
            User.LastCommand = DateTime.Now;

            if (MM_Database_Connector.oConn == null || MM_Database_Connector.oConn.State != System.Data.ConnectionState.Open)
                return new Random().Next();
            using (DbCommand oCmd = MM_Database_Connector.CreateCommand(Settings.Default.DatabaseNoteInsertCommand, MM_Database_Connector.oConn))
            {
                oCmd.Prepare();

                MM_Database_Connector.AddParameter(oCmd, "CreatedOn", Note.CreatedOn);
                MM_Database_Connector.AddParameter(oCmd, "Author", Note.Author);
                MM_Database_Connector.AddParameter(oCmd, "Note", Note.Note);
                MM_Database_Connector.AddParameter(oCmd, "AssociatedElement", Note.AssociatedElement);
                MM_Database_Connector.AddParameter(oCmd, "Acknowledged", Note.Acknowledged);
                DbParameter Param = MM_Database_Connector.AddParameter(oCmd, "ID", null);
                Param.Direction = System.Data.ParameterDirection.Output;
                Param.DbType = System.Data.DbType.Int32;
                oCmd.ExecuteNonQuery();
                return Convert.ToInt32(oCmd.Parameters["ID"].Value);
            }
        }

        /// <summary>
        /// Load in all the training levels
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.MM_Training_Level[] LoadTrainingLevels()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Database_Connector.Levels;
        }

        /// <summary>
        /// Load in the collection of database models
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.MM_Database_Model[] LoadDatabaseModels()
        {
            User.LastReceivedMessage = DateTime.Now;
            return MM_Database_Connector.Models;
        }

        /// <summary>
        /// Start the training session
        /// </summary>
        /// <returns></returns>
        public int StartTrainingSession()
        {
            
            User.LastCommand = DateTime.Now;
            if (MM_Database_Connector.oConn == null)
                return -1;
            else
                try
                {
                    using (DbCommand dCmd = MM_Database_Connector.CreateCommand(Settings.Default.DatabaseTrainingStartCommand, MM_Database_Connector.oConn))
                    {
                        MM_Database_Connector.AddParameter(dCmd, "UserName", User.UserName);
                        MM_Database_Connector.AddParameter(dCmd, "CurrentDate", DateTime.Now);
                        MM_Database_Connector.AddParameter(dCmd, "MachineName", Environment.MachineName);
                        DbParameter Param = MM_Database_Connector.AddParameter(dCmd, "ID", null);
                        Object Resp = dCmd.ExecuteScalar();
                        if (Resp == null)
                            return Convert.ToInt32(Param.Value);
                        else
                            return Convert.ToInt32(Resp);
                    }
                }
                catch (Exception ex)
                { return -1; }
        }

        /// <summary>
        /// Update a training informational point
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="NewValue"></param>
        /// <param name="SessionId"></param>
        public bool UpdateTrainingInformation(string Title, float NewValue, int SessionId)
        {
            User.LastCommand = DateTime.Now;
            using (DbCommand dCmd = MM_Database_Connector.CreateCommand(Settings.Default.DatabaseTrainingUpdateCommand.Replace("{Title}", Title), MM_Database_Connector.oConn))
            {
                Object TitleValue = null;
                if (NewValue != 0)
                    TitleValue = NewValue;
                MM_Database_Connector.AddParameter(dCmd, "Title", TitleValue);
                MM_Database_Connector.AddParameter(dCmd, "LastUpdate", DateTime.Now);
                MM_Database_Connector.AddParameter(dCmd, "Id", SessionId);
                return dCmd.ExecuteNonQuery() == 1;
            }
        }

        /// <summary>
        /// Load model information
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        public byte[] LoadModel(MM_Database_Model Model)
        {
            User.LastReceivedMessage = DateTime.Now;
            MM_Database_Connector.LoadNetworkModelFlatFile();
            return MM_Database_Connector.MM_Model_File;

            //return OutString;
            /*using (OracleCommand oCmd = new OracleCommand("SELECT TO_CLOB(MMModel) FROM MM_DATABASE_MODEL WHERE ID={ModelNumber}", MM_Oracle_Connector.oConn))
            using (OracleDataReader oRd = oCmd.ExecuteReader())
                while (oRd.Read())
                    using (StreamReader sRd = new StreamReader(oRd.GetOracleLob(0), Encoding.Unicode))
                        return sRd.ReadToEnd();
            return null;*/
        }

        /// <summary>
        /// Load our Macomber Map configuration
        /// </summary>
        /// <returns></returns>
        public string LoadMacomberMapConfiguration()
        {
            User.LastReceivedMessage = DateTime.Now;
            return File.ReadAllText(Path.Combine(Settings.Default.SystemFileDirectory, "MacomberMapConfiguration.xml"));
        }

        /// <summary>
        /// Load our one-line data
        /// </summary>
        /// <param name="ElementName"></param>
        /// <param name="ElementType"></param>
        /// <returns></returns>
        public MM_OneLine_Data LoadOneLineData(string ElementName, MM_OneLine_Data.enumElementType ElementType)
        {
            User.LastReceivedMessage = DateTime.Now;
            MM_OneLine_Data OutData = new MM_OneLine_Data();
            OutData.ElementName = ElementName;
            OutData.ElementType = ElementType;
            if (ElementType == MM_OneLine_Data.enumElementType.BreakerToBreaker)
                ElementName = "__" + ElementName;
            else if (ElementType == MM_OneLine_Data.enumElementType.CompanyWide)
                ElementName = "___" + ElementName;

            String TargetFilename = Path.Combine(Settings.Default.OneLineDirectory, ElementName + ".MM_OneLine");
            if (File.Exists(TargetFilename))
                OutData.OneLineXml = File.ReadAllText(TargetFilename);
            return OutData;
        }

        /// <summary>
        /// Post coordinate suggestions
        /// </summary>
        /// <param name="Suggestions"></param>
        /// <returns></returns>
        public bool PostCoordinateSuggestions(MM_Coordinate_Suggestion[] Suggestions)
        {
            User.LastCommand = DateTime.Now;
            MM_Database_Connector.PostCoordinateSuggestions(Suggestions);
            return true;
        }
        #pragma warning restore 0618
        #endregion

        public delegate void SendMessageDelegate(MethodInfo Method, MM_WCF_Interface FoundConnection, object[] parameters);

        /// <summary>
        /// Send a message to our client
        /// </summary>
        /// <param name="SystemGuid"></param>
        /// <param name="ProcedureName"></param>
        /// <param name="Parameters"></param>
        public static void SendMessage(Guid SystemGuid, String ProcedureName, params object[] Parameters)
        {
            //First, try and find our client
            MM_WCF_Interface FoundConnection;
            MethodInfo FoundMethod;
            if (MM_Server.FindUser(SystemGuid, out FoundConnection))
                if (FoundConnection.ConversationHandler != null)
                    if ((FoundMethod = typeof(IMM_ConversationMessage_Types).GetMethod(ProcedureName)) != null)
                        new SendMessageDelegate(SendMessageInternal).BeginInvoke(FoundMethod, FoundConnection, Parameters, MessageSentCompletion, FoundConnection);
        }

        /// <summary>
        /// Send our message internally
        /// </summary>
        /// <param name="SystemGuid"></param>
        /// <param name="ProcedureName"></param>
        /// <param name="Parameters"></param>
        private static void SendMessageInternal(MethodInfo Method, MM_WCF_Interface FoundConnection, object[] parameters)
        {
            try
            {
                Method.Invoke(FoundConnection.ConversationHandler, parameters);
            }
            catch (Exception ex)
            {
                MM_Notification.WriteLine(ConsoleColor.Red, "Error sending {0} to {1}: {2}", Method.Name, FoundConnection.User.UserName, ex.Message);
            }
        }

        /// <summary>
        /// Handle message completion
        /// </summary>
        /// <param name="ar"></param>
        private static void MessageSentCompletion(IAsyncResult ar)
        {
            MM_WCF_Interface FoundConnection = (MM_WCF_Interface)ar.AsyncState;
            FoundConnection.User.LastSentMessage = DateTime.Now;
            //MM_Notification.WriteLine(ConsoleColor.Green, "Sent {0} to {1}", "?", FoundConnection.User.UserName);
        }

        /// <summary>
        /// Report our user status
        /// </summary>
        /// <param name="Status"></param>
        /// <returns></returns>
        public bool ReportUserStatus(MM_Client_Status Status)
        {
            foreach (PropertyInfo pI in typeof(MM_Client_Status).GetProperties())
                typeof(MM_User).GetProperty(pI.Name).SetValue(User, pI.GetValue(Status));
            return true;
        }

        /// <summary>
        /// Return the collection of EMS commands
        /// </summary>
        /// <returns></returns>
        public MM_EMS_Command[] GetEMSCommands()
        {
            User.LastReceivedMessage = DateTime.Now;
            return (Properties.Settings.Default.ForwardCommandsToERCOTClients) ? MM_Server.EMSCommands.ToArray() : new MM_EMS_Command[0];
        }

        /// <summary>
        /// Return our system epoch
        /// </summary>
        /// <returns></returns>
        public DateTime GetServerEpoch()
        {
            return MM_Server.ServerEpoch;
        }
    }
}