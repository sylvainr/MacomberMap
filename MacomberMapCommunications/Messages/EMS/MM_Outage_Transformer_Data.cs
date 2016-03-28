using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.EMS
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability CoXfcil of Texas, Inc. All Rights Reserved.
    /// This class holds information on Transformer outage data
    /// </summary>
    [RetrievalCommand("GetTransformerOutageData"), UpdateCommand("UpdateTransformerOutageData"), FileName("QKNET_XF_OUTAGES.csv"), RemovalCommand("RemoveTransformerOutageData", "TEID_Xf")]
    public class MM_Outage_Transformer_Data
    {
        #region Variable declarations
        /// <summary></summary>
        public int TEID_Xf { get; set; }

        /// <summary></summary>
        public float OTMW_QkXf { get; set; }

        /// <summary></summary>
        public DateTime TStart_QkXf { get; set; }

        /// <summary></summary>
        public DateTime TEnd_QkXf { get; set; }

        /// <summary></summary>
        public DateTime TmpLst_QkXf { get; set; }

        /// <summary></summary>
        public DateTime TmpLed_QkXf { get; set; }

        /// <summary></summary>
        public String OSTyp_QkXf { get; set; }

        /// <summary></summary>
        public String OtTyp1_QkXf { get; set; }

        /// <summary></summary>
        public String OtTyp2_QkXf { get; set; }

        /// <summary></summary>
        public String OtTyp3_QkXf { get; set; }


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