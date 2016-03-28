using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on an EPS Meter
    /// </summary>
    public class MM_EPSMeter : MM_Element
    {
        #region Variable declarations
        /// <summary>The resource ID of the pricing vector</summary>
        public string RID
        {
            get { return _RID; }
            set { _RID = value; }
        }
        private string _RID;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new node from a data reader
        /// </summary>
        /// <param name="AddIfNew"></param>
        /// <param name="ElementSource"></param>
        public MM_EPSMeter(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
        }

        /// <summary>
        /// Initialize a new node from an XML document
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew"></param>
        public MM_EPSMeter(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
        }

        /// <summary>
        /// Initialize a parameterless node
        /// </summary>
        public MM_EPSMeter()
            : base()
        { }
        #endregion
    }
}
