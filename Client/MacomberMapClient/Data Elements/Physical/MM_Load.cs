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
    /// This class defines a generator/unit
    /// </summary>
    public class MM_Load : MM_Element, IComparable
    {

        #region Variable declarations
        /// <summary>Any upcoming outages</summary>
        public MM_ScheduledOutage UpcomingOutage = null;

        /// <summary>The estimated MW of the load/// </summary>
        public float Estimated_MW = float.NaN;

        public float Lmp = float.NaN;

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

        /// <summary>The boolean in manual override point</summary>
        public bool Manual;

        /// <summary>The manual target (WM)</summary>
        public float ManualTarget;

        /// <summary>The maximum load (MW)</summary>
        public float LoadMaximum;

        /// <summary>The wide area load</summary>
        public float WLoad;
        
        /// <summary>The MVAR load</summary>
        public float RM;

        /// <summary>Whether the load's remove is enabled</summary>
        public bool RemoveEnabled;

        /// <summary>Whether the load is removed</summary>
        public bool Removed;

        /// <summary>Whether our bus is dead</summary>
        public bool BusDead { get { return NearBus != null ? NearBus.Dead : true; } }

        /// <summary> The name of the market pricing node</summary>
        public string PnodeName;

        #endregion


        #region Initialization

        /// <summary>
        /// Initialize a new CIM load
        /// </summary>
        public MM_Load()
        { this.ElemType = MM_Repository.FindElementType("Load"); }

        public float MW
        {
            get
            {
                if (Data_Integration.UseEstimates)
                    return Estimated_MW;
                else if (!float.IsNaN(Telemetered_MW) && Telemetered_MW != 0)
                    return Telemetered_MW;
                else
                    return Estimated_MW;
            }

        }

        /// <summary>
        /// Initialize a new CIM Transformer
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Load(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        { this.ElemType = MM_Repository.FindElementType("Load"); }

        /// <summary>
        /// Initialize a new CIM Load
        /// </summary>
        /// <param name="ElementSource">The data source for this load</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Load(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
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
