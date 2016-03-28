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
    /// This class holds information on bus data
    /// </summary>
    [RetrievalCommand("GetBusData"), UpdateCommand("UpdateBusData"), FileName("SE_BUS_DATA.csv"), RemovalCommand("HandleBusRemoval", "TEID_Nd")]
    public class MM_Bus_Data
    {
        /// <summary></summary>
        public int Bus_Num { get; set; }
        
        /// <summary>The TEID of the associated node</summary>
        public int TEID_Nd { get; set; }

        /// <summary>The TEID of the substation</summary>
        public int TEID_St { get; set; }

        /// <summary></summary>
        public float NomKv { get; set; }
        
        /// <summary></summary>
        public float V_Pu { get; set; }
        
        /// <summary></summary>
        public float Angle { get; set; }
        
        /// <summary></summary>
        public int Island_ID { get; set; }
        
        /// <summary></summary>
        public bool Dead { get; set; }

        /// <summary></summary>
        public bool Open { get; set; }
    }
}
