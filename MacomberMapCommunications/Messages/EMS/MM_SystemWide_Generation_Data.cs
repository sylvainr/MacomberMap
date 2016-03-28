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
    /// This class holds information on system-wide generation data
    /// </summary>
    [RetrievalCommand("GetSystemWideGenerationData"), UpdateCommand("UpdateSystemWideGenerationData"), FileName("GEN_OPA_DATA.csv")]
    public class MM_SystemWide_Generation_Data
    {
        /// <summary></summary>
        public String Id_OPA { get; set; }

        /// <summary></summary>
        public float Ld_OPA { get; set; }

        /// <summary></summary>
        public float FHZ_OPA { get; set; }

        /// <summary></summary>
        public float ACE_OPA { get; set; }

        /// <summary></summary>
        public float Gen_OPA { get; set; }

        /// <summary></summary>
        public float MWDistL_OPA { get; set; }

        /// <summary></summary>
        public float PRCT_OPA { get; set; }

        /// <summary></summary>
        public float IBPTCAPT_OPA { get; set; }

        /// <summary></summary>
        public float DBPTCAPT_OPA { get; set; }

        /// <summary></summary>
        public float HASLT_OPA { get; set; }

        /// <summary></summary>
        public float HSLT_OPA { get; set; }

        /// <summary></summary>
        public float HDLT_OPA { get; set; }

        /// <summary></summary>
        public float RGUASST_OPA { get; set; }

        /// <summary></summary>
        public float RGDASST_OPA { get; set; }

        /// <summary></summary>
        public float RVRRCPT_OPA { get; set; }

        /// <summary></summary>
        public float RVNSPT_OPA { get; set; }

        /// <summary></summary>
        public float RVSP_OPA { get; set; }

        /// <summary></summary>
        public float RVBLKS_OPA { get; set; }

        /// <summary></summary>
        public bool RVRMRN_OPA { get; set; }

        /// <summary></summary>
        public bool DBAND_OPA { get; set; }

        /// <summary></summary>
        public bool REGUL_OPA { get; set; }

        /// <summary></summary>
        public bool REGULI_OPA { get; set; }

        /// <summary></summary>
        public bool ASS_OPA { get; set; }

        /// <summary></summary>
        public bool EME_OPA { get; set; }

        /// <summary></summary>
        public DateTime TAGCLast_OPA { get; set; }
    }
}