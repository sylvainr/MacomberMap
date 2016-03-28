using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.EMS
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on line outage data
    /// </summary>
    [RetrievalCommand("GetLineOutageData"), UpdateCommand("UpdateLineOutageData"), FileName("QKNET_LINE_OUTAGES.csv"), RemovalCommand("RemoveLineOutageData", "TEID_LN")]
    public class MM_Outage_Line_Data
    {
        #region Variable declarations
       /// <summary></summary>
        public int TEID_LN { get; set; }

        /// <summary></summary>
        public float OTMW_QkLn { get; set; }

        /// <summary></summary>
        public DateTime TStart_QkLn { get; set; }

        /// <summary></summary>
        public DateTime TEnd_QkLn { get; set; }

        /// <summary></summary>
        public DateTime TmpLst_QkLn { get; set; }

        /// <summary></summary>
        public DateTime TmpLed_QkLn { get; set; }

        /// <summary></summary>
        public String OSTyp_QkLn { get; set; }

        /// <summary></summary>
        public String OtTyp1_QkLn { get; set; }

        /// <summary></summary>
        public String OtTyp2_QkLn { get; set; }

        /// <summary></summary>
        public String OtTyp3_QkLn { get; set; }

        /// <summary>
        /// substation name
        /// </summary>
        public string Station { get; set; }
        /// <summary>
        /// ems key2 (usually voltage)
        /// </summary>
        public string Key2 { get; set; }
        /// <summary>
        /// ems key 3
        /// </summary>
        public string Key3 { get; set; }
        /// <summary>
        /// ems key 4
        /// </summary>
        public string Key4 { get; set; }

        #endregion
    }
}
