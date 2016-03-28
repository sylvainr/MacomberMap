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
    /// This class holds information on transformer data
    /// </summary>
    [RetrievalCommand("GetTransformerData"), UpdateCommand("UpdateTransformerData"), FileName("SE_XF_DATA.csv")]
    public class MM_Transformer_Data
    {
        /// <summary></summary>
        public int TEID_Xf { get; set; }

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

        /// <summary></summary>
        public int Tap { get; set; }

        /// <summary></summary>
        public int ZTap { get; set; }

        /// <summary>The bus the transformer is connected to on the near side</summary>
        public int End1_Bus { get; set; }

        /// <summary>The bus the transformer is connected to on the near side</summary>
        public int End2_Bus { get; set; }

        /// <summary></summary>
        public bool AVR_XF { get; set; }

        /// <summary></summary>
        public bool WAVR_XF { get; set; }

        /// <summary></summary>
        public bool WAVRN_XF { get; set; }

        /// <summary></summary>
        public bool OffAVR_XF { get; set; }

        /// <summary></summary>
        public bool VTargMan_XF { get; set; }

        /// <summary></summary>
        public int TapMin { get; set; }

        /// <summary></summary>
        public int ZTapMin { get; set; }

        /// <summary></summary>
        public int TapMax { get; set; }

        /// <summary></summary>
        public int ZTapMax { get; set; }

        /// <summary></summary>
        public int TEID_REGND { get; set; }

        /// <summary></summary>
        public float DisTarg_XF { get; set; }

        /// <summary></summary>
        public float DisDev_XF { get; set; }

        /// <summary></summary>
        public float VTarget_XF { get; set; }

        /// <summary></summary>
        public float Deviat_XF { get; set; }

        /// <summary></summary>
        public float DMM_XF { get; set; }

        /// <summary></summary>
        public bool Open_End1 { get; set; }
        
        /// <summary></summary>
        public bool Open_End2 { get; set; }

    }
}
