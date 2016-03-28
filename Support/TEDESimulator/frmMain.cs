using MacomberMapCommunications.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TEDESimulator.Elements;
using TEDESimulator.Server_Connectivity;

namespace TEDESimulator
{
    /// <summary>
    /// © 2016, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This form provides us a simulator for entering data
    /// </summary>
    public partial class frmMain : Form
    {
        Dictionary<Uri, MM_Server_Information> MMServers = new Dictionary<Uri, MM_Server_Information>();
        SortedDictionary<String, Type> DataTypes = new SortedDictionary<string, Type>();
        IBindingList BoundList = null;
        TcpListener Listener;
        Sim_Builder Builder;

        /// <summary>
        /// Start up our simulator
        /// </summary>
        public frmMain()
        {
            InitializeComponent();
            foreach (Type TypeToAdd in typeof(MacomberMapCommunications.Messages.EMS.MM_Analog_Measurement).Assembly.GetTypes())
                if (TypeToAdd.GetCustomAttributes(typeof(FileNameAttribute), true).Length > 0)
                    DataTypes.Add(TypeToAdd.Name, TypeToAdd);
            cmbDataType.Items.AddRange(DataTypes.Keys.ToArray());
        }

        /// <summary>
        /// When our selected index changes, create our types.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Type TargetType = DataTypes[cmbDataType.Text];
            BoundList = (IBindingList)Activator.CreateInstance(typeof(BindingList<>).MakeGenericType(new[] { TargetType }));

            dgvData.AutoGenerateColumns = false;
            dgvData.Columns.Clear();
            //Now, generate our columns
            foreach (PropertyInfo pI in TargetType.GetProperties())
                if (pI.CanRead && pI.CanWrite)
                    if (pI.PropertyType.IsEnum)
                        dgvData.Columns.Add(new DataGridViewComboBoxColumn() { DataPropertyName = pI.Name, HeaderText = pI.Name, DataSource = Enum.GetValues(pI.PropertyType) });
                    else
                        dgvData.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = pI.Name, HeaderText = pI.Name });
            dgvData.DataSource = BoundList;
        }


        /// <summary>
        /// Send our row to TEDE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            string[] splStr = txtMMServer.Text.Split(':');

            //Create our batch ID
            int BatchID = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;

            using (TcpClient Client = new TcpClient(splStr[0], int.Parse(splStr[1])))
            using (StreamWriter sW = new StreamWriter(Client.GetStream()))
            {
                Type InType = BoundList.GetType().GetGenericArguments()[0];

                sW.WriteLine("#" + BatchID.ToString()+ ","+InType.GetCustomAttribute<FileNameAttribute>().FileName);
                //Write our header
                for (int a = 0; a < dgvData.Columns.Count; a++)
                    sW.Write((a == 0 ? "#" : ",") + dgvData.Columns[a].DataPropertyName);
                sW.WriteLine();

                //Write each row
                foreach (Object OutObj in BoundList)
                {
                    PropertyInfo[] pI = OutObj.GetType().GetProperties();
                    for (int a = 0; a < pI.Length; a++)
                    {
                        Object OutVal = pI[a].GetValue(OutObj);
                        if (OutVal == null)
                            sW.Write(a == 0 ? "" : ",");
                        else if (OutVal is bool)
                            sW.Write((a == 0 ? "" : ",") + ((bool)OutVal ? "T" : "F"));
                        else if (OutVal is DateTime)
                            sW.Write((a == 0 ? "" : ",") + ((DateTime)OutVal - new DateTime(1970, 1, 1)).TotalSeconds.ToString("0"));
                        else
                            sW.Write((a == 0 ? "" : ",") + OutVal.ToString());
                        sW.WriteLine();
                    }
                }
                sW.WriteLine("#" + BatchID.ToString());
            }
        }

        /// <summary>
        /// Handle our listener for commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (Listener == null)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(StartListener));
                btnOpen.BackColor = Color.LightGreen;
            }
            else
            {
                Listener.Stop();
                btnOpen.BackColor = SystemColors.ButtonFace;
            }
        }

        private void StartListener(object state)
        {
            Listener = new TcpListener(IPAddress.Any,int.Parse(txtListningPort.Text));
            Listener.Start();
            while (true)
                try
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessTCPMessage), Listener.AcceptTcpClient());
                }
            catch (Exception ex)
                {
                    MessageBox.Show("Error listening to TCP/IP port: " + ex.ToString());
                }
        }

        
        /// <summary>
        /// Process a TCP/IP message
        /// </summary>
        /// <param name="state"></param>
        private void ProcessTCPMessage(object state)
        { 
            TcpClient Conn = (TcpClient)state;
            using (NetworkStream nS = Conn.GetStream())
            using (StreamReader sRd = new StreamReader(nS))
                WriteLine(sRd.ReadToEnd());
        }

        private delegate void SafeWriteLine(String InLine);

        private void WriteLine(String InLine)
        {
            if (lbIncoming.InvokeRequired)
                lbIncoming.Invoke(new SafeWriteLine(WriteLine), InLine);
            else
                lbIncoming.Items.Add(InLine);
        }

        /// <summary>
        /// Handle the click of the shapefile button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleShapefileClick(object sender, EventArgs e)
        {
            Button Sender = (Button)sender;

            using (OpenFileDialog oFd = new OpenFileDialog() { Filter = "Shapefile (*.shp)|*.shp", Title = "Open Shapefile..." })
            if (oFd.ShowDialog()== DialogResult.OK)
                {
                    Sender.BackColor = Color.LightGreen;
                    Sender.Tag = oFd.FileName;
                }
        }

/// <summary>
/// Handle the click of the previous MM button
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
        private void btrnOpenBaseModel_Click(object sender, EventArgs e)
        {
            Button Sender = (Button)sender;

            using (OpenFileDialog oFd = new OpenFileDialog() { Filter = "Macomber Map Model (*.MM_Model)|*.MM_Model", Title = "Open Macomber Map model..." })
                if (oFd.ShowDialog() == DialogResult.OK)
                {
                    Sender.BackColor = Color.LightGreen;
                    Sender.Tag = oFd.FileName;
                }
        }
        /// <summary>
        /// Select our target
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectTarget_Click(object sender, EventArgs e)
        {

            Button Sender = (Button)sender;
            using (SaveFileDialog sFd = new SaveFileDialog() { Filter = "Macomber Map model (MacomberMap-Combined.MM_Model)|MacomberMap-Combined.MM_Model", FileName = "MacomberMap-Combined.MM_Model", Title = "Save MM Model output" })
                if (sFd.ShowDialog() == DialogResult.OK) 
                {
                    Sender.BackColor = Color.LightGreen;
                    Sender.Tag = sFd.FileName;
                }
        }

        /// <summary>
        /// Select our one-line folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOneLineFolder_Click(object sender, EventArgs e)
        {
            Button Sender = (Button)sender;
            using (SaveFileDialog sFd = new SaveFileDialog() { Filter = "Macomber Map One-line (*.MM_OneLine)|*.MM_OneLine", FileName = "MacomberMapOneLine.MM_OneLine", Title = "Macomber Map One-Line Folder" })
                if (sFd.ShowDialog() == DialogResult.OK)
                {
                    Sender.BackColor = Color.LightGreen;
                    Sender.Tag = Path.GetDirectoryName(sFd.FileName);
                }

        }
        /// <summary>
        /// Generate our file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            Sim_Builder_Configuration BuilderConfig  = new Sim_Builder_Configuration(tpBuildingModels.Controls);
            Builder = new Sim_Builder(BuilderConfig, cmbSubstationModel.Text);
            Builder.Export();
            MessageBox.Show("Export completed.");
        }

        private void btnReExport_Click(object sender, EventArgs e)
        {
            Builder.Savecase = new MacomberMapCommunications.Messages.MM_Savecase();
            Builder.Export();
            MessageBox.Show("Export completed.");
        }

        #region WCF and server controls
        /// <summary>
        /// Retrieve our data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRetrieveData_Click(object sender, EventArgs e)
        {
            if (MM_Server_Interface.Client == null && cmbMacomberMapServer.SelectedIndex != -1)
            {
                MM_Server_Information FoundServer = MM_Server_Interface.MMServers.Values.FirstOrDefault<MM_Server_Information>(t => t.ServerName == cmbMacomberMapServer.Text);
                if (FoundServer != null)
                    LoginToServer(FoundServer.ServerName, FoundServer.ServerURI);
            }

            if (MM_Server_Interface.Client == null || MM_Server_Interface.Client.State != System.ServiceModel.CommunicationState.Opened)
                MessageBox.Show("Not connected to a Macomber Map Server", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            else if (cmbDataType.SelectedItem == null)
                MessageBox.Show("No data type selected", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            else
            {
                Type TargetType = DataTypes[cmbDataType.Text];
                RetrievalCommandAttribute rca = TargetType.GetCustomAttribute<RetrievalCommandAttribute>();
                MethodInfo mI = typeof(MM_WCF_Interface).GetMethod(rca.RetrievalCommand);

                BoundList = (IBindingList)Activator.CreateInstance(typeof(BindingList<>).MakeGenericType(new[] { TargetType }));
                Object InResult = mI.Invoke(MM_Server_Interface.Client, null);
                foreach (Object obj in (IEnumerable)InResult)
                    BoundList.Add(obj);
                dgvData.DataSource = BoundList;

            }

        }

        /// <summary>
        /// Log in to a server
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="ConnectionUri"></param>
        private void LoginToServer(String Title, Uri ConnectionUri)
        {
            frmWindowsSecurityDialog SecurityDialog = new frmWindowsSecurityDialog();
            SecurityDialog.CaptionText = Application.ProductName + " " + Application.ProductVersion;
            SecurityDialog.MessageText = "Enter the credentials to log into Macomber Map Server " + Title;
            string Username, Password, Domain;
            Exception LoginError;

            if (SecurityDialog.ShowLoginDialog(out Username, out Password, out Domain))
            {
                if (MM_Server_Interface.TryLogin(Title, ConnectionUri, Username, Password, out LoginError))
                {
                    MM_Server_Interface.UserName = Username;
                    MM_Server_Interface.Password = Password;

                }
                else
                    MessageBox.Show("Unable to log into Macomber Map Server: " + LoginError.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        /// <summary>
        /// When our timer triggers, update our server information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            MM_Server_Information.UpdateServerInformation(MMServers, cmbMacomberMapServer);
        }
    }
}