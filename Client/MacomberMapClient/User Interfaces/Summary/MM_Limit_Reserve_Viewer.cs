using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.User_Interfaces.Summary
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds the information for a viewer for CSC/IROL and reserve information
    /// </summary>
    public partial class MM_Limit_Reserve_Viewer : Form
    {
       

        /// <summary>
        /// Handle the closing of the form (to hide instead of shut down)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }

        }
        /// <summary>
        /// Initialize a new reserve viewer
        /// </summary>
        public MM_Limit_Reserve_Viewer(XmlElement BaseElement)
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            foreach (MM_Reserve Reserve in Data_Integration.Reserves.Values)
                Reserve.CreateDisplayItem(lv);

            //Add in our historical viewer
            MM_Historic_Viewer hv = new MM_Historic_Viewer(MM_Historic_Viewer.GraphModeEnum.HistoricalOnly, BaseElement.Attributes["PIQueries"].Value.Split(','), new string[] { },"");
            hv.Dock = DockStyle.Fill;
            tpHistoric.Controls.Add(hv);
        }


        /// <summary>
        /// Handle the timer update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            //First, fill in our reserves            
            try
            {
                lock (Data_Integration.Reserves)
                    foreach (MM_Reserve Reserve in Data_Integration.Reserves.Values)
                        if (Reserve.DisplayItem == null)
                            Reserve.CreateDisplayItem(this.lv);
                        else
                        {
                            String InVal = Reserve.Value.ToString("#,##0") + " mw";
                            if (Reserve.DisplayItem.SubItems[1].Text != InVal)
                                Reserve.DisplayItem.SubItems[1].Text = InVal;
                        }

            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError("UI Error updating reserves: " + ex.Message);
            }

            //Now, fill in our limits
            try
            {
                lock (Data_Integration.CSCIROLLimits)
                    foreach (MM_Limit Limit in Data_Integration.CSCIROLLimits.Values)
                    {
                        ListViewItem lvI = lv.Items[Limit.Name];
                        if (lvI == null)
                        {
                            lvI = lv.Items.Add(Limit.Name, Limit.Name, 0);
                            lvI.SubItems.Add((Limit.Current / Limit.Max).ToString("0.0%") + " (" + Limit.Current.ToString("#,##0") + " / " + Limit.Max.ToString("#,##0") + " mw)");
                            if (Limit.Name.StartsWith("Stability"))
                                lvI.Group = lv.Groups["IROL Limits"];
                            else
                                lvI.Group = lv.Groups["CSC Limits"];
                        }
                        else
                            lvI.SubItems[1].Text = (Limit.Current / Limit.Max).ToString("0.0%") + " (" + Limit.Current.ToString("#,##0") + " / " + Limit.Max.ToString("#,##0") + " mw)";
                    }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError("UI Error updating CSC/IROL limits: " + ex.Message);
            }

            //Now, autosize everything
            if (!HasResized && lv.Items.Count > 0)
            {
                foreach (ColumnHeader col in lv.Columns)
                    col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                Rectangle LastOne = lv.Items[lv.Items.Count - 1].Bounds;
                this.Size = new Size(253, 491);
                HasResized = true;
            }
        }
    }
}
