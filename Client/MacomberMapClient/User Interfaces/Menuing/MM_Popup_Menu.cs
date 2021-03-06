﻿using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.SystemInformation;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Data_Elements.Weather;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Information;
using MacomberMapClient.User_Interfaces.NetworkMap;
using MacomberMapClient.User_Interfaces.OneLines;
using MacomberMapClient.User_Interfaces.Overrides;
using MacomberMapClient.User_Interfaces.Summary;
using MacomberMapClient.User_Interfaces.Violations;
using MacomberMapCommunications.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MacomberMapClient.User_Interfaces.Menuing
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides a menu strip across multiple windows for the right-click popup on an item, along with unified handling.
    /// </summary>
    public class MM_Popup_Menu : ContextMenuStrip
    {
        #region Variable declarations
        /// <summary>The element for which the popup menu is showing items</summary>
        private MM_Element BaseElement;

        /// <summary>Whether the user is permitted to move the map</summary>
        private bool AllowMapMoving;

        /// <summary>The object owning the control</summary>
        public Control OwnerObject;
        #endregion

        #region Menu showing
        /// <summary>
        /// Show the context menu strip, after assigning the MM element and click handler
        /// </summary>
        /// <param name="Element"></param>
        /// <param name="Location"></param>
        /// <param name="BaseElement"></param>
        /// <param name="AllowMapMoving"></param>
        /// <param name="SourceApplication"></param>
        public void Show(MM_OneLine_Element Element, Point Location, MM_Element BaseElement, bool AllowMapMoving, String SourceApplication)
        {
            //TODO: Fix code below
            /*
    this.OwnerObject = Element;

    this.BaseElement = BaseElement;
    this.AllowMapMoving = AllowMapMoving;

    this.Items.Clear();
    BuildItems(BaseElement, Items);

    for (int a = 0; a < Items.Count; a++)
        if (Items[a] is ToolStripSeparator)
        {
            DataRow dR = Element.BaseRow;

            ToolStripMenuItem tsi = new ToolStripMenuItem("One-Line data:");
            if (dR != null)
                foreach (DataColumn dCol in dR.Table.Columns)
                    if (dR[dCol] != null && dR[dCol] is DBNull == false)
                        tsi.DropDownItems.Add(dCol.ColumnName + ": " + dR[dCol].ToString());
            Items.Insert(a + 1, tsi);

            //Add in our XML definition
            ToolStripMenuItem tsiX = new ToolStripMenuItem("XML Data:");


            List<String> OutXml = new List<string>();
            foreach (System.Xml.XmlElement xSubElem in Element.xElement.ChildNodes)
            {
                OutXml.Clear();
                ToolStripMenuItem tsNew = new ToolStripMenuItem(xSubElem.Name);
                tsiX.DropDownItems.Add(tsNew);
                foreach (System.Xml.XmlAttribute xAttr in xSubElem.Attributes)
                    OutXml.Add(xAttr.Name + ": " + xAttr.Value);
                OutXml.Sort(StringComparer.CurrentCultureIgnoreCase);
                foreach (String str in OutXml)
                    tsNew.DropDownItems.Add(str);

            }
            OutXml.Clear();
            foreach (System.Xml.XmlAttribute xAttr in Element.xElement.Attributes)
                OutXml.Add(xAttr.Name + ": " + xAttr.Value);
            OutXml.Sort(StringComparer.CurrentCultureIgnoreCase);
            foreach (String str in OutXml)
                tsiX.DropDownItems.Add(str);


            Items.Insert(a + 2, tsiX);

            Items.Insert(a + 3, new ToolStripSeparator());

                    
            if (BaseElement.ElemType != null && MM_Server_Interface.Client != null && Array.IndexOf(MM_Server_Interface.Client.UserOperatorships, 999999) == -1 && Array.IndexOf(MM_Server_Interface.Client.UserOperatorships, BaseElement.Operator.TEID) == -1)
            {
                System.Xml.XmlNode xDatabase = MM_Server_Interface.Client.OneLineMappings.SelectSingleNode("ElementType[@Name='" + BaseElement.ElemType.Name + "']/DataLocator[@Database='" + (SourceApplication == "Telemetered" ? "SCADA" : "NETMOM") + "']");
                if (xDatabase != null)
                    foreach (System.Xml.XmlNode xControlPanel in xDatabase.SelectNodes("ControlPanel"))
                        if (xControlPanel.Attributes["Type"] == null || xControlPanel.Attributes["Type"].Value != "MM_Synchroscope_Display" || (BaseElement as MM_Breaker_Switch).HasSynchroscope)
                            AddMenuItem(Items, "Display " + xControlPanel.Attributes["Title"].Value + " (" + SourceApplication + ")", false, null, new object[] { Element, xControlPanel, SourceApplication });
            }
            //If we have a control panel to offer, show it.
            break;

        }
    base.Show(Element, Location);*/
        }


        /// <summary>
        /// Show the context menu strip, after assigning the MM element and click handler
        /// </summary>
        /// <param name="ParentControl">The parent control for the item</param>
        /// <param name="Location">The location at which the menu should be shown (relative to the parent control)</param>
        /// <param name="BaseElement">The MM element driving the event</param>
        /// <param name="AllowMapMoving">Whether the user can move the map</param>
        public void Show(Control ParentControl, Point Location, MM_Element BaseElement, bool AllowMapMoving)
        {
            this.OwnerObject = ParentControl;
            this.BaseElement = BaseElement;
            this.AllowMapMoving = AllowMapMoving;

            this.Items.Clear();
            BuildItems(BaseElement, Items);

            //TODO: Fix showing the menu on a one-line element
            /*
            if (ParentControl is MM_OneLine_Element)
                for (int a = 0; a < Items.Count; a++)
                    if (Items[a] is ToolStripSeparator)
                    {
                        DataRow dR = (ParentControl as MM_OneLine_Element).BaseRow;

                        ToolStripMenuItem tsi = new ToolStripMenuItem("One-Line data:");
                        foreach (DataColumn dCol in dR.Table.Columns)
                            if (dR[dCol] != null && dR[dCol] is DBNull == false)
                                tsi.DropDownItems.Add(dCol.ColumnName + ": " + dR[dCol].ToString());
                        Items.Insert(a + 1, tsi);
                        Items.Insert(a + 2, new ToolStripSeparator());

                        //Go through the list of all available panels, and display them.
                        //(ParentControl as MM_OneLine_Element).BaseElement.ElemType



                        //If we have a control panel to offer, show it.

                        break;
                    }*/
            base.Show(ParentControl, Location);
        }

        /// <summary>
        /// Show the context menu strip, for multiple elements.
        /// </summary>
        /// <param name="ParentControl">The parent control for the item</param>
        /// <param name="Location">The location at which the menu should be shown (relative to the parent control)</param>
        /// <param name="BaseElements">The MM element driving the event</param>
        /// <param name="AllowMapMoving">Whether the user can move the map</param>
        public void Show(Control ParentControl, Point Location, IEnumerable<MM_Element> BaseElements, bool AllowMapMoving)
        {
            this.OwnerObject = ParentControl;
            this.BaseElement = null;
            this.AllowMapMoving = AllowMapMoving;


            this.Items.Clear();
            bool IsAllLines = true;
            int Lines = 0;
            foreach (MM_Element Elem in BaseElements)
            {
                AddExpandableMenuItem(this.Items, Elem.MenuDescription(), true, ViolationImage(Elem.WorstViolationOverall), Elem);
                if (Elem is MM_Line == false)
                    IsAllLines = false;
                Lines++;
            }

            if (IsAllLines)
            {
                ToolStripMenuItem Header = new ToolStripMenuItem(Lines.ToString("#,##0") + " lines: " + MM_Line.MultiLinePercentageText(BaseElements));
                Header.Enabled = false;
                Items.Insert(0, Header);
                Items.Insert(1, new ToolStripSeparator());
            }
            base.Show(ParentControl, Location);
        }


        /// <summary>
        /// Show the summary violation view
        /// </summary>
        /// <param name="ParentControl">The violation viewer</param>
        /// <param name="Location"></param>        
        /// <param name="btn">The tool strip button driving the display</param>
        /// <param name="ViolComparer">The list view comparer</param>
        public void Show(MM_Violation_Viewer ParentControl, Point Location, MM_Violation_Viewer.AlarmViolation_Button btn, MM_Violation_Viewer.ListViewItemComparer ViolComparer)
        {
            OwnerObject = ParentControl;
            Items.Clear();
            Tag = btn;
            foreach (String str in btn.ToolTipText.Split('\n'))
                AddMenuItem(Items, str, true);
            AddMenuItem(Items, "-", false);
            if (btn.New > 0)
                AddMenuItem(Items, "Acknowledge all", false);
            AddMenuItem(Items, "Archive all", false);
            base.Show(btn.Owner, Location);
        }

        /// <summary>
        /// Show a tooltip popup menu for the violation viewer
        /// </summary>
        /// <param name="ParentControl"></param>
        /// <param name="Location"></param>
        /// <param name="ts">The toolstrip to process</param>
        /// <param name="lvSorter">The list view sorter</param>
        public void Show(MM_Violation_Viewer ParentControl, Point Location, ToolStrip ts, MM_Violation_Viewer.ListViewItemComparer lvSorter)
        {
            this.OwnerObject = ParentControl;
            this.Items.Clear();
            AddMenuItem(Items, "Groupings", true);
            ToolStripMenuItem ViewBy = AddMenuItem(Items, "View by...", false) as ToolStripMenuItem;
            AddMenuItem(ViewBy.DropDownItems, "Violation type", false, null, MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.None).Checked = lvSorter.Split == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.None;
            AddMenuItem(ViewBy.DropDownItems, "Violation type and voltage level", false, null, MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Voltage).Checked = lvSorter.Split == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Voltage;
            AddMenuItem(ViewBy.DropDownItems, "Violation type and element type", false, null, MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.ElemType).Checked = lvSorter.Split == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.ElemType;

            ToolStripMenuItem ShowJust = AddMenuItem(Items, "Show just...", false) as ToolStripMenuItem;
            if (lvSorter.Split == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Voltage)
            {
                (ShowJust.DropDownItems.Add("Voltage Level") as ToolStripMenuItem).ForeColor = MM_Element.MenuDeactivatedColor;
                ShowJust.DropDownItems.Add("-");
                foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                    AddMenuItem(ShowJust.DropDownItems, KVLevel.Name, false, null, KVLevel);
                ShowJust.DropDownItems.Add("-");
            }
            else if (lvSorter.Split == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.ElemType)
            {
                (ShowJust.DropDownItems.Add("Element Type") as ToolStripMenuItem).ForeColor = MM_Element.MenuDeactivatedColor;
                ShowJust.DropDownItems.Add("-");
                foreach (MM_Element_Type ElemType in MM_Repository.ElemTypes.Values)
                    AddMenuItem(ShowJust.DropDownItems, ElemType.Name, false, null, ElemType);
                ShowJust.DropDownItems.Add("-");
            }
            (ShowJust.DropDownItems.Add("Violation Types") as ToolStripMenuItem).ForeColor = MM_Element.MenuDeactivatedColor;
            ShowJust.DropDownItems.Add("-");
            foreach (MM_AlarmViolation_Type ViolType in MM_Repository.ViolationTypes.Values)
                AddMenuItem(ShowJust.DropDownItems, ViolType.Name, false, null, ViolType);

            AddMenuItem(Items, "-", false);
            AddMenuItem(Items, "Violations", true);
            
            ToolStripMenuItem SortBy = AddMenuItem(Items, "Sort by...", false) as ToolStripMenuItem;
            AddMenuItem(SortBy.DropDownItems, "Voltage", false, null, MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Voltage).Checked = lvSorter.ComparisonType == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Title;
            AddMenuItem(SortBy.DropDownItems, "Text", false, null, MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Title).Checked = lvSorter.ComparisonType == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Title;
            AddMenuItem(SortBy.DropDownItems, "Date", false, null, MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Date).Checked = lvSorter.ComparisonType == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Date;
            AddMenuItem(SortBy.DropDownItems, "Violation type", false, null, MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.ViolType).Checked = lvSorter.ComparisonType == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.ViolType;
            AddMenuItem(SortBy.DropDownItems, "Element type", false, null, MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.ElemType).Checked = lvSorter.ComparisonType == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.ElemType;
            AddMenuItem(SortBy.DropDownItems, "Operator", false, null, MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Operator).Checked = lvSorter.ComparisonType == MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum.Title;

            ToolStripMenuItem ViewMode = AddMenuItem(Items, "View mode", false) as ToolStripMenuItem;
            foreach (View ThisView in Enum.GetValues(typeof(View)))
                if (ThisView != View.Tile)
                    AddMenuItem(ViewMode.DropDownItems, ThisView.ToString(), false, null, ThisView);

            AddMenuItem(Items, "-", false);
            AddMenuItem(Items, "Select all", false);
            AddMenuItem(Items, "Select none", false);
            AddMenuItem(Items, "Invert selection", false);
            base.Show(ts, Location);
        }

        /// <summary>
        /// Show the context menu strip, for multiple elements.
        /// </summary>
        /// <param name="ParentControl">The parent control for the item</param>
        /// <param name="Location">The location at which the menu should be shown (relative to the parent control)</param>
        /// <param name="Violations">The collection of violations driving the display</param>
        /// <param name="NewCount">The number of new elements</param>
        /// <param name="AllowMapMoving">Whether the user can move the map</param>
        public void Show(Control ParentControl, Point Location, List<MM_AlarmViolation> Violations, int NewCount, bool AllowMapMoving)
        {
            this.OwnerObject = ParentControl;
            this.BaseElement = null;
            this.AllowMapMoving = AllowMapMoving;

            this.Items.Clear();
            this.Tag = Violations;



            AddMenuItem(Items, "Selected items: " + Violations.Count.ToString("#,##0"), true);
            if (NewCount > 0)
                AddMenuItem(Items, "New violations: " + NewCount.ToString("#,##0"), true);
            AddMenuItem(Items, "-", true);
            if (NewCount > 0)
                AddMenuItem(Items, "Acknowledge selected", false);
            AddMenuItem(Items, "Archive selected", false);
            base.Show(ParentControl, Location);
        }

        /// <summary>
        /// Set an MM click handler, and display the mini-map control
        /// </summary>
        /// <param name="ParentControl"></param>
        /// <param name="Location"></param>
        public void Show(MM_Mini_Map ParentControl, Point Location)
        {
            this.OwnerObject = ParentControl;
            this.Items.Clear();


            //Display the header info
            AddMenuItem(Items, "Display flow in", false).Checked = ParentControl.ShowFlowIn;
            AddMenuItem(Items, "Display flow out", false).Checked = ParentControl.ShowFlowOut;
            AddMenuItem(Items, "Display generation", false).Checked = ParentControl.ShowGen;
            AddMenuItem(Items, "Display load", false).Checked = ParentControl.ShowLoad;
            AddMenuItem(Items, "Display capacitors", false).Checked = ParentControl.ShowCaps;
            AddMenuItem(Items, "Display reactors", false).Checked = ParentControl.ShowReactors;
            AddMenuItem(Items, "Display MVA", false).Checked = ParentControl.ShowMVA;
            AddMenuItem(Items, "Display generation capacity online only", false).Checked = ParentControl.ShowOnlineGenerationOnly;
            AddMenuItem(Items, "Display emergency generation", false).Checked = ParentControl.ShowEmergencyMode;
            AddMenuItem(Items, "-", true);
            AddMenuItem(Items, "Display elements within zoom region", false).Checked = ParentControl.DisplayElementsWithinZoomRegion;
            AddMenuItem(Items, "Display map position", false).Checked = ParentControl.DisplayMapPosition;
            AddMenuItem(Items, "Display violations", false).Checked = ParentControl.DisplayViolations;

            if (!ParentControl.isNetworkMap)
            {
                AddMenuItem(Items, "-", true);
                AddMenuItem(Items, "State-wide view", false).Checked = !ParentControl.ShowRegionalView;
                AddMenuItem(Items, "Regional view", false).Checked = ParentControl.ShowRegionalView;
                AddLineFlowItems(Items, ParentControl.BorderingLines);


                List<MM_Line> Lines = new List<MM_Line>();
                if (ParentControl.BaseData.Data.Tables.Contains("Line"))
                    foreach (DataRow dR in ParentControl.BaseData.Data.Tables["Line"].Rows)
                        Lines.Add(dR[0] as MM_Line);
                if (Lines.Count > 0)
                {
                    AddMenuItem(Items, "-", true);
                    AddLineFlowItems(AddMenuItem(Items, "Within region", false).DropDownItems, Lines);
                }

                List<MM_Unit> Units = new List<MM_Unit>();
                if (ParentControl.BaseData.Data.Tables.Contains("Unit"))
                    foreach (System.Data.DataRow dR in ParentControl.BaseData.Data.Tables["Unit"].Rows)
                        Units.Add(dR[0] as MM_Unit);

                if (Units.Count > 0)
                    AddExpandableMenuItem(Items, "Units (" + Units.Count.ToString() + ")", false, null, Units);


            }
            base.Show(ParentControl, Location);
        }


        /// <summary>
        /// Recursively assign a click handler to menu items
        /// </summary>
        /// <param name="Item"></param>
        private void AssignClickHandler(ToolStripItem Item)
        {
            if (Item is ToolStripMenuItem)
            {
                Item.Click += new EventHandler(HandleItemClick);
                foreach (ToolStripItem SubItem in (Item as ToolStripMenuItem).DropDownItems)
                    AssignClickHandler(SubItem);
            }

        }
        #endregion

        #region Element identification
        /// <summary>
        /// Determine if an element is a transmission element
        /// </summary>
        /// <param name="ElementToTest"></param>
        /// <returns></returns>
        private bool IsTransmissionElement(MM_Element ElementToTest)
        {
            if (ElementToTest is MM_Substation)
                return true;
            else if (ElementToTest is MM_Line)
                return true;
            else if (ElementToTest is MM_AlarmViolation)
                return IsTransmissionElement((ElementToTest as MM_AlarmViolation).ViolatedElement);
            else
                return ElementToTest.Substation != null;
        }

        #endregion

        #region Item generation
        /// <summary>
        /// Build all of the items for the element
        /// </summary>
        /// <param name="BaseElement">The MM element for which the items should be developed</param>
        /// <param name="Items">The collection of items</param>
        private void BuildItems(MM_Element BaseElement, ToolStripItemCollection Items)
        {
            //First, add the item's detail.
            try
            {
                AddItemDetails(BaseElement, Items);
                AddMenuItem(Items, "-", true);

                //Add in ownership/operatorship
                if (Data_Integration.Permissions.ShowOwnership && BaseElement is MM_Boundary == false)
                    AddOwnershipItems(BaseElement, Items);

            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }

            //If a transmission element, show one-line and move map options when appropriate
            MM_Element DisplayElem = BaseElement is MM_AlarmViolation ? (BaseElement as MM_AlarmViolation).ViolatedElement : BaseElement;

            if (IsTransmissionElement(DisplayElem))
            {
                //If we're a menu for a one-line panel, offer to show synchroscopes if appropriate
                if (OwnerObject is MM_OneLine_Viewer && DisplayElem is MM_Breaker_Switch && ((MM_Breaker_Switch)DisplayElem).HasSynchroscope)
                {
                    MM_OneLine_Viewer Viewer = (MM_OneLine_Viewer)OwnerObject;
                    MM_OneLine_Element Elem = Viewer.DisplayElements[DisplayElem];
                    List<MM_Element> FoundElems = new List<MM_Element>();
                    FoundElems.Add(DisplayElem);
                    foreach (KeyValuePair<MM_Element, MM_OneLine_Node> Node in Viewer.DisplayNodes)
                        if (Node.Value.NodeTargets.ContainsKey(Elem))
                            FoundElems.Add(Node.Key);
                    AddMenuItem(Items, "Display synchroscope", false, MM_Repository.ViolationImages.Images["OneLine"], FoundElems.ToArray());
                }
                else if (Data_Integration.Permissions.ShowOneLines && OwnerObject is MM_OneLine_Viewer == false)
                {
                    if (DisplayElem is MM_Substation)
                        AddMenuItem(Items, "Display one-line", false, MM_Repository.ViolationImages.Images["OneLine"], DisplayElem);

                    if (DisplayElem.Substation != null)
                        AddMenuItem(Items, "Display substation one-line: " + DisplayElem.Substation.LongName, false, MM_Repository.ViolationImages.Images["OneLine"], DisplayElem.Substation);

                    if (DisplayElem.Contingencies != null && DisplayElem.Contingencies.Count > 0)
                    {
                        DisplayElem.Contingencies = DisplayElem.Contingencies.Distinct().ToList();

                        foreach (MM_Contingency Ctg in DisplayElem.Contingencies)
                            if (Ctg is MM_Flowgate) //Element properties
                                AddMenuItem(Items, string.Format("FG {0} {1:##0%} Properties", Ctg.Name, ((MM_Flowgate)Ctg).PercentLoaded), false, MM_Repository.ViolationImages.Images["Properties"], Ctg);
                            else
                                AddMenuItem(Items, "Display b2b one-line: " + Ctg.Name + " / " + Ctg.Description, false, MM_Repository.ViolationImages.Images["OneLine"], Ctg);
                    }
                    if (DisplayElem is MM_Line)
                        foreach (MM_Substation Sub in new MM_Substation[] { (DisplayElem as MM_Line).Substation1, (DisplayElem as MM_Line).Substation2 })
                            AddMenuItem(Items, "Display substation one-line: " + Sub.LongName, false, MM_Repository.ViolationImages.Images["OneLine"], Sub);
                }

                //Also, offer the map move option if a transmission element and map moving is intended
                if (AllowMapMoving && Data_Integration.Permissions.ShowMoveMap)
                    AddMenuItem(Items, "Move map", false, MM_Repository.ViolationImages.Images["MoveMap"], DisplayElem);
            }

            //If a county, show the properties
            if (BaseElement is MM_Boundary)
            {
                if (Data_Integration.Permissions.ShowWebsite && (BaseElement as MM_Boundary).Website != "#")
                    AddMenuItem(Items, "Open Website", false, null, BaseElement);

                AddMenuItem(Items, "Lasso Area", false, null, BaseElement);

            }

            //Show the element properties dialog  

            if (Data_Integration.Permissions.ShowPropertyPage && BaseElement.ElemType != null && !BaseElement.ElemType.Name.Equals("Contingency"))
                AddMenuItem(Items, "Element properties", false, MM_Repository.ViolationImages.Images["Properties"], BaseElement);

            //If we have violations, show them.
            if (BaseElement.Violations.Count > 0 && Data_Integration.Permissions.ShowViolations)
            {
                AddMenuItem(Items, "-", true);
                foreach (MM_AlarmViolation Viol in BaseElement.Violations.Values)
                    AddExpandableMenuItem(Items, Viol.ToString(), false, MM_Repository.ViolationImages.Images[Viol.Type.ViolationIndex], Viol);
            }

            //If we have any notes, show them.
            MM_Note[] Notes;
            if (Data_Integration.Permissions.ShowNotes && (Notes = BaseElement.FindNotes()).Length > 0)
            {
                AddMenuItem(Items, "-", true);
                AddExpandableMenuItem(Items, "Notes: " + Notes.Length.ToString("#,##0"), false, MM_Repository.ViolationImages.Images["Note"], Notes);
            }

            //Add our 'create note' dialog
            if (Data_Integration.Permissions.CreateNotes && BaseElement.ElemType != null)
                AddMenuItem(Items, "Write note", false, MM_Repository.ViolationImages.Images["Note"], BaseElement);

            //If this is a violation, show the acknowledgement option
            if (BaseElement is MM_AlarmViolation && Data_Integration.Permissions.ShowViolations)
            {
                if ((BaseElement as MM_AlarmViolation).New)
                    AddMenuItem(Items, "Acknowledge", false, MM_Repository.ViolationImages.Images["Acknowledge"], BaseElement);
                else
                    AddMenuItem(Items, "Archive", false, MM_Repository.ViolationImages.Images["Archive"], BaseElement);
            }

            //If we have commands, run through
            List<MM_Command> Commands = new List<MM_Command>();
            foreach (MM_Command CmdToTest in Data_Integration.Commands)
                if (CmdToTest.Validate(BaseElement))
                    Commands.Add(CmdToTest);
            if (Commands.Count > 0)
            {
                AddMenuItem(Items, "-", false);
                foreach (MM_Command CmdToAdd in Commands)
                    AddMenuItem(ItemLocation(CmdToAdd.Title, Items), CmdToAdd.Title.Substring(CmdToAdd.Title.LastIndexOf('\\') + 1), false, null, new KeyValuePair<MM_Command, MM_Element>(CmdToAdd, BaseElement));
            }
        }

        /// <summary>
        /// Locate and/or build a path for items.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Collection"></param>
        /// <returns></returns>
        private ToolStripItemCollection ItemLocation(String Path, ToolStripItemCollection Collection)
        {
            String[] splStr = Path.Split('\\');
            for (int a = 0; a < splStr.Length - 1; a++)
                if (!Collection.ContainsKey(splStr[a]))
                {
                    ToolStripMenuItem NewItem = AddMenuItem(Collection, splStr[a], false);
                    NewItem.Name = splStr[a];
                    Collection = NewItem.DropDownItems;
                }
                else
                {
                    ToolStripItem MenuItem = Collection[splStr[a]];
                    Collection = (MenuItem as ToolStripMenuItem).DropDownItems;
                }
            return Collection;
        }



        /// <summary>
        /// Build summary items 
        /// </summary>
        /// <param name="Substations"></param>
        /// <param name="Items"></param>
        /// <returns>Whether new items were added</returns>
        private bool AddBusAndShuntItems(MM_Substation[] Substations, ToolStripItemCollection Items)
        {
            List<MM_Bus> Buses = new List<MM_Bus>();
            List<MM_Bus> BusAngles = new List<MM_Bus>();
            List<MM_ShuntCompensator> Caps = new List<MM_ShuntCompensator>();
            List<MM_ShuntCompensator> Reactors = new List<MM_ShuntCompensator>();
            List<MM_Unit> ReactiveUnits = new List<MM_Unit>(), CapacitiveUnits = new List<MM_Unit>(), OfflineUnits = new List<MM_Unit>();
            float CapCount = 0, CapOnline = 0, CapMVAR = 0, CapNom = 0, CapWorst = float.NaN, CapRemaining = 0, UnitCapCount=0, UnitCapMVAR=0, UnitCapRemaining=0;
            float ReacCount = 0, ReacOnline = 0, ReacMVAR = 0, ReacNom = 0, ReacWorst = float.NaN, ReacRemaining = 0, UnitReacCount = 0, UnitReacMVAR=0, UnitReacRemaining = 0;
            float OfflineUnitReactive = 0, OfflineUnitCapacitive = 0;

            float BusSum = 0f, BusSum2 = 0f, BusNum = 1f, BusDenom = 1f;
            float BusAngSum = 0f, BusAngSum2 = 0f, BusAngNum = 1f, BusAngDenom = 1f;
            float HighAngle = float.NaN, LowAngle = float.NaN;

            //Go through every substation, and add to our collection
            foreach (MM_Substation Sub in Substations)
            {               
                    foreach (MM_Bus Bus in Sub.Buses)
                        if (Bus.Permitted && Bus.KVLevel.ShowInMenu && !Bus.Dead && !Buses.Contains(Bus) && !float.IsNaN(Bus.Estimated_kV) && Bus.Estimated_kV != 0)
                        {
                            if (float.IsNaN(ReacWorst) || (Bus.PerUnitVoltage > ReacWorst))
                                ReacWorst = Bus.PerUnitVoltage;
                            if (float.IsNaN(CapWorst) || (Bus.PerUnitVoltage < CapWorst))
                                CapWorst = Bus.PerUnitVoltage;

                            if (float.IsNaN(HighAngle) || Bus.Estimated_Angle > HighAngle)
                                HighAngle = Bus.Estimated_Angle;
                            if (float.IsNaN(LowAngle) || Bus.Estimated_Angle < LowAngle)
                                LowAngle = Bus.Estimated_Angle;


                            Buses.Add(Bus);
                            BusSum += Bus.PerUnitVoltage;
                            BusSum2 += Bus.PerUnitVoltage * Bus.PerUnitVoltage;
                            BusNum += (Bus.PerUnitVoltage * Math.Abs(1f - Bus.PerUnitVoltage) * Bus.Estimated_kV);
                            BusDenom += Math.Abs(1f - Bus.PerUnitVoltage) * Bus.Estimated_kV;

                            BusAngles.Add(Bus);
                            BusAngSum += Bus.Estimated_Angle;
                            BusAngSum2 += Bus.Estimated_Angle * Bus.Estimated_Angle;
                            BusAngNum += (Bus.Estimated_Angle * Bus.Estimated_kV);
                            BusAngDenom += Bus.Estimated_kV;
                        }

                if (Sub.Units != null)
                    foreach (MM_Unit Unit in Sub.Units)
                        if (Unit.MVARCapabilityCurve.Length > 0)
                        if (!float.IsNaN(Unit.Estimated_MVAR) && Unit.Estimated_MVAR>0)
                        {
                            ReactiveUnits.Add(Unit);
                            UnitReacCount++;
                            UnitReacMVAR += Unit.Estimated_MVAR;
                            UnitReacRemaining += Unit.UnitMVARCapacity - Unit.Estimated_MVAR;
                        }
                        else if (!float.IsNaN(Unit.Estimated_MVAR) && Unit.Estimated_MVAR < 0 )
                        {
                            CapacitiveUnits.Add(Unit);
                            UnitCapCount++;
                            UnitCapMVAR += Unit.Estimated_MVAR;
                            UnitCapRemaining += Unit.UnitMVARCapacity - Unit.Estimated_MVAR;
                        }
                        else if (float.IsNaN(Unit.Estimated_MVAR) || Unit.Estimated_MVAR==0f)
                        {
                            OfflineUnitReactive += Unit.MVARCapabilityCurve[1];
                            OfflineUnitCapacitive += Unit.MVARCapabilityCurve[2];
                            OfflineUnits.Add(Unit);
                        }

                if (Sub.ShuntCompensators != null)
                {
                    foreach (MM_ShuntCompensator Shunt in Sub.ShuntCompensators)
                        if (Shunt.Permitted)
                        {
                            if (Shunt.ElemType.Name == "Capacitor")
                            {
                                Caps.Add(Shunt);
                                CapCount++;
                                CapNom += Shunt.Nominal_MVAR;
                                if (Shunt.Open)
                                    CapRemaining += Shunt.Nominal_MVAR;
                                else
                                {
                                    CapOnline++;
                                    CapMVAR += Shunt.Estimated_MVAR;
                                }
                            }
                            else if (Shunt.ElemType.Name == "Reactor")
                            {
                                Reactors.Add(Shunt);
                                ReacCount++;
                                ReacNom += Shunt.Nominal_MVAR;
                                if (Shunt.Open)
                                    ReacRemaining += Shunt.Nominal_MVAR;
                                else
                                {
                                    ReacOnline++;
                                    ReacMVAR += Shunt.Estimated_MVAR;
                                }
                            }

                        }
                }
            }



            if (Buses.Count > 0)
            {
                AddMenuItem(Items, "-", false);
                Buses.Sort();
                Buses.TrimExcess();
                float WeightedpU = BusNum / BusDenom;
                float AveragepU = (BusSum / (float)Buses.Count);
                float StdDevpU = (float)Math.Sqrt((((float)Buses.Count * BusSum2) - (BusSum * BusSum)) / ((float)Buses.Count * ((float)Buses.Count - 1f)));

                BusAngles.Sort(new BusPhaseSorter());
                float WeightedAngle = BusAngNum / BusAngDenom;
                float AverageAngle = BusAngSum / (float)Buses.Count;
                float StdDevAngle = (float)Math.Sqrt((((float)Buses.Count * BusAngSum2) - (BusAngSum * BusAngSum)) / ((float)Buses.Count * ((float)Buses.Count - 1f)));

                //If the user wishes to have the buses split out by KV level, do so
                if (MM_Repository.OverallDisplay.SplitBusesByVoltage)
                {
                    SortedDictionary<MM_KVLevel, Dictionary<MM_Substation, List<MM_Bus>>> VoltageByKV = new SortedDictionary<MM_KVLevel, Dictionary<MM_Substation, List<MM_Bus>>>();
                    SortedDictionary<MM_KVLevel, Dictionary<MM_Substation, List<MM_Bus>>> AngleByKV = new SortedDictionary<MM_KVLevel, Dictionary<MM_Substation, List<MM_Bus>>>();
                    foreach (MM_Bus Bus in Buses)
                        if (Bus.KVLevel != null && !float.IsNaN(Bus.Estimated_kV) && Bus.Estimated_kV != 0)
                        {
                            Dictionary<MM_Substation, List<MM_Bus>> FoundVoltage;
                            if (!VoltageByKV.TryGetValue(Bus.KVLevel, out FoundVoltage))
                                VoltageByKV.Add(Bus.KVLevel, FoundVoltage = new Dictionary<MM_Substation, List<MM_Bus>>());

                            List<MM_Bus> BusList;
                            if (Bus.Substation != null)
                            {
                                if (!FoundVoltage.TryGetValue(Bus.Substation, out BusList))
                                    FoundVoltage.Add(Bus.Substation, BusList = new List<MM_Bus>(8));
                                BusList.Add(Bus);
                            }
                        }
                    foreach (MM_Bus Bus in BusAngles)
                        if (Bus.KVLevel != null)
                        {
                            Dictionary<MM_Substation, List<MM_Bus>> FoundVoltage;
                            if (!AngleByKV.TryGetValue(Bus.KVLevel, out FoundVoltage))
                                AngleByKV.Add(Bus.KVLevel, FoundVoltage = new Dictionary<MM_Substation, List<MM_Bus>>());

                            List<MM_Bus> BusList;
                            if (Bus.Substation != null)
                            {
                                if (!FoundVoltage.TryGetValue(Bus.Substation, out BusList))
                                    FoundVoltage.Add(Bus.Substation, BusList = new List<MM_Bus>(8));
                                BusList.Add(Bus);
                            }
                        }

                    AddExpandableMenuItem(Items, String.Format("Weighted pU: {0:0.0%} ({1:0.0%} ± {2:0.0%})", WeightedpU, AveragepU, StdDevpU), false, ViolationImage(WorstViolation(Buses)), VoltageByKV);
                    AddExpandableMenuItem(Items, String.Format("Weighted angles: {0:0.0}° ({1:0.0}° ± {2:0.0}°), {3:0.0}° to {4:0.0}°", WeightedAngle, AverageAngle, StdDevAngle, LowAngle, HighAngle), false, ViolationImage(WorstViolation(BusAngles)), AngleByKV);
                }
                else
                {
                    AddExpandableMenuItem(Items, String.Format("Weighted pU: {0:0.0%} ({1:0.0%} ± {2:0.0%})", WeightedpU, AveragepU, StdDevpU), false, ViolationImage(WorstViolation(Buses)), Buses);
                    AddExpandableMenuItem(Items, String.Format("Weighted angles: {0:0.0}° ({1:0.0}° ± {2:0.0}°), {3:0.0}° to {4:0.0}°", WeightedAngle, AverageAngle, StdDevAngle, LowAngle, HighAngle), false, ViolationImage(WorstViolation(BusAngles)), BusAngles);
                }

                //Add in our voltage based on reactors
                if (Reactors.Count > 0)
                {
                    Reactors.Sort();
                    Reactors.TrimExcess();                    
                    AddExpandableMenuItem(Items, "Worst +pU: " + ReacWorst.ToString("0.0%") + " Reacs (" + ReacOnline.ToString("#,##0") + "/" + ReacCount.ToString("#,##0") + "): " + ReacMVAR.ToString("#,##0.0") + " MVar online, " + ReacRemaining.ToString("#,##0") + " MVar remaining", false, ViolationImage(WorstViolation(Reactors)), Reactors);
                }

                    //Add in our voltage based on reactive units
                if (ReactiveUnits.Count > 0)
                {
                    ReactiveUnits.Sort();
                    ReactiveUnits.TrimExcess();
                    AddExpandableMenuItem(Items, "Worst +pU: " + ReacWorst.ToString("0.0%") + " Units (" + UnitReacCount.ToString("#,##0") + "): " + UnitReacMVAR.ToString("#,##0.0") + " MVar online, " + UnitReacRemaining.ToString("#,##0") + " MVar remaining", false, ViolationImage(WorstViolation(ReactiveUnits)), ReactiveUnits);
                }

                //If we have no reactors or units, show accordingly.
                if (Reactors.Count + ReactiveUnits.Count == 0 && !float.IsNaN(ReacWorst))
                    AddMenuItem(Items, "Worst +pU: " + ReacWorst.ToString("0.0%"), true);

                //If we have offline units, show thier 0th point MVAR potential
                if (OfflineUnits.Count > 0)
                {
                    OfflineUnits.Sort();
                    OfflineUnits.TrimExcess();
                    AddExpandableMenuItem(Items, "Offline Unit MVAR: " + OfflineUnits.Count.ToString("#,##0") + " (" + OfflineUnitReactive.ToString("#,##0") + " / " + OfflineUnitCapacitive.ToString("#,##0") + " MVAR)", false, ViolationImage(WorstViolation(OfflineUnits)), OfflineUnits);
                }

                //If we have units with capacitive capacity (ha!) show.
                if (CapacitiveUnits.Count > 0)
                {
                    CapacitiveUnits.Sort();
                    CapacitiveUnits.TrimExcess();
                    AddExpandableMenuItem(Items, "Worst -pU: " + CapWorst.ToString("0.0%") + " Units (" + UnitCapCount.ToString("#,##0") + "): " + UnitCapMVAR.ToString("#,##0.0") + " MVar online, " + UnitCapRemaining.ToString("#,##0") + " MVar remaining", false, ViolationImage(WorstViolation(CapacitiveUnits)), CapacitiveUnits);
                }

                //If we have capacitors, show their potential
                if (Caps.Count > 0)
                {
                    Caps.Sort();
                    Caps.TrimExcess();
                    AddExpandableMenuItem(Items, "Worst -pU: " + CapWorst.ToString("0.0%") + " Caps (" + CapOnline.ToString("#,##0") + "/" + CapCount.ToString("#,##0") + "): " + CapMVAR.ToString("#,##0.0") + " MVar online, " + CapRemaining.ToString("#,##0") + " MVar remaining", false, ViolationImage(WorstViolation(Caps)), Caps);
                }

                //If we have no capacity available, show our lowest voltage
                if (Caps.Count + CapacitiveUnits.Count == 00 && !float.IsNaN(CapWorst))
                    AddMenuItem(Items, "Worst -pU: " + CapWorst.ToString("0.0%"), true);

            }
            return Buses.Count > 0 || Caps.Count > 0 || Reactors.Count > 0;
        }

        private bool AddUnitItems(MM_Substation[] Substations, ToolStripItemCollection Items)
        {
            return true;
        }


        /// <summary>
        /// Add a boundary's details to the menu
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <param name="Items"></param>
        private void AddBoundaryItemDetails(MM_Boundary BaseElement, ToolStripItemCollection Items)
        {
            AddMenuItem(Items, "Area " + BaseElement.Name, true);
            AddMenuItem(Items, "-", true);
            if (BaseElement.WeatherStations != null && BaseElement.WeatherStations.Count > 0)
                foreach (MM_WeatherStation wx in BaseElement.WeatherStations)
                    AddExpandableMenuItem(Items, wx.Description, true, null, wx);

            if (BaseElement.WeatherAlerts != null && BaseElement.WeatherAlerts.Count > 0)
                AddExpandableMenuItem(Items, "Weather alerts: " + BaseElement.WeatherAlerts.Count.ToString("#,##0"), true, null, BaseElement.WeatherAlerts);

            if (BaseElement.WeatherForecast != null && BaseElement.WeatherForecast.Count > 0)
                AddExpandableMenuItem(Items, "Weather forecast", false, null, BaseElement.WeatherForecast);



            //Now, add in our collection of units, loads, buses, caps and reactors.
            List<MM_Zone> WeatherZones = new List<MM_Zone>();
            List<MM_Zone> LoadZones = new List<MM_Zone>();

            List<MM_Unit> Units = new List<MM_Unit>();
            List<MM_Load> Loads = new List<MM_Load>();
            List<MM_Load> LAARs = new List<MM_Load>();
            SortedDictionary<MM_KVLevel, List<MM_Substation>> Substations = new SortedDictionary<MM_KVLevel, List<MM_Substation>>();
            float UnitCount = 0, UnitOnline = 0, UnitMW = 0, UnitHASL = 0, UnitHSL = 0;
            float LoadCount = 0, LoadOnline = 0, LoadMW = 0;
            float LAARCount = 0, LAAROnline = 0, LAARMW = 0;
            int zindex = 0;

            foreach (MM_Substation Sub in BaseElement.Substations)
            {
                foreach (MM_KVLevel KVLevel in Sub.KVLevels)
                    if (KVLevel.Permitted)
                    {
                        if (!Substations.ContainsKey(KVLevel))
                            Substations.Add(KVLevel, new List<MM_Substation>());
                        Substations[KVLevel].Add(Sub);
                        if (Sub.WeatherZone != null && !WeatherZones.Contains(Sub.WeatherZone))
                            WeatherZones.Add(Sub.WeatherZone);
                        if (Sub.LoadZone != null && !LoadZones.Contains(Sub.LoadZone))
                            LoadZones.Add(Sub.LoadZone);
                        else if (Sub.Area != null && !LoadZones.Exists(a => a.Name == Sub.Area))
                        {
                            LoadZones.Add(new MM_Zone(Sub.Area,zindex++));
                        }
                    }

                if (Sub.Units != null)
                    foreach (MM_Unit Unit in Sub.Units)
                        if (Unit.Permitted)
                        {
                            Units.Add(Unit);
                            UnitCount++;
                            if (!float.IsNaN(Unit.Estimated_MW))
                                UnitMW += Unit.Estimated_MW;
                            UnitHASL += Unit.RegUp;
                            UnitHSL += Unit.EcoMax;
                            if (Unit.Estimated_MW >= 1f)
                                UnitOnline++;
                        }

                if (Sub.Loads != null)
                    foreach (MM_Load Load in Sub.Loads)
                        if (Load.Permitted && Load.ElemType.Name == "Load")
                        {
                            Loads.Add(Load);
                            LoadCount++;
                            if (!float.IsNaN(Load.Estimated_MW))
                                LoadMW += Load.Estimated_MW;
                            if (Load.Estimated_MW >= 1f)
                                LoadOnline++;
                        }
                        else if (Load.Permitted && Load.ElemType.Name == "LAAR")
                        {
                            LAARs.Add(Load);
                            LAARCount++;
                            if (!float.IsNaN(Load.Estimated_MW))
                                LAARMW += Load.Estimated_MW;
                            if (Load.Estimated_MW >= 1f)
                                LAAROnline++;
                        }
            }




            if (WeatherZones.Count == 1)
                AddExpandableMenuItem(Items, "Weather zone: " + WeatherZones[0].Name, false, ViolationImage(WeatherZones[0].WorstViolationOverall), WeatherZones[0]);
            else if (WeatherZones.Count > 1)
                AddExpandableMenuItem(Items, "Weather zones (" + WeatherZones.Count.ToString() + ")", false, null, WeatherZones);

            if (LoadZones.Count == 1)
                AddExpandableMenuItem(Items, "Load zone: " + LoadZones[0].Name, false, ViolationImage(LoadZones[0].WorstViolationOverall), LoadZones[0]);
            else if (LoadZones.Count > 1)
                AddExpandableMenuItem(Items, "Load zones (" + LoadZones.Count.ToString() + ")", false, null, LoadZones);

            if (Units.Count > 0)
            {
                Units.Sort();
                Units.TrimExcess();
                AddExpandableMenuItem(Items, "Units (" + UnitOnline.ToString("#,##0") + "/" + UnitCount.ToString("#,##0") + "): " + UnitMW.ToString("#,##0.0") + " mw, " + (UnitHASL - UnitMW).ToString("#,##0") + " rem.", false, ViolationImage(WorstViolation(Units)), Units);
            }

            if (Loads.Count > 0)
            {
                Loads.Sort();
                Loads.TrimExcess();
                AddExpandableMenuItem(Items, "Loads (" + LoadOnline.ToString("#,##0") + "/" + LoadCount.ToString("#,##0") + "): " + LoadMW.ToString("#,##0.0") + " mw", false, ViolationImage(WorstViolation(Loads)), Loads);
            }

            if (LAARs.Count > 0)
            {
                LAARs.Sort();
                LAARs.TrimExcess();
                AddExpandableMenuItem(Items, "LAARs (" + LAAROnline.ToString("#,##0") + "/" + LAARCount.ToString("#,##0") + "): " + LAARMW.ToString("#,##0.0") + " mw", false, ViolationImage(WorstViolation(LAARs)), LAARs);
            }

            AddBusAndShuntItems(BaseElement.Substations.ToArray(), Items);


            //Now, add in our substations
            if (Substations.Count > 0)
            {
                AddMenuItem(Items, "-", true);
                foreach (KeyValuePair<MM_KVLevel, List<MM_Substation>> SubKVs in Substations)
                {
                    SubKVs.Value.TrimExcess();
                    SubKVs.Value.Sort();
                    Image WorstViol = ViolationImage(WorstViolation(SubKVs.Value));
                    if (WorstViol == null)
                    {
                        foreach (MM_Substation Sub in SubKVs.Value)
                            if (Sub.FindNotes().Length > 0)
                            {
                                WorstViol = MM_Repository.ViolationImages.Images["Note"];
                                break;
                            }
                    }

                    AddExpandableMenuItem(Items, SubKVs.Key.Name + " substations (" + SubKVs.Value.Count.ToString("#,##0") + ")", false, WorstViol, SubKVs.Value);
                }
            }

            AddLineFlowItems(Items, BaseElement);
        }

        /// <summary>
        /// This class sorts bus voltages by phase angle
        /// </summary>
        private class BusPhaseSorter : IComparer<MM_Bus>
        {
            #region IComparer<MM_BusbarSection> Members

            public int Compare(MM_Bus x, MM_Bus y)
            {
                return x.Estimated_Angle.CompareTo(y.Estimated_Angle);
            }

            #endregion
        }

        private class LineRatingComparer : IComparer<MM_Line>
        {
            public int Compare(MM_Line x, MM_Line y)
            {
                return -x.LinePercentage.CompareTo(y.LinePercentage);
            }
        }

        /// <summary>
        /// Add details for a note
        /// </summary>
        /// <param name="BaseElement">The note to be added</param>
        /// <param name="Items">The item list</param>
        private void AddNoteItemDetails(MM_Note BaseElement, ToolStripItemCollection Items)
        {
            MM_Element AssociatedElement;
            if (!MM_Repository.TEIDs.TryGetValue(BaseElement.AssociatedElement, out AssociatedElement))
                return;
            AddExpandableMenuItem(Items, "Note on " + AssociatedElement.ElemType.Name + " " + AssociatedElement.Name, true, ViolationImage(AssociatedElement.WorstViolationOverall), BaseElement.AssociatedElement);
            AddMenuItem(Items, "Created on " + BaseElement.CreatedOn.ToString(), true);
            AddMenuItem(Items, "Created by " + BaseElement.Author, true);
            AddMenuItem(Items, "-", true);
            AddMenuItem(Items, BaseElement.Note, true);
        }


        /// <summary>
        /// Add a substation's detials to a menu
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <param name="Items"></param>
        private void AddSubstationItemDetails(MM_Substation BaseElement, ToolStripItemCollection Items)
        {
            if (!String.Equals(BaseElement.LongName, BaseElement.Name, StringComparison.CurrentCultureIgnoreCase))
                AddMenuItem(Items, "Substation " + BaseElement.LongName + " (" + BaseElement.Name + ")", true, ViolationImage(BaseElement.WorstViolationOverall), BaseElement);
            else
                AddMenuItem(Items, "Substation " + BaseElement.Name.Replace("TOPOLOGY.", ""), true, ViolationImage(BaseElement.WorstViolationOverall), BaseElement);
            if (BaseElement.County != null && Data_Integration.Permissions.ShowCounties)
            {
                AddExpandableMenuItem(Items, "  Area: " + BaseElement.County.Name, true, ViolationImage(BaseElement.County.WorstViolationOverall), BaseElement.County);
                AddMenuItem(Items, "  Est. Location: " + BaseElement.LngLat.Y.ToString("#.00") + ", " + BaseElement.LngLat.X.ToString("#.00"), true);
            }

            if (BaseElement.County != null && MM_Repository.OverallDisplay.DisplayDistricts)
                AddExpandableMenuItem(Items, "  District: " + BaseElement.District.Name, true, ViolationImage(BaseElement.District.WorstViolationOverall), BaseElement.District);

            if (BaseElement.Loads.Count > 0 || BaseElement.Units.Count > 0)
            {
                var lmp = BaseElement.GetAverageLMP();
                if (lmp != 0)
                    AddMenuItem(Items, string.Format("  Avg LMP: {0:$##0.00}", BaseElement.GetAverageLMP()), true);
            }

            if (BaseElement.WeatherZone != null)
                AddExpandableMenuItem(Items, "  Weather zone: " + BaseElement.WeatherZone.Name, true, ViolationImage(BaseElement.WeatherZone.WorstViolationOverall), BaseElement.WeatherZone);
            if (BaseElement.LoadZone != null)
                AddExpandableMenuItem(Items, "  Load zone: " + BaseElement.LoadZone.Name, true, ViolationImage(BaseElement.LoadZone.WorstViolationOverall), BaseElement.LoadZone);


            if (Data_Integration.Permissions.ShowElementTypes)
                AddMenuItem(Items, "  Elements:  " + BaseElement.ElementTypeList, true);
            if (Data_Integration.Permissions.ShowKVLevels)
                AddMenuItem(Items, "  KV Levels: " + BaseElement.KVLevelList, true);
            if (Data_Integration.Permissions.ShowBreakerToBreakers || Data_Integration.Permissions.ShowRASs)
            {
                List<MM_Contingency> Ctgs = new List<MM_Contingency>();
                List<MM_RemedialActionScheme> RASs = new List<MM_RemedialActionScheme>();
                foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                    if (BaseElement.Equals(Elem.Substation))
                    {
                        if (Elem.Contingencies != null && Elem.Contingencies.Count > 0)
                            foreach (MM_Contingency Ctg in Elem.Contingencies)
                                if (!Ctgs.Any(c => c.Name == Ctg.Name))
                                    Ctgs.Add(Ctg);
                        if (Elem.RASs != null)
                            foreach (MM_RemedialActionScheme RAS in Elem.RASs)
                                if (!RASs.Contains(RAS))
                                    RASs.Add(RAS);
                    }
                if (Data_Integration.Permissions.ShowBreakerToBreakers)
                {
                    Ctgs = Ctgs.Distinct().OrderBy(c => c.Name).ToList();
                    // remove duplicates
                    for (int i=1; i < Ctgs.Count; i++)
                        if (Ctgs[i].Name.Trim().Equals(Ctgs[i-1].Name.Trim(), StringComparison.OrdinalIgnoreCase))
                            Ctgs.RemoveAt(i--);

                    if (Ctgs.Count == 1)
                        AddExpandableMenuItem(Items, "  Trace: " + Ctgs[0].Name + " / " + Ctgs[0].Description, false, null, Ctgs[0]);
                    else if (Ctgs.Count > 0)
                        AddExpandableMenuItem(Items, "  Traces: (" + Ctgs.Count.ToString() + ")", false, null, Ctgs.ToArray());
                    if (Data_Integration.Permissions.ShowRASs)
                        if (RASs.Count == 1)
                            AddExpandableMenuItem(Items, "  RAS: " + RASs[0].ToString(), false, null, RASs[0]);
                        else if (RASs.Count > 0)
                            AddExpandableMenuItem(Items, "  RASs: (" + RASs.Count.ToString() + ")", false, null, RASs.ToArray());
                }
            }

            if (Data_Integration.Permissions.ShowTEIDs && BaseElement.TEID != 0)
                AddMenuItem(Items, "  TEID: " + BaseElement.TEID.ToString("#,##0"), false);

            //If the substation has units, sum up and provide list.
            if ((BaseElement.Units != null) || (BaseElement.Loads != null))
                AddMenuItem(Items, "-", true);

            MM_AlarmViolation_Type WorstViol = null;
            if (Data_Integration.Permissions.ShowUnits && BaseElement.Units != null)
            {
                List<MM_Unit> Units = new List<MM_Unit>();
                bool FreqCtrl = false;
                float TotalMW = 0, TotalHSL = 0, TotalHASL = 0, TotalUnits = 0, TotalOnline = 0, TotalMWWithHSL = 0;
                foreach (MM_Unit Elem in BaseElement.Units)
                    if (Elem.Permitted)
                    {
                        TotalUnits++;
                        Units.Add(Elem);
                        if (Elem.FrequencyControl)
                            FreqCtrl = true;
                         if (!float.IsNaN(Elem.HASL))
                        {
                            TotalHASL += Elem.HASL;
                            TotalHSL += Elem.HSL;
                            TotalMWWithHSL += Elem.Estimated_MW;
                        }
						else if (!float.IsNaN(Elem.RegUp))
                        {
                            TotalHASL += Elem.RegUp;
                            TotalHSL += Elem.EcoMax;
                            TotalMWWithHSL += Elem.Estimated_MW;
                        }
                         else if (!float.IsNaN(Elem.MaxCapacity))
                        {
                           // TotalHASL += Elem.MaxCapacity;
                            TotalHSL += Elem.MaxCapacity;
                            TotalMWWithHSL += Elem.Estimated_MW;
                        }


                        if (!float.IsNaN(Elem.Estimated_MW) && Elem.Estimated_MW > 0f && Elem.IsPhysical)
                        {
                            TotalOnline++;
                            TotalMW += (Elem as MM_Unit).Estimated_MW;
                            WorstViol = Elem.WorstViolation(WorstViol);
                        }
                    }

                Units.Sort();
                if (TotalUnits > 0)
                {
                    ToolStripMenuItem tsmi = AddExpandableMenuItem(Items, "Units: (" + TotalOnline.ToString("#,##0") + "/" + TotalUnits.ToString("#,##0") + "): " + TotalMW.ToString("#,##0") + " mw (" + ((TotalMW + TotalHASL) / TotalHSL).ToString("0.0%") + ", " + (TotalHSL - TotalMW).ToString("#,##0.0") + " mw rem)", false, ViolationImage(WorstViol), Units.ToArray());
                    if (FreqCtrl)
                        tsmi.ForeColor = Color.Purple;
                }
            }

            //If the substation has loads, sum up and provide list.
            if (Data_Integration.Permissions.ShowLoads && BaseElement.Loads != null)
            {
                float TotalLoadMW = 0, TotalLaarMW = 0, TotalLoads = 0, TotalLaars = 0, TotalLoadOnline = 0, TotalLaarOnline = 0;
                List<MM_Load> Loads = new List<MM_Load>();
                List<MM_Load> Laars = new List<MM_Load>();
                foreach (MM_Load Elem in BaseElement.Loads)
                    if (Elem.Permitted && Elem.ElemType.Name == "LAAR")
                    {
                        TotalLaars++;
                        Laars.Add(Elem);
                        if ((Elem as MM_Load).Estimated_MW > 0.5f)
                        {
                            TotalLaarOnline++;
                            TotalLaarMW += (Elem as MM_Load).Estimated_MW;
                        }
                    }
                    else if (Elem.Permitted)
                    {
                        TotalLoads++;
                        Loads.Add(Elem);
                        if ((Elem as MM_Load).Estimated_MW > 0f)
                        {
                            TotalLoadOnline++;
                            TotalLoadMW += (Elem as MM_Load).Estimated_MW;
                        }

                    }
                Loads.TrimExcess();
                Loads.Sort();
                Laars.TrimExcess();
                Laars.Sort();

                if (TotalLoads > 0)
                    AddExpandableMenuItem(Items, "Loads: (" + TotalLoadOnline.ToString("#,##0") + "/" + TotalLoads.ToString("#,##0") + "): " + TotalLoadMW.ToString("#,##0") + " mw", false, ViolationImage(WorstViolation(Loads)), Loads);

                if (TotalLaars > 0)
                    AddExpandableMenuItem(Items, "Laars: (" + TotalLaarOnline.ToString("#,##0") + "/" + TotalLaars.ToString("#,##0") + "): " + TotalLaarMW.ToString("#,##0") + " mw", false, ViolationImage(WorstViolation(Laars)), Laars);
            }


            if (Data_Integration.Permissions.ShowShuntCompensators && BaseElement.ShuntCompensators != null)
            {
                Dictionary<String, List<MM_ShuntCompensator>> Shunts = new Dictionary<string, List<MM_ShuntCompensator>>();
                foreach (String BaseElementType in "Capacitor,Reactor".Split(','))
                {
                    Shunts.Add(BaseElementType, new List<MM_ShuntCompensator>());
                    float MVarCurrent = 0, MVarMax = 0, MVarTally = 0;
                    foreach (MM_ShuntCompensator SC in BaseElement.ShuntCompensators)
                        if (SC.Permitted && SC.ElemType.Name == BaseElementType)
                        {
                            Shunts[BaseElementType].Add(SC);
                            MVarCurrent += SC.Estimated_MVAR;
                            MVarMax += SC.Nominal_MVAR;
                            MVarTally++;
                        }
                    if (MVarTally > 0)
                        AddExpandableMenuItem(Items, BaseElementType + "s: (" + MVarTally.ToString() + ") " + MVarCurrent.ToString("#,##0.0") + "/" + MVarMax.ToString("#,##0.0") + " MVar", false, ViolationImage(WorstViolation(Shunts[BaseElementType])), Shunts[BaseElementType]);
                }
            }

            if (Data_Integration.Permissions.ShowBuses && BaseElement.BusbarSections != null)
            {
                float pUMin = float.NaN, pUMax = float.NaN, puTally = 0, puCount = 0, puValidCount = 0;
                List<MM_Bus> Buses = new List<MM_Bus>();
                foreach (MM_Bus Busbar in BaseElement.BusbarSections)
                    if (Busbar.BusNumber != -1 && Busbar.Permitted && Busbar.KVLevel.ShowInMenu && !Busbar.Dead && !float.IsNaN(Busbar.Estimated_kV) && Busbar.Estimated_kV != 0)
                    {
                        Buses.Add(Busbar);
                        if (float.IsNaN(Busbar.PerUnitVoltage) && Busbar.PerUnitVoltage != 0)
                            puCount++;
                        else
                        {
                            if (float.IsNaN(pUMin) || pUMin > Busbar.PerUnitVoltage)
                                pUMin = Busbar.PerUnitVoltage;
                            if (float.IsNaN(pUMax) || pUMax < Busbar.PerUnitVoltage)
                                pUMax = Busbar.PerUnitVoltage;
                            puTally += Busbar.PerUnitVoltage;
                            puCount++;
                            puValidCount++;
                        }
                    }

                Buses.TrimExcess();
                Buses.Sort();
                if (puCount > 0 && puValidCount > 0)
                    AddExpandableMenuItem(Items, "Buses: (" + puValidCount + "/" + puCount + "): " + (puTally / puValidCount).ToString("0.0%") + " pU (" + pUMin.ToString("0.0%") + " pU to " + pUMax.ToString("0.0%") + " pU)", false, ViolationImage(WorstViolation(BaseElement.BusbarSections)), Buses);
                else if (puCount > 0)
                    AddExpandableMenuItem(Items, "Buses: (" + puValidCount + "/" + puCount + ")", false, ViolationImage(WorstViolation(BaseElement.BusbarSections)), Buses);
            }

            //Now, sum up our transformers, if any            
            if (Data_Integration.Permissions.ShowTransformers && BaseElement.Transformers != null)
            {
                float TotalFlow = 0f, MaxFlow = 0f, XFCount = 0f;
                List<MM_Transformer> Transformers = new List<MM_Transformer>();
                foreach (MM_Transformer XF in BaseElement.Transformers)
                    if (XF.Permitted)
                    {
                        Transformers.Add(XF);
                        TotalFlow += XF.MVAFlow;
                        if (XF.MVAFlow > XF.NormalLimit)
                            MaxFlow += XF.MVAFlow;
                        else
                            MaxFlow += XF.NormalLimit;
                        XFCount++;
                    }

                Transformers.TrimExcess();
                Transformers.Sort();
                if (XFCount > 0)
                    AddExpandableMenuItem(Items, "Transformers: (" + XFCount.ToString("#,##0") + ") " + TotalFlow.ToString("#,##0.00") + " mva  (" + (TotalFlow / MaxFlow).ToString("0%") + " - " + (MaxFlow - TotalFlow).ToString("#,##0.00") + " mva rem.)", false, ViolationImage(WorstViolation(BaseElement.Transformers)), Transformers);
            }



            //Now sum up our line flows in and out
            AddLineFlowItems(Items, BaseElement);
        }

        /// <summary>
        /// Add in menu items corresponding to line flows
        /// </summary>
        /// <param name="Items">The menu collection</param>
        /// <param name="ComparisonElement">The collection of items to parse through</param>        
        private void AddLineFlowItems(ToolStripItemCollection Items, Object ComparisonElement)
        {


            Dictionary<int, SortedDictionary<MM_KVLevel, List<MM_Line>>> Flows = new Dictionary<int, SortedDictionary<MM_KVLevel, List<MM_Line>>>(3);
            for (int a = -1; a <= 7; a++)
                Flows.Add(a, new SortedDictionary<MM_KVLevel, List<MM_Line>>());

            //Depending on our comparison element, add lines accordingly
            if (ComparisonElement is MM_Boundary)
            {
                Dictionary<MM_Substation, bool> Subs = new Dictionary<MM_Substation, bool>();
                foreach (MM_Substation Sub in (ComparisonElement as MM_Boundary).Substations)
                    Subs.Add(Sub, true);
                foreach (MM_Line TestLine in MM_Repository.Lines.Values)
                    if (TestLine.Permitted && Subs.ContainsKey(TestLine.Substation1) && TestLine.IsSeriesCompensator)
                        AddLineValue(TestLine, TestLine.Substation1, Flows, 3);
                    else if (TestLine.Permitted && Subs.ContainsKey(TestLine.Substation1) && !Subs.ContainsKey(TestLine.Substation2))
                        AddLineValue(TestLine, TestLine.Substation1, Flows, 0);
                    else if (TestLine.Permitted && Subs.ContainsKey(TestLine.Substation2) && !Subs.ContainsKey(TestLine.Substation1))
                        AddLineValue(TestLine, TestLine.Substation2, Flows, 0);
                    else if (TestLine.Permitted && Subs.ContainsKey(TestLine.Substation1) && Subs.ContainsKey(TestLine.Substation2))
                        AddLineValue(TestLine, TestLine.Substation1, Flows, 2);
                    else if (TestLine.Counties != null && TestLine.Permitted && TestLine.Counties.IndexOf(ComparisonElement as MM_Boundary) != -1)
                        AddLineValue(TestLine, TestLine.Substation1, Flows, 1);

            }
            else if (ComparisonElement is List<MM_Line>)
            {
                foreach (MM_Line TestLine in (ComparisonElement as List<MM_Line>))
                    AddLineValue(TestLine, ComparisonElement as MM_Substation, Flows, TestLine.IsSeriesCompensator ? 3 : 2);

            }
            else if (ComparisonElement is MM_Substation)
            {
                foreach (MM_Line TestLine in MM_Repository.Lines.Values)
                    if (TestLine.Permitted && Array.IndexOf<MM_Substation>(TestLine.ConnectedStations, ComparisonElement as MM_Substation) != -1)
                        AddLineValue(TestLine, ComparisonElement as MM_Substation, Flows, TestLine.IsSeriesCompensator ? 3 : 0);
            }
            else if (ComparisonElement is Dictionary<MM_Line, MM_Substation>)
                foreach (KeyValuePair<MM_Line, MM_Substation> kvp in (Dictionary<MM_Line, MM_Substation>)ComparisonElement)
                    AddLineValue(kvp.Key, kvp.Value, Flows, kvp.Key.IsSeriesCompensator ? 3 : 0);


            //Now, sort everything by ratings
            foreach (SortedDictionary<MM_KVLevel, List<MM_Line>> SDs in Flows.Values)
                foreach (List<MM_Line> Lst in SDs.Values)
                    Lst.Sort(new LineRatingComparer());

            //Now give our summary line information
            //AddMenuItem(Items, "-", true);
            bool DrewLine = false;
            if (MM_Repository.OverallDisplay.GroupLinesByVoltage)
            {
                foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                {
                    DrewLine = false;
                    if (Data_Integration.Permissions.ShowEnergizedLines && Flows[-1].ContainsKey(KVLevel))
                    {
                        if (!DrewLine)
                            DrewLine = (AddMenuItem(Items, "-", true) == null);
                        AddExpandableMenuItem(Items, KVLevel.Name + " Flows in (" + Flows[-1][KVLevel].Count.ToString("#,##0") + "): " + LineSummary(Flows[-1][KVLevel], true), false, ViolationImage(WorstViolation(Flows[-1][KVLevel])), Flows[-1][KVLevel]);
                    }
                    if (Data_Integration.Permissions.ShowEnergizedLines && Flows[1].ContainsKey(KVLevel))
                    {
                        if (!DrewLine)
                            DrewLine = (AddMenuItem(Items, "-", true) == null);
                        AddExpandableMenuItem(Items, KVLevel.Name + " Flows out (" + Flows[1][KVLevel].Count.ToString("#,##0") + "): " + LineSummary(Flows[1][KVLevel], true), false, ViolationImage(WorstViolation(Flows[1][KVLevel])), Flows[1][KVLevel]);
                    }
                    if (Data_Integration.Permissions.ShowDeenergizedLines && Flows[0].ContainsKey(KVLevel))
                    {
                        if (!DrewLine)
                            DrewLine = (AddMenuItem(Items, "-", true) == null);
                        AddExpandableMenuItem(Items, KVLevel.Name + " De-energized (" + Flows[0][KVLevel].Count.ToString("#,##0") + "): " + LineSummary(Flows[0][KVLevel], false), false, ViolationImage(WorstViolation(Flows[0][KVLevel])), Flows[0][KVLevel]);
                    }
                    if (Data_Integration.Permissions.ShowDeenergizedLines && Flows[2].ContainsKey(KVLevel))
                    {
                        if (!DrewLine)
                            DrewLine = (AddMenuItem(Items, "-", true) == null);
                        AddExpandableMenuItem(Items, KVLevel.Name + " Unknown (" + Flows[2][KVLevel].Count.ToString("#,##0") + "): " + LineSummary(Flows[2][KVLevel], false), false, ViolationImage(WorstViolation(Flows[2][KVLevel])), Flows[2][KVLevel]);
                    }
                    if (Data_Integration.Permissions.ShowEnergizedLines && Flows[3].ContainsKey(KVLevel))
                    {
                        if (!DrewLine)
                            DrewLine = (AddMenuItem(Items, "-", true) == null);
                        AddExpandableMenuItem(Items, KVLevel.Name + " De-energized lines through (" + Flows[3][KVLevel].Count.ToString("#,##0") + "): " + LineSummary(Flows[3][KVLevel], true), false, ViolationImage(WorstViolation(Flows[3][KVLevel])), Flows[3][KVLevel]);
                    }
                    if (Data_Integration.Permissions.ShowEnergizedLines && Flows[4].ContainsKey(KVLevel))
                    {
                        if (!DrewLine)
                            DrewLine = (AddMenuItem(Items, "-", true) == null);
                        AddExpandableMenuItem(Items, KVLevel.Name + " Flows through (" + Flows[4][KVLevel].Count.ToString("#,##0") + "): " + LineSummary(Flows[4][KVLevel], true), false, ViolationImage(WorstViolation(Flows[4][KVLevel])), Flows[4][KVLevel]);
                    }
                    if (Data_Integration.Permissions.ShowEnergizedLines && Flows[5].ContainsKey(KVLevel))
                    {
                        if (!DrewLine)
                            DrewLine = (AddMenuItem(Items, "-", true) == null);
                        AddExpandableMenuItem(Items, KVLevel.Name + " Unknown lines through (" + Flows[5][KVLevel].Count.ToString("#,##0") + "): " + LineSummary(Flows[5][KVLevel], true), false, ViolationImage(WorstViolation(Flows[5][KVLevel])), Flows[5][KVLevel]);
                    }

                }
                DrewLine = false;
                foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                {
                    if (Data_Integration.Permissions.ShowEnergizedLines && Flows[6].ContainsKey(KVLevel))
                    {
                        if (!DrewLine)
                            DrewLine = (AddMenuItem(Items, "-", true) == null);
                        AddExpandableMenuItem(Items, KVLevel.Name + " Flows within (" + Flows[6][KVLevel].Count.ToString("#,##0") + "): " + LineSummary(Flows[6][KVLevel], true), false, ViolationImage(WorstViolation(Flows[6][KVLevel])), Flows[6][KVLevel]);
                    }
                }
                DrewLine = false;
                foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                {
                    if (Data_Integration.Permissions.ShowEnergizedLines && Flows[7].ContainsKey(KVLevel))
                    {
                        if (!DrewLine)
                            DrewLine = (AddMenuItem(Items, "-", true) == null);
                        AddExpandableMenuItem(Items, KVLevel.Name + " Series compensators (" + Flows[7][KVLevel].Count.ToString("#,##0") + "): " + LineSummary(Flows[7][KVLevel], true), false, ViolationImage(WorstViolation(Flows[7][KVLevel])), Flows[7][KVLevel]);
                    }
                }
            }
            else
            {
                if (Data_Integration.Permissions.ShowEnergizedLines)
                {
                    foreach (KeyValuePair<MM_KVLevel, List<MM_Line>> kvp in Flows[-1])
                        AddExpandableMenuItem(Items, kvp.Key.Name + " Flows in (" + kvp.Value.Count.ToString("#,##0") + "): " + LineSummary(kvp.Value, true), false, ViolationImage(WorstViolation(kvp.Value)), kvp.Value);
                    AddMenuItem(Items, "-", true);
                    foreach (KeyValuePair<MM_KVLevel, List<MM_Line>> kvp in Flows[1])
                        AddExpandableMenuItem(Items, kvp.Key.Name + " Flows out (" + kvp.Value.Count.ToString("#,##0") + "): " + LineSummary(kvp.Value, true), false, ViolationImage(WorstViolation(kvp.Value)), kvp.Value);
                    foreach (KeyValuePair<MM_KVLevel, List<MM_Line>> kvp in Flows[4])
                        AddExpandableMenuItem(Items, kvp.Key.Name + " Flows through (" + kvp.Value.Count.ToString("#,##0") + "): " + LineSummary(kvp.Value, true), false, ViolationImage(WorstViolation(kvp.Value)), kvp.Value);

                }

                if (Data_Integration.Permissions.ShowDeenergizedLines)
                {
                    foreach (KeyValuePair<MM_KVLevel, List<MM_Line>> kvp in Flows[0])
                        AddExpandableMenuItem(Items, kvp.Key.Name + " De-energized (" + kvp.Value.Count.ToString("#,##0") + "): " + LineSummary(kvp.Value, false), false, ViolationImage(WorstViolation(kvp.Value)), kvp.Value);
                    foreach (KeyValuePair<MM_KVLevel, List<MM_Line>> kvp in Flows[2])
                        AddExpandableMenuItem(Items, kvp.Key.Name + " Unknown (" + kvp.Value.Count.ToString("#,##0") + "): " + LineSummary(kvp.Value, false), false, ViolationImage(WorstViolation(kvp.Value)), kvp.Value);
                    foreach (KeyValuePair<MM_KVLevel, List<MM_Line>> kvp in Flows[3])
                        AddExpandableMenuItem(Items, kvp.Key.Name + " De-energized lines through (" + kvp.Value.Count.ToString("#,##0") + "): " + LineSummary(kvp.Value, true), false, ViolationImage(WorstViolation(kvp.Value)), kvp.Value);
                    foreach (KeyValuePair<MM_KVLevel, List<MM_Line>> kvp in Flows[5])
                        AddExpandableMenuItem(Items, kvp.Key.Name + " Unknown lines through (" + kvp.Value.Count.ToString("#,##0") + "): " + LineSummary(kvp.Value, false), false, ViolationImage(WorstViolation(kvp.Value)), kvp.Value);
                }
            }
        }

        /// <summary>
        /// Add a line to a list of line flows
        /// </summary>
        /// <param name="TestLine">The line to be added</param>
        /// <param name="ComparisonSubstation">The substation to which the line should be compared</param>
        /// <param name="Flows">The collection of flows</param>
        /// <param name="ThroughWithinOrSC">Whether the line is flagged as going through or within our region, or a SC</param>
        private void AddLineValue(MM_Line TestLine, MM_Substation ComparisonSubstation, Dictionary<int, SortedDictionary<MM_KVLevel, List<MM_Line>>> Flows, int ThroughWithinOrSC)
        {
            //First, determine the flow of the test line.
            MM_Substation FlowDir = TestLine.MVAFlowDirection;

            int FlowDirection;
            if (FlowDir == null)
                FlowDirection = (ThroughWithinOrSC == 1 ? 5 : 2);
            else if (TestLine.MVAFlow <= MM_Repository.OverallDisplay.EnergizationThreshold)
                FlowDirection = ThroughWithinOrSC == 1 ? 3 : 0;
            else if (ThroughWithinOrSC == 1)
                FlowDirection = 4;
            else if (ThroughWithinOrSC == 2)
                FlowDirection = 6;
            else if (ThroughWithinOrSC == 3)
                FlowDirection = 7;
            else if (FlowDir == ComparisonSubstation)
                FlowDirection = -1;
            else
                FlowDirection = 1;

            SortedDictionary<MM_KVLevel, List<MM_Line>> FlowType;
            if (!Flows.TryGetValue(FlowDirection, out FlowType))
                Flows.Add(FlowDirection, FlowType = new SortedDictionary<MM_KVLevel, List<MM_Line>>());


            List<MM_Line> Lines;
            if (!FlowType.TryGetValue(TestLine.KVLevel, out Lines))
                FlowType.Add(TestLine.KVLevel, Lines = new List<MM_Line>());

            Lines.Add(TestLine);
        }

        /// <summary>
        /// Determine the worst violation from a series of elements
        /// </summary>
        /// <param name="Elements"></param>
        /// <returns></returns>
        private MM_AlarmViolation_Type WorstViolation(System.Collections.IEnumerable Elements)
        {
            MM_AlarmViolation_Type WorstViol = null;
            if (Elements is Dictionary<MM_Substation, List<MM_Bus>>)
                foreach (List<MM_Bus> Elem in (Elements as Dictionary<MM_Substation, List<MM_Bus>>).Values)
                    foreach (MM_Bus ElemToTest in Elem)
                        WorstViol = ElemToTest.WorstViolation(WorstViol);
            else
                foreach (MM_Element ElemToTest in Elements)
                    WorstViol = ElemToTest.WorstViolation(WorstViol);
            return WorstViol;
        }

        /// <summary>
        /// Retrieve the appropriate image for a violation type
        /// </summary>
        /// <param name="ViolationType"></param>
        /// <returns></returns>
        private Image ViolationImage(MM_AlarmViolation_Type ViolationType)
        {
            if (ViolationType == null)
                return null;
            else
                return MM_Repository.ViolationImages.Images[ViolationType.Name];
        }

        /// <summary>
        /// Add an line's details to a menu
        /// </summary>
        /// <param name="BaseElement">The element for which the items should be added</param>
        /// <param name="Items">The collection of menu items</param>
        private void AddLineItemDetails(MM_Line BaseElement, ToolStripItemCollection Items)
        {
           
            if (BaseElement is MM_Tie)
                AddMenuItem(Items, BaseElement.ElemType + " " + (BaseElement as MM_Tie).TieDescriptor, true);
            else
                AddMenuItem(Items, BaseElement.ElemType + " " + BaseElement.Name, true);

            if (BaseElement.Substation1 != null && BaseElement.Substation2 != null && BaseElement.Substation1.LongName != null && BaseElement.Substation2.LongName != null)
                AddMenuItem(Items, (BaseElement.Substation1 == BaseElement.MVAFlowDirection ? (BaseElement.Substation2.LongName + " - " + BaseElement.Substation1.LongName) : (BaseElement.Substation1.LongName + " - " + BaseElement.Substation2.LongName)) + " " + BaseElement.KVLevel, true);

            if (Data_Integration.Permissions.ShowCounties)
                if (BaseElement.ConnectedStations[0].County == BaseElement.ConnectedStations[1].County)
                {
                    if (BaseElement.ConnectedStations[0].County != null)
                        AddExpandableMenuItem(Items, "  Area: " + BaseElement.ConnectedStations[0].County.Name, true, ViolationImage(BaseElement.ConnectedStations[0].County.WorstViolationOverall), BaseElement.ConnectedStations[0].County);
                    AddExpandableMenuItem(Items, "  Between " + BaseElement.ConnectedStations[0].DisplayName(), true, ViolationImage(BaseElement.ConnectedStations[0].WorstViolationOverall), BaseElement.ConnectedStations[0]);
                    AddExpandableMenuItem(Items, "  And     " + BaseElement.ConnectedStations[1].DisplayName(), true, ViolationImage(BaseElement.ConnectedStations[1].WorstViolationOverall), BaseElement.ConnectedStations[1]);
                }
                else
                {
                    if (BaseElement.ConnectedStations[0].County != null)
                        AddExpandableMenuItem(Items, "  Between " + BaseElement.ConnectedStations[0].DisplayName() + " (" + BaseElement.ConnectedStations[0].County.Name + " area)", true, ViolationImage(BaseElement.ConnectedStations[0].WorstViolationOverall), BaseElement.ConnectedStations[0]);
                    else
                        AddExpandableMenuItem(Items, "  Between " + BaseElement.ConnectedStations[0].DisplayName(), true, ViolationImage(BaseElement.ConnectedStations[0].WorstViolationOverall), BaseElement.ConnectedStations[0]);
                    if (BaseElement.ConnectedStations[1].County != null)
                        AddExpandableMenuItem(Items, "  And     " + BaseElement.ConnectedStations[1].DisplayName() + " (" + BaseElement.ConnectedStations[1].County.Name + " area)", true, ViolationImage(BaseElement.ConnectedStations[1].WorstViolationOverall), BaseElement.ConnectedStations[1]);
                    else
                        AddExpandableMenuItem(Items, "  And     " + BaseElement.ConnectedStations[1].DisplayName(), true, ViolationImage(BaseElement.ConnectedStations[1].WorstViolationOverall), BaseElement.ConnectedStations[1]);
                }

            //if (Data_Integration.Permissions.ShowContingencies)
            //  AddExpandableMenuItem(Items, "  " + (BaseElement.Contingencies.Length == 1 ? "Contingency (1)" : "Contingencies (" + BaseElement.Contingencies.Length.ToString() + ")"), false, ViolationImage(WorstViolation(BaseElement.Contingencies));//m, BaseElement.Contingencies);

            if (Data_Integration.Permissions.ShowDistance)
                AddMenuItem(Items, "  Est. Distance: " + BaseElement.Length.ToString("#,##0") + " miles", true);

            if (Data_Integration.Permissions.ShowKVLevels)
                AddMenuItem(Items, "  Voltage: " + BaseElement.KVLevel.Name, true);
            MM_Bus FoundBus;
            if (Data_Integration.Permissions.ShowBuses && (FoundBus = BaseElement.NearBus) != null)
                AddExpandableMenuItem(Items, "  Bus: " + FoundBus.BusNumber + " (" + FoundBus.Estimated_kV.ToString("#,##0.0") + " kV / " + FoundBus.Estimated_Angle.ToString("0.0") + "°)", false, null, FoundBus);
            if (Data_Integration.Permissions.ShowBuses && (FoundBus = BaseElement.FarBus) != null && BaseElement.FarBusNumber != BaseElement.NearBusNumber)
               AddExpandableMenuItem(Items, "  Far Bus: " + BaseElement.FarBus.BusNumber + " (" + BaseElement.FarBus.Estimated_kV.ToString("#,##0.0") + " kV / " + FoundBus.Estimated_Angle.ToString("0.0") + "°)", false, null, BaseElement.FarBus);
            
            if (Data_Integration.Permissions.ShowBreakerToBreakers && BaseElement.Contingencies != null)
                if (BaseElement.Contingencies.Count == 1)
                    AddExpandableMenuItem(Items, "  Trace: " + BaseElement.Contingencies[0].Name + " / " + BaseElement.Contingencies[0].Description, false, null, BaseElement.Contingencies[0]);
                else if (BaseElement.Contingencies.Count > 1)
                    AddExpandableMenuItem(Items, "  Traces: (" + BaseElement.Contingencies.Count.ToString() + ")", false, null, BaseElement.Contingencies);
            if (Data_Integration.Permissions.ShowRASs && BaseElement.RASs != null)
                if (BaseElement.RASs.Length == 1)
                    AddExpandableMenuItem(Items, "  RAS: " + BaseElement.RASs[0].ToString(), false, null, BaseElement.RASs[0]);
                else
                    AddExpandableMenuItem(Items, "  RAS: (" + BaseElement.RASs.Length.ToString() + ")", false, null, BaseElement.RASs);
            if (Data_Integration.Permissions.ShowTEIDs && BaseElement.TEID != 0)
                AddMenuItem(Items, "  TEID: " + BaseElement.TEID.ToString("#,##0"), false);

            AddMenuItem(Items, "-", true);
            if (Data_Integration.Permissions.ShowPercentageLoading)
                if (BaseElement is MM_Tie && (BaseElement as MM_Tie).MW_Integrated > 0f)
                    AddMenuItem(Items, "Flow: " + (BaseElement as MM_Tie).MW_Integrated.ToString("#,##0.0") + " MW Export (" + ((BaseElement as MM_Tie).MW_Integrated / (BaseElement as MM_Tie).Limits[1]).ToString("0.0%") + ")", true);
                else if (BaseElement is MM_Tie && (BaseElement as MM_Tie).MW_Integrated < 0f)
                    AddMenuItem(Items, "Flow: " + (-(BaseElement as MM_Tie).MW_Integrated).ToString("#,##0.0") + " MW Import (" + (-(BaseElement as MM_Tie).MW_Integrated / (BaseElement as MM_Tie).Limits[0]).ToString("0.0%") + ")", true);
                else if (BaseElement is MM_Tie)
                    AddMenuItem(Items, "Flow: 0 MW", true);
                else if (BaseElement.MVAFlowDirection != null)
                    AddMenuItem(Items, "Flow: " + BaseElement.LinePercentageText + " towards " + BaseElement.MVAFlowDirection.DisplayName(), true);

            //Add in our subcomponent for our unit.
            if (BaseElement is MM_Tie && (BaseElement as MM_Tie).Unit != null && (BaseElement as MM_Tie).IsDC)
            {
                MM_Unit DCUnit = (BaseElement as MM_Tie).Unit;
                AddExpandableMenuItem(Items, DCUnit.UnitType.Name + " " + DCUnit.Name + " (" + DCUnit.Estimated_MW.ToString("#,##0.0") + " MW)", false, null, DCUnit);
            }

            if (BaseElement is MM_Tie && (BaseElement as MM_Tie).Load != null && (BaseElement as MM_Tie).IsDC)
            {
                MM_Load DCLoad = (BaseElement as MM_Tie).Load;
                AddExpandableMenuItem(Items, DCLoad.ElemType.Name + " " + DCLoad.Name + " (" + DCLoad.Estimated_MW.ToString("#,##0.0") + " MW)", false, null, DCLoad);
            }
       
            //This is associated with the MM_Data_Source - basically identifies whether a source is all estimates (STNET), telemetry or both (RT)

            //Initialize our new menu system, and add in all of our values
            MM_Popup_Menu_ValueItems ValueItems = new MM_Popup_Menu_ValueItems(this);
            ValueItems.ShowEstimates = (Data_Integration.Permissions.ShowEstMVAs || Data_Integration.Permissions.ShowEstMWs || Data_Integration.Permissions.ShowEstMVARs) && Data_Integration.NetworkSource.Estimates;

            ValueItems.ShowTelemetry = (Data_Integration.Permissions.ShowTelMVAs || Data_Integration.Permissions.ShowTelMWs || Data_Integration.Permissions.ShowTelMVARs) && Data_Integration.NetworkSource.Telemetry;
            for (int a = 0; a < 2; a++)
            {
                String SubName = BaseElement.IsSeriesCompensator ? "(" + a.ToString() + ")" : BaseElement.ConnectedStations[a].DisplayName();
                ValueItems.AddValue("MVA", SubName, true, BaseElement.Estimated_MVA[a].ToString("#,##0.0"));
                ValueItems.AddValue("MW", SubName, true, BaseElement.Estimated_MW[a].ToString("#,##0.0"));
                if (!float.IsNaN(BaseElement.Estimated_MVAR[a]) && BaseElement.Estimated_MVAR[a] != 0)
                    ValueItems.AddValue("MVAR", SubName, true, BaseElement.Estimated_MVAR[a].ToString("#,##0.0"));
                if (!float.IsNaN(BaseElement.Telemetered_MVA[a]) && BaseElement.Telemetered_MVA[a] != 0)
                    ValueItems.AddValue("MVA", SubName, false, BaseElement.Telemetered_MVA[a].ToString("#,##0.0"));
                ValueItems.AddValue("MW", SubName, false, BaseElement.Telemetered_MW[a].ToString("#,##0.0"));
                if (!float.IsNaN(BaseElement.Telemetered_MVAR[a]) && BaseElement.Telemetered_MVAR[a] != 0)
                    ValueItems.AddValue("MVAR", SubName, false, BaseElement.Telemetered_MVAR[a].ToString("#,##0.0"));
            }
            ValueItems.AddMenuItems(Items);

            if (Data_Integration.Permissions.ShowLimits)
            {
                AddMenuItem(Items, "-", false);
                if (BaseElement is MM_Tie)
                    AddMenuItem(Items, "Limits: " + (BaseElement as MM_Tie).Limits[0].ToString("#,##0.0") + " in, " + (BaseElement as MM_Tie).Limits[1].ToString("#,##0.0") + " out.", false);
                else if (!float.IsNaN(BaseElement.Limits[0]))
                    AddMenuItem(Items, "Limits:  " + BaseElement.Limits[0].ToString("#,##0.0") + "   " + BaseElement.Limits[1].ToString("#,##0.0") + "    " + BaseElement.Limits[2].ToString("#,##0.0"), true);
            }
        }


        /// <summary>
        /// Build information containing one or two arrays
        /// </summary>
        /// <param name="Array1">The first array</param>
        /// <param name="Array2">The second array</param>
        /// <param name="Array1Allowed">Whehter the first array is allowed</param>
        /// <param name="Array2Allowed">Whether the second array is allowed</param>
        /// <param name="Array1First">Whether the first array is first</param>
        /// <returns></returns>
        private string BuildInformation(float[] Array1, float[] Array2, bool Array1Allowed, bool Array2Allowed, bool Array1First)
        {
            StringBuilder OutString = new StringBuilder();
            float[] First = Array1First ? (Array1Allowed ? Array1 : new float[0]) : (Array2Allowed ? Array2 : new float[0]);
            float[] Second = Array1First ? (Array2Allowed ? Array2 : new float[0]) : (Array1Allowed ? Array1 : new float[0]);

            for (int a = 0; a < First.Length; a++)
                OutString.Append((a > 0 ? "    " : "") + First[a].ToString("#,##0.0"));
            if (First.Length > 0 && Second.Length > 0)
                OutString.Append("    ");
            for (int a = 0; a < Second.Length; a++)
                OutString.Append((a > 0 ? "    " : "") + Second[a].ToString("#,##0.0"));
            return OutString.ToString();

        }
        public const string MILITARY_TIME_FORMAT = "MM/dd/yyyy HH:mm";
        /// <summary>
        /// Build information containing one or two arrays
        /// </summary>
        /// <param name="Val1">The first array</param>
        /// <param name="Val2">The second array</param>
        /// <param name="Val1Allowed">Whehter the first array is allowed</param>
        /// <param name="Val2Allowed">Whether the second array is allowed</param>
        /// <param name="Val1First">Whether the first array is first</param>
        /// <returns></returns>
        private string BuildInformation(float Val1, float Val2, bool Val1Allowed, bool Val2Allowed, bool Val1First)
        {
            StringBuilder OutString = new StringBuilder();
            float First = Val1First ? (Val1Allowed ? Val1 : 0f) : (Val2Allowed ? Val2 : 0f);
            float Second = Val1First ? (Val2Allowed ? Val2 : 0f) : (Val1Allowed ? Val1 : 0f);
            OutString.Append(First.ToString("#,##0.0"));
            if (Val1Allowed && Val2Allowed)
                OutString.Append("    " + Second.ToString("#,##0.0"));
            return OutString.ToString();
        }

        /// <summary>
        /// Add an item's details to a menu
        /// </summary>
        /// <param name="BaseElement">The element for which the items should be added</param>
        /// <param name="Items">The collection of menu items</param>
        private void AddItemDetails(MM_Element BaseElement, ToolStripItemCollection Items)
        {

            try
            {

                if (BaseElement != null && BaseElement.Contingencies != null && BaseElement.Contingencies.Count > 0)
                {
                    BaseElement.Contingencies = BaseElement.Contingencies.OrderBy(c => c.Name).ToList();
                    for (int i = 1; i < BaseElement.Contingencies.Count; i++)
                        if (BaseElement.Contingencies[i].Name.Trim().Equals(BaseElement.Contingencies[i-1].Name.Trim(), StringComparison.OrdinalIgnoreCase))
                            BaseElement.Contingencies.RemoveAt(i--);
                }
            }
            catch (Exception)
            {

            }

            //Based on the element type, add in the appropriate information
            if (BaseElement is MM_AlarmViolation)
            {
                MM_AlarmViolation BaseViolation = BaseElement as MM_AlarmViolation;
                AddMenuItem(Items, BaseViolation.EventTime.ToString(MILITARY_TIME_FORMAT) + " age: " + BaseViolation.Recency, true);
                if (BaseViolation.KVLevel != null && Data_Integration.Permissions.ShowKVLevels)
                    AddMenuItem(Items, BaseViolation.Type.Name + " - " + BaseViolation.ViolatedElement.ElemType.Name + " - " + BaseViolation.KVLevel.Name, false, null, BaseViolation);
                else if (Data_Integration.Permissions.ShowKVLevels)
                    AddMenuItem(Items, BaseViolation.Type.Name + " - " + BaseViolation.ViolatedElement.ElemType.Name, false, null, BaseViolation);

                if (BaseViolation.ReportedStart.ToOADate() > 0)
                    AddMenuItem(Items, "Reported start: " + BaseViolation.ReportedStart.ToString(MILITARY_TIME_FORMAT), true);
                if (BaseViolation.ReportedEnd.ToOADate() > 0)
                {
                    if (BaseViolation.ReportedStart.ToOADate() == 0)
                        AddMenuItem(Items, "Reported end: " + BaseViolation.ReportedEnd.ToString(MILITARY_TIME_FORMAT), true);
                    else
                    {
                        TimeSpan Diff = BaseViolation.ReportedEnd - BaseViolation.ReportedStart;
                        if (Diff.TotalMinutes < 1)
                            AddMenuItem(Items, "Reported end: " + BaseViolation.ReportedEnd.ToString(MILITARY_TIME_FORMAT) + " (" + Diff.TotalSeconds.ToString("#0") + " sec.)", true);
                        else if (Diff.TotalMinutes < 60)
                            AddMenuItem(Items, "Reported end: " + BaseViolation.ReportedEnd.ToString(MILITARY_TIME_FORMAT) + " (" + Diff.TotalMinutes.ToString("#0") + " min.)", true);
                        else if (Diff.TotalHours < 24)
                            AddMenuItem(Items, "Reported end: " + BaseViolation.ReportedEnd.ToString(MILITARY_TIME_FORMAT) + " (" + Diff.TotalHours.ToString("#0") + " hrs.)", true);
                        else
                            AddMenuItem(Items, "Reported end: " + BaseViolation.ReportedEnd.ToString(MILITARY_TIME_FORMAT) + " (" + Diff.TotalDays.ToString("##0.#") + " days)", true);
                    }
                }
                AddExpandableMenuItem(Items, BaseViolation.ViolatedElement.ToString() + " " + BaseViolation.Text, false, MM_Repository.ViolationImages.Images[BaseViolation.Type.ViolationIndex], BaseViolation.ViolatedElement);
                if (BaseViolation.Contingency != null)
                    AddExpandableMenuItem(Items, "Contingency " + BaseViolation.Contingency.Name + " / " + BaseViolation.Contingency.Description, false, null, BaseViolation.Contingency);
                if (!float.IsNaN(BaseViolation.PreCtgValue))
                    AddMenuItem(Items, "Pre-Ctg: " + BaseViolation.PreCtgValue.ToString(BaseViolation.Type.Name.EndsWith("Voltage") ? "0.0% pU" : "#,##0 MVA"), true);
                if (!float.IsNaN(BaseViolation.PostCtgValue))
                    AddMenuItem(Items, "Post-Ctg: " + BaseViolation.PostCtgValue.ToString(BaseViolation.Type.Name.EndsWith("Voltage") ? "0.0% pU" : "#,##0 MVA"), true);

            }
            else if (BaseElement is MM_Contingency)
            {
                MM_Contingency Ctg = BaseElement as MM_Contingency;
                AddMenuItem(Items, Ctg.ToString(), true);
                //AddMenuItem(Items, "Active: " + Ctg.Active.ToString(), true);
                AddExpandableMenuItem(Items, "Voltages: " + Ctg.KVLevels.Length.ToString(), true, null, Ctg.KVLevels);
                AddExpandableMenuItem(Items, "Counties: " + Ctg.Counties.Length.ToString(), true, ViolationImage(WorstViolation(Ctg.Counties)), Ctg.Counties);

                // dedup substation list
                var subs = Ctg.Substations.OrderBy(s => s.Name).ToList();
                for (int i=1; i < subs.Count; i++)
                    if (subs[i].Name == subs[i-1].Name)
                        subs.RemoveAt(i--);

                Ctg.Substations = subs.ToArray();

                AddExpandableMenuItem(Items, "Substations: " + Ctg.Substations.Length.ToString(), true, ViolationImage(WorstViolation(Ctg.Substations)), Ctg.Substations);

                AddMenuItem(Items, "-", true);
                SortedDictionary<String, List<MM_Element>> ElemsInCtg = new SortedDictionary<string, List<MM_Element>>();
                List<MM_Element> FoundElems;

                foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                    if (Elem.Contingencies != null && Elem.Contingencies.IndexOf(Ctg) != -1)
                    {
                        if (!ElemsInCtg.TryGetValue(Elem.ElemType.Name, out FoundElems))
                            ElemsInCtg.Add(Elem.ElemType.Name, FoundElems = new List<MM_Element>());
                        FoundElems.Add(Elem);
                        // dedup
                        FoundElems = FoundElems.OrderBy(fe => fe.Name).ToList();
                        for (int i = 1;i < FoundElems.Count;i++)
                            if (FoundElems[i].Name.Trim().Equals(FoundElems[i-1].Name.Trim(), StringComparison.OrdinalIgnoreCase))
                                FoundElems.RemoveAt(i--);
                    }

                foreach (KeyValuePair<String, List<MM_Element>> kvp in ElemsInCtg)
                    AddExpandableMenuItem(Items, kvp.Key + "s: " + kvp.Value.Count.ToString(), true, ViolationImage(WorstViolation(kvp.Value)), kvp.Value.ToArray());
                if (Data_Integration.Permissions.OTS == true)
                    AddMenuItem(Items, "Display b2b one-line: " + Ctg.Name + " / " + Ctg.Description, false, MM_Repository.ViolationImages.Images["OneLine"], Ctg);
            }
            else if (BaseElement is MM_Boundary)
                AddBoundaryItemDetails(BaseElement as MM_Boundary, Items);
            else if (BaseElement is MM_Substation)
                AddSubstationItemDetails(BaseElement as MM_Substation, Items);
            else if (BaseElement is MM_Line)
                AddLineItemDetails(BaseElement as MM_Line, Items);            
            else
            {
                MM_Unit BaseUnit = null;
                if (BaseElement.ElemType != null && BaseElement.ElemType.Name == "Unit" && Data_Integration.Permissions.ShowUnitGenerationType)
                {
                    AddMenuItem(Items, (BaseElement as MM_Unit).UnitType.Name + " " + BaseElement.Name + (Data_Integration.Permissions.ShowKVLevels ? " (" + BaseElement.KVLevel.Name + ")" : ""), true, ViolationImage(BaseElement.WorstViolationOverall), BaseElement);

                    BaseUnit = BaseElement as MM_Unit;
                    if (BaseUnit.MarketResourceName != null)
                        AddMenuItem(Items, BaseUnit.MarketResourceName, false);
                   // if (BaseUnit.GenmomName != null)
                    //    AddMenuItem(Items, BaseUnit.GenmomName.Replace("TOPOLOGY.", ""), false);
                }
                else if (BaseElement is MM_Transformer)
                    AddMenuItem(Items, BaseElement.ElemType.Name + " " + BaseElement.Name.Replace("TOPOLOGY.", "") + " " + (Data_Integration.Permissions.ShowKVLevels ? " (" + (BaseElement as MM_Transformer).KVLevel1 + " / " + (BaseElement as MM_Transformer).KVLevel2 + ")" : ""), true, ViolationImage(BaseElement.WorstViolationOverall), BaseElement);
                else if (BaseElement.KVLevel != null && BaseElement.Name != null)
                    AddMenuItem(Items, BaseElement.ElemType.Name + " " + BaseElement.Name.Replace("TOPOLOGY.", "") + " " + (Data_Integration.Permissions.ShowKVLevels ? " (" + BaseElement.KVLevel.Name + ")" : ""), true, ViolationImage(BaseElement.WorstViolationOverall), BaseElement);
                else
                    AddMenuItem(Items, BaseElement.ToString(), true, ViolationImage(BaseElement.WorstViolationOverall), BaseElement);

                if (BaseElement.Substation != null)
                {
                    AddExpandableMenuItem(Items, "  in substation: " + (string.IsNullOrWhiteSpace(BaseElement.Substation.LongName) || BaseElement.Substation.LongName == BaseElement.Substation.Name ? BaseElement.Substation.Name : string.Format("{0} ({1})", BaseElement.Substation.LongName, BaseElement.Substation.Name)), true, ViolationImage(BaseElement.Substation.WorstViolationOverall), BaseElement.Substation);
                    if (BaseElement.Substation.County != null && Data_Integration.Permissions.ShowCounties)
                        AddExpandableMenuItem(Items, "  in area: " + BaseElement.Substation.County.Name, true, ViolationImage(BaseElement.Substation.County.WorstViolationOverall), BaseElement.Substation.County);
                }
                if (Data_Integration.Permissions.ShowBreakerToBreakers && BaseElement.Contingencies != null)
                    if (BaseElement.Contingencies.Count == 1)
                        AddExpandableMenuItem(Items, "  Trace: " + BaseElement.Contingencies[0].Name + " / " + BaseElement.Contingencies[0].Description, false, null, BaseElement.Contingencies[0]);
                    else if (BaseElement.Contingencies.Count > 1)
                        AddExpandableMenuItem(Items, "  Traces: (" + BaseElement.Contingencies.Count.ToString() + ")", false, null, BaseElement.Contingencies);
                if (Data_Integration.Permissions.ShowRASs && BaseElement.RASs != null)
                    if (BaseElement.Contingencies.Count == 1)
                        AddExpandableMenuItem(Items, "  RAS: " + BaseElement.RASs[0].ToString(), false, null, BaseElement.RASs[0]);
                    else
                        AddExpandableMenuItem(Items, "  RAS: (" + BaseElement.RASs.Length.ToString() + ")", false, null, BaseElement.RASs);

                if (BaseElement is MM_RemedialActionScheme)
                {
                    MM_RemedialActionScheme RAS = (MM_RemedialActionScheme)BaseElement;
                    SortedDictionary<String, List<MM_Element>> ElemsInCtg = new SortedDictionary<string, List<MM_Element>>();
                    List<MM_Element> FoundElems;

                    if (RAS.RASElements != null)
                        foreach (MM_Element Elem in RAS.RASElements)
                        {
                            if (!ElemsInCtg.TryGetValue(Elem.ElemType.Name, out FoundElems))
                                ElemsInCtg.Add(Elem.ElemType.Name, FoundElems = new List<MM_Element>());
                            FoundElems.Add(Elem);
                        }

                    foreach (KeyValuePair<String, List<MM_Element>> kvp in ElemsInCtg)
                        AddExpandableMenuItem(Items, "  " + kvp.Key + (kvp.Value.Count == 1 ? ": " : "s: " + kvp.Value.Count.ToString()), true, ViolationImage(WorstViolation(kvp.Value)), kvp.Value.ToArray());

                }

                MM_Bus FoundBus;
                if (Data_Integration.Permissions.ShowBuses && (FoundBus = BaseElement.NearBus) != null)
                    AddExpandableMenuItem(Items, "  Bus: " + FoundBus.BusNumber + " (" + FoundBus.Estimated_kV.ToString("#,##0.0") + " kV / " + FoundBus.Estimated_Angle.ToString("0.0") + "°)", false, ViolationImage(BaseElement.NearBus.WorstViolationOverall), FoundBus);
                if (Data_Integration.Permissions.ShowBuses && (FoundBus = BaseElement.FarBus) != null && BaseElement.FarBusNumber != BaseElement.NearBusNumber)
                    AddExpandableMenuItem(Items, "  Far Bus: " + BaseElement.FarBus.BusNumber + " (" + BaseElement.FarBus.Estimated_kV.ToString("#,##0.0") + " kV / " + FoundBus.Estimated_Angle.ToString("0.0") + "°)", false, ViolationImage(BaseElement.FarBus.WorstViolationOverall), BaseElement.FarBus);
                
                

               // if (Data_Integration.Permissions.ShowTEIDs && BaseElement.TEID != 0)
                //    AddMenuItem(Items, "  TEID: " + BaseElement.TEID.ToString("#,##0"), false);

                if (BaseElement is MM_Transformer)
                    AddTransformerItemDetails(BaseElement as MM_Transformer, Items);
                else if (BaseElement is MM_TransformerWinding)
                    AddTransformerWindingItemDetails(BaseElement as MM_TransformerWinding, Items);
                else if (BaseElement is MM_Load)
                {
                    MM_Load BaseLoad = BaseElement as MM_Load;
                    AddMenuItem(Items, "-", false);
                    if (!float.IsNaN(BaseLoad.Lmp) && BaseLoad.Lmp != 0)
                        AddMenuItem(Items, string.Format("LMP: {0:$##0.##}", BaseLoad.Lmp), true);

                    //Initialize our new menu system, and add in all of our values
                    MM_Popup_Menu_ValueItems ValueItems = new MM_Popup_Menu_ValueItems(this);
                    
                    ValueItems.ShowEstimates = (Data_Integration.Permissions.ShowEstMVAs || Data_Integration.Permissions.ShowEstMWs || Data_Integration.Permissions.ShowEstMVARs) && Data_Integration.NetworkSource.Estimates;
                    ValueItems.ShowTelemetry = (Data_Integration.Permissions.ShowTelMVAs || Data_Integration.Permissions.ShowTelMWs || Data_Integration.Permissions.ShowTelMVARs) && Data_Integration.NetworkSource.Telemetry;
                    ValueItems.AddValue("MVA", BaseElement.Substation.DisplayName(), true, BaseLoad.Estimated_MVA.ToString("#,##0.0"));
                    ValueItems.AddValue("MW", BaseElement.Substation.DisplayName(), true, BaseLoad.Estimated_MW.ToString("#,##0.0"));
                    if (!float.IsNaN(BaseLoad.Estimated_MVAR) && BaseLoad.Estimated_MVAR != 0)
                        ValueItems.AddValue("MVAR", BaseElement.Substation.DisplayName(), true, BaseLoad.Estimated_MVAR.ToString("#,##0.0"));
                    ValueItems.AddValue("MVA", BaseElement.Substation.DisplayName(), false, BaseLoad.Telemetered_MVA.ToString("#,##0.0"));
                    ValueItems.AddValue("MW", BaseElement.Substation.DisplayName(), false, BaseLoad.Telemetered_MW.ToString("#,##0.0"));
                    ValueItems.AddValue("MVAR", BaseElement.Substation.DisplayName(), false, BaseLoad.Telemetered_MVAR.ToString("#,##0.0"));
                    
                    ValueItems.AddMenuItems(Items);

                    /*AddMenuItem(Items, "        Est.    Telem.", true);

                    AddMenuItem(Items, "MVA: " + (BaseElement as MM_Load).Estimated_MVA.ToString("#,##0.0") + "  " + (BaseElement as MM_Load).Telemetered_MVA.ToString("#,##0.0"), true);
                    AddMenuItem(Items, "MW: " + (BaseElement as MM_Load).Estimated_MW.ToString("#,##0.0") + "  " + (BaseElement as MM_Load).Telemetered_MW.ToString("#,##0.0"), true);
                    AddMenuItem(Items, "MVAR: " + (BaseElement as MM_Load).Estimated_MVAR.ToString("#,##0.0") + "  " + (BaseElement as MM_Load).Telemetered_MVAR.ToString("#,##0.0"), true);
                
                     */
                }
                else if (BaseElement is MM_Island)
                {
                    MM_Island BaseIsland = BaseElement as MM_Island;
                    AddMenuItem(Items, "Frequency: " + BaseIsland.Frequency.ToString("#,##0.000") + " Hz", true);
                    AddMenuItem(Items, "Frequency Control: " + BaseIsland.FrequencyControl.ToString(), true);
                    AddMenuItem(Items, "-", true);

                    AddMenuItem(Items, "Available generation: " + BaseIsland.AvailableGenCapacity.ToString("#,##0") + " MW", true);
                    MM_Element[] Units = BaseIsland.Units;
                    AddExpandableMenuItem(Items, "Units: " + BaseIsland.Generation.ToString("#,##0") + " MW / " + Units.Length.ToString("#,##0"), false, null, Units);

                    MM_Element[] Loads = BaseIsland.Loads;
                    AddExpandableMenuItem(Items, "Loads: " + BaseIsland.Load.ToString("#,##0") + "MW / " + BaseIsland.Loads.Length.ToString("#,##0"), false, null, Loads);




                }
                else if (BaseElement is MM_Unit)
                {
                    AddMenuItem(Items, "-", false);
                    if (!string.IsNullOrWhiteSpace(BaseUnit.FriendlyName))
                        AddMenuItem(Items, BaseUnit.FriendlyName, true);
                    if (!string.IsNullOrWhiteSpace(BaseUnit.MarketParticipantName))
                        AddMenuItem(Items, "Market Participant: " + BaseUnit.MarketParticipantName, true);
                    if (!string.IsNullOrWhiteSpace(BaseUnit.MarketResourceType))
                        AddMenuItem(Items, "Market Asset Type: " + BaseUnit.MarketResourceType, true);
                    if (!string.IsNullOrWhiteSpace(BaseUnit.ContactInfo))
                        AddMenuItem(Items, "Contact: " + BaseUnit.ContactInfo, true);
                    if (!string.IsNullOrWhiteSpace(BaseUnit.Description))
                        AddMenuItem(Items, "Description: " + BaseUnit.Description, true);
                    //AddMenuItem(Items, (BaseElement as MM_Unit).UnitPercentageText + " of Capacity", true);
                    AddMenuItem(Items, "-", true);
                    //Initialize our new menu system, and add in all of our values
                    MM_Popup_Menu_ValueItems ValueItems = new MM_Popup_Menu_ValueItems(this);
                    
                    ValueItems.ShowEstimates = (Data_Integration.Permissions.ShowEstMVAs || Data_Integration.Permissions.ShowEstMWs || Data_Integration.Permissions.ShowEstMVARs) && Data_Integration.NetworkSource.Estimates;
                    ValueItems.ShowTelemetry = (Data_Integration.Permissions.ShowTelMVAs || Data_Integration.Permissions.ShowTelMWs || Data_Integration.Permissions.ShowTelMVARs) && Data_Integration.NetworkSource.Telemetry;
                    ValueItems.AddValue("MVA", BaseElement.Substation.DisplayName(), true, BaseUnit.Estimated_MVA.ToString("#,##0.0"));
                    ValueItems.AddValue("MW", BaseElement.Substation.DisplayName(), true, BaseUnit.Estimated_MW.ToString("#,##0.0"));
                    ValueItems.AddValue("MVAR", BaseElement.Substation.DisplayName(), true, BaseUnit.Estimated_MVAR.ToString("#,##0.0"));
                    ValueItems.AddValue("MVA", BaseElement.Substation.DisplayName(), false, BaseUnit.Telemetered_MVA.ToString("#,##0.0"));
                    ValueItems.AddValue("MW", BaseElement.Substation.DisplayName(), false, BaseUnit.Telemetered_MW.ToString("#,##0.0"));
                    ValueItems.AddValue("MVAR", BaseElement.Substation.DisplayName(), false, BaseUnit.Telemetered_MVAR.ToString("#,##0.0"));
                    ValueItems.AddMenuItems(Items);

                    if (Data_Integration.Permissions.ShowLimits)
                    {
                        AddMenuItem(Items, "-", true);
                        AddMenuItem(Items, "Capacity:", true);
                        AddMenuItem(Items,  BaseUnit.MaxCapacity.ToString("#,##0.0") + "MW (" + BaseUnit.UnitPercentageText + ")", true);
                        AddMenuItem(Items,  BaseUnit.UnitMVARCapacity.ToString("#,##0.0") + " MVAR (" + BaseUnit.UnitReactivePercentage.ToString("0.0%") + ")", true);
                        if ((BaseElement as MM_Unit).MVARCapabilityCurve.Length > 1)
                        {
                            ToolStripMenuItem tmi = new ToolStripMenuItem("");
                            tmi.DisplayStyle = ToolStripItemDisplayStyle.Text;
                            tmi.Paint += tmi_Paint;
                            tmi.AutoSize = false;
                            tmi.Width = 400;
                            tmi.Height = 200;
                            tmi.Tag = BaseUnit;
                            Items.Add(tmi);
                        }
                        AddMenuItem(Items, "-", true);
                        if (BaseUnit.Fuel != null || BaseUnit.UnitType != null)
                            AddMenuItem(Items, (BaseUnit.UnitType != null && BaseUnit.UnitType.Name != null ? BaseUnit.UnitType.Name : BaseUnit.Fuel) + " " +  BaseUnit.RampRateUp.ToString("##0.#") + " MW/min Up", true);
                       AddMenuItem(Items, "HASL: " + BaseUnit.HASL.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "LASL: " + BaseUnit.LASL.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "-", true);
                        AddMenuItem(Items, "HEL: " + BaseUnit.HEL.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "HSL: " + BaseUnit.HSL.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "HDL: " + BaseUnit.HDL.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "SCED BP: " + BaseUnit.SCED_Basepoint.ToString("#,##0.0"), true);
 AddMenuItem(Items, "LDL: " + BaseUnit.LDL.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "LSL: " + BaseUnit.LSL.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "LEL: " + BaseUnit.LEL.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "-", true);
					   
					   
					    if (!float.IsNaN(BaseUnit.RegUp) && BaseUnit.RegUp > 0)
                            AddMenuItem(Items, "Cleared Reg Up: " + BaseUnit.RegUp.ToString("#,##0.0"), true);
                        if (!float.IsNaN(BaseUnit.RegDown) && BaseUnit.RegDown > 0)
                            AddMenuItem(Items, "Cleared Reg Down: " + BaseUnit.RegDown.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "CMode: " + BaseUnit.CMode, true);
                        AddMenuItem(Items, "-", true);
                        AddMenuItem(Items, "RTGEN Max Cap: " + BaseUnit.EmerMax.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "Eco Max: " + BaseUnit.EcoMax.ToString("#,##0.0"), true);
                        if (BaseUnit.RegUp > 0)
                            AddMenuItem(Items, "Reg Up: " + BaseUnit.RegUp.ToString("#,##0.0"), true);
                        if (!float.IsNaN(BaseUnit.SCED_Basepoint) && BaseUnit.SCED_Basepoint > 0)
                            AddMenuItem(Items, "SCED BP: " + BaseUnit.SCED_Basepoint.ToString("#,##0.0"), true);
                        if (BaseUnit.RegDown > 0)
                            AddMenuItem(Items, "Reg Down: " + BaseUnit.RegDown.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "Eco Min: " + BaseUnit.EcoMin.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "Emer Min: " + BaseUnit.EmerMin.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "-", true);
                        if (!float.IsNaN(BaseUnit.LMP) && BaseUnit.LMP > 0)
                            AddMenuItem(Items, "LMP: " + BaseUnit.LMP.ToString("$#,##0.0"), true);
                        if (!float.IsNaN(BaseUnit.SPP) && BaseUnit.SPP > 0)
                            AddMenuItem(Items, "SCED SPP: " + BaseUnit.SPP.ToString("$#,##0.0"), true);
                        AddMenuItem(Items, "-", true);
                        if (!float.IsNaN(BaseUnit.SpinningCapacity) && BaseUnit.SpinningCapacity > 0)
                            AddMenuItem(Items, "Spinning Cap: " + BaseUnit.SpinningCapacity.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "Ramped Set Point: " + BaseUnit.RampedSetPoint.ToString("#,##0.0"), true);
                        if (!float.IsNaN(BaseUnit.RegulationDeployed) && BaseUnit.RegulationDeployed > 0)
                            AddMenuItem(Items, "Regulation Deployed: " + BaseUnit.RegulationDeployed.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "Dispatch MW: " + BaseUnit.DispatchMW.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "Set Point MW: " + BaseUnit.SetPoint.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "RTGEN MW: " + BaseUnit.RTGenMW.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "-", true);
						                        AddMenuItem(Items, "Spinning Cap: " + BaseUnit.SpinningCapacity.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "PRC Online: " + BaseUnit.Physical_Responsive_Online.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "PRC Sync: " + BaseUnit.Physical_Responsive_Sync.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "Blackstart: " + BaseUnit.Blackstart_Capacity.ToString("#,##0.0"), true);
                        AddMenuItem(Items, "RMR: " + BaseUnit.RMR_Capacity.ToString("#,##0.0"), true);
                         AddMenuItem(Items, "-", true);
					 if (!float.IsNaN(BaseUnit.Frequency) && BaseUnit.Frequency > 0)
                            AddMenuItem(Items, "Frequency: " + BaseUnit.Frequency.ToString("0.00") + " Hz", true);

                        MM_Island Island = BaseElement.NearIsland;
                        if (Island != null)
                            AddMenuItem(Items, "Island: " + Island.ToString(), true);
                        AddMenuItem(Items, "Frequency Control: " + (BaseUnit.FrequencyControl ? "Enabled" : "Disabled"), true);

                        if (Island != null)                       
                            AddMenuItem(Items, "Island Frequency Control: " + (Island.FrequencyControl ? "Enabled" : "Disabled"), true);

                    }
                }
                else if (BaseElement is MM_Node && Data_Integration.Permissions.ShowEstVs)
                {
                    AddMenuItem(Items, "-", false);
                    MM_Node BaseNode = BaseElement as MM_Node;
                    if (BaseNode != null && BaseNode.NearBus != null && BaseNode.NearBusNumber != -1)
                    {
                        if (BaseNode.NearBus.Node != null && BaseNode.NearBus.Node.Name != null) 
                            AddExpandableMenuItem(Items, "Bus " + BaseNode.NearBus.Node.Name, false, null, BaseNode.NearBus);
                        AddExpandableMenuItem(Items, "Island: " + BaseNode.NearBus.IslandNumber.ToString(), false, null, BaseNode.NearIsland);
                        MM_Element[] ConnectedElems = BaseNode.ConnectedElements.ToArray();

                        AddExpandableMenuItem(Items, "Connected Elements: " + ConnectedElems.Length.ToString(), false, null, ConnectedElems);
                        AddMenuItem(Items, "kV    : " + BaseNode.NearBus.Estimated_kV.ToString("0.0"), true);
                        AddMenuItem(Items, "pU    : " + BaseNode.NearBus.PerUnitVoltage.ToString("0.00%"), true);
                    }
                    if (BaseNode.FarBus != null && BaseNode.NearBus != BaseNode.FarBus && BaseNode.FarBusNumber != -1)
                    {
                        AddExpandableMenuItem(Items, "Bus " + BaseNode.FarBus.Node.Name, false, null, BaseNode.FarBus);
                        AddExpandableMenuItem(Items, "Island: " + BaseNode.FarBus.IslandNumber.ToString(), false, null, BaseNode.FarIsland);
                        MM_Element[] ConnectedElems = BaseNode.FarBus.ConnectedElements;
                        AddExpandableMenuItem(Items, "Connected Elements: " + ConnectedElems.Length.ToString(), false, null, ConnectedElems);
                        AddMenuItem(Items, "kV    : " + BaseNode.FarBus.Estimated_kV.ToString("0.0"), true);
                        AddMenuItem(Items, "pU    : " + BaseNode.FarBus.PerUnitVoltage.ToString("0.00%"), true);
                    }
                    //AddMenuItem(Items, "Limits: " + BaseNode.Limits[3].ToString("#,##0") + "   " + BaseNode.Limits[1].ToString("#,##0") + "   " + BaseNode.Limits[0].ToString("#,##0") + "   " + BaseNode.Limits[2].ToString("#,##0"), true);
                }
                else if (BaseElement is MM_Bus && Data_Integration.Permissions.ShowEstVs)
                {
                    AddMenuItem(Items, "-", false);
                    MM_Bus BaseBB = BaseElement as MM_Bus;
                    AddExpandableMenuItem(Items, "Island: " + BaseBB.IslandNumber.ToString(), false, null, BaseBB.Island);
                    MM_Element[] ConnectedElems = BaseBB.ConnectedElements;
                    
                    if (ConnectedElems.Length < 1000)
                    {
                        AddExpandableMenuItem(Items, "Connected Elements: " + ConnectedElems.Length.ToString(), false, null, ConnectedElems);
                        AddMenuItem(Items, "kV    : " + BaseBB.Estimated_kV.ToString("0.0"), true);
                        AddMenuItem(Items, "pU    : " + BaseBB.PerUnitVoltage.ToString("0.00%"), true);
                        if (BaseBB.Limits != null && BaseBB.Limits.Length > 3 && !float.IsNaN(BaseBB.Limits[0]))
                            AddMenuItem(Items, "Limits: " + BaseBB.Limits[3].ToString("#,##0") + "   " + BaseBB.Limits[1].ToString("#,##0") + "   " + BaseBB.Limits[0].ToString("#,##0") + "   " + BaseBB.Limits[2].ToString("#,##0"), true);
                    }
                }
                else if (BaseElement is MM_ShuntCompensator)
                {
                    AddMenuItem(Items, "-", false);
                    AddMenuItem(Items, "State: " + ((BaseElement as MM_ShuntCompensator).Open ? "Open" : "Closed"), true);
                    AddMenuItem(Items, "Nominal: " + (BaseElement as MM_ShuntCompensator).Nominal_MVAR.ToString("#,##0.0") + " MVar", true);
                    AddMenuItem(Items, "Estimated: " + (BaseElement as MM_ShuntCompensator).Estimated_MVAR.ToString("#,##0.0") + " MVar", true);
                }
            }

            //Now, check to see whether we have any notes
            if (Data_Integration.Permissions.ShowNotes && MM_Repository.Notes.ContainsKey(BaseElement.TEID))
            {
                AddMenuItem(Items, "-", false);
                MM_Note BaseElementNote = MM_Repository.Notes[BaseElement.TEID];
                ToolStripMenuItem NoteItem = (ToolStripMenuItem)AddMenuItem(Items, BaseElementNote.Author + " @ " + BaseElementNote.CreatedOn, false);
                AddMenuItem(NoteItem.DropDownItems, BaseElementNote.Note, true);
            }
        }

        /// <summary>
        /// When our indicator becomes visible, adjust accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmi_VisibleChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem tmi = (ToolStripMenuItem)sender;
            tmi.AutoSize = false;
            tmi.Height = tmi.Width / 2;
        }

        /// <summary>
        /// Handle the painting of our unit 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmi_Paint(object sender, PaintEventArgs e)
        {
            MM_Unit unit = (MM_Unit)((ToolStripMenuItem)sender).Tag;
            unit.DrawUnitMVARCapabilityCurve(e.Graphics, e.ClipRectangle,false);
			//TODO: Note, this has been moved to MM_Unit, because of the new generator control panel.
        }

        /// <summary>
        /// Add details for a transformer
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <param name="Items"></param>
        private void AddTransformerItemDetails(MM_Transformer BaseElement, ToolStripItemCollection Items)
        {
            {
                AddMenuItem(Items, "-", true);

                if (Data_Integration.Permissions.ShowPercentageLoading)
                    AddMenuItem(Items, "Flow: " + (BaseElement as MM_Transformer).FlowPercentageText + " towards " + (BaseElement as MM_Transformer).MVAFlowDirection.Name, true);
                //todo: Figure out how this is always getting set to false.
             //TODO: It's MM_Data_Source.
                //Initialize our new menu system, and add in all of our values
                MM_Popup_Menu_ValueItems ValueItems = new MM_Popup_Menu_ValueItems(this);
                ValueItems.ShowEstimates = (Data_Integration.Permissions.ShowEstMVAs || Data_Integration.Permissions.ShowEstMWs || Data_Integration.Permissions.ShowEstMVARs) && Data_Integration.NetworkSource.Estimates;
                ValueItems.ShowTelemetry = (Data_Integration.Permissions.ShowTelMVAs || Data_Integration.Permissions.ShowTelMWs || Data_Integration.Permissions.ShowTelMVARs) && Data_Integration.NetworkSource.Telemetry;

                String KV1 = !float.IsNaN(BaseElement.Winding1.Voltage) && BaseElement.Winding1.Voltage != 0 ? BaseElement.Winding1.Voltage + " KV" : BaseElement.KVLevel1.Name;
                String KV2 = !float.IsNaN(BaseElement.Winding2.Voltage) && BaseElement.Winding2.Voltage != 0 ? BaseElement.Winding2.Voltage + " KV" : BaseElement.KVLevel2.Name;

                

                if (KV1 == KV2)
                {
                    KV1 += "(1)";
                    KV2 += "(2)";
                }

                if (BaseElement.Winding1.WindingType == MM_TransformerWinding.enumWindingType.Primary)
                    KV1 = "[" + KV1 + "]";
                else if (BaseElement.Winding2.WindingType == MM_TransformerWinding.enumWindingType.Primary)
                    KV2 = "[" + KV2 + "]";


                ValueItems.AddValue("MVA", KV1, true, BaseElement.Winding1.Estimated_MVA.ToString("#,##0.0"));
                ValueItems.AddValue("MVA", KV2, true, BaseElement.Winding2.Estimated_MVA.ToString("#,##0.0"));
                ValueItems.AddValue("MW", KV1, true, BaseElement.Winding1.Estimated_MW.ToString("#,##0.0"));
                ValueItems.AddValue("MW", KV2, true, BaseElement.Winding2.Estimated_MW.ToString("#,##0.0"));
                ValueItems.AddValue("MVAR", KV1, true, BaseElement.Winding1.Estimated_MVAR.ToString("#,##0.0"));
                ValueItems.AddValue("MVAR", KV2, true, BaseElement.Winding2.Estimated_MVAR.ToString("#,##0.0"));

                ValueItems.AddValue("MVA", KV1, false, BaseElement.Winding1.Telemetered_MVA.ToString("#,##0.0"));
                ValueItems.AddValue("MVA", KV2, false, BaseElement.Winding2.Telemetered_MVA.ToString("#,##0.0"));
                ValueItems.AddValue("MW", KV1, false, BaseElement.Winding1.Telemetered_MW.ToString("#,##0.0"));
                ValueItems.AddValue("MW", KV2, false, BaseElement.Winding2.Telemetered_MW.ToString("#,##0.0"));
                ValueItems.AddValue("MVAR", KV1, false, BaseElement.Winding1.Telemetered_MVAR.ToString("#,##0.0"));
                ValueItems.AddValue("MVAR", KV2, false, BaseElement.Winding2.Telemetered_MVAR.ToString("#,##0.0"));
                ValueItems.AddMenuItems(Items);



                if (Data_Integration.Permissions.ShowLimits)
                {
                    AddMenuItem(Items, "-", false);
                    if (!float.IsNaN(BaseElement.NormalLimit))
                        AddMenuItem(Items, "Limits:  " + BaseElement.NormalLimit.ToString("#,##0.0") + "   " + BaseElement.Limits[1].ToString("#,##0.0") + "    " + BaseElement.Limits[2].ToString("#,##0.0"), true);
                    if (!float.IsNaN(BaseElement.Estimated_Tap[0]) && BaseElement.Estimated_Tap[0] != 0)
                        AddMenuItem(Items, "Tap:  " + BaseElement.Estimated_Tap[0].ToString() + "   " + BaseElement.Estimated_Tap[1].ToString(), true);
                    AddMenuItem(Items, "Voltage:  " + BaseElement.Voltage[0].ToString() + "   " + BaseElement.Voltage[1].ToString(), true);
                }
            }
        }

        /// <summary>
        /// Add details for a transformer winding
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <param name="Items"></param>
        private void AddTransformerWindingItemDetails(MM_TransformerWinding BaseElement, ToolStripItemCollection Items)
        {
            {
                AddMenuItem(Items, "-", true);

                bool ShowEst = (Data_Integration.NetworkSource.Estimates && (Data_Integration.Permissions.ShowEstMVAs || Data_Integration.Permissions.ShowEstMVARs || Data_Integration.Permissions.ShowEstMWs));
                bool ShowTelem = (Data_Integration.NetworkSource.Telemetry && (Data_Integration.Permissions.ShowTelMVAs || Data_Integration.Permissions.ShowTelMVARs || Data_Integration.Permissions.ShowTelMWs));



                if (ShowEst ^ ShowTelem)
                    AddMenuItem(Items, "         " + BaseElement.KVLevel.Name, true);
                else if (ShowEst && ShowTelem)
                {
                    if (Data_Integration.UseEstimates)
                        AddMenuItem(Items, "        Est.    Telem.", true);
                    else
                        AddMenuItem(Items, "        Telem.    Est.", true);

                }

                if ((ShowEst && Data_Integration.Permissions.ShowEstMVAs) || (ShowTelem && Data_Integration.Permissions.ShowTelMVAs))
                    AddMenuItem(Items, "MVA:   " + BuildInformation(BaseElement.Estimated_MVA, BaseElement.Telemetered_MVA, ShowEst && Data_Integration.Permissions.ShowEstMVAs, ShowTelem && Data_Integration.Permissions.ShowTelMVAs, Data_Integration.UseEstimates), true);
                if ((ShowEst && Data_Integration.Permissions.ShowEstMWs) || (ShowTelem && Data_Integration.Permissions.ShowTelMWs))
                    AddMenuItem(Items, "MW:    " + BuildInformation(BaseElement.Estimated_MW, BaseElement.Telemetered_MW, ShowEst && Data_Integration.Permissions.ShowEstMWs, ShowTelem && Data_Integration.Permissions.ShowTelMWs, Data_Integration.UseEstimates), true);
                if ((ShowEst && Data_Integration.Permissions.ShowEstMVARs) || (ShowTelem && Data_Integration.Permissions.ShowTelMVARs))
                    AddMenuItem(Items, "MVAR:  " + BuildInformation(BaseElement.Estimated_MVAR, BaseElement.Telemetered_MVAR, ShowEst && Data_Integration.Permissions.ShowEstMVARs, ShowTelem && Data_Integration.Permissions.ShowTelMVARs, Data_Integration.UseEstimates), true);


                if (Data_Integration.Permissions.ShowLimits)
                {
                    AddMenuItem(Items, "-", false);
                    AddMenuItem(Items, "Tap:  " + BaseElement.Tap.ToString(), true);
                    if (float.IsNaN(BaseElement.Voltage) || BaseElement.Voltage == 0)
                        AddMenuItem(Items, "Voltage:  " + BaseElement.KVLevel.Nominal.ToString(), true);
                    else
                    AddMenuItem(Items, "Voltage:  " + BaseElement.Voltage.ToString(), true);
                }
            }
        }


        /// <summary>
        /// Produce a string description about a group of lines
        /// </summary>
        /// <param name="Lines"></param>
        /// <param name="IncludePercentage"></param>
        /// <returns></returns>
        private string LineSummary(List<MM_Line> Lines, bool IncludePercentage)
        {
            float TotalFlow = 0f, MaxFlow = 0f;
            foreach (MM_Line Line in Lines)
            {
                TotalFlow += Line.MVAFlow;
                if (Line.MVAFlow > Line.NormalLimit)
                    MaxFlow += Line.MVAFlow;
                else
                    MaxFlow += Line.NormalLimit;
            }
            if (IncludePercentage)
                return TotalFlow.ToString("#,##0.00") + " mva  (" + (TotalFlow / MaxFlow).ToString("0%") + " - " + (MaxFlow - TotalFlow).ToString("#,##0.00") + " mva rem.)";
            else
                return (MaxFlow - TotalFlow).ToString("#,##0.00") + " mva rem.";
        }



        /// <summary>
        /// Add ownership items to a menu
        /// </summary>
        /// <param name="BaseElement">The element for which the items should be added</param>
        /// <param name="Items">The collection of menu items</param>
        private void AddOwnershipItems(MM_Element BaseElement, ToolStripItemCollection Items)
        {
            if (BaseElement is MM_Company)
            {
                AddMenuItem(Items, "Company: " + BaseElement.Name.Replace("TOPOLOGY.", ""), true);
                if (Data_Integration.Permissions.ShowTelephone && (BaseElement as MM_Company).PrimaryPhone != null)
                    AddMenuItem(Items, "  Phone: " + (BaseElement as MM_Company).PrimaryPhone, true);
                if (Data_Integration.Permissions.ShowDUNS && (BaseElement as MM_Company).DUNS != null)
                    AddMenuItem(Items, "  DUNS: " + (BaseElement as MM_Company).DUNS, true);
            }
            else if (BaseElement.Operator != null)
            {
                if (BaseElement.Operator == BaseElement.Owner)
                {
                    AddMenuItem(Items, "Operator/Owner: " + BaseElement.Operator.Name.Replace("TOPOLOGY.", "") + " (" + BaseElement.Operator.Alias + ")", true);
                    if (Data_Integration.Permissions.ShowTelephone && BaseElement.Operator != null && BaseElement.Operator.PrimaryPhone != null)
                        AddMenuItem(Items, "  Phone: " + BaseElement.Operator.PrimaryPhone, true);
                    if (Data_Integration.Permissions.ShowDUNS && BaseElement.Operator != null && BaseElement.Operator.DUNS != null)
                        AddMenuItem(Items, "  DUNS: " + BaseElement.Operator.DUNS, true);
                }
                else
                {
                    AddMenuItem(Items, "Operator: " + BaseElement.Operator.Name.Replace("TOPOLOGY.", "") + " (" + BaseElement.Operator.Alias.Replace("TOPOLOGY.", "") + ")", true);
                    if (Data_Integration.Permissions.ShowTelephone && BaseElement.Operator.PrimaryPhone != null)
                        AddMenuItem(Items, "  Phone: " + BaseElement.Operator.PrimaryPhone, true);
                    if (Data_Integration.Permissions.ShowDUNS && BaseElement.Operator.DUNS != null)
                        AddMenuItem(Items, "  DUNS: " + BaseElement.Operator.DUNS, true);
                    AddMenuItem(Items, "-", true);
                    AddMenuItem(Items, "Owner: " + BaseElement.Owner.Name.Replace("TOPOLOGY.", "") + " (" + BaseElement.Owner.Alias + ")", true);
                    if (Data_Integration.Permissions.ShowTelephone && BaseElement.Owner.PrimaryPhone != null)
                        AddMenuItem(Items, "  Phone: " + BaseElement.Owner.PrimaryPhone, true);
                    if (Data_Integration.Permissions.ShowDUNS && BaseElement.Owner.DUNS != null)
                        AddMenuItem(Items, "  DUNS: " + BaseElement.Owner.DUNS, true);
                }
            }
            else
                AddMenuItem(Items, "No Ownership/Operatorship information available", true);
            AddMenuItem(Items, "-", true);
        }

        /// <summary>
        /// Create a new menu item
        /// </summary>
        /// <param name="ItemSource">The repository for this item</param>
        /// <param name="Text">The text of the item</param>
        /// <param name="SetDeactivated">Whether the item should be flagged as being deactivated</param>
        /// <returns></returns>
        private ToolStripMenuItem AddMenuItem(ToolStripItemCollection ItemSource, String Text, bool SetDeactivated)
        {
            ToolStripMenuItem OutItem = ItemSource.Add(Text) as ToolStripMenuItem;
            if (OutItem == null)
                return null;
            else if (SetDeactivated)
                OutItem.ForeColor = MM_Element.MenuDeactivatedColor;
            else
                OutItem.Click += new EventHandler(HandleItemClick);
            return OutItem;
        }

        /// <summary>
        /// Crete a new menu item, with a specified image and element
        /// </summary>
        /// <param name="ItemSource">The repository for this item</param>
        /// <param name="Text">The text of the item</param>
        /// <param name="SetDeactivated">Whether the item should be flagged as being deactivated</param>
        /// <param name="Image">The image for the item</param>
        /// <param name="TagObject">The element or other object associated with the menu item</param>
        /// <returns></returns>
        private ToolStripMenuItem AddMenuItem(ToolStripItemCollection ItemSource, String Text, bool SetDeactivated, Image Image, Object TagObject)
        {
            ToolStripMenuItem NewItem = AddMenuItem(ItemSource, Text, SetDeactivated);
            NewItem.Image = Image;
            NewItem.Tag = TagObject;
            return NewItem;
        }

        /// <summary>
        /// Crete a new menu item that can be expanded
        /// </summary>
        /// <param name="ItemSource">The repository for this item</param>
        /// <param name="Text">The text of the item</param>
        /// <param name="SetDeactivated">Whether the item should be flagged as being deactivated</param>
        /// <param name="Image">The image for the item</param>
        /// <param name="TagObject">The element or other object associated with the menu item</param>
        /// <returns></returns>
        private ToolStripMenuItem AddExpandableMenuItem(ToolStripItemCollection ItemSource, String Text, bool SetDeactivated, Image Image, Object TagObject)
        {
            try
            {
                ToolStripMenuItem NewItem = AddMenuItem(ItemSource, Text, SetDeactivated);
                NewItem.Image = Image;
                NewItem.Tag = TagObject;
                NewItem.DropDownItems.Add(" ");
                NewItem.DropDownOpening += new EventHandler(HandleDropDownOpening);
                return NewItem;
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.MessageBox("Error: " + ex.Message + "\n\n" + ex.StackTrace, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Handle the opening of a dropdown menu by propigating its elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleDropDownOpening(object sender, EventArgs e)
        {
            //First, retrieve our item, and clear the dropdown items
            ToolStripMenuItem InItem = (ToolStripMenuItem)sender;
            InItem.DropDownItems.Clear();

            //If this is a dropdown off an alarm violation, show the appropriate information
            if (InItem.Tag is MM_AlarmViolation)
            {
                MM_AlarmViolation Viol = InItem.Tag as MM_AlarmViolation;
                if (Viol.Contingency != null)
                    AddMenuItem(InItem.DropDownItems, "Contingency " + Viol.Contingency.Name + " (" + Viol.Contingency.Description + ")", true, null, Viol);
                //if (PropigateViolations)                
                BuildItems(Viol, InItem.DropDownItems);
            }
            else if (InItem.Tag is SortedDictionary<MM_KVLevel, Dictionary<MM_Substation, List<MM_Bus>>>)
            {
                bool pU = InItem.Text.StartsWith("Weighted pU");
                foreach (KeyValuePair<MM_KVLevel, Dictionary<MM_Substation, List<MM_Bus>>> kvp in InItem.Tag as SortedDictionary<MM_KVLevel, Dictionary<MM_Substation, List<MM_Bus>>>)
                {

                    float BusSum = 0, BusSum2 = 0f, BusNum = 0f, BusDenom = 0f, MinAngle = float.NaN, MaxAngle = float.NaN;
                    foreach (KeyValuePair<MM_Substation, List<MM_Bus>> kvp2 in kvp.Value)
                        foreach (MM_Bus Bus in kvp2.Value)
                        {
                            BusSum += pU ? Bus.PerUnitVoltage : Bus.Estimated_Angle;
                            BusSum2 += pU ? Bus.PerUnitVoltage * Bus.PerUnitVoltage : Bus.Estimated_Angle * Bus.Estimated_Angle;
                            BusNum += pU ? (Bus.PerUnitVoltage * Math.Abs(1f - Bus.PerUnitVoltage) * Bus.Estimated_kV) : (Bus.Estimated_Angle * Bus.Estimated_kV);
                            BusDenom += pU ? Math.Abs(1f - Bus.PerUnitVoltage) * Bus.Estimated_kV : Bus.Estimated_kV;
                            if (float.IsNaN(MinAngle) || MinAngle > Bus.Estimated_Angle)
                                MinAngle = Bus.Estimated_Angle;
                            if (float.IsNaN(MaxAngle) || MaxAngle < Bus.Estimated_Angle)
                                MaxAngle = Bus.Estimated_Angle;
                        }



                    float Weighted = BusNum / BusDenom;
                    float Average = (BusSum / (float)kvp.Value.Count);
                    float StdDev = (float)Math.Sqrt((((float)kvp.Value.Count * BusSum2) - (BusSum * BusSum)) / ((float)kvp.Value.Count * ((float)kvp.Value.Count - 1f)));
                    if (pU)
                        AddExpandableMenuItem(InItem.DropDownItems, String.Format("{0} buses: {1:0.0%} ± {2:0.0%}", kvp.Key.Name, Average, StdDev), false, ViolationImage(WorstViolation(kvp.Value)), kvp.Value);
                    else
                        AddExpandableMenuItem(InItem.DropDownItems, String.Format("{0} buses: {1:0.0}° ± {2:0.0}°, {3:0.0}° to {4:0.0}°", kvp.Key.Name, Average, StdDev, MinAngle, MaxAngle), false, ViolationImage(WorstViolation(kvp.Value)), kvp.Value);
                }
            }
            else if (InItem.Tag is Dictionary<MM_Substation, List<MM_Bus>>)
            {
                bool pU = InItem.OwnerItem.Text.StartsWith("Weighted pU");
                float BusSum = 0, BusSum2 = 0f, BusNum = 0f, BusDenom = 0f, MinAngle = float.NaN, MaxAngle = float.NaN;
                foreach (KeyValuePair<MM_Substation, List<MM_Bus>> kvp2 in (Dictionary<MM_Substation, List<MM_Bus>>)InItem.Tag)
                {
                    if (kvp2.Value.Count == 1)
                        AddExpandableMenuItem(InItem.DropDownItems, kvp2.Key.MenuDescription() + " " + kvp2.Value[0].MenuDescription(), true, null, kvp2.Value[0]);
                    else
                    {

                        foreach (MM_Bus Bus in kvp2.Value)
                        {
                            BusSum += pU ? Bus.PerUnitVoltage : Bus.Estimated_Angle;
                            BusSum2 += pU ? Bus.PerUnitVoltage * Bus.PerUnitVoltage : Bus.Estimated_Angle * Bus.Estimated_Angle;
                            BusNum += pU ? (Bus.PerUnitVoltage * Math.Abs(1f - Bus.PerUnitVoltage) * Bus.Estimated_kV) : (Bus.Estimated_Angle * Bus.Estimated_kV);
                            BusDenom += pU ? Math.Abs(1f - Bus.PerUnitVoltage) * Bus.Estimated_kV : Bus.Estimated_kV;
                            if (float.IsNaN(MinAngle) || MinAngle > Bus.Estimated_Angle)
                                MinAngle = Bus.Estimated_Angle;
                            if (float.IsNaN(MaxAngle) || MaxAngle < Bus.Estimated_Angle)
                                MaxAngle = Bus.Estimated_Angle;
                        }


                        float Weighted = BusNum / BusDenom;
                        float Average = (BusSum / (float)kvp2.Value.Count);
                        float StdDev = (float)Math.Sqrt((((float)kvp2.Value.Count * BusSum2) - (BusSum * BusSum)) / ((float)kvp2.Value.Count * ((float)kvp2.Value.Count - 1f)));
                        if (pU)
                            AddExpandableMenuItem(InItem.DropDownItems, String.Format("{0} buses: {1:0.0%} ± {2:0.0%}", kvp2.Key.Name, Average, StdDev), false, ViolationImage(WorstViolation(kvp2.Value)), kvp2.Value);
                        else
                            AddExpandableMenuItem(InItem.DropDownItems, String.Format("{0} buses: {1:0.0}° ± {2:0.0}°, {3:0.0}° to {4:0.0}°", kvp2.Key.Name, Average, StdDev, MinAngle, MaxAngle), false, ViolationImage(WorstViolation(kvp2.Value)), kvp2.Value);
                    }
                }
            }
            else if (InItem.Tag is IEnumerable)
                foreach (Object Elem in InItem.Tag as IEnumerable)
                    if (Elem is MM_KVLevel)
                        AddMenuItem(InItem.DropDownItems, (Elem as MM_KVLevel).Name, true);
                    else if (Elem is MM_Line && this.BaseElement is MM_Substation)
                        AddExpandableMenuItem(InItem.DropDownItems, (Elem as MM_Line).MenuDescription(this.BaseElement as MM_Substation), true, (Elem as MM_Line).WorstViolationImage, Elem);
                    else if (InItem.DropDownItems.Count < 50 && Elem is MM_Element)
                    {

                        ToolStripMenuItem NewMenu = AddExpandableMenuItem(InItem.DropDownItems, (Elem as MM_Element).MenuDescription(), true, (Elem as MM_Element).WorstViolationImage, Elem);
                        if (Elem is MM_Unit && (Elem as MM_Unit).FrequencyControl)
                            NewMenu.ForeColor = Color.Purple;
                    }
                    else
                    { }
            else if (InItem.Tag is MM_Element)
                BuildItems(InItem.Tag as MM_Element, InItem.DropDownItems);


        }
        #endregion

        #region Click handlers
        /// <summary>
        /// Locate the network map associated with the display
        /// </summary>
        private MM_Network_Map_DX LocateNetworkMap
        {
            get
            {
                if (OwnerObject is MM_Network_Map_DX)
                    return OwnerObject as MM_Network_Map_DX;
                else if (OwnerObject.Parent is MM_Violation_Viewer)
                    return (OwnerObject.Parent as MM_Violation_Viewer).networkMap;
                else if (OwnerObject is MM_Violation_Viewer)
                    return (OwnerObject as MM_Violation_Viewer).networkMap;
                else if (OwnerObject is MM_Mini_Map)
                    return (OwnerObject as MM_Mini_Map).networkMap;
                else if (OwnerObject is MM_Search_Results)
                    return (OwnerObject as MM_Search_Results).nMap;
                else if (OwnerObject.FindForm() is MM_Form_Builder)
                    return (OwnerObject.FindForm() as MM_Form_Builder).nMap;
                else if (OwnerObject is MM_Form)
                    return (OwnerObject.GetType().GetField("nMap").GetValue(OwnerObject) as MM_Network_Map_DX);
                else if (OwnerObject is MM_Note_Viewer)
                    return (OwnerObject as MM_Note_Viewer).nMap;
             //TODO: Restore one-lines
                    /*
                else if (OwnerObject is MM_OneLine_Viewer)
                    return (OwnerObject as MM_OneLine_Viewer).nMap;
                else if (OwnerObject is MM_OneLine_Element)
                    return ((OwnerObject as MM_OneLine_Element).FindForm() as MM_Blackstart_Display).nMap;*/
                else
                    return Program.MM.ctlNetworkMap;
            }
        }

        /// <summary>
        /// Handle an item being clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem Sender = (ToolStripMenuItem)sender;
            MM_Element Element = (Sender.Tag is MM_Element) ? (Sender.Tag as MM_Element) : BaseElement;

            //If the clicked item is a command, handle accordingly.
            if (Sender.Tag is KeyValuePair<MM_Command, MM_Element>)
            {
                KeyValuePair<MM_Command, MM_Element> kvp = (KeyValuePair<MM_Command, MM_Element>)Sender.Tag;
                kvp.Key.Execute(kvp.Value);
            }
            else if (Sender.Text.Contains("Display") && Sender.Text.Contains("one-line")) ////nataros - may have to take a look at this to ensure does not break anything in ots? was this the line? check where was added by mike
                if (Element is MM_AlarmViolation)
                    MM_Form_Builder.OneLine_Display((Element as MM_AlarmViolation).SubstationOrLine, LocateNetworkMap);
                else
                    MM_Form_Builder.OneLine_Display(Element, LocateNetworkMap);
            else
                switch (Sender.Text)
                {
                    case "Open Website":
                        if ((Element as MM_Boundary).Website != null)
                            System.Diagnostics.Process.Start((Element as MM_Boundary).Website);
                        break;
                    case "Display one-line":
                        if (Element is MM_AlarmViolation)
                            MM_Form_Builder.OneLine_Display((Element as MM_AlarmViolation).SubstationOrLine, LocateNetworkMap);
                        else
                            MM_Form_Builder.OneLine_Display(Element, LocateNetworkMap);
                        break;


                    case "Write note": MM_Note_Writer.CreateNote(Element); break;
                    case "Lasso County": MM_Form_Builder.Lasso_Display(Element as MM_Boundary, LocateNetworkMap, true); break;
                    case "Lasso Area": MM_Form_Builder.Lasso_Display(Element as MM_Boundary, LocateNetworkMap, true); break;
                    case "Acknowledge":
                        Data_Integration.AcknowledgeViolation(Element as MM_AlarmViolation);
                        break;
                    case "Archive":
                        MM_AlarmViolation Viol = (MM_AlarmViolation)Element;
                        if (Viol.Type.InternallyGenerated)
                            Data_Integration.RemoveMonitoringAlert(Viol.ViolatedElement);
                        else
                            Data_Integration.RemoveViolation(Viol);
                        break;
                    case "Display flow in": (OwnerObject as MM_Mini_Map).ShowFlowIn ^= true; break;
                    case "Display flow out": (OwnerObject as MM_Mini_Map).ShowFlowOut ^= true; break;
                    case "Display generation": (OwnerObject as MM_Mini_Map).ShowGen ^= true; break;
                    case "Display load": (OwnerObject as MM_Mini_Map).ShowLoad ^= true; break;
                    case "Display capacitors": (OwnerObject as MM_Mini_Map).ShowCaps ^= true; break;
                    case "Display reactors": (OwnerObject as MM_Mini_Map).ShowReactors ^= true; break;
                    case "Display MVA": (OwnerObject as MM_Mini_Map).ShowMVA ^= true; break;
                    case "Display emergency generation": (OwnerObject as MM_Mini_Map).ShowEmergencyMode ^= true; break;
                    case "Display generation capacity online only": (OwnerObject as MM_Mini_Map).ShowOnlineGenerationOnly ^= true; break;
                    case "Display elements within zoom region": (OwnerObject as MM_Mini_Map).DisplayElementsWithinZoomRegion ^= true; break;
                    case "Display map position": (OwnerObject as MM_Mini_Map).DisplayMapPosition ^= true; break;
                    case "Display violations": (OwnerObject as MM_Mini_Map).DisplayViolations ^= true; break;
                    case "State-wide view": (OwnerObject as MM_Mini_Map).ShowRegionalView = false; (OwnerObject as MM_Mini_Map).RefreshContent(); break;
                    case "Regional view": (OwnerObject as MM_Mini_Map).ShowRegionalView = true; break;
                    case "Move map":
                        if (Element is MM_AlarmViolation)
                            Element = (Element as MM_AlarmViolation).ViolatedElement;
                        if (Element is MM_Contingency)
                        {
                            if ((Element as MM_Contingency).ConElements.Count > 0)
                            {
                                MM_Element conEl = null;
                                // this zoom pan to point doesn't work unless you set the zoom level first.
                                LocateNetworkMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel - 2);
                                MM_Repository.TEIDs.TryGetValue((Element as MM_Contingency).ConElements[0], out conEl);

                                if (Element is MM_Flowgate && (Element as MM_Flowgate).MonitoredElements.Count > 0)
                                    MM_Repository.TEIDs.TryGetValue((Element as MM_Flowgate).MonitoredElements[0], out conEl);

                                if (conEl != null && conEl is MM_Line)
                                    LocateNetworkMap.Coordinates.Center = (((MM_Line)conEl).Midpoint);
                                else if (conEl != null && conEl.Substation != null)
                                    LocateNetworkMap.Coordinates.Center = conEl.Substation.LngLat;
                                else if (Element.Substation != null)
                                    LocateNetworkMap.Coordinates.Center = Element.Substation.LngLat;
                            }
                            
                        }
                        if (Element is MM_Substation)
                        {
                            LocateNetworkMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                            LocateNetworkMap.Coordinates.Center = (Element as MM_Substation).LngLat;
                        }
                        else if (Element is MM_Line)
                        {
                            LocateNetworkMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel-1);
                            LocateNetworkMap.Coordinates.Center = (((Element as MM_Line).Midpoint));
                        }
                        else if (Element.Substation != null)
                        {
                            LocateNetworkMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                            LocateNetworkMap.Coordinates.Center = (Element.Substation.LngLat);
                        }
                        else
                            MM_System_Interfaces.MessageBox("Unable to locate element " + Element + " on the map!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case "View property page":
                    case "Element properties":
                        if (Element is MM_AlarmViolation)
                            MM_Form_Builder.PropertyPage((Element as MM_AlarmViolation).ViolatedElement, LocateNetworkMap);
                        else
                            MM_Form_Builder.PropertyPage(Element, LocateNetworkMap);
                        break;

                    case "Select all":
                    case "Select none":
                    case "Invert selection":
                        (OwnerObject as MM_Violation_Viewer).HandleSelection(Sender.Text);
                        break;

                    case "Acknowledge selected":
                        foreach (ListViewItem I in (OwnerObject as MM_Violation_Viewer).ShownViolations.Values)
                            if (I.Selected)
                                Data_Integration.AcknowledgeViolation(I.Tag as MM_AlarmViolation);
                        break;
                    case "Acknowledge all":
                        foreach (MM_AlarmViolation ThisViol in (Tag as MM_Violation_Viewer.AlarmViolation_Button).Violations.Values.ToList())
                            Data_Integration.AcknowledgeViolation(ThisViol);
                        break;
                    case "Archive selected":
                        {
                            List<MM_AlarmViolation> ToRemove = new List<MM_AlarmViolation>();
                            lock ((OwnerObject as MM_Violation_Viewer).ShownViolationList)
                            {
                              
                                foreach (var I in (OwnerObject as MM_Violation_Viewer).SelectedUIDs.Values.ToList())
                                {
                                    try
                                    {

                                        //if (I.Selected) // this doesn't work consistantly... (index out of range exception)
                                            ToRemove.Add(I);
                                    }
                                    catch (Exception ex)
                                    {
                                        MM_System_Interfaces.LogError(ex);
                                    }
                                }
                            }
                            foreach (MM_AlarmViolation ThisViol in ToRemove.ToList())
                            {
                                if (ThisViol.Type.InternallyGenerated)
                                    Data_Integration.RemoveMonitoringAlert(ThisViol.ViolatedElement);
                                // else
                                Data_Integration.RemoveViolation(ThisViol);
                            }
                            break;
                        }
                    case "Archive all":
                        {
                            List<MM_AlarmViolation> ToRemove = new List<MM_AlarmViolation>((Tag as MM_Violation_Viewer.AlarmViolation_Button).Violations.Values);
                            foreach (MM_AlarmViolation ThisViol in ToRemove)
                            {
                                if (ThisViol.Type.InternallyGenerated)
                                    Data_Integration.RemoveMonitoringAlert(ThisViol.ViolatedElement);
                                // else
                                Data_Integration.RemoveViolation(ThisViol);
                            }
                            break;
                        }

                    //Otherwise, check to see if it's a KV level or violation type. If so, set it only to those
                    default:
                        if (OwnerObject is MM_Violation_Viewer)
                        {
                            if (Sender.OwnerItem == null)
                            { }
                            else if (Sender.OwnerItem.Text == "Show just...")
                                (OwnerObject as MM_Violation_Viewer).SearchSummary(Sender.Tag);
                            else if (Sender.OwnerItem.Text == "View mode")
                                (OwnerObject as MM_Violation_Viewer).lvViolations.View = (View)Sender.Tag;
                            else if (Sender.OwnerItem.Text == "View by...")
                                (OwnerObject as MM_Violation_Viewer).SetSplit((MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum)Sender.Tag);
                            else if (Sender.OwnerItem.Text == "Group by...")
                                (OwnerObject as MM_Violation_Viewer).SetGrouping((MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum)Sender.Tag);
                            else if (Sender.OwnerItem.Text == "Sort by...")
                                (OwnerObject as MM_Violation_Viewer).SetSort((MM_Violation_Viewer.ListViewItemComparer.ComparisonTypeEnum)Sender.Tag);
                        }
                        else if (Sender.Text.Contains("TEID:"))
                        {
                            foreach (Form RunForm in Data_Integration.RunningForms)
                                if (RunForm is MM_Data_Entry)
                                    (RunForm as MM_Data_Entry).SelectElement(MM_Repository.TEIDs[Int32.Parse(Sender.Text.Substring(Sender.Text.IndexOf("TEID:") + 5).Replace(",", "").Trim())]);

                        }
                        else if (Sender.Text.Contains("roperties"))
                        {
                            MM_Form_Builder.PropertyPage(Element, LocateNetworkMap);
                        }
                        else
                            MM_System_Interfaces.LogError("Received unknown command: " + Sender.Text);
                        break;

                }
            OwnerObject.Invalidate();
        }
        #endregion
    }
}