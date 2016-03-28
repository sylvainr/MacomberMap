using MacomberMapAdministrationService;
using MacomberMapCommunications.Messages.EMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapAdministrationConsole
{
    /// <summary>
    /// Handle an incoming message type
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class MM_AdministratorMessage_Types : IMM_AdministratorMessage_Types
    {
        /// <summary>The context of our handler</summary>
        public InstanceContext Context;

        /// <summary>The server</summary>
        public MM_Administrator_Types Server;

        /// <summary>
        /// Initialize a new administratior message type
        /// </summary>
        public MM_AdministratorMessage_Types()
        {
            this.Context = new InstanceContext(this);
        }

        /// <summary>
        /// Add an EMS command
        /// </summary>
        /// <param name="Command"></param>
        public void AddEMSCommands(MM_EMS_Command[] Command)
        {
            frm_Command_Information.Instance.AddCommands(Command,Server);
        }
    }
}
