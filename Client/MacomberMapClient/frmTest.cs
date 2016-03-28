using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Startup;
using MacomberMapCommunications;
using MacomberMapCommunications.Attributes;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.EMS;
using MacomberMapCommunications.WCF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides a test form
    /// </summary>
    public partial class frmTest : Form
    {
        IMM_WCF_Interface Client;
        DuplexChannelFactory<IMM_WCF_Interface> ClientFactory;
        private MM_Server_Callback_Handler CallbackHandler = new MM_Server_Callback_Handler();
        
        /// <summary>
        /// Initialize a new test form
        /// </summary>
        public frmTest()
        {
            InitializeComponent();

            //Add in all known message types
            foreach (Type t in typeof(MM_Line_Data).Assembly.GetTypes())
            {
                RetrievalCommandAttribute FoundAttr = null;
                foreach (Object obj in t.GetCustomAttributes(false))
                    if (obj is RetrievalCommandAttribute)
                        FoundAttr = (RetrievalCommandAttribute)obj;
                if (FoundAttr != null)
                    btnFillData.DropDownItems.Add(t.Name).Tag = FoundAttr.RetrievalCommand;
            }          
        }


        /// <summary>
        /// Fill our datagridview with data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFillData_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            btnFillData.DropDown.Close();

            if (Client == null)
                MessageBox.Show(this, "Not connected!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            else
            {
                DateTime StartTime = DateTime.Now;


                //Search for our target method
                MethodInfo TargetMethod = typeof(IMM_WCF_Interface).GetMethod(e.ClickedItem.Tag.ToString());
                foreach (TypeInfo Interface in typeof(IMM_WCF_Interface).GetInterfaces())
                    if (TargetMethod == null)
                        TargetMethod = Interface.GetMethod(e.ClickedItem.Tag.ToString());
                if (TargetMethod == null)
                    MessageBox.Show(this, "Unable to run command " + e.ClickedItem.Text + ", as this method could not be found.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    dataGridView1.DataSource = TargetMethod.Invoke(Client, null);
                    MessageBox.Show(this, "Loaded " + e.ClickedItem.Text + " in " + (DateTime.Now - StartTime), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Query our application version
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryApplicationVersion_Click(object sender, EventArgs e)
        {
             if (Client == null)
                MessageBox.Show(this, "Not connected!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            else            
                MessageBox.Show("Server name: " + Client.ServerName() + "\r\nServer version: " + Client.ServerVersion() + "\r\nServer uptime: " + Client.ServerUptime(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Test our PI server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestPI_Click(object sender, EventArgs e)
        {
            if (Client == null)
                MessageBox.Show(this, "Not connected!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            else
            {
                DateTime StartTime = DateTime.Now;
                Guid Resp = Client.QueryTags("W_BATESV.LN.*", DateTime.Now.AddDays(-7), DateTime.Now, 10);
                MM_Historic_Query.enumQueryState State = Client.CheckQueryStatus(Resp);
                while (State != MM_Historic_Query.enumQueryState.HadError && State != MM_Historic_Query.enumQueryState.Completed)
                {
                    Application.DoEvents();
                    State = Client.CheckQueryStatus(Resp);
                }
                MM_Historic_Query Results = Client.RetrieveQueryResults(Resp);
                dataGridView1.DataSource = Results.DeserializeTable();
                if (State == MM_Historic_Query.enumQueryState.HadError)
                    MessageBox.Show(this, "An error occurred retrieving PI data.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(this, "Historic data retrieved in " + (DateTime.Now - StartTime).ToString() + ".", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Handle our open button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpen_Click(object sender, EventArgs e)
        {



            NetTcpBinding tcpBinding = MM_Binding_Configuration.CreateBinding();

            ClientFactory = new DuplexChannelFactory<IMM_WCF_Interface>(new MM_Server_Callback_Handler(), tcpBinding, txtServer.Text);
            ClientFactory.Open();
            Client = ClientFactory.CreateChannel();
            frmWindowsSecurityDialog SecurityDialog = new frmWindowsSecurityDialog();
            SecurityDialog.CaptionText = Application.ProductName + " " + Application.ProductVersion;
            SecurityDialog.MessageText = "Enter the credentials to log into Macomber Map Server";
            string Username, Password, Domain;
            Exception LoginError;
            if (SecurityDialog.ShowLoginDialog(out Username, out Password, out Domain))
            {
                Client.HandleUserLogin(Username, Password);
                MessageBox.Show(this, "Connected.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
