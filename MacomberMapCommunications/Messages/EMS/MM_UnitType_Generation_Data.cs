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
    /// This class holds information on unit type generation data
    /// </summary>
    [RetrievalCommand("GetUnitTypeGenerationData"), UpdateCommand("UpdateUnitTypeGenerationData"), FileName("GEN_GTY_DATA.csv")]
    public class MM_UnitType_Generation_Data
    {
        /// <summary>The Operating Area</summary>
        public String ID_OPA { get; set; }

        /// <summary>The fuel type WIND, HYDRO, etc</summary>
        public string ID_GTY { get; set; }

        /// <summary></summary>
        public float RVSP_GTY { get; set; }

        /// <summary></summary>
        public float GEN_GTY { get; set; }
    }
}
