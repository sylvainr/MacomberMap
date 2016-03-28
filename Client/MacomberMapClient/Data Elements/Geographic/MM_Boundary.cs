using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml;
using MacomberMapClient.User_Interfaces.Generic;

namespace MacomberMapClient.Data_Elements.Geographic
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class contains information on a boundary/region (e.g., State, County, District)
    /// </summary>
    public class MM_Boundary : MM_Element
    {
        #region Variable Declarations

        /// <summary>The largest points within the boundary</summary>
        public PointF Max = new PointF(float.NaN, float.NaN);

        /// <summary>The largest X within the boundary</summary>
        public float Max_X
        {
            get { return Max.X; }
            set { Max.X = value; }
        }

        /// <summary>The largest Y within the boundary</summary>
        public float Max_Y
        {
            get { return Max.Y; }
            set { Max.Y = value; }
        }

        /// <summary>The smallest points within the boundary</summary>
        public PointF Min = new PointF(float.NaN, float.NaN);

        /// <summary>The smallest X within the boundary</summary>
        public float Min_X
        {
            get { return Min.X; }
            set { Min.X = value; }
        }

        /// <summary>The smallest Y within the boundary</summary>
        public float Min_Y
        {
            get { return Min.Y; }
            set { Min.Y = value; }
        }
        /// <summary>The centroid of the boundary</summary>
        public PointF Centroid = new PointF(float.NaN, float.NaN);

        /// <summary>The centroid X within the boundary</summary>
        public float Centroid_X
        {
            get { return Centroid.X; }
            set { Centroid.X = value; }
        }

        /// <summary>The centroid Y within the boundary</summary>
        public float Centroid_Y
        {
            get { return Centroid.Y; }
            set { Centroid.Y = value; }
        }

        /// <summary>The series of coordinates that comprise the boundary</summary>
        public List<PointF> Coordinates = new List<PointF>();

        /// <summary>The website (if any) for the boundary</summary>
        public String Website;

        /// <summary>The graphics path used for hit testing</summary>
        private GraphicsPath HitTestPath = new GraphicsPath();

        /// <summary>The display settings to be used for drawing the state</summary>
        public static MM_DisplayParameter StateDisplay;

        /// <summary>The display settings to be used for drawing the county</summary>
        public static MM_DisplayParameter CountyDisplay;

        /// <summary>The collection of weather stations within the county</summary>
        public List<Weather.MM_WeatherStation> WeatherStations;

        /// <summary>Weather alerts for the location</summary>
        public List<String> WeatherAlerts;

        /// <summary>The weather forecast for the region</summary>
        public Dictionary<String, String> WeatherForecast;

        /// <summary>The SQL-based locator for the boundary.</summary>
        public int Index;

        /// <summary>The collection of substations within this boundary.</summary>
        public List<MM_Substation> Substations = new List<MM_Substation>();
        #endregion

        #region Data aggregation
        /// <summary>
        /// Get the list of buses in our susbstations
        /// </summary>
        public MM_Bus[] Buses
        {
            get
            {
                Dictionary<int, MM_Bus> FoundBuses = new Dictionary<int, MM_Bus>();
                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.BreakerSwitches != null)
                        foreach (MM_Breaker_Switch BreakerSwitch in Sub.BreakerSwitches)
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
                    if (Bus.Permitted && (Bus.KVLevel == null || Bus.KVLevel.ShowOnMap) && !float.IsNaN(BusPU = Bus.PerUnit) && !Bus.Dead)
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
        /// Determine the largest absolute angle in the county
        /// </summary>
        public float LargestAngle
        {
            get
            {
                float MaxAngle = 0f;
                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.BusbarSections != null)
                        foreach (MM_Bus Bus in Sub.BusbarSections)
                            if (Bus.Permitted && Bus.KVLevel.ShowOnMap && !float.IsNaN(Bus.Estimated_Angle))
                                if (Math.Abs(Bus.Estimated_Angle) > Math.Abs(MaxAngle))
                                    MaxAngle = Bus.Estimated_Angle;
                return MaxAngle;
            }
        }

        /// <summary>
        /// Determine the standard deviation of angle differentials in the county
        /// </summary>
        public float AngleStdDev
        {
            get
            {
                float Sum = 0f, Sum2 = 0f, Count = 0f;
                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.BusbarSections != null)
                        foreach (MM_Bus Bus in Sub.BusbarSections)
                            if (Bus.Permitted && Bus.KVLevel.ShowOnMap && !float.IsNaN(Bus.Estimated_Angle))
                            {
                                Sum += Bus.Estimated_Angle;
                                Sum2 += Bus.Estimated_Angle * Bus.Estimated_Angle;
                                Count += 1f;
                            }
                if (Count < 2f)
                    return 0;
                return -(float)Math.Sign(Sum) * (float)Math.Sqrt((((float)Count * Sum2) - (Sum * Sum)) / ((float)Count * ((float)Count - 1f)));
            }
        }


        /// <summary>
        /// Determine the standard deviation of angle differentials in the county
        /// </summary>
        public float LMPStdDev
        {
            get
            {
                float Sum = 0f, Sum2 = 0f, Count = 0f;
                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.Units != null)
                        foreach (MM_Unit Unit in Sub.Units)
                            if (Unit.Permitted && !float.IsNaN(Unit.LMP))
                            {
                                Sum += Unit.LMP;
                                Sum2 += Unit.LMP * Unit.LMP;
                                Count += 1f;
                            }
                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.Loads != null)
                        foreach (MM_Load load in Sub.Loads)
                            if (load.Permitted && !float.IsNaN(load.Lmp))
                            {
                                Sum += load.Lmp;
                                Sum2 += load.Lmp * load.Lmp;
                                Count += 1f;
                            }


                if (Count < 2f)
                    return 0;
                return -(float)Math.Sign(Sum) * (float)Math.Sqrt((((float)Count * Sum2) - (Sum * Sum)) / ((float)Count * ((float)Count - 1f)));
            }
        }


        /// <summary>
        /// Determine the average LMP        
        /// </summary>
        public float LMPAverage
        {
            get
            {
                float SumLMP = 0f, CountLMP = 0f;
                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.Units != null)
                        foreach (MM_Unit Unit in Sub.Units)
                            if (Unit.Permitted && !float.IsNaN(Unit.LMP))
                            {
                                CountLMP++;
                                SumLMP += Unit.LMP;
                            }

                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.Loads != null)
                        foreach (MM_Load load in Sub.Loads)
                            if (load.Permitted && !float.IsNaN(load.Lmp) && load.Lmp != 0)
                            {
                                CountLMP++;
                                SumLMP += load.Lmp;
                            }


                return SumLMP / CountLMP;
            }
        }

        /// <summary>
        /// Determine the average LMP        
        /// </summary>
        public float LMPMaximum
        {
            get
            {
                float MaxLMP = float.NaN;

                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.Units != null)
                        foreach (MM_Unit Unit in Sub.Units)
                            if (Unit.Permitted && !float.IsNaN(Unit.LMP))
                                MaxLMP = Math.Max(MaxLMP, Unit.LMP);

                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.Loads != null)
                        foreach (MM_Load load in Sub.Loads)
                            if (load.Permitted && !float.IsNaN(load.Lmp) && load.Lmp != 0)
                                MaxLMP = Math.Max(MaxLMP, load.Lmp);


                return MaxLMP;
            }
        }

        /// <summary>
        /// Determine the average LMP        
        /// </summary>
        public float LMPMinimum
        {
            get
            {
                float MinLMP = float.NaN;

                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.Units != null)
                        foreach (MM_Unit Unit in Sub.Units)
                            if (Unit.Permitted && !float.IsNaN(Unit.LMP))
                                MinLMP = Math.Min(MinLMP, Unit.LMP);

                foreach (MM_Substation Sub in this.Substations)
                    if (Sub.Loads != null)
                        foreach (MM_Load load in Sub.Loads)
                            if (load.Permitted && !float.IsNaN(load.Lmp) && load.Lmp != 0)
                                MinLMP = Math.Min(MinLMP, load.Lmp);


                return MinLMP; 
            }
        }

        /// <summary>
        /// Retrieve the worst pU for the boundary
        /// </summary>
        public float Worst_pU
        {
            get
            {
                float CapWorst = float.NaN, ReacWorst = float.NaN;
                foreach (MM_Substation Sub in Substations)
                    if (Sub.BusbarSections != null)
                        foreach (MM_Bus Bus in Sub.BusbarSections)
                            if (Bus.KVLevel.Permitted)
                                if (float.IsNaN(CapWorst) || (!float.IsNaN(Bus.PerUnitVoltage) && Bus.PerUnitVoltage > CapWorst))
                                    CapWorst = Bus.PerUnitVoltage;
                                else if (float.IsNaN(ReacWorst) || (!float.IsNaN(Bus.PerUnitVoltage) && Bus.PerUnitVoltage < ReacWorst))
                                    ReacWorst = Bus.PerUnitVoltage;
                if (float.IsNaN(CapWorst) && float.IsNaN(ReacWorst))
                    return float.NaN;
                else if (float.IsNaN(CapWorst))
                    return ReacWorst;
                else if (float.IsNaN(ReacWorst))
                    return CapWorst;
                else if (1 - ReacWorst > CapWorst)
                    return ReacWorst;
                else
                    return CapWorst;
            }
        }



        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new boundary
        /// </summary>
        public MM_Boundary()
            : base()
        {
        }

        /// <summary>
        /// Create a boundary using a data reader
        /// </summary>
        ///<param name="sRd">SQL Data reader</param>
        ///<param name="AddIfNew"></param>
        public MM_Boundary(DbDataReader sRd, bool AddIfNew)
            : base(sRd, AddIfNew)
        {
            byte[] inBytes = (byte[])sRd["COORDINATES"];
            Coordinates = new List<PointF>(inBytes.Length / 8);
            List<PointF> ConvertedPoints = new List<PointF>(Coordinates.Count);
            for (int a = 0; a < inBytes.Length; a += 8)
            {
                Coordinates[a / 8] = new PointF(BitConverter.ToSingle(inBytes, a), BitConverter.ToSingle(inBytes, a + 4));
                ConvertedPoints.Add(new PointF(Coordinates[a / 8].X * 1000f, Coordinates[a / 8].Y * 1000f));
            }
            HitTestPath.AddLines(ConvertedPoints.ToArray());
            ConvertedPoints.Clear();

            if ((string)sRd["Name"] == "STATE")
                this.ElemType = MM_Repository.FindElementType("State");
            else
                this.ElemType = MM_Repository.FindElementType("County");
        }

        /// <summary>
        /// Initialize a new boundary
        /// </summary>
        /// <param name="xE"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Boundary(XmlElement xE, bool AddIfNew)
            : base(xE, AddIfNew)
        {
            //Because of an issue where some boundaries contain multiple substations of the same TEID, fix it here.
            Dictionary<Int32, MM_Substation> Subs = new Dictionary<Int32, MM_Substation>(Substations.Count);
            foreach (MM_Substation Sub in this.Substations)
                if (!Subs.ContainsKey(Sub.TEID))
                    Subs.Add(Sub.TEID, Sub);
            Substations.Clear();
            Substations.AddRange(Subs.Values);

            if (xE.HasAttribute("Max_X"))
            {
                this.Max = new PointF(MM_Converter.ToSingle(xE.Attributes["Max_X"].Value), MM_Converter.ToSingle(xE.Attributes["Max_Y"].Value));
                this.Min = new PointF(MM_Converter.ToSingle(xE.Attributes["Min_X"].Value), MM_Converter.ToSingle(xE.Attributes["Min_Y"].Value));
                this.Centroid = new PointF(MM_Converter.ToSingle(xE.Attributes["Centroid_X"].Value), MM_Converter.ToSingle(xE.Attributes["Centroid_Y"].Value));
            }
            if (xE.HasAttribute("Boundary_Website"))
                this.Website = xE.Attributes["Boundary_Website"].Value;

            if (xE.HasAttribute("Coordinates") || xE["Coordinates"] != null)
            {
                String[] Coords = null;
                if (xE["Coordinates"] != null)
                    Coords = xE["Coordinates"].InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                else
                    Coords = xE.Attributes["Coordinates"].InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<PointF> OutCoordinates = new List<PointF>();
                List<PointF> OutConvertedCoordinates = new List<PointF>();
                for (int a = 0; a < Coords.Length; a += 2)
                {
                    PointF OutPt = new PointF(MM_Converter.ToSingle(Coords[a]), MM_Converter.ToSingle(Coords[a + 1]));
                    OutCoordinates.Add(OutPt);
                    OutConvertedCoordinates.Add(new PointF(OutPt.X * 1000f, OutPt.Y * 1000f));
                }

                //Now make sure the last coordiante closes it out.
                if (OutCoordinates[OutCoordinates.Count - 1].Equals(OutCoordinates[0]) == false)
                {
                    OutCoordinates.Add(OutCoordinates[0]);
                    OutConvertedCoordinates.Add(OutConvertedCoordinates[0]);
                }
                Coordinates = OutCoordinates;
                HitTestPath.AddLines(OutConvertedCoordinates.ToArray());
            }
            else
            {
                List<PointF> OutConvertedCoordinates = new List<PointF>();
                foreach (PointF OutPt in Coordinates)
                    OutConvertedCoordinates.Add(new PointF(OutPt.X * 1000f, OutPt.Y * 1000f));
                try
                {
                    HitTestPath.AddLines(OutConvertedCoordinates.ToArray());
                } catch (Exception ex)
                {
                    MM_System_Interfaces.LogError(ex);
                }
            }
        }
        /// <summary>
        /// Do some sort of hit point test
        /// </summary>
        /// <param name="coordinates"></param>
        public void AddLines(PointF[] coordinates)
        {
            for (int i = 0; i < coordinates.Length; i++)
                coordinates[i] = new PointF(coordinates[i].X * 1000f, coordinates[i].Y * 1000f);

            HitTestPath.AddLines(coordinates);
        }

        /*
        /// <summary>
        /// Create a new boundary (from an XML element)
        /// </summary>
        /// <param name="xE">The element containing boundary information</param>
        public MM_Boundary(XmlElement xE)
        {
            this.Name = xE.Attributes["Name"].Value;
            this.Max = new PointF(MM_Converter.ToSingle(xE.Attributes["Max_X"].Value), MM_Converter.ToSingle(xE.Attributes["Max_Y"].Value));
            this.Min = new PointF(MM_Converter.ToSingle(xE.Attributes["Min_X"].Value), MM_Converter.ToSingle(xE.Attributes["Min_Y"].Value));
            this.Centroid = new PointF(MM_Converter.ToSingle(xE.Attributes["Centroid_X"].Value), MM_Converter.ToSingle(xE.Attributes["Centroid_Y"].Value));
            if (xE.HasAttribute("Boundary_Website"))
                this.Website = xE.Attributes["Boundary_Website"].Value;

            String[] splCoords;
            if (xE["Coordinates"] == null)
                splCoords = xE.InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            else
                splCoords = xE["Coordinates"].InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Coordinates = new PointF[splCoords.Length / 2];
            List<PointF> ConvertedPoints = new List<PointF>(Coordinates.Length);

#if LimitedCoordinates
            List<PointF> NewPoints = new List<PointF>();
            int NumDecPlaces = 3;
            for (int a = 0; a < splCoords.Length; a += 2)
            {
                PointF ThisPoint = RoundedPoint(splCoords[a], splCoords[a + 1], NumDecPlaces);
                if (NewPoints.Count == 0 || ThisPoint.X != NewPoints[NewPoints.Count - 1].X || ThisPoint.Y != NewPoints[NewPoints.Count - 1].Y)
                {
                    NewPoints.Add(ThisPoint);
                    ConvertedPoints.Add(new PointF(ThisPoint.X * 1000f, ThisPoint.Y * 1000f));
                }
            }
            Coordinates = NewPoints.ToArray();

#else
            for (int a = 0; a < Coordinates.Length; a++)
            {
                Coordinates[a] = new PointF(MM_Converter.ToSingle(splCoords[a * 2]), MM_Converter.ToSingle(splCoords[(a * 2) + 1]));
                ConvertedPoints.Add(new PointF(Coordinates[a].X * 1000f, Coordinates[a].Y * 1000f));
            }
#endif
            HitTestPath.AddLines(ConvertedPoints.ToArray());
            ConvertedPoints.Clear();

            WeatherAlerts = new List<string>(xE.SelectNodes("WeatherAlert").Count);
            WeatherForecast = new Dictionary<string, string>(xE.SelectNodes("WeatherForecast").Count);
            WeatherStations = new List<MM_WeatherStation>(xE.SelectNodes("WeatherStation").Count);

            foreach (XmlNode xE2 in xE.ChildNodes)
                if (xE2 is XmlElement == false)
                { }
                else if (xE2.Name == "WeatherAlert")
                    WeatherAlerts.Add((xE2.Attributes["Value"].Value));
                else if (xE2.Name == "WeatherForecast")
                    WeatherForecast.Add((xE2.Attributes["Name"].Value), (xE2.Attributes["Value"].Value));
                else if (xE2.Name == "WeatherStation")
                    this.WeatherStations.Add(new MM_WeatherStation(xE2 as XmlElement));



        }*/

        /// <summary>
        /// Convert a string to a rounded decimal point
        /// </summary>
        /// <param name="X">The X coordinate</param>
        /// <param name="Y">The Y coordinate</param>
        /// <param name="NumDecPlaces">The number of decimal places</param>
        /// <returns></returns>
        private PointF RoundedPoint(string X, string Y, int NumDecPlaces)
        {
            return new PointF((float)Math.Round(MM_Converter.ToSingle(X), NumDecPlaces), (float)Math.Round(MM_Converter.ToSingle(Y), NumDecPlaces));
        }


        #endregion

        #region Xml Element saving
        /// <summary>
        /// Convert the boundary into an XML element
        /// </summary>
        /// <param name="xDoc">The XML Document used to create the element</param>
        /// <returns>The newly-created element</returns>
        public XmlElement BoundaryToXML(XmlDocument xDoc)
        {
            XmlElement outElem = xDoc.CreateElement("Boundary");
            outElem.Attributes.Append(xDoc.CreateAttribute("Name")).Value = this.Name;
            outElem.Attributes.Append(xDoc.CreateAttribute("Max_X")).Value = this.Max.X.ToString();
            outElem.Attributes.Append(xDoc.CreateAttribute("Max_Y")).Value = this.Max.Y.ToString();
            outElem.Attributes.Append(xDoc.CreateAttribute("Min_X")).Value = this.Min.X.ToString();
            outElem.Attributes.Append(xDoc.CreateAttribute("Min_Y")).Value = this.Min.Y.ToString();
            outElem.Attributes.Append(xDoc.CreateAttribute("Centroid_X")).Value = this.Centroid.X.ToString();
            outElem.Attributes.Append(xDoc.CreateAttribute("Centroid_Y")).Value = this.Centroid.Y.ToString();
            outElem.Attributes.Append(xDoc.CreateAttribute("Boundary_Website")).Value = this.Website;
            foreach (PointF LngLat in Coordinates)
                outElem.InnerText += LngLat.X.ToString() + "," + LngLat.Y.ToString() + ",";
            return outElem;
        }
        #endregion

        #region Hit testing
        /// <summary>
        /// Test to see whether the specified lat/long is within this boundary
        /// </summary>
        /// <param name="TestPoint"></param>
        /// <returns></returns>
        public bool HitTest(PointF TestPoint)
        {
            return HitTestPath.IsVisible(new PointF(TestPoint.X * 1000f, TestPoint.Y * 1000f));
        }


        /// <summary>
        /// Determine whether a line is contained within, or intersects the hit test path
        /// </summary>
        /// <param name="StartLine"></param>
        /// <param name="EndLine"></param>
        /// <returns></returns>
        public bool HitTest(PointF StartLine, PointF EndLine)
        {
            using (GraphicsPath LinePath = new GraphicsPath())
            {

                LinePath.AddLine(StartLine.X * 1000f, StartLine.Y * 1000f, EndLine.X * 1000f, EndLine.Y * 1000f);
                using (Region LineRegion = new Region(LinePath))
                using (Region HitTestRegion = new Region(HitTestPath))
                {
                    LineRegion.Intersect(HitTestRegion);
                    return !LineRegion.GetBounds(Graphics.FromHwnd(IntPtr.Zero)).IsEmpty;
                }

            }
        }
        #endregion

        /// <summary>
        /// Retrieve the name of the boundary
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Determine whether the boundary is visible
        /// </summary>
        /// <param name="Coord">The coordinates to check against</param>
        /// <returns>Whether or not the boundary could possibly be visible on the screen</returns>
        public bool CheckVisibility(RectangleF Coord)
        {
            if (Coord.Right < Min.X || Coord.Top < Min.Y)
                return false;
            if (Coord.Left > Max.X || Coord.Bottom > Max.Y)
                return false;
            return true;
        }

        /// <summary>
        /// Return the appropriate display parameters for the boundary
        /// </summary>

        public MM_DisplayParameter DisplayParameter
        {
            get
            {
                if (this.Name == "STATE")
                    return MM_Boundary.StateDisplay;
                else
                    return MM_Boundary.CountyDisplay;
            }
        }
    }
}