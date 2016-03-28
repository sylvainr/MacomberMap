using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacomberMapClient.Data_Elements.Physical;
using System.Reflection;
using ZedGraph;
using MacomberMapClient.User_Interfaces.Menuing;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.Integration;
using System.Threading;

namespace MacomberMapClient.User_Interfaces.Blackstart
{
    /// <summary>
    /// © 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a load or gen tracking control, by operator
    /// </summary>
    public partial class MM_LoadGen_Tracking_Operator : UserControl
    {
        #region Variable declarations
        /// <summary>The last time our points were collected</summary>
        private DateTime LastTime;

        /// <summary>The title of our panel</summary>
        public String Title { get; set; }

        /// <summary>The type of element associated with our control</summary>
        public MM_Element_Type ElemType { get; set; }

        /// <summary>Our collection of tracked elements</summary>
        public List<MM_Element> Elements = new List<MM_Element>();

        /// <summary>The field to be read</summary>
        public FieldInfo FieldToCheck;

        /// <summary>The property to be read</summary>
        public PropertyInfo PropertyToCheck;

        /// <summary>Our base value for our control</summary>
        public float BaseValue;

        /// <summary>The tracking curve</summary>
        private CurveItem TrackingCurve;

        /// <summary>The time of our last value change</summary>
        private DateTime LastValueChange;

        /// <summary>Our popup menu with elements</summary>
        private MM_Popup_Menu Menu = new MM_Popup_Menu();

        /// <summary>The number format for our display point</summary>
        private String NumberFormat;
        #endregion

        #region Initialization
        /// <summary>
        /// Generate a standalone tracking form
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="ElemType"></param>
        /// <param name="MemberToCheck"></param>
        /// <param name="Elements"></param>
        public static MM_Form GenerateTrackingForm(String Title, MM_Element_Type ElemType, MemberInfo MemberToCheck, MM_Element[] Elements)
        {
            MM_Form NewForm = new MM_Form();
            MM_LoadGen_Tracking_Operator Tracking = new MM_LoadGen_Tracking_Operator(Title, ElemType, MemberToCheck);
            NewForm.Icon = Properties.Resources.CompanyIcon;
            Tracking.Elements.AddRange(Elements);
            Tracking.Dock = DockStyle.Fill;
            NewForm.Controls.Add(Tracking);
            return NewForm;
        }

        /// <summary>
        /// Initialize a new tracking operator
        /// </summary>
        public MM_LoadGen_Tracking_Operator()
        {
            InitializeComponent();
        }
        /// <param name="Title"></param>
        /// <param name="ElemType"></param>
        /// <param name="MemberToCheck"></param>
        public MM_LoadGen_Tracking_Operator(String Title, MM_Element_Type ElemType, MemberInfo MemberToCheck)
        {
            InitializeComponent();
            InitializeTracker(Title, ElemType, MemberToCheck);
        }

        /// <summary>
        /// Initialize our tracking form
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="ElemType"></param>
        /// <param name="MemberToCheck"></param>
        public void InitializeTracker(String Title, MM_Element_Type ElemType, MemberInfo MemberToCheck)
        {
            StringBuilder TitleIter = new StringBuilder();
            for (int a = 0; a < Title.Length; a++)
            {
                if (a > 0 && char.IsUpper(Title[a]) && Char.IsLower(Title[a - 1]))
                    TitleIter.Append(' ');
                TitleIter.Append(Title[a]);
            }
            this.Title = TitleIter.ToString().Replace('_', ' ');
            this.ElemType = ElemType;
            lblOperator.Text = this.Title;
            if (MemberToCheck is FieldInfo)
                this.FieldToCheck = (FieldInfo)MemberToCheck;
            else
                this.PropertyToCheck = (PropertyInfo)MemberToCheck;

            if (MemberToCheck.Name.Contains("Frequency") || MemberToCheck.Name.Contains("PerUnit"))
            {
                nudMax.DecimalPlaces = nudMin.DecimalPlaces = 2;
                NumberFormat = "#,##0.00";
            }
            else if (MemberToCheck.Name.Contains("Voltage"))
            {
                nudMax.DecimalPlaces = nudMin.DecimalPlaces = 1;
                NumberFormat = "#,##0.0";
            }
            else
            {
                nudMax.DecimalPlaces = nudMin.DecimalPlaces = 0;
                NumberFormat = "#,##0";
            }
        }

        /// <summary>
        /// Begin tracking our value
        /// </summary>
        public void BeginTracking()
        {
            BaseValue = 0;
            foreach (MM_Element Elem in Elements)
            {
                float CurVal = FieldToCheck != null ? (float)FieldToCheck.GetValue(Elem) : (float)PropertyToCheck.GetValue(Elem);
                if (!float.IsNaN(CurVal))
                    BaseValue += CurVal;
            }

            
            lblBaseValue.Text = BaseValue.ToString(NumberFormat);
            lblDeltaValue.Text = (0).ToString(NumberFormat);
            lblDeltaValue.ForeColor = Color.White;

            //Set our master panel
            GraphPane myMaster = zgGraph.GraphPane;
            myMaster.CurveList.Clear();
            myMaster.Title.IsVisible = false;
            myMaster.Title.FontSpec.FontColor = Color.White;
            myMaster.Legend.Fill = myMaster.Fill = new Fill(Color.Black);
            myMaster.Legend.FontSpec.FontColor = Color.White;
            myMaster.Legend.FontSpec.Fill = new Fill(Color.Black);
            myMaster.Legend.IsVisible = true;
            myMaster.Legend.Position = LegendPos.TopCenter;
            myMaster.XAxis.Title.FontSpec.FontColor = Color.White;
            myMaster.YAxis.Title.FontSpec.FontColor = Color.White;
            myMaster.XAxis.MajorTic.Color = Color.White;
            myMaster.XAxis.MinorTic.Color = Color.White;
            myMaster.YAxis.MajorTic.Color = Color.White;
            myMaster.YAxis.MinorTic.Color = Color.White;
            myMaster.XAxis.MajorGrid.Color = Color.White;
            myMaster.YAxis.MajorGrid.Color = Color.White;
            myMaster.Chart.Fill = new Fill(Color.Black);
            myMaster.Chart.Border.Color = Color.DarkGray;
            myMaster.XAxis.Scale.FontSpec.FontColor = Color.White;
            myMaster.YAxis.Scale.FontSpec.FontColor = Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.None;
            lblOperator.TextAlign = ContentAlignment.MiddleCenter;
            zgGraph.GraphPane.Legend.IsVisible = false; 
            zgGraph.GraphPane.XAxis.Title.Text = "Date/Time";
            zgGraph.GraphPane.XAxis.Type = AxisType.Date;
            zgGraph.GraphPane.XAxis.IsVisible = false;
            zgGraph.GraphPane.YAxis.IsVisible = false;
            zgGraph.IsShowPointValues = true;
            TrackingCurve = myMaster.AddCurve(Title, new PointPairList(), Color.White);
            TrackingCurve.AddPoint(ZedGraph.XDate.DateTimeToXLDate(LastTime = Data_Integration.CurrentTime), BaseValue);
            
            LastValueChange = DateTime.Now;
            zgGraph.AxisChange();

            ThresholdLine = new LineObj(Color.Green, zgGraph.GraphPane.XAxis.Scale.Min, BaseValue, zgGraph.GraphPane.XAxis.Scale.Max, BaseValue);
            zgGraph.GraphPane.GraphObjList.Add(ThresholdLine);

            zgGraph.Invalidate();
        }

        LineObj ThresholdLine;

        /// <summary>
        /// Handle our reset button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, EventArgs e)
        {
            BaseValue = 0;
            LastTime = default(DateTime);
            foreach (MM_Element Elem in Elements)
            {
                float CurVal = FieldToCheck != null ? (float)FieldToCheck.GetValue(Elem) : (float)PropertyToCheck.GetValue(Elem);
                if (!float.IsNaN(CurVal))
                    BaseValue += CurVal;
            }
            lblBaseValue.Text = BaseValue.ToString(NumberFormat);
            lblDeltaValue.Text = (0).ToString(NumberFormat);
            lblDeltaValue.ForeColor = Color.White;
            TrackingCurve.Clear();
            tmrUpdate_Tick(tmrUpdate, EventArgs.Empty);
        }

        #endregion

        /// <summary>
        /// When our user right-clicks, produce a pop-up menu for our items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MM_LoadGen_Tracking_Operator_MouseClick(object sender, MouseEventArgs e)
        {
           
            if (e.Button== System.Windows.Forms.MouseButtons.Right)
                Menu.Show(this, e.Location, Elements, true);
        }

        

        /// <summary>
        /// Handle the timer update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            //If our simulator is paused, don't add points
            if (Data_Integration.SimulatorStatus != MacomberMapCommunications.Messages.EMS.MM_Simulation_Time.enumSimulationStatus.Running || Data_Integration.CurrentTime == LastTime)
                return;

            LastTime = Data_Integration.CurrentTime;
            float NewValue = 0;
            foreach (MM_Element Elem in Elements)
            {
                float CurVal = FieldToCheck != null ? (float)FieldToCheck.GetValue(Elem) : (float)PropertyToCheck.GetValue(Elem);
                if (!float.IsNaN(CurVal))
                    NewValue += CurVal;
            }

            TrackingCurve.AddPoint(ZedGraph.XDate.DateTimeToXLDate(LastTime), NewValue);
            zgGraph.AxisChange();

            if (chkMax.Checked)
                zgGraph.GraphPane.YAxis.Scale.Max = (double)nudMax.Value;
            else
                nudMax.Value = (decimal)zgGraph.GraphPane.YAxis.Scale.Max;
            if (chkMin.Checked)
                zgGraph.GraphPane.YAxis.Scale.Min = (double)nudMin.Value;
            else
                nudMin.Value = (decimal)zgGraph.GraphPane.YAxis.Scale.Min;

            ThresholdLine = new LineObj(Color.Green, zgGraph.GraphPane.XAxis.Scale.Min, BaseValue, zgGraph.GraphPane.XAxis.Scale.Max, BaseValue);
            zgGraph.GraphPane.GraphObjList.Add(ThresholdLine);

            //    .X2 = zgGraph.GraphPane.XAxis.Scale.Max;
            zgGraph.Invalidate();

          
            String NewText = (NewValue - BaseValue).ToString(NumberFormat);
            if (lblDeltaValue.Text != NewText)
            {
                lblDeltaValue.Text = NewText;
                LastValueChange = DateTime.Now;
            }
            
            int Sec = Math.Min(255, (int)(DateTime.Now - LastValueChange).TotalSeconds * 6);
            lblDeltaValue.ForeColor = lblBaseValue.ForeColor = lblOperator.ForeColor = Color.FromArgb(Sec, 255, Sec);       
        }

        /// <summary>
        /// When we double-click, show our form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MM_LoadGen_Tracking_Operator_MouseDoubleClick(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(CreateCloneOfDisplay));
        }

        /// <summary>
        /// Create a clone of our display, and show
        /// </summary>
        /// <param name="state"></param>
        private void CreateCloneOfDisplay(object state)
        {
            MemberInfo mI = FieldToCheck == null ? (MemberInfo)PropertyToCheck : (MemberInfo)FieldToCheck;
            MM_Form NewForm = GenerateTrackingForm(this.Title, this.ElemType, mI, Elements.ToArray());
            if (Elements.Count == 1)
                NewForm.Text = this.Title + " " + Elements[0].ToString();
            else
                NewForm.Text = this.Title + " " + this.ElemType + " (" + Elements.Count.ToString("#,##0") + ")";
            NewForm.StartPosition = FormStartPosition.Manual;
            NewForm.Location = Cursor.Position;
            MM_LoadGen_Tracking_Operator Oper = (MM_LoadGen_Tracking_Operator) NewForm.Controls[0];
            Oper.BeginTracking();
            Oper.TrackingCurve.Clear();
            for (int a = 0; a < TrackingCurve.Points.Count; a++)
                Oper.TrackingCurve.AddPoint(TrackingCurve.Points[a]);
            Oper.BaseValue = BaseValue;
            


            Oper.zgGraph.GraphPane.XAxis.IsVisible = true;
            Oper.zgGraph.GraphPane.YAxis.IsVisible = true;
            zgGraph.AxisChange();
            zgGraph.Invalidate();
            Data_Integration.DisplayForm(NewForm, Thread.CurrentThread);
        }

        /// <summary>
        /// Refresh the graph on startup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            try
            {
            zgGraph.AxisChange();
            zgGraph.Invalidate();
            }
            catch { }
        }

        /// <summary>
        /// Update our axis as needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateAxis(object sender, EventArgs e)
        {
            if ((sender == nudMax || sender == chkMax) && chkMax.Checked)
                zgGraph.GraphPane.YAxis.Scale.Max = (double)nudMax.Value;
            else if ((sender == nudMin || sender == chkMin) && chkMin.Checked)
                zgGraph.GraphPane.YAxis.Scale.Min = (double)nudMin.Value;
            else if (sender == chkMin || sender == chkMax)
            {
                zgGraph.AxisChange();
                if (chkMax.Checked)
                    zgGraph.GraphPane.YAxis.Scale.Max = (double)nudMax.Value;
                else
                    nudMax.Value = (decimal)zgGraph.GraphPane.YAxis.Scale.Max;

                if (chkMin.Checked)
                    zgGraph.GraphPane.YAxis.Scale.Min = (double)nudMin.Value;
                else
                    nudMin.Value = (decimal)zgGraph.GraphPane.YAxis.Scale.Min;                
            }
            zgGraph.Invalidate();
        }       
    }
}
