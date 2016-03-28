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
    /// This class holds information on a state measurement (e.g., is a breaker closed)
    /// </summary>
    [RetrievalCommand("GetSCADAAnalogData"), UpdateCommand("UpdateSCADAAnalogData"), FileName("SCADA_MISC_ANALOG_DATA.csv"), UpdateParametersAttribute(ErrorTime = int.MaxValue, WarningTime = int.MaxValue)]
    public class MM_Scada_Analog
    {
        /// <summary></summary>
        public int TEID_ANALOG { get; set; }

        /// <summary></summary>
        public string SUBSTN { get; set; }

        /// <summary></summary>
        public enumDeviceType DEVTYP { get; set; }

        /// <summary></summary>
        public String Device { get; set; }

        /// <summary></summary>
        public enumMeasurementType Meas { get; set; }

        /// <summary></summary>
        public String Analog { get; set; }

        /// <summary></summary>
        public float Value { get; set; }

        /// <summary></summary>
        public enumSourceType Source { get; set; }

        /// <summary></summary>
        public enumQualityType Quality { get; set; }
    }
}