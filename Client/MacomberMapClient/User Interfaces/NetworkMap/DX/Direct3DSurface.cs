using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.NetworkMap.Layers;
using MacomberMapClient.User_Interfaces.NetworkMap.Proxies;
using MacomberMapCommunications;
using MacomberMapCommunications.Extensions;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
//using System.Drawing.Imaging;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.WIC.Bitmap;
using Color = System.Drawing.Color;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.Direct2D1.Factory;
using Buffer = SharpDX.Direct3D11.Buffer;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using Resource = SharpDX.Direct3D11.Resource;

namespace MacomberMapClient.User_Interfaces.NetworkMap.DX
{
    public class RenderProfile
    {
        private readonly float[] _updateTime = new float[30];
        private readonly float[] _renderTime = new float[30];
        int _updateTimeIx = 0;
        int _renderTimeIx = 0;
        public float UpdateTime
        {
            get { return _updateTime.Average(); }
            set
            {
                _updateTimeIx = (++_updateTimeIx % _updateTime.Length);
                _updateTime[_updateTimeIx] = value;
            }
        }

        public float RenderTime
        {
            get { return _renderTime.Average(); }
            set
            {
                _renderTimeIx = (++_renderTimeIx % _renderTime.Length);
                _renderTime[_renderTimeIx] = value;
            }
        }
    }

    /// <summary>
    /// A DirectX rendering control capable of rendering Direct2D and Direct3D. 
    /// </summary>
    public class Direct3DSurface : Control
    {
        private SwapChain _swapChain;
        private Device _device;
        private Texture2D _backBuffer;
        private DateTime _lastUpdate;
        internal RenderTime _renderTimer;
        internal RenderTime _updateTimer;

        internal RenderTime _renderDebugTimer;
        internal RenderTime _updateDebugTimer;
        private BrushContainer _brushes;
        private TextFormatContainer _fonts;

        public BrushContainer Brushes { get { return _brushes; } }
        public TextFormatContainer Fonts { get { return _fonts; } }

        /// <summary>Wheter or not the map has been moved, and elements need to be recalculated</summary>
        public bool IsDirty { get; set; }

        public MM_Coordinates Coordinates { get; protected set; }

        public Dictionary<string, RenderProfile> ProfileStats { get; private set; }

        /// <summary>
        /// The device interface represents a virtual adapter; it is used to create other resources.
        /// </summary>
        public Device Device
        {
            get { return _device; }
        }

        protected List<IRenderComponent> Layers { get; private set; }

        /// <summary>
        /// Creats a <see cref="PathGeometry"/> from a collection of GPS <see cref="PointF"/>s. This object should be disposed.
        /// </summary>
        /// <param name="lngLats">GPS Coordinates.</param>
        /// <param name="closed">Close the geometry.</param>
        /// <param name="filled">Fill the geometry.</param>
        /// <returns>IDisposable Object</returns>
        public PathGeometry CreateRegionPathGeometry(IList<Vector2> xy, out RawRectangleF bounds, bool closed = true, bool filled = false, bool simplify = false, float compression = 0.1f, float scaleX = 1f, float scaleY = 1f)
        {
            PathGeometry geometry = new PathGeometry(Factory2D);
            int pnts = 0;

            Vector2 scaleVector = new Vector2(scaleX, scaleY);
            if (xy.Count > 0)
            {
                var point = xy[0];
                if (Math.Abs(scaleX - 1f) > float.Epsilon || Math.Abs(scaleY - 1f) > float.Epsilon)
                    point *= scaleVector;
                var prev = point;

                float minX = point.X;
                float minY = point.Y;
                float maxX = point.X;
                float maxY = point.Y;

                using (var sink = geometry.Open())
                {
                    sink.SetFillMode(SharpDX.Direct2D1.FillMode.Winding);
                    sink.BeginFigure(point, filled ? FigureBegin.Filled : FigureBegin.Hollow);
                    int count = xy.Count;

                    for (int i = 1; i < count; i++)
                    {
                        // compression
                        if (simplify && i != count - 1) // don't compress last point
                        {
                            // linear compression
                            var next = xy[i + 1];
                            bool isLinear = LineProxy.HitTestLineSegmentF(xy[i].X, xy[i].Y, prev.X, prev.Y, next.X, next.Y, compression);

                            // distance compression
                            float d2 = LineProxy.DistanceSquared(xy[i].X, xy[i].Y, prev.X, prev.Y);

                            if (isLinear || d2 < compression)
                            {
                                continue;
                            }
                        }

                        if (Math.Abs(scaleX - 1f) > float.Epsilon || Math.Abs(scaleY - 1f) > float.Epsilon)
                            point = xy[i] * scaleVector;
                        else
                            point = xy[i];

                        sink.AddLine(point);
                        if (point.X < minX) minX = point.X;
                        if (point.X > maxX) maxX = point.X;

                        if (point.Y < minY) minY = point.Y;
                        if (point.Y > maxY) maxY = point.Y;


                        prev = xy[i];
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

        /// <summary>Returns all the layers of a specified type.</summary>
        /// <typeparam name="TLayer">The layer type.</typeparam>
        /// <returns>IEnumerable of components</returns>
        public IEnumerable<TLayer> GetLayers<TLayer>() where TLayer : IRenderComponent
        {
            return Layers.OfType<TLayer>().Select(layer => (TLayer)layer);
        }

        /// <summary>
        /// Adds a layer to the render components of the surface.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <param name="order">Override the layer order.</param>
        public void AddLayer(IRenderComponent layer, int order = -1)
        {
            if (order != -1)
                layer.Order = order;

            lock (Layers)
            {
                Layers.AddSorted(layer);
            }
        }

        /// <summary>
        /// Removes a layer from the render components.
        /// </summary>
        /// <param name="layer">The component to remove.</param>
        protected void RemoveLayer(IRenderComponent layer)
        {
            lock (Layers)
            {
                Layers.Remove(layer);
            }
        }

        /// <summary>
        /// Reorders layers based on their layer order property.
        /// </summary>
        public void UpdateLayerOrder()
        {
            lock (Layers)
            {
                var temp = Layers.OrderByDescending(l => l.Order).ToList();
                Layers = temp;
            }
        }

        /// <summary>The DXGI Surface.</summary>
        protected Surface DxgiSurface { get; private set; }

        /// <summary>The Direct2D Factory.</summary>
        public Factory Factory2D { get; private set; }

        /// <summary>The DirectWrite Factory.</summary>
        public SharpDX.DirectWrite.Factory FactoryDirectWrite { get; private set; }

        /// <summary>The DXGI Factory.</summary>
        public SharpDX.DXGI.Factory FactoryDxgi { get; private set; }

        /// <summary>The Direct2D RenderTarget.</summary>
        public SharpDX.Direct2D1.RenderTarget RenderTarget2D { get; private set; }

        /// <summary>The Direct3D RenderTargetView.</summary>
        public RenderTargetView RenderTargetView { get; private set; }

        /// <summary>The Direct3D Device Context.</summary>
        public SharpDX.Direct3D11.DeviceContext Context { get; private set; }

        /// <summary> Render debug information.</summary>
        public bool IsDebug { get; set; }

        /// <summary>Is the DirectX surface ready to be drawn to?</summary>
        protected bool PresenterReady { get; private set; }

        private AntialiasMode _antialiasMode;
        private DriverType _driverType;
        private ScreenLogLayer _logLayer;
        public DriverType DriverType { get { return _driverType; } }



        /// <summary>
        /// Initializes a new <see cref="Direct3DSurface"/>.
        /// </summary>
        protected Direct3DSurface()
        {
            IsDirty = true;
            ProfileStats = new Dictionary<string, RenderProfile>();
            Layers = new List<IRenderComponent>();

            // Don't use GDI double buffer here, it causes problems
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.UserPaint, true);

            // Enable Antialiasing.
            AntialiasMode = AntialiasMode.PerPrimitive;
        }

        /// <summary>
        /// The <see cref="AntialiasMode"/> of the surface. Changing this resets the presenter.
        /// </summary>
        public AntialiasMode AntialiasMode
        {
            //get { return SystemInformation.TerminalServerSession ? AntialiasMode.Aliased : _antialiasMode; }
            get { return _antialiasMode; }
            protected set { _antialiasMode = value; }
        }

        /// <summary>
        /// Get the parent form of this control.
        /// </summary>
        public Form ParentForm
        {
            get
            {
                Control parent = this.Parent;
                while (!(parent is Form))
                {
                    parent = parent.Parent;
                }
                return (Form)parent;
            }
        }

        /// <summary>
        /// Reinitialize the DirectX swapchain and render targets.
        /// </summary>
        public void ResetPresenter()
        {
            if (!DesignMode && PresenterReady)
            {
                UpdatePresenter();
                Invalidate();
            }
        }

        /// <summary>
        /// Initialize graphics components when the control is created.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (DesignMode) return;

            _lastUpdate = DateTime.Now;
            _renderTimer = new RenderTime();
            _updateTimer = new RenderTime();
            _renderDebugTimer = new RenderTime();
            _updateDebugTimer = new RenderTime();
            InitGraphics();
            LoadContent();

            RenderLoop();
        }

        #region Override in Derived Controls

        /// <summary>
        /// Occurs when the DirectX device is initialized. Objects should load local resources here.
        /// </summary>
        protected virtual void OnLoadContent() { }

        /// <summary>
        /// Occurs when the DirectX device is unloading. Cleanup any disposable objects referencing factories or render targets here.
        /// </summary>
        protected virtual void OnUnloadContent() { }

        /// <summary>
        /// Update cycle: perform calculations and update data here.
        /// </summary>
        protected virtual void OnUpdating(RenderTime updateTime) { }

        /// <summary>
        /// Update cycle: perform calculations and update data here.
        /// </summary>
        protected virtual void OnUpdated(RenderTime updateTime) { }

        /// <summary>
        /// Render cycle: only perform drawing code here. Use OnUpdateDx to perform calculations.
        /// </summary>
        protected virtual void OnRendering(RenderTime renderTime) { }

        /// <summary>
        /// Render cycle: only perform drawing code here. Use OnUpdateDx to perform calculations.
        /// </summary>
        protected virtual void OnRendered(RenderTime renderTime) { }

        /// <summary>
        /// Render cycle: only perform drawing code here. Use OnUpdateDx to perform calculations.
        /// </summary>
        protected virtual void OnRender3D() { }

        #endregion

        /// <summary>
        /// Override control OnPaint to Render DirectX.
        /// </summary>
        /// <param name="e">Paint event arguments.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                e.Graphics.Clear(Color.CornflowerBlue);
                e.Graphics.DrawString("DX Control", DefaultFont, System.Drawing.Brushes.White, 0, 0);
            }
            else
            {
                // since we aren't using a render loop, perform update then render. Ideally, update would occur separately from render.
                //   RenderLoop();
            }
        }

        /// <summary>
        /// Hook resize event to rebuild our render targets and swap chain.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResetPresenter();
        }

        public void RenderLoop()
        {
            var renderLoopStartTime = DateTime.Now;
            var sw = Stopwatch.StartNew();

            _updateTimer.Update(renderLoopStartTime - _lastUpdate);

            UpdateDx(_updateTimer);

            sw.Stop();
            _updateDebugTimer.Update(sw.Elapsed);

            sw.Restart();

            _renderTimer.Update(renderLoopStartTime - _lastUpdate);
            Render(_renderTimer);
            sw.Stop();
            _renderDebugTimer.Update(sw.Elapsed);

            _lastUpdate = renderLoopStartTime;
        }



        /// <summary>
        /// Initialize DirectX factories, swap chain and render targets.
        /// </summary>
        protected void InitGraphics()
        {
            var width = Size.Width;
            var height = Size.Height;
            Factory2D = new Factory();
            FactoryDirectWrite = new SharpDX.DirectWrite.Factory();

            var desc = new SwapChainDescription
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
            };

            // use software driver if RDP or VirtualMachine
            _driverType = MM_System_Profiling.IsTerminalOrVirtual() ? DriverType.Warp : DriverType.Hardware;
            bool swapChainCreated = false;
            while (!swapChainCreated)
            {
                try
                {
                    Device.CreateWithSwapChain(_driverType, DeviceCreationFlags.BgraSupport, desc, out _device, out _swapChain);
                    MM_System_Interfaces.LogError("Initialized DirectX Driver: {0}", _driverType);
                    swapChainCreated = true;
                }
                catch (Exception ex)
                {
                    // downgrade driver and try again using outer while loop
                    if (_driverType == DriverType.Hardware)
                        _driverType = DriverType.Warp;
                    else if (_driverType == DriverType.Warp)
                        _driverType = DriverType.Software;
                    else
                    {
                        MM_System_Interfaces.LogError(ex);
                        throw new NotSupportedException("DirectX Rendering could not be initialized using hardware of software.", ex);
                    }
                }
            }

            Context = _device.ImmediateContext;

            // Ignore all windows events
            FactoryDxgi = _swapChain.GetParent<SharpDX.DXGI.Factory>();
            FactoryDxgi.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            _backBuffer = Resource.FromSwapChain<Texture2D>(_swapChain, 0);
            RenderTargetView = new RenderTargetView(_device, _backBuffer);
            DxgiSurface = _backBuffer.QueryInterface<Surface>();
            RenderTarget2D = new RenderTarget(Factory2D, DxgiSurface, new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
            RenderTarget2D.AntialiasMode = AntialiasMode;

            PresenterReady = true;
        }


        /// <summary>
        /// Create local resources then call <see cref="OnLoadContent()"/> for inheriting objects.
        /// </summary>
        private void LoadContent()
        {
            AddLayer(new DebugLayer(this), 9999);

            _logLayer = new ScreenLogLayer(this);
            AddLayer(_logLayer, 9998);

            _brushes = new BrushContainer(this);
            _fonts = new TextFormatContainer(this);
            OnLoadContent();

            lock (Layers)
            {
                foreach (var layer in Layers)
                {
                    layer.LoadContent();
                }
            }
        }

        /// <summary>
        /// Unloads local content then calls <see cref="OnUnloadContent()"/> for inheriting controls.
        /// </summary>
        protected void UnloadContent()
        {
            _brushes.Cleanup();
            _brushes = null;
            _fonts.Cleanup();
            _fonts = null;

            foreach (var layer in Layers)
            {
                layer.UnloadContent();

                var disposableLayer = layer as IDisposable;
                if (disposableLayer != null)
                    disposableLayer.Dispose();
            }
            Layers.Clear();

            OnUnloadContent();
        }


        #region Overrides of Control

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Click"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. </param>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            // Focus this on click.
            if (!this.Focused)
                this.Focus();
        }

        #endregion

        /// <summary>
        /// Add a message to the map screen. Will fade out after display seconds had elapsed.
        /// </summary>
        /// <param name="message">The message text</param>
        /// <param name="color">The message color</param>
        /// <param name="displaySeconds">Time to display the message</param>
        public void AddOnscreenMessage(string message, RawColor4 color, float displaySeconds = 10f)
        {
            if (_logLayer != null)
                _logLayer.AddMessage(message, color, displaySeconds);
        }

        /// <summary>
        /// Local render. Performs timing calculations and debug rendering if required then calls <see cref="OnRendering"/> for inheriting controls. Presents the swap chain when ready.
        /// </summary>
        protected void Render(RenderTime renderTime)
        {
            if (!PresenterReady) return;
            // Update our derived control

            RenderTarget2D.BeginDraw();
            OnRendering(renderTime);

            lock (Layers)
            {
                Stopwatch sw = Stopwatch.StartNew();
                foreach (var layer in Layers)
                {
                    RenderProfile profile;
                    if (!ProfileStats.TryGetValue(layer.Name, out profile))
                    {
                        profile = new RenderProfile();
                        ProfileStats.Add(layer.Name, profile);
                    }

                    var start = sw.Elapsed;
                    layer.Render(renderTime);
                    profile.RenderTime = (float)(sw.Elapsed - start).TotalMilliseconds;
                }
                sw.Stop();
            }

            OnRendered(renderTime);
            RenderTarget2D.EndDraw();

            OnRender3D();

            _swapChain.Present(0, PresentFlags.None);

        }

        public SharpDX.Direct2D1.BitmapRenderTarget CreateRenderTarget(int height, int width)
        {
            var desc = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.R8G8B8A8_UNorm,
                Height = height,
                Width = width,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            var texture = new Texture2D(Device, desc);
            var d3DSurface = texture.QueryInterface<Surface>();
            var rt2d = new RenderTarget(Factory2D, d3DSurface, new RenderTargetProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)));
            var rt = new SharpDX.Direct2D1.BitmapRenderTarget(rt2d, CompatibleRenderTargetOptions.None);
            rt.AntialiasMode = AntialiasMode;
            return rt;
        }


        /// <summary>
        /// Updates DirectX objects and calls.
        /// Layers are updated first, then the derived control is updated.
        /// </summary>
        protected virtual void UpdateDx(RenderTime updateTime)
        {
            OnUpdating(updateTime);

            lock (Layers)
            {
                Stopwatch sw = Stopwatch.StartNew();
                foreach (var layer in Layers)
                {
                    RenderProfile profile;
                    if (!ProfileStats.TryGetValue(layer.Name, out profile))
                    {
                        profile = new RenderProfile();
                        ProfileStats.Add(layer.Name, profile);
                    }

                    var start = sw.Elapsed;
                    layer.Update(updateTime);
                    profile.UpdateTime = (float)(sw.Elapsed - start).TotalMilliseconds;
                }
            }

            // Update our derived control
            OnUpdated(updateTime);
        }

        /// <summary>
        /// Rebuilds the swap chain and render targets.
        /// </summary>
        protected void UpdatePresenter()
        {
            PresenterReady = false;

            // release our buffers since we are updating the swap chain
            RenderTarget2D.Dispose();
            DxgiSurface.Dispose();
            RenderTargetView.Dispose();
            _backBuffer.Dispose();

            // resize swapchain
            _swapChain.ResizeBuffers(1, Size.Width, Size.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

            // recreate the buffers
            _backBuffer = Resource.FromSwapChain<Texture2D>(_swapChain, 0);
            RenderTargetView = new RenderTargetView(_device, _backBuffer);
            DxgiSurface = _backBuffer.QueryInterface<Surface>();
            RenderTarget2D = new RenderTarget(Factory2D, DxgiSurface, new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
            RenderTarget2D.AntialiasMode = AntialiasMode;
            PresenterReady = true;

            Brushes.Cleanup();
            Fonts.Cleanup();
            IsDirty = true;
            foreach (IRenderComponent component in Layers)
            {
                component.PresenterReset();
            }
        }

        #region IDisposable

        /// <summary>
        /// Release the unmanaged resources used by the <see cref="Direct3DSurface"/> and it's child controls and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release managed resources and unmanaged resources. False to only release unmanaged resources.</param>

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                UnloadContent();

                Factory2D.Dispose();
                FactoryDirectWrite.Dispose();
                FactoryDxgi.Dispose();

                RenderTargetView.Dispose();
                RenderTarget2D.Dispose();

                _backBuffer.Dispose();
                _swapChain.Dispose();

                Context.Dispose();
                _device.Dispose();
                ProfileStats.Clear();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Direct3DSurface()
        {
            Dispose(false);
        }

        #endregion
    }
}