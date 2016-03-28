using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;

namespace MacomberMap.UI
{
    //[System.ComponentModel.ToolboxItem(true)]
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    [ToolboxItem(true), ToolboxBitmap(typeof(GenBarGraph), "MacomberMap.UI.Icons.BarGraphDevColumnIco.png")]
    public class GenBarGraph : ContainerControl
    {

        #region Fields

        private DeviationBarData _Value;

        private BufferedGraphicsContext backbufferContext;

        private BufferedGraphics backbufferGraphics;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Graphics drawingGraphics;

        private bool initializationComplete;

        private bool isDisposing;

        #endregion

        #region Constructors

        public GenBarGraph()
        {
            InitializeComponent();

            // Set the control style to double buffer.
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            // Assign our buffer context.
            backbufferContext = BufferedGraphicsManager.Current;
            initializationComplete = true;

            RecreateBuffers();

            Redraw();
        }

        #endregion

        #region Properties

        public new bool DesignMode
        {
            get
            {
                return (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv");
            }
        }

        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Data"),
         Browsable(true),
         Description("Value")]
        public DeviationBarData Value
        {
            get { return _Value; }
            set { _Value = value; Redraw(); }
        }

        #endregion

        #region Protected Methods

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            isDisposing = true;
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                // We must dispose of backbufferGraphics before we dispose of backbufferContext or we will get an exception.
                if (backbufferGraphics != null)
                    backbufferGraphics.Dispose();
                if (backbufferContext != null)
                    backbufferContext.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // If we've initialized the backbuffer properly, render it on the control. 
            // Otherwise, do just the standard control paint.
            if (!isDisposing && backbufferGraphics != null)
                backbufferGraphics.Render(e.Graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecreateBuffers();
            Redraw();
        }

        #endregion

        #region Private Methods

        private void DrawBar(Graphics graphics)
        {
            // TODO: Implement myProc
            DeviationBarData cellVal = this.Value as DeviationBarData ?? new DeviationBarData();

            double maxActDisp = Math.Max(cellVal.Actual, cellVal.Dispatch);
            double maxVal = Math.Max(maxActDisp, cellVal.Max);

            float barWidthDev = 0;
            float barWidthDisp = 0;

            int drawHeight = this.Height - 1;
            int drawWidth = this.Width - 1;

            // Draw Background
            Rectangle background = new Rectangle(0, 0, drawWidth, drawHeight);
            graphics.FillRectangle(Brushes.Black, background);
            //graphics.SetClip(background);
            if (maxVal > 0 && maxActDisp > 0)
            {

                barWidthDisp = Math.Max(0, (float)(cellVal.Dispatch / maxVal * drawWidth));
                barWidthDev = (float)(Math.Abs(cellVal.Deviation / maxVal) * drawWidth);
                //barWidthMW = (float)(cellVal.Actual / cellVal.Max) * drawWidth;

                //if (barWidthDisp < 0) { barWidthDisp = 0; }
                //if (barWidthMW < 0) { barWidthMW = 0; }
                if (cellVal.Dispatch < 0 && barWidthDev > barWidthDisp) { barWidthDev = barWidthDisp; }

                // Draw Dispatch Blue Bar
                Rectangle barDisp = new Rectangle(0, 0, (int)barWidthDisp, drawHeight);
                if (cellVal.IsDispatched)
                    graphics.FillRectangle(new SolidBrush(Color.Navy), barDisp);
                else
                    graphics.FillRectangle(new SolidBrush(Color.ForestGreen), barDisp);

                // Draw Deviation Bar
                Rectangle barDEV;
                if (cellVal.Deviation >= 0) // Green +
                {
                    barDEV = new Rectangle(0 + (int)barWidthDisp, 0, (int)barWidthDev, drawHeight);
                    graphics.FillRectangle(Brushes.Lime, barDEV);
                }
                else // Red -
                {
                    barDEV = new Rectangle(0 + (int)barWidthDisp - (int)(barWidthDev), 0, (int)barWidthDev, drawHeight);
                    graphics.FillRectangle(Brushes.Red, barDEV);
                }

                // Draw End Line
                Pen penCap = new Pen(Brushes.Yellow, 2.0f);
                int endCapX = 0;
                if ((int)barWidthDisp - 1 > 0)
                    endCapX = endCapX + (int)barWidthDisp - 1;

                Point start = new Point(endCapX, 0);
                Point end = new Point(endCapX, 0 + drawHeight);
                graphics.DrawLine(penCap, start, end);

                // Draw Super-awesome gradient overlay
                Color PageStartColor = Color.FromArgb(50, Color.Gray);
                Color PageEndColor = Color.FromArgb(80, Color.White);
                Rectangle barAreaTop;
                Rectangle barAreaBottom;
                int barWidth = (int)Math.Max(0, (float)(maxActDisp / maxVal * drawWidth));
                barAreaTop = new Rectangle(0, 0, barWidth, this.Height / 2);
                barAreaBottom = new Rectangle(0, 0 + (this.Height / 2), barWidth, (this.Height / 2) - 1);

                System.Drawing.Drawing2D.LinearGradientBrush gradBrushHighlight;
                System.Drawing.Drawing2D.LinearGradientBrush gradBrushLowlight;

                gradBrushHighlight = new System.Drawing.Drawing2D.LinearGradientBrush(
                            new Point(0, 0),
                            new Point(0, 0 + this.Height),
                            PageStartColor, PageEndColor);

                PageStartColor = Color.FromArgb(25, Color.White);
                PageEndColor = Color.FromArgb(100, Color.Black);
                gradBrushLowlight = new System.Drawing.Drawing2D.LinearGradientBrush(
                            new Point(0, 0),
                            new Point(0, 0 + this.Height),
                            PageStartColor, PageEndColor);

                graphics.FillRectangle(gradBrushHighlight, barAreaTop);
                graphics.FillRectangle(gradBrushLowlight, barAreaBottom);

            }
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GenBarGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "GenBarGraph";
            this.Size = new System.Drawing.Size(251, 32);
            this.ResumeLayout(false);

        }

        private void RecreateBuffers()
        {
            // Check initialization has completed so we know backbufferContext has been assigned.
            // Check that we aren't disposing or this could be invalid.
            if (!initializationComplete || isDisposing)
                return;

            // We recreate the buffer with a width and height of the control. The "+ 1" 
            // guarantees we never have a buffer with a width or height of 0. 
            backbufferContext.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);

            // Dispose of old backbufferGraphics (if one has been created already)
            if (backbufferGraphics != null)
                backbufferGraphics.Dispose();

            // Create new backbufferGrpahics that matches the current size of buffer.
            backbufferGraphics = backbufferContext.Allocate(this.CreateGraphics(),
                new Rectangle(0, 0, Math.Max(this.Width, 1), Math.Max(this.Height, 1)));

            // Assign the Graphics object on backbufferGraphics to "drawingGraphics" for easy reference elsewhere.
            drawingGraphics = backbufferGraphics.Graphics;

            // This is a good place to assign drawingGraphics.SmoothingMode if you want a better anti-aliasing technique.

            // Invalidate the control so a repaint gets called somewhere down the line.
            this.Invalidate();
        }

        private void Redraw()
        {
            // In this Redraw method, we simply make the control fade from black to white on a timer.
            // But, you can put whatever you want here and detach the timer. The trick is just making
            // sure redraw gets called when appropriate and only when appropriate. Examples would include
            // when you resize, when underlying data is changed, when any of the draqwing properties are changed
            // (like BackColor, Font, ForeColor, etc.)
            if (drawingGraphics == null)
                return;

            #region Perform Custom Drawing Here
            DrawBar(drawingGraphics);

            #endregion

            // Force the control to both invalidate and update. 
            this.Refresh();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Redraw();
        }

        #endregion

    }
}
