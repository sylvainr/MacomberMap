using MacomberMapCommunications.Messages.EMS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapAdministrationService
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on the state of the system.
    /// </summary>
    public class MM_System_Information
    {
        /// <summary>Our phase shifter count</summary>
        [Category("EMS Data"), Description("Our phase shifter count")]
        public int PhaseShifterCount { get; set; }

        /// <summary>Our current CPU usage</summary>
        [Category("System"), Description("Our current CPU usage")]
        public double CPUUsage { get; set; }

        /// <summary>Our system total memory</summary>
        [Category("System"), Description("Our system total memory")]
        public ulong SystemTotalMemory { get; set; }

        /// <summary>Our system free memory</summary>
        [Category("System"), Description("Our system free memory")]
        public ulong SystemFreeMemory { get; set; }

        /// <summary>The amount of memory being used by the server application</summary>
        [Category("System"), Description("The amount of memory being used by the server application")]
        public long ApplicationMemoryUse { get; set; }

        /// <summary>The number of lines in the system</summary>
        [Category("EMS Data"), Description("The number of lines in the system")]
        public int LineCount { get; set; }

        /// <summary>The number of loads in the system</summary>
        [Category("EMS Data"), Description("The number of loads in the system")]
        public int LoadCount { get; set; }

        /// <summary>The number of buses in the system</summary>
        [Category("EMS Data"), Description("The number of buses in the system")]
        public int BusCount { get; set; }

        /// <summary>The number of line outages in the system</summary>
        [Category("EMS Data"), Description("The number of line outages in the system")]
        public int LineOutageCount { get; set; }

        /// <summary>The number of transformer outages in the system</summary>
        [Category("EMS Data"), Description("The number of transformer outages in the system")]
        public int TransformerOutageCount { get; set; }

        /// <summary>The number of units in the system</summary>
        [Category("EMS Data"), Description("The number of units in the system")]
        public int UnitCount { get; set; }

        /// <summary>The number of transformers in the system</summary>
        [Category("EMS Data"), Description("The number of transformers in the system")]
        public int TransformerCount { get; set; }

        /// <summary>The number of analog values in the system</summary>
        [Category("EMS Data"), Description("The number of analog values in the system")]
        public int AnalogCount { get; set; }

        /// <summary>The number of status points in the system</summary>
        [Category("EMS Data"), Description("The number of status points in the system")]
        public int StateCount { get; set; }

        /// <summary>The number of shunt compensators in the system</summary>
        [Category("EMS Data"), Description("The number of shunt compensators in the system")]
        public int ShuntCompensatorCount { get; set; }

        /// <summary>The number of zero impedance bridges in the system</summary>
        [Category("EMS Data"), Description("The number of zero impedance bridges in the system")]
        public int ZBRCount { get; set; }

        /// <summary>The number of unit outages in the system</summary>
        [Category("EMS Data"), Description("The number of unit outages in the system")]
        public int UnitOutageCount { get; set; }

        /// <summary>The number of users in the system</summary>
        [Category("System"), Description("The number of users in the system")]
        public int UserCount { get; set; }

        /// <summary>The number of unit gen data</summary>
        [Category("EMS Data"), Description("The number of unit gen data")]
        public int UnitGenCount { get; set; }

        /// <summary>The number of basecase violations</summary>
        [Category("EMS Data"), Description("The number of basecase violations")]
        public int BasecaseCount { get; set; }

        /// <summary>The number of contingency violations</summary>
        [Category("EMS Data"), Description("The number of contingency violations")]
        public int ContingencyCount { get; set; }

        /// <summary>The number of interfaces</summary>
        [Category("EMS Data"), Description("The number of interfaces")]
        public int InterfaceCount { get; set; }

        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of operatorship updates we have")]
        public int OperatorshipUpdateCount { get; set; }

        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of commands that we have had we have")]
        public int CommandCount { get; set; }

        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of unit simulation control parameters we have")]
        public int UnitControlCount { get; set; }
        
        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of synchroscopes we have")]
        public int SynchroscopeCount { get; set; }
        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of simulated units we have")]
        public int UnitSimulationCount { get; set; }
        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of load forecast points we have")]
        public int LoadForecastCount { get; set; }

        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of SCADA analog points we have")]
        public int SCADAAnalogCount { get; set; }
        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of SCADA status points we have")]
        public object SCADAStatusCount { get; set; }

        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of flowgates we have")]
        public int FlowgateCount { get; set; }

        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of LMP price points we have")]
        public int LMPCount { get; set; }

        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of system-level stauts points we have")]
        public int SystemCount { get; set; }

        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of generation by fuel type informational points we have")]
        public int UnitTypeCount { get; set; }

        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of static var compensators we have")]
        public int SVCCount { get; set; }
        
        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of DC ties we have")]
        public int TieCount { get; set; }

        /// <summary>The number of operatorship updates we have</summary>
        [Category("EMS Data"), Description("The number of breakers and switches we have")]
        public int BreakerSwitchCount { get; set; }

        /// <summary>The last IP address of our server</summary>
        [Category("Server"), Description("The last IP address of our server")]
        public System.Net.IPAddress LastTEDeAddress { get; set; }

        /// <summary>The last IP address of our server</summary>
        [Category("Server"), Description("The last time we received TEDE data")]
        public DateTime LastTEDeUpdate { get; set; }

        /// <summary>The status of the simulation</summary>
        [Category("Server"), Description("The current time of the simulation")]
        public DateTime SimulationTime { get; set; }

        /// <summary>The status of the simulation</summary>
        [Category("Server"), Description("The status of the simulation")]
        public MM_Simulation_Time.enumSimulationStatus SimulationStatus { get; set; }

       
    }
}