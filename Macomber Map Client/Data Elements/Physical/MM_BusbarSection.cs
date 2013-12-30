using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.Common;
using Macomber_Map.Data_Connections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class holds information on the busbar section
    /// </summary>
    public class MM_BusbarSection : MM_Element
    {
        #region Variable declarations 
        /// <summary>The current voltage (pU) of the electrical bus</summary>
        public float PerUnitVoltage = float.NaN;

        /// <summary>The current voltage (kV) of the electrical bus</summary>
        public float Estimated_kV = float.NaN;

        /// <summary>The angle of the electrical bus</summary>
        public float Estimated_Angle = float.NaN;

        /// <summary>The voltage limits of the bus (High normal, Low normal, High emergency, Low emergency)</summary>
        public float[] Limits = new float[] { float.NaN, float.NaN, float.NaN, float.NaN };

        /// <summary>The island associated with the bus</summary>
        public int IslandNumber;

        /// <summary>The next scheduled outage, if any</summary>
        public MM_ScheduledOutage UpcomingOutage = null;

        /// <summary>The node associated with the busbar section</summary>
        private MM_Node _Node;

        /// <summary>The node associated with the busbar section</summary>
        public MM_Node Node
        {
            get { return _Node; }
            set { _Node = value; }
        }

        /// <summary>
        /// Report the per-unit voltage of the busbar section, whether real-time or post-ctg.
        /// </summary>
        public float PerUnit
        {
            get
            {
                float OutResult = PerUnitVoltage;
                if (Data_Integration.ShowPostCtgValues && Violations != null)
                    foreach (MM_AlarmViolation Viol in Violations.Values)
                        if (Viol.Type.Name == "ContingencyHighVoltage" && (float.IsNaN(OutResult) || Viol.PostCtgValue > OutResult))
                            OutResult = Viol.PostCtgValue;
                        else if (Viol.Type.Name == "ContingencyLowVoltage" &&(float.IsNaN(OutResult) || Viol.PostCtgValue < OutResult))
                            OutResult = Viol.PostCtgValue;
                return OutResult;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new node from a data reader
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew"></param>
        public MM_BusbarSection(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("BusbarSection");         
        }

        /// <summary>
        /// Initialize a new node from an XML document
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew"></param>
        public MM_BusbarSection(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("BusbarSection");           
        }

        /// <summary>
        /// Initialize a parameterless node
        /// </summary>
        public MM_BusbarSection()
            : base()
        {
            this.ElemType = MM_Repository.FindElementType("BusbarSection");          
        }
       #endregion

        #region IComparable Members
        /// <summary>
        /// Compare two busbar sections
        /// </summary>
        /// <param name="BB">The other busbar section</param>
        /// <returns></returns>
        public int CompareBusbarSections(MM_BusbarSection BB)
        {
            if (BB.TEID == TEID)
                return 0;
            else if (PerUnitVoltage == BB.PerUnitVoltage)
                return TEID.CompareTo(BB.TEID);
            else
                return -PerUnitVoltage.CompareTo(BB.PerUnitVoltage);
        }

        /// <summary>
        /// Return the text string indicating this item's voltage
        /// </summary>
        public string VoltageText
        {
            get
            {
                return PerUnitVoltage.ToString("0.0%") + " (" + Estimated_kV.ToString() + " kV)";
            }
        }     
        #endregion
    }
}
