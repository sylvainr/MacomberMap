using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using System.Threading;
using Macomber_Map.Data_Elements;
using System.Diagnostics;
using Macomber_Map.Misc._Components;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// Display the communications state between the server and other components
    /// </summary>
    public partial class Communication_Viewer : MM_Form
    {

        #region Variable declarations
        /// <summary>The key indicators text box showing the summary of communication status</summary>
        public Communication_Status CommStatus;    

        /// <summary>The collection of queries running</summary>
        public Dictionary<Guid, ListViewItem> Queries = new Dictionary<Guid, ListViewItem>();
     
        /// <summary>The time when the comm viewer was instantiated</summary>
        private DateTime StartTime = DateTime.Now;

        /// <summary>Our collection of data connectors</summary>
        private Dictionary<Type, ToolStripLabel> DataConnectors = new Dictionary<Type, ToolStripLabel>(6);

        /// <summary>Our collection of list view groups</summary>
        private Dictionary<String, ListViewGroup> Groups = new Dictionary<string, ListViewGroup>(10);
        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the communication viewer
        /// <param name="CommStatus">The key indicators text box showing the summary of communication status</param>
        /// </summary>
        public Communication_Viewer(Communication_Status CommStatus)
        {
            InitializeComponent();
            this.CommStatus = CommStatus;
            this.CommStatus.Tag = this;
            this.Title = "Communications Status - " + Data_Integration.UserName.ToUpper() + " ";            
            tcMain.DrawItem += new DrawItemEventHandler(tcMain_DrawItem);
            tcMain.DrawMode = TabDrawMode.OwnerDrawFixed;
            SetControls();
            this.Visible = false;
        }


        
        /// <summary>
        /// Whent the form is first shown, hide it.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            this.Hide();
            base.OnShown(e);
        }

        /// <summary>
        /// Create a seperate thread to run the communications viewer, and run it.
        /// </summary>
        /// <param name="CommStatus">The communications status label</param>
        /// <returns></returns>
        public static void CreateInstanceInSeparateThread(Communication_Status CommStatus)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(InstantiateForm), CommStatus);
        }

        /// <summary>
        /// Instantiate a comm viewer
        /// </summary>
        /// <param name="state">The state of the form</param>
        private static void InstantiateForm(object state)
        {
            using (Communication_Viewer cView = new Communication_Viewer(state as Communication_Status))
                Data_Integration.DisplayForm(cView, Thread.CurrentThread);
        }

       

        /// <summary>
        /// Assign the data integration layer to the control, and start the query
        /// </summary>
        public void SetControls()
        {
            //DataConnectors.Add(typeof(Historic_Connector), ssHistoric);
            this.ssMM.Text = "MM: " + Data_Integration.MMServerName;            
            this.ssMMS.Text = "MMS: " + Data_Integration.MMSServerName;
            this.ssHist.Text = "Hist: " + Data_Integration.HistoryServerName;
            tmrUpdate.Enabled = true;
            btnSpeech.Text = "Speech " + (Data_Integration.UseSpeech ? "ON" : "OFF");            
            Violation_Viewer.SetListViewGroupColor(this.lvComm, Color.FromArgb(0x00cccccc));          
        }

        /// <summary>
        /// Handle the closing event by instead hiding the window
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
        #endregion

        #region Updating
        /// <summary>
        /// Every interval, update all of our statuses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateDisplay(object sender, EventArgs e)
        {
            if (Data_Integration.CommLoaded)
                UpdateCommunicationsLinkages();
            UpdateMMCommStatus();
            UpdateLogText();
            try
            {
                UpdateServerStatusText();
            }
            catch
            {
                Server_Status_Details_Text = "Server Status: Starting";
                Server_Status_Details_Level = 1;
            }

            Process CurrentProcess = Process.GetCurrentProcess();
            ssMem.Text = "Mem: " + (CurrentProcess.WorkingSet64 / 1024).ToString("#,##0K");
            lblCPU.Text = "CPU: " + ProcessDetails.GetUsage() + "%";
            lblCPU.Invalidate();
            lblUptime.Text = "Uptime: " + (DateTime.Now - StartTime).ToString();

            //Update our queries
            //lock (Data_Integration.MMServer.ElementTypeStatus)
            //    foreach (KeyValuePair<MM_Communication.Elements.MM_Element.enumElementType, MM_Communication.Data_Integration.MM_ElementType_Status> kvp in Data_Integration.MMServer.ElementTypeStatus)
            //        kvp.Value.Update(lvQueryStatus);
            if (Data_Integration.MMServer == null)
                return;

        //    lock (Data_Integration.MMServer.ElementTypeStatus)
        //        if (Data_Integration.MMServer.StudyMode)
        //            foreach (KeyValuePair<MM_Communication.Elements.MM_Element.enumElementType, MM_Communication.Data_Integration.MM_ElementType_Status> kvp in Data_Integration.MMServer.ElementTypeStatus)
        //                kvp.Value.Remove(lvQueryStatus);
        //        else
        //            foreach (KeyValuePair<MM_Communication.Elements.MM_Element.enumElementType, MM_Communication.Data_Integration.MM_ElementType_Status> kvp in Data_Integration.MMServer.ElementTypeStatus)
        //                kvp.Value.Update(lvQueryStatus);
        }

        /// <summary>
        /// Update the log text for mm
        /// </summary>
        private void UpdateLogText()
        {
            while (Program.Events.Count > 0)
            {
                if (lbLog.Items.Count > 20)
                    lbLog.Items.Clear();
                String InLine = Program.Events.Dequeue();
                if (!String.IsNullOrEmpty(InLine))
                    lbLog.Items.Add(InLine);
                lbLog.SelectedIndex = lbLog.Items.Count - 1;
            }
        }


        /// <summary>
        /// Handle the drawing of our tabs, based on our target 
        /// Thanks to http://stackoverflow.com/questions/2107463/is-there-a-way-to-color-tabs-of-a-tabpage-in-winforms for how to selectively color the tabs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcMain_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage CurPage = tcMain.TabPages[e.Index];
            int CurStatus=0;
            if (CurPage == tpServer)
                CurStatus = Server_Status_Details_Level;
            else if (CurPage == tpQueries)
                CurStatus = CommStatus.QueryStatus;
            else if (CurPage == tpComm)
                CurStatus = CommStatus.EMSStatus;

            Rectangle paddedBounds = e.Bounds;            
            paddedBounds.Offset(1, (e.State == DrawItemState.Selected) ? -2 : 1);
            if (CurStatus == 0)
            {
                e.Graphics.FillRectangle(Brushes.LightGreen, e.Bounds);
                TextRenderer.DrawText(e.Graphics, CurPage.Text, this.Font, paddedBounds, Color.Black);
            }
            else if (CurStatus == 1)
            {
                using (SolidBrush sB = new SolidBrush(MM_Repository.OverallDisplay.WarningColor))
                    e.Graphics.FillRectangle(sB, e.Bounds);

                TextRenderer.DrawText(e.Graphics, CurPage.Text, this.Font, paddedBounds, MM_Repository.OverallDisplay.WarningBackground);
            }
            else if (CurStatus == 2)
            {
                using (SolidBrush sB = new SolidBrush(MM_Repository.OverallDisplay.ErrorColor))
                    e.Graphics.FillRectangle(sB, e.Bounds);
                TextRenderer.DrawText(e.Graphics, CurPage.Text, this.Font, paddedBounds, MM_Repository.OverallDisplay.ErrorBackground);
            }                      
        }


        /// <summary>
        /// Update the server status text for server status tab | added 20130607 - mn
        /// </summary>
        private void UpdateServerStatusText()
        {
                if (lbServerStatus.Items.Count > 0)
                    lbServerStatus.Items.Clear();
                lbServerStatus.Items.Add(Server_Status_Details_Text);                
        }

        

        /// <summary>
        /// Get and set the server tab alert level and text for alert level | added 20130607 - mn
        /// </summary>
        public static string Server_Status_Details_Text { get; set; }

        /// <summary>
        /// Detailed server status
        /// </summary>
        public static int Server_Status_Details_Level { get; set; } 

        /// <summary>
        /// Update the linkage status between the core systems and their data connections
        /// </summary>
        private void UpdateCommunicationsLinkages()
        {
            int EMSStatus = 0;




            //First, go through all of our communications links, and show them.
            bool AddedItems = false;
            MM_Communication_Linkage[] Comms = new List<MM_Communication_Linkage>(Data_Integration.Communications.Values).ToArray();
                foreach (MM_Communication_Linkage Comm in Comms)
                    if (Comm.CommStatus != null)
                        Comm.UpdateStatus(ref EMSStatus);
                    else
                    {
                        AddedItems = true;                                                
                        ListViewGroup ThisGroup;
                        if (!Groups.TryGetValue(Comm.Group, out ThisGroup))
                            Groups.Add(Comm.Group, ThisGroup = lvComm.Groups.Add(Comm.Group, Comm.Group));
                                                
                        Comm.CommStatus = lvComm.Items.Add(Comm.Name);
                        Comm.CommStatus.Group = ThisGroup;
                        Comm.CommStatus.ToolTipText = Comm.SCADAName;
                        if (Comm.Name == "Overall")                        
                            Comm.CommStatus.Text = Comm.Group;                        
                        else if (Comm.Critical)
                        {
                            Comm.CommStatus.Font = new Font(Comm.CommStatus.Font, FontStyle.Bold | FontStyle.Underline);                            
                            Comm.CommStatus.Text = Comm.Name;
                        }
                        else
                            Comm.CommStatus.Text = Comm.Name;                        
                        Comm.CommStatus.ForeColor = Color.White;                                                
                    }

            CommStatus.SetEMSStatus(EMSStatus);


            //Now, if we have new linkages, let's clear everything and redo.
            if (AddedItems)
                using (Graphics g = Graphics.FromHwnd(lvComm.Handle))
                {

                    Size MaxSize = Size.Empty;
                    foreach (MM_Communication_Linkage Comm in Comms)
                    {
                        Size ThisSize = Size.Ceiling(g.MeasureString(Comm.CommStatus.Text, Comm.CommStatus.Font));
                        MaxSize.Width = Math.Max(MaxSize.Width, ThisSize.Width);
                        MaxSize.Height = Math.Max(MaxSize.Height, ThisSize.Height);
                    }
                    lvComm.TileSize = new Size(MaxSize.Width + 5, MaxSize.Height + 5);
                }

            this.ssMM.Text = "MM: " + Data_Integration.MMServerName;
            this.ssEMS.Text = "EMS: " + (Data_Integration.MMServer == null ? "?" : Data_Integration.MMServer.EMSConsoleName);
            this.ssMMS.Text = "MMS: " + Data_Integration.MMSServerName;
            this.ssHist.Text = "Hist: " + Data_Integration.HistoryServerName;
        }

        private int CompareLabels(Label Label1, Label Label2)
        {
            if (Label1 == Label2)
                return 0;
            else if (Label1.Name == "Overall")
                return -1;
            else if (Label2.Name == "Overall")
                return 1;
            else
                return String.Compare(Label1.Name, Label2.Name);
        }

        /// <summary>
        /// Update a label based on the data connector's state
        /// </summary>
        /// <param name="State"></param>
        /// <param name="Label"></param>
        /// <param name="QueryStatus"></param>
        private void CheckState(ConnectionState State, ToolStripLabel Label, ref int QueryStatus)
        {
            if (State == ConnectionState.Open)
            {
                Label.BackColor = MM_Repository.OverallDisplay.BackgroundColor;
                Label.ForeColor = MM_Repository.OverallDisplay.ForegroundColor;
            }
            else if (State == ConnectionState.Connecting)
            {
                Label.BackColor = MM_Repository.OverallDisplay.WarningColor;
                Label.ForeColor = MM_Repository.OverallDisplay.WarningBackground;
                QueryStatus = Math.Max(QueryStatus, 1);
            }
            else
            {
                Label.BackColor = MM_Repository.OverallDisplay.ErrorColor;
                Label.ForeColor = MM_Repository.OverallDisplay.ErrorBackground;
                QueryStatus = Math.Max(QueryStatus, 2);
            }
        }


        /// <summary>
        /// Update the communications status between MM and the outside
        /// </summary>
        private void UpdateMMCommStatus()
        {

            //First position our query list            
            /*
            lvQueries.Top = grpEMS.Bottom + 10;
            lvQueries.Width = grpEMS.Width;

            if (lvQueries.Items.Count > 0)
                lvQueries.Height = lvQueries.Items[lvQueries.Items.Count - 1].Bounds.Bottom + 5;
            */
            int QueryStatus = 0;
            if (Data_Integration.MMServer == null)
                CommStatus.SetQueryAndServerStatus(QueryStatus, 2);
            else
            {
                double SecDiff = (DateTime.Now - Data_Integration.MMServer.LastEMSHeartbeat).TotalSeconds;
                int ServerStatus = Data_Integration.MMServer.State != ConnectionState.Open ? 2 : SecDiff > 15 ? 2 : SecDiff > 6 ? 1 : 0;
                CommStatus.SetQueryAndServerStatus(QueryStatus, ServerStatus);
            }
        }
        #endregion

     


        /// <summary>
        /// Run the garbage collector upon request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ssMem_DoubleClick(object sender, EventArgs e)
        {
            GC.Collect(2);
        }

        /// <summary>
        /// Update the speech preferences
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSpeech_Click(object sender, EventArgs e)
        {
            Data_Integration.UseSpeech ^= true;
            btnSpeech.Text = "Speech " + (Data_Integration.UseSpeech ? "ON" : "OFF");
        }

       
        /// <summary>
        /// Report the status of our communications window
        /// </summary>
        /// <returns></returns>
        public string ReportCommStatus()
        {
            StringBuilder sB = new StringBuilder();

            List<String> WarningComm = new List<string>();
            List<String> ErrorComm = new List<string>();            
            
            MM_Communication_Linkage[] Comms = new List<MM_Communication_Linkage>(Data_Integration.Communications.Values).ToArray();
            foreach (MM_Communication_Linkage Comm in Comms)
                if (Comm.CommStatus != null)
                    if (Comm.ConnectionState == MM_Communication_Linkage.ConnectionStateEnum.Bad)
                        ErrorComm.Add(Comm.Name);
                    else if (Comm.ConnectionState == MM_Communication_Linkage.ConnectionStateEnum.Unknown)
                        WarningComm.Add(Comm.Name);

            if (ErrorComm.Count > 0)
            {
                ErrorComm.Sort();
                sB.AppendLine("Down: " + String.Join(", ", ErrorComm.ToArray()));
            }

            if (WarningComm.Count > 0)
            {
                WarningComm.Sort();
                sB.AppendLine("Unknown: " + String.Join(", ", WarningComm.ToArray()));
            }


                
                    


            return sB.ToString();
        }

        /// <summary>
        /// When we double-click on an item, pop up a message box w/ needed info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbLog_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(Program.MessageBoxInSeparateThread),new object[] { lbLog.SelectedItem.ToString(), Application.ProductName, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information });
        }

        

        
            
    }
}