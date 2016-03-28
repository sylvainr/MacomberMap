using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MacomberMap.UI.DgvFilterPopup
{



    /// <summary>
    /// A standard <i>column filter</i> implementation for ComboBox columns.
    /// </summary>
    /// <remarks>
    /// If the <b>DataGridView</b> column to which this <i>column filter</i> is applied
    /// is not a ComboBox column, it automatically creates a distinct list of values from the bound <b>DataView</b> column.
    /// If the DataView changes, you should do an explicit call to <see cref="DgvComboBoxColumnFilter.RefreshValues"/> method.
    /// </remarks>
    public partial class DgvCheckedListBoxColumn : DgvBaseColumnFilter
    {
        List<string> selectedItems = new List<string>();
        private bool suspendUpdating = false;

        internal override void deactivateFilter()
        {
            suspendUpdating = true;
            for (int i = 0; i < checkedListBoxValue.Items.Count; i++)
            {
                checkedListBoxValue.SetItemChecked(i, false);
            }
            suspendUpdating = false;
            RefreshValues(true);
            onFilterChanged(this, new EventArgs());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DgvComboBoxColumnFilter"/> class.
        /// </summary>
        public DgvCheckedListBoxColumn()
        {
            InitializeComponent();
            checkedListBoxValue.ItemCheck += new ItemCheckEventHandler(checkedListBoxValue_ItemCheck);
            checkedListBoxValue.MouseUp += new MouseEventHandler(checkedListBoxValue_MouseUp);
            checkedListBoxValue.MouseDown += new MouseEventHandler(checkedListBoxValue_MouseDown);
        }

        private void checkedListBoxValue_MouseDown(object sender, MouseEventArgs e)
        {
            int ifp = checkedListBoxValue.IndexFromPoint(e.Location);
            if (ifp >= 0)
            {
                checkedListBoxValue.SelectedIndex = ifp;
            }
        }

        private void checkedListBoxValue_MouseUp(object sender, MouseEventArgs e)
        {
            int ifp = checkedListBoxValue.IndexFromPoint(e.Location);
            if (ifp >= 0)
            {
                checkedListBoxValue.SelectedIndex = ifp;
                suspendUpdating = true;
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    for (int i = 0; i < checkedListBoxValue.Items.Count; i++)
                    {
                        if (i != checkedListBoxValue.SelectedIndex)
                            checkedListBoxValue.SetItemChecked(i, true);
                        else
                            checkedListBoxValue.SetItemChecked(i, false);
                    }
                }
                suspendUpdating = false;
                //else if (e.Button == System.Windows.Forms.MouseButtons.Left)
                //{
                //    bool currentCheck = checkedListBoxValue.GetItemChecked(ifp);
                //    System.Diagnostics.Debug.WriteLine(ifp.ToString() + ": "+ currentCheck.ToString());
                //    if (!currentCheck)
                //        checkedListBoxValue.SetItemChecked(ifp, !currentCheck);
                //}
                onFilterChanged(sender, e);
            }
        }

        private void checkedListBoxValue_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                selectedItems.Add(checkedListBoxValue.Items[e.Index].ToString());
            }
            else
            {
                selectedItems.Remove(checkedListBoxValue.Items[e.Index].ToString());
            }

            if (!suspendUpdating)
                onFilterChanged(sender, e);
        }


        /// <summary>
        /// Perform filter initialitazion and raises the FilterInitializing event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// When this <i>column filter</i> control is added to the <i>column filters</i> array of the <i>filter manager</i>,
        /// the latter calls the <see cref="DgvBaseColumnFilter.Init"/> method which, in turn, calls this method.
        /// You can ovverride this method to provide initialization code or you can create an event handler and 
        /// set the <i>Cancel</i> property of event argument to true, to skip standard initialization.
        /// </remarks>
        protected override void OnFilterInitializing(object sender, CancelEventArgs e)
        {
            base.OnFilterInitializing(sender, e);
            if (e.Cancel) return;

            RefreshValues();
            FilterCaption = OriginalDataGridViewColumnHeaderText;           
        }

        private void RefreshValues()
        {
            RefreshValues(false);
        }
        private void RefreshValues(bool append)
        {
            DataTable DistinctDataTable = this.BoundDataView.Table.DefaultView.ToTable(true, new string[] { this.DataGridViewColumn.DataPropertyName });
            DistinctDataTable.DefaultView.Sort = this.DataGridViewColumn.DataPropertyName;

            if (!append)
            {
                checkedListBoxValue.Items.Clear();
                selectedItems.Clear();
            }

            foreach (DataRowView item in DistinctDataTable.DefaultView)
            {
                string itemVal = (item[0] ?? "").ToString();
                if (!checkedListBoxValue.Items.Contains(itemVal))
                    checkedListBoxValue.Items.Add(itemVal);
            }
        }


        /// <summary>
        /// Builds the filter expression and raises the FilterExpressionBuilding event
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Override <b>OnFilterExpressionBuilding</b> to provide a filter expression construction
        /// logic and to set the values of the <see cref="DgvBaseColumnFilter.FilterExpression"/> and <see cref="DgvBaseColumnFilter.FilterCaption"/> properties.
        /// The <see cref="DgvFilterManager"/> will use these properties in constructing the whole filter expression and to change the header text of the filtered column.
        /// Otherwise, you can create an event handler and set the <i>Cancel</i> property of event argument to true, to skip standard filter expression building logic.
        /// </remarks>
        protected override void OnFilterExpressionBuilding(object sender, CancelEventArgs e)
        {
            base.OnFilterExpressionBuilding(sender, e);
            if (e.Cancel)
            {
                FilterManager.RebuildFilter();
                return;
            }

            string ResultFilterExpression = "";

            string ResultFilterCaption = OriginalDataGridViewColumnHeaderText;

            // Managing the NULL and NOT NULL cases which are type-independent
            //if (checkedListBoxValue.SelectedItems.Count == 0) ResultFilterExpression = GetNullCondition(this.DataGridViewColumn.DataPropertyName);

            //if (comboBoxOperator.Text == "= Ø") ResultFilterExpression = GetNullCondition(this.DataGridViewColumn.DataPropertyName);
            //if (comboBoxOperator.Text == "<> Ø") ResultFilterExpression = GetNotNullCondition(this.DataGridViewColumn.DataPropertyName);

            //if (ResultFilterExpression != "")
            //{
            //    FilterExpression = ResultFilterExpression;
            //    FilterCaption = ResultFilterCaption + "\n NOT NULL";
            //    FilterManager.RebuildFilter();
            //    return;
            //}


            foreach (var checkItem in selectedItems)
            {
                //string checkItem = (item ?? "").ToString();

                if (ResultFilterExpression != "") ResultFilterExpression += " OR ";

                if (ColumnDataType == typeof(string))
                {
                    // Managing the string-column case
                    string EscapedFilterValue = StringEscape(checkItem.ToString());
                    ResultFilterExpression += this.DataGridViewColumn.DataPropertyName + " = '" + EscapedFilterValue + "'";
                    ResultFilterCaption += "\n " + EscapedFilterValue;
                }
                else
                {
                    // Managing the other cases
                    string FormattedValue = FormatValue(checkItem, this.ColumnDataType);
                    if (FormattedValue != "")
                    {
                        ResultFilterExpression += this.DataGridViewColumn.DataPropertyName + " = "  + FormattedValue;
                        ResultFilterCaption += "\n " + FormattedValue;
                    }

                }
            }

            //string FormattedValue = "";


            //if (ResultFilterExpression != "")
            //{
                FilterExpression = ResultFilterExpression;
                FilterCaption = ResultFilterCaption;
                FilterManager.RebuildFilter();
            //}
        }

        private void onFilterChanged(object sender, EventArgs e)
        {
            if (!this.Visible) return;
            Active = true;
            FilterExpressionBuild();
        }

    }
}
