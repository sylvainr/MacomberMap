using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapCommunications.Messages.EMS;
using MacomberMapCommunications.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacomberMapCommunications.Messages;

namespace MacomberMapClient.Integration
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

        private delegate void SafeReceiveMessage(string Info, string MessageSource, MessageBoxIcon Icon);


        /// <summary>
        /// Recieve a message from the server
        /// </summary>
        /// <param name="Info"></param>
        /// <param name="MessageSource"></param>
        /// <param name="Icon"></param>        
        public void ReceiveMessage(string Info, string MessageSource, MessageBoxIcon Icon)
        {
            if (Program.MM != null && Program.MM.InvokeRequired)
                Program.MM.Invoke(new SafeReceiveMessage(ReceiveMessage), Info, MessageSource, Icon);
            else 
                MessageBox.Show(new Form() { TopMost = true }, Info, "Message received from " + MessageSource, MessageBoxButtons.OK, Icon);
        }

        /// <summary>
        /// Update our line data
        /// </summary>
        /// <param name="LineData"></param>
        public void UpdateLineData(MM_Line_Data[] LineData)
        {
            MM_Element FoundElem;
            foreach (MM_Line_Data InLine in LineData)
                if (MM_Repository.TEIDs.TryGetValue(InLine.TEID_Ln, out FoundElem) && FoundElem is MM_Line)
                    MM_Server_Interface.UpdateLineData(InLine, FoundElem as MM_Line, true);
            if (LineData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_Line_Data));

        }

        /// <summary>
        /// Update our operatorship data
        /// </summary>
        /// <param name="OperatorshipUpdateData"></param>
        public void UpdateOperatorshipData(MM_Operatorship_Update[] OperatorshipUpdateData)
        {
            foreach (MM_Operatorship_Update OperatorshipUpdate in OperatorshipUpdateData)
                MM_Server_Interface.UpdateOperatorshipData(OperatorshipUpdate);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Operatorship_Update));

        }


        /// <summary>
        /// Immediately shut down our client
        /// </summary>
        public void CloseClient()
        {
            Application.Exit();
        }


        /// <summary>
        /// Update our bus data
        /// </summary>
        /// <param name="BusData"></param>
        public void UpdateBusData(MM_Bus_Data[] BusData)
        {
            MM_Bus FoundBus;
            MM_Element FoundElem;
            foreach (MM_Bus_Data InBus in BusData)
                if (InBus != null)
                {
                    if (!MM_Repository.BusNumbers.TryGetValue(InBus.Bus_Num, out FoundBus))
                        MM_Repository.BusNumbers.Add(InBus.Bus_Num, FoundBus = new MM_Bus(InBus.Bus_Num));

                    MM_Repository.TEIDs.TryGetValue(InBus.TEID_Nd, out FoundElem);
                    MM_Server_Interface.UpdateBusData(InBus, FoundBus, FoundElem, true);
                }
            if (BusData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_Bus_Data));
        }



        /// <summary>
        /// Update our load data
        /// </summary>
        /// <param name="LoadData"></param>
        public void UpdateLoadData(MM_Load_Data[] LoadData)
        {
            MM_Element FoundElem;
            foreach (MM_Load_Data InLoad in LoadData)
                if (MM_Repository.TEIDs.TryGetValue(InLoad.TEID_Ld, out FoundElem) && FoundElem is MM_Load)
                    MM_Server_Interface.UpdateLoadData(InLoad, FoundElem as MM_Load, true);
            if (LoadData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_Load_Data));
        }


        /// <summary>
        /// Update our island data
        /// </summary>
        /// <param name="IslandData"></param>
        public void UpdateIslandData(MM_Island_Data[] IslandData)
        {
            //Create a new island if we need it
            foreach (MM_Island_Data InIsland in IslandData)
                MM_Server_Interface.UpdateIslandData(InIsland,true);
            if (IslandData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_Island_Data));
        }


        /// <summary>
        /// Update our analog measurement data
        /// </summary>
        /// <param name="AnalogMeasurementData"></param>
        public void UpdateAnalogMeasurementData(MM_Analog_Measurement[] AnalogMeasurementData)
        {
            MM_Element FoundElem;
            foreach (MM_Analog_Measurement Meas in AnalogMeasurementData)
                if (MM_Repository.TEIDs.TryGetValue(Meas.TEID, out FoundElem))
                { }
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Analog_Measurement));

        }

        /// <summary>
        /// Update our shunt compensator data
        /// </summary>
        /// <param name="ShuntCompensatorData"></param>
        public void UpdateShuntCompensatorData(MM_ShuntCompensator_Data[] ShuntCompensatorData)
        {
            MM_Element FoundElem;
            foreach (MM_ShuntCompensator_Data InSc in ShuntCompensatorData)
                if (MM_Repository.TEIDs.TryGetValue(InSc.TEID_CP, out FoundElem) && FoundElem is MM_ShuntCompensator)
                    MM_Server_Interface.UpdateShuntCompensatorData(InSc, FoundElem as MM_ShuntCompensator, true);
            if (ShuntCompensatorData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_ShuntCompensator_Data));
        }

        /// <summary>
        /// Update our state measurement data
        /// </summary>
        /// <param name="StateMeasurementData"></param>
        public void UpdateStateMeasurementData(MM_State_Measurement[] StateMeasurementData)
        {
            MM_Element FoundElem;
            foreach (MM_State_Measurement InState in StateMeasurementData)
                if (MM_Repository.TEIDs.TryGetValue(InState.TEID_Stat, out FoundElem) && FoundElem is MM_Breaker_Switch)
                    MM_Server_Interface.UpdateStateData(InState, FoundElem as MM_Breaker_Switch, true);
            if (StateMeasurementData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_State_Measurement));

        }

        /// <summary>
        /// Update our transformer data
        /// </summary>
        /// <param name="TransformerData"></param>
        public void UpdateTransformerData(MM_Transformer_Data[] TransformerData)
        {
            MM_Element FoundElem;
            foreach (MM_Transformer_Data InXf in TransformerData)
                if (MM_Repository.TEIDs.TryGetValue(InXf.TEID_Xf, out FoundElem) && FoundElem is MM_TransformerWinding)
                    MM_Server_Interface.UpdateTransformerData(InXf, FoundElem as MM_TransformerWinding, true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Transformer_Data));
        }

        /// <summary>
        /// Update our transformer PS data
        /// </summary>
        /// <param name="PhaseShiftData"></param>
        public void UpdateTransformerPhaseShifterData(MM_Transformer_PhaseShifter_Data[] PhaseShiftData)
        {
            MM_Element FoundElem;
            foreach (MM_Transformer_PhaseShifter_Data InPS in PhaseShiftData)
                if (MM_Repository.TEIDs.TryGetValue(InPS.TEID_XF, out FoundElem) && FoundElem is MM_TransformerWinding)
                    MM_Server_Interface.UpdateTransformerPhaseShifterData(InPS, FoundElem as MM_TransformerWinding, true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Transformer_PhaseShifter_Data));
        }


        /// <summary>
        /// Update our unit data
        /// </summary>
        /// <param name="UnitData"></param>
        public void UpdateUnitData(MM_Unit_Data[] UnitData)
        {
            MM_Element FoundElem;
            foreach (MM_Unit_Data InUnit in UnitData)
                if (MM_Repository.TEIDs.TryGetValue(InUnit.TEID_Un, out FoundElem) && FoundElem is MM_Unit)
                    MM_Server_Interface.UpdateUnitData(InUnit, FoundElem as MM_Unit, true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Unit_Data));

        }

        /// <summary>
        /// Report an updated unit control status
        /// </summary>
        /// <param name="UnitStatus"></param>
        public void UpdateUnitControlStatus(MM_Unit_Control_Status[] UnitStatus)
        {
            MM_Element FoundElem;
            foreach (MM_Unit_Control_Status InUnit in UnitStatus)
                if (MM_Repository.TEIDs.TryGetValue(InUnit.TEID, out FoundElem) && FoundElem is MM_Unit)
                    MM_Server_Interface.UpdateUnitControlStatus(InUnit, FoundElem as MM_Unit, true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Unit_Control_Status));
        }

        /// <summary>
        /// Update our ZBR data
        /// </summary>
        /// <param name="ZBRData"></param>
        public void UpdateZBRData(MM_ZeroImpedanceBridge_Data[] ZBRData)
        {
            MM_Element FoundElem;
            foreach (MM_ZeroImpedanceBridge_Data InLine in ZBRData)
                if (MM_Repository.TEIDs.TryGetValue(InLine.TEID_ZBR, out FoundElem) && FoundElem is MM_Line)
                    MM_Server_Interface.UpdateZBRData(InLine, (MM_Line)FoundElem, true);
            if (ZBRData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_ZeroImpedanceBridge_Data));
        }

        /// <summary>
        /// Update data on a breaker or switch
        /// </summary>
        /// <param name="BreakerSwitchData"></param>
        public void UpdateBreakerSwitchData(MM_BreakerSwitch_Data[] BreakerSwitchData)
        {
            MM_Element FoundElem;
            foreach (MM_BreakerSwitch_Data InBreakerSwitch in BreakerSwitchData)
                if (MM_Repository.TEIDs.TryGetValue(InBreakerSwitch.TEID_CB, out FoundElem) && FoundElem is MM_Breaker_Switch)
                    MM_Server_Interface.UpdateBreakerSwitchData(InBreakerSwitch, (MM_Breaker_Switch)FoundElem, true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_BreakerSwitch_Data));
        
        }

        /// <summary>
        /// Update our chart information
        /// </summary>
        /// <param name="ChartData"></param>
        public void UpdateChartData(MM_Chart_Data[] ChartData)
        {
            Data_Integration.ChartData = ChartData;
            Data_Integration.ChartDate = DateTime.Now;
            if (ChartData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_Chart_Data));

        }

        /// <summary>
        /// Update our DC Tie data
        /// </summary>
        /// <param name="TieData"></param>
        public void UpdateTieData(MM_Tie_Data[] TieData)
        {
            MM_Tie FoundTie;
            MM_Element el = null;

            foreach (MM_Tie_Data TieDatum in TieData)
            {
                
                if (MM_Repository.Ties.TryGetValue(TieDatum.ID_TIE, out FoundTie))
                    MM_Server_Interface.UpdateTieData(TieDatum, FoundTie, true);
                else if (MM_Repository.TEIDs.TryGetValue(TieDatum.TEID_TIE, out el))
                    MM_Server_Interface.UpdateTieData(TieDatum, el as MM_Tie, true);
            }
            if (TieData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_Tie_Data));

        }

        /// <summary>
        /// Update Static Var Compensator data
        /// </summary>
        /// <param name="SVCData"></param>
        public void UpdateStaticVarCompensatorData(MM_StaticVarCompensator_Data[] SVCData)
        {
            MM_Element FoundElem;
            foreach (MM_StaticVarCompensator_Data InSVC in SVCData)
                if (MM_Repository.TEIDs.TryGetValue(InSVC.TEID_SVS, out FoundElem) && FoundElem is MM_StaticVarCompensator)
                    MM_Server_Interface.UpdateSVCData(InSVC, (MM_StaticVarCompensator)FoundElem, true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_StaticVarCompensator_Data));

        }

        /// <summary>
        /// Update system wide generation data
        /// </summary>
        /// <param name="SystemWideData"></param>
        public void UpdateSystemWideGenerationData(MM_SystemWide_Generation_Data[] SystemWideData)
        {
            foreach (MM_SystemWide_Generation_Data SystemWideDatum in SystemWideData)
                MM_Server_Interface.UpdateSystemWideData(SystemWideDatum);
            if (SystemWideData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_SystemWide_Generation_Data));

        }

        /// <summary>
        /// Update our unit type data
        /// </summary>
        /// <param name="UnitTypeData"></param>
        public void UpdateUnitTypeGenerationData(MM_UnitType_Generation_Data[] UnitTypeData)
        {
            foreach (MM_UnitType_Generation_Data UnitTypeDatum in UnitTypeData)
                MM_Server_Interface.UpdateUnitTypeData(UnitTypeDatum);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_UnitType_Generation_Data));
        }


        /// <summary>
        /// Update our unit gen data
        /// </summary>
        /// <param name="UnitGenData"></param>
        public void UpdateUnitGenData(MM_Unit_Gen_Data[] UnitGenData)
        {
            MM_Element FoundElem;
            foreach (MM_Unit_Gen_Data Unit in UnitGenData)
                if ((MM_Repository.TEIDs.TryGetValue(Unit.TEID_UNIT, out FoundElem) && FoundElem is MM_Unit))
                    MM_Server_Interface.UpdateUnitGenData(Unit, (MM_Unit)FoundElem, true);
            if (UnitGenData.Length > 0)
                MM_Server_Interface.UpdateTimestamp(typeof(MM_Unit_Gen_Data));
        }


        /// <summary>
        /// Handle our updated interface data
        /// </summary>
        /// <param name="InterfaceData"></param>
        public void UpdateInterfaceData(MM_Interface_Monitoring_Data[] InterfaceData)
        {
            foreach (MM_Interface_Monitoring_Data Interface in InterfaceData)
                MM_Server_Interface.UpdateInterfaceData(Interface, true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Interface_Monitoring_Data));

        }

        /// <summary>
        /// When a bus disappears, set it to N/A
        /// </summary>
        /// <param name="BusData"></param>
        public void HandleBusRemoval(MM_Bus_Data[] BusData)
        {
            MM_Bus FoundBus;
            foreach (MM_Bus_Data Bus in BusData)
                if (MM_Repository.BusNumbers.TryGetValue(Bus.Bus_Num, out FoundBus))
                {
                    FoundBus.Open = FoundBus.Dead = true;
                    FoundBus.Estimated_Angle = FoundBus.Estimated_kV = float.NaN;
                    FoundBus.IslandNumber = -1;
                    FoundBus.BusNumber = -1;
                    MM_Repository.BusNumbers.Remove(Bus.Bus_Num);
                }
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Bus_Data));

        }

        /// <summary>
        /// Update flowgate data
        /// </summary>
        /// <param name="flowgateData"></param>
        public void UpdateFlowgateData(MM_Flowgate_Data[] flowgateData)
        {
            MM_Server_Interface.UpdateFlowgateData(flowgateData);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Flowgate_Data));
        }

        /// <summary>
        /// Handle an island simulation removal
        /// </summary>
        /// <param name="IslandData"></param>
        public void HandleIslandSimulationRemoval(MM_Island_Simulation_Data[] IslandData)
        {
            MM_Island FoundIsland;
            foreach (MM_Island_Simulation_Data Island in IslandData)
            {
                if (MM_Repository.Islands.TryGetValue(Island.Isl_Num, out FoundIsland))
                {
                    FoundIsland.Frequency = FoundIsland.Generation = FoundIsland.Load  = float.NaN;                    
                    MM_Repository.Islands.Remove(Island.Isl_Num);
                    Data_Integration.HandleIslandRemoval(FoundIsland);
                }
            }
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Island_Simulation_Data));
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.IslandCount] = MM_Repository.Islands.Count;
        }

        /// <summary>
        /// Handle an island being removed
        /// </summary>
        /// <param name="IslandData"></param>
        public void HandleIslandRemoval(MM_Island_Data[] IslandData)
        {
            MM_Island FoundIsland;
            foreach (MM_Island_Data Island in IslandData)
            {
                if (MM_Repository.Islands.TryGetValue(Island.Isl_Num, out FoundIsland))
                {
                    FoundIsland.Frequency = FoundIsland.Generation = FoundIsland.Load  = float.NaN;
                    MM_Repository.Islands.Remove(Island.Isl_Num);                    
                }
            }
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Island_Data));
            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.IslandCount] = MM_Repository.Islands.Count;
        }

       
        /// <summary>
        /// Update our basecase violation data
        /// </summary>
        /// <param name="BasecaseData"></param>
        public void UpdateBasecaseViolationData(MM_Basecase_Violation_Data[] BasecaseData)
        {
            foreach (MM_Basecase_Violation_Data Basecase in BasecaseData)
                Data_Integration.UpdateViolation(MM_Server_Interface.CreateViolation(Basecase));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Basecase_Violation_Data));

        }

        /// <summary>
        /// Update our contingency violation data
        /// </summary>
        /// <param name="ContingencyData"></param>
        public void UpdateContingencyViolationData(MM_Contingency_Violation_Data[] ContingencyData)
        {
            foreach (MM_Contingency_Violation_Data Contingency in ContingencyData)
                Data_Integration.UpdateViolation(MM_Server_Interface.CreateViolation(Contingency));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Contingency_Violation_Data));
        }


        /// <summary>
        /// Remove a basecase violation
        /// </summary>
        /// <param name="BasecaseData"></param>
        public void RemoveBasecaseViolationData(MM_Basecase_Violation_Data[] BasecaseData)
        {
            foreach (MM_Basecase_Violation_Data Basecase in BasecaseData)
                Data_Integration.RemoveViolation(MM_Server_Interface.CreateViolation(Basecase));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Basecase_Violation_Data));
        }

        /// <summary>
        /// Remove a contingency violation
        /// </summary>
        /// <param name="ContingencyData"></param>
        public void RemoveContingencyViolationData(MM_Contingency_Violation_Data[] ContingencyData)
        {
            foreach (MM_Contingency_Violation_Data Contingency in ContingencyData)
                Data_Integration.RemoveViolation(MM_Server_Interface.CreateViolation(Contingency));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Contingency_Violation_Data));
        }

        /// <summary>
        /// Remove a line outage
        /// </summary>
        /// <param name="LineOutageData"></param>
        public void RemoveLineOutageData(MM_Outage_Line_Data[] LineOutageData)
        {
            foreach (MM_Outage_Line_Data LineOutage in LineOutageData)
                Data_Integration.RemoveViolation(MM_Server_Interface.CreateViolation(LineOutage));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Outage_Line_Data));
        }

        /// <summary>
        /// Remove a unit outage
        /// </summary>
        /// <param name="UnitOutageData"></param>
        public void RemoveUnitOutageData(MM_Outage_Unit_Data[] UnitOutageData)
        {
            foreach (MM_Outage_Unit_Data UnitOutage in UnitOutageData)
                Data_Integration.RemoveViolation(MM_Server_Interface.CreateViolation(UnitOutage));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Outage_Unit_Data));
        }

        /// <summary>
        /// Remove a transformer outage
        /// </summary>
        /// <param name="TransformerOutageData"></param>
        public void RemoveTransformerOutageData(MM_Outage_Transformer_Data[] TransformerOutageData)
        {
            foreach (MM_Outage_Transformer_Data TransformerOutage in TransformerOutageData)
                Data_Integration.RemoveViolation(MM_Server_Interface.CreateViolation(TransformerOutage));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Outage_Transformer_Data));
        }

        /// <summary>
        /// Update our unit outage data
        /// </summary>
        /// <param name="UnitOutageData"></param>
        public void UpdateUnitOutageData(MM_Outage_Unit_Data[] UnitOutageData)
        {
            foreach (MM_Outage_Unit_Data UnitData in UnitOutageData)
                Data_Integration.UpdateViolation(MM_Server_Interface.CreateViolation(UnitData));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Outage_Unit_Data));
        }

        /// <summary>
        /// Update our transformer outage data
        /// </summary>
        /// <param name="TransformerOutageData"></param>
        public void UpdateTransformerOutageData(MM_Outage_Transformer_Data[] TransformerOutageData)
        {
            foreach (MM_Outage_Transformer_Data TransformerData in TransformerOutageData)
                Data_Integration.UpdateViolation(MM_Server_Interface.CreateViolation(TransformerData));
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Outage_Transformer_Data));
        }

        /// <summary>
        /// Update our line outage data
        /// </summary>
        /// <param name="LineOutageData"></param>
        public void UpdateLineOutageData(MM_Outage_Line_Data[] LineOutageData)
        {
            foreach (MM_Outage_Line_Data LineData in LineOutageData)
                Data_Integration.UpdateViolation(MM_Server_Interface.CreateViolation(LineData));

            MM_Server_Interface.CheckForOutagedLines();

            MM_Server_Interface.UpdateTimestamp(typeof(MM_Outage_Line_Data));
        }



        /// <summary>
        /// Update our SCADA analog data
        /// </summary>
        /// <param name="AnalogData"></param>
        public void UpdateSCADAAnalogData(MM_Scada_Analog[] AnalogData)
        {
            MM_Server_Interface.UpdateSCADAAnalogData(AnalogData);
        }

        /// <summary>
        /// Update our SCADA status data
        /// </summary>
        /// <param name="StatusData"></param>
        public void UpdateSCADAStatusData(MM_Scada_Status[] StatusData)
        {
            MM_Server_Interface.UpdateSCADAStatusData(StatusData);
        }

        /// <summary>
        /// Update our load forecast data
        /// </summary>
        /// <param name="LoadForecastData"></param>
        public void UpdateLoadForecastData(MM_Load_Forecast_Data[] LoadForecastData)
        {
            MM_Server_Interface.UpdateLoadForecastData(LoadForecastData);
        }


        /// <summary>
        /// Update our unit simulation data
        /// </summary>
        /// <param name="UnitSimulationData"></param>
        public void UpdateUnitSimulationData(MM_Unit_Simulation_Data[] UnitSimulationData)
        {
            MM_Element FoundElem;
            foreach (MM_Unit_Simulation_Data InUnit in UnitSimulationData)
                if (MM_Repository.TEIDs.TryGetValue(InUnit.TEID_Un, out FoundElem) && FoundElem is MM_Unit)
                    MM_Server_Interface.UpdateUnitSimulationData(InUnit, FoundElem as MM_Unit, true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Unit_Simulation_Data));
        }
        /// <summary>
        /// update lmp data in mm
        /// </summary>
        /// <param name="lmpData"></param>
        public void UpdateLmpData(MM_LMP_Data[] lmpData)
        {
            foreach (var data in lmpData)
            {
                
                try
                {
                    MM_Server_Interface.UpdateLmpData(data, true);
                    
                } catch (Exception)
                {
                   
                }
            }
            MM_Server_Interface.UpdateTimestamp(typeof(MM_LMP_Data));
        }

        /// <summary>
        /// Update our island simulation data
        /// </summary>
        /// <param name="IslandSimulationData"></param>
        public void UpdateIslandSimulationData(MM_Island_Simulation_Data[] IslandSimulationData)
        {
            //Create a new island if we need it
            foreach (MM_Island_Simulation_Data InIsland in IslandSimulationData)
                MM_Server_Interface.UpdateIslandSimulationData(InIsland,true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Island_Simulation_Data));
        }

        /// <summary>
        /// Update our synchroscope data
        /// </summary>
        /// <param name="SynchroscopeData"></param>
        public void UpdateSynchroscopeData(MM_Synchroscope_Data[] SynchroscopeData)
        {

            MM_Element FoundElem;
            foreach (MM_Synchroscope_Data InBreakerSwitch in SynchroscopeData)
                if (MM_Repository.TEIDs.TryGetValue(InBreakerSwitch.TEID_CB, out FoundElem) && FoundElem is MM_Breaker_Switch)
                    MM_Server_Interface.UpdateSynchroscopeData(InBreakerSwitch, (MM_Breaker_Switch)FoundElem, true);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Synchroscope_Data));
        }

        /// <summary>
        /// Update our simulator time
        /// </summary>
        /// <param name="TimeData"></param>
        public void UpdateSimulatorTime(MM_Simulation_Time[] TimeData)
        {
            foreach (MM_Simulation_Time Time in TimeData)
                MM_Server_Interface.UpdateSimulatorTime(Time);
            MM_Server_Interface.UpdateTimestamp(typeof(MM_Simulation_Time));
        }


        /// <summary>
        /// Add an EMS command
        /// </summary>
        /// <param name="Command"></param>
        public void AddEMSCommands(MM_EMS_Command[] Command)
        {
            Data_Integration.HandleEMSCommands(Command);
        }
    }
}