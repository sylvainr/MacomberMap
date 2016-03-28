using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a database model
    /// </summary>
    [RetrievalCommand("LoadDatabaseModels")]
    public class MM_Database_Model
    {
        /// <summary>The full class of our model</summary>
        public String FullClass { get; set; }

        /// <summary>The category of our model (e.g., production, future)</summary>
        public String ModelCategory { get; set; }

        /// <summary>When our model becomes valid</summary>
        public DateTime ValidDate { get; set; }

        /// <summary>The unique identifier for our model</summary>
        public int ID { get; set; }

        /// <summary>The file name for our model</summary>
        public string FileName { get; set; }
    }
}
