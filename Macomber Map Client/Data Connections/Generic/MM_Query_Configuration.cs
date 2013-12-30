using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Macomber_Map.User_Interface_Components;

namespace Macomber_Map.Data_Connections.Generic
{
    /// <summary>
    /// This class holds the information necessary to execute a query
    /// </summary>
    public class MM_Query_Configuration
    {
        #region Variable declarations
        /// <summary>Whether the filter specifies the full row lookups to be returned </summary>
        public bool FullRecords;

        /// <summary>The procedure to be run upon data table update</summary>
        public delegate void UpdateProcedureDelegate();

        /// <summary>The procedure to be run upon data table update</summary>
        public UpdateProcedureDelegate UpdateProcedure;

        /// <summary>The GUID of the query</summary>
        public Guid QueryGUID;

        /// <summary>The name of the query</summary>
        public String QueryName;

        /// <summary>The priority of the query</summary>
        public int Priority = 15;

        /// <summary>The data source for the query</summary>
        public MM_Data_Source DataSource;

        /// <summary>The target table to be updated</summary>
        public DataSet TargetData;

        /// <summary>The data connector associated with the query</summary>
        public MM_DataConnector Connector;

        /// <summary>The list of objects associated with our query</summary>
        public Dictionary<int, Object> TargetObjects = new Dictionary<int, object>();

        /// <summary>The target window for our query</summary>
        public MM_Form TargetWindow;
        #endregion
    }
}
