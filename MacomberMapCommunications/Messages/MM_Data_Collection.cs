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
    /// This class holds information on a data set, including its name, count, and latest values
    /// </summary>
    [RetrievalCommand("CheckLatestTimestamps")]
    public class MM_Data_Collection
    {
        /// <summary>The update message to be handled</summary>
        public String UpdateMessage;

        /// <summary>When our last update occurred</summary>
        public DateTime LastUpdate;

        /// <summary>When our last update occurred, how many rows of data were presented</summary>
        public int LastRowCount;

        /// <summary>
        /// Mark an update to our data collection
        /// </summary>
        /// <param name="NumRows"></param>
        public void MarkUpdate(int NumRows)
        {
            this.LastRowCount = NumRows;
            this.LastUpdate=DateTime.Now;
        }
    }
}
