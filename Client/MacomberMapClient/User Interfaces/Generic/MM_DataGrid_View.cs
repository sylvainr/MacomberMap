using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Menuing;
using MacomberMapClient.User_Interfaces.NetworkMap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacomberMapCommunications.Extensions;
using System.Reflection;

namespace MacomberMapClient.User_Interfaces.Generic
{
    /// <summary>
    /// (C) 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides an overload of the data grid view, that handles element-based DGV displays
    /// </summary>

    public class MM_DataGrid_View<T> : DataGridView where T: MM_Bound_Element
    {
        #region Variable declarations
        /// <summary>Our collection of bound elements associated with the display</summary>
        public BindingList<T> Elements = new BindingList<T>();

        /// <summary>The main map we're using for controls</summary>
        public MM_Network_Map_DX networkMap;

        /// <summary>Our popup menu</summary>
        public MM_Popup_Menu mnuPopup = new MM_Popup_Menu();

        /// <summary>The timer to refresh our content</summary>
        public Timer RefreshTimer = new Timer();

        /// <summary>Our collection of properties</summary>
        public List<PropertyInfo> Properties = new List<PropertyInfo>();
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our data grid view
        /// </summary>
        public MM_DataGrid_View()
        {
            this.DoubleBuffered = true;
            this.BackgroundColor = Color.Black;
            this.DefaultCellStyle.BackColor = Color.Black;
            this.DefaultCellStyle.ForeColor = Color.White;
            this.RowHeadersVisible = false;
            this.AllowUserToResizeRows = false;
            this.VirtualMode = true;
            this.CellValueNeeded += MM_DataGrid_View_CellValueNeeded;
            Elements = new BindingList<T>();
            DataSource = Elements;
            MM_KVLevel.MonitoringChanged += MM_KVLevel_MonitoringChanged;
            foreach (DataGridViewColumn dCol in Columns)
                Properties.Add(typeof(T).GetProperty(dCol.DataPropertyName));
        }

        /// <summary>
        /// Return our needed value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MM_DataGrid_View_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            T FoundElem = Elements[e.RowIndex];
            e.Value = Properties[e.ColumnIndex].GetValue(Elements[e.RowIndex]);
        }

        /// <summary>
        /// Refresh our screen every n seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            Invalidate();
            return;
            if (DateTime.Now.Second % 10 == 0 && SortedColumn != null)
            {
                bool SortAscending = SortedColumn.HeaderCell.SortGlyphDirection == System.Windows.Forms.SortOrder.Ascending;
                Elements.Sort(delegate(T X, T Y)
                {
                    IComparable InValX = (IComparable)X.GetType().GetProperty(SortedColumn.Name).GetValue(X);
                    IComparable InValY = (IComparable)Y.GetType().GetProperty(SortedColumn.Name).GetValue(Y);
                    return (SortAscending ? 1 : -1) * InValX.CompareTo(InValY);
                });
            }
            else
                Invalidate();
        }

        /// <summary>
        /// When our monitoring status changes, update our display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MM_KVLevel_MonitoringChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler(MM_KVLevel_MonitoringChanged), sender, e);
            else
                Invalidate();
        }


        /// <summary>
        /// When our column is shown, handle accordingly
        /// </summary>
        /// <param name="e"></param>
        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            if (e.Column.HeaderText == "Line" || e.Column.HeaderText == "Unit")
            { }
            else if (e.Column.HeaderText == "Frequency")
                e.Column.DefaultCellStyle.Format = "0.00 \\H\\z";
            else if (e.Column.HeaderText.Contains("PerUnit"))
                e.Column.DefaultCellStyle.Format = "0.00 \\P\\u";
            else if (e.Column.HeaderText.Contains("Voltage"))
                e.Column.DefaultCellStyle.Format = "0.0 \\k\\V";
            else if (e.Column.HeaderText.Contains("Percent"))
                e.Column.DefaultCellStyle.Format = "0%";
            else if (e.Column.HeaderText.Contains("MW"))
                e.Column.DefaultCellStyle.Format = "#,##0 \\M\\W";
            else if (e.Column.HeaderText.Contains("MVAR"))
                e.Column.DefaultCellStyle.Format = "#,##0 \\M\\V\\A\\R";
            else if (e.Column.HeaderText.Contains("MVA"))
                e.Column.DefaultCellStyle.Format = "#,##0 \\M\\V\\A";
            if (e.Column.HeaderText.Contains("_"))
                e.Column.HeaderText = e.Column.HeaderText.Substring(0, e.Column.HeaderText.LastIndexOf('_')).Replace('_',' ');
            e.Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            base.OnColumnAdded(e);            
        }

        
        #endregion

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
                T Element = (T)Rows[e.RowIndex].DataBoundItem;
                //determine our back color
                Color BackColor;
                if (MM_Server_Interface.ISQse || MM_Server_Interface.ClientAreas.ContainsKey("ERCOT"))
                BackColor = (SelectedCells.Count == 1 && SelectedCells[0].RowIndex == e.RowIndex) ? Color.DarkBlue : (Element.BaseElement.NearIsland != null && !float.IsNaN(Element.BaseElement.NearFrequency)) ? Color.DarkGreen : e.RowIndex % 2 == 0 ? Color.Black : Color.FromArgb(25, 25, 25);
                else
                    BackColor = (SelectedCells.Count == 1 && SelectedCells[0].RowIndex == e.RowIndex) ? Color.DarkBlue : e.RowIndex % 2 == 0 ? Color.Black : Color.FromArgb(25, 25, 25);
                //Check for voltage
                if (Columns[e.ColumnIndex].Name.Contains("PerUnit") && Element.BaseElement.KVLevel!= null && Element.BaseElement.KVLevel.MonitorEquipment)
                {
                    float InVal = (float)e.Value;
                    if (InVal != 0)
                        if (InVal > Element.BaseElement.KVLevel.HighVoltageAlert || InVal < Element.BaseElement.KVLevel.LowVoltageAlert)
                        {
                            BackColor = MM_Repository.OverallDisplay.ErrorColor;
                            e.CellStyle.ForeColor = MM_Repository.OverallDisplay.ErrorBackground;
                        }
                        else if (InVal > Element.BaseElement.KVLevel.HighVoltageWarning || InVal < Element.BaseElement.KVLevel.LowVoltageWarning)
                        {
                            BackColor = MM_Repository.OverallDisplay.WarningColor;
                            e.CellStyle.ForeColor = MM_Repository.OverallDisplay.WarningBackground;
                        }
                }
                else if (Columns[e.ColumnIndex].Name.Contains("Percent") && (Element.BaseElement.KVLevel != null && Element.BaseElement.KVLevel.MonitorEquipment))
                {
                    float InVal = (float)e.Value;
                    if (InVal != 0)
                        if (InVal > Element.BaseElement.KVLevel.ThermalAlert)
                        {
                            BackColor = MM_Repository.OverallDisplay.ErrorColor;
                            e.CellStyle.ForeColor = MM_Repository.OverallDisplay.ErrorBackground;
                        }
                        else if (InVal > Element.BaseElement.KVLevel.ThermalWarning)
                        {
                            BackColor = MM_Repository.OverallDisplay.WarningColor;
                            e.CellStyle.ForeColor = MM_Repository.OverallDisplay.WarningBackground;
                        }
                }
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
                if (MM_Server_Interface.ISQse || MM_Server_Interface.ClientAreas.ContainsKey("ERCOT"))
                    e.Value = ((MM_Unit)e.Value).SubName + " " + ((MM_Unit)e.Value).Name;
                else
                    e.Value = ((MM_Unit)e.Value).SubLongName + "." + ((MM_Unit)e.Value).Name;
            else if (e.Value is float && (float.IsNaN((float)e.Value) || (float)e.Value == 0f))
                e.Value = "";
            else if (e.Value is DateTime && (DateTime)e.Value != default(DateTime))
                e.Value = ((DateTime)e.Value).ToString("MM/dd/yyyy HH:mm:ss");
            else
                base.OnCellFormatting(e);
        }

        /// <summary>
        /// Handle our column header clicking by sorting.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
        {
            DataGridViewColumn SortColumn = Columns[e.ColumnIndex];
            bool SortAscending = SortColumn.HeaderCell.SortGlyphDirection == System.Windows.Forms.SortOrder.Ascending;
            Elements.Sort(delegate(T X, T Y) {
                IComparable InValX = (IComparable)X.GetType().GetProperty(SortColumn.Name).GetValue(X);
                IComparable InValY = (IComparable)Y.GetType().GetProperty(SortColumn.Name).GetValue(Y);
                return  (SortAscending ? 1 : -1) * InValX.CompareTo(InValY);
            });
            DataSource = Elements;
            foreach (DataGridViewColumn dCol in Columns)
                if (dCol != SortColumn)
                    dCol.HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.None;
                else if (SortAscending)
                    dCol.HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.Descending;
                else
                    dCol.HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.Ascending;
        }

        /// <summary>
        /// Handle our cell mouse click
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCellMouseClick(DataGridViewCellMouseEventArgs e)
        {
            base.OnCellMouseClick(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Right && e.RowIndex >= 0)
            {
                MM_Bound_Element Elem = (MM_Bound_Element)Rows[e.RowIndex].DataBoundItem;
                if (Elem != null)
                {
                    CurrentCell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                    Rectangle CellRect = GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                    mnuPopup.Show(this, new Point(CellRect.Left + e.X, CellRect.Top + e.Y), Elem.BaseElement, true);                    
                }
            }
        }


        /// <summary>
        /// When our selection changes, invaldate our data grid view
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(EventArgs e)
        {
            this.Invalidate();
            base.OnSelectionChanged(e);
        }
    }
}