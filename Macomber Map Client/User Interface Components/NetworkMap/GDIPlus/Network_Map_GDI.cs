using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using Macomber_Map.Data_Connections;
using System.Drawing.Drawing2D;
using Macomber_Map.User_Interface_Components.NetworkMap;
using System.Threading;
using Macomber_Map.User_Interface_Components.NetworkMap.GDIPlus;
using Macomber_Map.Data_Elements.Display;
using System.Drawing.Imaging;
using Macomber_Map.Data_Elements.Positional;

namespace Macomber_Map.User_Interface_Components.GDIPlus
{
    /// <summary>
    /// This class renders the network map using GDI.
    /// </summary>
    public partial class Network_Map_GDI : Network_Map
    {
        #region Variable declarations
        /// <summary>Our collection of counties</summary>
        public Dictionary<MM_Boundary, MM_GDI_County> Counties = new Dictionary<MM_Boundary, MM_GDI_County>();

        /// <summary>Our collection of lines</summary>
        public Dictionary<MM_Line, MM_GDI_Line> Lines = new Dictionary<MM_Line, MM_GDI_Line>(); //was sorted dictonary

        /// <summary>Our collection of substations</summary>        
        public Dictionary<MM_Substation, MM_GDI_Substation> Substations = new Dictionary<MM_Substation, MM_GDI_Substation>();

        /// <summary>Our collection of multiple lines</summary>
        public List<MM_GDI_MultipleLine> MultipleLines = new List<MM_GDI_MultipleLine>();

        
        /// <summary>Our state boundary</summary>
        public MM_Boundary State;

        /// <summary>Our state boundary path</summary>
        public GraphicsPath StatePath;

        /// <summary>The render state (ready to be drawn, building for drawing, drawn.</summary>
        private enum enumRenderState { Ready, Compiling, Drawing };

        /// <summary>Our current rendering state</summary>
        private volatile enumRenderState RenderState = enumRenderState.Ready;

        /// <summary>Our Image Attributes for drawing the map</summary>
        private ImageAttributes DrawAttributes = new ImageAttributes();

        /// <summary>Whether the application is handling the mouse activity</summary>
        private bool HandlingMouse = false;

        /// <summary>The pen for hit-testing elements</summary>
        private Pen HitTestPen = new Pen(Brushes.Brown, 20f);

        /// <summary>The flag to ensure that operations do not occur with partially loaded data sets</summary>
        private static bool _LockForDataUpdate = false;

        //counter to ensure all DC_Tie related lines are taken care of once, to save operations
        private static int _DCUpdateTotal = 0;

        /// <summary>The element currently being updated</summary>
        public MM_Coordinate_Suggestion UpdatingElement = null;

        /// <summary>The actively moving element</summary>
        public MM_Element ActivelyMovingElement = null;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our network map control
        /// </summary>
        public Network_Map_GDI(MM_Coordinates Coordinates)
            : base(Coordinates)
        {
            InitializeComponent();
            BlinkTimer.Elapsed += new System.Timers.ElapsedEventHandler(BlinkTimer_Elapsed);
            FlowTimer.Elapsed += new System.Timers.ElapsedEventHandler(FlowTimer_Elapsed);
            MM_Repository.OverallDisplay.MapTransparencyChanged += DisplayOptions_MapTransparencyChanged;
        }
        #endregion

        #region Network map functions

        /// <summary>
        /// Handle the blink timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BlinkTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.IsBlinking ^= true;
            this.Invalidate();
        }



        /// <summary>
        /// Handle the flow timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlowTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!PanPoint.IsEmpty)
                Coordinates.UpdateFromXY(new Point(Coordinates.TopLeftXY.X + PanPoint.X, Coordinates.TopLeftXY.Y + PanPoint.Y));
            else if (Coordinates.ZoomLevel >= MM_Repository.OverallDisplay.LineFlows)
            {
                bool MovedFlow = false;
                while (RenderState != enumRenderState.Ready)
                    return;
                foreach (KeyValuePair<MM_Line, MM_GDI_Line> kvp in Lines)
                    if ((kvp.Value.Visible || kvp.Value.ViolationVisible) && kvp.Key.KVLevel.ShowMVA)
                    {
                        MovedFlow = true;
                        kvp.Value.IncrementLineFlow();
                    }
                if (MovedFlow)
                    this.Invalidate();
            }
        }

        /// <summary>
        /// Update information on all substations
        /// </summary>
        public override void UpdateSubstationInformation()
        {
            lock (Substations)
                foreach (MM_Substation Substation in MM_Repository.Substations.Values)
                    if (!Substations.ContainsKey(Substation))
                        Substations.Add(Substation, new MM_GDI_Substation(Coordinates, Substation, violViewer.ShownViolations, this));
                    else
                        Substations[Substation].ComputeSubstation(Coordinates, Substation, violViewer.ShownViolations, this, true);

        }

        /// <summary>
        /// Update information on a particular substation
        /// </summary>
        /// <param name="Substation">The substation to be updated</param>
        public override void UpdateSubstationInformation(MM_Substation Substation)
        {
            if (!Substations.ContainsKey(Substation))
                Substations.Add(Substation, new MM_GDI_Substation(Coordinates, Substation, violViewer.ShownViolations, this));
            else
                Substations[Substation].ComputeSubstation(Coordinates, Substation, violViewer.ShownViolations, this, true);
        }


        /// <summary>
        /// Update information on all lines
        /// </summary>
        public override void UpdateLineInformation()
        {

            foreach (MM_Line Line in MM_Repository.Lines.Values)
            {
                //lock(Line)
                if (Line.ConnectedStations[0] != Line.ConnectedStations[1] && !float.IsNaN(Line.ConnectedStations[0].LatLong.X) && !float.IsNaN(Line.ConnectedStations[1].LatLong.X))
                    if (!Lines.ContainsKey(Line))
                    {

                        MM_GDI_Line NewLine = new MM_GDI_Line(Line, Coordinates, violViewer.ShownViolations, this, State);
                        Lines.Add(Line, NewLine);
                        Line.ValuesChanged += new MM_Element.ValuesChangedDelegate(Line_ValueChanged);
                        foreach (KeyValuePair<MM_Line, MM_GDI_Line> kvp in Lines)
                            if (kvp.Key != Line && ((kvp.Key.Substation1 == Line.Substation1 && kvp.Key.Substation2 == Line.Substation2) || (kvp.Key.Substation1 == Line.Substation2 && kvp.Key.Substation2 == Line.Substation1)))
                            {
                                if (kvp.Value.MultipleLine == null)
                                {
                                    MultipleLines.Add(new MM_GDI_MultipleLine(kvp.Key, kvp.Value, Line, NewLine));

                                }
                                else
                                    kvp.Value.MultipleLine.AddLine(Line, NewLine);
                            }
                    }
                    else
                        Lines[Line].ComputeLine(Line, violViewer.ShownViolations, this, Coordinates, State, true);
            }
        }

        /// <summary>
        /// Update information on a particular line
        /// </summary>
        /// <param name="Line">The line to be updated</param>
        /// <param name="AllowAdd">Whether to add new elements</param>
        public override void UpdateLineInformation(MM_Line Line, bool AllowAdd)
        {
            if (Line.ConnectedStations[0] != Line.ConnectedStations[1] && !float.IsNaN(Line.ConnectedStations[0].LatLong.X) && !float.IsNaN(Line.ConnectedStations[1].LatLong.X))
                if (!Lines.ContainsKey(Line))
                {
                    MM_GDI_Line NewLine = new MM_GDI_Line(Line, Coordinates, violViewer.ShownViolations, this, State);
                    Lines.Add(Line, NewLine);
                    Line.ValuesChanged += new MM_Element.ValuesChangedDelegate(Line_ValueChanged);
                    foreach (KeyValuePair<MM_Line, MM_GDI_Line> kvp in Lines)
                        if (kvp.Key != Line && ((kvp.Key.Substation1 == Line.Substation1 && kvp.Key.Substation2 == Line.Substation2) || (kvp.Key.Substation1 == Line.Substation2 && kvp.Key.Substation2 == Line.Substation1)))
                        {
                            if (kvp.Key != Line && ((kvp.Key.Substation1 == Line.Substation1 && kvp.Key.Substation2 == Line.Substation2) || (kvp.Key.Substation1 == Line.Substation2 && kvp.Key.Substation2 == Line.Substation1)))
                            {
                                if (kvp.Value.MultipleLine == null)
                                    MultipleLines.Add(new MM_GDI_MultipleLine(kvp.Key, kvp.Value, Line, NewLine));
                                else
                                    kvp.Value.MultipleLine.AddLine(Line, NewLine);
                            }
                        }
                }
                else
                    Lines[Line].ComputeLine(Line, violViewer.ShownViolations, this, Coordinates, State, true);
        }

        /// <summary>
        /// Reset the network map
        /// </summary>
        public override void ResetMap()
        {
            ResetDisplayCoordinates();
            UpdateGraphicsPaths(true, true);
        }



        /// <summary>Lock Interaction and Key operations during switch from data types //MN//20130607 added this function</summary>
        public static bool LockForDataUpdate
        {
            get { return _LockForDataUpdate; }
            set { _LockForDataUpdate = value; }
        }

        /// <summary>Lock Interaction and Key operations during switch from data types //MN//20130607 added this function</summary>
        public static int DCUpdateTotal
        {
            get { return _DCUpdateTotal; }
            set { _DCUpdateTotal = value; }
        }

        /// <summary>
        /// Set the controls and data sources associated with the network map
        /// </summary>
        /// <param name="violViewer">The violation viewer</param>
        /// <param name="miniMap">The mini-map to be associated with the network map</param>
        /// <param name="resetZoom">Whether to reset our zoom</param>
        public override void SetControls(Violation_Viewer violViewer, Mini_Map miniMap, int resetZoom)
        {
            //MN//20130607 should I use this location to lock all zoom events??? not sure....
            //User_Interface_Components.GDIPlus.Network_Map_GDI.LockForDataUpdate = true;
            this.miniMap = miniMap;
            this.violViewer = violViewer;
            this.DoubleBuffered = true;
            MM_Repository.OverallDisplay.MapTransparency = MM_Repository.OverallDisplay.MapTransparency;
            if (resetZoom == 0) //mn//
            {
                Coordinates.SetControls();
                Coordinates.Zoomed += new MM_Coordinates.ZoomEvent(Coordinates_Zoomed);
                Coordinates.Panned += new MM_Coordinates.PanEvent(Coordinates_Panned);

                //Set up our counties, subs and lines
                Counties = new Dictionary<MM_Boundary, MM_GDI_County>(MM_Repository.Counties.Count);
                Substations = new Dictionary<MM_Substation, MM_GDI_Substation>(MM_Repository.Substations.Count);
            }
            //Lines = new Dictionary<MM_Line, MM_GDI_Line>();
            //MultipleLines = new List<MM_GDI_MultipleLine>(); //Note – also, want to look at clearing rather than re-instantiating all of these. //mn//
            if (resetZoom == 0) //mn//
            {
                UpdateGraphicsPaths(true, true);
                BlinkTimer.Enabled = true;
                FlowTimer.Enabled = true;
                tmrFPS = new System.Timers.Timer(1000);
                tmrFPS.Elapsed += tmrFPS_Elapsed;
                tmrFPS.Start();

                MM_Repository.ViewChanged += new MM_Repository.ViewChangedDelegate(Repository_ViewChanged);
                Data_Integration.NetworkSourceChanged += new EventHandler(Data_Integration_NetworkSourceChanged);
            }

           
            User_Interface_Components.GDIPlus.Network_Map_GDI.LockForDataUpdate = false;

        }

        /// <summary>
        /// When our network source changes, update the map location to simulate object replacement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_Integration_NetworkSourceChanged(object sender, EventArgs e)
        {
            MapMoved = true;
            foreach (KeyValuePair<MM_Line, MM_GDI_Line> kvp in Lines)
                kvp.Value.Line_ValuesChanged(kvp.Key);

        }

        /// <summary>
        /// Handle the updating of the repository view, by updating the graphics paths
        /// </summary>
        /// <param name="ActiveView"></param>
        void Repository_ViewChanged(MM_Display_View ActiveView)
        {

            Coordinates.UpdateZoom(Coordinates.ZoomLevel);

            //FlowTimer.Stop();
            //BlinkTimer.Stop();
            //UpdateGraphicsPaths(true); //mn//
            //MapMoved = true;
            //FlowTimer.Start();
            // BlinkTimer.Stop();
        }

        /// <summary>
        /// Handle the map pan event - find our pixel difference between the two points, and adjust everything linearly.
        /// </summary>
        /// <param name="OldCenter">The old center of the screen</param>
        /// <param name="NewCenter">The new center of the screen</param>
        private void Coordinates_Panned(PointF OldCenter, PointF NewCenter)
        {
            MapMoved = true;
            this.Invalidate();

        }


        /// <summary>
        /// Handle the zoom event - rebuild all points.
        /// </summary>
        /// <param name="OldZoom"></param>
        /// <param name="NewZoom"></param>
        private void Coordinates_Zoomed(float OldZoom, float NewZoom)
        {
            //while (RenderState != enumRenderState.Ready)
            //Application.DoEvents();

            //RenderState = enumRenderState.Compiling;
            UpdateGraphicsPaths(false, false); //MN//20130607
            MapMoved = true;
            this.Invalidate();
            GC.Collect();
        }

        /// <summary>
        /// Reset the display controls
        /// </summary>
        public override void ResetDisplayCoordinates()
        {
            Coordinates.SetTopLeftAndBottomRight(MM_Repository.Counties["STATE"].Min, MM_Repository.Counties["STATE"].Max);
            UpdateGraphicsPaths(false, true);
            MapMoved = true;
        }

        /// <summary>
        /// Set display coordiantes to a target value
        /// </summary>
        /// <param name="TopLeft"></param>
        /// <param name="BottomRight"></param>
        public override void SetDisplayCoordinates(PointF TopLeft, PointF BottomRight)
        {
            Coordinates.SetTopLeftAndBottomRight(TopLeft, BottomRight);
            UpdateGraphicsPaths(false, true); //MN//20130607
        }

        /// <summary>
        /// Handle the size change by updating our graphics paths
        /// </summary>
        public override void ResizeComplete()
        {
            if (this.Visible) //mn// what is this for?? what is it really doing??
                Coordinates.UpdateZoom(Coordinates.ZoomLevel);
            UpdateGraphicsPaths(false, true); //MN//20130607
        }

        /// <summary>
        /// Update our graphics paths for all images
        /// </summary>
        /// <param name="RedrawText">Whether our text needs to be withdrawn</param>
        /// <param name="AllowAdd">Whether to add new elements in to our repository</param>
        private void UpdateGraphicsPaths(bool RedrawText, bool AllowAdd)
        {
            while (RenderState != enumRenderState.Ready)
                Application.DoEvents();

            FlowTimer.Interval = MM_Repository.OverallDisplay.FlowInterval;

            RenderState = enumRenderState.Compiling;

            lock (this)
            {
                //Update our display rectangle 
                //TODO: Move to correct aspect ratio for rendering, so looks better on 16:9 displays
                Coordinates.DisplayRectangle = this.DisplayRectangle;
                //float MinDim = Math.Max(DisplayRectangle.Width, DisplayRectangle.Height);
                //Coordinates.DisplayRectangle = new RectangleF(this.DisplayRectangle.Left, this.DisplayRectangle.Top, MinDim, MinDim);

                //Compute the state and county boundaries            
                foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                    if (Bound.Name == "STATE")
                    {
                        State = Bound;
                        StatePath = CreateGraphicsPath(Bound.Coordinates);
                    }

                foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                    if (Bound.Name != "STATE" && Counties.ContainsKey(Bound))
                        Counties[Bound].CountyPath = CreateGraphicsPath(Bound.Coordinates);
                    else if (Bound.Name != "STATE")
                        Counties.Add(Bound, new MM_GDI_County(CreateGraphicsPath(Bound.Coordinates), Bound.DisplayParameter.ForeColor, Bound.DisplayParameter.Width));

                //Compute the substation positions.
                foreach (MM_Substation Substation in new List<MM_Substation>(MM_Repository.Substations.Values))
                    if (!Substations.ContainsKey(Substation))
                        Substations.Add(Substation, new MM_GDI_Substation(Coordinates, Substation, violViewer.ShownViolations, this));
                    else
                        Substations[Substation].ComputeSubstation(Coordinates, Substation, violViewer.ShownViolations, this, RedrawText);


                foreach (MM_Line Line in new List<MM_Line>(MM_Repository.Lines.Values))
                    if (Line.ConnectedStations[0] != Line.ConnectedStations[1] && !float.IsNaN(Line.ConnectedStations[0].LatLong.X) && !float.IsNaN(Line.ConnectedStations[1].LatLong.X))
                        try
                        {
                            UpdateLineInformation(Line, AllowAdd);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Compute Position Mismatch " + ex.Message);
                        }
                if (DCUpdateTotal < 1) //if (DCUpdateTotal < MM_Repository.DCTies.Count)
                {
                    MM_Line tempCoordinates = null;
                    bool UpdateCoords = false;
                    foreach (MM_GDI_MultipleLine Mult in MultipleLines)
                    {
                        UpdateCoords = false;
                        foreach (KeyValuePair<MM_Line, MM_GDI_Line> Line in Mult.LineCollection)
                        {
                            if (Line.Key is MM_DCTie)
                            {
                                UpdateCoords = true;
                                tempCoordinates = Line.Key;
                            }
                        }
                        if (UpdateCoords)
                        {
                            //update all multilines that share a path with a DCTie so that the moved substations can be accounted for.
                            foreach (KeyValuePair<MM_Line, MM_GDI_Line> Line in Mult.LineCollection)
                            {
                                Line.Key.Coordinates = tempCoordinates.Coordinates;
                            }
                        }
                    }
                    DCUpdateTotal++;
                }
            }
            RenderState = enumRenderState.Ready;
            //User_Interface_Components.GDIPlus.Network_Map_GDI.LockForDataUpdate = false;
        }

        /// <summary>
        /// Turn a series of latitude/longitude points into a unique graphics path
        /// </summary>
        /// <param name="LatLongs">The collection of latitudes/longitudes that define the path</param>
        /// <returns></returns>
        public GraphicsPath CreateGraphicsPath(PointF[] LatLongs)
        {
            GraphicsPath OutG = new GraphicsPath();
            Point LastPoint = Point.Empty;
            List<Point> OutPoints = new List<Point>(LatLongs.Length);
            foreach (PointF pt in LatLongs)
            {
                Point InPoint = MM_Coordinates.LatLngToXY(pt, Coordinates.ZoomLevel);
                if (LastPoint.IsEmpty || InPoint.X != LastPoint.X || InPoint.Y != LastPoint.Y)
                    OutPoints.Add(InPoint);
                LastPoint = InPoint;
            }

            OutG.AddLines(OutPoints.ToArray());
            return OutG;
        }

        /// <summary>
        /// Update all components following a resize event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            this.SuspendLayout();
            base.OnResize(e);
            if (Data_Integration.InitializationComplete)
                UpdateGraphicsPaths(false, true); //MN//20130607
            MapMoved = true;
            this.ResumeLayout(true);
        }

        /// <summary>
        /// When our map transparency options change, update our color matrix
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayOptions_MapTransparencyChanged(object sender, EventArgs e)
        {
            float MapTransparency = MM_Repository.OverallDisplay.MapTransparency;
            ColorMatrix OutMatrix = new ColorMatrix(new float[][] { new float[] { 1, 0, 0, 0, 0 }, new float[] { 0, 1, 0, 0, 0 }, new float[] { 0, 0, 1, 0, 0 }, new float[] { 0, 0, 0, MapTransparency, 0 }, new float[] { 0, 0, 0, 0, 1 } });
            this.DrawAttributes.SetColorMatrix(OutMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            this.Invalidate();
        }


        /// <summary>
        /// Handle the refresh of the map
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (RenderState != enumRenderState.Ready)
                return;
            RenderState = enumRenderState.Drawing;

            try
            {
                //Set our graphics parameters
                e.Graphics.TextRenderingHint = MM_Repository.OverallDisplay.TextRenderingHint;
                e.Graphics.TextContrast = MM_Repository.OverallDisplay.TextContrast;
                e.Graphics.PixelOffsetMode = MM_Repository.OverallDisplay.PixelOffsetMode;

                if (MM_Repository.AdaptiveRendering && MM_Repository.SystemCPUUsage >= .2f)
                    e.Graphics.SmoothingMode = SmoothingMode.None;
                else
                    e.Graphics.SmoothingMode = MM_Repository.OverallDisplay.SmoothingMode;



                e.Graphics.CompositingMode = MM_Repository.OverallDisplay.CompositingMode;
                e.Graphics.CompositingQuality = MM_Repository.OverallDisplay.CompositingQuality;
                e.Graphics.InterpolationMode = MM_Repository.OverallDisplay.InterpolationMode;

                //if (LockForDataUpdate)                    
                //        e.Graphics.FillRectangle(Brushes.Gray, e.ClipRectangle);
                // else
                using (SolidBrush sB = new SolidBrush(MM_Repository.OverallDisplay.BackgroundColor))
                    e.Graphics.FillRectangle(sB, e.ClipRectangle);
                using (SolidBrush sB = new SolidBrush(MM_Repository.OverallDisplay.BackgroundColor))
                    e.Graphics.FillRectangle(sB, e.ClipRectangle);


                //If we've moved, update our top-left point
                if (MapMoved)
                {
                    //PointF NewC = Coordinates.ConvertInvertedPoint(TopLeftLatLng);
                    //TopLeft = new PointF(NewC.X, -NewC.Y);

                    //Reset our connectors
                    foreach (MM_GDI_Substation Sub in Substations.Values)
                    {
                        Sub.ConnectorVisible = false;
                        Sub.ViolationVisible = false;
                    }
                    foreach (MM_GDI_Line Line in Lines.Values)
                    {
                        Line.Visible = false;
                        Line.ViolationVisible = false;
                    }

                    //Check counties for visibility
                    foreach (KeyValuePair<MM_Boundary, MM_GDI_County> County in Counties)
                        County.Value.Visible = MM_Repository.OverallDisplay.DisplayCounties && County.Key.CheckVisibility(Coordinates);

                    //Check subs for visibility
                    foreach (KeyValuePair<MM_Substation, MM_GDI_Substation> Substation in Substations)
                        CheckSubstationVisibility(Substation);

                    //Check lines for visibility
                    foreach (KeyValuePair<MM_Line, MM_GDI_Line> Line in Lines)
                        CheckLineVisibility(Line);

                    //Make sure our map tiles are available if necessary

                    MapMoved = false;
                }

                //while (RenderState != enumRenderState.Ready)
                //    Application.DoEvents();

                //RenderState = enumRenderState.Drawing;

                Graphics g = e.Graphics;
                g.ResetTransform();
                if (MM_Repository.OverallDisplay.FPS)
                    g.DrawString("FPS: " + LastFPS.ToString(), this.Font, Brushes.White, PointF.Empty);



                //If the user requested map tiles, draw them
                //First, draw our tiles if requested            
                MM_MapTile FoundTile;
                if (MM_Repository.OverallDisplay.MapTiles != MM_MapTile.enumMapType.None)
                {
                    Point TopLeftTile = MM_Coordinates.XYToTile(Coordinates.TopLeftXY);
                    Point BottomRightTile = MM_Coordinates.XYToTile(Coordinates.BottomRightXY);
                    Point TopLeftTileShift = new Point(Coordinates.TopLeftXY.X % MM_Repository.OverallDisplay.MapTileSize.Width, Coordinates.TopLeftXY.Y % MM_Repository.OverallDisplay.MapTileSize.Height);
                    int ExtraRadius = 4;
                    for (Point ThisTile = new Point(TopLeftTile.X - ExtraRadius, TopLeftTile.Y - ExtraRadius); ThisTile.X <= BottomRightTile.X + ExtraRadius; ThisTile.X++, ThisTile.Y = TopLeftTile.Y)
                        for (; ThisTile.Y <= BottomRightTile.Y + ExtraRadius; ThisTile.Y++)
                        {
                            MM_MapTile.Coordinates TileCoord = new MM_MapTile.Coordinates(MM_Repository.OverallDisplay.MapTiles, ThisTile, (int)Coordinates.ZoomLevel);
                            if (!MM_Repository.MapTiles.TryGetValue(TileCoord, out FoundTile))
                                MM_Repository.MapTiles.Add(TileCoord, new MM_MapTile(TileCoord));
                            else if (FoundTile.Failed)
                                FoundTile.DownloadTile();
                            else if (!FoundTile.Saving && FoundTile.Tile != null && ThisTile.X >= TopLeftTile.X && ThisTile.Y >= TopLeftTile.Y && ThisTile.X <= BottomRightTile.X && ThisTile.Y <= BottomRightTile.Y)
                            {
                                Rectangle TargetRect = new Rectangle(((ThisTile.X - TopLeftTile.X) * MM_Repository.OverallDisplay.MapTileSize.Width) - TopLeftTileShift.X, ((ThisTile.Y - TopLeftTile.Y) * MM_Repository.OverallDisplay.MapTileSize.Height) - TopLeftTileShift.Y, MM_Repository.OverallDisplay.MapTileSize.Width, MM_Repository.OverallDisplay.MapTileSize.Height);
                                e.Graphics.DrawImage(FoundTile.Tile, TargetRect, 0, 0, MM_Repository.OverallDisplay.MapTileSize.Width, MM_Repository.OverallDisplay.MapTileSize.Height, GraphicsUnit.Pixel, DrawAttributes);
                            }
                            else if (!FoundTile.Saving)
                                FoundTile.DownloadTile();

                        }
                }

                g.TranslateTransform(-Coordinates.TopLeftXY.X, -Coordinates.TopLeftXY.Y);
                g.RotateTransform(Coordinates.Rotation);

                //First, if we've requested contours, draw them.
                if (MM_Repository.OverallDisplay.Contour != MM_Display.MM_Contour_Enum.None && MM_Repository.OverallDisplay.DisplayCounties)
                    foreach (KeyValuePair<MM_Boundary, MM_GDI_County> County in Counties)
                        if (County.Value.Visible)
                            DrawCounty(County, g);


                //If the user is drawing a lasso, display it
                if (Coordinates.LassoMultiplePoints)
                    DrawLassoPoints(Coordinates.LassoPoints, g);
                else if (!Coordinates.LassoEnd.IsEmpty)
                    DrawLasso(Coordinates.LassoStart, Coordinates.LassoEnd, g);


                //Draw the counties                     
                if (MM_Repository.OverallDisplay.DisplayCounties)
                    foreach (KeyValuePair<MM_Boundary, MM_GDI_County> County in Counties)
                        if (County.Value.Visible)
                            g.DrawPath(County.Value.CountyPen, County.Value.CountyPath);

                //Draw the State
                if (MM_Repository.OverallDisplay.DisplayStateBorder)
                    using (Pen StatePen = new Pen(State.DisplayParameter.ForeColor, State.DisplayParameter.Width))
                        g.DrawPath(StatePen, StatePath);

                //Draw the substations
                foreach (KeyValuePair<MM_Substation, MM_GDI_Substation> Substation in Substations)
                    CheckDrawSubstation(Substation, g);

                //Draw the multiple line underpinnings
                foreach (MM_GDI_MultipleLine Mult in MultipleLines)
                    if (Mult.Visible)
                        try
                        {
                            if (MM_Repository.SubstationDisplay.HighlightMultipleLines)
                                Mult.DrawMultipleLine(g);
                            CheckDrawLine(Mult.BestLine, g);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Whoops! " + ex.Message);
                        }
                //Draw the standalone lines
                foreach (KeyValuePair<MM_Line, MM_GDI_Line> Line in Lines)
                    if (Line.Value.MultipleLine == null)
                    {
                        try
                        {
                            CheckDrawLine(Line, g);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Whoops! " + ex.Message);
                        }
                    }


                //If we're in line moving mode, handle accordingly
                if (Key_Indicators.DeltaDisplay.Visible && ActivelyMovingElement != null)
                {
                    PointF[] ActiveCoordinates = (ActivelyMovingElement is MM_Substation) ? new PointF[] { (ActivelyMovingElement as MM_Substation).LatLong } : (ActivelyMovingElement as MM_Line).Coordinates;
                    for (int a = 0; a < ActiveCoordinates.Length; a++)
                    {
                        Point XYPt = MM_Coordinates.LatLngToXY(ActiveCoordinates[a], Coordinates.ZoomLevel);
                        DrawCrosshair(Pens.LightGoldenrodYellow, XYPt, 10, e.Graphics, a.ToString());
                    }
                }
                //If we're in training mode, handle accordingly
                else if (MM_Repository.Training == null || MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.NotTraning)
                { }
                else
                    using (StringFormat sF = new StringFormat())
                    {
                        MM_Repository.Training.CheckTimes();
                        sF.Alignment = StringAlignment.Center;
                        e.Graphics.ResetTransform();

                        //Draw our question text
                        String TextToDraw;
                        Color DrawColor;
                        if (MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.UserWon)
                        {
                            TextToDraw = "Congratulations! You won!";
                            DrawColor = Color.LightGreen;
                        }
                        else if (MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.UserFailed)
                        {
                            TextToDraw = "Sorry - please play again!";
                            DrawColor = Color.Red;
                        }
                        else
                        {
                            TextToDraw = MM_Repository.Training.QuestionText;
                            DrawColor = MM_Repository.Training.QuestionTextColor;
                        }

                        SizeF DrawRect = e.Graphics.MeasureString(TextToDraw, MM_Repository.Training.QuestionFont, e.ClipRectangle.Width, sF);
                        RectangleF TrainingRect = new RectangleF(0, 0, e.ClipRectangle.Width, DrawRect.Height);
                        e.Graphics.FillRectangle(Brushes.Black, 0, 0, e.ClipRectangle.Width, DrawRect.Height);
                        using (SolidBrush Br = new SolidBrush(DrawColor))
                        {
                            //Draw our question text
                            e.Graphics.DrawString(TextToDraw, MM_Repository.Training.QuestionFont, Br, TrainingRect, sF);

                            //Draw our score in the top-left
                            sF.Alignment = StringAlignment.Near;
                            e.Graphics.DrawString("Score: " + MM_Repository.Training.Score.ToString(), MM_Repository.Training.QuestionFont, Br, TrainingRect);

                            //If we're in question mode, show our countdown timer
                            if (MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.QuestionAsked)
                            {
                                sF.Alignment = StringAlignment.Far;
                                double TimeLeft = MM_Repository.Training.CurrentLevel.QuestionTimeout - MM_Repository.Training.TimeSincePresentation;
                                e.Graphics.DrawString("Time: " + TimeLeft.ToString("0"), MM_Repository.Training.QuestionFont, Br, TrainingRect, sF);
                            }

                            //If we have the incorrect answer, draw accordingly.
                            if (MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.AnswerCorrect || MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.AnswerWrong)
                            {
                                sF.Alignment = StringAlignment.Center;
                                bool Correct = MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.AnswerCorrect;

                                SizeF AnswerRect = g.MeasureString(MM_Repository.Training.AnswerText, MM_Repository.Training.AnswerFont);
                                e.Graphics.FillRectangle(Brushes.Black, 0, TrainingRect.Bottom, e.ClipRectangle.Width, AnswerRect.Height);

                                using (SolidBrush DrawBrush = new SolidBrush(Correct ? MM_Repository.Training.CorrectAnswerColor : MM_Repository.Training.IncorrectAnswerColor))
                                    e.Graphics.DrawString(MM_Repository.Training.AnswerText, MM_Repository.Training.AnswerFont, DrawBrush, new RectangleF(0, TrainingRect.Bottom, e.ClipRectangle.Width, TrainingRect.Height), sF);

                                Point CorrectAnswerXY = Point.Subtract(MM_Coordinates.LatLngToXY(MM_Repository.Training.CorrectAnswer, Coordinates.ZoomLevel), new Size(Coordinates.TopLeftXY));

                                if (!MM_Repository.Training.UserAnswer.IsEmpty)
                                {
                                    Point UserAnswerXY = Point.Subtract(MM_Coordinates.LatLngToXY(MM_Repository.Training.UserAnswer, Coordinates.ZoomLevel), new Size(Coordinates.TopLeftXY));
                                    using (Pen DrawPen = new Pen(Color.Black, 5f))
                                        e.Graphics.DrawLine(DrawPen, UserAnswerXY, CorrectAnswerXY);
                                    using (Pen DrawPen = new Pen(Color.White, 2f))
                                    {
                                        e.Graphics.DrawLine(DrawPen, UserAnswerXY, CorrectAnswerXY);
                                        DrawCrosshair(DrawPen, UserAnswerXY, 10, e.Graphics, "");
                                    }
                                }

                                using (Pen DrawPen = new Pen((Correct ? Color.Green : Color.Red), 3))
                                    if (MM_Repository.Training.TargetElement is MM_Substation)
                                        DrawCrosshair(DrawPen, CorrectAnswerXY, 20, e.Graphics, ((MM_Substation)MM_Repository.Training.TargetElement).LongName);
                                    else
                                        DrawCrosshair(DrawPen, CorrectAnswerXY, 20, e.Graphics, MM_Repository.Training.TargetElement.Name);
                            }

                        }
                    }

                /*                else if (MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.QuestionAsked)
                                {
                                }
                                else if (MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.AnswerCorrect)
                                {
                                }*/
            }
            catch (Exception ex)
            {
                Program.LogError(ex);
            }
            FPS++;
            RenderState = enumRenderState.Ready;
        }

        /// <summary>
        /// Draw a crosshair of our target size
        /// </summary>
        /// <param name="DrawPen"></param>
        /// <param name="Location"></param>
        /// <param name="Radius"></param>
        /// <param name="g"></param>
        /// <param name="TextToDraw"></param>
        private void DrawCrosshair(Pen DrawPen, Point Location, int Radius, Graphics g, String TextToDraw)
        {
            //Fill an elipse 5 points bigger to highlight everything
            g.FillEllipse(Brushes.Black, Location.X - Radius - 5, Location.Y - Radius - 5, Radius + Radius + 10, Radius + Radius + 10);

            g.DrawEllipse(DrawPen, Location.X - Radius, Location.Y - Radius, Radius + Radius, Radius + Radius);
            g.DrawLine(DrawPen, Location.X, Location.Y - Radius, Location.X, Location.Y + Radius);
            g.DrawLine(DrawPen, Location.X - Radius, Location.Y, Location.X + Radius, Location.Y);

            //Now, if requested, show the text to draw
            if (!String.IsNullOrEmpty(TextToDraw))
            {
                SizeF StringSize = g.MeasureString(TextToDraw, MM_Repository.OverallDisplay.KeyIndicatorLabelFont);
                g.FillRectangle(Brushes.Black, Location.X - (StringSize.Width / 2f), Location.Y + Radius + 5, StringSize.Width, StringSize.Height);
                g.DrawString(TextToDraw, MM_Repository.OverallDisplay.KeyIndicatorLabelFont, Brushes.White, Location.X - (StringSize.Width / 2f), Location.Y + Radius + 5);
            }
        }

        /// <summary>
        /// Check a line after its value has changed
        /// </summary>
        /// <param name="Line"></param>
        private void Line_ValueChanged(MM_Element Line)
        {
            MM_Line UpdatedLine = Line as MM_Line;
            MM_GDI_Line GLine;
            if (!Lines.TryGetValue(UpdatedLine, out GLine))
                return;

            GLine.Visible = false;
            GLine.ViolationVisible = false;
            CheckLineVisibility(new KeyValuePair<MM_Line, MM_GDI_Line>(UpdatedLine, GLine));
        }

        /// <summary>
        /// Determine whether a line should be visible.
        /// </summary>
        /// <param name="Line"></param>
        private void CheckLineVisibility(KeyValuePair<MM_Line, MM_GDI_Line> Line)
        {
            //Don't try and render invalid lines
            if (Line.Value.LineImage == null)
                return;


            //Determine whether the line is visible, and if not, go no firther.
            if (!Line.Key.CheckVisibility(Coordinates))
                return;
            else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonBlackstartElements && Line.Key.IsBlackstart == false)
            { Line.Value.Visible = false; return; }
            else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonOperatorElements && Line.Key.Operator != MM_Repository.OverallDisplay.DisplayCompany)
            { Line.Value.Visible = false; return; }
            else if (!Line.Key.KVLevel.ShowEnergized && (Line.Key.LineEnergizationState == Line.Key.KVLevel.Energized))
            { Line.Value.Visible = false; return; }
            else if (!Line.Key.KVLevel.ShowUnknown && (Line.Key.LineEnergizationState == Line.Key.KVLevel.Unknown))
            { Line.Value.Visible = false; return; }
            else if (!Line.Key.KVLevel.ShowDeEnergized && (Line.Key.LineEnergizationState == Line.Key.KVLevel.DeEnergized))
            { Line.Value.Visible = false; return; }
            else if (!Line.Key.KVLevel.ShowPartiallyEnergized && (Line.Key.LineEnergizationState == Line.Key.KVLevel.PartiallyEnergized))
            { Line.Value.Visible = false; return; }
            else if (Line.Key.KVLevel.VisibilityThreshold > 0f && Line.Key.KVLevel.VisibilityThreshold < Line.Key.LinePercentage)
            { Line.Value.Visible = false; return; }
            else if (Line.Key.KVLevel.VisibilityThreshold < 0f && -Line.Key.KVLevel.VisibilityThreshold > Line.Key.LinePercentage)
            { Line.Value.Visible = false; return; }
            else
            {
                Line.Value.Visible = true;
                foreach (MM_Substation Sub in Line.Key.ConnectedStations)
                    if (Substations.ContainsKey(Sub))
                        Substations[Sub].ConnectorVisible = true;
            }

        }

        /// <summary>
        /// Check to see whether a substation should be drawn. If so, draw it.
        /// </summary>
        /// <param name="Substation">The substation to test</param>
        /// <param name="g">The graphics connector</param>
        private void CheckDrawSubstation(KeyValuePair<MM_Substation, MM_GDI_Substation> Substation, Graphics g)
        {
            MM_AlarmViolation_Type WorstViolation = null;
            if (Substation.Value.Visible)
                Substation.Value.DrawSubstation(Substation.Key, Coordinates, g, violViewer.ShownViolations, this, IsBlinking);
            else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonBlackstartElements)
                return;
            else if (float.IsNaN(Substation.Key.LatLong.X))
                return;
            else if (MM_Repository.SubstationDisplay.ShowAutoTransformersOut && Substation.Value.OutagedTransformer)
            {
                Substation.Value.DrawSubstation(Substation.Key, Coordinates, g, violViewer.ShownViolations, this, IsBlinking);
                Substation.Value.ViolationVisible = true;
            }
            else if (MM_Repository.SubstationDisplay.ShowFrequencyControl && Substation.Key.FrequencyControlStatus != MM_Substation.enumFrequencyControlStatus.None)
            {
                Substation.Value.DrawSubstation(Substation.Key, Coordinates, g, violViewer.ShownViolations, this, IsBlinking);
                Substation.Value.ViolationVisible = true;
            }
            else if (((WorstViolation = Substation.Key.WorstVisibleViolation(violViewer.ShownViolations, this)) != null) && (WorstViolation.NetworkMap_Substation != MM_AlarmViolation_Type.DisplayModeEnum.Never) && Substation.Key.CheckVisibility(Coordinates))
            {
                Substation.Value.DrawSubstation(Substation.Key, Coordinates, g, violViewer.ShownViolations, this, IsBlinking);
                Substation.Value.ViolationVisible = true;
            }
            else if (Substation.Value.ConnectorVisible && MM_Repository.SubstationDisplay.ShowSubsOnLines && (Coordinates.ZoomLevel >= MM_Repository.OverallDisplay.StationNames))
            {
                Substation.Value.DrawSubstation(Substation.Key, Coordinates, g, violViewer.ShownViolations, this, IsBlinking);
                Substation.Value.Visible = true;
            }
        }

        /// <summary>
        /// Check to see whether a substation should be drawn. If so, draw it.
        /// </summary>
        /// <param name="Line">The line to test</param>
        /// <param name="g">The graphics connector</param>
        private void CheckDrawLine(KeyValuePair<MM_Line, MM_GDI_Line> Line, Graphics g)
        {

            MM_AlarmViolation_Type WorstViolation = null;
            if (Line.Value.Visible)
                try
                {
                    DrawLine(Line, g);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error drawing line {0}: {1}", Line.Key.Name, ex.Message);
                }
            else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonBlackstartElements)
                return;
            else if ((WorstViolation = Line.Key.WorstVisibleViolation(violViewer.ShownViolations, this)) != null)
                if (WorstViolation.NetworkMap_Line != MM_AlarmViolation_Type.DisplayModeEnum.Never)
                    if (Line.Key.CheckVisibility(Coordinates))
                    {
                        Line.Value.ViolationVisible = true;
                        DrawLine(Line, g);
                    }
        }



        /// <summary>
        /// If the appropriate criteria are met, draw the line
        /// </summary>
        /// <param name="Line">the line to be drawn</param>
        /// <param name="g">The GDI+ graphics connector</param>        
        private void DrawLine(KeyValuePair<MM_Line, MM_GDI_Line> Line, Graphics g)
        {
            //Now, draw the line.
            Dictionary<MM_AlarmViolation, ListViewItem> Viols = violViewer.ShownViolations;
            Line.Value.DrawLine(Line.Key, g, Viols, this, IsBlinking);


            //Flag our connected subs as being visible
            foreach (MM_Substation Sub in Line.Key.ConnectedStations)
                if (Substations.ContainsKey(Sub))
                    Substations[Sub].ConnectorVisible = true;

            //If we're within the zoom level, and our line meets our criteria, draw the line flow.
            lock (Line.Value)
                if (Coordinates.ZoomLevel >= MM_Repository.OverallDisplay.LineFlows)
                    if (Line.Key.KVLevel.ShowMVA && (100f * Line.Key.LinePercentage >= Line.Key.KVLevel.MVAThreshold))
                        Line.Value.DrawFlowLine(g, Line.Key, Viols, this, IsBlinking, Coordinates.ZoomLevel);

            //If we're within the zoom level, draw the requested line data
            if (Coordinates.ZoomLevel >= MM_Repository.OverallDisplay.LineText)
                Line.Value.DrawLineText(Line.Key, g, Coordinates);
        }


        /// <summary>
        /// Determine whether a substation should be visible
        /// </summary>
        /// <param name="Substation"></param>
        private void CheckSubstationVisibility(KeyValuePair<MM_Substation, MM_GDI_Substation> Substation)
        {

            //First, check to see if we have lat/long and permission
            if (float.IsNaN(Substation.Key.LatLong.X) || !Substation.Key.Permitted)
                Substation.Value.Visible = false;
            else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonBlackstartElements && Substation.Key.IsBlackstart == false)
                Substation.Value.Visible = false;
            else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonOperatorElements && Array.IndexOf(Substation.Key.Operators, MM_Repository.OverallDisplay.DisplayCompany) == -1)
                Substation.Value.Visible = false;

            //Otherwise, check our other types
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.None)
                Substation.Value.Visible = false;
            else if ((MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Capacitors || MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.CapacitorsAvailable || MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.CapacitorsOnline) && !Substation.Key.ElemTypes.Contains(MM_Repository.FindElementType("Capacitor")))
                Substation.Value.Visible = false;
            else if ((MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Reactors || MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.ReactorsAvailable || MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.ReactorsOnline) && !Substation.Key.ElemTypes.Contains(MM_Repository.FindElementType("Reactor")))
                Substation.Value.Visible = false;
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Loads && !Substation.Key.ElemTypes.Contains(MM_Repository.FindElementType("Load")))
                Substation.Value.Visible = false;
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.LAARs && !Substation.Key.ElemTypes.Contains(MM_Repository.FindElementType("LAAR")))
                Substation.Value.Visible = false;
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Transformers && !Substation.Key.ElemTypes.Contains(MM_Repository.FindElementType("Transformer")))
                Substation.Value.Visible = false;
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Units && !Substation.Key.ElemTypes.Contains(MM_Repository.FindElementType("Unit")))
                Substation.Value.Visible = false;
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.CapacitorsAvailable)
            {
                foreach (MM_ShuntCompensator Shunt in Substation.Key.ShuntCompensators)
                    if (Shunt.ElemType.Name == "Capacitor" && Shunt.Open)
                        if (Substation.Value.Visible = Substation.Key.CheckVisibility(Coordinates))
                            return;
                Substation.Value.Visible = false;
            }
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.CapacitorsOnline)
            {
                foreach (MM_ShuntCompensator Shunt in Substation.Key.ShuntCompensators)
                    if (Shunt.ElemType.Name == "Capacitor" && !Shunt.Open)
                        if (Substation.Value.Visible = Substation.Key.CheckVisibility(Coordinates))
                            return;
                Substation.Value.Visible = false;
            }
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.ReactorsAvailable)
            {
                foreach (MM_ShuntCompensator Shunt in Substation.Key.ShuntCompensators)
                    if (Shunt.ElemType.Name == "Reactor" && Shunt.Open)
                        if (Substation.Value.Visible = Substation.Key.CheckVisibility(Coordinates))
                            return;
                Substation.Value.Visible = false;
            }
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.ReactorsOnline)
            {
                foreach (MM_ShuntCompensator Shunt in Substation.Key.ShuntCompensators)
                    if (Shunt.ElemType.Name == "Reactor" && !Shunt.Open)
                        if (Substation.Value.Visible = Substation.Key.CheckVisibility(Coordinates))
                            return;
                Substation.Value.Visible = false;
            }
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.StaticVarCompensators)
                Substation.Value.Visible = Substation.Key.ElemTypes.Contains(MM_Repository.FindElementType("StaticVarCompensator"));
            else
                Substation.Value.Visible = Substation.Key.CheckVisibility(Coordinates);
        }



        /// <summary>
        /// Draw the state and/or county boundary
        /// </summary>
        /// <param name="Boundary">The boundary to be drawn</param>        
        /// <param name="g">The graphics connector</param>        
        private void DrawCounty(KeyValuePair<MM_Boundary, MM_GDI_County> Boundary, Graphics g)
        {

            if (MM_Repository.OverallDisplay.Contour == MM_Display.MM_Contour_Enum.BusVoltage)
            {
                float WorstPu = Boundary.Key.Average_pU;
                if (!float.IsNaN(WorstPu) && (WorstPu >= 1f + (MM_Repository.OverallDisplay.ContourThreshold / 100f) || WorstPu <= 1f - (MM_Repository.OverallDisplay.ContourThreshold / 100f)))
                    using (SolidBrush OutBrush = new SolidBrush(RelativeColor(WorstPu - 1f)))
                        g.FillPath(OutBrush, Boundary.Value.CountyPath);
            }
            else if (MM_Repository.OverallDisplay.Contour == MM_Display.MM_Contour_Enum.BusAngle)
            {
                float AngleStdDev = Boundary.Key.AngleStdDev / 100f;
                if (!float.IsNaN(AngleStdDev) && (AngleStdDev >= (MM_Repository.OverallDisplay.ContourThreshold / 100f) || AngleStdDev <= (MM_Repository.OverallDisplay.ContourThreshold / 100f)))
                    using (SolidBrush OutBrush = new SolidBrush(RelativeColor(AngleStdDev)))
                        g.FillPath(OutBrush, Boundary.Value.CountyPath);
            }
            else if (MM_Repository.OverallDisplay.Contour == MM_Display.MM_Contour_Enum.LMPStdDev)
            {
                float LMPStdDev = Boundary.Key.LMPStdDev;
                if (!float.IsNaN(LMPStdDev) && (LMPStdDev >= (MM_Repository.OverallDisplay.ContourThreshold / 100f) || LMPStdDev <= (MM_Repository.OverallDisplay.ContourThreshold / 100f)))
                    using (SolidBrush OutBrush = new SolidBrush(RelativeColor(LMPStdDev)))
                        g.FillPath(OutBrush, Boundary.Value.CountyPath);
            }
            else if (MM_Repository.OverallDisplay.Contour == MM_Display.MM_Contour_Enum.WeatherZones)
            {
                List<MM_Zone> WeatherZones = new List<MM_Zone>();
                foreach (MM_Substation Sub in Boundary.Key.Substations)
                    if (Sub.WeatherZone != null && !WeatherZones.Contains(Sub.WeatherZone))
                        WeatherZones.Add(Sub.WeatherZone);
                int R = 0, G = 0, B = 0;
                foreach (MM_Zone WeatherZone in WeatherZones)
                {
                    R += WeatherZone.DrawColor.R;
                    G += WeatherZone.DrawColor.G;
                    B += WeatherZone.DrawColor.B;
                }
                if (WeatherZones.Count > 0)
                    using (SolidBrush OutBrush = new SolidBrush(Color.FromArgb(R / WeatherZones.Count, G / WeatherZones.Count, B / WeatherZones.Count)))
                        g.FillPath(OutBrush, Boundary.Value.CountyPath);
            }
            else if (MM_Repository.OverallDisplay.Contour == MM_Display.MM_Contour_Enum.None)
            { }
            else
            {
                float Min = float.NaN, Max = float.NaN, Sum = 0f, TallyCount = 0f;
                //Draw small circles for each substation containing a unit
                foreach (MM_Substation Sub in Boundary.Key.Substations)
                    if (Sub.Units != null)
                    {
                        foreach (MM_Unit Unit in Sub.Units)
                            if (Unit.Permitted && !float.IsNaN(Unit.LMP))
                            {
                                Sum += Unit.LMP;
                                if (float.IsNaN(Min) || Unit.LMP < Min)
                                    Min = Unit.LMP;
                                if (float.IsNaN(Max) || Unit.LMP > Max)
                                    Max = Unit.LMP;
                                TallyCount++;

                                float AvgLMP = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.AverageLMP];
                                float LMPSpread = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.LMPSpread];

                                //Find the substation in question
                                MM_GDI_Substation GDISub = Substations[Sub];
                                Color DrawColor = Color.Transparent;
                                if (MM_Repository.OverallDisplay.Contour == MM_Display.MM_Contour_Enum.LMPAverage)
                                    DrawColor = RelativeColor(((Sum / TallyCount) - AvgLMP) / LMPSpread);
                                else if (MM_Repository.OverallDisplay.Contour == MM_Display.MM_Contour_Enum.LMPMaximum)
                                    DrawColor = RelativeColor((Max - AvgLMP) / LMPSpread);
                                else if (MM_Repository.OverallDisplay.Contour == MM_Display.MM_Contour_Enum.LMPMinimum)
                                    DrawColor = RelativeColor((Min - AvgLMP) / LMPSpread);

                                using (Brush DrawBrush = new SolidBrush(DrawColor))
                                    g.FillEllipse(DrawBrush, GDISub.SubstationCenter.X - 4, GDISub.SubstationCenter.Y - 4, 8, 8);

                            }
                    }
            }

        }



        /// <summary>
        /// Return a color corresponding to the value (0 is average)
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        private Color RelativeColor(float Value)
        {
            if (float.IsNaN(Value))
                return Color.Transparent;
            else if (Value > 0)
                return Color.FromArgb((int)Math.Min(MM_Repository.OverallDisplay.ContourBrightness * Value, 255f), 255, 0, 0);
            else
                return Color.FromArgb((int)Math.Min(MM_Repository.OverallDisplay.ContourBrightness * -Value, 255f), 0, 0, 255);
            //return Color.FromArgb(128, OtherBase, OtherBase, (int)Math.Min(MM_Repository.OverallDisplay.ContourBrightness * -Value, 255f));
        }

        /// <summary>
        /// Draw all lasso points
        /// </summary>
        /// <param name="LassoPoints"></param>
        /// <param name="g"></param>
        private void DrawLassoPoints(List<PointF> LassoPoints, Graphics g)
        {
            //First, turn our lasso points into screen points
            PointF[] OutPts = new PointF[LassoPoints.Count];
            for (int a = 0; a < LassoPoints.Count; a++)
                OutPts[a] = MM_Coordinates.LatLngToXY(LassoPoints[a], Coordinates.ZoomLevel);
            g.FillPolygon(Coordinates.LassoBrush, OutPts);
            //g.DrawLines(Coordinates.LassoPen, OutPts);                
        }


        /// <summary>
        /// Draw a lasso on the screen
        /// </summary>
        /// <param name="Start">The first coordinate for the lasso</param>
        /// <param name="End">The last coordinate for the lasso</param>
        /// <param name="g">The graphics connector</param>
        private void DrawLasso(PointF Start, PointF End, Graphics g)
        {
            Point LassoStart = MM_Coordinates.LatLngToXY(Start, Coordinates.ZoomLevel);
            Point LassoEnd = MM_Coordinates.LatLngToXY(End, Coordinates.ZoomLevel);
            //PointF LassoStart = Coordinates.ConvertPoint(Start);
            //PointF LassoEnd = Coordinates.ConvertPoint(End);
            if (LassoStart.X > LassoEnd.X)
            {
                int TempX = LassoStart.X;
                LassoStart.X = LassoEnd.X;
                LassoEnd.X = TempX;
            }
            if (LassoStart.Y > LassoEnd.Y)
            {
                int TempY = LassoStart.Y;
                LassoStart.Y = LassoEnd.Y;
                LassoEnd.Y = TempY;
            }
            RectangleF OutRect = new RectangleF(LassoStart.X, LassoStart.Y, LassoEnd.X - LassoStart.X, LassoEnd.Y - LassoStart.Y);

            g.FillRectangle(Coordinates.LassoBrush, OutRect);
            g.DrawRectangle(Pens.White, OutRect.X, OutRect.Y, OutRect.Width, OutRect.Height);
        }

        /// <summary>
        /// Determine whether a line or substation is visible
        /// </summary>
        /// <param name="Element"></param>
        /// <returns></returns>
        public override bool IsVisible(MM_Element Element)
        {
            if (Element is MM_Substation)
                if (Substations.ContainsKey(Element as MM_Substation))
                {
                    MM_GDI_Substation FoundSub = Substations[Element as MM_Substation];
                    return FoundSub.Visible || FoundSub.ViolationVisible;
                }
                else
                    return false;
            else if (Element is MM_Line)
            {
                MM_GDI_Line FoundLine;
                if ((Element as MM_Line).IsSeriesCompensator)
                {
                    MM_GDI_Substation FoundSub = Substations[(Element as MM_Line).Substation1];
                    return FoundSub.Visible || FoundSub.ViolationVisible;
                }
                else if (Lines.TryGetValue(Element as MM_Line, out FoundLine))
                    return FoundLine.ViolationVisible || FoundLine.Visible;
                else
                    return false;
            }
            else
                return true;
        }

        /// <summary>
        /// Determine whether a line or substation's connector is visible.
        /// </summary>
        /// <param name="Element"></param>
        /// <returns></returns>
        public override bool IsConnectorVisible(MM_Element Element)
        {
            if (Element is MM_Substation)
                return Substations[Element as MM_Substation].ConnectorVisible;
            else
                return false;
        }

        #endregion

        #region Mouse/keyboard handling
        /// <summary>
        /// Handle the left and mouse button down events
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {//MN//20130607 catch here verified for top level catch
            //if (!LockForDataUpdate)
            //{

            MousePos = Cursor.Position;
            if (MM_Repository.Training != null && MM_Repository.Training.TrainingMode != Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.NotTraning)
            { }
            else if (e.Button == MouseButtons.Right && ShiftDown && Data_Integration.Permissions.LassoStandard)
                Coordinates.LassoPoints.Add(Coordinates.UnconvertPoint(e.Location));
            else if (e.Button == MouseButtons.Right && Data_Integration.Permissions.LassoStandard)
                Coordinates.LassoStart = Coordinates.UnconvertPoint(e.Location);
            else if (e.Button == MouseButtons.Middle)
                Cursor = Cursors.NoMove2D;
            else if (Key_Indicators.DeltaDisplay.Visible)
            {
                //Check to see which element we found
                Point PixelLocation = new Point(e.X + Coordinates.TopLeftXY.X, e.Y + Coordinates.TopLeftXY.Y);
                List<MM_Element> Elems = HitTest(PixelLocation);
                if (Elems.Count >= 1)
                    UpdatingElement = Key_Indicators.DeltaDisplay.HandleElementSelection(Elems[0], Coordinates, PixelLocation, ref ActivelyMovingElement);
                else
                {
                    UpdatingElement = null;
                    ActivelyMovingElement = null;
                }
            }
            HandlingMouse = true;
            base.OnMouseDown(e);
            //}
        }



        /// <summary>
        /// Handle left and right mouse movement
        /// </summary>        
        /// <param name="e"></param>    
        protected override void OnMouseMove(MouseEventArgs e)
        {//MN//20130607 catch here verified for top level catch
            if (!HandlingMouse)// || LockForDataUpdate)
                return;
            else if (MM_Repository.Training != null && MM_Repository.Training.TrainingMode != Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.NotTraning)
            { }
            else if (e.Button == MouseButtons.Left)
            {
                if (UpdatingElement == null)
                    Coordinates.UpdateFromXY(new Point(Coordinates.TopLeftXY.X - Cursor.Position.X + MousePos.X, Coordinates.TopLeftXY.Y - Cursor.Position.Y + MousePos.Y));
                else if (UpdatingElement.BaseElement is MM_Substation)
                {
                    PointF NewPos = MM_Coordinates.XYToLatLng(new Point(Coordinates.TopLeftXY.X + e.X, Coordinates.TopLeftXY.Y + e.Y), Coordinates.ZoomLevel);
                    foreach (MM_Line LineToUpdate in UpdatingElement.UpdateSubstationCoordinates(NewPos))
                        UpdateLineInformation(LineToUpdate, false);
                    UpdateSubstationInformation(UpdatingElement.BaseElement as MM_Substation);
                    Key_Indicators.DeltaDisplay.UpdateElement(UpdatingElement.BaseElement);
                }
                else if (UpdatingElement.BaseElement is MM_Line)
                {
                    Point NewPosXY  = new Point(Coordinates.TopLeftXY.X + e.X, Coordinates.TopLeftXY.Y + e.Y);
                    PointF NewPos = MM_Coordinates.XYToLatLng(NewPosXY, Coordinates.ZoomLevel);
                    UpdatingElement.UpdateLineCoordinates(NewPos, NewPosXY);
                    UpdateLineInformation(UpdatingElement.BaseElement as MM_Line, false);
                    //Key_Indicators.DeltaDisplay.UpdateElement(UpdatingElement.BaseElement);
                }
                MousePos = Cursor.Position;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                PanPoint = new Point(Cursor.Position.X - MousePos.X, Cursor.Position.Y - MousePos.Y);

                //Now, based on the pan point, set the cursor.
                PointF CurPoint;
                if (PanPoint.X == 0 && PanPoint.Y == 0)
                    CurPoint = PointF.Empty;
                else if (Math.Abs(PanPoint.X) > Math.Abs(PanPoint.Y))
                    CurPoint = new PointF(Math.Sign(PanPoint.X), PanPoint.Y / Math.Abs(PanPoint.X));
                else
                    CurPoint = new PointF(PanPoint.X / Math.Abs(PanPoint.Y), Math.Sign(PanPoint.Y));


                Cursor = MM_Repository.PanCursors[Math.Sign(Math.Round(CurPoint.Y)) + 1, Math.Sign(Math.Round(CurPoint.X)) + 1];
            }
            else if (e.Button == MouseButtons.Right && ShiftDown && Data_Integration.Permissions.LassoStandard)
            {
                Coordinates.AddLassoPoint(Coordinates.UnconvertPoint(e.Location));
                MousePos = Cursor.Position;
                this.Invalidate();
            }
            else if (e.Button == MouseButtons.Right && Data_Integration.Permissions.LassoStandard)
            {
                Coordinates.LassoEnd = Coordinates.UnconvertPoint(e.Location);
                MousePos = Cursor.Position;
                /*
                if ((e.Location.X <= 0) && (e.Location.Y <= 0))
                    Coordinates.TopLeft = Coordinates.LassoEnd;
                else if (e.Location.X <= 0)
                    Coordinates.TopLeft = new PointF(Coordinates.LassoEnd.X, Coordinates.TopLeft.Y);
                else if (e.Location.Y <= 0)
                    Coordinates.TopLeft = new PointF(Coordinates.TopLeft.X, Coordinates.LassoEnd.Y);
                else if ((e.Location.X >= DisplayRectangle.Width - 4) && (e.Location.Y >= DisplayRectangle.Bottom))
                    Coordinates.BottomRight = Coordinates.LassoEnd;
                else if (e.Location.X >= DisplayRectangle.Width - 4)
                    Coordinates.BottomRight = new PointF(Coordinates.LassoEnd.X, Coordinates.BottomRight.Y);
                else if (e.Location.Y >= DisplayRectangle.Bottom)
                    Coordinates.BottomRight = new PointF(Coordinates.BottomRight.X, Coordinates.LassoEnd.Y);
                 */
                this.Invalidate();
            }
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Handle the left and right mouse up events
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {           //MN//20130607 catch here verified for top level catch
            //if (!LockForDataUpdate)
            //{
            UpdatingElement = null;
            if (MM_Repository.Training == null || MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.NotTraning)
                if (e.Button == MouseButtons.Left)
                    MapMoved = true;
                else if (e.Button == MouseButtons.Middle)
                    MapMoved = true;
                else if (e.Button == MouseButtons.Right)
                    if (Coordinates.LassoMultiplePoints && Data_Integration.Permissions.LassoStandard)
                    {
                        List<PointF> LassoPoints = new List<PointF>(Coordinates.LassoPoints);
                        Form_Builder.Lasso_Display(LassoPoints, this, Coordinates.ControlDown);
                    }
                    else if (Coordinates.LassoEnd.IsEmpty || Coordinates.LassoStart == Coordinates.LassoEnd)
                        HitTestAndPopup(e.Location);
                    else if (Data_Integration.Permissions.LassoStandard)
                        Form_Builder.Lasso_Display(Coordinates.LassoStart, Coordinates.LassoEnd, this, Coordinates.ControlDown);

            Cursor = Cursors.Default;
            MousePos = Point.Empty;
            Coordinates.LassoStart = Point.Empty;
            Coordinates.LassoEnd = Point.Empty;
            PanPoint = Point.Empty;
            Coordinates.ControlDown = false;
            this.Invalidate();
            HandlingMouse = false;
            base.OnMouseUp(e);

            //If we're in training mode, handle accordingly.
            if (MM_Repository.Training != null)
                if (MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.QuestionAsked)
                    MM_Repository.Training.HandleResponse(Coordinates.UnconvertPoint(e.Location));
                else if (MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.AnswerCorrect || MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.AnswerWrong)
                    MM_Repository.Training.NextQuestion();
                else if (MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.UserWon || MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.UserFailed)
                {
                    MM_Repository.Training.TrainingMode = Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.NotTraning;
                    (ParentForm as MacomberMap).ctlNetworkMap.ResetDisplayCoordinates();
                }
        }

        /// <summary>
        /// Handle the map's losing focus
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(EventArgs e)
        {//MN//20130607 ooo.... this is good, perhaps use this type of event? else just leave alone?
            HandlingMouse = false;
            Coordinates.ControlDown = false;
            ShiftDown = false;
            base.OnLostFocus(e);
        }


        /// <summary>
        /// Check to see whether a user has clicked on a substation or line
        /// </summary>
        /// <param name="MousePosition">The position of the mouse</param>
        private void HitTestAndPopup(Point MousePosition)
        {
            Point PixelLocation = new Point(MousePosition.X + Coordinates.TopLeftXY.X, MousePosition.Y + Coordinates.TopLeftXY.Y);
            List<MM_Element> Elements = HitTest(PixelLocation);
            if (Elements.Count > 0)
                PopupMenu(Elements, MousePosition);
            else
                //Search through the boundaries, and find any valid boundaries
                foreach (KeyValuePair<MM_Boundary, MM_GDI_County> County in Counties)
                    if (County.Value.Visible && County.Value.CountyPath.IsVisible(PixelLocation))
                    {
                        RightClickMenu.Show(this, MousePosition, County.Key, false);
                        return;
                    }
        }

        /// <summary>
        /// Hit test the map to find substations and/or lines that fit our coordinates
        /// </summary>
        /// <param name="PixelLocation"></param>
        /// <returns></returns>
        private List<MM_Element> HitTest(Point PixelLocation)
        {

            List<MM_Element> Elements = new List<MM_Element>();
            //See if we have any substation matches
            foreach (KeyValuePair<MM_Substation, MM_GDI_Substation> Substation in Substations)
                if ((Substation.Value.Visible || Substation.Value.ViolationVisible) && Substation.Value.HitTest(PixelLocation, IsBlinking, Coordinates.ZoomLevel / 3f, Substation.Key.Units != null))
                    Elements.Add(Substation.Key);
            if (Elements.Count > 0)
                return Elements;
            else
                //Look through the lines, and show any valid lines
                foreach (KeyValuePair<MM_Line, MM_GDI_Line> Line in Lines)
                    if ((Line.Value.Visible || Line.Value.ViolationVisible) && Line.Value.LineImage != null && Line.Value.LineImage.IsOutlineVisible(PixelLocation, HitTestPen))
                        Elements.Add(Line.Key);
            return Elements;
        }

        /// <summary>
        /// Popup a menu offering a list of elements to be shown
        /// </summary>
        /// <param name="ElemToPopup"></param>
        /// <param name="MousePosition">The current mouse position</param>
        private void PopupMenu(List<MM_Element> ElemToPopup, Point MousePosition)
        {//MN//20130607 catch here verified for top level catch
            //if (!LockForDataUpdate)
            //{
            //Set up our menu for display
            RightClickMenu.Tag = ElemToPopup;
            RightClickMenu.Items.Clear();
            if (ElemToPopup.Count == 1)
                RightClickMenu.Show(this, MousePosition, ElemToPopup[0], false);
            else
                RightClickMenu.Show(this, MousePosition, ElemToPopup, false);
            //}
        }



        /// <summary>
        /// Handle mouse wheel
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //MN//20130607 catch here verified for top level catch
            if (!LockForDataUpdate)
            {
                //If the user is moving the mouse wheel and they can zoom, do it.
                Coordinates.UpdateZoom(Coordinates.ZoomLevel + Math.Sign(e.Delta), Coordinates.ZoomLevel, e.Location);
            }
        }

        /// <summary>
        /// Handle a key press event. Current keys include:
        /// r - reset the display to zoom all the way out to the state of Texas
        /// +/-  Zoom in and out
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            //MN//20130607 catch here verified for top level catch
            if (!LockForDataUpdate)
            {
                switch (Char.ToLower(e.KeyChar))
                {
                    case 'r': ResetDisplayCoordinates(); MapMoved = true;  break;
                    case '+': Coordinates.UpdateZoom(Coordinates.ZoomLevel + 1); break;
                    case '-': Coordinates.UpdateZoom(Coordinates.ZoomLevel - 1); break;
                }
                base.OnKeyPress(e);
            }

        }

        /// <summary>
        /// Pick up the control key being pressed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            //MN//20130607 catch here verified for top level catch
            //if (!LockForDataUpdate)
            //{
            if (Data_Integration.Permissions.LassoInvisible)
                Coordinates.ControlDown = e.Control;
            this.ShiftDown = e.Shift;
            base.OnKeyDown(e);
            //}
        }

        /// <summary>
        /// Pick up the control key being released
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (Data_Integration.Permissions.LassoInvisible)
                Coordinates.ControlDown = e.Control;
            this.ShiftDown = e.Shift;
            base.OnKeyUp(e);
        }
        #endregion
    }
}