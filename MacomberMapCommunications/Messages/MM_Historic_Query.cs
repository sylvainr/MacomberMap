using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the information on a historic query.
    /// </summary>
    public class MM_Historic_Query
    {
        #region Variable declarations
        /// <summary>The unique ID of the query</summary>
        public Guid Id;

        /// <summary>Our collection of query states</summary>
        public enum enumQueryState
        {
            /// <summary>Status unknown</summary>
            Unknown,

            /// <summary>The query is queued</summary>
            Queued,

            /// <summary>The query is in progress</summary>
            InProgress,

            /// <summary>The query had an error</summary>
            HadError,

            /// <summary>The query has completed</summary>
            Completed,

            /// <summary>The query has been removed from the server queue</summary>
            Removed,

            /// <summary>The query has been retrieved by the client</summary>
            Retrieved
        }

        /// <summary>The state of our query</summary>
        public enumQueryState State = enumQueryState.Unknown;

        /// <summary>The table schema of our query</summary>
        public String TableSchema;

        /// <summary>The bytes that correspond to the table data</summary>
        public byte[] SerializedTableData;

        /// <summary>The text of our query</summary>
        public String QueryText;
       
        /// <summary>The timestamp of the last time the query was updated</summary>
        public DateTime LastUpdate;

        /// <summary>The start time of our query</summary>
        public DateTime StartTime;

        /// <summary>The end time of the values</summary>
        public DateTime EndTime;

        /// <summary>The number of interpolated points to retrieve</summary>
        public int NumPoints;
        #endregion

        /// <summary>
        /// Update the state of our query, and reflect the last update
        /// </summary>
        /// <param name="State"></param>
        public void UpdateState(enumQueryState State)
        {
            this.State = State;
            this.LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Convert a data table to a binary object - profound thanks to blogs.msdn.com/b/shitals/archive/2009/12/04/9932598.aspx
        /// </summary>
        /// <param name="InTable"></param>
        public void SerializeTable(DataTable InTable)
        {
            //Store our data in an object array
            object[][] tableItems = new object[InTable.Rows.Count][];
            for (int rowIndex = 0; rowIndex < InTable.Rows.Count; rowIndex++)
                tableItems[rowIndex] = InTable.Rows[rowIndex].ItemArray;

            //Turn our table object data into bytes
            BinaryFormatter sFormat = new BinaryFormatter();
            using (MemoryStream mS = new MemoryStream())
            {
                sFormat.Serialize(mS, tableItems);
                this.SerializedTableData = mS.ToArray();
            }

            //Grab our schema information
            using (StringWriter sW = new StringWriter())
            {
                InTable.WriteXmlSchema(sW);
                this.TableSchema = sW.ToString();
            }
        }

        /// <summary>
        /// Return a data table with de-serialized results
        /// </summary>
        /// <returns></returns>
        public DataTable DeserializeTable()
        {
            DataTable OutTable = new DataTable();
            OutTable.ReadXmlSchema(new StringReader(TableSchema));

            BinaryFormatter sFormat = new BinaryFormatter();
            object[][] InRows;
            using (MemoryStream mS = new MemoryStream(SerializedTableData))
                InRows = (object[][])sFormat.Deserialize(mS);

            OutTable.MinimumCapacity = InRows.Length;
            OutTable.BeginLoadData();
            for (int rowIndex = 0; rowIndex < InRows.Length; rowIndex++)
                OutTable.Rows.Add(InRows[rowIndex]);
            OutTable.EndLoadData();
            return OutTable;
        }
    }
}
