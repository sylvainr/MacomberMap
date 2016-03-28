using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Summary
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D., ERCOT, Inc. All Rights Reserved
    /// This form provides a date/time popup for the user
    /// </summary>
    public partial class MM_Historic_Viewer_Date_SelectionForm : Form
    {
        /// <summary>
        /// Initialize a new date selection form
        /// </summary>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        public MM_Historic_Viewer_Date_SelectionForm(DateTime StartTime, DateTime EndTime)
        {
            InitializeComponent(); this.dtFrom.MinDate = this.dtTo.MinDate = DateTime.Now - Data_Integration.Permissions.MaxSpan;
            this.dtTo.MaxDate = this.dtFrom.MaxDate = DateTime.Now;
            this.dtFrom.Value = StartTime;
            this.dtTo.Value = EndTime;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        /// <summary>
        /// Get/set the from: date
        /// </summary>
        public DateTime StartDate
        {
            get { return dtFrom.Value; }
            set { dtFrom.Value = value; }
        }

        /// <summary>
        /// Get/set the to: date
        /// </summary>
        public DateTime EndDate
        {
            get { return dtTo.Value; }
            set { dtTo.Value = value; }
        }

    }
}
