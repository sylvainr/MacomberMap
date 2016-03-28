using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// This class holds information on an island
    /// </summary>
    public class MM_Island : MM_Element
    {

        #region Variable declarations
        /// <summary>The ID of our island</summary>
        public int ID;

        /// <summary>Generation in the island</summary>
        public float Generation = float.NaN;

        /// <summary>
        /// Report the list of units associated with this island
        /// </summary>
        public MM_Unit[] Units
        {
            get
            {
                List<MM_Unit> OutElems = new List<MM_Unit>();
                MM_Bus FoundBus;
                foreach (MM_Unit Unit in MM_Repository.Units.Values)
                    if ((FoundBus = Unit.NearBus) != null && FoundBus.IslandNumber == this.ID)
                        OutElems.Add(Unit);
                return OutElems.ToArray();
            }
        }


        /// <summary>The load of the island</summary>
        public float Load = float.NaN;

        /// <summary>
        /// Report the list of units associated with this island
        /// </summary>
        public MM_Load[] Loads
        {
            get
            {
                List<MM_Load> OutElems = new List<MM_Load>();
                MM_Bus FoundBus;
                foreach (MM_Load LD in MM_Repository.Loads.Values)
                    if ((FoundBus = LD.NearBus) != null && FoundBus.IslandNumber == this.ID)
                        OutElems.Add(LD);
                return OutElems.ToArray();
            }
        }


        /// <summary>MW Losses on the island</summary>
        public float MWLosses = float.NaN;

        /// <summary>The frequency of the island</summary>
        public float Frequency = float.NaN;




        /// <summary>
        /// Report the list of units associated with this island
        /// </summary>
        public MM_ShuntCompensator[] ShuntCompensators
        {
            get
            {
                List<MM_ShuntCompensator> OutElems = new List<MM_ShuntCompensator>();
                foreach (MM_ShuntCompensator SC in MM_Repository.ShuntCompensators.Values)
                    if (this == SC.NearIsland)
                        OutElems.Add(SC);
                return OutElems.ToArray();
            }
        }




        /// <summary>
        /// The maximum capacity for all units
        /// </summary>
        public float AvailableGenCapacity
        {
            get
            {
                float TotalCap = 0;
                foreach (MM_Unit Unit in Units)
                    if (!float.IsNaN(Unit.MaxCapacity) && !float.IsNaN(Unit.Estimated_MW) && Unit.Estimated_MW > MM_Repository.OverallDisplay.EnergizationThreshold)
                        if (Unit.MaxCapacity > Unit.Estimated_MW)
                            TotalCap += Unit.MaxCapacity - Unit.Estimated_MW;
                return TotalCap;
            }
        }

        /// <summary>
        /// The maximum capacity for all units
        /// </summary>
        public float TotalGenCapacity
        {
            get
            {
                float TotalCap = 0;
                foreach (MM_Unit Unit in Units)
                    if (!float.IsNaN(Unit.MaxCapacity))
                        if (Unit.MaxCapacity > Unit.Estimated_MW)
                            TotalCap += Unit.MaxCapacity - Unit.Estimated_MW;
                return TotalCap;
            }
        }

        /// <summary>Whether the island is on frequency control</summary>
        public bool FrequencyControl = false;

        /// <summary>
        /// The unit that is in isochronous mode
        /// </summary>
        public MM_Unit IsochronousUnit
        {
            get
            {
                foreach (MM_Unit Unit in Units)
                    if (Unit.FrequencyControl && (FrequencyControl || ManualTarget))
                        return Unit;
                return null;
            }
        }

        /// <summary>Whether the island's frequency is a manual target</summary>
        public bool ManualTarget = false;


        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new island
        /// </summary>
        public MM_Island()
        {
            this.Name = "Island";
            this.ElemType = MM_Repository.FindElementType("Island");
        }

        /// <summary>
        /// Initialize a new island
        /// </summary>
        /// <param name="ID"></param>
        public MM_Island(int ID)
        {
            this.ID = ID;
            this.Name = "Island " + this.ID.ToString();
            this.ElemType = MM_Repository.FindElementType("Island");
            this.TEID = Data_Integration.GetTEID();
            MM_Repository.TEIDs.Add(this.TEID, this);
        }

        /// <summary>
        /// Create a new island information point from XML
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Island(XmlElement xElem, bool AddIfNew)
            : base(xElem, AddIfNew)
        {
            this.Name = "Island " + this.ID.ToString();
            this.ElemType = MM_Repository.FindElementType("Island");
        }

        /// <summary>
        /// Create a new island point from a database
        /// </summary>
        /// <param name="dRd"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Island(DbDataReader dRd, bool AddIfNew)
            : base(dRd, AddIfNew)
        {
            this.Name = "Island " + this.ID.ToString();
            this.ElemType = MM_Repository.FindElementType("Island");
        }
        #endregion

        /// <summary>
        /// Compute the bounds for our island
        /// </summary>
        /// <param name="StartPoint"></param>
        /// <param name="EndPoint"></param>
        /// <param name="nMap"></param>
        /// <param name="ControlDown"></param>
        public void ComputeBounds(out PointF StartPoint, out PointF EndPoint, MM_Network_Map_DX nMap, bool ControlDown)
        {
            StartPoint = new PointF(float.NaN, float.NaN);
            EndPoint = new PointF(float.NaN, float.NaN);


            //Go through our data elements that are in our island            
            foreach (MM_Substation Sub in MM_Repository.Substations.Values)

                if (Sub.Permitted && (nMap.IsVisible(Sub) || ControlDown))
                {
                    bool Include = false;
                    if (Sub.BusbarSections != null)
                        foreach (MM_Bus Bus in Sub.BusbarSections)
                            if (Bus.IslandNumber == ID)
                                Include = true;
                    if (!Include && Sub.Loads != null)
                        foreach (MM_Load Load in Sub.Loads)
                            if (this == Load.NearIsland)
                                Include = true;



                    if (Include)
                    {
                        if (float.IsNaN(StartPoint.X) || Sub.Longitude < StartPoint.X)
                            StartPoint.X = Sub.Longitude;
                        if (float.IsNaN(StartPoint.Y) || Sub.Latitude < StartPoint.Y)
                            StartPoint.Y = Sub.Latitude;
                        if (float.IsNaN(EndPoint.X) || Sub.Longitude > EndPoint.X)
                            EndPoint.X = Sub.Longitude;
                        if (float.IsNaN(EndPoint.Y) || Sub.Latitude > EndPoint.Y)
                            EndPoint.Y = Sub.Latitude;
                    }
                }
        }
    }
}