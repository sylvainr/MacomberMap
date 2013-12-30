using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using System.Xml;
using Macomber_Map.Data_Elements;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class retrieves updated information every second on the status of all base processes (EMS, MMS, etc.)
    /// </summary>
    public partial class Process_Viewer : Form
    {

        #region Variable declarations
        /// <summary>The text box in key indicators whose color will be changed depending on process status</summary>
        private Label ProcessOverview;
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize the process viewer
        /// </summary>
        public Process_Viewer(Label ProcessOverview)
        {
            this.ProcessOverview = ProcessOverview;
            InitializeComponent();
            
        }

        /// <summary>
        /// Assign an integration layer to the control
        /// </summary>
        public void SetControls()
        {
            

            //Set up our list view.
            lvProcesses.Columns.Add("Name", "Name");
            lvProcesses.Columns.Add("Time since last completion", "Time since last completion");            
            tmr.Enabled = true;
        } 
        #endregion

        /// <summary>
        /// Handle the timer update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTimer(object sender, EventArgs e)
        {
            int WorstProcess = 0;
            foreach (MM_Process Process in new List<MM_Process>(Data_Integration.Processes.Values) )                           
                try
                {

                    TimeSpan ThisTime = (Data_Integration.SaveTime.ToOADate() == 0 ? DateTime.Now : Data_Integration.SaveTime) - Process.LastExecution;
                    //If it's not already set, locate the item associated with the process
                    if (Process.lvItem == null)
                    {
                        Process.lvItem = lvProcesses.Items.Add(Process.Name);
                        if (Process.LastExecution.ToOADate() == 0)
                            Process.lvItem.SubItems.Add("Never");
                        else
                            Process.lvItem.SubItems.Add(ThisTime.TotalMinutes.ToString("#,##0") + " min, " + ThisTime.Seconds + " sec.");
                        ListViewGroup ProcessGroup = lvProcesses.Groups[Process.BaseElement.ParentNode.Attributes["Name"].Value];
                        if (ProcessGroup == null)
                            ProcessGroup = lvProcesses.Groups.Add(Process.BaseElement.ParentNode.Attributes["Name"].Value, Process.BaseElement.ParentNode.Attributes["Name"].Value);
                        Process.lvItem.Group = ProcessGroup;
                    }
                    else if (Process.LastExecution.ToOADate() == 0)
                        Process.lvItem.SubItems[1].Text = "Never";
                    else
                        Process.lvItem.SubItems[1].Text = ThisTime.TotalMinutes.ToString("#,##0") + " min, " + ThisTime.Seconds + " sec.";
                    if (Process.ErrorTime == TimeSpan.Zero)
                    {}
                    else if (ThisTime >= Process.ErrorTime)
                        WorstProcess = 2;
                    else if (ThisTime >= Process.WarningTime && WorstProcess != 2)
                        WorstProcess = 1;
                        
                }
                catch (Exception ex)
                {                    
                    Program.LogError("Error processing process completion " + Process.Name + ": " + ex.Message);
                }

            foreach (ColumnHeader col in lvProcesses.Columns)
                col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

            //Update our process viewer
            SetProcessColor(WorstProcess);
        }

        /// <summary>
        /// A thread-safe delegate for setting the worst process color
        /// </summary>
        /// <param name="WorstProcess"></param>
        private delegate void SafeSetProcessColor(int WorstProcess);

        /// <summary>
        /// Set the color of the worst process
        /// </summary>
        /// <param name="WorstProcess"></param>
        private void SetProcessColor(int WorstProcess)
        {
            if (ProcessOverview.InvokeRequired)
                ProcessOverview.BeginInvoke(new SafeSetProcessColor(SetProcessColor), WorstProcess);
            else
            {
                ProcessOverview.ForeColor = (WorstProcess == 0 ? ProcessOverview.Parent.ForeColor : MM_Repository.OverallDisplay.BackgroundColor);
                if (WorstProcess == 0)
                    ProcessOverview.BackColor = ProcessOverview.Parent.BackColor;
                else if (WorstProcess == 1)
                    ProcessOverview.BackColor = Color.Yellow;
                else
                    ProcessOverview.BackColor = Color.Red;
            }
        }

        /// <summary>
        /// Handle the closing of our processing viewer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_Viewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }
        }

        
    }
}
