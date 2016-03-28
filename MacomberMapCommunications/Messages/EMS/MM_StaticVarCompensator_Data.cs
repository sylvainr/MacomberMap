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
    /// This class holds information on static var compensator data
    /// </summary>
    [RetrievalCommand("GetStaticVarCompensatorData"), UpdateCommand("UpdateStaticVarCompensatorData"), FileName("SE_SVS_DATA.csv")]
   public class MM_StaticVarCompensator_Data
    {

        /// <summary></summary>
        public int TEID_SVS { get; set; }

        /// <summary></summary>
        public float R_SVS { get; set; }

        /// <summary></summary>
        public float RMeas_SVS { get; set; }

        /// <summary></summary>
        public bool Open_SVS { get; set; }

        /// <summary></summary>
        public int Reg_Bus { get; set; }

        /// <summary></summary>
        public int Conn_Bus { get; set; }

        /// <summary></summary>
        public float MRNom_SVS { get; set; }

        /// <summary></summary>
        public bool AVR_SVS { get; set; }

        /// <summary></summary>
        public bool REMOVE_SVS { get; set; }

        /// <summary></summary>
        public float DisTarg_SVS { get; set; }

        /// <summary></summary>
        public bool VTARGMAN_SVS{ get; set; }

        /// <summary>The minimum MVAR</summary>
        public float RMN_SVS { get; set; }

        /// <summary>The minimum MVAR</summary>
        public float RMX_SVS { get; set; }
    }
}
