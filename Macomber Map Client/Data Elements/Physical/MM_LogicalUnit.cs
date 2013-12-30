using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.Common;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class holds information on a logical unit
    /// </summary>
    public class MM_LogicalUnit: MM_Unit
    {
        #region Variable declarations
        /// <summary>The physical unit attached to this logical one</summary>
        public MM_Unit PhysicalUnit;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new logical unit
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_LogicalUnit(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource,AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("LogicalUnit");
        }
        
        /// <summary>
        /// Initialize a new logical unit
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_LogicalUnit(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource,AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("LogicalUnit");
        }

        /// <summary>
        /// Initialize a new logical unit
        /// </summary>
        public MM_LogicalUnit()
            : base()
        {
            this.ElemType = MM_Repository.FindElementType("LogicalUnit");
        }
        #endregion
    }
}
