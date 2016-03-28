using MacomberMapCommunications.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Server_Connectivity
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides our wrapper to 
    /// </summary>
    public class MM_WCF_Interface : DuplexClientBase<IMM_WCF_Interface>, IMM_WCF_Interface, IDisposable
    {
        #region Variable declarations
        /// <summary>Our inner channel</summary>
        private IMM_WCF_Interface innerChannel;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new WCF interface
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="remoteAddress"></param>
        public MM_WCF_Interface(InstanceContext binding, ServiceEndpoint remoteAddress)
            : base(binding, remoteAddress)
        {
            innerChannel = (IMM_WCF_Interface)base.InnerChannel;
        }
        ~MM_WCF_Interface()
        {
            Dispose();
        }
        public void Dispose()
        {
            try
            {
                if (base.InnerChannel != null)
                {
                    try
                    {
                        if (base.InnerChannel.State == CommunicationState.Faulted)
                            base.InnerChannel.Abort();
                        else
                            base.InnerChannel.Close();
                    }
                    catch (Exception)
                    {
                    }
                    base.InnerChannel.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Initialize a new WCF interface
        /// </summary>
        /// <param name="callbackInstance"></param>
        /// <param name="binding"></param>
        /// <param name="remoteAddress"></param>
        public MM_WCF_Interface(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
            : base(callbackInstance, binding, remoteAddress)
        {
            innerChannel = (IMM_WCF_Interface)base.InnerChannel;

        }
        #endregion

        #region Proxy interfaces
        /// <summary>
        /// Ping a server
        /// </summary>
        /// <returns></returns>
        public bool Ping()
        {
            return innerChannel.Ping();
        }

        /// <summary>
        /// Get our load data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Load_Data[] GetLoadData()
        {
            return innerChannel.GetLoadData();
        }

        /// <summary>
        /// Get our LF data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Load_Forecast_Data[] GetLoadForecastData()
        {
            return innerChannel.GetLoadForecastData();
        }

        /// <summary>
        /// Get our operatorship updates
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Operatorship_Update[] GetOperatorshipUpdates()
        {
            return innerChannel.GetOperatorshipUpdates();
        }

        /// <summary>
        /// Get our interface data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Interface_Monitoring_Data[] GetInterfaceData()
        {
            return innerChannel.GetInterfaceData();
        }



        /// <summary>
        /// Get island data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Island_Data[] GetIslandData()
        {
            return innerChannel.GetIslandData();
        }

        /// <summary>
        /// Get our line data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Line_Data[] GetLineData()
        {
            return innerChannel.GetLineData();
        }

        /// <summary>
        /// Get our bus data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Bus_Data[] GetBusData()
        {
            return innerChannel.GetBusData();
        }

        /// <summary>
        /// Get our line outage data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Outage_Line_Data[] GetLineOutageData()
        {
            return innerChannel.GetLineOutageData();
        }

        /// <summary>
        /// Get our unit outage data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Outage_Unit_Data[] GetUnitOutageData()
        {
            return innerChannel.GetUnitOutageData();
        }

        /// <summary>
        /// Get our transformer outage data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Outage_Transformer_Data[] GetTransformerOutageData()
        {
            return innerChannel.GetTransformerOutageData();
        }

        /// <summary>
        /// Check our latest timestamps
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.MM_Data_Collection[] CheckLatestTimestamps()
        {
            return innerChannel.CheckLatestTimestamps();
        }

        /// <summary>
        /// Get our analog measurement data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Analog_Measurement[] GetAnalogMeasurementData()
        {
            return innerChannel.GetAnalogMeasurementData();
        }

        /// <summary>
        /// Get our shunt compensator data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_ShuntCompensator_Data[] GetShuntCompensatorData()
        {
            return innerChannel.GetShuntCompensatorData();
        }

        /// <summary>
        /// Get our state measurement data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_State_Measurement[] GetStateMeasurementData()
        {
            return innerChannel.GetStateMeasurementData();
        }

        /// <summary>
        /// Get our transformer data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Transformer_Data[] GetTransformerData()
        {
            return innerChannel.GetTransformerData();
        }

        /// <summary>
        /// Get our phase-shifting transformer information
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Transformer_PhaseShifter_Data[] GetTransformerPhaseShifterData()
        {
            return innerChannel.GetTransformerPhaseShifterData();
        }

        /// <summary>
        /// Get our unit data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Unit_Data[] GetUnitData()
        {
            return innerChannel.GetUnitData();
        }

        /// <summary>
        /// Get our unit simulation data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Unit_Simulation_Data[] GetUnitSimulationData()
        {
            return innerChannel.GetUnitSimulationData();
        }

        /// <summary>
        /// Get our island simulation data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Island_Simulation_Data[] GetIslandSimulationData()
        {
            return innerChannel.GetIslandSimulationData();
        }

        /// <summary>
        /// Get our synchroscope data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Synchroscope_Data[] GetSynchroscopeData()
        {
            return innerChannel.GetSynchroscopeData();
        }

        /// <summary>
        /// Get our simulator time
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Simulation_Time[] GetSimulatorTime()
        {
            return innerChannel.GetSimulatorTime();
        }

        /// <summary>
        /// Get our ZBR data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_ZeroImpedanceBridge_Data[] GetZBRData()
        {
            return innerChannel.GetZBRData();
        }

        /// <summary>
        /// Get our breaker/switch data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_BreakerSwitch_Data[] GetBreakerSwitchData()
        {
            return innerChannel.GetBreakerSwitchData();
        }

        /// <summary>
        /// Get our chart data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Chart_Data[] GetChartData()
        {
            return innerChannel.GetChartData();
        }

        /// <summary>
        /// Get our chart data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.MM_LMP_Data[] GetLmpData()
        {
            return innerChannel.GetLmpData();
        }


        /// <summary>Get our DC Tie data</summary><returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Tie_Data[] GetTieData()
        {
            return innerChannel.GetTieData();
        }

        /// <summary>Get our SVC data</summary><returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_StaticVarCompensator_Data[] GetStaticVarCompensatorData()
        {
            return innerChannel.GetStaticVarCompensatorData();
        }

        /// <summary>Get our system-wide data</summary><returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_SystemWide_Generation_Data[] GetSystemWideGenerationData()
        {
            return innerChannel.GetSystemWideGenerationData();
        }

        /// <summary>Get our unit-type generation data</summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_UnitType_Generation_Data[] GetUnitTypeGenerationData()
        {
            return innerChannel.GetUnitTypeGenerationData();
        }

        /// <summary>Get our unit generation-specific data</summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Unit_Gen_Data[] GetUnitGenData()
        {
            return innerChannel.GetUnitGenData();
        }

        /// <summary>
        /// Get flowgate data
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Flowgate_Data[] GetFlowgateData()
        {
            return innerChannel.GetFlowgateData();
        }



        /// <summary>Get our basecase violation data</summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Basecase_Violation_Data[] GetBasecaseViolationData()
        {
            return innerChannel.GetBasecaseViolationData();
        }

        /// <summary>Get our contingency violation data</summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Contingency_Violation_Data[] GetContingencyVioltationData()
        {
            return innerChannel.GetContingencyVioltationData();
        }

        /// <summary>Get our SCADA Analog data</summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Scada_Analog[] GetSCADAAnalogData()
        {
            return innerChannel.GetSCADAAnalogData();
        }

        /// <summary>Get our SCADA Analog data</summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_Scada_Status[] GetSCADAStatusData()
        {
            return innerChannel.GetSCADAStatusData();
        }

        /// <summary>
        /// Send a command to our system
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="OldValue"></param>
        /// <returns></returns>
        public bool SendCommand(string Command, string OldValue)
        {
            return innerChannel.SendCommand(Command, OldValue);
        }
        DateTime? lastConnect = null;
        /// <summary>Report the server name</summary>
        /// <returns></returns>
        public string ServerName()
        {
            try
            {
                return innerChannel.ServerName();
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex.Message);

                try
                {
                    if (ex.Message.IndexOf("aborted", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("Faulted", StringComparison.OrdinalIgnoreCase) >= 0 && (!lastConnect.HasValue || (DateTime.Now - lastConnect.Value).TotalSeconds > 10))
                    {
                        // can we reconnect?
                        Exception error = null;
                        // this login method should reconnect us.
                        if (MM_Server_Interface.TryLogin(MM_Server_Interface.ServerName, MM_Server_Interface.ConnectionURI, MM_Server_Interface.UserName, MM_Server_Interface.Password, out error))
                            return MM_Server_Interface.ServerName;
                        lastConnect = DateTime.Now;
                        if (error != null)
                            throw error;

                    }
                }
                catch (Exception exp)
                {
                    MM_System_Interfaces.LogError(exp.Message);
                }

                return "SERVER ERROR";
            }
        }

        /// <summary>Report the server version</summary>
        /// <returns></returns>
        public string ServerVersion()
        {
            return innerChannel.ServerVersion();
        }

        /// <summary>Report the server uptime</summary>
        public TimeSpan ServerUptime()
        {
            return innerChannel.ServerUptime();
        }

        /// <summary>
        /// Handle a user logout
        /// </summary>
        /// <param name="UserGuid">The user's GUID</param>
        public bool HandleUserLogout(Guid UserGuid)
        {
            return innerChannel.HandleUserLogout(UserGuid);
        }

        /// <summary>
        /// Initiate a historic systems query, and return a Guid of the request
        /// </summary>
        /// <param name="TagSQL"></param>
        /// <param name="EndTime"></param>
        /// <param name="NumPoints"></param>
        /// <param name="StartTime"></param>
        /// <returns></returns>
        public Guid QueryTags(string TagSQL, DateTime StartTime, DateTime EndTime, int NumPoints)
        {
            return innerChannel.QueryTags(TagSQL, StartTime, EndTime, NumPoints);
        }

        /// <summary>
        /// Check on the status of a historic query
        /// </summary>
        /// <param name="QueryID"></param>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.MM_Historic_Query.enumQueryState CheckQueryStatus(Guid QueryID)
        {
            return innerChannel.CheckQueryStatus(QueryID);
        }

        /// <summary>
        /// Retrieve the results of a query
        /// </summary>
        /// <param name="QueryID"></param>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.MM_Historic_Query RetrieveQueryResults(Guid QueryID)
        {
            return innerChannel.RetrieveQueryResults(QueryID);
        }

        /// <summary>Check the most recent client version</summary>
        /// <returns></returns>
        public Version CheckApplicationVersion(string ClientName = "Macomber Map")
        {
            return innerChannel.CheckApplicationVersion(ClientName);
        }

        /// <summary>
        /// Handle a user login, including passing a user-specific token
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public string[] HandleUserLogin(string Name, string Password)
        {
            try
            {
                return innerChannel.HandleUserLogin(Name, Password);
            }
            catch (Exception ex)
            {
                /* if (ex.Message.IndexOf("could not", StringComparison.CurrentCultureIgnoreCase) >= 0)
                 {
                     if (innerChannel != null)
                         ((MM_WCF_Interface)innerChannel).Dispose();
                     innerChannel = new MM_WCF_Interface(MM_Server_Interface.CallbackHandler.Context, Endpoint);
                 } */
                throw ex;
            }
        }

        /// <summary>
        /// Get the configuration XML for our client, based on our login credentials
        /// </summary>
        /// <param name="ClientName"></param>
        /// <returns></returns>
        public string GetConfigurationXml(string ClientName = "Macomber Map")
        {
            return innerChannel.GetConfigurationXml(ClientName);
        }

        /// <summary>
        /// Retrieve the installer for a particular version of the application
        /// </summary>
        /// <param name="ClientName"></param>
        /// <param name="VersionToRetrieve"></param>
        /// <returns></returns>
        public byte[] GetApplicationVersion(string ClientName = "Macomber Map", Version VersionToRetrieve = default(Version))
        {
            return innerChannel.GetApplicationVersion(ClientName, VersionToRetrieve);
        }

        /// <summary>
        /// Get the most recent date/time stamp on our notes
        /// </summary>
        /// <returns></returns>
        public DateTime GetMostRecentNoteDate()
        {
            return innerChannel.GetMostRecentNoteDate();
        }

        /// <summary>
        /// Load in our collection of notes
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.MM_Note[] LoadNotes()
        {
            return innerChannel.LoadNotes();
        }

        /// <summary>
        /// Upload a note, and return the ID for the note
        /// </summary>
        /// <param name="Note"></param>
        /// <returns></returns>
        public int UploadNote(MacomberMapCommunications.Messages.MM_Note Note)
        {
            return innerChannel.UploadNote(Note);
        }

        /// <summary>
        /// Get the level information on the training program
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.MM_Training_Level[] LoadTrainingLevels()
        {
            return innerChannel.LoadTrainingLevels();
        }

        /// <summary>
        /// Load in our collection of database models
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.MM_Database_Model[] LoadDatabaseModels()
        {
            return innerChannel.LoadDatabaseModels();
        }

        /// <summary>
        /// Load our model information
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        public byte[] LoadModel(MacomberMapCommunications.Messages.MM_Database_Model Model)
        {
            return innerChannel.LoadModel(Model);
        }

        /// <summary>
        /// Start a training session, and return the unique ID for the training session
        /// </summary>
        /// <returns></returns>
        public int StartTrainingSession()
        {
            return innerChannel.StartTrainingSession();
        }

        /// <summary>
        /// Update a parameter on our training session
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="NewValue"></param>
        /// <param name="SessionId"></param>
        public bool UpdateTrainingInformation(string Title, float NewValue, int SessionId)
        {
            return innerChannel.UpdateTrainingInformation(Title, NewValue, SessionId);
        }

        /// <summary>
        /// Load the Macomber Map configuration
        /// </summary>
        /// <returns></returns>
        public string LoadMacomberMapConfiguration()
        {
            return innerChannel.LoadMacomberMapConfiguration();
        }

        /// <summary>
        /// Load one-line data, and send to 
        /// </summary>
        /// <param name="ElementName"></param>
        /// <param name="ElementType"></param>        
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_OneLine_Data LoadOneLineData(string ElementName, MacomberMapCommunications.Messages.EMS.MM_OneLine_Data.enumElementType ElementType)
        {
            return innerChannel.LoadOneLineData(ElementName, ElementType);
        }

        /// <summary>
        /// Propose a coordinate update, storing in the database
        /// </summary>
        /// <param name="Suggestions"></param>
        /// <returns></returns>
        public bool PostCoordinateSuggestions(MacomberMapCommunications.Messages.Communications.MM_Coordinate_Suggestion[] Suggestions)
        {
            return innerChannel.PostCoordinateSuggestions(Suggestions);
        }

        /// <summary>
        /// Register for our callback
        /// </summary>
        /// <returns></returns>
        public bool RegisterCallback(String VersionNumber = "Unknown")
        {
            return innerChannel.RegisterCallback(VersionNumber);
        }

        /// <summary>
        /// Report the user status
        /// </summary>
        /// <param name="Status"></param>
        /// <returns></returns>
        public bool ReportUserStatus(MacomberMapCommunications.Messages.MM_Client_Status Status)
        {
            try
            {
                return innerChannel.ReportUserStatus(Status);
            }
            catch (Exception ex)
            {

                MM_System_Interfaces.LogError(ex.Message);

                try
                {
                    if (ex.Message.IndexOf("aborted", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("Faulted", StringComparison.OrdinalIgnoreCase) >= 0 && (!lastConnect.HasValue || (DateTime.Now - lastConnect.Value).TotalSeconds > 10))
                    {
                        // can we reconnect?
                        Exception error = null;
                        // this login method should reconnect us.
                        MM_Server_Interface.TryLogin(MM_Server_Interface.ServerName, MM_Server_Interface.ConnectionURI, MM_Server_Interface.UserName, MM_Server_Interface.Password, out error);
                        lastConnect = DateTime.Now;
                        if (error != null)
                            throw error;

                    }
                }
                catch (Exception exp)
                {
                    MM_System_Interfaces.LogError(exp.Message);
                }
                return false;
            }
        }

        /// <summary>
        /// Retrieve our historical list of EMS commands
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_EMS_Command[] GetEMSCommands()
        {
            return innerChannel.GetEMSCommands();
        }

        public MacomberMapCommunications.Messages.EMS.MM_Unit_Control_Status[] GetUnitControlStatus()
        {
            return innerChannel.GetUnitControlStatus();
        }


        public bool UpdateUnitControlStatusInformation(MacomberMapCommunications.Messages.EMS.MM_Unit_Control_Status Status)
        {
            return innerChannel.UpdateUnitControlStatusInformation(Status);
        }

        public DateTime GetServerEpoch()
        {
            return innerChannel.GetServerEpoch();
        }
        #endregion
    }
}