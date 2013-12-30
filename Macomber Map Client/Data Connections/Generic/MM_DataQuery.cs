using System;
using System.Collections.Generic;
using System.Text;
using MM_Communication.Data_Integration;

namespace Macomber_Map.Data_Connections
{
    /// <summary>
    /// This abstract class contains the collection of data queries, including their refresh time and last refresh.
    /// </summary>
    public class MM_DataQuery
    {
        /// <summary>The GUID of the query</summary>
        public Guid Guid;

        /// <summary>How often the query should be refreshed (in seconds)</summary>
        public int RefreshTime;

        /// <summary>The current status of the query</summary>
        public MM_Integration.enumQueryStatus QueryStatus;

        /// <summary>The time of the item's last refresh</summary>
        public DateTime LastRefresh = DateTime.FromOADate(0);

        /// <summary>How much time the data query took last time</summary>
        public TimeSpan LastSpan;

        /// <summary>The name of the query</summary>
        public String QueryName;

        /// <summary>The amount of time since the last refresh</summary>
        public TimeSpan SinceLast
        {
            get
            {
                return DateTime.Now - LastRefresh;
            }
        }

        /// <summary>The priority of the query. The lower the number, the higher the priority.</summary>
        public int Priority;

        /// <summary>
        /// Return the name of the query on conversion
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return QueryName;
        }

        /// <summary>
        /// Create a new data query
        /// </summary>
        /// <param name="Priority">The priority for the query</param>
        public MM_DataQuery(int Priority)
        {
            this.Priority = Priority;
        }
    }
}
