using MacomberMapClient.Data_Elements.Blackstart;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
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

namespace MacomberMapClient.User_Interfaces.NetworkMap
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class displays information on equipment operated by an entity
    /// </summary>
    public partial class MM_Operatorship_Display : MM_Form
    {
        #region Variable declarations
        /// <summary>The network map associated with our display</summary>
        public MM_Network_Map_GDI nMap;

        /// <summary>The list of substations we have operatorship for</summary>
        private Dictionary<int, MM_Operatorship_Display_Substation> OperatedSubstations = new Dictionary<int, MM_Operatorship_Display_Substation>();

        /// <summary>Our base data handler</summary>
        public MM_DataSet_Base BaseData = new MM_DataSet_Base("Operatorship");
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our operatorship display
        /// </summary>
        public MM_Operatorship_Display(MM_Network_Map_GDI nMap)
        {
            InitializeComponent();
            this.nMap = nMap;
            lvEquipment.HideSelection = false;
            lvEquipment.FullRowSelect = true;
            olView.GetType().GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(olView, true);
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
        public static void CreateInstanceInSeparateThread(ToolStripMenuItem MenuItem, MM_Network_Map_GDI nMap)
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
            using (MM_Operatorship_Display oDisp = new MM_Operatorship_Display(State[1] as MM_Network_Map_GDI))
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
            btnRefresh_Click(btnRefresh, EventArgs.Empty);
            //if (MM_Server_Interface.ClientAreas.Contains("ERCOT"))
            //    this.Hide();
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
            OperatedSubstations.Clear();
            MM_Operatorship_Display_Substation FoundSub = null;
            bool IsMaster = MM_Server_Interface.ClientAreas.Contains("ERCOT");
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem.Substation != null && Elem.ElemType != null && Elem.Operator != null && Elem is MM_Blackstart_Corridor_Element == false && (IsMaster || MM_Server_Interface.ClientAreas.Contains(Elem.Operator.Alias)))
                {
                    if (!OperatedSubstations.TryGetValue(Elem.Substation.TEID, out FoundSub))
                        OperatedSubstations.Add(Elem.Substation.TEID, FoundSub = new MM_Operatorship_Display_Substation(Elem.Substation));
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
                else if (Elem is MM_Line && (IsMaster || MM_Server_Interface.ClientAreas.Contains(Elem.Operator.Alias)))
                {
                    foreach (MM_Substation Sub in ((MM_Line)Elem).ConnectedStations)
                    {
                        if (!OperatedSubstations.TryGetValue(Sub.TEID, out FoundSub))
                            OperatedSubstations.Add(Sub.TEID, FoundSub = new MM_Operatorship_Display_Substation(Sub));
                        FoundSub.Lines++;
                    }
                }

            PropertyInfo[] Properties = typeof(MM_Operatorship_Display_Substation).GetProperties();
            lvEquipment.Items.Clear();
            lvEquipment.Columns.Clear();
            lvEquipment.Columns.Add("Substation");
            lvEquipment.Columns.Add("LongName");
         /*   foreach (PropertyInfo pI in Properties)
                lvEquipment.Columns.Add(pI.Name);*/

            //Now, write out each element
            foreach (MM_Operatorship_Display_Substation Sub in OperatedSubstations.Values)
            {
                ListViewItem lvI = new ListViewItem(Sub.Substation.Name);
                lvI.Tag = Sub;
                lvI.SubItems.Add(Sub.Substation.LongName);
               /* foreach (PropertyInfo pI in Properties)
                    lvI.SubItems.Add(((int)pI.GetValue(Sub)).ToString("#,##0"));*/
                lvEquipment.Items.Add(lvI);
            }
            lvEquipment.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }


        /// <summary>
        /// When our index changes, update accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvEquipment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvEquipment.SelectedItems.Count == 1 && lvEquipment.UseWaitCursor == false)
            {
                try
                {
                    MM_Operatorship_Display_Substation Sub = lvEquipment.SelectedItems[0].Tag as MM_Operatorship_Display_Substation;
                    lvEquipment.UseWaitCursor = true;
                    if (Sub.Substation != olView.BaseElement)
                        if (olView.DataSource == null)
                            olView.SetControls(Sub.Substation, nMap, BaseData, null, Data_Integration.NetworkSource);
                        else
                            olView.LoadOneLine(Sub.Substation, null);
                    Thread.Sleep(4000);
                    lvEquipment.UseWaitCursor = false;
                }
                catch
                {
                }
            }
        }
        #endregion

        /// <summary>
        /// This class holds the information on our substation
        /// </summary>
        private class MM_Operatorship_Display_Substation
        {
            /// <summary>The substation with which we're associated</summary>
            public MM_Substation Substation;

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
            public MM_Operatorship_Display_Substation(MM_Substation Substation)
            {
                this.Substation = Substation;
            }
        }


    }
}