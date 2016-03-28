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
    /// This class holds information on unit data
    /// </summary>
    [RetrievalCommand("GetUnitData"), UpdateCommand("UpdateUnitData"), FileName("SE_Unit_Data.csv")]
    public class MM_Unit_Data
    {

        /// <summary></summary>
        public int TEID_Un { get; set; }

        /// <summary></summary>
        public float W_Un { get; set; }

        /// <summary></summary>
        public float WMeas_Un { get; set; }

        /// <summary></summary>
        public float R_Un { get; set; }

        /// <summary></summary>
        public float RMeas_Un { get; set; }

        /// <summary></summary>
        public bool Open_Un { get; set; }

        /// <summary></summary>
        public int Reg_Bus { get; set; }

        /// <summary></summary>
        public int Conn_Bus { get; set; }

        /// <summary></summary>
        public bool NoPSS_Un { get; set; }
        
        /// <summary></summary>
        public bool NoAVR_Un { get; set; }
    }
}
