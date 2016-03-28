using MacomberMapClient.Data_Elements.Violations;
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
    /// This class holds information on the busbar section
    /// </summary>
    public class MM_Bus : MM_Element
    {
        #region Variable declarations
        /// <summary>The current voltage (pU) of the electrical bus</summary>
        public float PerUnitVoltage = float.NaN;

        /// <summary>The current voltage (kV) of the electrical bus</summary>
        public float Estimated_kV = float.NaN;

        /// <summary>The angle of the electrical bus</summary>
        public float Estimated_Angle = float.NaN;

        /// <summary>The voltage limits of the bus (High normal, Low normal, High emergency, Low emergency)</summary>
        internal float[] Limits = null;

        /// <summary>The island associated with the bus</summary>
        public int IslandNumber=-1;

        /// <summary>
        /// The island associated with our bus
        /// </summary>
        public MM_Island Island
        {
            get {
                MM_Island FoundIsland;
                if (IslandNumber == -1)
                    return null;
                MM_Repository.Islands.TryGetValue(IslandNumber, out FoundIsland);
                return FoundIsland;
            }
            set { IslandNumber = value.ID; }
        }
        
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
                if (Integration.Data_Integration.ShowPostCtgValues && Violations != null)
                    foreach (MM_AlarmViolation Viol in Violations.Values)
                        if (Viol.Type.Name == "ContingencyHighVoltage" && (float.IsNaN(OutResult) || Viol.PostCtgValue > OutResult))
                            OutResult = Viol.PostCtgValue;
                        else if (Viol.Type.Name == "ContingencyLowVoltage" && (float.IsNaN(OutResult) || Viol.PostCtgValue < OutResult))
                            OutResult = Viol.PostCtgValue;
                return OutResult;
            }
        }

        /// <summary>Whether the node is dead</summary>
        public bool Dead = false;


        /// <summary>Whether the node is open</summary>
        public bool Open = false;

        /// <summary>
        /// The low normal limit on voltage
        /// </summary>
        public float LowNormalLimit
        {
            get {
                if (Limits != null && Limits.Length > 1)
                    return Limits[1];
                else
                    return float.NaN;
            }
            set {
                if (Limits == null)
                    Limits = new float[4];
                Limits[1] = value;
            }
        }

        /// <summary>
        /// The low emergency limit on voltage
        /// </summary>
        public float LowEmergencyLimit
        {
            get
            {
                if (Limits != null && Limits.Length > 0)
                    return Limits[0];
                else
                    return float.NaN;
            }
            set
            {
                if (Limits == null)
                    Limits = new float[4];
                Limits[0] = value;
            }
        }


        /// <summary>
        /// The high normal limit on voltage
        /// </summary>
        public float HighNormalLimit
        {
            get
            {
                if (Limits != null && Limits.Length > 2)
                    return Limits[2];
                else
                    return float.NaN;
            }
            set
            {
                if (Limits == null)
                    Limits = new float[4];
                Limits[2] = value;
            }
        }

        /// <summary>
        /// The high emergency limit on voltage
        /// </summary>
        public float HighEmergencyLimit
        {
            get
            {
                if (Limits != null && Limits.Length > 3)
                    return Limits[3];
                else
                    return float.NaN;
            }
            set
            {
                if (Limits == null)
                    Limits = new float[4];
                Limits[3] = value;
            }
        }

        /// <summary>The nbumber associated with our bus</summary>
        public int BusNumber=-1;

        /// <summary>
        /// Go through all of our elements, and find the ones that match our bus number
        /// </summary>
        public MM_Element[] ConnectedElements
        {
            get
            {

                List<MM_Element> Elems = new List<MM_Element>(5);
                if (BusNumber == -1)
                    return Elems.ToArray();
                foreach (MM_Element Elem in MM_Repository.TEIDs.Values.ToList())
                    if (Elem != null && (Elem.NearBusNumber == BusNumber || Elem.FarBusNumber == BusNumber))
                        Elems.Add(Elem);
                return Elems.ToArray();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new bus bar section
        /// </summary>
        /// <param name="BusNumber"></param>
        public MM_Bus(int BusNumber)
        {
            this.BusNumber = BusNumber;
            this.TEID = -BusNumber;
            this.Name = BusNumber.ToString();
            this.ElemType = MM_Repository.FindElementType("BusbarSection");
        }

        /// <summary>
        /// Initialize a new node from a data reader
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew"></param>
        public MM_Bus(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("BusbarSection");
        }

        /// <summary>
        /// Initialize a new node from an XML document
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew"></param>
        public MM_Bus(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("BusbarSection");
        }

        /// <summary>
        /// Initialize a parameterless node
        /// </summary>
        public MM_Bus()
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
        public int CompareBusbarSections(MM_Bus BB)
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
