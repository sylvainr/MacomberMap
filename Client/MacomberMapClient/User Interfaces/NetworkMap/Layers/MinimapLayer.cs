using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using MacomberMapClient.User_Interfaces.NetworkMap.Proxies;
using MacomberMapClient.User_Interfaces.Violations;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using BitmapRenderTarget = SharpDX.Direct2D1.BitmapRenderTarget;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    public class MinimapLayer : RenderLayer
    {
        private bool _isDirty = true;
        private RegionProxy _state;
        private TextFormat _font;
        private BitmapRenderTarget _target;
        private Vector2 _scale;
        private Vector2 _location;
        private MM_Violation_Viewer _violationViewer;
        private MM_Coordinates _coordinates;
        private Vector2 _start;
        private Vector2 _end;
        private SharpDX.RectangleF _viewLocation;
        private TimeSpan _lastRender = TimeSpan.Zero;

        public RegionProxy State {get{return _state;}}

        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        /// <param name="name">The name of the layer.</param>
        /// <param name="order">The order of the layer</param>
        public MinimapLayer(Direct3DSurface surface, MM_Violation_Viewer violationViewer) : base(surface, "Minimap", 99999)
        {
            _violationViewer = violationViewer;

            var map = this.Surface as MM_Network_Map_DX;
            _coordinates = map.Coordinates;
            violationViewer.ViolationsChanged += (s, e) => _isDirty = true;
            // Hook events to update render state
            MM_Repository.ViewChanged += v => _isDirty = true;
            _coordinates.Panned += (center, newCenter) => _isDirty = true;
            _coordinates.Zoomed += (zoom, newZoom) => _isDirty = true;
            _coordinates.LassoDrawing += () => _isDirty = true;
        }

        public bool PointOnMap(RawVector2 pixel)
        {
            return _viewLocation.Contains(pixel);
        }

        public void CenterMap(RawVector2 pixel)
        {
            if (!PointOnMap(pixel)) return;

            var relativePixel = pixel - _viewLocation.TopLeft;

            var center = new PointF(
                ((relativePixel.X / (float)_target.PixelSize.Width) * (MM_Repository.Counties["STATE"].Max.X - MM_Repository.Counties["STATE"].Min.X)) + MM_Repository.Counties["STATE"].Min.X,
                ((relativePixel.Y / (float)_target.PixelSize.Height) * (MM_Repository.Counties["STATE"].Min.Y - MM_Repository.Counties["STATE"].Max.Y)) + MM_Repository.Counties["STATE"].Max.Y);
            _coordinates.Center = center;
            //  = new System.Drawing.PointF(actualPixel.X, actualPixel.Y);
        }

        /// <summary>
        /// Perform calculations and update data.
        /// </summary>
        /// <param name="updateTime">Time since last update.</param>
        /// <remarks>include base.Update(renderTime); in overloads to preserve updating UpdateTime field.</remarks>
        public override void Update(RenderTime updateTime)
        {
            base.Update(updateTime);
        }

        /// <summary>
        /// Draw the layer. 
        /// </summary>
        /// <param name="renderTime">Time since last render.</param>
        /// <remarks>include base.Render(renderTime); in overloads to preserve updating RenderTime field.</remarks>
        public override void Render(RenderTime renderTime)
        {
            base.Render(renderTime);


            if (_lastRender.TotalSeconds > 4)
                _isDirty = true;


            if (_isDirty)
            {
                _target.BeginDraw();
                _target.Clear(new RawColor4(0, 0, 0, 0.95f));
                _target.Transform = Matrix3x2.Translation(-_location);

                // draw borders
                _target.AntialiasMode = AntialiasMode.PerPrimitive;
                _target.FillGeometry(_state.Geometry, Surface.Brushes.GetBrush(new RawColor4(0, 0, 0, .9f)));
                _target.DrawGeometry(_state.Geometry, Surface.Brushes.GetBrush(_state.Color), 1);


                // draw violations
                DrawVisibleViolations();

                // draw crosshair
                DrawCrosshair();

                // Draw lasso
                DrawLasso();

                // draw view rectangle
                DrawViewRectangle();

                // DrawText();

                _target.Transform = Matrix3x2.Identity;
                _target.EndDraw();
                _isDirty = false;
                _lastRender = TimeSpan.Zero;
            }
            else
                _lastRender += renderTime.ElapsedTime;



            var minimap = _target.Bitmap;

            var tx = Surface.RenderTarget2D.Transform;
            Surface.RenderTarget2D.Transform = Matrix3x2.Identity;
            Surface.RenderTarget2D.DrawBitmap(
                minimap,
                _viewLocation,
                1f,
                BitmapInterpolationMode.Linear);
            Surface.RenderTarget2D.Transform = tx;
        }

        private void DrawLasso()
        {
            if (_coordinates == null) return;

            if (_coordinates.LassoPoints.Count > 1)
            {
                var points = MM_Coordinates.ConvertPoints(_coordinates.LassoPoints, 4).Select(p => p * _scale).ToList();
                RawRectangleF bounds;
                var geometry = Surface.CreateRegionPathGeometry(points, out bounds, filled: true, closed: true, simplify: true);

                _target.FillGeometry(geometry, Surface.Brushes.GetBrush(_coordinates.LassoColor, 0.5f));
                _target.DrawGeometry(geometry, Surface.Brushes.GetBrush(Color4.White), 2f);
                geometry.Dispose();
            }
            else if (!_coordinates.LassoEnd.IsEmpty)
            {
                var start = ConvertPoint(_coordinates.LassoStart);
                var end = ConvertPoint(_coordinates.LassoEnd);

                var rawRectangleF = new RawRectangleF(start.X, start.Y, end.X, end.Y);
                _target.FillRectangle(rawRectangleF, Surface.Brushes.GetBrush(_coordinates.LassoColor, 0.5f));
                _target.DrawRectangle(rawRectangleF, Surface.Brushes.GetBrush(Color4.White), 2f);
            }
        }

        private void DrawText()
        {
            float FlowIn = -Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieIn];
            float FlowOut = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieOut];
            float Gen = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Gen];
            float GenC = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.GenCapacity];
            float GenEC = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.GenEmrgCapacity];
            float Ld = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Load];

            string flowIn = String.Format("In: \n{0:#,##0}\nMW",FlowIn);
            string flowOut = String.Format("Out:\n{0:#,##0}\nMW",FlowOut);

            // TODO: Emer Mod use GenEC
            string gen = String.Format("Gen:\n{0:#,##0} MW\nAvail:{1:#,##0} MW",Gen,GenC);
            string load = String.Format("Load:\n{0:#,##0}\nMW",Ld);


            var flowInTextLayout = new TextLayout(Surface.FactoryDirectWrite, flowIn, _font, 300, 300);
            var flowOutTextLayout = new TextLayout(Surface.FactoryDirectWrite, flowOut, _font, 300, 300);
            var genTextLayout = new TextLayout(Surface.FactoryDirectWrite, gen, _font, 300, 300);
            var loadTextLayout = new TextLayout(Surface.FactoryDirectWrite, load, _font, 300, 300);

            var brush = Surface.Brushes.GetBrush(Color4.White);
            var bgBrush = Surface.Brushes.GetBrush(Color4.Black, 0.75f);

            var tx = _target.Transform;
            _target.Transform = Matrix3x2.Identity;

            _target.DrawTextAtPoint(flowInTextLayout, brush, 10, 10, brushBackground: bgBrush);
            _target.DrawTextAtPoint(flowOutTextLayout, brush, _target.PixelSize.Width - flowOutTextLayout.Metrics.Width - 10, 10, brushBackground: bgBrush);
            _target.DrawTextAtPoint(genTextLayout, brush, 10, _target.PixelSize.Height - genTextLayout.Metrics.Height - 10, brushBackground: bgBrush);
            _target.DrawTextAtPoint(loadTextLayout, brush, _target.PixelSize.Width - loadTextLayout.Metrics.Width - 10, _target.PixelSize.Height - loadTextLayout.Metrics.Height - 10, brushBackground: bgBrush);


            flowInTextLayout.Dispose();
            flowOutTextLayout.Dispose();
            genTextLayout.Dispose();
            loadTextLayout.Dispose();

            _target.Transform = tx;
        }

        private void DrawVisibleViolations()
        {
            foreach (var violation in _violationViewer.ShownViolations)
            {
                var violationKey = violation.Key;
                var violationValue = violation.Value;

                MM_AlarmViolation_Type worstVisibleViolation = null;
                try
                {
                    worstVisibleViolation = violationKey.SubstationOrLine.WorstVisibleViolation(_violationViewer.ShownViolations, null);
                    if (worstVisibleViolation == null)
                        continue;
                }
                catch (Exception exp)
                {
                    continue;
                }
                var brush = Surface.Brushes.GetBrush(worstVisibleViolation.ForeColor);
                float violationWidth = worstVisibleViolation.Width;

                if (violationKey.SubstationOrLine is MM_Line)
                {
                    var line = violationKey.SubstationOrLine as MM_Line;

                    var lineStart = line.Coordinates.First();
                    var lineEnd = line.Coordinates.Last();

                    var pixelStart = ConvertPoint(lineStart);
                    var pixelEnd = ConvertPoint(lineEnd);

                    _target.AntialiasMode = AntialiasMode.PerPrimitive;
                    _target.DrawLine(pixelStart, pixelEnd, brush, violationWidth);
                }
                else if (violationKey.SubstationOrLine is MM_Substation)
                {
                    var station = violationKey.SubstationOrLine as MM_Substation;

                    var center = ConvertPoint(station.LngLat);
                    _target.AntialiasMode = AntialiasMode.Aliased;
                    _target.FillRectangle(new RawRectangleF(center.X - violationWidth, center.Y - violationWidth, center.X + violationWidth, center.Y + violationWidth), brush);
                }
            }
        }

        private Vector2 ConvertPoint(PointF point)
        {
            return MM_Coordinates.LngLatToScreenVector2(point, 4) * _scale;
        }

        private Vector2 ConvertPoint(Vector2 point)
        {
            return MM_Coordinates.LngLatToScreenVector2(point, 4) * _scale;
        }

        private void DrawCrosshair()
        {
            _target.AntialiasMode = AntialiasMode.Aliased;
            if (_violationViewer.SelectedUIDs.Count > 0)
            {
                var selected = _violationViewer.SelectedUIDs;
                foreach (var violation in selected)
                {
                    var violationKey = violation.Key;
                    var violationValue = violation.Value;

                    var brush = Surface.Brushes.GetBrush(violationKey.Type.ForeColor);
                    float violationWidth = violationKey.Type.Width;

                    if (violationKey.SubstationOrLine is MM_Line)
                    {
                        var line = violationKey.SubstationOrLine as MM_Line;

                        var lineStart = line.Coordinates.First();
                        var lineEnd = line.Coordinates.Last();

                        var pixelStart = ConvertPoint(lineStart);
                        var pixelEnd = ConvertPoint(lineEnd);

                        float minX = (float)Math.Round(Math.Min(pixelStart.X, pixelEnd.X), 0) - violationWidth * 2;
                        float minY = (float)Math.Round(Math.Min(pixelStart.Y, pixelEnd.Y), 0) - violationWidth * 2;
                        float maxX = (float)Math.Round(Math.Max(pixelStart.X, pixelEnd.X), 0) + violationWidth * 2;
                        float maxY = (float)Math.Round(Math.Max(pixelStart.Y, pixelEnd.Y), 0) + violationWidth * 2;


                        _target.DrawRectangle(new RawRectangleF(minX, minY, maxX, maxY), brush, violationWidth);

                        float midX = (float)Math.Round((minX + maxX) / 2f, 0);
                        float midY = (float)Math.Round((minY + maxY) / 2f, 0);

                        violationWidth = (float)Math.Truncate(violationWidth);

                        _target.DrawLine(new Vector2(midX, 0), new Vector2(midX, minY), brush, violationWidth);
                        _target.DrawLine(new Vector2(midX, maxY), new Vector2(midX, _target.PixelSize.Height + _location.Y), brush, violationWidth);

                        _target.DrawLine(new Vector2(0, midY), new Vector2(midX, midY), brush, violationWidth);
                        _target.DrawLine(new Vector2(maxX, midY), new Vector2(_target.PixelSize.Width + _location.X, midY), brush, violationWidth);
                    }
                    else if (violationKey.SubstationOrLine is MM_Substation)
                    {
                        var station = violationKey.SubstationOrLine as MM_Substation;
                        violationWidth = (float)Math.Truncate(violationWidth);

                        var center = ConvertPoint(station.LngLat);
                        _target.DrawRectangle(new RawRectangleF(center.X - violationWidth * 2, center.Y - violationWidth * 2, center.X + violationWidth * 2, center.Y + violationWidth * 2), brush);

                        _target.DrawLine(new Vector2(center.X, 0), new Vector2(center.X, center.Y - violationWidth * 2), brush, violationWidth);
                        _target.DrawLine(new Vector2(center.X, center.Y + violationWidth * 2), new Vector2(center.X, _target.PixelSize.Height + _location.Y), brush, violationWidth);

                        _target.DrawLine(new Vector2(0, center.Y), new Vector2(center.X - violationWidth * 2, center.Y), brush, violationWidth);
                        _target.DrawLine(new Vector2(center.X + violationWidth * 2, center.Y), new Vector2(_target.PixelSize.Width + _location.X, center.Y), brush, violationWidth);
                    }

                    break;
                }
            }
        }

        private void DrawViewRectangle()
        {
            if (_coordinates != null)
            {

                var view = _coordinates.GetRawViewLngLat();
                var topLeft = ConvertPoint(new Vector2(view.Left, view.Top));
                var bottomRight = ConvertPoint(new Vector2(view.Right, view.Bottom));

                var rect = new SharpDX.RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);

                var whiteBrush = Surface.Brushes.GetBrush(Color4.White);
                _target.DrawRectangle(rect, whiteBrush, 1);

                if (_coordinates.ZoomLevel > 9)
                {
                    _target.DrawLine(new Vector2(_location.X, rect.Center.Y), new Vector2(rect.Left, rect.Center.Y), whiteBrush, 1);
                    _target.DrawLine(new Vector2(rect.Right, rect.Center.Y), new Vector2(_target.PixelSize.Width + _location.X, rect.Center.Y), whiteBrush, 1);
                    _target.DrawLine(new Vector2(rect.Center.X, _location.Y), new Vector2(rect.Center.X, rect.Top), whiteBrush, 1);
                    _target.DrawLine(new Vector2(rect.Center.X, rect.Bottom), new Vector2(rect.Center.X, _target.PixelSize.Height + _location.Y), whiteBrush, 1);
                }
            }
        }

        /// <summary>
        /// Load Content and initialize unmanaged resources.
        /// </summary>
        public override void LoadContent()
        {
            int width = 256;
            int height = 256;

            var size = Surface.RenderTarget2D.PixelSize;
            var sizeV = new Vector2(size.Width, size.Height);

            _start = sizeV - new Vector2(width, height) - new Vector2(10, 10);
            _end = sizeV - new Vector2(10, 10);
            _viewLocation = new SharpDX.RectangleF(_start.X, _start.Y, _end.X - _start.X, _end.Y - _start.Y);

            base.LoadContent();

            if (_font == null || _font.IsDisposed)
                _font = Surface.Fonts.GetTextFormat(MM_Repository.OverallDisplay.NetworkMapFont, 5);

            if (_target == null || _target.IsDisposed)
                _target = Surface.CreateRenderTarget(width, height);

            MM_Boundary State;
            if (MM_Repository.Counties.TryGetValue("STATE", out State))
            {
                RawRectangleF bounds;
                var geometry = Surface.CreateRegionPathGeometry(MM_Coordinates.ConvertPoints(State.Coordinates, 4).ToList(), out bounds, filled: true, simplify: true);
                geometry.Dispose();

                _scale = new Vector2(width / (bounds.Right - bounds.Left), height / (bounds.Bottom - bounds.Top));

                var geometryScaled = Surface.CreateRegionPathGeometry(MM_Coordinates.ConvertPoints(State.Coordinates, 4).ToList(), out bounds, filled: true, compression: 0.001f, simplify: true, scaleX: _scale.X, scaleY: _scale.Y);
                _location = new Vector2(bounds.Left, bounds.Top);

                _state = new RegionProxy(geometryScaled, State.DisplayParameter.ForeColor, State.DisplayParameter.Width, bounds);
            }
        }


        /// <summary>
        /// Unload content and cleanup unmanaged resources.
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            if (_state != null && !_state.IsDisposed)
            {
                _state.Dispose();
                _state = null;
            }
            if (_font != null)
            _font.Dispose();
            _font = null;
            if (_target != null)
            _target.Dispose();
            _target = null;
        }
    }
}