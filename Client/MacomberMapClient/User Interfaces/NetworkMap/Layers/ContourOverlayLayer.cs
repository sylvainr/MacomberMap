using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.Contours;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using Rectangle = System.Drawing.Rectangle;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{

    public class ContourOverlayLayer : RenderLayer
    { 

        private KDTree.KDTree<ContourData> _kdProxy = new KDTree.KDTree<ContourData>(2);

        private int _zoomLevel;
        // private BitmapRenderTarget _rt;
        private Rectangle _displayView;
        private Bitmap _bmp;
        private Rectangle _displayRect;
        private Size2 _size;

        private bool isRendering = false;
        private ContourDataProvider DataProvider;
        private MM_Display.ContourOverlayDataTypes _lastProvider;
        private TimeSpan _lastUpdate;

        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        /// <param name="dataProvider">Data provider for the layer</param>
        /// <param name="name">The name of the layer.</param>
        /// <param name="order">The order of the layer</param>
        public ContourOverlayLayer(Direct3DSurface surface, string name = "Contour Layer", int order = 11) : base(surface, name, order)
        {            
        }

        /// <summary>
        /// Load Content and initialize unmanaged resources.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();
            //_rt = Surface.CreateRenderTarget(Surface.RenderTarget2D.PixelSize.Height, Surface.RenderTarget2D.PixelSize.Width);
        }

        /// <summary>
        /// Unload content and cleanup unmanaged resources.
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();
            Cleanup();
        }

        private void Cleanup()
        {
            _kdProxy = null;
            if (_bmp != null)
                _bmp.Dispose();
            _bmp = null;
        }

        /// <summary>
        /// Perform calculations and update data.
        /// </summary>
        /// <param name="updateTime">Time since last update.</param>
        /// <remarks>include base.Update(renderTime); in overloads to preserve updating UpdateTime field.</remarks>
        public override void Update(RenderTime updateTime)
        {
            // if we don't have a data provider, don't update

            if (MM_Repository.OverallDisplay.Contour == MM_Display.MM_Contour_Enum.None) { return; }

            if (!MM_Repository.ContourDataProviders.TryGetValue(MM_Repository.OverallDisplay.ContourData, out DataProvider))
                return;

            base.Update(updateTime);

            bool contourChanged = _lastProvider != MM_Repository.OverallDisplay.ContourData;
            if (Surface.Coordinates.ZoomLevel != _zoomLevel || _kdProxy == null || contourChanged || (updateTime.TotalTime - _lastUpdate).TotalSeconds > MM_Repository.OverallDisplay.ContourRefreshTime)
            {
                _lastUpdate = updateTime.TotalTime;
                _lastProvider = MM_Repository.OverallDisplay.ContourData;
                _kdProxy = new KDTree.KDTree<ContourData>(2);
                _zoomLevel = Surface.Coordinates.ZoomLevel;

                foreach (var item in DataProvider.GetData())
                {
                    var coord = MM_Coordinates.LngLatToScreenVector2(item.LngLat, _zoomLevel);
                    item.XY = coord;

                    _kdProxy.AddPoint(new double[] { coord.X, coord.Y }, item);
                }

                if (_bmp != null)
                    _bmp.Dispose();

                if (contourChanged)
                {
                    Surface.AddOnscreenMessage("Contour layer changed: "+_lastProvider.ToString(), SharpDX.Color.Aqua);
                }
            }


        }

        /// <summary>
        /// Draw the layer. 
        /// </summary>
        /// <param name="renderTime">Time since last render.</param>
        /// <remarks>include base.Render(renderTime); in overloads to preserve updating RenderTime field.</remarks>
        public override void Render(RenderTime renderTime)
        {
            // if we don't have a data provider, don't render
            if (DataProvider == null || _kdProxy == null || MM_Repository.OverallDisplay.Contour == MM_Display.MM_Contour_Enum.None) { return; }

            base.Render(renderTime);
            var displayView = Surface.Coordinates.GetViewXY();
            var displayRect = Surface.DisplayRectangle;
            var size = Surface.RenderTarget2D.PixelSize;
            bool resized = _displayRect != displayRect;

            bool viewSizeMatchesDisplaySize = displayView.Width == displayRect.Width && displayView.Height == displayRect.Height;
            if ((displayView != _displayView || _bmp == null || _bmp.IsDisposed || resized) && !((MM_Network_Map_DX)Surface).HandlingMouse && viewSizeMatchesDisplaySize && !isRendering)
            {
                _displayRect = displayRect;
                _displayView = displayView;

                _size = size;

                DrawBitmap(Surface.RenderTarget2D.PixelSize.Height / 10,
                           Surface.RenderTarget2D.PixelSize.Width / 10,
                           (float)MM_Coordinates.GConstants[_zoomLevel][1] * _zoomLevel * _zoomLevel, _displayView);
            }

            if (_bmp != null && Surface.RenderTarget2D != null && !((MM_Network_Map_DX)Surface).HandlingMouse)
            {
                var stateLayer = Surface.GetLayers<StateRegionsLayer>().FirstOrDefault();

                var tx = Surface.RenderTarget2D.Transform;
                Surface.RenderTarget2D.Transform = Matrix3x2.Translation(-Surface.Coordinates.TopLeftXY.X, -Surface.Coordinates.TopLeftXY.Y);
                var dest = new SharpDX.RectangleF(-Surface.RenderTarget2D.Transform.M31, -Surface.RenderTarget2D.Transform.M32, Surface.RenderTarget2D.Size.Width, Surface.RenderTarget2D.Size.Height);

                if (MM_Repository.OverallDisplay.ClipContours  && stateLayer != null && stateLayer.State != null && stateLayer.State.Geometry != null && !(DataProvider is LineLoadingPercentageContourData))
                {
                    var layerParameters = new LayerParameters()
                    {
                        ContentBounds = RectangleF.Infinite,
                        GeometricMask = stateLayer.State.Geometry,
                        Opacity = 1,
                        MaskTransform = tx
                    };
                    using (var layer = new Layer(Surface.RenderTarget2D, Surface.RenderTarget2D.Size))
                    {
                        Surface.RenderTarget2D.PushLayer(ref layerParameters, layer);
                        Surface.RenderTarget2D.DrawBitmap(_bmp, dest, 0.5f, BitmapInterpolationMode.Linear);
                        Surface.RenderTarget2D.PopLayer();
                    }
                }
                else
                {
                    Surface.RenderTarget2D.DrawBitmap(_bmp, dest, 0.5f, BitmapInterpolationMode.Linear);
                }

                Surface.RenderTarget2D.Transform = tx;
            }
        }

        private void DrawBitmap(int height, int width, float kdDistance, Rectangle view)
        {

            int stride = width * sizeof(int);

            using (var tempStream = new DataStream(height * stride, true, true))
            {
                for (int yPixel = 0; yPixel < height; yPixel++)
                {
                    float ySamplePosition = ((float)yPixel / height) * view.Height + view.Top;

                    for (int xPixel = 0; xPixel < width; xPixel++)
                    {
                        float xSamplePosition = ((float)xPixel / width) * view.Width + view.Left;

                        var neighbors = _kdProxy.NearestNeighbors(new double[] { xSamplePosition, ySamplePosition }, 25, kdDistance);
                        if (!neighbors.MoveNext())
                        {
                            //    neighbors = _kdProxy.NearestNeighbors(new[] { xPosition, yPosition }, 25, kdDistance * 1.5);
                            //    neighbors.MoveNext();
                        }
                        float totalWeight = 0;
                        float totalValue = 0;
                        int numMatches = 0;



                        if (neighbors.CurrentDistance >= 0)
                        {
                            do
                            {
                                var curDistance = neighbors.CurrentDistance / kdDistance;
                                var substation = (ContourData)((IEnumerator)neighbors).Current;
                                if (float.IsNaN(substation.Value) || float.IsInfinity(substation.Delta) || Math.Abs(substation.Delta) < 0.01) continue;

                                float weight = (float)curDistance < float.Epsilon ? 1 : 1 / (float)curDistance;
                                weight = weight * weight;
                                totalWeight += weight;
                                totalValue += substation.Delta * weight;

                                numMatches++;
                            }
                            while (neighbors.MoveNext());
                        }


                        float ratio = 0;
                        float avgValue = 0;
                        if (numMatches > 0)
                        {
                            avgValue = totalValue / totalWeight;
                        }
                        // totalValue -= 0.2;
                        if (numMatches > 2 && !float.IsNaN(totalValue))
                        {
                            var c = DataProvider.Palette.GetInterpolateColor(avgValue);
                            int rgba = (byte)(255 * c.R) | ((byte)(255 * c.G) << 8) | ((byte)(255 * c.B) << 16) | ((byte)(255 * c.A) << 24);

                            tempStream.Write(rgba);
                        }
                        else
                        {
                            tempStream.Write(0);
                        }
                    }
                }
                tempStream.Position = 0;


                if (_bmp == null || _bmp.IsDisposed || _bmp.PixelSize.Width != width || _bmp.PixelSize.Height != height)
                {
                    // create a new bitmap if we resize or cannot find the old bitmap
                    if (_bmp != null)
                        _bmp.Dispose(); // cleanup if required
                    var bitmapProperties = new BitmapProperties(new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
                    _bmp = new SharpDX.Direct2D1.Bitmap(Surface.RenderTarget2D, new Size2(width, height), tempStream, stride, bitmapProperties);
                }
                else
                {
                    // reuse existing bitmap and just update the data
                    _bmp.CopyFromStream(tempStream, stride, height * width * sizeof(int), new RawRectangle(0, 0, width, height));
                }

            }
        }



        /// <summary>
        /// Return a color corresponding to the value (0 is average)
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        private Color4 RelativeColor(float Value)
        {
            LinearGradientBrush b;

            float gray = MathUtil.Clamp((Value + 1) / 2f, 0, 1f);
            return new Color4(gray, gray, gray, 1);

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
            return new Color4(MathUtil.Clamp(R / 255f, 0, 1f),
                              MathUtil.Clamp(G / 255f, 0, 1f),
                              MathUtil.Clamp(B / 255f, 0, 1f),
                              1f);
        }
    }
}