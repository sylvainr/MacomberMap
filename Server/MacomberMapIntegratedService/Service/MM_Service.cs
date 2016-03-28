using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MacomberMapIntegratedService.Service;

namespace MacomberMapIntegratedService.Service
{
  /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the actual service startup/shutdown events
    /// </summary>
    public partial class MM_Service : ServiceBase
    {
        
        #region Variable declarations
        /// <summary>The thread for running the MM service</summary>
        public Thread RunThread;
        #endregion

        #region Initialization
        /// <summary>
        /// Start up the Macomber Map service
        /// </summary>
        public MM_Service()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle the service starting up
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            RunThread = new Thread(new ThreadStart(MM_Server.StartServer));
            RunThread.Name = "Macomber Map Server thread";
            RunThread.IsBackground = true;
            RunThread.Start();
        }

        /// <summary>
        /// Handle the service shutting down.
        /// </summary>
        protected override void OnStop()
        {
            if (RunThread != null && RunThread.ThreadState == ThreadState.Running)
                RunThread.Abort();
            MM_Notification.Notify("Macomber Map Service Shutting Down", "The Macomber Map Service is shutting down.");
        }
        #endregion
    }
}
