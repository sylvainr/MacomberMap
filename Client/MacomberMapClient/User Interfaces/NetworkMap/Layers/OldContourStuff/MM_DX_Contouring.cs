using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KDTree;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using BitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;
using Buffer = SharpDX.Direct3D11.Buffer;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using RectangleF = System.Drawing.RectangleF;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    public class MM_DX_Contouring
    {
        private List<MM_Substation> substations = new List<MM_Substation>();

        private Direct3DSurface _surface;
        private Buffer _vertexBuffer;
        private Buffer _indexBuffer;
        private int _indexCount;
        private int _triangleCount;
        private KDTree<MM_Substation> _kdTree;
        private Bitmap _bmp;
        private Timer _redrawTimer;
        private bool draw = false;
        private static bool featureEnableContour = true;

        public MM_DX_Contouring(Direct3DSurface surface)
        {
            _surface = surface;
            _kdTree = new KDTree.KDTree<MM_Substation>(2);

            if (featureEnableContour)
                _redrawTimer = new Timer(AllowRedraw, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void AllowRedraw(object state)
        {
            draw = true;
        }

        public void UpdateCoordinates(IList<MM_Substation> subs, int zoomLevel)
        {
            if (!featureEnableContour) return;

            substations = subs.ToList();

            //UpdateCoordinatesTriangulation(subs, zoomLevel);
            UpdateCoordinatesKDTree(subs, zoomLevel);
            _bmp = null;
            draw = false;
            _redrawTimer.Change(2000, Timeout.Infinite);
        }


        public void Update(RectangleF view)
        {
            if (!featureEnableContour) return;

            //UpdateTriangulation(view);
            _bmp = null;
            draw = false;
            _redrawTimer.Change(2000, Timeout.Infinite);
        }
        public void DrawContour(RenderTarget renderTarget2d, RectangleF view, int zoomLevel)
        {
            if (!featureEnableContour) return;

            //DrawContourTringulation();
            DrawKDTree(renderTarget2d, view, zoomLevel);

        }

        private async void DrawKDTree(RenderTarget renderTarget2d, RectangleF view, int zoomLevel)
        {
            if (substations == null || substations.Count == 0 || !draw) return;

            // renderTarget2d.Clear(new Color4(0,0,0,0));

            if (_bmp == null)
            {
                float min = float.MaxValue;
                float max = float.MinValue;
                foreach (MM_Substation sub in substations)
                {
                    var pu = float.IsNaN(sub.Average_pU) ? 1 : sub.Average_pU;

                    if (pu < min) min = pu;
                    if (pu > max) max = pu;
                }

                var range = Math.Max(Math.Abs(1 - min), Math.Abs(max - 1));
                max = 1 + range;
                min = 1 - range;

                int height = 64 * 2;
                int width = 96 * 2;
                int stride = width * sizeof(int);
                var bitmapProperties = new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));

                using (var tempStream = new DataStream(height * stride, true, true))
                {
                    for (int yPixel = 0; yPixel < height; yPixel++)

                    {
                        double yPosition = ((double)yPixel / height) * view.Height + view.Top;

                        for (int xPixel = 0; xPixel < width; xPixel++)
                        {
                            double xPosition = ((double)xPixel / width) * view.Width + view.Left;

                            int searchRange = 1;
                            int maxSearchRange = 5;
                            var neighbors = _kdTree.NearestNeighbors(new[] { xPosition, yPosition }, 25, zoomLevel);

                            while (!neighbors.MoveNext() && searchRange < maxSearchRange)
                            {
                                neighbors = _kdTree.NearestNeighbors(new[] { xPosition, yPosition }, 25, Math.Pow(zoomLevel, ++searchRange));
                            }
                            neighbors = _kdTree.NearestNeighbors(new[] { xPosition, yPosition }, 25, Math.Pow(zoomLevel, ++searchRange));
                            neighbors.MoveNext();
                            double totalWeight = 0;
                            double totalValue = 0;
                            List<Tuple<double, MM_Substation>> subWeights = new List<Tuple<double, MM_Substation>>();

                            int numMatches = 0;
                            if (neighbors.CurrentDistance >= 0)
                            {
                                do
                                {
                                    var curDistance = neighbors.CurrentDistance / zoomLevel;
                                    var substation = (MM_Substation)((IEnumerator)neighbors).Current;
                                    if (float.IsNaN(substation.Average_pU)) continue;

                                    var weight = curDistance < double.Epsilon ? 1 : 1 / curDistance;
                                    totalWeight += weight;
                                    totalValue += substation.Average_pU * weight;
                                    subWeights.Add(new Tuple<double, MM_Substation>(weight, substation));
                                    numMatches++;
                                }
                                while (neighbors.MoveNext());
                            }

                            double ratio = 0;
                            if (numMatches > 0)
                            {
                                totalValue /= totalWeight;
                                ratio = (totalValue - min) / (max - min);
                                if (numMatches <= 0)
                                    ratio = 0;
                            }

                            if (numMatches > 0)
                            {
                                byte B = (byte)(ratio * byte.MaxValue);
                                byte G = ratio > 0.4 && ratio < 0.6 ? (byte)(1 - (Math.Abs(ratio - 0.5) / 0.2) * byte.MaxValue) : (byte)0;
                                byte R = (byte)(1 - ratio * byte.MaxValue);
                                byte A = 255;
                                int rgba = R | (G << 8) | (B << 16) | (A << 24);

                                tempStream.Write(rgba);
                            }
                            else
                            {
                                tempStream.Write(0);
                            }
                        }
                    }
                    tempStream.Position = 0;
                    _bmp = new SharpDX.Direct2D1.Bitmap(renderTarget2d, new Size2(width, height), tempStream, stride, bitmapProperties);
                }
            }
            if (_bmp != null && _surface != null)
            {
                var dest = new SharpDX.RectangleF(-_surface.RenderTarget2D.Transform.M31, -_surface.RenderTarget2D.Transform.M32, _surface.RenderTarget2D.Size.Width, _surface.RenderTarget2D.Size.Height);
                _surface.RenderTarget2D.DrawBitmap(_bmp, dest, 0.5f, BitmapInterpolationMode.Linear);
            }
        }

        private void UpdateCoordinatesKDTree(IList<MM_Substation> subs, int zoomLevel)
        {
            foreach (var sub in subs)
            {
                var xy = MM_Coordinates.LngLatToXY(sub.LngLat, zoomLevel);
                _kdTree.AddPoint(new double[] { xy.X, xy.Y }, sub);
            }
        }

    }
}

