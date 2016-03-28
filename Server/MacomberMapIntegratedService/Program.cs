using MacomberMapIntegratedService.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //If we're in console mode, display the console view. Otherwise, start the service mode.            
            if (Environment.UserInteractive)
            {
               // Console.SetWindowSize(132, 43);
                MM_Server.StartServer();
            }
            else
                ServiceBase.Run(new MM_Service());
        }
    }
}
