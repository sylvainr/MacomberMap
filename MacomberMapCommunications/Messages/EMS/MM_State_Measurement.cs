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
    /// This class holds information on a state measurement (e.g., is a breaker closed)
    /// </summary>
    [RetrievalCommand("GetStateMeasurementData"), UpdateCommand("UpdateStateMeasurementData"), FileName("QKNET_STATUS_MEAS.csv")]
    public class MM_State_Measurement
    {    
        /// <summary></summary>
        public int TEID_Stat { get; set; }

        /// <summary></summary>
        public bool Open_Stat { get; set; }

        /// <summary></summary>
        public bool Good_Stat { get; set; }

    }
}
