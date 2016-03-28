using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MacomberMapCommunications.Attributes;

namespace MacomberMapCommunications.Messages.EMS
{
    /// <summary>
    /// Flowgate data from RTLODF
    /// </summary>
    [RetrievalCommand("GetFlowgateData"), UpdateCommand("UpdateFlowgateData"), FileName("FLOWGATE_DATA.csv"), UpdateParameters(WarningTime = 300, ErrorTime = 350, FullRefreshTime = 200, FullRefreshCommand = "LoadFlowgateData")]
    public class MM_Flowgate_Data
    {
        /// <summary>
        /// Flowgate ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// IDC number
        /// </summary>
        public int Idc
        {
            get; set;
        }
        /// <summary>
        /// Line outage distributio factor
        /// </summary>
        public float Lodf
        {
            get; set;
        }
        /// <summary>
        /// Flowgate type
        /// </summary>
        public string Type
        {
            get; set;
        }
        /// <summary>
        /// Reason for creating the flowgate
        /// </summary>
        public string Reason
        {
            get; set;
        }
        /// <summary>
        /// Post contingent flow
        /// </summary>
        public float PCTGFlow
        {
            get; set;
        }
        /// <summary>
        /// Normal limit for this flowgate
        /// </summary>
        public float NormLimit
        {
            get; set;
        }
        /// <summary>
        /// Emer limit for this flowgate
        /// </summary>
        public float EmerLimit
        {
            get; set;
        }
        /// <summary>
        /// Limit for IROL (reciporical) flowgates
        /// </summary>
        public float IROLLimit
        {
            get; set;
        }
        /// <summary>
        /// Operational hint of action to take when this flowgate is loaded.
        /// </summary>
        public string Hint
        {
            get; set;
        }

    }
}
