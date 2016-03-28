using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Attributes
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the attribute necessary for retrieving data
    /// </summary>
    public class RetrievalCommandAttribute : Attribute
    {
        /// <summary>
        /// The command that is associated with retrieving our data
        /// </summary>
        public String RetrievalCommand { get; set; }
   
        /// <summary>
        /// Initialize a new retrieval command
        /// </summary>
        /// <param name="RetrievalCommand"></param>
        public RetrievalCommandAttribute(String RetrievalCommand)
        {
            this.RetrievalCommand = RetrievalCommand;
        }

    }
}