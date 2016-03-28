using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.External;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Menuing;
using MacomberMapClient.User_Interfaces.NetworkMap;
using MacomberMapClient.User_Interfaces.Summary;
using MacomberMapCommunications.Messages.EMS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Blackstart
{
    /// <summary>
    /// © 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class displays the history of all EMS commands issued to the system
    /// </summary>
    public partial class MM_EMS_Command_Display : MM_Form
    {
        #region Variable declarations
        /// <summary>Our menu handler for our application</summary>
        private MM_Popup_Menu PopupItemMenu = new MM_Popup_Menu();

        /// <summary>Our main map</summary>
        public MM_Network_Map_DX nMap;

        /// <summary>Our base data handler</summary>
        public MM_DataSet_Base BaseData = new MM_DataSet_Base("Commands");

        /// <summary>Our collection of commands</summary>
        private MM_DataGrid_View<MM_EMS_Command_Client> dgvCommands = new MM_DataGrid_View<MM_EMS_Command_Client>();

        /// <summary>Our collection of EMS server commands</summary>
        private MM_EMS_Command_Client[] EMSServerCommands = new MM_EMS_Command_Client[0];
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize an EMS commands window
        /// </summary>
        /// <param name="nMap"></param>
        public MM_EMS_Command_Display(MM_Network_Map_DX nMap)
        {
            InitializeComponent();
            this.nMap = nMap;
            this.Title = this.Text;
            olView.BackColor = Color.Black;
        }

        /// <summary>
        /// Auto-hide our form when it becomes visible
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            this.Hide();
            olView.BackColor = Color.Black;
            Data_Integration.EMSCommandAdded += Data_Integration_EventAdded;
            Data_Integration.EMSCommandsCleared += Data_Integration_EMSCommandsCleared;
            
            Data_Integration_EventAdded(this, new EMSCommandEventArgs(Data_Integration.EMSCommands.ToArray()));
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
        #endregion

        /// <summary>
        /// Create a seperate thread to run the communications viewer, and run it.
        /// </summary>
        /// <param name="nMap"></param>
        /// <param name="MenuItem"></param>
        /// <returns></returns>
        public static void CreateInstanceInSeparateThread(ToolStripMenuItem MenuItem, MM_Network_Map_DX nMap)
        {           
            Thread CommandThread = new Thread(new ParameterizedThreadStart(InstantiateForm));
            CommandThread.Name = "EMS Command Window";
            CommandThread.SetApartmentState(ApartmentState.STA);
            CommandThread.Start(new object[] { MenuItem, nMap });
        }

        /// <summary>
        /// Instantiate a comm viewer
        /// </summary>
        /// <param name="state">The state of the form</param>
        private static void InstantiateForm(object state)
        {
            object[] State = (object[])state;
            using (MM_EMS_Command_Display bDisp = new MM_EMS_Command_Display(State[1] as MM_Network_Map_DX))
            {
                (State[0] as ToolStripMenuItem).Tag = bDisp;
                Data_Integration.DisplayForm(bDisp, Thread.CurrentThread);
            }
        }

        FileSystemWatcher fsw = null;

        /// <summary>
        /// Synch our command display against our core system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSynch_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog oFd = new OpenFileDialog() { Title = "Select the long log file", Filter="Log files (long_log_*)|long_log_*"})
            if (oFd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (fsw != null)
                    fsw.Dispose();
                fsw = new FileSystemWatcher(Path.GetDirectoryName(oFd.FileName), Path.GetFileName(oFd.FileName));
                fsw.Changed += fsw_Changed;
                fsw.EnableRaisingEvents = true;
                fsw_Changed(fsw, new FileSystemEventArgs(WatcherChangeTypes.All, Path.GetDirectoryName(oFd.FileName), Path.GetFileName(oFd.FileName)));
            }                        
        }

        /// <summary>
        /// Show/hide our one-line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowOneLine_Click(object sender, EventArgs e)
        {
            btnShowOneLine.Checked ^= true;
            splMain.Panel2Collapsed = !btnShowOneLine.Checked;
        }


        /// <summary>
        /// Handle an update file system watch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ReadAndUpdateEMSFile), e.FullPath);
        }

        /// <summary>
        /// Read and update our EMS files
        /// </summary>
        /// <param name="state"></param>
        private void ReadAndUpdateEMSFile(object state)
        {
            //First, pause 1/2 second
            Thread.Sleep(500);

            //Now, read all of our lines in from our file.
            String[] InLines = File.ReadAllLines((string)state);
            DateTime FoundDate;
            String TrimLine;

            List<MM_EMS_Command> DisplayCommands = new List<MM_EMS_Command>();
            List<String> UnknownTags = new List<string>();
            for (int a = 0; a < InLines.Length; a++)
            {
                String InLine = InLines[a];
                if (InLine.Length > 20 && DateTime.TryParseExact(InLine.Substring(0, 20), "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out FoundDate))
                {
                    String CurrentLine = InLine.Substring(21).Trim();
                    int CurRow = a + 1;

                    while (CurRow < InLines.Length &&  !String.IsNullOrEmpty(TrimLine = InLines[CurRow].Trim()) && (TrimLine.Length < 20 ||                         !DateTime.TryParseExact(InLines[CurRow].Substring(0, 20), "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out FoundDate)))
                    {
                        CurrentLine += (Char.IsNumber(TrimLine[0]) ? "" : " ") + TrimLine;
                        CurRow++;
                    }

                    if (!string.IsNullOrEmpty(CurrentLine))
                        ProcessLine(ref CurrentLine, DisplayCommands, UnknownTags, FoundDate);
                }
            }

          

            UpdateCommands(DisplayCommands);
            
            foreach (String str in UnknownTags)
                Console.WriteLine("Unknown tag: " + str);
        }

        private delegate void SafeUpdateCommands(List<MM_EMS_Command> Commands);

        /// <summary>
        /// Update our commands on the system
        /// </summary>
        /// <param name="Commands"></param>
        private void UpdateCommands(List<MM_EMS_Command> Commands)
        {
            if (InvokeRequired)
                Invoke(new SafeUpdateCommands(UpdateCommands), Commands);
            else
            {
                dgvCommands.Elements.Clear();
                Data_Integration_EventAdded(this, new EMSCommandEventArgs(Data_Integration.EMSCommands.ToArray()));
                List<MM_EMS_Command_Client> NewCommands = new List<MM_EMS_Command_Client>();

                foreach (MM_EMS_Command Command in Commands)
                {
                    MM_EMS_Command_Client NewCommand = new MM_EMS_Command_Client(Command, this, MM_EMS_Command_Client.enumSource.EMSServer);
                    dgvCommands.Elements.Add(NewCommand);
                    NewCommands.Add(NewCommand);
                }
                EMSServerCommands = NewCommands.ToArray();
            }
        }

        /// <summary>
        /// Process a line
        /// </summary>
        /// <param name="CurrentLine"></param>
        /// <param name="DisplayCommands"></param>
        /// <param name="UnknownTags"></param>
        /// <param name="SimTime"></param>
        private void ProcessLine(ref string CurrentLine, List<MM_EMS_Command> DisplayCommands, List<String> UnknownTags, DateTime SimTime)
        {
            String[] splStr = CurrentLine.Split(new char[] { ' ',',' }, StringSplitOptions.RemoveEmptyEntries);
            MM_EMS_Command Command=null;
            try
            {
                if (splStr[0] == "DTS300")
                {
                    if (splStr[4] == "CB")
                    {
                        foreach (MM_Breaker_Switch BreakerSwitch in MM_Repository.Substations[splStr[7]].BreakerSwitches)
                            if (BreakerSwitch.Name.Equals(splStr[5], StringComparison.CurrentCultureIgnoreCase))
                                Command = new MM_EMS_Command() { Value = splStr[2], UserName = splStr[9], SimulatorTime = SimTime, Field = splStr[4], TEID = BreakerSwitch.TEID };
                    }
                    else
                    { }
                }
                else if (splStr[0] == "DY308")
                {
                    MM_Substation Site = MM_Repository.Substations[splStr[5]];
                    Command = new MM_EMS_Command() { SimulatorTime = SimTime, TEID = Site.TEID, UserName = String.Join(" ", splStr, 9, splStr.Length - 9) };
                }

                else if (splStr[0] == "MSC004" || splStr[0] == "DY016")
                {
                    MM_Substation Sub = MM_Repository.Substations[splStr[2]];
                    MM_Element FoundElem = null;
                    //Find our element
                    foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                        if (Elem.Substation == Sub && Elem.ElemType != null && Elem.ElemType.Acronym.Equals(splStr[3], StringComparison.CurrentCultureIgnoreCase))
                            if (Elem.Name.Equals(splStr[4], StringComparison.CurrentCultureIgnoreCase) || (Elem is MM_Transformer && ((MM_Transformer)Elem).PrimaryWinding.Name.Equals(splStr[4], StringComparison.CurrentCultureIgnoreCase)))
                                FoundElem = Elem;

                    if (splStr[0] == "MSC004")
                        if (splStr[6] == "TOGGLED")
                            Command = new MM_EMS_Command() { Field = splStr[5], Value = splStr[8].TrimEnd(','), UserName = splStr[10], SimulatorTime = SimTime, TEID = FoundElem.TEID };
                        else
                            Command = new MM_EMS_Command() { Field = splStr[5], Value = splStr[7].TrimEnd(','), OldValue = splStr[9], UserName = splStr[11], SimulatorTime = SimTime, TEID = FoundElem.TEID };
                    else
                    {
                        int From = Array.IndexOf(splStr, "FROM");

                        Command = new MM_EMS_Command() { Field = splStr[5], Value = splStr[7].TrimEnd(','), OldValue = splStr[9], UserName = (From==-1?"?": splStr[From+1] + splStr[From+2]), SimulatorTime = SimTime, TEID = FoundElem.TEID };
                    }

                }

                else if (!UnknownTags.Contains(splStr[0]))
                    UnknownTags.Add(splStr[0]);

                if (Command != null)
                    DisplayCommands.Add(Command);
                CurrentLine = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to process line " + CurrentLine + ": " + ex.ToString());
            }
        }

        /// <summary>
        /// Allow our list to be exported to Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sFd = new SaveFileDialog())
            {
                sFd.Title = "Export Excel spreadsheet";
                sFd.Filter = "Excel Workbook 2007 (*.xlsx)|*.xlsx";
                if (sFd.ShowDialog() == DialogResult.OK)
                {
                    DataSet OutSet = new DataSet();
                    DataTable OutCommand = new DataTable("EMS_Commands");
                    OutSet.Tables.Add(OutCommand);
                    foreach (ColumnHeader colHead in dgvCommands.Columns)
                        OutCommand.Columns.Add(colHead.Text).ColumnName = colHead.Text;
                    foreach (DataGridViewRow dgvr in dgvCommands.Rows)
                    {
                        DataRow NewRow = OutCommand.NewRow();
                        for (int a = 0; a < dgvCommands.Columns.Count; a++)
                            NewRow[a] = dgvr.Cells[a].Value;
                        OutCommand.Rows.Add(NewRow);
                    }
                    using (MM_Excel_Exporter Excel = new MM_Excel_Exporter(OutSet, sFd.FileName))
                        Excel.ShowDialog(this);
                }
            }
        }

        /// <summary>
        /// Handle our selection changing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommands_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCommands.SelectedCells.Count != 1)
                return;

            MM_EMS_Command_Client Command = (MM_EMS_Command_Client)dgvCommands.Rows[dgvCommands.SelectedCells[0].RowIndex].DataBoundItem;
            MM_Element SelectedObject = Command.BaseElement;

            MM_Element HighlightElement = SelectedObject is MM_Substation ? null : SelectedObject;
            if (SelectedObject is MM_Line)
                SelectedObject = SelectedObject.Contingencies[0];
            else if (SelectedObject.Substation != null)
                SelectedObject = SelectedObject.Substation;
            this.UseWaitCursor = true;

            if (SelectedObject == olView.BaseElement)
                olView.HighlightElement(HighlightElement);
            else if (olView.DataSource == null)
            {
                olView.SetControls(SelectedObject, nMap, BaseData, null, Data_Integration.NetworkSource);
                olView.HighlightElement(HighlightElement);
            }
            else
                try
                {
                    olView.LoadOneLine(SelectedObject, HighlightElement);
                    olView.HighlightElement(HighlightElement);
                }
                catch { }
            this.UseWaitCursor = false;
            dgvCommands.Invalidate();
        }

        /// <summary>
        /// Handle our mouse click on our commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommands_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                PopupItemMenu.Show(this, e.Location, ((MM_EMS_Command_Client)dgvCommands.Rows[e.RowIndex].DataBoundItem).BaseElement, true);
        }

        #region Command class for display
        /// <summary>
        /// This class overloads our commands for visibility
        /// </summary>
        public class MM_EMS_Command_Client : MM_Bound_Element
        {
            /// <summary>Our command associated with the display version</summary>
            public MM_EMS_Command BaseCommand;

            /// <summary>The source of our command</summary>
            public enumSource Source;

            /// <summary>Our source type</summary>
            public enum enumSource
            {
                /// <summary>A command from an operator action</summary>
                OperatorAction,
            /// <summary>A command /info from the EMS server</summary>
                EMSServer
            }

            /// <summary>
            /// Initialize a new command client
            /// </summary>
            /// <param name="BaseCommand"></param>
            /// <param name="Disp"></param>
            public MM_EMS_Command_Client(MM_EMS_Command BaseCommand, Control Disp, enumSource Source):
                base(MM_Repository.TEIDs[BaseCommand.TEID], Disp)
            {
                this.Source = Source;
                this.BaseCommand = BaseCommand;
            }

            /// <summary></summary>
            public String UserName { get { return BaseCommand.UserName; } }

            /// <summary></summary>
            public String ComputerName { get { return BaseCommand.ComputerName; } }

            /// <summary></summary>
            public DateTime IssuedOn { get { return BaseCommand.IssuedOn; } }
            
            /// <summary></summary>
            public DateTime SimulatorTime { get { return BaseCommand.SimulatorTime; } }
            
            /// <summary></summary>
            public MM_Element_Type Type { get { return BaseElement.ElemType; } }
            
            /// <summary></summary>
            public MM_Substation Substation { get { return BaseElement.Substation; } }

            /// <summary></summary>
            public String Name { get { return BaseElement.Name; } }

            /// <summary></summary>
            public String Operator { get { return BaseElement.Operator == null ? "?" : BaseElement.Operator.Alias; } }

            /// <summary></summary>
            public String Owner { get { return BaseElement.Owner == null ? "?" : BaseElement.Owner.Alias; } }

            /// <summary></summary>
            public String CommandType { get { return BaseCommand.CommandType; } }

            /// <summary></summary>
            public String CommandName { get { return BaseCommand.CommandName; } }

            /// <summary></summary>
            public String Family { get { return BaseCommand.Family; } }

            /// <summary></summary>
            public String Application { get { return BaseCommand.Application; } }

            /// <summary></summary>
            public String Database { get { return BaseCommand.Database; } }

            /// <summary></summary>
            public String Record { get { return BaseCommand.Record; } }

            /// <summary></summary>
            public String Field { get { return BaseCommand.Field; } }

            /// <summary></summary>
            public String Value { get { return BaseCommand.Value; } }

            /// <summary></summary>
            public String OldValue { get { return BaseCommand.OldValue; } }
        }
        #endregion

        #region Event updating
        /// <summary>
        /// This class handles our events around new commands
        /// </summary>
        public class EMSCommandEventArgs : EventArgs
        {
            /// <summary>
            /// Our command
            /// </summary>
            public MM_EMS_Command[] Command;
            /// <summary>
            /// Initialize a new command event args
            /// </summary>
            /// <param name="Command"></param>
            public EMSCommandEventArgs(MM_EMS_Command[] Command)
            {
                this.Command = Command;
            }
        }


        /// <summary>
        /// Handle our EMS commands clearing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_Integration_EMSCommandsCleared(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler(Data_Integration_EMSCommandsCleared), sender, e);
            else
            {
                dgvCommands.Elements.Clear();
                foreach (MM_EMS_Command_Client EMSServerCommand in EMSServerCommands)
                    dgvCommands.Elements.Add(EMSServerCommand);

                Text = "Macomber Map Command History";
            }
        }


        /// <summary>
        /// Handle a new event being added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Data_Integration_EventAdded(object sender, MM_EMS_Command_Display.EMSCommandEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler<EMSCommandEventArgs>(Data_Integration_EventAdded), sender, e);
            else
            {
                foreach (MM_EMS_Command Command in e.Command)
                    try
                    {
                        if (MM_Repository.TEIDs.ContainsKey(Command.TEID))
                            dgvCommands.Elements.Add(new MM_EMS_Command_Client(Command, this, MM_EMS_Command_Client.enumSource.OperatorAction));
                    }
                    catch { }
                Text = "Macomber Map Command History: " + dgvCommands.Elements.Count.ToString("#,##0");
            }
        }

      
        #endregion

        
    }
}
