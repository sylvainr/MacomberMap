using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using System.Drawing.Drawing2D;
using System.Xml;
using Macomber_Map.Data_Connections.Generic;


namespace Macomber_Map.User_Interface_Components.OneLines
{
    /// <summary>
    /// This class presnts a synchroscope form 
    /// </summary>
    public partial class MM_OneLine_Synchroscope : UserControl
    {
        #region Variable declarations
        /// <summary>Number of time slots</summary>
        private const int TMax = 2;
        private float[] FreqI = new float[TMax];
        private float[] FreqZ = new float[TMax];
        private float[] AngleI = new float[TMax];
        private float[] AngleZ = new float[TMax];
        private float[] PhaseI = new float[TMax];
        private float[] PhaseZ = new float[TMax];
        private float[] FreqGCP = new float[TMax];
        private float[] PhaseGCP = new float[TMax];
        private float FrequencyDifferential;
        private float[] PhaseDifferential = new float[TMax];
        private float GCPFrequency=0;
        private int Tmr, _Tmr;

        
        /// <summary>The state of our synchroscope</summary>        
        /// <remarks>
        /// 1 - Same Island
        /// 2 - Different Island
        /// 3 - Data Missing (Near)
        /// 4 - Data Missing (Far)
        /// 5 - Data Missing (Both)
        /// 6 - Unit Offline, GCP Near Side
        /// 7 - Unit Offline, GCP Far Side
        /// </remarks>
        private int State;

        /// <summary>The control panel associated with our display</summary>
        public MM_ControlPanel ControlPanel;
        
        /// <summary>The element associated with the control panel</summary>
        public MM_OneLine_Element SourceElement;

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
            /// <summary>The unit on the near node side is offline</summary>
            NearUnitOffline = 6,
            /// <summary>The unit on the far node side is offline</summary>
            FarUnitOffline = 7
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new synchroscope displays
        /// </summary>
        /// <param name="Breaker">The breaker to be displayed</param>
        /// <param name="SourceApplication"></param>
        /// <param name="xConfig"></param>
        /// <param name="DataSource"></param>
        /// <param name="ControlPanel"></param>
        public MM_OneLine_Synchroscope(MM_OneLine_Element Breaker, XmlElement xConfig, String SourceApplication, MM_Data_Source DataSource, MM_ControlPanel ControlPanel)            
        {
            InitializeComponent();
            MM_Serializable.ReadXml(xConfig, this, false);
            Synchroscope.Paint += new PaintEventHandler(Synchroscope_Paint);
            this.SourceElement = Breaker;
            this.lblApplication.Text = SourceApplication;
            this.ControlPanel = ControlPanel;
        }

        #endregion

        #region Synchroscope rending
        /// <summary>
        /// Paint the synchroscope
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Synchroscope_Paint(object sender, PaintEventArgs e)
        {
            float MinSynch = ControlPanel.GetSingle("MinSynch");
            float MaxSynch = ControlPanel.GetSingle("MaxSynch");

            Rectangle Bounds = new Rectangle(0, 0, Synchroscope.ClientRectangle.Width - 1, Synchroscope.ClientRectangle.Height - 1);
            PointF Center = new PointF(Bounds.Width / 2f, Bounds.Height / 2f);
            using (SolidBrush BackBrush = new SolidBrush(Color.FromArgb(16, 16, 16)))
                e.Graphics.FillEllipse(BackBrush, Bounds);

            if (!float.IsNaN(MaxSynch) && !float.IsNaN(MinSynch))
                using (SolidBrush BackBrush = new SolidBrush(Color.FromArgb(64, 64, 64)))
                    e.Graphics.FillPie(BackBrush, Bounds, MaxSynch - 90, MinSynch - MaxSynch);
            using (Pen DrawPen = new Pen(Color.Gray, 3))
                e.Graphics.DrawEllipse(DrawPen, Bounds);
            using (Pen DrawPen = new Pen(Color.White, 3))
                e.Graphics.DrawEllipse(DrawPen, 2, 2, Bounds.Width - 4, Bounds.Height - 4);

            float Radius = Bounds.Width / 2f;
            for (int Angle = 0; Angle < 360; Angle += 5)
                e.Graphics.DrawLine(Pens.Gray, Offset(Radius - 3, Angle), Offset(Radius - (Angle % 15 == 0 ? 15 : 10), Angle));

            if (!float.IsNaN(MaxSynch))
                WriteText(e.Graphics, Radius - 30, MaxSynch, (MaxSynch > 0 ? "+" : "") + MaxSynch.ToString());
            if (!float.IsNaN(MinSynch))
                WriteText(e.Graphics, Radius - 30, MinSynch, (MinSynch > 0 ? "+" : "") + MinSynch.ToString());
            WriteText(e.Graphics, Radius - 30, 0, "0");
            WriteText(e.Graphics, Radius - 30, -90, "-90");
            WriteText(e.Graphics, Radius - 30, 90, "+90");
            WriteText(e.Graphics, Radius - 30, 180, "180");
            WriteText(e.Graphics, Radius - 50, -50, "Slow");
            WriteText(e.Graphics, Radius - 50, 50, "Fast");

            //Draw our arrows to the 'slow' and 'fast' words
            foreach (float Mult in new float[] { 1, -1 })
            {
                List<PointF> Points = new List<PointF>();
                for (float a = Mult * 10; a != Mult * 45; a += Mult * 0.5f)
                    Points.Add(Offset(Radius - 40, a));
                e.Graphics.DrawCurve(Pens.LightGray, Points.ToArray());
                e.Graphics.DrawLine(Pens.LightGray, Offset(Radius - 40, Mult * 45), Offset(Radius - 35, Mult * 37.5f));
                e.Graphics.DrawLine(Pens.LightGray, Offset(Radius - 40, Mult * 45), Offset(Radius - 45, Mult * 37.5f));
            }

            //Draw our scope angle
            float NearAngle = ControlPanel.GetSingle("NearAngle");
            float FarAngle = ControlPanel.GetSingle("FarAngle");
            float AngularDifference;
            if (State == 2)
                AngularDifference = PhaseI[Tmr] -  PhaseZ[Tmr];
            else
                AngularDifference = NearAngle- FarAngle;

            


            if (!float.IsNaN(AngularDifference))
            {
                while (AngularDifference >= 360)
                    AngularDifference -= 360;
                while (AngularDifference < 0)
                    AngularDifference += 360;
                float aRad = (AngularDifference * (float)Math.PI / 180f) + (1.5f * (float)Math.PI);
                e.Graphics.DrawLine(Pens.Aqua, Center, Offset(Radius - 12, AngularDifference));
                e.Graphics.DrawLine(Pens.Aqua, Offset(Radius - 12, AngularDifference), Offset(Radius - 18, AngularDifference - 3f));
                e.Graphics.DrawLine(Pens.Aqua, Offset(Radius - 12, AngularDifference), Offset(Radius - 18, AngularDifference + 3f));
                e.Graphics.FillEllipse(Brushes.Aqua, Center.X - 5, Center.Y - 5, 10, 10);


                //Draw our Synch lamp
                int z = Math.Abs(((int)AngularDifference - 180) * 255 / 180);
                PointF CenterSync = Offset(Radius + 10, -135);
                using (SolidBrush DrawBrush = new SolidBrush(Color.FromArgb(255 - z, 255 - z, 0)))
                    e.Graphics.FillEllipse(DrawBrush, CenterSync.X - 14, CenterSync.Y - 14, 28, 28);
                e.Graphics.DrawEllipse(Pens.White, CenterSync.X - 14, CenterSync.Y - 14, 28, 28);
            }
        }

        /// <summary>
        /// Offset a point against angle/radius
        /// </summary>
        /// <param name="Radius"></param>
        /// <param name="Angle"></param>
        /// <returns></returns>
        private PointF Offset(float Radius, float Angle)
        {
            return new PointF(
                (Radius * -(float)Math.Cos(Math.PI * (Angle + 90) / 180f)) + (Synchroscope.ClientRectangle.Width / 2f),
                (Radius * -(float)Math.Sin(Math.PI * (Angle + 90) / 180f)) + (Synchroscope.ClientRectangle.Height / 2f));

        }

        /// <summary>
        /// Calculate the phase angle based on the current phase, frequency and dt
        /// </summary>
        /// <param name="Phase"></param>
        /// <param name="Frequency"></param>
        /// <returns></returns>
        private float CalculatePhase(float Phase, float Frequency)
        {
            float _Phase = Modd(Phase + (360f * (Frequency - 60f) * ((float)tmrUpdate.Interval / 1000f)), 360f);
            while (_Phase < 0)
                _Phase += 360;
            return _Phase;
        }

        /// <summary>
        /// Perform the mod function while allowing for decimal values
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private float Modd(float a, float b)
        {
            return a - (b * ((int)(a/b)));
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

            MM_KVLevel KVLevel = SourceElement.BaseElement.KVLevel;

            //Determine if we're violated, and if so, change to our worst violation
            using (Pen DrawPen = new Pen(KVLevel.Energized.ForeColor))
            using (Brush BackBrush = new SolidBrush(LabelCB.BackColor))
            using (Font DrawFont = new Font("Arial", 13, FontStyle.Bold))
            {
                Point Center = new Point(LabelCB.ClientRectangle.Width / 2, LabelCB.ClientRectangle.Height - 12);
                Rectangle DrawRect = new Rectangle(Center.X - 10, Center.Y - 10, 20, 20);

                bool Opened = ControlPanel.GetBoolean("Opened");
                Pen NearPen = new Pen(KVLevel.Energized.ForeColor, 3);
                Pen FarPen = new Pen(KVLevel.Energized.ForeColor, 3);
                if (Opened)
                {
                    e.Graphics.DrawRectangle(DrawPen, DrawRect);
                    e.Graphics.DrawString("O", DrawFont, DrawPen.Brush, DrawRect, MM_OneLine_Element.CenterFormat);
                }
                else
                {
                    e.Graphics.FillRectangle(DrawPen.Brush, DrawRect);
                    e.Graphics.DrawString("C", DrawFont, BackBrush, DrawRect, MM_OneLine_Element.CenterFormat);
                }


                if (State == 3 || State == 5)
                {
                    NearPen.Color = Color.Gray;
                    NearPen.Width = 1;
                    NearPen.DashStyle = DashStyle.Dash;
                }
                if (State == 4 || State == 5)
                {
                    FarPen.Color = Color.Gray;
                    FarPen.Width = 1;
                    FarPen.DashStyle = DashStyle.Dash;
                }
                e.Graphics.DrawLine(NearPen, 0, Center.Y, DrawRect.Left, Center.Y);
                e.Graphics.DrawLine(FarPen, DrawRect.Right, Center.Y, LabelCB.ClientRectangle.Width, Center.Y);
                NearPen.Dispose();
                FarPen.Dispose();

                e.Graphics.DrawString(ControlPanel.GetString("NearNodeName"), this.Font, Brushes.White, Point.Empty);
                using (StringFormat sF = new StringFormat())
                {
                    sF.Alignment = StringAlignment.Far;
                    e.Graphics.DrawString(ControlPanel.GetString("FarNodeName"), this.Font, Brushes.White, LabelCB.Width, 0, sF);
                }

            }
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
            _Tmr = Tmr;
            Tmr = (Tmr + 1) % TMax;
            FreqI[Tmr] = ControlPanel.GetSingle("NearFrequency");
            FreqZ[Tmr] = ControlPanel.GetSingle("FarFrequency");
            //FreqGCP[Tmr] = GCPFrequency;
            //PhaseGCP[Tmr] = CalculatePhase(PhaseGCP[_Tmr], FreqGCP[_Tmr]);
            AngleI[Tmr] = ControlPanel.GetSingle("NearAngle");
            if (float.IsNaN(AngleI[Tmr]))
                AngleI[Tmr] = 0;
            AngleZ[Tmr] = ControlPanel.GetSingle("FarAngle");
            if (float.IsNaN(AngleZ[Tmr]))
                AngleZ[Tmr] = 0;
            PhaseI[Tmr] = CalculatePhase(PhaseI[_Tmr], FreqI[_Tmr]);
            PhaseZ[Tmr] = CalculatePhase(PhaseZ[_Tmr], FreqZ[_Tmr]);
            PhaseDifferential[Tmr] = PhaseI[Tmr] - PhaseZ[Tmr];
            Synchroscope.Refresh();
        }

     

      
        #endregion

        /// <summary>
        /// When the data source changes, refresh.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private delegate void SafeDataUpdated();

        /// <summary>
        /// Refresh our data, update our synchroscope. This carries out the 'Sync' function in the original EMS display
        /// </summary>
        public void DataUpdated()
        {
            if (InvokeRequired)
                Invoke(new SafeDataUpdated(DataUpdated));
            else
            {
                //Start the timer if needed
                if (!tmrUpdate.Enabled)
                    tmrUpdate.Enabled = true;               

                //Initialize our internal handling data
                FreqI[0] = ControlPanel.GetSingle("NearFrequency");
                FreqZ[0] = ControlPanel.GetSingle("FarFrequency");
                FreqGCP[0] = GCPFrequency;
                FrequencyDifferential = FreqI[0] - FreqZ[0];
                PhaseGCP[0] = 0;
                AngleI[0] = PhaseI[0] = ControlPanel.GetSingle("NearAngle");
                AngleZ[0] = PhaseZ[0] = ControlPanel.GetSingle("FarAngle");
                PhaseDifferential[0] = PhaseI[0] - PhaseZ[0];

                //Determine the state of our system
                if (float.IsNaN(FreqI[0]) && float.IsNaN(FreqZ[0]))
                    State = 5;
                else if (float.IsNaN(FreqI[0]))
                    State = 3;
                else if (float.IsNaN(FreqZ[0]))
                    State = 4;
                else if (ControlPanel.GetInt("NearIsland") == ControlPanel.GetInt("FarIsland"))
                    State = 1;
                else 
                    State = 2;

                //Update the labels
                LabelStation.Text = ControlPanel.GetString("Substation");
                LabelCBID.Text = ControlPanel.GetString("Name");

                if (State == 3 || State==5)
                {
                    LabelFreqI.Text = "--";
                    LabelKVI.Text = "--";
                }
                else
                {
                    LabelFreqI.Text = ControlPanel.GetSingle("NearFrequency").ToString("0.000");
                    LabelKVI.Text = ControlPanel.GetSingle("NearVoltage").ToString("0.00");
                }


                if (State == 4 || State == 5)
                {
                    LabelFreqZ.Text = "--";
                    LabelKVZ.Text = "--";
                }
                else
                {
                    LabelFreqZ.Text = ControlPanel.GetSingle("FarFrequency").ToString("0.000");
                    LabelKVZ.Text = ControlPanel.GetSingle("FarVoltage").ToString("0.00");
                }

                if (State == 1)
                    LabelPhaseDiff.Text = (ControlPanel.GetSingle("NearAngle") - ControlPanel.GetSingle("FarAngle")).ToString("0.0°");
                else
                    LabelPhaseDiff.Text = "--";


                LabelStation.Text = ControlPanel.GetString("Substation");
                LabelCBID.Text = ControlPanel.GetString("Name");
                Synchroscope.Refresh();
                LabelCB.Refresh();
            }
        }
    }
}