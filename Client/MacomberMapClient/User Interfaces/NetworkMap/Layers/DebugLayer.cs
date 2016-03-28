using System.Linq;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    public class DebugLayer : RenderLayer
    {

        /// <summary>The debug <see cref="TextFormat"/>.</summary>
        protected TextFormat DebugTextFormat;

        private float[] fpsBuffer = Enumerable.Repeat(15f, 60).ToArray();
        private float[] updateMsBuffer = Enumerable.Repeat(0f, 60).ToArray();
        private float[] renderMsBuffer = Enumerable.Repeat(0f, 60).ToArray();
        private int fpsBufferIx = 0;
        private float _averageFps;
        private float _frameRenderMs;
        private float _frameUpdateMs;


        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        public DebugLayer(Direct3DSurface surface) : base(surface, "Debug Layer", 99999)
        {
            
        }

        public override void LoadContent()
        {
            if (DebugTextFormat != null && !DebugTextFormat.IsDisposed)            
                DebugTextFormat.Dispose();
            DebugTextFormat = new TextFormat(Surface.FactoryDirectWrite, "Calibri", 16) { TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Near };

            WhiteBrush = new SolidColorBrush(Surface.RenderTarget2D, SharpDX.Color.White);
            RedBrush = new SolidColorBrush(Surface.RenderTarget2D, SharpDX.Color.Red);
            GreenBrush = new SolidColorBrush(Surface.RenderTarget2D, SharpDX.Color.LimeGreen);
            BlueBrush = new SolidColorBrush(Surface.RenderTarget2D, SharpDX.Color.Blue);
            GrayBrush = new SolidColorBrush(Surface.RenderTarget2D, new RawColor4(.3f, .3f, .3f, 1));
        }

        public SolidColorBrush GrayBrush;
        public SolidColorBrush BlueBrush;
        public SolidColorBrush GreenBrush;
        public SolidColorBrush RedBrush;
        public SolidColorBrush WhiteBrush;

        public override void UnloadContent()
        {
            if (GrayBrush != null)
                GrayBrush.Dispose();
            GrayBrush = null;

            if (BlueBrush != null)
                BlueBrush.Dispose();
            BlueBrush = null;

            if (GreenBrush != null)
                GreenBrush.Dispose();
            GreenBrush = null;

            if (RedBrush != null)
                RedBrush.Dispose();
            RedBrush = null;

            if (WhiteBrush != null)
                WhiteBrush.Dispose();
            WhiteBrush = null;

            if (DebugTextFormat != null && !DebugTextFormat.IsDisposed)
                DebugTextFormat.Dispose();
        }

        public override void Update(RenderTime updateTime)
        {
            base.Update(updateTime);

        }

        public override void Render(RenderTime renderTime)
        {
            base.Render(renderTime);


            // Always Calculate FPS
            float frameFps = (float)(1.0f / UpdateTime.ElapsedTime.TotalSeconds);
            _frameRenderMs = (float)Surface._renderDebugTimer.ElapsedTime.TotalMilliseconds;
            _frameUpdateMs = (float)(Surface._updateDebugTimer.ElapsedTime.TotalMilliseconds);
            // frameUpdateMs = 4;

            // update our performance buffers
            int bufIx = fpsBufferIx++ % fpsBuffer.Length;
            fpsBuffer[bufIx] = frameFps;
            renderMsBuffer[bufIx] = _frameRenderMs;
            updateMsBuffer[bufIx] = _frameUpdateMs;
            _averageFps = fpsBuffer.Average();


            var maxFps = fpsBuffer.Max();
            float updateMsMax = updateMsBuffer.Max();
            float renderMsMax = renderMsBuffer.Max();
            var maxRenderMs = renderMsMax + updateMsMax;

            var avgRenderMs = renderMsBuffer.Average();
            var avgUpdateMs = updateMsBuffer.Average();
            var avgRenderMsLine = 120f * (avgRenderMs / maxRenderMs);
            var avgUpdateMsLine = 120f * (avgUpdateMs / maxRenderMs);
            var avgFpsLine = 120f * (_averageFps / maxFps);

            MM_Repository.AverageFPS = _averageFps;

            // Don't draw debug if FPS is not active
            if (!MM_Repository.OverallDisplay.FPS) return;

            // Add debug text
            float start = 0;
            var t = new TextLayout(Surface.FactoryDirectWrite, string.Format("Est FPS Avg: {0:0}",_averageFps), DebugTextFormat, Surface.ClientRectangle.Width, 500);
            Surface.RenderTarget2D.DrawTextLayout(new Vector2(0, start), t, WhiteBrush);
            start += t.Metrics.Height + 2;
            t.Dispose();

            t = new TextLayout(Surface.FactoryDirectWrite, string.Format("Render: {0:00.000} ms | Avg: {1:0} | Max: {2:0}", _frameRenderMs,avgRenderMs,renderMsMax),DebugTextFormat, Surface.ClientRectangle.Width, 500);
            Surface.RenderTarget2D.DrawTextLayout(new Vector2(0, start), t, GreenBrush);
            start += t.Metrics.Height + 2;
            t.Dispose();

            t = new TextLayout(Surface.FactoryDirectWrite, string.Format("Update: {0:00.000} ms | Avg: {1:0}| Max: {2:0}", _frameUpdateMs, avgUpdateMs,updateMsMax),DebugTextFormat, Surface.ClientRectangle.Width, 500);
            Surface.RenderTarget2D.DrawTextLayout(new Vector2(0, start), t, RedBrush);
            start += t.Metrics.Height + 2;
            t.Dispose();

            t = new TextLayout(Surface.FactoryDirectWrite, string.Format("Driver Type: {0}",Surface.DriverType), DebugTextFormat, Surface.ClientRectangle.Width, 500);
            Surface.RenderTarget2D.DrawTextLayout(new Vector2(0, start), t, BlueBrush);
            start += t.Metrics.Height + 2;
            t.Dispose();
            foreach (var profile in Surface.ProfileStats.ToList().OrderByDescending(p => p.Value.RenderTime))
            {
                t = new TextLayout(Surface.FactoryDirectWrite, string.Format("Layer {0,-20}\tUpdate {1:00.000} ms\tRender {2:00.000} ms",profile.Key,profile.Value.UpdateTime,profile.Value.RenderTime), DebugTextFormat, Surface.ClientRectangle.Width, 500);
                
                Surface.RenderTarget2D.DrawTextLayout(new Vector2(0, start), t, GrayBrush);
                start += t.Metrics.Height + 2;
                t.Dispose();
            }

            start = Surface.RenderTarget2D.Size.Height - 10;
            var aa = Surface.RenderTarget2D.AntialiasMode;

            t = new TextLayout(Surface.FactoryDirectWrite, string.Format("Scale: {0:0} ms  | {1:0} FPS", maxRenderMs, maxFps), DebugTextFormat, Surface.ClientRectangle.Width, 500);
            Surface.RenderTarget2D.DrawTextLayout(new Vector2(fpsBuffer.Length + 10, start - 120 - t.Metrics.Height / 2), t, WhiteBrush);
            t.Dispose();


            Surface.RenderTarget2D.AntialiasMode = AntialiasMode.Aliased;


            for (int i = 0; i < fpsBuffer.Length; i++)
            {
                int ix = (i + bufIx) % fpsBuffer.Length;
                Surface.RenderTarget2D.DrawLine(new Vector2(i, start - 120f * (fpsBuffer[ix] / maxFps)), new Vector2(i, start), GrayBrush, 1f);

                var renderMsHeight = 120f * (renderMsBuffer[ix] / maxRenderMs);
                var updateMsHeight = 120f * (updateMsBuffer[ix] / maxRenderMs);
                Surface.RenderTarget2D.DrawLine(new Vector2(i, start - updateMsHeight - renderMsHeight), new Vector2(i, start), GreenBrush, 1f);

                Surface.RenderTarget2D.DrawLine(new Vector2(i, start - updateMsHeight), new Vector2(i, start), RedBrush, 1f);
            }

            Surface.RenderTarget2D.DrawLine(new Vector2(0, start - 120), new Vector2(fpsBuffer.Length + 10, start - 120), WhiteBrush, 1f);
            Surface.RenderTarget2D.DrawLine(new Vector2(0, start - avgUpdateMsLine), new Vector2(fpsBuffer.Length + 10, start - avgUpdateMsLine), RedBrush, 1f);
            Surface.RenderTarget2D.DrawLine(new Vector2(0, start - avgRenderMsLine), new Vector2(fpsBuffer.Length + 10, start - avgRenderMsLine), GreenBrush, 1f);
            Surface.RenderTarget2D.DrawLine(new Vector2(0, start - avgFpsLine), new Vector2(fpsBuffer.Length + 10, start - avgFpsLine), GrayBrush, 1f);


            Surface.RenderTarget2D.AntialiasMode = aa;


            //var LastFPS = 1000 / renderTime.ElapsedTime.Milliseconds;

            //if (MM_Repository.OverallDisplay.FPS)
            //{
            //    var tt = new TextLayout(Surface.FactoryDirectWrite, "FR: " + LastFPS.ToString(), DebugTextFormat, 200, 32);
            //    Surface.RenderTarget2D.DrawTextLayout(new Vector2(125, 0), tt, Surface.WhiteBrush);
            //}
        }

    }
}