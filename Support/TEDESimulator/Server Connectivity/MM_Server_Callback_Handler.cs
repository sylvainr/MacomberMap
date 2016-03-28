using MacomberMapCommunications.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.EMS;
using System.Windows.Forms;
using System.Diagnostics;

namespace TEDESimulator.Server_Connectivity
{
    /// <summary>
    /// This class provides our server callback handler
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class MM_Server_Callback_Handler : IMM_ConversationMessage_Types
    {
        /// <summary>The context of our handler</summary>
        public InstanceContext Context;

        /// <summary>
        /// Initialize a new callback handler
        /// </summary>
        public MM_Server_Callback_Handler()
        {
            MM_Server_Interface.CallbackHandler = this;
            this.Context = new InstanceContext(this);
        }

        public void AddEMSCommands(MM_EMS_Command[] Command)
        {
           
        }

        public void CloseClient()
        {
            Process.GetCurrentProcess().Kill();
        }

        public void HandleBusRemoval(MM_Bus_Data[] BusData)
        {
           
        }

        public void HandleIslandRemoval(MM_Island_Data[] IslandData)
        {
           
        }

        public void HandleIslandSimulationRemoval(MM_Island_Simulation_Data[] IslandData)
        {
           
        }

        public void ReceiveMessage(string Info, string MessageSource, MessageBoxIcon Icon)
        {
           
        }

        public void RemoveBasecaseViolationData(MM_Basecase_Violation_Data[] BasecaseData)
        {
           
        }

        public void RemoveContingencyViolationData(MM_Contingency_Violation_Data[] ContingencyData)
        {
           
        }

        public void RemoveLineOutageData(MM_Outage_Line_Data[] LineOutageData)
        {
           
        }

        public void RemoveTransformerOutageData(MM_Outage_Transformer_Data[] TransformerOutageData)
        {
           
        }

        public void RemoveUnitOutageData(MM_Outage_Unit_Data[] UnitOutageData)
        {
           
        }

        public void UpdateAnalogMeasurementData(MM_Analog_Measurement[] AnalogMeasurementData)
        {
           
        }

        public void UpdateBasecaseViolationData(MM_Basecase_Violation_Data[] BasecaseData)
        {
           
        }

        public void UpdateBreakerSwitchData(MM_BreakerSwitch_Data[] BreakerSwitchData)
        {
           
        }

        public void UpdateBusData(MM_Bus_Data[] BusData)
        {
           
        }

        public void UpdateChartData(MM_Chart_Data[] ChartData)
        {
           
        }

        public void UpdateContingencyViolationData(MM_Contingency_Violation_Data[] ContingencyData)
        {
           
        }

        public void UpdateFlowgateData(MM_Flowgate_Data[] flowgateData)
        {
           
        }

        public void UpdateInterfaceData(MM_Interface_Monitoring_Data[] InterfaceData)
        {
           
        }

        public void UpdateIslandData(MM_Island_Data[] IslandData)
        {
           
        }

        public void UpdateIslandSimulationData(MM_Island_Simulation_Data[] IslandSimulationData)
        {
           
        }

        public void UpdateLineData(MM_Line_Data[] LineData)
        {
           
        }

        public void UpdateLineOutageData(MM_Outage_Line_Data[] LineOutageData)
        {
           
        }

        public void UpdateLmpData(MM_LMP_Data[] lmpData)
        {
           
        }

        public void UpdateLoadData(MM_Load_Data[] LoadData)
        {
           
        }

        public void UpdateLoadForecastData(MM_Load_Forecast_Data[] LoadData)
        {
           
        }

        public void UpdateOperatorshipData(MM_Operatorship_Update[] OperatorshipUpdateData)
        {
           
        }

        public void UpdateSCADAAnalogData(MM_Scada_Analog[] AnalogData)
        {
           
        }

        public void UpdateSCADAStatusData(MM_Scada_Status[] StatusData)
        {
           
        }

        public void UpdateShuntCompensatorData(MM_ShuntCompensator_Data[] ShuntCompensatorData)
        {
           
        }

        public void UpdateSimulatorTime(MM_Simulation_Time[] TimeData)
        {
           
        }

        public void UpdateStateMeasurementData(MM_State_Measurement[] StateMeasurementData)
        {
           
        }

        public void UpdateStaticVarCompensatorData(MM_StaticVarCompensator_Data[] SVCData)
        {
           
        }

        public void UpdateSynchroscopeData(MM_Synchroscope_Data[] SynchroscopeData)
        {
           
        }

        public void UpdateSystemWideGenerationData(MM_SystemWide_Generation_Data[] SystemWideData)
        {
           
        }

        public void UpdateTieData(MM_Tie_Data[] TieData)
        {
           
        }

        public void UpdateTransformerData(MM_Transformer_Data[] TransformerData)
        {
           
        }

        public void UpdateTransformerOutageData(MM_Outage_Transformer_Data[] TransformerOutageData)
        {
           
        }

        public void UpdateTransformerPhaseShifterData(MM_Transformer_PhaseShifter_Data[] PhaseShiftData)
        {
           
        }

        public void UpdateUnitControlStatus(MM_Unit_Control_Status[] UnitStatus)
        {
           
        }

        public void UpdateUnitData(MM_Unit_Data[] UnitData)
        {
           
        }

        public void UpdateUnitGenData(MM_Unit_Gen_Data[] UnitGenData)
        {
           
        }

        public void UpdateUnitOutageData(MM_Outage_Unit_Data[] UnitOutageData)
        {
           
        }

        public void UpdateUnitSimulationData(MM_Unit_Simulation_Data[] UnitSimulationData)
        {
           
        }

        public void UpdateUnitTypeGenerationData(MM_UnitType_Generation_Data[] UnitTypeData)
        {
           
        }

        public void UpdateZBRData(MM_ZeroImpedanceBridge_Data[] ZBRData)
        {
           
        }
    }
}