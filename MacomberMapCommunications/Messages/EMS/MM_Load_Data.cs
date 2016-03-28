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
    /// This class holds information on load data
    /// </summary>
   [RetrievalCommand("GetLoadData"), UpdateCommand("UpdateLoadData"), FileName("SE_LOAD_DATA.csv")]

    public class MM_Load_Data
    {
        /// <summary></summary>
        public int TEID_Ld { get; set; }
        
       /// <summary></summary>
        public float W_Ld { get; set; }

       /// <summary></summary>
        public float WMeas_Ld { get; set; }

       /// <summary></summary>
        public float R_Ld { get; set; }

       /// <summary></summary>
        public float RMeas_Ld { get; set; }

       /// <summary></summary>
        public bool Open_Ld { get; set; }

       /// <summary>The bus the load is connected to</summary>
        public int Conn_Bus { get; set; }

       /// <summary></summary>
       public bool Manual_Ld {get;set;}

       /// <summary></summary>
       public float WM_Ld {get;set;}

       /// <summary></summary>
       public float WMX_Ld { get; set; }

       /// <summary></summary>
       public float WLoad_LdArea {get;set;}

       /// <summary></summary>
       public float RM_Ld {get;set;}

       /// <summary></summary>
       public bool RMVEnabl_Ld {get;set;}

       /// <summary></summary>
       public bool Remove_Ld {get;set;}                     
    }

}
