using MacomberMapAdministrationService;
using MacomberMapCommunications.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapAdministrationConsole
{
    /// <summary>
    /// This class wraps the administrator type interface to support proxy handling
    /// </summary>
    public class MM_Administrator_Types : ClientBase<IMM_Administrator_Types>, IMM_Administrator_Types
    {
        #region Variable declarations
        /// <summary>Our inner channel</summary>
        private IMM_Administrator_Types innerChannel;

        /// <summary>The context of our connection</summary>
        public InstanceContext Context; 
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our administrator types
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="remoteAddress"></param>
        /// <param name="callbackInstance"></param>
        public MM_Administrator_Types(InstanceContext callbackInstance,System.ServiceModel.Channels.Binding binding, EndpointAddress remoteAddress)
            : base(callbackInstance, binding, remoteAddress)
        {
            innerChannel = (IMM_Administrator_Types)base.InnerChannel;
            Context = callbackInstance;
        }
        #endregion

        /// <summary>
        /// Get our user information
        /// </summary>
        /// <returns></returns>
        public MM_User[] GetUserInformation()
        {
            return innerChannel.GetUserInformation();
        }

        /// <summary>
        /// Retrieve the system information
        /// </summary>
        /// <returns></returns>
        public MM_System_Information GetSystemInformation()
        {
            return innerChannel.GetSystemInformation();
        }

        /// <summary>
        /// Send a message to one client
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Message"></param>
        /// <param name="MessageSource"></param>
        /// <param name="Icon"></param>
        public void SendMessage(MM_User User, string Message, string MessageSource, MessageBoxIcon Icon)
        {
            innerChannel.SendMessage(User, Message, MessageSource, Icon);
        }

        /// <summary>
        /// Send message to all clients
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="MessageSource"></param>
        /// <param name="Icon"></param>
        public void SendMessageToAllClients(string Message, string MessageSource, MessageBoxIcon Icon)
        {
            innerChannel.SendMessageToAllClients(Message, MessageSource, Icon);
        }

        /// <summary>
        /// Close a single client
        /// </summary>
        /// <param name="Client"></param>
        public void CloseClient(MM_User Client)
        {
            try
            {
            innerChannel.CloseClient(Client);
            }
            catch { }
        }

        /// <summary>
        /// Close all clients
        /// </summary>
        public void CloseAllClients()
        {
            try
            {
                innerChannel.CloseAllClients();
            }
            catch { }
        }

        /// <summary>
        /// Generate our savecase
        /// </summary>
        /// <returns></returns>
        public MM_Savecase GenerateSavecase()
        {
            return innerChannel.GenerateSavecase();
        }

        /// <summary>
        /// Apply our savecase
        /// </summary>
        /// <param name="Savecase"></param>
        /// <returns></returns>
        public bool ApplySavecase(MM_Savecase Savecase)
        {
            return innerChannel.ApplySavecase(Savecase);
        }

        /// <summary>
        /// Apply our server description
        /// </summary>
        /// <param name="ServerDescription"></param>
        /// <returns></returns>
        public bool SetServerDescription(string ServerDescription)
        {
            return innerChannel.SetServerDescription(ServerDescription);
        }

        /// <summary>
        /// Set our stress tester
        /// </summary>
        /// <param name="Active"></param>
        /// <returns></returns>
        public bool SetServerClientStressTest(bool Active)
        {
            return innerChannel.SetServerClientStressTest(Active);
        }

        /// <summary>
        /// Register our callback
        /// </summary>
        /// <param name="VersionNumber"></param>
        /// <returns></returns>
        public bool RegisterCallback(string VersionNumber = "Unknown")
        {
            return innerChannel.RegisterCallback(VersionNumber);
        }

        /// <summary>
        /// Get our EMS commands
        /// </summary>
        /// <returns></returns>
        public MacomberMapCommunications.Messages.EMS.MM_EMS_Command[] GetEMSCommands()
        {
            return innerChannel.GetEMSCommands();
        }

        /// <summary>
        /// Send an EMS command
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public bool SendCommand(string Command)
        {
            return innerChannel.SendCommand(Command);
        }

        /// <summary>
        /// Report an easy-to-read string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Endpoint.ListenUri.Host + ":" + this.Endpoint.ListenUri.Port.ToString();
        }
    }
}