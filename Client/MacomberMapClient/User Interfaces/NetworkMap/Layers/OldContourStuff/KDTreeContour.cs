using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using KDTree;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Framework;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Buffer = SharpDX.Direct3D11.Buffer;
using Color = System.Drawing.Color;
using RectangleF = System.Drawing.RectangleF;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    public interface IBaseContour
    {
        SharpDX.Direct2D1.Bitmap RenderRegionBitmap(System.Drawing.RectangleF view, int zoomLevel);
        Texture2D RenderRegionTexture(System.Drawing.RectangleF view, int zoomLevel);
        void UpdateCoordinates(IList<Vector2> coordinates, IList<float> values);
        void UpdateValues(IList<float> values);
    }

    public abstract class BaseContour : IDisposable, IBaseContour
    {
        private readonly Direct3DSurface _surface;

        protected IList<Vector2> Coordinates;
        protected IList<float> Values;

        protected RenderTarget _renderTarget2D;
        protected Texture2D _tileTexture2D;
        protected RenderTargetView _renderTargetView;
        protected Surface _dxgiSurface;
        protected BitmapRenderTarget _bitmapRenderTarget;

        protected Buffer _vertexBuffer;
        protected Buffer _indexBuffer;

        protected VertexShader _vertexShader;
        protected PixelShader _pixelShader;
        protected InputLayout _inputLayout;
        private int _triangleCount;
        private int _lastResolution;

        protected float ValueMin;
        protected float ValueMax;
        protected float ValueAvg;
        private short[] _indices;

        protected BaseContour(Direct3DSurface surface)
        {
            if (surface == null)
                throw new ArgumentNullException("Surface", "Surface is null. Cannot create render target for contour.");

            _surface = surface;

            var desc = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource ,
                Format = Format.R8G8B8A8_UNorm,
                Height = MM_Repository.OverallDisplay.MapTileSize.Height,
                Width = MM_Repository.OverallDisplay.MapTileSize.Width,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            _tileTexture2D = new Texture2D(_surface.Device, desc);
            
           
            _dxgiSurface = _tileTexture2D.QueryInterface<Surface>();


            _renderTarget2D = new RenderTarget(_surface.Factory2D, _dxgiSurface, new RenderTargetProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)));
            _bitmapRenderTarget = new BitmapRenderTarget(_renderTarget2D, CompatibleRenderTargetOptions.None) { AntialiasMode = _surface.AntialiasMode };

            _renderTargetView = new RenderTargetView(_surface.Device, _tileTexture2D);

            InitializeShader();
        }

        protected virtual void InitializeShader()
        {
            try
            {
                string shaderSource = EmbeddedResourceReader.ReadTextResource("MacomberMapClient.Effects.VertexColorShader.fx", Assembly.GetExecutingAssembly());
                // load our default shader

                var vertexShaderByteCode = ShaderBytecode.Compile(shaderSource, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
                _vertexShader = new VertexShader(_surface.Device, vertexShaderByteCode);

                var pixelShaderByteCode = ShaderBytecode.Compile(shaderSource, "PS", "ps_4_0", ShaderFlags.None, EffectFlags.None);
                _pixelShader = new PixelShader(_surface.Device, pixelShaderByteCode);

                // Layout from VertexShader input signature
                _inputLayout = new InputLayout(
                    _surface.Device,
                    ShaderSignature.GetInputSignature(vertexShaderByteCode),
                    new[]
                    {
                        new SharpDX.Direct3D11.InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new SharpDX.Direct3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
                        new SharpDX.Direct3D11.InputElement("NORMAL", 0, Format.R32G32B32_Float, 32, 0),
                    });

                // Instantiate Vertex buiffer from vertex data

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public virtual void UpdateCoordinates(IList<Vector2> coordinates, IList<float> values)
        {
            if (coordinates.Count != values.Count)
                throw new ArgumentOutOfRangeException("values", "Values must be the same length as coordinates.");

            Coordinates = coordinates;
            UpdateValues(values);
        }

        public virtual void UpdateValues(IList<float> values)
        {
            if (Coordinates.Count != values.Count)
                throw new ArgumentOutOfRangeException("values", "Values must be the same length as coordinates.");

            Values = values;

            ValueMax = Values.Max();
            ValueMin = Values.Min();
            ValueAvg = Values.Average();
        }
        private void RenderRegion(RectangleF view, int zoomLevel)
        {
            UpdateBuffers(view, zoomLevel);
            RenderBuffers();
        }


        public virtual Bitmap RenderRegionBitmap(RectangleF view, int zoomLevel)
        {
            if (Coordinates == null || Values == null || Coordinates.Count == 0 || Values.Count == 0)
                return null;

            RenderRegion(view, zoomLevel);
            _bitmapRenderTarget.BeginDraw();
            _bitmapRenderTarget.DrawEllipse(new Ellipse(new Vector2(128f,128f), 32f, 32f), ((MM_Network_Map_DX)_surface).Brushes.GetBrush(Color.White), 3f);
           _bitmapRenderTarget.EndDraw();
            // _bitmapRenderTarget.draw
            var tileBmp = new Bitmap(_bitmapRenderTarget,
                                     new Size2(MM_Repository.OverallDisplay.MapTileSize.Width, MM_Repository.OverallDisplay.MapTileSize.Height),
                                     new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied), 96, 96));
            tileBmp.CopyFromBitmap(_bitmapRenderTarget.Bitmap);

            return tileBmp;
        }

        public virtual Texture2D RenderRegionTexture(RectangleF view, int zoomLevel)
        {
            if (Coordinates == null || Values == null || Coordinates.Count == 0 || Values.Count == 0)
                return null;

            RenderRegion(view, zoomLevel);

            Texture2D copy = new Texture2D(_tileTexture2D.Device, _tileTexture2D.Description);
            _surface.Context.CopyResource(_tileTexture2D, copy);
            return copy;
        }

        protected static short[] GenerateGridIndexBuffer(int xSamples, int ySamples)
        {
            if (xSamples <= 0)
                throw new ArgumentOutOfRangeException("xSamples", "Sample size must be greater than 0.");
            if (ySamples <= 0)
                throw new ArgumentOutOfRangeException("ySamples", "Sample size must be greater than 0.");

            // a quad's index buffer looks like this

            // 1 - 2
            // | / |
            // 0 - 3

            // 0, 1, 2, 3, 0, 2

            // with the 8x8 grid, the index buffer is a constant
            // the mesh  points look like this
            // 0 - 1 - 2 - 3 - 4 - 5 - 6 - 7
            // | / | / | / | / | / | / | / |
            // 8 - 9 - 10- 11- 12- 13- 14- 15
            // | / | / | / | / | / | / | / |
            // 16- 17- 19- 19- 20- 21- 22- 23
            // | / | / | / | / | / | / | / |
            // 24- 25- 26- 27- 28- 29- 30- 31
            // | / | / | / | / | / | / | / |
            // 32- 33- 34- 35- 36- 37- 38- 39
            // | / | / | / | / | / | / | / |
            // 40- 41- 42- 43- 44- 45- 46- 47
            // | / | / | / | / | / | / | / |
            // 48- 49- 50- 51- 52- 53- 54- 55
            // | / | / | / | / | / | / | / |
            // 56- 57- 58- 59- 60- 61- 62- 63

            int numTriangles = (xSamples - 1) * (ySamples - 1);
            List<short> buffer = new List<short>(numTriangles * 6); // 6 indices per triangle

            for (int y = 0; y < ySamples - 1; y++)
            {
                for (int x = 0; x < xSamples - 1; x++)
                {
                    // add a quad
                    int offset = x + (y * xSamples);

                    buffer.Add((short)(xSamples + offset));
                    buffer.Add((short)offset);
                    buffer.Add((short)(offset + 1));
                    buffer.Add((short)(xSamples + offset + 1));
                    buffer.Add((short)(xSamples + offset));
                    buffer.Add((short)(offset + 1));
                }
            }

            return buffer.ToArray();
        }

        private unsafe void UpdateBuffers(RectangleF view, int zoomLevel, int sampleResolution = 9)
        {
            // y = latitude
            // x = longitude

            // we only need to generate this if our resolution changes or our index buffer is null
            if (_indexBuffer == null || sampleResolution != _lastResolution)
            {
                _indices = GenerateGridIndexBuffer(sampleResolution, sampleResolution);
                _indexBuffer = Buffer.Create(_tileTexture2D.Device, BindFlags.IndexBuffer, _indices);
                _triangleCount = _indices.Length / 3;
            }
            _lastResolution = sampleResolution;

            var vertexData = new VertexPositionColorNormal[sampleResolution * sampleResolution];
            // sample data at vertices and create vertex buffer
            int pos = 0;
            for (int y = 0; y < sampleResolution; y++)
            {
                for (int x = 0; x < sampleResolution; x++)
                {
                    float lat = x * (view.Width / sampleResolution) + view.Left;
                    float lon = y * (view.Height / sampleResolution) + view.Top;
                    float value = SampleData(lon, lat, zoomLevel);

                    // if (ValueMax - ValueMin)
                    float valueRatio = (value - ValueMin) / (ValueMax - ValueMin);

                    vertexData[pos++] = new VertexPositionColorNormal(
                        new Vector4((float)x / (sampleResolution - 1) * 256f, (float)y / (sampleResolution - 1) * 256f, 1, 1f),
                        new Vector4(valueRatio, valueRatio, valueRatio, 0),
                        Vector3.Zero);
                }
            }

            CalculateNormals(ref vertexData, ref _indices);
            _vertexBuffer = Buffer.Create(_tileTexture2D.Device, BindFlags.VertexBuffer, vertexData, structureByteStride: sizeof(VertexPositionColorNormal));

         //   SharpDX.Direct2D1.Mesh m = new Mesh(_renderTarget2D);
       //    _renderTarget2D.drawt
        }

        protected abstract float SampleData(float lon, float lat, int zoomLevel);

        private void RenderBuffers()
        {
            if (_indexBuffer == null || _vertexBuffer == null) return;
            var context = _tileTexture2D.Device.ImmediateContext;
            
            _renderTarget2D.BeginDraw();
            _renderTarget2D.Clear(SharpDX.Color.Red);

            context.InputAssembler.InputLayout = _inputLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, 16 + 16 + 12, 0));
            context.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
            context.VertexShader.Set(_vertexShader);
            context.Rasterizer.SetViewport(new Viewport(0, 0, 256, 256));
            context.PixelShader.Set(_pixelShader);
            context.OutputMerger.SetTargets(_renderTargetView);
            context.OutputMerger.ResetTargets();
            // int indexBufferTriangleCount = _indexBuffer.Description.SizeInBytes / _indexBuffer.Description.StructureByteStride / 3;
            context.DrawIndexed(_triangleCount, 0, 0);

            _renderTarget2D.EndDraw();
        }

        /// <summary>
        /// Calculate normal space for vertices.
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Index buffer</param>
        protected static void CalculateNormals(ref VertexPositionColorNormal[] vertices, ref short[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = Vector3.Zero;

            for (int i = 0; i < indices.Length / 3; i++)
            {
                Vector3 firstvec = (Vector3)vertices[indices[i * 3 + 1]].Position - (Vector3)vertices[indices[i * 3]].Position;
                Vector3 secondvec = (Vector3)vertices[indices[i * 3]].Position - (Vector3)vertices[indices[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                vertices[indices[i * 3]].Normal += normal;
                vertices[indices[i * 3 + 1]].Normal += normal;
                vertices[indices[i * 3 + 2]].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal.Normalize();
            }
        }

        #region IDisposable

        /// <summary>
        /// Release the unmanaged resources used by the <see cref="Direct3DSurface"/> and it's child controls and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release managed resources and unmanaged resources. False to only release unmanaged resources.</param>

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                Coordinates.Clear();
                Values.Clear();

                _tileTexture2D.Dispose();
                _renderTargetView.Dispose();
                _dxgiSurface.Dispose();
                _renderTarget2D.Dispose();
                _bitmapRenderTarget.Dispose();

                _vertexBuffer.Dispose();
                _indexBuffer.Dispose();

                _vertexShader.Dispose();
                _pixelShader.Dispose();
                _inputLayout.Dispose();
            }

            //  base.Dispose(disposing);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~BaseContour()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }

    public class KDTreeContour : BaseContour
    {
        private KDTree<float> _data;

        public int CoordCount
        {
            get { return Coordinates==null?0:Coordinates.Count;}
        }

        public KDTreeContour(Direct3DSurface surface) : base(surface)
        {
            _data = new KDTree<float>(2);
        }


        public override void UpdateCoordinates(IList<Vector2> coordinates, IList<float> values)
        {
            base.UpdateCoordinates(coordinates, values);

            GenerateKDTree();
        }


        public override void UpdateValues(IList<float> values)
        {
            base.UpdateValues(values);

            GenerateKDTree();
        }

        protected override float SampleData(float lon, float lat, int zoomLevel)
        {
            var neighbors = _data.NearestNeighbors(new double[] { lat, lon }, 25, 5f);

            neighbors.MoveNext();
            double totalWeight = 0;
            double totalValue = 0;
            var weightedValues = new List<Tuple<double, float>>();

            int numMatches = 0;
            if (neighbors.CurrentDistance >= 0)
            {
                do
                {
                    var curDistance = neighbors.CurrentDistance / zoomLevel;
                    var value = (float)((IEnumerator)neighbors).Current;

                    var weight = curDistance < double.Epsilon ? 1 : 1 / curDistance;
                    totalWeight += weight;
                    totalValue += value * weight;
                    weightedValues.Add(new Tuple<double, float>(weight, value));
                    numMatches++;
                }
                while (neighbors.MoveNext());
            }

            if (numMatches <= 0 || totalWeight == 0)
                return 1;

            return (float)(totalValue / totalWeight);
        }

        private void GenerateKDTree()
        {
            var tree = new KDTree<float>(2);
            for (int i = 0; i < Coordinates.Count; i++)
            {
                var coord = Coordinates[i];
                float val = Values[i];
                tree.AddPoint(new double[] { coord.X, coord.Y }, val);
            }

            // swap kdtree with newly generated tree
            _data = tree;
        }

    }
}