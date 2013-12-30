using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Macomber_Map.User_Interface_Components
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
                this.Text = value + " - " + Macomber_Map.Data_Connections.Data_Integration.UserName.ToUpper() + " - Macomber Map®";
            }
        }
        private string _Title;
        #endregion

        #region Menu altering
        //This code thanks to http://pietschsoft.com/post/2008/03/Add-System-Menu-Items-to-a-Form-using-Windows-API.aspx

        // Define the Win32 API methods we are going to use
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        /// Define our Constants we will use
        private const Int32 WM_SYSCOMMAND = 0x112;
        private const Int32 MF_SEPARATOR = 0x800;
        private const Int32 MF_BYPOSITION = 0x400;
        private const Int32 MF_STRING = 0x0;

        // The constants we'll use to identify our custom system menu items
        private const Int32 _RenameSysMenuID = 1000;
        private const Int32 _AboutSysMenuID = 1001;
        #endregion
         
        /// <summary>
        /// Initialize a new form.
        /// </summary>
        public MM_Form()
        {
            InitializeComponent();

            if (this is About_Form == false && this is Communication_Viewer == false)
            {
                // Create our new System Menu items just before the Close menu item
                IntPtr systemMenuHandle = GetSystemMenu(this.Handle, false);
                InsertMenu(systemMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty); // <-- Add a menu seperator
                InsertMenu(systemMenuHandle, 6, MF_BYPOSITION, _RenameSysMenuID, "Rename window");
                InsertMenu(systemMenuHandle, 7, MF_BYPOSITION, _AboutSysMenuID, "About...");
            }
        }
        
        private delegate void ThreadSafeActivate();
        /// <summary>
        /// Safely activate a new window
        /// </summary>
        public void SafeActivate()
        {
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
            if (m.Msg == WM_SYSCOMMAND)
                if (m.WParam.ToInt32() == _RenameSysMenuID)
                {
                    String WinName = this.Title;
                    if (Program.InputBox(Application.ProductName, "Please enter the title for this window", ref WinName) == DialogResult.OK)
                        this.Title = WinName;
                }
                else if (m.WParam.ToInt32() == _AboutSysMenuID)
                    new About_Form().Show();

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