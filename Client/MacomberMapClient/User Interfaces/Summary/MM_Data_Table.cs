using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Menuing;
using MacomberMapClient.User_Interfaces.NetworkMap;
using MacomberMapClient.User_Interfaces.Violations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Summary
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// The data table provides an interface into the core data
    /// </summary>
    public class MM_Data_Table : DataGridView
    {
        #region Variable declarations
        /// <summary>The property page associated with this item</summary>
        public MM_Property_Page AssociatedPropertyPage;

        /// <summary>This flag will prevent the property page from firing during a refresh</summary>
        private bool SettingDataSource = false;

        /// <summary>The menu for handling right clicks on a cell</summary>
        private MM_Popup_Menu RightClickMenu = new MM_Popup_Menu();

        /// <summary>The network map associated with the large display (for zooming/panning the map on right-click</summary>
        private MM_Network_Map_DX nMap;

        /// <summary>The violation viewer associated with this display</summary>
        private MM_Violation_Viewer violView;

        /// <summary>The mini-map associated with this display</summary>
        private MM_Mini_Map miniMap;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new data table
        /// </summary>
        /// <param name="AssociatedPropertyPage">The property page associated with the table</param>
        /// <param name="miniMap">The mini-map associated with the table</param>
        /// <param name="nMap">The network map associated with the table</param>
        /// <param name="violView">The violation view assocaited with the table</param>
        public MM_Data_Table(MM_Property_Page AssociatedPropertyPage, MM_Network_Map_DX nMap, MM_Violation_Viewer violView, MM_Mini_Map miniMap)
        {
            this.nMap = nMap;
            this.miniMap = miniMap;
            this.AssociatedPropertyPage = AssociatedPropertyPage;
            this.AllowUserToAddRows = false;
            this.AllowUserToResizeColumns = true;
            this.violView = violView;
            this.AllowUserToResizeRows = true;
            this.AllowUserToOrderColumns = true;
            if (MM_Server_Interface.Client != null)
                this.EditMode = DataGridViewEditMode.EditProgrammatically;
            else
            {
                this.EditMode = DataGridViewEditMode.EditOnEnter;
                this.CellValueChanged += new DataGridViewCellEventHandler(Data_Table_CellValueChanged);
            }


            this.DataError += new DataGridViewDataErrorEventHandler(Data_Table_DataError);
            this.BackgroundColor = Color.Black;
            this.DefaultCellStyle.BackColor = Color.Black;
            this.DefaultCellStyle.ForeColor = Color.White;
            this.RowHeadersVisible = false;
            this.AllowUserToResizeRows = false;
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
            MM_System_Interfaces.LogError(e.Exception);
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
                if (SettingDataSource || MM_Server_Interface.Client == null)
                    return;
                else if (this.SelectedCells.Count == 1)
                    AssociatedPropertyPage.SetElement(this.CurrentRow.Cells["TEID"].Value as MM_Element);
                else
                    AssociatedPropertyPage.SetElement(null);
                base.OnSelectionChanged(e);
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Error updating data table selection: " + ex.Message);
            }
        }

        /// <summary>
        /// Handle the right-click event on a cell
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCellMouseClick(DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1) return;
            MM_Element Elem = Rows[e.RowIndex].Cells["TEID"].Value as MM_Element;
            SettingDataSource = true;
            this.CurrentCell = Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (MM_Server_Interface.Client != null)
                AssociatedPropertyPage.SetElement(Elem);
            SettingDataSource = false;
            if (e.Button == MouseButtons.Right)
                RightClickMenu.Show(this, e.Location, Elem, true);

            base.OnCellMouseClick(e);
            if (e.Button == MouseButtons.Left)
                FindForm().Refresh();
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
                    if (Nulls[x] > 0)
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


        /// <summary>
        /// Handle the painting of our cell
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1)
                base.OnCellPainting(e);

            else
            {
                //Now, determine our back color;
                Color BackColor = (SelectedCells.Count == 1 && SelectedCells[0].RowIndex == e.RowIndex) ? Color.DarkBlue : e.RowIndex % 2 == 0 ? Color.Black : Color.FromArgb(25, 25, 25);              
                using (SolidBrush sB = new SolidBrush(BackColor))
                    e.Graphics.FillRectangle(sB, e.CellBounds);

                //Draw our vertical lines if we're selected
                if (SelectedCells.Count == 1 && SelectedCells[0].RowIndex == e.RowIndex)
                {
                    e.Graphics.DrawLine(Pens.DarkGray, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Top);
                    e.Graphics.DrawLine(Pens.DarkGray, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                }
                e.PaintContent(e.ClipBounds);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle the formatting of our cell
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is MM_Substation)
                e.Value = ((MM_Substation)e.Value).Name;
            else if (e.Value is MM_Island)
                e.Value = ((MM_Island)e.Value).ID.ToString();
            else if (e.Value is MM_Unit)
                e.Value = ((MM_Unit)e.Value).SubLongName + "." + ((MM_Unit)e.Value).Name;
            else if (e.Value is float && (float.IsNaN((float)e.Value) || (float)e.Value == 0f))
                e.Value = "";
            else if (Columns[e.ColumnIndex].Name.Contains("Percent") && e.Value is float)
                e.Value = ((float)e.Value).ToString("0.0%");
            else
                base.OnCellFormatting(e);
        }

    }
}
