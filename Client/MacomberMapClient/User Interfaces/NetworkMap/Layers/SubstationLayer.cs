using System;
using System.Collections.Generic;
using System.Linq;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using MacomberMapClient.Properties;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using MacomberMapClient.User_Interfaces.NetworkMap.Proxies;
using MacomberMapClient.User_Interfaces.Violations;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using Color = System.Drawing.Color;
using Factory = SharpDX.Direct2D1.Factory;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    public class SubstationLayer : RenderLayer
    {
        static float Sqrt2 = (float)Math.Sqrt(2);
        private readonly MM_Violation_Viewer _violationViewer;
        private int _lastZoom;
        private readonly Dictionary<int, SubstationProxy> _substationProxies = new Dictionary<int, SubstationProxy>();

        private SharpDX.Direct2D1.Bitmap UnitFrequencyControlBitmap;
        private SharpDX.Direct2D1.Bitmap OutagedAutoTransformerBitmap;
        private SharpDX.Direct2D1.Bitmap SynchroscopeBitmap;

        public static readonly Dictionary<string, Color4> FuelColors = new Dictionary<string, Color4>()
        {
            { "COAL", SharpDxExtensions.Color4FromHexCode("#58595B") },
            { "GAS", SharpDxExtensions.Color4FromHexCode("#C66B18") },
            { "HYDRO", SharpDxExtensions.Color4FromHexCode("#0082C6") },
            { "NUCLEAR", SharpDxExtensions.Color4FromHexCode("#CCCC00") },
            { "WDS", SharpDxExtensions.Color4FromHexCode("#666600") },
            { "OIL", SharpDxExtensions.Color4FromHexCode("#ED1C24") },
            { "SOLAR", SharpDxExtensions.Color4FromHexCode("#D7C945") },
            { "WIND", SharpDxExtensions.Color4FromHexCode("#0DB04B") },
            { "EXTERN", SharpDxExtensions.Color4FromHexCode("#4C1C39") },
            { "OTHER", SharpDxExtensions.Color4FromHexCode("#FFFFFF") },
            { "LOAD", SharpDxExtensions.Color4FromHexCode("#D358F7") },
        };

        private bool _isBlinking;
        private TextFormat _textFormat;
        private StrokeStyle _dashStyle;

        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        /// <param name="order">The order of the layer</param>
        public SubstationLayer(Direct3DSurface surface, MM_Violation_Viewer violationViewer) : base(surface, "SubstationLayer", 10)
        {
            _violationViewer = violationViewer;
        }

        public SubstationProxy GetProxy(int teid)
        {
            SubstationProxy proxy;
            return _substationProxies.TryGetValue(teid, out proxy) ? proxy : null;
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
            foreach (var proxy in _substationProxies)
            {
                if (onlyVisible && !proxy.Value.Visible) continue;

                if (proxy.Value.HitTest(screenCoordinates, radius))
                    yield return proxy.Value.Substation;
            }
        }

        /// <summary>
        /// Load Content and initialize unmanaged resources.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();
            UnitFrequencyControlBitmap = Resources.UnitFrequencyControl.ToDxBitmap(Surface.RenderTarget2D);
            OutagedAutoTransformerBitmap = Resources.OutagedAutoTransformer.ToDxBitmap(Surface.RenderTarget2D);
            SynchroscopeBitmap = Resources.Synchroscope.ToDxBitmap(Surface.RenderTarget2D);

            _dashStyle = new StrokeStyle(Surface.Factory2D,
                                        new StrokeStyleProperties() { DashCap = CapStyle.Flat, DashStyle = DashStyle.Custom, DashOffset = 10f, LineJoin = LineJoin.MiterOrBevel, MiterLimit = 2f }, new float[] { 1, 1 });
        }

        /// <summary>
        /// Unload content and cleanup unmanaged resources.
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            foreach (var proxy in _substationProxies)
            {
                if (proxy.Value.PieGeometry != null)
                    proxy.Value.PieGeometry.Dispose();
                proxy.Value.PieGeometry = null;
            }

            _textFormat = null;
            if (_dashStyle != null)
                _dashStyle.Dispose();
            _dashStyle = null;

            if (UnitFrequencyControlBitmap != null)
                UnitFrequencyControlBitmap.Dispose();
            UnitFrequencyControlBitmap = null;

            if (OutagedAutoTransformerBitmap != null)
                OutagedAutoTransformerBitmap.Dispose();
            OutagedAutoTransformerBitmap = null;

            if (SynchroscopeBitmap != null)
                SynchroscopeBitmap.Dispose();
            SynchroscopeBitmap = null;
        }

        /// <summary>
        /// Perform calculations and update data.
        /// </summary>
        /// <param name="updateTime">Time since last update.</param>
        /// <remarks>include base.Update(renderTime); in overloads to preserve updating UpdateTime field.</remarks>
        public override void Update(RenderTime updateTime)
        {
            base.Update(updateTime);
            if (!Surface.IsDirty && _lastZoom == Surface.Coordinates.ZoomLevel)
                return;

            _lastZoom = Surface.Coordinates.ZoomLevel;

            foreach (var substationKvp in MM_Repository.Substations)
            {
                var element = substationKvp.Value;

                SubstationProxy proxy;
                if (!_substationProxies.TryGetValue(element.TEID, out proxy))
                {
                    proxy = new SubstationProxy()
                    {
                        Substation = element,
                        Coordinates = MM_Coordinates.LngLatToScreenVector2(element.LngLat, _lastZoom)
                    };
                    _substationProxies.Add(element.TEID, proxy);
                }
                proxy.Coordinates = MM_Coordinates.LngLatToScreenVector2(element.LngLat, _lastZoom);
                proxy.BlackStartHidden = proxy.BlackStartDim = false;

                // calculate blackstart parameters
                if ((MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonOperatorElements || MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonOperatorElements) && (Array.IndexOf(proxy.Substation.Operators, MM_Repository.OverallDisplay.DisplayCompany) == -1))
                    if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonOperatorElements)
                        proxy.BlackStartHidden = true;
                    else
                        proxy.BlackStartDim = true;
                else if ((MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonBlackstartElements || MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonBlackstartElements) && (!element.IsBlackstart || Array.IndexOf(proxy.Substation.Operators, MM_Repository.OverallDisplay.DisplayCompany) == -1))
                    if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonBlackstartElements)
                        proxy.BlackStartHidden = true;
                    else
                        proxy.BlackStartDim = true;
                else if ((MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideExternalElements || MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimExternalElements) && !element.IsInternal)
                    if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideExternalElements)
                        proxy.BlackStartHidden = true;
                    else
                        proxy.BlackStartDim = true;

             

                //Handle null line operator values to keep from breaking the rest
                if ((element.Operators == null || element.Operators.Length == 0) &&
                    (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonBlackstartElements ||
                    MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonOperatorElements))
                    proxy.BlackStartDim = true;
                else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonBlackstartElements &&
                         (!element.IsBlackstart || Array.IndexOf(proxy.Substation.Operators, MM_Repository.OverallDisplay.DisplayCompany) == -1))
                    proxy.BlackStartDim = true;
                else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimExternalElements && !element.IsInternal)
                    proxy.BlackStartDim = true;
                else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonOperatorElements && Array.IndexOf(proxy.Substation.Operators, MM_Repository.OverallDisplay.DisplayCompany) == -1)
                    proxy.BlackStartDim = true;
                else
                    proxy.BlackStartDim = false;


            }
        }

        /// <summary>
        /// Return a color corresponding to the value (0 is average)
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        private Color RelativeColor(float Value)
        {
            float R = 176;
            float G = 196;
            float B = 222;
            float Percentage = Value * 15f;


            if (float.IsNaN(Value))
            { }
            else if (Value > 0)
            {
                R = (255f * Percentage) + (176f * (1f - Percentage));
                G = 196 * (1f - Percentage);
                B = 222 * (1f - Percentage);
            }
            else
            {
                R = (176 * (1f + Percentage));
                G = 196 * (1f + Percentage);
                B = (-255f * Percentage) + (222 * (1f + Percentage));
            }
            return Color.FromArgb(Math.Max(0, Math.Min((int)R, 255)), Math.Max(0, Math.Min((int)G, 255)), Math.Max(0, Math.Min(255, (int)B)));
        }

        /// <summary>
        /// Draw the layer. 
        /// </summary>
        /// <param name="renderTime">Time since last render.</param>
        /// <remarks>include base.Render(renderTime); in overloads to preserve updating RenderTime field.</remarks>
        public override void Render(RenderTime renderTime)
        {
            base.Render(renderTime);

            int zoomLevel = Surface.Coordinates.ZoomLevel;
            var bounds = Surface.Coordinates.GetBounds();

            var tx = Surface.RenderTarget2D.Transform;
            Surface.RenderTarget2D.Transform = Matrix3x2.Translation(-Surface.Coordinates.TopLeftXY.X, -Surface.Coordinates.TopLeftXY.Y);

            // if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.None)
            //     return;
            _isBlinking = ((MM_Network_Map_DX)Surface).IsBlinking;

            DrawSubstations(bounds, zoomLevel, _isBlinking);
            Surface.RenderTarget2D.Transform = tx;
        }



        private void DrawSubstations(RawRectangleF bounds, int zoomLevel, bool showViolation)
        {
            foreach (var proxy in _substationProxies.Values)
            {
                proxy.Visible = false;

                if (proxy.BlackStartHidden)
                    continue;

                // check visibility and bounds
                if (!bounds.Contains(proxy.Coordinates))
                    continue;

                if (!IsVisible(proxy, zoomLevel))
                    continue;



                MM_DisplayParameter Disp = proxy.Substation.SubstationDisplay(_violationViewer.ShownViolations, Surface);
                if (Disp.Blink && !showViolation)
                    Disp = MM_Repository.SubstationDisplay;

                bool showingViolation = Disp != MM_Repository.SubstationDisplay;

                bool isInternal = proxy.Substation.IsInternal;
                var DrawColor = Disp.ForeColor;
                float width = Disp.Width;

                //If drawing the substation with voltage profile, update the color accordingly
                if (MM_Repository.SubstationDisplay.ShowSubstationVoltages && (!Disp.Blink || (Disp.Blink && showViolation)))
                {
                    try
                    {
                        float WorstPu = proxy.Substation.Average_pU;

                        if (float.IsNaN(WorstPu))
                            DrawColor = Color.FromArgb(50, 50, 50);
                        else if ((WorstPu >= 1f + (MM_Repository.OverallDisplay.ContourThreshold / 100f) || WorstPu <= 1f - (MM_Repository.OverallDisplay.ContourThreshold / 100f)))
                            DrawColor = RelativeColor(WorstPu - 1f);
                    }
                    catch
                    {
                        /* suppress errors */
                    }
                }

                if (proxy.BlackStartDim)
                {
                    DrawColor = MM_Repository.OverallDisplay.BlackstartDim(DrawColor);
                }

                proxy.Color = DrawColor.ToDxColor4();
                var solidColorBrush = Surface.Brushes.GetBrush(proxy.Color);
                bool drawBubbles = false;
                proxy.Visible = true;

                drawBubbles = MM_Repository.SubstationDisplay.GeneratorBubblesMode != MM_Substation_Display.MM_Generator_Bubble_Mode.None;

                bool fuelBubbleVisible = false;
                bool loadBubbleVisible = false;
                // Draw load squares if requested


                if (proxy.Substation.HasLoads && MM_Repository.SubstationDisplay.ShowLoadBubbles)
                {
                    DrawLoadBubbles(proxy, zoomLevel);
                    loadBubbleVisible = true;
                }

                if (drawBubbles && proxy.Substation.HasUnits)
                {
                    DrawFuelBubbles(proxy, zoomLevel);
                    fuelBubbleVisible = true;
                }

                if (MM_Repository.SubstationDisplay.ShowReserveZones && proxy.Substation.HasUnits)
                {
                    ShowReserveZonesBubbles(proxy, zoomLevel);
                }

                // Draw unit substations
                if (proxy.Substation.HasUnits &&
                    (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Units ||
                     MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.All))
                {

                    if ((showingViolation || !fuelBubbleVisible))
                    {
                        // scale down diamonds for substations if we are drawing bubbles
                        float scale = 1.75f;
                        using (var geom = SharpDxExtensions.CreatePathGeometry(Surface.Factory2D, true, true,
                                                                               new Vector2(proxy.Coordinates.X - (width * scale), proxy.Coordinates.Y),
                                                                               new Vector2(proxy.Coordinates.X, proxy.Coordinates.Y - (width * scale)),
                                                                               new Vector2(proxy.Coordinates.X + (width * scale), proxy.Coordinates.Y),
                                                                               new Vector2(proxy.Coordinates.X, proxy.Coordinates.Y + (width * scale))))
                        {
                            Surface.RenderTarget2D.FillGeometry(geom, solidColorBrush);
                        }
                    }
                }
                else if ((showingViolation || !loadBubbleVisible))
                {
                    // we don't need to AA squares. this saves performance
                    var aa = Surface.RenderTarget2D.AntialiasMode;
                    Surface.RenderTarget2D.AntialiasMode = AntialiasMode.Aliased;
                    Surface.RenderTarget2D.FillRectangle(new RectangleF(proxy.Coordinates.X - width, proxy.Coordinates.Y - width, width * 2f, width * 2f), solidColorBrush);
                    Surface.RenderTarget2D.AntialiasMode = aa;
                }

                bool hasIcon = DrawIcons(proxy, showViolation);
                if (proxy.Substation.KVLevels != null)
                {
                    foreach (MM_KVLevel Voltage in proxy.Substation.KVLevels)
                    {
                        if (zoomLevel >= Voltage.StationMW || zoomLevel >= Voltage.StationNames)
                        {
                            DrawText(proxy, zoomLevel, solidColorBrush, hasIcon);
                        }
                    }
                }
            }
        }

        private bool DrawIcons(SubstationProxy proxy, bool showViolation)
        {
            // gather display options
            bool hasOutage = MM_Repository.SubstationDisplay.ShowAutoTransformersOut && proxy.Substation.HasOutagedAutoTransformer;
            MM_Substation.enumFrequencyControlStatus freqCtrlStatus = MM_Substation.enumFrequencyControlStatus.None;
            if (MM_Repository.SubstationDisplay.ShowFrequencyControl)
                freqCtrlStatus = proxy.Substation.FrequencyControlStatus;
            bool hasSynchroscope = MM_Repository.SubstationDisplay.ShowSynchroscopes && proxy.Substation.HasSynchroscope;

            //Draw the substation frequency control information if needed
            if ((freqCtrlStatus == MM_Substation.enumFrequencyControlStatus.Island && showViolation) || freqCtrlStatus == MM_Substation.enumFrequencyControlStatus.Unit)
            {
                Surface.RenderTarget2D.DrawBitmap(UnitFrequencyControlBitmap,
                                                  new RawRectangleF(proxy.Coordinates.X - 10, proxy.Coordinates.Y - 10, proxy.Coordinates.X + 10, proxy.Coordinates.Y + 10),
                                                  1,
                                                  BitmapInterpolationMode.Linear);
                return true;
            }
            else if (hasOutage)
            {
                Surface.RenderTarget2D.DrawBitmap(OutagedAutoTransformerBitmap,
                                                  new RawRectangleF(proxy.Coordinates.X - 10, proxy.Coordinates.Y - 10, proxy.Coordinates.X + 10, proxy.Coordinates.Y + 10),
                                                  1,
                                                  BitmapInterpolationMode.Linear);
                return true;
            }
            else if (hasSynchroscope)
            {
                Surface.RenderTarget2D.DrawBitmap(SynchroscopeBitmap,
                                                  new RawRectangleF(proxy.Coordinates.X - 10, proxy.Coordinates.Y - 10, proxy.Coordinates.X + 10, proxy.Coordinates.Y + 10),
                                                  1,
                                                  BitmapInterpolationMode.Linear);
                return true;
            }

            return false;
        }


        private void DrawText(SubstationProxy proxy, int zoomLevel, Brush brush, bool hasIcon = false)
        {
            if (_textFormat == null || _textFormat.IsDisposed)
                _textFormat = Surface.Fonts.GetTextFormat(MM_Repository.OverallDisplay.NetworkMapFont);

            var textCoord = proxy.Coordinates;
            textCoord.X += 5;
            textCoord.Y += 2;
            if (zoomLevel >= MM_Repository.OverallDisplay.StationMW)
            {
                string detailsText = proxy.Substation.MapText(zoomLevel);
                if (proxy.DetailsText == null || !proxy.DetailsText.Equals(detailsText, StringComparison.OrdinalIgnoreCase) || (proxy.DetailsTextLayout == null || proxy.DetailsTextLayout.IsDisposed))
                {
                    var detailsLayout = new TextLayout(Surface.FactoryDirectWrite, detailsText, _textFormat, 200, 250);
                    proxy.DetailsText = detailsText;
                    if (proxy.DetailsTextLayout!=null)
                    proxy.DetailsTextLayout.Dispose();
                    proxy.DetailsTextLayout = detailsLayout;
                }

                Surface.RenderTarget2D.DrawTextAtPoint(proxy.DetailsTextLayout, brush, textCoord.X, textCoord.Y);
            }

            // create name TextLayout
            string nameText = proxy.Substation.MapText(1);

            if (proxy.NameText == null || !proxy.NameText.Equals(nameText, StringComparison.OrdinalIgnoreCase) || (proxy.NameTextLayout == null || proxy.NameTextLayout.IsDisposed))
            {
                proxy.NameText = nameText;
                var nameLayout = new TextLayout(Surface.FactoryDirectWrite, nameText, _textFormat, 250, 20);
                if (proxy.NameTextLayout!=null)
                proxy.NameTextLayout.Dispose();
                proxy.NameTextLayout = nameLayout;
            }

            Surface.RenderTarget2D.DrawTextAtPoint(proxy.NameTextLayout, brush, textCoord.X, textCoord.Y + (hasIcon ? 8 : 0));

        }

        private void DrawLoadBubbles(SubstationProxy proxy, int zoomLevel)
        {
            var substation = proxy.Substation;
            bool isInternal = substation.IsInternal;
            var loads = substation.Loads;


            var mw = Math.Round(loads.Sum(l => l.MW), 2);

            float ringRadius = 1f;
            float scaleFactor = MM_Repository.SubstationDisplay.GeneratorBubblesScale;
            scaleFactor = scaleFactor * 8 / ((18 - zoomLevel));
            if (zoomLevel > 12) scaleFactor *= 1.5f;

            if (mw >= 1)
                ringRadius = (float)Math.Log(mw) * scaleFactor;

            float opacity = 0.6f;

            Color4 color = FuelColors["LOAD"];

            // black start dim factor
            if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimExternalElements && !isInternal)
            {
                opacity *= MathUtil.Clamp(MM_Repository.OverallDisplay.BlackstartDimmingFactor / 100f, 0, 1);
            }

            if (opacity <= 0) return;

            var fillBrush = Surface.Brushes.GetBrush(new Color4(color.Red, color.Green, color.Blue, opacity));
            var ringBrush = Surface.Brushes.GetBrush(new Color4(color.Red, color.Green, color.Blue, opacity / 2));

            var aa = Surface.RenderTarget2D.AntialiasMode;
            Surface.RenderTarget2D.AntialiasMode = AntialiasMode.Aliased;
            var rect = new RawRectangleF(proxy.Coordinates.X - ringRadius, proxy.Coordinates.Y - ringRadius, proxy.Coordinates.X + ringRadius, proxy.Coordinates.Y + ringRadius);
            if (mw > 0)
                Surface.RenderTarget2D.FillRectangle(rect, fillBrush);

            //Surface.RenderTarget2D.DrawRectangle(rect, ringBrush, 1f);
            Surface.RenderTarget2D.AntialiasMode = aa;
        }

        private void DrawFuelBubbles(SubstationProxy proxy, int zoomLevel)
        {
            var substation = proxy.Substation;
            bool isInternal = substation.IsInternal;
            var units = substation.Units;

            var max = Math.Round(units.Sum(unit => unit.MaxCapacity), 2);
            var mw_est = Math.Round(units.Sum(unit => unit.Estimated_MW), 2);
            var mw_tel = Math.Round(units.Sum(unit => unit.Telemetered_MW), 2);
            var mw_rtgen = Math.Round(units.Sum(unit => !float.IsNaN(unit.RTGenMW) && !unit.MultipleNetmomForEachGenmom ? unit.RTGenMW : unit.Estimated_MW), 2);
            var mw = Data_Integration.UseEstimates && mw_tel != 0 ? mw_tel : mw_est;
            var disp = Math.Round(units.Sum(unit => unit.DispatchMW), 2);
            if  (!double.IsNaN(mw_rtgen) && mw_rtgen != 0) // RTGEN MW should be more accurate.
                mw = mw_rtgen;

            bool isNDver = units.Any(unit => string.Equals(unit.MarketResourceType, "NDVER"));

            //var regUp = Math.Round(units.Sum(unit => unit.RegUp), 2);
            var unitType = (units[0].FuelType.Name ?? "OTHER").ToUpper();

            if (!MM_Repository.SubstationDisplay.GeneratorBubbleFuelTypesVisible.Contains(unitType))
                return;


            // calculate ring sizes
            float scaleFactor = MM_Repository.SubstationDisplay.GeneratorBubblesScale;
            var genBubbleMode = MM_Repository.SubstationDisplay.GeneratorBubblesMode;

            if (!genBubbleMode.HasFlag(MM_Substation_Display.MM_Generator_Bubble_Mode.Offline) && mw <= 0)
                return;

            if (!(genBubbleMode.HasFlag(MM_Substation_Display.MM_Generator_Bubble_Mode.Online) ||
                  genBubbleMode.HasFlag(MM_Substation_Display.MM_Generator_Bubble_Mode.Remaining)) &&
                  mw > 0)
                return;

            if (zoomLevel <= 7)
            {
                scaleFactor /= (8 - zoomLevel);

            }
            if (zoomLevel >= 10) scaleFactor *= (1.75f * (zoomLevel / 10f));

            float minRadius = (zoomLevel > 7) ? 10f : 5f;
            float mwRadius = minRadius;
            if (mw > 1)
                mwRadius = (float)Math.Log(mw) * scaleFactor;

            if (mwRadius < minRadius) mwRadius = minRadius;

            Color4 color = FuelColors["OTHER"];
            if (FuelColors.ContainsKey(unitType))
                color = FuelColors[unitType];
     
            //if (unitType == "EXTERN" || unitType == "OTHER" || unitType == "UNKNOWN") return;

            float opacity = 0.6f;

            // dim down offline
            if (mw <= 0)
                opacity *= 0.8f;

            // black start dim factor
            if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimExternalElements && !isInternal)
                opacity *= MathUtil.Clamp(MM_Repository.OverallDisplay.BlackstartDimmingFactor / 100f, 0, 1);

            // create brushes based on dimming and fuel color
            var fillBrush = Surface.Brushes.GetBrush(new Color4(color.Red, color.Green, color.Blue, opacity));

            var tx = Surface.RenderTarget2D.Transform;

            var scale = Matrix3x2.Scaling(1);
            var rotation = Matrix3x2.Rotation(0, Vector2.Zero);
            var translation = Matrix3x2.Translation(((Matrix3x2)tx).TranslationVector + (Vector2)proxy.Coordinates);
            Surface.RenderTarget2D.Transform = scale * rotation * translation;

            var circle = new Ellipse(Vector2.Zero, mwRadius, mwRadius);

            if ((mw <= 0) && (genBubbleMode.HasFlag(MM_Substation_Display.MM_Generator_Bubble_Mode.Offline)))
            {
                Surface.RenderTarget2D.DrawEllipse(circle, fillBrush, 2f, _dashStyle);

                if (isNDver)
                {
                    var ndverLen = mwRadius;//* Sqrt2;
                    var ndverVec = new Vector2(ndverLen, -ndverLen);
                    Surface.RenderTarget2D.DrawLine(-ndverVec, ndverVec, fillBrush, 2);
                }
            }

            if (mw > 0)
            {
                Surface.RenderTarget2D.DrawEllipse(circle, fillBrush, 2f);

                // draw pi chart
                if (genBubbleMode.HasFlag(MM_Substation_Display.MM_Generator_Bubble_Mode.Remaining) && mw < max)
                {
                    float pieValue = (float)(mw / max);
                    if (proxy.PieGeometry == null || proxy.PieGeometry.IsDisposed ||
                        Math.Abs(proxy.PieValue - pieValue) > float.Epsilon ||
                        Math.Abs(proxy.PieRadius - mwRadius) > float.Epsilon)
                    {
                        if (proxy.PieGeometry != null)
                            proxy.PieGeometry.Dispose();
                        proxy.PieGeometry = Surface.Factory2D.CreatePieSlice((float)(mw / max), mwRadius);
                        proxy.PieValue = pieValue;
                        proxy.PieRadius = mwRadius;
                    }
                    Surface.RenderTarget2D.FillGeometry(proxy.PieGeometry, fillBrush);
                }
                else
                {
                    Surface.RenderTarget2D.FillEllipse(circle, fillBrush);
                }

                if (genBubbleMode.HasFlag(MM_Substation_Display.MM_Generator_Bubble_Mode.Remaining) && !isNDver)
                {
                    var dispatchPoint = new Vector2()
                    {
                        X = mwRadius * (float)Math.Cos((disp / max) * MathUtil.TwoPi),
                        Y = mwRadius * (float)Math.Sin((disp / max) * MathUtil.TwoPi)
                    };

                    Surface.RenderTarget2D.DrawLine(Vector2.Zero, dispatchPoint, Surface.Brushes.GetBrush(Color.White, opacity * 1.2f), 2f);
                }

                if (isNDver)
                {
                    var ndverLen = mwRadius;//* Sqrt2;
                    var ndverVec = new Vector2(ndverLen, -ndverLen);
                    Surface.RenderTarget2D.DrawLine(-ndverVec, ndverVec, fillBrush, 2);
                }

            }

            Surface.RenderTarget2D.Transform = tx;
        }

        private bool IsVisible(SubstationProxy proxy, int zoomLevel)
        {
            if (proxy.Substation == null)
                return false;

            var substation = proxy.Substation;

            bool isInternal = substation.IsInternal;

            if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideExternalElements && !isInternal)
                return false;

            if (proxy.Substation.KVLevels.Count == 0)
                return false;

            var maxKv = proxy.Substation.KVLevels.OrderByDescending(kv => kv.Nominal).FirstOrDefault();
            if (maxKv == null)
                return false;

        //    if (maxKv.StationNames <= zoomLevel)
         //       return true;

            MM_AlarmViolation_Type WorstViolation = null;
            if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Units && substation.HasUnits)
                return true;
            if (MM_Repository.SubstationDisplay.ShowAutoTransformersOut && substation.HasOutagedAutoTransformer)
                return true;
            if (MM_Repository.SubstationDisplay.ShowFrequencyControl && substation.FrequencyControlStatus != MM_Substation.enumFrequencyControlStatus.None)
                return true;
            if (((WorstViolation = substation.WorstVisibleViolation(_violationViewer.ShownViolations, Surface)) != null) &&
                (WorstViolation.NetworkMap_Substation != MM_AlarmViolation_Type.DisplayModeEnum.Never))
                return true;
            if (false && MM_Repository.SubstationDisplay.ShowSubsOnLines && (zoomLevel >= MM_Repository.OverallDisplay.StationNames)) // show substations connected to visible lines.
                return true;
            if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonBlackstartElements && substation.IsBlackstart == false)
                return false;
            if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonOperatorElements && Array.IndexOf(substation.Operators, MM_Repository.OverallDisplay.DisplayCompany) == -1)
                return false;
            if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.None)
                return false;
            if ((MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Capacitors || MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.CapacitorsAvailable || MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.CapacitorsOnline) && !substation.ElemTypes.Contains(MM_Repository.FindElementType("Capacitor")))
                return false;
            if ((MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Reactors || MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.ReactorsAvailable || MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.ReactorsOnline) && !substation.ElemTypes.Contains(MM_Repository.FindElementType("Reactor")))
                return false;
            if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Loads && !substation.ElemTypes.Contains(MM_Repository.FindElementType("Load")))
                return false;
            if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.LAARs && !substation.ElemTypes.Contains(MM_Repository.FindElementType("LAAR")))
                return false;
            if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Transformers && !substation.ElemTypes.Contains(MM_Repository.FindElementType("Transformer")))
                return false;
            if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.Units && !substation.ElemTypes.Contains(MM_Repository.FindElementType("Unit")))
                return false;
            if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.CapacitorsAvailable)
            {
                foreach (MM_ShuntCompensator Shunt in substation.ShuntCompensators)
                    if (Shunt.ElemType.Name == "Capacitor" && Shunt.Open)
                        return true;
            }
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.CapacitorsOnline)
            {
                foreach (MM_ShuntCompensator Shunt in substation.ShuntCompensators)
                    if (Shunt.ElemType.Name == "Capacitor" && !Shunt.Open)
                        return true;
            }
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.ReactorsAvailable)
            {
                foreach (MM_ShuntCompensator Shunt in substation.ShuntCompensators)
                    if (Shunt.ElemType.Name == "Reactor" && Shunt.Open)
                        return true;
            }
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.ReactorsOnline)
            {
                foreach (MM_ShuntCompensator Shunt in substation.ShuntCompensators)
                    if (Shunt.ElemType.Name == "Reactor" && !Shunt.Open)
                        return true;
            }
            else if (MM_Repository.SubstationDisplay.ShowSubstations == MM_Substation_Display.SubstationViewEnum.StaticVarCompensators)
                return substation.ElemTypes.Contains(MM_Repository.FindElementType("StaticVarCompensator"));



            return true;
        }


        private void ShowReserveZonesBubbles(SubstationProxy proxy, int zoomLevel)
        {
            var substation = proxy.Substation;
            var units = substation.Units;
            float scaleFactor = MM_Repository.SubstationDisplay.GeneratorBubblesScale;

            var regUp = Math.Round(units.Sum(unit => unit.RegUp), 2);

            // show bubbles by reserve zone if that's set in the display options.
            if (units.Count > 0 && (units[0].ReserveZone > 0 || (units.Count > 1 && units[1].ReserveZone > 0)))
            {
                var cola = SharpDxExtensions.Color4FromHexCode("#58FAF4");

                int rz = units[0].ReserveZone;
                if (rz == 0)
                    rz = units[1].ReserveZone;

                if (rz == 2)
                    cola = SharpDxExtensions.Color4FromHexCode("#D358F7");
                else if (rz == 3)
                    cola = SharpDxExtensions.Color4FromHexCode("#00FF40");
                else if (rz == 4)
                    cola = SharpDxExtensions.Color4FromHexCode("#B45F04");
                else
                    cola = SharpDxExtensions.Color4FromHexCode("#F7D358");

                var rad = (float)Math.Log(regUp * 100 + 500 + (zoomLevel*65)) * scaleFactor * 2;

                float alpha = 0.1f;
                if (regUp > 5)
                    alpha = 0.2f;
                if (regUp > 10)
                    alpha = 0.3f;
                if (regUp > 20)
                    alpha = 0.38f;
                if (regUp > 35)
                    alpha = 0.46f;
                if (regUp > 50)
                    alpha = 0.51f;

                var brush = Surface.Brushes.GetBrush(new Color4(cola.R, cola.G, cola.B, alpha));
                Surface.RenderTarget2D.FillEllipse(new Ellipse(new Vector2(proxy.Coordinates.X, proxy.Coordinates.Y), rad, rad), brush);
            }
        }
    }
}