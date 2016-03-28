using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using MacomberMapClient.Integration;
using System.Threading;
using MacomberMapClient.Data_Elements.Physical;
using System.Reflection;
using System.ServiceModel;
using MacomberMapClient.Properties;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.EMS;

namespace MacomberMapClient.User_Interfaces.Summary
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc.
    /// This form provides a real-time and historical view into data.
    /// </summary>
    public partial class MM_Historic_Viewer : UserControl
    {
         #region Variable declarations
        /// <summary>Our incoming table data</summary>
        private DataTable InTable = null;

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
        
        /// <summary>Our most recently moused-over point</summary>
        private Double LastMouseOverTime = XDate.DateTimeToXLDate(DateTime.Now);

        /// <summary>The last pane that was used in searching</summary>
        private GraphPane LastPane;

        /// <summary>The item's current graph mode</summary>
        public GraphModeEnum GraphMode;

        /// <summary>The delegate to handle the updating of graph data</summary>
        public delegate void UpdateGraphData();

        /// <summary>The EMS category for our data - that indicates a row in the chart data is for this graph</summary>
        public string SourceCategory;

        /// <summary>The mappings between the data and their titles</summary>
        private string[] SourceMappings;

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

        /// <summary>Our query results</summary>
        private MM_Historic_Query QueryResults = null;

        /// <summary>Our historic mappings</summary>
        private string[] HistoricMappings;
        #endregion

        #region Initialization
        /// <summary>
        /// Set up the historical viewer
        /// </summary>
        /// <param name="GraphMode">Our graphics mode</param>        
        /// <param name="HistoricMappings">The PI mappings</param>
        /// <param name="SourceMappings">The mappings</param>
        /// <param name="SourceCategory">The source category</param>
        public MM_Historic_Viewer(GraphModeEnum GraphMode, string[] HistoricMappings, string[] SourceMappings, String SourceCategory)
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

            this.HistoricMappings = HistoricMappings;
            this.SourceCategory = SourceCategory;
            this.SourceMappings = SourceMappings;
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

           

            foreach (KeyValuePair<String, TimeSpan> kvp in TimeSpans)
                if (-kvp.Value <= Data_Integration.Permissions.MaxSpan)
                    cmbHistoricRange.Items.Add(kvp.Key);
            cmbHistoricRange.Items.Add("Custom range");
            cmbHistoricRange.SelectedIndex = cmbHistoricRange.Items.IndexOf("- 1 hour");

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
                using (MM_Historic_Viewer_Date_SelectionForm frm = new MM_Historic_Viewer_Date_SelectionForm(this.StartTime, this.EndTime))
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        this.StartTime = frm.StartDate;
                        this.EndTime = frm.EndDate;
                        RequeryPI();
                    }
                    else
                        cmbHistoricRange.SelectedIndex = (int)cmbHistoricRange.Tag;
            }
            else if (TimeSpans.TryGetValue(cmbHistoricRange.Text, out FoundTime))
                RequeryPI();
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
            // TODO: fix for AF.
            if (BaseElement is MM_Line)
            {
                var line =  BaseElement as MM_Line;
                string tag = "LINE*";
                if (((MM_Line)BaseElement).IsZBR)
                    tag = "ZBR*";
                try
                {
                    if (line.ToEndKey != null && line.ToEndKey.Contains(line.Substation2.Name))
                        OutQuery.Add("*" + line.ToEndKey.Substring(line.ToEndKey.IndexOf(line.Substation2.Name)) + "*");
                    if (line.ToEndKey != null && line.ToEndKey.Contains(line.Substation1.Name))
                        OutQuery.Add("*" + line.ToEndKey.Substring(line.ToEndKey.IndexOf(line.Substation1.Name)) + "*");
                    if (line.FromEndKey != null && line.FromEndKey.Contains(line.Substation1.Name))
                        OutQuery.Add("*" + line.FromEndKey.Substring(line.FromEndKey.IndexOf(line.Substation1.Name)) + "*");
                    if (line.FromEndKey != null && line.FromEndKey.Contains(line.Substation2.Name))
                        OutQuery.Add("*" + line.FromEndKey.Substring(line.FromEndKey.IndexOf(line.Substation2.Name)) + "*");
                }
                catch (Exception exp)
                {
                    OutQuery.Add(tag + line.Substation1.Name + "*");
                }
                OutQuery.Add(tag + line.Substation2.Name + "*");
            }else if (BaseElement is MM_Flowgate)
            {
                OutQuery.Add(BaseElement.Name + "*");
            }
            else
                OutQuery.Add("*" + BaseElement.Name.Replace("TOPOLOGY.", "") + "*");

            if (BaseElement is MM_Bus)
                OutQuery.Add(BaseElement.Substation.Name + ".BS." + BaseElement.Name + ".*");
            else if (BaseElement is MM_Unit)
            {
                OutQuery.Add("*" + ((MM_Unit)BaseElement).MarketResourceName + "*");
            }
            else if (BaseElement.ElemType.Name == "Load" || BaseElement.ElemType.Name == "LaaR")
            {
                OutQuery.Add("SUBSTN." + BaseElement.Substation.Name + "*.MW");
            }
           /* else if (BaseElement is MM_Transformer )
            {
                OutQuery.Add("*" + ((MM_Transformer)BaseElement).Winding2.Name.Replace("TOPOLOGY.", "") + "*");
                OutQuery.Add("*" + ((MM_Transformer)BaseElement).Winding1.Name.Replace("TOPOLOGY.", "") + "*");
            } */
            else if (BaseElement is MM_TransformerWinding)
            {
                OutQuery.Add("*" + BaseElement.Name.Replace("TOPOLOGY.", "") + "*");
            }
            else if (BaseElement is MM_Substation)
                OutQuery.Add("*" + BaseElement.Name + ".*");
            else if (BaseElement is MM_ShuntCompensator)
                OutQuery.Add("CAPACITOR.*" + BaseElement.Substation.Name + "*");
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
        /// <param name="state"></param>
        private void LoadTags(object state)
        {
            //Send out our query 
            String[] Tags = (string[])state;
            if (Tags.Length == 0 || MM_Server_Interface.Client == null || MM_Server_Interface.Client.InnerChannel == null || MM_Server_Interface.Client.InnerChannel.State != CommunicationState.Opened)
                return;
            try
            {
                string query = Tags[0];

                if (Tags.Length > 1)
                    query = Tags[0] + ";" + Tags[1];

                Guid QueryGuid = MM_Server_Interface.Client.QueryTags(query, StartTime, EndTime, 1000);

                //Now, wait for our results
                MM_Historic_Query.enumQueryState QueryState = MM_Server_Interface.Client.CheckQueryStatus(QueryGuid);
                while (QueryState == MM_Historic_Query.enumQueryState.InProgress || QueryState == MM_Historic_Query.enumQueryState.Queued)
                {
                    Thread.Sleep(250);
                    QueryState = MM_Server_Interface.Client.CheckQueryStatus(QueryGuid);
                }

                if (QueryState == MM_Historic_Query.enumQueryState.Completed)
                    this.QueryResults = MM_Server_Interface.Client.RetrieveQueryResults(QueryGuid);

                SafeSetCursor();
                CheckUpdateGraphData();
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
        }
        

        /// <summary>
        /// Check when a query is completed, whether we have all the tags
        /// </summary>
        private void CheckUpdateGraphData()
        {
            if (InvokeRequired)
                BeginInvoke(new SafeSetCursorDelegate(UpdateHistoricInformation));
            else
                UpdateHistoricInformation();
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
        private delegate void SafeAddTag(DataColumn Tag);

        /// <summary>
        /// Add a tag to the check box
        /// </summary>
        /// <param name="Tag"></param>
        public void AddTag(DataColumn Tag)
        {
            if (tvTags.InvokeRequired)
                tvTags.Invoke(new SafeAddTag(AddTag), Tag);
            else if (!Tag.ColumnName.EndsWith(".DQ") && !Tag.ColumnName.EndsWith(".DQ.AVCAL"))
            {
                TreeNode NewNode = new TreeNode(Tag.ColumnName.Trim());
                NewNode.Name = Tag.ColumnName;                                
                NewNode.Tag = Tag;
                NewNode.ToolTipText = Tag.Caption;
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
        /// <param name="SourceMappings">The EMS mappings</param>
        /// <param name="PIQueries">The PI queries</param>
        /// <param name="Title">The title of the window</param>
        /// <param name="SourceCategory">The source category</param>
        /// <returns></returns>
        public static Form Trend_Graph(String Title, GraphModeEnum GraphMode, string[] PIQueries, string[] SourceMappings, String SourceCategory)
        {
            Form TrendForm = new Form();
            TrendForm.Text = Title + " - " + Data_Integration.UserName.ToUpper() + " - Macomber Map®";
            TrendForm.Icon = Properties.Resources.CompanyIcon;
            TrendForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            TrendForm.FormClosing += new FormClosingEventHandler(TrendForm_FormClosing);
            TrendForm.VisibleChanged += new EventHandler(TrendForm_VisibleChanged);
            MM_Historic_Viewer gView = new MM_Historic_Viewer(GraphMode, PIQueries, SourceMappings, SourceCategory);
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
            ((sender as Form).Controls[0] as MM_Historic_Viewer).RedrawNow();
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

        #region PI query handling
        /// <summary>
        /// Update our view based on the check change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvTags_AfterCheck(object sender, TreeViewEventArgs e)
        {
            CheckUpdateGraphData();
        }

        /// <summary>
        /// When the selected range changes, update accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            RequeryPI();
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
            else if (GraphMode == GraphModeEnum.SystemWide && Data_Integration.ChartDate != LastDate)
            {
                LastDate = Data_Integration.ChartDate;
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
        private void RequeryPI()
        {
            if (!Data_Integration.Permissions.ShowHistory)
                return;

            //Make sure we're dealing with the proper time ranges
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

            //If our query needs to be executed, do so.
            if (StartTime != default(DateTime) && (QueryResults == null || QueryResults.StartTime != StartTime || QueryResults.EndTime != EndTime))
            { 
                this.Cursor = tvTags.Cursor = Cursors.WaitCursor;
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadTags), HistoricMappings);
            }
        }


        /// <summary>
        /// After a query is completed or errored, update our historic information
        /// </summary>
        private void UpdateHistoricInformation()
        {
            //Set our master panel
            GraphPane myMaster = zgHistoricOverview.GraphPane;
            if (myMaster == null)
                return;
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

            if (QueryResults == null || QueryResults.SerializedTableData == null)
                return;


            //Make sure all of our tags are added
            InTable = QueryResults.DeserializeTable();
            foreach (DataColumn dCol in InTable.Columns)
                if (dCol.DataType == typeof(double) && !tvTags.Nodes.ContainsKey(dCol.ColumnName))
                    AddTag(dCol);
            InTable.AcceptChanges();
            try
            {
                ColorSymbolRotator c = new ColorSymbolRotator();
                foreach (TreeNode Node in tvTags.Nodes)
                    if (!Node.Checked)
                        Node.ForeColor = Color.White;
                    else
                    {
                        DataColumn ThisPoint = Node.Tag as DataColumn;
                        String ColumnName = ThisPoint.ColumnName.Substring(ThisPoint.ColumnName.LastIndexOf('.')+1);
                        int Axis = zgHistoricOverview.GraphPane.YAxisList.IndexOf(ColumnName);
                        if (Axis == -1)
                        {
                            Axis = zgHistoricOverview.GraphPane.YAxisList.Add(ColumnName);
                            Axis NewAxis = zgHistoricOverview.GraphPane.YAxisList[Axis];
                            NewAxis.Title.FontSpec.FontColor = Color.White;
                            NewAxis.Scale.FontSpec.FontColor = Color.White;
                            NewAxis.MajorTic.Color = Color.White;
                            NewAxis.MinorTic.Color = Color.White;
                            NewAxis.MajorGrid.Color = Color.White;
                        }

                        PointPairList OutList = new PointPairList();
                        foreach (DataRow dR in InTable.Rows)
                        {
                            try
                            {
                                string colName = null;
                                for (int i = 0; i < InTable.Columns.Count; i++)
                                {
                                    if (ThisPoint.ColumnName.Equals(InTable.Columns[i].ColumnName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        colName = InTable.Columns[i].ColumnName;
                                        break;
                                    }
                                }
                                Object OutVal = null;
                                if (colName != null)
                                    OutVal = dR[colName];
                                else
                                    OutVal = dR[ThisPoint];
                                if (OutVal is Double == false)
                                    OutVal = Double.NaN;
                                else if (btnLog.Text == "Log Scale (10)")
                                    OutVal = Math.Log10(Convert.ToDouble(OutVal));
                                else if (btnLog.Text == "Log Scale (e)")
                                    OutVal = Math.Log(Convert.ToDouble(OutVal));
                                else
                                    OutVal = Convert.ToDouble(OutVal);
                                OutList.Add(XDate.DateTimeToXLDate((DateTime) dR[0]), (Double) OutVal);
                            } catch (Exception ex)
                            {
                                MM_System_Interfaces.WriteConsoleLine("Historic Data presentation error: " + ex.Message);
                            }
                        }
                        LineItem NewLine = zgHistoricOverview.GraphPane.AddCurve((string)ThisPoint.Caption, OutList, Node.ForeColor = c.NextColor, SymbolType.None);
                        NewLine.YAxisIndex = Axis;
                        NewLine.Tag = ThisPoint;
                    }

            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Historic Data retrieval error: " + ex.Message);
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
            try
            {
                //Build our dictionary of colors matched by generation type
                ColorSymbolRotator csr = new ColorSymbolRotator();
                Dictionary<MM_Unit_Type, Color> Colors = new Dictionary<MM_Unit_Type, Color>();
                foreach (MM_Unit_Type GenType in MM_Repository.GenerationTypes.Values)
                    if (GenType.Name != "TotalGeneration" && !Colors.ContainsKey(GenType) && !String.IsNullOrEmpty(GenType.EMSName))
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
                foreach (PropertyInfo pI in new PropertyInfo[]
                                            {
                                                typeof (MM_Unit_Type).GetProperty("MW"),
                                                typeof (MM_Unit_Type).GetProperty("Remaining"),
                                                typeof (MM_Unit_Type).GetProperty("Capacity")
                                            })
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
                    foreach (MM_Unit_Type GenType in MM_Repository.GenerationTypes.Values)
                        if (GenType.Name == "TotalGeneration")
                            TotalMW = (float)pI.GetValue(GenType, null);
                    
                    if (TotalMW == 0)
                        foreach (MM_Unit_Type GenType in MM_Repository.GenerationTypes.Values)
                            TotalMW += (float)pI.GetValue(GenType, null);

                    StringBuilder sB = new StringBuilder();
                    sB.AppendLine(NewPane.Title.Text + ":");
                    sB.AppendLine(TotalMW.ToString("#,##0.0"));

                    sB.AppendLine(new String('-', sB.Length));

                    //Build our list of generation types
                    GenComparer GenComp = new GenComparer(pI);
                    List<MM_Unit_Type> GenTypes = MM_Repository.GenerationTypes.Values.ToList();
                    SortedDictionary<String, MM_Unit_Type> GenNames = new SortedDictionary<string, MM_Unit_Type>(StringComparer.CurrentCultureIgnoreCase);

                    if (TotalMW == 0 || float.IsNaN(TotalMW))
                    {
                        foreach (MM_Unit_Type GenType in GenTypes)
                            TotalMW += GenType.MW;
                    }


                    foreach (MM_Unit_Type GenType in MM_Repository.GenerationTypes.Values)
                        if (GenType.Name != "TotalGeneration" && !GenTypes.Contains(GenType) && !String.IsNullOrEmpty(GenType.EMSName) && !GenNames.ContainsKey(GenType.EMSName))
                        {
                            if (GenType.EMSName == null)
                                GenType.EMSName = GenType.Name;
                            if (GenType.Name == null)
                                GenType.Name = GenType.EMSName;
                            GenTypes.Add(GenType);
                            GenNames.Add(GenType.EMSName, GenType);
                        }

                    foreach (MM_Unit_Type GenType in GenNames.Values)
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
                    foreach (MM_Unit_Type GenType in GenTypes)
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
                catch (Exception exp)
                {
                    MM_System_Interfaces.LogError(exp);

                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
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
            PointPairList ForecastedLoad = new PointPairList();
            PointPairList ActualLoad = new PointPairList();
            PointPairList OverriddenLoad= new PointPairList();

            foreach (MM_Load_Forecast_Data LFData in Data_Integration.LoadForecastData)
                if (LFData.Alias_FCA=="SPP")
            {
                ForecastedLoad.Add(XDate.DateTimeToXLDate(LFData.TimeEnd_H), LFData.FLD_H_FCA);
                if (LFData.LD_H_FCA != 0)
                    ActualLoad.Add(XDate.DateTimeToXLDate(LFData.TimeEnd_H), LFData.LD_H_FCA);
                if (LFData.MLD_H_FCA != 0)
                OverriddenLoad.Add(XDate.DateTimeToXLDate(LFData.TimeEnd_H), LFData.MLD_H_FCA);
                
            }


            //Add everything in and update
            ColorSymbolRotator csr = new ColorSymbolRotator();
            zgRealTime.GraphPane.AddCurve("Forecast", ForecastedLoad, csr.NextColor, SymbolType.None);
            if (ActualLoad.Count > 0)
                zgRealTime.GraphPane.AddCurve("Actual", ActualLoad, csr.NextColor, SymbolType.None);
            if (OverriddenLoad.Count > 0)
                zgRealTime.GraphPane.AddCurve("Override", OverriddenLoad, csr.NextColor, SymbolType.None);

            zgRealTime.GraphPane.XAxis.Type = AxisType.Date;
            zgRealTime.GraphPane.XAxis.Title.Text = "Time";
            if (zgRealTime.GraphPane.YAxisList.Count == 0)
                zgRealTime.GraphPane.YAxisList.Add("MW");
            else
                zgRealTime.GraphPane.YAxis.Title.Text = "MW";
            zgRealTime.GraphPane.AxisChange();
            zgRealTime.Invalidate();
        }

    
       /// <summary>
        /// Update SystemWide data
        /// </summary>
        private void UpdateSystemWideData()
        {
            if (Data_Integration.Permissions.showRTHist == false)
                tcMain.TabPages.Remove(tpRealTime);
            
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
            if (Data_Integration.ChartData == null)
                return;

            //Determine all of our column headings
            PointPairList[] OutPoints = new PointPairList[SourceMappings.Length / 2];
            PropertyInfo[] OutSources = new PropertyInfo[SourceMappings.Length / 2];
            String[] Titles = new string[SourceMappings.Length / 2];
            for (int a = 0; a < SourceMappings.Length; a+= 2 )
            {
                OutPoints[a / 2] = new PointPairList();
                OutSources[a / 2] = typeof(MM_Chart_Data).GetProperty(SourceMappings[a + 1]);
                Titles[a / 2] = SourceMappings[a];
            }
                
            //Now, pull in our data
            foreach (MM_Chart_Data ChartRow in Data_Integration.ChartData)
                if (ChartRow.Chart_ID== SourceCategory)
                for (int a = 0; a < OutPoints.Length; a++)
                    OutPoints[a].Add(XDate.DateTimeToXLDate(ChartRow.XT_SAMPLE), (float)OutSources[a].GetValue(ChartRow));

            //Set up our Axis information
            zgRealTime.GraphPane.XAxis.Type = AxisType.Date;
            zgRealTime.GraphPane.XAxis.Title.Text = "Time";            
            if (zgRealTime.GraphPane.YAxisList.Count == 0)
                zgRealTime.GraphPane.YAxisList.Add("MW");
            else
                zgRealTime.GraphPane.YAxis.Title.Text = "MW";


            ColorSymbolRotator csr = new ColorSymbolRotator();
            for (int a = 0; a < OutPoints.Length; a++)
                zgRealTime.GraphPane.AddCurve(Titles[a], OutPoints[a], csr.NextColor, SymbolType.None);
            zgRealTime.GraphPane.AxisChange();
            zgRealTime.Invalidate();
        }
        #endregion

        #region Generation type updating
        private class GenComparer : IComparer<MM_Unit_Type>
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
            public int Compare(MM_Unit_Type x, MM_Unit_Type y)
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
            RequeryPI();
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
            RequeryPI();
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
            if (curve.Tag is DataColumn)
                Axis = (curve.Tag as DataColumn).ColumnName;
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
            DateTime CurDate = XDate.XLDateToDateTime(LastMouseOverTime);
            zgHistoricDetails.GraphPane.Title.Text = CurDate.ToString();
            zgHistoricDetails.GraphPane.Title.IsVisible = true;


            
            Double CurVal;
            Dictionary<String, PieItem> CurPies = new Dictionary<String, PieItem>();
            foreach (PieItem ItemToHandle in zgHistoricDetails.GraphPane.CurveList)
                CurPies.Add(ItemToHandle.Label.Text,ItemToHandle);

            foreach (LineItem lI in LastPane.CurveList)
            {
                try
                {
                    if (!Double.IsNaN(CurVal = Math.Abs((double) InTable.Rows.Find(CurDate)[(lI.Tag as DataColumn).ColumnName])))
                    {
                        Color CurColor = lI.Color;
                        Color NewColor = Color.FromArgb(Math.Min(CurColor.R + 20, 255), Math.Min(CurColor.G + 20, 255), Math.Min(CurColor.B + 20, 255));
                        PieItem NewPie;
                        if (CurPies.TryGetValue(lI.Label.Text, out NewPie))
                        {
                            CurPies.Remove(lI.Label.Text);
                            NewPie.Fill = new Fill(CurColor, NewColor, 90f);
                            NewPie.Value = CurVal;
                        } else
                            NewPie = zgHistoricDetails.GraphPane.AddPieSlice(CurVal, CurColor, NewColor, 90f, 0.0f, lI.Label.Text);
                        if (btnLog.Text == "Log Scale (10)")
                            NewPie.Tag = lI.Label.Text + " " + CurVal.ToString("#,##0.00") + " (" + (lI.Tag as DataColumn).Caption + " in base 10)";
                        else if (btnLog.Text == "Log Scale (e)")
                            NewPie.Tag = lI.Label.Text + " " + CurVal.ToString("#,##0.00") + " (" + (lI.Tag as DataColumn).Caption + " in base e)";
                        else
                            NewPie.Tag = lI.Label.Text + " " + CurVal.ToString("#,##0.00") + " " + (lI.Tag as DataColumn).Caption;
                        NewPie.LabelType = PieLabelType.Name_Value_Percent;
                    }
                } catch (Exception ex)
                {
                    MM_System_Interfaces.LogError(ex);
                }
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
