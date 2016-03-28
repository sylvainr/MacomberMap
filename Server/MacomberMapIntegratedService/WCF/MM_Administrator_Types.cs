using MacomberMapAdministrationService;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.EMS;
using MacomberMapIntegratedService.Properties;
using MacomberMapIntegratedService.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapIntegratedService.WCF
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides all of our service types
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class MM_Administrator_Types : IMM_Administrator_Types
    {
        #region Instance variables
        /// <summary>The GUID for the conversation
        public Guid ConversationGuid = Guid.Empty;

        /// <summary>Our conversation handler</summary>
        public IMM_AdministratorMessage_Types ConversationHandler;

        /// <summary>The random number generator for our EMS outgoing commands</summary>
        private static Random RandomGenerator = new Random();

        /// <summary>The reverse DNS lookup</summary>
        private String Entry;
        #endregion

        #region Startup/Shutdown
        /// <summary>
        /// 
        /// </summary>
        public MM_Administrator_Types()
        {
            RemoteEndpointMessageProperty EndPoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            try
            {
                Entry = Dns.GetHostEntry(EndPoint.Address).HostName + " (" + EndPoint.Address.ToString() + ")";
            }
            catch
            {
                Entry = "Unknown (" + EndPoint.Address.ToString() + ")";
            }
            ConversationGuid = Guid.NewGuid();
            OperationContext.Current.Channel.Faulted += Channel_Closed;
            OperationContext.Current.Channel.Closed += Channel_Closed;
        }

        /// <summary>
        /// Handle our channel closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Channel_Closed(object sender, EventArgs e)
        {
            MM_Server.RemoveAdministrator(this);
        }
        #endregion

        /// <summary>
        /// Report all of our user information
        /// </summary>
        /// <returns></returns>
        public MM_User[] GetUserInformation()
        {
            List<MM_User> Users = new List<MM_User>();
            foreach (MM_WCF_Interface Interface in MM_Server.ConnectedUserList)
                Users.Add(Interface.User);
            return Users.ToArray();
        }

        /// <summary>
        /// Report our system information
        /// </summary>
        /// <returns></returns>
        public MM_System_Information GetSystemInformation()
        {
            return MM_Server.SystemInformation;
        }

        /// <summary>
        /// Send a message to our client
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Message"></param>
        /// <param name="MessageSource"></param>
        /// <param name="Icon"></param>
        public void SendMessage(MM_User User, string Message, String MessageSource, MessageBoxIcon Icon)
        {
            MM_WCF_Interface.SendMessage(User.UserId, "ReceiveMessage", Message, MessageSource, Icon);
        }

        /// <summary>
        /// Send a message to all users
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="MessageSource"></param>
        /// <param name="Icon"></param>
        public void SendMessageToAllClients(string Message, String MessageSource, MessageBoxIcon Icon)
        {
            foreach (MM_WCF_Interface Interface in MM_Server.ConnectedUserList)
                MM_WCF_Interface.SendMessage(Interface.User.UserId, "ReceiveMessage", Message, MessageSource, Icon);
        }

        /// <summary>
        /// Send a close message to a particular client
        /// </summary>
        /// <param name="Client"></param>
        public void CloseClient(MM_User Client)
        {
            MM_WCF_Interface.SendMessage(Client.UserId, "CloseClient");
        }

        /// <summary>
        /// Send a close message to all clients
        /// </summary>
        public void CloseAllClients()
        {
            foreach (MM_WCF_Interface Interface in MM_Server.ConnectedUserList)
                MM_WCF_Interface.SendMessage(Interface.User.UserId, "CloseClient");
        }


        /// <summary>
        /// Generate a savecase, and return to our client
        /// </summary>
        /// <returns></returns>
        public MM_Savecase GenerateSavecase()
        {
            MM_Savecase Savecase = new MM_Savecase();
            Savecase.LoadCurrentSavecase(typeof(MM_Server));
            return Savecase;
        }



        /// <summary>
        /// Apply a savecase to our system
        /// </summary>
        /// <param name="Savecase"></param>
        /// <returns></returns>
        public bool ApplySavecase(MM_Savecase Savecase)
        {            
            Savecase.ApplySavecase(typeof(MM_Server));
            return true;
        }


        /// <summary>
        /// Set our server description
        /// </summary>
        /// <param name="ServerDescription"></param>
        /// <returns></returns>
        public bool SetServerDescription(string ServerDescription)
        {
            MM_Server.MacomberMapServerDescription = ServerDescription;
            return true;
        }


        /// <summary>
        /// Handle the stress testing of our clients
        /// </summary>
        /// <param name="Active"></param>
        /// <returns></returns>
        public bool SetServerClientStressTest(bool Active)
        {
            MM_Server.StressTestClients(Active);
            return true;
        }


        /// <summary>
        /// Register our callback of admin push
        /// </summary>
        /// <param name="VersionNumber"></param>
        /// <returns></returns>
        public bool RegisterCallback(string VersionNumber = "Unknown")
        {
            ConversationHandler = OperationContext.Current.GetCallbackChannel<IMM_AdministratorMessage_Types>();
            MM_Server.AddAdministrator(this);
            return true;
        }

        /// <summary>
        /// Get our list of EMS commands
        /// </summary>
        /// <returns></returns>
        public MM_EMS_Command[] GetEMSCommands()
        {
            return (Properties.Settings.Default.ForwardCommandsToAdminClients) ? MM_Server.EMSCommands.ToArray() : new MM_EMS_Command[0];
        }

        /// <summary>
        /// Handle the sending of an EMS command
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public bool SendCommand(string Command)
        {
            byte[] OutBytes = new UTF8Encoding(false).GetBytes("#CMD_TYPE,APP,FAM,CMD_NAME,DB,RECORD,FIELD,TEID,VALUE" + Environment.NewLine + Command + Environment.NewLine);
            DateTime SimTime = MM_Server.SimulatorTimeData.Length == 0 ? DateTime.Now : MM_Server.SimulatorTimeData[0].Simulation_Time;

            if (!String.IsNullOrEmpty(Settings.Default.OperatorCommandTCPAddress))
                try
                {
                    using (TcpClient Client = new TcpClient())
                    {
                        Client.Connect(Settings.Default.OperatorCommandTCPAddress, Settings.Default.OperatorCommandTCPPort);
                        using (NetworkStream nS = Client.GetStream())
                            nS.Write(OutBytes, 0, OutBytes.Length);
                        MM_Notification.WriteLine(ConsoleColor.White, "Sent {1} command to TEDE/TCP: by {0} on {2}. SimTime {3}", "ADMIN", Command, DateTime.Now, SimTime);                    
                    }
                }
                catch (Exception ex)
                {
                    MM_Notification.WriteLine(ConsoleColor.Red, "Error sending {1} command to TEDE/TCP: by {0} on {2} SimTime {3}: {4} ", "ADMIN", Command, DateTime.Now, SimTime, ex);
                    return false;
                }

            if (!String.IsNullOrEmpty(Properties.Settings.Default.OperatorCommandPath))
            {
                String TargetFileName = Path.Combine(Properties.Settings.Default.OperatorCommandPath, "EMS_COMMANDS_" + RandomGenerator.Next().ToString() + ".csv");
                using (FileStream fS = new FileStream(TargetFileName, FileMode.CreateNew, FileAccess.Write))
                    fS.Write(OutBytes, 0, OutBytes.Length);
                MM_Notification.WriteLine(ConsoleColor.White, "Sent {1} command to TEDE/File: by {0} on {2}. SimTime {3}", "ADMIN", Command, DateTime.Now, SimTime);
            }

            //Now, write out our command
            MM_EMS_Command[] OutCommands = new MM_EMS_Command[] { new MM_EMS_Command(Command, "ADMIN", Entry, SimTime, "?") };
            try
            {
                List<String> OutLines = new List<string>();
                if (!File.Exists("EMSCommands.csv"))
                    OutLines.Add(MM_EMS_Command.HeaderLine());

                OutLines.Add(OutCommands[0].BuildLine());
                File.AppendAllLines("EMSCommands.csv", OutLines.ToArray());
            }
            catch (Exception ex)
            {
                MM_Notification.WriteLine(ConsoleColor.Red, "Unable to write out command to local log: {0}", ex);
            }

            //Now, send our commands to all administrators
            if (Properties.Settings.Default.ForwardCommandsToAdminClients)
                foreach (MM_Administrator_Types Interface in MM_Server.ConnectedAdministatorList)
                    try
                    {
                        MM_Administrator_Types.SendMessageToConsole(Interface.ConversationGuid, "AddEMSCommands", new object[] { OutCommands });
                    }
                    catch (Exception ex)
                    {
                        MM_Notification.WriteLine(ConsoleColor.Red, "Unable to send {0} to Admin: {1}", "AddEMSCommands", ex);
                    }

            //Now, send out our commands to all clients
            if (Properties.Settings.Default.ForwardCommandsToERCOTClients)
                foreach (MM_WCF_Interface Interface in MM_Server.ConnectedUserList)
                    if (Interface.User.LoggedOnTime != default(DateTime) && Interface.User.ERCOTUser)
                        try
                        {
                            MM_WCF_Interface.SendMessage(Interface.User.UserId, "AddEMSCommands", new object[] { OutCommands });
                        }
                        catch (Exception ex)
                        {
                            MM_Notification.WriteLine(ConsoleColor.Red, "Unable to send {0} to {1}: {2}", "AddEMSCommands", Interface.User.UserName, ex);
                        }
            return true;
        }


        /// <summary>
        /// Send a message to our client
        /// </summary>
        /// <param name="SystemGuid"></param>
        /// <param name="ProcedureName"></param>
        /// <param name="Parameters"></param>
        public static void SendMessageToConsole(Guid SystemGuid, String ProcedureName, params object[] Parameters)
        {
            //First, try and find our client
            MethodInfo FoundMethod;
            MM_Administrator_Types FoundConnection;
            if (MM_Server.FindAdministrator(SystemGuid, out FoundConnection))
                if (FoundConnection.ConversationHandler != null)
                    if ((FoundMethod = typeof(IMM_AdministratorMessage_Types).GetMethod(ProcedureName)) != null)
                        new SendMessageDelegate(SendMessageInternal).BeginInvoke(FoundMethod, FoundConnection, Parameters, MessageSentCompletion, FoundConnection);
        }

        private delegate void SendMessageDelegate(MethodInfo Method, MM_Administrator_Types FoundConnection, object[] parameters);


        /// <summary>
        /// Send our message internally
        /// </summary>
        /// <param name="SystemGuid"></param>
        /// <param name="ProcedureName"></param>
        /// <param name="Parameters"></param>
        private static void SendMessageInternal(MethodInfo Method, MM_Administrator_Types FoundConnection, object[] parameters)
        {
            try
            {
                Method.Invoke(FoundConnection.ConversationHandler, parameters);
            }
            catch (Exception ex)
            {
                MM_Notification.WriteLine(ConsoleColor.Red, "Error sending {0} to {1}: {2}", Method.Name, "ADMIN", ex.Message);
            }
        }

        /// <summary>
        /// Handle message completion
        /// </summary>
        /// <param name="ar"></param>
        private static void MessageSentCompletion(IAsyncResult ar)
        {
            MM_Administrator_Types FoundConnection = (MM_Administrator_Types)ar.AsyncState;
            //FoundConnection.User.LastSentMessage = DateTime.Now;
            //MM_Notification.WriteLine(ConsoleColor.Green, "Sent {0} to {1}", "?", FoundConnection.User.UserName);

        }

    }
}