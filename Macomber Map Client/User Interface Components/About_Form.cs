using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using Macomber_Map.Data_Connections;
using Macomber_Map.Properties;

namespace Macomber_Map.User_Interface_Components
{
    partial class About_Form : MM_Form
    {
        #region Variable declarations
        /// <summary>Our text for our about field</summary>
        private String AboutText = Application.ProductName + "\n" + Application.ProductVersion + "\n" +   Resources.Acknowledgements;

        /// <summary>How many pixels to offset Y</summary>
        private int YOffset = 0;

        /// <summary>The number of pixels to step every timer tick</summary>
        private int YStep = 1;

        /// <summary>The number of ms between ticks</summary>
        private int tmrInterval = 25;

        /// <summary>The size of our about text window</summary>
        private Size AboutTextSize;
        #endregion

        #region Form display / initialization
        /// <summary>
        /// Display the about form in its own thread
        /// </summary>
        public static void DisplayForm()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(DisplayAboutForm));
        }

        
        /// <summary>
        /// Display an about form in its own thread
        /// </summary>
        /// <param name="state"></param>
        private static void DisplayAboutForm(object state)        
        {
            using (About_Form frmAbout = new About_Form())
                Data_Integration.DisplayForm(frmAbout, Thread.CurrentThread);
        }

        /// <summary>
        /// Initialize our about form
        /// </summary>
        public About_Form()
        {
            //Create our description Image            
            InitializeComponent();            
            this.Text = "About " + Application.ProductName;
            tmrScroll.Interval = tmrInterval;
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                AboutTextSize = g.MeasureString(AboutText, this.Font).ToSize();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            picDescription.Paint += new PaintEventHandler(picDescription_Paint);            
            tmrScroll.Enabled = true;
        }

        #endregion

        #region Screen refresh handlers
        /// <summary>
        /// Handle the repaint of our about window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDescription_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            e.Graphics.TranslateTransform(0, -YOffset);
            Image Logo = Macomber_Map.Properties.Resources.ERCOT_Logo;
            Rectangle LogoBounds = new Rectangle(0, 0, e.ClipRectangle.Width, (Logo.Height * e.ClipRectangle.Width) / Logo.Width);
            
            if (YOffset < LogoBounds.Height)
                e.Graphics.DrawImage(Logo, LogoBounds);
            e.Graphics.DrawString(AboutText, this.Font, Brushes.White, 0, LogoBounds.Bottom);
            
            YOffset += YStep;
            if (YOffset >= AboutTextSize.Height + LogoBounds.Height)
                YOffset = -picDescription.Height;

       
        }
     

        /// <summary>
        /// Every interval, update our location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrScroll_Tick(object sender, EventArgs e)
        {
            if (this.Visible)
                picDescription.Refresh();
        }
        #endregion


        /// <summary>
        /// Handle the OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
