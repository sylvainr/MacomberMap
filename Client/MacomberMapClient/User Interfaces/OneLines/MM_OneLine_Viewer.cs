using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacomberMapClient.User_Interfaces.Violations;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.User_Interfaces.NetworkMap;
using MacomberMapClient.User_Interfaces.Summary;
using MacomberMapClient.Integration;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Data_Elements.SystemInformation;
using System.Drawing.Drawing2D;
using MacomberMapCommunications.Messages.EMS;
using System.Xml;
using MacomberMapClient.User_Interfaces.Communications;

namespace MacomberMapClient.User_Interfaces.OneLines
{
    /// <summary>
    /// This class provides the one-line viewer
    /// </summary>
    public partial class MM_OneLine_Viewer : UserControl
    {        
        #region Variable declarations
        /// <summary>Our flag to track one-line loading</summary>
        private bool OneLineIsLoading = false;

        /// <summary>Our analog threshold for highlighting differences</summary>
        public float AnalogThreshold = .1f;

        /// <summary>Our zoom level</summary>
        private float ZoomLevel = 1f;

        /// <summary>The right/bottom most points on the one-line</summary>
        private int MaxX = 0, MaxY = 0;

        /// <summary>The GUID of the current one-line</summary>
        private Guid OneLineGuid = Guid.Empty;

        /// <summary>The element we're highlighting</summary>
        private MM_OneLine_Element HighlightedElement = null;

        /// <summary>Our base data</summary>
        private MM_DataSet_Base BaseData;

        /// <summary>Our event handler for a new one-line being loaded</summary>
        public event EventHandler OneLineLoadCompleted;

        /// <summary>Our collection of unlinked elements</summary>
        private List<MM_OneLine_Element> UnlinkedElements = new List<MM_OneLine_Element>();

        /// <summary>The date/time of CIM export</summary>
        public DateTime ExportDate;

        /// <summary>The element at the seed of the one-line</summary>
        public MM_Element BaseElement;

        /// <summary>The network map from which the one-line was spooled up</summary>
        public MM_Network_Map_DX nMap;

        /// <summary>The collection of one-line elements</summary>
        public Dictionary<MM_Element, MM_OneLine_Element> DisplayElements = new Dictionary<MM_Element, MM_OneLine_Element>();

        /// <summary>The collection of one-line nodes</summary>
        public Dictionary<MM_Element, MM_OneLine_Node> DisplayNodes = new Dictionary<MM_Element, MM_OneLine_Node>();

        /// <summary>The collection of descriptors</summary>
        public Dictionary<MM_Element, MM_OneLine_Element> Descriptors = new Dictionary<MM_Element, MM_OneLine_Element>();

        /// <summary>The collection of secondary descriptors</summary>
        public Dictionary<MM_Element, MM_OneLine_Element> SecondaryDescriptors = new Dictionary<MM_Element, MM_OneLine_Element>();

        /// <summary>Our collection of elements by TEID</summary>
        public Dictionary<int, MM_OneLine_Element> ElementsByTEID = new Dictionary<int, MM_OneLine_Element>();

        /// <summary>Our collection of windings by TEID</summary>
        public Dictionary<int, MM_OneLine_TransformerWinding> WindingsByTEID = new Dictionary<int, MM_OneLine_TransformerWinding>();

        /// <summary>Our list of all display components</summary>
        private List<MM_OneLine_Element> DisplayComponents = new List<MM_OneLine_Element>();

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
        public MM_Violation_Viewer ViolViewer;

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

        /// <summary>The application that drives our data</summary>
        public string DataSourceApplication;

        /// <summary>Whether we should choose worst violation colors</summary>
        public bool IsFlashing = false;

        /// <summary>Our collection of value changes</summary>
        public Dictionary<MM_Element, Object> ValueChanges = new Dictionary<MM_Element, object>();
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize an empty one-line viewer
        /// </summary>
        public MM_OneLine_Viewer()
        {
            InitializeComponent();
           // this.DoubleBuffered = true;
           // this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            
        }

        /// <summary>
        /// Initialize a new 
        /// </summary>
        public class DoubleBufferedPanel : Panel
        {
            /// <summary>
            /// Creates an instance.
            /// </summary>
            public DoubleBufferedPanel():base()
            {
                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            }
        }
  

        /// <summary>
        /// Initialize the one-line viewer
        /// </summary>
        /// <param name="AutoFollow">Whether to automatically follow the selections</param>
        public MM_OneLine_Viewer(bool AutoFollow):this()
        {            
            pnlElements.MouseWheel += pnlElements_MouseWheel;
            foreach (string DataSrc in new string[] { "Real-Time", "Telemetered", "Study" })
                this.btnDataSource.DropDownItems.Add(DataSrc);
            this.btnDataSource.Text = "Real-Time";
            Data_Integration.ViolationAcknowledged += new Data_Integration.ViolationChangeDelegate(UpdateViolation);
            Data_Integration.ViolationAdded += new Data_Integration.ViolationChangeDelegate(UpdateViolation);
            Data_Integration.ViolationModified += new Data_Integration.ViolationChangeDelegate(UpdateViolation);
            Data_Integration.ViolationRemoved += new Data_Integration.ViolationChangeDelegate(UpdateViolation);
            //Double-buffer our display
            this.DoubleBuffered = true;
           // pnlElements.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pnlElements, true, null);
            btnFollowMap.Checked = AutoFollow;
            ZoomLevel = 1f;
            txtZoomLevel.Text = "100";

        }

        /// <summary>
        /// Assign the controls to the one-line viewer
        /// </summary>
        /// <param name="BaseElement">The base element, from which the Data Integration layer should query for parameters</param>
        /// <param name="nMap">The network map</param>
        /// <param name="BaseData">The base data</param>
        /// <param name="ViolViewer">The violation viewer associated with the one-line</param>
        /// <param name="DataSource">The active data source</param>
        public void SetControls(MM_Element BaseElement, MM_Network_Map_DX nMap, MM_DataSet_Base BaseData, MM_Violation_Viewer ViolViewer, MM_Data_Source DataSource)
        {
            //Assign our data connectors
            this.DataSource = DataSource;
            this.BaseData = BaseData;
            this.nMap = nMap;
            this.ViolViewer = ViolViewer;

            LoadOneLine(BaseElement, null);

            //Assign our events
            tsMain.ItemClicked += new ToolStripItemClickedEventHandler(tsMain_ItemClicked);
            btnSwapDetails.Visible = this.Parent is SplitterPanel;

        }

        /// <summary>
        /// Load a one-line into memory
        /// </summary>
        /// <param name="BaseElement">The base element to be highlighting</param>
        /// <param name="HighlightElem">The element to highlight (e.g., line from the other substation)</param>
        public void LoadOneLine(MM_Element BaseElement, MM_Element HighlightElem)
        {
            OneLineIsLoading = true;
            if (!cmbSubstation.Items.Contains(BaseElement))            
                cmbSubstation.Items.Add(BaseElement.Name);
            cmbSubstation.SelectedIndex = cmbSubstation.Items.IndexOf(BaseElement.Name);

            //First, clear all of our elements
            ShutDownQuery();
            DisplayElements.Clear();
            DisplayNodes.Clear();
            UnlinkedElements.Clear();
            Descriptors.Clear();
            SecondaryDescriptors.Clear();
            pnlElements.Controls.Clear();
            DisplayComponents.Clear();
            this.BaseElement = BaseElement;
            ElementsByTEID.Clear();

            //Now, load our one-line
            MM_OneLine_Data OLData = MM_Server_Interface.LoadOneLine(BaseElement);

            if (OLData.OneLineXml == null)
            {
                MessageBox.Show("The one line for " + BaseElement.ToString() + " could not be found.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Now, parse our elements
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(OLData.OneLineXml);


            //Load in our elements
            if (xDoc.DocumentElement["Elements"] != null)
                foreach (XmlElement xElem in xDoc.DocumentElement["Elements"])
                {
                    MM_OneLine_Element Elem = new MM_OneLine_Element(xElem);
                    if (Elem.Descriptor != null)
                    {
                        DisplayComponents.Add(Elem.Descriptor);
                        Descriptors.Add(Elem.BaseElement, Elem.Descriptor);
                    }
                    if (Elem.SecondaryDescriptor != null)
                    {
                        DisplayComponents.Add(Elem.SecondaryDescriptor);
                        SecondaryDescriptors.Add(Elem.BaseElement, Elem.SecondaryDescriptor);
                    }
                    DisplayComponents.Add(Elem);
                    ElementsByTEID.Add(Elem.BaseElement.TEID, Elem);
                    if (Elem.BaseElement is MM_Tie)
                        ElementsByTEID.Add(((MM_Tie)Elem.BaseElement).AssociatedLine.TEID, Elem);
                    DisplayElements.Add(Elem.BaseElement, Elem);
                    Elem.BaseElement.ValuesChanged += ElemValueChange;
                }

            //Load in our nodes
            if (xDoc.DocumentElement["Nodes"] != null)
                foreach (XmlElement xElem in xDoc.DocumentElement["Nodes"])
                {
                    MM_OneLine_Node Node = new MM_OneLine_Node(xElem, ElementsByTEID);
                    if (Node.Descriptor != null)
                    {
                        DisplayComponents.Add(Node.Descriptor);
                        Descriptors.Add(Node.BaseElement, Node.Descriptor);
                    }
                    if (Node.SecondaryDescriptor != null)
                    {
                        SecondaryDescriptors.Add(Node.BaseElement, Node.SecondaryDescriptor);
                        DisplayComponents.Add(Node.SecondaryDescriptor);
                    }
                    ElementsByTEID.Add(Node.BaseElement.TEID, Node);
                    DisplayComponents.Add(Node);
                    DisplayNodes.Add(Node.BaseElement, Node);
                    Node.BaseElement.ValuesChanged += ElemValueChange;
                    foreach (MM_OneLine_Node[] Pokes in Node.NodeTargets.Values)
                        foreach (MM_OneLine_Node Poke in Pokes)
                        {
                            DisplayComponents.Add(Poke);
                            if (Poke.ElemType == MM_OneLine_Element.enumElemTypes.PricingVector || Poke.IsJumper)
                                UnlinkedElements.Add(Poke);
                            if (Poke.Descriptor != null)
                            {
                                DisplayComponents.Add(Poke.Descriptor);
                                Descriptors.Add(Poke.BaseElement, Poke.Descriptor);
                            }
                            if (Poke.SecondaryDescriptor != null)
                            {
                                SecondaryDescriptors.Add(Poke.BaseElement, Poke.SecondaryDescriptor);
                                DisplayComponents.Add(Poke.SecondaryDescriptor);
                            }
                        }

                }

            if (xDoc.DocumentElement["Unlinked_Elements"] != null)
                foreach (XmlElement xElem in xDoc.DocumentElement["Unlinked_Elements"].ChildNodes)
                {
                    MM_OneLine_Element UnlinkedElement = new MM_OneLine_Element(xElem);
                    DisplayComponents.Add(UnlinkedElement);
                    UnlinkedElements.Add(UnlinkedElement);
                }
            HighlightElement(HighlightElem);

            //Track our positions for our scroller, and create a new pseudo-element in the bottom right to make scrolling work
            MaxX = 0;
            MaxY = 0;

            foreach (MM_OneLine_Element Elem in DisplayComponents)
            {
                MaxX = Math.Max(MaxX, Elem.Bounds.Right);
                MaxY = Math.Max(MaxY, Elem.Bounds.Bottom);
            }

            HighlightElement(HighlightElem);

            //If we have an element, let's show it
            UpdateZoom();
            if (HighlightedElement != null)
                using (Control ctl = new Control())
                {
                    Rectangle TargetBounds = HighlightedElement.Bounds;
                    if (HighlightedElement.Descriptor != null)
                        TargetBounds = Rectangle.Union(TargetBounds, HighlightedElement.Descriptor.Bounds);
                    if (HighlightedElement.SecondaryDescriptor != null)
                        TargetBounds = Rectangle.Union(TargetBounds, HighlightedElement.SecondaryDescriptor.Bounds);
                    ctl.Bounds = TargetBounds;
                    pnlElements.Controls.Add(ctl);
                    pnlElements.ScrollControlIntoView(ctl);
                    pnlElements.Controls.Remove(ctl);
                }


            if (OneLineLoadCompleted != null)
                OneLineLoadCompleted(this, EventArgs.Empty);
            tmrUpdate.Enabled = true;
            pnlElements.Refresh();
            OneLineIsLoading = false;
        }
        #endregion

        /// <summary>
        /// Report all the node targets for an element
        /// </summary>
        /// <param name="Elem"></param>
        public MM_Element[] ElementNodes(MM_OneLine_Element Elem)
        {
            List<MM_Element> FoundElems = new List<MM_Element>();
            foreach (KeyValuePair<MM_Element, MM_OneLine_Node> Node in DisplayNodes)
                if (Node.Value.NodeTargets.ContainsKey(Elem))
                    FoundElems.Add(Node.Key);
            return FoundElems.ToArray();
        }


        /// <summary>
        /// Highlight a one-line element
        /// </summary>
        /// <param name="HighlightElem"></param>
        public void HighlightElement(MM_Element HighlightElem)
        {
            if (HighlightElem == null)
                HighlightedElement = null;
            else if (!ElementsByTEID.TryGetValue(HighlightElem.TEID, out HighlightedElement))
                HighlightedElement = null;
            else if (!Rectangle.FromLTRB(-pnlElements.AutoScrollPosition.X, -pnlElements.AutoScrollPosition.Y, pnlElements.DisplayRectangle.Width, pnlElements.DisplayRectangle.Height).IntersectsWith(HighlightedElement.Bounds))
            {
                Control ctl = new Control() { Bounds = HighlightedElement.Bounds };
                pnlElements.Controls.Add(ctl);
                pnlElements.ScrollControlIntoView(ctl);
                pnlElements.Controls.Remove(ctl);
            }
        }

        /// <summary>
        /// Shut down our query 
        /// </summary>
        public void ShutDownQuery()
        {
            //Unhook updates to all of our elements
            foreach (MM_Element Elem in DisplayElements.Keys)
                Elem.ValuesChanged -= ElemValueChange;
            foreach (MM_Element Node in DisplayNodes.Keys)
                Node.ValuesChanged -= ElemValueChange;
        }

        /// <summary>
        /// Handle an element value change
        /// </summary>
        /// <param name="Element"></param>
        /// <param name="Property"></param>
        private void ElemValueChange(MM_Element Element, string Property)
        {
            ValueChanges.Remove(Element);
           /* MM_OneLine_Element FoundElem;
            if (DisplayElements.TryGetValue(Element, out FoundElem))
            {
                pnlElements.Invalidate(FoundElem.Bounds);
                if (FoundElem.Descriptor != null)
                    pnlElements.Invalidate(FoundElem.Descriptor.Bounds);
            }

            MM_OneLine_Node FoundNode;
            if (DisplayNodes.TryGetValue(Element, out FoundNode))
            {
                if (FoundNode.Descriptor != null)
                    pnlElements.Invalidate(FoundNode.Descriptor.Bounds);
                foreach (MM_OneLine_Node[] NodeTargets in FoundNode.NodeTargets.Values)
                    foreach (MM_OneLine_Node NodeTarget in NodeTargets)
                        if (NodeTarget.Descriptor != null)
                            pnlElements.Invalidate(NodeTarget.Descriptor.Bounds);
            }*/
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
                try
                {
                    Invoke(new SafeUpdateViolation(UpdateViolation), Violation);
                }
                catch { }
            else if (Violation.ViolatedElement is MM_Node)
            {
                MM_OneLine_Node FoundNode;
                if (DisplayNodes.TryGetValue(Violation.ViolatedElement, out FoundNode))
                    pnlElements.Invalidate(FoundNode.Bounds);
            }
            else
            {
                MM_OneLine_Element FoundElem;
                if (DisplayElements.TryGetValue(Violation.ViolatedElement, out FoundElem))
                    pnlElements.Invalidate(FoundElem.Bounds);
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


        private void btnDataSource_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Handle our panel being painted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PanelPaint(object sender, PaintEventArgs e)
        {
            try
            {
                if (Data_Integration.SimulatorStatus != MM_Simulation_Time.enumSimulationStatus.Running && (DateTime.Now.Second % 2)==0)
                    e.Graphics.Clear(Color.FromArgb(50,0,0));

                //Determine our zoom
                e.Graphics.ScaleTransform(ZoomLevel, ZoomLevel);

                //Draw our header bar
                Rectangle HeaderRect = new Rectangle(pnlElements.DisplayRectangle.Left, pnlElements.DisplayRectangle.Top, pnlElements.DisplayRectangle.Width, 40);
                if (HeaderRect.IntersectsWith(e.ClipRectangle))
                    MM_OneLine_Element.DrawHeader(e.Graphics, HeaderRect, BaseElement);
                
                e.Graphics.TranslateTransform(pnlElements.AutoScrollPosition.X, pnlElements.AutoScrollPosition.Y);

                //Draw our arrows
                if (arrowsToolStripMenuItem.Checked)
                    foreach (MM_OneLine_Element Elem in DisplayElements.Values)
                        if (Elem.DescriptorArrow || (Elem.Descriptor != null && Elem.Descriptor.DescriptorArrow))
                        {
                            if (Elem.ParentElement != null)
                                MM_OneLine_Element.DrawArrow(e.Graphics, Elem.ParentElement.Bounds, Elem.Bounds);
                            if (Elem.Descriptor != null)
                                MM_OneLine_Element.DrawArrow(e.Graphics, Elem.Bounds, Elem.Descriptor.Bounds);
                            if (Elem.SecondaryDescriptor != null)
                                MM_OneLine_Element.DrawArrow(e.Graphics, Elem.Bounds, Elem.SecondaryDescriptor.Bounds);
                        }

                //Write out our node paths          
                foreach (MM_OneLine_Node Node in DisplayNodes.Values)
                    if (Node.NodePaths != null)
                    {
                        Node.DetermineAssociatedBus();
                        if (mnuNodeToElementLines.Checked)
                            foreach (KeyValuePair<MM_OneLine_Element, GraphicsPath> gpNode in Node.NodePaths)
                            {
                                MM_AlarmViolation_Type WorstViol = null;
                                if (IsFlashing && MM_Repository.OverallDisplay.NodeToElementViolations)
                                {
                                    foreach (MM_AlarmViolation Viol in gpNode.Key.BaseElement.Violations.Values)
                                        if (WorstViol == null || Viol.Type.Priority < WorstViol.Priority)
                                            WorstViol = Viol.Type;
                                    MM_Bus NearBus = gpNode.Key.BaseElement.NearBus;
                                    MM_Bus FarBus = gpNode.Key.BaseElement.FarBus;
                                    if (NearBus != null)
                                        foreach (MM_AlarmViolation Viol in NearBus.Violations.Values)
                                            if (WorstViol == null || Viol.Type.Priority < WorstViol.Priority)
                                                WorstViol = Viol.Type;
                                    if (FarBus != null)
                                        foreach (MM_AlarmViolation Viol in FarBus.Violations.Values)
                                            if (WorstViol == null || Viol.Type.Priority < WorstViol.Priority)
                                                WorstViol = Viol.Type;
                                }
                                Color DrawColor = WorstViol == null ? Node.KVLevel.Energized.ForeColor : WorstViol.ForeColor;
                                using (Pen DrawPen = new Pen(DrawColor))
                                    e.Graphics.DrawPath(DrawPen, gpNode.Value);
                            }
                    }

                //Write out our elements, nodes, and unlinked elements
                foreach (MM_OneLine_Element Elem in DisplayElements.Values)
                    // if (e.ClipRectangle.IntersectsWith(Elem.Bounds))
                    Elem.PaintElement(e.Graphics, this, IsFlashing);
                foreach (MM_OneLine_Node Node in DisplayNodes.Values)
                    // if (e.ClipRectangle.IntersectsWith(Node.Bounds))
                    Node.PaintElement(e.Graphics, this, IsFlashing);
                foreach (MM_OneLine_Element UnlinkedElem in UnlinkedElements)
                    // if (e.ClipRectangle.IntersectsWith(UnlinkedElem.Bounds))
                    UnlinkedElem.PaintElement(e.Graphics, this, IsFlashing);

                //Write out our descriptors and secondary descriptors
                foreach (MM_OneLine_Element Elem in Descriptors.Values)
                    //  if (e.ClipRectangle.IntersectsWith(Elem.Bounds))
                    Elem.PaintElement(e.Graphics, this, IsFlashing);
                foreach (MM_OneLine_Element Elem in SecondaryDescriptors.Values)
                    //if (e.ClipRectangle.IntersectsWith(Elem.Bounds))
                    Elem.PaintElement(e.Graphics, this, IsFlashing);

                //If we have a highlighted element, let's make it visible
                int HighlightRadius = 20;
                if (HighlightedElement != null)
                    using (Pen HighlightPen = new Pen(Color.White, 3) { DashStyle = DashStyle.Custom, DashPattern = IsFlashing ? new float[] { 1, 2, 3 } : new float[] { 3, 2, 1 } })
                        e.Graphics.DrawEllipse(HighlightPen, HighlightedElement.Bounds.Left - HighlightRadius, HighlightedElement.Bounds.Top - HighlightRadius, HighlightedElement.Bounds.Width + HighlightRadius + HighlightRadius, HighlightedElement.Bounds.Height + HighlightRadius + HighlightRadius);

                //Highlight any elements with pending changes
                MM_OneLine_Element FoundElem;
                if (ValueChanges.Count > 0)
                    using (Pen HighlightPen = new Pen(Color.White, 3))
                        foreach (MM_Element Elem in ValueChanges.Keys.ToArray())
                            if (DisplayElements.TryGetValue(Elem, out FoundElem))
                            {
                                e.Graphics.DrawRectangle(HighlightPen, FoundElem.Bounds.Left - HighlightRadius, FoundElem.Bounds.Top - HighlightRadius, FoundElem.Bounds.Width + HighlightRadius + HighlightRadius, FoundElem.Bounds.Height + HighlightRadius + HighlightRadius);
                                e.Graphics.DrawLine(HighlightPen, FoundElem.Bounds.Left - HighlightRadius, FoundElem.Bounds.Top - HighlightRadius, FoundElem.Bounds.Left, FoundElem.Bounds.Top);
                                e.Graphics.DrawLine(HighlightPen, FoundElem.Bounds.Right + HighlightRadius, FoundElem.Bounds.Top - HighlightRadius, FoundElem.Bounds.Right, FoundElem.Bounds.Top);
                                e.Graphics.DrawLine(HighlightPen, FoundElem.Bounds.Left - HighlightRadius, FoundElem.Bounds.Bottom + HighlightRadius, FoundElem.Bounds.Left, FoundElem.Bounds.Bottom);
                                e.Graphics.DrawLine(HighlightPen, FoundElem.Bounds.Right + HighlightRadius, FoundElem.Bounds.Bottom + HighlightRadius, FoundElem.Bounds.Right, FoundElem.Bounds.Bottom);
                            }
                e.Graphics.TranslateTransform(-pnlElements.AutoScrollPosition.X, -pnlElements.AutoScrollPosition.Y);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Update our zoom levels
        /// </summary>
        private void UpdateZoom()
        {
            if (btnFitToPage.Checked)
                ZoomLevel = Math.Min((float)pnlElements.Width / (float)MaxX, (float)pnlElements.Height / (float)MaxY);
            else if (float.TryParse(txtZoomLevel.Text, out ZoomLevel))
                ZoomLevel /= 100f;

            //Add our invisible control to fix our arrows
            pnlElements.Controls.Clear();
            pnlElements.Controls.Add(new Control() { Bounds = new Rectangle((int)((MaxX + 50) * ZoomLevel), (int)((MaxY + 50) * ZoomLevel), 1, 1), BackColor = Color.Black });
            this.Invalidate();
            this.Refresh();
        }


        /// <summary>
        /// Swap the property page left/right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSwapDetails_Click(object sender, System.EventArgs e)
        {
            if (this.Parent is SplitterPanel)
                (this.Parent.Parent as SplitContainer).Orientation = (this.Parent.Parent as SplitContainer).Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
        }

        /// <summary>
        /// When our mouse is pressed, find and highlight our element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnlElements_MouseDown(object sender, MouseEventArgs e)
        {
            pnlElements.Focus();

            //If we have a middle button down, start to display our cursor
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                pnlElements.Cursor = Cursors.SizeAll;
                pnlElements.Capture = true;
                pnlElements.Tag = e.Location;
                return;
            }
            //Find our element, and highlight it
            foreach (MM_OneLine_Element Elem in DisplayComponents)
                if (Elem.BaseElement != null && Elem.Bounds.Contains((int)((e.X / ZoomLevel)-pnlElements.AutoScrollPosition.X), (int)((e.Y / ZoomLevel)- pnlElements.AutoScrollPosition.Y)))
                {
                    MM_OneLine_Element SelectedElement;
                    if (Elem.ElemType == MM_OneLine_Element.enumElemTypes.Descriptor || Elem.ElemType == MM_OneLine_Element.enumElemTypes.SecondaryDescriptor)
                        SelectedElement = HighlightedElement = Elem.ParentElement;
                    else
                        SelectedElement = HighlightedElement = Elem;
                    Refresh();
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                        new User_Interfaces.Menuing.MM_Popup_Menu().Show(this, e.Location, SelectedElement.BaseElement, true);
                    else if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        if (OneLineElementClicked != null && (Control.ModifierKeys & Keys.Control) == Keys.Control)
                            OneLineElementClicked(SelectedElement, e);
                        if (btnFollowMap.Checked)
                            if (Elem.ElemType == MM_OneLine_Element.enumElemTypes.Line)
                            {
                                nMap.Coordinates.Center = ((Elem.BaseElement as MM_Line).Midpoint);
                                nMap.Coordinates.UpdateZoom(19);
                            }
                            else if (Elem.BaseElement.Substation != null)
                            {
                                nMap.Coordinates.Center = (((Elem.BaseElement.Substation).LngLat));
                                nMap.Coordinates.UpdateZoom(19);
                            }
                    }
                    return;
                }
            HighlightedElement = null;
            if (OneLineElementClicked != null)
                OneLineElementClicked(null, e);
        }

        /// <summary>
        /// When our mouse moves, offer our panning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnlElements_MouseMove(object sender, MouseEventArgs e)
        {
            if (pnlElements.Capture && e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                Point Tag = (Point)pnlElements.Tag;
                Point DeltaPt = new Point(e.X - Tag.X, e.Y - Tag.Y);
                pnlElements.AutoScrollPosition = new Point(-pnlElements.AutoScrollPosition.X + DeltaPt.X / 8, -pnlElements.AutoScrollPosition.Y + DeltaPt.Y / 8);
                pnlElements.Refresh();
            }
        }

        /// <summary>
        /// When our mouse wheel moves, update our zoom parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pnlElements_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!btnFitToPage.Checked && e.Button == System.Windows.Forms.MouseButtons.None && pnlElements.Cursor == Cursors.Default && (Control.ModifierKeys & Keys.Control) == Keys.Control)
                HandleZoomButton(e.Delta > 0 ? btnZoomUp : btnZoomDown, EventArgs.Empty);

        }

        /// <summary>
        /// Handle our mouse up event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnlElements_MouseUp(object sender, MouseEventArgs e)
        {
            //Restore our cursor
            pnlElements.Cursor = Cursors.Default;
            pnlElements.Capture = false;
        }

        /// <summary>
        /// Handle a mouse double-click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnlElements_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Find our element
            foreach (MM_OneLine_Element Elem in DisplayComponents)
                if (Elem.BaseElement != null && Elem.Bounds.Contains((int)((e.X / ZoomLevel) - pnlElements.AutoScrollPosition.X), (int)((e.Y / ZoomLevel) - pnlElements.AutoScrollPosition.Y)))
                {
                    MM_OneLine_Element SelectedElement = null;
                    if (Elem.ElemType == MM_OneLine_Element.enumElemTypes.Descriptor || Elem.ElemType == MM_OneLine_Element.enumElemTypes.SecondaryDescriptor)
                        SelectedElement = Elem.ParentElement;
                    else
                        SelectedElement = Elem;
                    if (SelectedElement.ElemType == MM_OneLine_Element.enumElemTypes.Line && BaseElement is MM_Substation)
                    {
                        MM_Line Line = (MM_Line)SelectedElement.BaseElement;
                        if (Line.Substation1 == BaseElement)
                            LoadOneLine(Line.Substation2, Line);
                        else
                            LoadOneLine(Line.Substation1, Line);
                    }
                    else if (SelectedElement.ElemType == MM_OneLine_Element.enumElemTypes.Node)
                        if (BaseElement is MM_Substation)
                            LoadOneLine(SelectedElement.Contingencies[0], SelectedElement.BaseElement);
                        else
                            LoadOneLine(SelectedElement.BaseElement.Substation, SelectedElement.BaseElement);
                    else if (SelectedElement.BaseElement != null && SelectedElement.BaseElement.ElemType.Configuration != null && SelectedElement.BaseElement.ElemType.Configuration["ControlPanel"] != null)
                    {
                        if (MM_Server_Interface.ClientAreas.ContainsKey("ERCOT") || MM_Server_Interface.ClientAreas.ContainsKey(SelectedElement.BaseElement.Operator.Alias))
                            new MM_ControlPanel(Elem, SelectedElement.BaseElement.ElemType.Configuration["ControlPanel"], this) { StartPosition = FormStartPosition.Manual, Location = Cursor.Position }.Show(this);
                        else
                            MessageBox.Show("Control of " + (Elem.BaseElement.Substation == null ? "" : Elem.BaseElement.Substation.LongName) + " " + Elem.BaseElement.ElemType.Name + " " + Elem.BaseElement.Name + " is not permitted.\n(operated by " + Elem.BaseElement.Operator.Name + " - " + Elem.BaseElement.Operator.Alias + ").", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    return;

                }
        }

        /// <summary>
        /// Every second, refresh our screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            IsFlashing ^= true;
            pnlElements.Invalidate();
        }

        /// <summary>
        /// Handle our fit to page button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFitToPage_Click(object sender, EventArgs e)
        {
            btnFitToPage.Checked ^= true;
            txtZoomLevel.ForeColor = btnFitToPage.Checked ? Color.LightGray : Color.Black;
            UpdateZoom();
        }

        /// <summary>
        /// Try and update our analog threshold
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtAnalogThreshold_TextChanged(object sender, EventArgs e)
        {
            float.TryParse(txtAnalogThreshold.Text, out AnalogThreshold);
        }

        /// <summary>
        /// Handle our zoom button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleZoomButton(object sender, EventArgs e)
        {
            //If we're in fit to page mode, turn it off
            btnFitToPage.Checked = false;
            txtZoomLevel.ForeColor = Color.Black;

            //First, try and determine our zoom level
            if (float.TryParse(txtZoomLevel.Text, out ZoomLevel))
                ZoomLevel /= 100f;
            else
                ZoomLevel = 1;

            //Now, adjust our zoom level.
            if (sender == btnZoomUp)
                ZoomLevel += .05f;
            else
                ZoomLevel -= .05f;
            ZoomLevel = (float)Math.Min(Math.Max(0.05, Math.Round(ZoomLevel, 2)), 2);

            txtZoomLevel.Text = (ZoomLevel * 100).ToString();
            UpdateZoom();
        }

        #region Search        
        /// <summary>
        /// When we see a key press, handle accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbSubstation_KeyPress(object sender, KeyPressEventArgs e)
        {
            MM_Substation FoundSub;
            MM_Contingency FoundCtg;
            if (e.KeyChar == '\r' && MM_Repository.Substations.TryGetValue(cmbSubstation.Text, out FoundSub))
                LoadOneLine(FoundSub, null);
            else if (e.KeyChar == '\r' && MM_Repository.Contingencies.TryGetValue(cmbSubstation.Text, out FoundCtg))
                LoadOneLine(FoundCtg, null);
        }

        /// <summary>
        /// Handle our prev/fwd buttons being clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleDirectionalClick(object sender, EventArgs e)
        {
            if (sender == tsBack && cmbSubstation.SelectedIndex > 0)
                cmbSubstation.SelectedIndex--;
            else if (sender == tsForward && cmbSubstation.SelectedIndex < cmbSubstation.Items.Count - 1)
                cmbSubstation.SelectedIndex++;
        }

        /// <summary>
        /// When our index changes, update accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbSubstation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!OneLineIsLoading && cmbSubstation.SelectedItem is String)
                cmbSubstation_KeyPress(cmbSubstation, new KeyPressEventArgs('\r'));
        }
        #endregion

    }
}
