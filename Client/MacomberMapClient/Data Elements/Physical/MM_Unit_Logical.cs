using MacomberMapClient.Integration;
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
    /// This class holds information on a logical unit
    /// </summary>
    public class MM_Unit_Logical : MM_Unit
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
        public MM_Unit_Logical(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("LogicalUnit");
        }

        /// <summary>
        /// Initialize a new logical unit
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Unit_Logical(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("LogicalUnit");
        }

        /// <summary>
        /// Initialize a new logical unit
        /// </summary>
        public MM_Unit_Logical()
            : base()
        {
            this.ElemType = MM_Repository.FindElementType("LogicalUnit");
        }
        #endregion
    }
}
