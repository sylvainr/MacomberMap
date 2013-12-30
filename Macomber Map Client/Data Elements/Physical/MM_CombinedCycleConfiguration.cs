using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.Common;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class holds information on a combined cycle unit
    /// </summary>
    public class MM_CombinedCycleConfiguration: MM_Unit
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
        public MM_CombinedCycleConfiguration()
            : base()
        { }

        /// <summary>
        /// Initialize a new Combined cycle configuration
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_CombinedCycleConfiguration(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        { }

        /// <summary>
        /// Initialize a new Combined cycle configuration
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_CombinedCycleConfiguration(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        { }
        #endregion

    }
}
