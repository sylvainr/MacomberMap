using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MacomberMapClient.Data_Elements.Geographic;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Brush = SharpDX.Direct2D1.Brush;
using Buffer = SharpDX.Direct3D11.Buffer;
using DashStyle = System.Drawing.Drawing2D.DashStyle;
using Factory = SharpDX.Direct2D1.Factory;
using LineJoin = System.Drawing.Drawing2D.LineJoin;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace MacomberMapClient.User_Interfaces.NetworkMap.DX
{
    /// <summary>
    /// A collection of helper methods for SharpDX.
    /// </summary>
    public static class SharpDxExtensions
    {
        #region Pen to StrokeStyle conversion
        static Dictionary<DashCap, SharpDX.Direct2D1.CapStyle> _dashCapMap = new Dictionary<DashCap, SharpDX.Direct2D1.CapStyle>
        {
            { DashCap.Flat, SharpDX.Direct2D1.CapStyle.Flat },
            { DashCap.Round, SharpDX.Direct2D1.CapStyle.Round },
            { DashCap.Triangle, SharpDX.Direct2D1.CapStyle.Triangle },
        };

        static Dictionary<LineCap, SharpDX.Direct2D1.CapStyle> _lineCapMap = new Dictionary<LineCap, SharpDX.Direct2D1.CapStyle>
        {
            { LineCap.Flat, SharpDX.Direct2D1.CapStyle.Flat },
            { LineCap.Round, SharpDX.Direct2D1.CapStyle.Round },
            { LineCap.Triangle, SharpDX.Direct2D1.CapStyle.Triangle },
            { LineCap.Square, SharpDX.Direct2D1.CapStyle.Square },
        };

        static Dictionary<DashStyle, SharpDX.Direct2D1.DashStyle> _dashStyleMap = new Dictionary<DashStyle, SharpDX.Direct2D1.DashStyle>
        {
            { DashStyle.Custom, SharpDX.Direct2D1.DashStyle.Custom },
            { DashStyle.Dash, SharpDX.Direct2D1.DashStyle.Dash },
            { DashStyle.DashDot, SharpDX.Direct2D1.DashStyle.DashDot },
            { DashStyle.DashDotDot, SharpDX.Direct2D1.DashStyle.DashDotDot },
            { DashStyle.Dot, SharpDX.Direct2D1.DashStyle.Dot },
            { DashStyle.Solid, SharpDX.Direct2D1.DashStyle.Solid },
        };

        static Dictionary<LineJoin, SharpDX.Direct2D1.LineJoin> _lineJoinMap = new Dictionary<LineJoin, SharpDX.Direct2D1.LineJoin>
        {
            { LineJoin.Bevel, SharpDX.Direct2D1.LineJoin.Bevel },
            { LineJoin.Miter, SharpDX.Direct2D1.LineJoin.Miter },
            { LineJoin.MiterClipped, SharpDX.Direct2D1.LineJoin.MiterOrBevel },
            { LineJoin.Round, SharpDX.Direct2D1.LineJoin.Round },
        };

        public static SharpDX.Direct2D1.StrokeStyle ToStrokeStyle(this Pen pen, SharpDX.Direct2D1.Factory factory)
        {
            SharpDX.Direct2D1.StrokeStyleProperties properties = new SharpDX.Direct2D1.StrokeStyleProperties
            {
                DashCap = _dashCapMap[pen.DashCap],
                DashOffset = pen.DashOffset,
                DashStyle = _dashStyleMap[pen.DashStyle],
                EndCap = _lineCapMap[pen.EndCap],
                LineJoin = _lineJoinMap[pen.LineJoin],
                MiterLimit = pen.MiterLimit,
                StartCap = _lineCapMap[pen.StartCap],
            };

            if (pen.DashStyle == DashStyle.Custom)
                return new SharpDX.Direct2D1.StrokeStyle(factory, properties, pen.DashPattern);
            return new SharpDX.Direct2D1.StrokeStyle(factory, properties);
        }
        #endregion

        public static RawColor4 Color4FromHexCode(string hex)
        {
            hex = hex.Trim('#');
            if (hex.Length == 3)
            {
                return new RawColor4(byte.Parse(hex.Substring(0, 1), NumberStyles.HexNumber) / (float)0x0F,
                                     byte.Parse(hex.Substring(1, 1), NumberStyles.HexNumber) / (float)0x0F,
                                     byte.Parse(hex.Substring(2, 1), NumberStyles.HexNumber) / (float)0x0F,
                                     0f);
            }
            else if (hex.Length == 6)
            {
                return new RawColor4(byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) / (float)0xFF,
                                     byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) / (float)0xFF,
                                     byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) / (float)0xFF,
                                     0f);
            }
            else if (hex.Length == 8)
            {
                return new RawColor4(byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) / (float)0xFF,
                                     byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) / (float)0xFF,
                                     byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) / (float)0xFF,
                                     byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber) / (float)0xFF);
            }

            return new Color4();
        }

        public static string[] HexTbl = Enumerable.Range(0, 256).Select(v => v.ToString("X2")).ToArray();
        public static string HexCodeFromRawColor4(this RawColor4 color)
        {
            return string.Format("#{0}{1}{2}{3}",
                                 HexTbl[(int)MathUtil.Clamp((0xff * color.R), 0, 254)],
                                 HexTbl[(int)MathUtil.Clamp((0xff * color.G), 0, 254)],
                                 HexTbl[(int)MathUtil.Clamp((0xff * color.B), 0, 254)],
                                 HexTbl[(int)MathUtil.Clamp((0xff * color.A), 0, 254)]);
        }

        public static string ToHex(this IEnumerable<byte> array)
        {
            StringBuilder s = new StringBuilder();
            foreach (var v in array)
                s.Append(HexTbl[v]);
            return s.ToString();
        }
        public static string ToHex(this byte[] array)
        {
            StringBuilder s = new StringBuilder(array.Length * 2);
            foreach (var v in array)
                s.Append(HexTbl[v]);
            return s.ToString();
        }

        /// <summary>
        /// Creates a <see cref="PathGeometry"/> from a collection of <see cref="RawVector2"/>s.
        /// </summary>
        /// <param name="factory2D">The Direct2D1 Factory used.</param>
        /// <param name="filled">Should the figure be filled.</param>
        /// <param name="closed">Should the figure be closed.</param>
        /// <param name="Points">The points of the geometry.</param>
        /// <returns></returns>
        public static PathGeometry CreatePathGeometry(this Factory factory2D, bool filled, bool closed, params RawVector2[] Points)
        {
            PathGeometry geometry = new PathGeometry(factory2D);

            if (Points.Length > 0)
            {
                using (var sink = geometry.Open())
                {
                    sink.BeginFigure(Points[0], filled ? FigureBegin.Filled : FigureBegin.Hollow);
                    for (int i = 1; i < Points.Length; i++)
                    {
                        sink.AddLine(Points[i]);
                    }
                    sink.EndFigure(closed ? FigureEnd.Closed : FigureEnd.Open);
                    sink.Close();
                }
            }

            return geometry;
        }

        /// <summary>
        /// Generate a PIE chart slice.
        /// </summary>
        /// <param name="factory2D"></param>
        /// <param name="percent"></param>
        /// <param name="radius"></param>
        /// <param name="center"></param>
        /// <param name="rotationAngle"></param>
        /// <returns></returns>
        public static PathGeometry CreatePieSlice(this Factory factory2D, float percent, float radius, float rotationAngle = 0)
        {
            PathGeometry g = new PathGeometry(factory2D);
            var startArc = new Vector2() { X = radius, Y = 0 };
            var endArc = new Vector2()
            {
                X = radius * (float)Math.Cos(percent * MathUtil.TwoPi),
                Y = radius * (float)Math.Sin(percent * MathUtil.TwoPi)
            };
            using (var sink = g.Open())
            {
                sink.SetFillMode(SharpDX.Direct2D1.FillMode.Winding);
                sink.BeginFigure(Vector2.Zero, FigureBegin.Filled);

                sink.AddLine(startArc);

                sink.AddArc(new ArcSegment()
                {
                    ArcSize = percent <= 0.5 ? ArcSize.Small : ArcSize.Large,
                    RotationAngle = rotationAngle,
                    Point = endArc,
                    Size = new Size2F(radius, radius),
                    SweepDirection = SweepDirection.Clockwise
                });
                sink.AddLine(Vector2.Zero);
                sink.EndFigure(FigureEnd.Closed);
                sink.Close();
            }

            return g;
        }

        /// <summary>
        /// Draws the outline of a <see cref="GraphicsPath"/> using Direct2D.
        /// </summary>
        /// <param name="graph">The RenderTarget used.</param>
        /// <param name="path">The GraphicsPath used.</param>
        /// <param name="brush">The brush used.</param>
        /// <param name="width">The stroke width.</param>
        [Obsolete("Create a DirectX Geometry instead.")]
        public static void DrawGraphicsPath(this RenderTarget graph, GraphicsPath path, SharpDX.Direct2D1.Brush brush, float width = 1, StrokeStyle style = null)
        {
            for (int i = 0; i < path.PointCount - 1; i++)
            {
                if (style != null)
                    graph.DrawLine(new Vector2(path.PathPoints[i].X, path.PathPoints[i].Y), new Vector2(path.PathPoints[i + 1].X, path.PathPoints[i + 1].Y), brush, width, style);
                else
                    graph.DrawLine(new Vector2(path.PathPoints[i].X, path.PathPoints[i].Y), new Vector2(path.PathPoints[i + 1].X, path.PathPoints[i + 1].Y), brush, width);
            }
        }
        /// <summary>
        /// Draws a <see cref="TextLayout"/> object to a render target at X/Y Coordinates.
        /// </summary>
        /// <param name="renderTarget">The render target.</param>
        /// <param name="textLayout">The text to render.</param>
        /// <param name="brush">The foreground brush.</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y Position</param>
        /// <param name="brushBackground">The background brush, null for no background.</param>
        /// <param name="centerX">Center the text on the X axis</param>
        /// <param name="centerY">Center the text on the y axis</param>
        public static void DrawTextAtPoint(this SharpDX.Direct2D1.RenderTarget renderTarget, TextLayout textLayout, Brush brush, float x, float y, Brush brushBackground = null, bool centerX = false, bool centerY = false, DrawTextOptions options = DrawTextOptions.None)
        {
            float width = textLayout.Metrics.Width;
            float height = textLayout.Metrics.Height;

            if (centerX) x -= width / 2;
            if (centerY) y -= height / 2;

            if (brushBackground != null)
            {
                renderTarget.FillRectangle(new RawRectangleF(x, y, x + width, y + height), brushBackground);
            }

            renderTarget.DrawTextLayout(new RawVector2 { X = x, Y = y }, textLayout, brush, options);
        }

        /// <summary>
        /// Test two <see cref="SharpDX.RectangleF"/> for overlap.
        /// </summary>
        /// <param name="rectangle">The source rectagle.</param>
        /// <param name="test">The test rectangle.</param>
        /// <returns></returns>
        public static bool Overlaps(this RawRectangleF rectangle, RawRectangleF test)
        {
            if (test.Right < rectangle.Left) return false;
            if (test.Left > rectangle.Right) return false;
            if (test.Top > rectangle.Bottom) return false;
            if (test.Bottom < rectangle.Top) return false;

            return true;
        }

        /// <summary>
        /// Test two <see cref="SharpDX.RectangleF"/> for overlap.
        /// </summary>
        /// <param name="rectangle">The source rectagle.</param>
        /// <param name="point">The test rectangle.</param>
        /// <returns></returns>
        public static bool Contains(this RawRectangleF rectangle, RawVector2 point)
        {
            if (point.X < rectangle.Left || point.X > rectangle.Right) return false;
            if (point.Y > rectangle.Bottom || point.Y < rectangle.Top) return false;

            return true;
        }

        /// <summary>
        /// Convert a <see cref="System.Drawing.Rectangle"/> to a <see cref="SharpDX.RectangleF"/>.
        /// </summary>
        /// <param name="rectangle">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static SharpDX.RectangleF ToRectangleF(this System.Drawing.Rectangle rectangle)
        {
            return new SharpDX.RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        /// <summary>
        /// Convert a <see cref="System.Drawing.PointF"/> to a <see cref="RawVector2"/>.
        /// </summary>
        /// <param name="points">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static RawVector2 ToRawVector2(this System.Drawing.PointF points)
        {
            return new RawVector2() { X = points.X, Y = points.Y };
        }

        /// <summary>
        /// Convert a <see cref="System.Drawing.Point"/> to a <see cref="RawVector2"/>.
        /// </summary>
        /// <param name="points">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static RawVector2 ToRawVector2(this System.Drawing.Point points)
        {
            return new RawVector2() { X = points.X, Y = points.Y };
        }

        /// <summary>
        /// Convert a <see cref="System.Drawing.Point"/> to a <see cref="RawColor4"/>.
        /// </summary>
        /// <param name="color">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static RawColor4 ToDxColor4(this System.Drawing.Color color)
        {
            return new RawColor4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }


        /// <summary>
        /// Convert a <see cref="GraphicsPath"/> to a <see cref="Geometry"/>.
        /// </summary>
        /// <param name="path">The value to convert.</param>
        /// <param name="factory">The Direct2D1 Factory.</param>
        /// <param name="closed">Is the figure closed or open.</param>
        /// <returns>The converted value.</returns>
        public static Geometry ToDxGeometry(this GraphicsPath path, Factory factory, bool closed = true)
        {
            PathGeometry g = new PathGeometry(factory);

            if (path.PointCount > 0)
            {
                using (var sink = g.Open())
                {

                    sink.BeginFigure(new Vector2(path.PathPoints[0].X, path.PathPoints[0].Y), FigureBegin.Hollow);
                    for (int i = 1; i < path.PointCount; i++)
                    {
                        sink.AddLine(new Vector2(path.PathPoints[i].X, path.PathPoints[i].Y));
                    }

                    sink.EndFigure(closed ? FigureEnd.Closed : FigureEnd.Open);
                    sink.Close();
                }
            }

            return g;
        }

        /// <summary>
        /// Loads a Direct2D Bitmap from a file using <see cref="System.Drawing.Image.FromFile(string)"/>
        /// </summary>
        /// <param name="renderTarget">The render target.</param>
        /// <param name="file">The file.</param>
        /// <returns>A D2D1 Bitmap</returns>
        public static SharpDX.Direct2D1.Bitmap LoadBitmapFromFile(this RenderTarget renderTarget, string file)
        {
            try
            {
                // Loads from file using System.Drawing.Image
                using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(file))
                {
                    var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    var bitmapProperties = new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
                    var size = new SharpDX.Size2(bitmap.Width, bitmap.Height);

                    // Transform pixels from BGRA to RGBA
                    int stride = bitmap.Width * sizeof(int);
                    using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
                    {
                        // Lock System.Drawing.Bitmap
                        var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                        // Convert all pixels 
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            int offset = bitmapData.Stride * y;
                            for (int x = 0; x < bitmap.Width; x++)
                            {
                                // Not optimized 
                                byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                                byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                                byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                                byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                                int rgba = R | (G << 8) | (B << 16) | (A << 24);
                                tempStream.Write(rgba);
                            }

                        }
                        bitmap.UnlockBits(bitmapData);
                        tempStream.Position = 0;

                        return new SharpDX.Direct2D1.Bitmap(renderTarget, size, tempStream, stride, bitmapProperties);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DX Bitmap Conversion Error: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Convert a <see cref="System.Drawing.Bitmap"/> to a <see cref="SharpDX.Direct2D1.Bitmap"/>.
        /// </summary>
        /// <param name="bitmap">The value to convert.</param>
        /// <param name="renderTarget">The Direct2D1 RenderTarget.</param>
        /// <returns>The converted value.</returns>
        public static SharpDX.Direct2D1.Bitmap ToDxBitmap(this System.Drawing.Bitmap bitmap, RenderTarget renderTarget)
        {
            var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapProperties = new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
            var size = new SharpDX.Size2(bitmap.Width, bitmap.Height);

            // Transform pixels from BGRA to RGBA
            int stride = bitmap.Width * sizeof(int);
            using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
            {
                // Lock System.Drawing.Bitmap
                var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                // Convert all pixels 
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int offset = bitmapData.Stride * y;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        // Not optimized 
                        byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        int rgba = R | (G << 8) | (B << 16) | (A << 24);
                        tempStream.Write(rgba);
                    }

                }
                bitmap.UnlockBits(bitmapData);
                tempStream.Position = 0;

                return new SharpDX.Direct2D1.Bitmap(renderTarget, size, tempStream, stride, bitmapProperties);
            }
        }



        /// <summary>
        /// Copies the content an array of data on CPU memory to this buffer into GPU memory.
        /// </summary>
        /// <typeparam name="TData">The type of the T data.</typeparam>
        /// <param name="buffer">The buffer to update.</param>
        /// <param name="device">The <see cref="SharpDX.Direct3D11.Device"/>.</param>
        /// <param name="data">The data to copy from.</param>
        /// <param name="startIndex">The starting index to begin setting data from.</param>
        /// <param name="elementCount">The number of elements to set.</param>
        /// <param name="offsetInBytes">The offset in bytes to write to.</param>
        /// <param name="mode">Buffer data behavior.</param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <remarks>
        /// See the unmanaged documentation about Map/UnMap for usage and restrictions.
        /// </remarks>
        /// <msdn-id>ff476457</msdn-id>
        /// <unmanaged>HRESULT ID3D11DeviceContext::Map([In] ID3D11Resource* pResource,[In] unsigned int Subresource,[In] D3D11_MAP MapType,[In] D3D11_MAP_FLAG MapFlags,[Out] D3D11_MAPPED_SUBRESOURCE* pMappedResource)</unmanaged>
        /// <unmanaged-short>ID3D11DeviceContext::Map</unmanaged-short>
        public static unsafe void SetData<TData>(this Buffer buffer, SharpDX.Direct3D11.Device device, TData[] data, int startIndex = 0, int elementCount = 0, int offsetInBytes = 0, MapMode mode = MapMode.WriteDiscard) where TData : struct
        {
            // var sizeOfT = Utilities.SizeOf<TData>();

            Utilities.Pin(data, sourcePtr =>
            {
                var sizeOfData = Utilities.SizeOf(data);
                SetData(buffer, device, new DataPointer(sourcePtr, sizeOfData), offsetInBytes, mode);
            });
        }


        /// <summary>
        /// Copies the content an array of data on CPU memory to this buffer into GPU memory.
        /// </summary>
        /// <param name="buffer">The buffer to update.</param>
        /// <param name="device">The <see cref="SharpDX.Direct3D11.Device"/>.</param>
        /// <param name="fromData">A data pointer.</param>
        /// <param name="offsetInBytes">The offset in bytes to write to.</param>
        /// <param name="mode">Buffer data behavior.</param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <msdn-id>ff476457</msdn-id>
        ///   <unmanaged>HRESULT ID3D11DeviceContext::Map([In] ID3D11Resource* pResource,[In] unsigned int Subresource,[In] D3D11_MAP MapType,[In] D3D11_MAP_FLAG MapFlags,[Out] D3D11_MAPPED_SUBRESOURCE* pMappedResource)</unmanaged>
        ///   <unmanaged-short>ID3D11DeviceContext::Map</unmanaged-short>
        /// <remarks>
        /// See the unmanaged documentation about Map/UnMap for usage and restrictions.
        /// </remarks>
        public static unsafe void SetData(this Buffer buffer, SharpDX.Direct3D11.Device device, DataPointer fromData, int offsetInBytes = 0, MapMode mode = MapMode.WriteDiscard)
        {
            // Check size validity of data to copy to
            if ((fromData.Size + offsetInBytes) > buffer.Description.SizeInBytes)
                throw new ArgumentException("Size of data to upload + offset is larger than size of buffer");

            var deviceContext = (SharpDX.Direct3D11.DeviceContext)device.ImmediateContext;

            // If this bufefer is declared as default usage, we can only use UpdateSubresource, which is not optimal but better than nothing
            if (buffer.Description.Usage == ResourceUsage.Default)
            {
                // Setup the dest region inside the buffer
                if ((buffer.Description.BindFlags & BindFlags.ConstantBuffer) != 0)
                {
                    deviceContext.UpdateSubresource(new DataBox(fromData.Pointer, 0, 0), buffer);
                }
                else
                {
                    var destRegion = new ResourceRegion(offsetInBytes, 0, 0, offsetInBytes + fromData.Size, 1, 1);
                    deviceContext.UpdateSubresource(new DataBox(fromData.Pointer, 0, 0), buffer, 0, destRegion);
                }
            }
            else
            {
                try
                {
                    var box = deviceContext.MapSubresource(buffer, 0, mode, SharpDX.Direct3D11.MapFlags.None);
                    Utilities.CopyMemory((IntPtr)((byte*)box.DataPointer + offsetInBytes), fromData.Pointer, fromData.Size);
                }
                finally
                {
                    deviceContext.UnmapSubresource(buffer, 0);
                }
            }
        }

    }
}