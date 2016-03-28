using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.Properties;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Menuing;
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
using MacomberMapCommunications.Extensions;

namespace MacomberMapClient.User_Interfaces.NetworkMap
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc.
    /// This class provides a view into our collection of islands
    /// </summary>
    public partial class MM_Island_View : MM_Form
    {
        /// <summary>Our island display</summary>
        private MM_DataGrid_View<MM_Island_Display> dgvIslandView = new MM_DataGrid_View<MM_Island_Display>();
        
        /// <summary>Our main network map</summary>
        private MM_Network_Map_DX MainMap;

        /// <summary>Our pop-up menu</summary>
        private MM_Popup_Menu MapMenu = new MM_Popup_Menu();

        /// <summary>Our collection of islands</summary>
        private Dictionary<int, MM_Island_Display> Islands = new Dictionary<int, MM_Island_Display>();

        /// <summary>
        /// Initialize our island view
        /// </summary>
        /// <param name="MainMap"></param>
        public MM_Island_View(MM_Network_Map_DX MainMap)
        {
            InitializeComponent();
            this.MainMap = MainMap;
            this.Text = "Island Summary - Macomber Map®";
            this.Icon = Properties.Resources.CompanyIcon;
            Data_Integration.IslandAdded += Data_Integration_IslandAdded;
            Data_Integration.IslandRemoved += Data_Integration_IslandRemoved;
            foreach (MM_Island Island in MM_Repository.Islands.Values)
            {
                MM_Island_Display IslandDisplay = new MM_Island_Display(Island, this);
                Islands.Add(Island.ID, IslandDisplay);
                dgvIslandView.Elements.Add(IslandDisplay);
            }
        }

        /// <summary>
        /// Hide our main form when it shows
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            dgvIslandView.CellMouseClick += dgvIslandView_CellMouseClick;
            dgvIslandView.AutoResizeColumns();

            int CurWidth = dgvIslandView.Left;
            
            foreach (DataGridViewColumn dCol in dgvIslandView.Columns)
                CurWidth += dCol.Width + 1;
            this.Width = CurWidth + 1;
            
            

            this.Hide();
            base.OnShown(e);
        }

        /// <summary>
        /// Handle an island removal
        /// </summary>
        /// <param name="Island"></param>
        private void Data_Integration_IslandRemoved(MM_Island Island)
        {
            MM_Island_Display FoundIsland;
            if (InvokeRequired)
                Invoke(new Data_Integration.IslandChangeDelegate(Data_Integration_IslandRemoved), Island);
            else if (Islands.TryGetValue(Island.ID, out FoundIsland))
            {
                Islands.Remove(Island.ID);
                dgvIslandView.Elements.Remove(FoundIsland);
            }
        }

        /// <summary>
        /// Handle an island addition
        /// </summary>
        /// <param name="Island"></param>
        private void Data_Integration_IslandAdded(MM_Island Island)
        {
            if (Island != null && !float.IsNaN(Island.Frequency))
            {
            if (!Islands.ContainsKey(Island.ID))
            if (InvokeRequired)
                Invoke(new Data_Integration.IslandChangeDelegate(Data_Integration_IslandAdded), Island);
            else 
                {
                    MM_Island_Display IslandDisplay = new MM_Island_Display(Island, this);
                    Islands.Add(Island.ID, IslandDisplay);
                    dgvIslandView.Elements.Add(IslandDisplay);
                }
            }
        }



      

        /// <summary>
        /// Handle the closing of the form (to hide instead of shut down)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }
        }


        /// <summary>
        /// Handle a right-click on our island
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvIslandView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
          /*  if (e.RowIndex == -1 || e.Button != System.Windows.Forms.MouseButtons.Right)
                return;
            MM_Island_Display IslandDisplay = (MM_Island_Display)dgvIslandView.Rows[e.RowIndex].DataBoundItem;
            dgvIslandView.CurrentCell = dgvIslandView.Rows[e.RowIndex].Cells[e.ColumnIndex];
           */
        }

      
        
        /// <summary>
        /// This class provides our island display and property change notifications
        /// </summary>
        private class MM_Island_Display : MM_Bound_Element
        {
            /// <summary>The island associated with our display</summary>
            public MM_Island Island { get; private set; }
            public float Frequency { get { return Island.Frequency; } }
            public MM_Unit IsochronousUnit { get { return Island.IsochronousUnit; } }

            public float Generation_MW { get { return Island.Generation; } }
            
            public int Units { get { return Island.Units.Length; } }
            public float Load_MW { get { return Island.Load; } }
            public int Loads { get { return Island.Loads.Length; } }
            public float Losses_MW { get { return Island.MWLosses; } }
            
            
            


            public float AvailableGenCapacity_MW { get { return Island.AvailableGenCapacity; } }
            public float TotalGenCapacity_MW { get { return Island.TotalGenCapacity; } }



            public float Unit_Reactive_MVAR
            {
                get
                {
                    float pMVAR = 0;
                    foreach (MM_Unit Unit  in Island.Units)
                        if (Unit.Estimated_MVAR > 0)
                            pMVAR += Unit.Estimated_MVAR;
                    return pMVAR;
                }
            }


            public float Unit_Reactive_Remaining_MVAR
            {
                get
                {
                    float pMVAR = 0;
                    foreach (MM_Unit Unit in Island.Units)
                        if (Unit.Estimated_MVAR > 0)
                            pMVAR += Unit.UnitMVARCapacity - Unit.Estimated_MVAR;
                    return pMVAR;
                }
            }

            public float Reactor_MVAR
            {
                get
                {
                    float pMVAR = 0;
                    foreach (MM_ShuntCompensator SC in Island.ShuntCompensators)
                        if (SC.Estimated_MVAR > 0)
                            pMVAR += SC.Estimated_MVAR;
                    return pMVAR;
                }
            }


            public float Unit_Capacitive_MVAR
            {
                get
                {
                    float pMVAR = 0;
                    foreach (MM_Unit Unit in Island.Units)
                        if (Unit.Estimated_MVAR < 0)
                            pMVAR += Unit.Estimated_MVAR;
                    return pMVAR;
                }
            }

            public float Unit_Capacitive_Remaining_MVAR
            {
                get
                {
                    float pMVAR = 0;
                    foreach (MM_Unit Unit in Island.Units)
                        if (Unit.Estimated_MVAR < 0)
                            pMVAR += Unit.UnitMVARCapacity - Unit.Estimated_MVAR;
                    return pMVAR;
                }
            }


            public float Capacitor_MVAR
            {
                get
                {
                    float nMVAR = 0;
                    foreach (MM_ShuntCompensator SC in Island.ShuntCompensators)
                        if (SC.Estimated_MVAR < 0)
                            nMVAR += SC.Estimated_MVAR;
                    return nMVAR;
                }
            }

            public float Highest_Voltage_PerUnit
            {
                get
                {
                    float HighestPu = float.MinValue;
                    foreach (MM_Bus Bus in MM_Repository.BusNumbers.Values.ToArray())
                        if (Bus != null && Bus.IslandNumber == Island.ID)
                            HighestPu = Math.Max(Bus.PerUnitVoltage, HighestPu);
                    return (HighestPu >= 0 && HighestPu < 2) ? HighestPu : 0;
                }
            }

            public float Lowest_Voltage_PerUnit
            {
                get
                {
                    float LowestPu = float.MaxValue;
                    foreach (MM_Bus Bus in MM_Repository.BusNumbers.Values.ToArray())
                        if (Bus != null && Bus.IslandNumber == Island.ID)
                            LowestPu = Math.Min(Bus.PerUnitVoltage, LowestPu);
                    return (LowestPu >= 0 && LowestPu < 2) ? LowestPu : 0;
                }
            }


            public float Average_Weighted_Voltage_PerUnit
            {
                get
                {
                    float BusNum = 1f, BusDenom = 1f, BusPU;
                    int CountBuses = 0;
                    foreach (MM_Bus Bus in MM_Repository.BusNumbers.Values.ToArray())
                        if (Bus != null && Bus.IslandNumber == Island.ID && !Bus.Dead && !float.IsNaN(BusPU = Bus.PerUnit)) 
                        {
                            BusNum += (BusPU * Math.Abs(1f - BusPU) * Bus.Estimated_kV);
                            BusDenom += Math.Abs(1f - BusPU) * Bus.Estimated_kV;
                            CountBuses++;
                        }

                    if (CountBuses == 0)
                        return float.NaN;
                    else
                        return BusNum / BusDenom;                       
                }
            }

            /// <summary>
            /// Initialize a new island diplsya
            /// </summary>
            /// <param name="Island"></param>
            /// <param name="Disp"></param>
            public MM_Island_Display(MM_Island Island, MM_Island_View Disp):base(Island, Disp)
            {
                if (Island != null && !float.IsNaN(Island.Frequency))
                this.Island = Island;
            }
        }

        /// <summary>
        /// When our timer triggers, invalidate the display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            dgvIslandView.Invalidate();
        }
    }
}
