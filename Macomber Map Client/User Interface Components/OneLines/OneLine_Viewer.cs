using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using System.Threading;
using Macomber_Map.Data_Connections;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Xml;
using Macomber_Map.User_Interface_Components.NetworkMap;
using Macomber_Map.Data_Connections.Generic;
using MM_Communication.Data_Integration;

namespace Macomber_Map.User_Interface_Components.OneLines
{
    /// <summary>Display a schematic of a substation or line, containing a violation viewer, mini-map, and a schematic view</summary>
    public partial class OneLine_Viewer : UserControl
    {

        #region Variable declarations
        /// <summary>The GUID of the current one-line</summary>
        private Guid OneLineGuid = Guid.Empty;

        /// <summary>Our threshold for highlighting analog differentials</summary>
        private float analogThreshold = .1f;

        /// <summary>The element we're highlighting</summary>
        private MM_OneLine_Element HighlightedElement = null;

        /// <summary>Our base data</summary>
        private DataSet_Base BaseData;

        /// <summary>The flag indicating the one-line has completed loading</summary>
        private bool OneLineLoaded = false;
        /// <summary>The flag indicating the one-line has completed loading</summary>
        private bool OneLineUpdated = false;


        /// <summary>Our event handler for a new one-line being loaded</summary>
        public event EventHandler OneLineLoadCompleted;

        /// <summary>Our collection of unlinked elements</summary>
        private List<MM_OneLine_Element> UnlinkedElements = new List<MM_OneLine_Element>();

        /// <summary>The date/time of CIM export</summary>
        public DateTime ExportDate;

        /// <summary>The CIM file that contained the data</summary>
        public String LastCIM;

        /// <summary>The element at the seed of the one-line</summary>
        public MM_Element BaseElement;

        /// <summary>The network map</summary>
        public Network_Map nMap;

        /// <summary>The collection of one-line elements</summary>
        public Dictionary<MM_Element, MM_OneLine_Element> DisplayElements;

        /// <summary>The collection of one-line nodes</summary>
        public Dictionary<MM_Element, MM_OneLine_Node> DisplayNodes;

        /// <summary>The collection of descriptors</summary>
        public Dictionary<MM_Element, MM_OneLine_Element> Descriptors = new Dictionary<MM_Element, MM_OneLine_Element>();

        /// <summary>The collection of secondary descriptors</summary>
        public Dictionary<MM_Element, MM_OneLine_Element> SecondaryDescriptors = new Dictionary<MM_Element, MM_OneLine_Element>();

        /// <summary>The poke points on the display</summary>
        public Dictionary<Point, MM_OneLine_PokePoint> PokePoints = new Dictionary<Point, MM_OneLine_PokePoint>();

        /// <summary>Our collection of elements by TEID</summary>
        public Dictionary<int, MM_OneLine_Element> ElementsByTEID = new Dictionary<int, MM_OneLine_Element>();

        /// <summary>Our collection of windings by TEID</summary>
        public Dictionary<int, MM_OneLine_TransformerWinding> WindingsByTEID = new Dictionary<int, MM_OneLine_TransformerWinding>();

        /// <summary>
        /// The delegate to handle the clicking of a one-line element
        /// </summary>
        /// <param name="ClickedElement">The clicked element</param>
        /// <param name="e">Information on the mouse click</param>
        public delegate void OneLineElementClickedDelegate(MM_OneLine_Element ClickedElement, MouseEventArgs e);

        /// <summary>This event is fired whenever the user clicks on a one-line element</summary>
        public event OneLineElementClickedDelegate OneLineElementClicked;

       
        /// <summary>
        /// The delegate to handle the clicking of a one-line element
        /// </summary>
        /// <param name="OutData">The outgoing data set</param>
        public delegate void OneLineDataUpdatedDelegate(DataSet OutData);

        /// <summary>This event is fired whenever the user clicks on a one-line element</summary>
        public event OneLineDataUpdatedDelegate OneLineDataUpdated;


        /// <summary>The violation viewer associated with the one-line</summary>
        public Violation_Viewer ViolViewer;

        /// <summary>
        /// The threshold by which SCADA to other values are flagged as different.
        /// </summary>
        public float SCADAThreshold
        {
            get { return 0.05f; }
        }

        /// <summary>Whether violations are currently blinking</summary>
        public bool Blink = false;

        /// <summary>The data source that's driving this display</summary>
        public MM_Data_Source DataSource;
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize the one-line viewer
        /// </summary>
        /// <param name="AutoFollow">Whether to automatically follow the selections</param>
        public OneLine_Viewer(bool AutoFollow)
        {
            InitializeComponent();
           
            foreach (string DataSrc in new string[] { "Real-Time", "Telemetered", "Study" })
                this.btnDataSource.DropDownItems.Add(DataSrc);
            this.btnDataSource.Text = "Real-Time";
            Data_Integration.ViolationAcknowledged += new Data_Integration.ViolationChangeDelegate(UpdateViolation);
            Data_Integration.ViolationAdded += new Data_Integration.ViolationChangeDelegate(UpdateViolation);
            Data_Integration.ViolationModified += new Data_Integration.ViolationChangeDelegate(UpdateViolation);
            Data_Integration.ViolationRemoved += new Data_Integration.ViolationChangeDelegate(UpdateViolation);
            
            //Double-buffer our display
            this.DoubleBuffered = true;
            pnlElements.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pnlElements, true,null);
            btnFollowMap.Checked = AutoFollow;

        }

        private delegate void SafeUpdateViolation(MM_AlarmViolation Violation);

        /// <summary>
        /// If a violation changes, check to see whether we have it. If so, trigger accordingly.
        /// </summary>
        /// <param name="Violation"></param>
        private void UpdateViolation(MM_AlarmViolation Violation)
        {
            if (DisplayElements == null)
                return;
            else if (InvokeRequired)//nataros - crash invistigation
                Invoke(new SafeUpdateViolation(UpdateViolation), Violation);                
            else if (Violation.ViolatedElement is MM_Node)
            {
                MM_OneLine_Node FoundNode;
                if (DisplayNodes.TryGetValue(Violation.ViolatedElement, out FoundNode))
                    FoundNode.Refresh();
            }
            else
            {
                MM_OneLine_Element FoundElem;
                if (DisplayElements.TryGetValue(Violation.ViolatedElement, out FoundElem))
                    FoundElem.Refresh();
            }
        }

        /// <summary>
        /// Assign the controls to the one-line viewer
        /// </summary>
        /// <param name="BaseElement">The base element, from which the Data Integration layer should query for parameters</param>
        /// <param name="nMap">The network map</param>
        /// <param name="BaseData">The base data</param>
        /// <param name="ViolViewer">The violation viewer associated with the one-line</param>
        /// <param name="DataSource">The active data source</param>
        public void SetControls(MM_Element BaseElement, Network_Map nMap, DataSet_Base BaseData, Violation_Viewer ViolViewer, MM_Data_Source DataSource)
        {
            //Assign our data connectors
            this.DataSource = DataSource;
            this.BaseData = BaseData;
            this.nMap = nMap;
            this.ViolViewer = ViolViewer;            

            LoadOneLine(BaseElement,null);
            
            //Assign our events
            tsMain.ItemClicked += new ToolStripItemClickedEventHandler(tsMain_ItemClicked);
        }

        /// <summary>
        /// Set up to open a one-line, and load it.
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <param name="ElementToHighlight"></param>
        public void LoadOneLine(MM_Element BaseElement, MM_Element ElementToHighlight)
        {
            try
            {
                OneLineLoaded = false;
                OneLineUpdated = false;

                this.BaseElement = BaseElement;
                if (this.FindForm() is MM_Blackstart_Display)
                { }
                else if (this.BaseElement is MM_Substation)
                    (this.FindForm() as MM_Form).Title = "One-Line: " + BaseElement.ElemType.Name + " " + (BaseElement as MM_Substation).LongName;
                else
                    (this.FindForm() as MM_Form).Title = "One-Line: " + BaseElement.ElemType.Name + " " + BaseElement.Name;


                //Clear our data, reload in needed info
                BaseData.BaseElement = BaseElement;
                BaseData.Clear();
                foreach (MM_Element Elem2 in MM_Repository.TEIDs.Values)
                    if (Elem2 is MM_Blackstart_Corridor_Element == false && Elem2.Substation == BaseElement)
                        BaseData.AddDataElement(Elem2);
                LoadOneLineConfiguration((BaseElement is MM_Contingency ? "__" : "") + BaseElement.Name, DataSource);
                this.cmbSubstation.SelectedIndex = this.cmbSubstation.Items.Add((BaseElement is MM_Substation ? (BaseElement as MM_Substation).LongName : BaseElement.Name));
                HighlightElement(ElementToHighlight);

                //If we're set to follow map, do so
                if (btnFollowMap.Checked)
                    if (BaseElement is MM_Substation)
                    {
                        nMap.miniMap.ZoomPanToPoint((BaseElement as MM_Substation).LatLong);
                        nMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                    }
                    else if (BaseElement is MM_Line)
                    {
                        nMap.miniMap.ZoomPanToPoint(((BaseElement as MM_Line).Midpoint));
                        nMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                    }

                //If we have no data connectivity, simulate completion
                if (Data_Integration.MMServer == null)
                    OneLineUpdated = true;

            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Highlight an element 
        /// </summary>
        /// <param name="ElementToHighlight"></param>
        public void HighlightElement(MM_Element ElementToHighlight)
        {
            HighlightedElement = null;
            if (ElementToHighlight != null)
            {
                foreach (KeyValuePair<MM_Element, MM_OneLine_Element> kvp in DisplayElements)
                    if (kvp.Key.TEID == ElementToHighlight.TEID)
                    {
                        pnlElements.ScrollControlIntoView(kvp.Value);
                        kvp.Value.Focus();
                        HighlightedElement = kvp.Value;
                        break;
                    }
                foreach (KeyValuePair<MM_Element, MM_OneLine_Node> kvp in DisplayNodes)
                    if (kvp.Key.TEID == ElementToHighlight.TEID)
                    {
                        pnlElements.ScrollControlIntoView(kvp.Value);
                        kvp.Value.Focus();
                        HighlightedElement = kvp.Value;
                        break;
                    }
            }           
            pnlElements.Refresh();
        }

        
        /// <summary>
        /// When our one-line viewer is shutting down, make sure our query is cancelled with the server
        /// </summary>
        ~OneLine_Viewer()
        {
            ShutDownQuery();
        }

        /// <summary>
        /// Shut down our one-line query, if any.
        /// </summary>
        public void ShutDownQuery()
        {
              if (Data_Integration.MMServer != null)               
                    Data_Integration.MMServer.ShutdownOneLine(OneLineGuid);
        }

        
        /// <summary>
        /// Retrieve the XML configuration for a one-line, and display.
        /// </summary>
        /// <param name="OneLineName">The one-line to load</param>
        /// <param name="DataSource">The data source being used for the display</param>
        private void LoadOneLineConfiguration(String OneLineName, MM_Data_Source DataSource)
        {
            OneLineLoaded = false;
            
                try
                {
                    //If we have an old one-line loaded, shut it down
                    if (Data_Integration.MMServer != null && OneLineGuid != Guid.Empty)
                        Data_Integration.MMServer.ShutdownOneLine(OneLineGuid);

                    //Create our data table and query
                    XmlDocument xDoc;
                    if (Data_Integration.MMServer != null)
                        xDoc = Data_Integration.MMServer.LoadOneLine(OneLineName, this, out OneLineGuid);
                    else
                        xDoc = MM_Server_Connector.LoadOneLineFromFile(OneLineName, this, out OneLineGuid);

                    DisplayElements = new Dictionary<MM_Element, MM_OneLine_Element>();
                    DisplayNodes = new Dictionary<MM_Element, MM_OneLine_Node>();
                    List<MM_Element_Type> ShownTypes = new List<MM_Element_Type>(10);
                    ElementsByTEID.Clear();

                    MM_Serializable.ReadXml(xDoc.DocumentElement, this, false);
                    pnlElements.Controls.Clear();
                    Descriptors.Clear();
                    SecondaryDescriptors.Clear();
                    UnlinkedElements.Clear();
                    foreach (XmlElement xNodeElem in new XmlElement[] { xDoc.DocumentElement["Elements"], xDoc.DocumentElement["Nodes"] })
                        foreach (XmlElement xCh in xNodeElem.ChildNodes)
                        {
                            MM_OneLine_Element NewElem;
                            if (xCh.Name == "Node")
                                NewElem = new MM_OneLine_Node(xCh, ViolViewer, this);
                            else if (xCh.Name == "PricingVector")
                                NewElem = new MM_OneLine_PricingVector(xCh, ViolViewer, this);
                            else if (xCh.Name == "Transformer")
                                NewElem = new MM_OneLine_Transformer(xCh, ViolViewer, this);
                            else
                                NewElem = new MM_OneLine_Element(xCh, ViolViewer, this);
                            if (NewElem.BaseElement is MM_DCTie)
                                NewElem.BaseElement = (NewElem.BaseElement as MM_DCTie).AssociatedLine;

                            //If we have a physical element, make sure it's displayed
                            if (NewElem.TEID != 0)
                            {
                                MM_Element CoreElement;
                                if (MM_Repository.TEIDs.TryGetValue(NewElem.BaseElement.TEID, out CoreElement))
                                    NewElem.BaseElement = CoreElement;
                                if (!DisplayElements.ContainsKey(NewElem.BaseElement))
                                {
                                    DisplayElements.Add(NewElem.BaseElement, NewElem);
                                    ElementsByTEID.Add(NewElem.TEID, NewElem);

                                    if (NewElem.ElemType == MM_OneLine_Element.enumElemTypes.Transformer)
                                        foreach (MM_OneLine_TransformerWinding Winding in (NewElem as MM_OneLine_Transformer).TransformerWindings)
                                            if (!WindingsByTEID.ContainsKey(Winding.BaseWinding.TEID))
                                                WindingsByTEID.Add(Winding.BaseWinding.TEID, Winding);
                                    if (Data_Integration.DataConnections.Count > 0)
                                    {
                                        AddElementTypeToQuery(NewElem.BaseElement.ElemType, ShownTypes);
                                        AddElementToQuery(NewElem);

                                        if (NewElem.ElemType == MM_OneLine_Element.enumElemTypes.Node && (NewElem.BaseElement as MM_Node).BusbarSection != null)
                                        {
                                            AddElementTypeToQuery((NewElem.BaseElement as MM_Node).BusbarSection.ElemType, ShownTypes);
                                            AddElementToQuery((NewElem.BaseElement as MM_Node).BusbarSection);
                                        }


                                    }
                                }


                                //Make sure our nodes are handled properly
                                if (NewElem is MM_OneLine_Node)
                                {
                                    MM_OneLine_Node NewNode = NewElem as MM_OneLine_Node;
                                    DisplayNodes.Add(NewElem.BaseElement, NewNode);
                                    foreach (MM_OneLine_PokePoint[] Pokes in NewNode.ConnectionPoints.Values)
                                        foreach (MM_OneLine_PokePoint Poke in Pokes)
                                            if (Poke is MM_OneLine_PricingVector || Poke.IsVisible || Poke.IsJumper)
                                                pnlElements.Controls.Add(Poke);

                                    //Add our mappings to our elements if needed
                                    foreach (XmlElement xLinkage in xCh.ChildNodes)
                                        if (xLinkage.Name == "Transformer")
                                        {
                                            Int32 TEID = Convert.ToInt32(xLinkage.Attributes["TEID"].Value);
                                            foreach (MM_OneLine_Element Elem in DisplayElements.Values)
                                                if (Elem.TEID == TEID)
                                                {
                                                    MM_OneLine_Transformer XF = Elem as MM_OneLine_Transformer;
                                                    XF.AssignNode(NewNode);
                                                }

                                        }
                                }



                                NewElem.BaseRow = BaseData.AddDataElement(NewElem.BaseElement);

                                if (NewElem is MM_OneLine_Transformer)
                                    foreach (MM_OneLine_TransformerWinding Winding in (NewElem as MM_OneLine_Transformer).TransformerWindings)
                                        Winding.BaseRow = BaseData.AddDataElement(Winding.BaseWinding);
                                pnlElements.Controls.Add(NewElem);
                            }
                            else
                                NewElem.Dispose();
                        }



                    //Handle all of our unlinked elements
                    if (xDoc.DocumentElement["Unlinked_Elements"] != null)
                        foreach (XmlElement xElem in xDoc.DocumentElement["Unlinked_Elements"].ChildNodes)
                        {
                            MM_OneLine_Element NewElem = new MM_OneLine_Element(xElem, ViolViewer, this);
                            if (NewElem.ElemType != MM_OneLine_Element.enumElemTypes.Label)
                                pnlElements.Controls.Add(NewElem);
                            UnlinkedElements.Add(NewElem);
                        }

                    //Now, assign all of our elements
                    foreach (MM_OneLine_Element Elem in DisplayElements.Values)
                    {
                        Elem.MouseClick += new MouseEventHandler(Elem_MouseClick);
                        Elem.MouseDoubleClick += new MouseEventHandler(Elem_MouseDoubleClick);
                        if (Elem.ElemType == MM_OneLine_Element.enumElemTypes.Transformer)
                        {
                            MM_OneLine_Transformer XF = Elem as MM_OneLine_Transformer;
                            XF.TransformerWindings[0].BaseWinding.NodeName = XF.AssociatedNodes[0].BaseElement.Name;
                            XF.TransformerWindings[1].BaseWinding.NodeName = XF.AssociatedNodes[1].BaseElement.Name;
                            AddElementTypeToQuery(XF.TransformerWindings[0].BaseWinding.ElemType, ShownTypes);
                            AddElementToQuery(XF.TransformerWindings[0].BaseWinding);
                            AddElementToQuery(XF.TransformerWindings[1].BaseWinding);
                        }

                    }

                    //Update our tables to hold all needed elements
                    XmlNode xTableLinkage;
                    foreach (DataTable tbl in BaseData.Data.Tables)
                    {
                        if (Data_Integration.MMServer != null)
                            xTableLinkage = Data_Integration.MMServer.OneLineMappings.SelectSingleNode("/OneLineData/ElementType[@Name='" + tbl.TableName + "']");
                        else
                            xTableLinkage = MM_Repository.xConfiguration.SelectSingleNode("/Configuration/OneLineData/ElementType[@Name='" + tbl.TableName + "']");
                        if (xTableLinkage != null)
                            foreach (XmlNode xDataLocator in xTableLinkage.SelectNodes("DataLocator"))
                                foreach (string LineSplit in (tbl.TableName == "Line" ? "_1,_2" : "").Split(','))
                                {
                                    if (xDataLocator.Attributes["Database"].Value == "State Estimator")
                                        foreach (String str in new string[] { "Real-Time", "Study" })
                                            foreach (XmlNode xLinkage in xDataLocator.SelectNodes("DatabaseLinkage[@Category='Status']"))
                                                AddOneLineDataColumn(tbl, str + "\\" + xLinkage.Attributes["Name"].Value + LineSplit, Macomber_Map.Data_Connections.MM_Type_Finder.LocateType(xLinkage.Attributes["VarType"].Value, null));
                                    else if (xDataLocator.Attributes["ApplicationName"] != null)
                                        foreach (XmlNode xLinkage in xDataLocator.SelectNodes("DatabaseLinkage[@Category='Status']"))
                                            AddOneLineDataColumn(tbl, xDataLocator.Attributes["ApplicationName"].Value + "\\" + xLinkage.Attributes["Name"].Value + LineSplit, Macomber_Map.Data_Connections.MM_Type_Finder.LocateType(xLinkage.Attributes["VarType"].Value, null));
                                    else
                                        foreach (XmlNode xLinkage in xDataLocator.SelectNodes("DatabaseLinkage[@Category='Status']"))
                                            AddOneLineDataColumn(tbl, "Telemetered\\" + xLinkage.Attributes["Name"].Value + LineSplit, Macomber_Map.Data_Connections.MM_Type_Finder.LocateType(xLinkage.Attributes["VarType"].Value, null));
                                    if (xDataLocator.SelectNodes("DatabaseLinkage[@Name='SubTEID']") != null)
                                        AddOneLineDataColumn(tbl, "SubTEID" + LineSplit, typeof(Int32));
                                }
                    }

                    //Clone our lines to have substations
                    foreach (KeyValuePair<MM_Element, MM_OneLine_Element> kvp in DisplayElements)
                        if (kvp.Key is MM_Line && (kvp.Key as MM_Line).IsSeriesCompensator == false && kvp.Value.OtherRow == null)
                        {
                            kvp.Value.BaseRow["SubTEID_1"] = (kvp.Key as MM_Line).Substation1.TEID;
                            kvp.Value.BaseRow["SubTEID_2"] = (kvp.Key as MM_Line).Substation2.TEID;
                        }

                    //Update our transformer to pull in its windings
                    foreach (KeyValuePair<MM_Element, MM_OneLine_Element> kvp in DisplayElements)
                        if (kvp.Key is MM_Transformer && kvp.Value.OtherRow == null)
                        {
                            kvp.Value.BaseRow = BaseData.Data.Tables["TransformerWinding"].Rows.Find((kvp.Key as MM_Transformer).Winding1.TEID);
                            kvp.Value.OtherRow = BaseData.Data.Tables["TransformerWinding"].Rows.Find((kvp.Key as MM_Transformer).Winding2.TEID);
                        }

                    //Slightly increase the size.
                    Point RightBottom = Point.Empty;
                    foreach (IEnumerable<MM_OneLine_Element> Elems in new IEnumerable<MM_OneLine_Element>[] { DisplayElements.Values, Descriptors.Values, SecondaryDescriptors.Values })
                        foreach (MM_OneLine_Element Elem in Elems)
                            RightBottom = new Point(Math.Max(RightBottom.X, Elem.Right + 50), Math.Max(RightBottom.Y, Elem.Bottom + 50));

                    //If offline, retrieve any element status data we may find
                    if (Data_Integration.MMServer == null)
                    {
                        String AttrName;
                        DataColumn FoundCol;
                        foreach (MM_OneLine_Element Elem in DisplayElements.Values)
                            foreach (XmlAttribute xAttr in Elem.xElement.Attributes)
                                if ((AttrName = XmlConvert.DecodeName(xAttr.Name)).Contains("\\") && (FoundCol = Elem.BaseRow.Table.Columns[AttrName]) != null)
                                    try
                                    {
                                        Elem.BaseRow[AttrName] = MM_Serializable.RetrieveConvertedValue(FoundCol.DataType, xAttr.Value, Elem, false);
                                    }
                                    catch
                                    {
                                    }
                    }


                    Label TestLabel = new Label();
                    TestLabel.Location = RightBottom;
                    TestLabel.Text = " ";
                    pnlElements.Controls.Add(TestLabel);
                    pnlElements.Size = MaxSize;
                    //pnlElements.DisplayRectangle = new Rectangle(0, 0, pnlElements.ClientRectangle.Width + 400, pnlElements.ClientRectangle.Height + 400);                                             
                    OneLineLoaded = true;

                }
                catch (Exception ex)
                {                    
                    OneLineLoaded = true;
                    Console.WriteLine("Error opening one-line " + OneLineName + ": " + ex.ToString());
                }
            if (OneLineLoadCompleted != null)
                OneLineLoadCompleted(this, EventArgs.Empty);
        }

        

        /// <summary>
        /// Add a column to our one-line table
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="ColumnName"></param>
        /// <param name="ColumnType"></param>
        private void AddOneLineDataColumn(DataTable tbl, String ColumnName, Type ColumnType)
        {
            if (ColumnName.Contains("{Measurement}"))
                foreach (String str in "MW,MVAR,MVA".Split(','))
                    AddOneLineDataColumn(tbl, ColumnName.Replace("{Measurement}", str), ColumnType);
            else if (!tbl.Columns.Contains(ColumnName))
                tbl.Columns.Add(ColumnName, ColumnType);
        }



        /// <summary>
        /// Add a new element type to the query
        /// </summary>
        /// <param name="ElemType">The element type to add</param>
        /// <param name="ShownTypes">The collection of already-parsed types</param>
        private void AddElementTypeToQuery(MM_Element_Type ElemType, List<MM_Element_Type> ShownTypes)
        {




            //Now set our primary key on our table
            // if (BaseData.Data.Tables.Contains(ElemType.Name))
            //   BaseData.Data.Tables[ElemType.Name].PrimaryKey = new DataColumn[] { BaseData.Data.Tables[ElemType.Name].Columns["TEID"] };
            ShownTypes.Add(ElemType);
        }






        /// <summary>
        /// Locate or create a row corresponding to our line and substation
        /// </summary>
        /// <param name="BaseTable"></param>
        /// <param name="AssociatedNode"></param>
        /// <param name="XFw"></param>
        /// <returns></returns>
        private DataRow FindRow(DataTable BaseTable, MM_TransformerWinding XFw, MM_Element AssociatedNode)
        {
            DataRow FoundRow = BaseTable.Rows.Find(new object[] { XFw, AssociatedNode });

            if (FoundRow == null)
            {
                FoundRow = BaseTable.NewRow();
                FoundRow["TEID"] = XFw;
                FoundRow["NodeTEID"] = AssociatedNode;
                FoundRow["Node"] = AssociatedNode.Name;
                FoundRow["Name"] = XFw.Transformer.Name;
                FoundRow["Substation"] = XFw.Transformer.Substation.Name;

                if (XFw.KVLevel != null && BaseTable.Columns.Contains("Voltage") && BaseTable.Columns["Voltage"].DataType == typeof(string))
                    FoundRow["Voltage"] = XFw.KVLevel.Name;
                FoundRow.Table.Rows.Add(FoundRow);
            }


            return FoundRow;
        }

        /// <summary>
        /// Locate or create a new row in our data table
        /// </summary>
        /// <param name="BaseTable"></param>
        /// <param name="Elem"></param>
        /// <returns></returns>
        private DataRow FindRow(DataTable BaseTable, MM_Element Elem)
        {
            if (BaseTable == null)
                return null;
            DataRow FoundRow = BaseTable.Rows.Find(Elem);

            if (FoundRow == null)
            {
                FoundRow = BaseTable.NewRow();
                FoundRow["TEID"] = Elem;
                FoundRow["Name"] = Elem.Name;
                if (Elem.Substation != null)
                    FoundRow["Substation"] = Elem.Substation.Name;
                if (Elem.KVLevel != null && BaseTable.Columns.Contains("Voltage"))
                    FoundRow["Voltage"] = Elem.KVLevel.Name;


                FoundRow.Table.Rows.Add(FoundRow);
            }
            else if (FoundRow.Table.Columns.Contains("Substation"))
                if (FoundRow.Table.Columns["Substation"].DataType == typeof(String))
                    FoundRow["Substation"] = Elem.Substation.Name;
                else if (FoundRow.Table.Columns["Substation"].DataType == typeof(string[]))
                    FoundRow["Substation"] = new string[] { (Elem as MM_Line).Substation1.Name, (Elem as MM_Line).Substation2.Name };

            return FoundRow;
        }

        /// <summary>        
        /// Add an additional element to the query configuration
        /// </summary>
        /// <param name="Elem"></param>
        private void AddElementToQuery(MM_OneLine_Element Elem)
        {

        }

        /// <summary>        
        /// Add an additional element to the query configuration
        /// </summary>
        /// <param name="Elem"></param>
        private void AddElementToQuery(MM_Element Elem)
        {

        }

        /// <summary>
        /// Report the maximum size this window should be.
        /// </summary>
        public Size MaxSize
        {
            get
            {

                Size MaxSize = new Size();
                foreach (Control c in pnlElements.Controls)
                {
                    if (MaxSize.Width < c.Right)
                        MaxSize.Width = c.Right;
                    if (MaxSize.Height < c.Bottom)
                        MaxSize.Height = c.Bottom;
                }
                MaxSize.Width += 100;
                MaxSize.Height += 100;
                return MaxSize;
            }
        }
        /// <summary>
        /// Handle the clicking on a one-line element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Elem_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (!OneLineUpdated)
                    return;

                if (OneLineElementClicked != null)
                    OneLineElementClicked(sender as MM_OneLine_Element, e);
                MM_OneLine_Element Elem = sender as MM_OneLine_Element;
                HighlightElement(Elem.BaseElement);
                if (btnFollowMap.Checked)
                    if (Elem.ElemType == MM_OneLine_Element.enumElemTypes.Line)
                    {
                        nMap.miniMap.ZoomPanToPoint(((Elem.BaseElement as MM_Line).Midpoint));
                        nMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                    }
                    else if (BaseElement is MM_Substation == false && Elem.BaseElement.Substation != null)
                    {
                        nMap.miniMap.ZoomPanToPoint(((Elem.BaseElement.Substation).LatLong));
                        nMap.Coordinates.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                    }
            }
            catch
            { 
            }
        }
        
        /// <summary>
        /// Handle a one-line element double-click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Elem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
           //Handle the double-click on an element
            MM_OneLine_Element Elem = (MM_OneLine_Element)sender;
            MM_Contingency FoundCtg;

            if (!OneLineUpdated)
                return;

            if (Elem.ElemType == MM_OneLine_Element.enumElemTypes.Line & BaseElement is MM_Substation)
            {
                MM_Line BaseLine = Elem.BaseElement as MM_Line;
                MM_Substation TargetStation = BaseLine.Substation1.Equals(BaseElement) ? BaseLine.Substation2 : BaseLine.Substation1;
                if (MM_Repository.TEIDs.ContainsKey(TargetStation.TEID))
                {
                    LoadOneLine(TargetStation, BaseLine);

                }

            }
            else if (Elem.ElemType == MM_OneLine_Element.enumElemTypes.Node)
            {
                if (BaseElement is MM_Substation == false)
                {
                    if (MM_Repository.TEIDs.ContainsKey(Elem.BaseElement.Substation.TEID))
                    {
                        LoadOneLine(Elem.BaseElement.Substation, Elem.BaseElement);
                    }

                }
                else
                    foreach (String str in Elem.xElement.Attributes["Contingencies"].Value.Split(','))
                        if (!str.StartsWith("D") && MM_Repository.Contingencies.TryGetValue(str, out FoundCtg))
                        {
                            LoadOneLine(FoundCtg, Elem.BaseElement);

                            return;
                        }
            }
            else
                if (Elem.BaseElement.ElemType != null)
                {
                    if (Data_Integration.MMServer != null && Array.IndexOf(Data_Integration.MMServer.UserOperatorships, 999999) == -1 && Array.IndexOf(Data_Integration.MMServer.UserOperatorships, Elem.BaseElement.Operator.TEID) == -1)
                        MessageBox.Show("Control of " + (Elem.BaseElement.Substation == null ? "" : Elem.BaseElement.Substation.LongName) + " " + Elem.BaseElement.ElemType.Name + " " + Elem.BaseElement.Name + " is not permitted (operated by " + Elem.BaseElement.Operator.Name + " - " + Elem.BaseElement.Operator.Alias + ").", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    else
                    {
                        System.Xml.XmlNode xDatabase;
                        if (Data_Integration.MMServer != null)
                        xDatabase = Data_Integration.MMServer.OneLineMappings.SelectSingleNode("ElementType[@Name='" + Elem.BaseElement.ElemType.Name + "']/DataLocator[@Database='" + (btnDataSource.Text == "Telemetered" ? "SCADA" : "State Estimator") + "']");
                        else
                            xDatabase = MM_Repository.xConfiguration.SelectSingleNode("/Configuration/OneLineData/ElementType[@Name='" + Elem.BaseElement.ElemType.Name + "']/DataLocator[@Database='" + (btnDataSource.Text == "Telemetered" ? "SCADA" : "State Estimator") + "']");

                        if (xDatabase != null)
                            foreach (System.Xml.XmlNode xControlPanel in xDatabase.SelectNodes("ControlPanel"))
                            {                                
                                MM_Form NewForm = new MM_ControlPanel(Elem, xControlPanel as XmlElement, btnDataSource.Text, Data_Integration.NetworkSource);
                                NewForm.AutoSize = true;
                                //First, configure the one-line directly to the right/center of our element
                                Rectangle TargetBounds = RectangleToScreen(Elem.Bounds);
                                NewForm.Location = new Point(TargetBounds.Right + 5, (TargetBounds.Top + TargetBounds.Height / 2) - (NewForm.Height / 2));

                                //Now, shift our control panel until we're on the target screen
                                Rectangle ThisBounds = Screen.GetWorkingArea(Elem);
                                if (NewForm.Top < ThisBounds.Top)
                                    NewForm.Top = TargetBounds.Bottom + 20;
                                if (NewForm.Bottom > ThisBounds.Bottom)
                                    NewForm.Top = TargetBounds.Top - NewForm.Height - 20;
                                if (NewForm.Right > ThisBounds.Right)
                                    NewForm.Left = TargetBounds.Left - NewForm.Width - 20;

                                NewForm.StartPosition = FormStartPosition.Manual;
                                NewForm.Show();
                            }
                    }               
                }

        }

        /// <summary>
        /// Draw the header bar for our one
        /// </summary>
        /// <param name="g"></param>
        /// <param name="BaseElement"></param>
        /// <param name="Bounds"></param>
        public void DrawHeader(Graphics g, Rectangle Bounds, MM_Element BaseElement)
        {
            String LongName = "?";
            String Name = BaseElement.Name;
            String County = "Unknown";
            PointF LatLng = PointF.Empty;
            if (BaseElement is MM_Substation)
            {
                MM_Substation Sub = BaseElement as MM_Substation;
                LongName = Sub.LongName;
                County = Sub.County.Name;
                LatLng = Sub.LatLong;
            }
            else if (BaseElement is MM_Contingency)
            {
                MM_Contingency Ctg = BaseElement as MM_Contingency;
                LongName = Ctg.Description;
                StringBuilder sB = new StringBuilder();
                foreach (MM_Boundary Bound in Ctg.Counties)
                    if (Bound != null)
                        sB.Append((sB.Length == 0 ? "" : ",") + Bound.Name);
                County = sB.ToString();
            }
            else if (BaseElement is MM_Company)
                LongName = (BaseElement as MM_Company).Name;



            String ExportedOn = "Exported on: " + Data_Integration.ModelDate;
            String CIMFile = "CIM file: " + Data_Integration.ModelName;
            String OutText = (String.IsNullOrEmpty(LongName) || LongName.Equals(Name, StringComparison.CurrentCultureIgnoreCase)) ? Name : LongName + " (" + Name + ")";
            String Coord = String.Format("County: {0}\nLat={1:0.000}, Lng={2:0.000}", County, LatLng.Y, LatLng.X);


            //Now, write out our header
            //if (User_Interface_Components.GDIPlus.Network_Map_GDI.LockForDataUpdate)
           // {
            //    g.FillRectangle(Brushes.LightGray, Bounds);
            //}
            //else
            //{
                if (btnDataSource.Text == "Study")
                    g.FillRectangle(Brushes.Pink, Bounds);
                else if (btnDataSource.Text == "Telemetered")
                    g.FillRectangle(Brushes.MediumAquamarine, Bounds);
                else
                    g.FillRectangle(Brushes.SteelBlue, Bounds);
            //}


            using (Font Arial = new Font("Arial", 9))
            {
                int XCoord = Bounds.Right - (int)Math.Ceiling(g.MeasureString(ExportedOn + "\n" + CIMFile, Arial).Width);
                g.DrawString(ExportedOn, Arial, Brushes.White, XCoord, Bounds.Top + 3);
                g.DrawString(CIMFile, Arial, Brushes.White, XCoord, Bounds.Top + 21);
                g.DrawString(Coord, Arial, Brushes.White, Bounds.Left + 2, Bounds.Top + 3);
            }
            using (Font Arial = new Font("Arial", 15))
            {
                Size HeadSize = g.MeasureString(OutText, Arial).ToSize();
                g.DrawString(OutText, Arial, Brushes.White, Bounds.Left + ((Bounds.Width - HeadSize.Width) / 2), Bounds.Top + ((Bounds.Height - HeadSize.Height) / 2));
            }
        }
        /// <summary>
        /// Set the background image of the panel (the lines connecting the one-line elements)
        /// </summary>
        private void PanelPaint(object Sender, PaintEventArgs e)
        {
            try
            {
                if (DisplayElements == null)
                    return;
                //Draw our PUN equipment
                foreach (MM_OneLine_Element Elem in DisplayElements.Values)
                    if (Elem.BaseElement != null && Elem.BaseElement.PUNElement)
                        e.Graphics.FillRectangle(Brushes.DarkSlateGray, new Rectangle(Elem.Left - 10, Elem.Top - 10, Elem.Width + 20, Elem.Height + 20));
                foreach (MM_OneLine_Element Elem in DisplayNodes.Values)
                    if (Elem.BaseElement != null && Elem.BaseElement.PUNElement)
                        e.Graphics.FillRectangle(Brushes.DarkSlateGray, new Rectangle(Elem.Left - 10, Elem.Top - 10, Elem.Width + 20, Elem.Height + 20));

                //Zoom as appropriate
                /*{
                    float ZoomSize = 1f;
                    if (cmbZoom.Text == "Fit to page")
                        ZoomSize = 0.2f;
                    else if (Single.TryParse(cmbZoom.Text.Replace("%", ""), out ZoomSize))
                        ZoomSize = 1 - ZoomSize;
                    e.Graphics.ScaleTransform(ZoomSize, ZoomSize);
                }*/


                //First, reposition the graphics 
                e.Graphics.TranslateTransform(pnlElements.AutoScrollPosition.X, pnlElements.AutoScrollPosition.Y);

                //Paint our header information      
                DrawHeader(e.Graphics, new Rectangle(0, 0, pnlElements.DisplayRectangle.Width, 40), BaseElement);


                if (mnuNodeToElementLines.Checked)
                    foreach (MM_OneLine_Node Node in DisplayNodes.Values)
                        Node.DrawConnectingLines(e.Graphics);


                //e.Graphics.TranslateTransform(pnlElements.AutoScrollPosition.X, pnlElements.AutoScrollPosition.Y);


                //Now, untranslate the image
                //e.Graphics.TranslateTransform(-pnlElements.AutoScrollPosition.X, -pnlElements.AutoScrollPosition.Y);


                //Paint our descriptor arrows
                foreach (MM_OneLine_Element Desc in Descriptors.Values)
                    if (Desc != null)
                        if (Desc.ParentElement.DescriptorArrow || Desc.DescriptorArrow)
                            MM_OneLine_Element.DrawArrow(e.Graphics, Desc.Bounds, MM_OneLine_Element.ShiftRectangle(Desc.ParentElement.Bounds, pnlElements.AutoScrollPosition), Desc.DrawBrush);

                foreach (MM_OneLine_Element Desc in SecondaryDescriptors.Values)
                    if (Desc != null)
                        if (Desc.ParentElement.DescriptorArrow || Desc.DescriptorArrow)
                            MM_OneLine_Element.DrawArrow(e.Graphics, Desc.Bounds, MM_OneLine_Element.ShiftRectangle(Desc.ParentElement.Bounds, pnlElements.AutoScrollPosition), Desc.DrawBrush);

                //Determine which analogs we should check
                List<string> AnalogsToCheck = new List<string>();
                foreach (ToolStripMenuItem tsi in new ToolStripMenuItem[] { mWToolStripMenuItem, mVARToolStripMenuItem, mVAToolStripMenuItem, gMWToolStripMenuItem, gMVToolStripMenuItem })
                    if (tsi.Checked)
                        AnalogsToCheck.Add(tsi.Text);

                //Write our secondary and primary descriptors
                foreach (MM_OneLine_Element SecondaryDescriptor in SecondaryDescriptors.Values)
                    SecondaryDescriptor.DrawDescriptor(e.Graphics);
                foreach (MM_OneLine_Element Descriptor in Descriptors.Values)
                {
                    Descriptor.DrawDescriptor(e.Graphics);
                    if (Descriptor.ParentElement.CheckSCADAMismatch(analogThreshold, tsIgnoreMissing.Checked, breakerStateToolStripMenuItem.Checked, switchStateToolStripMenuItem.Checked, AnalogsToCheck))
                    {
                        Rectangle DescriptorRectangle = Descriptor.DescriptorRectangle;
                        ControlPaint.DrawBorder3D(e.Graphics, DescriptorRectangle.Left - 5, DescriptorRectangle.Top - 5, DescriptorRectangle.Width + 10, DescriptorRectangle.Height + 10);
                    }
                }

                //If we have an element to highlight, do so
                if (HighlightedElement != null)
                    using (Pen DrawPen = new Pen(Color.Red, 3f))
                    {
                        DrawPen.DashStyle = DashStyle.Dot;
                        DrawPen.LineJoin = LineJoin.Round;
                        Rectangle TargetRect = new Rectangle(HighlightedElement.Left - pnlElements.AutoScrollPosition.X, HighlightedElement.Top - pnlElements.AutoScrollPosition.Y, HighlightedElement.Width, HighlightedElement.Height);
                        if (HighlightedElement.Descriptor == null)
                        { }
                        else
                        {
                            Rectangle LabelRect = HighlightedElement.Descriptor.DescriptorRectangle;
                            if (LabelRect.IsEmpty)
                                TargetRect = Rectangle.Union(TargetRect, HighlightedElement.Descriptor.Bounds);
                            else
                                TargetRect = Rectangle.Union(TargetRect, LabelRect);
                        }
                        e.Graphics.DrawRectangle(DrawPen, TargetRect.Left - 10, TargetRect.Top - 10, TargetRect.Width + 20, TargetRect.Height + 20);
                    }

                foreach (MM_OneLine_Element Elem in UnlinkedElements)
                    if (Elem.ElemType == MM_OneLine_Element.enumElemTypes.Label)
                        using (Brush DrawBrush = new SolidBrush(Elem.ForeColor))
                        {
                            Rectangle ParentBounds = MM_OneLine_Element.ShiftRectangle(Elem.Bounds, pnlElements.AutoScrollPosition);
                            e.Graphics.DrawString(Elem.Text, Elem.Font, DrawBrush, ParentBounds.Location);
                        }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Draw an arrow between two points
        /// </summary>
        /// <param name="SourceRect">The starting rectangle</param>
        /// <param name="TargetRect">The target rectangle</param>
        /// <param name="g">The graphics connection</param>
        /// <param name="ArrowEnd">Whether an arrow should appear at the end</param>
        private void DrawArrow(Rectangle SourceRect, Rectangle TargetRect, Graphics g, bool ArrowEnd)
        {
            Point StartPoint = new Point(SourceRect.X + (SourceRect.Width / 2), SourceRect.Y + (SourceRect.Height / 2));
            Point EndPoint = new Point(TargetRect.X + (TargetRect.Width / 2), TargetRect.Y + (TargetRect.Height / 2));

            if (TargetRect.Top > SourceRect.Bottom)
                EndPoint.Y = TargetRect.Top;
            else if (SourceRect.Top > TargetRect.Bottom)
                EndPoint.Y = TargetRect.Bottom;
            if (TargetRect.Left > SourceRect.Right)
                EndPoint.X = TargetRect.Left;
            else if (SourceRect.Left > TargetRect.Right)
                EndPoint.X = TargetRect.Right;

            if (SourceRect.Top > TargetRect.Bottom)
                StartPoint.Y = SourceRect.Top;
            else if (TargetRect.Top > SourceRect.Bottom)
                StartPoint.Y = SourceRect.Bottom;
            if (SourceRect.Left > TargetRect.Right)
                StartPoint.X = SourceRect.Left;
            else if (TargetRect.Left > SourceRect.Right)
                StartPoint.X = SourceRect.Right;

            using (Pen OutPen = new Pen(Color.DarkGray))
            {
                OutPen.StartCap = (ArrowEnd ? LineCap.Round : LineCap.ArrowAnchor);
                OutPen.EndCap = LineCap.ArrowAnchor;
                OutPen.MiterLimit = 5;
                OutPen.LineJoin = LineJoin.Round;


                g.DrawLine(OutPen, StartPoint, EndPoint);
            }
        }



        /// <summary>
        /// Repaint a one-line item
        /// </summary>
        public void RefreshOneLineItem(MM_Element ElementToRepaint)
        {
            try
            {
                MM_OneLine_Element Elem;
                if (ElementToRepaint == null)
                    return;
                else if (!DisplayElements.TryGetValue(ElementToRepaint, out Elem))
                    Program.LogError("Can't find " + ElementToRepaint.ToString() + " in one-line display.");
                else
                {
                    Elem.Refresh();
                    if (Elem.Descriptor != null)
                        Elem.Descriptor.Refresh();
                    if (Elem.SecondaryDescriptor != null)
                        Elem.SecondaryDescriptor.Refresh();
                }
            }
            catch (Exception ex)
            { Program.LogError(ex); }
        }

        /// <summary>
        /// Handle the clicking of an item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsMain_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if ((e.ClickedItem is ToolStripButton) && e.ClickedItem.DisplayStyle == ToolStripItemDisplayStyle.Text)
            {
                (e.ClickedItem as ToolStripButton).Checked = !(e.ClickedItem as ToolStripButton).Checked;
                pnlElements.Invalidate();
            }
        }

        #endregion


        #region Data retrieval

        private delegate void SafeUpdateOneLine();
        /// <summary>
        /// Handle the updating of a one line's data.
        /// </summary>
        private void UpdateOneLine()
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new SafeUpdateOneLine(UpdateOneLine));
            else
            {
                if (OneLineDataUpdated != null)
                    OneLineDataUpdated(BaseData.Data);
                /* foreach (Control c in pnlElements.Controls)
                     if (c is MM_OneLine_Element)
                     {
                         MM_OneLine_Element Elem = c as MM_OneLine_Element;
                         if (Elem.ElemType == MM_OneLine_Element.enumElemTypes.Descriptor || Elem.ElemType == MM_OneLine_Element.enumElemTypes.SecondaryDescriptor)
                             Elem.Text = Elem.DescriptorText;
                         else
                             Elem.Refresh();
                     }
                 this.Invalidate();*/
                this.Refresh();
            }
        }
        #endregion





        /// <summary>
        /// Handle the selection of the data source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDataSource_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            btnDataSource.Text = e.ClickedItem.Text;
            this.DataSource = e.ClickedItem.Tag as MM_Data_Source;
            /*
            //Now, re-run our query
            if (Data_Integration.DataConnections.Count > 0)
                foreach (MM_Query_Configuration qConfig in OneLineConfiguration)
                    Data_Integration.Query(qConfig, DataSource);
            */
            //Refresh our controls
            this.Refresh();
        }

        /// <summary>
        /// Report the data source application
        /// </summary>
        public String DataSourceApplication
        {
            get { return btnDataSource.Text; }
        }

        /// <summary>
        /// Locate a poke point at a specified location. If not found, create it.
        /// </summary>
        /// <param name="Bounds"></param>
        /// <param name="BaseNode"></param>
        /// <param name="Elem"></param>
        /// <param name="Orientation"></param>
        /// <param name="IsJumper"></param>
        /// <param name="IsVisible"></param>
        /// <returns></returns>                
        public MM_OneLine_PokePoint LocatePoke(Rectangle Bounds, MM_OneLine_Node BaseNode, MM_Element Elem, MM_OneLine_Element.enumOrientations Orientation, bool IsJumper, bool IsVisible)
        {
            Rectangle NewBounds = new Rectangle(Bounds.X + pnlElements.AutoScrollPosition.X, Bounds.Y + pnlElements.AutoScrollPosition.Y, Bounds.Width, Bounds.Height);
            if (PokePoints.ContainsKey(NewBounds.Location))
                return PokePoints[NewBounds.Location];
            else
            {
                MM_OneLine_PokePoint NewPoint = new MM_OneLine_PokePoint(Elem, BaseNode, Orientation, IsJumper, IsVisible, ViolViewer, this);
                NewPoint.Bounds = NewBounds;
                PokePoints.Add(NewBounds.Location, NewPoint);
                NewPoint.BringToFront();
                return NewPoint;
            }
        }

        /// <summary>
        /// Handle the drop-down item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayDropDown_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
                (sender as ToolStripMenuItem).Checked ^= true;
            //else if (sender is ToolStripButton)
              //  (sender as ToolStripButton).Checked ^= true;


            this.Refresh();
        }

        /// <summary>
        /// Swap the position of the slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSwapDetails_Click(object sender, EventArgs e)
        {
            if (this.Parent is SplitterPanel)
                (this.Parent.Parent as SplitContainer).Orientation = (this.Parent.Parent as SplitContainer).Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
        }

        /// <summary>
        /// The set of value updates for processing
        /// </summary>
        /// <param name="InMessages"></param>
        public void ProcessValueUpdates(byte[] InMessages)
        {
            //Wait to make sure our oneline is load
            while (!OneLineLoaded)
            {
                Thread.Sleep(250);
                Application.DoEvents();
            }

            ////Handle all the messages that are received for this one-line
            //int CurPos = 0;
            //while (CurPos < InMessages.Length)
            //{
            //    TCP_Message.enumMacomberMapCodes Command = (TCP_Message.enumMacomberMapCodes)InMessages[CurPos];
            //    byte[] MessageLengthBytes = new byte[4];
            //    Array.Copy(InMessages, CurPos + 1, MessageLengthBytes, 0, 4);
            //    int MessageLength = BitConverter.ToInt32(MessageLengthBytes, 0);
            //    byte[] OutgoingMessage = new byte[MessageLength];
            //    Array.Copy(InMessages, CurPos + 5, OutgoingMessage, 0, MessageLength);
            //    TCP_Message NewMessage = new TCP_Message(Command, MessageLength, OutgoingMessage);

            //    //Process our message
            //    int TEID = BitConverter.ToInt32(NewMessage.IncomingData[0], 0);
            //    MM_OneLine_TableLocator tLoc = MM_Integration.MasterLocators[(byte)NewMessage.IncomingData[0][4]];
            //    String AppName = "Telemetered,Real-Time,Real-Time,Study".Split(',')[NewMessage.IncomingData[0][5]];
            //    String TestType = "";
            //    if (NewMessage.IncomingData[0].Length == 8)
            //        TestType = ",MW,MVAR,MVA,KV".Split(',')[NewMessage.IncomingData[0][6]];
            //    int Index = NewMessage.IncomingData[0][NewMessage.IncomingData[0].Length - 1];

            //    for (int a = 1; a < NewMessage.IncomingData.Length; a += 2)
            //    {
            //        MM_OneLine_RecordLocator rLoc = tLoc.DatabaseLinkages[(int)NewMessage.IncomingData[a][0]];
            //        Object InValue = rLoc.GetValue(NewMessage.IncomingData[a + 1]);

            //        //Now, build our column header
                  
            //        String rLocName = rLoc.Name == "{Measurement}" ? TestType : rLoc.Name;
                    
            //        MM_OneLine_Element FoundElem; 
            //        DataColumn FoundCol;
            //        DataRow FoundRow;
            //        if (ElementsByTEID.TryGetValue(TEID, out FoundElem))
            //        {
            //            FoundCol = FoundElem.BaseRow.Table.Columns[AppName + "\\" + rLocName];
            //            if (FoundCol == null)
            //                FoundCol = FoundElem.BaseRow.Table.Columns[AppName + "\\" + rLocName + (Index == 0 || Index == 2 ? "_1" : "_2")];
                        
            //            FoundRow = FoundElem.BaseRow;
            //        }
            //        else
            //        {
            //            MM_OneLine_TransformerWinding FoundWinding = WindingsByTEID[TEID];
            //            FoundCol = FoundWinding.BaseRow.Table.Columns[AppName + "\\" + rLocName];
            //            if (FoundCol == null)
            //                FoundCol = FoundWinding.BaseRow.Table.Columns[AppName + "\\" + rLocName + (Index == 0 || Index == 2 ? "_1" : "_2")];
            //            FoundRow = FoundWinding.BaseRow;

            //        }
            //        if (FoundCol == null)
            //            Console.WriteLine("Unable to find row for " + AppName + "\\" + rLocName + " for " + FoundElem.ToString());
            //        else
            //            FoundRow[FoundCol] = InValue;
            //    }
            //    CurPos += 5 + MessageLength;
            //}


            //Refresh our display
            RefreshDisplay();

            OneLineUpdated = true;



        }

        private delegate void SafeRefresh();

        /// <summary>
        /// Refresh the display
        /// </summary>
        private void RefreshDisplay()
        {
            if (InvokeRequired)
                Invoke(new SafeRefresh(RefreshDisplay));
            else
                Refresh();
        }

        /// <summary>
        /// Update our display threshold
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dToolStripMenuItem_TextChanged(object sender, EventArgs e)
        {
        float Diff;
            if (float.TryParse(dToolStripMenuItem.Text, out Diff))
                analogThreshold=Diff/100f;
        }

        /// <summary>
        /// When our zoom update changes, invalidate our panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlElements.Refresh();
        }
    }
}