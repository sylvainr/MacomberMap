using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Attributes
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the attribute necessary for file name handling
    /// </summary>
    public class FileNameAttribute : Attribute
    {
        /// <summary>
        /// The file name that is associated with retrieving our data
        /// </summary>
        public String FileName { get; set; }
   
        /// <summary>
        /// Initialize a new retrieval command
        /// </summary>
        /// <param name="FileName"></param>
        public FileNameAttribute(String FileName)
        {
            this.FileName = FileName;
        }

    }
}