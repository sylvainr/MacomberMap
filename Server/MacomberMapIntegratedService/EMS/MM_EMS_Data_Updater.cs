using MacomberMapCommunications.Attributes;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.EMS;
using MacomberMapIntegratedService.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService.Service
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides all of our service types for handling EMS updates
    /// </summary>
    public static class MM_EMS_Data_Updater
    {
        #region Update data handler
        /// <summary>
        /// Handle our update data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="IncomingData"></param>
        /// <param name="ArrayType"></param>
        /// <param name="DataSource"></param>
        /// <param name="OutgoingData"></param>
        /// <returns></returns>
        private static void HandleUpdateData<T>(Array InArray, ref T[] OutgoingData, String ArrayType, String DataSource)
        {
            T[] IncomingData = (T[])InArray;
       
            //Build our list of changes, and send out
            List<T> ChangedData = new List<T>();
            T[] OldData = OutgoingData;
            int MinLength = Math.Min(OldData.Length, IncomingData.Length);
            if (OldData != null && OldData.Length > 0)
            {
                for (int a = 0; a < MinLength; a++)
                    if (!MM_Comparable.IsDataIdentical(IncomingData[a], OldData[a]))
                        ChangedData.Add(IncomingData[a]);

                //Based on the system design, this should never be necessary.
                //But, if we have additional lines, make sure to flag them as changes.
                if (IncomingData.Length > MinLength)
                    for (int a = MinLength; a < IncomingData.Length; a++)
                        ChangedData.Add(IncomingData[a]);
            }
            else
                ChangedData.AddRange(IncomingData);

            //Now, assign our new data
            OutgoingData = IncomingData;

            //If we have changed data, send it out.
                T[] UpdatesToSend = ChangedData.ToArray();

                //Find our update command
                UpdateCommandAttribute UpdateCommand = null;
                foreach (object obj in typeof(T).GetCustomAttributes(false))
                    if (obj is UpdateCommandAttribute)
                        UpdateCommand = (UpdateCommandAttribute)obj;

                //Update our timestamp
                MM_Server.UpdateTimestamp(typeof(T), IncomingData.Length);

                //Send out notifications to our clients
                DateTime StartTime = DateTime.Now;
                foreach (MM_WCF_Interface Interface in MM_Server.ConnectedUserList)
                    if (Interface.User.LoggedOnTime != default(DateTime) && Interface.User.UserName != "?")
                        try
                        {                            
                            MM_WCF_Interface.SendMessage(Interface.User.UserId, UpdateCommand.UpdateCommand, UpdatesToSend);
                        }
                        catch (Exception ex)
                        {
                            while (ex.InnerException != null)
                                ex = ex.InnerException;
                            MM_Notification.WriteLine(ConsoleColor.Red, "Unable to send {0} to {1} on {2}: {3}", UpdateCommand.UpdateCommand, Interface.User.UserName, Interface.User.IPAddress, ex.Message);
                        }
        }

        /// <summary>
        /// Handle our update data with a dictionary system
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="IncomingData"></param>
        /// <param name="OutgoingData"></param>
        /// <param name="ArrayType"></param>
        /// <param name="DataSource"></param>
        private static void HandleUpdateData<S, T>(Array InArray, ref Dictionary<S, T> OutgoingData, String ArrayType, String DataSource)
        {
            T[] IncomingData = (T[])InArray;

            //Capture the old and new data
            //MM_Notification.WriteLine(ConsoleColor.Green, "Received {0:#,##0} {1} from {2} on {3}.", IncomingData.Length, ArrayType, DataSource, DateTime.Now);

            //First, find our attribute data for updates and removes
            UpdateCommandAttribute UpdateCommand = null;
            RemovalCommandAttribute RemoveCommand = null;
            DoNotRemoveAttribute DoNotRemoveCommand = null;
            foreach (object obj in typeof(T).GetCustomAttributes(false))
                if (obj is UpdateCommandAttribute)
                    UpdateCommand = (UpdateCommandAttribute)obj;
                else if (obj is RemovalCommandAttribute)
                    RemoveCommand = (RemovalCommandAttribute)obj;
                else if (obj is DoNotRemoveAttribute)
                    DoNotRemoveCommand = (DoNotRemoveAttribute)obj;

            //Make sure we have the proper index information for our remove command
            List<PropertyInfo> IndexInfo = new List<PropertyInfo>();
            foreach (String str in RemoveCommand.IndexColumn.Split(','))
                if (!String.IsNullOrEmpty(str))
                    IndexInfo.Add(typeof(T).GetProperty(str));

            //Build our list of changes, and send out
            List<T> UpdatedData = new List<T>();
            Dictionary<S, T> OldData = OutgoingData;
            Dictionary<S, T> NewData = new Dictionary<S, T>();

            //Now, go line by line and check for our values
            T FoundT;
            foreach (T InLine in IncomingData)
            {
                S Index = default(S);
                if (typeof(S) == typeof(int))
                    Index = (S)IndexInfo[0].GetValue(InLine);
                else if (typeof(S) == typeof(String))
                {
                    String ThisIndex = "";
                    foreach (PropertyInfo pI in IndexInfo)
                        ThisIndex += pI.GetValue(InLine).ToString();
                    Index = (S)(object)ThisIndex;
                }

                if (!OutgoingData.TryGetValue(Index, out FoundT))
                    UpdatedData.Add(InLine);
                else if (!MM_Comparable.IsDataIdentical(InLine, FoundT))
                    UpdatedData.Add(InLine);
                if (!NewData.ContainsKey(Index))
                    NewData.Add(Index, InLine);
            }

            //Now, swap in our new data and track anything missing - old data now is tracking anything left has been removed
            OutgoingData = NewData;
            if (DoNotRemoveCommand == null)
                foreach (S Index in NewData.Keys)
                    OldData.Remove(Index);

            //If we have changed data, send it out.
            //if (UpdatedData.Count != 0 || OldData.Count != 0)

            T[] UpdatesToSend = UpdatedData.ToArray();
            T[] RemovalsToHandle = OldData.Values.ToArray();

            //Update our timestamp
            MM_Server.UpdateTimestamp(typeof(T), IncomingData.Length);

            //Send out notifications to our clients
            // if (UpdatesToSend.Length > 0)
            foreach (MM_WCF_Interface Interface in MM_Server.ConnectedUserList)
                if (Interface.User.LoggedOnTime != default(DateTime))
                    try
                    {
                        MM_WCF_Interface.SendMessage(Interface.User.UserId, UpdateCommand.UpdateCommand, UpdatesToSend);
                    }
                    catch (Exception ex)
                    {
                        MM_Notification.WriteLine(ConsoleColor.Red, "Unable to send {0} to {1}: {2}", UpdateCommand.UpdateCommand, Interface.User.UserName, ex);
                    }

            if (RemovalsToHandle.Length > 0 && !String.IsNullOrEmpty(RemoveCommand.RemovalCommand) && DoNotRemoveCommand == null)
                foreach (MM_WCF_Interface Interface in MM_Server.ConnectedUserList)
                    if (Interface.User.LoggedOnTime != default(DateTime))
                        try
                        {
                            MM_WCF_Interface.SendMessage(Interface.User.UserId, RemoveCommand.RemovalCommand,RemovalsToHandle );
                        }
                        catch (Exception ex)
                        {
                            MM_Notification.WriteLine(ConsoleColor.Red, "Unable to send {0} to {1}: {2}", RemoveCommand.RemovalCommand, Interface.User.UserName, ex);
                        }
        }
        #endregion

        #region Indiviual update processing
        /// <summary>
        /// Process our incoming data
        /// </summary>
        /// <param name="DataType"></param>
        /// <param name="InData"></param>
        public static void ProcessUpdate(Type DataType, Array InData)
        {
            if (DataType == typeof(MM_Line_Data))
                HandleUpdateData<MM_Line_Data>(InData, ref MM_Server.LineData, "lines", "line data");
            else if (DataType == typeof(MM_Island_Data))
                HandleUpdateData<int, MM_Island_Data>(InData, ref MM_Server.IslandData, "islands", "island data");
            else if (DataType == typeof(MM_Load_Data))
                HandleUpdateData<MM_Load_Data>(InData, ref MM_Server.LoadData, "loads", "load data");
            else if (DataType == typeof(MM_Bus_Data))
                HandleUpdateData<int, MM_Bus_Data>(InData, ref MM_Server.BusData, "buses", "bus data");
            else if (DataType == typeof(MM_Outage_Line_Data))
                HandleUpdateData<int, MM_Outage_Line_Data>(InData, ref MM_Server.LineOutageData, "line outages", "line outage data");
            else if (DataType == typeof(MM_Outage_Unit_Data))
                HandleUpdateData<int, MM_Outage_Unit_Data>(InData, ref MM_Server.UnitOutageData, "unit outages", "unit outage data");
            else if (DataType == typeof(MM_Outage_Transformer_Data))
                HandleUpdateData<int, MM_Outage_Transformer_Data>(InData, ref MM_Server.TransformerOutageData, "transformer outages", "transformer outage data");
            else if (DataType == typeof(MM_Analog_Measurement))
                HandleUpdateData<MM_Analog_Measurement>(InData, ref MM_Server.AnalogMeasurementData, "analog measurements", "analog measurement data");
            else if (DataType == typeof(MM_ShuntCompensator_Data))
                HandleUpdateData<MM_ShuntCompensator_Data>(InData, ref MM_Server.ShuntCompensatorData, "shunt compensators", "shunt compensator data");
            else if (DataType == typeof(MM_State_Measurement))
                HandleUpdateData<MM_State_Measurement>(InData, ref MM_Server.StateMeasurementData, "state measurements", "state measurement data");
            else if (DataType == typeof(MM_Transformer_Data))
                HandleUpdateData<MM_Transformer_Data>(InData, ref MM_Server.TransformerData, "transformers", "transformer data");
            else if (DataType == typeof(MM_Transformer_PhaseShifter_Data))
                HandleUpdateData<MM_Transformer_PhaseShifter_Data>(InData, ref MM_Server.TransformerPhaseShifterData, "phase shifters", "phase shifter data");
            else if (DataType == typeof(MM_Unit_Data))
                HandleUpdateData<MM_Unit_Data>(InData, ref MM_Server.UnitData, "units", "unit data");
            else if (DataType == typeof(MM_ZeroImpedanceBridge_Data))
                HandleUpdateData<MM_ZeroImpedanceBridge_Data>(InData, ref MM_Server.ZBRData, "ZBRs", "ZBR data");
            else if (DataType == typeof(MM_BreakerSwitch_Data))
                HandleUpdateData<MM_BreakerSwitch_Data>(InData, ref MM_Server.BreakerSwitchData, "Breakers/Switches", "CB data");
            else if (DataType == typeof(MM_Scada_Analog))
                HandleUpdateData<MM_Scada_Analog>(InData, ref MM_Server.SCADAAnalogs, "SCADA Analogs", "SCADA data");
            else if (DataType == typeof(MM_Scada_Status))
                HandleUpdateData<MM_Scada_Status>(InData, ref MM_Server.SCADAStatuses, "SCADA status points", "SCADA data");
            else if (DataType == typeof(MM_Chart_Data))
                HandleUpdateData<MM_Chart_Data>(InData, ref MM_Server.ChartData, "Chart points", "chart data");
            else if (DataType == typeof(MM_Tie_Data))
                HandleUpdateData<MM_Tie_Data>(InData, ref MM_Server.TieData, "DC Ties", "Tie data");
            else if (DataType == typeof(MM_StaticVarCompensator_Data))
                HandleUpdateData<MM_StaticVarCompensator_Data>(InData, ref MM_Server.SVCData, "SVCs", "SVC data");
            else if (DataType == typeof(MM_SystemWide_Generation_Data))
                HandleUpdateData<MM_SystemWide_Generation_Data>(InData, ref MM_Server.SystemWideGenerationData, "Areas", "System-wide area data");
            else if (DataType == typeof(MM_UnitType_Generation_Data))
                HandleUpdateData<MM_UnitType_Generation_Data>(InData, ref MM_Server.UnitTypeGenerationData, "Unit Types", "Unit Type data");
            else if (DataType == typeof(MM_Unit_Gen_Data))
                HandleUpdateData<MM_Unit_Gen_Data>(InData, ref MM_Server.UnitGenData, "units", "Unit generation data");
            else if (DataType == typeof(MM_Operatorship_Update))
                HandleUpdateData<int, MM_Operatorship_Update>(InData, ref MM_Server.OperatorshipUpdates, "operatorship changes", "Operatorship update data");
            else if (DataType == typeof(MM_Basecase_Violation_Data))
                HandleUpdateData<int, MM_Basecase_Violation_Data>(InData, ref MM_Server.BasecaseViolationData, "Basecase violations", "basecase data");
            else if (DataType == typeof(MM_Contingency_Violation_Data))
                HandleUpdateData<string, MM_Contingency_Violation_Data>(InData, ref MM_Server.ContingencyViolationData, "Contingencies", "contingency data");
            else if (DataType == typeof(MM_Load_Forecast_Data))
                HandleUpdateData<MM_Load_Forecast_Data>(InData, ref MM_Server.LoadForecastData, "Load forecast points", "LF data");
            else if (DataType == typeof(MM_Interface_Monitoring_Data))
                HandleUpdateData<MM_Interface_Monitoring_Data>(InData, ref MM_Server.InterfaceData, "Interfaces", "interface data");
            else if (DataType == typeof(MM_Unit_Simulation_Data))
                HandleUpdateData<MM_Unit_Simulation_Data>(InData, ref MM_Server.UnitSimulationData, "Unit simulation points", "Simulation data");
            else if (DataType == typeof(MM_Island_Simulation_Data))
                HandleUpdateData<int, MM_Island_Simulation_Data>(InData, ref MM_Server.IslandSimulationData, "Island simulation points", "Simulation data");
            else if (DataType == typeof(MM_Synchroscope_Data))
                HandleUpdateData<MM_Synchroscope_Data>(InData, ref MM_Server.SynchroscopeData, "Synchroscope points", "Simulation data");
            else if (DataType == typeof(MM_Simulation_Time))
                HandleUpdateData<MM_Simulation_Time>(InData, ref MM_Server.SimulatorTimeData, "Simulator time", "Simulation data");
            else if (DataType == typeof(MM_Unit_Control_Status))
                HandleUpdateData<int, MM_Unit_Control_Status>(InData, ref MM_Server.UnitControlStatusData, "Unit control status data", "Unit control data");
            else
                MM_Notification.WriteLine(ConsoleColor.Red, "Unknown data type: {0}", DataType.Name);
        }
        #endregion
    }
}