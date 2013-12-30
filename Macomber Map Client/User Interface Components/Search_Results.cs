using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using Macomber_Map.Data_Elements;
using System.Collections;
using Macomber_Map.User_Interface_Components.NetworkMap;
using System.Text.RegularExpressions;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class executes a search and stores the results
    /// </summary>
    public partial class Search_Results : UserControl
    {
        #region Variable declarations
        /// <summary>The calling network map</summary>
        public Network_Map nMap;

        /// <summary>The associated violation viewer</summary>
        public Violation_Viewer violView;
        
        /// <summary>The associated mini-map</summary>
        public Mini_Map miniMap;
        private SplitContainer splSearch;
        private ListView lvSearch;
        private PictureBox picSearch;
        private TextBox txtSearch;
        private Label lblSearch;
        private Property_Page propPage;
        private DataSet_Base BaseData;
        private Element_Summary viewSummary;

        /// <summary>Whether the current search should be aborted</summary>
        public bool AbortSearch = false;

        /// <summary>The collection of base elements returned from the search </summary>
        public List<MM_Element> BaseElements = new List<MM_Element>();

        /// <summary>
        /// The search text used
        /// </summary>
        public String SearchText;

        /// <summary>
        /// The sorter for the list view
        /// </summary>
        private  SimpleListViewItemSorter lvSorter = new SimpleListViewItemSorter();
        private PictureBox picAbort;

        /// <summary>The menu for handling right clicks on a cell</summary>
        private MM_Popup_Menu RightClickMenu = new MM_Popup_Menu();

        /// <summary>
        /// A delegate for handling item selection
        /// </summary>
        /// <param name="SelectedElement"></param>
        public delegate void ItemSelectionDelegate(MM_Element SelectedElement);

        /// <summary>Handle an item selection change</summary>
        public event ItemSelectionDelegate ItemSelectionChanged;

        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the search results
        /// </summary>
        public Search_Results()
        {
            InitializeComponent();
            this.lvSearch.ListViewItemSorter = lvSorter;
            this.lvSearch.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(lvSearch_ItemSelectionChanged);
            this.DoubleBuffered = true;            
        }

        /// <summary>
        /// Handle the change in an item's selection, by calling up the appropriate property page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSearch_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (ItemSelectionChanged != null)
                if (lvSearch.SelectedItems.Count == 1)
                    ItemSelectionChanged(lvSearch.SelectedItems[0].Tag as MM_Element);
                else
                    ItemSelectionChanged(null);


            if (propPage != null)
            if (lvSearch.SelectedItems.Count == 1)
                propPage.SetElement(lvSearch.SelectedItems[0].Tag as MM_Element);
            else
                propPage.SetElement(null);
        }

        #endregion

        /// <summary>
        /// Assign controls to the search results window
        /// </summary>
        /// <param name="nMap">Network Map</param>
        /// <param name="violView">Violations view</param>
        /// <param name="miniMap">The mini-map</param>
        /// <param name="SearchText">Search Text</param>
        /// <param name="Parent">The parent builder</param>
        /// <param name="BaseData">The base data tables</param>                
        /// <param name="ControlPressed">Whether the control button is pressed</param>
        /// <param name="viewSummary"></param>
        /// <param name="AltPressed">Whether the alt button is pressed</param>
        public bool SetControls(Network_Map nMap, Violation_Viewer violView, Mini_Map miniMap, String SearchText, Form_Builder Parent, out DataSet_Base BaseData, Element_Summary viewSummary, bool ControlPressed, bool AltPressed)
        {
            
            MM_Repository.ViewChanged += new MM_Repository.ViewChangedDelegate(Repository_ViewChanged);
            this.nMap = nMap;
            this.miniMap = miniMap;
            this.violView = violView;
            this.viewSummary = viewSummary;
            ConfigureSearch();
            this.txtSearch.Text = SearchText;
            this.SearchText = SearchText;
            bool Resp = PerformSearch(ControlPressed, AltPressed);
            BaseData = this.BaseData;
            return Resp;
        }

        /// <summary>
        /// Configure our search menu
        /// </summary>
        public void ConfigureSearch()
        {
            this.lvSearch.Columns.Add("TEID");
            this.lvSearch.Columns.Add("Name");
            this.lvSearch.Columns.Add("Type");
            this.lvSearch.Columns.Add("KV Level");
            this.lvSearch.Columns.Add("Substation");
            this.lvSearch.Columns.Add("County");
            this.lvSearch.Columns.Add("Owner");
            this.lvSearch.Columns.Add("Operator");
            this.lvSearch.Columns.Add("Load Zone");
            this.lvSearch.Columns.Add("Weather Zone");
            this.lvSearch.SmallImageList = MM_Repository.ViolationImages;
            this.lvSearch.LargeImageList = MM_Repository.ViolationImages;
            this.lvSearch.ColumnClick += new ColumnClickEventHandler(lvSearch_ColumnClick);
            this.lvSearch.View = View.Details;
            this.BaseData = new DataSet_Base("Search");
        }

        /// <summary>
        /// This delegate handles updating the view safely
        /// </summary>
        /// <param name="ActiveView">The new active view</param>
        private delegate void SafeUpdateView(MM_Display_View ActiveView);

        /// <summary>
        /// Update our list view's image list to reflect our new active view.
        /// </summary>
        /// <param name="ActiveView">The updated view</param>
        private void Repository_ViewChanged(MM_Display_View ActiveView)
        {
            if (this.lvSearch.InvokeRequired)
                this.lvSearch.BeginInvoke(new SafeUpdateView(Repository_ViewChanged), ActiveView);
            else
            {
                this.lvSearch.SmallImageList = MM_Repository.ViolationImages;
                this.lvSearch.LargeImageList = MM_Repository.ViolationImages;
            }
        }

        /// <summary>
        /// Assign a property page to the search results form
        /// </summary>
        /// <param name="propPage"></param>
        public void SetPropertyPage(Property_Page propPage)
        {
            this.propPage = propPage;
        }

        /// <summary>
        /// Handle a column click in the search results
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSearch_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (lvSorter.ColumnNumber == e.Column)
                lvSorter.Ascending = !lvSorter.Ascending;
            else
            {
                lvSorter.ColumnNumber = e.Column;
                lvSorter.Ascending = true;
            }
            lvSearch.Sort();
        }


        /// <summary>
        /// Handle the search within the search window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PerformSearch(object sender, EventArgs e)
        {
            lvSearch.Items.Clear();
            if (Visible)
                lvSearch.SuspendLayout();
            PerformSearch(false,false);
            if (Visible)
                lvSearch.ResumeLayout(true);
        }

        /// <summary>
        /// Perform the actual searching
        /// </summary>
        /// <param name="ControlPressed">Whether to return just substations with one item</param>
        /// <param name="AltPressed">Whether alt is pressed</param>
        /// <returns>True if a succesful match has been found</returns>        
        public bool PerformSearch(bool ControlPressed, bool AltPressed)
        {
            picAbort.Visible = true;
            BaseData.Data.Tables.Clear();
            this.lvSearch.Items.Clear();
            this.Cursor = Cursors.WaitCursor;

            //First, see if the string is a TEID, and if so, just popup the property page.
            Int32 TEIDTest = 0;
            MM_Element FoundElem;
            if (ItemSelectionChanged == null && Int32.TryParse(SearchText.Trim(), out TEIDTest))
                if ( MM_Repository.TEIDs.TryGetValue(TEIDTest, out FoundElem))
                {                    
                    if (ControlPressed)
                    {
                        if (FoundElem is MM_Substation)
                            nMap.miniMap.ZoomPanToPoint((FoundElem as MM_Substation).LatLong);
                        else if (FoundElem is MM_Line)
                            nMap.miniMap.ZoomPanToPoint((FoundElem as MM_Line).Midpoint);
                        else if (FoundElem.Substation != null)
                            nMap.miniMap.ZoomPanToPoint(FoundElem.Substation.LatLong);
                        else
                        {
                            Form_Builder.PropertyPage(MM_Repository.TEIDs[TEIDTest], nMap);                            
                            return false;
                        }
                        nMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                        
                    }                   
                    else
                        Form_Builder.PropertyPage(MM_Repository.TEIDs[TEIDTest], nMap);
                    return false;
                }
            
                else
                    return FailError("Unable to find TEID #" + TEIDTest.ToString() + "!");


            //Split apart the string
            List<String> Words = new List<string>();

            //Define the parameters we might be looking for.
            MM_KVLevel KVLevel = null;
            MM_Element_Type ElemType = null;

            if (ItemSelectionChanged != null && int.TryParse(SearchText, out TEIDTest) && MM_Repository.TEIDs.TryGetValue(TEIDTest, out FoundElem))
                AddElement(FoundElem);

            //Go through each word, and test it.
            foreach (String Word in SearchText.Split(new char[] { ' ', '\t', '_' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Object FoundWord = FindWord(Word);
                if (FoundWord == null)
                    Words.Add(Word);
                else if (FoundWord is MM_KVLevel)
                    if (KVLevel != null)
                        return FailError("Please only specify one KV Level in the search query.\nFound " + KVLevel.Name + " and " + (FoundWord as MM_KVLevel).Name);
                    else
                        KVLevel = FoundWord as MM_KVLevel;
                else if (FoundWord is MM_Element_Type)
                    if (ElemType != null)
                        return FailError("Please only specify one element type in the search query.\nFound " + ElemType.Name + " and " + (FoundWord as MM_Element_Type).Name);
                    else
                        ElemType = FoundWord as MM_Element_Type;
            }

            //If we're doing a control, add 'substation'
            if (ElemType == null && (ControlPressed || AltPressed))
                ElemType = MM_Repository.FindElementType("Substation");


            //Now go through all elements, searching for results
            Dictionary<String, bool> ElementWords = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
            //int tempref = 0; /////nataros this is for a catch test, to remove later
            foreach (MM_Element Elem in new List<MM_Element>(MM_Repository.TEIDs.Values))
                if (Elem.Name != null && Elem.TEID > 0)
                    if (Elem is MM_Blackstart_Corridor_Element == false)
                    {
                        if (Elem.Permitted && CheckElement(Elem, KVLevel, ElemType, Words, ElementWords) && Elem.ElemType != null)
                            AddElement(Elem);
                        if (AbortSearch)
                        {
                            AbortSearch = false;
                            return lvSearch.Items.Count > 0;
                        }
                    }
                    else
                    {
                        //catch test, to remove this else case later, needed break pt and counter for test - nataros
                        
                        //tempref = tempref++;
                    }

            this.lvSearch.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.Cursor = Cursors.Default;

            if (viewSummary != null)
                viewSummary.SetControls(BaseData);

            picAbort.Visible = false;
            if (lvSearch.Items.Count == 0)
                return FailError("No results found for: " + this.SearchText);
            else if (ControlPressed)
            {
                MM_Element SelectedElement = null;
                bool OnlyOneElement = lvSearch.Items.Count == 1;
                if (lvSearch.Items.Count == 1)
                    SelectedElement = lvSearch.Items[0].Tag as MM_Element;
                else
                    foreach (ListViewItem lvi in lvSearch.Items)
                        if ((lvi.Tag as MM_Element).Name.Equals(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase))
                            if (SelectedElement != null)
                                OnlyOneElement = false;
                            else if (SelectedElement == null)
                            {
                                SelectedElement = lvi.Tag as MM_Element;
                                OnlyOneElement = true;
                            }

                if (OnlyOneElement)
                {
                    if (SelectedElement is MM_Substation)
                    {
                        nMap.miniMap.ZoomPanToPoint((SelectedElement as MM_Substation).LatLong);
                        nMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                    }
                    else if (SelectedElement is MM_Line)
                    {
                        nMap.miniMap.ZoomPanToPoint(((SelectedElement as MM_Line).Midpoint));
                        nMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                    }
                    else if (SelectedElement.Substation != null)
                    {
                        nMap.miniMap.ZoomPanToPoint(SelectedElement.Substation.LatLong);
                        nMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                    }
                    return false;
                }
            }
            else if (AltPressed)
            {
                MM_Element SelectedElement = null;
                bool OnlyOneElement = lvSearch.Items.Count == 1;
                if (lvSearch.Items.Count == 1)
                    SelectedElement = lvSearch.Items[0].Tag as MM_Element;
                else
                    foreach (ListViewItem lvi in lvSearch.Items)
                        if ((lvi.Tag as MM_Element).Name.Equals(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase))
                            if (SelectedElement != null)
                                OnlyOneElement = false;
                            else if (SelectedElement == null)
                            {
                                SelectedElement = lvi.Tag as MM_Element;
                                OnlyOneElement = true;
                            }
                if (Data_Integration.Permissions.ShowOneLines == true && OnlyOneElement) 
                {
                    if (SelectedElement is MM_Substation)
                        Form_Builder.OneLine_Display(SelectedElement, nMap);
                    else if (SelectedElement is MM_Line)
                        Form_Builder.OneLine_Display(SelectedElement, nMap);
                    else if (SelectedElement.Substation != null)
                        Form_Builder.OneLine_Display(SelectedElement.Substation, nMap);
                    return false;
               }
            }

            //If we have results, select them
            if (ItemSelectionChanged != null && lvSearch.Items.Count > 0)
                lvSearch.Items[0].Selected = true;
            return true;
        }



        /// <summary>
        /// Check to see whether an element meets the inclusion criteria
        /// </summary>
        /// <param name="Elem">The element to be checked </param>
        /// <param name="KVLevel">The KV Level to be used</param>
        /// <param name="ElemType">The Element type to be used</param>
        /// <param name="Words">The words to search for</param>
        /// <param name="ElementWords">The collection of words for an element, for testing</param>        
        /// <returns>True if the element passes all criteria</returns>        
        /// 
        private bool CheckElement(MM_Element Elem, MM_KVLevel KVLevel, MM_Element_Type ElemType, List<String> Words, Dictionary<String, bool> ElementWords)
        {

            if ((KVLevel != null) && (Elem.KVLevel != KVLevel))
                return false;
            else if (ElemType!= null)
                if (ElemType.Name == "SeriesCompensator" && Elem is MM_Line && (Elem as MM_Line).IsSeriesCompensator)
                { }
                else if (Elem.ElemType != ElemType)
                    return false;

            //Now build up our collection of words for this element
            BuildWords(Elem, ElementWords);

            //Now if we have any words, let's test them against this element. If all pass, then we're ok.
            //If a substation, check both name and long name            
            foreach (String Word in Words)
                if (Word.IndexOfAny(new char[] { '?', '*' }) == -1)
                {
                    if (!ElementWords.ContainsKey(Word))
                        return false;
                }
                else
                {
                    Regex TestReg = new Regex("^" + Regex.Escape(Word).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
                    bool FoundMatch = false;
                    foreach (String str in ElementWords.Keys)
                        if (!FoundMatch && TestReg.IsMatch(str))
                            FoundMatch = true;
                    if (!FoundMatch)
                        return false;
                }
            return true;
        }

        /// <summary>
        /// Build the list of words for this item
        /// </summary>
        /// <param name="Elem"></param>
        /// <param name="OutWords"></param>
        private void BuildWords(MM_Element Elem, Dictionary<String, bool> OutWords)
        {
            OutWords.Clear();
            if (!String.IsNullOrEmpty(Elem.Name)) 
                AddWord(OutWords, Elem.Name);
            if (Elem is MM_Substation)
            {
                MM_Substation Sub = Elem as MM_Substation;
                if (!String.IsNullOrEmpty(Sub.LongName))
                    AddWord(OutWords, Sub.LongName);
                if (Sub.County != null)
                    AddWord(OutWords, Sub.County.Name);
                if (Sub.LoadZone != null)
                    AddWord(OutWords, Sub.LoadZone.Name);
                if (Sub.WeatherZone != null)
                    AddWord(OutWords, Sub.WeatherZone.Name);
                if (Sub.Units != null)
                    foreach (MM_Unit Unit in Sub.Units)
                    {
                        AddWord(OutWords, Unit.UnitType.Name);
                        if (!String.IsNullOrEmpty(Unit.UnitType.EMSName))
                            AddWord(OutWords, Unit.UnitType.EMSName);
                        if (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates)
                            AddWord(OutWords, Unit.Estimated_MW > 1f ? "Online" : "Offline");
                        else
                            AddWord(OutWords, Unit.Telemetered_MW > 1f ? "Online" : "Offline");
                    }
                if (Sub.Loads != null)
                    foreach (MM_Load Load in Sub.Loads)
                        if (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates)
                            AddWord(OutWords, Load.Estimated_MW > 1f ? "Online" : "Offline");
                        else
                            AddWord(OutWords, Load.Telemetered_MW > 1f ? "Online" : "Offline");
            }
            else if (Elem is MM_Unit)
            {
                MM_Unit Unit = Elem as MM_Unit;
                AddWord(OutWords, Unit.UnitType.Name);
                if (!String.IsNullOrEmpty(Unit.UnitType.EMSName))
                    AddWord(OutWords, Unit.UnitType.EMSName);
                if (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates)                    
                    AddWord(OutWords, Unit.Estimated_MW > 1f ? "Online" : "Offline");
                else
                    AddWord(OutWords, Unit.Telemetered_MW > 1f ? "Online" : "Offline");
            }
            else if (Elem is MM_Load)
            {
                MM_Load Load = Elem as MM_Load;
                if (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates)
                    AddWord(OutWords, Load.Estimated_MW > 1f ? "Online" : "Offline");
                else
                    AddWord(OutWords, Load.Telemetered_MW > 1f ? "Online" : "Offline");
            }

            else if (Elem is MM_ShuntCompensator)
                AddWord(OutWords, (Elem as MM_ShuntCompensator).Open ? "Open" : "Closed");
            
            if (Elem.Operator != null)
                AddWord(OutWords, Elem.Operator.Name);

            if (Elem.Owner != null)
                AddWord(OutWords, Elem.Owner.Name);

            if (Elem.Substation != null)
            {
                AddWord(OutWords, Elem.Substation.Name);                
                AddWord(OutWords, Elem.Substation.LongName);
                if (Elem.Substation.County != null)
                    AddWord(OutWords, Elem.Substation.County.Name);
                if (Elem.Substation.LoadZone != null)
                    AddWord(OutWords, Elem.Substation.LoadZone.Name);
                if (Elem.Substation.WeatherZone != null)
                    AddWord(OutWords, Elem.Substation.WeatherZone.Name);
                if (Elem.Substation.Units != null)
                    foreach (MM_Unit Unit in Elem.Substation.Units)
                    {
                        AddWord(OutWords, Unit.UnitType.Name);
                        if (!string.IsNullOrEmpty(Unit.UnitType.EMSName))
                            AddWord(OutWords, Unit.UnitType.EMSName);
                        if (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates)
                            if (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates)                    
                                AddWord(OutWords, Unit.Estimated_MW > 1f ? "Online" : "Offline");
                            else
                                AddWord(OutWords, Unit.Telemetered_MW > 1f ? "Online" : "Offline");
                    }
                if (Elem.Substation.Loads != null)
                    foreach (MM_Load Load in Elem.Substation.Loads)                    
                        if (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates)
                            if (Data_Integration.NetworkSource.Estimates && Data_Integration.UseEstimates)
                                AddWord(OutWords, Load.Estimated_MW > 1f ? "Online" : "Offline");
                            else
                                AddWord(OutWords, Load.Telemetered_MW > 1f ? "Online" : "Offline");                    
            }

            if (Elem is MM_Line)
            {
                MM_Line Line = Elem as MM_Line;
                AddWord(OutWords, Line.ConnectedStations[0].Name);
                AddWord(OutWords, Line.ConnectedStations[0].LongName);
                AddWord(OutWords, Line.ConnectedStations[1].Name);
                AddWord(OutWords, Line.ConnectedStations[1].LongName);
                if (Line.ConnectedStations[0].County != null)
                    AddWord(OutWords, Line.ConnectedStations[0].County.Name);
                if (Line.ConnectedStations[0].LoadZone != null)
                    AddWord(OutWords, Line.ConnectedStations[0].LoadZone.Name);
                if (Line.ConnectedStations[0].WeatherZone != null)
                    AddWord(OutWords, Line.ConnectedStations[0].WeatherZone.Name);
                
                if (Line.ConnectedStations[1].County != null)
                    AddWord(OutWords, Line.ConnectedStations[1].County.Name);
                if (Line.ConnectedStations[1].LoadZone != null)
                    AddWord(OutWords, Line.ConnectedStations[1].LoadZone.Name);
                if (Line.ConnectedStations[1].WeatherZone != null)
                    AddWord(OutWords, Line.ConnectedStations[1].WeatherZone.Name);
                
                }
        }

        /// <summary>
        /// Add a word into the search words collection
        /// </summary>
        /// <param name="OutWords"></param>
        /// <param name="WordToAdd"></param>
        private void AddWord(Dictionary<String, bool> OutWords, String WordToAdd)
        {
            if (WordToAdd != null)
            foreach (String Word in WordToAdd.ToLower().Split(' ','_'))
                if (!OutWords.ContainsKey(Word))
                    OutWords.Add(Word, false);
        }

        /// <summary>
        /// Add an element to the list view
        /// </summary>
        /// <param name="Elem">The element to add</param>
        private void AddElement(MM_Element Elem)
        {
            ListViewItem l = new ListViewItem(Elem.TEID.ToString());
            if (Elem is MM_Substation && MM_Repository.OverallDisplay.UseLongNames)
                l.SubItems.Add((Elem as MM_Substation).LongName);            
            else
                l.SubItems.Add(Elem.Name);
           

            l.SubItems.Add(Elem.ElemType.Name);
            if (Elem.KVLevel != null)
                l.SubItems.Add(Elem.KVLevel.Name);
            else
                l.SubItems.Add("");

            if (Elem.Substation != null)                
                l.SubItems.Add(Elem.Substation.DisplayName());                    
            else if (Elem is MM_Line)
                l.SubItems.Add((Elem as MM_Line).ConnectedStations[0].DisplayName() + " to " + (Elem as MM_Line).ConnectedStations[1].DisplayName());
            else
                l.SubItems.Add("");

            if ((Elem is MM_Substation) && ((Elem as MM_Substation).County != null))
                l.SubItems.Add((Elem as MM_Substation).County.Name);
            else if ((Elem.Substation != null) && (Elem.Substation.County != null))
                l.SubItems.Add(Elem.Substation.County.Name);
            else if ((Elem is MM_Line) && ((Elem as MM_Line).ConnectedStations[0].County != null) && ((Elem as MM_Line).ConnectedStations[1].County != null))                                
                if ((Elem as MM_Line).ConnectedStations[0].County == (Elem as MM_Line).ConnectedStations[1].County)
                    l.SubItems.Add((Elem as MM_Line).ConnectedStations[0].County.Name);
                else
                    l.SubItems.Add((Elem as MM_Line).ConnectedStations[0].County.Name + " to " + (Elem as MM_Line).ConnectedStations[1].County.Name);
            else
                l.SubItems.Add("");


            if (Elem.Owner != null)
                l.SubItems.Add(Elem.Owner.Name);
            else
                l.SubItems.Add("");

            if (Elem.Operator != null)
                l.SubItems.Add(Elem.Operator.Name);
            else
                l.SubItems.Add("");

            //Now add in load zone information
            if ((Elem is MM_Substation) && ((Elem as MM_Substation).LoadZone != null))
                l.SubItems.Add((Elem as MM_Substation).LoadZone.Name);
            else if ((Elem.Substation != null) && (Elem.Substation.LoadZone != null))
                l.SubItems.Add(Elem.Substation.LoadZone.Name);
            else if ((Elem is MM_Line) && ((Elem as MM_Line).ConnectedStations[0].LoadZone != null) && ((Elem as MM_Line).ConnectedStations[1].LoadZone != null))
                if ((Elem as MM_Line).ConnectedStations[0].LoadZone == (Elem as MM_Line).ConnectedStations[1].LoadZone)
                    l.SubItems.Add((Elem as MM_Line).ConnectedStations[0].LoadZone.Name);
                else
                    l.SubItems.Add((Elem as MM_Line).ConnectedStations[0].LoadZone.Name + " to " + (Elem as MM_Line).ConnectedStations[1].LoadZone.Name);
            else
                l.SubItems.Add("");

            if ((Elem is MM_Substation) && ((Elem as MM_Substation).WeatherZone != null))
                l.SubItems.Add((Elem as MM_Substation).WeatherZone.Name);
            else if ((Elem.Substation != null) && (Elem.Substation.WeatherZone != null))
                l.SubItems.Add(Elem.Substation.WeatherZone.Name);
            else if ((Elem is MM_Line) && ((Elem as MM_Line).ConnectedStations[0].WeatherZone != null) && ((Elem as MM_Line).ConnectedStations[1].WeatherZone != null))
                if ((Elem as MM_Line).ConnectedStations[0].WeatherZone == (Elem as MM_Line).ConnectedStations[1].WeatherZone)
                    l.SubItems.Add((Elem as MM_Line).ConnectedStations[0].WeatherZone.Name);
                else
                    l.SubItems.Add((Elem as MM_Line).ConnectedStations[0].WeatherZone.Name + " to " + (Elem as MM_Line).ConnectedStations[1].WeatherZone.Name);
            else
                l.SubItems.Add("");


            l.Tag = Elem;
            lvSearch.Items.Add(l);
            BaseElements.Add(Elem);
            BaseData.AddDataElement(Elem);
            Application.DoEvents();
        }


        /// <summary>
        /// Update with our new violation viewer, and set everything up.
        /// </summary>
        /// <param name="ViolViewer">The violation viewer</param>
        public void SetViolationViews(Violation_Viewer ViolViewer)
        {
            this.violView = ViolViewer;
            foreach (ListViewItem l in this.lvSearch.Items)
            {
                MM_Element Elem = l.Tag as MM_Element;
                //If we have a violation, let's add it in.
                MM_AlarmViolation_Type WorstViol = Elem.WorstVisibleViolation(violView.ShownViolations, this.Parent);
                if (WorstViol != null)
                    l.ImageIndex = WorstViol.ViolationIndex;
            }
        }

        /// <summary>
        /// Search for a word within our context
        /// </summary>
        /// <param name="Word">The word to search for</param>
        /// <returns></returns>
        private object FindWord(string Word)
        {            
            Regex TestReg = Word.IndexOfAny(new char[] { '?', '*' }) == -1 ? new Regex("^" + Word + "$", RegexOptions.IgnoreCase) : new Regex("^" + Regex.Escape(Word).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);

            //Start looking at all the KV levels
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                if (KVLevel.Permitted && TestReg.IsMatch(KVLevel.Name))                    
                    return KVLevel;
                else if (KVLevel.Permitted && TestReg.IsMatch(KVLevel.Name.Split(' ')[0]))
                    return KVLevel;

            foreach (MM_Element_Type ElemType in MM_Repository.ElemTypes.Values)
                if (TestReg.IsMatch(ElemType.Name))
                    return ElemType;
                else if (TestReg.IsMatch(ElemType.Acronym))
                    return ElemType;
            return null;
        }

        /// <summary>
        /// Show a message box with the specified error text, and return false
        /// </summary>
        /// <param name="ErrorText">The error text to be displayed</param>
        /// <returns>False</returns>
        private bool FailError(string ErrorText)
        {
            MessageBox.Show(ErrorText, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }


        /// <summary>
        /// This class handles the list view item sorting
        /// </summary>
        private class SimpleListViewItemSorter : IComparer
        {
            public int ColumnNumber = 0;
            public bool Ascending = true;


            public int Compare(object x, object y)
            {
                ListViewItem X = (ListViewItem)x;
                ListViewItem Y = (ListViewItem)y;
                return String.Compare(X.SubItems[ColumnNumber].Text, Y.SubItems[ColumnNumber].Text, true);
            }
        }
        


        private void InitializeComponent()
        {
            this.splSearch = new System.Windows.Forms.SplitContainer();
            this.picAbort = new System.Windows.Forms.PictureBox();
            this.picSearch = new System.Windows.Forms.PictureBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.lvSearch = new System.Windows.Forms.ListView();
            this.splSearch.Panel1.SuspendLayout();
            this.splSearch.Panel2.SuspendLayout();
            this.splSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAbort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).BeginInit();
            this.SuspendLayout();
            // 
            // splSearch
            // 
            this.splSearch.BackColor = System.Drawing.Color.Black;
            this.splSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splSearch.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splSearch.Location = new System.Drawing.Point(0, 0);
            this.splSearch.Name = "splSearch";
            this.splSearch.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splSearch.Panel1
            // 
            this.splSearch.Panel1.Controls.Add(this.picAbort);
            this.splSearch.Panel1.Controls.Add(this.picSearch);
            this.splSearch.Panel1.Controls.Add(this.txtSearch);
            this.splSearch.Panel1.Controls.Add(this.lblSearch);
            // 
            // splSearch.Panel2
            // 
            this.splSearch.Panel2.Controls.Add(this.lvSearch);
            this.splSearch.Size = new System.Drawing.Size(379, 183);
            this.splSearch.SplitterDistance = 34;
            this.splSearch.TabIndex = 0;
            // 
            // picAbort
            // 
            this.picAbort.BackColor = System.Drawing.Color.Transparent;
            this.picAbort.Image = global::Macomber_Map.Properties.Resources.StopHS1;
            this.picAbort.Location = new System.Drawing.Point(316, 6);
            this.picAbort.Name = "picAbort";
            this.picAbort.Size = new System.Drawing.Size(22, 22);
            this.picAbort.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picAbort.TabIndex = 40;
            this.picAbort.TabStop = false;
            this.picAbort.Visible = false;
            this.picAbort.Click += new System.EventHandler(this.picAbort_Click);
            // 
            // picSearch
            // 
            this.picSearch.BackColor = System.Drawing.Color.Transparent;
            this.picSearch.Image = global::Macomber_Map.Properties.Resources.FindNextHS;
            this.picSearch.Location = new System.Drawing.Point(288, 6);
            this.picSearch.Name = "picSearch";
            this.picSearch.Size = new System.Drawing.Size(22, 22);
            this.picSearch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picSearch.TabIndex = 39;
            this.picSearch.TabStop = false;
            this.picSearch.Click += new System.EventHandler(this.PerformSearch);
            // 
            // txtSearch
            // 
            this.txtSearch.AutoCompleteCustomSource.AddRange(new string[] {
            "Substation",
            "Line",
            "Company",
            "County"});
            this.txtSearch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtSearch.BackColor = System.Drawing.Color.Black;
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.ForeColor = System.Drawing.Color.LightGray;
            this.txtSearch.Location = new System.Drawing.Point(75, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(207, 22);
            this.txtSearch.TabIndex = 38;
            this.txtSearch.TextChanged += new System.EventHandler(this.UpdateText);
            this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPress);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Arial Black", 10F);
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSearch.Location = new System.Drawing.Point(3, 9);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(66, 19);
            this.lblSearch.TabIndex = 37;
            this.lblSearch.Text = "Search:";
            // 
            // lvSearch
            // 
            this.lvSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSearch.FullRowSelect = true;
            this.lvSearch.Location = new System.Drawing.Point(0, 0);
            this.lvSearch.Name = "lvSearch";
            this.lvSearch.Size = new System.Drawing.Size(379, 145);
            this.lvSearch.TabIndex = 0;
            this.lvSearch.UseCompatibleStateImageBehavior = false;
            this.lvSearch.View = System.Windows.Forms.View.Details;
            this.lvSearch.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvSearch_MouseClick);
            // 
            // Search_Results
            // 
            this.Controls.Add(this.splSearch);
            this.Name = "Search_Results";
            this.Size = new System.Drawing.Size(379, 183);
            this.splSearch.Panel1.ResumeLayout(false);
            this.splSearch.Panel1.PerformLayout();
            this.splSearch.Panel2.ResumeLayout(false);
            this.splSearch.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picAbort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).EndInit();
            this.ResumeLayout(false);

        }

        private void UpdateText(object sender, EventArgs e)
        {
            this.SearchText = txtSearch.Text;            
        }

        /// <summary>
        /// Handle the user's right-click on an item, to bring up the appropriate menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSearch_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo ht = lvSearch.HitTest(e.Location);
            if (ht.Item == null) return;
            if (e.Button == MouseButtons.Right && ht.Item.Tag is MM_Element)
                RightClickMenu.Show(lvSearch, e.Location, ht.Item.Tag as MM_Element, true);           
        }

        /// <summary>
        /// Set our summary view
        /// </summary>
        /// <param name="viewSummary"></param>
        public  void SetViewSummary(Element_Summary viewSummary)
        {
            this.viewSummary = viewSummary;
        }

        /// <summary>
        /// Abort the current search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picAbort_Click(object sender, EventArgs e)
        {
            AbortSearch = true;
            picAbort.Visible = false;
        }

        /// <summary>
        /// Handle the enter key press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                PerformSearch(sender, e);
        }
    }
}
