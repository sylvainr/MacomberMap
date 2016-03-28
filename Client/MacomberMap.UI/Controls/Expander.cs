using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace MacomberMap.UI
{
    [ToolboxItem(true), ToolboxBitmap(typeof(Expander), "MacomberMap.UI.Icons.ExpanderIco.png")]
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public class Expander : ContainerControl
    {
        #region Drawing Properties
        private bool _initializationComplete;
        private bool _isDisposing;
        private BufferedGraphicsContext _backbufferContext;
        private BufferedGraphics _backbufferGraphics;
        private Graphics _drawingGraphics;

        public new bool DesignMode
        {
            get
            {
                return (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv");
            }
        }
        #endregion

        #region Fields for Redraw
        private bool _isResizing = false;
        private bool _isMouseOver = false;
        private Size _restoreSize = new Size();
        private Rectangle _tabRectangle;
        private Timer timer;
        #endregion

        #region Public Fields

        public override Rectangle DisplayRectangle
        {
            get
            {
                Size clientSize = base.ClientSize;

                if (!_IsCollapsed)
                {
                    return new Rectangle(3, headerHeight + 6, Math.Max(clientSize.Width - 6, 0), Math.Max(clientSize.Height - headerHeight - 8, 0));
                }
                else
                    return new Rectangle(0, 0, 0, 0);
            }
        }

        #region Collapsed
        private CollapseDirection _CollapseDir;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Expander Collapsing"),
         Browsable(true),
         DefaultValue(CollapseDirection.Left),
         Description("Collapse Direction")]
        public CollapseDirection CollapseDir
        {
            get { return _CollapseDir; }
            set { _CollapseDir = value; this.Redraw(); UpdateResizeRectangle(); }
        }

        private bool _IsCollapsed = false;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Expander Collapsing"),
         Browsable(true),
         Description("Default Description")]
        public bool IsCollapsed
        {
            get { return _IsCollapsed; }
            set {
                if ((bool)value != _IsCollapsed)
                    ToggleCollapse();
            }
        }

        private bool _IsAnimated = true;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Expander Collapsing"),
         Browsable(true),
         DefaultValue(true),
         Description("Default Description")]
        public bool IsAnimated
        {
            get { return _IsAnimated; }
            set { _IsAnimated = value; }
        }

        private bool _AllowUserResize = true;
        /// <summary>
        /// Allow user to resize panel
        /// </summary>
        [Category("Expander Collapsing"),
         Browsable(true),
        DefaultValue(true),
         Description("Allow user to resize panel")]
        public bool AllowUserResize
        {
            get { return _AllowUserResize; }
            set { _AllowUserResize = value; }
        }


        #endregion

        #region Header
        private string _HeaderText;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Expander Header"),
         Browsable(true),
         Description("Default Description")]
        public string HeaderText
        {
            get { return _HeaderText; }
            set { _HeaderText = value; this.Redraw(); }
        }

        private Color _HeaderTextColor = Color.White;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Expander Header"),
         Browsable(true),
         Description("Header Text Color")]
        public Color HeaderTextColor
        {
            get { return _HeaderTextColor; }
            set { _HeaderTextColor = value; this.Redraw(); }
        }

        private Color _HeaderTextHighlightColor = Color.Black;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Expander Header"),
         Browsable(true),
         Description("Header Text Highlighted Color")]
        public Color HeaderTextHighlightColor
        {
            get { return _HeaderTextHighlightColor; }
            set { _HeaderTextHighlightColor = value; this.Redraw(); }
        }

        private Font _HeaderFont = Expander.DefaultFont;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Expander Header"),
         Browsable(true),
         Description("Default Description")]
        public Font HeaderFont
        {
            get { return _HeaderFont; }
            set { _HeaderFont = value; this.Redraw(); }
        }

        private int headerHeight
        {
            get { return Math.Max(_HeaderFont.Height, 18); }
        }

        #endregion

        #region Background Style
        private Color _BackColor = Color.FromArgb(55, 63, 78);
        /// <summary>
        /// Background Hatch Color
        /// </summary>
        [Category("Appearance"),
         Browsable(true),
         DefaultValue("55, 63, 78"),
         Description("Background Color")]
        public override Color BackColor
        {
            get { return _BackColor; }
            set { _BackColor = value; this.Redraw(); }
        }

        private Color _BackHatchColor = Color.FromArgb(66, 76, 93);
        /// <summary>
        /// Background Hatch Color
        /// </summary>
        [Category("Appearance"),
         Browsable(true),
        DefaultValue("66, 76, 93"),
         Description("Background Hatch Color")]
        public Color BackHatchColor
        {
            get { return _BackHatchColor; }
            set { _BackHatchColor = value; this.Redraw(); }
        }

        private Color _WorkAreaColor = Color.White;
        /// <summary>
        /// Background Hatch Color
        /// </summary>
        [Category("Appearance"),
         Browsable(true),
         DefaultValue("White"),
         Description("Work area color")]
        public Color WorkAreaColor
        {
            get { return _WorkAreaColor; }
            set { _WorkAreaColor = value; this.Redraw(); }
        }

        private HatchStyle _HatchStyle = HatchStyle.Percent20;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Appearance"),
         Browsable(true),
         DefaultValue("HatchStyle.Percent20"),
         Description("Background Hatch Style")]
        public HatchStyle HatchStyle
        {
            get { return _HatchStyle; }
            set { _HatchStyle = value; this.Redraw(); }
        }

        #endregion

        #region Tab Color

        private Color _InactiveHighlight = Color.FromArgb(90, 100, 117);
        /// <summary>
        /// BackColor
        /// </summary>
        [Category("Expander Header Color"),
         Browsable(true),
         Description("Default Description")]
        public Color InactiveHighlight
        {
            get { return _InactiveHighlight; }
            set { _InactiveHighlight = value; this.Redraw(); }
        }
        private Color _Inactive = Color.FromArgb(77, 87, 105);
        /// <summary>
        /// BackColor
        /// </summary>
        [Category("Expander Header Color"),
         Browsable(true),
         Description("Default Description")]
        public Color Inactive
        {
            get { return _Inactive; }
            set { _Inactive = value; this.Redraw(); }
        }

        private Color _HightlightColor = Color.FromArgb(189, 223, 246);
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Expander Header Color"),
         Browsable(true),
         Description("Default Description")]
        public Color HightlightColor
        {
            get { return _HightlightColor; }
            set { _HightlightColor = value; this.Redraw(); }
        }
        private Color _HightlightColorDim = Color.FromArgb(176, 203, 241);
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Expander Header Color"),
         Browsable(true),
         Description("Default Description")]
        public Color HightlightColorDim
        {
            get { return _HightlightColorDim; }
            set { _HightlightColorDim = value; this.Redraw(); }
        }

        private Color _ActiveColor = Color.FromArgb(141, 163, 193);
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Expander Header Color"),
         Browsable(true),
         Description("Default Description")]
        public Color ActiveColor
        {
            get { return _ActiveColor; }
            set { _ActiveColor = value; this.Redraw(); }
        }


        #endregion

        #endregion

        #region Designer

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox pictureBoxResizer = new PictureBox();
        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBoxResizer = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResizer)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxResizer
            // 
            this.pictureBoxResizer.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxResizer.Cursor = System.Windows.Forms.Cursors.SizeNS;
            this.pictureBoxResizer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pictureBoxResizer.Location = new System.Drawing.Point(140, 0);
            this.pictureBoxResizer.Name = "pictureBoxResizer";
            this.pictureBoxResizer.Size = new System.Drawing.Size(10, 150);
            this.pictureBoxResizer.TabIndex = 0;
            this.pictureBoxResizer.TabStop = false;
            this.pictureBoxResizer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBoxResizer_MouseDown);
            this.pictureBoxResizer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxResizer_MouseMove);
            this.pictureBoxResizer.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBoxResizer_MouseUp);
            // 
            // Expander
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "Expander";
            this.Controls.Add(this.pictureBoxResizer);
            this.Size = new System.Drawing.Size(150,150);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResizer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        #endregion

        #region Constructor

        public Expander()
        {
            InitializeComponent();

            // Set the control style to double buffer.
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            pictureBoxResizer.Enabled = (!_IsCollapsed && _AllowUserResize);
            _restoreSize = this.Size;
            // Assign our buffer context.
            _backbufferContext = BufferedGraphicsManager.Current;
            _initializationComplete = true;
            RecreateBuffers();

            Redraw();

            #region Timed Redraw Logic
            timer = new Timer();
            timer.Interval = 5;
            timer.Tick += new EventHandler(timer_Tick);

            #endregion
        }

        #endregion

        #region Buffer on OnEvents

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateResizeRectangle();
            RecreateBuffers();
            Redraw();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            _isDisposing = true;
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                // We must dispose of backbufferGraphics before we dispose of backbufferContext or we will get an exception.
                //if (_backbufferGraphics != null)
                //{
                //    _backbufferGraphics.Dispose();
                //    _backbufferGraphics = null;
                //}
                //if (_backbufferContext != null)
                //{
                //    _backbufferContext.Dispose();
                //}
            }

            base.Dispose(disposing);
        }

        private void RecreateBuffers()
        {
            // Check initialization has completed so we know backbufferContext has been assigned.
            // Check that we aren't disposing or this could be invalid.
            if (!_initializationComplete || _isDisposing)
                return;

            // We recreate the buffer with a width and height of the control. The "+ 1" 
            // guarantees we never have a buffer with a width or height of 0. 
            _backbufferContext.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);

            // Dispose of old backbufferGraphics (if one has been created already)
            if (_backbufferGraphics != null)
                _backbufferGraphics.Dispose();

            // Create new backbufferGrpahics that matches the current size of buffer.
            _backbufferGraphics = _backbufferContext.Allocate(this.CreateGraphics(),
                new Rectangle(0, 0, Math.Max(this.Width, 1), Math.Max(this.Height, 1)));

            // Assign the Graphics object on backbufferGraphics to "drawingGraphics" for easy reference elsewhere.
            _drawingGraphics = _backbufferGraphics.Graphics;

            // This is a good place to assign drawingGraphics.SmoothingMode if you want a better anti-aliasing technique.

            // Invalidate the control so a repaint gets called somewhere down the line.
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // If we've initialized the backbuffer properly, render it on the control. 
            // Otherwise, do just the standard control paint.
            if (!_isDisposing && _backbufferGraphics != null)
            {
                _backbufferGraphics.Render(e.Graphics);
            }
        }

        #endregion

        #region Tab Resizer

        private void pictureBoxResizer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                switch (CollapseDir)
                {
                    case CollapseDirection.Down:
                        this.Height -= e.Y;
                        break;
                    case CollapseDirection.Up:
                        this.Height += e.Y;
                        break;
                    case CollapseDirection.Right:
                        this.Width -= e.X;
                        break;
                    default:
                        this.Width += e.X;
                        break;
                }
                this.Width = Math.Max(headerHeight + base.Padding.Left, this.Width);
                this.Height = Math.Max(headerHeight + base.Padding.Top, this.Height);

                _restoreSize = this.Size;
            }
        }

        private void pictureBoxResizer_MouseUp(object sender, MouseEventArgs e)
        {
            _isResizing = false;
        }

        private void pictureBoxResizer_MouseDown(object sender, MouseEventArgs e)
        {
            _isResizing = true;
        }

        private void UpdateResizeRectangle()
        {
            int resizeWidth = 3;
            Rectangle _ResizeRectangle;
            switch (CollapseDir)
            {
                case CollapseDirection.Up:
                    _ResizeRectangle = new Rectangle(0, this.Height - resizeWidth, this.Width, resizeWidth);
                    this.pictureBoxResizer.Cursor = Cursors.SizeNS;
                    this.pictureBoxResizer.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                    break;
                case CollapseDirection.Down:
                    _ResizeRectangle = new Rectangle(0, 0, this.Width, resizeWidth);
                    this.pictureBoxResizer.Cursor = Cursors.SizeNS;
                    this.pictureBoxResizer.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                    break;
                case CollapseDirection.Right:
                    _ResizeRectangle = new Rectangle(0, 0, resizeWidth, this.Height);
                    this.pictureBoxResizer.Cursor = Cursors.SizeWE;
                    this.pictureBoxResizer.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
                    break;
                default: // Left
                    _ResizeRectangle = new Rectangle(this.Width - resizeWidth, 0, resizeWidth, this.Height);
                    this.pictureBoxResizer.Cursor = Cursors.SizeWE;
                    this.pictureBoxResizer.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                    break;
            }
            pictureBoxResizer.Location = _ResizeRectangle.Location;
            pictureBoxResizer.Size = _ResizeRectangle.Size;
        }

        public void ToggleCollapse()
        {
            if (!_IsCollapsed)
                _restoreSize = this.Size;

            _IsCollapsed = !_IsCollapsed;


            pictureBoxResizer.Enabled = (!_IsCollapsed && _AllowUserResize);
            timer.Start();
        }

        #endregion

        #region Custom Drawing Code

        private void timer_Tick(object sender, EventArgs e)
        {
            bool isComplete = false;

            int movespeed = 25;

            int collapsedWidth = (headerHeight + base.Margin.Left);
            int collapsedHeight = (headerHeight + base.Margin.Top);

            if (!_IsCollapsed)
            {
                if (CollapseDir == CollapseDirection.Left || CollapseDir == CollapseDirection.Right)
                {
                    if (this.DesignMode || !this.IsAnimated)
                    {
                        this.Width = _restoreSize.Width;
                        isComplete = true;
                    }
                    else
                    {
                        if (this.Width < (_restoreSize.Width - movespeed))
                        {
                            this.Width = this.Width + movespeed;
                        }
                        else
                        {
                            this.Width = _restoreSize.Width;
                            isComplete = true;
                        }
                    }
                }
                else
                {
                    if (this.DesignMode || !this.IsAnimated)
                    {
                        this.Height = _restoreSize.Height;
                        isComplete = true;
                    }
                    else
                    {
                        if (this.Height < (_restoreSize.Height - movespeed))
                        {
                            this.Height = this.Height + movespeed;
                        }
                        else
                        {
                            this.Height = _restoreSize.Height;
                            isComplete = true;
                        }
                    }
                }
            }
            else
            {
                movespeed = 50;
                if (CollapseDir == CollapseDirection.Left || CollapseDir == CollapseDirection.Right)
                {
                    if (this.DesignMode || !this.IsAnimated)
                    {
                        this.Width = collapsedWidth;
                        isComplete = true;
                    }
                    else
                    {
                        if (this.Width > collapsedWidth + movespeed)
                        {
                            this.Width = this.Width - movespeed;
                        }
                        else
                        {
                            this.Width = collapsedWidth;
                            isComplete = true;
                        }
                    }
                }
                {
                    if (this.DesignMode || !this.IsAnimated)
                    {
                        this.Height = headerHeight + base.Margin.Top;
                        isComplete = true;
                    }
                    else
                    {
                        if (this.Height > (collapsedHeight + movespeed))
                        {
                            this.Height = this.Height - movespeed;
                        }
                        else
                        {
                            this.Height = collapsedHeight;
                            isComplete = true;
                        }
                    }
                }
            }

            if (isComplete)
            {
                timer.Stop();
                foreach (Control ctrl in this.Controls)
                {
                    if (_IsCollapsed)
                        ctrl.Region = new Region(new Rectangle(0, 0, 0, 0));
                    else
                        ctrl.Region = new Region(ctrl.ClientRectangle);
                }
            }
        }

        private void Redraw()
        {
            // In this Redraw method, we simply make the control fade from black to white on a timer.
            // But, you can put whatever you want here and detach the timer. The trick is just making
            // sure redraw gets called when appropriate and only when appropriate. Examples would include
            // when you resize, when underlying data is changed, when any of the draqwing properties are changed
            // (like BackColor, Font, ForeColor, etc.)
            if (_drawingGraphics == null)
                return;

            #region Perform Custom Drawing Here
            PaintBack(_drawingGraphics);
            PaintFore(_drawingGraphics);
            #endregion

            // Force the control to both invalidate and update. 
            this.Refresh();
        }

        private void PaintFore(Graphics g)
        {
            int headersize = headerHeight;

            Color headerTextColor = _HeaderTextColor;
            Color gradientStart = _Inactive;
            Color gradientEnd = _InactiveHighlight;
            Color borderColor = _InactiveHighlight;

            if (_isMouseOver)
            {
                headerTextColor = _HeaderTextHighlightColor;
                gradientStart = _HightlightColorDim;
                gradientEnd = _HightlightColor;
                borderColor = _HightlightColor;
            }

            if (!IsCollapsed || CollapseDir == CollapseDirection.Up || CollapseDir == CollapseDirection.Down)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                _tabRectangle = new Rectangle(base.Margin.Left, base.Margin.Top, this.Width - (base.Margin.Left + base.Margin.Right), headersize);
                GraphicsPath rr = RoundedRectangle.Create(_tabRectangle, 3, RoundedRectangle.RectangleCorners.TopLeft | RoundedRectangle.RectangleCorners.TopRight);
                g.FillPath(new LinearGradientBrush(_tabRectangle, gradientStart, gradientEnd, 270), rr);
                g.DrawPath(new Pen(borderColor, 1), rr);
                g.SmoothingMode = SmoothingMode.Default;
                g.DrawString(_HeaderText, _HeaderFont, new SolidBrush(headerTextColor), _tabRectangle.X, _tabRectangle.Y + 3);
                //g.DrawLine(new Pen(_HightlightColorDim, 3), 0, 20, this.Width, 20);
            }
            else
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                SizeF stringsize = g.MeasureString(_HeaderText, _HeaderFont);
                GraphicsPath rr;
                if (CollapseDir == CollapseDirection.Left)
                {
                    _tabRectangle = new Rectangle(0, 4, headersize, Math.Min((int)stringsize.Width + 7, this.Height - 10));
                    rr = RoundedRectangle.Create(_tabRectangle, 3, RoundedRectangle.RectangleCorners.BottomRight | RoundedRectangle.RectangleCorners.TopRight);
                }
                else
                {
                    _tabRectangle = new Rectangle(this.Width - headersize, 4, headersize, Math.Min((int)stringsize.Width + 7, this.Height - 10));
                    rr = RoundedRectangle.Create(_tabRectangle, 3, RoundedRectangle.RectangleCorners.BottomLeft | RoundedRectangle.RectangleCorners.TopLeft);
                }

                StringFormat sf = new StringFormat(StringFormatFlags.DirectionVertical);
                g.FillPath(new LinearGradientBrush(_tabRectangle, gradientStart, gradientEnd, 180), rr);
                g.DrawPath(new Pen(borderColor, 1), rr);
                g.SmoothingMode = SmoothingMode.Default;
                g.DrawString(_HeaderText, _HeaderFont, new SolidBrush(headerTextColor), _tabRectangle.X, _tabRectangle.Y + 3, sf);
                //g.DrawLine(new Pen(_HightlightColorDim, 3), 0, 20, this.Width, 20);

            }
        }

        private void PaintBack(Graphics g)
        {
            g.Clear(_BackColor);
            //g.FillRectangle(new SolidBrush(Color.FromArgb(64, 64, 64)), this.ClientRectangle);
            g.FillRectangle(new HatchBrush(_HatchStyle, _BackHatchColor, _BackColor), this.ClientRectangle);

            if (!IsCollapsed)
            {
                SolidBrush workareabrush = new SolidBrush(_WorkAreaColor);
                Rectangle workarea = new Rectangle(base.Margin.Left,  headerHeight + base.Margin.Top, this.Width - (base.Margin.Right + base.Margin.Left), this.Height - (headerHeight + base.Margin.Top + base.Margin.Bottom + 3));
                g.FillRectangle(workareabrush , workarea);
                g.DrawRectangle(new Pen(workareabrush), workarea);
                workareabrush.Dispose();
            }
        }

        
        #endregion

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_isMouseOver)
                this.ToggleCollapse();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isMouseOver != (_tabRectangle.Contains(e.Location)))
            {
                _isMouseOver = (_tabRectangle.Contains(e.Location));
                Redraw();
            }
            
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isMouseOver = false;
            Redraw();
        }

        #region Enums
        public enum CollapseDirection
        {
            Left,
            Up,
            Right,
            Down
        }
        #endregion
    }
}
