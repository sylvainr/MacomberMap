using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Contours
{
    /// <summary>
    /// An indexed palette and linear brush for linear gradients.
    /// </summary>
    public class LinearGradientPalette : IDisposable
    {
        private readonly Direct3DSurface _surface;
        private readonly int _paletteSize;
        private readonly RawVector2 _startPosition;
        private readonly RawVector2 _endPosition;
        private readonly List<GradientStop> _gradientStops;

        /// <summary>
        /// The name of this palette
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The number of indexed colors in the palette.
        /// </summary>
        public int PaletteSize { get { return _paletteSize; } }

        /// <summary>Gradient Brush.</summary>
        public LinearGradientBrush Brush;

        /// <summary>Indexed color palette.</summary>
        public RawColor4[] Palette;

        /// <summary>Create an indexed palette and linear brush for linear gradients.</summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> used for rendering.</param>
        /// <param name="gradientStops">Collection of gradient stops to use in brush and palette.</param>
        /// <param name="paletteSize">Number of indexed colors to create.</param>
        /// <param name="startPosition"><see cref="LinearGradientBrush"/> start vector.</param>
        /// <param name="endPosition"><see cref="LinearGradientBrush"/> end vector.</param>
        public LinearGradientPalette(Direct3DSurface surface, IEnumerable<GradientStop> gradientStops, RawVector2 startPosition, RawVector2 endPosition, int paletteSize = 256)
        {
            IsDisposed = false;
            _surface = surface;
            _paletteSize = paletteSize;
            _startPosition = startPosition;
            _endPosition = endPosition;
            _gradientStops = gradientStops.ToList();
            Update();
        }

        /// <summary>Gets the indexed color.</summary>
        /// <param name="index">index</param>
        public RawColor4 this[int index] { get { return Palette[index]; } }

        /// <summary>
        ///  Regenerate the palette and <see cref="LinearGradientBrush"/>.
        /// </summary>
        public void Update()
        {
            // create the brush
            if (Brush != null)
                Brush.Dispose();
            Brush = GetLinearGradientBrush(_surface.RenderTarget2D, _gradientStops, _startPosition, _endPosition);

            // create the palette
            Palette = BuildPalette(_gradientStops, _paletteSize);
        }

        /// <summary>
        /// Get indexed color along gradient.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Color4</returns>
        public RawColor4 GetIndexedColor(int index)
        {
            if (index < 0 || index > _paletteSize - 1)
                throw new IndexOutOfRangeException("index cannot exceded palette size");

            return Palette[index];
        }

        /// <summary>
        /// Get interpolated color along gradient.
        /// </summary>
        /// <param name="position">Interpolation position along the gradient.</param>
        /// <returns><see cref="Color4"/></returns>
        public RawColor4 GetInterpolateColor(float position)
        {
            return InterpolateGradientStops(_gradientStops, position);
        }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var stop in _gradientStops)
            {
                sb.Append('{');
                sb.Append(stop.Color.HexCodeFromRawColor4());
                sb.Append(',');
                sb.Append(stop.Position);
                sb.Append("},");
            }

            return sb.ToString();
        }

        public static LinearGradientPalette Deserialize(string xml)
        {
            throw new NotImplementedException();
        }



        #region Static Members

        /// <summary>
        /// Create a <see cref="LinearGradientBrush"/> from parameters.
        /// </summary>
        /// <param name="renderTarget">The render target to use.</param>
        /// <param name="gradientStops">Collection of <see cref="GradientStop"/>s.</param>
        /// <param name="startPosition">The <see cref="LinearGradientBrush"/> start vector.</param>
        /// <param name="endPosition">The <see cref="LinearGradientBrush"/> end vector.</param>
        /// <returns><see cref="LinearGradientBrush"/></returns>
        public static LinearGradientBrush GetLinearGradientBrush(RenderTarget renderTarget, IList<GradientStop> gradientStops, RawVector2 startPosition, RawVector2 endPosition)
        {
            var lprops = new LinearGradientBrushProperties
            {
                StartPoint = startPosition,
                EndPoint = endPosition
            };

            var brush = new LinearGradientBrush(renderTarget, lprops, null, new GradientStopCollection(renderTarget, gradientStops.ToArray()));

            return brush;
        }

        /// <summary>
        /// Create an indexed palette from a collection of <see cref="GradientStop"/>s.
        /// </summary>
        /// <param name="gradientStops">Collection of <see cref="GradientStop"/>s.</param>
        /// <param name="paletteSize">The number of indexed colors to create.</param>
        /// <returns><see cref="RawColor4"/></returns>
        public static RawColor4[] BuildPalette(IList<GradientStop> gradientStops, int paletteSize)
        {
            var palette = new RawColor4[paletteSize];

            for (int i = 0; i < paletteSize; i++)
            {
                float interpolationPoint = (float)i / (paletteSize - 1);
                palette[i] = InterpolateGradientStops(gradientStops, interpolationPoint);
            }

            return palette;
        }

        /// <summary>
        /// Get an interpolated color along a collection of <see cref="GradientStop"/>s.
        /// </summary>
        /// <param name="gradientStops">Collection of <see cref="GradientStop"/>s.</param>
        /// <param name="position">Interpolation position along the gradient.</param>
        /// <returns><see cref="RawColor4"/></returns>
        public static RawColor4 InterpolateGradientStops(IList<GradientStop> gradientStops, float position)
        {
            GradientStop? before = null;
            GradientStop? after = null;

            foreach (var gradientStop in gradientStops)
            {
                if (gradientStop.Position < position && (before == null || before.Value.Position < gradientStop.Position))
                {
                    before = gradientStop;
                }

                if (gradientStop.Position >= position && (after == null || after.Value.Position > gradientStop.Position))
                {
                    after = gradientStop;
                }
            }

            // no stops, return black
            if (before == null && after == null) return Color4.Black;

            // only one stop, return that color
            if (after != null && before == null) return after.Value.Color;
            if (before != null && after == null) return before.Value.Color;

            // two stops, interpolate
            return new RawColor4(
               (position - before.Value.Position) * (after.Value.Color.R - before.Value.Color.R) / (after.Value.Position - before.Value.Position) + before.Value.Color.R,
               (position - before.Value.Position) * (after.Value.Color.G - before.Value.Color.G) / (after.Value.Position - before.Value.Position) + before.Value.Color.G,
               (position - before.Value.Position) * (after.Value.Color.B - before.Value.Color.B) / (after.Value.Position - before.Value.Position) + before.Value.Color.B,
               (position - before.Value.Position) * (after.Value.Color.A - before.Value.Color.A) / (after.Value.Position - before.Value.Position) + before.Value.Color.A
            );
        }

        #endregion Static Members

        #region IDisposable Implementation

        /// <summary>Finalizer</summary>
        ~LinearGradientPalette()
        {
            Dispose(false);
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
            GC.SuppressFinalize(this);
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
                    if (Brush != null)
                        Brush.Dispose();
                    Brush = null;
                    Palette = null;
                }

                IsDisposed = true;
            }
        }

        #endregion IDisposable Implementation
    }
}