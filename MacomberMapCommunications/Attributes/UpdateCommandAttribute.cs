using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Attributes
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the attribute necessary for updating data
    /// </summary>
    public class UpdateCommandAttribute : Attribute
    {
        /// <summary>
        /// The command that is associated with retrieving our data
        /// </summary>
        public String UpdateCommand { get; set; }

        /// <summary>
        /// Initialize a new retrieval command
        /// </summary>
        /// <param name="UpdateCommand"></param>
        public UpdateCommandAttribute(String UpdateCommand)
        {
            this.UpdateCommand = UpdateCommand;
        }
    }
}