using MacomberMapClient.Integration;
using MacomberMapCommunications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComTypes = System.Runtime.InteropServices;

namespace MacomberMapClient.User_Interfaces.Generic
{
    /// <summary>
    /// This class holds a UI helper, that tracks CPU and memory usage
    /// </summary>
    public class MM_UserInterface_Helper
    {
        #region Variable declarations
        /// <summary>Our label for memory updates</summary>
        public ToolStripStatusLabel lblMem;

        /// <summary>Our label for CPU usage updates</summary>
        public ToolStripStatusLabel lblCPU;

        /// <summary>Our main application toolstrip</summary>
        public StatusStrip ssMain;
        #endregion




        #region Initialization
        /// <summary>
        /// Initialize a new UI helper
        /// </summary>
        /// <param name="ssMain"></param>
        /// <param name="lblCPU"></param>
        /// <param name="lblMem"></param>
        public MM_UserInterface_Helper(StatusStrip ssMain, ToolStripStatusLabel lblMem, ToolStripStatusLabel lblCPU)
        {
            this.ssMain = ssMain;
            this.lblMem = lblMem;
            this.lblCPU = lblCPU;
        }
        #endregion

        #region Updating CPU/memory
        /// <summary>
        /// Update our CPU and memory usage
        /// </summary>
        /// <param name="Mem"></param>
        public void UpdateCPUAndMemory(out long Mem)
        {
            lblCPU.Text = "CPU: " + MM_Server_Interface.ClientStatus.CPUUsage + "%";
            Mem = MM_Server_Interface.ClientStatus.ApplicationMemoryUse;

            if (Mem > 1024 * 1024 * 1024)
                lblMem.Text = "Mem: " + (Mem / (1024 * 1024)).ToString("#,##0 M");
            else if (Mem > 1024 * 1024)
                lblMem.Text = "Mem: " + (Mem / (1024)).ToString("#,##0 K");
            else
                lblMem.Text = "Mem: " + Mem.ToString("#,##0");
        }
        #endregion
    }
}