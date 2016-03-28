using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MacomberMapCommunications.Attributes;

namespace MacomberMapCommunications.Messages
{
    /// <summary>
    /// price node data
    /// </summary>
    [RetrievalCommand("GetLmpData"), UpdateCommand("UpdateLmpData"), FileName("Pnode_LMP_DATA.csv")]
    public class MM_LMP_Data
    {
        /// <summary>
        /// Station for this pnode
        /// </summary>
        public string Station { get; set; }

        /// <summary>
        /// price node name
        /// </summary>
        public string PnodeName { get; set; }

        /// <summary>
        /// Associated resource (if there is one)
        /// </summary>
        public string ResourceName { get; set; }
        /// <summary>
        /// locational marginal price
        /// </summary>
        public float Lmp { get; set; }

    }
}
