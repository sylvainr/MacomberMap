using MacomberMapCommunications.Messages.EMS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacomberMapCommunications.Extensions;

namespace MacomberMapAdministrationConsole
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides our command information view port
    /// </summary>
    public partial class frm_Command_Information : Form
    {
        #region Variable declarations
        /// <summary>Our instance of the command information form</summary>
        public static frm_Command_Information Instance;

        /// <summary>Our collection of EMS commands</summary>
        public static BindingList<MM_EMS_Command> EMSCommands = new BindingList<MM_EMS_Command>();

        /// <summary>Our button for tracking command history</summary>
        public ToolStripButton btnCommandHistory;

        /// <summary>Our collection of servers</summary>
        public SortedDictionary<String, MM_Administrator_Types> Servers = new SortedDictionary<string, MM_Administrator_Types>();
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our command information
        /// </summary>
        public frm_Command_Information(ToolStripButton btnCommandHistory)
        {
            InitializeComponent();
            dgvCommandHistory.CellPainting += dgvCommandHistory_CellPainting;
            dgvCommandHistory.CellFormatting += dgvCommandHistory_CellFormatting;
            dgvCommandHistory.DataSource = EMSCommands;
            this.btnCommandHistory = btnCommandHistory;
            dgvCommandHistory.RowHeadersVisible = false;
            foreach (DataGridViewColumn dCol in dgvCommandHistory.Columns)
                if (dCol.ValueType == typeof(DateTime))
                    dCol.DefaultCellStyle.Format = "MM/dd/yyyy HH:mm:ss";
        }

        /// <summary>
        /// Handle the formatting of a cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is DateTime && (DateTime)e.Value == default(DateTime))
                e.Value = "";
        }
        #endregion

        #region Rendering
        /// <summary>
        /// Handle the painting of our cell
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        private void dgvCommandHistory_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1)
                return;

            else
            {
                //Now, determine our back color;
                Color BackColor = (dgvCommandHistory.SelectedCells.Count == 1 && dgvCommandHistory.SelectedCells[0].RowIndex == e.RowIndex) ? Color.DarkBlue : e.RowIndex % 2 == 0 ? Color.Black : Color.FromArgb(25, 25, 25);

                using (SolidBrush sB = new SolidBrush(BackColor))
                    e.Graphics.FillRectangle(sB, e.CellBounds);

                //Draw our vertical lines if we're selected
                if (dgvCommandHistory.SelectedCells.Count == 1 && dgvCommandHistory.SelectedCells[0].RowIndex == e.RowIndex)
                {
                    e.Graphics.DrawLine(Pens.DarkGray, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Top);
                    e.Graphics.DrawLine(Pens.DarkGray, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                }
                e.PaintContent(e.ClipBounds);
                e.Handled = true;
            }
        }

        /// <summary>
        /// When our command form hides, make sure it's hidden
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
        #endregion

        #region Command handling
        /// <summary>
        /// Our delegate for handling commands
        /// </summary>
        /// <param name="Commands"></param>
        /// <param name="Server"></param>
        public delegate void SafeAddCommands(MM_EMS_Command[] Commands, MM_Administrator_Types Server);


        /// <summary>
        /// Handle an EMS command
        /// </summary>
        /// <param name="Commands"></param>
        /// <param name="Server"></param>
        public void AddCommands(MM_EMS_Command[] Commands, MM_Administrator_Types Server)
        {
            if (btnCommandHistory.Owner.InvokeRequired)
                btnCommandHistory.Owner.Invoke(new SafeAddCommands(AddCommands), Commands, Server);
            else
            {
                foreach (MM_EMS_Command Command in Commands)
                {
                    Command.Server = Server.ToString();
                    if (!Servers.ContainsKey(Server.ToString()))
                    {
                        Servers.Add(Server.ToString(), Server);
                        cmbServer.Items.Add(Server.ToString());
                        if (cmbServer.SelectedIndex == -1)
                            cmbServer.SelectedIndex = 0;
                    }
                    EMSCommands.Add(Command);
                }
                EMSCommands.Sort();
                btnCommandHistory.Text = "Command History: " + EMSCommands.Count.ToString("#,##0");

                //If our checkbox is checked, update the near and far dates
                DateTime FarDate = DateTime.Now.AddDays(5000);
                DateTime NearDate = DateTime.Now.AddDays(-5000);
                foreach (MM_EMS_Command Command in EMSCommands)
                {
                    if (Command.IssuedOn < FarDate)
                        FarDate = Command.IssuedOn;
                    if (Command.IssuedOn > NearDate)
                        NearDate = Command.IssuedOn;
                }
                if (dtStartTime.Checked)
                    dtStartTime.Value = FarDate;
                if (dtEndTime.Checked)
                    dtEndTime.Value = NearDate;
            }
        }
        #endregion

        #region Simulation of commands
        private Thread SimulationThread;

        /// <summary>
        /// Run our simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRunSimulation_Click(object sender, EventArgs e)
        {
            if (SimulationThread == null)
            {
                SimulationThread = new Thread(new ParameterizedThreadStart(RunSimulation));
                SimulationThread.Name = "EMS Command Simulator";
                SimulationThread.IsBackground = true;
                btnRunSimulation.BackColor = Color.LightGreen;
                btnRunSimulation.Text = "Abort Simulation";
                SimulationThread.Start();
            }
            else
            {
                SimulationThread.Abort();
                SimulationThread = null;
                btnRunSimulation.BackColor = SystemColors.ButtonFace;
                btnRunSimulation.Text = "Run Simulation";
            }
        }

        /// <summary>
        /// Run our simulation
        /// </summary>
        /// <param name="state"></param>
        private void RunSimulation(object state)
        {
            DateTime CurrentTime = dtStartTime.Value;
            DateTime StartTime = dtStartTime.Value;
            DateTime EndTime = dtEndTime.Value;

            int CurrentRow = 0;
            while (CurrentRow < EMSCommands.Count)
            {
                MM_EMS_Command ThisCommand = EMSCommands[CurrentRow];

                //Now, send our command
                IssueCommand(ThisCommand, CurrentRow);
                CurrentRow++;
                if (CurrentRow < EMSCommands.Count - 1)
                {
                    MM_EMS_Command NextCommand = EMSCommands[CurrentRow + 1];
                    if (NextCommand.IssuedOn > EndTime)
                        break;
                    Thread.Sleep((int)((NextCommand.IssuedOn - ThisCommand.IssuedOn).TotalSeconds * 1000.0 / (double)nudSpeed.Value));
                }
            }
            MessageBox.Show("Simulation completed.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Issue an EMS command
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="CurrentRow"></param>
        /// <param name="Server"></param>
        private void IssueCommand(MM_EMS_Command Command, int CurrentRow)
        {
            if (InvokeRequired)
                Invoke((MethodInvoker)delegate { IssueCommand(Command, CurrentRow); });
            else
            {
                dgvCommandHistory.CurrentCell = dgvCommandHistory.Rows[CurrentRow].Cells[0];
                dgvCommandHistory.Refresh();
                Servers[cmbServer.SelectedItem.ToString()].SendCommand(Command.BuildOutgoingLine());
            }
        }
        #endregion

        /// <summary>
        /// Load our command list from our log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadCommandsFromLog_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog oFd = new OpenFileDialog() { Title = "Open Macomber Map Server log...", Filter = "Macomber Map Server log (MacomberMapServerLog.html)|MacomberMapServerLog.html", InitialDirectory = Environment.CurrentDirectory })
                if (oFd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessCommandLog), oFd.FileName);
        }

        /// <summary>
        /// Process our command log
        /// </summary>
        /// <param name="state"></param>
        private void ProcessCommandLog(object state)
        {
            List<MM_EMS_Command> NewCommands = new List<MM_EMS_Command>();
            String InLine;
            using (FileStream fS = new FileStream((string)state, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sRd = new StreamReader(fS))
                while ((InLine = sRd.ReadLine()) != null)
                {
                    if (InLine.Contains("Sent") && InLine.Contains("TEDE"))
                    {
                        InLine = InLine.Substring(InLine.IndexOf('>') + 1);
                        InLine = InLine.Substring(0, InLine.IndexOf('<'));
                        String[] splStr = InLine.Split(' ');
                        String[] splCommand = splStr[5].Split(',');
                        NewCommands.Add(new MM_EMS_Command()
                                 {
                                     UserName = splStr[1],
                                     CommandType = splCommand[0],
                                     Application = splCommand[1],
                                     Family = splCommand[2],
                                     CommandName = splCommand[3],
                                     Database = splCommand[4],
                                     Record = splCommand[5],
                                     Field = splCommand[6],
                                     TEID = Convert.ToInt32(splCommand[7]),
                                     Value = splCommand[8],
                                     IssuedOn = DateTime.Parse(splStr[7] + " " + splStr[8] + " " + splStr[9])
                                 });
                    }
                }
            Invoke((MethodInvoker)delegate
            {
                EMSCommands.Clear();
                AddCommands(NewCommands.ToArray(), Servers[cmbServer.SelectedItem.ToString()]);
            });
        }

        /// <summary>
        /// When our cell changes, update our start time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandHistory_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dgvCommandHistory.SelectedCells.Count != 1)
                return;
            try
            {
                MM_EMS_Command Cmd = (MM_EMS_Command)dgvCommandHistory.Rows[dgvCommandHistory.SelectedCells[0].RowIndex].DataBoundItem;
                dtStartTime.Value = Cmd.IssuedOn;
                dgvCommandHistory.Invalidate();
            }
            catch { }
        }
    }
}                    
