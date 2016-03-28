using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.EMS
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on line data
    /// </summary>
    [RetrievalCommand("GetInterfaceData"), UpdateCommand("UpdateInterfaceData"), FileName("RTMONI_RTMGRP_DATA.csv")]
    public class MM_Interface_Monitoring_Data
    {
        /// <summary></summary>
        public string ID_RTMGRP { get; set; }

        /// <summary></summary>
        public float RTOTMW_RTMGRP { get; set; }

        /// <summary></summary>
        public float LTOTMW_RTMGRP { get; set; }
    }
}