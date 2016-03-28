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
    /// This class holds information on island data
    /// </summary>
    [RetrievalCommand("GetIslandData"), UpdateCommand("UpdateIslandData"), FileName("SE_Island_Data.csv"),RemovalCommand("HandleIslandRemoval", "Isl_Num")]
    public class MM_Island_Data
    {
        #region Variable declarations
        /// <summary></summary>
        public int Isl_Num { get; set; }

        /// <summary></summary>
        public float Frequency { get; set; }

        /// <summary></summary>
        public float WGen { get; set; }

        /// <summary></summary>
        public float WLoad { get; set; }

        /// <summary></summary>
        public float WLoss { get; set; }
        #endregion
    }
}
