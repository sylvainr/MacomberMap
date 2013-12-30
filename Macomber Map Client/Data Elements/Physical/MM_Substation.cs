using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Data.Common;
using Macomber_Map.Data_Connections;
using System.Windows.Forms;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class contains information on substations
    /// </summary>
    public class MM_Substation : MM_Element
    {
        #region Variable declarations
        /// <summary>
        /// The long name of the substation
        /// </summary>
        public String LongName;

        /// <summary>Whether the substation is has blackstart components</summary>
        public bool IsBlackstart = false;

        /// <summary>Whether the substation has a synchroscope</summary>
        public bool HasSynchroscope = false;


        /// <summary>Whether the substation has a synchrocheck relay</summary>
        public bool HasSynchrocheck= false;
        
        /// <summary>
        /// The longitude and latitude of the substation
        /// </summary>
        public PointF LatLong = new PointF(float.NaN, float.NaN);

        /// <summary>
        /// The longitude of the substation
        /// </summary>
        public float Longitude
        {
            get { return LatLong.X; }
            set { LatLong.X = value; }
        }

        /// <summary>
        /// The latitude of the substation
        /// </summary>
        public float Latitude
        {
            get { return LatLong.Y; }
            set { LatLong.Y = value; }
        }

        /// <summary>
        /// Return the display name of the substation (Name or Long name, depending on the preference
        /// </summary>
        public String DisplayName()
        {
            if (MM_Repository.OverallDisplay.UseLongNames)
                return this.LongName;
            else
                return this.Name;
        }

        /// <summary>Our collection of operators</summary>
        private MM_Company[] _Operators = null;

        /// <summary>Our collection of operators found within our substation</summary>
        public MM_Company[] Operators
        {
            get
            {
                if (_Operators == null)
                {
                    List<MM_Company> OutOps = new List<MM_Company>();
                    Operator.OperatesEquipment = true;
                    OutOps.Add(Operator);
                    foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                        if (Elem.Operator != null && Elem is MM_Blackstart_Corridor_Element == false && this.Equals(Elem.Substation) && !OutOps.Contains(Elem.Operator))
                        {
                            OutOps.Add(Elem.Operator);
                            Elem.Operator.OperatesEquipment = true;
                        }
                    _Operators = OutOps.ToArray();
                }
                return _Operators;
            }
        }

        /// <summary>
        /// Report the total generation HSL in the substation
        /// </summary>
        public Single TotalHSL
        {
            get
            {
                float TotalGen = 0;
                if (Units != null)
                    foreach (MM_Unit Unit in Units)
                        if (!float.IsNaN(Unit.HSL))
                            TotalGen += Unit.HSL;
                return TotalGen;
            }
        }

        /// <summary>
        /// Report the total number of lines connected to our substation
        /// </summary>
        public Single TotalLines
        {
            get
            {
                int LineCount=0;
                foreach (MM_Line Line in MM_Repository.Lines.Values)
                    if (Line.Substation1 == this ^ Line.Substation2 == this)
                        LineCount++;
                return LineCount;
            }
        }

        /// <summary>
        /// Report the total transmission capacity into/out of the substation
        /// </summary>
        public Single TotalLineCapacity
        {
            get
            {
                Single LineSum=0;
                foreach (MM_Line Line in MM_Repository.Lines.Values)
                    if (Line.Substation1 == this ^ Line.Substation2 == this)
                        if (!float.IsNaN(Line.NormalLimit))
                            LineSum += Line.NormalLimit;
                return LineSum;
            }
        }


        /// <summary>
        /// The 
        /// </summary>
        public enum enumFrequencyControlStatus
        {
            /// <summary>No frequency control</summary>
            None = 0,
            /// <summary>Unit on frequency control</summary>
            Unit = 1,
            /// <summary>Island on frequency control</summary>
            Island = 2
        }

        /// <summary>
        /// Determine the frequency control state of the substation
        /// </summary>
        public enumFrequencyControlStatus FrequencyControlStatus
        {
            get
            {
                bool UnitFC = false, IslandFC = false;
                if (Units != null)
                    foreach (MM_Unit Unit in Units)
                        if (Unit.FrequencyControl)
                        {
                            UnitFC = true;
                            IslandFC |= Unit.IslandFreqCtrl;
                        }
                if (IslandFC)
                    return enumFrequencyControlStatus.Island;
                else if (UnitFC)
                    return enumFrequencyControlStatus.Unit;
                else
                    return enumFrequencyControlStatus.None;
            }
        }


        /// <summary>
        /// The collection of KV levels found within the substation
        /// </summary>
        public List<MM_KVLevel> KVLevels = new List<MM_KVLevel>(4);


        /// <summary>
        /// The county in which the substation resides
        /// </summary>
        public MM_Boundary County;

        /// <summary>
        /// The collection of element types within the substation
        /// </summary>
        public List<MM_Element_Type> ElemTypes = new List<MM_Element_Type>(5);

        /// <summary>
        /// Return a string of element types within the substation
        /// </summary>
        public String ElementTypeList
        {
            get
            {
                StringBuilder outStr = new StringBuilder();
                List<MM_Element_Type> ElemTypes = new List<MM_Element_Type>(10);
                foreach (MM_Element[] Elems in new MM_Element[][] { this.Units, this.Loads, this.Transformers, this.BusbarSections, this.ShuntCompensators })
                    if (Elems != null)
                        foreach (MM_Element Elem in Elems)
                            if (Elem.Permitted && !ElemTypes.Contains(Elem.ElemType))
                                ElemTypes.Add(Elem.ElemType);

                foreach (MM_Element_Type ElemType in ElemTypes)
                    outStr.Append((outStr.Length == 0 ? "" : ", ") + ElemType.Name);
                return outStr.ToString();
            }
        }

        /// <summary>
        /// Return a string of KV levels within the substation
        /// </summary>
        public String KVLevelList
        {
            get
            {
                StringBuilder outStr = new StringBuilder();
                for (int a = 0; a < KVLevels.Count; a++)
                    if (KVLevels[a].Permitted)
                        outStr.Append((outStr.Length == 0 ? "" : ", ") + KVLevels[a].Name.Split(' ')[0]);
                return outStr.ToString();
            }
        }

        /// <summary>
        /// Return a string of KV levels (including 'KV') within the substation
        /// </summary>
        public String KVLevelFullList
        {
            get
            {
                StringBuilder outStr = new StringBuilder();
                for (int a = 0; a < KVLevels.Count; a++)
                    outStr.Append((a == 0 ? "" : ", ") + KVLevels[a].Name);
                return outStr.ToString();
            }
        }
        /// <summary>Units within this substation</summary>
        public MM_Unit[] Units;

        /// <summary>Loads within this substation</summary>
        public MM_Load[] Loads;

        /// <summary>Transofrmers within this substation</summary>
        public MM_Transformer[] Transformers;

        /// <summary>Our collection of static var compensators within the substation</summary>
        public MM_StaticVarCompensator[] StaticVarCompensators;

        /// <summary>Electrical buses within this substation</summary>
        public MM_BusbarSection[] BusbarSections;

        /// <summary>Capacitors and reactors within this substation</summary>
        public MM_ShuntCompensator[] ShuntCompensators;

        /// <summary>The current weather zone</summary>
        public MM_Zone WeatherZone;

        /// <summary>The current load zone</summary>
        public MM_Zone LoadZone;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new CIM Substation
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew"></param>
        public MM_Substation(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            ElemType = MM_Repository.FindElementType("Substation");
        }

        /// <summary>
        /// Initialize a new CIM substation
        /// </summary>
        public MM_Substation()
        {
            ElemType = MM_Repository.FindElementType("Substation");
        }

        /// <summary>
        /// Initialize a new CIM Substation
        /// </summary>
        /// <param name="ElementSource">The data source for this substation</param>
        /// <param name="AddIfNew"></param>
        public MM_Substation(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            ElemType = MM_Repository.FindElementType("Substation");
        }

        #endregion

        #region Xml Element saving
        /// <summary>
        /// Convert the boundary into an XML element
        /// </summary>
        /// <param name="xDoc">The XML Document used to create the element</param>
        /// <returns>The newly-created element</returns>
        public XmlElement SubstationToXml(XmlDocument xDoc)
        {
            XmlElement outElem = xDoc.CreateElement("Substation");
            outElem.Attributes.Append(xDoc.CreateAttribute("Name")).Value = this.Name;
            outElem.Attributes.Append(xDoc.CreateAttribute("LongName")).Value = this.LongName;
            outElem.Attributes.Append(xDoc.CreateAttribute("Longitude")).Value = this.LatLong.X.ToString();
            outElem.Attributes.Append(xDoc.CreateAttribute("Latitude")).Value = this.LatLong.Y.ToString();
            if (this.County != null)
                outElem.Attributes.Append(xDoc.CreateAttribute("County")).Value = this.County.Name;
            outElem.Attributes.Append(xDoc.CreateAttribute("TEID")).Value = base.TEID.ToString();
            outElem.Attributes.Append(xDoc.CreateAttribute("Operator")).Value = base.Operator.Name;
            outElem.Attributes.Append(xDoc.CreateAttribute("Owner")).Value = base.Owner.Name;
            return outElem;
        }
        #endregion

        /// <summary>
        /// Return the distance between this substation and another, in miles
        /// </summary>
        /// <param name="Substation">The other substation</param>
        /// <returns>Distance, in miles.</returns>
        public float DistanceTo(MM_Substation Substation)
        {
            return DistanceTo(Substation.LatLong);            
        }

        /// <summary>
        /// Return the distance between this substation and another, in miles
        /// </summary>
        /// <param name="OtherLatLong">The other lat/long</param>
        /// <returns>Distance, in miles.</returns>
        public float DistanceTo(PointF OtherLatLong)
        {
            return ComputeDistance(LatLong, OtherLatLong);
        }

        /// <summary>
        /// Compute the distances between two lat/longs
        /// </summary>
        /// <param name="LatLong"></param>
        /// <param name="OtherLatLong"></param>
        /// <returns></returns>
        public static float ComputeDistance(PointF LatLong, PointF OtherLatLong)
        {
            float R = 3963.1f; //Radius of the earth in miles
            float deltaLat = (LatLong.Y - OtherLatLong.Y) * (float)Math.PI / 180f;
            float deltaLon = (LatLong.X - OtherLatLong.X) * (float)Math.PI / 180f;
            float a = (float)Math.Sin(deltaLat / 2f) * (float)Math.Sin(deltaLat / 2f) + (float)Math.Cos(OtherLatLong.Y * (float)Math.PI / 180f) * (float)Math.Cos(LatLong.Y * Math.PI / 180d) * (float)Math.Sin(deltaLon / 2f) * (float)Math.Sin(deltaLon / 2f);
            float c = 2f * (float)Math.Atan2(Math.Sqrt(a), Math.Sqrt(1f - a));
            return R * c;
        }


        /// <summary>
        /// Return the angle between this substation and another, in radians
        /// </summary>
        /// <param name="Substation">The other substation</param>
        /// <returns>Angle, in radians</returns>
        public float AngleTo(MM_Substation Substation)
        {
            float DeltaX = Substation.LatLong.X - LatLong.X;
            float DeltaY = Substation.LatLong.Y - LatLong.Y;
            return (float)Math.Atan2(DeltaY, DeltaX);
        }

        /// <summary>
        /// Return the average voltage deviation 
        /// </summary>
        public float Average_pU
        {
            get
            {
                float BusNum = 1f, BusDenom = 1f, BusPU;                
                    if (BusbarSections != null)
                        foreach (MM_BusbarSection Bus in BusbarSections)
                            if (Bus.KVLevel != null)
                                if (Bus.Permitted && Bus.KVLevel.ShowOnMap && !float.IsNaN(BusPU = Bus.PerUnit))
                                {
                                    BusNum += (BusPU * Math.Abs(1f - BusPU) * Bus.Estimated_kV);
                                    BusDenom += Math.Abs(1f - BusPU) * Bus.Estimated_kV;
                                }
                return BusNum / BusDenom;
            }
        }

        /// <summary>
        /// Determine whether the substation is visible on the screen
        /// </summary>
        /// <param name="Coordinates">The coordinates of the viewport</param>
        /// <returns></returns>
        public bool CheckVisibility(MM_Coordinates Coordinates)
        {
            if (!this.Permitted)
                return false;

            if (float.IsNaN(LatLong.X))
                return false;
            else if (LatLong.X < Coordinates.TopLeft.X || LatLong.X > Coordinates.BottomRight.X || LatLong.Y > Coordinates.TopLeft.Y || LatLong.Y < Coordinates.BottomRight.Y)
                return false;
            else
                foreach (MM_KVLevel KVLevel in KVLevels)
                    if (Coordinates.ZoomLevel >= KVLevel.VisibilityByZoom)
                    return true;
            return false;
        }


        /// <summary>
        /// Determine the appropriate display parameter for the substation
        /// </summary>
        /// <param name="CallingObject">The calling object</param>
        /// <param name="ShownViolations">The shown violations</param>
        public MM_DisplayParameter SubstationDisplay(Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations, Object CallingObject)
        {
            //TODO: Update this to include energization state
            MM_AlarmViolation_Type WorstViol = this.WorstVisibleViolation(ShownViolations, CallingObject);
            if (WorstViol == null)
                return MM_Repository.SubstationDisplay;
            else
                return WorstViol;

        }


        /// <summary>
        /// Return the appropriate text for the substation, as requested
        /// </summary>
        /// <param name="ZoomLevel">The current zoom level</param>
        /// <returns></returns>
        public string MapText(float ZoomLevel)
        {
            StringBuilder OutStr = new StringBuilder(DisplayName());
            if ((ZoomLevel < MM_Repository.OverallDisplay.StationMW) || ((Units == null) && (Loads == null)))
                return OutStr.ToString();
            if (MM_Repository.SubstationDisplay.ShowTotalLoad && (Loads != null))
            {
                float TotalLoad = 0f;
                foreach (MM_Load Load in Loads)
                    TotalLoad += Load.Estimated_MW;
                OutStr.Append("\nLoad: " + TotalLoad.ToString("#,##0"));
            }
            if (Units != null)
            {
                float TotalGen = 0f, TotalHSL = 0f;
                foreach (MM_Unit Unit in Units)
                {
                    if (!float.IsNaN(Unit.Estimated_MW) && Unit.IsPhysical)
                        TotalGen += Unit.Estimated_MW;
                    if (!float.IsNaN(Unit.HSL))
                        TotalHSL += Unit.HSL;
                }

                if (MM_Repository.SubstationDisplay.ShowTotalGeneration)
                    OutStr.Append("\nGen: " + TotalGen.ToString("#,##0"));
                if (MM_Repository.SubstationDisplay.ShowTotalHSL)
                    OutStr.Append("\nHSL: " + TotalHSL.ToString("#,##0"));
                if (MM_Repository.SubstationDisplay.ShowRemainingCapacity)
                    OutStr.Append("\nRem: " + (TotalHSL - TotalGen).ToString("#,##0"));
            }

            return OutStr.ToString();
        }
    }
}