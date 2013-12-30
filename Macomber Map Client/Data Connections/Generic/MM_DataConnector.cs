using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;
using Macomber_Map.Data_Connections.Generic;

namespace Macomber_Map.Data_Connections
{
    /// <summary>
    /// This class is the base for a data connector
    /// </summary>
    public abstract class MM_DataConnector
    {
        #region Variable initialization
        /// <summary>The name of the connector (e.g., Real-time EMS)</summary>
        public String ConnectorName;

        /// <summary>The current state of the connector</summary>
        public abstract ConnectionState State { get; }

        /// <summary>
        /// The delegate to handle the results of the query
        /// </summary>
        public delegate void QueryResultHandler(DataSet InResults, Guid QueryGuid);

        /// <summary>The collection of EMS commands</summary>
        public Dictionary<Guid, MM_DataQuery> Queries = new Dictionary<Guid, MM_DataQuery>();

        /// <summary>
        /// The state of the system
        /// </summary>
        public enum ActiveState
        {
            /// <summary>The data connector is inactive</summary>
            Inactive,
            /// <summary>The data connector is active and running</summary>
            Active,
            /// <summary>The data connector is active but paused</summary>
            Paused,
            /// <summary>The data connector is quitting</summary>
            Quitting
        };

        /// <summary>Whether the data connector is active</summary>
        public ActiveState Active = ActiveState.Inactive;
        #endregion

        #region Subroutines and events
        /// <summary>
        /// Initialize the data connector, and assign it a name
        /// </summary>
        /// <param name="ConnectorName"></param>
        public MM_DataConnector(String ConnectorName)
        {
            this.ConnectorName = ConnectorName;
        }

        /// <summary>
        /// Send a command text to a data connector
        /// </summary>
        /// <param name="CommandText">The text to be processed</param>
        public abstract void ExecuteCommand(String CommandText);



        /// <summary>
        /// Disconnect from the server
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Initiate a data query 
        /// </summary>
        /// <param name="QueryConfiguration">The configuration of the query</param>
        /// <param name="DataSource">The data source to be used for the query</param>
        /// <returns>The GUID of the query</returns>
        public abstract Guid Query(MM_Query_Configuration QueryConfiguration, MM_Data_Source DataSource);

        /// <summary>
        /// Close down and remove a query
        /// </summary>
        /// <param name="QueryGuid">The GUID of the query to remove</param>
        public abstract void RemoveQuery(Guid QueryGuid);

        /// <summary>
        /// Add an element to a data table, as appropriate
        /// </summary>
        /// <param name="qConfig">The query configuration</param>
        /// <param name="qDs">The query data set</param>
        /// <param name="DbLinkage">The database linkage XML element</param>
        /// <param name="Application">The target application</param>        
        /// <param name="TargetTable">The name of the table</param>
        public abstract void AddToQuery(MM_Query_Configuration qConfig, DataSet qDs, XmlElement DbLinkage, string Application, String TargetTable);

        /// <summary>
        /// Change the query's network source (e.g., StateEstimator, StudySystem, QKNET, SCADA)
        /// </summary>
        /// <param name="value">The new network source</param>
        public abstract void SetNetworkSource(MM_Data_Source value);

        /// <summary>
        /// Re-execute a query
        /// </summary>
        /// <param name="Query">The query to execute</param>
        public abstract void Reexecute(MM_DataQuery Query);

        /// <summary>Pause all running queries</summary>
        public abstract void PauseQueries();

        /// <summary>Refresh all study/non-automatically running queries</summary>
        public abstract void RefreshQueries();

        /// <summary>Resume all paused queries</summary>
        public abstract void ResumeQueries();
        #endregion
    }
}
