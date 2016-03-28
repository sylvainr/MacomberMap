using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.Integration
{
    /// <summary>
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
        /// <param name="lvMacomberMapServers"></param>
        public static void UpdateServerInformation(Dictionary<Uri, ListViewItem> MMServers, ListView lvMacomberMapServers)
        {
            
                ListViewItem FoundItem;
            lock (MM_Server_Interface.MMServers) // changed to list and added lock to avoid "destination not long enough" exception
            {
                foreach (MM_Server_Information Info in MM_Server_Interface.MMServers.Values.ToList()) 
                {
                    if (!MMServers.TryGetValue(Info.ServerURI, out FoundItem))
                    {
                        FoundItem = new ListViewItem(Info.ServerName);
                        FoundItem.SubItems.Add(Info.Description);
                        FoundItem.SubItems.Add(Info.UserCount.ToString() + " users");
                        FoundItem.SubItems.Add(Info.PingTime.ToString() + " ms").ForeColor = Info.PingTime < 10 ? Color.LightGreen : Info.PingTime < 20 ? Color.Yellow : Color.Red;
                        FoundItem.Tag = Info;
                        FoundItem.UseItemStyleForSubItems = false;
                        FoundItem.ImageIndex = 0;
                        FoundItem.BackColor = Color.Black;
                        lvMacomberMapServers.Items.Add(FoundItem);
                        MMServers.Add(Info.ServerURI, FoundItem);
                    }
                    else if ((DateTime.Now - Info.LastUDP).TotalSeconds > 30)
                    {
                        lvMacomberMapServers.Items.Remove(FoundItem);
                        MM_Server_Interface.MMServers.Remove(Info.ServerURI);
                        MMServers.Remove(Info.ServerURI);
                    }
                    else
                    {
                        FoundItem.SubItems[1].Text = Info.Description;
                        FoundItem.SubItems[2].Text = Info.UserCount.ToString() + " users";
                        FoundItem.SubItems[3].Text = Info.PingTime == -1 ? "???" :                             Info.PingTime.ToString() + " ms";
                        FoundItem.SubItems[3].ForeColor = Info.PingTime == -1 ? Color.Red :  Info.PingTime < 10 ? Color.LightGreen : Info.PingTime < 20 ? Color.Yellow : Color.Red;

                        //If we have a server that is connected, update accordingly.                        
                        if (MM_Server_Interface.Client != null && MM_Server_Interface.Client.Endpoint.Address.Uri.Equals(Info.ServerURI))
                        {
                            if (MM_Server_Interface.Client.InnerChannel.State != System.ServiceModel.CommunicationState.Opened)
                                FoundItem.BackColor = Color.DarkRed;
                            else if (MM_Server_Interface.CallbackHandler.Context.State != System.ServiceModel.CommunicationState.Opened)
                                FoundItem.BackColor = Color.DarkOrange;
                            else
                                FoundItem.BackColor = Color.DarkGreen;
                        }
                        else
                            FoundItem.BackColor = Color.Black;
                    }
                }
            }
        }
    }
}
