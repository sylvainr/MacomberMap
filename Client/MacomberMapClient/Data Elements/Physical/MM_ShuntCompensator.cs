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
    /// This class holds information on capacitors and reactors
    /// </summary>
    public class MM_ShuntCompensator : MM_Element, IComparable
    {
        #region Variable declarations
        /// <summary>Estimated MVAR flow</summary>
        public float Estimated_MVAR = float.NaN;

        /// <summary>Estimated MVAR flow</summary>
        public float Telemetered_MVAR = float.NaN;

        /// <summary>Nominal MVAR flow</summary>
        public float Nominal_MVAR = float.NaN;

        /// <summary>Whether the cap/reactor is open</summary>
        public bool Open;

        /// <summary>The bus voltage associated with our shunt compensator</summary>
        public float BusVoltage { get { return NearBus != null ? NearBus.PerUnitVoltage : float.NaN; } }

        /// <summary>Whether the AVR is on or off</summary>
        public bool AVR;

        /// <summary>The regulation node</summary>
        public int Reg;

        /// <summary>Whether remove is enabled</summary>
        public bool RemoveEnabled;

        /// <summary>Whether the element is removed</summary>
        public bool Removed;

        /// <summary>Whether the element is in a bank</summary>
        public bool Bank;

        /// <summary>Whether the manual target is on</summary>
        public bool ManualTarget;

        /// <summary>The SC's manual target MVAR</summary>
        public float ManualTargetMVAR;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new CIM Transformer
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_ShuntCompensator(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
        }


        /// <summary>
        /// Initialize a new CIM capactior or reactor
        /// </summary>
        /// <param name="ElementSource">The SQL source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_ShuntCompensator(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
        }

        /// <summary>
        /// Initialize a new CIM capactior or reactor
        /// </summary>
        public MM_ShuntCompensator()
        { }
        #endregion

        #region IComparable Members

        /// <summary>
        /// Compare two shunt compensators
        /// </summary>
        /// <param name="SC">The other shunt compensator to which it should be compared</param>
        /// <returns></returns>
        public int CompareShuntCompensators(MM_ShuntCompensator SC)
        {
            if (SC == null)
                throw new InvalidOperationException("Shunt compensators can only be compared to shunt compensators");
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
