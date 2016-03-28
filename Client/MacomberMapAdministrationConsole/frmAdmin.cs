using MacomberMapAdministrationConsole.Properties;
using MacomberMapAdministrationService;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.WCF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapAdministrationConsole
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides our administraton form
    /// </summary>
    public partial class frmAdmin : Form
    {
        #region Variable declarations
        /// <summary>Our collection of Macomber Map servers</summary>
        private Dictionary<Uri, ServerLinkedColumnHeader> MMServers = new Dictionary<Uri, ServerLinkedColumnHeader>();

        /// <summary>Our collection of items</summary>
        private Dictionary<PropertyInfo, ListViewItem> ServerDataItems = new Dictionary<PropertyInfo, ListViewItem>();

        /// <summary>Our linakges to the administrative console of the servers</summary>
        private Dictionary<Uri, MM_Administrator_Types> MMServerConnections = new Dictionary<Uri, MM_Administrator_Types>();

        /// <summary>Our UDP listner</summary>
        private UdpClient UdpListener;

        /// <summary>Our collection of MM Server users</summary>
        private Dictionary<Guid, ListViewItem> MMServerUsers = new Dictionary<Guid, ListViewItem>();
        #endregion

        #region Strongly-typed column
        /// <summary>
        /// A small class for strongly-typing our column header
        /// </summary>
        private class ServerLinkedColumnHeader : ColumnHeader
        {
            /// <summary>The URI of our server</summary>
            public Uri Uri;

            /// <summary>The system information of our server</summary>            
            public MM_System_Information SysInfo;

            /// <summary>Our list of connected users</summary>
            public MM_User[] Users;

            /// <summary>
            /// Initialize our new strongly-linked column header
            /// </summary>
            /// <param name="Text"></param>
            /// <param name="Uri"></param>
            /// <param name="SysInfo"></param>
            public ServerLinkedColumnHeader(String Text, Uri Uri, MM_System_Information SysInfo)
            {
                this.Text = Text;
                this.Uri = Uri;
                this.SysInfo = SysInfo;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our main form
        /// </summary>
        public frmAdmin()
        {
            InitializeComponent();
            this.Text = Application.ProductName + " version " + Application.ProductVersion;
            this.DoubleBuffered = true;
            lvMacomberMapServers.GetType().GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(lvMacomberMapServers, true);
            lvUsers.GetType().GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(lvUsers, true);

            ListViewGroup Group = lvMacomberMapServers.Groups.Add("Server", "Server");
            lvMacomberMapServers.Items.Add("Description").Group = Group;
            lvMacomberMapServers.Items.Add("Users").Group = Group;
            lvMacomberMapServers.Items.Add("Version").Group = Group;
            lvMacomberMapServers.Items.Add("Ping").Group = Group;
            SortedDictionary<String, PropertyInfo> Properties = new SortedDictionary<string, PropertyInfo>(StringComparer.CurrentCultureIgnoreCase);
            foreach (PropertyInfo pI in typeof(MM_System_Information).GetProperties())
                Properties.Add(pI.Name, pI);

            foreach (PropertyInfo pI in Properties.Values)
            {
                ListViewItem lvI = lvMacomberMapServers.Items.Add(FixTitle(pI.Name));
                String Category = pI.GetCustomAttribute<CategoryAttribute>() == null ? "Unknown" : pI.GetCustomAttribute<CategoryAttribute>().Category;
                lvI.Group = lvMacomberMapServers.Groups[Category];
                if (lvI.Group == null)
                    lvI.Group = lvMacomberMapServers.Groups.Add(Category, Category);
                lvI.ToolTipText = pI.GetCustomAttribute<DescriptionAttribute>().Description;

                lvI.UseItemStyleForSubItems = false;
                lvI.Tag = pI;
                ServerDataItems.Add(pI, lvI);
            }

            lvUsers.Columns.Add("Server");
            foreach (PropertyInfo pI in typeof(MM_User).GetProperties())
                if (pI.PropertyType != typeof(Guid))

                lvUsers.Columns.Add(FixTitle(pI.Name)).Tag = pI;
            (frm_Command_Information.Instance  =  new frm_Command_Information(btnCommandHistory)).Hide();
            ThreadPool.QueueUserWorkItem(new WaitCallback(ServerInformationUpdater));
        }

        /// <summary>
        /// Fix our title to be easier to read
        /// </summary>
        /// <param name="InString"></param>
        /// <returns></returns>
        private string FixTitle(String InString)
        {
            StringBuilder sB = new StringBuilder();
            for (int a = 0; a < InString.Length; a++)
            {
                if (a > 0 && Char.IsUpper(InString[a]) && !Char.IsUpper(InString[a - 1]))
                    sB.Append(' ');
                sB.Append(InString[a]);
            }
            return sB.ToString();
        }

        /// <summary>
        /// When our form is shown, start our UDP listner
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            UdpListener = new UdpClient();
            UdpListener.ExclusiveAddressUse = false;
            UdpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpListener.Client.Bind(new IPEndPoint(IPAddress.Any, Settings.Default.MacomberMapServerBroadcastPort));
            UdpListener.BeginReceive(ReceiveUDPMessage, null);
        }
        #endregion

        #region UDP identification
        /// <summary>
        /// Receive a UDP message
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveUDPMessage(IAsyncResult ar)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, 0);
            byte[] InLine;
            try
            {
                if (ar.AsyncState == null)
                    InLine = UdpListener.EndReceive(ar, ref remoteEndPoint);
                else
                    InLine = ((UdpClient)ar.AsyncState).EndReceive(ar, ref remoteEndPoint);

                String[] splStr = new UTF8Encoding(false).GetString(InLine).Split('|');
                HandleUDPMessage(remoteEndPoint, splStr);
            }
            catch { }

            if (ar.AsyncState == null)
                UdpListener.BeginReceive(ReceiveUDPMessage, null);
            else
                ((UdpClient)ar.AsyncState).Close();
        }

        private delegate void SafeHandleUDPMessage(IPEndPoint remoteEndPoint, string[] splStr);

        /// <summary>
        /// Handle an input UDP message
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="splStr"></param>
        private void HandleUDPMessage(IPEndPoint remoteEndPoint, string[] splStr)
        {
            try
            {
                if (IsDisposed || Disposing)
                    return;
                else if (InvokeRequired)
                    Invoke(new SafeHandleUDPMessage(HandleUDPMessage), remoteEndPoint, splStr);
                else
                {
                    Uri TargetUri = new Uri(splStr[3]);
                    ServerLinkedColumnHeader TargetCol;
                    if (!MMServers.TryGetValue(TargetUri, out TargetCol))
                    {
                        TargetCol = new ServerLinkedColumnHeader(splStr[2], TargetUri, null);
                        lvMacomberMapServers.Columns.Add(TargetCol);
                        MMServers.Add(TargetUri, TargetCol);
                        foreach (ListViewItem lvI in lvMacomberMapServers.Items)
                            lvI.SubItems.Add("").Tag = DateTime.Now;

                        lvMacomberMapServers.Items[0].SubItems[TargetCol.Index].Text = splStr.Length == 5 ? "(null)" : splStr[5];
                        lvMacomberMapServers.Items[1].SubItems[TargetCol.Index].Text = splStr[4] + " users";
                        lvMacomberMapServers.Items[2].SubItems[TargetCol.Index].Text = splStr[1];

                        MM_Administrator_Types AdminClient = CreateProxy(TargetUri.ToString().Replace("MacomberMapWCFService", "MacomberMapAdministratorInterface"));
                        MMServerConnections.Add(TargetUri, AdminClient);
                        AdminClient.RegisterCallback();
                        frm_Command_Information.Instance.AddCommands(AdminClient.GetEMSCommands(), AdminClient);

                        lvMacomberMapServers.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    }
                    else
                    {
                        MM_Administrator_Types Proxy = MMServerConnections[TargetUri];
                        if (Proxy.State == CommunicationState.Faulted || Proxy.State == CommunicationState.Closing || Proxy.State == CommunicationState.Closed)
                        {
                            MMServerConnections[TargetUri] = CreateProxy(TargetUri.ToString().Replace("MacomberMapWCFService", "MacomberMapAdministratorInterface"));
                            for (int a = 4; a < lvMacomberMapServers.Items.Count; a++)
                                lvMacomberMapServers.Items[a].SubItems[MMServers[TargetUri].Index].Tag = DateTime.Now;
                        }
                        lvMacomberMapServers.Items[0].SubItems[TargetCol.Index].Text = splStr.Length == 5 ? "(null)" : splStr[5];
                        lvMacomberMapServers.Items[1].SubItems[TargetCol.Index].Text = splStr[4] + " users";
                        lvMacomberMapServers.Items[2].SubItems[TargetCol.Index].Text = splStr[1];
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Create a new TCP proxy to our server
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        private MM_Administrator_Types CreateProxy(String Address)
        {
            NetTcpBinding tcpBinding = MM_Binding_Configuration.CreateBinding();                            
            MM_AdministratorMessage_Types Callback = new MM_AdministratorMessage_Types();
            MM_Administrator_Types OutComm = new MM_Administrator_Types(Callback.Context, tcpBinding, new EndpointAddress(Address));
            Callback.Server = OutComm;
            return OutComm;
        }

        /// <summary>
        /// Ping our servers to check for response times
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrPingServer_Tick(object sender, EventArgs e)
        {
            using (Ping p = new Ping())
            foreach (KeyValuePair<Uri, ServerLinkedColumnHeader> kvp in MMServers)
                try
                {
                    long PingTime = p.Send(kvp.Key.Host).RoundtripTime;
                    lvMacomberMapServers.Items[3].SubItems[kvp.Value.Index].Text = PingTime.ToString("#,##0") + " ms";
                    lvMacomberMapServers.Items[3].SubItems[kvp.Value.Index].ForeColor = PingTime < 250 ? Color.LightGreen : PingTime < 500 ? Color.Yellow : Color.Red;
                }
                catch
                {
                    lvMacomberMapServers.Items[3].SubItems[kvp.Value.Index].Text = "??? ms";
                    lvMacomberMapServers.Items[3].SubItems[kvp.Value.Index].ForeColor = Color.Red;
                }
        }

        /// <summary>
        /// Every 5 seconds, check our manual servers. Hopefully, sending a UDP packet on that port will open it up to our response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrRefreshManualServers_Tick(object sender, EventArgs e)
        {
            foreach (String str in Settings.Default.ServersToPing.Split(','))
                try
                {
                    Dns.BeginGetHostEntry(str, HandleDnsCallback, str);
                }
                catch { }
        }

        /// <summary>
        /// Handle our DNS lookup callback
        /// </summary>
        /// <param name="ar"></param>
        private void HandleDnsCallback(IAsyncResult ar)
        {
            try
            {
                foreach (IPAddress Addr in Dns.EndGetHostEntry(ar).AddressList)
                    if (Addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        UdpClient OutUdp = new UdpClient();

                        //Thanks to http://stackoverflow.com/questions/7201862/an-existing-connection-was-forcibly-closed-by-the-remote-host
                        //Update our UDP client to not fail on ICMP no listening responses
                        uint IOC_IN = 0x80000000;
                        uint IOC_VENDOR = 0x18000000;
                        uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                        OutUdp.Client.SendTimeout = 500;
                        OutUdp.Client.ReceiveTimeout = 500;
                        OutUdp.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                        byte[] OutBytes = new UTF8Encoding(false).GetBytes("Macomber Map Client|" + Application.ProductVersion + "|" + Environment.MachineName + "|");
                        OutUdp.Send(OutBytes, OutBytes.Length, new IPEndPoint(Addr, Settings.Default.MacomberMapServerQueryPort));
                        OutUdp.BeginReceive(ReceiveUDPMessage, OutUdp);
                    }
            }
            catch { }
        }
        #endregion

        /// <summary>
        /// Return an easy-to-read string
        /// </summary>
        /// <param name="InVal"></param>
        /// <returns></returns>
        private String GetReadableString(Object InVal)
        {
            if (InVal is int)
                return ((int)InVal).ToString("#,##0");
            else if (InVal is long)
                return ((long)InVal).ToString("#,##0");
            else if (InVal is ulong)
                return ((ulong)InVal).ToString("#,##0");
            else if (InVal is DateTime && (DateTime)InVal == default(DateTime))
                return "";
            else if (InVal == null)
                return "(null)";
            else
                return InVal.ToString();
        }

        /// <summary>
        /// Update our color value
        /// </summary>
        /// <param name="SubItem"></param>
        /// <param name="InValue"></param>
        /// <param name="ItemName"></param>
        private void UpdateProperty(ListViewItem.ListViewSubItem SubItem, Object InValue, String ItemName)
        {
            DateTime LastDate = (DateTime)SubItem.Tag;
            String NewText;
            if (InValue == null)
                NewText = "(null)";
            else if (InValue.GetType() == typeof(double))
                NewText = ((double)InValue).ToString("#,##0.0");
            else if (InValue.GetType() == typeof(int))
                NewText = ((int)InValue).ToString("#,##0");
            else if (InValue.GetType() == typeof(long))
                NewText = ((long)InValue).ToString("#,##0");
            else if (InValue.GetType() == typeof(DateTime))
            {
                TimeSpan TimeDiff = DateTime.Now - (DateTime)InValue;
                if (ItemName == "Simulation Time")
                    NewText = InValue.ToString();
                else                    if ((DateTime)InValue == default(DateTime))
                    NewText = "(null)";
                else if (TimeDiff.TotalDays > 1)
                    NewText = string.Format("{0} days, {1:0.0} hrs", TimeDiff.Days, (double)TimeDiff.Hours + ((double)TimeDiff.Minutes / 60));
                else if (TimeDiff.TotalHours > 1)
                    NewText = String.Format("{0} hrs, {1:0.0} min", TimeDiff.Hours, (double)TimeDiff.Minutes + ((double)TimeDiff.Seconds / 60));
                else if (TimeDiff.TotalMinutes >= 1)
                    NewText = String.Format("{0} min, {1:0.0} sec", TimeDiff.Minutes, TimeDiff.Seconds);
                else
                    NewText = TimeDiff.Seconds + " sec";
            }
            else if (InValue.GetType() == typeof(UInt64))
                NewText = ((UInt64)InValue).ToString("#,##0");
            else
                NewText = InValue.ToString();

            if (NewText != SubItem.Text)
            {
                SubItem.Text = NewText;
                SubItem.Tag = LastDate = DateTime.Now;
            }

            int Sec = Math.Min(255, (int)(DateTime.Now - LastDate).TotalSeconds * 6);
            SubItem.ForeColor = Color.FromArgb(Sec, 255, Sec);
        }

        private delegate MM_System_Information GetSystemInformationDelegate();

        /// <summary>
        /// This thread-safe object goes through all the servers, pulling in values
        /// </summary>
        /// <param name="state"></param>
        private void ServerInformationUpdater(object state)
        {
            while (true)
            {
                foreach (Uri ServerURI in MMServerConnections.Keys.ToArray())
                {
                    MM_Administrator_Types Client = MMServerConnections[ServerURI];
                    ServerLinkedColumnHeader TargetCol = MMServers[ServerURI];

                    if (Client.State == CommunicationState.Opening)
                        SetColor(TargetCol, Color.Yellow);
                    else if (Client.State == CommunicationState.Faulted || Client.State == CommunicationState.Closed || Client.State == CommunicationState.Closing)
                        SetColor(TargetCol, Color.Red);
                    else
                        try
                        {
                            TargetCol.SysInfo = Client.GetSystemInformation();
                            TargetCol.Users = Client.GetUserInformation();
                        }
                        catch (Exception ex)
                        {
                            SetColor(TargetCol, Color.Red);
                        }
                }
                Thread.Sleep(1000);
            }
        }

        private delegate void SafeSetColor(ServerLinkedColumnHeader Column, Color TargetColor);

        private void SetColor(ServerLinkedColumnHeader Column, Color TargetColor)
        {
            if (InvokeRequired)
                lvMacomberMapServers.Invoke(new SafeSetColor(SetColor), Column, TargetColor);
            else
            {
                for (int a = 3; a < lvMacomberMapServers.Items.Count; a++)
                    lvMacomberMapServers.Items[a].SubItems[Column.Index].ForeColor = TargetColor;
            }
        }

        /// <summary>
        /// For all of our connected servers, determine our user information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdateUserInfo_Tick(object sender, EventArgs e)
        {
            //Create our total list of users
            Dictionary<Guid, bool> ToRemove = new Dictionary<Guid, bool>();
            foreach (Guid guid in MMServerUsers.Keys)
                ToRemove.Add(guid, true);

            foreach (Uri ServerURI in MMServerConnections.Keys.ToArray())
            {
                MM_Administrator_Types Client = MMServerConnections[ServerURI];
                ServerLinkedColumnHeader TargetCol = MMServers[ServerURI];
                if (Client.State == CommunicationState.Opened && TargetCol.SysInfo != null && TargetCol.Users != null)
                {
                    for (int a = 4; a < lvMacomberMapServers.Items.Count; a++)
                    {
                        ListViewItem lvI = lvMacomberMapServers.Items[a];
                        PropertyInfo pI = (PropertyInfo)lvI.Tag;
                        UpdateProperty(lvI.SubItems[TargetCol.Index], pI.GetValue(TargetCol.SysInfo), lvI.Text);
                    }

                    //Update the users, removing any disconnected ones                
                    foreach (MM_User User in TargetCol.Users)
                    {
                        ListViewItem lvUser;
                        if (!MMServerUsers.TryGetValue(User.UserId, out lvUser))
                        {
                            MMServerUsers.Add(User.UserId, lvUser = lvUsers.Items.Add(ServerURI.Host + ":" + ServerURI.Port.ToString()));
                            lvUser.Tag = new KeyValuePair<Uri, MM_User>(ServerURI, User);
                            lvUser.UseItemStyleForSubItems = false;
                            for (int a = 1; a < lvUsers.Columns.Count; a++)
                                lvUser.SubItems.Add(GetReadableString(((PropertyInfo)lvUsers.Columns[a].Tag).GetValue(User))).Tag = DateTime.Now;
                        }
                        else
                        {
                            ToRemove.Remove(User.UserId);
                            for (int a = 1; a < lvUsers.Columns.Count; a++)
                                UpdateProperty(lvUser.SubItems[a], ((PropertyInfo)lvUsers.Columns[a].Tag).GetValue(User),lvUsers.Columns[a].Text);
                        }
                    }
                }
                else if (Client.State == CommunicationState.Faulted || Client.State == CommunicationState.Closing || Client.State == CommunicationState.Closed)
                    foreach (ListViewItem lvI in lvUsers.Items)
                    {
                        KeyValuePair<Uri, MM_User> kvp = (KeyValuePair<Uri, MM_User>)lvI.Tag;
                        if (kvp.Key == ServerURI)
                        {
                            lvI.ForeColor = Color.Red;
                            ToRemove.Remove(kvp.Value.UserId);
                        }
                    }
            }

            //Now, remove all users not found
            ListViewItem FoundItem;
            foreach (Guid guid in ToRemove.Keys)
                if (MMServerUsers.TryGetValue(guid, out FoundItem))
                {
                    lvUsers.Items.Remove(FoundItem);
                    MMServerUsers.Remove(guid);
                }
        }

        /// <summary>
        /// Reconnect our server
        /// </summary>
        /// <param name="ServerName"></param>
        private void ReconnectServer(Uri ServerName, Exception ex)
        {
            MM_Administrator_Types Admin = MMServerConnections[ServerName];
            if (Admin.State == CommunicationState.Faulted || Admin.State == CommunicationState.Closed)
                MMServerConnections[ServerName] = CreateProxy(ServerName.ToString());
        }

        /// <summary>
        /// Handle our users mouse click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvUsers_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hti = lvUsers.HitTest(e.Location);
            if (e.Button == MouseButtons.Right && hti.Item != null)
            {
                cmsSystemInteraction.Items.Clear();
                KeyValuePair<Uri, MM_User> User = (KeyValuePair<Uri, MM_User>)hti.Item.Tag;
                cmsSystemInteraction.Items.Add("User " + User.Value.UserName).Enabled = false;
                cmsSystemInteraction.Items.Add("Logged onto " + User.Key.Host).Enabled = false;
                cmsSystemInteraction.Items.Add("Logged on at " + User.Value.LoggedOnTime.ToString()).Enabled = false;
                cmsSystemInteraction.Items.Add("-");
                cmsSystemInteraction.Items.Add("Send a message to user").Tag = User;
                cmsSystemInteraction.Items.Add("Force user logoff").Tag = User;
                cmsSystemInteraction.Show(Cursor.Position);
            }
        }

        /// <summary>
        /// Handle a column click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvMacomberMapServers_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ServerLinkedColumnHeader col = lvMacomberMapServers.Columns[e.Column] as ServerLinkedColumnHeader;
            if (col == null)
                return;
            cmsSystemInteraction.Items.Clear();
            cmsSystemInteraction.Items.Add("Server " + col.Uri.Host + ":" + col.Uri.Port).Enabled = false;
            if (col.SysInfo != null)
            {
                cmsSystemInteraction.Items.Add("Server process memory: " + col.SysInfo.SystemFreeMemory.ToString("#,##0")).Enabled = false;
                cmsSystemInteraction.Items.Add("Free memory: " + col.SysInfo.SystemFreeMemory.ToString("#,##0")).Enabled = false;
            }
            cmsSystemInteraction.Items.Add("-");
            cmsSystemInteraction.Items.Add("Change Server Description").Tag=col;
            cmsSystemInteraction.Items.Add("-");
            cmsSystemInteraction.Items.Add("Generate server savecase").Tag = col;
            cmsSystemInteraction.Items.Add("Load server savecase").Tag = col;
            cmsSystemInteraction.Items.Add("-");
            if (Environment.UserName.ToLower().Contains("legat"))
            {
                cmsSystemInteraction.Items.Add("Stress test clients ON").Tag = col;
                cmsSystemInteraction.Items.Add("Stress test clients OFF").Tag = col;
                cmsSystemInteraction.Items.Add("-");
            }
            cmsSystemInteraction.Items.Add("Send a message to all users").Tag = col;
            cmsSystemInteraction.Items.Add("Force all user logoff").Tag = col;
            cmsSystemInteraction.Show(Cursor.Position);
        }

        /// <summary>
        /// Handle a menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmsSystemInteraction_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            cmsSystemInteraction.Close();
            if (e.ClickedItem.Tag is KeyValuePair<Uri, MM_User>)
            {
                KeyValuePair<Uri, MM_User> User = (KeyValuePair<Uri, MM_User>)e.ClickedItem.Tag;
                IMM_Administrator_Types AdminConn = MMServerConnections[User.Key];
                if (e.ClickedItem.Text == "Send a message to user")
                {
                    using (MM_Input_Box InputBox = new MM_Input_Box())
                        if (InputBox.ShowDialog(this, "Please enter the message to send to " + User.Value.UserName, Application.ProductName) == DialogResult.OK)
                            AdminConn.SendMessage(User.Value, InputBox.Message, Environment.UserName + " on " + Environment.MachineName, InputBox.TargetIcon);
                }
                else if (e.ClickedItem.Text == "Force user logoff" && MessageBox.Show("Are you sure you want to log off " + User.Value.UserName + " from " + User.Key.Host + "?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    AdminConn.CloseClient(User.Value);
            }
            else if (e.ClickedItem.Tag is ServerLinkedColumnHeader)
            {
                ServerLinkedColumnHeader Server = (ServerLinkedColumnHeader)e.ClickedItem.Tag;
                IMM_Administrator_Types Admin = MMServerConnections[Server.Uri];
                if (e.ClickedItem.Text == "Send a message to all users")
                {
                    using (MM_Input_Box InputBox = new MM_Input_Box())
                        if (InputBox.ShowDialog(this, "Please enter the message to send to all users on server " + Server.Uri.Host, Application.ProductName) == DialogResult.OK)
                            Admin.SendMessageToAllClients(InputBox.Message, Environment.UserName + " on " + Environment.MachineName, InputBox.TargetIcon);
                }
                else if (e.ClickedItem.Text == "Force all user logoff" && MessageBox.Show("Are you sure you want to log off all users on server " + Server.Uri.Host + "?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Admin.CloseAllClients();
                else if (e.ClickedItem.Text == "Generate server savecase")
                {
                    using (SaveFileDialog sFd = new SaveFileDialog() { Title = "Macomber Map Server Savecase Generation", Filter = "MM Server Savecase (*.MM_Savecase)|*.MM_Savecase" })
                        if (sFd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            using (FileStream fsOut = new FileStream(sFd.FileName, FileMode.Create))
                            {
                                MM_Savecase Savecase = Admin.GenerateSavecase();
                                DataContractSerializer dcs = new DataContractSerializer(typeof(MM_Savecase));
                                XmlDictionaryWriter xdw = XmlDictionaryWriter.CreateBinaryWriter(fsOut);
                                dcs.WriteObject(xdw, Savecase);
                                xdw.Flush();
                            }
                }
                else if (e.ClickedItem.Text == "Load server savecase")
                {
                    using (OpenFileDialog oFd = new OpenFileDialog() { Title = "Macomber Map Server Savecase Generation", Filter = "MM Server Savecase (*.MM_Savecase)|*.MM_Savecase" })
                        if (oFd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            using (FileStream fsIn = new FileStream(oFd.FileName, FileMode.Open))
                            {
                                DataContractSerializer dcs = new DataContractSerializer(typeof(MM_Savecase));
                                XmlDictionaryReader xrd = XmlDictionaryReader.CreateBinaryReader(fsIn, new XmlDictionaryReaderQuotas());
                                MM_Savecase Savecase = (MM_Savecase)dcs.ReadObject(xrd);
                                Admin.ApplySavecase(Savecase);
                            }
                }
                else if (e.ClickedItem.Text.StartsWith("Stress test clients"))
                    Admin.SetServerClientStressTest(e.ClickedItem.Text.EndsWith("ON"));
                else if (e.ClickedItem.Text == "Change Server Description")
                    using (MM_Input_Box InputBox = new MM_Input_Box() { Message = lvMacomberMapServers.Items[0].SubItems[Server.Index].Text })
                        if (InputBox.ShowDialog(this, "Please enter the updated server description", Application.ProductName) == DialogResult.OK)
                            Admin.SetServerDescription(InputBox.Message);
            }
        }

        /// <summary>
        /// Show our operatorship form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMergeOperatorship_Click(object sender, EventArgs e)
        {
            new frmMergeOperatorship().Show(this);
        }

        /// <summary>
        /// Show our command history form       
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandHistory_Click(object sender, EventArgs e)
        {
            try
            {
            frm_Command_Information.Instance.Show();
            }
            catch { }
        }
    }
}