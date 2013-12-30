using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Macomber_Map.Data_Connections;
using Macomber_Map.Data_Connections.Generic;
using Macomber_Map.Data_Elements;
namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class holds the start-up form, which will determine the appropriate databases and their parameters
    /// </summary>
    public partial class Startup_Form : Form
    {
        #region Variable declarations
        /// <summary>An indication when Macomber Map is </summary>
        public bool MapReady = false;

        /// <summary>The newly created Macomber Map</summary>
        public MacomberMap Map;

        /// <summary>The full list of available connectors</summary>
        private DataTable Connectors;

        /// <summary>The ConnectionAttempt counter to track what 'ok' press .this is from to ensure old threds are closed properly</summary>
        private int ConnectionAttempt = 0;
   
        /// <summary>The coordinates to be used in starting up the map</summary>
        public MM_Coordinates Coordinates = new MM_Coordinates(-98.7919159f, 29.9901752f, -97.41264f, 31.3694439f, 16f);

        /// <summary>Our callback to start up the map</summary>
        private WaitCallback MapStart;
        #endregion

        /// <summary>
        /// Offer a startup dialog
        /// </summary>
        /// <param name="MapStart"></param>
        public DialogResult ShowDialog(WaitCallback MapStart)
        {
            this.MapStart = MapStart;
            return ShowDialog();
        }

        /// <summary>
        /// Initialize a new startup form
        /// </summary>
        public Startup_Form()
        {

            InitializeComponent();
            this.txtUserName.Text = Environment.UserName;
            this.lblMMVersion.Text = "v" + Application.ProductVersion;
            cmbMMServer.Sorted = cmbMMSServer.Sorted = cmbHistoryServer.Sorted = cmbHistoryServer2.Sorted=true;

            //Build our table of data sources
            Data_Integration.DataSources = new DataTable("DataSources");
            Data_Integration.DataSources.Columns.Add("Name", typeof(string));
            Data_Integration.DataSources.Columns.Add("Value", typeof(MM_Data_Source));
            chkLog.Checked = Program.ErrorLog != null;
            chkLog.CheckedChanged += chkLog_CheckedChanged;
            cmbDataSource.DataSource = Data_Integration.DataSources;
            cmbDataSource.DisplayMember = "Name";
            cmbDataSource.ValueMember = "Value";

            //Now build our table of data connectors.
            Connectors = new DataTable("Connectors");
            Connectors.Columns.Add("Type", typeof(string));
            Connectors.Columns.Add("Name", typeof(string));
            Connectors.Columns.Add("Default", typeof(bool));
            Connectors.Columns.Add("Configuration", typeof(XmlElement));
            foreach (String ToBlank in "MM_Server_Connector,MMS_Connector,MM_Historic_Connector,CIMServer_Connector".Split(','))
                Connectors.Rows.Add(ToBlank, "", false, null);
            if (MM_Repository.xConfiguration != null)
                foreach (XmlElement xE in MM_Repository.xConfiguration["Configuration"]["Databases"].ChildNodes)
                    if (xE.Name == "MMServer")
                        Connectors.Rows.Add("MM_Server_Connector", xE.Attributes["Name"].Value, (xE.HasAttribute("Default") ? XmlConvert.ToBoolean(xE.Attributes["Default"].Value) : false), xE);
                    else if (xE.Name == "Database")
                        Connectors.Rows.Add(xE.Attributes["Type"].Value, xE.Attributes["Name"].Value, (xE.HasAttribute("Default") ? XmlConvert.ToBoolean(xE.Attributes["Default"].Value) : false), xE);


            //Now assign the table's types as appropriate
            cmbMMServer.DataSource = new DataView(Connectors, "Type='" + "MM_Server_Connector'", "Name ASC", DataViewRowState.CurrentRows);
            cmbMMSServer.DataSource = new DataView(Connectors, "Type='" + "MMS_Connector'", "Name ASC", DataViewRowState.CurrentRows);
            cmbHistoryServer.DataSource = new DataView(Connectors, "Type='" + "MM_Historic_Connector'", "Name ASC", DataViewRowState.CurrentRows);
            cmbHistoryServer2.DataSource = new DataView(Connectors, "Type='" + "MM_Historic_Connector'", "Name ASC", DataViewRowState.CurrentRows);
            cmbMMSServer.DisplayMember = cmbHistoryServer.DisplayMember = cmbHistoryServer2.DisplayMember = cmbMMServer.DisplayMember = "Name";
            cmbMMSServer.ValueMember = cmbHistoryServer.ValueMember = cmbMMServer.ValueMember = "Configuration";

            //Now check to see if we have defaults
            foreach (DataRow dr in Connectors.Rows)
                if (Convert.ToBoolean(dr["Default"]))
                    if ((string)dr["Type"] == "MM_Server_Connector")
                        cmbMMServer.SelectedItem = dr;
                    else if ((string)dr["Type"] == "MMS_Connector")
                        cmbMMSServer.SelectedItem = dr;
                    else if ((string)dr["Type"] == "MM_Historic_Connector")
                    {
                        cmbHistoryServer.SelectedItem = dr;
                        cmbHistoryServer2.SelectedItem = dr;
                    }
            if (MM_Repository.xConfiguration != null && MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/MMServer[@Type='MM_Server_Connector' and @Default='true']") != null)
                cmbMMServer.Text = MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/MMServer[@Type='MM_Server_Connector' and @Default='true']").Attributes["Name"].Value;
            if (MM_Repository.xConfiguration != null && MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/Database[@Type='MMS_Connector' and @Default='true']") != null)
                cmbMMSServer.Text = MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/Database[@Type='MMS_Connector' and @Default='true']").Attributes["Name"].Value;
            if (MM_Repository.xConfiguration != null && MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/Database[@Type='MM_Historic_Connector' and @Default='true']") != null)
            {
                cmbHistoryServer.Text = MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/Database[@Type='MM_Historic_Connector' and @Default='true']").Attributes["Name"].Value;
                cmbHistoryServer2.Text = MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/Database[@Type='MM_Historic_Connector' and @Default='true']").Attributes["Name"].Value;
            }
            
            //Alter the display if a public user is presented
            if (Data_Integration.Permissions.PublicUser)
            {
                lblMMS.Visible = false;
                cmbMMSServer.Visible = false;
                lblHistoryServer.Visible = false;
                cmbHistoryServer.Visible = false;
                lblDisplay.Visible = false;
                cmbNetworkMap.Visible = false;
                lblDataSource.Visible = false;
                cmbDataSource.Visible = false;
                chkLog.Visible = false;
                chkSpeakViolations.Visible = false;
                chkWeatherData.Visible = false;
                chkPipe.Visible = false;
                chkRecordTelemetryChanges.Visible = false;
                tabPages.TabPages.Remove(tabSavecase);
                this.Height = 275;
                tabPages.Size = new Size(291, 160);
                btnOK.Location = new System.Drawing.Point(45, 101);
            }

            //Set the Load savecase tabpage in this open source version
            this.tabPages.SelectedIndex = 1;
        }

        /// <summary>
        /// Create a new Macomber Map instance
        /// </summary>
        public void CreateMap()
        {                       
            Coordinates.SetTopLeftAndBottomRight(MM_Repository.Counties["STATE"].Min, MM_Repository.Counties["STATE"].Max);        
        }

        /// <summary>
        /// Wait for MM to be online and ready
        /// </summary>
        public void WaitForMap()
        {
            if (MapStart == null)
                return;
            ThreadPool.QueueUserWorkItem(MapStart, new object[] { Coordinates, this});
            while (!this.MapReady)
            {
                Thread.Sleep(500);
                Application.DoEvents();
            }
        }

        /// <summary>
        /// When our form closes, kill the processes if our login wasn't completely successful.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
                Process.GetCurrentProcess().Kill();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// If we're set to only use defaults, hide everything and simulate the connect button.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            this.cmbNetworkMap.SelectedIndex = this.cmbNetworkMap2.SelectedIndex = 1;
            this.Hide();
            if (!String.IsNullOrEmpty(Data_Integration.Permissions.BoilerplateText))
                if (MessageBox.Show(this,Data_Integration.Permissions.BoilerplateText, Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }
            this.Show();            
            this.BringToFront();
            this.Focus();
            if (Data_Integration.Permissions.DefaultConnectors)
            {
                tabPages.TabPages.Remove(tabDataConnections);
                tabPages.TabPages.Remove(tabSavecase);                
                btnOK_Click(this, null);
            }
            txtPassword.Focus();
        }

        /// <summary>
        /// Sign in automatically with username and password
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        public void SignIn(String UserName, String Password)
        {
            txtUserName.Text = UserName;
            txtPassword.Text = Password;
            btnOK_Click(btnOK, EventArgs.Empty);
        }

        /// <summary>
        /// Report the bounds of components needed for logging in.
        /// </summary>
        public Rectangle[] LoginBounds
        {
            get
            {
                List<Rectangle> OutRect = new List<Rectangle>();
                foreach (Control c in new Control[] { txtUserName, txtPassword, btnOK })
                {
                    Control d = c.Parent;
                    Rectangle r = c.Bounds;
                   /* while (d != null)
                    {
                        r.X += d.Left;
                        r.Y += d.Top;
                        d = d.Parent;
                    }*/
                    OutRect.Add(r);
                }
                return OutRect.ToArray();
            }
        }

        /// <summary>        
        /// Handle the user clicking the 'connect' button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            ConnectionAttempt++;
            Data_Integration.DisplayAlarms = chkDisplayAlarmsStart.Checked;
      
            //Set our network data source
            Data_Integration.NetworkSource = cmbDataSource.SelectedItem as MM_Data_Source;
            
            if (cmbNetworkMap.Text == "DirectX")
                MM_Repository.RenderMode = Macomber_Map.Data_Elements.MM_Repository.enumRenderMode.DirectX;
            else if (cmbNetworkMap.Text == "GDI+")
            {
                MM_Repository.RenderMode = Macomber_Map.Data_Elements.MM_Repository.enumRenderMode.GDIPlus;
                MM_Repository.AdaptiveRendering = false;
            }
            else if (cmbNetworkMap.Text == "GDI+ (Adaptive Rendering")
            {
                MM_Repository.StartCPUMonitor();
                MM_Repository.AdaptiveRendering = true;
            }
            else if (cmbNetworkMap.Text == "WPF")
                MM_Repository.RenderMode = Macomber_Map.Data_Elements.MM_Repository.enumRenderMode.WPF;
            

            //First, try and log in.

             XmlElement xMMServer = (XmlElement)MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/MMServer[@Name='" + cmbMMServer.Text + "']");
            //XmlElement xMMServer = (XmlElement)MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/Database[@Name='" + cmbMMServer.Text + "']");
             MM_Server_Connector MMServer = new MM_Server_Connector(xMMServer, txtUserName.Text, txtPassword.Text, cmbDataSource.Text, ConnectionAttempt);
            try
            {
                tabPages.SelectedTab = tabProgress;
                lstProgress.Items.Add("Initializing connection to MM Server.");
                MMServer.Initialize();

                lstProgress.Items.Add("Connecting to MM Server.");
                MMServer.Connect();

                lstProgress.Items.Add("Logging in to MM Server.");
                MMServer.LogIn();
            }
            catch (Exception ex)
            {
                lstProgress.Items.Add("Error logging into MM server: " + ex.Message);
                tabPages.SelectedIndex = 0;
                MessageBox.Show("Unable to log in: " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (ex.Equals("Not able to connect to any servers")) //fix race condition??
                {
                    MMServer.Close(1);
                    return;
                }
                MMServer.Close(0);
                return;
            }


            tabPages.SelectedTab = tabProgress;
            bool tryAgain = true;
            while (tryAgain)
                try
                {
                    //if (chkLog.Checked)
                    //    Data_Integration.StartupModel(cmbCIMServer.Text, null, null, null, chkWeatherData.Checked, this, (cmbDataSource.SelectedItem as DataRowView)["Value"] as MM_Data_Source);
                    //else
                        Data_Integration.StartupModel(MMServer,cmbMMServer.Text, cmbMMSServer.Text, cmbHistoryServer.Text, chkWeatherData.Checked, this, (cmbDataSource.SelectedItem as DataRowView)["Value"] as MM_Data_Source, txtUserName.Text, txtPassword.Text, chkRecordTelemetryChanges.Checked);

                    tryAgain = false;
                    this.DialogResult = DialogResult.OK;                    
                    this.Close();
                }
                catch (Exception ex)
                {
                    DialogResult Resp = Program.MessageBox( "Error starting up Macomber Map. Would you like to retry?\n" + ex.Message + "\n" + ex.StackTrace, Application.ProductName, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                    if (Resp == DialogResult.Retry)
                        tryAgain = true;
                    else if (Resp == DialogResult.Abort)
                    {
                        this.DialogResult = DialogResult.Abort;
                        this.Close();
                    }
                    else if (Resp == DialogResult.Ignore)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }            
        }

        private delegate void SafeLogEvent(String EventText);

        /// <summary>
        /// Log an event to the progess bar
        /// </summary>
        /// <param name="EventText">The text of the event</param>
        public void LogEvent(String EventText)
        {                       
            if (lstProgress.InvokeRequired)
                lstProgress.BeginInvoke(new SafeLogEvent(LogEvent), EventText);
            else
            {                
                lstProgress.SelectedIndex = lstProgress.Items.Add(EventText);
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Load a one-line from savecase
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSavecase_Click(object sender, EventArgs e)
        {
            String OneLineDirectory = null;
            String InResult = new ThreadSafeOpenFileDialog().ShowDialog(ref OneLineDirectory);
            if (!String.IsNullOrEmpty(InResult))
            {
                tabPages.SelectedTab = tabProgress;                
                if (cmbNetworkMap2.Text == "DirectX")
                    MM_Repository.RenderMode = Macomber_Map.Data_Elements.MM_Repository.enumRenderMode.DirectX;
                else if (cmbNetworkMap.Text == "GDI+")
                {
                    MM_Repository.RenderMode = Macomber_Map.Data_Elements.MM_Repository.enumRenderMode.GDIPlus;
                    MM_Repository.AdaptiveRendering = false;
                }
                else if (cmbNetworkMap.Text == "GDI+ (Adaptive Rendering")
                {
                    MM_Repository.StartCPUMonitor();
                    MM_Repository.AdaptiveRendering = true;
                }
                else if (cmbNetworkMap2.Text == "WPF")
                    MM_Repository.RenderMode = Macomber_Map.Data_Elements.MM_Repository.enumRenderMode.WPF;
                Data_Integration.StartupSavecase(InResult, this, OneLineDirectory, cmbHistoryServer2.Text);
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }


        /// <summary>
        /// This class provides a thread-safe way of offering a open file dialog
        /// </summary>
        public class ThreadSafeOpenFileDialog
        {
            /// <summary>The thread in which the dialog is presented</summary>
            private Thread OpenFileDialogThread;

            /// <summary>The outgoing path of the query</summary>
            public String ResultPath = "";

            /// <summary>The path of the one-lines</summary>
            public String ResultOneLineDirectory = "";

            /// <summary>The outgoing result of the query</summary>
            public DialogResult Result = DialogResult.None;

            /// <summary>
            /// Show the open file dialog, and return the path (if selected - none if not)
            /// </summary>
            /// <returns>The result of the query</returns>
            /// <param name="ResultOneLineDirectory"></param>
            public string ShowDialog(ref string ResultOneLineDirectory)
            {
                OpenFileDialogThread = new Thread(PresentDialog);
                OpenFileDialogThread.SetApartmentState(ApartmentState.STA);
                OpenFileDialogThread.Name = "Open File Dialog thread";
                OpenFileDialogThread.Start();
                while (Result == DialogResult.None)
                    Application.DoEvents();                
                OpenFileDialogThread = null;
                    ResultOneLineDirectory = this.ResultOneLineDirectory;
                return ResultPath;

            }

            /// <summary>
            /// Present the dialog in its own thread.
            /// </summary>
            [STAThread]
            private void PresentDialog()
            {
                using (Form TestForm = new Form())
                using (OpenFileDialog o = new OpenFileDialog())
                {
                    o.Title = "Open up a Macomber Map savecase";
                    o.Filter = "Macomber Map Savecase (*.MM_Model)|*.MM_Model|Xml file (*.xml)|*.xml|All files (*.*)|*.*";
                    o.CheckFileExists = true;
                    o.CheckPathExists = true;
                    o.Multiselect = false;
                    o.ReadOnlyChecked = true;
                    o.RestoreDirectory = true;
                    TestForm.TopMost = true;
                    o.ShowReadOnly = true;
                    o.SupportMultiDottedExtensions = true;

                    DialogResult QueryResponse = o.ShowDialog(TestForm);
                    if (QueryResponse == DialogResult.OK)
                    {
                        ResultPath = o.FileName;
                        if (Directory.Exists(Path.Combine(Path.GetDirectoryName(o.FileName), "MacomberMapXml")))
                            ResultOneLineDirectory = Path.Combine(Path.GetDirectoryName(o.FileName), "MacomberMapXml");
                        else
                        {
                            o.Title = "Select a one-line to choose the one-line directory";
                            o.Filter = "MM One-Line (*.MM_OneLine)|*.MM_OneLine";
                            if (o.ShowDialog() == DialogResult.OK)
                                ResultOneLineDirectory = Path.GetDirectoryName(o.FileName);
                        }
                    }
                    else
                        ResultPath = null;
                    Result = QueryResponse;
                }
            }
        }

        /// <summary>
        /// Update the list of data sources, by providing the list of available data connections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateDataSources(object sender, EventArgs e)
        {
            //Build up our list of data sources
            Data_Integration.DataSources.Rows.Clear();
            Data_Integration.DataSources.Rows.Add();
            Data_Integration.DataSources.Clear();
            foreach (ComboBox CmbToPoll in new ComboBox[] { cmbMMServer, cmbMMSServer, cmbHistoryServer })
                if (CmbToPoll.SelectedItem != null && (CmbToPoll.SelectedItem as DataRowView)["Configuration"] != DBNull.Value)
                    foreach (XmlNode xCh in ((CmbToPoll.SelectedItem as DataRowView)["Configuration"] as XmlElement).ChildNodes)
                        if (xCh.Name == "DataSource")
                        {
                            MM_Data_Source NewSource = new MM_Data_Source(xCh as XmlElement, MM_Type_Finder.LocateType((CmbToPoll.SelectedItem as DataRowView)["Type"] as String, null),true);                            
                            Data_Integration.DataSources.Rows.Add(NewSource.Name, NewSource);                            
                        }

            for (int a=0; a < cmbDataSource.Items.Count; a++)
            {                
                MM_Data_Source ThisSource = (cmbDataSource.Items[a] as DataRowView)["Value"] as MM_Data_Source;
                if (ThisSource != null && ThisSource.Default)
                    cmbDataSource.SelectedIndex = a;
            }
        }

        /// <summary>
        /// Update our speech preferences
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSpeakViolations_CheckedChanged(object sender, EventArgs e)
        {
            Data_Integration.UseSpeech = chkSpeakViolations.Checked;
        }

        /// <summary>
        /// Handle the update timer by checking for new events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (Data_Integration.InitializationComplete)
                tmrUpdate.Enabled = false;
            else
                while (Program.Events.Count > 0)
                    LogEvent(Program.Events.Dequeue());

        }

        /// <summary>
        /// Handle the updating of the log status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkLog_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLog.Checked)
                Program.StartLog();
            else if (Program.ErrorLog != null)
            {
                Program.ErrorLog.Close();
                Program.ErrorLog.Dispose();
                Program.ErrorLog = null;
            }
        }

        /// <summary>
        /// When the pipe checkbox is changed, update the integration information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkPipe_CheckedChanged(object sender, EventArgs e)
        {
            Data_Integration.UsePipe = chkPipe.Checked;
        }

        /// <summary>
        /// If the user clicks on the about window, show our dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picLogo_Click(object sender, EventArgs e)
        {
            new About_Form().ShowDialog(this);
        }

        /// <summary>
        /// Remove sensitive information from our model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemoveSensitiveInformation_Click(object sender, EventArgs e)
        {
            Thread SensitiveThread = new Thread(new ThreadStart(delegate
            {
                using (OpenFileDialog oFd = new OpenFileDialog())
                using (OpenFileDialog oFd2 = new OpenFileDialog())
                {
                    oFd.Title = "Select a Macomber Map model";
                    oFd.Filter = "Macomber Map Model (*.MM_Model)|*.MM_Model";
                    oFd2.Title = "Select a list of words (one per line)";
                    oFd2.Filter = "All files (*.*)|*.*";
                    if (oFd.ShowDialog() == System.Windows.Forms.DialogResult.OK && oFd2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        DateTime StartTime = DateTime.Now;
                        ModelRandomizer.RandomizeNetworkModel(oFd.FileName, oFd2.FileName);
                        MessageBox.Show("Completed randomization in " + (DateTime.Now - StartTime).ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }));
            SensitiveThread.SetApartmentState(ApartmentState.STA);
            SensitiveThread.Start();
        }

    


    }
}