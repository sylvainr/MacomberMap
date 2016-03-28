using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Attributes
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the attribute necessary for removal data
    /// </summary>
    public class RemovalCommandAttribute : Attribute
    {
        /// <summary>The command that is associated with removing our data</summary>
        public String RemovalCommand { get; set; }

        /// <summary>The index to identify what a column belongs to</summary>
        public String IndexColumn { get; set; }


        /// <summary>
        /// Initialize a new retrieval command
        /// </summary>
        /// <param name="RemovalCommand"></param>
        /// <param name="IndexColumn"></param>
        public RemovalCommandAttribute(String RemovalCommand, String IndexColumn)
        {
            this.RemovalCommand = RemovalCommand;
            this.IndexColumn = IndexColumn;
        }
    }
}