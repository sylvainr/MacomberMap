using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Macomber_Map.Data_Connections.Generic;
using System.Data;
using System.Xml;
using System.Data.OleDb;
using Macomber_Map.Data_Elements;

namespace Macomber_Map.Data_Connections.Historic
{
    /// <summary>
    /// This class provides an interface to the historic connector
    /// </summary>
    public class MM_Historic_Connector : MM_DataConnector
    {
        #region Variable declarations
        /// <summary>Our connection to a historic system</summary>
        public OleDbConnection HistoricConnection;

        /// <summary>Our connection string to our historic database</summary>
        public string ConnectionString;

        /// <summary>Our query string to search for tags</summary>
        public string TagQueryString;
            
        /// <summary>Our query string to search for values</summary>
        public string ValuesQueryString;

        /// <summary>The reference to our tag title</summary>
        public string TagTitleReference;

        /// <summary>The reference to our tag description</summary>
        public string TagDescriptionReference;

        /// <summary>The reference to our point tag</summary>
        public string PointTagReference;

        /// <summary>The reference to our point tag</summary>
        public string PointValueReference;

        /// <summary>The reference to our point date</summary>
        public string PointDateReference;
        #endregion

        #region Initialization
        /// <summary>
        /// Intiailize a new historic connector
        /// </summary>
        public MM_Historic_Connector(XmlElement xElem)
            : base("Historic")
        {
            MM_Serializable.ReadXml(xElem, this, false);
            HistoricConnection = new OleDbConnection(ConnectionString);
            HistoricConnection.Open();
        }
        #endregion

        /// <summary>
        /// Attempt to locate tags that fit a particular criteria
        /// </summary>
        /// <param name="TagSQL">The paramters to pass to the query</param>
        /// <param name="AddTag">The delegate to flag if found</param>       
        /// <param name="CompletedDelegate">The delegate for completion of all historic tags</param>
        public void QueryTags(string TagSQL, TagAssignmentDelegate AddTag, TagQueryCompleted CompletedDelegate)
        {
            try
            {
                using (OleDbCommand oCmd = new OleDbCommand(TagQueryString, HistoricConnection))
                {
                    oCmd.Parameters.AddWithValue("Title", TagSQL);
                    using (OleDbDataReader oRd = oCmd.ExecuteReader())
                        while (oRd.Read())
                            AddTag(new MM_Historic_DataPoint(this, oRd[TagTitleReference].ToString(), oRd[TagDescriptionReference].ToString()));
                }
                CompletedDelegate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding historic tag " + TagSQL + ": " + ex.Message);
            }
        }

        /// <summary>
        /// The delegate for handling tag assignments
        /// </summary>
        /// <param name="Tag"></param>
        public delegate void TagAssignmentDelegate(MM_Historic_DataPoint Tag);


        /// <summary>The delegate for handling a tag query completion</summary>
        public delegate void TagQueryCompleted();


        /// <summary>Report the state of our historic connector</summary>
        public override System.Data.ConnectionState State
        {
            get { return HistoricConnection.State; }
        }

        /// <summary>Execute a command against the historic connector</summary>
        /// <param name="CommandText"></param>
        public override void ExecuteCommand(string CommandText)
        {
            using (OleDbCommand oCmd = new OleDbCommand(CommandText, HistoricConnection))
                oCmd.ExecuteNonQuery();
        }

        /// <summary>Disconnect from the historic query</summary>
        public override void Disconnect()
        {
            HistoricConnection.Close();
        }

        /// <summary>
        /// Initiate a new historic query
        /// </summary>
        /// <param name="QueryConfiguration"></param>
        /// <param name="DataSource"></param>
        /// <returns></returns>
        public override Guid Query(MM_Query_Configuration QueryConfiguration, MM_Data_Source DataSource)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove a query from the active queue
        /// </summary>
        /// <param name="QueryGuid"></param>
        public override void RemoveQuery(Guid QueryGuid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add a component to our query
        /// </summary>
        /// <param name="qConfig"></param>
        /// <param name="qDs"></param>
        /// <param name="DbLinkage"></param>
        /// <param name="Application"></param>
        /// <param name="TargetTable"></param>
        public override void AddToQuery(MM_Query_Configuration qConfig, DataSet qDs, XmlElement DbLinkage, string Application, string TargetTable)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determine our network source from our historic system
        /// </summary>
        /// <param name="value"></param>
        public override void SetNetworkSource(MM_Data_Source value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Re-execute a query
        /// </summary>
        /// <param name="Query"></param>
        public override void Reexecute(MM_DataQuery Query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Pause all queries
        /// </summary>
        public override void PauseQueries()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Refresh queries
        /// </summary>
        public override void RefreshQueries()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resume queries
        /// </summary>
        public override void ResumeQueries()
        {
            throw new NotImplementedException();
        }
    }
}
