using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using Macomber_Map.Data_Elements;
using Macomber_Map.User_Interface_Components;
using System.Threading;
using Macomber_Map.Properties;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class displays summaries of elements in a region
    /// </summary>
    public partial class Element_Summary : ListView
    {
        #region Variable declarations
        /// <summary>The tables on which the data are based</summary>
        public DataSet_Base BaseData;

        /// <summary>
        /// This delegate allows transferring selected types and KV Levels
        /// </summary>
        /// <param name="SelectedTable">The table underlying the row selected</param>
        /// <param name="SelectedKVLevel">The KV Level underlying the Column selected</param>
        public delegate void TypeSelectedDeletage(DataTable SelectedTable, MM_KVLevel SelectedKVLevel);

        /// <summary>This event is fired when the user selects a particular type. If the type has a voltage level, that is passed as well.</summary>
        public event TypeSelectedDeletage TypeSelected;

        /// <summary>The menu for handling export to Excel</summary>
        private ContextMenuStrip cms = new ContextMenuStrip();

       #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new element summary list view
        /// </summary>
        public Element_Summary()
        {
            this.BackColor = MM_Repository.OverallDisplay.BackgroundColor;
            this.ForeColor = MM_Repository.OverallDisplay.ForegroundColor;
            this.View = View.Details;
            this.DoubleBuffered = true;
            Columns.Add("Type");            
            Columns.Add("#");
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)   
                if (KVLevel.Permitted)
                    (Columns.Add(KVLevel.Name.Split(' ')[0]) as ColumnHeader).Tag = KVLevel;
            this.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            this.FullRowSelect = true;
            cms.ItemClicked +=new ToolStripItemClickedEventHandler(cms_ItemClicked);
        }

      
        #endregion

        
        private delegate void SafeSetControls(DataSet_Base BaseData);

        /// <summary>
        /// Asssign controls to the summary viewer
        /// </summary>
        /// <param name="BaseData">The base data for the display</param>        
        public void SetControls(DataSet_Base BaseData)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new SafeSetControls(SetControls), BaseData);
            else
            {
                //Assign our base table and integration layer
                this.BaseData = BaseData;


                //Go through the data tables, and tally up both total and by KV level into the columns
                this.Items.Clear();
                foreach (DataTable dt in BaseData.Data.Tables)
                    if (dt.Rows.Count >0)
                {


                    ListViewItem ElementRow = this.Items.Add(dt.TableName);
                    ElementRow.UseItemStyleForSubItems = false;
                    ElementRow.SubItems.Add(dt.Rows.Count.ToString("#,##0")).ForeColor = Color.White;
                    ElementRow.Tag = dt;
                    Dictionary<MM_KVLevel, int> KVLevels = new Dictionary<MM_KVLevel, int>(MM_Repository.KVLevels.Count);
                    foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                        if (KVLevel.Permitted)
                            KVLevels.Add(KVLevel, 0);

                    foreach (DataRow dr in dt.Rows)
                    {
                        MM_Element Elem = dr["TEID"] as MM_Element;
                        if (Elem is MM_Substation)
                        {
                            foreach (MM_KVLevel KVLevel in (Elem as MM_Substation).KVLevels)
                                if (KVLevel.Permitted)
                                    KVLevels[KVLevel]++;
                        }
                        else if (Elem is MM_Transformer)
                        {
                            foreach (MM_TransformerWinding Winding in (Elem as MM_Transformer).Windings)
                                if (Winding.KVLevel.Permitted)
                                    KVLevels[Winding.KVLevel]++;
                        }
                        else if (Elem.KVLevel != null)
                            KVLevels[Elem.KVLevel]++;
                    }

                    foreach (KeyValuePair<MM_KVLevel, int> KVLevel in KVLevels)
                        if (KVLevel.Value == 0)
                            ElementRow.SubItems.Add("").Tag = KVLevel;
                        else
                        {
                            ListViewItem.ListViewSubItem SubItem = ElementRow.SubItems.Add(KVLevel.Value.ToString("#,##0"));
                            SubItem.ForeColor = KVLevel.Key.Energized.ForeColor;
                            SubItem.Tag = KVLevel;
                        }
                }
                this.Sorting = SortOrder.Ascending;
                this.Sort();
                MM_Repository.ViewChanged += new MM_Repository.ViewChangedDelegate(MM_Repository_ViewChanged);
            }
        }

        /// <summary>
        /// Update the KV level colors as needed
        /// </summary>
        /// <param name="ActiveView"></param>
        private void MM_Repository_ViewChanged(MM_Display_View ActiveView)
        {
            if (InvokeRequired)
                this.BeginInvoke(new SafeViewUpdate(MM_Repository_ViewChanged), ActiveView);
            else
                foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                    for (int a = 0; a < Columns.Count; a++)
                        if (Columns[a].Tag == KVLevel)
                            foreach (ListViewItem lvI in Items)
                                lvI.SubItems[a].ForeColor = KVLevel.Energized.ForeColor;
        }


        private delegate void SafeViewUpdate(MM_Display_View ActiveView);


        #region Mouse click handling  
    

        /// <summary>
        /// Handle the mouse clicking in the list view
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            ListViewHitTestInfo ht = this.HitTest(e.Location);
            if (ht.Item != null && ht.Item.Text == "(get more...)")
            {
                ht.Item.Text = "(Loading...)";
                (TypeSelected.Target as Form_Builder).Lasso_GetMoreElements();
            }
            else if (ht.Item != null && TypeSelected != null)
                if (ht.SubItem.Tag == null)
                {
                    BoxSubItem(ht.Item.SubItems[1]);
                    TypeSelected(ht.Item.Tag as DataTable, null);
                }
                else
                {
                    BoxSubItem(ht.SubItem);
                    TypeSelected(ht.Item.Tag as DataTable, ((KeyValuePair<MM_KVLevel, int>)ht.SubItem.Tag).Key);
                }
            if (e.Button == MouseButtons.Right)
            {
                cms.Items.Clear();
                cms.Items.Add("Export to Excel", Resources.Excel);
                cms.Show(this, e.Location);
            }
        }

        /// <summary>
        /// Handle the user's clicking a menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cms_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Export to Excel")
            {
                Thread ExcelThread = new Thread(ExportToExcel);
                ExcelThread.SetApartmentState(ApartmentState.STA);
                ExcelThread.Start();
            }
        }



        /// <summary>
        /// Handle exporting to Excel
        /// </summary>
        private void ExportToExcel()
        {
            using (SaveFileDialog sFd = new SaveFileDialog())
            {
                sFd.Title = "Export Excel spreadsheet";
                sFd.Filter = "Excel 2007 Workbook (*.xlsb)|*.xlsb|Excel 2003 Workbook (*.xls)|*.xls";
                if (sFd.ShowDialog() == DialogResult.OK)
                    Data_Integration.DisplayForm(new MM_Excel_Exporter(BaseData.Data, sFd.FileName), Thread.CurrentThread);
            }
        }
        /// <summary>
        /// Go through all elements in the list view, and only box the one we've selected
        /// </summary>
        /// <param name="SubItem"></param>
        public void BoxSubItem(ListViewItem.ListViewSubItem SubItem)
        {
            foreach (ListViewItem lI in this.Items)
                foreach (ListViewItem.ListViewSubItem lSI in lI.SubItems)
                    lSI.BackColor = (lSI == SubItem ? SystemColors.Highlight : this.BackColor);
            this.SelectedItems.Clear();
        }
        #endregion
    }
}
