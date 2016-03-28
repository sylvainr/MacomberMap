using MacomberMapCommunications.Attributes;
using MacomberMapCommunications.Messages.EMS;
using MacomberMapCommunications.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapIntegratedService.WCF
{
    /// <summary>
    /// This class provides the conversation message handler, receiving an input via named pipe from the MM server, which is relayed to the client.
    /// </summary>
    public class MM_ConversationMessage_Types : IMM_ConversationMessage_Types
    {
        #region Variable declarations
        /// <summary>The unique GUID for our conversation handler</summary>
        public Guid ConversationGuid = Guid.Empty;

        /// <summary>Our conversation handler</summary>
        public MM_ConversationMessage_Types ConversationHandler;

        /// <summary>Our callback handler to talk with the client</summary>
        public IMM_ConversationMessage_Types Callback;
        #endregion

        #region Initialization
       
        /// <summary>
        /// When our channel faults, disconnect and indicate to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Channel_Faulted(object sender, EventArgs e)
        {
           // MM_Interprocess_Communication.RequestData<bool>("HandleUserLogout", MM_Interprocess_Communication.enumCommunicationService.Base, ConversationGuid);
        }

        /// <summary>
        /// Handle the initializion of our conversation handler
        /// </summary>
        public MM_ConversationMessage_Types()
        {
            OperationContext.Current.Channel.Faulted += Channel_Faulted;            

            //MM_ConversationMessage_Types.ConversationHandler = this;
        }
        #endregion


        /// <summary>
        /// When we receive a message, send over to the client
        /// </summary>
        /// <param name="MessageSource"></param>
        /// <param name="Icon"></param>
        /// <param name="Info"></param>
        public void ReceiveMessage(string Info, string MessageSource, MessageBoxIcon Icon)
        {
            Callback.ReceiveMessage(Info, MessageSource, Icon);
        }

        /// <summary>
        /// When we receive a callback request, send it
        /// </summary>
        public void CloseClient()
        {
            Callback.CloseClient();
        }

        /// <summary>
        /// Process data, and send to the client
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="InArray"></param>
        public void ProcessData<T>(T[] InArray)
        {
            //Unfortunately we need to copy our array, and send that.
            T[] OutData = new T[InArray.Length];
            Array.Copy(InArray, OutData, OutData.Length);
            Callback.GetType().GetMethod(((UpdateCommandAttribute)typeof(T).GetCustomAttributes(typeof(UpdateCommandAttribute), false)[0]).UpdateCommand).Invoke(Callback, new object[] { OutData });
        }

        /// <summary>
        /// Process data removal, and send to the client
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="InArray"></param>
        public void ProcessDataRemoval<T>(T[] InArray)
        {
            //Unfortunately we need to copy our array, and send that.
            T[] OutData = new T[InArray.Length];
            Array.Copy(InArray, OutData, OutData.Length);
            RemovalCommandAttribute RemovalCommand = (RemovalCommandAttribute)typeof(T).GetCustomAttributes(typeof(RemovalCommandAttribute), false)[0];
            Callback.GetType().GetMethod(RemovalCommand.RemovalCommand).Invoke(Callback, new object[] { OutData });
        }

        #region Message command transfers to client
        /// <summary>
        /// When we receive line data, send over to the client
        /// </summary>
        /// <param name="LineData"></param>
        public void UpdateLineData(MM_Line_Data[] LineData)
        {
            ProcessData<MM_Line_Data>(LineData);
        }

        /// <summary>
        /// When we receive bus data, send over to the client
        /// </summary>
        /// <param name="BusData"></param>
        public void UpdateBusData(MM_Bus_Data[] BusData)
        {
            ProcessData<MM_Bus_Data>(BusData);
        }

        /// <summary>
        /// When we receive line outage data, send over to the client
        /// </summary>
        /// <param name="LineOutageData"></param>
        public void UpdateLineOutageData(MM_Outage_Line_Data[] LineOutageData)
        {
            ProcessData<MM_Outage_Line_Data>(LineOutageData);
        }


        /// <summary>
        /// When we receive load data, send over to the client
        /// </summary>
        /// <param name="LoadData"></param>
        public void UpdateLoadData(MM_Load_Data[] LoadData)
        {
            ProcessData<MM_Load_Data>(LoadData);
        }

        /// <summary>
        /// When we receive load forecast data, send over to the client
        /// </summary>
        /// <param name="LoadForecastData"></param>
        public void UpdateLoadForecastData(MM_Load_Forecast_Data[] LoadForecastData)
        {
            ProcessData<MM_Load_Forecast_Data>(LoadForecastData);
        }

        /// <summary>
        /// Update our island data
        /// </summary>
        /// <param name="IslandData"></param>
        public void UpdateIslandData(MM_Island_Data[] IslandData)
        {
            ProcessData<MM_Island_Data>(IslandData);
        }

        /// <summary>
        /// Update our unit outage data
        /// </summary>
        /// <param name="UnitOutageData"></param>
        public void UpdateUnitOutageData(MM_Outage_Unit_Data[] UnitOutageData)
        {
            ProcessData<MM_Outage_Unit_Data>(UnitOutageData);
        }

        /// <summary>
        /// Update our transformer outage data
        /// </summary>
        /// <param name="TransformerOutageData"></param>
        public void UpdateTransformerOutageData(MM_Outage_Transformer_Data[] TransformerOutageData)
        {
            ProcessData<MM_Outage_Transformer_Data>(TransformerOutageData);
        }

        /// <summary>
        /// Update our analog measurement data
        /// </summary>
        /// <param name="AnalogMeasurementData"></param>
        public void UpdateAnalogMeasurementData(MM_Analog_Measurement[] AnalogMeasurementData)
        {
            ProcessData<MM_Analog_Measurement>(AnalogMeasurementData);
        }

        /// <summary>
        /// Update our shunt compensator data
        /// </summary>
        /// <param name="ShuntCompensatorData"></param>
        public void UpdateShuntCompensatorData(MM_ShuntCompensator_Data[] ShuntCompensatorData)
        {
            ProcessData<MM_ShuntCompensator_Data>(ShuntCompensatorData);
        }

        /// <summary>
        /// Update our state measurement data
        /// </summary>
        /// <param name="StateMeasurementData"></param>
        public void UpdateStateMeasurementData(MM_State_Measurement[] StateMeasurementData)
        {
            ProcessData<MM_State_Measurement>(StateMeasurementData);
        }

        /// <summary>
        /// Update our transformer data
        /// </summary>
        /// <param name="TransformerData"></param>
        public void UpdateTransformerData(MM_Transformer_Data[] TransformerData)
        {
            ProcessData<MM_Transformer_Data>(TransformerData);
        }

        /// <summary>
        /// Update our phase shifter XF data
        /// </summary>
        /// <param name="PhaseShiftData"></param>
        public void UpdateTransformerPhaseShifterData(MM_Transformer_PhaseShifter_Data[] PhaseShiftData)
        {
            ProcessData<MM_Transformer_PhaseShifter_Data>(PhaseShiftData);
        }

        /// <summary>
        /// Update our unit data
        /// </summary>
        /// <param name="UnitData"></param>
        public void UpdateUnitData(MM_Unit_Data[] UnitData)
        {
            ProcessData<MM_Unit_Data>(UnitData);
        }

        /// <summary>
        /// Update our ZBR data
        /// </summary>
        /// <param name="ZBRData"></param>
        public void UpdateZBRData(MM_ZeroImpedanceBridge_Data[] ZBRData)
        {
            ProcessData<MM_ZeroImpedanceBridge_Data>(ZBRData);
        }


        /// <summary>
        /// Update switch/breaker data
        /// </summary>
        /// <param name="BreakerSwitchData"></param>
        public void UpdateBreakerSwitchData(MM_BreakerSwitch_Data[] BreakerSwitchData)
        {
            ProcessData<MM_BreakerSwitch_Data>(BreakerSwitchData);
        }

        /// <summary>
        /// Update SCADA analog data
        /// </summary>
        /// <param name="AnalogData"></param>
        public void UpdateSCADAAnalogData(MM_Scada_Analog[] AnalogData)
        {
            ProcessData<MM_Scada_Analog>(AnalogData);
        }

        /// <summary>
        /// Update SCADA state data
        /// </summary>
        /// <param name="StatusData"></param>
        public void UpdateSCADAStatusData(MM_Scada_Status[] StatusData)
        {
            ProcessData<MM_Scada_Status>(StatusData);
        }

        /// <summary>
        /// Update a chart point
        /// </summary>
        /// <param name="ChartData"></param>
        public void UpdateChartData(MM_Chart_Data[] ChartData)
        {
            ProcessData<MM_Chart_Data>(ChartData);
        }

        /// <summary>
        /// Update DC tie data
        /// </summary>
        /// <param name="TieData"></param>
        public void UpdateDCTieData(MM_DCTie_Data[] TieData)
        {
            ProcessData<MM_DCTie_Data>(TieData);
        }

        /// <summary>
        /// Update Static Var Compensator data
        /// </summary>
        /// <param name="SVCData"></param>
        public void UpdateStaticVarCompensatorData(MM_StaticVarCompensator_Data[] SVCData)
        {
            ProcessData<MM_StaticVarCompensator_Data>(SVCData);
        }

        /// <summary>
        /// Update system-wide generation data
        /// </summary>
        /// <param name="SystemWideData"></param>
        public void UpdateSystemWideGenerationData(MM_SystemWide_Generation_Data[] SystemWideData)
        {
            ProcessData<MM_SystemWide_Generation_Data>(SystemWideData);
        }

        /// <summary>
        /// Update our unit-type generation data
        /// </summary>
        /// <param name="UnitTypeData"></param>
        public void UpdateUnitTypeGenerationData(MM_UnitType_Generation_Data[] UnitTypeData)
        {
            ProcessData<MM_UnitType_Generation_Data>(UnitTypeData);
        }

        /// <summary>
        /// Update our unit generation data
        /// </summary>
        /// <param name="UnitGenData"></param>
        public void UpdateUnitGenData(MM_Unit_Gen_Data[] UnitGenData)
        {
            ProcessData<MM_Unit_Gen_Data>(UnitGenData);
        }

        /// <summary>
        /// Update our basecase violation data
        /// </summary>
        /// <param name="BasecaseData"></param>
        public void UpdateBasecaseViolationData(MM_Basecase_Violation_Data[] BasecaseData)
        {
            ProcessData<MM_Basecase_Violation_Data>(BasecaseData);
        }

        /// <summary>
        /// Update our operatorship data
        /// </summary>
        /// <param name="OperatorshipUpdateData"></param>
        public void UpdateOperatorshipData(MM_Operatorship_Update[] OperatorshipUpdateData)
        {
            ProcessData<MM_Operatorship_Update>(OperatorshipUpdateData);
        }

        /// <summary>
        /// Update the contingency violation data
        /// </summary>
        /// <param name="ContingencyData"></param>
        public void UpdateContingencyViolationData(MM_Contingency_Violation_Data[] ContingencyData)
        {
            ProcessData<MM_Contingency_Violation_Data>(ContingencyData);
        }

        /// <summary>
        /// Handle a bus disappearing
        /// </summary>
        /// <param name="BusData"></param>
        public void HandleBusRemoval(MM_Bus_Data[] BusData)
        {
            ProcessDataRemoval<MM_Bus_Data>(BusData);
        }

        /// <summary>
        /// Handle an island disappearing
        /// </summary>
        /// <param name="IslandData"></param>
        public void HandleIslandRemoval(MM_Island_Data[] IslandData)
        {
            ProcessDataRemoval<MM_Island_Data>(IslandData);
        }

        /// <summary>
        /// Remove a basecase violation
        /// </summary>
        /// <param name="BasecaseData"></param>
        public void RemoveBasecaseViolationData(MM_Basecase_Violation_Data[] BasecaseData)
        {
            ProcessDataRemoval<MM_Basecase_Violation_Data>(BasecaseData);
        }

        /// <summary>
        /// Remove a contingency violation
        /// </summary>
        /// <param name="ContingencyData"></param>
        public void RemoveContingencyViolationData(MM_Contingency_Violation_Data[] ContingencyData)
        {
            ProcessDataRemoval<MM_Contingency_Violation_Data>(ContingencyData);

        }

        /// <summary>
        /// Update an interface data
        /// </summary>
        /// <param name="InterfaceData"></param>
        public void UpdateInterfaceData(MM_Interface_Monitoring_Data[] InterfaceData)
        {
            ProcessData<MM_Interface_Monitoring_Data>(InterfaceData);
        }

        /// <summary>
        /// Remove a line outage data point
        /// </summary>
        /// <param name="LineOutageData"></param>
        public void RemoveLineOutageData(MM_Outage_Line_Data[] LineOutageData)
        {
            ProcessDataRemoval<MM_Outage_Line_Data>(LineOutageData);
        }

        /// <summary>
        /// Remove a unit outage point
        /// </summary>
        /// <param name="UnitOutageData"></param>
        public void RemoveUnitOutageData(MM_Outage_Unit_Data[] UnitOutageData)
        {
            ProcessDataRemoval<MM_Outage_Unit_Data>(UnitOutageData);
        }

        /// <summary>
        /// Remove a transformer outage point
        /// </summary>
        /// <param name="TransformerOutageData"></param>
        public void RemoveTransformerOutageData(MM_Outage_Transformer_Data[] TransformerOutageData)
        {
            ProcessDataRemoval<MM_Outage_Transformer_Data>(TransformerOutageData);
        }

        /// <summary>
        /// Handle an island going away
        /// </summary>
        /// <param name="IslandData"></param>
        public void HandleIslandSimulationRemoval(MM_Island_Simulation_Data[] IslandData)
        {
            ProcessDataRemoval<MM_Island_Simulation_Data>(IslandData);
        }

        /// <summary>
        /// Update a unit simulation data
        /// </summary>
        /// <param name="UnitSimulationData"></param>
        public void UpdateUnitSimulationData(MM_Unit_Simulation_Data[] UnitSimulationData)
        {
            ProcessData<MM_Unit_Simulation_Data>(UnitSimulationData);
        }

        /// <summary>
        /// Update an island simulation data
        /// </summary>
        /// <param name="IslandSimulationData"></param>
        public void UpdateIslandSimulationData(MM_Island_Simulation_Data[] IslandSimulationData)
        {
            ProcessData<MM_Island_Simulation_Data>(IslandSimulationData);
        }

        /// <summary>
        /// Update a synchroscope data
        /// </summary>
        /// <param name="SynchroscopeData"></param>
        public void UpdateSynchroscopeData(MM_Synchroscope_Data[] SynchroscopeData)
        {
            ProcessData<MM_Synchroscope_Data>(SynchroscopeData);
        }

        /// <summary>
        /// Update the simulator time
        /// </summary>
        /// <param name="TimeData"></param>
        public void UpdateSimulatorTime(MM_Simulation_Time[] TimeData)
        {
            ProcessData<MM_Simulation_Time>(TimeData);
        }
        #endregion


       
    }
}
