using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.NetworkMap;
using MacomberMapClient.User_Interfaces.Violations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.User_Interfaces.Configuration
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This form allows for customization of the display options, by allowing for a default view, and then deltas from the normal based on parameters.
    /// </summary>
    public partial class MM_Display_Options : MM_Form
    {

        #region Variable declarations
        /// <summary>The network map</summary>
        public MM_Network_Map_DX networkMap;

        /// <summary>The mini-map associated with the network map</summary>
        public MM_Mini_Map miniMap;

        /// <summary>The violation viewer associated with the network map</summary>
        public MM_Violation_Viewer violViewer;

        /// <summary>The menu strip for the views right-click menu</summary>
        private ContextMenuStrip ViewMenu = new ContextMenuStrip();

        /// <summary>Our flag for initializing the quick view</summary>
        private bool Initializing = true;

        /// <summary>The UI helper for our display</summary>
        private MM_UserInterface_Helper Helper;

        /// <summary>Our timer for handling a key press</summary>
        private Timer HandleKeyPressTimer;

        /// <summary>The text box currently being edited</summary>
        private TextBox EditingTextBox = null;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize an empty display options
        /// </summary>
        public MM_Display_Options()
        {
            InitializeComponent();
            HandleKeyPressTimer = new Timer();
            HandleKeyPressTimer.Interval = 1000;
            HandleKeyPressTimer.Tick += HandleKeyPressTimer_Tick;
        }

        /// <summary>
        /// Create the property view of display options
        /// </summary>
        /// <param name="networkMap">The network map</param>
        /// <param name="miniMap">The mini-map</param>    
        /// <param name="violViewer">The violation viewer</param>
        public MM_Display_Options(MM_Network_Map_DX networkMap, MM_Mini_Map miniMap, MM_Violation_Viewer violViewer) : this()
        {
            this.Helper = new MM_UserInterface_Helper(ssMain, tsMem, lblCPU);

            this.pgMain.PropertyValueChanged += new PropertyValueChangedEventHandler(pgMain_PropertyValueChanged);
            this.networkMap = networkMap;
            this.miniMap = miniMap;
            this.violViewer = violViewer;
            this.ViewMenu.ItemClicked += new ToolStripItemClickedEventHandler(Menu_ItemClicked);
            this.tvViews.MouseUp += new MouseEventHandler(tvViews_MouseUp);
            this.Size = new Size(676, 661);
            MM_Repository.ViewChanged += new MM_Repository.ViewChangedDelegate(Repository_ViewChanged);
            Repository_ViewChanged(MM_Repository.ActiveView);
            ResetViews();

            Initializing = false;
        }

        /// <summary>
        /// Handle a view change by updating our display accordingly.
        /// </summary>
        /// <param name="ActiveView"></param>
        private void Repository_ViewChanged(MM_Display_View ActiveView)
        {
            UpdateDisplay();
            UpdateControls();
        }

        /// <summary>
        /// Recursively look through all of the nodes, and find any matches
        /// </summary>
        /// <param name="ActiveView">The active view</param>
        /// <param name="RootNode">The tree node to test</param>
        private void FindActivateView(MM_Display_View ActiveView, TreeNode RootNode)
        {
            foreach (TreeNode tn in RootNode.Nodes)
                if ((tn.Tag as MM_Display_View) == ActiveView)
                {
                    tvViews.SelectedNode = tn;
                    return;
                }
                else
                    FindActivateView(ActiveView, tn);
        }
        #endregion

        #region Value changing / property page handling
        /// <summary>
        /// Handle when a value is changed. If the value is one that we need to refresh the network and mini-map for, do so.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void pgMain_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //If our current view is active, we'll need to refresh it.
            MM_Display_View CurrentView = (tvViews.SelectedNode.Tag as MM_Display_View);
            if (e.ChangedItem.PropertyDescriptor.ComponentType == typeof(MM_DisplayParameter))
            {
                if (e.ChangedItem.Parent.Value is MM_AlarmViolation_Type)
                    (e.ChangedItem.Parent.Value as MM_AlarmViolation_Type).RebuildViolationGraphic();
                else
                    foreach (MM_AlarmViolation_Type ViolType in MM_Repository.ViolationTypes.Values)
                        ViolType.RebuildViolationGraphic();
            }
            if (e.ChangedItem.PropertyDescriptor.ComponentType == typeof(MM_AlarmViolation_Type))
                if (e.ChangedItem.Label == "NetworkMap_Substation")
                    networkMap.IsDirty = true;
                else if (e.ChangedItem.Label == "NetworkMap_Line")
                    networkMap.UpdateLineInformation();
                else if (e.ChangedItem.Parent.Parent.Value is MM_Substation_Display)
                    networkMap.IsDirty = true;
                else if (e.ChangedItem.Label == "MiniMap")
                    miniMap.Invalidate();
                else if (e.ChangedItem.Label == "ViolationViewer")
                    violViewer.UpdateViolations(e.ChangedItem.Parent.Parent.Value as MM_AlarmViolation_Type);
                else if (e.ChangedItem.Label == "Acronym")
                    if (e.ChangedItem.Parent.Value is MM_AlarmViolation_Type)
                        (e.ChangedItem.Parent.Value as MM_AlarmViolation_Type).RebuildViolationGraphic();
                    else
                        foreach (MM_AlarmViolation_Type ViolType in MM_Repository.ViolationTypes.Values)
                            ViolType.RebuildViolationGraphic();


            String ThisTitle = e.ChangedItem.Label;


            GridItem curItem = e.ChangedItem.Parent.Parent;
            while (curItem != null)
            {
                if (curItem.Value is MM_Display)
                    ThisTitle = "Overall." + ThisTitle;
                else if (curItem.Value is MM_Substation_Display)
                    ThisTitle = "Substations." + ThisTitle;
                else if (curItem.Value is MM_DisplayParameter)

                    ThisTitle = (curItem.Value as MM_DisplayParameter).Name + "." + ThisTitle;
                else
                    ThisTitle = curItem.Label + "." + ThisTitle;
                if (curItem.Parent == null)
                    break;
                curItem = curItem.Parent.Parent;
            }

            String PullInValue = e.ChangedItem.Value.ToString();
            if (e.ChangedItem.Value is Color)
                PullInValue = ColorTranslator.ToHtml((Color)e.ChangedItem.Value);
            else if (e.ChangedItem.Value is Font)
                PullInValue = new FontConverter().ConvertToString((Font)e.ChangedItem.Value);
            if (CurrentView.DisplayParameters.ContainsKey(ThisTitle))
                CurrentView.DisplayParameters[ThisTitle] = PullInValue;
            else
                CurrentView.DisplayParameters.Add(ThisTitle, PullInValue);

            CurrentView.Activate();
        }

        /// <summary>
        /// When the node selection changes, adjust the property page accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateNodeSelection(object sender, TreeViewEventArgs e)
        {
            pgMain.SelectedObject = e.Node.Tag;
        }
        #endregion

        #region Form handling
        /// <summary>
        /// Handle the form close event - if the user tried to close it, flag a cancel and hide.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Display_Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Visible = false;
                e.Cancel = true;
            }
        }
        #endregion

        #region System updating (timer)
        /// <summary>
        /// Handle our update timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                long Mem;
                Helper.UpdateCPUAndMemory(out Mem);
                this.tsMem.Text = "Mem: " + GC.GetTotalMemory(false).ToString("#,##0");
                int LastFPS = (int)MM_Repository.AverageFPS;
                this.tsFPS.Text = "FPS: " + LastFPS.ToString();
                this.tsZoom.Text = "Zoom: " + networkMap.Coordinates.ZoomLevel.ToString();
                this.tsCenter.Text = "Center: (" + networkMap.Coordinates.Center.X.ToString("0.00") + "," + networkMap.Coordinates.Center.Y.ToString("0.00") + ")";
                PointF CurMouse = networkMap.MouseLngLat;
                if (float.IsNaN(CurMouse.X))
                    this.tsMouse.Text = "Off-screen";
                else
                    this.tsMouse.Text = "Mouse: (" + CurMouse.X.ToString("0.00") + "," + CurMouse.Y.ToString("0.00") + ")";
            }
        }
        #endregion

        #region Control addition, refreshing and value updating
        /// <summary>
        /// Add a button to the parent control
        /// </summary>
        /// <param name="Title">The title of the button</param>
        /// <param name="Location">The location of the button (y is updated when completed)</param>
        /// <param name="ParentControl">The control in which this button should be placed</param>
        /// <param name="AssociatedObject">The object associated with the placement</param>
        /// <param name="Property"></param>
        private TextBox AddTextBox(String Title, ref Point Location, Object AssociatedObject, String Property, Control ParentControl)
        {
            int StartX = Location.X;
            if (!String.IsNullOrEmpty(Title))
            {
                Label OutLabel = new Label();
                OutLabel.Location = Location;
                OutLabel.Text = Title;
                ParentControl.Controls.Add(OutLabel);
                OutLabel.AutoSize = true;
                Application.DoEvents();
                StartX = OutLabel.Right + 4;
            }

            TextBox t = new TextBox();
            t.Width = 40;
            t.Location = new Point(StartX, Location.Y);
            Location.Y = t.Bottom + 5;
            PropertyInfo Prop = AssociatedObject.GetType().GetProperty(Property);
            foreach (Object obj in Prop.GetCustomAttributes(true))
                if (obj is DescriptionAttribute)
                    tTip.SetToolTip(t, (obj as DescriptionAttribute).Description);
            t.Tag = new KeyValuePair<Object, PropertyInfo>(AssociatedObject, Prop);
            t.Text = Prop.GetValue(AssociatedObject, null).ToString();
            t.KeyPress += HandleKeyPress;
            t.LostFocus += HandleKeyPressTimer_Tick;
            ParentControl.Controls.Add(t);
            return t;
        }

        /// <summary>
        /// Add a combo box to the display
        /// </summary>
        /// <param name="Location"></param>
        /// <param name="AssociatedObject"></param>
        /// <param name="Property"></param>
        /// <param name="ParentControl"></param>
        /// <param name="Title"></param>        
        /// <returns></returns>
        private ComboBox AddCombo(String Title, ref Point Location, Object AssociatedObject, String Property, Control ParentControl)
        {
            int StartX = Location.X;
            if (!String.IsNullOrEmpty(Title))
            {
                Label OutLabel = new Label();
                OutLabel.Location = new Point(Location.X - 2, Location.Y + 2);
                OutLabel.Text = Title;
                OutLabel.AutoSize = true;
                StartX = OutLabel.Right + 4;
                ParentControl.Controls.Add(OutLabel);
            }

            ComboBox C = new ComboBox();
            C.Width = 200;

            PropertyInfo pI = AssociatedObject.GetType().GetProperty(Property);
            if (pI.PropertyType == typeof(MM_Company))
            {
                List<String> CompanyNames = new List<string>();
                foreach (MM_Company Company in MM_Repository.Companies.Values)
                    if (/*Company.OperatesEquipment && */(Company.Alias.StartsWith("Q") || Company.Alias.StartsWith("T")))
                        CompanyNames.Add(Company.Alias + " (" + Company.Name + ")");
                CompanyNames.Sort();
                C.Items.AddRange(CompanyNames.ToArray());
            }
            else
                foreach (String EnumOption in Enum.GetNames(pI.PropertyType))
                    C.Items.Add(EnumOption);

            foreach (Object obj in pI.GetCustomAttributes(true))
                if (obj is DescriptionAttribute)
                    tTip.SetToolTip(C, (obj as DescriptionAttribute).Description);

            C.DropDownStyle = ComboBoxStyle.DropDownList;
            if (Property != "DisplayCompany")
                C.Text = Enum.GetName(pI.PropertyType, pI.GetValue(AssociatedObject, null));
            C.Location = new Point(StartX, Location.Y);
            Location.Y = C.Bottom + 5;
            C.Tag = new KeyValuePair<Object, PropertyInfo>(AssociatedObject, pI);
            C.SelectedIndexChanged += ChangeValue;
            ParentControl.Controls.Add(C);
            return C;
        }

        /// <summary>
        /// Add a checkbox to the parent control
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Location"></param>
        /// <param name="AssociatedObject"></param>
        /// <param name="Property"></param>
        /// <param name="ParentControl"></param>
        /// <returns></returns>
        private CheckBox AddCheckbox(String Title, ref Point Location, Object AssociatedObject, String Property, Control ParentControl)
        {
            CheckBox b = new CheckBox();
            b.AutoSize = true;
            b.Text = Title;
            b.Location = Location;

            PropertyInfo Prop = AssociatedObject.GetType().GetProperty(Property);
            b.Tag = new KeyValuePair<Object, PropertyInfo>(AssociatedObject, Prop);

            foreach (Object obj in Prop.GetCustomAttributes(true))
                if (obj is DescriptionAttribute)
                    tTip.SetToolTip(b, (obj as DescriptionAttribute).Description);

            Location.Y = b.Bottom + 5;
            b.Click += ChangeValue;
            ParentControl.Controls.Add(b);
            return b;
        }

        /// <summary>
        /// Add a button to the parent control
        /// </summary>
        /// <param name="Title">The title of the button</param>
        /// <param name="Location">The location of the button (y is updated when completed)</param>
        /// <param name="ParentControl">The control in which this button should be placed</param>
        /// <param name="AssociatedObject">The object holding the value</param>
        /// <param name="Property">The property to be shown and updated</param>        
        /// <param name="NextBelow">Whether the next object is below. If not, it's to the right.</param>
        /// <param name="AutoSize">Whether to autosize the new button</param>
        private Button AddButton(String Title, ref Point Location, Object AssociatedObject, String Property, Control ParentControl, bool NextBelow = true, bool AutoSize = false)
        {
            Button b = new Button();
            b.AutoSize = true;
            b.Text = Title;
            b.Location = Location;

            PropertyInfo Prop = AssociatedObject.GetType().GetProperty(Property);
            b.Tag = new KeyValuePair<Object, PropertyInfo>(AssociatedObject, Prop);
            foreach (Object obj in Prop.GetCustomAttributes(true))
                if (obj is DescriptionAttribute)
                    tTip.SetToolTip(b, (obj as DescriptionAttribute).Description);


            bool SelectedState = (bool)Prop.GetValue(AssociatedObject, null);
            b.BackColor = (SelectedState ? SystemColors.Highlight : SystemColors.Control);
            b.ForeColor = (SelectedState ? SystemColors.Menu : SystemColors.ControlText);


            b.Click += ChangeValue;
            ParentControl.Controls.Add(b);

            //Now, if we're in autosize mode, update
            if (AutoSize)
            {
                b.AutoSize = true;
                b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            }

            if (NextBelow)
                Location.Y = b.Bottom + 5;
            else
                Location.X = b.Right + 5;

            return b;
        }

        /// <summary>
        /// Add a label to the parent control
        /// </summary>
        /// <param name="Title">The title of the label</param>
        /// <param name="Location">The location of the label (y is updated when completed)</param>
        /// <param name="ParentControl">The control in which this label should be placed</param>
        /// <param name="AssociatedObject">The object holding the value</param>
        /// <param name="Property">The property to be shown and updated</param>        
        private Label AddLabel(String Title, ref Point Location, Object AssociatedObject, String Property, Control ParentControl)
        {
            Label b = new Label();
            b.AutoSize = true;
            b.Text = Title;
            b.Location = Location;

            PropertyInfo Prop = AssociatedObject.GetType().GetProperty(Property);
            b.Tag = new KeyValuePair<Object, PropertyInfo>(AssociatedObject, Prop);
            foreach (Object obj in Prop.GetCustomAttributes(true))
                if (obj is DescriptionAttribute)
                    tTip.SetToolTip(b, (obj as DescriptionAttribute).Description);


            Location.Y = b.Bottom + 5;
            b.Click += ChangeValue;
            ParentControl.Controls.Add(b);
            return b;
        }

        /// <summary>
        /// Handle a key press in a text box; start a timer, or handle enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyPress(object sender, KeyPressEventArgs e)
        {
            HandleKeyPressTimer.Stop();
            EditingTextBox = (TextBox)sender;
            if (e.KeyChar == '\r')
                ChangeValue(sender, e);
            else
                HandleKeyPressTimer.Start();
        }

        /// <summary>
        /// When our timer kicks off, stop the timer and trigger a value change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyPressTimer_Tick(object sender, EventArgs e)
        {
            if (HandleKeyPressTimer.Enabled)
            {
                HandleKeyPressTimer.Stop();
                ChangeValue(EditingTextBox, e);
            }
        }

        /// <summary>
        /// Change a button value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeValue(object sender, EventArgs e)
        {
            KeyValuePair<Object, PropertyInfo> Item = (KeyValuePair<Object, PropertyInfo>)(sender as Control).Tag;
            if (sender is Button || sender is Label)
            {
                bool Resp = (bool)Item.Value.GetValue(Item.Key, null);
                Item.Value.SetValue(Item.Key, !Resp, null);
            }
            else if (sender is CheckBox)
            {
                Item.Value.SetValue(Item.Key, (sender as CheckBox).Checked, null);
                if (Item.Value.Name == "ShowLineRouting")
                {
                    foreach (MM_Form Form in Data_Integration.RunningForms.ToArray())
                        foreach (MM_Network_Map_DX Map in Form.Controls.OfType<MM_Network_Map_DX>())
                            Map.UpdateLineInformation();
                    networkMap.UpdateLineInformation();
                }
            }
            else if (sender is ComboBox)
            {
                if (Item.Value.PropertyType == typeof(MM_Company))
                {
                    foreach (MM_Company Company in MM_Repository.Companies.Values)
                        if (Company.Alias == (sender as ComboBox).Text.Split(' ')[0])
                        {
                            Item.Value.SetValue(Item.Key, Company, null);
                            break;
                        }
                }
                else
                    Item.Value.SetValue(Item.Key, Enum.Parse(Item.Value.PropertyType, (sender as ComboBox).Text), null);
            }
            else if (sender is TextBox)
            {
                float TryVal;
                if (float.TryParse((sender as TextBox).Text, out TryVal))
                {
                    Item.Value.SetValue(Item.Key, Convert.ChangeType(TryVal, Item.Value.PropertyType), null);
                    if (Item.Value.Name == "MVASize")
                        networkMap.UpdateLineInformation();
                }
            }

            UpdateDisplay();
            UpdateControls();
            networkMap.IsDirty = true;
            foreach (MM_Form Form in Data_Integration.RunningForms.ToArray())
                foreach (MM_Network_Map_DX Map in Form.Controls.OfType<MM_Network_Map_DX>())
                    Map.IsDirty = true;
        }

        /// <summary>
        /// Update the values of all controls
        /// </summary>
        private void UpdateControls()
        {
            //Build our list of all controls to be updated
            List<Control> ControlsToUpdate = new List<Control>();
            foreach (TabPage tp in tabDisplay.TabPages)
                foreach (Control ctl in tp.Controls)
                    if (ctl is GroupBox)
                    {
                        foreach (Control ctl2 in ctl.Controls)
                            if (ctl2.Tag is KeyValuePair<Object, PropertyInfo>)
                                ControlsToUpdate.Add(ctl2);
                    }
                    else if (ctl.Tag is KeyValuePair<Object, PropertyInfo>)
                        ControlsToUpdate.Add(ctl);

            //Perform the control updates
            foreach (Control c in ControlsToUpdate)
                if (c is Button)
                {
                    KeyValuePair<Object, PropertyInfo> kvp = (KeyValuePair<Object, PropertyInfo>)c.Tag;
                    bool SelectedState = (bool)kvp.Value.GetValue(kvp.Key, null);
                    c.BackColor = (SelectedState ? SystemColors.Highlight : SystemColors.Control);
                    c.ForeColor = (SelectedState ? SystemColors.Menu : SystemColors.ControlText);
                }
                else if (c is CheckBox)
                {
                    KeyValuePair<Object, PropertyInfo> kvp = (KeyValuePair<Object, PropertyInfo>)c.Tag;
                    CheckBox chk = c as CheckBox;
                    chk.CheckedChanged -= ChangeValue;
                    chk.Checked = (bool)kvp.Value.GetValue(kvp.Key, null);
                    chk.CheckedChanged += ChangeValue;
                }
                else if (c is ComboBox)
                {
                    KeyValuePair<Object, PropertyInfo> kvp = (KeyValuePair<Object, PropertyInfo>)c.Tag;
                    ComboBox cmb = c as ComboBox;
                    cmb.SelectedIndexChanged -= ChangeValue;

                    if (kvp.Value != null)
                    {
                        Object InResult = kvp.Value.GetValue(kvp.Key, null);
                        if (InResult != null)
                            cmb.Text = InResult.ToString();
                    }
                    cmb.SelectedIndexChanged += ChangeValue;
                }
                else if (c is TextBox)
                {
                    KeyValuePair<Object, PropertyInfo> kvp = (KeyValuePair<Object, PropertyInfo>)c.Tag;
                    c.Text = kvp.Value.GetValue(kvp.Key, null).ToString();
                }


            //Also, update our line MVA display
            chkLineMVAAnimation.CheckedChanged -= chkLineMVAAnimation_CheckedChanged;
            int NumAnimated = 0, NumNotAnimated = 0;
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                if (KVLevel.ShowMVA)
                    NumAnimated++;
                else NumNotAnimated++;
            if (NumAnimated == 0)
                chkLineMVAAnimation.Checked = false;
            else if (NumNotAnimated == 0)
                chkLineMVAAnimation.Checked = true;
            else
                chkLineMVAAnimation.CheckState = CheckState.Indeterminate;
            chkLineMVAAnimation.CheckedChanged += chkLineMVAAnimation_CheckedChanged;



        }
        #endregion

        #region Views
        private void tvViews_MouseUp(object sender, MouseEventArgs e)
        {
            //Do this only for right-click
            if (e.Button != MouseButtons.Right)
                return;
            //First, find out if we're on a node.
            TreeViewHitTestInfo ht = tvViews.HitTest(e.Location);
            if ((ht.Node == null) || (ht.Location != TreeViewHitTestLocations.Label))
                PopupMenu(null, e, "&Save", "&Reset to defaults", "-", "New &category", "New &view");
            /*{
                XmlElement ThisElem = ht.Node.Tag as XmlElement;
                if (ThisElem.Name == "Category")
                    PopupMenu(ThisElem, e, "Add new &category under " + ht.Node.Text, "Add new &view under " + ht.Node.Text, "&Delete view " + ht.Node.Text);
                else if (ThisElem.Name == "View")
                    PopupMenu(ThisElem, e, "&Delete view " + ht.Node.Text);
            }*/
        }

        /// <summary>
        /// Reset the collection of views
        /// </summary>
        private void ResetViews()
        {
            //Set all of our controls to auto-size
            grpCounty.Enabled = false; //not using counties
            grpCounty.AutoSize = true;
            grpSubstations.AutoSize = true;
            grpLineVisibility.AutoSize = true;
            grpLineMVA.AutoSize = true;
            grpEquipmentMonitoring.AutoSize = true;

            //Start with our county components
            grpCounty.Controls.Clear();
            Point ColoringPt = new Point(6, 26);
            AddCombo("Coloring", ref ColoringPt, MM_Repository.OverallDisplay, "Contour", grpCounty);
            AddCombo("Map", ref ColoringPt, MM_Repository.OverallDisplay, "MapTiles", grpCounty);
            AddTextBox("Transparency", ref ColoringPt, MM_Repository.OverallDisplay, "MapTransparency", grpCounty);


            // Set up our substation quick control
            Point SubstationPt = new Point(6, 29);
            AddCombo("Visibility", ref SubstationPt, MM_Repository.SubstationDisplay, "ShowSubstations", grpSubstations);
            AddCombo("Gen Fuel Bubbles", ref SubstationPt, MM_Repository.SubstationDisplay, "GeneratorBubblesMode", grpSubstations);
            AddCheckbox("Show Load Bubbles", ref SubstationPt, MM_Repository.SubstationDisplay, "ShowLoadBubbles", grpSubstations);
            AddCheckbox("Show Gen Reserve Zone", ref SubstationPt, MM_Repository.SubstationDisplay, "ShowReserveZones", grpSubstations);
            AddCheckbox("Outaged Auto-Transformers", ref SubstationPt, MM_Repository.SubstationDisplay, "ShowAutoTransformersOut", grpSubstations);
            AddCheckbox("Frequency-Controlled Gen", ref SubstationPt, MM_Repository.SubstationDisplay, "ShowFrequencyControl", grpSubstations);
            AddCheckbox("Synchroscopes", ref SubstationPt, MM_Repository.SubstationDisplay, "ShowSynchroscopes", grpSubstations);
            AddCheckbox("Long Names", ref SubstationPt, MM_Repository.OverallDisplay, "UseLongNames", grpSubstations);
            AddCheckbox("Show voltage coloring on substations", ref SubstationPt, MM_Repository.SubstationDisplay, "ShowSubstationVoltages", grpSubstations);
            AddCheckbox("Show Tie Import/Export", ref SubstationPt, MM_Repository.OverallDisplay, "TieFlowDirectionVisible", grpSubstations);
            AddCombo("Contour Data", ref SubstationPt, MM_Repository.OverallDisplay, "ContourData", grpSubstations);
            AddCheckbox("FPS", ref SubstationPt, MM_Repository.OverallDisplay, "FPS", grpSubstations);
            AddCombo("Visibility-Mode", ref SubstationPt, MM_Repository.OverallDisplay, "BlackstartMode", grpSubstations);
            AddCombo("Visibility-Company", ref SubstationPt, MM_Repository.OverallDisplay, "DisplayCompany", grpSubstations);

            //Add in our controls for each voltage level 
            int CurTop = 38, EquipMonitorTop = 81;
            Button CountyButton = null;
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values.OrderByDescending(kv => kv.Nominal))
            {
                Point CurPoint = new Point(20, CurTop);
                Point EquipMonitorPoint = new Point(20, EquipMonitorTop);
                AddLabel(KVLevel.Name, ref CurPoint, KVLevel, "SimpleVisibility", grpLineVisibility);
                CountyButton = AddButton(KVLevel.Name.Split(' ')[0], ref ColoringPt, KVLevel, "ShowOnMap", grpCounty, false, true);

                foreach (String str in "Energized,PartiallyEnergized,DeEnergized,Unknown,LineRouting,NormalOpened".Split(','))
                {
                    CurPoint = new Point(grpLineVisibility.Controls["lbl" + str].Left, CurTop);
                    AddCheckbox("", ref CurPoint, KVLevel, "Show" + str, grpLineVisibility);
                }

                CurPoint = new Point(grpLineVisibility.Controls["lblMVAFlowSize"].Left, CurTop);
                AddTextBox("", ref CurPoint, KVLevel, "MVASize", grpLineVisibility);


                CheckBox VoltageBox = AddCheckbox("", ref EquipMonitorPoint, KVLevel, "MonitorEquipment", grpEquipmentMonitoring);
                EquipMonitorPoint = new Point(VoltageBox.Right + 5, VoltageBox.Top);
                AddLabel(KVLevel.Name, ref EquipMonitorPoint, KVLevel, "MonitorEquipment", grpEquipmentMonitoring);


                foreach (String str in "ThermalWarning,ThermalAlert,LowVoltageAlert,LowVoltageWarning,HighVoltageWarning,HighVoltageAlert".Split(','))
                {
                    EquipMonitorPoint = new Point(grpEquipmentMonitoring.Controls["lbl" + str].Left, EquipMonitorTop);
                    AddTextBox("", ref EquipMonitorPoint, KVLevel, str, grpEquipmentMonitoring);
                }

                EquipMonitorTop += 30;
                CurTop += 30;
            }



            ColoringPt = new Point(6, CountyButton.Bottom + 5);
            AddButton("State", ref ColoringPt, MM_Repository.OverallDisplay, "DisplayStateBorder", grpCounty, false);
            AddButton("Counties", ref ColoringPt, MM_Repository.OverallDisplay, "DisplayCounties", grpCounty, false);
            if (MM_Repository.Districts.Count > 0)
                AddButton("Districts", ref ColoringPt, MM_Repository.OverallDisplay, "DisplayDistricts", grpCounty, false);

            //Now, size our elements
            grpSubstations.Top = grpCounty.Bottom + 10;
            grpLineVisibility.Location = new Point(grpCounty.Right + 10, grpCounty.Top);
            grpLineMVA.Location = new Point(grpLineVisibility.Left, grpLineVisibility.Bottom + 10);
            grpEquipmentMonitoring.Location = new Point(grpLineVisibility.Left, grpLineMVA.Bottom + 10);



            //Clear our view window, and add in the default
            tvViews.Nodes.Clear();
            AddView(null, MM_Repository.xConfiguration["Configuration"]["DisplayParameters"]);

            //Now parse through all of the views, and add them in                        
            foreach (XmlElement xView in MM_Repository.xConfiguration.DocumentElement["DisplayParameters"]["Views"].ChildNodes)
                AddView(null, xView);

            //Now adjust our splitter
            TreeNode LastNode = tvViews.Nodes[tvViews.Nodes.Count - 1];
            while (LastNode.LastNode != null)
                LastNode = LastNode.LastNode;

            splLeft.SplitterDistance = 8 + LastNode.Bounds.Bottom + grpViews.Top + tvViews.Top;

            //Update our overall tab
            tabOverall.Controls.Clear();
            tabOverall.AutoScroll = true;
            Point CurPos = new Point(3, 3);
            AddCombo("Contour Type", ref CurPos, MM_Repository.OverallDisplay, "Contour", tabOverall);
            AddTextBox("Contour Brightness", ref CurPos, MM_Repository.OverallDisplay, "ContourBrightness", tabOverall);
            AddTextBox("Contour Threshold", ref CurPos, MM_Repository.OverallDisplay, "ContourThreshold", tabOverall);

            AddButton("Display counties", ref CurPos, MM_Repository.OverallDisplay, "DisplayCounties", tabOverall);
            AddButton("Display state border", ref CurPos, MM_Repository.OverallDisplay, "DisplayStateBorder", tabOverall);

            AddTextBox("Flow interval", ref CurPos, MM_Repository.OverallDisplay, "FlowInterval", tabOverall);
            AddTextBox("Energization Threshold", ref CurPos, MM_Repository.OverallDisplay, "EnergizationThreshold", tabOverall);


            AddButton("Group lines by voltage", ref CurPos, MM_Repository.OverallDisplay, "GroupLinesByVoltage", tabOverall);
            AddButton("Split buses by voltage", ref CurPos, MM_Repository.OverallDisplay, "SplitBusesByVoltage", tabOverall);

            AddTextBox("Line flows", ref CurPos, MM_Repository.OverallDisplay, "LineFlows", tabOverall);
            AddTextBox("Line text", ref CurPos, MM_Repository.OverallDisplay, "LineText", tabOverall);
            AddTextBox("Station summaries", ref CurPos, MM_Repository.OverallDisplay, "StationMW", tabOverall);
            AddTextBox("Station names", ref CurPos, MM_Repository.OverallDisplay, "StationNames", tabOverall);

            AddButton("Station long names", ref CurPos, MM_Repository.OverallDisplay, "UseLongNames", tabOverall);
            AddButton("Highlight outaged auto-transformers", ref CurPos, MM_Repository.SubstationDisplay, "ShowAutoTransformersOut", tabOverall);
            AddButton("Highlight substations with synchroscopes", ref CurPos, MM_Repository.SubstationDisplay, "ShowSynchroscopes", tabOverall);
            AddButton("Highlight frequency-controlled substations", ref CurPos, MM_Repository.SubstationDisplay, "ShowFrequencyControl", tabOverall);
            AddButton("Highlight stacked lines", ref CurPos, MM_Repository.SubstationDisplay, "HighlightMultipleLines", tabOverall);
            AddCombo("Blackstart mode", ref CurPos, MM_Repository.OverallDisplay, "BlackstartMode", tabOverall);
            AddTextBox("Non-selected dimming", ref CurPos, MM_Repository.OverallDisplay, "BlackstartDimmingFactor", tabOverall);
            AddCombo("Operator to display", ref CurPos, MM_Repository.OverallDisplay, "DisplayCompany", tabOverall);
            //Now, update our violation tab, going through all priorities
            tabViolations.Controls.Clear();
            tabViolations.AutoScroll = true;
            CurPos = new Point(3, 3);
            for (int Pri = 1; Pri < 5; Pri++)
            {
                Label NewPri = new Label();
                NewPri.Location = CurPos;
                NewPri.Text = "Priority " + Pri.ToString();
                NewPri.AutoSize = true;
                tabViolations.Controls.Add(NewPri);
                CurPos.Y = NewPri.Bottom + 5;

                //Now add in all buttons of that priority
                foreach (MM_AlarmViolation_Type AlmType in MM_Repository.ViolationTypes.Values)
                    if (AlmType.Priority == Pri)
                        AddButton(AlmType.Name, ref CurPos, AlmType, "SimpleVisibility", tabViolations);

            }
            //Now add in our lines

            tabLines.Controls.Clear();
            tabLines.AutoScroll = true;
            CurPos = new Point(3, 3);

            var kvLevels = MM_Repository.KVLevels.Values.OrderByDescending(kv => kv.Nominal).ToList();

            foreach (MM_KVLevel KVLevel in kvLevels)
            {
                //Set our X coordinate, and add in our line level
                CurPos.X = 3;
                int TopY = CurPos.Y;
                Point TempPoint = new Point(CurPos.X + 100, TopY);
                AddButton(KVLevel.Name, ref CurPos, KVLevel, "SimpleVisibility", tabLines);

                //Add in our MW flow line and label                    
                TempPoint.X = AddButton("MVA Flow", ref TempPoint, KVLevel, "ShowMVA", tabLines).Right + 20;
                TempPoint.Y = TopY;
                Point MVAFlowTop = new Point(TempPoint.X, TempPoint.Y + 10);
                TempPoint.X = AddTextBox("MVA Flow Threshold", ref TempPoint, KVLevel, "MVAThreshold", tabLines).Right + 20;
                MVAFlowTop.Y = TempPoint.Y;

                //Add in our MVA, MW, MVAR, % information
                foreach (string str in "Name,%,MVA,MW,MVAR".Split(','))
                {
                    TempPoint.Y = TopY;
                    Button NewButton = AddButton(str, ref TempPoint, KVLevel, (str == "%" ? "ShowPercentageText" : str == "Name" ? "ShowLineName" : "Show" + str + "Text"), tabLines);
                    NewButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                    NewButton.AutoSize = true;
                    TempPoint.X = NewButton.Right + 10;
                }

                //Add in our text for zoom levels                
                AddTextBox("Visibility (by zoom)", ref MVAFlowTop, KVLevel, "VisibilityByZoom", tabLines);
                AddTextBox("Visibility (min %)", ref MVAFlowTop, KVLevel, "VisibilityThreshold", tabLines);
                AddTextBox("Station Names", ref MVAFlowTop, KVLevel, "StationNames", tabLines);
                AddTextBox("Station MW", ref MVAFlowTop, KVLevel, "StationMW", tabLines);


                //Set our X coordinate, and add in our energized, deenergized, and partially energized options
                CurPos.X = 12;
                AddButton("Energized", ref CurPos, KVLevel, "ShowEnergized", tabLines);
                AddButton("Partially energized", ref CurPos, KVLevel, "ShowPartiallyEnergized", tabLines);
                AddButton("De-energized", ref CurPos, KVLevel, "ShowDeEnergized", tabLines);
                AddButton("Unknown", ref CurPos, KVLevel, "ShowUnknown", tabLines);
            }


            //Now update our substations tab
            tabSubstations.Controls.Clear();
            tabSubstations.AutoScroll = true;
            CurPos = new Point(3, 3);
            AddButton("Show Remaining Capacity", ref CurPos, MM_Repository.SubstationDisplay, "ShowRemainingCapacity", tabSubstations);
            AddButton("Show Total Generation", ref CurPos, MM_Repository.SubstationDisplay, "ShowTotalGeneration", tabSubstations);
            AddButton("Show Total HSL", ref CurPos, MM_Repository.SubstationDisplay, "ShowTotalHSL", tabSubstations);
            AddButton("Show Total Load", ref CurPos, MM_Repository.SubstationDisplay, "ShowTotalLoad", tabSubstations);
            AddButton("Always show visible line substations", ref CurPos, MM_Repository.SubstationDisplay, "ShowSubsOnLines", tabSubstations);

            AddCombo("Substations:", ref CurPos, MM_Repository.SubstationDisplay, "ShowSubstations", tabSubstations);
            this.Size = new Size(grpEquipmentMonitoring.Right + 20, grpEquipmentMonitoring.Bottom + this.Height - DisplayRectangle.Height + ssMain.Height + 20);
        }











        /// <summary>
        /// Update our display depending on what's shown
        /// </summary>
        public void UpdateDisplay()
        {

            SelectNode(Data_Integration.ActiveView, tvViews.Nodes);
            // if (tvParameters.Nodes.Count > 0)
            //     tvParameters.SelectedNode = tvParameters.Nodes[0];
        }

        /// <summary>
        /// Select a node based on its text
        /// </summary>
        /// <param name="Node">The node to search for</param>
        /// <param name="Collection"></param>
        private void SelectNode(String Node, TreeNodeCollection Collection)
        {
            if (Collection.Count == 0)
                return;
            else if (String.IsNullOrEmpty(Node))
                tvViews.SelectedNode = tvViews.Nodes[0];
            else if (Node.Contains("\\"))
            {
                foreach (TreeNode TestNode in Collection)
                    if (TestNode.Text == Node.Substring(0, Node.IndexOf('\\')))
                        SelectNode(Node.Substring(Node.IndexOf('\\') + 1), TestNode.Nodes);
            }
            else
            {
                foreach (TreeNode TestNode in Collection)
                    if (TestNode.Text == Node)
                    {
                        tvViews.SelectedNode = TestNode;
                        return;
                    }
                tvViews.SelectedNode = tvViews.Nodes[0];
            }
        }







        /// <summary>
        /// Add a view to the tvViews node collection
        /// </summary>
        /// <param name="tvNode">The parent node for this item (or null for the whole view)</param>
        /// <param name="xView">The XML element to add</param>
        private void AddView(TreeNode tvNode, XmlElement xView)
        {
            //First, create our new node and make sure it's visible
            TreeNode NewNode;
            String NodeName = "Default";
            if ((xView.Name == "View") || (xView.Name == "Category"))
                NodeName = xView.Attributes["Name"].Value;
            if (tvNode == null)
                NewNode = tvViews.Nodes.Add(NodeName);
            else
                NewNode = tvNode.Nodes.Add(NodeName);
            NewNode.EnsureVisible();

            if (xView.Name == "View" || xView.Name == "DisplayParameters")
            {
                NewNode.Tag = MM_Repository.Views[xView];
                if (NewNode.Tag == MM_Repository.ActiveView)
                    tvViews.SelectedNode = NewNode;
            }
            else if (xView.Name == "Category")
                foreach (XmlElement xViewChild in xView.ChildNodes)
                    AddView(NewNode, xViewChild);

        }


        private void PopupMenu(TreeNode ThisElem, MouseEventArgs e, params string[] MenuItems)
        {
            ViewMenu.Tag = ThisElem;
            ViewMenu.Items.Clear();
            foreach (string strToAdd in MenuItems)
            {
                ToolStripMenuItem NewItem = (ViewMenu.Items.Add(strToAdd) as ToolStripMenuItem);
                if (NewItem != null)
                    NewItem.Tag = ThisElem;
            }
            ViewMenu.Show(tvViews, e.Location);
        }

        /// <summary>
        /// Handle a menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text.Replace("&", "").Split(' ')[0])
            {
                case "Save":
                    foreach (MM_Display_View View in MM_Repository.Views.Values)
                        View.Save();
                    break;
                case "Reset":
                case "New":
                case "Add":
                case "Remove":
                case "Delete":
                    break;
            }
        }


        /// <summary>
        /// Add a new node to the tree, and make sure it's visible
        /// </summary>
        /// <param name="ParentNode">The parent node of the new node</param>
        /// <param name="Title">The title for the node</param>
        /// <param name="Tag">The object to associate with the node</param>
        private TreeNode AddNode(TreeNode ParentNode, String Title, Object Tag)
        {
            TreeNode NewNode = ParentNode.Nodes.Add(Title);
            NewNode.Tag = Tag;
            NewNode.EnsureVisible();
            return NewNode;
        }
        #endregion

        #region Quick control display

        /// <summary>
        /// Change the visibility of a transmission line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeLineVisibility(object sender, EventArgs e)
        {
            if (Initializing)
                return;
            String[] SenderName = (sender as CheckBox).Name.Split('_');
            MM_KVLevel VoltageLevel = MM_Repository.FindKVLevel(SenderName[1]);
            VoltageLevel.GetType().GetProperty("Show" + SenderName[2], System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase).SetValue(VoltageLevel, (sender as CheckBox).Checked, null);
            networkMap.IsDirty = true;
        }

        /// <summary>
        /// Update all lines by the requested energized state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BulkUpdateByEnergizedState(object sender, EventArgs e)
        {
            List<MM_KVLevel> KVLevels = new List<MM_KVLevel>(MM_Repository.KVLevels.Values);
            PropertyInfo pI = typeof(MM_KVLevel).GetProperty("Show" + (sender as Control).Name.Substring(3));
            bool TargetValue = !(bool)pI.GetValue(KVLevels[0], null);
            foreach (MM_KVLevel KVLevel in KVLevels)
                pI.SetValue(KVLevel, TargetValue, null);
            networkMap.IsDirty = true;

            if ((sender as Control).Name == "lblLineRouting")
                networkMap.UpdateLineInformation();

            foreach (MM_Form Form in Data_Integration.RunningForms.ToArray())
                foreach (MM_Network_Map_DX Map in Form.Controls.OfType<MM_Network_Map_DX>())
                {
                    Map.IsDirty = true;
                    if ((sender as Control).Name == "lblLineRouting")
                        Map.UpdateLineInformation();
                }
            UpdateControls();
        }


        /// <summary>
        /// Update the visibility of a transmission line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkLineMVAAnimation_CheckedChanged(object sender, EventArgs e)
        {
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                KVLevel.ShowMVA = chkLineMVAAnimation.Checked;
            MM_Repository.OverallDisplay.ShowLineFlows = chkLineMVAAnimation.Checked;
            lblLineMVAAnimation.Visible = chkLineMVAAnimation.Visible;
            networkMap.IsDirty = true;
        }

        /// <summary>
        /// Handel the scroll change of a line animation threshold
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbLineAnimation_Scroll(object sender, EventArgs e)
        {
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                KVLevel.MVAThreshold = tbLineAnimation.Value;

            lblLineMVAAnimation.Visible = chkLineMVAAnimation.Checked;
            lblLineMVAAnimation.Text = ">" + tbLineAnimation.Value.ToString("0") + "%";
            networkMap.IsDirty = true;

        }


        /// <summary>
        /// Handle the scroll change of a line visibility threshold
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateLineVisibility(object sender, EventArgs e)
        {
            float Threshold = chkLineVisibility.Checked ? (float)tbLineVisibility.Value / 100f : 0;
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                KVLevel.VisibilityThreshold = (rbLessThan.Checked ? 1 : -1) * Threshold;
            lblLineVisibility.Text = (rbLessThan.Checked ? "< " : "> ") + tbLineVisibility.Value.ToString("0") + "%";
            lblLineVisibility.Visible = chkLineVisibility.Checked;
            networkMap.IsDirty = true;
        }
        #endregion

        /// <summary>
        /// Go through the selected item's properties, and add them to the display.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvViews_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //Clear our parameters, and select our view. Also, activate it.
            String SelectedNode;
            if (tvParameters.SelectedNode != null)
                SelectedNode = tvParameters.SelectedNode.FullPath;

            tvParameters.Nodes.Clear();
            MM_Display_View SelectedView = e.Node.Tag as MM_Display_View;
            if (SelectedView == null)
                return;
            MM_Repository.ActiveView = SelectedView;
            SelectedView.Activate();

            //Add in our KV Levels to the property page
            TreeNode KVLevelsNode = tvParameters.Nodes.Add("KV Levels");
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
            {
                TreeNode KVLevelNode = AddNode(KVLevelsNode, KVLevel.Name, KVLevel);
                AddNode(KVLevelNode, "Energized", KVLevel.Energized);
                AddNode(KVLevelNode, "Partially Energized", KVLevel.PartiallyEnergized);
                AddNode(KVLevelNode, "De-energized", KVLevel.DeEnergized);
                AddNode(KVLevelNode, "Unknown", KVLevel.Unknown);
            }

            //Add in our substation to the property page.
            (tvParameters.Nodes.Add("Substations") as TreeNode).Tag = MM_Repository.SubstationDisplay;


            //Add in our violations to the property page
            TreeNode ViolTypes = tvParameters.Nodes.Add("Violations");
            foreach (MM_AlarmViolation_Type ViolType in MM_Repository.ViolationTypes.Values)
                AddNode(ViolTypes, ViolType.Name, ViolType);

            //Add in our zoom levels
            (tvParameters.Nodes.Add("Overall Options") as TreeNode).Tag = MM_Repository.OverallDisplay;


        }
    }
}