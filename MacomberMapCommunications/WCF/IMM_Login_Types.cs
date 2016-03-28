using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.WCF
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the interface and class to our login processes
    /// </summary>
    [ServiceContract]
    public interface IMM_Login_Types
    {
       
        /// <summary>Check the most recent client version</summary>
        /// <returns></returns>
        [OperationContract]
        Version CheckApplicationVersion(String ClientName="Macomber Map");

        /// <summary>
        /// Handle a user login, including passing a user-specific token
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [OperationContract]
        String[] HandleUserLogin(String Name, String Password);

        
        /// <summary>
        /// Get the configuration XML for our client, based on our login credentials
        /// </summary>
        /// <param name="ClientName"></param>
        /// <returns></returns>
        [OperationContract]
        String GetConfigurationXml(String ClientName = "Macomber Map");

        /// <summary>
        /// Retrieve the installer for a particular version of the application
        /// </summary>
        /// <param name="ClientName"></param>
        /// <param name="VersionToRetrieve"></param>
        /// <returns></returns>
        [OperationContract]
        byte[] GetApplicationVersion(String ClientName = "Macomber Map", Version VersionToRetrieve=default(Version));        
    }
}
