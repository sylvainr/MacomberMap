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
    [RetrievalCommand("GetOperatorshipUpdates"), UpdateCommand("UpdateOperatorshipData"), FileName("OPERATORSHIP_DATA.csv"), RemovalCommand("", "TEID"), UpdateParametersAttribute(ErrorTime = int.MaxValue, WarningTime = int.MaxValue)]
    public class MM_Operatorship_Update
    {
        /// <summary>The TEID of our element</summary>
        public int TEID { get; set; }

        /// <summary>The new owner of the equipment</summary>
        public String Owner { get; set; }
    }
}
