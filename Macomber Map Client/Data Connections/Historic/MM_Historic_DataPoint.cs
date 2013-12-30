using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;

namespace Macomber_Map.Data_Connections.Historic
{
    /// <summary>
    /// This class provides an interface to a historic data point
    /// </summary>
    public class MM_Historic_DataPoint
    {
        #region Variable declarations
        /// <summary>The title of our historic data point</summary>
        public String Title { get; set; }

        /// <summary>The description for our historic data point</summary>
        public String Description { get; set; }

        /// <summary>Our historic connector</summary>
        public MM_Historic_Connector Conn;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new data point
        /// </summary>
        /// <param name="Conn"></param>
        public MM_Historic_DataPoint(MM_Historic_Connector Conn, String Title, String Description)
        {
            this.Conn = Conn;
            this.Title = Title;
            this.Description = Description;
        }
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
            using (OleDbCommand oCmd = new System.Data.OleDb.OleDbCommand(Conn.ValuesQueryString, Conn.HistoricConnection))
            {
                oCmd.Parameters.AddWithValue("Tag", Title);
                oCmd.Parameters.Add("StartTime", OleDbType.Date).Value = StartTime;
                oCmd.Parameters.Add("EndTime", OleDbType.Date).Value =  EndTime;
                using (OleDbDataReader oRd = oCmd.ExecuteReader())
                    while (oRd.Read())
                        OutList.Add(new MM_Historic_DataValue(Convert.ToDateTime(oRd[Conn.PointDateReference]), oRd[Conn.PointValueReference]));
                return OutList;
            }
        #endregion
        }
    }
}
