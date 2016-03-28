using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Training;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using MacomberMapClient.Properties;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Menuing;
using MacomberMapClient.User_Interfaces.NetworkMap.Contours;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using MacomberMapClient.User_Interfaces.NetworkMap.Layers;
using MacomberMapClient.User_Interfaces.Summary;
using MacomberMapClient.User_Interfaces.Violations;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using Brush = SharpDX.Direct2D1.Brush;
using Color = System.Drawing.Color;
using DashStyle = System.Drawing.Drawing2D.DashStyle;
using Font = System.Drawing.Font;
using Point = System.Drawing.Point;
using RectangleF = SharpDX.RectangleF;
using Timer = System.Timers.Timer;
using System.Threading;

namespace MacomberMapClient.User_Interfaces.NetworkMap
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the main network map, which displays a wide area view
    /// From an architectural standpoint, the network map is designed to house all internal representations of components,
    /// so that multiple windows can be opened up simultaneously with different display characteristics
    /// </summary>
    public class MM_Network_Map_DX : Direct3DSurface
    {
        private readonly ToolTip _toolTip;
        private bool _allowToolTip = true;

        #region Initialization

        /// <summary>
        /// Initialize our network map control
        /// </summary>
        public MM_Network_Map_DX(MM_Coordinates Coordinates)
        {
            DCUpdateTotal= 0;
            _toolTip = new ToolTip
            {
                IsBalloon = false,
                AutoPopDelay = 0,
                AutomaticDelay = 0,
                InitialDelay = 0,
                ReshowDelay = 250,
                UseAnimation = false,
                UseFading = false,
            };

            GotFocus += (s, e) => _allowToolTip = true;
            this.Coordinates = Coordinates;

            BlinkTimer.Elapsed += new ElapsedEventHandler(BlinkTimer_Elapsed);

            //  contour = new MM_DX_Contouring(this);
#if DEBUG
            MM_Repository.OverallDisplay.FPS = true;
#endif

            IsDebug = MM_Repository.OverallDisplay.FPS;
        }

        #endregion

        /// <summary>
        /// Return the current mouse position upon request
        /// </summary>
        public PointF MouseLngLat
        {
            get
            {
                if (RectangleToScreen(ClientRectangle).Contains(Cursor.Position))
                    return Coordinates.XYToLngLat(PointToClient(Cursor.Position));
                else
                    return new PointF(float.NaN, float.NaN);
            }
        }


        public void UpdateLineInformation()
        {
            UpdateGraphicsPaths();
        }

        // private MM_DX_Contouring contour;
        // private KDTreeContour _kdTreeContour;

        #region DirectX Helpers and Cache

        private readonly Dictionary<string, StrokeStyle> _strokeStyles = new Dictionary<string, StrokeStyle>();


        public StrokeStyle GetStrokeStyle(Pen pen)
        {
            if (pen == null) pen = new Pen(Color.White);

            string key = string.Join("_", pen.DashCap, pen.DashOffset, pen.DashStyle, pen.EndCap, pen.LineJoin, pen.MiterLimit, pen.StartCap);
            if (pen.DashStyle == DashStyle.Custom)
                key += "_" + string.Join("-", pen.DashPattern);


            StrokeStyle style;

            if (!_strokeStyles.TryGetValue(key, out style))
            {
                style = pen.ToStrokeStyle(Factory2D);
                _strokeStyles[key] = style;
            }

            return style;
        }



        /// <summary>
        /// Renders a string using DirectX. 
        /// </summary>
        /// <param name="text">The text to layout.</param>
        /// <param name="textFormat">The font used.</param>
        /// <param name="maxHeight">Maximum height of the layout area.</param>
        /// <param name="maxWidth">Maximum width of the layout area.</param>
        /// <param name="brush">Brush used to render the text.</param>
        /// <param name="x">X Position.</param>
        /// <param name="y">Y Position.</param>
        /// <param name="brushBackground">Fill the background with a brush.</param>
        /// <param name="centerX">Center the TextLayout on the X position.</param>
        /// <param name="centerY">Center the TextLayout on the Y position.</param>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, Brush brush, TextFormat textFormat, float x, float y, float maxHeight, float maxWidth, Brush brushBackground = null, bool centerX = false, bool centerY = false)
        {
            using (var textLayout = new TextLayout(FactoryDirectWrite, text, textFormat, maxHeight, maxWidth))
            {
                float width = textLayout.Metrics.Width;
                float height = textLayout.Metrics.Height;

                if (centerX) x -= width / 2;
                if (centerY) y -= height / 2;

                if (brushBackground != null)
                {
                    RenderTarget2D.FillRectangle(new RawRectangleF(x, y, x + width, y + height), brushBackground);
                }

                RenderTarget2D.DrawTextLayout(new RawVector2 { X = x, Y = y }, textLayout, brush);
            }
        }

        #endregion

        #region Variable declarations

        /// <summary>Our last cursor position</summary>
        private Point _lastCursorPosition = Point.Empty;

        /// <summary>Whether our tooltip was shown</summary>
        private bool ShowedToolTip = false;

        /// <summary>Our state boundary</summary>
        public MM_Boundary State;

        /// <summary>Our state boundary path</summary>
        public PathGeometry StatePath;

        /// <summary>Our Image Attributes for drawing the map</summary>

        /// <summary>Whether the application is handling the mouse activity</summary>
        public bool HandlingMouse = false;

        /// <summary>The flag to ensure that operations do not occur with partially loaded data sets</summary>
        private static bool _LockForDataUpdate = false;

        //counter to ensure all DC_Tie related lines are taken care of once, to save operations

        /// <summary>The element currently being updated</summary>
        public MM_Coordinate_Suggestion UpdatingElement = null;

        /// <summary>The actively moving element</summary>
        public MM_Element ActivelyMovingElement = null;

        /// <summary>The current view on the network map</summary>
        // public MM_Coordinates Coordinates { get; }
        /// <summary>The x/y position of the left mouse press (for panning)</summary>
        internal Point MousePos = Point.Empty;

        /// <summary>The menu handling right-click actions</summary>
        internal MM_Popup_Menu RightClickMenu = new MM_Popup_Menu();

        /// <summary>Whether or not an ongoing zoom/pan action is occurring, requiring constant redraws</summary>
        public bool OngoingZoomPan;

        /// <summary>The timer that handles blinking events</summary>
        internal Timer BlinkTimer = new Timer(400);

        /// <summary>Whether or not the worst state should be shown (for handling blinking events)</summary>
        internal bool IsBlinking = false;

        /// <summary>The mini-map associated with the network map (for zoom/pan activity)</summary>

        /// <summary>Whether the map is in the midst of rendering</summary>
        internal bool IsRendering = false;

        /// <summary>Whether the control is resizing</summary>
        public bool Resizing = false;

        /// <summary>The number of pixels to shift the display every 100 ms</summary>
        internal Point PanPoint = Point.Empty;

        /// <summary>Whether the user is holding the shift key down while drawing a lasso</summary>
        internal bool ShiftDown = false;

        /// <summary>The violation viewer associated with the network map</summary>
        internal MM_Violation_Viewer violViewer;

        /// <summary>Debug timer, if requested</summary>
        internal Timer tmrFPS;

        #endregion

        #region Network map functions

        /// <summary>
        /// Handle the blink timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BlinkTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            IsBlinking ^= true;
            Invalidate();
        }


        /// <summary>
        /// Reset the network map
        /// </summary>
        public void ResetMap()
        {
            ResetDisplayCoordinates();
            UpdateGraphicsPaths(true);
        }


        /// <summary>Lock Interaction and Key operations during switch from data types //MN//20130607 added this function</summary>
        public static bool LockForDataUpdate
        {
            get { return _LockForDataUpdate; }
            set { _LockForDataUpdate = value; }
        }

        /// <summary>Lock Interaction and Key operations during switch from data types //MN//20130607 added this function</summary>
        public static int DCUpdateTotal { get; set; } 

        /// <summary>
        /// Set the controls and data sources associated with the network map
        /// </summary>
        /// <param name="violViewer">The violation viewer</param>
        /// <param name="miniMap">The mini-map to be associated with the network map</param>
        /// <param name="resetZoom">Whether to reset our zoom</param>
        public void SetControls(MM_Violation_Viewer violViewer, int resetZoom)
        {
            //MN//20130607 should I use this location to lock all zoom events??? not sure....
            //User_Interface_Components.GDIPlus.Network_Map_GDI.LockForDataUpdate = true;
            this.violViewer = violViewer;
            // MM_Repository.Violations.
            MM_Repository.OverallDisplay.MapTransparency = MM_Repository.OverallDisplay.MapTransparency;
            if (resetZoom == 0) //mn//
            {
                Coordinates.SetControls();
                Coordinates.Zoomed += Coordinates_Zoomed;
                Coordinates.Panned += Coordinates_Panned;
            }
            //Lines = new Dictionary<MM_Line, MM_GDI_Line>();
            //MultipleLines = new List<MM_GDI_MultipleLine>(); //Note – also, want to look at clearing rather than re-instantiating all of these. //mn//
            if (resetZoom == 0) //mn//
            {
                UpdateGraphicsPaths(true);
                BlinkTimer.Enabled = true;
                tmrFPS = new Timer(250);
                tmrFPS.Elapsed += tmrFPS_Elapsed;
                tmrFPS.Start();

                MM_Repository.ViewChanged += new MM_Repository.ViewChangedDelegate(Repository_ViewChanged);
                Data_Integration.NetworkSourceChanged += new EventHandler(Data_Integration_NetworkSourceChanged);
            }
        }

        /// <summary>
        /// Handle the elapsing of the FPS timer by resetting and doing background events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void tmrFPS_Elapsed(object sender, EventArgs e) { }

        private void CheckToolTipSafe()
        {
            if (InvokeRequired)
                Invoke(new Action(CheckToolTip));
            else
                CheckToolTip();
        }

        private void HideToolTip()
        {
            _toolTipElements.Clear();
            _toolTip.Hide(this);
            ShowedToolTip = false;
        }

        /// <summary>
        /// Check our tool tip
        /// </summary>
        private void CheckToolTip()
        {
            if (!_allowToolTip) return;

            //Also, while we're here, check position. If it's unchanged in the last second and tooltip never shown, do so.
            if (Cursor.Position != _lastCursorPosition)
            {
                _lastCursorPosition = Cursor.Position;
                _toolTipElements.Clear();
                ShowedToolTip = false;
            }

            if (!ShowedToolTip && Focused)
            {
                var MousePosition = PointToClient(_lastCursorPosition);
                var PixelLocation = new Vector2(MousePosition.X + Coordinates.TopLeftXY.X, MousePosition.Y + Coordinates.TopLeftXY.Y);
                var PixelPoint = new Point((int)PixelLocation.X, (int)PixelLocation.Y);


                var elements = HitTest(PixelPoint, true);

                // Remove any hidden elements
                if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideExternalElements)
                {
                    List<MM_Element> HiddenElements = new List<MM_Element>();
                    foreach (MM_Element elem in elements)
                        if (elem is MM_Substation && !((MM_Substation)elem).IsInternal)
                            HiddenElements.Add(elem);
                        else if (elem is MM_Line && !((MM_Line)elem).IsInternal)
                            HiddenElements.Add(elem);
                    
                    foreach (MM_Element HiddenElement in HiddenElements)
                        elements.Remove(HiddenElement);
                    //foreach (var externalElement in elements.Where(e => (!(e as MM_Substation)?.IsInternal ?? false) || !((e as MM_Line)?.IsInternal ?? false)).ToList())
                     //   elements.Remove(externalElement);
                }


                //Check our regions
                if (elements.Count == 0)
                {
                    var regionLayer = GetLayers<StateRegionsLayer>().FirstOrDefault();
                    if (regionLayer != null)
                    {
                        var hitRegions = regionLayer.HitTest(PixelLocation, 1, true);
                        elements.AddRange(hitRegions);
                    }
                }

                // If we have elements, build our tooltip text
                if (elements.Count > 0)
                {
                    var sB = new StringBuilder();
                    foreach (var elem in elements)
                    {
                        if (elem is MM_Boundary)
                            sB.AppendLine(elem.Name);
                        else
                            sB.AppendLine(elem.MenuDescription());
                        try
                        {
                            if (elem is MM_Line)
                            {
                                var line = elem as MM_Line;

                                string subTo = line.MVAFlowDirection.DisplayName();
                                string subFrom = line.Substation1.DisplayName();
                                if (subFrom == subTo)
                                    subFrom = line.Substation2.DisplayName();
                                string voltage = line.KVLevel.ToString().Replace("KV", "").Trim();
                                if (voltage.IndexOf("Other", StringComparison.OrdinalIgnoreCase) >= 0)
                                    voltage = line.NearVoltage.ToString("###.#");

                                /*float MW = (Math.Abs(line.Estimated_MW[0]) + Math.Abs(line.Estimated_MW[1])) / 2f;
                                if (!float.IsNaN(line.Telemetered_MW[0]) && Math.Abs(MW - Math.Abs(line.Telemetered_MW[0])) < Math.Abs(MW * .1))
                                    MW = (MW + Math.Abs(line.Telemetered_MW[0])) / 2f; */

                                sB.AppendFormat(" {3} / {4} MVA on {0} - {1} {2} kV", subFrom, subTo, voltage, line.MVAFlow.ToString("##0.#"), line.NormalLimit.ToString("###"));

                                string fg = line.OnFlowgates;
                                if (fg != null && fg.Length > 0)
                                {
                                    sB.AppendLine();
                                    sB.Append("  FG: ");

                                    for (int i = 0; i < line.Contingencies.Count; i++)
                                    {
                                        bool found = false;
                                        for (int j = 0; j < i; j++)
                                            if (line.Contingencies[i].Name == line.Contingencies[j].Name)
                                            {
                                                found = true;
                                                break;
                                            }
                                        if (!found && line.Contingencies[i].Type == "Flowgate")
                                            sB.AppendFormat("{0} {1}% , ", line.Contingencies[i].Name, (((MM_Flowgate)line.Contingencies[i]).PercentLoaded * 100).ToString("##0"));
                                    }
                                    if (sB.Length > 3 && line.Contingencies.Count > 0)
                                        sB.Remove(sB.Length - 3, 2);
                                }
                                if (line is MM_Tie && ((MM_Tie)line).TieDescriptor != null)
                                {
                                    sB.AppendLine();
                                    sB.Append("  TyLine: " + ((MM_Tie)line).TieDescriptor + (((MM_Tie)line).IsDC ? " DC" : " AC"));
                                }
                                sB.AppendLine();
                            }
                            else if (elem is MM_Substation)
                            {
                                var sub = elem as MM_Substation;

                                if (sub.Units != null && sub.Units.Count > 0)
                                {
                                    Dictionary<String, float> GenTypes = new Dictionary<string, float>();
                                    Dictionary<String, float> GenCapacities = new Dictionary<string, float>();
                                    foreach (MM_Unit Unit in sub.Units)
                                        if (!GenTypes.ContainsKey(Unit.FuelType.Name))
                                        {
                                            GenTypes.Add(Unit.FuelType.Name, Unit.RTGenMW > 0 ? Unit.RTGenMW : Unit.Estimated_MW);
                                            GenCapacities.Add(Unit.FuelType.Name, !float.IsNaN(Unit.EcoMax) && Unit.EcoMax > 0 ? Unit.EcoMax : Unit.MaxCapacity);
                                        }
                                        else
                                        {
                                            GenTypes[Unit.FuelType.Name] += Unit.RTGenMW > 0 ? Unit.RTGenMW : Unit.Estimated_MW;
                                            GenCapacities[Unit.FuelType.Name] += (!float.IsNaN(Unit.EcoMax) && Unit.EcoMax > 0 ? Unit.EcoMax : Unit.MaxCapacity);
                                        }

                                    float Capacity;
                                    foreach (KeyValuePair<String, float> kvp in GenTypes)
                                        if (GenCapacities.TryGetValue(kvp.Key, out Capacity))
                                            if (float.IsNaN(kvp.Value) && float.IsNaN(Capacity))
                                                sB.AppendLine(" Gen (" + kvp.Key + "): None");
                                            else
                                                sB.AppendLine(" Gen (" + kvp.Key + "): " + kvp.Value.ToString("###,##0.#") + " (" + (kvp.Value / Capacity).ToString("0%") + ") → " + (kvp.Value > Capacity ? (kvp.Value - Capacity).ToString("###,##0.#") + " mw OVER" : (Capacity - kvp.Value).ToString("###,##0.#") + " mw"));

                                    //float gen = sub.Units.Sum(u => u.RTGenMW > 0 ? u.RTGenMW : u.Estimated_MW);
                                    // if (gen > .1f)
                                    //  sB.Append(" Gen: " + gen.ToString("###,###.#"));
                                }

                                if (sub.ShuntCompensators.Count > 0)
                                {
                                    float Caps = 0, Reacs = 0;
                                    foreach (MM_ShuntCompensator SC in sub.ShuntCompensators)
                                        if (SC.Open && !float.IsNaN(SC.Nominal_MVAR))
                                            if (SC.Nominal_MVAR < 0)
                                                Reacs -= SC.Nominal_MVAR;
                                            else if (SC.Nominal_MVAR > 0)
                                                Caps += SC.Nominal_MVAR;
                                    if (Caps != 0)
                                        sB.AppendLine(" Caps: " + Caps.ToString("#,##0.#") + " mvar");
                                    if (Reacs != 0)
                                        sB.AppendLine(" Reacs: " + Reacs.ToString("#,##0.#") + " mvar");

                                }


                                if (sub.Loads != null && sub.Loads.Count > 0)
                                {
                                    float load = sub.Loads.Sum(l => l.Estimated_MW);
                                    if (load > 0.1)
                                        sB.AppendLine(" Load: " + load.ToString("###,###.#") + " mw");
                                }

                                sB.AppendLine();
                            }
                        }
                        catch (Exception ex)
                        {
                            MM_System_Interfaces.LogError(ex);
                        }
                    }
                    //_toolTip.AutomaticDelay = 3;
                    _toolTip.Show(sB.ToString(), this, PointToClient(new Point(15 + _lastCursorPosition.X, _lastCursorPosition.Y)));
                }
                _toolTipElements = elements;
                ShowedToolTip = true;
            }

            if (ShowedToolTip && (_toolTipElements == null || _toolTipElements.Count == 0))
            {
                _toolTip.Hide(this);
            }
        }


        /// <summary>
        /// When our network source changes, update the map location to simulate object replacement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_Integration_NetworkSourceChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        /// <summary>
        /// Handle the updating of the repository view, by updating the graphics paths
        /// </summary>
        /// <param name="ActiveView"></param>
        private void Repository_ViewChanged(MM_Display_View ActiveView)
        {
            Coordinates.UpdateZoom(Coordinates.ZoomLevel);
        }

        /// <summary>
        /// Handle the map pan event - find our pixel difference between the two points, and adjust everything linearly.
        /// </summary>
        /// <param name="OldCenter">The old center of the screen</param>
        /// <param name="NewCenter">The new center of the screen</param>
        private void Coordinates_Panned(PointF OldCenter, PointF NewCenter)
        {
            // IsDirty = true;
            Invalidate();
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
            // UpdateGraphicsPaths(false); //MN//20130607
            IsDirty = true;
            Invalidate();
            // GC.Collect();
        }

        /// <summary>
        /// Reset the display controls
        /// </summary>
        public void ResetDisplayCoordinates()
        {
            Coordinates.SetTopLeftAndBottomRight(MM_Repository.Counties["STATE"].Min, MM_Repository.Counties["STATE"].Max);
            UpdateGraphicsPaths(false);
            IsDirty = true;
        }

        /// <summary>
        /// Set display coordiantes to a target value
        /// </summary>
        /// <param name="TopLeft"></param>
        /// <param name="BottomRight"></param>
        public void SetDisplayCoordinates(double[] TopLeft, double[] BottomRight)
        {
            Coordinates.SetTopLeftAndBottomRight(new PointF((float)TopLeft[0], (float)TopLeft[1]), new PointF((float)BottomRight[0], (float)BottomRight[1]));
            UpdateGraphicsPaths(false); //MN//20130607
        }

        /// <summary>
        /// Set display coordiantes to a target value
        /// </summary>
        /// <param name="TopLeft"></param>
        /// <param name="BottomRight"></param>
        public void SetDisplayCoordinates(PointF TopLeft, PointF BottomRight)
        {
            Coordinates.SetTopLeftAndBottomRight(TopLeft, BottomRight);
            UpdateGraphicsPaths(false); //MN//20130607
        }


        /// <summary>
        /// Update our graphics paths for all images
        /// </summary>
        /// <param name="RedrawText">Whether our text needs to be withdrawn</param>
        private void UpdateGraphicsPaths(bool RedrawText = false)
        {
            if (!PresenterReady) return;
            if (RedrawText)
                IsDirty = true;
        }

        //  [Obsolete("This is a new experimental method. Obsolete is used to flag while debugging.")]
        //  private void UpdateContourInfo()
        //  {
        //      // get our substations and values
        //      var subs = Substations.ToList();
        //      var vals = subs.Select(sub => float.IsNaN(sub.Key.Average_pU) ? 1 : sub.Key.Average_pU).ToList();
        //
        //      bool updateCoords = false;
        //
        //      if (_kdTreeContour == null)
        //      {
        //          _kdTreeContour = new KDTreeContour(this);
        //          updateCoords = true;
        //      }
        //
        //      if (updateCoords || vals.Count != _kdTreeContour.CoordCount)
        //      {
        //          // if required update coordinates as well
        //          var coords = subs.Select(sub => new Vector2(sub.Key.LngLat.X, sub.Key.LngLat.Y)).ToList();
        //          _kdTreeContour.UpdateCoordinates(coords, vals);
        //      }
        //      else
        //      {
        //          _kdTreeContour.UpdateValues(vals);
        //      }
        //  }

        /// <summary>
        /// Turn a series of latitude/longitude points into a unique graphics path
        /// </summary>
        /// <param name="LngLats">The collection of latitudes/longitudes that define the path</param>
        /// <returns></returns>
        public GraphicsPath CreateGraphicsPath(IList<PointF> LngLats)
        {
            var OutG = new GraphicsPath();
            var LastPoint = Point.Empty;
            var OutPoints = new List<Point>(LngLats.Count);
            foreach (var pt in LngLats)
            {
                var InPoint = MM_Coordinates.LngLatToXY(pt, Coordinates.ZoomLevel);
                if (LastPoint.IsEmpty || InPoint.X != LastPoint.X || InPoint.Y != LastPoint.Y)
                    OutPoints.Add(InPoint);
                LastPoint = InPoint;
            }

            OutG.AddLines(OutPoints.ToArray());
            return OutG;
        }

        /// <summary>
        /// Creats a <see cref="PathGeometry"/> from a collection of GPS <see cref="PointF"/>s. This object should be disposed.
        /// </summary>
        /// <param name="lngLats">GPS Coordinates.</param>
        /// <param name="closed">Close the geometry.</param>
        /// <param name="filled">Fill the geometry.</param>
        /// <returns></returns>
        public PathGeometry CreatePathGeometry(IList<PointF> lngLats, out RawRectangleF bounds, bool closed = true, bool filled = false)
        {
            var geometry = new PathGeometry(Factory2D);

            if (lngLats.Count > 0)
            {
                var point = MM_Coordinates.LngLatToScreenVector2(lngLats[0], Coordinates.ZoomLevel);

                float minX = point.X;
                float minY = point.Y;
                float maxX = point.X;
                float maxY = point.Y;

                using (var sink = geometry.Open())
                {
                    sink.BeginFigure(point, filled ? FigureBegin.Filled : FigureBegin.Hollow);
                    for (int i = 1; i < lngLats.Count(); i++)
                    {
                        point = MM_Coordinates.LngLatToScreenVector2(lngLats[i], Coordinates.ZoomLevel);
                        sink.AddLine(point);
                        if (point.X < minX) minX = point.X;
                        if (point.X > maxX) maxX = point.X;

                        if (point.Y < minY) minY = point.Y;
                        if (point.Y > maxY) maxY = point.Y;
                    }
                    sink.EndFigure(closed ? FigureEnd.Closed : FigureEnd.Open);
                    sink.Close();
                }

                bounds = new RawRectangleF(minX, minY, maxX, maxY);
            }
            else
            {
                bounds = new RawRectangleF(0, 0, 0, 0);
            }

            return geometry;
        }


        /// <summary>
        /// Occurs when the DirectX device is initialized. Objects should load local resources here.
        /// </summary>
        protected override void OnLoadContent()
        {
            InitializeContours();

            AddLayer(new BaseMapLayer(this));

            _stateRegionLayer = new StateRegionsLayer(this);
            AddLayer(_stateRegionLayer);

#if DEBUG
            AddLayer(new DynamicRegionsLayer(this));
#endif
            _lineLayer = new LineLayer(this, violViewer);
            AddLayer(_lineLayer);

            _substationLayer = new SubstationLayer(this, violViewer);
            AddLayer(_substationLayer);
            AddLayer(new ContourOverlayLayer(this));

#if DEBUG
            AddLayer(new WeatherMapLayer(this));
#endif
            AddLayer(new LegendLayer(this));
            _minimapLayer = new MinimapLayer(this, violViewer);
            AddLayer(_minimapLayer);

            _trainingLayer = new TrainingGameLayer(this);
            AddLayer(_trainingLayer);
        }

        private void InitializeContours()
        {
            var lmpPalette = new LinearGradientPalette(
                this,
                new[]
                {
                    new GradientStop { Color = SharpDxExtensions.Color4FromHexCode("#9DFAB2"), Position = 0f },
                    new GradientStop { Color = SharpDxExtensions.Color4FromHexCode("#165BAD"), Position = 0.01f },
                    new GradientStop { Color = SharpDxExtensions.Color4FromHexCode("#4B91FA"), Position = 0.25f },
                    new GradientStop { Color = SharpDxExtensions.Color4FromHexCode("#3CD4FA"), Position = 0.49f },
                    new GradientStop { Color = SharpDxExtensions.Color4FromHexCode("#30EDFB"), Position = 0.5f },
                    new GradientStop { Color = SharpDxExtensions.Color4FromHexCode("#43FDF4"), Position = 0.51f },
                    new GradientStop { Color = SharpDxExtensions.Color4FromHexCode("#B2FC9F"), Position = 0.6f },
                    new GradientStop { Color = SharpDxExtensions.Color4FromHexCode("#FFFF00"), Position = 0.75f },
                    new GradientStop { Color = SharpDxExtensions.Color4FromHexCode("#FF0000"), Position = 1.0f },
                    new GradientStop { Color = SharpDX.Color.Red, Position = 1f },
                },
                Vector2.Zero, Vector2.One)
            {
                Name = "LMP Palette"
            };

            var kvPalette = new LinearGradientPalette(
                this,
                new[]
                {
                    new GradientStop { Color = new Color4(0, 0.58f, 1f, 1f), Position = 0f }, // Blue
                    new GradientStop { Color = new Color4(0, 0.58f, 1f, 0.5f), Position = 0.4f }, 
                    new GradientStop { Color = new Color4(0,0,0,0), Position = 0.5f }, // Gray (50% transparent)
                    new GradientStop { Color = new Color4(1f, 0, 0, 0.5f), Position = 0.6f }, 
                    new GradientStop { Color = new Color4(1f, 0, 0, 1f), Position = 1.0f }, // Red
                },
                Vector2.Zero, Vector2.One)
            {
                Name = "KV Palette"
            };

            var loadingPalette = new LinearGradientPalette(
                this,
                new[]
                {
                    new GradientStop { Color = new Color4(0, 0, 0f, 0f), Position = 0f },
                    new GradientStop { Color = new Color4(1f,1f,0,1f), Position = 0.5f }, // yellow
                    new GradientStop { Color = new Color4(1f, 0, 0, 1f), Position = 1.0f }, // Red
                },
                Vector2.Zero, Vector2.One)
            {
                Name = "Loading Palette"
            };
            try
            {
                var lmpContour = new SubstationLmpContourData(lmpPalette);
                MM_Repository.ContourDataProviders.Add(MM_Display.ContourOverlayDataTypes.LMPAverage, lmpContour);

                var kvContour = new SubstationVoltageContourData(kvPalette);
                MM_Repository.ContourDataProviders.Add(MM_Display.ContourOverlayDataTypes.KVAverage, kvContour);

                var congestionContour = new LineLoadingPercentageContourData(loadingPalette);
                MM_Repository.ContourDataProviders.Add(MM_Display.ContourOverlayDataTypes.LineLoadingPercentage, congestionContour);


            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Occurs when the DirectX device is unloading. Cleanup any disposable objects referencing factories or render targets here.
        /// </summary>
        protected override void OnUnloadContent()
        {
            // layers get cleaned up by base class
        }

        #region Overrides of Direct3DSurface

        /// <summary>
        /// Release the unmanaged resources used by the <see cref="Direct3DSurface"/> and it's child controls and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release managed resources and unmanaged resources. False to only release unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                OnUnloadContent();

                var mapTiles = MM_Repository.MapTiles.Values.ToList();
                MM_Repository.MapTiles.Clear();
                foreach (var bitmap in mapTiles)
                {
                    bitmap.Dispose();
                }
            }
        }

        #endregion

        /// <summary>
        /// Update cycle: perform calculations and update data here.
        /// </summary>
        protected override void OnUpdating(RenderTime updateTime)
        {
            if (!PanPoint.IsEmpty)
                Coordinates.UpdateFromXY(new Point(Coordinates.TopLeftXY.X + PanPoint.X, Coordinates.TopLeftXY.Y + PanPoint.Y));

            if (Coordinates.DisplayRectangle != DisplayRectangle)
                Coordinates.DisplayRectangle = DisplayRectangle;

            if (IsDebug != MM_Repository.OverallDisplay.FPS)
                IsDebug = MM_Repository.OverallDisplay.FPS;

            CheckToolTipSafe();
        }


        protected override void OnUpdated(RenderTime updateTime)
        {
            IsDirty = false;
        }


        /// <summary>
        /// Render cycle: only perform drawing code here. Use OnUpdateDx to perform calculations.
        /// </summary>
        protected override void OnRendering(RenderTime renderTime)
        {
            RenderTarget2D.Clear(MM_Repository.OverallDisplay.BackgroundColor.ToDxColor4());
        }

        /// <summary>
        /// Render cycle: only perform drawing code here. Use OnUpdateDx to perform calculations.
        /// </summary>
        protected override void OnRendered(RenderTime renderTime)
        {
            var tx = RenderTarget2D.Transform;
            RenderTarget2D.Transform = Matrix3x2.Translation(-Coordinates.TopLeftXY.X, -Coordinates.TopLeftXY.Y);

            if (_toolTipElements != null && _toolTipElements.Count > 0)
            {
                foreach (var element in _toolTipElements)
                {
                    var brush = Brushes.GetBrush(new Color4(0, 1, 0, 0.5f));

                    if (element is MM_Line)
                    {
                        var lineProxy = _lineLayer.GetProxy(element.TEID);
                        var dispNormal = ((MM_Line)element).LineDisplay(null, this, false);

                        for (int i = 0; i < lineProxy.Coordinates.Count - 1; i++)
                        {
                            RenderTarget2D.DrawLine(lineProxy.Coordinates[i], lineProxy.Coordinates[i + 1], brush, dispNormal.Width + 4);
                        }
                    }
                    if (element is MM_Substation)
                    {
                        var station = (MM_Substation)element;
                        var stationProxy = _substationLayer.GetProxy(station.TEID);

                        var tx2 = RenderTarget2D.Transform;
                        RenderTarget2D.Transform = Matrix3x2.Translation(stationProxy.Coordinates + ((Matrix3x2)tx2).TranslationVector);
                        RenderTarget2D.DrawEllipse(new Ellipse(Vector2.Zero, 20, 20), brush, 3);
                        RenderTarget2D.Transform = tx2;
                    }
                }
            }

            //If the user is drawing a lasso, display it
            if (Coordinates.LassoMultiplePoints)
            {
                DrawLassoPoints(Coordinates.LassoPoints);
            }
            else if (!Coordinates.LassoEnd.IsEmpty)
            {
                DrawLasso(Coordinates.LassoStart, Coordinates.LassoEnd);
            }

            RenderTarget2D.Transform = tx;
        }


        protected override void OnRender3D() { }


        /// <summary>
        /// Draw a crosshair of our target size
        /// </summary>
        /// <param name="DrawPen"></param>
        /// <param name="Location"></param>
        /// <param name="Radius"></param>
        /// <param name="TextToDraw"></param>
        private void DrawCrosshair(Pen DrawPen, Point Location, int Radius, string TextToDraw)
        {
            //Fill an elipse 5 points bigger to highlight everything
            var stroke = GetStrokeStyle(DrawPen);
            RenderTarget2D.FillEllipse(new Ellipse(new Vector2(Location.X - Radius - 5, Location.Y - Radius - 5), Radius + Radius + 10, Radius + Radius + 10), Brushes.GetBrush(SharpDX.Color.Black));
            RenderTarget2D.DrawEllipse(new Ellipse(new Vector2(Location.X - Radius, Location.Y - Radius), Radius + Radius, Radius + Radius), Brushes.GetBrush(DrawPen.Color), DrawPen.Width, stroke);
            RenderTarget2D.DrawLine(new Vector2(Location.X, Location.Y - Radius), new Vector2(Location.X, Location.Y + Radius), Brushes.GetBrush(DrawPen.Color), DrawPen.Width, stroke);
            RenderTarget2D.DrawLine(new Vector2(Location.X - Radius, Location.Y), new Vector2(Location.X + Radius, Location.Y), Brushes.GetBrush(DrawPen.Color), DrawPen.Width, stroke);

            //Now, if requested, show the text to draw
            if (!String.IsNullOrEmpty(TextToDraw))
            {

                DrawText(
                    RenderTarget2D,
                    TextToDraw,
                    Brushes.GetBrush(SharpDX.Color.White),
                    Fonts.GetTextFormat(MM_Repository.OverallDisplay.KeyIndicatorLabelFont),
                    Location.X,
                    Location.Y + Radius + 5, 500, 500,
                    Brushes.GetBrush(SharpDX.Color.Black), centerX: true);
            }
        }

        private List<MM_Element> _toolTipElements = new List<MM_Element>();
        private LineLayer _lineLayer;
        private SubstationLayer _substationLayer;
        private StateRegionsLayer _stateRegionLayer;
        private MinimapLayer _minimapLayer;
        private TrainingGameLayer _trainingLayer;
        private bool MinimapMouse;
        private bool _prevMouseOnMiniMap =false;

        /// <summary>
        /// Gets the violations currently displayed on the violation viewer.
        /// </summary>
        public Dictionary<MM_AlarmViolation, ListViewItem> GetShownViolations
        {
            get { return violViewer.ShownViolations; }
        }


        /// <summary>
        /// Draw all lasso points
        /// </summary>
        /// <param name="LassoPoints"></param>
        private void DrawLassoPoints(List<PointF> LassoPoints)
        {
            //First, turn our lasso points into screen points
            var outPts = new Vector2[LassoPoints.Count];
            for (int a = 0; a < LassoPoints.Count; a++)
                outPts[a] = MM_Coordinates.LngLatToScreenVector2(LassoPoints[a], Coordinates.ZoomLevel);

            var geometry = new PathGeometry(Factory2D);
            using (var sink = geometry.Open())
            {
                sink.BeginFigure(outPts[0], FigureBegin.Filled);

                for (int i = 1; i < outPts.Length; i++)
                {
                    sink.AddLine(outPts[i]);
                }
                sink.EndFigure(FigureEnd.Closed);
                sink.Close();
            }


            RenderTarget2D.FillGeometry(geometry, Brushes.GetBrush(((SolidBrush)Coordinates.LassoBrush).Color, 0.5f));
            RenderTarget2D.DrawGeometry(geometry, Brushes.GetBrush(SharpDX.Color.White));

            geometry.Dispose();
        }


        /// <summary>
        /// Draw a lasso on the screen
        /// </summary>
        /// <param name="Start">The first coordinate for the lasso</param>
        /// <param name="End">The last coordinate for the lasso</param>
        private void DrawLasso(PointF Start, PointF End)
        {
            var lassoStart = MM_Coordinates.LngLatToXY(Start, Coordinates.ZoomLevel);
            var lassoEnd = MM_Coordinates.LngLatToXY(End, Coordinates.ZoomLevel);

            if (lassoStart.X > lassoEnd.X)
            {
                int tempX = lassoStart.X;
                lassoStart.X = lassoEnd.X;
                lassoEnd.X = tempX;
            }
            if (lassoStart.Y > lassoEnd.Y)
            {
                int tempY = lassoStart.Y;
                lassoStart.Y = lassoEnd.Y;
                lassoEnd.Y = tempY;
            }
            var outRect = new RectangleF(lassoStart.X, lassoStart.Y, lassoEnd.X - lassoStart.X, lassoEnd.Y - lassoStart.Y);

            RenderTarget2D.FillRectangle(outRect, Brushes.GetBrush(((SolidBrush)Coordinates.LassoBrush).Color, 0.5f));
            RenderTarget2D.DrawRectangle(outRect, Brushes.GetBrush(SharpDX.Color.White));
        }

        /// <summary>
        /// Handle the size change by updating our graphics paths
        /// </summary>
        public void ResizeComplete()
        {
            if (Visible) //mn// what is this for?? what is it really doing??
            {
                if (Coordinates.DisplayRectangle != DisplayRectangle)
                    Coordinates.DisplayRectangle = DisplayRectangle;

                Coordinates.UpdateZoom(Coordinates.ZoomLevel);
                //Coordinates.UpdateFromXY();
            }
            UpdateGraphicsPaths(false); //MN//20130607
        }

        /// <summary>
        /// Determine whether a line or substation is visible
        /// </summary>
        /// <param name="Element"></param>
        /// <returns></returns>
        public bool IsVisible(MM_Element Element)
        {
            // used in lasso detection
            return true;
            // TODO: check if line and sub are visible
            throw new NotImplementedException("TODO: needs fixing");
        }

        /// <summary>
        /// Determine whether a line or substation's connector is visible.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool IsConnectorVisible(MM_Element element)
        {
            // used in lasso detection
            return true;
            throw new NotImplementedException("TODO: needs fixing");
            if (element is MM_Substation)
                return _substationLayer.GetProxy(element.TEID).ConnectorVisible;
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
        {
            //MN//20130607 catch here verified for top level catch
            //if (!LockForDataUpdate)
            //{

            MousePos = Cursor.Position;
            var mouseVector = new Vector2(e.X, e.Y);
            if (MM_Repository.Training != null && MM_Repository.Training.TrainingMode != MM_Training.enumTrainingMode.NotTraning) { }
            else if (MM_Repository.DisplayPositionLocked) { }
            else if (_minimapLayer.PointOnMap(mouseVector))
            {
                _minimapLayer.CenterMap(mouseVector);
                _allowToolTip = false;
                MinimapMouse = true;
            }
            else if (e.Button == MouseButtons.Right && ShiftDown && Data_Integration.Permissions.LassoStandard)
                Coordinates.LassoPoints.Add(Coordinates.XYToLngLat(e.Location));
            else if (e.Button == MouseButtons.Right && Data_Integration.Permissions.LassoStandard)
                Coordinates.LassoStart = Coordinates.XYToLngLat(e.Location);
            else if (e.Button == MouseButtons.Middle)
                Cursor = Cursors.NoMove2D;
            else if (MM_Key_Indicators.DeltaDisplay.Visible)
            {
                //Check to see which element we found
                var PixelLocation = new Point(e.X + Coordinates.TopLeftXY.X, e.Y + Coordinates.TopLeftXY.Y);
                var Elems = HitTest(PixelLocation);
                if (Elems.Count >= 1)
                    if (!(Elems[0] is MM_Line) || (Elems[0] is MM_Line && MM_Repository.OverallDisplay.MoveLines))
                    {
                    UpdatingElement = MM_Key_Indicators.DeltaDisplay.HandleElementSelection(Elems[0], Coordinates, PixelLocation, ref ActivelyMovingElement);
                    }
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
        {
            var mouseVector = new Vector2(e.X,e.Y);
            bool onMiniMap = _minimapLayer.PointOnMap(mouseVector);
            if (onMiniMap)
            {
                HideToolTip();
                _allowToolTip = false;
            }
            if (!onMiniMap && _prevMouseOnMiniMap && !_allowToolTip)
                _allowToolTip = true;
            _prevMouseOnMiniMap = onMiniMap;
            if (!HandlingMouse)
                return;
            else if (MM_Repository.Training != null && MM_Repository.Training.TrainingMode != MM_Training.enumTrainingMode.NotTraning) { }
            else if (MM_Repository.DisplayPositionLocked) { }
            if (MinimapMouse && _minimapLayer.PointOnMap(mouseVector) && e.Button == MouseButtons.Left)
            {
                _minimapLayer.CenterMap(mouseVector);
                return;
            }
            if (MinimapMouse)
            {
                return;
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (UpdatingElement == null)
                    Coordinates.UpdateFromXY(new Point(Coordinates.TopLeftXY.X - Cursor.Position.X + MousePos.X, Coordinates.TopLeftXY.Y - Cursor.Position.Y + MousePos.Y));
                else if (UpdatingElement.BaseSubstation != null)
                {
                    PointF NewPos = MM_Coordinates.XYToLngLat(new Point(Coordinates.TopLeftXY.X + e.X, Coordinates.TopLeftXY.Y + e.Y), Coordinates.ZoomLevel);
                    UpdatingElement.UpdateSubstationCoordinates(NewPos, new Point(Coordinates.TopLeftXY.X + e.X, Coordinates.TopLeftXY.Y + e.Y));
                    MM_Key_Indicators.DeltaDisplay.UpdateElement(UpdatingElement.BaseSubstation);
                    IsDirty = true;
                }
                else if (UpdatingElement.BaseLine != null)
                {
                    Point NewPosXY = new Point(Coordinates.TopLeftXY.X + e.X, Coordinates.TopLeftXY.Y + e.Y);
                    PointF NewPos = MM_Coordinates.XYToLngLat(NewPosXY, Coordinates.ZoomLevel);
                    UpdatingElement.UpdateLineCoordinates(NewPos, NewPosXY);
                    MM_Key_Indicators.DeltaDisplay.UpdateElement(UpdatingElement.BaseLine);
                    IsDirty = true;
                }
                MousePos = Cursor.Position;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                PanPoint = new Point((int)((Cursor.Position.X - MousePos.X) * MM_Repository.OverallDisplay.PanSpeed),
                                     (int)((Cursor.Position.Y - MousePos.Y) * MM_Repository.OverallDisplay.PanSpeed));

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
                Coordinates.AddLassoPoint(Coordinates.XYToLngLat(e.Location));
                MousePos = Cursor.Position;
                Invalidate();
            }
            else if (e.Button == MouseButtons.Right && Data_Integration.Permissions.LassoStandard)
            {
                Coordinates.LassoEnd = Coordinates.XYToLngLat(e.Location);
                MousePos = Cursor.Position;
                // TODO: Lasso zoom
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
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Handle the left and right mouse up events
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            UpdatingElement = null;
            if ((MM_Repository.Training == null || MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.NotTraning) && !MM_Repository.DisplayPositionLocked)
                if (e.Button == MouseButtons.Left)
                {
                    IsDirty = true;
                    _allowToolTip = true;
                }
                else if (e.Button == MouseButtons.Middle)
                {
                    IsDirty = true;
                    _allowToolTip = true;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    HideToolTip();
                    if (Coordinates.LassoMultiplePoints && Data_Integration.Permissions.LassoStandard)
                    {
                        _allowToolTip = false;
                        var LassoPoints = new List<PointF>(Coordinates.LassoPoints);
                        MM_Form_Builder.Lasso_Display(LassoPoints, this, Coordinates.ControlDown);
                    }
                    else if (Coordinates.LassoEnd.IsEmpty || Coordinates.LassoStart == Coordinates.LassoEnd)
                    {
                        _allowToolTip = false;
                        HitTestAndPopup(e.Location);
                    }
                    else if (Data_Integration.Permissions.LassoStandard)
                    {
                        _allowToolTip = false;
                        MM_Form_Builder.Lasso_Display(Coordinates.LassoStart, Coordinates.LassoEnd, this, Coordinates.ControlDown);
                    }
                }
            Cursor = Cursors.Default;
            MousePos = Point.Empty;
            Coordinates.LassoStart = Point.Empty;
            Coordinates.LassoEnd = Point.Empty;
            PanPoint = Point.Empty;
            Coordinates.ControlDown = false;
            Invalidate();
            HandlingMouse = false;
            MinimapMouse = false;
            base.OnMouseUp(e);

            //If we're in training mode, handle accordingly.
            if (MM_Repository.Training != null)
                if (MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.QuestionAsked)
                    MM_Repository.Training.HandleResponse(Coordinates.XYToLngLat(e.Location));
                else if (MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.AnswerCorrect || MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.AnswerWrong)
                    MM_Repository.Training.NextQuestion();
                else if (MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.UserWon || MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.UserFailed)
                {
                    if (DateTime.Now - MM_Repository.Training.MessageTime > TimeSpan.FromSeconds(10))
                    {
                        MM_Repository.Training.TrainingMode = MM_Training.enumTrainingMode.NotTraning;
                        (ParentForm as MacomberMap_Form).ctlNetworkMap.ResetDisplayCoordinates();
                    }
                }
        }

        /// <summary>
        /// Handle the map's losing focus
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(EventArgs e)
        {
            HandlingMouse = false;
            Coordinates.ControlDown = false;
            ShiftDown = false;
            base.OnLostFocus(e);
        }


        /// <summary>
        /// Check to see whether a user has clicked on a substation or line
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        private void HitTestAndPopup(Point mousePosition)
        {
            var pixelLocation = new Vector2(mousePosition.X + Coordinates.TopLeftXY.X, mousePosition.Y + Coordinates.TopLeftXY.Y);
            var elements = HitTest(new Point((int)pixelLocation.X, (int)pixelLocation.Y),false);
            if (elements.Count > 0)
                PopupMenu(elements, mousePosition);
            else
            {
                if (MM_Repository.OverallDisplay.DisplayCounties || MM_Repository.OverallDisplay.DisplayDistricts)
                {
                    var regions = _stateRegionLayer.HitTest(pixelLocation, 1).ToList();
                    if (regions.Count > 0)
                    {
                        RightClickMenu.Show(this, mousePosition, regions, false);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Hit test the map to find substations and/or lines that fit our coordinates
        /// </summary>
        /// <param name="PixelLocation"></param>
        /// <param name="ReturnSubstationsOnly"></param>
        /// <returns></returns>
        private List<MM_Element> HitTest(Point PixelLocation, bool ReturnSubstationsOnly = true)
        {
            var screenCoordinates = PixelLocation.ToRawVector2();
            var elements = _substationLayer.HitTest(screenCoordinates, Coordinates.ZoomLevel / 3f, true).ToList();
            if (ReturnSubstationsOnly && elements.Any())
            {
                return elements;
            }
            else
            {
            elements.AddRange(_lineLayer.HitTest(screenCoordinates, 0.8f + Coordinates.ZoomLevel / 2.2f, true).ToList());
            }
            return elements;
        }

        /// <summary>
        /// Popup a menu offering a list of elements to be shown
        /// </summary>
        /// <param name="elemToPopup"></param>
        /// <param name="mousePosition">The current mouse position</param>
        private void PopupMenu(List<MM_Element> elemToPopup, Point mousePosition)
        {
            //Set up our menu for display
            RightClickMenu.Tag = elemToPopup;
            RightClickMenu.Items.Clear();
            if (elemToPopup.Count == 1)
                RightClickMenu.Show(this, mousePosition, elemToPopup[0], false);
            else
                RightClickMenu.Show(this, mousePosition, elemToPopup, false);
        }


        /// <summary>
        /// Handle mouse wheel
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //MN//20130607 catch here verified for top level catch
            if (MM_Repository.DisplayPositionLocked) { }
            else if (MM_Repository.Training != null && MM_Repository.Training.TrainingMode != MM_Training.enumTrainingMode.NotTraning) { }
            else
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
                    case 'r':
                        ResetDisplayCoordinates();
                        IsDirty = true;
                        AddOnscreenMessage("Reset Map Position and Zoom", SharpDX.Color.Aqua);
                        break;
                    case '+':
                        Coordinates.UpdateZoom(Coordinates.ZoomLevel + 1);
                        break;
                    case '-':
                        Coordinates.UpdateZoom(Coordinates.ZoomLevel - 1);
                        break;
                    case 'p':
                        MM_Repository.OverallDisplay.FPS = !MM_Repository.OverallDisplay.FPS;
                        AddOnscreenMessage(MM_Repository.OverallDisplay.FPS ? "Display FPS" : "Hide FPS", 
                                           MM_Repository.OverallDisplay.FPS ? SharpDX.Color.Green : SharpDX.Color.Red);
                        break;
                    case 'h':
                        try
                        {
                            System.Diagnostics.Process.Start(Settings.Default.HelpLocation);
                        }
                        catch { }
                        break;
                    case 't':
                        MM_Repository.OverallDisplay.TieFlowDirectionVisible = !MM_Repository.OverallDisplay.TieFlowDirectionVisible;
                        AddOnscreenMessage(MM_Repository.OverallDisplay.TieFlowDirectionVisible ? "Show Enhance Tie Flow Arrows" : "Hide Enhance Tie Flow Arrows",
                                           MM_Repository.OverallDisplay.TieFlowDirectionVisible ? SharpDX.Color.Green : SharpDX.Color.Red);
                        break;
                    case 'a':
                        MM_Repository.OverallDisplay.ShowLineFlows = !MM_Repository.OverallDisplay.ShowLineFlows;
                        AddOnscreenMessage(MM_Repository.OverallDisplay.ShowLineFlows ? "Show Line Flow Arrows" : "Hide Line Flow Arrows",
                                           MM_Repository.OverallDisplay.ShowLineFlows ? SharpDX.Color.Green : SharpDX.Color.Red);
                        break;
                    case 'z':
                        Video.MM_Video_Player.ShowPlayer();
                        break;
                    case '9':
                    case '8':
                        if (VisibilityAdjustmentThread != null && VisibilityAdjustmentThread.IsAlive)
                            VisibilityAdjustmentThread.Abort();
                        VisibilityAdjustmentThread = new Thread(AdjustVisibility);
                        VisibilityAdjustmentThread.Start(e.KeyChar == '9');
                        break;
                    case '0':
                    case '7':
                        if (VisibilityAdjustmentThread != null && VisibilityAdjustmentThread.IsAlive)
                            VisibilityAdjustmentThread.Abort();
                        {
                            if (e.KeyChar == '7')
                                foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                                    KVLevel.VisibilityThreshold = 1;
                            IsDirty = true;
                        }
                            break;
                    case '3':
                        MM_Repository.GetVoltageLevel("345 KV").SimpleVisibility ^= true; IsDirty = true; break;
                    case '2':
                        MM_Repository.GetVoltageLevel("138 KV").SimpleVisibility ^= true; IsDirty = true; break;
                    case '1':
                        MM_Repository.GetVoltageLevel("69 KV").SimpleVisibility ^= true; IsDirty = true; break;

                    case 'v':
                        MM_Repository.OverallDisplay.VarFlows = !MM_Repository.OverallDisplay.VarFlows;
                        AddOnscreenMessage(MM_Repository.OverallDisplay.VarFlows ? "Show Line VAR Arrows" : "Hide Line VAR Arrows",
                                           MM_Repository.OverallDisplay.VarFlows ? SharpDX.Color.Green : SharpDX.Color.Red);
                        break;
                    case 'f':
                        MM_Repository.OverallDisplay.ShowFlowgates = !MM_Repository.OverallDisplay.ShowFlowgates;
                        AddOnscreenMessage(MM_Repository.OverallDisplay.ShowFlowgates ? "Show Flowgates" : "Hide Flowgates",
                                           MM_Repository.OverallDisplay.ShowFlowgates ? SharpDX.Color.Green : SharpDX.Color.Red);
                        break;
                    case '?':
                        MM_Repository.OverallDisplay.ShowLegend = !MM_Repository.OverallDisplay.ShowLegend;
                        AddOnscreenMessage(MM_Repository.OverallDisplay.ShowLegend ? "Show Legend" : "Hide Legend",
                                           MM_Repository.OverallDisplay.ShowLegend ? SharpDX.Color.Green : SharpDX.Color.Red);
                        break;
                    case 'c':
                        {
                            var modes = Enum.GetValues(typeof(MM_Display.ContourOverlayDataTypes)).Cast<MM_Display.ContourOverlayDataTypes>().ToList();
                            var ixCurrentMode = modes.BinarySearch(MM_Repository.OverallDisplay.ContourData);
                            var ixNextMode = Control.ModifierKeys.HasFlag(Keys.Shift) ? 0 : (ixCurrentMode + 1) % modes.Count;
                            MM_Repository.OverallDisplay.ContourData = modes[ixNextMode];
                        }
                        break;
                    case 's':
                        {
                            var modes = Enum.GetValues(typeof(MM_Substation_Display.SubstationViewEnum)).Cast<MM_Substation_Display.SubstationViewEnum>().ToList();
                            var ixCurrentMode = modes.BinarySearch(MM_Repository.SubstationDisplay.ShowSubstations);
                            var ixNextMode = Control.ModifierKeys.HasFlag(Keys.Shift) ? 0 : (ixCurrentMode + 1) % modes.Count;
                            MM_Repository.SubstationDisplay.ShowSubstations = modes[ixNextMode];
                            AddOnscreenMessage("Substations: " + MM_Repository.SubstationDisplay.ShowSubstations.ToString(),
                                        MM_Repository.SubstationDisplay.ShowSubstations != MM_Substation_Display.SubstationViewEnum.None ? SharpDX.Color.Green : SharpDX.Color.Red);
                        }
                        break;
                    case 'm':
                        {
                            var modes = Enum.GetValues(typeof(MM_MapTile.enumMapType)).Cast<MM_MapTile.enumMapType>().ToList();
                            var ixCurrentMode = modes.BinarySearch(MM_Repository.OverallDisplay.MapTiles);
                            var ixNextMode = Control.ModifierKeys.HasFlag(Keys.Shift) ? 0 : (ixCurrentMode + 1) % modes.Count;
                            MM_Repository.OverallDisplay.MapTiles = modes[ixNextMode];
                        }
                        break;
                    case 'w':
                        MM_Repository.OverallDisplay.ShowWeather = !MM_Repository.OverallDisplay.ShowWeather;
                        AddOnscreenMessage(MM_Repository.OverallDisplay.ShowWeather ? "Show Weather Layer" : "Hide Weather Layer",
                                           MM_Repository.OverallDisplay.ShowWeather ? SharpDX.Color.Green : SharpDX.Color.Red);
                        break;
                    case 'g':
                        {
                            var modes = Enum.GetValues(typeof(MM_Substation_Display.MM_Generator_Bubble_Mode)).Cast<MM_Substation_Display.MM_Generator_Bubble_Mode>().ToList();
                            var ixCurrentMode = modes.BinarySearch(MM_Repository.SubstationDisplay.GeneratorBubblesMode);
                            var ixNextMode = Control.ModifierKeys.HasFlag(Keys.Shift) ? 0 : (ixCurrentMode + 1) % modes.Count;
                            MM_Repository.SubstationDisplay.GeneratorBubblesMode = modes[ixNextMode];
                            AddOnscreenMessage("Generation Bubble Display Mode: " + MM_Repository.SubstationDisplay.GeneratorBubblesMode.ToString(), 
                                               MM_Repository.SubstationDisplay.GeneratorBubblesMode == MM_Substation_Display.MM_Generator_Bubble_Mode.None ? SharpDX.Color.Red : SharpDX.Color.Green);
                        }
                        break;
                    case 'l':
                        MM_Repository.SubstationDisplay.ShowLoadBubbles = !MM_Repository.SubstationDisplay.ShowLoadBubbles;
                        if (!MM_Repository.SubstationDisplay.ShowLoadBubbles)
                            MM_Repository.SubstationDisplay.ShowSubstations = MM_Substation_Display.SubstationViewEnum.Units;
                        AddOnscreenMessage(MM_Repository.SubstationDisplay.ShowLoadBubbles ? "Show Load Bubbles" : "Hide Load Bubbles",
                                           MM_Repository.SubstationDisplay.ShowLoadBubbles ? SharpDX.Color.Green : SharpDX.Color.Red);
                        break;
                }
                base.OnKeyPress(e);
            }
        }

        Thread VisibilityAdjustmentThread;
        

        /// <summary>
        /// Adjust line visibility so that over 5 seconds we go from 100-0 or 0-100%
        /// </summary>
        /// <param name="state"></param>
        private void AdjustVisibility(object state)
        {
            int Start = (int)(MM_Repository.KVLevels.Values.First().VisibilityThreshold * 100f);
            int End = (bool)state ? 0 : 100;
            int Step = (bool)state ? -1 : 1;
            if (Math.Sign(End - Start) != Math.Sign(Step))
                Start = (bool)state ? 100 : 0;
            for (int Visibility = Start; ; Visibility += Step)
            {
                foreach (MM_KVLevel Voltage in MM_Repository.KVLevels.Values)
                    if (Visibility == 0)
                        Voltage.VisibilityThreshold = .00001f;
                    else
                        Voltage.VisibilityThreshold = (float)Visibility / 100f;
                IsDirty = true;
                if (Visibility == End) break;
                Thread.Sleep(40);
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
            ShiftDown = e.Shift;
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
            ShiftDown = e.Shift;
            base.OnKeyUp(e);
        }

        #endregion
    }
}