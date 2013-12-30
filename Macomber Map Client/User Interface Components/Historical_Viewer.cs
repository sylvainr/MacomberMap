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
using System.Reflection;
using ZedGraph;
using Macomber_Map.Properties;
using System.Diagnostics;
using Macomber_Map.Data_Connections.Historic;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc.
    /// This form provides a real-time and historical view into data.
    /// </summary>
    public partial class Historical_Viewer : UserControl
    {
        #region Variable declarations
        /// <summary>The types of graphs that can be drawn.</summary>
        public enum GraphModeEnum
        {
            /// <summary>Only show historical data</summary>
            HistoricalOnly,
            /// <summary>Load vs. Load forecast</summary>
            LoadVsLoadForecast,
            /// <summary>Data from RTGEN SystemWide</summary>
            SystemWide,
            /// <summary>Generation Types</summary>
            GenTypes,
            /// <summary>Island display</summary>
            Island
        };

        /// <summary>This is the new container for the graph data</summary>
        /// 
        private DataSet GraphDataRecords = new DataSet("GraphDataRecord"); //http://www.dotnetperls.com/dataset
        
        /// <summary>The number of historical queries associated with this display that have returned</summary>
        private int HistoricQueriesCompleted =  0;

        /// <summary>The number of historical queries associated with this display that have been sent out</summary>
        private int HistoricQueriesTotal = 0;

        /// <summary>Our most recently moused-over point</summary>
        private Double LastMouseOverTime = XDate.DateTimeToXLDate(DateTime.Now);

        /// <summary>The last pane that was used in searching</summary>
        private GraphPane LastPane;

        /// <summary>Our collection of historical points for interpolation.</summary>
        private Dictionary<MM_Historic_DataPoint, PointPairList> HistoricPointList = new Dictionary<MM_Historic_DataPoint, PointPairList>();

        /// <summary>The item's current graph mode</summary>
        public GraphModeEnum GraphMode;

        /// <summary>The delegate to handle the updating of graph data</summary>
        public delegate void UpdateGraphData();

        /// <summary>The mappings between the data and their titles</summary>
        private string[] EMSMappings;

        /// <summary>The date from which the data was last pulled</summary>
        private DateTime LastDate;

        /// <summary>The start time for our query</summary>
        public DateTime StartTime;

        /// <summary>The end time for our query</summary>
        public DateTime EndTime;

        /// <summary>Our collection of appropriate time spans</summary>
        private Dictionary<String, TimeSpan> TimeSpans = new Dictionary<string, TimeSpan>();

        /// <summary>Our background brush for rendering our display</summary>
        private Brush BackBrush = null;
        #endregion

        #region Initialization
        /// <summary>
        /// Set up the historical viewer
        /// </summary>
        /// <param name="GraphMode">Our graphics mode</param>        
        /// <param name="HistoricMappings">The historic mappings</param>
        /// <param name="EMSMappings">The mappings</param>
        public Historical_Viewer(GraphModeEnum GraphMode, string[] HistoricMappings, string[] EMSMappings)
        {
            InitializeComponent();
            btnDetailedView_Click(btnDetailedView, EventArgs.Empty);
            if (!Data_Integration.Permissions.HistoricalSelection)
                btnDetailedView.Visible = false;

            //Also override the background drawing on the toolstrip, to match our background.
            BackBrush = new SolidBrush(this.BackColor);
            tsMain.Renderer.RenderToolStripBackground += new ToolStripRenderEventHandler(Renderer_RenderToolStripBackground);

            //Set up our time span ranges
            TimeSpans.Add("- 1 year", TimeSpan.FromDays(-366));
            TimeSpans.Add("- 6 months", TimeSpan.FromDays(-366 / 2));
            TimeSpans.Add("- 1 month", TimeSpan.FromDays(-31));
            TimeSpans.Add("- 1 week", TimeSpan.FromDays(-7));
            TimeSpans.Add("- 1 day", TimeSpan.FromDays(-1));
            TimeSpans.Add("- 12 hours", TimeSpan.FromHours(-12));
            TimeSpans.Add("- 6 hours", TimeSpan.FromHours(-6));
            TimeSpans.Add("- 1 hour", TimeSpan.FromHours(-1));
            TimeSpans.Add("- 30 minutes", TimeSpan.FromMinutes(-30));
            TimeSpans.Add("- 15 minutes", TimeSpan.FromMinutes(-15));
            TimeSpans.Add("- 1 minutes", TimeSpan.FromMinutes(-1));
            /*TimeSpans.Add("+ 1 minutes", TimeSpan.FromMinutes(+1));
            TimeSpans.Add("+ 15 minutes", TimeSpan.FromMinutes(+15));
            TimeSpans.Add("+ 30 minutes", TimeSpan.FromMinutes(+30));
            TimeSpans.Add("+ 1 hour", TimeSpan.FromHours(+1));
            TimeSpans.Add("+ 6 hours", TimeSpan.FromHours(+6));
            TimeSpans.Add("+ 12 hours", TimeSpan.FromHours(+12));
            TimeSpans.Add("+ 1 day", TimeSpan.FromDays(+1));
            TimeSpans.Add("+ 1 week", TimeSpan.FromDays(+7));
            TimeSpans.Add("+ 1 month", TimeSpan.FromDays(+31));
            TimeSpans.Add("+ 6 months", TimeSpan.FromDays(+366 / 2));
            TimeSpans.Add("+ 1 year", TimeSpan.FromDays(+366));
            */
            this.EMSMappings = EMSMappings;
            this.GraphMode = GraphMode;
            //Cycle the view based on the graph type
            if (this.GraphMode == GraphModeEnum.HistoricalOnly)
            {
                List<Control> ControlsToMove = new List<Control>();
                foreach (Control ctl in tpHistoric.Controls)
                    ControlsToMove.Add(ctl);
                this.Controls.Clear();
                this.Controls.AddRange(ControlsToMove.ToArray());
            }

            //If needed, pull in out historic tags
            if (HistoricMappings.Length > 0 && Data_Integration.Permissions.ShowHistory)
            {
                this.Cursor = tvTags.Cursor = Cursors.WaitCursor;
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadTags), HistoricMappings);
            }


            foreach (KeyValuePair<String, TimeSpan> kvp in TimeSpans)
                if (-kvp.Value <= Data_Integration.Permissions.MaxSpan)
                    cmbHistoricRange.Items.Add(kvp.Key);
            cmbHistoricRange.Items.Add("Custom range");
            cmbHistoricRange.SelectedIndexChanged -= cmbHistoricRange_SelectedIndexChanged;
            cmbHistoricRange.SelectedIndex = cmbHistoricRange.Items.IndexOf("- 1 hour");
            cmbHistoricRange.SelectedIndexChanged += cmbHistoricRange_SelectedIndexChanged;

            //Assign 'loading...' to titles
            zgHistoricDetails.GraphPane.Title.Text = zgHistoricOverview.GraphPane.Title.Text = zgRealTime.GraphPane.Title.Text = "Loading...";

            //Blank out our graphs
            //Set our master panel
            foreach (GraphPane myMaster in new GraphPane[] { zgRealTime.GraphPane, zgHistoricOverview.GraphPane, zgHistoricDetails.GraphPane })
            {
                myMaster.CurveList.Clear();
                myMaster.Title.IsVisible = false;
                myMaster.Title.FontSpec.FontColor = Color.White;
                myMaster.Legend.Fill = myMaster.Fill = new Fill(Color.Black);
                myMaster.Legend.FontSpec.FontColor = Color.White;
                myMaster.Legend.FontSpec.Fill = new Fill(Color.Black);
                myMaster.Legend.IsVisible = true;
                myMaster.Legend.Position = LegendPos.TopCenter;
                myMaster.Chart.Fill = new Fill(Color.Black);
                myMaster.Chart.Border.Color = Color.DarkGray;

                foreach (Axis AxisToHandle in new Axis[] { myMaster.XAxis, myMaster.YAxis })
                    if (AxisToHandle != null)
                    {
                        AxisToHandle.Title.FontSpec.FontColor = Color.White;
                        AxisToHandle.Scale.FontSpec.FontColor = Color.White;
                        AxisToHandle.MajorTic.Color = Color.White;
                        AxisToHandle.MinorTic.Color = Color.White;
                        AxisToHandle.MajorGrid.Color = Color.White;
                    }
            }
        }

        /// <summary>
        /// When the user changes our requested time span, update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbHistoricRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            TimeSpan FoundTime;            
            if (cmbHistoricRange.Text == "Custom range")
            {
                using (Historical_Viewer_Date_SelectionForm frm = new Historical_Viewer_Date_SelectionForm(this.StartTime, this.EndTime))
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        this.StartTime = frm.StartDate;
                        this.EndTime = frm.EndDate;
                        RequeryHistory();
                    }
                    else
                        cmbHistoricRange.SelectedIndex = (int)cmbHistoricRange.Tag;
            }
            else if (TimeSpans.TryGetValue(cmbHistoricRange.Text, out FoundTime))
                RequeryHistory();
            cmbHistoricRange.Tag = cmbHistoricRange.SelectedIndex;
        }


        /// <summary>
        /// Find all appropriate mappings for our element
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <returns></returns>
        public static string[] GetMappings(MM_Element BaseElement)
        {
            List<String> OutQuery = new List<string>();
            if (BaseElement is MM_Line)
                OutQuery.Add("*.LN." + BaseElement.Name + ".ANLG.*");
            else if (BaseElement is MM_BusbarSection)
                OutQuery.Add(BaseElement.Substation.Name + ".BS." + BaseElement.Name + ".*");
            else if (BaseElement is MM_Unit)
                OutQuery.Add(BaseElement.Substation.Name + ".UN." + BaseElement.Name + ".*");
            else if (BaseElement.ElemType.Name == "Load" || BaseElement.ElemType.Name == "LaaR")
                OutQuery.Add(BaseElement.Substation.Name + ".LD." + BaseElement.Name + ".*");
            else if (BaseElement is MM_Transformer || BaseElement is MM_TransformerWinding)
                OutQuery.Add(BaseElement.Substation.Name + ".XF*." + BaseElement.Name + ".*");
            else if (BaseElement is MM_Substation)
                OutQuery.Add(BaseElement.Name + ".*");
            else if (BaseElement is MM_ShuntCompensator)
                OutQuery.Add(BaseElement.Substation.Name + ".CP." + BaseElement.Name + ".*");
            else if (BaseElement.ElemType.Name == "Switch")
                OutQuery.Add(BaseElement.Substation.Name + ".DSC." + BaseElement.Name + ".*");
            else if (BaseElement.ElemType.Name == "Breaker")
                OutQuery.Add(BaseElement.Substation.Name + ".CB." + BaseElement.Name + ".*");
            return OutQuery.ToArray();
        }
        #endregion

        #region Tag queries and adding
        /// <summary>
        /// Load one or more tags to the list
        /// </summary>
        /// <param name="State"></param>
        private void LoadTags(object State)
        {
            


            //Wait for the history system to be initialized. Give it a few tries w/ 5 second delays, then close down.
            MM_Historic_Connector HistoricConnector = null;
            int TryCount = 0;
            while (HistoricConnector == null)
            {
                foreach (MM_DataConnector dConn in Data_Integration.DataConnections.Values)
                    if (dConn is MM_Historic_Connector && (dConn as MM_Historic_Connector).State == ConnectionState.Open)
                        HistoricConnector = dConn as MM_Historic_Connector;
                if (HistoricConnector == null)
                {
                    Thread.Sleep(5000);
                    if (++TryCount >= 3)
                        return;
                }
            }

            HistoricQueriesCompleted = HistoricQueriesTotal = 0;
            if (State is String)
            {
                HistoricQueriesTotal = 1;
                HistoricConnector.QueryTags((string)State, AddTag, CheckRequery);
            }
            else if (State is string[])
                foreach (String str in (string[])State)
                    if (!String.IsNullOrEmpty(str))
                    {
                        HistoricQueriesTotal++;
                        HistoricConnector.QueryTags(str, AddTag, CheckRequery);
                    }                                
                        
            SafeSetCursor();
        }
        

        /// <summary>
        /// Check when a query is completed, whether we have all the tags
        /// </summary>
        private void CheckRequery()
        {
            if (++HistoricQueriesCompleted == HistoricQueriesTotal)
                if (InvokeRequired)
                    BeginInvoke(new SafeSetCursorDelegate(RequeryHistory));
                else
                    RequeryHistory();
        }

        /// <summary>A thread-safe delegate for assigning the cursor</summary>        
        private delegate void SafeSetCursorDelegate();

        /// <summary>Safely reset our cursor</summary>
        private void SafeSetCursor()
        {
            if (this.InvokeRequired)
                this.Invoke(new SafeSetCursorDelegate(SafeSetCursor));
            else
                this.Cursor = tvTags.Cursor = Cursors.Arrow;
        }


        /// <summary>
        /// Safely add a tag to the check box
        /// </summary>
        /// <param name="Tag"></param>
        private delegate void SafeAddTag(MM_Historic_DataPoint Tag);

        /// <summary>
        /// Add a tag to the check box
        /// </summary>
        /// <param name="Tag"></param>
        public void AddTag(MM_Historic_DataPoint Tag)
        {
            if (tvTags.InvokeRequired)
                tvTags.Invoke(new SafeAddTag(AddTag), Tag);
            else if (!Tag.Title.EndsWith(".DQ") && !Tag.Title.EndsWith(".DQ.AVCAL"))
            {
                TreeNode NewNode = new TreeNode(Tag.Description.Trim());
                NewNode.Name = Tag.Title;                                
                NewNode.Tag = Tag;
                NewNode.ToolTipText = Tag.Title;
                int AlreadyChecked = 0;
                foreach (TreeNode TestNode in tvTags.Nodes)
                    if (TestNode.Checked)
                        AlreadyChecked++;
                if (AlreadyChecked <= 5)                
                    NewNode.Checked = true;
                tvTags.Nodes.Add(NewNode);
            }
        }
        #endregion

        #region Toolstrip rendering
        /// <summary>
        /// Handle the background painting of our toolstrip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Renderer_RenderToolStripBackground(object sender, ToolStripRenderEventArgs e)
        {
            e.Graphics.FillRectangle(BackBrush, e.AffectedBounds);
        }
        #endregion

        #region Trend graph form
        /// <summary>
        /// Initialize a new trend graph form
        /// </summary>
        /// <param name="GraphMode">The graph mode</param>
        /// <param name="EMSMappings">The EMS mappings</param>
        /// <param name="HistoricQueries">The historic queries</param>
        /// <param name="Title">The title of the windo</param>
        /// <returns></returns>
        public static Form Trend_Graph(String Title, GraphModeEnum GraphMode, string[] HistoricQueries, string[] EMSMappings)
        {
            Form TrendForm = new Form();
            TrendForm.Text = Title + " - " + Data_Integration.UserName.ToUpper() + " - Macomber Map�";
            TrendForm.Icon = Resources.ErcotRound;
            TrendForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            TrendForm.FormClosing += new FormClosingEventHandler(TrendForm_FormClosing);
            TrendForm.VisibleChanged += new EventHandler(TrendForm_VisibleChanged);
            Historical_Viewer gView = new Historical_Viewer(GraphMode, HistoricQueries, EMSMappings);
            gView.Dock = DockStyle.Fill;
            TrendForm.Controls.Add(gView);
            TrendForm.Size = new Size(835, 460);
            return TrendForm;
        }

        /// <summary>
        /// Handle the visibility change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TrendForm_VisibleChanged(object sender, EventArgs e)
        {
            ((sender as Form).Controls[0] as Historical_Viewer).RedrawNow();
        }

        /// <summary>
        /// Handle the form closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TrendForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                (sender as Form).Visible = false;
                e.Cancel = true;
            }
        }
        #endregion

        #region User interactions
        /// <summary>
        /// Update the data source request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDataSource_Click(object sender, EventArgs e)
        {
            if (GraphMode == GraphModeEnum.GenTypes)
                UpdateGenTypes();
            else if (GraphMode == GraphModeEnum.LoadVsLoadForecast)
                UpdateLFData();
            else if (GraphMode == GraphModeEnum.SystemWide)
                UpdateSystemWideData();
        }

        #endregion

        #region Historic query handling
        /// <summary>
        /// Update our view based on the check change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvTags_AfterCheck(object sender, TreeViewEventArgs e)
        {
            RequeryHistory();
        }

        /// <summary>
        /// When the selected range changes, update accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            RequeryHistory();
        }
        #endregion

        #region Timer updating
        /// <summary>
        /// Handle the updated timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (!Visible)
                return;
            LastDate = DateTime.Now;

            if (GraphMode == GraphModeEnum.GenTypes && Data_Integration.RTGenDate != LastDate)
            {
                LastDate = Data_Integration.RTGenDate;
                UpdateGenTypes();
            }
            else if (GraphMode == GraphModeEnum.LoadVsLoadForecast && Data_Integration.LFDate != LastDate)
            {
                LastDate = Data_Integration.LFDate;
                UpdateLFData();
            }
            else if (GraphMode == GraphModeEnum.SystemWide && Data_Integration.RTGenDate != LastDate)
            {
                LastDate = Data_Integration.RTGenDate;
                UpdateSystemWideData();
            }
        }

        /// <summary>
        /// Update the trend graph from an external source
        /// </summary>
        public void RedrawNow()
        {
            tmrUpdate_Tick(this, EventArgs.Empty);
        }
        #endregion

        #region Graph updating - history, Real-time Gen, LF, Wind, Frequency, ACE
        /// <summary>
        /// Requery the history database for the requested information
        /// </summary>
        private void RequeryHistory()
        {
            if (!Data_Integration.Permissions.ShowHistory)
                return;

            //Set our master panel
            GraphPane myMaster = zgHistoricOverview.GraphPane;
            myMaster.CurveList.Clear();
            myMaster.Title.IsVisible = false;
            myMaster.Title.FontSpec.FontColor = Color.White;
            myMaster.Legend.Fill = myMaster.Fill = new Fill(Color.Black);
            myMaster.Legend.FontSpec.FontColor = Color.White;
            myMaster.Legend.FontSpec.Fill = new Fill(Color.Black);
            myMaster.Legend.IsVisible = true;
            myMaster.Legend.Position = LegendPos.Right;
            myMaster.XAxis.Title.FontSpec.FontColor = Color.White;
            myMaster.XAxis.MajorTic.Color = Color.White;
            myMaster.XAxis.MinorTic.Color = Color.White;
            myMaster.XAxis.MajorGrid.Color = Color.White;
            myMaster.Chart.Fill = new Fill(Color.Black);
            myMaster.Chart.Border.Color = Color.DarkGray;
            myMaster.XAxis.Scale.FontSpec.FontColor = Color.White;
            
            //First, clear our graph
            zgHistoricOverview.GraphPane.CurveList.Clear();
            zgHistoricOverview.GraphPane.YAxisList.Clear();
            zgHistoricOverview.GraphPane.Legend.IsVisible = false;

            //Now, add in our X axis
            zgHistoricOverview.GraphPane.XAxis.Title.Text = "Date/Time";
            zgHistoricOverview.GraphPane.XAxis.Type = AxisType.Date;

            //Now, add in our Y axes and values
            TimeSpan FoundTime;
            if (TimeSpans.TryGetValue(cmbHistoricRange.SelectedItem.ToString(), out FoundTime))
            {
                this.StartTime = DateTime.Now + FoundTime;
                this.EndTime = DateTime.Now;
            }
            else if (EndTime < StartTime)
            {
                DateTime TempTime = EndTime;
                EndTime = StartTime;
                StartTime = TempTime;
            }

            try
            {
                HistoricPointList.Clear();
                ColorSymbolRotator c = new ColorSymbolRotator();
                foreach (TreeNode Node in tvTags.Nodes)
                    if (!Node.Checked)
                        Node.ForeColor = Color.White;
                    else
                    {
                        MM_Historic_DataPoint ThisPoint = Node.Tag as MM_Historic_DataPoint;
                        int Axis = zgHistoricOverview.GraphPane.YAxisList.IndexOf((string)ThisPoint.Title);
                        if (Axis == -1)
                        {
                            Axis = zgHistoricOverview.GraphPane.YAxisList.Add((string)ThisPoint.Title);
                            Axis NewAxis = zgHistoricOverview.GraphPane.YAxisList[Axis];
                            NewAxis.Title.FontSpec.FontColor = Color.White;
                            NewAxis.Scale.FontSpec.FontColor = Color.White;
                            NewAxis.MajorTic.Color = Color.White;
                            NewAxis.MinorTic.Color = Color.White;
                            NewAxis.MajorGrid.Color = Color.White;                            
                        }

                        PointPairList OutList = new PointPairList();

                        foreach (MM_Historic_DataValue Val in ThisPoint.InterpolateValues(StartTime, EndTime, 100))
                        {
                            Double OutVal;
                            if (Val.Value.GetType().IsEnum)
                                OutVal = Convert.ToDouble(Val.Value);
                            else if (btnLog.Text == "Log Scale (10)")
                                OutVal = Math.Log10(Convert.ToDouble(Val.Value));
                            else if (btnLog.Text == "Log Scale (e)")
                                OutVal = Math.Log(Convert.ToDouble(Val.Value));
                            else
                                OutVal = Convert.ToDouble(Val.Value);
                            OutList.Add(XDate.DateTimeToXLDate(Val.TimeStamp), OutVal);
                        }
                        LineItem NewLine = zgHistoricOverview.GraphPane.AddCurve((string)ThisPoint.Description, OutList, Node.ForeColor = c.NextColor, SymbolType.None);
                        NewLine.YAxisIndex = Axis;
                        NewLine.Tag = ThisPoint;
                        HistoricPointList.Add(ThisPoint, OutList);
                    }
            }
            catch (Exception ex)
            { 
                Console.WriteLine("Historic Data retrieval error: " + ex.Message); 
            }

            if (zgHistoricOverview.GraphPane.YAxisList.Count > 0)
                zgHistoricOverview.AxisChange();
            zgHistoricOverview.Invalidate();
            RefreshMouseOver();
            cmbHistoricRange.Focus();
        }

        /// <summary>
        /// Update the generation types.
        /// Thanks to http://goorman.free.fr/ZedGraph/zedgraph.org/wiki/indexc40e.html?title=Multi-Pie_Chart_Demo for the ZedGraph multi-pie chart demo
        /// </summary>
        private void UpdateGenTypes()
        {
            MasterPane myMaster = zgRealTime.MasterPane;
            myMaster.PaneList.Clear();

            //Build our dictionary of colors matched by generation type
            ColorSymbolRotator csr = new ColorSymbolRotator();
            Dictionary<MM_Generation_Type, Color> Colors = new Dictionary<MM_Generation_Type, Color>();
            foreach (MM_Generation_Type GenType in MM_Repository.GenerationTypes.Values)
                if (GenType.Name != "TotalGeneration" && GenType.EMS && !Colors.ContainsKey(GenType))
                    Colors.Add(GenType, csr.NextColor);

            //Set our master panel            
            myMaster.Title.Text = "Generation Fuel Mix";
            myMaster.Title.IsVisible = true;
            myMaster.Title.FontSpec.FontColor = Color.White;
            myMaster.Legend.Fill = myMaster.Fill = new Fill(Color.Black); //, Color.MediumSlateBlue, 45f);
            myMaster.Legend.FontSpec.FontColor = Color.White;
            myMaster.Legend.FontSpec.Fill = new Fill(Color.Black);
            myMaster.Margin.All = myMaster.InnerPaneGap = 10;
            myMaster.Legend.IsVisible = true;
            myMaster.Legend.Position = LegendPos.TopCenter;
            myMaster.IsUniformLegendEntries = true;


            //Create our panes
            ZedGraph.Label Title = null;
            foreach (PropertyInfo pI in new PropertyInfo[] { typeof(MM_Generation_Type).GetProperty("MW"), typeof(MM_Generation_Type).GetProperty("Remaining"), typeof(MM_Generation_Type).GetProperty("Capacity") })
            {
                GraphPane NewPane = new GraphPane();
                NewPane.Title.IsVisible = false;
                NewPane.Title.Text = pI.Name == "MW" ? "Current MW" : pI.Name + " MW";
                NewPane.Fill = NewPane.Chart.Fill = new Fill(Color.Black);
                NewPane.Legend.FontSpec.FontColor = Color.White;
                NewPane.Border.Color = Color.LightGray;
                NewPane.Title.FontSpec.FontColor = Color.White;
                NewPane.Title.FontSpec.IsBold = true;
                NewPane.Legend.IsVisible = false;
                NewPane.Legend.FontSpec.FontColor = Color.White;
                NewPane.Legend.Fill = new Fill(Color.Black);
                NewPane.Legend.Position = LegendPos.Right;
                if (Title == null)
                    Title = NewPane.Title;
                else
                    NewPane.Title.FontSpec.Size *= 2;


                //Determine our total MW.
                float TotalMW = float.NaN;
                foreach (MM_Generation_Type GenType in MM_Repository.GenerationTypes.Values)
                    if (GenType.Name == "TotalGeneration")
                        TotalMW = (float)pI.GetValue(GenType, null);


                StringBuilder sB = new StringBuilder();
                sB.AppendLine(NewPane.Title.Text + ":");
                sB.AppendLine(TotalMW.ToString("#,##0.0"));
                sB.AppendLine(new String('-', sB.Length));

                //Build our list of generation types
                GenComparer GenComp = new GenComparer(pI);
                List<MM_Generation_Type> GenTypes = new List<MM_Generation_Type>(MM_Repository.GenerationTypes.Count);
                SortedDictionary<String, MM_Generation_Type> GenNames = new SortedDictionary<string, MM_Generation_Type>(StringComparer.CurrentCultureIgnoreCase);

                foreach (MM_Generation_Type GenType in MM_Repository.GenerationTypes.Values)
                    if (GenType.Name != "TotalGeneration" && GenType.EMS && !GenTypes.Contains(GenType))
                    {
                        GenTypes.Add(GenType);
                        GenNames.Add(GenType.EMSName, GenType);
                    }

                foreach (MM_Generation_Type GenType in GenNames.Values)
                {
                    //Now, add our pie slice                
                    float DataValue = (Single)pI.GetValue(GenType, null);
                    Color CurColor = Colors[GenType];
                    Color NewColor = Color.FromArgb(Math.Min(CurColor.R + 20, 255), Math.Min(CurColor.G + 20, 255), Math.Min(CurColor.B + 20, 255));
                    PieItem NewPie = NewPane.AddPieSlice(DataValue, CurColor, NewColor, 90f, 0.0f, MM_Repository.TitleCase(GenType.EMSName));
                    NewPie.LabelType = PieLabelType.None;
                    NewPie.Tag = MM_Repository.TitleCase(GenType.EMSName) + " " + NewPane.Title.Text + " " + DataValue.ToString("#,##0.00") + " (" + (DataValue / TotalMW).ToString("0.00%") + ")";
                }

                GenTypes.Sort(GenComp);
                foreach (MM_Generation_Type GenType in GenTypes)
                {
                    float DataValue = (Single)pI.GetValue(GenType, null);
                    sB.AppendLine(MM_Repository.TitleCase(GenType.EMSName) + ": " + (DataValue / TotalMW).ToString("0.0%"));

                }

                TextObj Legend = new TextObj(sB.ToString(), 0.99, 0.1, CoordType.PaneFraction);
                Legend.Location.AlignH = AlignH.Right;
                Legend.Location.AlignV = AlignV.Top;
                Legend.FontSpec.Size = pI.Name == "MW" ? 16 : 26;
                Legend.FontSpec.Border.IsVisible = true;
                Legend.FontSpec.Fill = new Fill(Color.Black);
                Legend.FontSpec.FontColor = Color.White;
                NewPane.GraphObjList.Add(Legend);
                myMaster.Add(NewPane);
            }

            try
            {
                using (Graphics g = CreateGraphics())
                    myMaster.SetLayout(g, PaneLayout.ExplicitRow12);
                zgRealTime.AxisChange();
                zgRealTime.Invalidate();
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Update Load Forecast Data
        /// </summary>
        private void UpdateLFData()
        {
            if (Data_Integration.Permissions.showRTHist == false)
            {
                tcMain.TabPages.Remove(tpRealTime);
            
            }
            //test graph nataros
            float CurValue = Data_Integration.OverallIndicators[1];


            //Set our master panel
            GraphPane myMaster = zgRealTime.GraphPane;
            myMaster.CurveList.Clear();
            myMaster.Title.IsVisible = false;
            myMaster.Title.FontSpec.FontColor = Color.White;
            myMaster.Legend.Fill = myMaster.Fill = new Fill(Color.Black);
            myMaster.Legend.FontSpec.FontColor = Color.White;
            myMaster.Legend.FontSpec.Fill = new Fill(Color.Black);
            myMaster.Legend.IsVisible = true;
            myMaster.Legend.Position = LegendPos.TopCenter;
            myMaster.XAxis.Title.FontSpec.FontColor = Color.White;
            myMaster.YAxis.Title.FontSpec.FontColor = Color.White;
            myMaster.XAxis.MajorTic.Color = Color.White;
            myMaster.XAxis.MinorTic.Color = Color.White;
            myMaster.YAxis.MajorTic.Color = Color.White;
            myMaster.YAxis.MinorTic.Color = Color.White;
            myMaster.XAxis.MajorGrid.Color = Color.White;
            myMaster.YAxis.MajorGrid.Color = Color.White;
            myMaster.Chart.Fill = new Fill(Color.Black);
            myMaster.Chart.Border.Color = Color.DarkGray;
            myMaster.XAxis.Scale.FontSpec.FontColor = Color.White;
            myMaster.YAxis.Scale.FontSpec.FontColor = Color.White;

            //Create our list of points
            Dictionary<String, PointPairList> Points = new Dictionary<String, PointPairList>(EMSMappings.Length);
            for (int a = 2; a <= EMSMappings.Length; a += 2)
                Points.Add(EMSMappings[a], new PointPairList());

            //Create our data rows
            if (Data_Integration.LoadForecast != null && Data_Integration.LoadForecastDates != null)
                for (int CurRow = 0; CurRow < Data_Integration.LoadForecast.Rows.Count; CurRow++)
                {
                    DataRow LF = Data_Integration.LoadForecast.Rows[CurRow];
                    DataRow LFDate = Data_Integration.LoadForecastDates.Rows[CurRow];
                    Double ThisDate = XDate.DateTimeToXLDate(Convert.ToDateTime(Data_Integration.LoadForecastUseEndDates ? LFDate["TIMEEND"] : LFDate["TIME"]));
                    for (int a = 1; a < EMSMappings.Length; a += 2)
                        if (LF[EMSMappings[a]] is DBNull == false)
                            Points[EMSMappings[a + 1]].Add(ThisDate, Convert.ToSingle(CurValue));// old way nataros test LF[EMSMappings[a]]));
                }


            //Add everything in and update
            ColorSymbolRotator csr = new ColorSymbolRotator();
            foreach (KeyValuePair<String, PointPairList> Point in Points)
                if (Point.Value.Count > 0)
                {
                    //zgRealTime.GraphPane.AddCurve(Point.Key, Point.Value, csr.NextColor, SymbolType.None);  //real one for updates
                    //PointPairList OutPoints = new PointPairList();
                    //OutPoints.AddRange(Point.Value);
                    zgRealTime.GraphPane.AddCurve(Point.Key, Point.Value, csr.NextColor, SymbolType.None); //test one for seeing if data comes in properly
                }
            zgRealTime.GraphPane.XAxis.Type = AxisType.Date;
            zgRealTime.GraphPane.XAxis.Title.Text = "Time";
            if (zgRealTime.GraphPane.YAxisList.Count == 0)
                zgRealTime.GraphPane.YAxisList.Add("MW");
            else
                zgRealTime.GraphPane.YAxis.Title.Text = "MW";
            zgRealTime.GraphPane.AxisChange();
            zgRealTime.Invalidate();
        }

        ///<summary>
        ///This is a test SystemWideHandler set to use since one one is DOA - nataros
        ///
        /// </summary>
        ///

        private DataTable CreateTabletoAdd(string KeyFieldName)
        {
            DataTable TempTable1 = new DataTable("KeyFieldName");
            TempTable1.Columns.Add("time");
            TempTable1.Columns.Add("value");
            return TempTable1;
        }
        
        private void UpdateGraphDataSets()
        {
            string UpdateFieldName = "temp"; //this will be passed or generated for each data type in our list from the user tables, such as freq or wind that we need to care about
            try
            {
                //now to ensure datatables exist
                if (!GraphDataRecords.Tables.Contains(UpdateFieldName))
                {
                    GraphDataRecords.Tables.Add(CreateTabletoAdd(UpdateFieldName));
                }
                //now update the values
                //add datarow into that table? - nataros
                GraphDataRecords.Tables[UpdateFieldName].Rows.Add("time", "value");



                /* AssignSingle(dr["LD"], ref Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Load]);
                 AssignSingle(dr["FHZ"], ref Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Frequency]);
                 AssignSingle(dr["ACE"], ref Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.ACE]);
                 float HSLT = float.NaN, Gen = float.NaN, HASLT = float.NaN;
                 AssignSingle(dr["HASLT"], ref HASLT);
                 AssignSingle(dr["HSLT"], ref HSLT);
                 AssignSingle(dr["GEN"], ref Gen);

                 Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Gen] = Gen;
                 if (!float.IsNaN(HSLT) && !float.IsNaN(Gen))
                     Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.GenEmrgCapacity] = HSLT - Gen;
                 if (!float.IsNaN(HASLT) && !float.IsNaN(Gen))
                     Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.GenCapacity] = HASLT - Gen;




                 AssignSingle(dr["PRCT"], ref Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.LoadRes]);
                 AssignSingle(dr["PRCT"], ref Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.PRC]);
                 foreach (MM_Reserve Reserve in Data_Integration.Reserves.Values)
                     Reserve.UpdateValue(dr[Reserve.EMSValue]);

                 //Data_Integration.AddSystemValues(dr, Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Wind], Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Gen]);
             }
             catch (Exception ex)
             {
                 Program.LogError("EMS: Error in SystemWide handler: " + ex.Message);
             }

             ////

             //First, create our system values table if needed
             if (Data_Integration.SystemValues == null)
                 Data_Integration.SystemValues = new DataTable("SystemValues");

             //Now, validate all columns
             foreach (DataColumn dCol in dr.Table.Columns)
                 ValidateColumn(Data_Integration.SystemValues, dCol.ColumnName, dCol.DataType);
             ValidateColumn(Data_Integration.SystemValues, "Wind", typeof(Single));
             ValidateColumn(Data_Integration.SystemValues, "WindPercent", typeof(Single));

             //Now, update our tables
             lock (Data_Integration.SystemValues)
             {
                 if (Data_Integration.SystemValues.Rows.Count == MM_Repository.OverallDisplay.SystemValueCount)
                     Data_Integration.SystemValues.Rows.RemoveAt(0);
                 if (Data_Integration.SystemValues.Rows.Count == 0 || Convert.ToDateTime(Data_Integration.SystemValues.Rows[Data_Integration.SystemValues.Rows.Count - 1]["TAGCLAST"]) != Convert.ToDateTime(dr["TAGCLAST"]))
                 {
                     DataRow NewRow = Data_Integration.SystemValues.Rows.Add();
                     foreach (DataColumn dCol in dr.Table.Columns)
                         NewRow[dCol.ColumnName] = dr[dCol];
                     NewRow["Wind"] = Wind;
                     NewRow["WindPercent"] = 100f * Wind / TotalGen;
                 }

                 RTGenDate = Convert.ToDateTime(dr["TAGCLAST"]);

             }
                 */
            }
            catch
            {
            }
        }


        /// <summary>
        /// Update SystemWide data
        /// </summary>
        private void UpdateSystemWideData()
        {
            if (Data_Integration.Permissions.showRTHist == false)
            {
                tcMain.TabPages.Remove(tpRealTime);
            }
            //test graph nataros
            float CurValue = Data_Integration.OverallIndicators[1];



            ///////////end test

            
            //Set our master panel
            GraphPane myMaster = zgRealTime.GraphPane;
            myMaster.CurveList.Clear();
            myMaster.Title.IsVisible = false;
            myMaster.Title.FontSpec.FontColor = Color.White;
            myMaster.Legend.Fill = myMaster.Fill = new Fill(Color.Black);
            myMaster.Legend.FontSpec.FontColor = Color.White;
            myMaster.Legend.FontSpec.Fill = new Fill(Color.Black);
            myMaster.Legend.IsVisible = true;
            myMaster.Legend.Position = LegendPos.TopCenter;
            myMaster.XAxis.Title.FontSpec.FontColor = Color.White;
            myMaster.YAxis.Title.FontSpec.FontColor = Color.White;
            myMaster.XAxis.MajorTic.Color = Color.White;
            myMaster.XAxis.MinorTic.Color = Color.White;
            myMaster.YAxis.MajorTic.Color = Color.White;
            myMaster.YAxis.MinorTic.Color = Color.White;
            myMaster.XAxis.MajorGrid.Color = Color.White;
            myMaster.YAxis.MajorGrid.Color = Color.White;
            myMaster.Chart.Fill = new Fill(Color.Black);
            myMaster.Chart.Border.Color = Color.DarkGray;
            myMaster.XAxis.Scale.FontSpec.FontColor = Color.White;
            myMaster.YAxis.Scale.FontSpec.FontColor = Color.White;

            //Create our list of points            
            if (Data_Integration.SystemValues == null)
                return;
            Dictionary<String, List<PointPair>> Points = new Dictionary<String, List<PointPair>>(Data_Integration.SystemValues.Columns.Count, StringComparer.CurrentCultureIgnoreCase);
            for (int a = 2; a < EMSMappings.Length; a += 2)
                Points.Add(EMSMappings[a], new List<PointPair>());

            //Create our data rows
            try
            {
                foreach (DataRow dR in Data_Integration.SystemValues.Rows)
                {
                    double ThisDate = ZedGraph.XDate.DateTimeToXLDate(Convert.ToDateTime(dR["TAGCLAST"]));
                    for (int a = 1; a < EMSMappings.Length; a += 2)
                        if (dR[EMSMappings[a]] is DBNull == false)
                        {
                            float inPoint = Convert.ToSingle(dR[EMSMappings[a]]);
                            if (!float.IsNaN(inPoint))
                                Points[EMSMappings[a + 1]].Add(new PointPair(ThisDate, inPoint));
                        }
                }
            }
            catch (Exception ex)
            {
                LastDate = DateTime.Now;
                Program.LogError(ex);
            }

            //Add everything in and update
            zgRealTime.GraphPane.XAxis.Type = AxisType.Date;
            zgRealTime.GraphPane.XAxis.Title.Text = "Time";
            if (zgRealTime.GraphPane.YAxisList.Count == 0)
                zgRealTime.GraphPane.YAxisList.Add("MW");
            else
                zgRealTime.GraphPane.YAxis.Title.Text = "MW";
            ColorSymbolRotator csr = new ColorSymbolRotator();
            foreach (KeyValuePair<String, List<PointPair>> Point in Points)
                if (Point.Value.Count > 0)
                {
                    Point.Value.Sort(new PointPair.PointPairComparer(SortType.XValues));
                    PointPairList OutPoints = new PointPairList();
                    OutPoints.AddRange(Point.Value);
                    LineItem NewItem = zgRealTime.GraphPane.AddCurve(Point.Key, OutPoints, csr.NextColor, SymbolType.None);
                    if (Point.Key == "Frequency")
                    {
                        if (EMSMappings.Length > 3)
                        {
                            NewItem.IsY2Axis = true;
                            zgRealTime.GraphPane.Y2Axis.Title.Text = "Hz";
                        }
                        else
                            zgRealTime.GraphPane.YAxis.Title.Text = "Hz";
                        zgRealTime.GraphPane.Y2Axis.IsVisible = true;
                    }
                    else if (Point.Key == "Wind percentage")
                    {
                        NewItem.IsY2Axis = true;
                        zgRealTime.GraphPane.Y2Axis.Title.Text = "Percentage";
                        zgRealTime.GraphPane.Y2Axis.IsVisible = true;
                    }
                }
            zgRealTime.GraphPane.AxisChange();
            zgRealTime.Invalidate();
        }
        #endregion

        #region Generation type updating
        private class GenComparer : IComparer<MM_Generation_Type>
        {
            private PropertyInfo CompareField;

            /// <summary>
            /// Initialize a new gen comparer
            /// </summary>
            /// <param name="CompareField"></param>
            public GenComparer(PropertyInfo CompareField)
            {
                this.CompareField = CompareField;
            }

            /// <summary>
            /// Compare two generation types
            /// </summary>
            /// <param name="x">The first generation type</param>
            /// <param name="y">The second generation type</param>
            /// <returns></returns>
            public int Compare(MM_Generation_Type x, MM_Generation_Type y)
            {
                return -((Single)CompareField.GetValue(x, null)).CompareTo((Single)CompareField.GetValue(y, null));
            }
        }

        /// <summary>
        /// Update the view based on the current type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbData_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GraphMode == GraphModeEnum.GenTypes)
                UpdateGenTypes();
        }

        #endregion

        /// <summary>
        /// When the user requests a history refresh, do so.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RequeryHistory();
        }

        /// <summary>
        /// Change the log scale value of our indicator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLog_Click(object sender, EventArgs e)
        {
            if (btnLog.Text == "Log Scale (10)")
                btnLog.Text = "Log Scale (e)";
            else if (btnLog.Text == "Log Scale (e)")
                btnLog.Text = "Log Scale";
            else
                btnLog.Text = "Log Scale (10)";
            RequeryHistory();
        }

        /// <summary>
        /// Return a human-readable point-value tooltip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pane"></param>
        /// <param name="curve"></param>
        /// <param name="iPt"></param>
        /// <returns></returns>
        private string zg_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt)
        {           
            DateTime TimeVal;
            if (!DateTime.TryParse(pane.Title.Text, out TimeVal))
                TimeVal = DateTime.Now;

            String Axis = "??";
            if (curve.Tag is MM_Historic_DataPoint)
                Axis = (curve.Tag as MM_Historic_DataPoint).Title;
            else if (curve is LineItem)
                Axis = (curve as LineItem).GetYAxis(pane).Title.Text;


            String OutVal;
            if (curve.IsPie && curve.Tag is String)
                OutVal = (curve as PieItem).Tag.ToString();
            else if (curve.IsPie)
                OutVal = "?";

            else if (btnLog.Text == "Log Scale (10)")
            {
                OutVal = MM_Repository.TitleCase(curve.Label.Text) + ":\n" + curve[iPt].Y.ToString("#,##0.00") + " (" + Axis + " in Base 10)";
                TimeVal = XDate.XLDateToDateTime(curve[iPt].X);
            }
            else if (btnLog.Text == "Log Scale (e)")
            {
                OutVal = MM_Repository.TitleCase(curve.Label.Text) + ":\n" + curve[iPt].Y.ToString("#,##0.00") + " (" + Axis + " in Base e)";
                TimeVal = XDate.XLDateToDateTime(curve[iPt].X);
            }
            else
            {
                OutVal = MM_Repository.TitleCase(curve.Label.Text) + ":\n" + curve[iPt].Y.ToString("#,##0.00") + " " + Axis;
                TimeVal = XDate.XLDateToDateTime(curve[iPt].X);
            }

            return  OutVal + "\nat " + TimeVal.ToString();
        }

        /// <summary>
        /// Handle the user clicking on the historic overview graph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private bool zgHistoricOverview_MouseMoveEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (!splHistoric.Panel2Collapsed)
            {
                LastPane = sender.MasterPane.FindPane(e.Location);
                Object NearestObject = null;
                int NearestIndex = 0;
                if (LastPane != null)
                    using (Graphics g = CreateGraphics())
                        if (LastPane.FindNearestObject(e.Location, g, out NearestObject, out NearestIndex))
                            if (NearestObject is LineItem)
                            {
                                LineItem NearestLine = NearestObject as LineItem;
                                LastMouseOverTime = NearestLine.Points[NearestIndex].X;
                                RefreshMouseOver();
                            }
            }
            return default(bool);
        }

        /// <summary>
        /// Refresh our mouseover information
        /// </summary>
        private void RefreshMouseOver()
        {
            if (LastPane == null)
                return;
            //Now, go through all the line items for our pane, and find the interpolated values
            zgHistoricDetails.GraphPane.Title.Text = XDate.XLDateToDateTime(LastMouseOverTime).ToString();
            zgHistoricDetails.GraphPane.Title.IsVisible = true;

            Double CurVal;
            Dictionary<String, PieItem> CurPies = new Dictionary<String, PieItem>();
            foreach (PieItem ItemToHandle in zgHistoricDetails.GraphPane.CurveList)
                CurPies.Add(ItemToHandle.Label.Text,ItemToHandle);

            foreach (LineItem lI in LastPane.CurveList)
                if (!Double.IsNaN(CurVal = Math.Abs(HistoricPointList[lI.Tag as MM_Historic_DataPoint].InterpolateX(LastMouseOverTime))))
                {
                    Color CurColor = lI.Color;
                    Color NewColor = Color.FromArgb(Math.Min(CurColor.R + 20, 255), Math.Min(CurColor.G + 20, 255), Math.Min(CurColor.B + 20, 255));
                    PieItem NewPie;
                    if (CurPies.TryGetValue(lI.Label.Text, out NewPie))
                    {
                        CurPies.Remove(lI.Label.Text);
                        NewPie.Fill = new Fill(CurColor, NewColor, 90f);
                        NewPie.Value = CurVal;
                    }
                    else
                        NewPie = zgHistoricDetails.GraphPane.AddPieSlice(CurVal, CurColor, NewColor, 90f, 0.0f, lI.Label.Text);
                   if (btnLog.Text == "Log Scale (10)")
                       NewPie.Tag = lI.Label.Text + " " + CurVal.ToString("#,##0.00") + " (" + (lI.Tag as MM_Historic_DataPoint).Title + " in base 10)";
                   else if (btnLog.Text == "Log Scale (e)")
                       NewPie.Tag = lI.Label.Text + " " + CurVal.ToString("#,##0.00") + " (" + (lI.Tag as MM_Historic_DataPoint).Title + " in base e)";
                   else 
                        NewPie.Tag = lI.Label.Text +  " " + CurVal.ToString("#,##0.00") + " " + (lI.Tag as MM_Historic_DataPoint).Title;
                    NewPie.LabelType = PieLabelType.Name_Value_Percent;
                }

            foreach (PieItem PieToRemove in new List<PieItem>(CurPies.Values))
                zgHistoricDetails.GraphPane.CurveList.Remove(PieToRemove);
            zgHistoricDetails.GraphPane.Legend.IsVisible = false;
            zgHistoricDetails.GraphPane.XAxis.MajorTic.IsBetweenLabels = true;
            zgHistoricDetails.AxisChange();
            zgHistoricDetails.Invalidate();
        }

        /// <summary>
        /// Handle the request for a more detailed view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDetailedView_Click(object sender, EventArgs e)
        {
            splHistoric.Panel2Collapsed ^= true;
            
                List<Control> ControlsToMove2 = new List<Control>();
                foreach (Control ctl in splHistoricLookup.Panel2.Controls)
                    ControlsToMove2.Add(ctl);
                splHistoricLookup.Panel2.Controls.Clear();
                List<Control> CtlToMove = new List<Control>();
                foreach (Control ctl in splHistoric.Panel2.Controls)
                    CtlToMove.Add(ctl);
                splHistoric.Panel2.Controls.Clear();
                splHistoricLookup.Panel2.Controls.AddRange(CtlToMove.ToArray());
                splHistoric.Panel2.Controls.AddRange(ControlsToMove2.ToArray());
           
        }
    }
}
