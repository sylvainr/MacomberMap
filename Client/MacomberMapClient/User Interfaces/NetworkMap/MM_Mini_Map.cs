using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.User_Interfaces.Violations;
using System.Drawing.Drawing2D;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Menuing;
using MacomberMapClient.User_Interfaces.Summary;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Data_Elements.Display;
using System.Drawing.Imaging;
using MacomberMapClient.User_Interfaces.Generic;

namespace MacomberMapClient.User_Interfaces.NetworkMap
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// A small network map that can be zoomed and panned, show violations and zoom regions, and key indicators
    /// </summary>
    public partial class MM_Mini_Map : UserControl
    {
        #region Variable declarations
        /// <summary>The bounds of the state</summary>
        public MM_Boundary StateBoundary;

        /// <summary>The network map associated with the mini-map (if any)</summary>
        public MM_Network_Map_DX networkMap;

        /// <summary>The violation viewer associated within the mini-map</summary>
        public MM_Violation_Viewer Violations;

        /// <summary>Display selected violations on the map?</summary>
        public bool DisplayViolations = true;

        /// <summary>Show elements within the zoom region?</summary>
        public bool DisplayElementsWithinZoomRegion = false;

        /// <summary>Show the white box signifying the map's position?</summary>
        public bool DisplayMapPosition = true;

        /// <summary>Show flow in to the selected region (flow in from DCTies if the overall map)</summary>
        public bool ShowFlowIn = true;

        /// <summary>Show flow out to the selected region (flow out to DCTies if the overall map</summary>
        public bool ShowFlowOut = true;

        /// <summary>Show generation within the selected region (total gen if the overall map)</summary>
        public bool ShowGen = true;

        /// <summary>Show load wihtin the selected region (total load if the overall map</summary>
        public bool ShowLoad = true;

        /// <summary>Whether the total MVA should be shown, rather than total MW.</summary>
        public bool ShowMVA = false;

        /// <summary>Whether the capacitors should be shown</summary>
        public bool ShowCaps = true;

        /// <summary>Whether the operator should be shown emergency information (E.g., capacity = hsl-gen as opposed to hasl-gen).</summary>
        public bool ShowEmergencyMode = false;

        /// <summary>Only show online generation</summary>
        public bool ShowOnlineGenerationOnly = true;

        /// <summary>Whether the reactors should be shown</summary>
        public bool ShowReactors = true;

        /// <summary>The coordinates used to signify position (set to network map's if master, or the selected region otherwise)</summary>
        public MM_Coordinates Coord;

        /// <summary>The collection of elements passed to the mini-map, when the result of a one-line or lasso</summary>
        public Dictionary<MM_Element, bool> HighlightedElements;

        /// <summary>The collection of lines on the boundary of the mini-map</summary>
        public Dictionary<MM_Line, MM_Substation> BorderingLines;

        /// <summary>The base data for the display</summary>
        public MM_DataSet_Base BaseData;

        /// <summary>Whether the user can zoom and pan the mini-map</summary>
        public bool UserCanZoomPan = true;

        /// <summary>The menu strip for handling right-click menus</summary>
        private MM_Popup_Menu MiniMapMenu = new MM_Popup_Menu();

        /// <summary>The points to hold zoom/pan instructions</summary>
        private Queue<KeyValuePair<PointF, int>> ZoomPanPoints = new Queue<KeyValuePair<PointF, int>>();

        /// <summary>The timer to handle moving events</summary>
        private System.Timers.Timer ZoomPanTimer = new System.Timers.Timer(100);

        /// <summary>A bitmap containing the blank state map</summary>
        private Bitmap BlankStateMap;

        /// <summary>Whether or not the mini-map is associated with a network map</summary>
        public bool isNetworkMap;

        /// <summary>The distance in miles from one side of the mini-map to the other</summary>
        public float MileDistance = float.NaN;

        /// <summary>The top-left and bottom-right points, when displaying a lasso</summary>
        public PointF TopLeft = PointF.Empty, BottomRight = PointF.Empty;

        /// <summary>Whether to display a regional view of the map</summary>
        public bool ShowRegionalView = false;

        /// <summary>The graphics paths for regional elements shown on the display</summary>
        public Dictionary<GraphicsPath, MM_Element> RegionalElements;

        /// <summary>Our tool tip for displaying relevant information to the user</summary>
        private ToolTip tTip;

        /// <summary>A timer to handle refreshing the mini-map every 4 seconds</summary>
        private Timer RefreshTimer;

        /// <summary>The font for drawing text on the mini-map</summary>
        private Font TextFont = null;

        /// <summary>The font for drawing substation names on the mini-map</summary>
        private Font StationFont = null;

        /// <summary>A pen for hit-testing elements</summary>
        private Pen TestPen = new Pen(Color.Brown, 2f);

        /// <summary>A brush for slightly altering the background</summary>
        private SolidBrush BackBrush = null;

        /// <summary>The island to be displayed, if any</summary>
        public MM_Island Island = null;
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize the mini-map
        /// </summary>
        public MM_Mini_Map(bool IsNetworkMap)
        {
            InitializeComponent();
            this.Paint += new PaintEventHandler(HandleRepaint);
            this.DoubleBuffered = true;
            this.ZoomPanTimer.Elapsed += new System.Timers.ElapsedEventHandler(ZoomPanTimer_Elapsed);
            this.isNetworkMap = IsNetworkMap;
            this.ShowRegionalView = !isNetworkMap;
        }

        /// <summary>
        /// Initialize the mini-map (as a network map)
        /// </summary>
        public MM_Mini_Map()
        {
            InitializeComponent();
            this.Paint += new PaintEventHandler(HandleRepaint);
            this.DoubleBuffered = true;
            this.ZoomPanTimer.Elapsed += new System.Timers.ElapsedEventHandler(ZoomPanTimer_Elapsed);
            this.isNetworkMap = true;
            this.ShowRegionalView = false;
        }

        /// <summary>
        /// Handle the form's resizing
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            TextFont = new Font("Arial", (float)DisplayRectangle.Width / 21f);
            StationFont = new Font("Arial", DisplayRectangle.Width / 28f);
            BackBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
            base.OnResize(e);
        }


        /// <summary>
        /// Set controls for the mini-map, including all lines on the border (around the lassoed area, or connected to the substation), and
        /// all units and loads within
        /// </summary>
        /// <param name="networkMap">The associated network map</param>
        /// <param name="Viol">The violations viewer, showing currently-displayed violations</param>        
        /// <param name="BaseData">The base data for the display</param>
        /// <param name="BorderingLines">The lines on the border of the region, and loads/units within.</param>
        /// <param name="TopLeft">The top-left point of the lasso</param>
        /// <param name="BottomRight">The bottom-right point on the lasso</param>
        public void SetControls(MM_Network_Map_DX networkMap, MM_Violation_Viewer Viol, MM_DataSet_Base BaseData, MM_Line[] BorderingLines, PointF TopLeft, PointF BottomRight)
        {
            this.BorderingLines = new Dictionary<MM_Line, MM_Substation>();
            foreach (MM_Line ThisLine in BorderingLines)
                if (!this.BorderingLines.ContainsKey(ThisLine))
                {
                    try
                    {
                        if (BaseData.Data.Tables.Contains("Substation") && BaseData.Data.Tables["Substation"].Rows.Find(ThisLine.ConnectedStations[0]) != null)
                            this.BorderingLines.Add(ThisLine, ThisLine.ConnectedStations[0]);
                        else
                            this.BorderingLines.Add(ThisLine, ThisLine.ConnectedStations[1]);
                    } catch (Exception ex)
                    {
                    }

                }
            this.BaseData = BaseData;

            int TotalHighlights = 0;
            if (BaseData.Data.Tables.Contains("Substation"))
                TotalHighlights += BaseData.Data.Tables["Substation"].Rows.Count;
            if (BaseData.Data.Tables.Contains("Line"))
                TotalHighlights += BaseData.Data.Tables["Line"].Rows.Count;
            HighlightedElements = new Dictionary<MM_Element, bool>(TotalHighlights);

            if (BaseData.Data.Tables.Contains("Substation"))
                foreach (DataRow dr in BaseData.Data.Tables["Substation"].Rows)
                    HighlightedElements.Add(dr["TEID"] as MM_Element, true);
            if (BaseData.Data.Tables.Contains("Line"))
                foreach (DataRow dr in BaseData.Data.Tables["Line"].Rows)
                    if (!HighlightedElements.ContainsKey(dr["TEID"] as MM_Element))
                        HighlightedElements.Add(dr["TEID"] as MM_Element, true);

            //Determine the width of the mini-map, in miles.
            this.TopLeft = TopLeft;
            this.BottomRight = BottomRight;

            //If we have a substation, let's determine the maximum distance between it and its other substations
            if (BaseData.BaseElement is MM_Substation)
            {
                foreach (MM_Line Line in MM_Repository.Lines.Values)
                    if (Line.ConnectedStations[0] == BaseData.BaseElement)
                    {
                        float ThisDistance = Line.ConnectedStations[1].DistanceTo(BaseData.BaseElement as MM_Substation);
                        if (float.IsNaN(MileDistance) || (MileDistance < ThisDistance))
                            MileDistance = ThisDistance;
                    }
                    else if (Line.ConnectedStations[1] == BaseData.BaseElement)
                    {
                        float ThisDistance = Line.ConnectedStations[0].DistanceTo(BaseData.BaseElement as MM_Substation);
                        if (float.IsNaN(MileDistance) || (MileDistance < ThisDistance))
                            MileDistance = ThisDistance;
                    }
            }
            else if (BaseData.Data.Tables.Contains("Substation"))
                foreach (DataRow dr in BaseData.Data.Tables["Substation"].Rows)
                    foreach (DataRow dr2 in BaseData.Data.Tables["Substation"].Rows)
                        if (dr != dr2)
                        {
                            float ThisDistance = (dr["TEID"] as MM_Substation).DistanceTo(dr2["TEID"] as MM_Substation);
                            if (float.IsNaN(MileDistance) || (MileDistance < ThisDistance))
                                MileDistance = ThisDistance;
                        }
            if (!float.IsNaN(MileDistance))
                MileDistance *= 1.2f;

            //Prepare to keep our hit testing for regional elements
            int NumElements = 0;
            if (BaseData.Data.Tables.Contains("Substation"))
                NumElements += BaseData.Data.Tables["Substation"].Rows.Count;
            if (BaseData.Data.Tables.Contains("Line"))
                NumElements += BaseData.Data.Tables["Line"].Rows.Count;
            this.RegionalElements = new Dictionary<GraphicsPath, MM_Element>(NumElements);


            //Send off to the next control
            SetControls(networkMap, Viol);
        }




        /// <summary>
        /// Set the controls and data sources associated with the violation viewer
        /// </summary>
        /// <param name="networkMap">The associated network map</param>
        /// <param name="Viol">The violations viewer, showing currently-displayed violations</param>
        public void SetControls(MM_Network_Map_DX networkMap, MM_Violation_Viewer Viol)
        {
            //Assign our internal variables
            this.tTip = new ToolTip();
            this.tTip.AutoPopDelay = 5000;
            this.tTip.InitialDelay = 250;
            this.tTip.ReshowDelay = 50;
            this.tTip.ShowAlways = true;
            this.MiniMapMenu.ImageList = MM_Repository.ViolationImages;

            this.networkMap = networkMap;
            if (MM_Repository.Counties.ContainsKey("STATE"))
                this.StateBoundary = MM_Repository.Counties["STATE"];
            this.Violations = Viol;
            this.BackColor = Viol.BackColor;
            if (networkMap != null)
            {
                this.Coord = networkMap.Coordinates;
                this.networkMap.Coordinates.Panned += new MM_Coordinates.PanEvent(Coordinates_Panned);
                this.networkMap.Coordinates.Zoomed += new MM_Coordinates.ZoomEvent(Coordinates_Zoomed);
                this.networkMap.Coordinates.LassoDrawing += new MM_Coordinates.LassoDrawingEvent(Coordinates_LassoDrawing);
            }
            HandleResize(this, null);
            MM_Repository.ViewChanged += new MM_Repository.ViewChangedDelegate(Repository_ViewChanged);
            if (RefreshTimer != null)
                RefreshTimer.Stop();
            RefreshTimer = new Timer();
            RefreshTimer.Interval = 4000;
            RefreshTimer.Tick += new EventHandler(RefreshTimer_Tick);
            RefreshTimer.Start();
        }

        /// <summary>
        /// Every 4 seconds, update the mini-map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (this.Visible)
                this.Invalidate();
        }

        /// <summary>
        /// Refresh our mini-map when the view changes
        /// </summary>
        /// <param name="ActiveView"></param>
        private void Repository_ViewChanged(MM_Display_View ActiveView)
        {
            this.Invalidate();
        }


        /// <summary>
        /// Handle a pan event by resetting the mini-map
        /// </summary>
        /// <param name="OldZoom"></param>
        /// <param name="NewZoom"></param>
        private void Coordinates_Zoomed(float OldZoom, float NewZoom)
        {
            this.Invalidate();
        }

        /// <summary>
        /// Handle a pan event by resetting the mini-map
        /// </summary>
        /// <param name="OldCenter"></param>
        /// <param name="NewCenter"></param>
        private void Coordinates_Panned(PointF OldCenter, PointF NewCenter)
        {
            this.Invalidate();
        }

        /// <summary>
        /// Handle the lasso drawing by resetting the mini-map
        /// </summary>
        private void Coordinates_LassoDrawing()
        {
            this.Invalidate();
        }

        #endregion

        /// <summary>
        /// Refresh the mini map content
        /// </summary>
        public void RefreshContent()
        {
            HandleResize(this, EventArgs.Empty);
        }

        /// <summary>
        /// Rebuild our bitmap of the state image on the resize event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleResize(object sender, EventArgs e)
        {
            try
            {
                //Create our new blank state map
                Bitmap NewBitmap = new Bitmap(DisplayRectangle.Width, DisplayRectangle.Height, PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(NewBitmap);

                //Determine the points that will make up the boundary
                List<PointF> NewPoint = new List<PointF>();
                if (StateBoundary == null) return;
                foreach (PointF LngLat in StateBoundary.Coordinates)
                    NewPoint.Add(ConvertPoint(LngLat));

                //Clear the bitmap, and draw the state
                g.Clear(this.BackColor);
                g.FillPolygon(Brushes.Black, NewPoint.ToArray());
                g.DrawLines(Pens.DarkGray, NewPoint.ToArray());

                //Now, swap the bitmaps, and handle disposal of the old one.
                g.Dispose();
                Bitmap ToDispose = this.BlankStateMap;
                this.BlankStateMap = NewBitmap;
                if (ToDispose != null)
                    ToDispose.Dispose();
                ToDispose = null;

                //Now refresh the mini-map
                this.Invalidate();
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// Convert a Latitude/Longitude to single-precision X/Y coordinate on the mini-map.
        /// If the map is in state view, use the state boundaries. If not, use the lasso boundaries
        /// </summary>
        /// <param name="LngLat">The Lat/Long to convert</param>
        /// <returns>The resulting point on the mini-map</returns>
        private PointF ConvertPoint(PointF LngLat)
        {
            if (!ShowRegionalView)
                return new PointF(
                    (float)DisplayRectangle.Width * (LngLat.X - StateBoundary.Min.X) / (StateBoundary.Max.X - StateBoundary.Min.X),
                    (float)DisplayRectangle.Height * (LngLat.Y - StateBoundary.Max.Y) / (StateBoundary.Min.Y - StateBoundary.Max.Y));
            else
                return new PointF(
                    (float)DisplayRectangle.Width * (LngLat.X - TopLeft.X) / (BottomRight.X - TopLeft.X),
                    (float)DisplayRectangle.Height * (LngLat.Y - BottomRight.Y) / (TopLeft.Y - BottomRight.Y));
        }

        /// <summary>
        /// Convert a Latitude/Longitude array to single-precision X/Y coordinates on the mini-map.
        /// If the map is in state view, use the state boundaries. If not, use the lasso boundaries
        /// </summary>
        /// <param name="InPoints">The Lat/Long to convert</param>
        /// <returns>The resulting point on the mini-map</returns>
        private PointF[] ConvertPoints(List<PointF> InPoints)
        {
            PointF[] OutPoints = new PointF[InPoints.Count];
            for (int a = 0; a < InPoints.Count; a++)
                OutPoints[a] = ConvertPoint(InPoints[a]);
            return OutPoints;
        }

        /// <summary>
        /// Convert two Latitude/Longitude points to a single-precision X/Y rectangle on the mini-map
        /// </summary>
        /// <param name="Start">The first point to convert</param>
        /// <param name="End">The second point to convert</param>
        /// <returns>The resulting point on the mini-map</returns>
        private Rectangle ConvertRectangle(PointF Start, PointF End)
        {
            Start = ConvertPoint(Start);
            End = ConvertPoint(End);
            return new Rectangle((int)(Start.X > End.X ? End.X : Start.X), (int)(Start.Y > End.Y ? End.Y : Start.Y), (int)Math.Abs(End.X - Start.X), (int)Math.Abs(End.Y - Start.Y));
        }

        /// <summary>
        /// Convert a single-precision X/Y coordinate on the mini-map to a Latitude/Longitude
        /// </summary>
        /// <param name="XY">The point on the mini-map</param>
        /// <returns>The resulting lat/long</returns>        
        private PointF UnconvertPoint(PointF XY)
        {
            return new PointF(
                ((XY.X / (float)DisplayRectangle.Width) * (StateBoundary.Max.X - StateBoundary.Min.X)) + StateBoundary.Min.X,
                ((XY.Y / (float)DisplayRectangle.Height) * (StateBoundary.Min.Y - StateBoundary.Max.Y)) + StateBoundary.Max.Y);

        }

        /// <summary>
        /// Refresh the mini-map
        /// </summary>
        private void RefreshMiniMap()
        {
            this.Invalidate();
        }


        /// <summary>
        /// Repaint the mini-map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleRepaint(object sender, PaintEventArgs e)
        {
            try
            {

                if ((!Data_Integration.InitializationComplete) || (BlankStateMap == null))
                    e.Graphics.DrawString("Control not\ninitialized", this.Font, Brushes.White, PointF.Empty);
                else
                {
                    e.Graphics.TextRenderingHint = MM_Repository.OverallDisplay.TextRenderingHint;
                    e.Graphics.TextContrast = MM_Repository.OverallDisplay.TextContrast;
                    e.Graphics.PixelOffsetMode = MM_Repository.OverallDisplay.PixelOffsetMode;
                    e.Graphics.SmoothingMode = MM_Repository.OverallDisplay.SmoothingMode;
                    e.Graphics.CompositingMode = MM_Repository.OverallDisplay.CompositingMode;
                    e.Graphics.CompositingQuality = MM_Repository.OverallDisplay.CompositingQuality;
                    e.Graphics.InterpolationMode = MM_Repository.OverallDisplay.InterpolationMode;

                    if (ShowRegionalView && (BaseData.BaseElement is MM_Substation))
                        //Draw our regional view, first by drawing our lines, then our substations                
                        DrawRegionalAroundSubstation(BaseData.BaseElement as MM_Substation, e.Graphics);
                    else
                    {
                        //Draw our state view, or regional view for lasso, depending on the user's selection
                        if (!ShowRegionalView)
                            e.Graphics.DrawImageUnscaled(BlankStateMap, 0, 0);

                        //Now, if we're displaying violations, let's show them all here
                        Dictionary<MM_AlarmViolation, MM_AlarmViolation> SelectedUIDs = Violations.SelectedUIDs;

                        //Now, display the crosshairs if requested
                        if (isNetworkMap && DisplayMapPosition && Coord != null)
                        {
                            DrawMapPosition(e.Graphics, Coord.TopLeft, Coord.BottomRight);
                            DrawLassoBorder(e.Graphics, Coord);
                        }
                        else if (!isNetworkMap && DisplayMapPosition)
                            if (!float.IsNaN(TopLeft.X))
                                DrawMapPosition(e.Graphics, TopLeft, BottomRight);


                        //If we're a network map, show only the violations. Otherwise, draw our lines
                        if (isNetworkMap && DisplayViolations)
                        {
                            Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations = Violations.ShownViolations;

                            var violations = MM_Repository.Violations.Keys.ToList();

                            foreach (MM_AlarmViolation Viol in violations)
                            {
                                try
                                {
                                    if (Viol.Type != null && ((Viol.Type.MiniMap == MM_AlarmViolation_Type.DisplayModeEnum.Always) || ((Viol.Type.MiniMap == MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected) && (ShownViolations != null && ShownViolations.ContainsKey(Viol)))))
                                        DrawViolation(e.Graphics, Viol, SelectedUIDs.ContainsKey(Viol), SelectedUIDs.Count == 1);
                                } catch (Exception ) {

                                }
                            }
                        }
                        else
                            if (!isNetworkMap)
                            {
                                //Check to see if we have lines within to draw
                                if (BaseData.Data.Tables.Contains("Line"))
                                    for (int a = 0; a < BaseData.Data.Tables["Line"].Rows.Count; a++)
                                        DrawLine(e.Graphics, BaseData.Data.Tables["Line"].Rows[a]["TEID"] as MM_Line);
                                foreach (MM_Line ThisLine in BorderingLines.Keys)
                                    DrawLine(e.Graphics, ThisLine);
                            }

                        if (!isNetworkMap && ShowRegionalView && BaseData.Data.Tables.Contains("Substation"))
                        {
                            bool DrawNames = BaseData.Data.Tables["Substation"].Rows.Count < 7;
                            foreach (DataRow dr in BaseData.Data.Tables["Substation"].Rows)
                                DrawSubstation(e.Graphics, dr["TEID"] as MM_Substation, DrawNames);
                        }
                    }

                    if (Data_Integration.InitializationComplete)
                        DrawRequestedIndicators(e.Graphics);
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError("Error in mini-map repaint: " + ex.Message);
            }
        }


        /// <summary>
        /// Draw a substation on the display, and add it to the list of elements
        /// </summary>
        /// <param name="g">The graphics connector</param>
        /// <param name="Substation">The substation to draw</param>
        /// <param name="DisplayName">Wheter or not the substation's name should be displayed</param>
        private void DrawSubstation(Graphics g, MM_Substation Substation, bool DisplayName)
        {
            //Determine where the substation should be on the map
            PointF SubstationPoint = ConvertPoint(Substation.LngLat);


            //If we're displaying the name, determine its size, center & draw it


            using (SolidBrush DrawBrush = new SolidBrush(Substation.SubstationDisplay(Violations.ShownViolations, this).ForeColor))
                if (DisplayName)
                {
                    String StationName = Substation.DisplayName();
                    SizeF StationSize = g.MeasureString(StationName, StationFont);
                    if (SubstationPoint.X < DisplayRectangle.Width / 2f)
                        SubstationPoint.X -= (StationSize.Width / 2f);
                    g.DrawString(StationName, StationFont, DrawBrush, SubstationPoint);

                    //Now add in our hit test for our other substation
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddRectangle(new RectangleF(SubstationPoint, StationSize));
                    RegionalElements.Add(gp, Substation);
                }
                else
                {
                    //Otherwise, draw the substation rectangle
                    RectangleF OutRect = new RectangleF(SubstationPoint.X - 2f, SubstationPoint.Y - 2f, 4f, 4f);
                    g.FillRectangle(DrawBrush, OutRect);
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddRectangle(OutRect);
                    RegionalElements.Add(gp, Substation);
                }
        }


        /// <summary>
        /// Draw a regional view around a substation
        /// </summary>
        /// <param name="Substation">The core substation to be drawn around</param>
        /// <param name="g">The graphics connector</param>
        private void DrawRegionalAroundSubstation(MM_Substation Substation, Graphics g)
        {
            Font StationFont = new Font("Arial", DisplayRectangle.Width / 21f);
            PointF CenterPoint = new PointF(DisplayRectangle.Width / 2f, DisplayRectangle.Height / 2f);
            if (BaseData.Data.Tables.Contains("Line"))
                foreach (DataRow dr in BaseData.Data.Tables["Line"].Rows)
                {
                    MM_Substation OtherStation = null;
                    MM_Line Line = dr["TEID"] as MM_Line;
                    if (Line.ConnectedStations == null)
                    { }
                    else if (Line.ConnectedStations[0] == Substation)
                        OtherStation = Line.ConnectedStations[1];
                    else if (Line.ConnectedStations[1] == Substation)
                        OtherStation = Line.ConnectedStations[0];

                    if (OtherStation != null)
                    {
                        float DistanceRatio = CenterPoint.X * Substation.DistanceTo(OtherStation) / MileDistance;
                        PointF TargetRect = new PointF((float)Math.Cos(Substation.AngleTo(OtherStation)) * DistanceRatio + CenterPoint.X, -(float)Math.Sin(Substation.AngleTo(OtherStation)) * DistanceRatio + CenterPoint.Y);

                        //Now draw our line.

                        MM_DisplayParameter Disp = (dr["TEID"] as MM_Line).LineDisplay(Violations.ShownViolations, this, false);

                        using (Pen DrawPen = new Pen(Disp.ForeColor, Disp.Width))
                        {
                            DrawPen.DashStyle = Disp.ForePen.DashStyle;
                            g.DrawLine(DrawPen, CenterPoint, TargetRect);
                        }

                        //Now draw the other station and its name
                        TargetRect.Y -= StationFont.Height / 2;
                        String StationName = OtherStation.DisplayName();
                        SizeF StationSize = g.MeasureString(StationName, StationFont);
                        if (TargetRect.X < CenterPoint.X)
                            TargetRect.X -= (StationSize.Width / 2f);
                        g.DrawString(StationName, StationFont, OtherStation.SubstationDisplay(Violations.ShownViolations, this).ForePen.Brush, TargetRect);

                        //Now add in our hit test for our other substation
                        {
                            GraphicsPath gp = new GraphicsPath();
                            gp.AddRectangle(new RectangleF(TargetRect, StationSize));
                            RegionalElements.Add(gp, OtherStation);
                        }

                        {
                            //Now add in our hit test for our line
                            GraphicsPath gp = new GraphicsPath();
                            gp.AddLine(CenterPoint, TargetRect);
                            RegionalElements.Add(gp, (dr["TEID"] as MM_Line));
                        }
                    }
                }
            //Now draw our substation center
            float StnRadius = 5f;
            g.FillRectangle(Substation.SubstationDisplay(Violations.ShownViolations, this).ForePen.Brush, CenterPoint.X - StnRadius, CenterPoint.Y - StnRadius, StnRadius * 2f, StnRadius * 2f);

            //Draw our legend
            //Now draw our legend
            PointF StartLegend = new PointF(3f * DisplayRectangle.Width / 8, DisplayRectangle.Height - 15f);
            PointF EndLegend = new PointF(5f * DisplayRectangle.Width / 8, DisplayRectangle.Height - 15f);
            g.DrawLine(Pens.LightGray, StartLegend, EndLegend);
            g.DrawLine(Pens.LightGray, StartLegend.X, StartLegend.Y - 5f, StartLegend.X, StartLegend.Y + 5f);
            g.DrawLine(Pens.LightGray, EndLegend.X, EndLegend.Y - 5f, EndLegend.X, EndLegend.Y + 5f);
            g.DrawString(MileDistance.ToString("0.0") + " mi", StationFont, Brushes.LightGray, StartLegend.X + 2f, StartLegend.Y + 2f);

        }


        /// <summary>
        /// Draw a line on the mini-map
        /// </summary>
        /// <param name="g">The graphics connector</param>
        /// <param name="Line">The line to be drawn</param>
        private void DrawLine(Graphics g, MM_Line Line)
        {
            if (Line == null)
                return;
            PointF StartPoint = ConvertPoint(Line.ConnectedStations[0].LngLat);
            PointF EndPoint = ConvertPoint(Line.ConnectedStations[1].LngLat);
            MM_DisplayParameter ToDraw = (Line.WorstVisibleViolation(Violations.ShownViolations, this) != null ? Line.WorstVisibleViolation(Violations.ShownViolations, this) : Line.KVLevel.Energized);

            using (Pen DrawPen = new Pen(ToDraw.ForeColor, ToDraw.Width))
            {
                DrawPen.DashStyle = ToDraw.DashStyle;
                g.DrawLine(DrawPen, StartPoint, EndPoint);
            }

            if (ShowRegionalView)
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddLine(StartPoint, EndPoint);
                RegionalElements.Add(gp, Line);
            }
        }



        /// <summary>
        /// Draw the key indicators as requested by the user
        /// </summary>
        /// <param name="g">The graphics connector</param>
        private void DrawRequestedIndicators(Graphics g)
        {

            //Start by determining the flows in and out, and the generation and load. 
            //If we have no lines on the periphery (not a lasso or one-line), use the DCTie in and out, and the total load and generation.            
            float FlowIn = 0, FlowOut = 0, Gen = 0f, Ld = 0f, capTotalNominalMVar = 0f, reacsTotalNominalMVar = 0f, capsOpenCount = 0, ReacN = 0, regUp = 0,
            capClosedCount = 0, ReacA = 0, capClosedMVar = 0, ReacAs = 0, GenC = 0, GenEC = 0, GenReac=0, GenCap=0, GenCapCount=0, GenReacCount=0;
            if (BorderingLines == null || Island != null)
            {
                FlowIn = -Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieIn];
                FlowOut = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieOut];
                Gen = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Gen];
                GenC = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.GenCapacity];
                GenEC = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.GenEmrgCapacity];
                Ld = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Load];
                ShowMVA = false;
            }
            else
            {
                //For each line, look at the line terminal within our region, and determine its flow.
                //If it's coming in to our region/substation, count it as flow in, otherwise flow out.
                foreach (KeyValuePair<MM_Line, MM_Substation> Line in BorderingLines)
                {
                    float Flow = ShowMVA ? Line.Key.MVAFlow : Line.Key.MWFlow;
                    if (Line.Key.MVAFlowDirection == null || float.IsNaN(Flow))
                    { }
                    else if (Line.Key.MVAFlowDirection == Line.Value)
                        FlowIn += Math.Abs(Flow);
                    else
                        FlowOut += Math.Abs(Flow);
                }
              
                //Now tally up our generation
                if (BaseData.Data.Tables.Contains("Unit"))
                {
                    foreach (DataRow dr in BaseData.Data.Tables["Unit"].Rows)
                    {
                        MM_Unit Unit = (MM_Unit)dr["TEID"];

                        float UnitMW = Unit.RTGenMW > 0 && !Unit.MultipleNetmomForEachGenmom ? Unit.RTGenMW : Unit.Estimated_MW;
                        float Val = ShowMVA ? Unit.Estimated_MVA : UnitMW;
                          float UnitHASL = Unit.HASL;
                        float UnitHSL = Unit.HSL;
                        float ecoMax = !float.IsNaN(Unit.EcoMax) && Unit.EcoMax > 0 && !Unit.MultipleNetmomForEachGenmom ? Unit.EcoMax : Unit.MaxCapacity;
                        float emerMax = !float.IsNaN(Unit.EmerMax) && Unit.EcoMax > 0 && !Unit.MultipleNetmomForEachGenmom ? Unit.EmerMax : Unit.MaxCapacity;
                        
                        if (!float.IsNaN(Val))
                            Gen += Val;

                        if (!float.IsNaN(UnitMW) && (UnitMW > MM_Repository.OverallDisplay.EnergizationThreshold || !ShowOnlineGenerationOnly))
                        {
                             if (!float.IsNaN(UnitHASL) && UnitHASL > UnitMW)
                                GenC += UnitHASL - UnitMW;
							 else if (!float.IsNaN(ecoMax) && ecoMax > UnitMW)
                                GenC += ecoMax - UnitMW;
                            if (!float.IsNaN(Unit.RegUp) && ecoMax - (UnitMW + Unit.RegUp) > 0)
                                GenC -= Unit.RegUp;
                           if (!float.IsNaN(UnitHSL) && UnitHSL > UnitMW)
                                GenEC += UnitHSL - UnitMW;
						    else if (!float.IsNaN(emerMax) && emerMax > UnitMW)
                                GenEC += emerMax - UnitMW;
                            regUp += Unit.RegUp;
                        }

                        if (!float.IsNaN(UnitMW) && UnitMW > MM_Repository.OverallDisplay.EnergizationThreshold)
                        {
                            float MVARCap = Unit.UnitMVARCapacity;
                            if (MVARCap > 0)
                            {
                                GenReac += MVARCap - Unit.Estimated_MVAR;
                                GenReacCount++;
                            }
                            else if (MVARCap < 0)
                            {
                                GenCap += MVARCap - Unit.Estimated_MVAR;
                                GenCapCount++;
                            }
                        }
                    }
                }
                //Now tally up our load
                if (BaseData.Data.Tables.Contains("Load"))
                    foreach (DataRow dr in BaseData.Data.Tables["Load"].Rows)
                    {
                        float Val = ShowMVA ? (dr["TEID"] as MM_Load).Estimated_MVA : (dr["TEID"] as MM_Load).Estimated_MW;
                        if (!float.IsNaN(Val))
                            Ld += Val;
                    }

                //Now tally up our caps
                if (BaseData.Data.Tables.Contains("Capacitor"))
                    foreach (DataRow dr in BaseData.Data.Tables["Capacitor"].Rows)
                    {
                        MM_ShuntCompensator Cap = dr["TEID"] as MM_ShuntCompensator;
                        if (Cap.Open)
                        {
                            capTotalNominalMVar += Cap.Nominal_MVAR;
                            capsOpenCount++;
                        }
                        else if (!float.IsNaN(Cap.Estimated_MVAR))
                        {
                            capClosedMVar += Cap.Estimated_MVAR;
                            capClosedCount++;
                        }

                    }

                //Now tally up our reactors
                if (BaseData.Data.Tables.Contains("Reactor"))
                    foreach (DataRow dr in BaseData.Data.Tables["Reactor"].Rows)
                    {
                        MM_ShuntCompensator Reac = dr["TEID"] as MM_ShuntCompensator;
                        if (Reac.Open)
                        {
                            ReacN++;
                            reacsTotalNominalMVar += Reac.Nominal_MVAR;
                        }
                        else if (!float.IsNaN(Reac.Estimated_MVAR))
                        {
                            ReacA++;
                            ReacAs += Reac.Estimated_MVAR;
                        }
                    }
            }



            //Now, draw the flows in/out, load/gen if requested                                    
            if (ShowFlowIn)
            {
                String InToDraw;

                if (Island != null)
                {
                    InToDraw = "Island " + Island.ID.ToString();
                    foreach (MM_Unit Unit in Island.Units)
                        if (Unit.FrequencyControl)
                            InToDraw += "\n" + Unit.Substation.Name + " " + Unit.Name;
                }
                else
                    InToDraw = "In:\n" + FlowIn.ToString("#,##0") + (ShowMVA ? "\nMVA" : "\nMW");

                RectangleF OutRect = new RectangleF(new PointF(1, 1), g.MeasureString(InToDraw, TextFont));
                g.FillRectangle(BackBrush, OutRect);
                g.DrawString(InToDraw, TextFont, Brushes.White, OutRect.Location);
            }

            if (ShowFlowOut)
            {
                String OutToDraw;
                if (Island != null)
                    OutToDraw = (Island.FrequencyControl ? "Frq Ctrl\n" : "") + Island.Frequency.ToString("0.000") + " Hz";
                else
                    OutToDraw = "Out:\n" + FlowOut.ToString("#,##0") + (ShowMVA ? "\nMVA" : "\nMW");



                SizeF ThisSize = g.MeasureString(OutToDraw, TextFont);
                RectangleF OutRect = new RectangleF(new PointF(DisplayRectangle.Width - ThisSize.Width - 1, 1), ThisSize);
                g.FillRectangle(BackBrush, OutRect);
                g.DrawString(OutToDraw, TextFont, Brushes.White, OutRect.Location);
            }

            String GenToDraw = "";
            if (ShowGen && Island != null)
                GenToDraw = "Gen:\n" + Island.Generation.ToString("#,##0") + " MW/" + Island.Units.Length.ToString("#,##0") + "\nAvl Cap:" + Island.AvailableGenCapacity.ToString("#,##0") + (regUp > 0 ? (" MW RegUp " + regUp) : "");
            else if (ShowGen && (Gen > 0 || GenC > 0) && Island == null)
                GenToDraw = "Gen:\n" + Gen.ToString("#,##0") + (ShowMVA ? " MVA" : " MW") + "\n" + (ShowEmergencyMode ? "Avail(E): " + GenEC.ToString("#,##0") : "Avail: " + GenC.ToString("#,##0")) + " MW";
            if (ShowGen && !String.IsNullOrEmpty(GenToDraw))
            {
                SizeF ThisSize = g.MeasureString(GenToDraw, TextFont);
                RectangleF OutRect = new RectangleF(new PointF(1, DisplayRectangle.Height - ThisSize.Height), ThisSize);
                g.FillRectangle(BackBrush, OutRect);
                g.DrawString(GenToDraw, TextFont, Brushes.White, OutRect);
            }


            String LoadToDraw="";
            if (Ld > 0 && Island == null)
                LoadToDraw = "Load:\n" + Ld.ToString("#,##0") + (ShowMVA ? "\nMVA" : "\nMW");
            else if (Island != null)
                    LoadToDraw = "Load:\n" + Island.Load.ToString("#,##0") + "\nMW";
            if (ShowLoad && !String.IsNullOrEmpty(LoadToDraw))
            {
                SizeF ThisSize = g.MeasureString(LoadToDraw, TextFont);
                RectangleF OutRect = new RectangleF(DisplayRectangle.Width - ThisSize.Width, DisplayRectangle.Height - ThisSize.Height, ThisSize.Width, ThisSize.Height);
                g.FillRectangle(BackBrush, OutRect);
                g.DrawString(LoadToDraw, TextFont, Brushes.White, OutRect);
            }


            if (ShowCaps && (capTotalNominalMVar != 0 || GenCap != 0))
            {
                String CapsToDraw = String.Format("Caps (#/mvar):" + (capsOpenCount > 0 ? "\nAvail:{1:#,##0}/{2:#,##0}" : "") + (capClosedCount > 0 ? "\nUsed:{3:#,##0}/{4:#,##0}" : ""), ShowMVA ? "MVA" : "MVAR", capsOpenCount, capTotalNominalMVar, capClosedCount, capClosedMVar);
                if (GenCap != 0)
                    CapsToDraw += "\nUnits: " + GenCapCount.ToString() + "/"+(-GenCap).ToString("#,##0");
                //" + CapN.ToString("#,##0") + "/\n" + Caps.ToString("#,##0") + "\nAvail.";
                SizeF ThisSize = g.MeasureString(CapsToDraw, TextFont);
                RectangleF OutRect = new RectangleF(1, (DisplayRectangle.Height - ThisSize.Height) / 2f, ThisSize.Width, ThisSize.Height);
                g.FillRectangle(BackBrush, OutRect);
                g.DrawString(CapsToDraw, TextFont, Brushes.White, OutRect);
            }

            if (ShowReactors && (reacsTotalNominalMVar != 0  || GenReac != 0))
            {
                String ReacsToDraw = String.Format("Reacs (#/mvar):" + (ReacN > 0 ? "\nAvail:{1:#,##0}/{2:#,##0}" : "") + (ReacA > 0 ? "\nUsed:{3:#,##0}/{4:#,##0}" : ""), ShowMVA ? "MVA" : "MVAR", ReacN, -reacsTotalNominalMVar, ReacA, -ReacAs);
                if (GenReac != 0)
                    ReacsToDraw += "\nUnits: " + GenReacCount.ToString() + "/"+ GenReac.ToString("#,##0");
                //String ReacsToDraw = "Reacs:\n" + ReacN.ToString("#,##0") + "/\n" + Reacs.ToString("#,##0") + "\nAvail.";
                SizeF ThisSize = g.MeasureString(ReacsToDraw, TextFont);
                RectangleF OutRect = new RectangleF(DisplayRectangle.Width - ThisSize.Width, (DisplayRectangle.Height - ThisSize.Height) / 2f, ThisSize.Width, ThisSize.Height);
                g.FillRectangle(BackBrush, OutRect);
                g.DrawString(ReacsToDraw, TextFont, Brushes.White, OutRect);
            }
        }


        /// <summary>
        /// Draw a violation on the mini-map
        /// </summary>
        /// <param name="g">The graphics connector to the mini-map</param>
        /// <param name="ViolToDraw">The violation to be drawn</param>
        /// <param name="Selected">Whether the violation is selected by the user</param>
        /// <param name="SingleViolation">Whether only a single violation should be shown</param>
        private void DrawViolation(Graphics g, MM_AlarmViolation ViolToDraw, bool Selected, bool SingleViolation)
        {
            //If the violation is around a substation, let's color it.
            if (ViolToDraw.SubstationOrLine is MM_Substation)
            {
                if (float.IsNaN((ViolToDraw.SubstationOrLine as MM_Substation).LngLat.X))
                    return;

                PointF StationPoint = ConvertPoint((ViolToDraw.SubstationOrLine as MM_Substation).LngLat);

                if (Selected)
                    if (SingleViolation)
                    {
                        //g.DrawEllipse(DrawingColor, StationPoint.X - 2, StationPoint.Y - 2, 4, 4);
                        g.FillRectangle(ViolToDraw.Type.ForePen.Brush, StationPoint.X - 2, StationPoint.Y - 2, 4, 4);
                        g.DrawLine(ViolToDraw.Type.ForePen, 0, StationPoint.Y, StationPoint.X, StationPoint.Y);
                        g.DrawLine(ViolToDraw.Type.ForePen, StationPoint.X, StationPoint.Y, DisplayRectangle.Width, StationPoint.Y);
                        g.DrawLine(ViolToDraw.Type.ForePen, StationPoint.X, 0, StationPoint.X, StationPoint.Y);
                        g.DrawLine(ViolToDraw.Type.ForePen, StationPoint.X, StationPoint.Y, StationPoint.X, DisplayRectangle.Height);
                    }
                    else
                        g.FillRectangle(ViolToDraw.Type.ForePen.Brush, StationPoint.X - 2, StationPoint.Y - 2, 4, 4);
                else
                    g.FillRectangle(ViolToDraw.Type.ForePen.Brush, StationPoint.X - 1, StationPoint.Y - 1, 2, 2);
            }
            //If it's a line, handle accordingly
            else if (ViolToDraw.SubstationOrLine is MM_Line)
            {
                if (float.IsNaN((ViolToDraw.SubstationOrLine as MM_Line).ConnectedStations[0].LngLat.X))
                    return;

                PointF FirstPoint = ConvertPoint((ViolToDraw.SubstationOrLine as MM_Line).ConnectedStations[0].LngLat);
                PointF SecondPoint = ConvertPoint((ViolToDraw.SubstationOrLine as MM_Line).ConnectedStations[1].LngLat);

                if (Selected)
                    if (SingleViolation)
                    {
                        g.DrawLine(ViolToDraw.Type.ForePen, FirstPoint.X, FirstPoint.Y, SecondPoint.X, SecondPoint.Y);
                        Rectangle LineBox = Rectangle.Ceiling(new RectangleF(Math.Min(FirstPoint.X, SecondPoint.X) - 3, Math.Min(FirstPoint.Y, SecondPoint.Y) - 3, Math.Abs(FirstPoint.X - SecondPoint.X) + 6, Math.Abs(FirstPoint.Y - SecondPoint.Y) + 6));
                        g.DrawRectangle(ViolToDraw.Type.ForePen, LineBox);
                        g.DrawLine(ViolToDraw.Type.ForePen, 0, (LineBox.Top + LineBox.Bottom) / 2, LineBox.Left, (LineBox.Top + LineBox.Bottom) / 2);
                        g.DrawLine(ViolToDraw.Type.ForePen, LineBox.Right, (LineBox.Top + LineBox.Bottom) / 2, DisplayRectangle.Width, (LineBox.Top + LineBox.Bottom) / 2);
                        g.DrawLine(ViolToDraw.Type.ForePen, (LineBox.Left + LineBox.Right) / 2, 0, (LineBox.Left + LineBox.Right) / 2, LineBox.Top);
                        g.DrawLine(ViolToDraw.Type.ForePen, (LineBox.Left + LineBox.Right) / 2, LineBox.Bottom, (LineBox.Left + LineBox.Right) / 2, DisplayRectangle.Height);
                    }
                    else
                        g.DrawLine(ViolToDraw.Type.ForePen, FirstPoint.X, FirstPoint.Y, SecondPoint.X, SecondPoint.Y);
                else
                    g.DrawLine(ViolToDraw.Type.ForePen, FirstPoint.X, FirstPoint.Y, SecondPoint.X, SecondPoint.Y);
            }
        }

        /// <summary>
        /// Draw the white rectangle signifying the lasso's position        
        /// </summary>
        /// <param name="g">Graphics connector</param>
        /// <param name="Coord">The coordinates class with boundaries</param>
        private void DrawLassoBorder(Graphics g, MM_Coordinates Coord)
        {
            if (Coord == null) return;
            //g.DrawRectangle(Pens.LightGray, ConvertRectangle(Coord.TopLeft, Coord.BottomRight));

            if (Coord.LassoPoints.Count > 1)
                g.DrawLines(Coord.LassoPen, ConvertPoints(Coord.LassoPoints));
            else if (!Coord.LassoEnd.IsEmpty)
            {
                g.FillRectangle(Coord.LassoBrush, ConvertRectangle(Coord.LassoStart, Coord.LassoEnd));
                g.DrawRectangle(Pens.White, ConvertRectangle(Coord.LassoStart, Coord.LassoEnd));
            }
        }

        /// <summary>
        /// Draw the white rectangle signifying the map's position
        /// </summary>
        /// <param name="g">Graphics connector</param>
        /// <param name="TopLeft">Top-left point</param>
        /// <param name="BottomRight">Bottom-right point</param>
        private void DrawMapPosition(Graphics g, PointF TopLeft, PointF BottomRight)
        {
            Rectangle OutRect = ConvertRectangle(TopLeft, BottomRight);
            if (TopLeft.IsEmpty && BottomRight.IsEmpty)
                return;
            g.DrawRectangle(Pens.LightGray, OutRect);
            if (OutRect.Width < 6)
            {
                PointF MidPoint = new PointF(OutRect.X + (OutRect.Width / 2f), OutRect.Y + (OutRect.Height / 2f));
                g.DrawLine(Pens.LightGray, 0, MidPoint.Y, OutRect.Left, MidPoint.Y);
                g.DrawLine(Pens.LightGray, OutRect.Right, MidPoint.Y, DisplayRectangle.Width, MidPoint.Y);
                g.DrawLine(Pens.LightGray, MidPoint.X, 0, MidPoint.X, OutRect.Top);
                g.DrawLine(Pens.LightGray, MidPoint.X, OutRect.Bottom, MidPoint.X, DisplayRectangle.Height);
            }
        }

        #region Mouse handling
        /// <summary>
        /// Handle mouse down
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                OnMouseMove(e);
            base.OnMouseDoubleClick(e);
        }

        /// <summary>
        /// Handle mouse movement
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //If the user is pressing the left mouse button and can zoom/pan, do it
            if ((e.Button == MouseButtons.Left) && UserCanZoomPan && isNetworkMap)
            {
                Coord.Center = UnconvertPoint(e.Location);
                this.Invalidate();

            }
            else
            {
                if (ShowRegionalView && RegionalElements != null)
                {
                    foreach (KeyValuePair<GraphicsPath, MM_Element> Elem in RegionalElements)
                        if (Elem.Key.IsOutlineVisible(e.Location, TestPen))
                        {
                            tTip.Show(Elem.Value.ElemType.Name + " " + Elem.Value.Name, this, e.Location.X + 10, e.Location.Y + 20);
                            return;
                        }
                    tTip.Hide(this);
                }
            }
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Handle mouse wheel
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //If the user is moving the mouse wheel and they can zoom, do it.
            if (UserCanZoomPan && isNetworkMap)
            {
                Coord.UpdateZoom(Coord.ZoomLevel + Math.Sign(e.Delta));
                this.Invalidate();
            }
        }


        #endregion

        #region Menu handling
        /// <summary>
        /// Handle the right mouse-click to bring up a popup menu
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {
                if (RegionalElements != null && ShowRegionalView)
                    foreach (KeyValuePair<GraphicsPath, MM_Element> Elem in RegionalElements)
                        if (Elem.Key.IsOutlineVisible(e.Location, TestPen))
                        {
                            MiniMapMenu.Show(this, e.Location, Elem.Value, true);
                            return;
                        }
                MiniMapMenu.Show(this, e.Location);
            }
            base.OnMouseClick(e);
        }


        /// <summary>
        /// Zoom/pan the mini-map (and therefore associated larger maps) to the specified point
        /// </summary>
        /// <param name="TargetLngLat">The target latitude and longitude</param>
        public void ZoomPanToPoint(PointF TargetLngLat)
        {
            //Simply set our new lat/long
            Coord.Center = TargetLngLat;

            /* Old code with the zooming panning actions
            //First determine the pixel values on the mini-map for the current position and target position
            PointF CurrentPos = ConvertPoint(Coord.Center);
            PointF TargetPos = ConvertPoint(TargetLngLat);
            if (CurrentPos == TargetPos)
                return;
            //Now determine how much we need to move things each step in order to get our map to where it needs to be
            PointF DeltaPos = new PointF(TargetPos.X - CurrentPos.X, TargetPos.Y - CurrentPos.Y);
            float ZoomRate = 3f;
            if (Math.Abs(DeltaPos.X) > Math.Abs(DeltaPos.Y))
                DeltaPos = new PointF(ZoomRate * Math.Sign(DeltaPos.X), DeltaPos.Y / (Math.Abs(DeltaPos.X / ZoomRate)));
            else
                DeltaPos = new PointF(DeltaPos.X / (Math.Abs(DeltaPos.Y / ZoomRate)), ZoomRate * Math.Sign(DeltaPos.Y));

            bool XIncreasing = Math.Sign(DeltaPos.X) == 1;
            bool YIncreasing = Math.Sign(DeltaPos.Y) == 1;

            //Zoom out to our pan level
            //float PanLevel = 16;
            //for (float Zoom = Coord.ZoomLevel; Zoom > PanLevel; Zoom--)
            //    ZoomPanPoints.Enqueue(new KeyValuePair<PointF, float>(Coord.Center, Zoom));

            while ((XIncreasing ? CurrentPos.X < TargetPos.X : CurrentPos.X > TargetPos.X) || (YIncreasing ? CurrentPos.Y < TargetPos.Y : CurrentPos.Y > TargetPos.Y))
            {
                ZoomPanPoints.Enqueue(new KeyValuePair<PointF, float>(UnconvertPoint(CurrentPos), Coord.ZoomLevel));
                CurrentPos.X += DeltaPos.X;
                CurrentPos.Y += DeltaPos.Y;
            }

            ZoomPanPoints.Enqueue(new KeyValuePair<PointF, float>(TargetLngLat, Coord.ZoomLevel));
            ZoomPanPoints.Enqueue(new KeyValuePair<PointF,float>(TargetLngLat, 24f));

            * //These following lines were double commented.
            * //Now zoom back in
            * float ZoomInLevel = 24f;
            * for (float Zoom = PanLevel; Zoom < ZoomInLevel; Zoom++)
            *    ZoomPanPoints.Enqueue(new KeyValuePair<PointF, float>(TargetLngLat, Zoom));
            
            networkMap.OngoingZoomPan = true;
            ZoomPanTimer.Start();*/
        }


        /// <summary>
        /// A timer event has occurred. Zoom/pan the map to the next point in the stack, and if none are left close the timer
        /// </summary>  
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomPanTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            KeyValuePair<PointF, int> ZoomPanPoint = ZoomPanPoints.Dequeue();
            if (ZoomPanPoint.Key != Coord.Center)
                Coord.Center = ZoomPanPoint.Key;
            if (ZoomPanPoint.Value != Coord.ZoomLevel)
                Coord.UpdateZoom(ZoomPanPoint.Value);
            if (ZoomPanPoints.Count == 0)
            {
                //If we're out of zoom/pan points, let's trigger just one more update
                ZoomPanTimer.Stop();
                networkMap.OngoingZoomPan = false;
                networkMap.IsDirty = true;
            }
        }
        #endregion


    }
}
