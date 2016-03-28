using MacomberMapClient.Data_Elements.Blackstart;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Menuing;
using MacomberMapClient.User_Interfaces.NetworkMap;
using MacomberMapClient.User_Interfaces.OneLines;
using MacomberMapClient.User_Interfaces.Summary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Blackstart
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class displays blackstart corridors
    /// </summary>
    public partial class MM_Blackstart_Display : MM_Form
    {
        /// <summary>The main network map</summary>
        public MM_Network_Map_DX nMap;

        /// <summary>Our base data handler</summary>
        public MM_DataSet_Base BaseData = new MM_DataSet_Base("Blackstart");

        /// <summary>Our popup menu</summary>
        public MM_Popup_Menu mnuMain = new MM_Popup_Menu();

        /// <summary>
        /// Initialize our blackstart corridor display
        /// </summary>
        public MM_Blackstart_Display(MM_Network_Map_DX nMap)
        {
            InitializeComponent();
            this.nMap = nMap;
            this.Title = "Blackstart Corridor Information";
            cmbCorridor.Items.AddRange(MM_Repository.BlackstartCorridors.Values.ToArray());
            foreach (MM_Blackstart_Corridor Corridor in MM_Repository.BlackstartCorridors.Values)
            {
                TreeNode tvCorridor = new TreeNode(Corridor.Name) { Tag = Corridor };
                foreach (MM_Blackstart_Corridor_Target Target in Corridor.Blackstart_Targets)
                {
                    TreeNode tvTarget = new TreeNode(Target.Target) { Tag = Target };
                    tvCorridor.Nodes.Add(tvTarget);
                    AddLineElements(Target.Primary, "Primary", tvTarget);
                    AddLineElements(Target.Secondary, "Secondary", tvTarget);
                }
                tvSummary.Nodes.Add(tvCorridor);
            }
            //olView.GetType().GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(olView, true);
            //tvSummary.GetType().GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tvSummary, true);
            //lvItems.GetType().GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(lvItems, true);
            //this.DoubleBuffered = true;
        }

        /// <summary>
        /// Add in our information on line elements
        /// </summary>
        /// <param name="OutElems"></param>
        /// <param name="Title"></param>
        /// <param name="ParentNode"></param>
        private void AddLineElements(MM_Blackstart_Corridor_Element[] OutElems, String Title, TreeNode ParentNode)
        {
            if (OutElems == null || OutElems.Length == 0)
                return;
            List<MM_Line> LineElems = new List<MM_Line>();
            foreach (MM_Blackstart_Corridor_Element Elem in OutElems)
                if (Elem.AssociatedElement is MM_Line)
                    LineElems.Add(Elem.AssociatedElement as MM_Line);
            ParentNode.Nodes.Add(new TreeNode(Title) { Tag = new KeyValuePair<String, MM_Line[]>(Title, LineElems.ToArray()) });
        }

        
        /// <summary>
        /// Handle the user's clicking on a one-line element
        /// </summary>
        /// <param name="ClickedElement"></param>
        /// <param name="e"></param>
        private void OneLine_OneLineElementClicked(MM_OneLine_Element ClickedElement, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            //viewPropertyPage.SetElement(ClickedElement.BaseElement);
            //else
            {
                MM_Popup_Menu c = new MM_Popup_Menu();
                if (ClickedElement.ElemType == MM_OneLine_Element.enumElemTypes.Descriptor || ClickedElement.ElemType == MM_OneLine_Element.enumElemTypes.SecondaryDescriptor)
                    c.Show(ClickedElement, e.Location, ClickedElement.ParentElement.BaseElement, true, olView.DataSourceApplication);
                else
                    c.Show(ClickedElement, e.Location, ClickedElement.BaseElement, true, olView.DataSourceApplication);
                /*
                //ClickedElement.BaseElement.BuildMenuItems(c, false, true);
                if (ClickedElement.ConnectedElements != null)
                {
                    c.Items.Add("-");
                    foreach (Int32 Conn in ClickedElement.ConnectedElements)
                        c.Items.Add(" Connected to: " + viewOneLine.Elements[Conn].BaseElement.ElemType + " " + viewOneLine.Elements[Conn].BaseElement.Name);
                }
                c.Show(ClickedElement, e.Location);*/
            }
        }

        /// <summary>
        /// When the form is first shown, hide it.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            this.Hide();
            base.OnShown(e);
        }

        /// <summary>
        /// Handle the closing event by instead hiding the window
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        /// <summary>
        /// Create a seperate thread to run the communications viewer, and run it.
        /// </summary>
        /// <param name="nMap"></param>
        /// <param name="MenuItem"></param>
        /// <returns></returns>
        public static void CreateInstanceInSeparateThread(ToolStripMenuItem MenuItem, MM_Network_Map_DX nMap)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(InstantiateForm), new object[] { MenuItem, nMap });
        }
        /// <summary>
        /// Instantiate a comm viewer
        /// </summary>
        /// <param name="state">The state of the form</param>
        private static void InstantiateForm(object state)
        {
            object[] State = (object[])state;
            using (MM_Blackstart_Display bDisp = new MM_Blackstart_Display(State[1] as MM_Network_Map_DX))
            {
                (State[0] as ToolStripMenuItem).Tag = bDisp;
                Data_Integration.DisplayForm(bDisp, Thread.CurrentThread);
            }
        }

        /// <summary>
        /// When the corridor button is checked, alter our corridor view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbCorridor_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbCorridorTarget.Items.Clear();
            MM_Blackstart_Corridor Corridor = cmbCorridor.SelectedItem as MM_Blackstart_Corridor;
            if (Corridor != null)
            {
                cmbCorridorTarget.Items.AddRange(Corridor.Blackstart_Targets);
                cmbCorridorTarget.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// When the corridor target changes, update the primary/secondary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbCorridorTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbPrimarySecondary.Items.Clear();
            MM_Blackstart_Corridor_Target Target = cmbCorridorTarget.SelectedItem as MM_Blackstart_Corridor_Target;
            if (Target != null)
            {
                if (Target.Primary != null)
                    cmbPrimarySecondary.Items.Add("Primary");
                if (Target.Secondary != null)
                    cmbPrimarySecondary.Items.Add("Secondary");
                if (cmbPrimarySecondary.Items.Count > 0)
                    cmbPrimarySecondary.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// When the primary/secondary path is selected, display all of our elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbPrimarySecondary_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                lvItems.Columns.Clear();
                lvItems.Items.Clear();
                lvItems.View = View.Details;

                lvItems.Columns.Add("#");
                lvItems.Columns.Add("Action");
                lvItems.Columns.Add("Subsatation");
                lvItems.Columns.Add("Type");
                lvItems.Columns.Add("Element");
                lvItems.Columns.Add("Operator");
                lvItems.FullRowSelect = true;
                lvItems.CheckBoxes = true;
                MM_Blackstart_Corridor_Target Target = cmbCorridorTarget.SelectedItem as MM_Blackstart_Corridor_Target;
                FieldInfo fI = Target.GetType().GetField(cmbPrimarySecondary.Text);
                if (Target != null && fI != null)
                {
                    MM_Blackstart_Corridor_Element[] Elems = fI.GetValue(Target) as MM_Blackstart_Corridor_Element[];
                    MM_Substation LastSub = null;
                    for (int a = 0; a < Elems.Length; a++)
                        try
                        {
                            ListViewItem lvI = new ListViewItem((a + 1).ToString("#,##0"));
                            lvI.UseItemStyleForSubItems = true;
                            if (MM_Server_Interface.Client != null && Array.IndexOf(Data_Integration.UserOperatorships, 999999) == -1 && Array.IndexOf(Data_Integration.UserOperatorships, Elems[a].AssociatedElement.Operator.TEID) == -1)
                                lvI.ForeColor = Color.Gray;

                            lvI.SubItems.Add(Elems[a].Action.ToString());

                            if (Elems[a].AssociatedElement != null)
                            {
                                MM_Element Elem = Elems[a].AssociatedElement;


                                if (Elem is MM_Line)
                                {
                                    MM_Line Line = (MM_Line)Elem;
                                    if (LastSub == Line.Substation2)
                                        lvI.SubItems.Add(Line.Substation2.Name + " to " + Line.Substation1.Name);
                                    else
                                        lvI.SubItems.Add(Line.Substation1.Name + " to " + Line.Substation2.Name);
                                    LastSub = Line.Substation2;
                                }
                                else
                                {
                                    lvI.SubItems.Add(Elem.Substation.Name);
                                    LastSub = Elem.Substation;
                                }

                                lvI.SubItems.Add(Elem.ElemType.Name);
                                lvI.SubItems.Add(MM_Repository.TitleCase(Elem.Name));
                                lvI.SubItems.Add(Elem.Operator.Alias.Substring(0, 1) + MM_Repository.TitleCase(Elem.Operator.Alias.Substring(1)));
                            }
                            else
                            {
                                if (Elems[a].Substation == null)
                                    lvI.SubItems.Add("?");
                                else
                                    lvI.SubItems.Add(MM_Repository.TitleCase(Elems[a].Substation.Name));
                                lvI.SubItems.Add("?");
                                lvI.SubItems.Add("?");
                                lvI.SubItems.Add("?");
                                lvI.ForeColor = Color.Red;
                                lvI.UseItemStyleForSubItems = true;
                            }
                            lvI.Tag = Elems[a];
                            lvItems.Items.Add(lvI);
                        }
                        catch (Exception ex)
                        {
                            MM_System_Interfaces.LogError(ex);
                        }
                    tmrUpdate_Tick(tmrUpdate, EventArgs.Empty);
                    lvItems.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Handle the selecting of an index to open up a one-line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvItems.SelectedItems.Count == 1 && lvItems.UseWaitCursor == false)
            {
                try
                {

                    MM_Blackstart_Corridor_Element Elem = lvItems.SelectedItems[0].Tag as MM_Blackstart_Corridor_Element;
                    lvItems.UseWaitCursor = true;
                    if (Elem.Substation != olView.BaseElement)
                        if (olView.DataSource == null)
                            olView.SetControls(Elem.Substation, nMap, BaseData, null, Data_Integration.NetworkSource);
                        else
                            olView.LoadOneLine(Elem.Substation, Elem.AssociatedElement);
                    else
                        olView.HighlightElement(Elem.AssociatedElement);

                    lvItems.UseWaitCursor = false;
                }
                catch
                {
                    //?
                }

            }
        }

        /// <summary>
        /// Handle a mouse right-click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvItems_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                ListViewHitTestInfo hti = lvItems.HitTest(e.Location);
                if (hti.Item == null || e.Button != MouseButtons.Right)
                    return;

                MM_Blackstart_Corridor_Element Elem = hti.Item.Tag as MM_Blackstart_Corridor_Element;
                mnuMain.Show(lvItems, e.Location, Elem.AssociatedElement, true);
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Every four seconds, update our element status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            //Update our summary list
            foreach (TreeNode tnCorridor in tvSummary.Nodes)
            {

                int WorstCorridor = 0;
                foreach (TreeNode tnTarget in tnCorridor.Nodes)
                {
                    int WorstTarget = 0;
                    foreach (TreeNode tnPriSec in tnTarget.Nodes)
                    {
                        int WorstPriSec = 0;
                        KeyValuePair<String, MM_Line[]> LineCollection = (KeyValuePair<String, MM_Line[]>)tnPriSec.Tag;
                        int Energized = 0;
                        foreach (MM_Line Line in LineCollection.Value)
                            if (float.IsNaN(Line.MVAFlow))
                            { }
                            else if (Math.Abs(Line.MVAFlow) > MM_Repository.OverallDisplay.EnergizationThreshold)
                            {
                                WorstPriSec = Math.Max(WorstPriSec, 1);
                                Energized++;
                            }
                            else
                                WorstPriSec = Math.Max(WorstPriSec, 2);
                        tnPriSec.Text = LineCollection.Key + " (" + (Energized * 100 / LineCollection.Value.Length).ToString() + "% Lines)";
                        tnPriSec.ImageIndex = tnPriSec.SelectedImageIndex = WorstPriSec;
                        WorstTarget = Math.Max(WorstPriSec, WorstTarget);
                    }
                    WorstCorridor = Math.Max(WorstCorridor, WorstTarget);
                    tnTarget.ImageIndex = tnTarget.SelectedImageIndex = WorstTarget;
                    WorstCorridor = Math.Max(WorstCorridor, WorstTarget);
                }
                tnCorridor.ImageIndex = tnCorridor.SelectedImageIndex = WorstCorridor;
            }


            //Update our list status of our current items
            foreach (ListViewItem lvI in lvItems.Items)
            {
                MM_Element Elem = (lvI.Tag as MM_Blackstart_Corridor_Element).AssociatedElement;
                float DecisionPoint = 1;// float.NaN;
                if (Elem is MM_Breaker_Switch)
                {
                    MM_Breaker_Switch.BreakerStateEnum State = (Elem as MM_Breaker_Switch).BreakerState;
                    if (State == MM_Breaker_Switch.BreakerStateEnum.Closed)
                        DecisionPoint = 1;
                    else if (State == MM_Breaker_Switch.BreakerStateEnum.Open)
                        DecisionPoint = 0;
                }
                else if (Elem is MM_Line)
                    DecisionPoint = (Elem as MM_Line).MVAFlow;
                else if (Elem is MM_Load)
                    DecisionPoint = (float)Math.Sqrt(Math.Pow((Elem as MM_Load).Estimated_MW, 2) + Math.Pow((Elem as MM_Load).Estimated_MVAR, 2));
                else if (Elem is MM_Unit)
                    DecisionPoint = (float)Math.Sqrt(Math.Pow((Elem as MM_Unit).Estimated_MW, 2) + Math.Pow((Elem as MM_Unit).Estimated_MVAR, 2));

                if (float.IsNaN(DecisionPoint))
                    lvI.ImageIndex = 0;
                else if (Math.Abs(DecisionPoint) > MM_Repository.OverallDisplay.EnergizationThreshold)
                    lvI.ImageIndex = 1;
                else
                    lvI.ImageIndex = 2;

            }
        }

        /// <summary>
        /// Handle a node selection change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvSummary_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if ((e.Action == TreeViewAction.ByKeyboard || e.Action == TreeViewAction.ByMouse || e.Action == TreeViewAction.Unknown) && e.Node.Tag is KeyValuePair<String, MM_Line[]>)
            {
                cmbCorridor.SelectedItem = e.Node.Parent.Parent.Tag as MM_Blackstart_Corridor;
                cmbCorridorTarget.SelectedItem = e.Node.Parent.Tag as MM_Blackstart_Corridor_Target;
                cmbPrimarySecondary.SelectedItem = ((KeyValuePair<String, MM_Line[]>)e.Node.Tag).Key;



            }

        }
    }
}
