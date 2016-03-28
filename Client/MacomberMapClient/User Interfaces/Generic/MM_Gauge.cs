using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Generic
{
    /// <summary>
    /// © 2015, Michael E. Legatt, Ph.D., Paul Li, Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides a gauge control
    /// </summary>
    public partial class MM_Gauge : Control
    {
        #region Variable declarations
        /// <summary>Our minimum value</summary>
        public double Minimum { get; set; }

        /// <summary>Our maximum value</summary>
        public double Maximum { get; set; }

        /// <summary>Our current value</summary>
        public double Current
        {
            get { return _Current; }
            set { _Current = value; Invalidate();

            if (CurrentChanged != null)
                CurrentChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>Our event for current value changes</summary>
        public event EventHandler CurrentChanged;

        /// <summary>The format of our control for our digital readout</summary>        
        public String NumberFormat { get; set; }

        private double _Current = 0;

        /// <summary>Our minimum angle</summary>
        public int MinimumAngle { get; set; }

        /// <summary>Our minimum angle</summary>
        public int MaximumAngle { get; set; }

        /// <summary>Our error range band</summary>
        [Category("Ranges")]
        public double? ErrorRangeMinimum { get; set; }

        [Category("Ranges")]
        public double? ErrorRangeMaximum { get; set; }

        /// <summary>Our warning range band</summary>
        [Category("Ranges")]
        public double? WarningRangeMinimum { get; set; }

        [Category("Ranges")]
        public double? WarningRangeMaximum { get; set; }

        /// <summary>Our good range band</summary>        
        [Category("Ranges")]
        public double? GoodRangeMinimum { get; set; }

        [Category("Ranges")]
        public double? GoodRangeMaximum { get; set; }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new gauge
        /// </summary>
        public MM_Gauge()
        {
            InitializeComponent();
            this.BackColor = Color.Black;
            NumberFormat = "##.##";
            this.DoubleBuffered = true;
            MinimumAngle = -150;
            MaximumAngle = 150;
        }

        /// <summary>
        /// Assign our gauge
        /// </summary>
        /// <param name="CurrVal"></param>
        /// <param name="MinVal"></param>
        /// <param name="MaxVal"></param>
        public void AssignGauge(double CurrVal, double MinVal, double MaxVal)
        {
            this.Current = CurrVal;
            this.Minimum = MinVal;
            this.Maximum = MaxVal;
        }

        /// <summary>
        /// Paint our control
        /// </summary>
        /// <param name="pe"></param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            GenerateGaugeImage(pe);
            Gauge_Paint(pe);
        }

        /// <summary>
        /// Generate our gauge image
        /// </summary>
        public void GenerateGaugeImage(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            g.Clear(Color.Black);

            Rectangle Bounds = new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            float Radius = Bounds.Width / 2f;

            PointF Center = new PointF(Bounds.Width / 2f, Bounds.Height / 2f);
            using (SolidBrush BackBrush = new SolidBrush(Color.FromArgb(16, 16, 16)))
                g.FillEllipse(BackBrush, Bounds);

            //Draw our bands as requested
            if (ErrorRangeMinimum.HasValue && ErrorRangeMaximum.HasValue)
                DrawBand(g, ErrorRangeMinimum.Value, ErrorRangeMaximum.Value, Brushes.Red);
            if (WarningRangeMinimum.HasValue && WarningRangeMaximum.HasValue)
                DrawBand(g, WarningRangeMinimum.Value, WarningRangeMaximum.Value, Brushes.Yellow);
            if (GoodRangeMinimum.HasValue && GoodRangeMaximum.HasValue)
                DrawBand(g, GoodRangeMinimum.Value, GoodRangeMaximum.Value, Brushes.Green);

            for (int Angle = MinimumAngle; Angle <= MaximumAngle; Angle += 5)
                g.DrawLine(Pens.Gray, Offset(Radius - 3, Angle), Offset(Radius - (Angle % 15 == 0 ? 15 : 10), Angle));

            using (Pen DrawPen = new Pen(Color.Gray, 3))
                g.DrawEllipse(DrawPen, Bounds);
            using (Pen DrawPen = new Pen(Color.White, 3))
                g.DrawEllipse(DrawPen, 2, 2, Bounds.Width - 4, Bounds.Height - 4);

            WriteText(g, Radius - 30, MinimumAngle, Minimum.ToString(NumberFormat), Brushes.DarkGray, this.Font);
            WriteText(g, Radius - 30, MaximumAngle, Maximum.ToString(NumberFormat), Brushes.DarkGray, this.Font);
        }

        /// <summary>
        /// Draw a band on our display
        /// </summary>
        /// <param name="g"></param>
        /// <param name="MaxValue"></param>
        /// <param name="MinValue"></param>
        /// <param name="DrawBrush"></param>
        private void DrawBand(Graphics g, double MinValue, double MaxValue, Brush DrawBrush)
        {
            double MinAngle = ((MaximumAngle - MinimumAngle) * (MinValue - Minimum) / (Maximum - Minimum)) + MinimumAngle;
            double MaxAngle = ((MaximumAngle - MinimumAngle) * (MaxValue - Minimum) / (Maximum - Minimum)) + MinimumAngle;

            //Draw our boundary
            float Radius = Bounds.Width / 2f;
            List<PointF> AnglePoints = new List<PointF>();
            AnglePoints.Add(Offset(Radius - 3, MinAngle));
            for (double Angle = MinAngle; Angle <= MaxAngle; Angle += 5)
                AnglePoints.Add(Offset(Radius - 15, Angle));
            AnglePoints.Add(Offset(Radius - 15, MaxAngle));
            for (double Angle = MaxAngle; Angle >= MinAngle; Angle -= 5)
                AnglePoints.Add(Offset(Radius - 3, Angle));
            g.FillPolygon(DrawBrush, AnglePoints.ToArray());
        }
        #endregion

        #region Gauge rending
        /// <summary>
        /// Paint the gauge image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gauge_Paint(object sender, PaintEventArgs e)
        {
            Gauge_Paint(e);
        }

        /// <summary>
        /// Paint the gauge image
        /// </summary>
        /// <param name="e"></param>
        private void Gauge_Paint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;

                Rectangle Bounds = new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
                PointF Center = new PointF(Bounds.Width / 2f, Bounds.Height / 2f);

                float Radius = Bounds.Width / 2f;
                double AngularDifference = MinimumAngle;
                Pen DrawPen;
                if (Current < Minimum || Current >= Maximum)
                    DrawPen = Pens.Red;
                else if (GoodRangeMaximum.HasValue && GoodRangeMinimum.HasValue && Current >= GoodRangeMinimum.Value && Current <= GoodRangeMaximum.Value)
                    DrawPen = Pens.LightGreen;
                else if (WarningRangeMaximum.HasValue && WarningRangeMinimum.HasValue && Current >= WarningRangeMinimum.Value && Current <= WarningRangeMaximum.Value)
                    DrawPen = Pens.Yellow;
                else if (ErrorRangeMaximum.HasValue && ErrorRangeMinimum.HasValue && Current >= ErrorRangeMinimum.Value && Current <= ErrorRangeMaximum.Value)
                    DrawPen = Pens.Red;
                else
                    DrawPen = Pens.Aqua;
                if (Current > Minimum && Current <= Maximum)
                    AngularDifference = ((MaximumAngle - MinimumAngle) * (Current - Minimum) / (Maximum - Minimum)) + MinimumAngle;

                //If we have an angular difference, draw it
                if (!double.IsNaN(AngularDifference))
                {
                    e.Graphics.DrawLine(DrawPen, Center, Offset(Radius - 20, AngularDifference));
                    e.Graphics.FillPolygon(DrawPen.Brush, new PointF[] { Offset(Radius - 20, AngularDifference), Offset(Radius - 26, AngularDifference - 7f), Offset(Radius - 26, AngularDifference + 7f) });
                    e.Graphics.FillEllipse(DrawPen.Brush, Center.X - 5, Center.Y - 5, 10, 10);

                    if (AngularDifference > MaximumAngle)
                        e.Graphics.DrawLine(DrawPen, Center, Offset(Radius - 12, MaximumAngle));
                    if (AngularDifference < MinimumAngle)
                        e.Graphics.DrawLine(DrawPen, Center, Offset(Radius - 12, MinimumAngle));
                }

                using (Font DrawFont = new Font("Arial", 10, FontStyle.Bold))
                    WriteText(g, Radius - 12, -180, Current.ToString(NumberFormat), DrawPen.Brush, DrawFont);
            }
            catch { }
        }

        /// <summary>
        /// Write a text label
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Radius"></param>
        /// <param name="Angle"></param>
        /// <param name="Text"></param>
        /// <param name="DrawBrush"></param>
        /// <param name="DrawFont"></param>
        private void WriteText(Graphics g, float Radius, float Angle, String Text, Brush DrawBrush, Font DrawFont)
        {
            using (StringFormat sF = new StringFormat())
            {
                sF.LineAlignment = sF.Alignment = StringAlignment.Center;
                g.DrawString(Text, DrawFont, DrawBrush, Offset(Radius, Angle), sF);
            }
        }

        /// <summary>
        /// Offset a point against angle/radius
        /// </summary>
        /// <param name="Radius"></param>
        /// <param name="Angle"></param>
        /// <returns></returns>
        private PointF Offset(float Radius, double Angle)
        {
            return new PointF(
                (Radius * -(float)Math.Cos(Math.PI * (Angle + 90) / 180f)) + (ClientRectangle.Width / 2f),
                (Radius * -(float)Math.Sin(Math.PI * (Angle + 90) / 180f)) + (ClientRectangle.Height / 2f));
        }
        #endregion
    }
}
