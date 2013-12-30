using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using Macomber_Map.Data_Elements;
using System.Drawing;
using System.Xml;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class holds the information for a viewer for CSC/IROL and reserve information
    /// </summary>
    public class Limit_Reserve_Viewer : Form
    {
        private ListView lv;
        private ColumnHeader colName;
        private ColumnHeader colValue;
        private Timer tmrUpdate;
        private System.ComponentModel.IContainer components;
        private TabControl tcMain;
        private TabPage tpCurrent;
        private TabPage tpHistoric;
        private bool HasResized = false;

        /// <summary>
        /// Initialize a new limit reserve viewer
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("CSC Limits", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("IROL Limits", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Limit_Reserve_Viewer));
            this.lv = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colValue = new System.Windows.Forms.ColumnHeader();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpCurrent = new System.Windows.Forms.TabPage();
            this.tpHistoric = new System.Windows.Forms.TabPage();
            this.tcMain.SuspendLayout();
            this.tpCurrent.SuspendLayout();
            this.SuspendLayout();
            // 
            // lv
            // 
            this.lv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colValue});
            this.lv.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup1.Header = "CSC Limits";
            listViewGroup1.Name = "CSC Limits";
            listViewGroup2.Header = "IROL Limits";
            listViewGroup2.Name = "IROL Limits";
            this.lv.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.lv.Location = new System.Drawing.Point(3, 3);
            this.lv.Name = "lv";
            this.lv.Size = new System.Drawing.Size(805, 394);
            this.lv.TabIndex = 0;
            this.lv.UseCompatibleStateImageBehavior = false;
            this.lv.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 250;
            // 
            // colValue
            // 
            this.colValue.Text = "Value";
            this.colValue.Width = 216;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpCurrent);
            this.tcMain.Controls.Add(this.tpHistoric);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(819, 426);
            this.tcMain.TabIndex = 1;
            // 
            // tpCurrent
            // 
            this.tpCurrent.Controls.Add(this.lv);
            this.tpCurrent.Location = new System.Drawing.Point(4, 22);
            this.tpCurrent.Name = "tpCurrent";
            this.tpCurrent.Padding = new System.Windows.Forms.Padding(3);
            this.tpCurrent.Size = new System.Drawing.Size(811, 400);
            this.tpCurrent.TabIndex = 0;
            this.tpCurrent.Text = "Current";
            this.tpCurrent.UseVisualStyleBackColor = true;
            // 
            // tpHistoric
            // 
            this.tpHistoric.Location = new System.Drawing.Point(4, 22);
            this.tpHistoric.Name = "tpHistoric";
            this.tpHistoric.Padding = new System.Windows.Forms.Padding(3);
            this.tpHistoric.Size = new System.Drawing.Size(239, 296);
            this.tpHistoric.TabIndex = 1;
            this.tpHistoric.Text = "Historic";
            this.tpHistoric.UseVisualStyleBackColor = true;
            // 
            // Limit_Reserve_Viewer
            // 
            this.ClientSize = new System.Drawing.Size(819, 426);
            this.Controls.Add(this.tcMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Limit_Reserve_Viewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            this.Text = "Limits and Reserves";
            this.tcMain.ResumeLayout(false);
            this.tpCurrent.ResumeLayout(false);
            this.ResumeLayout(false);

        }

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
        public Limit_Reserve_Viewer(XmlElement BaseElement)
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            foreach (MM_Reserve Reserve in Data_Integration.Reserves.Values)
                Reserve.CreateDisplayItem(lv);

            //Add in our historical viewer
            Historical_Viewer hv = new Historical_Viewer(Historical_Viewer.GraphModeEnum.HistoricalOnly, BaseElement.Attributes["HistoricQueries"].Value.Split(','), new string[] { });
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
                Program.LogError("UI Error updating reserves: " + ex.Message);
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
                Program.LogError("UI Error updating CSC/IROL limits: " + ex.Message);
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
