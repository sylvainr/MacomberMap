using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using System.Xml;
using Macomber_Map.Data_Elements;
using Macomber_Map.User_Interface_Components.NetworkMap;

using Macomber_Map.User_Interface_Components.GDIPlus;
using System.Threading;
using Macomber_Map.Data_Connections.Generic;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This control displays the high-level indicators (e.g., ACE, Frequency, Generation, Load, etc.)
    /// </summary>
    public partial class Key_Indicators : UserControl
    {
        #region Variable Declarations
        /// <summary>Show the value type for the variable (MW, Hz)</summary>
        [Category("Display"), DefaultValue(true), Description("Show the value type for the variable (MW, Hz)")]
        public bool ShowValueType = true;

        /// <summary>Show the percentage of wind generation</summary>
        [Category("Display"), DefaultValue(true), Description("Show the percentage of wind generation")]
        public bool ShowWindPercent = true;

        /// <summary>Network map below</summary>
        public Network_Map networkMap;

        /// <summary>The display configuration window</summary>
        private Display_Options DisplayConfiguration;

        /// <summary>Our delta display window</summary>
        public static MM_Coordinate_Delta DeltaDisplay;

        /// <summary>Our note viewer</summary>
        private Note_Viewer NoteViewer;

        /// <summary>The generation type information for wind</summary>
        private MM_Generation_Type WindGen;

        /// <summary>Our collection of indicator labels</summary>
        private List<Key_Indicator_Label> IndicatorLabels = new List<Key_Indicator_Label>();
        #endregion

        #region Control as form option
        /// <summary>
        /// Create a new form with this control fully docked within it
        /// </summary>
        /// <param name="Title">The title of the window</param>
        /// <param name="TargetSize">The target size of the window</param>
        /// <param name="Style">The border style of the window</param>
        /// <returns>A newly creted form with a fully docked control</returns>
        public static Form CreateForm(String Title, Size TargetSize, FormBorderStyle Style)
        {
            Form NewForm = new Form();
            NewForm.Size = TargetSize;
            NewForm.Text = Title;
            NewForm.FormBorderStyle = Style;

            Key_Indicators NewKey = new Key_Indicators();
            NewForm.Controls.Add(NewKey);
            NewKey.Dock = DockStyle.Fill;
            return NewForm;
        }


        #endregion

        #region Initialization
        /// <summary>
        /// Set up our key indicators and their text boxes
        /// </summary>
        public Key_Indicators()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Whether or not the Min/Max indicators are shown
        /// </summary>
        [Category("Display"), DefaultValue(false), Description("Whether or not the min/max buttons are shown")]
        public bool ShowMinMaxIndicators
        {
            get { return btnMin.Visible; }
            set { btnMax.Visible = btnMin.Visible = value; UpdateElements(); }
        }

        /// <summary>
        /// Assign the relevant controls to the control
        /// </summary>
        /// <param name="networkMap">The network map</param>
        /// <param name="miniMap">The mini-map</param>
        /// <param name="violViewer">The violation viewer</param>
        /// <param name="xKeyIndicators">The XML configuration for key elements</param>
        public void SetControls(Network_Map networkMap, Mini_Map miniMap, Violation_Viewer violViewer, XmlElement xKeyIndicators)
        {

            cmbNetwork.DataSource = Data_Integration.DataSources;
            cmbNetwork.DisplayMember = "Name";
            cmbNetwork.ValueMember = "Value";
            WindGen = MM_Repository.GenerationTypes["WIND"];

            foreach (DataRowView dr in cmbNetwork.Items)
                if (dr.Row["Value"] == Data_Integration.NetworkSource)
                    cmbNetwork.SelectedItem = dr;

            this.cmbNetwork.SelectedIndexChanged += new System.EventHandler(this.cmbNetwork_SelectedIndexChanged);

            this.networkMap = networkMap;
            DisplayConfiguration = new Display_Options(networkMap, miniMap, violViewer);
            DeltaDisplay = new MM_Coordinate_Delta(networkMap);
            Data_Integration.NetworkSourceChanged += new EventHandler(Data_Integration_NetworkSourceChanged);
            if (Data_Integration.Permissions.ShowNotes)
            {
                NoteViewer = new Note_Viewer(this, networkMap);
                lblNotes.Tag = NoteViewer;
                NoteViewer.Hide();
            }
            else
                lblNotes.Visible = false;




            //Initialize the communications viewer
            if (Data_Integration.Permissions.DisplayICCPISDLinks || Data_Integration.Permissions.DisplayMMCommLinks)
                Communication_Viewer.CreateInstanceInSeparateThread(lblComm);
            else
                lblComm.Visible = false;

            lblViews.Visible = Data_Integration.Permissions.ShowViews;
            if (Data_Integration.Permissions.ShowViews)
                InitializeViews();

            lblProcesses.Visible = false; //Data_Integration.Permissions.ShowProcesses; //this will remove it from display however keeping around incase we need it later for reference.
            if (Data_Integration.Permissions.ShowProcesses)
            {
                Process_Viewer pView = new Process_Viewer(lblProcesses);
                lblProcesses.Tag = pView;
                pView.SetControls();
            }

            //Initialize the generation label            
            string[] EmptyString = new string[] { };
            foreach (XmlNode xNode in xKeyIndicators.ChildNodes)
                if (xNode is XmlElement)
                {
                    XmlElement xElem = xNode as XmlElement;
                    Key_Indicator_Label NewLabel;
                    if (xElem.Attributes["GraphMode"].Value == "Limit_Reserve_Viewer")
                        NewLabel = new Key_Indicator_Label(xElem, new Limit_Reserve_Viewer(xElem), ChangeVisibility);
                    else if (xElem.Attributes["GraphMode"].Value == "Island")
                        NewLabel = new Key_Indicator_Label(xElem, new MM_Island_View(networkMap), ChangeVisibility);
                    else
                        NewLabel = new Key_Indicator_Label(xElem, Historical_Viewer.Trend_Graph(xElem.Attributes["Name"].Value, (Historical_Viewer.GraphModeEnum)Enum.Parse(typeof(Historical_Viewer.GraphModeEnum), xElem.Attributes["GraphMode"].Value), xElem.Attributes["HistoricQueries"].Value.Split(','), xElem.Attributes["EMSMappings"].Value.Split(',')), ChangeVisibility);
                    IndicatorLabels.Add(NewLabel);
                    Controls.Add(NewLabel);
                }

            //Hide our model data if we don't want to show it.
            if (!Data_Integration.Permissions.AllowModelSwitching)
            {
                lblNetModel.Visible = false;
                cmbNetwork.Visible = false;
                //btnData.Visible = false; //removed for now, as this should be there at all times in study - 20130610 -mn
            }

            //Initialize our menu
            if (Data_Integration.Permissions.CreateSavecase)
            {
                mnuMain.Items.Add("&Save current state");
                mnuMain.Items.Add("-");
            }
            if (Data_Integration.MMServer == null)
            {
                mnuMain.Items.Add("&Manual data entry");
                mnuMain.Items.Add("-");
            }

            if (Data_Integration.Permissions.DisplayProperties)
            {
                mnuMain.Items.Add("&Display Properties");
                if (Data_Integration.Permissions.ShowViolations)
                {
                    mnuMain.Items.Add("&Switch main view style - no violations");
                    mnuMain.Items.Add("&Switch main view style - violations in main window");
                    mnuMain.Items.Add("&Switch main view style - violations in a separate window");
                }
                mnuMain.Items.Add("-");
            }

            if (Data_Integration.Permissions.PlayGame || MM_Repository.Training != null)
            {
                if (Data_Integration.Permissions.PlayGame)
                    mnuMain.Items.Add("&Play situation awareness game");
                if (MM_Repository.Training != null)
                    mnuMain.Items.Add("&Training program");
                mnuMain.Items.Add("-");
            }

            if (Data_Integration.Permissions.SuggestCoordinateUpdates)
            {
                mnuMain.Items.Add("&Suggest coordinate updates");
                mnuMain.Items.Add("-");
            }

            mnuMain.Items.Add("&Reset network map");
            mnuMain.Items.Add("-");
            mnuMain.Items.Add("&About Macomber Map");
            mnuMain.Items.Add("-");
            mnuMain.Items.Add("&Exit");

            chkUseEstimates.Checked = Data_Integration.UseEstimates;
            chkUseEstimates.Visible = Data_Integration.NetworkSource.Estimates && Data_Integration.NetworkSource.Telemetry;
            chkPostCtgs.Checked = Data_Integration.ShowPostCtgValues;
            chkPostCtgs.Visible = Data_Integration.Permissions.ShowViolations;
            btnData.Visible = !Data_Integration.NetworkSource.Estimates && !Data_Integration.NetworkSource.Telemetry;


            chkDisplayAlarms.Checked = Data_Integration.DisplayAlarms;

        }

        /// <summary>
        /// Handle the change of network source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_Integration_NetworkSourceChanged(object sender, EventArgs e)
        {

            //btnData.Visible = Data_Integration.NetworkSource.Estimates && Data_Integration.NetworkSource.Telemetry;
            if (Data_Integration.NetworkSource.Estimates && Data_Integration.NetworkSource.Telemetry)
            {
                chkUseEstimates.Visible = true;
                btnData.Visible = false;
            }
            else
            {
                chkUseEstimates.Visible = false;
                btnData.Text = "    Refresh";
                btnData.Image = global::Macomber_Map.Properties.Resources.RefreshDocViewHS;
                btnData.Visible = true;
            }

            /*
            chkUseEstimates.Visible = Data_Integration.NetworkSource.Estimates && Data_Integration.NetworkSource.Telemetry;
             
            if (Data_Integration.NetworkSource.Connector == null)
                return;
            if (Data_Integration.NetworkSource.RefreshOnly )
            {
                btnData.Text = "    Refresh";
                btnData.Image = global::Macomber_Map.Properties.Resources.RefreshDocViewHS;
                btnData.Visible = true;
            }
            else if (Data_Integration.NetworkSource.Connector.Active == MM_DataConnector.ActiveState.Active)
            {
                btnData.Text = "    Pause";
                btnData.Image = global::Macomber_Map.Properties.Resources.PauseHS;
                btnData.Visible = false;
            }
            else
            {
                btnData.Text = "    Resume";
                btnData.Image = global::Macomber_Map.Properties.Resources.PlayHS;
                btnData.Visible = false;
            }
             */
        }
        #endregion

        #region Element Updating / Resizing
        /// <summary>
        /// Shift elements around and display the appropriate ones, based on the user's preferences and requested data
        /// </summary>
        public void UpdateElements()
        {
            //Reposition the key indicators to equal-space based on our room
            int LabelWidth = 0, NumVisible = 0;
            foreach (Key_Indicator_Label Lbl in IndicatorLabels)
                if (Lbl.Visible)
                {
                    LabelWidth += Lbl.Width;
                    NumVisible++;
                }

            if (NumVisible == 0)
                return;

            //If our minimum and maximum boxes are visible, show them, and resize the search window to its left. If not, size the search window to the right
            if (btnMin.Visible)
            {
                btnMax.Left = this.DisplayRectangle.Width - btnMax.Width - 4;
                btnMin.Left = btnMax.Left - btnMin.Width;
                btnMin.Top = btnMax.Top = txtSearch.Top;
                txtSearch.Left = btnMin.Left - txtSearch.Width - 4;
            }
            else
            {
                txtSearch.Left = this.DisplayRectangle.Width - txtSearch.Width - 4;
            }

            //Now, move the remaining elements 
            lblNotes.Left = txtSearch.Left - lblNotes.Width - 10;
            lblViews.Left = lblNotes.Left - lblViews.Width - 4;
            lblProcesses.Left = lblViews.Left - lblProcesses.Width - 4;
            lblComm.Left = lblProcesses.Left - lblComm.Width;

            chkPostCtgs.Left = lblComm.Left - chkPostCtgs.Width - 10;
            chkUseEstimates.Left = chkPostCtgs.Left - chkUseEstimates.Width - 5;

            cmbSavecase.Left = btnData.Right + 5;
            cmbSavecase.Width = chkUseEstimates.Left - cmbSavecase.Left - 5;

            //Determine if we can fit everything on one row, or two
            int LeftMost = (cmbSavecase.Visible ? cmbSavecase.Left : chkUseEstimates.Left) - 16;
            int RightMost = (btnData.Visible ? btnData.Right : cmbNetwork.Right) + 16;


            if (LabelWidth < LeftMost - RightMost)
            {
                int LabelDelta = (LeftMost - RightMost) / NumVisible;
                int CurLeft = RightMost;
                foreach (Key_Indicator_Label lbl in IndicatorLabels)
                    if (lbl.Visible)
                    {
                        CurLeft = (lbl.Left = CurLeft) + LabelDelta;// +lbl.Width; //check this if needs width - natatos merge question
                        lbl.Top = lblNetModel.Top;
                    }
                if (this.Parent is SplitterPanel)
                    CheckSplitterSize(this.Parent as SplitterPanel, 45);

            }
            else
                if (LabelWidth > 0)
                {
                    int LabelDelta = ((this.DisplayRectangle.Width) - LabelWidth) / NumVisible;
                    int CurLeft = this.DisplayRectangle.Left + 16;
                    foreach (Key_Indicator_Label lbl in IndicatorLabels)
                        if (lbl.Visible)
                        {
                            lbl.Top = 47;
                            CurLeft = (lbl.Left = CurLeft) + LabelDelta + lbl.Width;
                        }
                    CheckSplitterSize(this.Parent as SplitterPanel, 73);
                }

            //Finally, update the data within the text boxes to reflect the changes
            UpdateElementData();

            //Move our results helper
            Rectangle KeyIndicatorBounds = new Rectangle(txtSearch.Left - txtSearch.Width, txtSearch.Bottom, txtSearch.Width*2, txtSearch.Font.Height * 8);
            ResultsHelper.Bounds = ResultsHelper.Parent.RectangleToClient(RectangleToScreen(KeyIndicatorBounds));
            ResultsHelper.Columns[0].Width = 20;
            ResultsHelper.Columns[1].Width = ResultsHelper.Width - ResultsHelper.Columns[0].Width - 5;   

        }

        /// <summary>
        /// Check the distance on our splitter
        /// </summary>
        /// <param name="Panel"></param>
        /// <param name="TargetWidth"></param>
        private void CheckSplitterSize(SplitterPanel Panel, int TargetWidth)
        {
            Application.DoEvents();
            SplitContainer sc = Panel.Parent as SplitContainer;
            if (sc.SplitterDistance != TargetWidth)
            {
                sc.SplitterDistance = TargetWidth;
                this.Refresh();
                UpdateElements();
            }
        }

        private delegate void SafeUpdateData();

        /// <summary>
        /// Update the data shown for the elements
        /// </summary>
        private void UpdateElementData()
        {
            if (!Data_Integration.InitializationComplete)
                return;
            if (InvokeRequired)
            {
                Invoke(new SafeUpdateData(UpdateElementData));
                return;
            }

            //Every minute, update our notes
            if (Data_Integration.MMServer != null && DateTime.Now.Second == 0)
                Data_Integration.MMServer.LoadNotes();

            //Update our note tally                       
            if (NoteViewer != null)
                lblNotes.Text = NoteViewer.NoteText;

            this.BackColor = Data_Integration.NetworkSource.BackColor;
            this.ForeColor = Data_Integration.NetworkSource.ForeColor;

            //For any visible parameters, update the text with our in-place data.
            foreach (Key_Indicator_Label Lbl in IndicatorLabels)
                if (Lbl.Visible)
                    Lbl.UpdateText(ShowValueType, ShowWindPercent);

            //Update the tooltip on our comm window | Added case for public catch to avoid sending ICCP data 20130607-MN
            if (lblComm.Tag is Communication_Viewer && Data_Integration.Permissions.PublicUser == false)
                tTip.SetToolTip(lblComm, (lblComm.Tag as Communication_Viewer).ReportCommStatus());

            //Update our savecase list
            if (Data_Integration.NetworkSource.Application != "StudySystem")
                cmbSavecase.Visible = false;
            else
                if (Data_Integration.Savecases != cmbSavecase.Tag)
                {
                    cmbSavecase.Visible = true;
                    cmbSavecase.Items.Clear();
                    foreach (String str in Data_Integration.Savecases.Keys)
                        cmbSavecase.Items.Add(MM_Repository.TitleCase(str));
                    cmbSavecase.Tag = Data_Integration.Savecases;
                }

            this.Invalidate();
        }

        #endregion

        /// <summary>
        /// Handle a resize of the form by updating the elements and handling accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyIndicators_Resize(object sender, EventArgs e)
        {
            if (this.Visible)
                UpdateElements();
        }

        /// <summary>
        /// Handle the painting of our key indicator background
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (System.Drawing.Drawing2D.LinearGradientBrush br = new System.Drawing.Drawing2D.LinearGradientBrush(this.Bounds, BackColor, Color.Black, 90f, true))
                e.Graphics.FillRectangle(br, e.ClipRectangle);
        }


        /// <summary>
        /// Update the system time every second
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateSystemTime(object sender, EventArgs e)
        {
            //Update the key indicators
            UpdateElementData();
        }

        #region Interaction Controls
        /// <summary>
        /// Maximize the parent window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMax_Click(object sender, EventArgs e)
        {
            this.ParentForm.WindowState = (this.ParentForm.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized);
        }


        /// <summary>
        /// Minimize the parent window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMin_Click(object sender, EventArgs e)
        {
            this.ParentForm.WindowState = FormWindowState.Minimized;
        }


        /// <summary>
        /// Handle a user's search request (use clicked the search icon)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
                Form_Builder.SearchDisplay(networkMap, txtSearch.Text, (Control.ModifierKeys & Keys.Control) == Keys.Control, (Control.ModifierKeys & Keys.Shift) == Keys.Shift);
        }

        /// <summary>
        /// Handle a user's search request (user hit enter in the search window)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearch_SearchClicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtSearch.Text) && txtSearch.Text.Trim().Length > 0)
                Form_Builder.SearchDisplay(networkMap, txtSearch.Text, (Control.ModifierKeys & Keys.Control) == Keys.Control, (Control.ModifierKeys & Keys.Shift) == Keys.Shift);
        }
        #endregion



        #region Views Menu
        /// <summary>
        /// Initialize the views menu
        /// </summary>
        private void InitializeViews()
        {
            mnuViews.Items.Clear();

            //Add in our default menu item
            (mnuViews.Items.Add(MM_Repository.xConfiguration["Configuration"]["DisplayParameters"].Attributes["DefaultName"].Value) as ToolStripMenuItem).Tag = MM_Repository.Views[MM_Repository.xConfiguration["Configuration"]["DisplayParameters"]];

            //Now go through and recursively add in all items
            foreach (XmlElement xE in MM_Repository.xConfiguration["Configuration"]["DisplayParameters"]["Views"].ChildNodes)
                AddMenuItem(mnuViews, xE);
            mnuViews.Items.Add("-");
            ToolStripMenuItem BSWin = mnuViews.Items.Add("&Blackstart window") as ToolStripMenuItem;
            MM_Blackstart_Display.CreateInstanceInSeparateThread(BSWin, networkMap);

            //ToolStripMenuItem ESWin = mnuViews.Items.Add("&Element Summary window") as ToolStripMenuItem;
            //Form_Builder.ElementSummaryDisplay(ESWin, networkMap);

            mnuViews.Items.Add("-");
            mnuViews.Items.Add("New &map window");
            ToolStripMenuItem mnuWindows = mnuViews.Items.Add("Windows") as ToolStripMenuItem;
            mnuWindows.DropDownItems.Add("-");
            mnuWindows.DropDownOpening += new EventHandler(mnuWindows_DropDownOpening);
            mnuWindows.DropDownItemClicked += new ToolStripItemClickedEventHandler(mnuWindows_DropDownItemClicked);
        }

        /// <summary>
        /// Focus a window when the user selects it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuWindows_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            (e.ClickedItem.Tag as MM_Form).SafeActivate();

        }

        /// <summary>
        /// Handle the dropdown menu window opening, so that we can provide the list of windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuWindows_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem mnuWindows = sender as ToolStripMenuItem;
            mnuWindows.DropDownItems.Clear();


            List<MM_Form> VisibleForms = new List<MM_Form>();
            foreach (MM_Form Form in Data_Integration.RunningForms)
                if (!Form.IsDisposed && Form.Visible)
                    VisibleForms.Add(Form);
            VisibleForms.Sort();
            foreach (MM_Form Form in VisibleForms)
                (mnuWindows.DropDownItems.Add(Form.Title) as ToolStripMenuItem).Tag = Form;
        }

        /// <summary>
        /// Add a new view item (view or category) to the menu
        /// </summary>
        /// <param name="BaseMenu"></param>
        /// <param name="RootElement"></param>
        private void AddMenuItem(Object BaseMenu, XmlElement RootElement)
        {
            ToolStripMenuItem NewItem = null;
            if (BaseMenu is ToolStripMenuItem)
                NewItem = (BaseMenu as ToolStripMenuItem).DropDownItems.Add(RootElement.Attributes["Name"].Value) as ToolStripMenuItem;
            else
                NewItem = (BaseMenu as ContextMenuStrip).Items.Add(RootElement.Attributes["Name"].Value) as ToolStripMenuItem;
            NewItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(mnuViews_ItemClicked);

            if (MM_Repository.Views.ContainsKey(RootElement))
                NewItem.Tag = MM_Repository.Views[RootElement];

            foreach (XmlNode xE in RootElement.ChildNodes)
                if (xE is XmlElement)
                    AddMenuItem(NewItem, xE as XmlElement);
        }

        /// <summary>
        /// Handle the clicking of a view menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuViews_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "&Blackstart window")
            {
                if ((e.ClickedItem.Tag as MM_Form).IsDisposed)
                    e.ClickedItem.Tag = new MM_Blackstart_Display(networkMap);
                ChangeVisibility(e.ClickedItem, EventArgs.Empty);
            }
            else if (e.ClickedItem.Text == "&Element Summary window")
            {
                if ((e.ClickedItem.Tag as MM_Form).IsDisposed)
                {
                    e.ClickedItem.Tag = null;
                    Form_Builder.ElementSummaryDisplay(e.ClickedItem as ToolStripMenuItem, networkMap);
                    while (e.ClickedItem.Tag == null)
                        Application.DoEvents();
                }
                ChangeVisibility(e.ClickedItem, EventArgs.Empty);
            }
            else if (e.ClickedItem.Text == "New &map window")
                ThreadPool.QueueUserWorkItem(new WaitCallback(AddMap));
            else
            {
                MM_Display_View SelectedView = e.ClickedItem.Tag as MM_Display_View;
                if (SelectedView != null)
                {
                    SelectedView.Activate();

                }
            }

        }


        /// <summary>
        /// Add a new network map display
        /// </summary>
        public void AddMap(object state)
        {
            MM_Form NewForm = new MM_Form();
            Network_Map_GDI newMap = new Network_Map_GDI(new MM_Coordinates(networkMap.Coordinates.TopLeft, networkMap.Coordinates.BottomRight, networkMap.Coordinates.ZoomLevel));
            newMap.Dock = DockStyle.Fill;
            newMap.violViewer = networkMap.violViewer;
            newMap.miniMap = new Mini_Map(true);
            newMap.miniMap.Violations = newMap.violViewer;
            newMap.BackColor = NewForm.BackColor = MM_Repository.OverallDisplay.BackgroundColor;
            NewForm.Title = "(window)";
            NewForm.Size = new Size(800, 600);
            NewForm.Icon = networkMap.ParentForm.Icon;
            newMap.SetControls(networkMap.violViewer, newMap.miniMap, 0);
            newMap.miniMap.SetControls(newMap, networkMap.violViewer);

            NewForm.Controls.Add(newMap.miniMap);
            NewForm.Controls.Add(newMap);
            newMap.miniMap.BringToFront();

            NewForm.SizeChanged += new EventHandler(NewForm_SizeChanged);
            NewForm.Shown += new EventHandler(NewForm_Shown);
            Data_Integration.DisplayForm(NewForm, Thread.CurrentThread);
        }

        /// <summary>
        /// Show the new form, and bring it to the forefont
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewForm_Shown(object sender, EventArgs e)
        {
            Form Sender = sender as Form;
            Sender.Focus();
            NewForm_SizeChanged(sender, e);
        }



        /// <summary>
        /// Update the size of our new window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewForm_SizeChanged(object sender, EventArgs e)
        {
            Form Sender = sender as Form;
            Network_Map_GDI nMap = Sender.Controls[1] as Network_Map_GDI;
            nMap.miniMap.Location = new Point(Sender.DisplayRectangle.Width - nMap.miniMap.Width, Sender.DisplayRectangle.Height - nMap.miniMap.Height);
            // throw new Exception("The method or operation is not implemented.");
        }
        #endregion

        /// <summary>
        /// Handle the menu click against the main menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuMain_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "&Switch main view style - no violations")
                (networkMap.ParentForm as MacomberMap).SwitchMMView(false, false);
            else if (e.ClickedItem.Text == "&Switch main view style - violations in main window")
                (networkMap.ParentForm as MacomberMap).SwitchMMView(true, false);
            else if (e.ClickedItem.Text == "&Switch main view style - violations in a separate window")
                (networkMap.ParentForm as MacomberMap).SwitchMMView(true, true);
            else if (e.ClickedItem.Text == "&Play situation awareness game")
                ThreadPool.QueueUserWorkItem(new WaitCallback(ShowSAGame));
            else if (e.ClickedItem.Text == "&Manual data entry")
                ThreadPool.QueueUserWorkItem(new WaitCallback(ShowDataEntry));
            else if (e.ClickedItem.Text == "&Training program")
            {
                if (MM_Repository.Training.TrainingMode == Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.NotTraning)
                    MM_Repository.Training.InitiateTraining((ParentForm as MacomberMap).ctlNetworkMap);
                else
                {
                    MM_Repository.Training.TrainingMode = Macomber_Map.Data_Elements.Training.MM_Training.enumTrainingMode.NotTraning;
                    (ParentForm as MacomberMap).ctlNetworkMap.ResetDisplayCoordinates();
                }
            }

            else
                switch (e.ClickedItem.Text.Replace("&", "").ToLower().Split(' ')[0])
                {
                    case "display": DisplayConfiguration.Visible = true; DisplayConfiguration.WindowState = FormWindowState.Normal; DisplayConfiguration.UpdateDisplay(); DisplayConfiguration.Activate(); break;
                    case "suggest": DeltaDisplay.TopMost = true; DeltaDisplay.Visible = true; DeltaDisplay.WindowState = FormWindowState.Normal; DeltaDisplay.Activate(); break;
                    case "save": Data_Integration.SaveXMLDataRequest(networkMap.Coordinates); break;
                    //case "about": Data_Integration.DisplayForm(new About_Form(), Thread.CurrentThread); break;
                    case "about": (new About_Form()).ShowDialog(); break;
                    case "reset": (ParentForm as MacomberMap).ResetNetworkMap(); break;
                    case "exit": System.Diagnostics.Process.GetCurrentProcess().Kill(); break;
                }
        }

        private SearchResultsHelper ResultsHelper;

        /// <summary>
        /// Assign our results helper
        /// </summary>
        /// <param name="ResultsHelper"></param>
        public void AssignResultsHelper(SearchResultsHelper ResultsHelper)
        {
            this.ResultsHelper = ResultsHelper;
            this.ResultsHelper.SearchOptionSelected += new EventHandler<ListViewItemSelectionChangedEventArgs>(ResultsHelper_SearchOptionSelected);
        }

        /// <summary>
        /// Handle the results of a search option selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultsHelper_SearchOptionSelected(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ResultsHelper.Hide();
            MM_Element SelectedElement = e.Item.Tag as MM_Element;
            if (SelectedElement == null)
                return;
            else if (SelectedElement is MM_Substation)
            {
                networkMap.miniMap.ZoomPanToPoint((SelectedElement as MM_Substation).LatLong);
                networkMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
            }
            else if (SelectedElement is MM_Line)
            {
                networkMap.miniMap.ZoomPanToPoint(((SelectedElement as MM_Line).Midpoint));
                networkMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
            }
            else if (SelectedElement is MM_Boundary)
            {
                networkMap.miniMap.ZoomPanToPoint(((SelectedElement as MM_Boundary).Centroid));
                networkMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
            }
            txtSearch.Text = "";
        }


        /// <summary>
        /// When our search text changes, update accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text == "")
                ResultsHelper.Hide();
            else
            {
                if (ResultsHelper != null)
                    ResultsHelper.UpdateSearchText(txtSearch.Text);
                if (ResultsHelper.Visible == false)
                    ResultsHelper.Show();
            }
        }

        
        /// <summary>
        /// Display our situation awareness game
        /// </summary>
        /// <param name="state"></param>
        private void ShowSAGame(object state)
        {
            Data_Integration.DisplayForm(new MM_Game(networkMap as Network_Map_GDI), Thread.CurrentThread);
        }

        private void ShowDataEntry(object state)
        {
            Data_Integration.DisplayForm(new MM_Data_Entry(), Thread.CurrentThread);
        }

        /// <summary>
        /// Handle a change of data source or time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbNetwork_SelectedIndexChanged(object sender, EventArgs e)
        {
            //MN//20130607 lblClientStatus.BackColor = Color.Red;        
            //MN//20130607 User_Interface_Components.GDIPlus.Network_Map_GDI.LockForDataUpdate = true;
            cmbNetwork.Enabled = false;
            chkDisplayAlarms.Enabled = false;
            btnData.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            ////btnData.Visible = false;

            ////
            // HandlingMouse = false;
            //Coordinates.ControlDown = false;
            //ShiftDown = false;
            ////
            //MN//20130607 root of network change event, we need to clear lasso events and lock data changes/queries as soon as possible from this point
            //WaitHandle.wa
            Data_Integration.NetworkSource = (cmbNetwork.SelectedItem as DataRowView)["Value"] as MM_Data_Source;

            ////if(cmbNetwork.Text == "Study Network")
            ////btnData.Visible = true;

            //btnData.Visible = Data_Integration.NetworkSource.RefreshOnly;
            ////btnData.Text = "    Refresh";

            //MN//20130607 (Parent.Parent.Parent as MacomberMap).ResetNetworkMap();

            //User_Interface_Components.GDIPlus.Network_Map_GDI.LockForDataUpdate = false;

            //MN//20130607 Thread.Sleep(5000);
            Thread.Sleep(5000);
            Cursor.Current = Cursors.Default;
            cmbNetwork.Enabled = true;
            btnData.Enabled = true;
            chkDisplayAlarms.Enabled = true;
            //MN//20130607 lblClientStatus.BackColor = Color.Lime;


        }

        /// <summary>
        /// A thread-safe delegate for changing visibility.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private delegate void SafeChangeVisibility(object sender, EventArgs e);

        /// <summary>
        /// When the user clicks on a key indicator, change that indicator's visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeVisibility(object sender, EventArgs e)
        {
            Form FoundGraph = sender.GetType().GetProperty("Tag").GetValue(sender, null) as Form;

            if (FoundGraph != null && FoundGraph.InvokeRequired)
                FoundGraph.BeginInvoke(new SafeChangeVisibility(ChangeVisibility), sender, e);
            else if (FoundGraph != null && !(FoundGraph.Text.Contains("Communications Status") && Data_Integration.Permissions.PublicUser == true))
            {
                FoundGraph.Visible ^= true;
                if (FoundGraph.Visible)
                {
                    FoundGraph.Location = new Point(Cursor.Position.X, this.Bottom);
                    FoundGraph.Activate();
                }
            }

        }

        /// <summary>
        /// Handle the click on the views - now left/right, etc.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblViews_MouseClick(object sender, MouseEventArgs e)
        {
            mnuViews.Show(sender as Control, e.Location);
        }

        /// <summary>
        /// Handle the click of the stop/go/refresh button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnData_Click(object sender, EventArgs e)
        { //mn//1 to FIX 20130610
            if (btnData.Text == "    Refresh")
                cmbNetwork_SelectedIndexChanged(cmbNetwork, EventArgs.Empty);
            /*
            if (btnData.Text == "    Resume")
            {
                btnData.Text = "    Pause";
                btnData.Image = global::Macomber_Map.Properties.Resources.PauseHS;
                foreach (MM_DataConnector Conn in Data_Integration.DataConnections.Values)
                    Conn.ResumeQueries();
            }
            else if (btnData.Text == "    Refresh")
                foreach (MM_DataConnector Conn in Data_Integration.DataConnections.Values)
                    Conn.RefreshQueries();
            else if (btnData.Text == "    Pause")
            {
                btnData.Text = "    Resume";
                btnData.Image = global::Macomber_Map.Properties.Resources.PlayHS;
                foreach (MM_DataConnector Conn in Data_Integration.DataConnections.Values)
                    Conn.PauseQueries();
      /// <param name="sender"></param>
        /// <param name="e"></param>
           }*/
        }

        /// <summary>
        /// Handle the mouse left-click, to stop speech immediately
        /// </summary>
        private void picERCOT_MouseClick(object sender, MouseEventArgs e)
        {
            Data_Integration.HandleSpeechRequest();
        }

        /// <summary>
        /// Handle the change of estimate use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkUseEstimates_CheckedChanged(object sender, EventArgs e)
        {
            Data_Integration.UseEstimates = chkUseEstimates.Checked;
        }

        /// <summary>
        /// Handle the change of alarm use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkDisplayalarms_CheckedChanged(object sender, EventArgs e)
        {
            Data_Integration.DisplayAlarms = chkDisplayAlarms.Checked;
            cmbNetwork_SelectedIndexChanged(cmbNetwork, EventArgs.Empty);
        }


        /// <summary>
        /// Handle the change of post-ctg value display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkPostCtgs_CheckedChanged(object sender, EventArgs e)
        {
            Data_Integration.ShowPostCtgValues = chkPostCtgs.Checked;
        }




    }
}
