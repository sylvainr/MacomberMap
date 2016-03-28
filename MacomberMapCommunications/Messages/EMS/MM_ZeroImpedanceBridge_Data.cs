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
    /// This class holds information on line data
    /// </summary>
    [RetrievalCommand("GetZBRData"), UpdateCommand("UpdateZBRData"), FileName("SE_ZBR_DATA.csv")]
    public class MM_ZeroImpedanceBridge_Data
    {
        /// <summary></summary>
        public int TEID_ZBR { get; set; }

        /// <summary></summary>
        public int TEID_End1 { get; set; }

        /// <summary></summary>
        public float W_End1 { get; set; }

        /// <summary></summary>
        public float R_End1 { get; set; }

        /// <summary></summary>
        public float MVA_End1 { get; set; }

        /// <summary></summary>
        public float WMeas_End1 { get; set; }

        /// <summary></summary>
        public float RMeas_End1 { get; set; }

        /// <summary></summary>
        public float W_End2 { get; set; }

        /// <summary></summary>
        public float R_End2 { get; set; }

        /// <summary></summary>
        public float MVA_End2 { get; set; }

        /// <summary></summary>
        public float WMeas_End2 { get; set; }

        /// <summary></summary>
        public float RMeas_End2 { get; set; }

        /// <summary></summary>
        public float LIMIT1 { get; set; }

        /// <summary></summary>
        public float LIMIT2 { get; set; }

        /// <summary></summary>
        public float LIMIT3 { get; set; }

        /// <summary>The bus the ZBR is connected to on the near side</summary>
        public int End1_Bus { get; set; }

        /// <summary>The bus the ZBR is connected to on the near side</summary>
        public int End2_Bus { get; set; }
    }
}