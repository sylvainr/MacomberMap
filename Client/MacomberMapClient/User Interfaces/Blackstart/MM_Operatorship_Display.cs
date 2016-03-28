using MacomberMapClient.Data_Elements.Blackstart;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.NetworkMap;
using MacomberMapClient.User_Interfaces.OneLines;
using MacomberMapClient.User_Interfaces.Summary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Blackstart
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class displays information on equipment operated by an entity
    /// </summary>
    public partial class MM_Operatorship_Display : MM_Form
    {
        #region Variable declarations
        /// <summary>Our collection of units</summary>
        private MM_DataGrid_View<MM_Unit_Display> dgvUnits = new MM_DataGrid_View<MM_Unit_Display>();

        /// <summary>Our collection of lines</summary>
        private MM_DataGrid_View<MM_Line_Display> dgvLines = new MM_DataGrid_View<MM_Line_Display>();

        /// <summary>Our collection of Substations</summary>
        private MM_DataGrid_View<MM_Substation_Display> dgvSubstations = new MM_DataGrid_View<MM_Substation_Display>();

        /// <summary>The network map associated with our display</summary>
        public MM_Network_Map_DX nMap;

        /// <summary>The list of substations we have operatorship for</summary>
        private Dictionary<int, MM_Substation_Display> OperatedSubstations = new Dictionary<int, MM_Substation_Display>();

        /// <summary>Our base data handler</summary>
        public MM_DataSet_Base BaseData = new MM_DataSet_Base("Operatorship");

        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our operatorship display
        /// </summary>
        public MM_Operatorship_Display(MM_Network_Map_DX nMap)
        {
            InitializeComponent();
            this.nMap = nMap;
            this.DoubleBuffered = true;
            if (MM_Server_Interface.ISQse)
                this.Title = this.Text.Replace("Substations","Units");
            else
            this.Title = this.Text;
            dgvLines.CellMouseClick += DataGridView_CellClick;
            dgvUnits.CellMouseClick += DataGridView_CellClick;
            dgvSubstations.CellMouseClick += DataGridView_CellClick;
            dgvLines.networkMap = nMap;
            dgvUnits.networkMap = nMap;
            dgvSubstations.networkMap = nMap;
            olView.Dock = DockStyle.Fill;
        }


        /// <summary>
        /// Initialize an empty operatorship display for UI editing
        /// </summary>
        public MM_Operatorship_Display()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Handle the closing event by instead hiding the window
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        /// <summary>
        /// Create a seperate thread to run the communications viewer, and run it.
        /// </summary>
        /// <param name="nMap"></param>
        /// <param name="MenuItem"></param>
        /// <returns></returns>
        public static void CreateInstanceInSeparateThread(ToolStripMenuItem MenuItem, MM_Network_Map_DX nMap)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(InstantiateForm), new object[] { MenuItem, nMap });
        }

        /// <summary>
        /// Instantiate a comm viewer
        /// </summary>
        /// <param name="state">The state of the form</param>
        private static void InstantiateForm(object state)
        {
            object[] State = (object[])state;
            using (MM_Operatorship_Display oDisp = new MM_Operatorship_Display(State[1] as MM_Network_Map_DX))
            {
                (State[0] as ToolStripMenuItem).Tag = oDisp;
                Data_Integration.DisplayForm(oDisp, Thread.CurrentThread);
            }
        }

        /// <summary>
        /// When our form is shown, recalculate
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            this.Hide();
            btnRefresh_Click(btnRefresh, EventArgs.Empty);
            if (MM_Server_Interface.ISQse)
                this.tcMain.SelectedIndex = this.tcMain.TabCount-1;
            btnShowOneLine.Checked = true;
            splTitle.Panel2Collapsed = !btnShowOneLine.Checked;
         //   olView.LoadOneLine((MM_Element)dgvSubstations.Rows[0].DataBoundItem.GetType().GetProperties()[0].GetValue(dgvSubstations.Rows[0].DataBoundItem), null);
            base.OnShown(e);
        }
        #endregion

        #region Refreshing

        /// <summary>
        /// Show/hide our one-line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowOneLine_Click(object sender, EventArgs e)
        {
            btnShowOneLine.Checked ^= true;
            splTitle.Panel2Collapsed = !btnShowOneLine.Checked;
        }

        /// <summary>
        /// Refresh our list of operated equipment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            OperatedSubstations.Clear();
            MM_Substation_Display FoundSub = null;
            Dictionary<int, MM_Line> LineCollection = new Dictionary<int, MM_Line>();
            bool IsMaster = MM_Server_Interface.ClientAreas.ContainsKey("ERCOT");


            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem.ElemType != null && Elem.Operator != null && Elem is MM_Blackstart_Corridor_Element == false && (IsMaster || MM_Server_Interface.ClientAreas.ContainsKey(Elem.Operator.Alias)))
                    if (Elem.Substation != null)
                    {
                        if (!OperatedSubstations.TryGetValue(Elem.Substation.TEID, out FoundSub))
                            OperatedSubstations.Add(Elem.Substation.TEID, FoundSub = new MM_Substation_Display(Elem.Substation, this));
                        if (Elem.ElemType.Name == "Breaker")
                            FoundSub.Breakers++;
                        else if (Elem.ElemType.Name == "Switch")
                            FoundSub.Switches++;
                        else if (Elem.ElemType.Name == "Capacitor")
                            FoundSub.Capacitors++;
                        else if (Elem.ElemType.Name == "Reactor")
                            FoundSub.Reactors++;
                        else if (Elem.ElemType.Name == "StaticVarCompensator")
                            FoundSub.SVCs++;
                    }
                    else if (Elem is MM_Line)
                    {
                        MM_Line Line = (MM_Line)Elem;
                        if (!Line.IsSeriesCompensator)
                            foreach (MM_Substation Sub in Line.ConnectedStations)
                            {
                                if (!OperatedSubstations.TryGetValue(Sub.TEID, out FoundSub))
                                    OperatedSubstations.Add(Sub.TEID, FoundSub = new MM_Substation_Display(Sub, this));
                                FoundSub.Lines++;
                                if (!LineCollection.ContainsKey(Line.TEID))
                                    LineCollection.Add(Line.TEID, Line);
                            }
                    }


            //Clear our lines and loads tables
            dgvLines.Elements.Clear();
            dgvUnits.Elements.Clear();
            dgvSubstations.Elements.Clear();

            //Now, write out each element
            dgvSubstations.Elements.RaiseListChangedEvents = false;
            dgvUnits.Elements.RaiseListChangedEvents = false;
            dgvLines.Elements.RaiseListChangedEvents = false;
            foreach (MM_Substation_Display Sub in OperatedSubstations.Values)
            {
               dgvSubstations.Elements.Add(Sub);
                if (Sub.Substation.Units != null)
                    foreach (MM_Unit Unit in Sub.Substation.Units)
                        if (MM_Server_Interface.ClientAreas.ContainsKey("ERCOT") || MM_Server_Interface.ClientAreas.ContainsKey(Unit.Operator.Alias))
                        dgvUnits.Elements.Add(new MM_Unit_Display(Unit, this));
            }

            foreach (MM_Line Line in LineCollection.Values)
                dgvLines.Elements.Add(new MM_Line_Display(Line, this));

            dgvSubstations.Elements.RaiseListChangedEvents = false;
            dgvUnits.Elements.RaiseListChangedEvents = false;
            dgvLines.Elements.RaiseListChangedEvents = false;

            dgvSubstations.Elements.ResetBindings();
            dgvUnits.Elements.ResetBindings();
            dgvLines.Elements.ResetBindings();
        }

        /// <summary>
        /// Handle our click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView_CellClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            DataGridView Sender = (DataGridView)sender;
            if (e.RowIndex == -1)
                return;
            MM_Element SelectedObject = (MM_Element)Sender.Rows[e.RowIndex].DataBoundItem.GetType().GetProperties()[0].GetValue(Sender.Rows[e.RowIndex].DataBoundItem);
            MM_Element HighlightElement = SelectedObject is MM_Substation ? null : SelectedObject;
            if (SelectedObject is MM_Line)
                SelectedObject = SelectedObject.Contingencies[0];
            else if (SelectedObject.Substation != null)
                SelectedObject = SelectedObject.Substation;
            Sender.UseWaitCursor = true;

            if (SelectedObject == olView.BaseElement)
                olView.HighlightElement(HighlightElement);
            else if (olView.DataSource == null)
            {
                olView.SetControls(SelectedObject, nMap, BaseData, null, Data_Integration.NetworkSource);
                olView.HighlightElement(HighlightElement);
            }
            else
            {
                olView.LoadOneLine(SelectedObject, HighlightElement);
                olView.HighlightElement(HighlightElement);
            }
            if (HighlightElement is MM_Unit)
            {
                if (HighlightElement != null && HighlightElement.ElemType.Configuration != null && HighlightElement.ElemType.Configuration["ControlPanel"] != null)
                {
                    if (MM_Server_Interface.ClientAreas.ContainsKey("ERCOT") || MM_Server_Interface.ClientAreas.ContainsKey(HighlightElement.Operator.Alias))
                    {
                        MM_OneLine_Element Unit = olView.ElementsByTEID[HighlightElement.TEID];
                        new OneLines.MM_ControlPanel(Unit, HighlightElement.ElemType.Configuration["ControlPanel"], olView) { StartPosition = FormStartPosition.Manual, Location = Cursor.Position }.Show(this);
                    }
                    else
                        MessageBox.Show("Control of " + (olView.BaseElement.Substation == null ? "" : olView.BaseElement.Substation.LongName) + " " + olView.BaseElement.ElemType.Name + " " + olView.BaseElement.Name + " is not permitted.\n(operated by " + olView.BaseElement.Operator.Name + " - " + olView.BaseElement.Operator.Alias + ").", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
            Sender.UseWaitCursor = false;
            Sender.Invalidate();
        }
                
        /// <summary>
        /// When our timer triggers, invalidate the display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            dgvSubstations.Invalidate();
            dgvLines.Invalidate();
            dgvUnits.Invalidate();
        }


        #endregion

        /// <summary>
        /// Hold our unit display component
        /// </summary>
        private class MM_Unit_Display : MM_Bound_Element
        {
            /// <summary>The unit associated with our display component</summary>
            public MM_Unit Unit { get; private set; }

            /// <summary>Our percent MW</summary>
            public float PercentMW { get { return Unit.UnitPercentage; } }

            /// <summary>Our frequency</summary>
            public float Frequency { get { return Unit.Frequency; } }

            /// <summary>Our percent MVAR</summary>
            public float PercentMVAR { get { return Unit.UnitReactivePercentage; } }

            /// <summary>
            /// Our unit display
            /// </summary>
            /// <param name="Unit"></param>
            /// <param name="Disp"></param>
            public MM_Unit_Display(MM_Unit Unit, MM_Operatorship_Display Disp)
                : base(Unit, Disp)
            {
                this.Unit = Unit;
            }
        }

        /// <summary>
        /// Hold our line display component
        /// </summary>
        private class MM_Line_Display : MM_Bound_Element
        {
            /// <summary>The line associated with our display component</summary>
            public MM_Line Line { get; private set; }

            /// <summary>Our line percentage</summary>
            public float Percentage { get { return Line.LinePercentage; } }

            /// <summary>MVA flow</summary>
            public float MVA { get { return Line.MVAFlow; } }

            /// <summary>MW flow</summary>
            public float MW { get { return Line.MWFlow; } }

            /// <summary>MVAR flow</summary>
            public float MVAR { get { return Line.MVARFlow; } }


            /// <summary>
            /// Initialize a new display
            /// </summary>
            /// <param name="Line"></param>
            /// <param name="Disp"></param>
            public MM_Line_Display(MM_Line Line, MM_Operatorship_Display Disp)
                : base(Line, Disp)
            {
                this.Line = Line;
            }

            /// <summary>
            /// Return the worst per-unit voltage on the line
            /// </summary>
            public float PerUnitVoltage
            {
                get
                {
                    MM_Bus NearBus = Line.NearBus;
                    MM_Bus FarBus = Line.FarBus;

                    if (NearBus == null || FarBus == null)
                        return float.NaN;
                    else if (NearBus.PerUnitVoltage >= 1.0f)
                        return Math.Max(NearBus.PerUnitVoltage, FarBus.PerUnitVoltage);
                    else
                        return Math.Min(NearBus.PerUnitVoltage, FarBus.PerUnitVoltage);
                }
            }

            /// <summary>
            /// Return the worst KV voltage on the line
            /// </summary>
            public float LineVoltage
            {
                get
                {
                    MM_Bus NearBus = Line.NearBus;
                    MM_Bus FarBus = Line.FarBus;

                    if (NearBus == null || FarBus == null)
                        return float.NaN;
                    else if (NearBus.PerUnitVoltage >= 1.0f)
                        return Math.Max(NearBus.Estimated_kV, FarBus.Estimated_kV);
                    else
                        return Math.Min(NearBus.Estimated_kV, FarBus.Estimated_kV);
                }
            }

        }


        /// <summary>
        /// This class holds the information on our substation
        /// </summary>
        private class MM_Substation_Display : MM_Bound_Element
        {
            /// <summary>The substation with which we're associated</summary>
            public MM_Substation Substation { get; private set; }

            /// <summary>Our substation long name</summary>
            public string LongName { get { return Substation.LongName; } }

            /// <summary>The number of breakers we're connected to</summary>
            public int Breakers { get; set; }

            /// <summary>The number of breakers we're connected to</summary>
            public int Switches { get; set; }

            /// <summary>The number of lines we're connected to</summary>
            public int Lines { get; set; }

            /// <summary>The number of transformers we're associated with</summary>
            public int Transformers { get; set; }

            /// <summary>The number of capacitors we're associated with</summary>
            public int Capacitors { get; set; }

            /// <summary>The number of reactors we're associated with</summary>
            public int Reactors { get; set; }

            /// <summary>The number of SVCs we're associated with</summary>
            public int SVCs { get; set; }

            /// <summary>
            /// Initialize a new substation
            /// </summary>
            /// <param name="Substation"></param>
            /// <param name="Disp"></param>
            public MM_Substation_Display(MM_Substation Substation, MM_Operatorship_Display Disp)
                : base(Substation, Disp)
            {
                this.Substation = Substation;
            }

        }
    }
}