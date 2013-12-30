using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MM_Communication.Data_Integration
{
    /// <summary>
    /// This class holds information on an additional data source for a one-line
    /// </summary>
    public class MM_OneLine_AdditionalDataSource
    {
        #region Variable declarations
        /// <summary>The application to target</summary>
        public String Application;

        /// <summary>The database from which the data are retrieved</summary>
        public String Database;

        /// <summary>The value locator for an element in our table</summary>
        public String Values;

        /// <summary>The name of our table</summary>
        public String TableName;

        /// <summary>Our collection of database linkages</summary>
        public List<MM_OneLine_RecordLocator> DatabaseLinkages = new List<MM_OneLine_RecordLocator>();

        /// <summary>The ID of our table locator</summary>
        public int Id;

        /// <summary>The lookup name of our additional data source</summary>
        public string Name;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new table locator
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="Id"></param>
        /// <param name="Parent"></param>
        public MM_OneLine_AdditionalDataSource(XmlElement xElem, int Id, MM_OneLine_TableLocator Parent)
        {
            this.Id = Id;
            this.Database = xElem.Attributes["Database"].Value;
            this.Values = xElem.Attributes["Values"].Value;
            this.TableName = xElem.Attributes["TableName"].Value;
            this.Name = xElem.Attributes["Name"].Value;
            this.Application = xElem.Attributes["Application"].Value;
        }
        #endregion
    }
}
