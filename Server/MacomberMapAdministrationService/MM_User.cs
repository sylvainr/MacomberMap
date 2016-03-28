using MacomberMapCommunications.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapAdministrationService
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a user
    /// </summary>
    public class MM_User
    {
        /// <summary></summary>
        public string UserName { get; set; }

        /// <summary></summary>
        public String MachineName { get; set; }

        /// <summary></summary>
        public int Port { get; set; }

        /// <summary></summary>
        public String IPAddress { get; set; }

        /// <summary></summary>
        public DateTime LoggedOnTime { get; set; }

        /// <summary></summary>
        public DateTime LastReceivedMessage { get; set; }

        /// <summary>The date/time the last command was set</summary>        
        public DateTime LastCommand { get; set; }

        /// <summary>The date/time the last data were sent to the client</summary>
        public DateTime LastSentMessage { get; set; }

        /// <summary></summary>
        public Guid UserId { get; set; }

        /// <summary>The version of our MM client</summary>
        public String ClientVersion { get; set; }

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

        /// <summary>Whether the user has ERCOT permissions</summary>
        [Category("System"), Description("The number of windows opened by the user")]
        public bool ERCOTUser { get; set; }
    }
}
