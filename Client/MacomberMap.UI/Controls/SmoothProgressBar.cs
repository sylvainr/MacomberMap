using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MacomberMap.UI.Controls
{
    [ToolboxItem(true), ToolboxBitmap(typeof(SmoothProgressBar), "MacomberMap.UI.Icons.SmoothProgressBarIco.png")]
    public class SmoothProgressBar : UserControl
    {
        int min = 0;	// Minimum value for progress range
        int max = 100;	// Maximum value for progress range
        int val = 0;		// Current progress
        Color BarColor = Color.CornflowerBlue;		// Color of progress meter
        Rectangle _marqueePrevRect = new Rectangle(0,0,0,0);
        int _marqueeLoc = 0;
        bool _isMarqueeAscending = true;

        System.Timers.Timer marqueeTimer = new System.Timers.Timer();

        public SmoothProgressBar()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);

            this.Paint += new PaintEventHandler(SmoothProgressBar_Paint);

            marqueeTimer.Elapsed +=new System.Timers.ElapsedEventHandler(marqueeTimer_Elapsed);
            marqueeTimer.AutoReset = true;
            marqueeTimer.Interval = 15;


            if (this._BarStyle == ProgressBarStyle.Marquee)
                marqueeTimer.Start();
            //this.Resize += new EventHandler(SmoothProgressBar_Resize);
        }

        private void marqueeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this._isMarqueeAscending)
                this._marqueeLoc = this._marqueeLoc + 1;
            else
                this._marqueeLoc = this._marqueeLoc - 1;

            if (this._marqueeLoc >= 100 || this._marqueeLoc <= 0)
                this._isMarqueeAscending = !this._isMarqueeAscending;
            this.Invalidate();
        }


        #region Designer

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SmoothProgressBar
            // 
            this.Name = "SmoothProgressBar";
            this.Size = new System.Drawing.Size(150, 21);
            this.ResumeLayout(false);

        }

        #endregion
        #endregion

        //private void SmoothProgressBar_Resize(object sender, EventArgs e)
        //{
        //    this.Invalidate();
        //}

        private void SmoothProgressBar_Paint(object sender, PaintEventArgs e)
        {
            // Force High Quality Graphics


            Bitmap bmp = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            Graphics bmpG = Graphics.FromImage(bmp);
            bmpG.CompositingQuality = CompositingQuality.HighQuality;
            bmpG.InterpolationMode = InterpolationMode.HighQualityBicubic;
            bmpG.PixelOffsetMode = PixelOffsetMode.HighQuality;
            bmpG.SmoothingMode = SmoothingMode.HighSpeed;
            bmpG.FillRectangle(new SolidBrush(this.BackColor), 0, 0, bmp.Width, bmp.Height);
            
            DrawBar(bmpG);
            
            e.Graphics.DrawImage(bmp, 0, 0, this.ClientRectangle, GraphicsUnit.Pixel);
            bmpG.Dispose();
            bmp.Dispose();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        private void DrawBar(Graphics g)
        {
            SolidBrush brush = new SolidBrush(BarColor);
            float percent = (float)(val - min) / (float)(max - min);
            Rectangle rect = this.ClientRectangle;

            // Calculate area for drawing the progress.
            if (this._BarStyle == ProgressBarStyle.Marquee)
            {
                rect = MarqueeRect();
            }
            else
            {
                rect.Width = (int)((float)rect.Width * percent);
            }

            if (rect.Width > 0)
            {
                // Draw the progress meter.
                g.FillRectangle(brush, rect);
                brush.Dispose();
                // Overlay Smooth Gradient
                if (this._OverlayMode == OverLayMode.Smooth)
                {
                    HSLColor lowlight = new HSLColor(BarColor);
                    lowlight.Luminosity = 0.4;

                    LinearGradientBrush lgb = new LinearGradientBrush((RectangleF)rect, Color.Transparent, Color.FromArgb((int)(255 * OverlayOpacity), lowlight), 90);
                    g.FillRectangle(lgb, rect);
                }
                else if (this._OverlayMode == OverLayMode.Gel)
                {
                    HSLColor lowlight = new HSLColor(BarColor);
                    lowlight.Luminosity = 0.2;

                    HSLColor highlight = new HSLColor(BarColor);
                    highlight.Saturation = ((1 - highlight.Saturation) / 2) + highlight.Saturation;

                    Rectangle topHalf = rect;
                    Rectangle bottomHalf = rect;
                    topHalf.Height = (int)(topHalf.Height * 0.45);
                    bottomHalf.Height = this.ClientRectangle.Height - topHalf.Height;
                    bottomHalf.Y = topHalf.Height;

                    LinearGradientBrush lgbTop = new LinearGradientBrush((RectangleF)topHalf, Color.FromArgb((int)(192 * OverlayOpacity), lowlight), Color.FromArgb((int)(64 * OverlayOpacity), lowlight), 90);
                    LinearGradientBrush lgbBottom = new LinearGradientBrush((RectangleF)bottomHalf, Color.Transparent, Color.FromArgb((int)(255 * OverlayOpacity), lowlight), 90);
                    g.FillRectangle(lgbTop, topHalf);
                    g.FillRectangle(lgbBottom, bottomHalf);
                    lgbTop.Dispose();
                    lgbBottom.Dispose();
                }
                else if (this._OverlayMode == OverLayMode.Blade)
                {
                    Rectangle topHalf = rect;
                    Rectangle bottomHalf = rect;
                    topHalf.Height = (int)(topHalf.Height * 0.5);
                    bottomHalf.Height = this.ClientRectangle.Height - topHalf.Height;
                    bottomHalf.Y = topHalf.Height;

                    Color overlayStartColor = Color.FromArgb((int)(50 * OverlayOpacity), Color.Gray);
                    Color overlayEndColor = Color.FromArgb((int)(80 * OverlayOpacity), Color.White);

                    LinearGradientBrush lgb = new LinearGradientBrush((RectangleF)topHalf, overlayStartColor, overlayEndColor, 90);
                    g.FillRectangle(lgb, topHalf);


                    overlayStartColor = Color.FromArgb((int)(25 * OverlayOpacity), Color.White);
                    overlayEndColor = Color.FromArgb((int)(100 * OverlayOpacity), Color.Black);

                    lgb = new LinearGradientBrush((RectangleF)bottomHalf, overlayStartColor, overlayEndColor, 90);
                    g.FillRectangle(lgb, bottomHalf);

                    lgb.Dispose();
                }
            }
        }

        private Rectangle MarqueeRect()
        {
            Rectangle rect = this.ClientRectangle;
            int marqueeWidth = (int)((float)this.ClientRectangle.Width * 0.25);

            rect.Width = marqueeWidth;
            rect.X = (int)(((float)_marqueeLoc / 100.0) * (this.ClientRectangle.Width - marqueeWidth));

            return rect;
        }

        // private BorderStyle _CustomBorderStyle = BorderStyle.Fixed3D;
        // /// <summary>
        // /// Default Description
        // /// </summary>
        // [Category("Appearance"),
        //  Browsable(true),
        //  Description("Default Description")]
        // public BorderStyle CustomBorderStyle
        // {
        //     get { return _CustomBorderStyle; }
        //     set { _CustomBorderStyle = value; this.Invalidate(); }
        // }

        public int Minimum
        {
            get
            {
                return min;
            }

            set
            {
                // Prevent a negative value.
                int newMin = value;
                if (value < 0)
                {
                    newMin = 0;
                }

                // Make sure that the minimum value is never set higher than the maximum value.
                if (value > max)
                {
                    max = newMin;
                }

                min = newMin;

                // Ensure value is still in range
                if (val < newMin)
                {
                    val = newMin;
                }

                // Invalidate the control to get a repaint.
                this.Invalidate();
            }
        }

        public int Maximum
        {
            get
            {
                return max;
            }

            set
            {
                int newMax = value;

                // Prevent a negative and 0 value.
                if (value < 1)
                {
                    newMax = 0;
                }

                // Make sure that the maximum value is never set lower than the minimum value.
                if (newMax < min)
                {
                    min = newMax;
                }

                max = newMax;

                // Make sure that value is still in range.
                if (val > newMax)
                {
                    val = newMax;
                }

                // Invalidate the control to get a repaint.
                this.Invalidate();
            }
        }

        public int Value
        {
            get
            {
                return val;
            }

            set
            {
                int oldValue = val;

                // Make sure that the value does not stray outside the valid range.
                if (value < min)
                {
                    val = min;
                }
                else if (value > max)
                {
                    val = max;
                }
                else
                {
                    val = value;
                }

                // Invalidate only the changed area.
                float percent;

                Rectangle newValueRect = this.ClientRectangle;
                Rectangle oldValueRect = this.ClientRectangle;

                // Use a new value to calculate the rectangle for progress.
                percent = (float)(val - min) / (float)(max - min);
                newValueRect.Width = (int)((float)newValueRect.Width * percent);

                // Use an old value to calculate the rectangle for progress.
                percent = (float)(oldValue - min) / (float)(max - min);
                oldValueRect.Width = (int)((float)oldValueRect.Width * percent);

                Rectangle updateRect = new Rectangle();

                // Find only the part of the screen that must be updated.
                if (newValueRect.Width > oldValueRect.Width)
                {
                    updateRect.X = oldValueRect.Size.Width;
                    updateRect.Width = newValueRect.Width - oldValueRect.Width;
                }
                else
                {
                    updateRect.X = newValueRect.Size.Width;
                    updateRect.Width = oldValueRect.Width - newValueRect.Width;
                }

                updateRect.Height = this.Height;

                // Invalidate the intersection region only.
                this.Invalidate(updateRect);
            }
        }

        private OverLayMode _OverlayMode = OverLayMode.None;
        /// <summary>
        /// Gradient Overlay Mode for the bar.
        /// </summary>
        [Category("Misc"),
         Browsable(true),
         Description("Gradient Overlay Mode for the bar."),
        DefaultValue("OverLayMode.None")]
        public OverLayMode OverlayMode
        {
            get { return _OverlayMode; }
            set { _OverlayMode = value; this.Invalidate(); }
        }

        private double _OverlayOpacity = 1.0;
        /// <summary>
        /// Overlay Opacity
        /// </summary>
        [Category("Misc"),
         Browsable(true),
         Description("Overlay Opacity"),
        DefaultValue(1.0)]
        public double OverlayOpacity
        {
            get { return _OverlayOpacity; }
            set
            {
                double newOpacity = value;

                if (value > 1.0)
                {
                    newOpacity = 1.0;
                }

                if (value < 0)
                {
                    newOpacity = 0;
                }

                _OverlayOpacity = newOpacity;
                this.Invalidate();
            }
        }

        private ProgressBarStyle _BarStyle;
        /// <summary>
        /// Progress bar animation style
        /// </summary>
        [Category("Custom"),
         Browsable(true),
         Description("Progress bar animation style")]
        public ProgressBarStyle BarStyle
        {
            get { return _BarStyle; }
            set
            {
                _BarStyle = value;
                if (this._BarStyle == ProgressBarStyle.Marquee)
                    marqueeTimer.Start();
                else
                    marqueeTimer.Stop();
            }
        }



        private Color _BorderColor = Color.Gray;
        public Color BorderColor
        {
            get { return _BorderColor; }
            set { _BorderColor = value; }
        }


        public Color ProgressBarColor
        {
            get
            {
                return BarColor;
            }

            set
            {
                BarColor = value;

                // Invalidate the control to get a repaint.
                this.Invalidate();
            }
        }

        private void DrawBorder(Graphics g)
        {
            int PenWidth = (int)Pens.White.Width;
            Pen borderPen = new Pen(_BorderColor);
            g.DrawLine(borderPen,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Top + PenWidth),
                new Point(this.ClientRectangle.Width, this.ClientRectangle.Top + PenWidth));
            g.DrawLine(borderPen,
                new Point(this.ClientRectangle.Left + PenWidth, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Left + PenWidth, this.ClientRectangle.Height - PenWidth));
            g.DrawLine(borderPen,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
            g.DrawLine(borderPen,
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
        }

        private void Draw3DBorder(Graphics g)
        {
            int PenWidth = (int)Pens.White.Width;

            g.DrawLine(Pens.DarkGray,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top));
            g.DrawLine(Pens.DarkGray,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.White,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.White,
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
        }

        public enum OverLayMode
        {
            None,
            Smooth,
            Gel,
            Blade
        }
    }
}
