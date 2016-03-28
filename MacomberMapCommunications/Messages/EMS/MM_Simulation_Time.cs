using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.EMS
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class reports the simulator time
    /// </summary>
    [RetrievalCommand("GetSimulatorTime"), UpdateCommand("UpdateSimulatorTime"), FileName("SIMULATOR_SIMULATE_DATA.csv")]
    public class MM_Simulation_Time
    {
        /// <summary>The simulation time of our system</summary>
        public DateTime Simulation_Time { get; set; }

        /// <summary>The current status of our simulation</summary>
        public enumSimulationStatus Status { get; set; }

        /// <summary>Our collection of simulation statuses</summary>
        public enum enumSimulationStatus
        {
            /// <summary></summary>
            Unknown,
            /// <summary></summary>
            PSM_Down,
            /// <summary></summary>
            DTSMgr_Down,
            /// <summary></summary>
            Starting,
            /// <summary></summary>
            DTSDown,
            /// <summary></summary>
            CaseRetr,
            /// <summary></summary>
            Recovery,
            /// <summary></summary>
            Running,
            /// <summary></summary>
            Paused,
            /// <summary></summary>
            Pausing
        }

        
    }
}
