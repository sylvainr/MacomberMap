using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService.History
{
    /// <summary>
    /// This class provides an interface to a historic data point
    /// </summary>
    public class MM_Historic_DataPoint
    {
        #region Variable declarations
        /// <summary>The title (human facing) of our historic data point</summary>
        public String Title { get; set; }

        /// <summary>The description for our historic data point</summary>
        public String Description { get; set; }

        /// <summary>The name of our data point</summary>
        public string Name { get; set; }
        #endregion

        #region Queries
        /// <summary>
        /// Interpolate a number of points over a specified time range
        /// </summary>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="NumVariables"></param>
        /// <returns></returns>
        public List<MM_Historic_DataValue> InterpolateValues(DateTime StartTime, DateTime EndTime, int NumVariables)
        {
            List<MM_Historic_DataValue> OutList = new List<MM_Historic_DataValue>();

            return OutList;
        }
        #endregion

        /// <summary>
        /// Retrieve our value collection
        /// </summary>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="PointCount"></param>
        /// <returns></returns>
        public MM_Historic_DataValue[] RetrieveValues(DateTime StartTime, DateTime EndTime, int PointCount)
        {
            return new MM_Historic_DataValue[0];
        }
    }
}
