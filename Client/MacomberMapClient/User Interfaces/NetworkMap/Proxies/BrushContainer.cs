using System;
using System.Collections.Generic;
using System.Linq;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Proxies
{
    public class TextFormatContainer
    {
        private readonly Dictionary<string, TextFormat> _textFormats = new Dictionary<string, TextFormat>();
        private Direct3DSurface _surface;

        public TextFormatContainer(Direct3DSurface surface)
        {
            _surface = surface;
        }


        public void Cleanup()
        {
            var items = _textFormats.Values.ToList();
            _textFormats.Clear();
            foreach (var item in items)
            {
                item.Dispose();
            }
        }

        /// <summary>
        /// Gets or creates a TextFormat based on a <see cref="System.Drawing.Font"/>.
        /// </summary>
        /// <param name="font">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public TextFormat GetTextFormat(Font font, int shiftFontSize = 0)
        {
            float fontSize = font.Size + shiftFontSize;
            var key = string.Join("_", Math.Round(fontSize, 2).ToString("0.00"), font.FontFamily.Name, font.Style, font.Bold);

            SharpDX.DirectWrite.FontStyle fs = SharpDX.DirectWrite.FontStyle.Normal;
            if (font.Style.HasFlag(System.Drawing.FontStyle.Italic))
            {
                fs = SharpDX.DirectWrite.FontStyle.Italic;
            }

            TextFormat textFormat;
            if (!_textFormats.TryGetValue(key, out textFormat))
            {

                textFormat = new TextFormat(_surface.FactoryDirectWrite, font.FontFamily.Name, font.Bold ? FontWeight.Bold : FontWeight.Normal, fs, _surface.Factory2D.DesktopDpi.Height / 96 * fontSize);
                _textFormats.Add(key, textFormat);
            }

            return textFormat;
        }
    }
    public class BrushContainer
    {
        private readonly Dictionary<Color4, SolidColorBrush> _brushes = new Dictionary<Color4, SolidColorBrush>();
        private Direct3DSurface _surface;

        public BrushContainer(Direct3DSurface surface)
        {
            _surface = surface;
        }

        public void Cleanup()
        {
            var items = _brushes.Values.ToList();
            _brushes.Clear();
            foreach (var item in items)
            {
                item.Dispose();
            }
        }

        public SolidColorBrush GetBrush(System.Drawing.Color color)
        {
            return GetBrush(new Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f));
        }

        public SolidColorBrush GetBrush(System.Drawing.Color color, float alpha)
        {
            return GetBrush(new Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, alpha));
        }

        public SolidColorBrush GetBrush(Color4 color, float alpha)
        {
            return GetBrush(new Color4(color.Red, color.Green, color.Blue, alpha));
        }
        public SolidColorBrush GetBrush(Color4 color)
        {
            SolidColorBrush brush;
            if (!_brushes.TryGetValue(color, out brush))
            {
                brush = new SolidColorBrush(_surface.RenderTarget2D, color);
                _brushes.Add(color, brush);
            }

            return brush;
        }
    }
}