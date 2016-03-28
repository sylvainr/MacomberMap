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
    /// This class holds information on island data
    /// </summary>
    [RetrievalCommand("GetAnalogMeasurementData"), UpdateCommand("UpdateAnalogMeasurementData"), FileName("QKNET_ANALOG_MEAS.csv")]
    public class MM_Analog_Measurement
    {
        /// <summary>The TEID of our analog</summary>
        public int TEID { get; set; }

        /// <summary>The value of our analog</summary>
        public float Value_Meas { get; set; }

        /// <summary>Whether the analog is good?</summary>
        public bool Good_Meas { get; set; }
    }
}
