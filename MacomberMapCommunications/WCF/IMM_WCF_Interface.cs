﻿using MacomberMapCommunications.Messages;
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
    /// This interface aggregates all of the interprocess interfaces that are shared with clients.
    /// </summary>
    [ServiceContract(CallbackContract=typeof(IMM_ConversationMessage_Types))]
    public interface IMM_WCF_Interface : IMM_EMS_Types, IMM_Base_Types, IMM_Historic_Types, IMM_Login_Types, IMM_Database_Types, IMM_Conversation_Types
    {
        /// <summary>
        /// Simply ping the server, just to make sure everything is still working.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool Ping();

        /// <summary>
        /// Report the user status
        /// </summary>
        /// <param name="Status"></param>
        /// <returns></returns>
        [OperationContract]
        bool ReportUserStatus(MM_Client_Status Status);
    }
}