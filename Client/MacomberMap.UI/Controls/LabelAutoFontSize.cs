using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace MacomberMap.UI.Controls
{
    [ToolboxItem(true), ToolboxBitmap(typeof(LabelAutoFontSize), "MacomberMap.UI.Icons.LabelIco.png")]
    public class LabelAutoFontSize : System.Windows.Forms.Label
    {
        private Padding _BorderWidth = new Padding(0,0,0,0);
        /// <summary>
        /// Border Width.
        /// </summary>
        [Category("Appearance"),
         Browsable(true),
         Description("Border Width."),
        DefaultValue("0,0,0,0")]
        public Padding BorderWidth
        {
            get { return _BorderWidth; }
            set { _BorderWidth = value; this.Invalidate(); }
        }

        private Color _BorderColor = Color.Black;
        /// <summary>
        /// Border Color.
        /// </summary>
        [Category("Appearance"),
         Browsable(true),
         Description("Border Color"), 
        DefaultValue("Black")]
        public Color BorderColor
        {
            get { return _BorderColor; }
            set { _BorderColor = value; this.Invalidate(); }
        }

        private float _MinFontSize = 5;
        /// <summary>
        /// Minimum Font Size
        /// </summary>
        [Category("Appearance"),
         Browsable(true),
         Description("Minimum Font Size (1-72)"),
        DefaultValue(5F)]
        public float MinFontSize
        {
            get { return _MinFontSize; }
            set { _MinFontSize = Math.Max(1, Math.Min(72, value)); this.ResizeText(); }
        }

        private float _MaxFontSize = 72;
        /// <summary>
        /// Maxumum Font Size
        /// </summary>
        [Category("Appearance"),
         Browsable(true),
         Description("Maxumum Font Size (1-72)"),
        DefaultValue(72F)]
        public float MaxFontSize
        {
            get { return _MaxFontSize; }
            set { _MaxFontSize = Math.Max(1, Math.Min(72, value)); this.ResizeText(); }
        }

        private bool _AutoSizeFont = true;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Layout"),
         Browsable(true),
         Description("Default Description. Disabled if AutoSize is true."),
         DefaultValue("true")]
        public bool AutoSizeFont
        {
            get { return _AutoSizeFont; }
            set { _AutoSizeFont = value; this.ResizeText(); }
        }

        /// <summary>
        /// Gets or sets the text displayed by the label
        /// </summary>
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                this.ResizeText();
            }
        }

        private void ResizeText()
		{
            if (this._AutoSizeFont && !this.AutoSize && !(this.Disposing || this.IsDisposed))
            {
                SizeF extent = new SizeF(0,0);

                Size displayArea = GetDisplaySize();
                //displayArea = this.ClientRectangle.Size;
                this.Font = AppropriateFont(this.CreateGraphics(), this._MinFontSize, this._MaxFontSize, displayArea, this.Text, this.Font, out extent);
            }
		}

        private Size GetDisplaySize()
        {
            int horizontalArea = this.Size.Width - Math.Max(this.Margin.Left + this.Margin.Right, this.BorderWidth.Left + this.BorderWidth.Right);
            int verticalArea = this.Size.Height - Math.Max(this.Margin.Top + this.Margin.Bottom, this.BorderWidth.Top + this.BorderWidth.Bottom);

            Size displayArea = new Size(horizontalArea, verticalArea);
            return displayArea;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            e.Graphics.DrawString(
                this.Text, 
                this.Font,
                new SolidBrush(this.ForeColor),
                new RectangleF(
                    new PointF(
                        Math.Max(this.Margin.Left, this.BorderWidth.Left), 
                        Math.Max(this.Margin.Top, this.BorderWidth.Top)), 
                    GetDisplaySize()), 
                GetStringFormatFromContentAllignment(this.TextAlign));  
          
            LabelBorder_Paint(this, e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeText();
        }

        private  static Font AppropriateFont(Graphics g, float minFontSize,
            float maxFontSize, Size layoutSize, string s, Font f, out SizeF extent)
        {
            if (maxFontSize == minFontSize)
                f = new Font(f.FontFamily, minFontSize, f.Style);

            extent = g.MeasureString(s, f);

            if (maxFontSize <= minFontSize)
                return f;

            float hRatio = layoutSize.Height / extent.Height;
            float wRatio = layoutSize.Width / extent.Width;
            float ratio = (hRatio < wRatio) ? hRatio : wRatio;

            float newSize = f.Size * ratio;

            if (newSize < minFontSize)
                newSize = minFontSize;
            else if (newSize > maxFontSize)
                newSize = maxFontSize;

            f = new Font(f.FontFamily, newSize, f.Style);
            extent = g.MeasureString(s, f);

            return f;
        }

        private StringFormat GetStringFormatFromContentAllignment(ContentAlignment ca)
        {
            StringFormat format = new StringFormat();
            switch (ca)
            {
                case ContentAlignment.TopLeft:
                    format.LineAlignment = StringAlignment.Near;
                    format.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopCenter:
                    format.LineAlignment = StringAlignment.Near;
                    format.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.TopRight:
                    format.LineAlignment = StringAlignment.Near;
                    format.Alignment = StringAlignment.Far;
                    break;
                case ContentAlignment.MiddleLeft:
                    format.LineAlignment = StringAlignment.Center;
                    format.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.MiddleCenter:
                    format.LineAlignment = StringAlignment.Center;
                    format.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleRight:
                    format.LineAlignment = StringAlignment.Center;
                    format.Alignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomLeft:
                    format.LineAlignment = StringAlignment.Far;
                    format.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.BottomCenter:
                    format.LineAlignment = StringAlignment.Far;
                    format.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomRight:
                    format.LineAlignment = StringAlignment.Far;
                    format.Alignment = StringAlignment.Far;
                    break;
            }
            return format;
        }

        private void LabelBorder_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            //e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Graphics g = e.Graphics;

            if (this._BorderWidth.Top > 0)
                g.DrawLine(new Pen(this._BorderColor, this._BorderWidth.Top), 0, (this._BorderWidth.Top / 2), Width, (this._BorderWidth.Top / 2));

            if (this._BorderWidth.Bottom > 0)
                g.DrawLine(new Pen(this._BorderColor, this._BorderWidth.Bottom), 0, Height - (int)Math.Ceiling((double)this._BorderWidth.Bottom / 2), Width, Height - (int)Math.Ceiling((double)this._BorderWidth.Bottom / 2.0));

            if (this._BorderWidth.Left > 0)
                g.DrawLine(new Pen(this._BorderColor, this._BorderWidth.Left), (this._BorderWidth.Left / 2), 0, (this._BorderWidth.Left / 2), Height);

            if (this._BorderWidth.Right > 0)
                g.DrawLine(new Pen(this._BorderColor, this._BorderWidth.Right), Width - (int)Math.Ceiling((double)this._BorderWidth.Right / 2), 0, Width - (int)Math.Ceiling((double)this._BorderWidth.Right / 2), Height);
        }
    }

}
