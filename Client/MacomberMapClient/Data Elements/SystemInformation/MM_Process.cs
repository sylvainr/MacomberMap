using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.Data_Elements.SystemInformation
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds all relevant information for a process
    /// </summary>
    public class MM_Process : MM_Serializable
    {
        #region Variable declaration
        /// <summary>The configuration element for this process</summary>
        public XmlElement BaseElement;

        /// <summary>The last execution time for this process</summary>
        public DateTime LastExecution = DateTime.FromOADate(0);

        /// <summary>The list view item associated with this process</summary>
        public ListViewItem lvItem;

        /// <summary>The time at which the process viewer goes into a warning state</summary>
        public TimeSpan WarningTime;

        /// <summary>The time at which the process viewer goes into an error state</summary>
        public TimeSpan ErrorTime;

        /// <summary>
        /// Return the application of the process
        /// </summary>
        public String Application
        {
            get { return BaseElement.Attributes["Application"].Value; }
            set { BaseElement.Attributes["Application"].Value = value; }

        }

        /// <summary>
        /// Return the database of the process
        /// </summary>
        public String Database
        {
            get { return BaseElement.Attributes["Database"].Value; }
            set { BaseElement.Attributes["Database"].Value = value; }
        }

        /// <summary>
        /// Return the Record of the process
        /// </summary>
        public String Record
        {
            get { return BaseElement.Attributes["Record"].Value; }
            set { BaseElement.Attributes["Record"].Value = value; }
        }

        /// <summary>The table name </summary>
        public String TableName
        {
            get { return BaseElement.Attributes["Application"].Value + "_" + BaseElement.Attributes["Database"].Value + "_" + BaseElement.Attributes["Record"].Value + ".EMS"; }
        }

        /// <summary>
        /// Return the column name for the process
        /// </summary>
        public String ColumnName
        {
            get { return BaseElement.Attributes["Field"].Value; }
            set { BaseElement.Attributes["Field"].Value = value; }
        }


        /// <summary>
        /// Return the name for the process
        /// </summary>
        public String Name
        {
            get { return BaseElement.Attributes["Name"].Value; }
            set { BaseElement.Attributes["Name"].Value = value; }
        }


        /// <summary>
        /// Return the key for the process
        /// </summary>
        public String Key
        {
            get { return BaseElement.Attributes["Key"].Value; }
            set { BaseElement.Attributes["Key"].Value = value; }
        }

        /// <summary>
        /// Return the field for the process
        /// </summary>
        public String Field
        {
            get { return BaseElement.Attributes["Field"].Value; }
            set { BaseElement.Attributes["Field"].Value = value; }
        }

        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the Macomber Map Process viewer
        /// </summary>
        /// <param name="BaseElement"></param>
        public MM_Process(XmlElement BaseElement)
        {
            this.BaseElement = BaseElement;
            if (BaseElement.HasAttribute("LastExecution"))
                LastExecution = XmlConvert.ToDateTime(BaseElement.Attributes["LastExecution"].Value, XmlDateTimeSerializationMode.Unspecified);
            if (BaseElement.HasAttribute("WarningTime"))
                WarningTime = XmlConvert.ToTimeSpan(BaseElement.Attributes["WarningTime"].Value);
            if (BaseElement.HasAttribute("ErrorTime"))
                ErrorTime = XmlConvert.ToTimeSpan(BaseElement.Attributes["ErrorTime"].Value);
        }
        #endregion


    }
}
