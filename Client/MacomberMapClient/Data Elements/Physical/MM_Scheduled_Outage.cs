using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class contains upcoming outages
    /// </summary>
    public class MM_ScheduledOutage
    {
        /// <summary>The outage's start</summary>
        public DateTime OutageStart;

        /// <summary>The outage's end</summary>
        public DateTime OutageEnd;

        /// <summary>The description of the outage</summary>
        public String OutageDescription;
    }
}