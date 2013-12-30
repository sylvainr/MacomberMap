using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Macomber_Map.User_Interface_Components
{
    //TODO: Complete the calls to provide the interface for viewing CIM topology, and breaker-to-breaker parsing based on the current topology and configuration.s

    /// <summary>
    /// This class displays the connectivity between various elements from within CIM
    /// (aka CIM Browser)
    /// </summary>
    public partial class Connectivity_Viewer : UserControl
    {
        /// <summary>
        /// Initialize the connectivity viewer
        /// </summary>
        public Connectivity_Viewer()
        {
            InitializeComponent();
        }
    }
}
