using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapAdministrationConsole
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class mimics the old input box VB style for communications to a client
    /// </summary>
    public partial class MM_Input_Box : Form
    {
        /// <summary>The message to send to our user(s)</summary>
        public String Message { get { return txtMessage.Text; } set { txtMessage.Text = value; } }

        /// <summary>The icon to send</summary>
        public MessageBoxIcon TargetIcon { get { return (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), cmbIcon.SelectedItem.ToString()); } }

        /// <summary>
        /// Initialize our input box
        /// </summary>
        public MM_Input_Box()
        {
            InitializeComponent();
            foreach (String str in Enum.GetNames(typeof(MessageBoxIcon)))
                cmbIcon.Items.Add(str);
            cmbIcon.SelectedIndex = 0;
        }

        /// <summary>
        /// Offer our show dialog form
        /// </summary>
        /// <param name="Owner"></param>
        /// <param name="Message"></param>
        /// <param name="Title"></param>
        /// <returns></returns>
        public DialogResult ShowDialog(IWin32Window Owner, String Message, String Title)
        {
            lblMessage.Text = Message;
            this.Text = Title;
            return ShowDialog(Owner);
        }
    }
}
