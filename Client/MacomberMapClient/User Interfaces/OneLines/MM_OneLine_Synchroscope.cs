using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.OneLines
{
    /// <summary>
    /// This class provides the synchroscope display
    /// </summary>
    public partial class MM_OneLine_Synchroscope : UserControl
    {
        #region Variable declarations
        /// <summary>Number of time slots</summary>
        private const int TMax = 4;
        private int Tmr, _Tmr;
        private float[] FreqI = new float[TMax];
        private float[] FreqZ = new float[TMax];
        private float[] VoltI = new float[TMax];
        private float[] VoltZ = new float[TMax];
        private float[] PhaseI = new float[TMax];
        private float[] PhaseZ = new float[TMax];
        public  float[] PhaseDifferential = new float[TMax];

        /// <summary>The state of our synchroscope</summary>
        private enumSynchroscopeState State = enumSynchroscopeState.Unknown;
        
        /// <summary>The element associated with the control panel</summary>
        public MM_Breaker_Switch BreakerSwitch;

        /// <summary>The near node, for naming purposes</summary>
        public MM_Node NearNode;

        /// <summary>The far node, for naming purposes</summary>
        public MM_Node FarNode;

        /// <summary>Our generator bus</summary>
        public MM_Bus GeneratorBus = null;

        /// <summary>Our generator island</summary>
        public MM_Island GeneratorIsland = null;
        #endregion

        #region Enumerations
        /// <summary>The collection of states of the synchroscope</summary>
        public enum enumSynchroscopeState
        {
            /// <summary>State is not yet known/initialized</summary>
            Unknown = 0,
            /// <summary>The nodes of the breaker are on the same island</summary>
            SameIsland = 1,
            /// <summary>The nodes of the breaker are on different islands</summary>
            DifferentIsland = 2,
            /// <summary>Data is missing from the near node side</summary>
            DataMissingNear = 3,
            /// <summary>Data is missing from the far node side</summary>
            DataMissingFar = 4,
            /// <summary>Data is missing from the both sides</summary>
            DataMissingBoth = 5,
            /// <summary>The unit breaker going from open to close</summary>
            BreakerOpenToClose = 6,
            /// <summary>The unit breaker going from close to open</summary>
            BreakerCloseToOpen = 7
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new synchroscope
        /// </summary>
        public MM_OneLine_Synchroscope()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <summary>
        /// Initialize a new synchroscope display
        /// </summary>
        /// <param name="BreakerSwitch"></param>
        /// <param name="NearNode"></param>
        /// <param name="FarNode"></param>
        public MM_OneLine_Synchroscope(MM_Breaker_Switch BreakerSwitch, MM_Node NearNode, MM_Node FarNode): this()
        {
            AssignSynchroscope(BreakerSwitch, NearNode, FarNode);
        }

        /// <summary>
        /// Assign our synchroscope
        /// </summary>
        /// <param name="BreakerSwitch"></param>
        /// <param name="NearNode"></param>
        /// <param name="FarNode"></param>
        public void AssignSynchroscope(MM_Breaker_Switch BreakerSwitch, MM_Node NearNode, MM_Node FarNode)
        {
            this.BreakerSwitch=BreakerSwitch;
            this.NearNode = NearNode;
            this.FarNode = FarNode;
        }

        /// <summary>
        /// Assign our synchroscope
        /// </summary>
        /// <param name="GeneratorBus"></param>
        /// <param name="GeneratorIsland"></param>
        public void AssignSynchroscope(MM_Bus GeneratorBus, MM_Island GeneratorIsland)
        {
            this.GeneratorBus = GeneratorBus;
            this.GeneratorIsland = GeneratorIsland;
        }

        /// <summary>
        /// Assign our synchroscope
        /// </summary>
        /// <param name="GeneratorBus"></param>
        /// <param name="GeneratorIsland"></param>
        /// <param name="TimeReference"></param>
        public void AssignSynchroscope(MM_Bus GeneratorBus, MM_Island GeneratorIsland, DateTime TimeReference)
        {
            AssignSynchroscope(GeneratorBus, GeneratorIsland);
        }
        #endregion

        #region Synchroscope rendering
        /// <summary>
        /// Draw our synchroscope
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Rect"></param>
        /// <param name="AngularDifference"></param>
        public static void DrawSynchroscope(Graphics g, Rectangle Rect, double AngularDifference)
        {
            g.DrawImage(Resources.SynchroscopeHiRes, Rect);
            PointF Center = new PointF(Rect.Left + 1 + (Rect.Width / 2), Rect.Top + (Rect.Height / 2) - 2);
            double Radius = Rect.Width * 0.48;
            if (!double.IsNaN(AngularDifference))
            {
                while (AngularDifference >= 360)
                    AngularDifference -= 360;
                while (AngularDifference < 0)
                    AngularDifference += 360;

                g.FillPolygon(Brushes.Black, new PointF[] { 
                    Offset(10, AngularDifference-45, Rect),
                    Offset(Radius-12,AngularDifference,Rect),
                    Offset(10,AngularDifference+45,Rect),
                    Offset(10,AngularDifference,Rect)
                });

                //Draw our Synch lamp
                double SinAmplitude = AngularDifference < 270 && AngularDifference > 90 ? Math.Abs(Math.Sin((180 + AngularDifference) * Math.PI / 180.0)) : 1;
                int z = (int)(SinAmplitude * 255);
                Rectangle SyncRectangle = new Rectangle(Rect.Left, Rect.Bottom - 26, 25, 25);
                using (SolidBrush DrawBrush = new SolidBrush(Color.FromArgb(255 - z, 255 - z, 0)))
                    g.FillEllipse(DrawBrush, SyncRectangle);
                g.DrawEllipse(Pens.Gray, SyncRectangle);
            }
        }

        /// <summary>
        /// Paint the synchroscope
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Synchroscope_Paint(object sender, PaintEventArgs e)
        {
            DrawSynchroscope(e.Graphics, Synchroscope.DisplayRectangle, PhaseDifferential[Tmr]);
        }

        /// <summary>
        /// Offset a point against angle/radius
        /// </summary>
        /// <param name="Radius"></param>
        /// <param name="Angle"></param>
        private PointF Offset(float Radius, float Angle)
        {
            return new PointF(
                (Radius * -(float)Math.Cos(Math.PI * (Angle + 90) / 180f)) + (Synchroscope.ClientRectangle.Width / 2f),
                (Radius * -(float)Math.Sin(Math.PI * (Angle + 90) / 180f)) + (Synchroscope.ClientRectangle.Height / 2f));
        }

        /// <summary>
        /// Offset a point against angle/radius
        /// </summary>
        /// <param name="Radius"></param>
        /// <param name="Angle"></param>
        /// <param name="Rect"></param>
        private static PointF Offset(double Radius, double Angle, Rectangle Rect)
        {
            return new PointF(
                (float)(Radius * -Math.Cos(Math.PI * (Angle + 90) / 180f)) + (Rect.Width / 2f),
                (float)(Radius * -Math.Sin(Math.PI * (Angle + 90) / 180f)) + (Rect.Height / 2f));
        }

        /// <summary>
        /// Calculate the phase angle based on the current phase, frequency and dt
        /// </summary>
        /// <param name="Phase"></param>
        /// <param name="Frequency"></param>
        private float CalculatePhase(float Phase, float Frequency)
        {
            float _Phase = Modd(Phase + (360f * (Frequency - 60f) * ((float)tmrUpdate.Interval / 1000f)), 360f);
            if (float.IsNaN(_Phase))
                return 0;
            while (_Phase < 0)
                _Phase += 360;
            return _Phase;
        }

        /// <summary>
        /// Calculate the phase angle based on the current phase, frequency and dt
        /// </summary>
        /// <param name="Phase"></param>
        /// <param name="Frequency"></param>
        /// <param name="UpdateInterval"></param>
        public static double CalculatePhase(double Phase, double Frequency, double UpdateInterval)
        {
            double _Phase = Modd(Phase + (360.0 * (Frequency - 60.0) * (UpdateInterval / 1000.0)), 360.0);
            while (_Phase < 0.0)
                _Phase += 360.0;
            return _Phase;
        }

        /// <summary>
        /// Perform the mod function while allowing for decimal values
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private static float Modd(float a, float b)
        {
            return a - (b * ((int)(a/b)));
        }

        /// <summary>
        /// Perform the mod function while allowing for decimal values
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private static double Modd(double a, double b)
        {
            return a - (b * ((int)(a / b)));
        }

        /// <summary>
        /// Write a text label
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Radius"></param>
        /// <param name="Angle"></param>
        /// <param name="Text"></param>
        private void WriteText(Graphics g, float Radius, float Angle, String Text)
        {
            using (StringFormat sF = new StringFormat())
            {
                sF.LineAlignment = sF.Alignment = StringAlignment.Center;
                g.DrawString(Text, this.Font, Brushes.White, Offset(Radius, Angle), sF);
            }
        }
        #endregion

        #region Label/breaker image rendering
        /// <summary>
        /// Handle the painting of the breaker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelCB_Paint(object sender, PaintEventArgs e)
        {
            if (BreakerSwitch == null)
                return;

            MM_KVLevel KVLevel = BreakerSwitch.KVLevel;

            //Draw our breaker
            using (Pen DrawPen = new Pen(KVLevel.Energized.ForeColor))
            using (Brush BackBrush = new SolidBrush(LabelCB.BackColor))
            using (Font DrawFont = new Font("Arial", 13, FontStyle.Bold))
            {
                Point Center = new Point(LabelCB.ClientRectangle.Width / 2, LabelCB.ClientRectangle.Height - 12);
                Rectangle DrawRect = new Rectangle(Center.X - 10, Center.Y - 10, 20, 20);

                bool Opened = BreakerSwitch.Open;
                Pen NearPen = new Pen(KVLevel.Energized.ForeColor, 3);
                Pen FarPen = new Pen(KVLevel.Energized.ForeColor, 3);
                if (Opened)
                {
                    e.Graphics.DrawRectangle(DrawPen, DrawRect);
                    e.Graphics.DrawString("O", DrawFont, DrawPen.Brush, DrawRect, MM_OneLine_Element.CenterFormat);
                }
                else if (BreakerSwitch.BreakerState == MM_Breaker_Switch.BreakerStateEnum.Closed)
                {
                    e.Graphics.FillRectangle(DrawPen.Brush, DrawRect);
                    e.Graphics.DrawString("C", DrawFont, BackBrush, DrawRect, MM_OneLine_Element.CenterFormat);
                }
                else
                {
                    e.Graphics.DrawRectangle(DrawPen, DrawRect);
                    e.Graphics.DrawString("?", DrawFont, DrawPen.Brush, DrawRect, MM_OneLine_Element.CenterFormat);
                }

                if (State == enumSynchroscopeState.DataMissingNear || State ==  enumSynchroscopeState.DataMissingBoth)
                {
                    NearPen.Color = Color.Gray;
                    NearPen.Width = 1;
                    NearPen.DashStyle = DashStyle.Dash;
                }

                if (State == enumSynchroscopeState.DataMissingFar || State == enumSynchroscopeState.DataMissingBoth)
                {
                    FarPen.Color = Color.Gray;
                    FarPen.Width = 1;
                    FarPen.DashStyle = DashStyle.Dash;
                }
                e.Graphics.DrawLine(NearPen, 0, Center.Y, DrawRect.Left, Center.Y);
                e.Graphics.DrawLine(FarPen, DrawRect.Right, Center.Y, LabelCB.ClientRectangle.Width, Center.Y);
                NearPen.Dispose();
                FarPen.Dispose();

                e.Graphics.DrawString(NearNode.Name, this.Font, Brushes.White, Point.Empty);
                using (StringFormat sF = new StringFormat())
                {
                    sF.Alignment = StringAlignment.Far;
                    e.Graphics.DrawString(FarNode.Name, this.Font, Brushes.White, LabelCB.Width, 0, sF);
                }
            }
        }
        #endregion

        #region Waveform generation
        /// <summary>
        /// Draw the waveform of what our synchroscope is seeing
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Rect"></param>
        public void DrawWaveform(Graphics g, Rectangle Rect)
        {
            g.Clear(Color.Black);

            //Create our image to display our waveform            
            using (Bitmap DrawBitmap = new Bitmap(Rect.Width, Rect.Height, PixelFormat.Format24bppRgb))
            using (Font DrawFont = new Font("Arial", 7))
                try
                {
                    //Determine our references
                    double FrequencyReference = 30.0;
                    double VoltageReference = Math.Max(VoltI[Tmr], VoltZ[Tmr]);

                    BitmapData BitmapData = DrawBitmap.LockBits(new Rectangle(0, 0, Rect.Width, Rect.Height), ImageLockMode.WriteOnly, DrawBitmap.PixelFormat);
                    int FontSize = (int)g.MeasureString("+" + VoltageReference.ToString("0.0"), DrawFont).Width;
                    Rectangle RenderRect = new Rectangle(Rect.Left + FontSize, Rect.Top, Rect.Width - FontSize, Rect.Height - 5);
                    //Let's start with a 60Hz reference. We're going to show two waveforms
                    double X1Mult = 2 * (FreqI[Tmr] / FrequencyReference) * Math.PI / RenderRect.Width;
                    double X1Add = (180.0 + PhaseI[Tmr]) * Math.PI / 180.0;
                    double Y1Mult = VoltI[Tmr] / VoltageReference;
                    double Y2Mult = VoltZ[Tmr] / VoltageReference;
                    double X2Mult = 2 * (FreqZ[Tmr] / FrequencyReference) * Math.PI / RenderRect.Width;
                    double X2Add = (180.0 + PhaseZ[Tmr]) * Math.PI / 180.0;

                    for (double x = 0; x < RenderRect.Width; x += .125)
                        unsafe
                        {
                            int y1 = (int)(((Math.Sin(x * X1Mult + X1Add) * Y1Mult) + 1.0) * (RenderRect.Height - 1) / 2.0) + RenderRect.Top;
                            byte* Row1 = (byte*)BitmapData.Scan0 + ((int)y1 * BitmapData.Stride);
                            Row1[(int)(x + RenderRect.Left) * 3 + 2] = 255;

                            int y2 = (int)(((Math.Sin(x * X2Mult + X2Add) * Y2Mult) + 1.0) * (RenderRect.Height - 1) / 2.0) + RenderRect.Top;
                            byte* Row2 = (byte*)BitmapData.Scan0 + ((int)y2 * BitmapData.Stride);
                            Row2[(int)(x + RenderRect.Left) * 3 + 1] = 255;
                        }

                    DrawBitmap.UnlockBits(BitmapData);
                    g.DrawImageUnscaled(DrawBitmap, Point.Empty);
                    g.DrawLine(Pens.DarkGray, RenderRect.Left, RenderRect.Top, RenderRect.Left, RenderRect.Bottom);
                    int MiddleRender = RenderRect.Top + (RenderRect.Height / 2);
                    g.DrawLine(Pens.DarkGray, RenderRect.Left, MiddleRender, RenderRect.Right, MiddleRender);
                    g.DrawString("+" + VoltageReference.ToString("0.0"), DrawFont, Brushes.White, PointF.Empty);
                    g.DrawString("0", DrawFont, Brushes.White, 0, RenderRect.Top + (RenderRect.Height - DrawFont.Height) / 2);
                    g.DrawString("-" + VoltageReference.ToString("0.0"), DrawFont, Brushes.White, 0, RenderRect.Bottom - DrawFont.Height);
                }
                catch { }
        }
        #endregion

        #region Updating
        /// <summary>
        /// Handle the timer tick   
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (BreakerSwitch == null)
                return;
            try
            {
                _Tmr = Tmr;
                Tmr = (Tmr + 1) % TMax;

                //If we have a generator bus, let's use that
                if (GeneratorBus != null)
                {
                    FreqI[Tmr] = GeneratorIsland.Frequency;
                    VoltI[Tmr] = GeneratorBus.Estimated_kV;
                }
                else
                {
                    FreqI[Tmr] = BreakerSwitch.NearIsland == null ? 60 : BreakerSwitch.NearIsland.Frequency;
                    VoltI[Tmr] = BreakerSwitch.NearBus == null ? 0 : BreakerSwitch.NearBus.Estimated_kV;
                }

                if (BreakerSwitch.FarBus != null && BreakerSwitch.FarIsland != null)
                {
                    FreqZ[Tmr] = BreakerSwitch.FarIsland == null ? 60 : BreakerSwitch.FarIsland.Frequency;
                    VoltZ[Tmr] = BreakerSwitch.FarBus == null ? 0 : BreakerSwitch.FarBus.Estimated_kV;
                }
                else
                {
                    FreqZ[Tmr] = BreakerSwitch.NearIsland == null ? 60 : BreakerSwitch.NearIsland.Frequency;
                    VoltZ[Tmr] = BreakerSwitch.NearBus == null ? 0 : BreakerSwitch.NearBus.Estimated_kV;
                }

                PhaseI[Tmr] = CalculatePhase(PhaseI[_Tmr], FreqI[_Tmr]);
                PhaseZ[Tmr] = CalculatePhase(PhaseZ[_Tmr], FreqZ[_Tmr]);

                if (State == enumSynchroscopeState.SameIsland)
                {
                    if (PhaseZ[Tmr] != PhaseI[Tmr])
                        PhaseZ[Tmr] = PhaseI[Tmr];
                }

                if (VoltI[Tmr] == 0 | VoltZ[Tmr] == 0 || FreqI[Tmr] < 0 || FreqZ[Tmr] < 0)
                    PhaseDifferential[Tmr] = float.NaN;
                else if (FreqI[Tmr] < 57 || FreqZ[Tmr] < 57 || State == enumSynchroscopeState.BreakerOpenToClose)
                    PhaseDifferential[Tmr] = 0;
                else
                    PhaseDifferential[Tmr] = PhaseI[Tmr] - PhaseZ[Tmr];

                Synchroscope.Refresh();
            }
            catch { }
        }
        #endregion

        #region Double-buffered panel to improve synchroscope rendering
        /// <summary>
        /// This class provides a synchroscope panel with built-in double buffering
        /// </summary>
        internal class DoubleBufferedPanel : Panel
        {
            /// <summary>
            /// Initialiize a double-buffered panel
            /// </summary>
            public DoubleBufferedPanel()
            {
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            }
        }
        #endregion

        /// <summary>
        /// Every second, update our labels
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdateLabels_Tick(object sender, EventArgs e)
        {
            try
            {
                //Start the timer if needed
                if (!tmrUpdate.Enabled)
                    tmrUpdate.Enabled = true;
                if (BreakerSwitch == null)
                    return;
                MM_Bus NearBus = BreakerSwitch.NearBus;
                MM_Bus FarBus = BreakerSwitch.FarBus;

                //Determine the state of our system
                if (float.IsNaN(FreqI[0]) && float.IsNaN(FreqZ[0]))
                    State = enumSynchroscopeState.DataMissingBoth;
                else if (float.IsNaN(FreqI[0]) || FreqI[0] == 0)
                    State = enumSynchroscopeState.DataMissingNear;
                else if (float.IsNaN(FreqZ[0]) || FreqZ[0] == 0)
                    State = enumSynchroscopeState.DataMissingFar;
                else if (NearBus == null || FarBus == null)
                    State = enumSynchroscopeState.DifferentIsland;
                else if (NearBus != null && FarBus != null && NearBus.IslandNumber != FarBus.IslandNumber)
                    State = enumSynchroscopeState.DifferentIsland;
                else if (NearBus != null && FarBus != null && NearBus.IslandNumber == FarBus.IslandNumber)
                    State = (!BreakerSwitch.Open) ? enumSynchroscopeState.SameIsland : enumSynchroscopeState.BreakerOpenToClose;
                else
                    State = enumSynchroscopeState.Unknown;

                //Update the labels
                LabelStation.Text = BreakerSwitch.Substation.Name;
                LabelCBID.Text = BreakerSwitch.Name;

                if (State == enumSynchroscopeState.DataMissingNear || State == enumSynchroscopeState.DataMissingBoth)
                {
                    LabelFreqI.Text = "--";
                    LabelKVI.Text = "--";
                }
                else
                {
                    if (State == enumSynchroscopeState.DifferentIsland)
                    {
                        LabelFreqI.Text = FreqI[0].ToString("0.000°");
                        LabelKVI.Text = GeneratorBus != null ? GeneratorBus.Estimated_kV.ToString("0.00") : NearBus == null ? VoltI[Tmr].ToString("0.00") : NearBus.Estimated_kV.ToString("0.00");
                    }
                    else if (State == enumSynchroscopeState.SameIsland)
                    {
                        LabelFreqI.Text = FreqZ[0].ToString("0.000°");
                        LabelKVI.Text = (NearBus == null || NearBus.Estimated_kV <= 1) ? VoltI[Tmr].ToString("0.00") : NearBus.Estimated_kV.ToString("0.00");
                    }
                }

                if (State == enumSynchroscopeState.DataMissingFar || State == enumSynchroscopeState.DataMissingBoth)
                {
                    LabelFreqZ.Text = "--";
                    LabelKVZ.Text = "--";
                }
                else
                {
                    LabelFreqZ.Text = FreqZ[0].ToString("0.000°");
                    LabelKVZ.Text = (FarBus == null || FarBus.Estimated_kV <= 1) ? VoltZ[Tmr].ToString("0.00") : FarBus.Estimated_kV.ToString("0.00");
                }

                if (State == enumSynchroscopeState.SameIsland || State == enumSynchroscopeState.DifferentIsland)
                    LabelPhaseDiff.Text = (NearBus != null && FarBus != null) ? (NearBus.Island.Frequency - FarBus.Island.Frequency).ToString("0.000°") : (FreqI[0] - FreqZ[0]).ToString("0.000°");
                else
                    LabelPhaseDiff.Text = "--";

                LabelCB.Refresh();
            }
            catch { }
        }
    }
}
