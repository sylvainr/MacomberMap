using MacomberMapCommunications.Messages;
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
    /// This class provides the interface and class to our base system
    /// </summary>
    [ServiceContract]
    public interface IMM_Base_Types
    {
      

        /// <summary>Report the server name</summary>
        /// <returns></returns>
        [OperationContract]
        String ServerName();

        /// <summary>Report the server version</summary>
        /// <returns></returns>
        [OperationContract]
        String ServerVersion();

        /// <summary>Report the server uptime</summary>
        [OperationContract]
        TimeSpan ServerUptime();

        /// <summary>
        /// Handle a user logout
        /// </summary>
        /// <param name="UserGuid">The user's GUID</param>
        [OperationContract]
        bool HandleUserLogout(Guid UserGuid);      
      
    }
}
