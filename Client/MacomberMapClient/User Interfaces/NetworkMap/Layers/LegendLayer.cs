using System;
using System.Collections.Generic;
using System.Linq;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    public class LegendLayer : RenderLayer
    {

        Dictionary<string, TextLayout> _strings = new Dictionary<string, TextLayout>();
        private TextFormat _legendFont;

        private StrokeStyle _dashStyle;
        private StrokeStyle _dotDashStyle;
        private StrokeStyle _solidStyle;
        private bool _isBlinking;
        private PathGeometry _slice;

        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        /// <param name="name">The name of the layer.</param>
        /// <param name="order">The order of the layer</param>
        public LegendLayer(Direct3DSurface surface, string name = "Legend", int order = 9997) : base(surface, name, order)
        {

        }

        /// <summary>
        /// Unload content and cleanup unmanaged resources.
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            if (_slice != null)
                _slice.Dispose();
            _slice = null;

            var legendItems = _strings.Values.ToList();
            _strings.Clear();
            foreach (var legendItem in legendItems)
            {
                legendItem.Dispose();
            }
            legendItems.Clear();
            _legendFont = null;
        }

        /// <summary>
        /// Load Content and initialize unmanaged resources.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            _slice = Surface.Factory2D.CreatePieSlice(0.7f, 10);

            _dashStyle = new StrokeStyle(Surface.Factory2D,
                                        new StrokeStyleProperties() { DashCap = CapStyle.Flat, DashStyle = DashStyle.Custom, DashOffset = 10f, LineJoin = LineJoin.MiterOrBevel, MiterLimit = 2f }, new float[] { 2, 2 });
            _dotDashStyle = new StrokeStyle(Surface.Factory2D,
                                         new StrokeStyleProperties() { DashCap = CapStyle.Flat, DashStyle = DashStyle.Custom, DashOffset = 10f, LineJoin = LineJoin.MiterOrBevel, MiterLimit = 2f }, new float[] { 3, 2, 1, 2 });
            _solidStyle = new StrokeStyle(Surface.Factory2D,
                                         new StrokeStyleProperties() { DashCap = CapStyle.Flat, DashStyle = DashStyle.Solid, LineJoin = LineJoin.MiterOrBevel, MiterLimit = 2f });
        }


        /// <summary>
        /// Draw the layer. 
        /// </summary>
        /// <param name="renderTime">Time since last render.</param>
        /// <remarks>include base.Render(renderTime); in overloads to preserve updating RenderTime field.</remarks>
        public override void Render(RenderTime renderTime)
        {
            base.Render(renderTime);
            if (MM_Repository.OverallDisplay.ShowLegend)
            {
                _isBlinking = ((MM_Network_Map_DX)Surface).IsBlinking;
                var tx = Surface.RenderTarget2D.Transform;
                Surface.RenderTarget2D.Transform = Matrix3x2.Identity;


                DrawLegend();
                DrawLinesLegend();

                Surface.RenderTarget2D.Transform = tx;
            }


        }

        private void DrawLegend()
        {
            if (_legendFont == null || _legendFont.IsDisposed)
                _legendFont = Surface.Fonts.GetTextFormat(MM_Repository.OverallDisplay.NetworkMapFont);

            Vector2 start = new Vector2(30, 30 + MM_Repository.KVLevels.Count * 20 + 10);
            Vector2 end = start + new Vector2(235, 125);

            Surface.RenderTarget2D.FillRectangle(new RawRectangleF(start.X - 10, start.Y - 10, end.X + 20, end.Y + 10), Surface.Brushes.GetBrush(new Color4(0, 0, 0, 0.5f)));

            var position = start + new Vector2(20, 5);
            var offset = new Vector2(50, 0);
            var textOffset = new Vector2(0, 15);
            var whiteBrush = Surface.Brushes.GetBrush(Color4.White);
            foreach (var fuelColor in SubstationLayer.FuelColors)
            {
                if (position.X > end.X)
                {
                    position.X = start.X + 20;
                    position.Y += textOffset.Y * 3;
                }

                var brush = Surface.Brushes.GetBrush(fuelColor.Value, 0.6f);

                if (fuelColor.Key == "LOAD")
                    Surface.RenderTarget2D.FillRectangle(new RectangleF(position.X - 7.5f, position.Y - 7.5f, 15, 15), brush);
                else
                {
                    Surface.RenderTarget2D.FillEllipse(new Ellipse(position, 10, 10), brush);
                }
                using (var layout = new TextLayout(Surface.FactoryDirectWrite, fuelColor.Key, _legendFont, offset.X, 14))
                {
                    var textLoc = position + textOffset;
                    Surface.RenderTarget2D.DrawTextAtPoint(layout, whiteBrush, textLoc.X, textLoc.Y, centerX: true, options: DrawTextOptions.Clip);
                    position += offset;
                }
            }

            var onlineBrush = Surface.Brushes.GetBrush(SubstationLayer.FuelColors["WIND"], 0.6f);

            var tx = Surface.RenderTarget2D.Transform;
            Surface.RenderTarget2D.Transform = Matrix3x2.Translation(position);
            if (_slice != null && !_slice.IsDisposed)
                Surface.RenderTarget2D.FillGeometry(_slice, onlineBrush);
            Surface.RenderTarget2D.Transform = tx;

            Surface.RenderTarget2D.DrawEllipse(new Ellipse(position, 10, 10), onlineBrush);
            using (var layout = new TextLayout(Surface.FactoryDirectWrite, "MW & Max", _legendFont, offset.X, 14))
            {
                var textLoc = position + textOffset;
                Surface.RenderTarget2D.DrawTextAtPoint(layout, whiteBrush, textLoc.X, textLoc.Y, centerX: true, options: DrawTextOptions.Clip);
            }

            position += offset;
            if (position.X > end.X)
            {
                position.X = start.X + 20;
                position.Y += textOffset.Y * 3;
            }

            Surface.RenderTarget2D.DrawEllipse(new Ellipse(position, 10, 10), onlineBrush, 2f, _dashStyle);
            using (var layout = new TextLayout(Surface.FactoryDirectWrite, "Offline", _legendFont, offset.X, 14))
            {
                var textLoc = position + textOffset;
                Surface.RenderTarget2D.DrawTextAtPoint(layout, whiteBrush, textLoc.X, textLoc.Y, centerX: true, options: DrawTextOptions.Clip);
            }

            position += offset;
            if (position.X > end.X)
            {
                position.X = start.X + 20;
                position.Y += textOffset.Y * 3;
            }

            using (var layout = new TextLayout(Surface.FactoryDirectWrite, "Unit", _legendFont, offset.X, 14))
            using (var geom = SharpDxExtensions.CreatePathGeometry(Surface.Factory2D, true, true,
                                                                               new Vector2(position.X - 5, position.Y),
                                                                               new Vector2(position.X, position.Y - 5),
                                                                               new Vector2(position.X + 5, position.Y),
                                                                               new Vector2(position.X, position.Y + 5)))
            {
                Surface.RenderTarget2D.FillGeometry(geom, whiteBrush);

                var textLoc = position + textOffset;
                Surface.RenderTarget2D.DrawTextAtPoint(layout, whiteBrush, textLoc.X, textLoc.Y, centerX: true, options: DrawTextOptions.Clip);
            }

            position += offset;
            if (position.X > end.X)
            {
                position.X = start.X + 20;
                position.Y += textOffset.Y * 3;
            }

            Surface.RenderTarget2D.FillRectangle(new RectangleF(position.X - 2, position.Y - 2, 2 * 2f, 2 * 2f), whiteBrush);
            using (var layout = new TextLayout(Surface.FactoryDirectWrite, "Substation", _legendFont, offset.X, 14))
            {
                var textLoc = position + textOffset;
                Surface.RenderTarget2D.DrawTextAtPoint(layout, whiteBrush, textLoc.X, textLoc.Y, centerX: true, options: DrawTextOptions.Clip);
            }
        }

        private void DrawLinesLegend()
        {
            var kvs = MM_Repository.KVLevels;

            Vector2 lineLoc = new Vector2(30, 30);
            Vector2 segment = new Vector2(85, 0);
            Vector2 spacing = new Vector2(0, 20);


            var end = kvs.Count * spacing + segment * 4 + lineLoc;
            Surface.RenderTarget2D.FillRectangle(new RawRectangleF(lineLoc.X - 10, lineLoc.Y - 20, end.X + 10, end.Y), Surface.Brushes.GetBrush(new Color4(0, 0, 0, 0.5f)));

            bool drawEnergizationState = true;
            foreach (var kvl in kvs.Values.OrderByDescending(kv => kv.Nominal))
            {
                DrawLegendLine(kvl.Energized, lineLoc, lineLoc + segment, true, drawEnergizationState: drawEnergizationState);
                DrawLegendLine(kvl.PartiallyEnergized, lineLoc + segment, lineLoc + segment * 2, drawEnergizationState: drawEnergizationState);
                DrawLegendLine(kvl.DeEnergized, lineLoc + segment * 2, lineLoc + segment * 3, drawEnergizationState: drawEnergizationState);
                DrawLegendLine(kvl.Unknown, lineLoc + segment * 3, lineLoc + segment * 4, drawEnergizationState: drawEnergizationState);
                drawEnergizationState = false;
                lineLoc += spacing;
            }
        }

        private void DrawLegendLine(MM_DisplayParameter state, Vector2 start, Vector2 end, bool drawKVText = false, bool drawEnergizationState = false)
        {
            if (_legendFont == null || _legendFont.IsDisposed)
            {
                _legendFont = null;
                _legendFont = Surface.Fonts.GetTextFormat(MM_Repository.OverallDisplay.NetworkMapFont);
            }

            var color = state.ForeColor;

            var width = state.Width;
            var stroke = state.DashStyle;

            var style = _solidStyle;
            if (stroke == System.Drawing.Drawing2D.DashStyle.Dash)
                style = _dashStyle;
            else if (stroke == System.Drawing.Drawing2D.DashStyle.DashDot || stroke == System.Drawing.Drawing2D.DashStyle.DashDotDot)
                style = _dotDashStyle;

            var brush = Surface.Brushes.GetBrush(color, 1f);

            if (drawKVText)
            {
                TextLayout textLayout = null;
                string text = state.Name.Split('.')[0];
                if (!_strings.TryGetValue(text, out textLayout) || textLayout == null || textLayout.IsDisposed)
                {
                    textLayout = new TextLayout(Surface.FactoryDirectWrite, text ?? string.Empty, _legendFont, end.X - start.X - 2, 16) { WordWrapping = WordWrapping.NoWrap };
                    _strings[text] = textLayout;
                }
                Surface.RenderTarget2D.DrawTextLayout(start + Vector2.One, textLayout, brush, DrawTextOptions.Clip);
            }

            if (drawEnergizationState)
            {
                TextLayout textLayout = null;
                string text = state.Name.Split('.')[1];
                if (!_strings.TryGetValue(text, out textLayout) || textLayout == null || textLayout.IsDisposed)
                {
                    textLayout = new TextLayout(Surface.FactoryDirectWrite, text ?? string.Empty, _legendFont, end.X - start.X - 2, 12) { WordWrapping = WordWrapping.NoWrap };
                    _strings[text] = textLayout;
                }
                Surface.RenderTarget2D.DrawTextLayout(start + new Vector2(1, -textLayout.Metrics.Height - 3), textLayout, Surface.Brushes.GetBrush(SharpDX.Color.White), DrawTextOptions.Clip);
            }

            if (_isBlinking || !state.Blink)
            {
                // offset by half a pixel to get hard lines
                bool offsetHalf = width - Math.Truncate(width) > 0 || ((int)width % 2 == 1);
                var aa = Surface.RenderTarget2D.AntialiasMode;
                Surface.RenderTarget2D.AntialiasMode = AntialiasMode.Aliased;
                Surface.RenderTarget2D.DrawLine(start + new Vector2(0, offsetHalf ? 0.5f : 0), end + new Vector2(0, offsetHalf ? 0.5f : 0), brush, width, style);
                Surface.RenderTarget2D.AntialiasMode = aa;
            }
        }

    }
}