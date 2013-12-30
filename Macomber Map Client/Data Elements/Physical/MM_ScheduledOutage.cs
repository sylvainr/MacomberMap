using System;
using System.Collections.Generic;
using System.Text;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
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
