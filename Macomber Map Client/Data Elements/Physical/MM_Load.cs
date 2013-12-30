using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Xml;
using Macomber_Map.Data_Connections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class defines a generator/unit
    /// </summary>
    public class MM_Load : MM_Element, IComparable
    {

        #region Variable declarations        
        /// <summary>Any upcoming outages</summary>
        public MM_ScheduledOutage UpcomingOutage = null;

        /// <summary>The estimated MW of the load/// </summary>
        public float Estimated_MW = float.NaN;

        /// <summary>The estimated MVAR of the load/// </summary>
        public float Estimated_MVAR = float.NaN;

        /// <summary>The estimated MVA of the load</summary>
        public float Estimated_MVA = float.NaN;
                
        /// <summary>The telemetered MW of the load</summary>
        public float Telemetered_MW = float.NaN;

        /// <summary>The telemetered MVAR of the load</summary>
        public float Telemetered_MVAR = float.NaN;

        /// <summary>The derived telemetered MVA of the load</summary>
        public float Telemetered_MVA = float.NaN;
        #endregion


        #region Initialization

        /// <summary>
        /// Initialize a new CIM load
        /// </summary>
        public MM_Load()
        { this.ElemType = MM_Repository.FindElementType("Load"); }
       


          /// <summary>
        /// Initialize a new CIM Transformer
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Load(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource,AddIfNew)
        { this.ElemType = MM_Repository.FindElementType("Load"); }

        /// <summary>
        /// Initialize a new CIM Load
        /// </summary>
        /// <param name="ElementSource">The data source for this load</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Load(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource,AddIfNew)
        { this.ElemType = MM_Repository.FindElementType("Load"); }
      

        #endregion

        

        #region IComparable Members
        /// <summary>
        /// Compare two loads, first by their current MW
        /// </summary>
        /// <param name="Ld"></param>
        /// <returns></returns>
        public int CompareLoads(MM_Load Ld)
        {
            if (Ld.TEID == TEID)
                return 0;
            else
                return -this.Estimated_MW.CompareTo(Ld.Estimated_MW);
        }
        #endregion
    }
}
