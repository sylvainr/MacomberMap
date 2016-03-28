using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.WCF
{
     /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides our interface for messaging from server to Macomber Map clients via push methods
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IMM_ConversationMessage_Types))]
    public interface IMM_Conversation_Types
    {
        /// <summary>
        /// Register for our callback
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool RegisterCallback(String VersionNumber="Unknown");
    }
}
