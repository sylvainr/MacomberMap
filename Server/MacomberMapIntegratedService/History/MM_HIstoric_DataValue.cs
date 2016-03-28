using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService.History
{

    /// <summary>
    /// This class holds information on a historic value
    /// </summary>
    public class MM_Historic_DataValue
    {
        /// <summary>The value retrieved</summary>
        public object Value { get; set; }

        /// <summary>The time stamp associated with our value</summary>
        public DateTime TimeStamp { get; set; }
    }
}
