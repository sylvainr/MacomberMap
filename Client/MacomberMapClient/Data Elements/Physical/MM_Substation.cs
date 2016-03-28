using MacomberMapClient.Data_Elements.Blackstart;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using RectangleF = SharpDX.RectangleF;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class contains information on a substation
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
        public bool HasSynchrocheck = false;

        /// <summary>
        /// Is this substation internal or external?
        /// </summary>
        public bool IsInternal = true;

        /// <summary>
        /// Is this substation participating in the market?
        /// </summary>
        public bool IsMarket = false;

        /// <summary>
        /// The operating area.
        /// </summary>
        public string Area = "";

        /// <summary>
        /// The longitude and latitude of the substation
        /// </summary>
        public PointF LngLat = new PointF(float.NaN, float.NaN);

        /// <summary>
        /// The longitude of the substation
        /// </summary>
        public float Longitude
        {
            get { return LngLat.X; }
            set { LngLat.X = value; }
        }

        /// <summary>
        /// The latitude of the substation
        /// </summary>
        public float Latitude
        {
            get { return LngLat.Y; }
            set { LngLat.Y = value; }
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

        public IEnumerable<MM_Element> GetAllSubstationEquipment()
        {
            foreach (var element in Units)
            {
                yield return element;
            }

            foreach (var element in Loads)
            {
                yield return element;
            }

            foreach (var element in Transformers)
            {
                yield return element;
            }

            foreach (var element in BusbarSections)
            {
                yield return element;
            }

            foreach (var element in ShuntCompensators)
            {
                yield return element;
            }

            foreach (var element in MM_Repository.Lines)
            {
                if (element.Value.Substation != null && Equals(element.Value.Substation))
                    yield return element.Value;
            }
        }
        /// <summary>Our collection of operators found within our substation</summary>
        public MM_Company[] Operators
        {
            get
            {
                if (_Operators == null)
                {
                    // List<MM_Company> OutOps = new List<MM_Company>();
                    // Operator.OperatesEquipment = true;
                    // OutOps.Add(Operator);
                    //
                    // foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                    //     if (Elem.Operator != null && Elem is MM_Blackstart_Corridor_Element == false && this.Equals(Elem.Substation) && !OutOps.Contains(Elem.Operator))
                    //     {
                    //         OutOps.Add(Elem.Operator);
                    //         Elem.Operator.OperatesEquipment = true;
                    //     }
                    //
                    // 

                    HashSet<MM_Company> company = new HashSet<MM_Company>();
                    foreach (var element in GetAllSubstationEquipment())
                    {
                        if (element.Operator != null && !company.Contains(element.Operator))
                            company.Add(element.Operator);
                    }

                    _Operators = company.ToArray();
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
		
		//TODO: This is a change to SPP code.
		 /// <summary>
        /// Report the total economic maximum in the substation
        /// </summary>
        public Single TotalEcoMax
        {
            get
            {
                float TotalGen = 0;
                if (Units != null)
                    foreach (MM_Unit Unit in Units)
                        if (!float.IsNaN(Unit.EcoMax))
                            TotalGen += Unit.EcoMax;
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
                int LineCount = 0;
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
                Single LineSum = 0;
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
                            if (Unit.NearIsland != null)
                                IslandFC |= Unit.NearIsland.FrequencyControl;
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

        public MM_KVLevel GetMaxKVLevel
        {
            get
            {
                if (KVLevels == null || KVLevels.Count <= 0)
                    return null;

                return KVLevels.OrderByDescending(kv => kv.Nominal).FirstOrDefault();
            }
        }
        /// <summary>
        /// The county in which the substation resides
        /// </summary>
        public MM_Boundary County;

        /// <summary>
        /// The district in which the substation resides
        /// </summary>
        public MM_Boundary District;

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
                foreach (MM_Element[] Elems in new MM_Element[][] { this.Units.ToArray(), this.Loads.ToArray(), this.Transformers.ToArray(), this.BusbarSections.ToArray(), this.ShuntCompensators.ToArray() })
                    if (Elems != null)
                        foreach (MM_Element Elem in Elems)
                            if (Elem.Permitted && !ElemTypes.Contains(Elem.ElemType))
                                ElemTypes.Add(Elem.ElemType);

                foreach (MM_Element_Type ElemType in ElemTypes)
                    if (ElemType != null)
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


        public bool HasUnits {get{return Units != null && Units.Count > 0;}}
        public bool HasTransformers {get{return Transformers != null && Transformers.Count > 0;}}
        public bool HasLoads {get{return Loads != null && Loads.Count > 0;}}
        public bool HasStaticVarCompensators {get{return StaticVarCompensators != null && StaticVarCompensators.Count > 0;}}
        public bool HasShuntCompensators { get { return ShuntCompensators != null && ShuntCompensators.Count > 0; } }

        public bool HasOutagedAutoTransformer
        {
            get
            {
                if (!HasTransformers) return false;
                foreach (var XF in Transformers)
                {
                    if (XF.IsAutoTransformer && XF.Winding1.KVLevel.Name != "Other KV" && XF.Winding2.KVLevel.Name != "Other KV")
                        if (float.IsNaN(XF.MVAFlow) || XF.MVAFlow <= MM_Repository.OverallDisplay.EnergizationThreshold)
                            return true;
                }
                return false;
            }
        }

        /// <summary>Units within this substation</summary>
        public List<MM_Unit> Units = new List<MM_Unit>(2);

        /// <summary>Loads within this substation</summary>
        public List<MM_Load> Loads = new List<MM_Load>(5);

        /// <summary>Transofrmers within this substation</summary>
        public List<MM_Transformer> Transformers = new List<MM_Transformer>(5);

        /// <summary>Our collection of static var compensators within the substation</summary>
        public List<MM_StaticVarCompensator> StaticVarCompensators = new List<MM_StaticVarCompensator>(2);

        /// <summary>Electrical buses within this substation</summary>
        public List<MM_Bus> BusbarSections = new List<MM_Bus>(5);

        /// <summary>Capacitors and reactors within this substation</summary>
        public List<MM_ShuntCompensator> ShuntCompensators = new List<MM_ShuntCompensator>(5);

        /// <summary>Breakers and switches within this substation</summary>
        public List<MM_Breaker_Switch> BreakerSwitches = new List<MM_Breaker_Switch>(5);

        /// <summary>The current weather zone</summary>
        public MM_Zone WeatherZone;

        /// <summary>The current load zone</summary>
        public MM_Zone LoadZone;

        /// <summary>
        /// Report whether transmission capacity is greater than 1500
        /// </summary>
        public bool TransmissionCapacityOver1500
        {
            get { return TotalLineCapacity >= 1500f; }
        }

        /// <summary>
        /// Report whether the generation capacity is greater than 1000.
        /// </summary>
        public bool GenerationCapacityOver1000
        {
            get { return TotalGenerationCapacity >= 1000f; }
        }

        /// <summary>
        /// Report the total generation capacity
        /// </summary>
        public float TotalGenerationCapacity
        {
            get
            {
                float Total = 0f;
                if (this.Units != null)
                    foreach (MM_Unit Unit in this.Units)
                        if (!float.IsNaN(Unit.MaxCapacity))
                            Total += Unit.MaxCapacity;
                return Total;
            }
        }

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
            outElem.Attributes.Append(xDoc.CreateAttribute("Longitude")).Value = this.LngLat.X.ToString();
            outElem.Attributes.Append(xDoc.CreateAttribute("Latitude")).Value = this.LngLat.Y.ToString();
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
            return DistanceTo(Substation.LngLat);
        }

        /// <summary>
        /// Return the distance between this substation and another, in miles
        /// </summary>
        /// <param name="OtherLngLat">The other lat/long</param>
        /// <returns>Distance, in miles.</returns>
        public float DistanceTo(PointF OtherLngLat)
        {
            return ComputeDistance(LngLat, OtherLngLat);
        }

        /// <summary>
        /// Compute the distances between two lat/longs
        /// </summary>
        /// <param name="LngLat"></param>
        /// <param name="OtherLngLat"></param>
        /// <returns></returns>
        public static float ComputeDistance(PointF LngLat, PointF OtherLngLat)
        {
            float R = 3963.1f; //Radius of the earth in miles
            float deltaLat = (LngLat.Y - OtherLngLat.Y) * (float)Math.PI / 180f;
            float deltaLon = (LngLat.X - OtherLngLat.X) * (float)Math.PI / 180f;
            float a = (float)Math.Sin(deltaLat / 2f) * (float)Math.Sin(deltaLat / 2f) + (float)Math.Cos(OtherLngLat.Y * (float)Math.PI / 180f) * (float)Math.Cos(LngLat.Y * Math.PI / 180d) * (float)Math.Sin(deltaLon / 2f) * (float)Math.Sin(deltaLon / 2f);
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
            float DeltaX = Substation.LngLat.X - LngLat.X;
            float DeltaY = Substation.LngLat.Y - LngLat.Y;
            return (float)Math.Atan2(DeltaY, DeltaX);
        }


        /// <summary>
        /// Report the list of active (non-dead) buses in our substation
        /// </summary>
        public MM_Bus[] Buses
        {
            get
            {
                if (this.BreakerSwitches == null)
                    return new MM_Bus[0];
                Dictionary<int, MM_Bus> FoundBuses = new Dictionary<int, MM_Bus>();
                foreach (MM_Breaker_Switch BreakerSwitch in this.BreakerSwitches)
                {
                    MM_Bus NearBus = BreakerSwitch.NearBus;
                    MM_Bus FarBus = BreakerSwitch.FarBus;
                    if (NearBus != null && !FoundBuses.ContainsKey(NearBus.BusNumber))
                        FoundBuses.Add(NearBus.BusNumber, NearBus);
                    if (FarBus != null && !FoundBuses.ContainsKey(FarBus.BusNumber))
                        FoundBuses.Add(FarBus.BusNumber, FarBus);
                }
                return FoundBuses.Values.ToArray();
            }
        }


        /// <summary>
        /// Return the average voltage deviation 
        /// </summary>
        public float Average_pU
        {
            get
            {
                float BusNum = 1f, BusDenom = 1f, BusPU;
                int CountBuses = 0;

                foreach (MM_Bus Bus in this.Buses)
                    if (Bus.Permitted && (Bus.KVLevel == null || Bus.KVLevel.ShowOnMap) && !float.IsNaN(BusPU = Bus.PerUnit) && !Bus.Dead && !float.IsNaN(Bus.Estimated_kV) && Bus.Estimated_kV != 0)
                    {
                        BusNum += (BusPU * Math.Abs(1f - BusPU) * Bus.Estimated_kV);
                        BusDenom += Math.Abs(1f - BusPU) * Bus.Estimated_kV;
                        CountBuses++;
                    }


                if (CountBuses == 0)
                    return float.NaN;
                else
                    return BusNum / BusDenom;
            }
        }

        /// <summary>
        /// Get the average LMP of a substation. Returns NaN if fails.
        /// </summary>
        /// <returns></returns>
        public float GetAverageLMP()
        {

            float loadLMP = 0;
            int loadCount = 0;
            float unitLMP = 0;
            int unitCount = 0;
            if (Loads != null && Loads.Count > 0)
            {
                foreach (var load in Loads)
                {
                    if (!float.IsNaN(load.Lmp) && load.Lmp != 0f)
                    {
                        loadLMP += load.Lmp;
                        loadCount++;
                    }
                }
                if (loadCount > 0)
                    loadLMP /= loadCount;
            }

            if (Units != null && Units.Count > 0)
            {
                foreach (var item in Units)
                {
                    if (!float.IsNaN(item.LMP) && item.LMP != 0f)
                    {
                        unitLMP += item.LMP;
                        unitCount++;
                    }
                                       
                }
                if (unitCount > 0)
                    unitLMP /= unitCount;
            }

            if (unitCount == 0 && loadCount == 0)
                return 0;

            // average if we have values
            if (unitCount != 0 && loadCount != 0)
                return (unitLMP + loadLMP) / 2;
            if (unitCount != 0)
                return unitLMP;
            if (loadCount != 0)
                return loadLMP;

            return float.NaN;


        }

        /// <summary>
        /// Determine whether the substation is visible on the screen
        /// </summary>
        /// <param name="Coordinates">The coordinates of the viewport</param>
        /// <returns></returns>
        public bool CheckVisibility(System.Drawing.RectangleF Coordinates, int zoomLevel)
        {
            if (!this.Permitted)
                return false;

            if (float.IsNaN(LngLat.X))
                return false;
            else if (LngLat.X < Coordinates.Left || LngLat.X > Coordinates.Right || LngLat.Y > Coordinates.Top || LngLat.Y < Coordinates.Bottom)
                return false;
            else
                foreach (MM_KVLevel KVLevel in KVLevels)
                    if (zoomLevel >= KVLevel.VisibilityByZoom)
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
            if ((ZoomLevel < MM_Repository.OverallDisplay.StationMW) || ((Units == null) && (Loads == null)))
                return DisplayName();

            StringBuilder OutStr = new StringBuilder();
            if (MM_Repository.SubstationDisplay.ShowTotalLoad && (Loads != null && Loads.Count > 0))
            {
                float TotalLoad = 0f;
                foreach (MM_Load Load in Loads)
                    TotalLoad += Load.Estimated_MW;

                OutStr.Append('\n');
                if (Math.Abs(TotalLoad) > float.Epsilon)
                    OutStr.Append("Load: " + TotalLoad.ToString("#,##0"));
            }
            if (Units != null && Units.Count > 0)
            {
                float TotalGen = 0f, TotalHSL = 0f;
                foreach (MM_Unit Unit in Units)
                {
                    if (!float.IsNaN(Unit.RTGenMW) && Unit.IsPhysical && Unit.RTGenMW != 0)
                        TotalGen += Unit.RTGenMW;
                    else if (!float.IsNaN(Unit.Estimated_MW) && Unit.IsPhysical)
                        TotalGen += Unit.Estimated_MW;
                    if (!float.IsNaN(Unit.EcoMax))
                        TotalHSL += Unit.EcoMax;
                }

                if (MM_Repository.SubstationDisplay.ShowTotalGeneration)
                {
                    OutStr.Append('\n');
                    if (Math.Abs(TotalGen) > float.Epsilon)
                        OutStr.Append("Gen: " + TotalGen.ToString("#,##0"));
                }


                if (MM_Repository.SubstationDisplay.ShowTotalHSL && Math.Abs(TotalHSL) > float.Epsilon)
                {
                    OutStr.Append('\n');
                    if (Math.Abs(TotalHSL) > float.Epsilon)
                        OutStr.Append("EcoMax: " + TotalHSL.ToString("#,##0"));
                }


                if (MM_Repository.SubstationDisplay.ShowRemainingCapacity)
                {
                    OutStr.Append('\n');
                    if (Math.Abs(TotalHSL) > float.Epsilon)
                        OutStr.Append("Rem: " + (TotalHSL - TotalGen).ToString("#,##0"));

                }
            }
            return OutStr.ToString();

        }
    }
}