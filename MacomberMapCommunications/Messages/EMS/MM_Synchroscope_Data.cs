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
    /// This class holds information on synchroscope data from the simulator
    /// </summary>
    [RetrievalCommand("GetSynchroscopeData"), UpdateCommand("UpdateSynchroscopeData"), FileName("SIMULATOR_SYNCRY_DATA.csv")]
    public class MM_Synchroscope_Data
    {
        /// <summary>The TEID of the circuit breaker</summary>
        public int TEID_CB {get;set;}

        /// <summary></summary>
        public int TEID_ST { get; set; }

        /// <summary>Disabled</summary>
        public bool Disabled_Syncry {get;set;}

        /// <summary>Actual angle</summary>
        public float Dangle_Syncry { get; set; }

        /// <summary>Actual frequency</summary>
        public float DFreq_Syncry { get; set; }

        /// <summary>Actual Magnitude</summary>
        public float DMag_Syncry { get; set; }

        /// <summary>Relay angle</summary>
        public float AngMX_Syncty { get; set; }
        
        /// <summary>Relay frequency</summary>
        public float FreqMX_Syncty { get; set; }
        
        /// <summary>Relay Magnitude</summary>
        public float MagMX_Syncty { get; set; }
    }
}
