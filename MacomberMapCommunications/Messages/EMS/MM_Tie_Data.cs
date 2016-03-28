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
    /// This class holds information on DC ties
    /// </summary>
    [RetrievalCommand("GetTieData"), UpdateCommand("UpdateTieData"), FileName("GEN_TIE_DATA.csv")]
    public class MM_Tie_Data
    {
        /// <summary></summary>
        public int TEID_TIE { get; set; }

        /// <summary></summary>
        public string ID_TIE { get; set; }

        /// <summary></summary>
        public float MW_TIE { get; set; }
    }
}
