using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Attributes
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the attribute necessary for file updating, including warning and error times, and periods between full refreshes
    /// </summary>
    public class UpdateParametersAttribute: Attribute
    {
        /// <summary>The number of seconds after which, with no updates, the data are considered in a warning state</summary>
        public int WarningTime { get; set; }

        /// <summary>The number of seconds after which, with no updates, the data are considered in an error state</summary>
        public int ErrorTime { get; set; }

        /// <summary>The number of seconds after which a full refresh is performed</summary>
        public int FullRefreshTime { get; set; }

        /// <summary>The command on a client that fully refreshes the data</summary>
        public String FullRefreshCommand { get; set; }
    }
}
