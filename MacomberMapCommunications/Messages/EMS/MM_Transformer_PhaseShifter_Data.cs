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
    /// This class holds information on transformer phase shifter data
    /// </summary>
    [RetrievalCommand("GetTransformerPhaseShifterData"), UpdateCommand("UpdateTransformerPhaseShifterData"), FileName("SE_PS_DATA.csv")]
    public class MM_Transformer_PhaseShifter_Data
    {
        /// <summary>The TEID of the power transformer</summary>
        public int TEID_XF {get;set;}

        /// <summary>The TEID of the phase shifter</summary>
        public int TEID_PS { get; set; }

        /// <summary>– Manual Regulation Target (true/false)</summary>
        public bool WTARGMAN_PS {get;set;}

        /// <summary>Regulation Target voltage (MW)</summary>
        public float WTARGET_PS {get;set;}

        /// <summary>Regulation target Deviation (MW)</summary>
        public float DEVIAT_PS{get;set;}
        
        /// <summary>“AWR” in control panel (true/false)/// </summary>
        public bool AWR_PS{get;set;}

        /// <summary>Tap level</summary>
        public int TAP_PS {get;set;}

        /// <summary>Min Tap value</summary>
        public int MN_TAPTY {get;set;}

        /// <summary>Max Tap value</summary>
        public int MX_TAPTY { get; set; }

    }
}
