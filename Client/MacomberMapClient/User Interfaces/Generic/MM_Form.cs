using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Communications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Generic
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D., Electrical Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides a generic form 
    /// </summary>
    public partial class MM_Form : Form, IComparable<MM_Form>
    {
        #region Application title
        /// <summary>The title of the application</summary>
        public String Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                if (Data_Integration.UserName != null)
                    this.Text = value + " - " + Data_Integration.UserName.ToUpper() + " - Macomber Map®";
                else
                {
                    this.Text = value + " - Macomber Map®";
                }
            }
        }
        private string _Title;
        #endregion

        #region Menu altering


        #endregion

        /// <summary>
        /// Initialize a new form.
        /// </summary>
        public MM_Form()
        {
            InitializeComponent();

            if (DesignMode) return;
            if (this is MM_About_Form == false && this is MM_Communication_Viewer == false)
            {
                // Create our new System Menu items just before the Close menu item
                IntPtr systemMenuHandle = MM_WindProcHelper.GetSystemMenu(this.Handle, false);
                MM_WindProcHelper.InsertMenu(systemMenuHandle, 5, MM_WindProcHelper.MF_BYPOSITION | MM_WindProcHelper.MF_SEPARATOR, 0, string.Empty); // <-- Add a menu seperator
                MM_WindProcHelper.InsertMenu(systemMenuHandle, 6, MM_WindProcHelper.MF_BYPOSITION, MM_WindProcHelper._RenameSysMenuID, "Rename window");
                MM_WindProcHelper.InsertMenu(systemMenuHandle, 7, MM_WindProcHelper.MF_BYPOSITION, MM_WindProcHelper._AboutSysMenuID, "About...");
            }
        }

        private delegate void ThreadSafeActivate();
        /// <summary>
        /// Safely activate a new window
        /// </summary>
        public void SafeActivate()
        {
            if (DesignMode) return;
            if (this.InvokeRequired)
                this.BeginInvoke(new ThreadSafeActivate(SafeActivate));
            else
                this.Activate();
        }

        #region Message handling
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (DesignMode)
            {
                base.WndProc(ref m);
                return;
            }
            if (m.Msg == MM_WindProcHelper.WM_SYSCOMMAND)
                if (m.WParam.ToInt32() == MM_WindProcHelper._RenameSysMenuID)
                {
                    String WinName = this.Title;
                    if (MM_System_Interfaces.InputBox(System.Windows.Forms.Application.ProductName, "Please enter the title for this window", ref WinName) == DialogResult.OK)
                        this.Title = WinName;
                }
                else if (m.WParam.ToInt32() == MM_WindProcHelper._AboutSysMenuID)
                    new MM_About_Form().Show();

            base.WndProc(ref m);
        }
        #endregion


        #region IComparable<MM_Form> Members
        /// <summary>
        /// Compare two MM Forms
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(MM_Form other)
        {
            return Title.CompareTo(other.Title);
        }

        #endregion
    }
}