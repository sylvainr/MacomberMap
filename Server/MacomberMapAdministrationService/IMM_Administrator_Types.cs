using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.EMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapAdministrationService
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the interface and class to our administrator system
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IMM_AdministratorMessage_Types))]
    public interface IMM_Administrator_Types
    {
        /// <summary>
        /// Retrieve the user information
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_User[] GetUserInformation();

        /// <summary>
        /// Load the system information, including CPU usage of all MM-related services, and free memory
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_System_Information GetSystemInformation();

        /// <summary>
        /// Send a message to a particular user
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Message"></param>
        /// <param name="MessageSource"></param>
        /// <param name="Icon"></param>
        [OperationContract]
        void SendMessage(MM_User User, String Message, String MessageSource, MessageBoxIcon Icon);

        /// <summary>
        /// Send a message to all users
        /// </summary>
        /// <param name="Icon"></param>
        /// <param name="MessageSource"></param>
        /// <param name="Message"></param>
        [OperationContract]
        void SendMessageToAllClients(String Message, String MessageSource, MessageBoxIcon Icon);

        /// <summary>
        /// Close a client
        /// </summary>
        /// <param name="Client"></param>
        [OperationContract]
        void CloseClient(MM_User Client);

        /// <summary>
        /// Close all clients
        /// </summary>
        [OperationContract]
        void CloseAllClients();

        /// <summary>
        /// Generate a savecase, and save to our server
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Savecase GenerateSavecase();

        /// <summary>
        /// Apply a savecase to our system
        /// </summary>
        /// <param name="Savecase"></param>
        /// <returns></returns>
        [OperationContract]
        bool ApplySavecase(MM_Savecase Savecase);

        /// <summary>
        /// Set our server description
        /// </summary>
        /// <param name="ServerDescription"></param>
        /// <returns></returns>
        [OperationContract]
        bool SetServerDescription(String ServerDescription);

        /// <summary>
        /// Determine whether the clients are stress tested
        /// </summary>
        /// <param name="Active"></param>
        /// <returns></returns>
        [OperationContract]
        bool SetServerClientStressTest(bool Active);

        /// <summary>
        /// Register for our callback
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool RegisterCallback(String VersionNumber = "Unknown");

        /// <summary>
        /// Retrieve our historical list of EMS commands
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_EMS_Command[] GetEMSCommands();

        /// <summary>
        /// Send a command to our system
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        [OperationContract]
        bool SendCommand(String Command);
    }
}
