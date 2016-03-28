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
    /// This class contains information on an electrical bus
    /// </summary>
    public class MM_Electrical_Bus : MM_Element
    {
        #region Variable declarations
        /// <summary>The node with which the EB is associated</summary>    
        public MM_Node AssociatedNode;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new CIM Electrical Bus
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Electrical_Bus(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        { this.ElemType = MM_Repository.FindElementType("ElectricalBus"); }

        /// <summary>
        /// Initialize a new CIM Substation
        /// </summary>
        /// <param name="ElementSource">The data source for this substation</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Electrical_Bus(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            //Make sure we have this element type
            this.ElemType = MM_Repository.FindElementType("ElectricalBus");
        }

        /// <summary>
        /// Initialize a blank electrical bus
        /// </summary>
        public MM_Electrical_Bus()
        {
            this.ElemType = MM_Repository.FindElementType("ElectricalBus");
        }
        #endregion


    }
}
