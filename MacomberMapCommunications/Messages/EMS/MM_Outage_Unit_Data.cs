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
    /// This class holds information on unit outage data
    /// </summary>
    [RetrievalCommand("GetUnitOutageData"), UpdateCommand("UpdateUnitOutageData"), FileName("QKNET_UNIT_OUTAGES.csv"), RemovalCommand("RemoveUnitOutageData", "TEID_Un")]
    public class MM_Outage_Unit_Data
    {
        #region Variable declarations
        /// <summary></summary>
        public int TEID_Un { get; set; }

        /// <summary></summary>
        public float OTMW_QkUn { get; set; }

        /// <summary></summary>
        public DateTime TStart_QkUn { get; set; }

        /// <summary></summary>
        public DateTime TEnd_QkUn { get; set; }

        /// <summary></summary>
        public DateTime TmpLst_QkUn { get; set; }

        /// <summary></summary>
        public DateTime TmpLed_QkUn { get; set; }

        /// <summary></summary>
        public String OSTyp_QkUn { get; set; }

        /// <summary></summary>
        public String OtTyp1_QkUn { get; set; }
        
        /// <summary></summary>        
        public String OtTyp2_QkUn { get; set; }

        /// <summary></summary>
        public String OtTyp3_QkUn { get; set; }

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