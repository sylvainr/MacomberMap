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
    /// This class holds information on a contingency violation
    /// </summary>
    [RetrievalCommand("GetContingencyVioltationData"), UpdateCommand("UpdateContingencyViolationData"), FileName("CA_Ctg_Viol.csv"), RemovalCommand("RemoveContingencyViolationData", "ID_CTG,MONELE_TEID"), UpdateParametersAttribute(ErrorTime = int.MaxValue, WarningTime = int.MaxValue)]
    public class MM_Contingency_Violation_Data
    {
        /// <summary></summary>
        public string ID_CTG { get; set; }

        /// <summary></summary>
        public int MONELE_TEID { get; set; }

        /// <summary></summary>
        public int ND_TEID { get; set; }

        /// <summary></summary>
        public string VIOLTYP { get; set; }

        /// <summary></summary>
        public float PERCLIM1 { get; set; }

        /// <summary></summary>
        public float PERCLIM2 { get; set; }

        /// <summary></summary>
        public float PERCLIM3 { get; set; }

        /// <summary></summary>
        public enumViolationBase BASE1 { get; set; }

        /// <summary></summary>
        public enumViolationBase BASE2 { get; set; }

        /// <summary></summary>
        public enumViolationBase BASE3 { get; set; }

        /// <summary></summary>
        public DateTime TIMIN { get; set; }

        /// <summary></summary>
        public bool NEW { get; set; }

        /// <summary></summary>
        public float PRECTG { get; set; }

        /// <summary></summary>
        public float NOMKV { get; set; }

        /// <summary></summary>
        public float CURVAL { get; set; }
        /// <summary>
        /// worst percentage violation
        /// </summary>
        public float WRSPCT { get; set; }
        /// <summary>
        /// Worst violation time
        /// </summary>
        public DateTime TIMWRST { get; set; }
        /// <summary>
        /// Monitored element
        /// </summary>
        public string MONELM { get; set; }

        /// <summary>Our violation base</summary>
        public enum enumViolationBase
        {
            /// <summary>Unknown limit</summary>
            Unknown,

            /// <summary>Normal limit</summary>
            Norm,
            /// <summary>Emergency limit</summary>
            Emer,
            /// <summary>Load shed limit</summary>
            LdSh
        }
    }
}
