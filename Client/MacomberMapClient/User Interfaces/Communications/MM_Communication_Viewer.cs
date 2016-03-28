using MacomberMapClient.Integration;
using MacomberMapClient.Properties;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Violations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Communications
{
    /// <summary>
    /// Display the communications state between the server and other components
    /// </summary>
    public partial class MM_Communication_Viewer : MM_Form
    {

        #region Variable declarations
        /// <summary>The key indicators text box showing the summary of communication status</summary>
        public MM_Communication_Status CommStatus;

        /// <summary>The collection of queries running</summary>
        public Dictionary<Type, ListViewItem> Queries = new Dictionary<Type, ListViewItem>();

        /// <summary>The time when the comm viewer was instantiated</summary>
        private DateTime StartTime = DateTime.Now;

        /// <summary>Our collection of list view groups</summary>
        private Dictionary<String, ListViewGroup> Groups = new Dictionary<string, ListViewGroup>(10);

        /// <summary>The UI helper for our object</summary>
        public MM_UserInterface_Helper UIHelper;

        /// <summary>Our collection of Macomber Map servers</summary>
        public Dictionary<Uri, ListViewItem> MMServers = new Dictionary<Uri, ListViewItem>();

        /// <summary>Our UDP listner</summary>
        public UdpClient UdpListener;

        /// <summary>Our communications viewer</summary>
        public static MM_Communication_Viewer CommViewer;
        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the communication viewer
        /// <param name="CommStatus">The key indicators text box showing the summary of communication status</param>
        /// </summary>
        public MM_Communication_Viewer(MM_Communication_Status CommStatus)
        {
            InitializeComponent();
            this.CommStatus = CommStatus;
            this.CommStatus.Tag = this;
            this.Title = "Communications Status - " + Data_Integration.UserName.ToUpper() + " ";
            this.UIHelper = new MM_UserInterface_Helper(ssLower, ssMem, lblCPU);
            tcMain.DrawItem += new DrawItemEventHandler(tcMain_DrawItem);
            tcMain.DrawMode = TabDrawMode.OwnerDrawFixed;
            SetControls();
            this.Visible = false;
            lvMacomberMapServers.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(lvMacomberMapServers, true, null);
            lvQueryStatus.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(lvQueryStatus, true, null);
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
        public static void CreateInstanceInSeparateThread(MM_Communication_Status CommStatus)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(InstantiateForm), CommStatus);
        }

        /// <summary>
        /// Instantiate a comm viewer
        /// </summary>
        /// <param name="state">The state of the form</param>
        private static void InstantiateForm(object state)
        {
            CommViewer = new MM_Communication_Viewer(state as MM_Communication_Status);
            Data_Integration.DisplayForm(CommViewer, Thread.CurrentThread);
        }



        /// <summary>
        /// Assign the data integration layer to the control, and start the query
        /// </summary>
        public void SetControls()
        {
            tmrUpdate.Enabled = true;
            btnSpeech.Text = "Speech " + (Data_Integration.UseSpeech ? "ON" : "OFF");
            MM_Violation_Viewer.SetListViewGroupColor(this.lvComm, Color.FromArgb(0x00cccccc));
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
            MM_Server_Information.UpdateServerInformation(MMServers, lvMacomberMapServers);

            long Mem;
            UIHelper.UpdateCPUAndMemory(out Mem);
            lblUptime.Text = "Uptime: " + MM_Repository.TimeSpanToText(DateTime.Now - Data_Integration.ModelLoadCompletion);


            //Update the status of all of our queries
            ListViewItem FoundListView;
            int WorstColor = 0;
            foreach (KeyValuePair<Type, MM_DataRetrieval_Information> kvp in MM_Server_Interface.DataRetrievalInformation)
            {
                Color DrawColor = kvp.Value.ProperColor();
                if (DrawColor == MM_Repository.OverallDisplay.WarningColor)
                    WorstColor = Math.Max(WorstColor, 1);
                else if (DrawColor == MM_Repository.OverallDisplay.ErrorColor)
                    WorstColor = 2;

                if (!Queries.TryGetValue(kvp.Key, out FoundListView))
                {
                    Queries.Add(kvp.Key, FoundListView = lvQueryStatus.Items.Add(kvp.Key.Name.Replace("MM_", "").Replace('_', ' ')));
                    FoundListView.Tag = kvp.Value;
                    FoundListView.SubItems.Add(MM_Repository.TimeSpanToText(DateTime.Now - kvp.Value.LastUpdate));
                    FoundListView.UseItemStyleForSubItems = true;
                    FoundListView.ForeColor = DrawColor;
                }
                else
                {
                    FoundListView.SubItems[1].Text = MM_Repository.TimeSpanToText(DateTime.Now - kvp.Value.LastUpdate);
                    FoundListView.ForeColor = DrawColor;
                }
            }

            CommStatus.SetQueryAndServerStatus(WorstColor, MM_Server_Interface.Client == null ? 2 : MM_Server_Interface.Client.InnerChannel.State == System.ServiceModel.CommunicationState.Opened ? 0 : 2);
        }

        /// <summary>
        /// Update the log text for mm
        /// </summary>
        private void UpdateLogText()
        {
            while (MM_System_Interfaces.Events.Count > 0)
            {
                if (lbLog.Items.Count > 100)
                    lbLog.Items.RemoveAt(0);
                String InLine = MM_System_Interfaces.Events.Dequeue();
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
            int CurStatus = 0;
            if (CurPage == tpServer)
                if (MM_Server_Interface.Client == null || MM_Server_Interface.Client.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted)
                    CurStatus = 2;
                else if (MM_Server_Interface.Client.InnerChannel.State == System.ServiceModel.CommunicationState.Opening)
                    CurStatus = 1;
                else
                    CurStatus = 0;
            else if (CurPage == tpQueries)
                CurStatus = MM_Communication_Status.QueryStatus;
            else if (CurPage == tpComm)
                CurStatus = MM_Communication_Status.EMSStatus;

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

            this.lblServer.Text = "Server: " + MM_Server_Interface.ClientName;
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
            /*  int QueryStatus = 0;
              if (MM_Server_Interface.Client == null)
                  CommStatus.SetQueryAndServerStatus(QueryStatus, 2);
              else
              {
                  double SecDiff = (DateTime.Now - MM_Server_Interface.Client.LastEMSHeartbeat).TotalSeconds;
                  int ServerStatus = MM_Server_Interface.Client.State != ConnectionState.Open ? 2 : SecDiff > 15 ? 2 : SecDiff > 6 ? 1 : 0;
                  CommStatus.SetQueryAndServerStatus(QueryStatus, ServerStatus);
              }*/
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
            ThreadPool.QueueUserWorkItem(new WaitCallback(MM_System_Interfaces.MessageBoxInSeparateThread), new object[] { lbLog.SelectedItem.ToString(), System.Windows.Forms.Application.ProductName, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information });
        }

        /// <summary>
        /// Handle a double-click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvQueryStatus_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hti = lvQueryStatus.HitTest(e.Location);
            if (hti.Item != null)
            {
                MM_DataRetrieval_Information Retrieval = (MM_DataRetrieval_Information)hti.Item.Tag;
                if (!String.IsNullOrEmpty(Retrieval.RefreshCommand))
                {
                    MethodInfo FoundMethod = typeof(MM_Server_Interface).GetMethod(Retrieval.RefreshCommand);
                    if (FoundMethod == null)
                        MM_System_Interfaces.LogError("Unable to find method to update type {0}", Retrieval.BaseType.Name);
                    else
                        FoundMethod.Invoke(null, null);
                }
            }
        }


        

       

        /// <summary>
        /// When we try and log in to a new server, update accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvMacomberMapServers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hti = lvMacomberMapServers.HitTest(e.Location);
            Exception LoginError;
            if (hti.Item != null)
            {
                try
                {
                    MM_Server_Interface.Client.InnerChannel.Close();
                }
                catch
                { }
                if (MM_Server_Interface.TryLoginAgain(hti.Item.Text, ((MM_Server_Information)hti.Item.Tag).ServerURI, out LoginError))
                    Data_Integration.RestartModel(MM_Server_Interface.Client);
                else
                    MessageBox.Show("Unable to log into Macomber Map Server: " + LoginError.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }


        /// <summary>
        /// Handle a click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleClick(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler(HandleClick), sender, e);
            else
                if (Visible ^= true)
                {
                    Location = new Point(Cursor.Position.X, Cursor.Position.Y + 5);
                    Activate();
                }
        }

        /// <summary>
        /// Handle our object painting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandlePaint(object sender, PaintEventArgs e)
        {
            Rectangle DrawRect = (Rectangle)sender.GetType().GetProperty("Bounds").GetValue(sender);
            Font DrawFont = (Font)sender.GetType().GetProperty("Font").GetValue(sender);
            e.Graphics.Clear(Color.Black);
            MM_Communication_Status.DrawStatus(e.Graphics, 2, (DrawRect.Width - 8) / 4, DrawRect.Height - 4, DrawRect.Width, Font);           
        }
    }
}