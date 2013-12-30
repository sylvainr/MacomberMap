using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using Macomber_Map.Data_Connections;
using System.Drawing;
using Macomber_Map.User_Interface_Components.NetworkMap;
using Macomber_Map.Properties;
using System.Threading;
using System.Data;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// The data table provides an interface into the core data
    /// </summary>
    public class Data_Table: DataGridView
    {
        #region Variable declarations
        /// <summary>The property page associated with this item</summary>
        public Property_Page AssociatedPropertyPage;

        /// <summary>This flag will prevent the property page from firing during a refresh</summary>
        private bool SettingDataSource = false;
        
        /// <summary>The menu for handling right clicks on a cell</summary>
        private MM_Popup_Menu RightClickMenu = new MM_Popup_Menu();

        /// <summary>The network map associated with the large display (for zooming/panning the map on right-click</summary>
        private Network_Map nMap;

        /// <summary>The violation viewer associated with this display</summary>
        private Violation_Viewer violView;

        /// <summary>The mini-map associated with this display</summary>
        private Mini_Map miniMap;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new data table
        /// </summary>
        /// <param name="AssociatedPropertyPage">The property page associated with the table</param>
        /// <param name="miniMap">The mini-map associated with the table</param>
        /// <param name="nMap">The network map associated with the table</param>
        /// <param name="violView">The violation view assocaited with the table</param>
        public Data_Table(Property_Page AssociatedPropertyPage, Network_Map nMap, Violation_Viewer violView, Mini_Map miniMap)
        {
            this.nMap = nMap;
            this.miniMap = miniMap;
            this.AssociatedPropertyPage = AssociatedPropertyPage;
            this.AllowUserToAddRows = false;                        
            this.AllowUserToResizeColumns = true;
            this.violView = violView;
            this.AllowUserToResizeRows = true;
            this.AllowUserToOrderColumns = true;
            if (Data_Integration.MMServer != null)
                this.EditMode = DataGridViewEditMode.EditProgrammatically;
            else
            {
                this.EditMode = DataGridViewEditMode.EditOnEnter;
                this.CellValueChanged += new DataGridViewCellEventHandler(Data_Table_CellValueChanged);
            }


            this.DataError += new DataGridViewDataErrorEventHandler(Data_Table_DataError);
        }

        /// <summary>
        /// Handle a value change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_Table_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            this.FindForm().Refresh();
        }


        /// <summary>
        /// Handle a grid data error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_Table_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            Program.LogError(e.Exception);
            e.ThrowException = false;
        }

        

        /// <summary>
        /// Handle a selection change within the data table, and send it to the associated property page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(EventArgs e)
        {
            try
            {
                if (SettingDataSource || Data_Integration.MMServer == null)
                    return;
                else if (this.SelectedCells.Count == 1)
                    AssociatedPropertyPage.SetElement(this.CurrentRow.Cells["TEID"].Value as MM_Element);
                else
                    AssociatedPropertyPage.SetElement(null);
                base.OnSelectionChanged(e);
            }
            catch (Exception ex)
            {
                Program.WriteConsoleLine("Error updating data table selection: " + ex.Message);
            }
        }

        /// <summary>
        /// Handle the right-click event on a cell
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCellMouseClick(DataGridViewCellMouseEventArgs e)
        {
            //make sure we have a valid row/column, and find our element
            if (e.RowIndex == -1 || e.ColumnIndex == -1) return;


            MM_Element Elem = Rows[e.RowIndex].Cells["TEID"].Value as MM_Element;

            //Set our property page
            SettingDataSource = true;
            this.CurrentCell = Rows[e.RowIndex].Cells[e.ColumnIndex];
            AssociatedPropertyPage.SetElement(Elem);
            SettingDataSource = false;

            //Show our menu or update accordingly
            if (e.Button == MouseButtons.Left)
                FindForm().Refresh();
            else if (e.Button == MouseButtons.Right)
                RightClickMenu.Show(this, e.Location, Elem, true);

            base.OnCellMouseClick(e);
        }

        #endregion

        /// <summary>
        /// Set the data source for a table, and make sure we don't accidently call up a property page
        /// </summary>
        /// <param name="bindingSource">The data binding and source for the table</param>
        public void SetDataSource(BindingSource bindingSource)
        {
            SettingDataSource = true;
            this.DataSource = bindingSource;

            //Set up our parameters
            DataTable tbl = bindingSource.DataSource as System.Data.DataTable;
            float[] Min = new float[tbl.Columns.Count];
            float[] Max = new float[tbl.Columns.Count];
            float[] Count = new float[tbl.Columns.Count];            
            float[] Sum = new float[tbl.Columns.Count];
            float[] Sum2 = new float[tbl.Columns.Count];
            float[] SumP = new float[tbl.Columns.Count];
            float[] Nulls = new float[tbl.Columns.Count];
            float[] Unknowns = new float[tbl.Columns.Count];
            float[] Trues = new float[tbl.Columns.Count];
            float[] Falses = new float[tbl.Columns.Count];
            for (int x = 0; x < tbl.Columns.Count; x++)
                Min[x] = Max[x] = float.NaN;

            //Make sure every column is sortable
            for (int Col = 0; Col < Columns.Count; Col++)
                Columns[Col].SortMode = DataGridViewColumnSortMode.Automatic;

            //Build our tooltips for each column
            for (int Row = 0; Row < bindingSource.Count; Row++)
            {
                bindingSource.Position = Row;
                DataRowView dv = bindingSource.Current as System.Data.DataRowView;
                for (int x = 0; x < tbl.Columns.Count; x++)
                {

                    Object CurVal = dv[x];
                    if (CurVal is DBNull)
                        Nulls[x]++;
                    else if (CurVal is bool)
                        if ((bool)CurVal)
                            Trues[x]++;
                        else
                            Falses[x]++;
                    else
                    {
                        float InVal = float.NaN;
                        if (CurVal is Single || CurVal is Double || CurVal is Int32 || CurVal is Int64 || CurVal is Int32 || CurVal is UInt32 || CurVal is UInt16 || CurVal is Int16)
                            InVal = Convert.ToSingle(CurVal);

                        if (float.IsNaN(InVal))
                            Unknowns[x]++;
                        else
                        {
                            if (float.IsNaN(Min[x]) || Min[x] > InVal)
                                Min[x] = InVal;
                            if (float.IsNaN(Max[x]) || Max[x] < InVal)
                                Max[x] = InVal;
                            Count[x]++;
                            Sum[x] += InVal;
                            SumP[x] += Math.Abs(InVal);
                            Sum2[x] += InVal * InVal;
                            
                        }

                    }
                }
            }
                

            //Now, add in our tooltips
            for (int x = 0; x < tbl.Columns.Count; x++)
                if (Columns.Contains(tbl.Columns[x].ColumnName))
                {
                    StringBuilder OutTtip = new StringBuilder();
                    OutTtip.AppendLine(tbl.Columns[x].ColumnName);
                    OutTtip.AppendLine();
                    OutTtip.AppendLine("Rows: " + bindingSource.Count.ToString("#,##0"));
                    OutTtip.AppendLine("Valid: " + Count[x].ToString("#,##0"));

                    if (Unknowns[x] > 0)
                        OutTtip.AppendLine("Unknown: " + Unknowns[x].ToString("#,##0"));
                    if (Nulls[x]>0)
                        OutTtip.AppendLine("Nulls: " + Nulls[x].ToString("#,##0"));
                    OutTtip.AppendLine();

                    if (Trues[x] > 0)
                        OutTtip.AppendLine("True: " + Trues[x].ToString("#,##0"));
                    if (Falses[x] > 0)
                        OutTtip.AppendLine("False: " + Falses[x].ToString("#,##0"));


                    float StdDev = float.NaN;                    
                    if (Count[x] > 1)
                        StdDev = ((float)Math.Sqrt((((float)Count[x] * Sum2[x]) - (Sum[x] * Sum[x])) / ((float)Count[x] * ((float)Count[x] - 1f))));
                    if (!float.IsNaN(StdDev))
                        OutTtip.AppendLine("Average: " + (Sum[x] / Count[x]).ToString("#,##0.0") + " ± " + StdDev.ToString("#,##0.0"));
                    else if (Count[x] >= 1)
                        OutTtip.AppendLine("Average: " + (Sum[x] / Count[x]).ToString("#,##0.0"));


                    
                    if (Count[x] > 1 && Sum[x] != SumP[x] && Sum[x] != -SumP[x])
                        OutTtip.AppendLine("Abs. Average: " + (SumP[x] / Count[x]).ToString("#,##0.0") + " ± " + ((float)Math.Sqrt((((float)Count[x] * Sum2[x]) - (SumP[x] * SumP[x])) / ((float)Count[x] * ((float)Count[x] - 1f)))).ToString("#,##0.0"));

                    OutTtip.AppendLine();

                    if (!float.IsNaN(Max[x]))
                        OutTtip.AppendLine("Range:   " + Min[x].ToString("#,##0.0") + " to " + Max[x].ToString("#,##0.0"));

                   if (Count[x] > 1)
                       OutTtip.AppendLine("Sum:   " + Sum[x].ToString("#,##0.0"));

                   if (Count[x] > 1 && Sum[x] != SumP[x] && Sum[x] != -SumP[x])
                       OutTtip.AppendLine("Abs. Sum:   " + SumP[x].ToString("#,##0.0"));

                    this.Columns[tbl.Columns[x].ColumnName].ToolTipText = OutTtip.ToString();
                }


            this.CurrentCell = null;


        }
        

        /// <summary>
        /// Handle the row post-paint, in order to show violations when they occur.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                MM_Element ThisElem = (Rows[e.RowIndex].Cells["TEID"].Value as MM_Element);
                if (ThisElem == null)
                    return;
                MM_AlarmViolation_Type ViolType = ThisElem.WorstVisibleViolation(violView.ShownViolations, this.Parent);
                if (ViolType != null)
                    e.Graphics.DrawImage(MM_Repository.ViolationImages.Images[ViolType.ViolationIndex], new Rectangle(e.RowBounds.Location, new Size(this.RowHeadersWidth, e.RowBounds.Height)));
                else if (ThisElem.FindNotes().Length > 0)
                    e.Graphics.DrawImage(MM_Repository.ViolationImages.Images["Note"], new Rectangle(e.RowBounds.Location, new Size(this.RowHeadersWidth, e.RowBounds.Height)));
                base.OnRowPostPaint(e);
            }
            catch (Exception)
            { }
        }
    }
}
