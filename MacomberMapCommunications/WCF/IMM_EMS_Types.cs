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
    /// This class provides the interface and class to our EMS types
    /// </summary>
    [ServiceContract]
    public interface IMM_EMS_Types
    {
        /// <summary>
        /// Retrieve our load data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Load_Data[] GetLoadData();

        /// <summary>
        /// Retrieve our load forecast data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Load_Forecast_Data[] GetLoadForecastData();

        /// <summary>
        /// Retrieve our operatorship updates
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Operatorship_Update[] GetOperatorshipUpdates();

        /// <summary>
        /// Retrieve our interface daa
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Interface_Monitoring_Data[] GetInterfaceData();

        /// <summary>
        /// Retrieve our island data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Island_Data[] GetIslandData();

        /// <summary>
        /// Retrieve our line data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Line_Data[] GetLineData();

        /// <summary>
        /// Retrieve our bus data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Bus_Data[] GetBusData();

        /// <summary>
        /// Retrieve our line outage data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Outage_Line_Data[] GetLineOutageData();


        /// <summary>
        /// Retrieve our unit outage data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Outage_Unit_Data[] GetUnitOutageData();



        /// <summary>
        /// Retrieve our transformer outage data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Outage_Transformer_Data[] GetTransformerOutageData();

        /// <summary>
        /// Retrieve the latest timestamps on our objects
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Data_Collection[] CheckLatestTimestamps();

        /// <summary>
        /// Get our analog measurement data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Analog_Measurement[] GetAnalogMeasurementData();

        /// <summary>
        /// Get our shunt compensator data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_ShuntCompensator_Data[] GetShuntCompensatorData();

        /// <summary>
        /// Get our state measurement data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_State_Measurement[] GetStateMeasurementData();

        /// <summary>
        /// Get our transformer data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Transformer_Data[] GetTransformerData();

        /// <summary>
        /// Get our transformer PS data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Transformer_PhaseShifter_Data[] GetTransformerPhaseShifterData();

        /// <summary>
        /// Get our unit data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Unit_Data[] GetUnitData();

        /// <summary>
        /// Get our unit simulation data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Unit_Simulation_Data[] GetUnitSimulationData();

        /// <summary>
        /// Get our island simulation data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Island_Simulation_Data[] GetIslandSimulationData();

        /// <summary>
        /// Get our synchroscope simulation data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Synchroscope_Data[] GetSynchroscopeData();

        /// <summary>
        /// Get our simulation time
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Simulation_Time[] GetSimulatorTime();

        /// <summary>Get our ZBR data</summary>
        /// <returns></returns>
        [OperationContract]
        MM_ZeroImpedanceBridge_Data[] GetZBRData();

        /// <summary>Get our breaker/switch data</summary><returns></returns>
        [OperationContract]
        MM_BreakerSwitch_Data[] GetBreakerSwitchData();


        /// <summary>Get our chart data</summary><returns></returns>
        [OperationContract]
        MM_Chart_Data[] GetChartData();

        /// <summary>
        /// get lmp data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_LMP_Data[] GetLmpData();

        /// <summary>Get our DC Tie data</summary><returns></returns>
        [OperationContract]
        MM_Tie_Data[] GetTieData();

        /// <summary>Get our SVC data</summary><returns></returns>
        [OperationContract]
        MM_StaticVarCompensator_Data[] GetStaticVarCompensatorData();

        /// <summary>Get our system-wide data</summary><returns></returns>
        [OperationContract]
        MM_SystemWide_Generation_Data[] GetSystemWideGenerationData();

        /// <summary>Get our unit-type generation data</summary>
        /// <returns></returns>
        [OperationContract]
        MM_UnitType_Generation_Data[] GetUnitTypeGenerationData();

        /// <summary>Get our unit generation-specific data</summary>
        /// <returns></returns>
        [OperationContract]
        MM_Unit_Gen_Data[] GetUnitGenData();


        /// <summary>Get our basecase violation data</summary>
        /// <returns></returns>
        [OperationContract]
        MM_Basecase_Violation_Data[] GetBasecaseViolationData();


        /// <summary>Get our contingency violation data</summary>
        /// <returns></returns>
        [OperationContract]
        MM_Contingency_Violation_Data[] GetContingencyVioltationData();

        /// <summary>Get our SCADA Analog data</summary>
        /// <returns></returns>
        [OperationContract]
        MM_Scada_Analog[] GetSCADAAnalogData();


        /// <summary>Get our SCADA Analog data</summary>
        /// <returns></returns>
        [OperationContract]
        MM_Scada_Status[] GetSCADAStatusData();

        /// <summary>
        /// Get flowgate data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Flowgate_Data[] GetFlowgateData();
        /// Send a command to our system
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="OldValue"></param>
        /// <returns></returns>
        [OperationContract]
        bool SendCommand(String Command, String OldValue);

        /// <summary>
        /// Retrieve our historical list of EMS commands
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_EMS_Command[] GetEMSCommands();

        /// <summary>
        /// Get our unit control status
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Unit_Control_Status[] GetUnitControlStatus();

        /// <summary>
        /// Get our system epoch
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        DateTime GetServerEpoch();
    }
}