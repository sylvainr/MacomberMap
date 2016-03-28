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
    /// This class holds information on system level charting data that we care about
    /// </summary>
    [RetrievalCommand("GetChartData"), UpdateCommand("UpdateChartData"), FileName("GEN_CHART_DATA.csv")]
    public class MM_Chart_Data
    {
        /// <summary></summary>
        public String Chart_ID { get; set; }

        /// <summary></summary>
        public float A_SAMPLE { get; set; }

        /// <summary></summary>
        public float B_SAMPLE { get; set; }

        /// <summary></summary>
        public DateTime XT_SAMPLE { get; set; }
    }
}
