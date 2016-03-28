using System;
using System.Drawing;
using MacomberMapClient.Data_Elements.Geographic;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Proxies
{
    /// <summary>
    /// A graphical representation of a geographical region.
    /// </summary>
    public class RegionProxy : IDisposable
    {
        /// <summary>
        /// Should this be displayed.
        /// </summary>
        public bool Visible;

        public MM_Boundary Boundary;

        /// <summary>
        /// Width of the pen to draw the region.
        /// </summary>
        public float Width { get; private set; }

        private SharpDX.Direct2D1.Geometry _geometry;
        public RawRectangleF Bounds;

        /// <summary>
        /// Creats a new region.
        /// </summary>
        /// <param name="geometry">The geometry of the region.</param>
        /// <param name="color">The border color of the region.</param>
        /// <param name="PenWidth">The width to draw the region.</param>
        public RegionProxy(Geometry geometry, Color color, float PenWidth, RawRectangleF bounds)
        {
            Bounds = bounds;
            Geometry = geometry;
            Width = PenWidth;
            Color = color;
        }


        /// <summary>
        /// The border color of the region.
        /// </summary>
        public Color Color { get; private set; }

        /// <summary>
        /// Gets or sets the border geometry.
        /// </summary>
        public Geometry Geometry
        {
            get { return _geometry; }
            set
            {
                if (_geometry == value) return;

                // swap and dispose
                Geometry oldGeometry = _geometry;
                _geometry = value;
                if (oldGeometry != null && !oldGeometry.IsDisposed) { oldGeometry.Dispose(); }
            }
        }
        #region IDisposable Implementation
        /// <summary>Finalizer</summary>
        ~RegionProxy()
        {
            Dispose(false);
        }

        public RegionProxy()
        {
             IsDisposed= false;
        }

        /// <summary>Has disposed been called on this object.</summary>
        public bool IsDisposed { get; private set; }

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
                if (Geometry != null && !Geometry.IsDisposed)
                    Geometry.Dispose();
                Geometry = null;

                if (freeManaged)
                {
                    // clean up c# managed objects

                }

                IsDisposed = true;
            }
        }

        #endregion
    }
}