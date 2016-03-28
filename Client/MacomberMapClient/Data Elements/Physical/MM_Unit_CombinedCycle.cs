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
    /// This class holds information on a combined cycle unit
    /// </summary>
    public class MM_Unit_CombinedCycle: MM_Unit
    {  
        #region Variable declarations
        /// <summary>Our collection of units</summary>
        public MM_Unit[] Units;

        /// <summary>Our collection of configurations</summary>
        public int[] Configurations;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new Combined cycle configuration
        /// </summary>
        public MM_Unit_CombinedCycle()
            : base()
        { }

        /// <summary>
        /// Initialize a new Combined cycle configuration
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Unit_CombinedCycle(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        { }

        /// <summary>
        /// Initialize a new Combined cycle configuration
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Unit_CombinedCycle(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        { }
        #endregion

    }
}
