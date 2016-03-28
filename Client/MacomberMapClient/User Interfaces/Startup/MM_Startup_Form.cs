using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.SystemInformation;
using MacomberMapClient.Integration;
using MacomberMapClient.Properties;
using MacomberMapClient.User_Interfaces.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Windows;

namespace MacomberMapClient.User_Interfaces.Startup
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This form provides the startup for to the main application
    /// </summary>
    public partial class MM_Startup_Form : Form
    {
        #region Variable declarations
        /// <summary>An indication when Macomber Map is </summary>
        public bool MapReady = false;

        /// <summary>Our file system watcher to detect new savecases</summary>
        public FileSystemWatcher SavecaseWatcher;

        /// <summary>Our collection of savecases</summary>
        public Dictionary<String, ListViewItem> Savecases = new Dictionary<string, ListViewItem>();

        /// <summary>The unique ID of our client</summary>
        public int ClientId;

        /// <summary>The newly created Macomber Map</summary>
        public MacomberMap_Form Map;

        /// <summary>The coordinates to be used in starting up the map</summary>
        public static MM_Coordinates Coordinates;

        /// <summary>Our callback to start up the map</summary>
        private WaitCallback MapStart;

        /// <summary>Our delegate for starting up the server connection</summary>
        private Data_Integration.StartupModelDelegate StartupDelegate;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our application
        /// </summary>
        public MM_Startup_Form()
        {
            InitializeComponent();
            this.lblMMVersion.Text = "v" + Application.ProductVersion;
            tpProgress.Hide();
            lvMacomberMapServers.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(lvMacomberMapServers, true, null);            
        }



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
        /// When our form is shown, start our UDP listner
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SavecaseWatcher = new FileSystemWatcher(Settings.Default.SavecaseDirectory, "*.mm_savecase");
            SavecaseWatcher.Changed += SavecaseWatcher_Changed;
            SavecaseWatcher.Created += SavecaseWatcher_Changed;
            SavecaseWatcher.Deleted += SavecaseWatcher_Changed;
            SavecaseWatcher.Renamed += SavecaseWatcher_Renamed;
            SavecaseWatcher.EnableRaisingEvents = true;
            foreach (FileInfo fI in new DirectoryInfo(SavecaseWatcher.Path).GetFiles(SavecaseWatcher.Filter))
                SavecaseWatcher_Changed(SavecaseWatcher, new FileSystemEventArgs(WatcherChangeTypes.Created, fI.DirectoryName, fI.Name));

            if (lvSavecases.Items.Count == 0)
                lvSavecases.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            else
                lvSavecases.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

        }


        #endregion

        #region Savecase handling
        /// <summary>
        /// Handle a change to savecase information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SavecaseWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileInfo fI = new FileInfo(e.FullPath);
            ListViewItem FoundCase;
            Savecases.TryGetValue(e.Name, out FoundCase);

            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                if (FoundCase != null)
                {
                    lvSavecases.Items.Remove(FoundCase);
                    Savecases.Remove(e.Name);
                }
            }
            else if (FoundCase == null)
            {
                Savecases.Add(e.Name, FoundCase = lvSavecases.Items.Add(Path.GetFileNameWithoutExtension(e.Name)));
                FoundCase.SubItems.Add(GetFileSize(fI.Length));
                FoundCase.SubItems.Add(fI.LastWriteTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat));
            }
            else
            {
                FoundCase.SubItems[1].Text = GetFileSize(fI.Length);
                FoundCase.SubItems[2].Text = fI.LastWriteTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat);
            }
        }

        /// <summary>
        /// Handle savecase renames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SavecaseWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            FileInfo fI = new FileInfo(e.FullPath);
            ListViewItem FoundCase;
            if (!Savecases.TryGetValue(e.OldName, out FoundCase))
                SavecaseWatcher_Changed(sender, e);
            else
            {
                FoundCase.Text = e.Name;
                FoundCase.SubItems[1].Text = GetFileSize(fI.Length);
                FoundCase.SubItems[2].Text = fI.LastWriteTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat);
                Savecases.Remove(e.OldName);
                Savecases.Add(e.Name, FoundCase);
            }
        }

        /// <summary>
        /// Retrieve the file size in an easy to read format
        /// </summary>
        /// <param name="Length"></param>
        /// <returns></returns>
        private String GetFileSize(long Length)
        {
            if (Length >= 1024 * 1024 * 1024)
                return (Length / (1024 * 1024 * 1024)).ToString("#,##0") + " Gb";
            else if (Length >= 1024 * 1024)
                return (Length / (1024 * 1024)).ToString("#,##0") + " Mb";
            else if (Length > 1024)
                return (Length / 1024).ToString("#,##0") + " kb";
            else
                return Length.ToString("#,##0") + " b";
        }
        #endregion

        #region Server connectivity
        /// <summary>
        /// Handle a mouse double-click on an item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvMacomberMapServers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hti = lvMacomberMapServers.HitTest(e.Location);
            if (hti.Item != null)
                LoginToServer(hti.Item.Text, ((MM_Server_Information)hti.Item.Tag).ServerURI);
        }

        /// <summary>
        /// Handle the completion of our login
        /// </summary>
        /// <param name="Resp"></param>
        private void HandleServerLoginCompletion(IAsyncResult Resp)
        {
            if (StartupDelegate.EndInvoke(Resp))
            {
                FixCoordinatesNull();
                Coordinates.SetTopLeftAndBottomRight(MM_Repository.Counties["STATE"].Min, MM_Repository.Counties["STATE"].Max);
               
                this.DialogResult = DialogResult.OK;
            }
            else
                MessageBox.Show("Unable to log into Macomber Map Server. Please try again. ", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        #region Application startup
        /// <summary>
        /// Create a new Macomber Map instance
        /// </summary>
        public void CreateMap()
        {
            FixCoordinatesNull();
            //Coordinates.SetTopLeftAndBottomRight(MM_Repository.Counties["STATE"].Min, MM_Repository.Counties["STATE"].Max);
        }

        internal void FixCoordinatesNull()
        {
            if (Coordinates == null)
            {
                // TODO: Fix this boundary issue
                Coordinates = new MM_Coordinates(-105.589265200513f, 44.5322075f, -86.09185f, 29.2118549f, 4);
                //Coordinates = new MM_Coordinates(MM_Repository.Counties["STATE"].Min, MM_Repository.Counties["STATE"].Max, Data_Integration.Permissions.MinZoom);
            }
        }

        /// <summary>
        /// Wait for MM to be online and ready
        /// </summary>
        public void WaitForMap()
        {
            if (MapStart == null)
                return;
            ThreadPool.QueueUserWorkItem(MapStart, new object[] { Coordinates, this });
            while (!this.MapReady)
            {
                Thread.Sleep(500);
                Application.DoEvents();
            }
        }

        /// <summary>
        /// When our visibility changes, hide our form
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!this.Visible)
                tmrUpdate.Stop();
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


        #endregion

        Dictionary<Uri, ListViewItem> MMServers = new Dictionary<Uri, ListViewItem>();

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
            {
                while (MM_System_Interfaces.Events.Count > 0)
                    LogEvent(MM_System_Interfaces.Events.Dequeue());

                MM_Server_Information.UpdateServerInformation(MMServers, lvMacomberMapServers);


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
        /// Connect to our optimal server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOptimalServer_Click(object sender, EventArgs e)
        {
            MM_Server_Information BestServer = null;
            lock (MM_Server_Interface.MMServers)
            {
            foreach (MM_Server_Information ServerInfo in MM_Server_Interface.MMServers.Values.ToArray())
                if (BestServer == null || ServerInfo.UserCount < BestServer.UserCount)
                    BestServer = ServerInfo;
            }
            if (BestServer != null)
                LoginToServer(BestServer.ServerName, BestServer.ServerURI);
        }

        /// <summary>
        /// Log in to a server
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="ConnectionUri"></param>
        private void LoginToServer(String Title, Uri ConnectionUri)
        {
            frmWindowsSecurityDialog SecurityDialog = new frmWindowsSecurityDialog();
            SecurityDialog.CaptionText = Application.ProductName + " " + Application.ProductVersion;
            SecurityDialog.MessageText = "Enter the credentials to log into Macomber Map Server " + Title;
            string Username, Password, Domain;
            Exception LoginError;

            if (SecurityDialog.ShowLoginDialog(out Username, out Password, out Domain))
            {
                if (MM_Server_Interface.TryLogin(Title, ConnectionUri, Username, Password, out LoginError))
                {
                    tpProgress.Show();
                    tcMain.SelectedTab = tpProgress;

                    StartupDelegate = new Data_Integration.StartupModelDelegate(Data_Integration.StartupModel);
                    IAsyncResult Resp = StartupDelegate.BeginInvoke(MM_Server_Interface.Client, this, false, HandleServerLoginCompletion, null);


                    MM_Repository.user = Username;
                    MM_Repository.pw = Password;

                } else
                    MessageBox.Show("Unable to log into Macomber Map Server: " + LoginError.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);



            }
        }



    }
}
