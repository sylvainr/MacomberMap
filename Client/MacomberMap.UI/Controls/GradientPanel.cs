using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MacomberMap.UI
{

    [ToolboxItem(true), ToolboxBitmap(typeof(GradientPanel), "MacomberMap.UI.Icons.GradientPanelIco.png")]
    public class GradientPanel : System.Windows.Forms.Panel
    {
        #region Constructor
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
            // GradientPanel
            // 
            this.Paint += new System.Windows.Forms.PaintEventHandler(GradientPanel_Paint);

            this.ResumeLayout(false);

        }
        #endregion
        #endregion

        #region Properties

        private Padding _BorderWidth = new Padding(1);
        /// <summary>
        /// The border width of the panel. Each side can be set individually.
        /// </summary>
        [Category("Gradient"),
         Browsable(true),
         DefaultValue("1"),
         Description("The border width of the panel. Each side can be set individually.")]
        public Padding BorderWidth
        {
            get { return _BorderWidth; }
            set { _BorderWidth = value; this.Invalidate(); }
        }

        private bool _DoubleBorder = false;
        /// <summary>
        /// Draws a double border using BorderColor2
        /// </summary>
        [Category("Gradient"),
         Browsable(true),
         Description("Draws a double border using BorderColor2"),
        DefaultValue(false)]
        public bool DoubleBorder
        {
            get { return _DoubleBorder; }
            set { _DoubleBorder = value; this.Invalidate(); }
        }

        private Color _BorderColor = Color.Silver;
        /// <summary>
        /// The color of the borders.
        /// </summary>
        [Category("Gradient"),
         Browsable(true),
         DefaultValue("Silver"),
         Description("The color of the border.")]
        public Color BorderColor
        {
            get { return _BorderColor; }
            set { _BorderColor = value; this.Invalidate(); }
        }

        private Color _BorderColor2 = Color.DimGray;
        /// <summary>
        /// The color of the borders.
        /// </summary>
        [Category("Gradient"),
         Browsable(true),
         DefaultValue("DimGray"),
         Description("The second color of the border.")]
        public Color BorderColor2
        {
            get { return _BorderColor2; }
            set { _BorderColor2 = value; this.Invalidate(); }
        }

        private Color _StartColor = Color.FromArgb(64, 64, 64);
        /// <summary>
        /// First color of the gradient.
        /// </summary>
        [Category("Gradient"),
         Browsable(true),
         DefaultValue("64,64,64"),
         Description("First color of the gradient.")]
        public Color StartColor
        {
            get { return _StartColor; }
            set { _StartColor = value; this.Invalidate(); }
        }

        private Color _EndColor = Color.Black;
        /// <summary>
        /// Second color of the Gradient.
        /// </summary>
        [Category("Gradient"),
         Browsable(true),
         DefaultValue("Black"),
         Description("Second color of the Gradient.")]
        public Color EndColor
        {
            get { return _EndColor; }
            set { _EndColor = value; this.Invalidate(); }
        }

        private int _GradientAngle = 0;
        /// <summary>
        /// The angle of the gradient.
        /// </summary>
        [Category("Gradient"),
         Browsable(true),
         DefaultValue(0),
         Description("The angle of the gradient.")]
        public int GradientAngle
        {
            get { return _GradientAngle; }
            set
            {
                if (value > 359)
                    _GradientAngle = value % 360;
                else if (value < 0)
                    _GradientAngle = 360 + (value % 360);
                else
                    _GradientAngle = value;
                this.Invalidate();
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

        #endregion

        public GradientPanel()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            this.Paint += new PaintEventHandler(GradientPanel_Paint);
            this.Resize += new EventHandler(GradientPanel_Resize);
        }

        #region Paint Methods
        private void GradientPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            PaintGradient(e.Graphics);
            PaintOverlay(e.Graphics);
            PaintBorders(e.Graphics);
        }

        private void PaintGradient(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            System.Drawing.Drawing2D.LinearGradientBrush gradBrush;
            Rectangle r = new Rectangle(0, 0, Math.Max(1, this.Width), Math.Max(1, this.Height));
            gradBrush = new LinearGradientBrush(r, _StartColor, _EndColor, _GradientAngle);
            g.FillRectangle(gradBrush, r);
        }

        private void PaintOverlay(Graphics g)
        {
            Rectangle rect = this.ClientRectangle;

            // Overlay Smooth Gradient
            if (this._OverlayMode == OverLayMode.Smooth)
            {
                LinearGradientBrush lgb = new LinearGradientBrush((RectangleF)rect, Color.Transparent, Color.FromArgb((int)(255 * OverlayOpacity), Color.Black), 90);
                g.FillRectangle(lgb, rect);
            }
            else if (this._OverlayMode == OverLayMode.Gel)
            {
                Rectangle topHalf = rect;
                Rectangle bottomHalf = rect;
                topHalf.Height = (int)(topHalf.Height * 0.5);
                bottomHalf.Height = this.ClientRectangle.Height - topHalf.Height;
                bottomHalf.Y = topHalf.Height;

                LinearGradientBrush lgb = new LinearGradientBrush((RectangleF)topHalf, Color.FromArgb((int)(192 * OverlayOpacity), Color.Black), Color.FromArgb((int)(64 * OverlayOpacity), Color.Black), 90);
                g.FillRectangle(lgb, topHalf);

                lgb = new LinearGradientBrush((RectangleF)bottomHalf, Color.Transparent, Color.FromArgb((int)(255 * OverlayOpacity), Color.Black), 90);
                g.FillRectangle(lgb, bottomHalf);

                lgb.Dispose();
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

        private void PaintBorders(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.None;
            Padding shift = new Padding();

            shift.Top = _BorderWidth.Top / 2;

            shift.Bottom = Height - _BorderWidth.Bottom / 2;
            if (_BorderWidth.Bottom % 2 != 0)
                shift.Bottom -= 1;

            shift.Left = _BorderWidth.Left / 2;

            shift.Right = Width - _BorderWidth.Right / 2;
            if (_BorderWidth.Right % 2 != 0)
                shift.Right -= 1;

            if (this._DoubleBorder)
            {
                if (_BorderWidth.Top > 0)
                    g.DrawLine(new Pen(new SolidBrush(_BorderColor2), _BorderWidth.Top),
                        0, shift.Top + _BorderWidth.Top, this.Width, shift.Top + _BorderWidth.Top);

                if (_BorderWidth.Bottom > 0)
                    g.DrawLine(new Pen(new SolidBrush(_BorderColor2), _BorderWidth.Bottom),
                        0, shift.Bottom - _BorderWidth.Bottom, Width, shift.Bottom - _BorderWidth.Bottom);

                if (_BorderWidth.Left > 0)
                    g.DrawLine(new Pen(new SolidBrush(_BorderColor2), _BorderWidth.Left),
                        shift.Left + _BorderWidth.Left, 0, shift.Left + _BorderWidth.Left, Height);

                if (_BorderWidth.Right > 0)
                    g.DrawLine(new Pen(new SolidBrush(_BorderColor2), _BorderWidth.Right),
                        shift.Right - _BorderWidth.Right, 0, shift.Right - _BorderWidth.Right, Height);
            }


            if (_BorderWidth.Top > 0)
                g.DrawLine(new Pen(new SolidBrush(_BorderColor), _BorderWidth.Top),
                    0, shift.Top, this.Width, shift.Top);

            if (_BorderWidth.Bottom > 0)
                g.DrawLine(new Pen(new SolidBrush(_BorderColor), _BorderWidth.Bottom),
                    0, shift.Bottom, Width, shift.Bottom);

            if (_BorderWidth.Left > 0)
                g.DrawLine(new Pen(new SolidBrush(_BorderColor), _BorderWidth.Left),
                    shift.Left, 0, shift.Left, Height);

            if (_BorderWidth.Right > 0)
                g.DrawLine(new Pen(new SolidBrush(_BorderColor), _BorderWidth.Right),
                    shift.Right, 0, shift.Right, Height);


        }

        #endregion

        private void GradientPanel_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
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
