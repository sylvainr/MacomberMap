using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.Common;
using System.Drawing;
using Macomber_Map.User_Interface_Components.NetworkMap;
using Macomber_Map.Data_Connections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class holds information on an island
    /// </summary>
    public class MM_Island : MM_Element
    {
        #region Variable declarations
        /// <summary>The ID of our island</summary>
        public int ID;

        /// <summary>The total capacity of online units in the island</summary>
        public float OnlineCapacity = float.NaN;

        /// <summary>Generation in the island</summary>
        public float Generation = float.NaN;

        /// <summary>MW Losses on the island</summary>
        public float MWLosses = float.NaN;

        /// <summary>The load of the island</summary>
        public float Island_Load = float.NaN;

        /// <summary>The frequency of the island</summary>
        public float Frequency = float.NaN;

        /// <summary>Whether the island is on frequency control</summary>
        public bool FrequencyControl = false;


        /// <summary>
        /// Report the list of units associated with this island
        /// </summary>
        public MM_Element[] Units
        {
            get
            {
                List<MM_Element> OutElems = new List<MM_Element>();
                MM_BusbarSection FoundBus;
                foreach (MM_Unit Unit in MM_Repository.Units.Values)
                    if ((FoundBus = Unit.Bus) != null && FoundBus.IslandNumber == this.ID)
                        OutElems.Add(Unit);
                return OutElems.ToArray();
            }
        }

        /// <summary>
        /// Report the list of units associated with this island
        /// </summary>
        public MM_Element[] ShuntCompensators
        {
            get
            {
                List<MM_Element> OutElems = new List<MM_Element>();
                foreach (MM_ShuntCompensator SC in MM_Repository.ShuntCompensators.Values)
                    if (this == SC.Island)
                        OutElems.Add(SC);
                return OutElems.ToArray();
            }
        }


        /// <summary>
        /// Report the list of units associated with this island
        /// </summary>
        public MM_Element[] Loads
        {
            get
            {
                List<MM_Element> OutElems = new List<MM_Element>();
                MM_BusbarSection FoundBus;
                foreach (MM_Load LD in MM_Repository.Loads.Values)
                    if ((FoundBus = LD.Bus) != null && FoundBus.IslandNumber == this.ID)
                        OutElems.Add(LD);
                return OutElems.ToArray();
            }
        }

        /// <summary>
        /// The maximum capacity for all units
        /// </summary>
        public float AvailableCapacity
        {
            get
            {
                float TotalCap = 0;
                foreach (MM_Unit Unit in Units)
                    if (!float.IsNaN(Unit.MaxCapacity) && !float.IsNaN(Unit.Estimated_MW))
                        if (Unit.MaxCapacity > Unit.Estimated_MW)
                            TotalCap += Unit.MaxCapacity - Unit.Estimated_MW;
                return TotalCap;
            }
        }
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
        public void ComputeBounds(out PointF StartPoint, out PointF EndPoint, Network_Map nMap, bool ControlDown)
        {
            StartPoint = new PointF(float.NaN, float.NaN);
            EndPoint = new PointF(float.NaN, float.NaN);


            //Go through our data elements that are in our island            
            foreach (MM_Substation Sub in MM_Repository.Substations.Values)

                if (Sub.Permitted && (nMap.IsVisible(Sub) || ControlDown))
                {
                    bool Include = false;
                    if (Sub.BusbarSections != null)
                        foreach (MM_BusbarSection Bus in Sub.BusbarSections)
                            if (Bus.IslandNumber == ID)
                                Include = true;
                    if (!Include && Sub.Loads != null)
                        foreach (MM_Load Load in Sub.Loads)
                            if (this == Load.Island)
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