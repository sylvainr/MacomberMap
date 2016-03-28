using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    class ScreenLogLayerProxy : IDisposable
    {
        public string Message;
        public RawColor4 Color;
        public float Opacity;
        public TextLayout Layout;
        public float SecondsRemaining = 5f;
        public RawVector2 LayoutSize;

        #region IDisposable Implementation

        /// <summary>Finalizer</summary>
        ~ScreenLogLayerProxy()
        {
            Dispose(false);
        }

        /// <summary>Has disposed been called on this object.</summary>
        public bool IsDisposed { get; private set; }

        public ScreenLogLayerProxy()
        {
            IsDisposed= false;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);

            // if this object inherites IDisposable from base class, make sure to call base.Dispose()
            // try
            // {
            //     Dispose(true);
            // }
            // finally 
            // {
            //     base.Dispose();
            // }

            // GC: don't bother calling finalize later
            System.GC.SuppressFinalize(this);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <param name="freeManaged">Clean up managed resources in addition to unmanaged resources.</param>
        public void Dispose(bool freeManaged)
        {
            if (!IsDisposed)
            {
                // clean up unmanaged native objects

                if (freeManaged)
                {
                    // clean up c# managed objects
                    if (Layout != null)
                    Layout.Dispose();
                    Layout = null;
                }

                IsDisposed = true;
            }
        }

        #endregion
    }

    public class ScreenLogLayer : RenderLayer
    {
        private ContentAlignment _alignment = ContentAlignment.TopRight;

        List<ScreenLogLayerProxy> _messages = new List<ScreenLogLayerProxy>();
        private Vector2 _startPosition;

        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        public ScreenLogLayer(Direct3DSurface surface) : base(surface, "Screen Logging Layer", 99)
        {

        }

        public void AddMessage(string message, RawColor4 color, float secondsToDisplay = 10f)
        {
            _messages.Add(new ScreenLogLayerProxy()
            {
                Message = message,
                Color = color,
                SecondsRemaining = secondsToDisplay,
            });

            while (_messages.Count > 50)
            {
                _messages[0].Dispose();
                _messages.RemoveAt(0);
            }
        }

        /// <summary>
        /// Perform calculations and update data.
        /// </summary>
        /// <param name="updateTime">Time since last update.</param>
        /// <remarks>include base.Update(renderTime); in overloads to preserve updating UpdateTime field.</remarks>
        public override void Update(RenderTime updateTime)
        {
            base.Update(updateTime);
            var textFormat = Surface.Fonts.GetTextFormat(MM_Repository.OverallDisplay.NetworkMapFont, 2);

            foreach (var message in _messages.ToList())
            {
                if (message.IsDisposed)
                {
                    _messages.Remove(message);
                    continue;
                }

                if ((message.SecondsRemaining -= (float)updateTime.ElapsedTime.TotalSeconds) < 0)
                {
                    message.Dispose();
                    _messages.Remove(message);
                }
                else
                {
                    // set opacity, fade out the last second
                    message.Opacity = MathUtil.Clamp(((int)(message.SecondsRemaining * 20)) / 20f, 0, 1);

                    // rebuild text layouts
                    if (message.Layout == null || message.Layout.IsDisposed)
                    {
                        message.Layout = new TextLayout(Surface.FactoryDirectWrite, message.Message, textFormat, 350, 50);
                        message.LayoutSize = new Vector2(message.Layout.Metrics.Width, message.Layout.Metrics.Height);
                    }
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
            base.Render(renderTime);

            int vertical = 0;
            if (_alignment == ContentAlignment.MiddleLeft || _alignment == ContentAlignment.MiddleCenter || _alignment == ContentAlignment.MiddleRight)
                vertical = 1;
            else if (_alignment == ContentAlignment.BottomLeft || _alignment == ContentAlignment.BottomCenter || _alignment == ContentAlignment.BottomRight)
                vertical = 2;

            int horizontal = 0;
            if (_alignment == ContentAlignment.TopCenter || _alignment == ContentAlignment.MiddleCenter || _alignment == ContentAlignment.BottomCenter)
                horizontal = 1;
            else if (_alignment == ContentAlignment.TopRight || _alignment == ContentAlignment.MiddleRight || _alignment == ContentAlignment.BottomRight)
                horizontal = 2;

            var currentPosition = _startPosition;


            foreach (var message in _messages.ToList())
            {
                if (message.IsDisposed || message.Layout == null || message.Layout.IsDisposed)
                    continue;

                var size = message.LayoutSize;
                var brush = Surface.Brushes.GetBrush(message.Color, message.Opacity);

                // shift upward before if bottom aligned
                if (vertical == 2)
                    currentPosition.Y -= size.Y;

                // set our horizontal origin
                if (horizontal == 2)
                    currentPosition.X = _startPosition.X - size.X;

                Surface.RenderTarget2D.DrawTextAtPoint(message.Layout, brush, currentPosition.X, currentPosition.Y, centerX: horizontal == 1, centerY: vertical == 1);

                // shift downward after if top aligned
                if (vertical == 0)
                    currentPosition.Y += size.Y;
            }
        }

        /// <summary>
        /// Unload content and cleanup unmanaged resources.
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            // cleanup screen proxies
            var copyMessages = _messages.ToList();
            _messages.Clear();

            foreach (var message in copyMessages)
            {
                message.Dispose();
            }
            copyMessages.Clear();
            copyMessages = null;
        }

        /// <summary>
        /// Load Content and initialize unmanaged resources.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            // rebuild text layouts
            var textFormat = Surface.Fonts.GetTextFormat(MM_Repository.OverallDisplay.NetworkMapFont);
            foreach (var message in _messages)
            {
                if (message.Layout != null)
                message.Layout.Dispose();
                message.Layout = new TextLayout(Surface.FactoryDirectWrite, message.Message, textFormat, 350, 50);
                message.LayoutSize = new Vector2(message.Layout.Metrics.Width, message.Layout.Metrics.Height);
            }

            switch (_alignment)
            {
                case ContentAlignment.TopLeft:
                    _startPosition = new Vector2(10, 10);
                    break;
                case ContentAlignment.TopCenter:
                    _startPosition = new Vector2(Surface.RenderTarget2D.PixelSize.Width / 2.0f, 10);
                    break;
                case ContentAlignment.TopRight:
                    _startPosition = new Vector2(Surface.RenderTarget2D.PixelSize.Width - 10, 10);
                    break;
                case ContentAlignment.MiddleLeft:
                    _startPosition = new Vector2(10, Surface.RenderTarget2D.PixelSize.Height / 2.0f);
                    break;
                case ContentAlignment.MiddleCenter:
                    _startPosition = new Vector2(Surface.RenderTarget2D.PixelSize.Width / 2.0f, Surface.RenderTarget2D.PixelSize.Height / 2.0f);
                    break;
                case ContentAlignment.MiddleRight:
                    _startPosition = new Vector2(Surface.RenderTarget2D.PixelSize.Width - 10, Surface.RenderTarget2D.PixelSize.Height / 2.0f);
                    break;
                case ContentAlignment.BottomLeft:
                    _startPosition = new Vector2(10, Surface.RenderTarget2D.PixelSize.Height - 10);
                    break;
                case ContentAlignment.BottomCenter:
                    _startPosition = new Vector2(Surface.RenderTarget2D.PixelSize.Width / 2.0f, Surface.RenderTarget2D.PixelSize.Height - 10);
                    break;
                case ContentAlignment.BottomRight:
                    _startPosition = new Vector2(Surface.RenderTarget2D.PixelSize.Width - 10, Surface.RenderTarget2D.PixelSize.Height - 10);
                    break;
                default:
                    _startPosition = Vector2.Zero;
                    break;
            }
        }
    }
}