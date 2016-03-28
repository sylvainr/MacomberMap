using MacomberMapCommunications;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.WCF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TEDESimulator.Properties;

namespace TEDESimulator.Server_Connectivity
{
    public static class  MM_Server_Interface
    {
        #region Variable declarations
        /// <summary>Our client status, to report to the MM server</summary>
        public static MM_Client_Status ClientStatus = new MM_Client_Status();
        
        /// <summary>The user name to our server interface</summary>
        internal static string UserName;

        /// <summary>The password to our server interface</summary>
        internal static string Password;

        /// <summary>Our MM WCF interface</summary>
        public static MM_WCF_Interface Client;

        /// <summary>Our callback handler</summary>
        public static MM_Server_Callback_Handler CallbackHandler;

        /// <summary>Our cached login information</summary>
        internal static String ServerName;

        /// <summary>The URI for our server instance</summary>
        public static Uri ConnectionURI;

        /// <summary>The list of client areas</summary>
        public static Dictionary<string, bool> ClientAreas = new Dictionary<string, bool>();
        #endregion

        #region Startup
        /// <summary>
        /// Handle the initailization of our server interface by kicking off a management thread
        /// </summary>
        static MM_Server_Interface()
        {
            StartUdpListener();
            StartPingMacomberMapServers();
        }

        private static void StartUdpListener()
        {
            Task.Run(async () =>
            {
                using (var udpClient = new UdpClient())
                {
                    udpClient.ExclusiveAddressUse = false;
                    udpClient.Client.SendTimeout = 500;
                    udpClient.Client.ReceiveTimeout = 500;
                    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, Settings.Default.MacomberMapServerBroadcastPort));
                    while (udpClient != null && udpClient.Client != null)
                    {
                        var response = await udpClient.ReceiveAsync();
                        HandleUDPMessage(response);
                    }
                }
            });
        }

        #endregion

        #region Login
        /// <summary>
        /// Attempt to log in a second time using the existing credentials.
        /// </summary>
        /// <param name="ServerName"></param>
        /// <param name="ConnectionURI"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static bool TryLoginAgain(String ServerName, Uri ConnectionURI, out Exception Error)
    {
        return TryLogin(ServerName, ConnectionURI, UserName, Password, out Error);
    }

    /// <summary>
    /// Attempt to log in to a MM Server
    /// </summary>
    /// <param name="ServerName"></param>
    /// <param name="UserName"></param>
    /// <param name="Password"></param>
    /// <param name="ConnectionURI"></param>
    /// <param name="Error"></param>
    /// <returns></returns>
    public static bool TryLogin(String ServerName, Uri ConnectionURI, String UserName, String Password, out Exception Error)
    {
        if (Client != null)
        {
            try
            {
                Client.Dispose();
            }
            catch
            {
            }
            Thread.Sleep(500);
        }
        Client = null;
        CallbackHandler = null;

        try
        {
            MM_Server_Interface.ServerName = ServerName;
            MM_Server_Interface.ConnectionURI = ConnectionURI;
            MM_Server_Interface.UserName = UserName;
            MM_Server_Interface.Password = Password;

            NetTcpBinding tcpBinding = MM_Binding_Configuration.CreateBinding();

            Client = new MM_WCF_Interface(new MM_Server_Callback_Handler().Context, tcpBinding, new EndpointAddress(ConnectionURI.ToString()));
            Client.ChannelFactory.Faulted += ChannelFactory_Faulted;
            Client.ChannelFactory.Opened += ChannelFactory_Opened;
            MM_Server_Interface.CallbackHandler.Context.Faulted += ChannelFactory_Faulted;
            MM_Server_Interface.CallbackHandler.Context.Opened += ChannelFactory_Opened;

            MM_Server_Interface.UserName = UserName;
            MM_Server_Interface.Password = Password;
            MM_Server_Interface.ClientAreas.Clear();
            foreach (String str in Client.HandleUserLogin(UserName, Password))
                MM_Server_Interface.ClientAreas.Add(str, true);
            if (MM_Server_Interface.ClientAreas.Count == 0)
            {
                MM_System_Interfaces.LogError("Unable to log in to the Macomber Map Server");
                Client.InnerChannel.Close();
                throw new InvalidOperationException("Unable to log in to the Macomber Map server. The username and password combination was invalid, and/or there were no operatorship areas for the client.");
            }
            else if (ClientAreas.ContainsKey("ERCOT"))
                MM_System_Interfaces.LogError("Logged in to Macomber Map Server, MASTER " + MM_Server_Interface.ClientAreas.Count.ToString("0") + " operatorships.");
            else
                MM_System_Interfaces.LogError("Logged in to Macomber Map Server, " + MM_Server_Interface.ClientAreas.Count.ToString("0") + " operatorships.");

            Error = null;
            return true;
        }
        catch (Exception ex)
        {
            Error = ex;
            return false;
        }
    }

        /// <summary>
        /// Report a channel factory fault
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ChannelFactory_Faulted(object sender, EventArgs e)
        {
            try
            {
                if (sender != null)
                    ((ICommunicationObject)sender).Abort();
            }
            catch (Exception)
            {
            }
            Exception Err;
            TryLogin(ServerName, ConnectionURI, UserName, Password, out Err);
        }

    /// <summary>
    /// Report a channel factory open
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void ChannelFactory_Opened(object sender, EventArgs e)
    {
        if (sender is InstanceContext)
            Console.WriteLine("MM: Server -> Client connection opened.");
        else
            Console.WriteLine("MM: Client -> Server connection opened.");
    }
        #endregion

        #region Server connectivity, listening, and ping


        /// <summary>Our collection of Macomber Map servers</summary>
        public static Dictionary<Uri, MM_Server_Information> MMServers = new Dictionary<Uri, MM_Server_Information>();

        /// <summary>
        /// Ping our Macomber Map servers
        /// </summary>
        internal static void StartPingMacomberMapServers()
        {
            Task.Run(() =>
            {
                var queryUdp = new UdpClient();
                queryUdp.ExclusiveAddressUse = false;
                queryUdp.Client.ReceiveTimeout = 500;
                queryUdp.Client.SendTimeout = 500;
                queryUdp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
                //queryUdp.Client.Bind(new IPEndPoint(IPAddress.Any, Settings.Default.MacomberMapServerQueryPort));

                int CurrentIter = 0;
                //if (Settings.Default.ServersToPing.IndexOf("localhost", StringComparison.CurrentCultureIgnoreCase) < 0)
                //    Settings.Default.ServersToPing += ",localhost";
                while (true)
                {
                    //First, once every 10 seconds, send our commands out for manual servers
                    if (CurrentIter == 0)
                        foreach (String str in Settings.Default.ServersToPing.Split(','))
                            try
                            {
                                foreach (IPAddress Addr in Dns.GetHostEntry(str).AddressList)
                                    if (Addr.AddressFamily == AddressFamily.InterNetwork)
                                    {
                                        byte[] OutBytes = new UTF8Encoding(false).GetBytes("Macomber Map Client|" + Application.ProductVersion + "|" + Environment.MachineName + "|");
                                        queryUdp.Send(OutBytes, OutBytes.Length, new IPEndPoint(Addr, Settings.Default.MacomberMapServerQueryPort));

                                        Task.Run(async () =>
                                        {
                                            var response = await queryUdp.ReceiveAsync();
                                            HandleUDPMessage(response);
                                        }).Wait(queryUdp.Client.ReceiveTimeout);
                                    }
                            }
                            catch (SocketException ex)
                            {
                                if (ex.ErrorCode == 10054)
                                {
                                    Debug.WriteLine("Server not available: " + str);
                                }
                            }
                            catch { }

                    //Every 5 seconds, ping our clients
                    if (CurrentIter % 5 == 0)
                        using (Ping p = new Ping())
                        {

                            foreach (MM_Server_Information Server in MMServers.Values.ToArray())
                                try
                                {
                                    var reply = p.Send(Server.ServerURI.Host, 25);
                                    Server.PingTime = reply.RoundtripTime;
                                }
                                catch
                                {
                                    Server.PingTime = -1;
                                }
                            p.Dispose();

                        }
                    //Capture our CPU usage
                    try
                    {
                        long Mem;
                        MM_System_Profiling.MEMORYSTATUSEX MemStatus;
                        ClientStatus.CPUUsage = MM_System_Profiling.GetCPUUsage(out Mem, out MemStatus);
                        ClientStatus.SystemFreeMemory = MemStatus.ullAvailPhys;
                        ClientStatus.SystemTotalMemory = MemStatus.ullTotalPhys;
                        ClientStatus.OpenWindows = -1;
                        ClientStatus.ApplicationMemoryUse = Mem;

                        //Send CPU status every 2 seconds
                        if (CurrentIter % 2 == 0 && Client != null)
                            Client.ReportUserStatus(ClientStatus);
                    }
                    catch { }

                  
                    //Wait a second
                    Thread.Sleep(1500);
                }
            });
        }

        /// <summary>
        /// Handle an input UDP message
        /// </summary>
        /// <param name="result">UDP Message</param>
        private static void HandleUDPMessage(UdpReceiveResult result) // IPEndPoint remoteEndPoint, string[] splStr)
        {
            if (result == default(UdpReceiveResult)) return;

            var buffer = result.Buffer;
            if (buffer == null || buffer.Length == 0) return;

            string[] splStr = new UTF8Encoding(false).GetString(buffer).Split('|');
            if (string.IsNullOrWhiteSpace(splStr[3]) || splStr[0] == "Macomber Map Client") return;

            Uri ServerUri = new Uri(splStr[3]);

            MM_Server_Information ServerInfo;
            if (!MMServers.TryGetValue(ServerUri, out ServerInfo))
                MMServers.Add(ServerUri, ServerInfo = new MM_Server_Information() { BroadcastEndpoint = result.RemoteEndPoint, ServerURI = ServerUri });
            ServerInfo.Description = splStr.Length == 5 ? "" : splStr[5];
            ServerInfo.UserCount = int.Parse(splStr[4]);
            ServerInfo.ServerName = splStr[2];
            ServerInfo.LastUDP = DateTime.Now;
        }

        #endregion
    }
}
