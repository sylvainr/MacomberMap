using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages
{
    /// <summary>
    /// © 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a user's client status
    /// </summary>
    public class MM_Client_Status
    {
        /// <summary>Our current CPU usage</summary>
        [Category("System"), Description("Our current CPU usage")]
        public double CPUUsage { get; set; }

        /// <summary>Our system total memory</summary>
        [Category("System"), Description("Our system total memory")]
        public ulong SystemTotalMemory { get; set; }

        /// <summary>Our system free memory</summary>
        [Category("System"), Description("Our system free memory")]
        public ulong SystemFreeMemory { get; set; }

        /// <summary>The amount of memory being used by the server application</summary>
        [Category("System"), Description("The amount of memory being used by the server application")]
        public long ApplicationMemoryUse { get; set; }

        /// <summary>The number of windows opened by the user</summary>
        [Category("System"), Description("The number of windows opened by the user")]
        public int OpenWindows { get; set; }
    }
}
