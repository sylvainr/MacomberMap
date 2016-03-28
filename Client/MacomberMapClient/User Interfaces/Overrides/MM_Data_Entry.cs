using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Summary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Overrides
{
    /// <summary>
    /// (C) 2013, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc.
    /// This class provides a data entry form for updating the status of a piece of equipment, adding in a violation, etc.
    /// </summary>
    public partial class MM_Data_Entry : MM_Form
    {
        #region Variable declarations
        /// <summary>The selected element</summary>
        public MM_Element SelectedElement;

        /// <summary>Our new violation</summary>
        public MM_AlarmViolation NewViolation = new MM_AlarmViolation();

        /// <summary>Our collection of display items for binding purposes</summary>
        private static Dictionary<Type, DataTable> DisplayItems = new Dictionary<Type, DataTable>();

        /// <summary>Our collection of changed values</summary>
        private List<String> ChangedValues = new List<string>();
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new data entry form
        /// </summary>
        public MM_Data_Entry()
        {
            InitializeComponent();

            //Add in all of our alarm/violation components
            int CurTop = 13;
            Label NewLabel = null;
            Control NewControl = null;
            foreach (FieldInfo fI in typeof(MM_AlarmViolation).GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                if (AddComponent(fI.Name, fI.FieldType, null, ref CurTop, out NewLabel, out NewControl))
                {
                    tabViolations.Controls.Add(NewLabel);
                    tabViolations.Controls.Add(NewControl);
                }

            //Add our submit button for a violation
            Button SubmitButton = new Button();
            SubmitButton.Text = "Generate Violation";
            SubmitButton.AutoSize = true;
            SubmitButton.Location = new Point(NewLabel.Left, NewControl.Bottom + 5);
            SubmitButton.Click += new EventHandler(SubmitButton_Click);
            tabViolations.Controls.Add(SubmitButton);

            //Set up our search results table
            srMain.ConfigureSearch();
            srMain.ItemSelectionChanged += new MM_Search_Results.ItemSelectionDelegate(srMain_ItemSelectionChanged);

            tabValueUpdates.AutoScroll = true;
            tabViolations.AutoScroll = true;
        }

        /// <summary>
        /// Load a data table with all the appropriate options
        /// </summary>
        /// <param name="TargetType"></param>
        /// <returns></returns>
        private static DataTable LoadOptionsTable(Type TargetType)
        {
            //First, look up our table in our dictionary
            DataTable OutTable;
            if (DisplayItems.TryGetValue(TargetType, out OutTable))
                return OutTable;

            //If not, create it            
            OutTable = new DataTable(TargetType.Name);
            OutTable.Columns.Add("Title", typeof(String));
            OutTable.Columns.Add("Value", TargetType);

            //Determine the array we should be searching through
            IEnumerable Options = null;
            if (TargetType == typeof(MM_AlarmViolation_Type))
                Options = MM_Repository.ViolationTypes.Values;
            else if (TargetType == typeof(MM_Contingency))
                Options = MM_Repository.Contingencies.Values;
            else if (TargetType == typeof(MM_Unit))
                Options = MM_Repository.Units.Values;
            else if (TargetType == typeof(MM_Unit_Type))
                Options = MM_Repository.GenerationTypes.Values;
            else if (TargetType == typeof(MM_Unit_Logical))
                Options = MM_Repository.Units.Values.OfType<MM_Unit_Logical>();
            else if (TargetType == typeof(MM_Substation))
                Options = MM_Repository.Substations.Values;
            else if (TargetType == typeof(MM_Company))
                Options = MM_Repository.Companies.Values;
            else if (TargetType == typeof(MM_KVLevel))
                Options = MM_Repository.KVLevels.Values;
            else if (TargetType.IsEnum)
                Options = Enum.GetValues(TargetType);
            else if (TargetType == typeof(MM_Element_Type))
                Options = MM_Repository.ElemTypes.Values;


            if (Options != null)
            {
                SortedDictionary<String, Object> Sorter = new SortedDictionary<string, object>();
                foreach (Object obj in Options)
                    if (!Sorter.ContainsKey(obj.ToString()))
                        Sorter.Add(obj.ToString(), obj);
                OutTable.Rows.Add("", null);
                foreach (KeyValuePair<String, Object> kvp in Sorter)
                    OutTable.Rows.Add(kvp.Key, kvp.Value);
            }
            else
                OutTable = null;
            DisplayItems.Add(TargetType, OutTable);
            return OutTable;
        }

        /// <summary>
        /// Add in a label and control
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="CurTop"></param>
        /// <param name="TargetType"></param>
        /// <param name="Value"></param>
        /// <param name="NewControl"></param>
        /// <param name="NewLabel"></param>
        private bool AddComponent(String Name, Type TargetType, Object Value, ref int CurTop, out Label NewLabel, out Control NewControl)
        {
            if (TargetType == typeof(MM_ScheduledOutage) || TargetType == typeof(MM_Element))
            {
                NewLabel = null;
                NewControl = null;
                return false;
            }
            //Create our new label
            NewLabel = new Label();
            NewLabel.Text = Name;
            NewLabel.Location = new Point(8, CurTop);
            NewLabel.AutoSize = true;

            //Dermine if we're a type that we have a dictionary for, and handle accordingly
            DataTable OptionsTable = LoadOptionsTable(TargetType);


            if (OptionsTable != null)
            {
                BindingSource NewSource = new BindingSource();
                NewSource.DataSource = OptionsTable;
                if (Value != null)
                    NewSource.Position = NewSource.Find("Value", Value);
                NewControl = new ComboBox() { DataSource = NewSource, ValueMember = "Value", DisplayMember = "Title", DropDownStyle = ComboBoxStyle.DropDownList, SelectedValue = Value };
                (NewControl as ComboBox).SelectedIndexChanged += new EventHandler(ComboValueChanged);
            }
            else if (TargetType == typeof(DateTime))
            {
                NewControl = new DateTimePicker() { Format = DateTimePickerFormat.Custom, CustomFormat = "MM/dd/yyyy HH:mm:ss", Value = Value == null ? DateTime.Now : (DateTime)Value };
                (NewControl as DateTimePicker).ValueChanged += new EventHandler(DateTimePickerValueChanged);
            }
            else if (TargetType == typeof(bool))
            {
                NewControl = new CheckBox() { Text = "", Checked = Value == null ? false : (bool)Value };
                (NewControl as CheckBox).CheckedChanged += new EventHandler(CheckBoxValueChanged);
            }
            else
            {
                NewControl = new TextBox() { Text = Value == null ? "" : Value.ToString() };
                (NewControl as TextBox).TextChanged += TextBoxTextChanged;
            }
            NewControl.Tag = Name;
            NewControl.Location = new Point(NewLabel.Right + 5, CurTop);
            CurTop = NewControl.Bottom + 5;
            return true;
        }

        /// <summary>
        /// Assign a value that we've specified
        /// </summary>
        /// <param name="TargetObject"></param>
        /// <param name="FieldName"></param>
        /// <param name="TargetValue"></param>
        private void AssignValue(Object TargetObject, String FieldName, Object TargetValue)
        {
            if (TargetObject == null)
                return;
            try
            {
                if (TargetValue is DataRowView)
                    TargetValue = (TargetValue as DataRowView).Row[1];
                if (TargetValue is DBNull)
                    TargetValue = null;
                ChangedValues.Add(FieldName);
                string[] splStr = FieldName.TrimEnd(']').Split('[');
                foreach (MemberInfo mI in TargetObject.GetType().GetMember(splStr[0]))
                    if (splStr.Length == 1 && mI is FieldInfo)
                        (mI as FieldInfo).SetValue(TargetObject, ChangeType(TargetValue, (mI as FieldInfo).FieldType));
                    else if (splStr.Length == 1 && mI is PropertyInfo)
                        (mI as PropertyInfo).SetValue(TargetObject, ChangeType(TargetValue, (mI as PropertyInfo).PropertyType), null);
                    else
                    {
                        Array InVal;
                        Type SingleType;
                        if (mI is FieldInfo)
                        {
                            InVal = (Array)(mI as FieldInfo).GetValue(TargetObject);
                            SingleType = (mI as FieldInfo).FieldType.GetElementType();
                        }
                        else
                        {
                            InVal = (Array)(mI as PropertyInfo).GetValue(TargetObject, null);
                            SingleType = (mI as PropertyInfo).PropertyType.GetElementType();

                        }
                        InVal.SetValue(ChangeType(TargetValue, SingleType), int.Parse(splStr[1]));
                    }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.MessageBox("Error setting " + FieldName + " for " + TargetObject + "\n" + ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Change the type of our object
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="TargetType"></param>
        /// <returns></returns>
        private Object ChangeType(Object Value, Type TargetType)
        {

            if (TargetType == typeof(Single) && Value is string)
            {
                float OutVal;
                if (float.TryParse(Value.ToString(), out OutVal))
                    return OutVal;
                else
                    return float.NaN;
            }
            else if (TargetType == typeof(Double) && Value is string)
            {
                double OutVal;
                if (double.TryParse(Value.ToString(), out OutVal))
                    return OutVal;
                else
                    return double.NaN;
            }
            else
                return Convert.ChangeType(Value, TargetType);
        }
        #endregion

        #region Event handlers for user input
        /// <summary>
        /// Handle a key press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxTextChanged(object sender, EventArgs e)
        {

            TextBox ctl = (TextBox)sender;
            AssignValue((ctl.Parent == tabValueUpdates ? SelectedElement : NewViolation), ctl.Tag.ToString(), ctl.Text);

        }

        /// <summary>
        /// Handle a check box value change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxValueChanged(object sender, EventArgs e)
        {
            CheckBox ctl = (CheckBox)sender;
            AssignValue((ctl.Parent == tabValueUpdates ? SelectedElement : NewViolation), ctl.Tag.ToString(), ctl.Checked);
        }

        /// <summary>
        /// Handle a combo box index change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DateTimePickerValueChanged(object sender, EventArgs e)
        {
            DateTimePicker ctl = (DateTimePicker)sender;
            AssignValue((ctl.Parent == tabValueUpdates ? SelectedElement : NewViolation), ctl.Tag.ToString(), ctl.Value);
        }

        /// <summary>
        /// Handle a combo box value change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboValueChanged(object sender, EventArgs e)
        {
            ComboBox ctl = (ComboBox)sender;
            AssignValue((ctl.Parent == tabValueUpdates ? SelectedElement : NewViolation), ctl.Tag.ToString(), ctl.SelectedItem);
        }

        /// <summary>
        /// Handle the submission of a new violation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if (SelectedElement == null)
                MM_System_Interfaces.MessageBox("Please select an element to be violated.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                try
                {
                    Data_Integration.CheckAddViolation(NewViolation);
                }
                catch (Exception ex)
                {
                    MM_System_Interfaces.MessageBox("Error generating specific alarm: " + ex.Message + "\n" + ex.StackTrace, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            //Now, clear our components and reset our violation
            NewViolation = new MM_AlarmViolation();
            NewViolation.ViolatedElement = SelectedElement;
            foreach (Control ctl in tabViolations.Controls)
                if (ctl is ComboBox)
                    (ctl as ComboBox).SelectedItem = null;
                else if (ctl is TextBox)
                    (ctl as TextBox).Text = "";
                else if (ctl is CheckBox)
                    (ctl as CheckBox).Checked = false;
        }

        #endregion

        /// <summary>
        /// Select an element after updating our search form
        /// </summary>
        /// <param name="SelectedElement"></param>
        public void SelectElement(MM_Element SelectedElement)
        {
            if (InvokeRequired)
                BeginInvoke(new MM_Search_Results.ItemSelectionDelegate(SelectElement), SelectedElement);
            else
            {
                srMain.SearchText = SelectedElement.TEID.ToString();
                srMain.PerformSearch(false, false);
            }
        }


        /// <summary>
        /// Handle the selection of an item
        /// </summary>
        /// <param name="SelectedElement"></param>
        private void srMain_ItemSelectionChanged(MM_Element SelectedElement)
        {
            try
            {
                this.SelectedElement = SelectedElement;
                NewViolation.ViolatedElement = SelectedElement;

                Button Trigger1 = new Button(), Trigger2 = new Button();
                Trigger1.Text = Trigger2.Text = "Trigger Value Change";
                Trigger1.AutoSize = Trigger2.AutoSize = true;
                Trigger1.Location = new Point(13, 8);
                Trigger1.Click += new EventHandler(TriggerUpdate);
                Trigger2.Click += new EventHandler(TriggerUpdate);
                ChangedValues.Clear();
                int CurTop = Trigger1.Bottom + 5;
                SortedDictionary<String, MemberInfo> Members = new SortedDictionary<string, MemberInfo>();
                if (SelectedElement != null)
                    foreach (MemberInfo mI in SelectedElement.GetType().GetMembers())
                        if (mI is FieldInfo || (mI is PropertyInfo && (mI as PropertyInfo).CanRead && (mI as PropertyInfo).CanWrite))
                            Members.Add(mI.Name, mI);
                Label NewLabel;
                Control NewControl;
                List<Label> Labels = new List<Label>();
                List<Control> Controls = new List<Control>();
                if (SelectedElement != null)
                    Controls.Add(Trigger1);
                foreach (MemberInfo mI in Members.Values)
                    if (mI.Name != "TEID")
                    {
                        Object inVal = null;
                        Type inType = null;
                        if (mI is PropertyInfo)
                        {
                            inVal = (mI as PropertyInfo).GetValue(SelectedElement, null);
                            inType = (mI as PropertyInfo).PropertyType;
                        }
                        else if (mI is FieldInfo)
                        {
                            inVal = (mI as FieldInfo).GetValue(SelectedElement);
                            inType = (mI as FieldInfo).FieldType;
                        }
                        if (inType.IsArray)
                        {
                            int ThisVal = 0;
                            if (inVal != null)
                                foreach (Object inObj in inVal as System.Collections.IEnumerable)
                                    if (ThisVal <= 10)
                                        if (AddComponent(mI.Name + "[" + ThisVal++ + "]", inObj.GetType(), inObj, ref CurTop, out NewLabel, out NewControl))
                                        {
                                            Labels.Add(NewLabel);
                                            Controls.Add(NewControl);
                                        }
                        }
                        else if (inVal is System.Collections.IDictionary || inVal is System.Collections.IList)
                        { }
                        else if (AddComponent(mI.Name, inType, inVal, ref CurTop, out NewLabel, out NewControl))
                        {
                            Labels.Add(NewLabel);
                            Controls.Add(NewControl);
                        }
                    }
                Trigger2.Top = CurTop;
                if (SelectedElement != null)
                    Controls.Add(Trigger2);

                tabValueUpdates.Controls.Clear();
                tabValueUpdates.SuspendLayout();
                if (Labels.Count > 0)
                    tabValueUpdates.Controls.AddRange(Labels.ToArray());
                int MaxWidth = 0;
                foreach (Label lbl in Labels)
                    MaxWidth = Math.Max(MaxWidth, lbl.Width);

                foreach (Control ctl in Controls)
                {
                    ctl.Left = Labels[0].Left + MaxWidth + 5;
                    ctl.Width = tabValueUpdates.ClientRectangle.Width - ctl.Left - 25;
                }
                tabValueUpdates.Controls.AddRange(Controls.ToArray());
                tabValueUpdates.ResumeLayout();
            }
            catch
            { }
        }

        /// <summary>
        /// Trigger a value update event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TriggerUpdate(object sender, EventArgs e)
        {
            foreach (String str in ChangedValues)
                SelectedElement.TriggerValueChange(str);
            ChangedValues.Clear();
        }
    }
}