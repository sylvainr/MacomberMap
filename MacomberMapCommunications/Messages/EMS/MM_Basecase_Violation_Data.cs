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
    /// This class holds information on a basecase violation
    /// </summary>
    [RetrievalCommand("GetBasecaseViolationData"), UpdateCommand("UpdateBasecaseViolationData"), FileName("CA_BC_VIOL.csv"), RemovalCommand("RemoveBasecaseViolationData", "MONELE_TEID"), UpdateParametersAttribute(ErrorTime = int.MaxValue, WarningTime = int.MaxValue)]
    public class MM_Basecase_Violation_Data
    {
        /// <summary>The TEID of our monitored element</summary>
        public int MONELE_TEID { get; set; }

        /// <summary>The node of our monitored element</summary>
        public int ND_TEID { get; set; }

        /// <summary>The violation type</summary>
        public string VIOLTYP_MNOL { get; set; }

        /// <summary></summary>
        public float WSVIOL_MNOL { get; set; }

        /// <summary></summary>
        public float LIMIT1_MNOL { get; set; }

        /// <summary></summary>
        public float LIMIT2_MNOL { get; set; }

        /// <summary></summary>
        public float LIMIT3_MNOL { get; set; }

        /// <summary></summary>
        public float PERCLIM1_MNOL { get; set; }

        /// <summary></summary>
        public float PERCLIM2_MNOL { get; set; }

        /// <summary></summary>
        public float PERCLIM3_MNOL { get; set; }

        /// <summary></summary>
        public string BASE1_MNOL { get; set; }

        /// <summary></summary>
        public string BASE2_MNOL { get; set; }

        /// <summary></summary>
        public string BASE3_MNOL { get; set; }

        /// <summary></summary>
        public DateTime TSTVIOL_MNOL { get; set; }

        /// <summary></summary>
        public DateTime TWSVIOL_MNOL { get; set; }

        /// <summary></summary>
        public bool DISPLAY_MNOL { get; set; }

        /// <summary></summary>
        public bool NEW_MNOL { get; set; }

        /// <summary></summary>
        public bool WARNVIOL_MNOL { get; set; }
    }
}
