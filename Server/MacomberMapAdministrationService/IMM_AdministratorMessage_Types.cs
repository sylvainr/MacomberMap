using MacomberMapCommunications.Messages.EMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapAdministrationService
{
    /// <summary>
    /// © 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides our interface for messaging from server to Macomber Map administrator via push methods
    /// </summary>
    [ServiceContract]
    public interface IMM_AdministratorMessage_Types
    {
        /// <summary>
        /// Note the entry of a new EMS command
        /// </summary>
        /// <param name="Command"></param>
        [OperationContract(IsOneWay = true)]
        void AddEMSCommands(MM_EMS_Command[] Command);
    }
}
