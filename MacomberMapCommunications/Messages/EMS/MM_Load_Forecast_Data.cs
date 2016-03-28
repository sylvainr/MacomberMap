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
    /// This class holds information on load vs. load forecast data
    /// </summary>
    [RetrievalCommand("GetLoadForecastData"), UpdateCommand("UpdateLoadForecastData"), FileName("LOAD_FORECAST_DATA.csv"), UpdateParametersAttribute(ErrorTime = int.MaxValue, WarningTime = int.MaxValue)]
    public class MM_Load_Forecast_Data
    {
        /// <summary>The type of load forecast data</summary>
        public String Alias_FCA { get; set; }

        /// <summary>The time stamp for the load forecast time stamp</summary>
        public DateTime TimeEnd_H { get; set; }

        /// <summary>The load forecast</summary>
        public float FLD_H_FCA { get; set; }

        /// <summary>The actual load</summary>
        public float LD_H_FCA { get; set; }

        /// <summary>The manually-overriden load forecast point</summary>
        public float MLD_H_FCA { get; set; }
    }
}
