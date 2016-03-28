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
    /// This class holds information on shunt compensator data
    /// </summary>
    [RetrievalCommand("GetShuntCompensatorData"), UpdateCommand("UpdateShuntCompensatorData"), FileName("SE_CP_DATA.csv")]
    public class MM_ShuntCompensator_Data
    {
        /// <summary></summary>
        public int TEID_CP { get; set; }

        /// <summary></summary>
        public float R_CP { get; set; }

        /// <summary></summary>
        public float RMeas_CP { get; set; }

        /// <summary></summary>
        public bool Open_CP { get; set; }

        /// <summary></summary>
        public int Reg_Bus { get; set; }

        /// <summary></summary>
        public int Conn_Bus { get; set; }

        /// <summary></summary>
        public float MRNom_CP { get; set; }

        /// <summary></summary>
        public bool AVR_CP { get; set; }

        /// <summary></summary>
        public bool RMVENABL_CP { get; set; }

        /// <summary></summary>
        public bool REMOVE_CP { get; set; }

        /// <summary></summary>
        public bool Bank_CP { get; set; }

        /// <summary></summary>
        public bool VTargMan_CP { get; set; }

        /// <summary></summary>
        public float DisTarg_CP { get; set; }
    }
}
