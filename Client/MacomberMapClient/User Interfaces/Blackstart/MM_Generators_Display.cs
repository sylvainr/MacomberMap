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
    public partial class MM_Generators_Display : MM_Form
    {
        #region Variable declarations
        /// <summary>Our collection of units</summary>
        private MM_DataGrid_View<MM_Unit_Display> dgvUnits = new MM_DataGrid_View<MM_Unit_Display>();

        /// <summary>The network map associated with our display</summary>
        public MM_Network_Map_DX nMap;

        /// <summary>Our base data handler</summary>
        public MM_DataSet_Base BaseData = new MM_DataSet_Base("Generators");
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our display
        /// </summary>
        public MM_Generators_Display(MM_Network_Map_DX nMap)
        {
            InitializeComponent();
            this.nMap = nMap;
            this.DoubleBuffered = true;
            this.Title = this.Text;
            dgvUnits.CellMouseClick += DataGridView_CellClick;
            dgvUnits.networkMap = nMap;
        }

        /// <summary>
        /// Initialize an empty display for UI editing
        /// </summary>
        public MM_Generators_Display()
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
            using (MM_Generators_Display oDisp = new MM_Generators_Display(State[1] as MM_Network_Map_DX))
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

            base.OnShown(e);
        }
        #endregion

        #region Refreshing
        /// <summary>
        /// Refresh our list of operated equipment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            bool IsMaster = MM_Server_Interface.ClientAreas.ContainsKey("ERCOT");

            dgvUnits.Elements.Clear();
            foreach (MM_Element Unit in MM_Repository.Units.Values)
                if (Unit.ElemType != null && Unit.Operator != null && Unit is MM_Blackstart_Corridor_Element == false && (IsMaster || MM_Server_Interface.ClientAreas.ContainsKey(Unit.Operator.Alias)))
                    dgvUnits.Elements.Add(new MM_Unit_Display((MM_Unit)Unit, this));
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
            MM_Element HighlightElement = (MM_Element)Sender.Rows[e.RowIndex].DataBoundItem.GetType().GetProperties()[0].GetValue(Sender.Rows[e.RowIndex].DataBoundItem);
            Sender.UseWaitCursor = true;

            if (HighlightElement != null && HighlightElement.ElemType.Configuration != null && HighlightElement.ElemType.Configuration["ControlPanel"] != null)
            {

                olView.LoadOneLine(HighlightElement.Substation, HighlightElement);
                //olView.BaseElement = HighlightElement;
                //olView.HighlightElement(HighlightElement);
                if (MM_Server_Interface.ClientAreas.ContainsKey("ERCOT") || MM_Server_Interface.ClientAreas.ContainsKey(HighlightElement.Operator.Alias))
                {
                    if (olView.ElementsByTEID.Count > 0)
                    {
                        MM_OneLine_Element Unit = olView.ElementsByTEID[HighlightElement.TEID];
                        new OneLines.MM_ControlPanel(Unit, HighlightElement.ElemType.Configuration["ControlPanel"], olView) { StartPosition = FormStartPosition.Manual, Location = Cursor.Position }.Show(this);
                    }
                    else
                        MessageBox.Show("unable to open Control Panel for " + (olView.BaseElement.Substation == null ? "" : olView.BaseElement.Substation.LongName) + " " + olView.BaseElement.ElemType.Name + " " + olView.BaseElement.Name + ".", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                else
                    MessageBox.Show("Control of " + (olView.BaseElement.Substation == null ? "" : olView.BaseElement.Substation.LongName) + " " + olView.BaseElement.ElemType.Name + " " + olView.BaseElement.Name + " is not permitted.\n(operated by " + olView.BaseElement.Operator.Name + " - " + olView.BaseElement.Operator.Alias + ").", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
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

            /// <summary>Our frequency control</summary>
            public string FrqCtrl
            {
                get { return Unit.FrequencyControl ? "Yes" : ""; }
            }

            /// <summary>Our MW</summary>
            public float CurrentMW { get { return Unit.Estimated_MW; } }

            /// <summary>Our target MW</summary>
            public float TargetMW { get { return Unit.Desired_MW; } }

            /// <summary>Our percent MW</summary>
            public float PercentMW { get { return Unit.UnitPercentage; } }

            /// <summary>Our min MVAR limit</summary>
            public float MinMVARLimit
            {   get
                {
                    float Cap, Reac;

                    Unit.DetermineMVARCapabilities(out Cap, out Reac);
                    return Cap;
                }
            }

            /// <summary>Our max MVAR limit</summary>
            public float MaxMVARLimit
            {
                get
                {
                    float Cap, Reac;

                    Unit.DetermineMVARCapabilities(out Cap, out Reac);
                    return Reac;
                }
            }

            /// <summary>Our MVAR</summary>
            public float CurrentMVAR { get { return Unit.Estimated_MVAR; } }

            /// <summary>Our percent MVAR</summary>
            public float PercentMVAR { get { return Unit.UnitReactivePercentage; } }

            /// <summary>Our percent KV</summary>
            public float Voltage { get { return Unit.VoltageTarget; } }

            /// <summary>Our frequency</summary>
            public float Frequency { get { return Unit.Frequency; } }

            /// <summary>Our Island</summary>
            public string Island { get { return (Unit.NearIsland != null) ? Unit.NearIsland.Name : ""; } }

            /// <summary>
            /// Our unit display
            /// </summary>
            /// <param name="Unit"></param>
            public MM_Unit_Display(MM_Unit Unit, MM_Generators_Display Disp)
                : base(Unit, Disp)
            {
                this.Unit = Unit;
            }
        }
    }
}