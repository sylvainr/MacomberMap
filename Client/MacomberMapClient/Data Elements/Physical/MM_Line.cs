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

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class contains information on a transmission line        
    /// </summary>
    public class MM_Line : MM_Element
    {
        #region Variable Declarations
        /// <summary>The counties this line passes through</summary>
        public List<MM_Boundary> Counties = new List<MM_Boundary>();

        /// <summary>Whether the line is a blackstart component</summary>
        public bool IsBlackstart = false;


        /// <summary>
        /// Flag is line has at least one internal substation.
        /// </summary>
        public bool IsInternal
        {
            get { return ((Substation1 != null && Substation1.IsInternal) || (Substation2 != null & Substation2.IsInternal)); }
        }

        /// <summary>Whether the line is a ZBR</summary>
        public bool IsZBR { get; set; }

        /// <summary>The two substations the line connects</summary>
        public MM_Substation[] ConnectedStations = new MM_Substation[2];

        /// <summary>The TEIDs of our connectivity nodes</summary>
        public int[] NodeTEIDs = new int[2];

        /// <summary>Whether the line is connected to normally opened breakers/switches
        public bool NormallyOpened = false;


       /* public bool OpenEnd1 {
            get {

                if (NodeTEIDs != null && NodeTEIDs.Length > 0)
                {
                    MM_Element element = null;
                    MM_Repository.TEIDs.TryGetValue(NodeTEIDs[0], out element);

                    if (element == null)
                        return false;

                    MM_Node node = (MM_Node)element;

                    for (int i = 0; i < node.ConnectedElements.Count; i++)
                        if (node.ConnectedElements[i] is MM_Breaker_Switch && ((MM_Breaker_Switch)node.ConnectedElements[i]).Open)
                            return true;
                }
                return false;
            }

        }

        public bool OpenEnd2
        {
            get
            {
                if (NodeTEIDs != null && NodeTEIDs.Length > 1)
                {
                    MM_Element element = null;
                    MM_Repository.TEIDs.TryGetValue(NodeTEIDs[1], out element);

                    if (element == null)
                        return false;

                    MM_Node node = (MM_Node)element;

                    for (int i = 0;i < node.ConnectedElements.Count;i++)
                        if (node.ConnectedElements[i] is MM_Breaker_Switch && ((MM_Breaker_Switch)node.ConnectedElements[i]).Open)
                            return true;
                }
                return false;
            }
        }
        */
        /// <summary>
        /// Report the bus associated with our element, if we can
        /// </summary>
        public override MM_Bus NearBus
        {
            get
            {
                if (NearBusNumber == -1)
                    return null;

                //If we're flagged as open, don't provide a bus

                MM_Bus Bus = null;
                MM_Repository.BusNumbers.TryGetValue(NearBusNumber, out Bus);

                if (Bus == null)
                    return null;
                try
                {
                    if (Bus.Substation != null && Bus.Substation.Name != this.Substation1.Name && Bus.Substation != this.Substation2)
                    {
                        string hashKey = this.Substation1.Name + this.KVLevel.ToString();
                        Bus = MM_Element.ExtendedBusSearch(hashKey);
                    }
                }catch (Exception)
                {

                    return null;
                }

                return Bus;
            }
        }

        /// <summary>
        /// Report the bus associated with our element, if we can
        /// </summary>
        public override MM_Bus FarBus
        {
            get
            {
                if (FarBusNumber == -1)
                    return null;
                MM_Bus Bus = null;
                MM_Repository.BusNumbers.TryGetValue(FarBusNumber, out Bus);

                if (Bus == null)
                    return null;
                try
                {
                    if (Bus.Substation != null && Bus.Substation.Name != this.Substation2.Name && Bus.Substation != this.Substation1)
                    {
                        string hashKey = this.Substation2.Name + this.KVLevel.ToString();
                        Bus = MM_Element.ExtendedBusSearch(hashKey);
                    }
                }
                catch (Exception)
                {
                    return null;
                }

                return Bus;
            }
        }


        /// <summary>
        /// Get the first substation
        /// </summary>
        public MM_Substation Substation1
        {
            get { return ConnectedStations[0]; }
            set { ConnectedStations[0] = value; }
        }

        /// <summary>
        /// Get the second substation
        /// </summary>
        public MM_Substation Substation2
        {
            get { return ConnectedStations[1]; }
            set { ConnectedStations[1] = value; }
        }


        public string ToEndKey 
        {
            get;
            set;
        }

        public string OnContingencies
        {

            get {
                StringBuilder sb = new StringBuilder(10);

                for (int i = 0; i < Contingencies.Count; i++)
                {
                    bool found = false;
                    for (int j = 0; j < i; j++)
                        if (Contingencies[i].Name == Contingencies[j].Name)
                        {
                            found = true;
                            break;
                        }
                    if (!found && Contingencies[i].Type != "Flowgate")
                        sb.Append(Contingencies[i].Name + " , ");
                }
                if (sb.Length > 1)
                    sb.Remove(sb.Length - 3, 2);

                return sb.ToString();
            }
        }

        public string OnFlowgates
        {

            get
            {
                StringBuilder sb = new StringBuilder(10);

                for (int i = 0;i < Contingencies.Count;i++)
                {
                    bool found = false;
                    for (int j = 0;j < i;j++)
                        if (Contingencies[i].Name == Contingencies[j].Name)
                        {
                            found = true;
                            break;
                        }
                    if (!found && Contingencies[i].Type == "Flowgate")
                        sb.Append(Contingencies[i].Name + " , ");
                }
                if (sb.Length > 1)
                    sb.Remove(sb.Length - 3, 2);

                return sb.ToString();
            }
        }

        public string FromEndKey
        { get; set; }

        /// <summary>Estimated MW flow at each terminal</summary>
        public float[] Estimated_MW = new float[] { float.NaN, float.NaN };

        /// <summary>Telemetered MW flow at each terminal</summary>
        public float[] Telemetered_MW = new float[] { float.NaN, float.NaN };

        /// <summary>Estimated MVAR flow at each terminal</summary>
        public float[] Estimated_MVAR = new float[] { float.NaN, float.NaN };

        /// <summary>Telemetered MVAR flow at each terminal</summary>
        public float[] Telemetered_MVAR = new float[] { float.NaN, float.NaN };

        /// <summary>Estimated MVA flow at each terminal</summary>
        public float[] Estimated_MVA = new float[] { float.NaN, float.NaN };

        /// <summary>Telemetered MVA flow at each terminal</summary>
        public float[] Telemetered_MVA = new float[] { float.NaN, float.NaN };

        /// <summary>The normal, 2 Hour and 15-Minute limits on the line</summary>
        public float[] Limits = new float[] { float.NaN, float.NaN, float.NaN };

        /// <summary>The normal limit of our line</summary>
        public float NormalLimit
        {
            get { return Limits[0]; }
            set { Limits[0] = value; }
        }

        /// <summary>The 2 hour limit of our line</summary>
        public float EmergencyLimit
        {
            get { return Limits[1]; }
            set { Limits[1] = value; }
        }

        /// <summary>The 15 minute limit of our line</summary>
        public float LoadshedLimit
        {
            get { return Limits[2]; }
            set { Limits[2] = value; }
        }


        /// <summary>The TEID of the cim:Line portion of the element</summary>
        public Int32 LineTEID;

        /// <summary>Return a the two counties to which the line is connected</summary>
        public MM_Boundary[] StationCounties
        {
            get { return new MM_Boundary[] { ConnectedStations[0].County, ConnectedStations[1].County }; }
        }

        /// <summary>The next schedeuld outage on the line</summary>
        public MM_ScheduledOutage UpcomingOutage = null;

        /// <summary>The coordinates that handle the line's routing</summary>
        internal List<PointF> _Coordinates = new List<PointF>(4);

        /// <summary>
        /// The coordinates of our line
        /// </summary>
        public List<PointF> Coordinates
        {
            get
            {
                try
                {
                    if (MM_Server_Interface.LoadModelFromSql || (KVLevel != null && KVLevel.ShowLineRouting))
                        return _Coordinates;
                    else if (Substation1 != null && Substation2 != null && KVLevel != null && !KVLevel.ShowLineRouting)
                        return new List<PointF>(6) { Substation1.LngLat, Substation2.LngLat };
                    else 
                        return _Coordinates;
                } catch (Exception)
                {
                    return _Coordinates;
                }
            }
            set { _Coordinates = value; }
        }
        /// <summary>
        /// add coord
        /// </summary>
        /// <param name="point"></param>
        public void AddCoordinate(PointF point)
        {
            _Coordinates.Add(point);
        }

        /// <summary>
        /// add coord at postion 1
        /// </summary>
        /// <param name="point"></param>
        public void InsertCoordinate(PointF point)
        {
            if (KVLevel != null)
                KVLevel.ShowLineRouting = true;
            if (_Coordinates.Count > 1)
                _Coordinates.Insert(1, point);
            else
                _Coordinates.Add(point);
        }

        float _length;
        /// <summary>Whether or not substations 1 and 2 should be flipped for optimal viewing</summary>
        public bool FlipSides;

        /// <summary>
        /// Return the estimated length of the line
        /// </summary>
        public float Length
        {
            get {
                if (_length <= 0)
                    _length = ConnectedStations[0].DistanceTo(ConnectedStations[1]);
                return _length;
            }
        }

        /// <summary>Whether the line is connected to a series compensator</summary>
        public bool IsSeriesCompensator;

        /// <summary>Whether the line is part of a multiple segment</summary>
        public bool IsMultipleSegment;
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize a new CIM Line
        /// </summary>
        public MM_Line()
        {
            this.ElemType = MM_Repository.FindElementType("Line");
        }

        /// <summary>
        /// Initialize a new CIM Line
        /// </summary>
        /// <param name="ElementSource">The data source for this substation</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Line(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {

            //TEID, Name, Owner, Operator, and KVLevel are taken care of by the MM_Element constructor. Here, load in the substations and coordinates if any.
            if (this is MM_Tie)
            {
                this.ConnectedStations = (this as MM_Tie).AssociatedLine.ConnectedStations;
                this.Coordinates = (this as MM_Tie).AssociatedLine.Coordinates;//(this as MM_Tie).AssociatedLine.Coordinates; -//MN// 20130607
            }
            else
            {
                this.ConnectedStations = new MM_Substation[] { (MM_Substation)MM_Repository.TEIDs[Convert.ToInt32(ElementSource["SUBSTATION1"])], (MM_Substation)MM_Repository.TEIDs[Convert.ToInt32(ElementSource["SUBSTATION2"])] };
                this.Coordinates = new List<PointF> { ConnectedStations[0].LngLat, ConnectedStations[1].LngLat };
            }
            //Make sure we have this element type
            this.ElemType = MM_Repository.FindElementType("Line");



        }

        /// <summary>
        /// Initialize a new CIM Line
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Line(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Line");

            if (ElementSource.HasAttribute("Contingencies"))
            {
                String[] splStr = ElementSource.Attributes["Contingencies"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string str in splStr)
                {
                    MM_Contingency con = null;
                    MM_Repository.Contingencies.TryGetValue(str, out con);
                    if (con != null && !Contingencies.Any(c => con.Name == c.Name))
                        this.Contingencies.Add(con);
                }
            }
        }

        private void CheckElementValue(XmlElement ElementSource, string AttributeName, ref float[] OutValues)
        {
            if (!ElementSource.HasAttribute(AttributeName))
                return;
            String[] splStr = ElementSource.Attributes[AttributeName].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int a = 0; a < OutValues.Length; a++)
                if (splStr[a] == "NaN")
                    OutValues[a] = float.NaN;
                else
                    OutValues[a] = MM_Converter.ToSingle(splStr[a]);
        }
        #endregion

        /// <summary>
        /// Determine the midpoint of the line
        /// </summary>
        public PointF Midpoint
        {
            get
            {
                return new PointF(
                    (ConnectedStations[0].LngLat.X + ConnectedStations[1].LngLat.X) / 2f,
                    (ConnectedStations[0].LngLat.Y + ConnectedStations[1].LngLat.Y) / 2f);
            }
        }

        /// <summary>
        /// Determine the appropriate display parameter for the line
        /// </summary>
        public MM_DisplayParameter LineDisplay(Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations, Object CallingObject, bool Blink)
        {
            if (!Blink || ShownViolations == null)
                return LineEnergizationState;

            MM_AlarmViolation_Type WorstViol = this.WorstVisibleViolation(ShownViolations, CallingObject);
            if (WorstViol == null)
                return LineEnergizationState;
            
            return WorstViol;
        }

        /// <summary>
        /// Produce a description for this line, based on the core station (e.g., Line X to [other station])
        /// </summary>
        /// <param name="CoreStation"></param>
        /// <returns></returns>
        public string MenuDescription(MM_Substation CoreStation)
        {
            MM_Substation MVAFlowDir = MVAFlowDirection;
            if (IsSeriesCompensator)
                return "SeriesCompensator " + this.Name + " (" + this.LinePercentageText + ")";
            else if (MVAFlowDir == null)
                return this.ElemType.Name + " " + this.Name + " with " + (this.Substation1 == CoreStation ? Substation2.LongName : Substation1.LongName) + " (" + this.LinePercentageText + ")";
            else
                return this.ElemType.Name + " " + this.Name + (MVAFlowDir == CoreStation ? " from " : " to ") + (this.Substation1 == CoreStation ? Substation2.LongName : Substation1.LongName) + " (" + this.LinePercentageText + ")";
        }

        /// <summary>
        /// Return the center of the line
        /// </summary>
        public PointF CenterLngLat
        {
            get
            {
                return new PointF(
                    (ConnectedStations[0].LngLat.X + ConnectedStations[1].LngLat.X) / 2f,
                    (ConnectedStations[0].LngLat.Y + ConnectedStations[1].LngLat.Y) / 2f);
            }
        }

        /// <summary>
        /// Dynamically calculate MVA from MW and MVAR
        /// </summary>
        /// <param name="MW"></param>
        /// <param name="MVAR"></param>
        /// <returns></returns>
        private float CalculateMVA(float MW, float MVAR)
        {
            return (float)Math.Sign(MW) * (float)Math.Sqrt((MW * MW) + (MVAR * MVAR));
        }

        /// <summary>
        /// Return the energization state of the line
        /// </summary>
        public MM_DisplayParameter LineEnergizationState
        {
            get
            {
                float EnergizationThreshold = MM_Repository.OverallDisplay.EnergizationThreshold;
                float[] MVA;
                if (this is MM_Tie)
                {
                    MM_Tie Tie = (this as MM_Tie);
                    if (!float.IsNaN(Tie.MW_Integrated) && Tie.MW_Integrated != 0f)
                        MVA = new float[] { Tie.MW_Integrated, Tie.MW_Integrated };
                    else
                        MVA = this.Estimated_MVA;
                }
                else if (Integration.Data_Integration.NetworkSource.Estimates && Integration.Data_Integration.UseEstimates) 
                    MVA = this.Estimated_MVA;
                else
                {
                    MVA = this.Telemetered_MVA;

                    //Perform a quick check to make sure we have telemetered MVAs if requested
                    //move this functionality to the server side to reduce client computation delay - mn
                    for (int a = 0; a < 2; a++)
                        if (float.IsNaN(MVA[a]) && !float.IsNaN(this.Telemetered_MW[a]) && !float.IsNaN(this.Telemetered_MVAR[a]))
                            MVA[a] = CalculateMVA(this.Telemetered_MW[a], this.Telemetered_MVAR[a]);

                    // Use estimated if we don't have telemetered
                    if ((MVA[0] == 0f || float.IsNaN(MVA[0])) && !float.IsNaN(Estimated_MVA[0]))
                        MVA[0] = Estimated_MVA[0];
                    if ((MVA[1] == 0f || float.IsNaN(MVA[1])) && !float.IsNaN(Estimated_MVA[1]))
                        MVA[1] = Estimated_MVA[1];
                }


                if (float.IsNaN(MVA[0]) && float.IsNaN(MVA[1]))
                    return KVLevel.Unknown;
                else if ((float.IsNaN(MVA[0]) || Math.Abs(MVA[0]) > EnergizationThreshold) && 
                        ((float.IsNaN(MVA[1]) || Math.Abs(MVA[1]) > EnergizationThreshold)))
                    return KVLevel.Energized;
                else if (Math.Abs(MVA[0]) <= EnergizationThreshold && (Math.Abs(MVA[1]) <= EnergizationThreshold))
                    return KVLevel.DeEnergized;
                else
                    return KVLevel.PartiallyEnergized;
            }
        }

        /// <summary>
        /// Return the percentage this line is loaded
        /// </summary>
        /// <param name="MVA1">The first MVA terminal reading</param>
        /// <param name="MVA2">The second MVA terminal reading</param>
        /// <returns></returns>
        public string LinePercentageTextFromValues(float MVA1, float MVA2)
        {
            float CurMVA = Math.Max(Math.Abs(MVA1), Math.Abs(MVA2));
            if (Limits[0] == 0)
                return "?";
            else if (CurMVA <= Limits[0])
                return (CurMVA / Limits[0]).ToString("0%") + " Norm";
            else 
                return (CurMVA / Limits[1]).ToString("0%") + " Emer";
        }

        /// <summary>
        /// Return the percentage this line is loaded
        /// </summary>
        public String LinePercentageText
        {
            get
            {
                float CurMVA = this.MVAFlow;
                //float[] MVA = (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates ? this.Estimated_MVA : this.Telemetered_MVA);                
                //float CurMVA = Math.Max(Math.Abs(MVA[0]), Math.Abs(MVA[1]));
                if (Limits[0] == 0 || float.IsNaN(CurMVA))
                    return "?%";
                else if (CurMVA <= Limits[0])
                    return (CurMVA / Limits[0]).ToString("0%") + " Norm";
                else 
                    return (CurMVA / Limits[1]).ToString("0%") + " Emer";
            }
        }

        /// <summary>
        /// Report the MVA flows of each of the lines
        /// </summary>
        /// <param name="Lines"></param>
        /// <returns></returns>
        public static String MultiLinePercentageText(IEnumerable<MM_Element> Lines)
        {
            float CurMVA = 0;
            float[] Limits = new float[] { 0f, 0f, 0f };
            foreach (MM_Line Line in Lines)
            {
                CurMVA += Line.MVAFlow;
                Limits[0] += Line.Limits[0];
                Limits[1] += Line.Limits[1];
                Limits[2] += Line.Limits[2];
            }
            if (Limits[0] == 0 || float.IsNaN(CurMVA))
                return "?%";
            else if (CurMVA <= Limits[0])
                return (CurMVA / Limits[0]).ToString("0%") + " Norm";
            else 
                return (CurMVA / Limits[1]).ToString("0%") + " Emer";
        }

        /// <summary>
        /// Returns the percentage loading of a line's MVA against its normal limit
        /// </summary>
        public float LinePercentage
        {
            get
            {
                
                if (this is MM_Tie && (((MM_Tie)this).AssociatedLine != null))
                {
                    return ((MM_Tie)this).AssociatedLine.LinePercentage;

                    /*MM_Tie Tie = this as MM_Tie;
                    if (Tie.MW_Integrated > 0f)
                        return Tie.MW_Integrated / this.Limits[1];
                    else
                        return -Tie.MW_Integrated / this.Limits[0];*/
                } 
                else
                {
                    if (Limits[0] == 0)
                        return 0;
                    float[] MVA = Telemetered_MVA;
                    if (Data_Integration.NetworkSource != null)
                        MVA = (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates ? this.Estimated_MVA : this.Telemetered_MVA);
                    if (!Data_Integration.UseEstimates)
                    {
                        // use the other side if we only have one end telemetered
                        if ((MVA[0] == 0f || float.IsNaN(MVA[0])) && !float.IsNaN(MVA[1]))
                            MVA[0] = -MVA[1];
                        if ((MVA[1] == 0f || float.IsNaN(MVA[1])) && !float.IsNaN(MVA[0]))
                            MVA[1] = -MVA[0];

                        // Use estimated if we don't have telemetered
                        if ((MVA[0] == 0f || float.IsNaN(MVA[0])) && !float.IsNaN(Estimated_MVA[0]) || Math.Abs(Estimated_MVA[0] - MVA[0]) > Math.Abs(Estimated_MVA[0]) * .4)
                            MVA[0] = Estimated_MVA[0];
                        if ((MVA[1] == 0f || float.IsNaN(MVA[1])) && !float.IsNaN(Estimated_MVA[1]) || Math.Abs(Estimated_MVA[1] - MVA[1]) > Math.Abs(Estimated_MVA[1]) * .4)
                            MVA[1] = Estimated_MVA[1];
                    }
                    if (float.IsNaN(MVA[0]) && !float.IsNaN(MVA[1]))
                        return Math.Abs(MVA[1]) / Limits[0];
                    else if (float.IsNaN(MVA[1]) && !float.IsNaN(MVA[0]))
                        return Math.Abs(MVA[0]) / Limits[0];
                    else
                        return (Math.Max(Math.Abs(MVA[0]), Math.Abs(MVA[1])) / Limits[0]);
                }
            }

        }

        /// <summary>
        /// Return the substation to which the MVA flow is heading
        /// </summary>
        public virtual MM_Substation MVAFlowDirection
        {
            get
            {
                float[] MVA = (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates ? this.Estimated_MVA : this.Telemetered_MVA);

                if (!Data_Integration.UseEstimates)
                {
                    // use the other side if we only have one end telemetered
                    if ((MVA[0] == 0f || float.IsNaN(MVA[0])) && !float.IsNaN(MVA[1]))
                        MVA[0] = -MVA[1];
                    if ((MVA[1] == 0f || float.IsNaN(MVA[1])) && !float.IsNaN(MVA[0]))
                        MVA[1] = -MVA[0];

                    // Use estimated if we don't have telemetered
                    if ((MVA[0] == 0f || float.IsNaN(MVA[0]) || Math.Abs(Estimated_MVA[0] - MVA[0]) > Math.Abs(Estimated_MVA[0]) * .4) && !float.IsNaN(Estimated_MVA[0]))
                        MVA[0] = Estimated_MVA[0];
                    if ((MVA[1] == 0f || float.IsNaN(MVA[1]) || Math.Abs(Estimated_MVA[1] - MVA[1]) > Math.Abs(Estimated_MVA[1]) * .4) && !float.IsNaN(Estimated_MVA[1]))
                        MVA[1] = Estimated_MVA[1];
                }


                if (float.IsNaN(MVA[0]) && float.IsNaN(MVA[1]))
                    return null;
                else if (float.IsNaN(MVA[0]))
                    if (MVA[1] > 0)
                        return ConnectedStations[0];
                    else
                        return ConnectedStations[1];
                else if (float.IsNaN(MVA[1]))
                    if (MVA[0] > 0)
                        return ConnectedStations[1];
                    else
                        return ConnectedStations[0];
                else if (MVA[0] > 0 || MVA[1] < 0)
                    return ConnectedStations[1];
                else
                    return ConnectedStations[0];
            }
        }

        /// <summary>
        /// Return the substation to which the MVAR flow is heading
        /// </summary>
        public virtual MM_Substation MVARFlowDirection
        {
            get
            {
                float[] MVAR = (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates ? this.Estimated_MVAR : this.Telemetered_MVAR);

                if (!Data_Integration.UseEstimates)
                {
                    // use the other side if we only have one end telemetered
                    if ((MVAR[0] == 0f || float.IsNaN(MVAR[0])) && !float.IsNaN(MVAR[1]))
                        MVAR[0] = -MVAR[1];
                    if ((MVAR[1] == 0f || float.IsNaN(MVAR[1])) && !float.IsNaN(MVAR[0]))
                        MVAR[1] = -MVAR[0];

                    // Use estimated if we don't have telemetered
                    if ((MVAR[0] == 0f || float.IsNaN(MVAR[0])) && !float.IsNaN(Estimated_MVAR[0]) || Math.Abs(Estimated_MVAR[0] - MVAR[0]) > Math.Abs(Estimated_MVAR[0]) * .4)
                        MVAR[0] = Estimated_MVAR[0];
                    if ((MVAR[1] == 0f || float.IsNaN(MVAR[1])) && !float.IsNaN(Estimated_MVAR[1]) || Math.Abs(Estimated_MVAR[1] - MVAR[1]) > Math.Abs(Estimated_MVAR[1]) * .4)
                        MVAR[1] = Estimated_MVAR[1];
                }


                if (float.IsNaN(MVAR[0]) && float.IsNaN(MVAR[1]))
                    return null;
                else if (float.IsNaN(MVAR[0]) && !float.IsNaN(MVAR[1]))
                    if (MVAR[1] > 0)
                        return ConnectedStations[0];
                    else
                        return ConnectedStations[1];
                else if (float.IsNaN(MVAR[1]) && !float.IsNaN(MVAR[0]))
                    if (MVAR[0] > 0)
                        return ConnectedStations[1];
                    else
                        return ConnectedStations[0];
                else if (MVAR[0] > 0 || MVAR[1] < 0)
                    return ConnectedStations[1];
                else
                    return ConnectedStations[0];
            }
        }

        /// <summary>
        /// Return the substation to which the flow is heading
        /// </summary>
        public virtual MM_Substation MWFlowDirection
        {
            get
            {
                float[] MW = (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates ? this.Estimated_MW : this.Telemetered_MW);

                if (!Data_Integration.UseEstimates)
                {
                    // use the other side if we only have one end telemetered
                    if ((MW[0] == 0f || float.IsNaN(MW[0])) && !float.IsNaN(MW[1]))
                        MW[0] = -MW[1];
                    if ((MW[1] == 0f || float.IsNaN(MW[1])) && !float.IsNaN(MW[0]))
                        MW[1] = -MW[0];

                    // Use estimated if we don't have telemetered
                    if ((MW[0] == 0f || float.IsNaN(MW[0])) && !float.IsNaN(Estimated_MW[0]) || Math.Abs(Estimated_MW[0] - MW[0]) > Math.Abs(Estimated_MW[0]) * .4)
                        MW[0] = Estimated_MW[0];
                    if ((MW[1] == 0f || float.IsNaN(MW[1])) && !float.IsNaN(Estimated_MW[1]) || Math.Abs(Estimated_MW[1] - MW[1]) > Math.Abs(Estimated_MW[1]) * .4)
                        MW[1] = Estimated_MW[1];
                }


                if (float.IsNaN(MW[0]) && float.IsNaN(MW[1]))
                    return null;
                else if (float.IsNaN(MW[0]) && !float.IsNaN(MW[1]))
                    if (MW[1] > 0)
                        return ConnectedStations[0];
                    else
                        return ConnectedStations[1];
                else if (float.IsNaN(MW[1]) && !float.IsNaN(MW[0]))
                    if (MW[0] > 0)
                        return ConnectedStations[1];
                    else
                        return ConnectedStations[0];
                else if (MW[0] > 0 || MW[1] < 0)
                    return ConnectedStations[1];
                else
                    return ConnectedStations[0];
            }
        }
        /// <summary>
        /// Return the largest MW flow (in absolute values)
        /// </summary>
        public float MWFlow
        {
            get
            {
                float[] MW = (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates ? this.Estimated_MW : this.Telemetered_MW);

                if (!Data_Integration.UseEstimates)
                {
                    // use the other side if we only have one end telemetered
                    if ((MW[0] == 0f || float.IsNaN(MW[0])) && !float.IsNaN(MW[1]))
                        MW[0] = -MW[1];
                    if ((MW[1] == 0f || float.IsNaN(MW[1])) && !float.IsNaN(MW[0]))
                        MW[1] = -MW[0];

                    // Use estimated if we don't have telemetered
                    if ((MW[0] == 0f || float.IsNaN(MW[0]) || Math.Abs(Estimated_MW[0] - MW[0]) > Math.Abs(Estimated_MW[0]) * .4) && !float.IsNaN(Estimated_MW[0]))
                        MW[0] = Estimated_MW[0];
                    if ((MW[1] == 0f || float.IsNaN(MW[1]) || Math.Abs(Estimated_MW[1] - MW[1]) > Math.Abs(Estimated_MW[1]) * .4) && !float.IsNaN(Estimated_MW[1]))
                        MW[1] = Estimated_MW[1];
                }

                return Math.Max(Math.Abs(MW[0]), Math.Abs(MW[1]));
            }
        }

        /// <summary>
        /// Return the largest MVA flow (in absolute values)
        /// </summary>
        public float MVAFlow
        {
            get
            {
                float[] MVA = (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates ? this.Estimated_MVA : this.Telemetered_MVA);

                if (!Data_Integration.UseEstimates)
                {
                    // use the other side if we only have one end telemetered
                    if ((MVA[0] == 0f || float.IsNaN(MVA[0])) && !float.IsNaN(MVA[1]))
                        MVA[0] = -MVA[1];
                    if ((MVA[1] == 0f || float.IsNaN(MVA[1])) && !float.IsNaN(MVA[0]))
                        MVA[1] = -MVA[0];

                    // Use estimated if we don't have telemetered
                    if ((MVA[0] == 0f || float.IsNaN(MVA[0]) || Math.Abs(Estimated_MVA[0] - MVA[0]) > Math.Abs(Estimated_MVA[0]) * .4) && !float.IsNaN(Estimated_MVA[0]))
                        MVA[0] = Estimated_MVA[0];
                    if ((MVA[1] == 0f || float.IsNaN(MVA[1]) || Math.Abs(Estimated_MVA[1] - MVA[1]) > Math.Abs(Estimated_MVA[1]) * .4) && !float.IsNaN(Estimated_MVA[1]))
                        MVA[1] = Estimated_MVA[1];
                }

                float OutVal;
                if (float.IsNaN(MVA[0]))
                    OutVal = MVA[1];
                else if (float.IsNaN(MVA[1]))
                    OutVal = MVA[0];
                else
                    OutVal = Math.Max(Math.Abs(MVA[0]), Math.Abs(MVA[1]));

                //If we're looking for Post-Ctg values, look accordingly
                if (Data_Integration.ShowPostCtgValues && this.Violations != null)
                    foreach (MM_AlarmViolation Viol in this.Violations.Values)
                        if (!float.IsNaN(Viol.PostCtgValue) && (float.IsNaN(OutVal) || Viol.PostCtgValue > OutVal))
                            OutVal = Viol.PostCtgValue;
                return OutVal;
            }
        }

        /// <summary>
        /// Return the largest MVAR flow (in absolute values)
        /// </summary>
        public float MVARFlow
        {
            get
            {
                float[] MVAR = (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates ? this.Estimated_MVA : this.Telemetered_MVA);

                if (!Data_Integration.UseEstimates)
                {
                    // use the other side if we only have one end telemetered
                    if ((MVAR[0] == 0f || float.IsNaN(MVAR[0])) && !float.IsNaN(MVAR[1]))
                        MVAR[0] = -MVAR[1];
                    if ((MVAR[1] == 0f || float.IsNaN(MVAR[1])) && !float.IsNaN(MVAR[0]))
                        MVAR[1] = -MVAR[0];

                    // Use estimated if we don't have telemetered
                    if ((MVAR[0] == 0f || float.IsNaN(MVAR[0]) || Math.Abs(Estimated_MVAR[0] - MVAR[0]) > Math.Abs(Estimated_MVAR[0]) * .4) && !float.IsNaN(Estimated_MVAR[0]))
                        MVAR[0] = Estimated_MVAR[0];
                    if ((MVAR[1] == 0f || float.IsNaN(MVAR[1]) || Math.Abs(Estimated_MVAR[1] - MVAR[1]) > Math.Abs(Estimated_MVAR[1]) * .4) && !float.IsNaN(Estimated_MVAR[1]))
                        MVAR[1] = Estimated_MVAR[1];
                }

                return Math.Max(Math.Abs(MVAR[0]), Math.Abs(MVAR[1]));
            }
        }



        /// <summary>
        /// Determine whether a line is visible on the screen
        /// </summary>
        /// <param name="Coord">The coordinates to check against</param>
        /// <returns></returns>
        public bool CheckVisibility(RectangleF Coord, int zoomLevel)
        {
            if ((Substation1.LngLat.X < Coord.Left) && (Substation2.LngLat.X < Coord.Left))
                return false;
            else if ((Substation1.LngLat.Y > Coord.Top) && (Substation2.LngLat.Y > Coord.Top))
                return false;
            else if ((Substation1.LngLat.X > Coord.Right) && (Substation2.LngLat.X > Coord.Right))
                return false;
            else if ((Substation1.LngLat.Y < Coord.Bottom) && (Substation2.LngLat.Y < Coord.Bottom))
                return false;
            return zoomLevel >= KVLevel.VisibilityByZoom;
        }

        /// <summary>
        /// Rebuild our line text
        /// </summary>
        /// <param name="Line">The line for which text should be built</param>
        public string GetLineText()
        {
    
            //Build our text for the line
            StringBuilder OutLine = new StringBuilder();

            if (KVLevel.ShowLineName)
                OutLine.Append(" " + Name);

            if (KVLevel.ShowPercentageText)
                if (MVAFlowDirection == null)
                    OutLine.Append(" " + LinePercentageText);
                else if (MVAFlowDirection == ConnectedStations[0] ^ !FlipSides)
                    OutLine.Append(" ◄ " + LinePercentageText);
                else
                    OutLine.Append(" ► " + LinePercentageText);

            if (KVLevel.ShowMVAText)
                if (MVAFlowDirection == null)
                    if (float.IsNaN(MVAFlow))
                        OutLine.Append(" ? mva");
                    else
                        OutLine.Append(" " + MVAFlow.ToString("0") + " mva");
                else if (MVAFlowDirection == ConnectedStations[0] ^ !FlipSides)
                    OutLine.Append(" ◄ " + MVAFlow.ToString("0") + " mva");
                else
                    OutLine.Append(" ► " + MVAFlow.ToString("0") + " mva");

            if (KVLevel.ShowMWText)
                if (MWFlowDirection == null)
                    if (float.IsNaN(MWFlow))
                        OutLine.Append(" ? mw");
                    else
                        OutLine.Append(" " + MWFlow.ToString("0") + " mw");
                else if (MWFlowDirection == ConnectedStations[0] ^ !FlipSides)
                    OutLine.Append(" ◄ " + MWFlow.ToString("0") + " mw");
                else
                    OutLine.Append(" ► " + MWFlow.ToString("0") + " mw");


            if (KVLevel.ShowMVARText)
                if (MVARFlowDirection == null)
                    if (float.IsNaN(MVARFlow))
                        OutLine.Append(" ? mw");
                    else
                        OutLine.Append(" ◄ ► " + MVARFlow.ToString("0") + " mvar");
                else if (MVARFlowDirection == ConnectedStations[0] ^ !FlipSides)
                    OutLine.Append(" ◄ " + MVARFlow.ToString("0") + " mvar");
                else
                    OutLine.Append(" ► " + MVARFlow.ToString("0") + " mvar");


            //Make sure we have text, and that it's valid
            if (OutLine.Length == 0)
                return string.Empty;

            return OutLine.ToString(1, OutLine.Length - 1);
        }

        /// <summary>
        /// This class sorts lines based on blackstart corridor membership, voltage
        /// </summary>
        public class LineSorter : IComparer<MM_Line>
        {

            /// <summary>
            /// Compare two transmission lines
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(MM_Line x, MM_Line y)
            {
                if (x.TEID == y.TEID)
                    return 0;

                int FirstCompare = 0;
                if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonBlackstartElements)
                    FirstCompare = x.IsBlackstart.CompareTo(y.IsBlackstart);
                else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonOperatorElements)
                    if (x.Operator.TEID == MM_Repository.OverallDisplay.DisplayCompany.TEID && y.Operator.TEID != MM_Repository.OverallDisplay.DisplayCompany.TEID)
                        return 1;
                    else if (x.Operator.TEID != MM_Repository.OverallDisplay.DisplayCompany.TEID && y.Operator.TEID == MM_Repository.OverallDisplay.DisplayCompany.TEID)
                        return -1;
                    else
                        FirstCompare = x.Operator.TEID.CompareTo(y.Operator.TEID);

                //   int FirstCompare = x.IsBlackstart.CompareTo(y.IsBlackstart);
                if (FirstCompare != 0)
                    return FirstCompare;

                //Otherwise, check energization status
                float xMVAFlow = x.MVAFlow, yMVAFlow = y.MVAFlow;
                if (xMVAFlow != 0 && yMVAFlow == 0)
                    return 1;
                else if (xMVAFlow == 0 && yMVAFlow != 0)
                    return -1;

                //Otherwise, sort by nominal voltages, then TEIDs.
                FirstCompare = x.KVLevel.Nominal.CompareTo(y.KVLevel.Nominal);
                if (FirstCompare != 0)
                    return FirstCompare;
                else
                    return x.TEID.CompareTo(y.TEID);
            }





        }

    }
}
