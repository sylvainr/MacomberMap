using MacomberMapClient.Integration;
using MacomberMapCommunications.Messages.EMS;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class defines a generator/unit
    /// </summary>
    public class MM_Unit : MM_Element, IComparable
    {
        

#region Variable declarations
        /// <summary>The type of generating unit this is</summary>
        public MM_Unit_Type UnitType;

        public bool MultipleNetmomForEachGenmom = false;
        /// <summary>
        /// Reserve zone this unit is in.
        /// </summary>
        public int ReserveZone = 0;

        /// <summary>The next scheduled outage, if any</summary>
        public MM_ScheduledOutage UpcomingOutage = null;

        /// <summary>SCED base point</summary>       
        public float SCED_Basepoint = float.NaN;

        /// <summary>Locational marginal price</summary>
        public float LMP = float.NaN;

        /// <summary>The maximal capacity of the unit</summary>
        public float MaxCapacity = float.NaN;

        /// <summary>Settlement point price</summary>
        public float SPP = float.NaN;

        /// <summary>High ancillary service limit</summary>
        public float RegUp = float.NaN;

        /// <summary>High emergency limit </summary>
        public float EmerMax = float.NaN;

        /// <summary>High sustained limit</summary>
        public float EcoMax = float.NaN;

        /// <summary>High dispatch limit</summary>
        public float MosMax = float.NaN;

        /// <summary>Low ancillary service limit</summary>        
        public float RegDown = float.NaN;

        /// <summary>Low emergency limit</summary>
        public float EmerMin = float.NaN;

        /// <summary>Low sustained limit</summary>
        public float EcoMin = float.NaN;

        /// <summary>Low dispatch limit</summary>
        public float MosMin = float.NaN;
		
		 /// <summary>High ancillary service limit</summary>
        public float HASL = float.NaN;

        /// <summary>High emergency limit </summary>
        public float HEL = float.NaN;

        /// <summary>High sustained limit</summary>
        public float HSL = float.NaN;

        /// <summary>High dispatch limit</summary>
        public float HDL = float.NaN;

        /// <summary>Low ancillary service limit</summary>        
        public float LASL = float.NaN;

        /// <summary>Low emergency limit</summary>
        public float LEL = float.NaN;

        /// <summary>Low sustained limit</summary>
        public float LSL = float.NaN;

        /// <summary>Low dispatch limit</summary>
        public float LDL = float.NaN;

		/// <summary>Desired MW</summary>
        public float Desired_MW = float.NaN;
		
        /// <summary>
        /// Generation from RTGen
        /// </summary>
        public float RTGenMW = 0;

        /// <summary>
        /// Spinning Cap from RTGen
        /// </summary>
        public float SpinningCapacity = float.NaN;

        /// <summary>Estimated MW flow</summary>
        public float Estimated_MW = float.NaN;

        /// <summary>Estimated MVAR flow</summary>
        public float Estimated_MVAR = float.NaN;

        /// <summary>Estimated MVA flow</summary>
        public float Estimated_MVA = float.NaN;

        /// <summary>Telemetered MW flow</summary>
        public float Telemetered_MW = float.NaN;

        /// <summary>Telemetered MVAR flow</summary>
        public float Telemetered_MVAR = float.NaN;

        /// <summary>The derived telemetered MVA of the load</summary>
        public float Telemetered_MVA = float.NaN;

        /// <summary>The spinning capacity of the unit</summary>
        public float ClearedSpinningCapacity = float.NaN;

        /// <summary>The online PRC of the unit</summary>
        public float RampedSetPoint = float.NaN;

        /// <summary>Our up ramp rate</summary>
        public float RampRateUp = float.NaN;

        /// <summary>The primary fuel type of our unit</summary>
        public MM_Unit_Type PrimaryFuelType;

        /// <summary>
        /// Our fuel type, either the primary fuel source, or the unit type (e.g., CC)
        /// </summary>
        public MM_Unit_Type FuelType
        {
            get { return PrimaryFuelType == null ? UnitType : PrimaryFuelType; }
        }

        /// <summary>Our maximum seasonal sustainable rating</summary>
        public float SeasonalMaxSustainableRating = float.NaN;

    /// <summary>The online PRC of the unit</summary>
        public float Physical_Responsive_Online = float.NaN;

   /// <summary>The PR sync</summary>
        public float Physical_Responsive_Sync = float.NaN;

        /// <summary>The blackstart capacity of the unit</summary>
        public float Blackstart_Capacity = float.NaN;

        /// <summary>The RMR capacity of the unit</summary>
        public float RMR_Capacity = float.NaN;
		
		      /// <summary>The Ramp Rate of the unit</summary>
        public float RampRate = float.NaN;

        /// <summary>The Voltage Target of the unit</summary>
        public float VoltageTarget = float.NaN;

        /// <summary>The frequency Target of the unit</summary>
        public float FreqTarget = float.NaN;

        /// <summary>The frequency tolerance of the unit</summary>
        public float FreqToler = float.NaN;
		
        /// <summary>The PR sync</summary>
        public float RegulationDeployed = float.NaN;

        /// <summary>The blackstart capacity of the unit</summary>
        public float DispatchMW = float.NaN;

        /// <summary>The RMR capacity of the unit</summary>
        public float SetPoint = float.NaN;

        /// <summary>Our collection of logical units</summary>
        public MM_Unit[] LogicalUnits = new MM_Unit_Logical[0];

        public string Fuel = String.Empty;

        /// <summary>Determine whether the unit is physical</summary>
        public bool IsPhysical
        {
            get { return this.GetType().Name == "MM_Unit"; }
        }

        /// <summary>Whether the unit is linked to a combined cycle unit</summary>
        public bool IsRC;

        /// <summary>The TEID of the physical unit</summary>
        public Int32 UnitTEID;

        /// <summary>Whether the unit is on frequency control</summary>
        public bool FrequencyControl = false;

        /// <summary>The unit's frequency</summary>
        public float Frequency = 60;

		/// <summary>This particular unit's nominal RPM</summary>
        public double NominalRPM = 1800;
		

        /// <summary>Removed status flag of our unit</summary>
        public bool RemovedStatus = false;

        /// <summary>Whether the unit is marked as open</summary>
        public bool OpenBreaker = false;

        /// <summary>Whether PSS is deactivated</summary>
        public bool NoPSS = false;

        /// <summary>Whether AVR is deactivated</summary>
        public bool NoAVR = false;

        /// <summary>Whether AGC is deactivated</summary>
        public bool isAGC = false;
		
        public int CMode;

        /// <summary>Whether Local or Fixed</summary>
        public bool isLocal = true;

        /// <summary>Whether in Manual Voltage Target</summary>
        public bool ManVoltageTarg = false;

        /// <summary>our unit status</summary>
        public string Unit_Status;

        /// <summary>Our collection of private area network types</summary>
        public enum enumPanType
        {
            /// <summary>Not part of a PAN</summary>
            None,
            /// <summary>Mothballed generation</summary>
            MothballedGeneration,
            /// <summary>Private use network</summary>
            PrivateUseNetwork,
            /// <summary>A normally-in BLT</summary>
            BLT_Normally_In,
            /// <summary>Non-modeled generation</summary>
            NonModeledGeneration,
            /// <summary>Retired generation</summary>
            RetiredGeneration
        }

        /// <summary>Our private area network type</summary>
        public enumPanType PANType = enumPanType.None;

        /// <summary>The MVAR capability curve of the unit</summary>
        public float[] MVARCapabilityCurve = new float[0];

        /// <summary>
        /// The market resource name.
        /// </summary>
        public string MarketResourceName;

        public string MarketResourceType;

        public string MarketParticipantName;

        public string ContactInfo;

        public string FriendlyName;

        public string Description;
        /// <summary>
        /// The GenMom Name.
        /// </summary>
        public string GenmomName;

  		/// <summary>Our information about the status around our generator control panel</summary>
        public MM_Unit_Control_Status UnitStatus = new MM_Unit_Control_Status();
        #endregion
        #region Initialization
        /// <summary>
        /// Initialize a new CIM Unit
        /// </summary>
        public MM_Unit()
        {
            this.ElemType = MM_Repository.FindElementType("Unit");
            if (this.UnitType == null)
                this.UnitType = MM_Repository.FindGenerationType("UNKNOWN");
        }


        /// <summary>
        /// Initialize a new CIM Transformer
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Unit(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Unit");
            if (this.UnitType == null)
                this.UnitType = MM_Repository.FindGenerationType("UNKNOWN");
            this.UnitStatus.TEID = this.TEID;
        }

        /// <summary>
        /// Initialize a new CIM Unit
        /// </summary>
        /// <param name="ElementSource">The data source for this Unit</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Unit(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Unit");
            if (this.UnitType == null)
                this.UnitType = MM_Repository.FindGenerationType("UNKNOWN");
            this.UnitStatus.TEID = this.TEID;
        }

        private void CheckElementValue(XmlElement ElementSource, string AttributeName, ref float OutValue)
        {
            if (!ElementSource.HasAttribute(AttributeName))
                return;
            OutValue = MM_Converter.ToSingle(ElementSource.Attributes[AttributeName].Value);
        }
        #endregion


       /* public override string ToString()
        {
            if (MarketResourceName != null)
                return MarketResourceName;
            else
            {
                return base.ToString();
            }
        } */

        //TODO: There's a global called EnergizationThreshold that I've used in the past to track whether components are online
        /// <summary>
        /// Return the percentage this unit is running at
        /// </summary>
        public String UnitPercentageText
        {
            get
            {
			 float MW = Data_Integration.UseEstimates ? Estimated_MW : Telemetered_MW;
			 if (MW < MM_Repository.OverallDisplay.EnergizationThreshold && (float.IsNaN(RTGenMW) || RTGenMW < MM_Repository.OverallDisplay.EnergizationThreshold))
                    return "Offline";
 			 if (float.IsNaN(MaxCapacity))
                    return "?";
                else if (!float.IsNaN(HASL) && Estimated_MW < HASL)               
			    return (Estimated_MW / HASL).ToString("0%") + " HASL";
                else if (!float.IsNaN(HSL) && Estimated_MW < HSL)
                    return (Estimated_MW / HSL).ToString("0%") + " HSL";
                else if (!float.IsNaN(HDL) && Estimated_MW < HDL)
                    return (Estimated_MW / HDL).ToString("0%") + " HDL";
                else if (!float.IsNaN(HEL) && Estimated_MW < HEL)
                    return (Estimated_MW / HEL).ToString("0%") + " HEL";
                else 
                return Math.Round((MW / (!float.IsNaN(EcoMax) && EcoMax > this.MaxCapacity ? EcoMax : MaxCapacity)) * 100) + " % " ;
            }
        }

        /// <summary>
        /// Return the percentage this unit is loaded
        /// </summary>
        /// <param name="MW">The MW of the unit</param>        
        /// <returns></returns>
        public string UnitPercentageTextFromValues(float MW)
        {
		if (!float.IsNaN(HSL))
		  return (MW / HSL).ToString("0%") + " HSL";
		  else
            return (MW / EcoMax).ToString("0%") + " EcoMax";
        }
		
		
        /// <summary>
        /// Max MVAR detected recently
        /// </summary>
        public float MaxMVAR = 0;
        /// <summary>
        /// Report the unit MVAR capacity at our current point
        /// </summary>
        public float UnitMVARCapacity
        {
            get
            {
                if (float.IsNaN(MaxMVAR) || MaxMVAR == 0)
                    MaxMVAR = .1f;
                if (Math.Abs(Estimated_MVAR) > Math.Abs(MaxMVAR))
                    MaxMVAR = Estimated_MVAR;
                if (Math.Abs(Telemetered_MVAR) > Math.Abs(MaxMVAR))
                    MaxMVAR = Telemetered_MVAR;
                if (float.IsNaN(Estimated_MW) || MVARCapabilityCurve == null || MVARCapabilityCurve.Length <= 1 || Estimated_MVA <= MM_Repository.OverallDisplay.EnergizationThreshold)
                    return MaxMVAR;

                if (MVARCapabilityCurve.Length > 2 && MVARCapabilityCurve[1] == 0)
				{
					float Cap, Reac;
	                DetermineMVARCapabilities(out Cap, out Reac);
					MaxMVAR = (Data_Integration.UseEstimates ? Estimated_MVAR : Telemetered_MVAR) < 0? Cap : Reac;
                }
                    return MaxMVAR;
     }
        }

        #region MVAR Capability Curve drawing
        /// <summary>
        /// Draw the unit's MVAR capability curve
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Rect"></param>
        /// <param name="DarkBackground"></param>
        public void DrawUnitMVARCapabilityCurve(Graphics g, Rectangle Rect, bool DarkBackground)
        {
            if (MVARCapabilityCurve.Length == 0)
            {
                g.Clear(DarkBackground ? Color.Black : Color.White);
                return;
            }
            using (Chart NewChart = new Chart() { Size = Rect.Size })
            using (Bitmap DrawBitmap = new Bitmap(Rect.Width, Rect.Height, PixelFormat.Format32bppArgb))
                try
                {
                    ChartArea Area = NewChart.ChartAreas.Add("MVAR Capability");
                    NewChart.Titles.Add("MVAR: " + this.ToString());
                    Area.AxisY.Title = "MVAR";
                    Area.AxisX.Title = "MW";
                    Area.AxisX.LabelStyle.Format = "0";
                    Area.AxisX.MajorGrid.LineColor = Area.AxisX.MinorGrid.LineColor = Area.AxisY.MajorGrid.LineColor = Area.AxisY.MinorGrid.LineColor = Color.LightGray;
                    Area.AxisX.MajorGrid.Enabled = Area.AxisY.MajorGrid.Enabled = false;
                    if (DarkBackground)
                    {
                        Area.BackColor = Color.Black;
                        NewChart.BackColor = Color.Black;
                        NewChart.ForeColor = Color.White;
                        Area.AxisX.LabelStyle.ForeColor = Color.White;
                        Area.AxisY.LabelStyle.ForeColor = Color.White;
                        Area.AxisX.TitleForeColor = Color.White;
                        Area.AxisX.LineColor = Color.White;
                        Area.AxisY.TitleForeColor = Color.White;
                        Area.AxisY.LineColor = Color.White;
                        NewChart.Titles[0].ForeColor = Color.White;
                        Area.AxisX.MajorTickMark.LineColor = Color.White;
                        Area.AxisY.MajorTickMark.LineColor = Color.White;
                    }
                    Series MVAR = NewChart.Series.Add("MVAR Capability Curve");
                    Series MVAR2 = NewChart.Series.Add("MVAR Capability Curve2");
                    MVAR.BorderWidth = 2;
                    MVAR2.BorderWidth = 2;
                    MVAR.ChartType = SeriesChartType.Line;
                    MVAR2.ChartType = SeriesChartType.Line;

                    MVAR.Points.AddXY(0, MVARCapabilityCurve[1]);
                    MVAR2.Points.AddXY(0, MVARCapabilityCurve[2]);
                    for (int a = 0; a < MVARCapabilityCurve.Length; a += 3)
                    {
                        MVAR.Points.AddXY(MVARCapabilityCurve[a], MVARCapabilityCurve[a + 1]);
                        MVAR2.Points.AddXY(MVARCapabilityCurve[a], MVARCapabilityCurve[a + 2]);
                    }
                    Area.AxisX.Minimum = 0;

                    //Add our lines
                    AddVerticalLine(NewChart, LEL, "LEL", Color.Red);
                    AddVerticalLine(NewChart, LSL, "LSL", Color.Yellow);
                    AddVerticalLine(NewChart, LDL, "LDL", Color.Blue);
                    AddVerticalLine(NewChart, LASL, "LASL", Color.Magenta);
                    AddVerticalLine(NewChart, HEL, "HEL", Color.Red);
                    AddVerticalLine(NewChart, HSL, "HSL", Color.Yellow);
                    AddVerticalLine(NewChart, HDL, "HDL", Color.Blue);
                    AddVerticalLine(NewChart, HASL, "HASL", Color.Magenta);
                    AddVerticalLine(NewChart, Estimated_MW, "MW", Color.Green, 2f);
                    AddVerticalLine(NewChart, MaxCapacity, "Max", Color.Red, 2f);

                    HorizontalLineAnnotation hla = new HorizontalLineAnnotation();
                    hla.ClipToChartArea = Area.Name;
                    hla.AxisX = Area.AxisX;
                    hla.AxisY = Area.AxisY;
                    hla.IsInfinitive = true;
                    hla.LineColor = Color.Green;
                    hla.Y = Estimated_MVAR;
                    hla.LineWidth = 2;

                    NewChart.Annotations.Add(hla);
                    NewChart.DrawToBitmap(DrawBitmap, Rect);
                    g.DrawImageUnscaled(DrawBitmap, Rect.Location);
                }
                catch { }
        }

        /// <summary>
        /// Add a vertical line to our chart
        /// </summary>
        /// <param name="Chart"></param>
        /// <param name="Value"></param>
        /// <param name="Title"></param>
        /// <param name="TargetColor"></param>
        /// <param name="Width"></param>
        private void AddVerticalLine(Chart Chart, float Value, String Title, Color TargetColor, float Width = 1f)
        {
            if (float.IsNaN(Value))
                return;

            VerticalLineAnnotation vla = new VerticalLineAnnotation();
            vla.ClipToChartArea = Chart.ChartAreas[0].Name;
            vla.AxisX = Chart.ChartAreas[0].AxisX;
            vla.LineColor = TargetColor;
            vla.LineWidth = (int)Width;
            vla.IsInfinitive = true;
            vla.AxisY = Chart.ChartAreas[0].AxisY;
            vla.X = Value;
            Chart.Annotations.Add(vla);
        }
        #endregion


        /// <summary>
        /// Report the MVAR cpabilities from our current MW Level;
        /// </summary>
        /// <param name="Cap"></param>
        /// <param name="Reac"></param>
        public void DetermineMVARCapabilities(out float Cap, out float Reac)
        {
            if (MVARCapabilityCurve.Length == 0)
                Cap = Reac = 0;
            else
            {
                int CurPosition = 0;
                while (CurPosition < MVARCapabilityCurve.Length && MVARCapabilityCurve[CurPosition] < (Data_Integration.UseEstimates ? Estimated_MW : Telemetered_MW))
                    CurPosition += 3;

                if (CurPosition == 0)
                {
                    Cap = MVARCapabilityCurve[1];
                    Reac = MVARCapabilityCurve[2];
                }
                else if (CurPosition == MVARCapabilityCurve.Length)
                {
                    Cap = 0;
                    Reac = 0;
                }
                else
                {
                    float DeltaMW = MVARCapabilityCurve[CurPosition] - MVARCapabilityCurve[CurPosition - 3];
                    float DeltaCap = MVARCapabilityCurve[CurPosition - 2] - MVARCapabilityCurve[CurPosition + 1];
                    float DeltaReac = MVARCapabilityCurve[CurPosition - 1] - MVARCapabilityCurve[CurPosition + 2];
                    float Perc = (MVARCapabilityCurve[CurPosition] - (Data_Integration.UseEstimates ? Estimated_MW : Telemetered_MW)) / DeltaMW;
                    Cap = MVARCapabilityCurve[CurPosition + 1] + (Perc * DeltaCap);
                    Reac = MVARCapabilityCurve[CurPosition + 2] + (Perc * DeltaReac);
                }
            }
        }

        /// <summary>
        /// Return the MVAR percentage that this unit is loaded
        /// </summary>
        public float UnitReactivePercentage
        {
            get
            {
                if (Estimated_MVA <= MM_Repository.OverallDisplay.EnergizationThreshold)
                    return 0;
                else if (!float.IsNaN(UnitMVARCapacity) && UnitMVARCapacity != 0)
                  return (Data_Integration.UseEstimates ? Estimated_MVAR : Telemetered_MVAR) / UnitMVARCapacity;
                if (MaxMVAR > 0)
                    return (Data_Integration.UseEstimates ? Estimated_MVAR : Telemetered_MVAR) / MaxMVAR;
                else return .95f;
            }
        }

        /// <summary>
        /// Return the percentage of the unit
        /// </summary>
        public float UnitPercentage
        {
            get
            {
               
			    float MW = (Data_Integration.UseEstimates ? Estimated_MVAR : Telemetered_MVAR);
                if (RTGenMW > 0)
                    MW = RTGenMW;
              
				float Max = !float.IsNaN(HSL) ? HSL : EcoMax;

                return (float)Math.Round((MW / (!float.IsNaN(Max) && Max > this.MaxCapacity ? Max : MaxCapacity)));
            }
        }        

        #region IComparable Members
        /// <summary>
        /// Compare two units
        /// </summary>
        /// <param name="Unit">The other unit</param>
        /// <returns></returns>
        public int CompareUnits(MM_Unit Unit)
        {
            if (Unit.TEID == TEID)
                return 0;
            else if (this.Estimated_MW == Unit.Estimated_MW)
				if (!float.IsNaN(HSL) && !float.IsNaN(Unit.HSL))
					return -HSL.CompareTo(Unit.HSL);
				else
	                return -EcoMax.CompareTo(Unit.EcoMax);
            else
                return -(Data_Integration.UseEstimates ? Estimated_MW : Telemetered_MW).CompareTo((Data_Integration.UseEstimates ? Unit.Estimated_MW : Unit.Telemetered_MW));
        }

        #endregion
    }
}