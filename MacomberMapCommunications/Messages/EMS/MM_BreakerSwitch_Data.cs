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
    /// This class holds information on breaker and switch data
    /// </summary>
    [RetrievalCommand("GetBreakerSwitchData"), UpdateCommand("UpdateBreakerSwitchData"), FileName("QKNET_CB_DATA.csv")]
    public class MM_BreakerSwitch_Data
    {
        /// <summary></summary>
        public int TEID_CB { get;set;}

        /// <summary></summary>
        public bool Open_CB { get; set; }

        /// <summary></summary>
        public int Near_BS { get; set; }

        /// <summary></summary>
        public int Far_BS { get; set; }

        /// <summary></summary>
        public int MaxSync_CB { get; set; }

        /// <summary></summary>
        public int MinSync_CB { get; set; }
    }
}
