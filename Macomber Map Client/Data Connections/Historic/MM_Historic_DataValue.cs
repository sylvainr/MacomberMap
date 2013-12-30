using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Macomber_Map.Data_Connections.Historic
{
    /// <summary>
    /// This class holds information on a historic value
    /// </summary>
    public struct MM_Historic_DataValue
    {
        /// <summary>The value retrieved</summary>
        public object Value;

        /// <summary>The time stamp associated with our value</summary>
        public DateTime TimeStamp;

        /// <summary>
        /// Initialize a new data value
        /// </summary>
        /// <param name="TimeStamp"></param>
        /// <param name="value"></param>
        public MM_Historic_DataValue(DateTime TimeStamp, object value)
        {
            this.TimeStamp = TimeStamp;
            this.Value = value;
        }
    }
}
