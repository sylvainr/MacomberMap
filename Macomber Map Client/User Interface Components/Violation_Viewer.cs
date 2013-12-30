using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using Macomber_Map.Data_Connections;
using System.Collections;
using System.Runtime.InteropServices;
using Macomber_Map.User_Interface_Components.NetworkMap;
using System.Drawing.Drawing2D;
using Macomber_Map.User_Interface_Components.OneLines;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This control displays violations, as customizable by the user
    /// </summary>
    public partial class Violation_Viewer : UserControl
    {
        #region Variable declarations
        /// <summary>The mini-map associated with the violation view</summary>
        private Mini_Map miniMap;

        /// <summary>Our update timer</summary>
        private Timer UpdateTimer = new Timer();

        /// <summary>The network map associated with the violation view</summary>
        public Network_Map networkMap;

        /// <summary>The one-line viewer associated with the violation view</summary>
        private OneLine_Viewer oneLine;

        /// <summary>The base element for the violation viewer. If none, it's the VV for the overall display.</summary>
        private MM_Element[] BaseElements = null;

        /// <summary>The item comparer for the list view</summary>
        private ListViewItemComparer lvSorter = new ListViewItemComparer();

        /// <summary>The button for storing totals and new data</summary>
        private AlarmViolation_Button NewButton;

        /// <summary>The collection of violations by violation type</summary>        
        private Dictionary<MM_AlarmViolation_Type, AlarmViolation_Button> TypeButtons;

        /// <summary>The collection of violations by violation type and voltage</summary>
        private Dictionary<KeyValuePair<MM_AlarmViolation_Type, MM_KVLevel>, AlarmViolation_Button> TypeVoltageButtons;

        /// <summary>The collection of violations by violation type and element type</summary>
        private Dictionary<KeyValuePair<MM_AlarmViolation_Type, MM_Element_Type>, AlarmViolation_Button> TypeElementButtons;

        /// <summary>The collection of violations shown on the screen</summary>
        public Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations = new Dictionary<MM_AlarmViolation, ListViewItem>();

        /// <summary>Our list of shown violations</summary>
        public ListViewItem[] ShownViolationList = new ListViewItem[0];

        /// <summary>Return the UID of the selected violation</summary>
        public Dictionary<MM_AlarmViolation, MM_AlarmViolation> SelectedUIDs
        {
            get
            {
                if (lvViolations.InvokeRequired)
                    return lvViolations.Invoke(new SafeGetUIDs(RetrieveUIDs)) as Dictionary<MM_AlarmViolation, MM_AlarmViolation>;
                else
                    return RetrieveUIDs();
            }
        }

        /// <summary>
        /// A delegate for retrieving UIDs
        /// </summary>
        /// <returns></returns>
        private delegate Dictionary<MM_AlarmViolation, MM_AlarmViolation> SafeGetUIDs();

        /// <summary>
        /// Retrieve the IDs of violations
        /// </summary>
        /// <returns></returns>
        private Dictionary<MM_AlarmViolation, MM_AlarmViolation> RetrieveUIDs()
        {
            Dictionary<MM_AlarmViolation, MM_AlarmViolation> outViols = new Dictionary<MM_AlarmViolation, MM_AlarmViolation>(lvViolations.SelectedIndices.Count);
            if (!lvViolations.VirtualMode)
                foreach (ListViewItem lvViol in lvViolations.SelectedItems)
                {
                    MM_AlarmViolation Viol = (MM_AlarmViolation)lvViol.Tag;
                    outViols.Add(Viol, Viol);
                }
            else
                foreach (KeyValuePair<MM_AlarmViolation, ListViewItem> kvp in ShownViolations)
                    if (kvp.Value.Selected)
                        outViols.Add(kvp.Key, kvp.Key);

            return outViols;
        }

        /// <summary>The right click menu for the violation viewer</summary>
        private MM_Popup_Menu RightClickMenu;

        /// <summary>The background brush</summary>
        private Brush BackBrush = null;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the violation viewer
        /// </summary>        
        public Violation_Viewer()
        {
            InitializeComponent();
            BackBrush = new SolidBrush(lvViolations.BackColor);
            lvViolations.View = View.Details;
            //Also override the background drawing on the toolstrip, to match our background.
            tsSummary.Renderer = new AlarmViolation_Renderer(this);
            //Set our handler for mouse down within the list view (not viewable from editor GUI)
            lvViolations.MouseUp += new MouseEventHandler(lvViolations_MouseUp);
            UpdateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            UpdateTimer.Interval = 500;
            lvViolations.FullRowSelect = true;

            //Set up the right-click menu viewer
            RightClickMenu = new MM_Popup_Menu();
            Data_Integration.NetworkSourceChanged += new EventHandler(Data_Integration_NetworkSourceChanged);
        }

        /// <summary>
        /// When the network source changes, refresh the toolstrip background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_Integration_NetworkSourceChanged(object sender, EventArgs e)
        {
            if (tsSummary.InvokeRequired)
                tsSummary.Invoke(new EventHandler(Data_Integration_NetworkSourceChanged), sender, e);
            else
            {
                tsSummary.Invalidate();
                tsSummary.Refresh();
            }
        }



        /// <summary>
        /// Set the controls and data sources associated with the violation viewer
        /// </summary>
        /// <param name="miniMap">The associated mini-map</param>
        /// <param name="networkMap">The associated network map</param>
        /// <param name="BaseElements">The base element for the violations</param>
        public void SetControls(Mini_Map miniMap, Network_Map networkMap, MM_Element[] BaseElements)
        {
            this.BaseElements = BaseElements;
            SetControls(miniMap, networkMap);
        }

        /// <summary>
        /// Associate this control with the one-line control
        /// </summary>
        /// <param name="oneLine">The one line control</param>
        public void SetControls(OneLine_Viewer oneLine)
        {
            this.oneLine = oneLine;
        }

        /// <summary>
        /// Set the controls and data sources associated with the violation viewer
        /// </summary>
        /// <param name="miniMap">The associated mini-map</param>
        /// <param name="networkMap">The associated network map</param>
        public void SetControls(Mini_Map miniMap, Network_Map networkMap)
        {
            //Assign our internal variables

            MM_Repository.ViewChanged += new MM_Repository.ViewChangedDelegate(Repository_ViewChanged);
            this.miniMap = miniMap;
            this.networkMap = networkMap;

            //Set up our columns
            lvViolations.Columns.Add("").Tag = ListViewItemComparer.ComparisonTypeEnum.ViolType;
            ColumnHeader KVCol = lvViolations.Columns.Add("KV");
            KVCol.Width = 26;
            KVCol.Tag = ListViewItemComparer.ComparisonTypeEnum.Voltage;
            lvViolations.Columns.Add("Elem").Tag = ListViewItemComparer.ComparisonTypeEnum.Name;
            lvViolations.Columns.Add("Text").Tag = ListViewItemComparer.ComparisonTypeEnum.Title;
            lvViolations.Columns.Add("Time").Tag = ListViewItemComparer.ComparisonTypeEnum.Date;
            lvViolations.Columns.Add("Operator").Tag = ListViewItemComparer.ComparisonTypeEnum.Operator;
            lvViolations.ListViewItemSorter = lvSorter;
            this.lvViolations.SmallImageList = MM_Repository.ViolationImages;
            this.lvViolations.LargeImageList = MM_Repository.ViolationImages;


            //Set up the list view images and sorter, and the summary bar image source                        
            this.tsSummary.ImageList = MM_Repository.ViolationImages;

            //Create our collection of buttons and our violation collections
            CreateSummaryButtons();

            //Prepare to handle incoming violations
            Data_Integration.ViolationAdded += _ViolationAdded;
            Data_Integration.ViolationModified += _ViolationModified;
            Data_Integration.ViolationRemoved += _ViolationRemoved;
            Data_Integration.ViolationAcknowledged += _ViolationAcknowledged;

            //Update our collection of violations.

            if (BaseElements == null)
            {
                foreach (MM_AlarmViolation Violation in new List<MM_AlarmViolation>(MM_Repository.Violations.Keys))
                    _ViolationAdded(Violation);
            }
            else
                foreach (MM_Element BaseElement in BaseElements)
                {
                    foreach (MM_AlarmViolation Violation in BaseElement.Violations.Values)
                        _ViolationAdded(Violation);
                    if (BaseElement is MM_Substation)
                        foreach (MM_Line Line in MM_Repository.Lines.Values)
                            if (Line.Substation1.Equals(BaseElement) || Line.Substation2.Equals(BaseElement))
                                foreach (MM_AlarmViolation Viol in Line.Violations.Keys)
                                    _ViolationAdded(Viol);

                }

            //Set our group color            
            SetListViewGroupColor(this.lvViolations, Color.FromArgb(0x00cccccc));

            //Assign our right click menus
            RightClickMenu.ImageList = MM_Repository.ViolationImages;
        }






        private delegate void SafeChangeView(MM_Display_View ActiveView);

        /// <summary>
        /// Handle the change of active view by altering our image colors
        /// </summary>
        /// <param name="ActiveView"></param>
        private void Repository_ViewChanged(MM_Display_View ActiveView)
        {
            if (lvViolations.InvokeRequired)
                lvViolations.Invoke(new SafeChangeView(Repository_ViewChanged), ActiveView);
            else
            {
                this.lvViolations.SmallImageList = null;
                this.lvViolations.LargeImageList = null;
                this.tsSummary.ImageList = null;

                //Now go through all of our views, and set our index
                foreach (AlarmViolation_Button btn in tsSummary.Items)
                    if (btn.BaseType != null)
                    {
                        btn.ImageIndex = -1;
                        btn.ImageIndex = btn.BaseType.ViolationIndex;
                    }

                foreach (ListViewItem l in ShownViolations.Values)
                    l.ImageIndex = (l.Tag as MM_AlarmViolation).Type.ViolationIndex;

                this.lvViolations.SmallImageList = MM_Repository.ViolationImages;
                this.lvViolations.LargeImageList = MM_Repository.ViolationImages;
                this.tsSummary.ImageList = MM_Repository.ViolationImages;

                this.Invalidate();
            }
        }

        /// <summary>
        /// Handle the modification of a violation
        /// </summary>
        /// <param name="Violation"></param>
        private void _ViolationModified(MM_AlarmViolation Violation)
        {
            ListViewItem FoundViol;
            if (lvViolations.InvokeRequired)
                lvViolations.Invoke(new SafeViolationModification(_ViolationModified), Violation);
            else if (ShownViolations.TryGetValue(Violation, out FoundViol))
            {
                FoundViol.SubItems[3].Text = Violation.Text;
                FoundViol.Font = new Font(lvViolations.Font, (Violation.New ? FontStyle.Bold : FontStyle.Regular));
            }
        }

        /// <summary>
        /// Handle the acknowledgement of a violation
        /// </summary>
        /// <param name="Violation"></param>
        private void _ViolationAcknowledged(MM_AlarmViolation Violation)
        {
            ListViewItem FoundViol;

            if (lvViolations.InvokeRequired)
                lvViolations.Invoke(new SafeViolationModification(_ViolationAcknowledged), Violation);
            else if (ShownViolations.TryGetValue(Violation, out FoundViol))
            {
                if (NewButton.Checked)
                    RemoveViolationFromDisplay(Violation);
                else
                    FoundViol.Font = new Font(lvViolations.Font, FontStyle.Regular);

                //Now, update the newness status
                if (TypeButtons[Violation.Type].Violations.ContainsKey(Violation))
                    AcknowledgeButtonViolation(TypeButtons[Violation.Type], Violation, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.None);


                KeyValuePair<MM_AlarmViolation_Type, MM_KVLevel> ViolKV = new KeyValuePair<MM_AlarmViolation_Type, MM_KVLevel>(Violation.Type, Violation.KVLevel);
                if (ViolKV.Value != null && TypeVoltageButtons[ViolKV].Violations.ContainsKey(Violation))
                    AcknowledgeButtonViolation(TypeVoltageButtons[ViolKV], Violation, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.Voltage);


                KeyValuePair<MM_AlarmViolation_Type, MM_Element_Type> ViolElem = new KeyValuePair<MM_AlarmViolation_Type, MM_Element_Type>(Violation.Type, Violation.ElemType);
                AlarmViolation_Button FoundButton;
                if (TypeElementButtons.TryGetValue(ViolElem, out FoundButton) && FoundButton.Violations.ContainsKey(Violation))
                    //if (TypeElementButtons[ViolElem]Violations.ContainsKey(Violation))
                    AcknowledgeButtonViolation(TypeElementButtons[ViolElem], Violation, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.ViolType);

            }
        }


        private void _ViolationRemoved(MM_AlarmViolation Violation)
        {
            //First, remove the violation from all the relevant lists         
            if (TypeButtons[Violation.Type].Violations.ContainsKey(Violation))
                RemoveViolationFromButton(TypeButtons[Violation.Type], Violation, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.None);


            KeyValuePair<MM_AlarmViolation_Type, MM_KVLevel> ViolKV = new KeyValuePair<MM_AlarmViolation_Type, MM_KVLevel>(Violation.Type, Violation.KVLevel);
            if (ViolKV.Value != null && TypeVoltageButtons[ViolKV].Violations.ContainsKey(Violation))
                RemoveViolationFromButton(TypeVoltageButtons[ViolKV], Violation, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.Voltage);


            KeyValuePair<MM_AlarmViolation_Type, MM_Element_Type> ViolElem = new KeyValuePair<MM_AlarmViolation_Type, MM_Element_Type>(Violation.Type, Violation.ViolatedElement.ElemType);
            AlarmViolation_Button FoundButton;
            if (TypeElementButtons.TryGetValue(ViolElem, out FoundButton) && FoundButton.Violations.ContainsKey(Violation)) //may not need FoundButton.Violations && condition - Merge with ym&mn
                RemoveViolationFromButton(TypeElementButtons[ViolElem], Violation, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.ViolType);

            RemoveViolationFromDisplay(Violation);

        }

        #region Handling of violation additions and removals
        /// <summary>
        /// Add a violation to the button
        /// </summary>
        /// <param name="btnType">The button to which the violation should be added</param>
        /// <param name="Violation">The violation to add</param>
        /// <param name="Visibility">The visiblity to add</param>
        private void AddViolationToButton(AlarmViolation_Button btnType, MM_AlarmViolation Violation, bool Visibility)
        {
            //First, add the violation into all of the relevant lists.
            //Console.WriteLine("Adding violation: " + Violation + " to " + (this.oneLine == null ? "Main" : "OneLine") + " for " + btnType.Name); //removed to speed up violations - mn - 6-4-13

            btnType.Violations.Add(Violation, Violation);
            if (Violation.New)
            {
                NewButton.New++;
                btnType.New++;
            }
            btnType.Total++;
            NewButton.Total++;
            UpdateSummaryButton(btnType, Visibility);
        }

        /// <summary>
        /// Remove a violation from a button
        /// </summary>
        /// <param name="btnType">The button to which the violation should be added</param>
        /// <param name="Violation">The violation to add</param>
        /// <param name="Visibility">The visiblity to add</param>
        private void RemoveViolationFromButton(AlarmViolation_Button btnType, MM_AlarmViolation Violation, bool Visibility)
        {
            //Console.WriteLine("Removing violation: " + Violation + " from " + (this.oneLine == null ? "Main" : "OneLine") + " for " + btnType.Name); //removed to speed up violations - mn - 6-4-13
            btnType.Violations.Remove(Violation);
            if (Violation.New)
            {
                btnType.New--;
                NewButton.New--;
            }
            btnType.Total--;
            NewButton.Total--;
            UpdateSummaryButton(btnType, Visibility);
        }

        /// <summary>
        /// Acknowledge a button violation
        /// </summary>
        /// <param name="btnType">The button to which the violation should be added</param>
        /// <param name="Violation">The violation to add</param>
        /// <param name="Visibility">The visiblity to add</param>
        private void AcknowledgeButtonViolation(AlarmViolation_Button btnType, MM_AlarmViolation Violation, bool Visibility)
        {
            btnType.New--;
            NewButton.New--;
            UpdateSummaryButton(btnType, Visibility);
        }

        private delegate void SafeUpdateSummaryButton(AlarmViolation_Button btnType, bool Visibility);


        /// <summary>
        /// Update a summary button to reflect the new count
        /// </summary>
        /// <param name="btnType">The button type</param>
        /// <param name="Visibility">Whether the button should be visible</param>
        private void UpdateSummaryButton(AlarmViolation_Button btnType, bool Visibility)
        {
            if (InvokeRequired)
                Invoke(new SafeUpdateSummaryButton(UpdateSummaryButton), btnType, Visibility);
            else
            {
                string[] splStr = btnType.Name.Split('-');

                if (btnType.New > 0)
                {
                    btnType.Text = (btnType.VoltageLevel == null ? "" : btnType.VoltageLevel.Name.Split(' ')[0] + ": ") + (btnType.ElemType == null ? "" : btnType.ElemType.Acronym + ": ") + btnType.New.ToString("#,##0") + " / " + btnType.Total.ToString("#,##0");
                    btnType.Font = new Font(btnType.Font, FontStyle.Bold);
                }
                else
                {
                    btnType.Text = (btnType.VoltageLevel == null ? "" : btnType.VoltageLevel.Name.Split(' ')[0] + ": ") + (btnType.ElemType == null ? "" : btnType.ElemType.Acronym + ": ") + btnType.Total.ToString("#,##0");
                    btnType.Font = new Font(btnType.Font, FontStyle.Regular);
                }

                btnType.Visible = Visibility && btnType.Total > 0;
                btnType.ToolTipText = btnType.Name + "\n" + btnType.New.ToString("#,##0") + " new\n" + btnType.Total.ToString("#,##0") + " total";

                //Now, update the new button
                if (NewButton.New > 0)
                    NewButton.Font = new Font(NewButton.Font, FontStyle.Bold);
                else
                    NewButton.Font = new Font(NewButton.Font, FontStyle.Regular);
                NewButton.ToolTipText = NewButton.New.ToString("#,##0") + " new\n" + NewButton.Total.ToString("#,##0") + " total";
            }
        }
        /// <summary>
        /// Add a new violation into the view
        /// </summary>
        /// <param name="Violation"></param>
        private void _ViolationAdded(MM_AlarmViolation Violation)
        {
            //First, check to see whether this violation viewer is the appropriate repository for this violation.
            if (this.BaseElements != null)
            {
                //If we have a line for a breaker-to-breaker, show it.
                bool Include = false;
                foreach (MM_Element BaseElement in BaseElements)
                    if (BaseElement is MM_Contingency)
                        Include = true;
                    else if (BaseElement is MM_Substation && Violation.ViolatedElement is MM_Line && ((Violation.ViolatedElement as MM_Line).Substation1.Equals(BaseElement) || (Violation.ViolatedElement as MM_Line).Substation2.Equals(BaseElement)))
                        Include = true;
                    else if ((Violation.ViolatedElement is MM_Substation || Violation.ViolatedElement is MM_Line) && Array.IndexOf(BaseElements, Violation.ViolatedElement) == -1)
                        Include = false;
                    else if (Violation.ViolatedElement.Substation != null && Array.IndexOf(BaseElements, Violation.ViolatedElement.Substation) == -1)
                        Include = false;
                    else
                        Include = true;
                if (!Include)
                    return;
            }
            try
            {
                if (lvViolations.InvokeRequired) //-crash here - nataros
                    lvViolations.Invoke(new SafeViolationModification(_ViolationAdded), Violation);
                else
                {
                    //Determine our appropriate violation type
                    MM_AlarmViolation_Type ThisType = null;
                    foreach (MM_AlarmViolation_Type TypeFinder in TypeButtons.Keys)
                        if (TypeFinder.Name == Violation.Type.Name)
                            ThisType = TypeFinder;


                    if (!TypeButtons[ThisType].Violations.ContainsKey(Violation))
                    {
                        AddViolationToButton(TypeButtons[ThisType], Violation, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.None);

                        //Now, add the violation to the KV level.

                        KeyValuePair<MM_AlarmViolation_Type, MM_KVLevel> ViolKV = new KeyValuePair<MM_AlarmViolation_Type, MM_KVLevel>(null, null);
                        if (Violation.KVLevel != null)
                        {
                            ViolKV = new KeyValuePair<MM_AlarmViolation_Type, MM_KVLevel>(ThisType, Violation.KVLevel);
                            AddViolationToButton(TypeVoltageButtons[ViolKV], Violation, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.Voltage);
                        }

                        KeyValuePair<MM_AlarmViolation_Type, MM_Element_Type> ViolElem = new KeyValuePair<MM_AlarmViolation_Type, MM_Element_Type>(ThisType, Violation.ViolatedElement.ElemType);
                        AlarmViolation_Button FoundBtn;
                        if (TypeElementButtons.TryGetValue(ViolElem, out FoundBtn))
                            AddViolationToButton(FoundBtn, Violation, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.ViolType);

                        //Now, if this violation should be shown, let's show it.
                        if (ThisType.ViolationViewer == MM_AlarmViolation_Type.DisplayModeEnum.Always)
                            AddViolationToDisplay(Violation);
                        else if (ThisType.ViolationViewer == MM_AlarmViolation_Type.DisplayModeEnum.Never)
                            return;
                        else if ((lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.None) && (TypeButtons[ThisType].Checked))
                            AddViolationToDisplay(Violation);
                        else if ((lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.Voltage) && (TypeVoltageButtons[ViolKV].Checked))
                            AddViolationToDisplay(Violation);
                        else if ((lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.ViolType) && (TypeElementButtons[ViolElem].Checked))
                            AddViolationToDisplay(Violation);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// Add a violation to the display
        /// </summary>
        /// <param name="Violation"></param>
        private void AddViolationToDisplay(MM_AlarmViolation Violation)
        {
            ListViewItem NewLVItem = new ListViewItem(" ", Violation.Type.ViolationIndex);
            if (Violation.New)
                NewLVItem.Font = new Font(lvViolations.Font, FontStyle.Bold);
            if (Violation.Notes.Count > 0)
                NewLVItem.Font = new Font(lvViolations.Font, FontStyle.Italic);
            if (Violation.KVLevel != null)
                NewLVItem.SubItems.Add(Violation.KVLevel.Name.Split(' ')[0]);
            else if (Violation.ViolatedElement.KVLevel != null)
                NewLVItem.SubItems.Add(Violation.ViolatedElement.KVLevel.Name.Split(' ')[0]);
            else
                NewLVItem.SubItems.Add("");

            NewLVItem.SubItems.Add(Violation.ViolatedElement.ElementDescription());
            NewLVItem.SubItems.Add(Violation.Text);
            NewLVItem.SubItems.Add(Violation.Recency);
            NewLVItem.SubItems.Add(Violation.ViolatedElement.Operator == null ? (Violation.ViolatedElement.Substation != null && Violation.ViolatedElement.Substation.Operator != null ? Violation.ViolatedElement.Substation.Operator.Alias : "") : Violation.ViolatedElement.Operator.Alias);

            if (Violation.KVLevel == null)
                NewLVItem.ToolTipText = Violation.Type.Name + " - " + Violation.ViolatedElement.ElemType.Name + " at " + Violation.EventTime.ToString();
            else
                NewLVItem.ToolTipText = Violation.Type.Name + " - " + Violation.ViolatedElement.ElemType.Name + " - " + Violation.KVLevel.Name + " at " + Violation.EventTime.ToString();



            //Check each item's width
            for (int x = 0; x < NewLVItem.SubItems.Count; x++)
            {
                int MeasureWidth = (int)Math.Ceiling(Graphics.FromHwnd(IntPtr.Zero).MeasureString(NewLVItem.SubItems[x].Text, lvViolations.Font).Width);
                if (MeasureWidth > lvViolations.Columns[x].Width)
                    lvViolations.Columns[x].Width = MeasureWidth;
            }
            //if ((btn.ForeColor != Color.White) && (btn.ForeColor != Color.Black))
            //    NewLVItem.ForeColor = Viol.Value.KVLevel.Energized.ForeColor;
            NewLVItem.Tag = Violation;
            NewLVItem.Group = FindGroup(Violation);

            ShownViolations.Add(Violation, NewLVItem);
            if (lvViolations.VirtualMode)
                TriggerUpdate();
            else
                lvViolations.Items.Add(NewLVItem);
            miniMap.Invalidate();

            if (oneLine != null)
                oneLine.RefreshOneLineItem(Violation.ViolatedElement);
        }

        /// <summary>
        /// Update violations for a particular alarm type (when the property is changed)
        /// </summary>
        /// <param name="ViolType">The alarm violation type</param>
        public void UpdateViolations(MM_AlarmViolation_Type ViolType)
        {
            if (ViolType.ViolationViewer == MM_AlarmViolation_Type.DisplayModeEnum.Always)
            {
                foreach (MM_AlarmViolation Viol in ViolType.Violations.Values)
                    if (!ShownViolations.ContainsKey(Viol))
                        AddViolationToDisplay(Viol);
            }
            else if (ViolType.ViolationViewer == MM_AlarmViolation_Type.DisplayModeEnum.Never)
            {
                foreach (MM_AlarmViolation Viol in ViolType.Violations.Values)
                    if (ShownViolations.ContainsKey(Viol))
                        RemoveViolationFromDisplay(Viol);
            }
            else if (ViolType.ViolationViewer == MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected)
            {
                foreach (AlarmViolation_Button btn in tsSummary.Items)
                    if (btn.Visible)
                        foreach (MM_AlarmViolation btnViol in btn.Violations.Values)
                            if (ShownViolations.ContainsKey(btnViol) && !btn.Checked)
                                RemoveViolationFromDisplay(btnViol);
                            else if (!ShownViolations.ContainsKey(btnViol) && btn.Checked)
                                AddViolationToDisplay(btnViol);
            }
        }


        /// <summary>
        /// This delegate handles safe addition and removal of violations from the list view.
        /// </summary>
        /// <param name="Violation"></param>
        private delegate void SafeViolationModification(MM_AlarmViolation Violation);


        /// <summary>
        /// Remove a violation from the display
        /// </summary>
        /// <param name="Violation">The violation to be removed</param>
        private void RemoveViolationFromDisplay(MM_AlarmViolation Violation)
        {
            if (lvViolations.InvokeRequired)
                lvViolations.Invoke(new SafeViolationModification(RemoveViolationFromDisplay), Violation);
            else
            {
                //If the alarm is displayed on the listview, remove it
                if (ShownViolations.ContainsKey(Violation))
                {
                    ShownViolations.Remove(Violation);
                    if (lvViolations.VirtualMode)
                        TriggerUpdate();
                    else
                        lvViolations.Items.Remove(ShownViolations[Violation]);

                    miniMap.Invalidate();

                    //Update the one-lines, if we have them.
                    if (oneLine != null)
                        oneLine.RefreshOneLineItem(Violation.ViolatedElement);
                }
            }
        }
        #endregion


        /// <summary>
        /// Trigger an update
        /// </summary>
        private void TriggerUpdate()
        {
            UpdateTimer.Start();
        }

        /// <summary>
        /// Handle our update timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimer.Stop();
            Console.WriteLine("UpdateTimer: " + DateTime.Now.ToString());
            List<ListViewItem> OutList = new List<ListViewItem>(ShownViolations.Values);
            OutList.Sort(lvSorter);
            ShownViolationList = OutList.ToArray();
            if (lvViolations.VirtualListSize == ShownViolations.Count)
                lvViolations.VirtualListSize = 0;
            lvViolations.VirtualListSize = ShownViolations.Count;
            lvViolations.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        /// <summary>
        /// Create the collection of summary buttons, both by type and by type/voltage
        /// </summary>
        private void CreateSummaryButtons()
        {
            TypeButtons = new Dictionary<MM_AlarmViolation_Type, AlarmViolation_Button>(MM_Repository.ViolationTypes.Count);
            TypeVoltageButtons = new Dictionary<KeyValuePair<MM_AlarmViolation_Type, MM_KVLevel>, AlarmViolation_Button>(MM_Repository.ViolationTypes.Count * MM_Repository.KVLevels.Count);
            TypeElementButtons = new Dictionary<KeyValuePair<MM_AlarmViolation_Type, MM_Element_Type>, AlarmViolation_Button>(MM_Repository.ViolationTypes.Count * MM_Repository.ElemTypes.Count);

            //Add in our 'new' button
            NewButton = new AlarmViolation_Button(null);
            NewButton.Text = "New";
            NewButton.Font = new Font(NewButton.Font, FontStyle.Bold);
            NewButton.ForeColor = Color.White;
            NewButton.CheckOnClick = false;
            NewButton.ImageAlign = ContentAlignment.MiddleLeft;
            NewButton.TextAlign = ContentAlignment.MiddleRight;
            NewButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            NewButton.Click += new EventHandler(HandleSummaryItem_Click);
            NewButton.Visible = true;
            NewButton.Checked = false;
            {
                Bitmap b = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.Clear(Color.LightBlue);
                    using (StringFormat sF = new StringFormat())
                    {
                        sF.Alignment = StringAlignment.Center;
                        sF.LineAlignment = StringAlignment.Center;
                        sF.Trimming = StringTrimming.None;
                        sF.FormatFlags = StringFormatFlags.NoWrap;
                        using (Brush OutBrush = new SolidBrush(Color.FromArgb(32, 32, 32)))
                            g.DrawString("*", new Font("Arial", 12, FontStyle.Bold), OutBrush, new RectangleF(0, 0, 16, 16), sF);
                    }
                }
                NewButton.Image = b;
            }
            tsSummary.Items.Add(NewButton);


            foreach (MM_AlarmViolation_Type ViolType in MM_Repository.ViolationTypes.Values)
            {
                TypeButtons.Add(ViolType, CreateToolStripButton(ViolType.ViolationIndex, ViolType.Name, Color.White, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.None, ViolType, null));
                foreach (MM_KVLevel KVLevel in ViolType.ViolationsByVoltage.Keys)
                    TypeVoltageButtons.Add(new KeyValuePair<MM_AlarmViolation_Type, MM_KVLevel>(ViolType, KVLevel), CreateToolStripButton(ViolType.ViolationIndex, ViolType.Name + " - " + KVLevel.Name, KVLevel.Energized.ForeColor, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.Voltage, ViolType, KVLevel));
                foreach (MM_Element_Type ElemType in ViolType.ViolationsByElementType.Keys)
                    TypeElementButtons.Add(new KeyValuePair<MM_AlarmViolation_Type, MM_Element_Type>(ViolType, ElemType), CreateToolStripButton(ViolType.ViolationIndex, ViolType.Name + " - " + ElemType.Name, Color.White, lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.ElemType, ViolType, ElemType));
            }
            UpdateAlarmButtons();
        }


        /// <summary>
        /// Create a new ToolStrip button containing the specified image, title and color
        /// </summary>
        /// <param name="ImageNumber">The image number for this violation type</param>
        /// <param name="ButtonTitle">The tooltip description of the button</param>
        /// <param name="TextColor">The color for the count text</param>                
        /// <param name="Visible">Whether the button should be visible</param>
        /// <param name="BaseType">The base type for the button (e.g., forced outage)</param>
        /// <param name="SecondaryValue">The secondary value (e.g., null, voltage, element type)</param>
        /// <returns></returns>
        private AlarmViolation_Button CreateToolStripButton(int ImageNumber, String ButtonTitle, Color TextColor, bool Visible, MM_AlarmViolation_Type BaseType, Object SecondaryValue)
        {
            AlarmViolation_Button SummaryItem = new AlarmViolation_Button(SecondaryValue);
            SummaryItem.BaseType = BaseType;
            SummaryItem.ImageIndex = BaseType.ViolationIndex;
            SummaryItem.Name = ButtonTitle;
            SummaryItem.ImageAlign = ContentAlignment.MiddleLeft;
            SummaryItem.TextAlign = ContentAlignment.MiddleRight;
            SummaryItem.ToolTipText = ButtonTitle;
            SummaryItem.Text = "0";
            SummaryItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            SummaryItem.ForeColor = TextColor;
            SummaryItem.CheckOnClick = false;
            SummaryItem.Click += new EventHandler(HandleSummaryItem_Click);
            SummaryItem.Visible = Visible;
            SummaryItem.Checked = true;


            //Set the intended color.
            if (SummaryItem.VoltageLevel != null)
                SummaryItem.ForeColor = SummaryItem.VoltageLevel.Energized.ForeColor;
            else
                SummaryItem.ForeColor = MM_Repository.OverallDisplay.ForegroundColor;

            if (SummaryItem.Checked)
                SummaryItem.BackColor = MM_Repository.OverallDisplay.BackgroundColor;
            else
                SummaryItem.BackColor = SummaryItem.Owner.BackColor;

            tsSummary.Items.Add(SummaryItem);
            return SummaryItem;
        }

        /// <summary>
        /// Handle the clicking of a button. Swap white/black backgrounds for readability.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandleSummaryItem_Click(object sender, EventArgs e)
        {
            AlarmViolation_Button SummaryItem = (AlarmViolation_Button)sender;

            //If the selected button is the new button, handle accordingly.
            if (SummaryItem == NewButton)
            {
                //First, properly set the checked status of the button
                NewButton.Checked ^= true;

                List<MM_AlarmViolation> ViolToHandle = new List<MM_AlarmViolation>();

                //Go through and add all non-new violations to the list.                
                foreach (AlarmViolation_Button Btn in TypeButtons.Values)
                    if (Btn.Checked)
                        foreach (MM_AlarmViolation Viol in Btn.Violations.Values)
                            if (NewButton.Checked && !Viol.New && ShownViolations.ContainsKey(Viol))
                                ViolToHandle.Add(Viol);
                            else if (!NewButton.Checked && !Viol.New && !ShownViolations.ContainsKey(Viol))
                                ViolToHandle.Add(Viol);


                foreach (MM_AlarmViolation Viol in ViolToHandle)
                    if (NewButton.Checked)
                        RemoveViolationFromDisplay(Viol);
                    else
                        AddViolationToDisplay(Viol);
            }

            else if (SummaryItem.Checked)
            {

                SummaryItem.Checked = false;

                //If the summary item is unchecked, make sure we've removed all old references to it 
                List<MM_AlarmViolation> Viol = new List<MM_AlarmViolation>();
                foreach (KeyValuePair<MM_AlarmViolation, MM_AlarmViolation> Violation in SummaryItem.Violations)
                    if (ShownViolations.ContainsKey(Violation.Key) && Violation.Value.Type.ViolationViewer != MM_AlarmViolation_Type.DisplayModeEnum.Always)
                        Viol.Add(Violation.Value);

                foreach (MM_AlarmViolation ViolToRemove in Viol)
                    RemoveViolationFromDisplay(ViolToRemove);
            }
            else
            {


                SummaryItem.Checked = true;
                foreach (KeyValuePair<MM_AlarmViolation, MM_AlarmViolation> Violation in SummaryItem.Violations)
                    if (!ShownViolations.ContainsKey(Violation.Key) && (Violation.Value.New || !NewButton.Checked) && Violation.Value.Type.ViolationViewer != MM_AlarmViolation_Type.DisplayModeEnum.Never)
                        AddViolationToDisplay(Violation.Value);
            }
            //Set the intended color.
            if (SummaryItem.VoltageLevel != null)
                SummaryItem.ForeColor = SummaryItem.VoltageLevel.Energized.ForeColor;
            else
                SummaryItem.ForeColor = MM_Repository.OverallDisplay.ForegroundColor;

            //SummaryItem.BackColor = SummaryItem.Owner.BackColor;

        }

        #endregion

        #region Violation View Updating





        /// <summary>
        /// Determine the appropriate grouping for an item
        /// </summary>
        /// <param name="Viol">The violation to group</param>
        /// <returns>The appropriate group for the item, which is created if necessary</returns>
        private ListViewGroup FindGroup(MM_AlarmViolation Viol)
        {
            String ViolTitle = "?";
            if (lvSorter.Grouping == ListViewItemComparer.ComparisonTypeEnum.Substation)
                if (Viol.ViolatedElement is MM_Line)
                    ViolTitle = "Line";
                else if (Viol.ViolatedElement is MM_Substation)
                    ViolTitle = (Viol.ViolatedElement as MM_Substation).LongName;
                else if (Viol.ViolatedElement.Substation != null)
                    ViolTitle = Viol.ViolatedElement.Substation.LongName;
                else
                    ViolTitle = "?";
            else if (lvSorter.Grouping == ListViewItemComparer.ComparisonTypeEnum.Voltage)
                ViolTitle = (Viol.KVLevel == null) ? "None" : Viol.KVLevel.Name;
            else if (lvSorter.Grouping == ListViewItemComparer.ComparisonTypeEnum.Date)
                ViolTitle = Viol.EventTime.ToString("dddd HH:00 - HH:59");
            else if (lvSorter.Grouping == ListViewItemComparer.ComparisonTypeEnum.Recency)
                ViolTitle = Viol.Recency;
            else if (lvSorter.Grouping == ListViewItemComparer.ComparisonTypeEnum.Owner)
                ViolTitle = Viol.ViolatedElement.Owner == null ? "Unknown" : Viol.ViolatedElement.Owner.Name;
            else if (lvSorter.Grouping == ListViewItemComparer.ComparisonTypeEnum.Operator)
                ViolTitle = Viol.ViolatedElement.Operator == null ? "Unknown" : Viol.ViolatedElement.Operator.Name;
            else if (lvSorter.Grouping == ListViewItemComparer.ComparisonTypeEnum.ViolType)
                ViolTitle = Viol.Type.Name;
            else if (lvSorter.Grouping == ListViewItemComparer.ComparisonTypeEnum.ElemType)
                ViolTitle = Viol.ElemType.Name;
            else if (lvSorter.Grouping == ListViewItemComparer.ComparisonTypeEnum.Contingency)
                ViolTitle = Viol.Contingency == null ? "None" : Viol.Contingency.Description;

            if (ViolTitle == "?")
                return null;
            else if (lvViolations.Groups[ViolTitle] != null)
                return lvViolations.Groups[ViolTitle];
            else
                return lvViolations.Groups.Add(ViolTitle, ViolTitle);
        }
        #endregion

        #region User Interactions
        /// <summary>
        /// If our selection has changed, refresh the associated mini-map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleNewSelection(object sender, EventArgs e)
        {
            miniMap.Invalidate();

        }
        /// <summary>
        /// Handle the resizing of the violation viewer, by updating the tile size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViolationViewer_Resize(object sender, EventArgs e)
        {
            try
            {
                lvViolations.TileSize = new Size(DisplayRectangle.Width - 24, lvViolations.Font.Height * lvViolations.Columns.Count);
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Handle the mouse clicking within the summary viewer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsSummary_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            AlarmViolation_Button FoundItem = (AlarmViolation_Button)tsSummary.GetItemAt(e.Location);
            if (FoundItem == null)
                RightClickMenu.Show(this, e.Location, tsSummary, lvSorter);
            else
                RightClickMenu.Show(this, e.Location, FoundItem, lvSorter);
        }

        private void lvViolations_MouseUp(object sender, MouseEventArgs e)
        {

            ListViewHitTestInfo ht = lvViolations.HitTest(e.Location);
            if (ht.Item != null && e.Button == MouseButtons.Right)
                // ((ht.Location == ListViewHitTestLocations.Label && ht.Item.SubItems.IndexOf(ht.SubItem) == 0) || (ht.Location == ListViewHitTestLocations.Image))
                PopupMenu(ht, e.Location);
            else if (ht.Item != null && e.Button == MouseButtons.Left)
            {
                MM_AlarmViolation Viol = ht.Item.Tag as MM_AlarmViolation;
                //UserInteraction_Viewer FoundUI = null;
                //if (Viol.ViolatedElement != null && Data_Integration.UserInteractions.TryGetValue(Viol.ViolatedElement.Name, out FoundUI))
                //    FoundUI.ShowForm();
                if (Data_Integration.Permissions.OTS == true && oneLine != null) //parameterize out for ots vs rt-st - nataros //r-rtst
                    oneLine.HighlightElement(Viol.ViolatedElement);
            }
            else if (Data_Integration.Permissions.OTS == true && (ht.Item == null && e.Button == MouseButtons.Left && oneLine != null)) //r-rtst
                oneLine.HighlightElement(null);
            else if (e.Button == MouseButtons.Right)
                RightClickMenu.Show(this, new Point(e.X + lvViolations.Left, e.Y + lvViolations.Top), tsSummary, lvSorter);
        }
        #endregion

        #region Menu handling
        /// <summary>
        /// Display the popup menu for a listview item
        /// </summary>
        /// <param name="ht">The hit-test results of the item</param>
        /// <param name="Location">The location to show the menu</param>
        private void PopupMenu(ListViewHitTestInfo ht, Point Location)
        {
            RightClickMenu.Items.Clear();

            //If we're selecting multiple items, handle accordingly
            if (ht.Item.Selected && ht.Item.ListView.SelectedIndices.Count > 1)
            {
                List<MM_AlarmViolation> Violations = new List<MM_AlarmViolation>();
                int NewViol = 0;
                foreach (ListViewItem I in ht.Item.ListView.SelectedItems)
                {
                    if ((I.Tag as MM_AlarmViolation).New)
                        NewViol++;
                    Violations.Add(I.Tag as MM_AlarmViolation);
                }
                RightClickMenu.Show(this, Location, Violations, NewViol, true);
            }
            else
                RightClickMenu.Show(this, Location, ht.Item.Tag as MM_Element, true);

        }



        /// <summary>
        /// Update the alarm buttons, showing only the requested ones, and altering visibility depending on the counts
        /// </summary>
        private void UpdateAlarmButtons()
        {
            foreach (AlarmViolation_Button btn in TypeButtons.Values)
                btn.Visible = ((lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.None) & btn.Violations.Count > 0);
            foreach (AlarmViolation_Button btn in TypeVoltageButtons.Values)
                btn.Visible = ((lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.Voltage) & btn.Violations.Count > 0);
            foreach (AlarmViolation_Button btn in TypeElementButtons.Values)
                btn.Visible = ((lvSorter.Split == ListViewItemComparer.ComparisonTypeEnum.ElemType) & btn.Violations.Count > 0);
        }


        #endregion

        #region Misc. API calls
        /// <summary>
        /// This structure holds the GroupMetrics list
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct LVGroupMetrics
        {
            public int cbSize;
            public int mask;
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int crLeft;
            public int crTop;
            public int crRight;
            public int crBottom;
            public int crHeader;
            public int crFooter;
        }
        [DllImport("User32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVGroupMetrics lParam);


        /// <summary>
        /// Send a Win32 API call to the ListView control, in order to specify the color for the group.
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="LVGroupColor"></param>
        public static void SetListViewGroupColor(ListView lv, Color LVGroupColor)
        {
            LVGroupMetrics GroupMetrics = new LVGroupMetrics();
            GroupMetrics.cbSize = Marshal.SizeOf(GroupMetrics);
            GroupMetrics.mask = 4;
            GroupMetrics.crHeader = LVGroupColor.ToArgb();
            int Result = (int)SendMessage(lv.Handle, 4096 + 155, 0, ref GroupMetrics);
        }
        #endregion

        #region Item comparison class
        /// <summary>
        /// This class is responsible for comparing items in order to properly sort the list
        /// </summary>
        public class ListViewItemComparer : IComparer<ListViewItem>, IComparer
        {
            #region Variable Declarations
            /// <summary>The different comparison types available</summary>
            public enum ComparisonTypeEnum
            {
                /// <summary>Title</summary>
                Title,
                /// <summary>The name of our element</summary>
                Name,
                /// <summary>Violation Type</summary>
                ViolType,
                /// <summary>Element Type</summary>
                ElemType,
                /// <summary>Date / hour</summary>
                Date,
                /// <summary>Recency</summary>
                Recency,
                /// <summary>Voltage</summary>
                Voltage,
                /// <summary>Substation</summary>
                Substation,
                /// <summary>Owner</summary>
                Owner,
                /// <summary>Operator</summary>
                Operator,
                /// <summary>Contingency Definition</summary>
                Contingency,
                /// <summary>None</summary>
                None
            };

            /// <summary>The current comparison type</summary>
            public ComparisonTypeEnum ComparisonType = ComparisonTypeEnum.Title;

            /// <summary>The current grouping</summary>
            public ComparisonTypeEnum Grouping = ComparisonTypeEnum.None;

            /// <summary>The current split (none, by voltage, or by element type)</summary>
            public ComparisonTypeEnum Split = ComparisonTypeEnum.None;
            #endregion

            /// <summary>
            /// Compare two alarm violations
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(ListViewItem x, ListViewItem y)
            {
                MM_AlarmViolation Viol1 = (MM_AlarmViolation)x.Tag;
                MM_AlarmViolation Viol2 = (MM_AlarmViolation)y.Tag;
                if (ComparisonType == ComparisonTypeEnum.Title)
                    return String.Compare(Viol1.Text, Viol2.Text);
                else if (ComparisonType == ComparisonTypeEnum.Name)
                    return Viol1.ViolatedElement.ToString().CompareTo(Viol2.ViolatedElement.ToString());
                else if (ComparisonType == ComparisonTypeEnum.Date)
                    return DateTime.Compare(Viol1.EventTime, Viol2.EventTime);
                else if (ComparisonType == ComparisonTypeEnum.ViolType)
                    return (Viol1.Type.ViolationIndex.CompareTo(Viol2.Type.ViolationIndex));
                else if (ComparisonType == ComparisonTypeEnum.ElemType)
                    return (Viol1.ElemType.Name.CompareTo(Viol2.ElemType.Name));
                else if (ComparisonType == ComparisonTypeEnum.Operator)
                    return x.SubItems[5].Text.CompareTo(y.SubItems[5].Text);
                else if (ComparisonType == ComparisonTypeEnum.Voltage)
                    return -Viol1.ViolatedElement.KVLevel.Nominal.CompareTo(Viol2.ViolatedElement.KVLevel.Nominal);
                else
                    return 0;
            }

            /// <summary>
            /// Compare two alarm violations
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(object x, object y)
            {
                return Compare(x as ListViewItem, y as ListViewItem);
            }
        }
        #endregion

        #region Alarm violation button class
        /// <summary>
        /// This class holds an a summary button for an alarm violation
        /// </summary>
        public class AlarmViolation_Button : ToolStripButton
        {
            /// <summary>The collection of violations for this button</summary>
            public Dictionary<MM_AlarmViolation, MM_AlarmViolation> Violations = new Dictionary<MM_AlarmViolation, MM_AlarmViolation>();

            /// <summary>The voltage level of our alarm button</summary>
            public MM_KVLevel VoltageLevel = null;

            /// <summary>The element type of our alarm button</summary>
            public MM_Element_Type ElemType = null;


            /// <summary>The Alarm violation type</summary>
            public MM_AlarmViolation_Type BaseType;

            /// <summary>The total number of violations within the type</summary>
            public int Total
            {
                get { return _Total; }
                set { _Total = value; }
            }
            private int _Total = 0;

            /// <summary>The number of new violations within the type</summary>
            public int New
            {
                get { return _New; }
                set { _New = value; }
            }
            private int _New;

            /// <summary>
            /// Initialize a new alarm button with detailed information
            /// </summary>
            /// <param name="InVal"></param>
            public AlarmViolation_Button(Object InVal)
            {
                VoltageLevel = InVal as MM_KVLevel;
                ElemType = InVal as MM_Element_Type;
            }



        }

        /// <summary>
        /// Handle the rendering of the buttons 
        /// </summary>
        partial class AlarmViolation_Renderer : ToolStripProfessionalRenderer
        {
            /// <summary>Our parent viewer</summary>
            private Violation_Viewer Parent;

            /// <summary>
            /// Initialize the renderer
            /// </summary>
            /// <param name="Parent"></param>
            public AlarmViolation_Renderer(Violation_Viewer Parent)
            {
                this.Parent = Parent;
            }

            /// <summary>
            /// Handle the rendering of our background of a button
            /// </summary>
            /// <param name="e"></param>
            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
            {

                Rectangle DrawRect = new Rectangle(0, 0, e.Item.Width - 1, e.Item.Height - 1);
                ControlPaint.DrawBorder(e.Graphics, DrawRect, Color.Blue, (e.Item as ToolStripButton).Checked ? ButtonBorderStyle.Solid : ButtonBorderStyle.None);

            }

            /// <summary>
            /// Handle the rendering of our parent
            /// </summary>
            /// <param name="e"></param>
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {

                if (Data_Integration.NetworkSource.UseMasterForViolations)
                    e.Graphics.FillRectangle(Parent.BackBrush, e.AffectedBounds);
                else
                    using (Brush bR = new SolidBrush(Data_Integration.NetworkSource.BackColor))
                        e.Graphics.FillRectangle(bR, e.AffectedBounds);
            }
        }
        #endregion


        /// <summary>
        /// Handle the column double-click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvViolations_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ColumnHeader col = lvViolations.Columns[e.Column];
            lvSorter.ComparisonType = (ListViewItemComparer.ComparisonTypeEnum)col.Tag;
            TriggerUpdate();
            //lvSorter.ComparisonType = ListViewItemComparer.ComparisonTypeEnum.
        }

        /// <summary>
        /// Set the sorting parameter for the violation viewer
        /// </summary>
        /// <param name="SortType"></param>
        public void SetSort(Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum SortType)
        {
            lvSorter.ComparisonType = SortType;
            TriggerUpdate();
        }

        /// <summary>
        /// Set the splitting parameter for the violation viewer
        /// </summary>
        /// <param name="SplitType"></param>
        public void SetSplit(Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum SplitType)
        {
            lvSorter.Split = SplitType;
            UpdateAlarmButtons();
        }

        /// <summary>
        /// Set the grouping parameter for the violation viewer
        /// </summary>
        /// <param name="GroupType"></param>
        public void SetGrouping(Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum GroupType)
        {
            lvSorter.Grouping = GroupType;
            lvViolations.Groups.Clear();

            //Now assign ownership to all groups.
            if (GroupType == ListViewItemComparer.ComparisonTypeEnum.None)
                lvViolations.ShowGroups = false;
            else
            {
                foreach (ListViewItem I in ShownViolations.Values)
                    I.Group = FindGroup((MM_AlarmViolation)I.Tag);
                lvViolations.ShowGroups = true;
            }
        }

        /// <summary>
        /// Handle a group-level selection
        /// </summary>
        /// <param name="Selection"></param>
        public void HandleSelection(String Selection)
        {
            switch (Selection)
            {
                case "Select all":
                    foreach (ToolStripButton btn in tsSummary.Items)
                        if (btn.Visible && !btn.Checked && btn.Text != "New")
                            HandleSummaryItem_Click(btn, null);
                    break;
                case "Select none":
                    foreach (ToolStripButton btn in tsSummary.Items)
                        if (btn.Visible && btn.Checked && btn.Text != "New")
                            HandleSummaryItem_Click(btn, null);
                    break;

                case "Invert selection":
                    foreach (ToolStripButton btn in tsSummary.Items)
                        if (btn.Visible && btn.Text != "New")
                            HandleSummaryItem_Click(btn, null);
                    break;
            }
        }

        /// <summary>
        /// Search through summary items for a particular text
        /// </summary>
        /// <param name="ComparisonObject">The button to be clicked</param>
        public void SearchSummary(Object ComparisonObject)
        {
            foreach (AlarmViolation_Button btn in tsSummary.Items)
                if (btn.Visible)
                    if (ComparisonObject is MM_AlarmViolation_Type && (!ComparisonObject.Equals(btn.BaseType) ^ !btn.Checked))
                        HandleSummaryItem_Click(btn, EventArgs.Empty);
                    else if (ComparisonObject is MM_KVLevel && (!ComparisonObject.Equals(btn.VoltageLevel) ^ !btn.Checked))
                        HandleSummaryItem_Click(btn, EventArgs.Empty);
                    else if (ComparisonObject is MM_Element_Type && (!ComparisonObject.Equals(btn.ElemType) ^ !btn.Checked))
                        HandleSummaryItem_Click(btn, EventArgs.Empty);
        }

        /// <summary>
        /// Go through all violations, to update recency as needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            foreach (ListViewItem lvI in ShownViolations.Values)
            {
                MM_AlarmViolation Viol = (MM_AlarmViolation)lvI.Tag;
                String Recency = Viol.Recency;
                if (Recency != lvI.SubItems[4].Text)
                    lvI.SubItems[4].Text = Recency;
            }
        }

        /// <summary>
        /// Return our virtual item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvViolations_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = ShownViolationList[e.ItemIndex];
        }

    }
}