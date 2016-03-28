using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TEDESimulator.Server_Connectivity
{ /// <summary>
  /// © 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
  /// This class holds information on a Macomber Map server
  /// </summary>
    public class MM_Server_Information
    {
        #region Variable declarations
        /// <summary>The end point of our server</summary>
        public IPEndPoint BroadcastEndpoint { get; set; }

        /// <summary>The name of our server</summary>
        public string ServerName { get; set; }

        /// <summary>The description of our MM server</summary>
        public String Description { get; set; }

        /// <summary>The number of users connected to the system</summary>
        public int UserCount { get; set; }

        /// <summary>The ping time (in ms) to the MM server</summary>
        public long PingTime { get; set; }

        /// <summary>The timestamp from our last UDP receive event</summary>
        public DateTime LastUDP { get; set; }

        /// <summary>The URI of the server</summary>
        public Uri ServerURI { get; set; }
        #endregion

        /// <summary>
        /// Update our Macomber Map server information
        /// </summary>
        /// <param name="MMServers"></param>
        /// <param name="cmbMacomberMapServer"></param>
        public static void UpdateServerInformation(Dictionary<Uri, MM_Server_Information> MMServers, ComboBox cmbMacomberMapServer)
        {

            MM_Server_Information FoundItem;
            lock (MM_Server_Interface.MMServers) // changed to list and added lock to avoid "destination not long enough" exception
            {
                foreach (MM_Server_Information Info in MM_Server_Interface.MMServers.Values.ToList())
                {
                    if (!MMServers.TryGetValue(Info.ServerURI, out FoundItem))
                    {
                        cmbMacomberMapServer.Items.Add(Info.ServerName);
                        MMServers.Add(Info.ServerURI, FoundItem);
                    }
                    else if ((DateTime.Now - Info.LastUDP).TotalSeconds > 30 && FoundItem != null)
                    {
                        cmbMacomberMapServer.Items.Remove(FoundItem.ServerName);
                        MM_Server_Interface.MMServers.Remove(Info.ServerURI);
                        MMServers.Remove(Info.ServerURI);
                    }
                
                }
            }
        }
    }
}
