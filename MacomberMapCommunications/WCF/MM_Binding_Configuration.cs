using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.WCF
{
    /// <summary>
    /// (C) 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides configuration for all WCF bindings, to streamline configuration
    /// </summary>
    public static class MM_Binding_Configuration
    {
        /// <summary>
        /// Create a new Net.TCP binding
        /// </summary>
        public static NetTcpBinding CreateBinding()
        {
            NetTcpBinding TcpBinding = new NetTcpBinding(SecurityMode.None, false);
            int TargetSize = 1024 * 1024 * 90;
            TcpBinding.MaxBufferPoolSize = 0;
            TcpBinding.MaxBufferSize = TargetSize;
           
            TcpBinding.ReaderQuotas.MaxArrayLength = TargetSize;
            TcpBinding.MaxReceivedMessageSize = TargetSize;
            TcpBinding.ReaderQuotas.MaxStringContentLength = TargetSize;
            TcpBinding.TransferMode = TransferMode.Buffered;
            TcpBinding.SendTimeout = TimeSpan.FromSeconds(60);
            return TcpBinding;
        }
    }
}
