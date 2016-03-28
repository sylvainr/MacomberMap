using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using MacomberMapClient.User_Interfaces.NetworkMap.Proxies;
using MacomberMapClient.User_Interfaces.Violations;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using Brush = SharpDX.Direct2D1.Brush;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{


    public class LineLayer : RenderLayer
    {
        /// <summary>The graphic of our line flow</summary>
        public PathGeometry LineFlow;

        private readonly MM_Violation_Viewer _violationViewer;
        Dictionary<int, LineProxy> _lineRenderProxies = new Dictionary<int, LineProxy>();
        private Dictionary<string, MultiLineProxy> _multiLines = new Dictionary<string, MultiLineProxy>();
        private int lastLineCount = 0;
        private StrokeStyle _dashStyle;
        private StrokeStyle _dotDashStyle;
        private StrokeStyle _solidStyle;

        private int _lastZoom = 0;

        private TextLayout textLayoutNormallyOpen;
        private TextLayout textLayoutTieLine;
        private bool _isBlinking;
        private TextFormat _lineFont;

        public LineProxy GetProxy(int teid)
        {
            LineProxy proxy;
            return _lineRenderProxies.TryGetValue(teid, out proxy) ? proxy : null;
        }

        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        public LineLayer(Direct3DSurface surface, MM_Violation_Viewer violationViewer) : base(surface, "LineLayer", 5)
        {
            _violationViewer = violationViewer;
        }

        /// <summary>
        /// Load Content and initialize unmanaged resources.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();
            _dashStyle = new StrokeStyle(Surface.Factory2D,
                                        new StrokeStyleProperties() { DashCap = CapStyle.Flat, DashStyle = DashStyle.Custom, DashOffset = 10f, LineJoin = LineJoin.MiterOrBevel, MiterLimit = 2f }, new float[] { 2, 2 });
            _dotDashStyle = new StrokeStyle(Surface.Factory2D,
                                         new StrokeStyleProperties() { DashCap = CapStyle.Flat, DashStyle = DashStyle.Custom, DashOffset = 10f, LineJoin = LineJoin.MiterOrBevel, MiterLimit = 2f }, new float[] { 3, 2, 1, 2 });
            _solidStyle = new StrokeStyle(Surface.Factory2D,
                                         new StrokeStyleProperties() { DashCap = CapStyle.Flat, DashStyle = DashStyle.Solid, LineJoin = LineJoin.MiterOrBevel, MiterLimit = 2f });

            var font = Surface.Fonts.GetTextFormat(MM_Repository.OverallDisplay.NetworkMapFont);
            textLayoutNormallyOpen = new TextLayout(Surface.FactoryDirectWrite, "N.O.", font, 40, 40);
            textLayoutTieLine = new TextLayout(Surface.FactoryDirectWrite, "Tie", font, 40, 40);

            if (LineFlow != null)
                LineFlow.Dispose();
            LineFlow = new PathGeometry(Surface.Factory2D);

            using (GeometrySink sink = LineFlow.Open())
            {
                RawVector2 p0 = new RawVector2 { X = 0f, Y = 0f };
                RawVector2 p1 = new RawVector2 { X = -1f, Y = 1.0f };
                RawVector2 p2 = new RawVector2 { X = -1f, Y = -1.0f };

                sink.BeginFigure(p0, FigureBegin.Filled);
                sink.AddLine(p1);
                sink.AddLine(p2);
                sink.EndFigure(FigureEnd.Closed);

                // Note that Close() and Dispose() are not equivalent like they are for
                // some other IDisposable() objects.
                sink.Close();
            }
        }

        /// <summary>
        /// Unload content and cleanup unmanaged resources.
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            _lineFont = null;
            if (LineFlow != null)
                LineFlow.Dispose();
            LineFlow = null;

            if (textLayoutNormallyOpen != null)
                textLayoutNormallyOpen.Dispose();
            textLayoutNormallyOpen = null;
            if (textLayoutTieLine != null)
                textLayoutTieLine.Dispose();
            textLayoutTieLine = null;

            if (_dashStyle != null)
                _dashStyle.Dispose();
            if (_dotDashStyle != null)
                _dotDashStyle.Dispose();
            if (_solidStyle != null)
                _solidStyle.Dispose();

            _dashStyle = null;
            _dotDashStyle = null;
            _solidStyle = null;
        }

        /// <summary>
        /// Returns a list of objects that are in the requested space.
        /// </summary>
        /// <param name="screenCoordinates">The test coordinates</param>
        /// <param name="radius">The radius to test.</param>
        /// <param name="onlyVisible">Only include elements visible on the screen.</param>
        /// <returns></returns>
        public override IEnumerable<MM_Element> HitTest(RawVector2 screenCoordinates, float radius, bool onlyVisible = true)
        {
            foreach (var proxy in _lineRenderProxies)
            {
                if (onlyVisible && !proxy.Value.Visible) continue;

                if (proxy.Value.HitTest(screenCoordinates, radius))
                    yield return proxy.Value.BaseLine;
            }
        }

        /// <summary>
        /// Rebuild our collection of lines to render
        /// </summary>
        /// <param name="updateTime">Time since last update.</param>
        /// <remarks>include base.Update(renderTime); in overloads to preserve updating UpdateTime field.</remarks>
        public override void Update(RenderTime updateTime)
        {
            base.Update(updateTime);

            if (!Surface.IsDirty && _lastZoom == Surface.Coordinates.ZoomLevel)
                return;

            _lastZoom = Surface.Coordinates.ZoomLevel;

            //_lineRenderProxies.Clear();
            //_multiLines.Clear();

            var lines = MM_Repository.Lines.Values.OrderBy(l => l.KVLevel.Nominal);

            Dictionary<string, int> lineStationMap = null;
            //_multiLines = new Dictionary<string, MultiLineProxy>();
            bool updateMultiLine = (_multiLines.Count == 0 || lastLineCount != MM_Repository.Lines.Count);
            lastLineCount = MM_Repository.Lines.Count;
            if (updateMultiLine)
                lineStationMap = new Dictionary<string, int>();
            foreach (var line in lines)
            {
                // checks to determine if we want to render this line
                if (line.IsZBR && line.Length > 150) // skip zbr
                    continue;

                // skip ties
                // if (line.ElemType.Name == "Tie")
                //     continue;

                // validations passed, calculate render info
                LineProxy proxy;
                if (!_lineRenderProxies.TryGetValue(line.TEID, out proxy))
                {
                    proxy = new LineProxy
                    {
                        BaseLine = line,
                        Coordinates = new List<RawVector2>(line.Coordinates.Count)
                    };
                    _lineRenderProxies.Add(line.TEID, proxy);
                }
                LineProxy.CalculateCoords(proxy, line, _lastZoom);
                proxy.BlackStartDim = proxy.BlackStartHidden = false;

                // calculate blackstart parameters
                if ((MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonOperatorElements || MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonOperatorElements) && (line.Operator == null || !line.Operator.Equals(MM_Repository.OverallDisplay.DisplayCompany)))
                    if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonOperatorElements)
                        proxy.BlackStartHidden = true;
                    else
                        proxy.BlackStartDim = true;
                else if ((MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonBlackstartElements || MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonBlackstartElements) && !line.IsBlackstart)
                    if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonBlackstartElements)
                        proxy.BlackStartHidden = true;
                    else
                        proxy.BlackStartDim = true;
                else if ((MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideExternalElements || MM_Repository.OverallDisplay.BlackstartMode== MM_Display.enumBlackstartMode.DimExternalElements) && !line.IsInternal)
                    if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideExternalElements)
                        proxy.BlackStartHidden = true;
                    else
                        proxy.BlackStartDim = true;


                // update multilines
                if (updateMultiLine)
                {
                    // fast multiline detection
                    int sub1Teid = line.Substation1.TEID;
                    int sub2Teid = line.Substation2.TEID;

                    if (sub2Teid < sub1Teid)
                    {
                        sub1Teid = line.Substation2.TEID;
                        sub2Teid = line.Substation1.TEID;
                    }


                    string subTEIDToken = String.Format("{0:0}_{1:0}",sub1Teid,sub2Teid);

                    int otherMultiTeid;
                    if (lineStationMap.TryGetValue(subTEIDToken, out otherMultiTeid))
                    {
                        // multiline!
                        MultiLineProxy multi;
                        if (!_multiLines.TryGetValue(subTEIDToken, out multi))
                        {
                            // doesn't exist, add both lines
                            multi = new MultiLineProxy();
                            multi.Lines.Add(_lineRenderProxies[otherMultiTeid]);
                            _multiLines.Add(subTEIDToken, multi);
                        }

                        if (multi.Lines.All(l => l.BaseLine != proxy.BaseLine))
                            multi.Lines.Add(proxy);
                    }
                    else
                    {
                        lineStationMap.Add(subTEIDToken, line.TEID);
                    }
                }
            }

            if (lineStationMap != null)
                lineStationMap.Clear();
            lineStationMap = null;

        }



        /// <summary>
        /// Draw the layer. 
        /// </summary>
        /// <param name="renderTime">Time since last render.</param>
        /// <remarks>include base.Render(renderTime); in overloads to preserve updating RenderTime field.</remarks>
        public override void Render(RenderTime renderTime)
        {
            base.Render(renderTime);

            if (!MM_Repository.OverallDisplay.ShowLines) return;

            _isBlinking = ((MM_Network_Map_DX)Surface).IsBlinking;

            var tx = Surface.RenderTarget2D.Transform;
            Surface.RenderTarget2D.Transform = Matrix3x2.Translation(-Surface.Coordinates.TopLeftXY.X, -Surface.Coordinates.TopLeftXY.Y);

            var coordinateBounds = Surface.Coordinates.GetBounds();

            DrawMultiLines(coordinateBounds);
            DrawLines(renderTime, coordinateBounds);

            Surface.RenderTarget2D.Transform = Matrix3x2.Identity;

            Surface.RenderTarget2D.Transform = tx;
        }


        private void DrawMultiLines(RawRectangleF coordinateBounds)
        {
            int zoomLevel = Surface.Coordinates.ZoomLevel;

            foreach (var multi in _multiLines)
            {
                if (multi.Value.Lines == null || multi.Value.Lines.Count <= 0)
                    continue;

                var firstLine = multi.Value.Lines.First();
                if (firstLine.BlackStartHidden || !IsLineVisible(coordinateBounds, firstLine, zoomLevel)) continue;
                if (firstLine.BaseLine == null || firstLine.BaseLine.LineEnergizationState == null)
                    continue;
                var energizationState = firstLine.BaseLine.LineEnergizationState;
                var width = energizationState.Width + MM_Repository.OverallDisplay.MultipleLineWidthAddition;

                var colorBrush = firstLine.BlackStartDim ? Surface.Brushes.GetBrush(MM_Repository.OverallDisplay.BlackstartDim(energizationState.ForeColor)) : Surface.Brushes.GetBrush(System.Drawing.Color.Thistle);

                var style = _solidStyle;

                if (multi.Value.Lines.Any(l => l.BaseLine.LineEnergizationState != l.BaseLine.KVLevel.Energized))
                    style = _dashStyle;

                for (int i = 0; i < firstLine.Coordinates.Count - 1; i++)
                {
                    Surface.RenderTarget2D.DrawLine(firstLine.Coordinates[i], firstLine.Coordinates[i + 1], colorBrush, width, style);
                }
            }
        }

        private void DrawLines(RenderTime renderTime, RawRectangleF coordinateBounds)
        {
            if (_lineFont == null || _lineFont.IsDisposed)
                _lineFont = Surface.Fonts.GetTextFormat(MM_Repository.OverallDisplay.NetworkMapFont);
            int zoomLevel = Surface.Coordinates.ZoomLevel;

            foreach (var lineKvp in _lineRenderProxies)
            {
                var proxy = lineKvp.Value;
                proxy.Visible = false;

                var line = proxy==null?null:proxy.BaseLine;


                if (line == null || proxy.BlackStartHidden || !IsLineVisible(coordinateBounds, proxy, zoomLevel)) continue;

                bool showViolation = ((MM_Network_Map_DX)Surface).IsBlinking;
                MM_DisplayParameter displayOptions = line.LineDisplay(_violationViewer.ShownViolations, Surface, showViolation);
                var stroke = displayOptions.ForePen.DashStyle;
                var color = displayOptions.ForePen.Color;
                var width = displayOptions.ForePen.Width;
                //var energizationState = line.LineEnergizationState;

                // set dash style
                var style = _solidStyle;

                if (stroke == System.Drawing.Drawing2D.DashStyle.Dash)
                    style = _dashStyle;
                else if (stroke == System.Drawing.Drawing2D.DashStyle.DashDot || stroke == System.Drawing.Drawing2D.DashStyle.DashDotDot)
                    style = _dotDashStyle;

                if (proxy.BlackStartDim)
                    color = MM_Repository.OverallDisplay.BlackstartDim(color);

                var colorBrush = Surface.Brushes.GetBrush(color);
                proxy.Visible = true;

                for (int i = 0; i < proxy.Coordinates.Count - 1; i++)
                {
                    Surface.RenderTarget2D.DrawLine(proxy.Coordinates[i], proxy.Coordinates[i + 1], colorBrush, width, style);
                }

                if (zoomLevel >= MM_Repository.OverallDisplay.LineFlows || (proxy.BaseLine is MM_Tie && MM_Repository.OverallDisplay.TieFlowDirectionVisible))
                    UpdateAndDrawFlowArrow(renderTime, proxy, zoomLevel, displayOptions, colorBrush);

                if (zoomLevel > (line.KVLevel.StationNames + 1) && (line.KVLevel.ShowMVARText || line.KVLevel.ShowMVAText || line.KVLevel.ShowMWText || line.KVLevel.ShowPercentageText || line.KVLevel.ShowLineName))
                {
                    // Draw Line Name
                    string nameText = line.GetLineText();
                    if (proxy.NameText == null || proxy.NameText != nameText || (proxy.NameTextLayout != null && proxy.NameTextLayout.IsDisposed))
                    {
                        if (proxy.NameTextLayout!=null)
                        proxy.NameTextLayout.Dispose();
                        proxy.NameText = nameText;
                        proxy.NameTextLayout = new TextLayout(Surface.FactoryDirectWrite, nameText, _lineFont, 300, 300);
                    }

                    var tx = Surface.RenderTarget2D.Transform;
                    Surface.RenderTarget2D.Transform = Matrix3x2.Rotation(proxy.Angle, proxy.Center) * Matrix3x2.Translation(-Surface.Coordinates.TopLeftXY.X, -Surface.Coordinates.TopLeftXY.Y);
                    Surface.RenderTarget2D.DrawTextAtPoint(proxy.NameTextLayout, colorBrush, proxy.Center.X, proxy.Center.Y, centerX: true);
                    Surface.RenderTarget2D.Transform = tx;
                }
                else
                {
                    if (proxy.NameTextLayout!=null)
                    proxy.NameTextLayout.Dispose();
                    proxy.NameTextLayout = null;
                }
            }

            // render flowgate text on top of lines
            if (MM_Repository.OverallDisplay.ShowFlowgates)
            {
                foreach (var proxy in _lineRenderProxies.Values)
                {
                    if (!proxy.Visible) continue;

                    var line = proxy.BaseLine;
                    var flowgate = line.Contingencies.Where(f => f.Type == "Flowgate").Cast<MM_Flowgate>().OrderByDescending(f => Math.Abs(f.PercentLoaded)).FirstOrDefault(f => Math.Abs(f.PercentLoaded) > 0.90);
                    if (flowgate != null && Math.Abs(flowgate.PCTGFlow) > float.Epsilon)
                    {
                        var fgName = flowgate.Name;
                        bool monitored = flowgate.MonitoredElements.Contains(line.LineTEID);
                        bool contingent = flowgate.ContingentElements.Contains(line.LineTEID);
                        fgName += monitored ? " (M)" : contingent ? " (C)" : "()";

                        if (proxy.FlowgateText != fgName || proxy.FlowgateText == null || (proxy.FlowgateTextLayout != null && proxy.FlowgateTextLayout.IsDisposed))
                        {
                            // rebuild text layout
                            if (proxy.FlowgateTextLayout!=null)
                            proxy.FlowgateTextLayout.Dispose();
                            proxy.FlowgateText = fgName;
                            proxy.FlowgateTextLayout = new TextLayout(Surface.FactoryDirectWrite, fgName, _lineFont, 300, 300);
                        }


                        var fgcolor = SharpDX.Color.Gray;

                        if (flowgate.PercentLoaded > 1)
                            fgcolor = SharpDX.Color.Red;
                        else if (Math.Abs(flowgate.PercentLoaded) > .95)
                            fgcolor = SharpDX.Color.Orange;
                        else if (Math.Abs(flowgate.PercentLoaded) > .90)
                            fgcolor = SharpDX.Color.Yellow;

                        var tx = Surface.RenderTarget2D.Transform;
                        if (zoomLevel > 8)
                            Surface.RenderTarget2D.Transform = Matrix3x2.Rotation(proxy.Angle, proxy.Center) * Matrix3x2.Translation(-Surface.Coordinates.TopLeftXY.X, -Surface.Coordinates.TopLeftXY.Y);

                        Surface.RenderTarget2D.DrawTextAtPoint(proxy.FlowgateTextLayout, Surface.Brushes.GetBrush(fgcolor), proxy.Center.X, proxy.Center.Y - proxy.FlowgateTextLayout.Metrics.Height, centerX: true);
                        Surface.RenderTarget2D.Transform = tx;
                    }
                }
            }
        }

        private void UpdateAndDrawFlowArrow(RenderTime time, LineProxy proxy, int zoomLevel, MM_DisplayParameter displayOptions, SharpDX.Direct2D1.Brush brush)
        {
            //Now, draw the line as appropriate
            if (proxy.Coordinates == null || proxy.Coordinates.Count == 0)
                return;

            // reset if bad
            if (proxy.FlowPositionPercentage > 1)
                proxy.FlowPositionPercentage = 1;
            if (proxy.FlowPositionPercentage < float.Epsilon)
                proxy.FlowPositionPercentage = 0;

            // reset if bad
            if (proxy.FlowVarPositionPercentage > 1)
                proxy.FlowVarPositionPercentage = 1;
            if (proxy.FlowVarPositionPercentage < float.Epsilon)
                proxy.FlowVarPositionPercentage = 0;

            var Line = proxy.BaseLine;

            // don't render dead lines flow arrows
            if (Line == null || Math.Abs(Line.MVAFlow) < 0.1 || (Line.NearBus != null && Line.NearBus.Dead) || (Line.FarBus != null && Line.FarBus.Dead))
                return;

            //Determine how large our arrow should be

            float baseSize = proxy.BaseLine.KVLevel.MVASize;
            float threshold = proxy.BaseLine.KVLevel.MVAThreshold / 100.0f;
            bool flowKvVisible = proxy.BaseLine.KVLevel.ShowMVA;
            float mvaSize = Line.KVLevel.MVASize;
            float lineWidth = displayOptions.Width;
            float flowIncrement = 0;
            float flowVarIncrement = 0;
            float flowSize;

            bool isTie = Line is MM_Tie;


            if ((!MM_Repository.OverallDisplay.ShowLineFlows || !flowKvVisible) && !(isTie && MM_Repository.OverallDisplay.TieFlowDirectionVisible))
                return;
            //MM_Repository.OverallDisplay.
            // absolute vs % limit
            bool absoluteValueFlowSize = zoomLevel < MM_Repository.OverallDisplay.LineFlows;
            if (isTie && MM_Repository.OverallDisplay.TieFlowDirectionVisible)
            {
                // show enhanced size/color for AC Tie lines if TieFlowDirectionVisible == true
                if (MM_Repository.KVLevels.ContainsKey("Tie"))
                    mvaSize = MM_Repository.KVLevels["Tie"].MVASize;
                else if (MM_Repository.KVLevels.ContainsKey("DCTie"))
                {
                    mvaSize = MM_Repository.KVLevels["DCTie"].MVASize;
                }
                else
                    mvaSize = 5;
            }
            else if (Line.KVLevel.Name.Contains("Tie"))
            {
                isTie = true;
            }
            bool moveForward = true;
            float zoomScale = (float)zoomLevel / MM_Repository.OverallDisplay.LineFlows;

            bool varForward = true;
            var varFlowSize = Math.Min(50, lineWidth * baseSize);

            if (absoluteValueFlowSize)
            {
                if (Math.Abs(Line.MVAFlow) < float.Epsilon)
                    flowSize = flowIncrement = 0;
                else
                {
                    moveForward = Line.MVAFlowDirection == Line.ConnectedStations[1];
                    varForward = Line.MVARFlowDirection == Line.ConnectedStations[1];
                    flowSize = (zoomLevel / 20f) * mvaSize * Line.MVAFlow * 0.02f;// + 1.5f * lineWidth;
                    varFlowSize = (zoomLevel / 20f) * mvaSize * Line.MVAFlow * 0.04f;// + 1.5f * lineWidth;
                }
            }
            else
            {
                // relative size based on limit
                if (Line.NormalLimit == 0)
                    flowSize = flowIncrement = 0;
                else
                {
                    var percentLimit = Line.MVAFlow / Line.NormalLimit;
                    var percentVar = Line.MVARFlow / Line.NormalLimit;

                    if (percentLimit < threshold) return;

                    moveForward = Line.MVAFlowDirection == Line.ConnectedStations[1];
                    varForward = Line.MVARFlowDirection == Line.ConnectedStations[1];
                    flowSize = 10f * mvaSize * percentLimit * ((float)(MathUtil.Clamp(lineWidth, 1, 3) / 2f));
                    varFlowSize = 30f * mvaSize * percentVar * ((float)(MathUtil.Clamp(lineWidth, 1, 3) / 2f));
                }
            }
            flowSize = MathUtil.Clamp(flowSize, 0, 100);
            varFlowSize = MathUtil.Clamp(varFlowSize, 0, 100);

            if (float.IsNaN(flowSize) || flowSize < 0)
            {
                flowSize = flowIncrement = 0;
                varFlowSize = flowVarIncrement = 0;
            }
            else
            {
                flowIncrement = MathUtil.Clamp(0.5f * zoomScale * flowSize, -20f, 20f) * (float)time.ElapsedTime.TotalSeconds;
                flowVarIncrement = MathUtil.Clamp(0.5f * zoomScale * varFlowSize, -20f, 20f) * (float)time.ElapsedTime.TotalSeconds;
            }

            // scale up arrow (but not speed) based on zoom


            if (flowSize < lineWidth + 0.25f || proxy.Length <= 0 ||
                (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideExternalElements && !Line.IsInternal))
            {
                return;
            }

            // Move MVA flow arrow
            if (moveForward)
                proxy.FlowPositionPercentage += flowIncrement / proxy.Length;
            else
                proxy.FlowPositionPercentage -= flowIncrement / proxy.Length;

            if (proxy.FlowPositionPercentage < float.Epsilon)
                proxy.FlowPositionPercentage = 1 - proxy.FlowPositionPercentage;
            else
                proxy.FlowPositionPercentage -= (float)Math.Floor(proxy.FlowPositionPercentage); // just keep the decimal

            // Move Var Arrow
            if (varForward)
                proxy.FlowVarPositionPercentage += flowVarIncrement / proxy.Length;
            else
                proxy.FlowVarPositionPercentage -= flowVarIncrement / proxy.Length;

            if (proxy.FlowVarPositionPercentage < float.Epsilon)
                proxy.FlowVarPositionPercentage = 1 - proxy.FlowVarPositionPercentage;
            else
                proxy.FlowVarPositionPercentage -= (float)Math.Floor(proxy.FlowVarPositionPercentage); // just keep the decimal


            if (zoomLevel < MM_Repository.OverallDisplay.LineFlows)
            {
                proxy.FlowPositionPercentage = 0.5f;
                proxy.FlowVarPositionPercentage = 0.5f;
            }

            SolidColorBrush borderBrush = null;
            if (isTie && MM_Repository.OverallDisplay.TieFlowDirectionVisible)
            {
                bool importing = (Line.Substation1.IsInternal && (Line.MVAFlowDirection == Line.Substation1)) ||
                                 (Line.Substation2.IsInternal && (Line.MVAFlowDirection == Line.Substation2));

                var color = importing ? MM_Repository.OverallDisplay.TieFlowImportColor : MM_Repository.OverallDisplay.TieFlowExportColor;
                if (proxy.BlackStartDim)
                    color = MM_Repository.OverallDisplay.BlackstartDim(color);

                borderBrush = Surface.Brushes.GetBrush(color);
            }

            // add border around flow arrow for tie

            //proxy.FlowPositionPercentage = 0;
            float rotAngle = 0;
            RawVector2 ArrowTip = proxy.GetPosition(proxy.FlowPositionPercentage, out rotAngle);

            float varrotAngle = 0;
            RawVector2 varArrowTip = proxy.GetPosition(proxy.FlowVarPositionPercentage, out varrotAngle);

            //var
            if (MM_Repository.OverallDisplay.VarFlows)
                DrawArrow(null, brush, varArrowTip, varrotAngle, varFlowSize, varForward, 0.1f);
            
            // mva
            DrawArrow(brush, borderBrush, ArrowTip, rotAngle, flowSize, moveForward, 0.25f);
        }

        private void DrawArrow(Brush brush, Brush borderBrush, RawVector2 arrowTip, float rotAngle, float flowSize, bool moveForward, float borderWidth)
        {
            var transform = Surface.RenderTarget2D.Transform;

            var scaleVector = new Vector2(flowSize, flowSize);
            if (!moveForward) rotAngle -= (float)Math.PI;
            var translationVector = arrowTip + ((Matrix3x2)Surface.RenderTarget2D.Transform).TranslationVector;

            var scale = Matrix3x2.Scaling(scaleVector);
            var rotation = Matrix3x2.Rotation(rotAngle, Vector2.Zero);
            var translation = Matrix3x2.Translation(translationVector);

            Surface.RenderTarget2D.Transform = scale * rotation * translation;

            if (brush != null)
                Surface.RenderTarget2D.FillGeometry(LineFlow, brush);

            if (borderBrush != null)
                Surface.RenderTarget2D.DrawGeometry(LineFlow, borderBrush, borderWidth);

            Surface.RenderTarget2D.Transform = transform;
        }


        private static bool IsLineVisible(RawRectangleF coordinateBounds, LineProxy line, int zoomLevel)
        {
            if (line == null || line.Coordinates==null || line.Coordinates.Count<2)
                return false;
            if (line.BaseLine == null)
                return false;
            if (!coordinateBounds.Overlaps(line.Bounds))
                return false;
            if (line.BaseLine.KVLevel.VisibilityByZoom > zoomLevel)
                return false;
            if (!line.BaseLine.KVLevel.ShowEnergized && (line.BaseLine.LineEnergizationState == line.BaseLine.KVLevel.Energized))
                return false;
            if (!line.BaseLine.KVLevel.ShowUnknown && (line.BaseLine.LineEnergizationState == line.BaseLine.KVLevel.Unknown))
                return false;
            if (!line.BaseLine.KVLevel.ShowDeEnergized && (line.BaseLine.LineEnergizationState == line.BaseLine.KVLevel.DeEnergized))
                return false;
            if (!line.BaseLine.KVLevel.ShowPartiallyEnergized && (line.BaseLine.LineEnergizationState == line.BaseLine.KVLevel.PartiallyEnergized))
                return false;
            if (line.BaseLine.KVLevel.VisibilityThreshold > 0f && line.BaseLine.KVLevel.VisibilityThreshold < line.BaseLine.LinePercentage)
                return false;
            if (line.BaseLine.KVLevel.VisibilityThreshold < 0f && -line.BaseLine.KVLevel.VisibilityThreshold > line.BaseLine.LinePercentage)
                return false;

            return true;
        }


    }
}