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
    /// This class holds information on island data from the simulator
    /// </summary>
    [RetrievalCommand("GetIslandSimulationData"), UpdateCommand("UpdateIslandSimulationData"), FileName("SIMULATOR_ISLAND_DATA.csv"), RemovalCommand("HandleIslandSimulationRemoval", "Isl_Num")]
    public class MM_Island_Simulation_Data
    {
        /// <summary>The island number</summary>
        public int Isl_Num { get; set; }

        /// <summary>Island frequency control</summary>
        public bool InProg_Island { get; set; }

        /// <summary>Island manual target</summary>
        public bool TarMan_Island { get; set; }
    }
}
