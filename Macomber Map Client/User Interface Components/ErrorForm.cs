using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class holds a simple window showing all errors
    /// </summary>
    public partial class ErrorForm : Form
    {
        /// <summary>
        /// Initialize the error form
        /// </summary>
        public ErrorForm()
        {
            InitializeComponent();
        }


        private delegate void SafeLogError(String ErrorMessage);

        /// <summary>
        /// Log an error to the console window
        /// </summary>
        /// <param name="ErrorMessage">The text of the error</param>
        public void LogError(String ErrorMessage)
        {
            if (lbErrors.InvokeRequired)
                lbErrors.BeginInvoke(new SafeLogError(LogError), ErrorMessage);
            else
                lbErrors.Items.Add(ErrorMessage);
        }

        /// <summary>
        /// Handle the user's closing of the form by making it invisible instead
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }
    }
}