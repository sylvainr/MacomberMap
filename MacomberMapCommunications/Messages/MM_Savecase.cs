using MacomberMapCommunications.Messages.EMS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages
{
    /// <summary>
    /// © 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides our serializable savecase information
    /// </summary>
    public class MM_Savecase
    {
        #region Variable declarations

        /// <summary>The last IP address that posted data to TEDE</summary>
        public IPAddress LastTEDEAddress { get; set; }

        /// <summary>The last timestamp from TEDE updates</summary>
        public DateTime LastTEDEUpdate { get; set; }

        /// <summary>Report the start time of our server</summary>
        public DateTime StartTime { get; set; }

        /// <summary>Our collection of line data</summary>
        public List<MM_Line_Data> LineData { get; set; }

        /// <summary>Our collection of loaddata</summary>
        public List<MM_Load_Data> LoadData { get; set; }

        /// <summary>Our collection of island data</summary>
        public Dictionary<int, MM_Island_Data> IslandData { get; set; }

        /// <summary>Our collection of simulation island data</summary>
        public Dictionary<int, MM_Island_Simulation_Data> IslandSimulationData { get; set; }

        /// <summary>Our collection of bus data</summary>
        public Dictionary<int, MM_Bus_Data> BusData { get; set; }

        /// <summary>Our collection of line outage data data</summary>
        public Dictionary<int, MM_Outage_Line_Data> LineOutageData { get; set; }

        /// <summary>Our collection of unit outage data data</summary>
        public Dictionary<int, MM_Outage_Unit_Data> UnitOutageData { get; set; }

        /// <summary>Our collection of transformer outage data data</summary>
        public Dictionary<int, MM_Outage_Transformer_Data> TransformerOutageData { get; set; }

        /// <summary>Our basecase violation data</summary>
        public Dictionary<int, MM_Basecase_Violation_Data> BasecaseViolationData { get; set; }

        /// <summary>Our contingency violation data</summary>
        public Dictionary<string, MM_Contingency_Violation_Data> ContingencyViolationData { get; set; }

        /// <summary>Our collection of analog measurement data</summary>
        public List<MM_Analog_Measurement> AnalogMeasurementData { get; set; }

        /// <summary>Our collection of shunt compensator data</summary>
        public List<MM_ShuntCompensator_Data> ShuntCompensatorData { get; set; }

        /// <summary>Our collection of state measurement data</summary>
        public List<MM_State_Measurement> StateMeasurementData { get; set; }

        /// <summary>Our collection of transformer data</summary>
        public List<MM_Transformer_Data> TransformerData { get; set; }

        /// <summary>Our collection of transformer phase shifter data</summary>
        public List<MM_Transformer_PhaseShifter_Data> TransformerPhaseShifterData { get; set; }

        /// <summary>Our collection of zero impedance bridge data</summary>
        public List<MM_ZeroImpedanceBridge_Data> ZBRData { get; set; }

        /// <summary>Our collection of unit data</summary>
        public List<MM_Unit_Data> UnitData { get; set; }

        /// <summary>Our array of timestamps that gets passed to the clients for updates</summary>
        public List<MM_Data_Collection> Timestamps { get; set; }

        /// <summary>Our collection of operatorship updates</summary>
        public Dictionary<int, MM_Operatorship_Update> OperatorshipUpdates { get; set; }

        /// <summary>Our breaker/switch data</summary>
        public List<MM_BreakerSwitch_Data> BreakerSwitchData { get; set; }

        /// <summary>Our chart data</summary>
        public List<MM_Chart_Data> ChartData { get; set; }

        /// <summary>Our DC Tie data</summary>
        public List<MM_Tie_Data> TieData { get; set; }

        /// <summary>Our DC tie data</summary>
        public List<MM_Tie_Data> DCTieData { get { return TieData; } set { TieData = value; } }

        /// <summary>Our SVC data</summary>
        public List<MM_StaticVarCompensator_Data> SVCData { get; set; }

        /// <summary>Our Unit-type generation data</summary>
        public List<MM_UnitType_Generation_Data> UnitTypeGenerationData { get; set; }

        /// <summary>Our system-wide generation data</summary>
        public List<MM_SystemWide_Generation_Data> SystemWideGenerationData { get; set; }

        /// <summary>Our unit generation-specific data</summary>
        public List<MM_Unit_Gen_Data> UnitGenData { get; set; }

        /// <summary>Our interface data</summary>
        public List<MM_Interface_Monitoring_Data> InterfaceData { get; set; }

        /// <summary>Our SCADA Status data</summary>
        public List<MM_Scada_Status> SCADAStatuses { get; set; }

        /// <summary>Our SCADA analog data</summary>
        public List<MM_Scada_Analog> SCADAAnalogs { get; set; }

        /// <summary>Our load forecast data</summary>
        public List<MM_Load_Forecast_Data> LoadForecastData { get; set; }

        /// <summary>Our unit simulation data</summary>
        public List<MM_Unit_Simulation_Data> UnitSimulationData { get; set; }

        /// <summary>Our synchroscope data</summary>
        public List<MM_Synchroscope_Data> SynchroscopeData { get; set; }

        /// <summary>Our simulator time data</summary>
        public List<MM_Simulation_Time> SimulatorTimeData { get; set; }

        /// <summary>Our collection of EMS commands</summary>
        public List<MM_EMS_Command> EMSCommands  { get;set; }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new savecase
        /// </summary>
        public MM_Savecase()
        {
            foreach (PropertyInfo pI in this.GetType().GetProperties())
                if (pI.PropertyType == typeof(DateTime))
                    pI.SetValue(this, DateTime.MinValue);
                else if (pI.PropertyType == typeof(IPAddress))
                    pI.SetValue(this, IPAddress.None);
                else if (pI.PropertyType.GetInterfaces().Contains(typeof(IList)))
                    pI.SetValue(this, Activator.CreateInstance(pI.PropertyType));
                else if (pI.PropertyType.GetInterfaces().Contains(typeof(IDictionary)))
                    pI.SetValue(this, Activator.CreateInstance(pI.PropertyType));
        }

        /// <summary>
        /// Load the current savecase from a server
        /// </summary>
        /// <param name="TargetType"></param>
        public void LoadCurrentSavecase(Type TargetType)
        {
            //Go through all the static fields in our target type, and apply
            foreach (PropertyInfo pI in this.GetType().GetProperties())
            {
                Object InVal = TargetType.GetField(pI.Name).GetValue(null);
                pI.SetValue(this, InVal);
            }

            if (EMSCommands == null)
                EMSCommands = new List<MM_EMS_Command>();
        }

        /// <summary>
        /// Apply our savecase to our system
        /// </summary>
        /// <param name="TargetType"></param>
        public void ApplySavecase(Type TargetType)
        {
            //Go through all the static fields in our target type, and apply
            foreach (PropertyInfo pI in this.GetType().GetProperties())
            {
                FieldInfo FoundField = TargetType.GetField(pI.Name);
                if (FoundField != null)
                {
                    Object InVal = pI.GetValue(this);
                    if (InVal.GetType() == FoundField.FieldType)
                        FoundField.SetValue(null, InVal);
                    else if (FoundField.FieldType.IsArray && InVal.GetType().GetInterfaces().Contains(typeof(IList)))
                    {
                        IList InList = (IList)InVal;
                        Array OutArray = Array.CreateInstance(FoundField.FieldType.GetElementType(), InList.Count);
                        for (int a = 0; a < InList.Count; a++)
                            OutArray.SetValue(InList[a], a);
                        FoundField.SetValue(null, OutArray);
                    }
                }
                else
                {
                    PropertyInfo FoundProperty = TargetType.GetProperty(pI.Name);
                    if (FoundProperty != null)
                        FoundProperty.SetValue(null, pI.GetValue(this));
                }
            }
        }
        #endregion
    }
}
