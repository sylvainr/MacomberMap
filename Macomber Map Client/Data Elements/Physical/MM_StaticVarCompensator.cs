using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.Common;
using Macomber_Map.Data_Connections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class holds information on capacitors and reactors
    /// </summary>
    public class MM_StaticVarCompensator : MM_Element, IComparable
    {
        #region Variable declarations
        /// <summary>Estimated MVAR flow</summary>
        public float Estimated_MVAR = float.NaN;

        /// <summary>Telemetered MVAR flow</summary>
        public float Telemetered_MVAR = float.NaN;

        /// <summary>Nominal MVAR flow</summary>
        public float Nominal_MVAR = float.NaN;

        /// <summary>Whether the cap/reactor is open</summary>
        public bool Open;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new CIM SVS
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_StaticVarCompensator(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
        }


        /// <summary>
        /// Initialize a new CIM SVS
        /// </summary>
        /// <param name="ElementSource">The SQL source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_StaticVarCompensator(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
        }

        /// <summary>
        /// Initialize a new CIM capactior or reactor
        /// </summary>
        public MM_StaticVarCompensator()
        { }
        #endregion

        #region IComparable Members

        /// <summary>
        /// Compare two StaticVar compensators
        /// </summary>
        /// <param name="SC">The other StaticVar compensator to which it should be compared</param>
        /// <returns></returns>
        public int CompareStaticVarCompensators(MM_StaticVarCompensator SC)
        {
            if (SC == null)
                throw new InvalidOperationException("StaticVar compensators can only be compared to StaticVar compensators");
            else if (SC.TEID == this.TEID)
                return 0;
            else if (this.Open && SC.Open)
                return -Math.Abs(this.Nominal_MVAR).CompareTo(Math.Abs(SC.Nominal_MVAR));
            else if (!this.Open && !SC.Open)
                return -Math.Abs(this.Estimated_MVAR).CompareTo(Math.Abs(SC.Nominal_MVAR));
            else
                return -this.Open.CompareTo(SC.Open);
        }

        #endregion
    }
}
