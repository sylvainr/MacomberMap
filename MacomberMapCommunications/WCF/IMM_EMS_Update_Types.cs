using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.EMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.WCF
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the interface and class to our EMS types for updating between EMS service and MM server
    /// </summary>
    [ServiceContract]
    public interface IMM_EMS_Update_Types
    {
        /// <summary>
        /// Update our line data
        /// </summary>
        /// <param name="LineData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateLineData(MM_Line_Data[] LineData);

        /// <summary>
        /// Update our interface data
        /// </summary>
        /// <param name="InterfaceData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateInterfaceData(MM_Interface_Monitoring_Data[] InterfaceData);

        /// <summary>
        /// Update our island data
        /// </summary>
        /// <param name="IslandData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateIslandData(MM_Island_Data[] IslandData);

        /// <summary>
        /// Update our load data
        /// </summary>
        /// <param name="LoadData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateLoadData(MM_Load_Data[] LoadData);

        /// <summary>
        /// Update our load data
        /// </summary>
        /// <param name="LoadData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateLoadForecastData(MM_Load_Forecast_Data[] LoadForecastData);

        /// <summary>
        /// Update our load data
        /// </summary>
        /// <param name="BusData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateBusData(MM_Bus_Data[] BusData);

        /// <summary>
        /// Handle the removal of a bus
        /// </summary>
        /// <param name="BusData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void HandleBusRemoval(MM_Bus_Data[] BusData);

        /// <summary>
        /// Handle the removal of an island
        /// </summary>
        /// <param name="IslandData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void HandleIslandRemoval(MM_Island_Data[] IslandData);

        /// <summary>
        /// Update our line outage data
        /// </summary>
        /// <param name="LineOutageData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateLineOutageData(MM_Outage_Line_Data[] LineOutageData);

        /// <summary>
        /// Remove our line outage data
        /// </summary>
        /// <param name="LineOutageData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void RemoveLineOutageData(MM_Outage_Line_Data[] LineOutageData);

        /// <summary>
        /// Update our unit outage data
        /// </summary>
        /// <param name="UnitOutageData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateUnitOutageData(MM_Outage_Unit_Data[] UnitOutageData);

        /// <summary>
        /// Remove our unit outage data
        /// </summary>
        /// <param name="UnitOutageData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void RemoveUnitOutageData(MM_Outage_Unit_Data[] UnitOutageData);

        /// <summary>
        /// Update our transformer outage data
        /// </summary>
        /// <param name="TransformerOutageData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateTransformerOutageData(MM_Outage_Transformer_Data[] TransformerOutageData);

        /// <summary>
        /// Remove our transformer outage data
        /// </summary>
        /// <param name="TransformerOutageData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void RemoveTransformerOutageData(MM_Outage_Transformer_Data[] TransformerOutageData);

        /// <summary>
        /// Update our analog measurement data
        /// </summary>
        /// <param name="AnalogMeasurementData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateAnalogMeasurementData(MM_Analog_Measurement[] AnalogMeasurementData);

        /// <summary>
        /// Update our shunt compensator data
        /// </summary>
        /// <param name="ShuntCompensatorData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateShuntCompensatorData(MM_ShuntCompensator_Data[] ShuntCompensatorData);

        /// <summary>
        /// Update our state measurement data
        /// </summary>
        /// <param name="StateMeasurementData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateStateMeasurementData(MM_State_Measurement[] StateMeasurementData);

        /// <summary>
        /// Update our transformer data
        /// </summary>
        /// <param name="TransformerData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateTransformerData(MM_Transformer_Data[] TransformerData);

        /// <summary>
        /// Update our unit data
        /// </summary>
        /// <param name="UnitData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateUnitData(MM_Unit_Data[] UnitData);

        /// <summary>
        /// Update our ZBR data
        /// </summary>
        /// <param name="ZBRData"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        void UpdateZBRData(MM_ZeroImpedanceBridge_Data[] ZBRData);

        /// <summary>
        /// Update our breaker/switch data
        /// </summary>
        /// <param name="BreakerSwitchData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateBreakerSwitchData(MM_BreakerSwitch_Data[] BreakerSwitchData);

        /// <summary>
        /// Update our SCADA analog data
        /// </summary>
        /// <param name="AnalogData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateSCADAAnalogData(MM_Scada_Analog[] AnalogData);

        /// <summary>
        /// Update our SCADA analog data
        /// </summary>
        /// <param name="StatusData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateSCADAStatusData(MM_Scada_Status[] StatusData);

        /// <summary>
        /// Update our chart data
        /// </summary>
        /// <param name="ChartData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateChartData(MM_Chart_Data[] ChartData);

        /// <summary>
        /// Update our DC Tie data
        /// </summary>
        /// <param name="TieData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateDCTieData(MM_DCTie_Data[] TieData);

        /// <summary>
        /// Update our SVC data
        /// </summary>
        /// <param name="SVCData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateStaticVarCompensatorData(MM_StaticVarCompensator_Data[] SVCData);

        /// <summary>
        /// Update our system-wide data
        /// </summary>
        /// <param name="SystemWideData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateSystemWideGenerationData(MM_SystemWide_Generation_Data[] SystemWideData);

        /// <summary>
        /// Update our unit-type data
        /// </summary>
        /// <param name="UnitTypeData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateUnitTypeGenerationData(MM_UnitType_Generation_Data[] UnitTypeData);

        /// <summary>
        /// Update our operatorship data
        /// </summary>
        /// <param name="UnitTypeData"></param>
        [OperationContract(IsOneWay = true)]        
        void UpdateOperatorshipData(MM_Operatorship_Update[] OperatorshipUpdateData);

        /// <summary>
        /// Update our unit generation-specific data
        /// </summary>
        /// <param name="UnitGenData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateUnitGenData(MM_Unit_Gen_Data[] UnitGenData);

        /// <summary>
        /// Update our basecase violation data
        /// </summary>
        /// <param name="BasecaseData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateBasecaseViolationData(MM_Basecase_Violation_Data[] BasecaseData);

        /// <summary>
        /// Handle the removal of a basecase violation
        /// </summary>
        /// <param name="BasecaseData"></param>
        [OperationContract(IsOneWay = true)]
        void RemoveBasecaseViolationData(MM_Basecase_Violation_Data[] BasecaseData);

        /// <summary>
        /// Update our contingency violation data
        /// </summary>
        /// <param name="ContingencyData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateContingencyViolationData(MM_Contingency_Violation_Data[] ContingencyData);


        /// <summary>
        /// Handle the removal of contingency violation data
        /// </summary>
        /// <param name="ContingencyData"></param>
        [OperationContract(IsOneWay = true)]
        void RemoveContingencyViolationData(MM_Contingency_Violation_Data[] ContingencyData);

        /// <summary>
        /// Handle the removal of a simulated island
        /// </summary>
        /// <param name="IslandData"></param>
        [OperationContract(IsOneWay = true)]        
        void HandleIslandSimulationRemoval(MM_Island_Simulation_Data[] IslandData);


        /// <summary>
        /// Handle the updating of our unit simulation data
        /// </summary>
        /// <param name="UnitSimulationData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateUnitSimulationData(MM_Unit_Simulation_Data[] UnitSimulationData);


        /// <summary>
        /// Handle the updating of our island simulation data
        /// </summary>
        /// <param name="IslandSimulationData"></param>
        [OperationContract(IsOneWay = true)]        
        void UpdateIslandSimulationData(MM_Island_Simulation_Data[] IslandSimulationData);

        /// <summary>
        /// Update our synchroscope data
        /// </summary>
        /// <param name="SynchroscopeData"></param>
        [OperationContract(IsOneWay = true)]
        void UpdateSynchroscopeData(MM_Synchroscope_Data[] SynchroscopeData);

        /// <summary>
        /// Update our time data
        /// </summary>
        /// <param name="TimeData"></param>
        [OperationContract(IsOneWay = true)]        
        void UpdateSimulatorTime(MM_Simulation_Time[] TimeData);


    }
}