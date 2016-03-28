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
    /// This class holds information on unit simulation data
    /// </summary>
    [RetrievalCommand("GetUnitSimulationData"), UpdateCommand("UpdateUnitSimulationData"), FileName("SIMULATOR_UNIT_DATA.csv")]
    public class MM_Unit_Simulation_Data
    {
        /// <summary>The TEID of the unit</summary>
        public int TEID_Un { get; set; }

        /// <summary>The frequency of our unit</summary>
        public float Fhz_Un { get; set; }

        /// <summary>Whether our unit is on frequency control</summary>
        public bool FrqCtrl_Un { get; set; }

        /// <summary>The island number of our unit</summary>
        public int Isl_Num { get; set; }

        /// <summary>our unit status</summary>
        public string GCPStatus_Un { get; set; }

        /// <summary>Removed flag of our unit</summary>
        public bool Remove_Un { get; set; }

        /// <summary></summary>
        public bool Tarman_SIsl { get; set; }

        /// <summary></summary>
        public bool VTargman_Un { get; set; }

        /// <summary></summary>
        public bool AGC_Plc { get; set; }

        /// <summary></summary>
        public bool AmPlc_Plc { get; set; }

        /// <summary></summary>
        public bool WLocal_Un { get; set; }

        /// <summary></summary>
        public bool Dnogen_Un { get; set; }

        /// <summary></summary>
        public bool Djou_Un { get; set; }

        /// <summary></summary>
        public float Dwdtloc_Un { get; set; }

        /// <summary></summary>
        public float Wdesloc_Un { get; set; }

        /// <summary></summary>
        public bool NoAGC_Un { get; set; }

        /// <summary></summary>
        public bool Varman_Un { get; set; }

        /// <summary></summary>
        public float FrqTarg_SIsl { get; set; }

        /// <summary></summary>
        public float FToler_SIsl { get; set; }

        /// <summary></summary>
        public float Distarg_Un { get; set; }
    }
}
