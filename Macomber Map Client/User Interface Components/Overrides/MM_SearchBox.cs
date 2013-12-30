using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Macomber_Map.Properties;

namespace Macomber_Map.User_Interface_Components.Overrides
{
    /// <summary>
    /// This class provides a search box
    /// </summary>
    public class MM_SearchBox: TextBox
    {
        /// <summary>Our event handler for clicking search</summary>
        public event EventHandler SearchClicked;

         #region Initialization
        /// <summary>
        /// Initialize a new text box and assign our hooks
        /// </summary>
        public MM_SearchBox()
        {
            this.TextChanged += new EventHandler(SearchTextBox_TextChanged);
            this.MouseMove += new MouseEventHandler(SearchTextBox_MouseMove);
            this.MouseClick += new MouseEventHandler(SearchTextBox_MouseClick);
        }
        #endregion

        #region Paint handling
        /// <summary>
        /// Intercept the paint call
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 15)
            {
                this.Invalidate();
                base.WndProc(ref m);
                PaintSearch(m.HWnd);
            }
            else            
                base.WndProc(ref m);            
        }

        /// <summary>
        /// When our text has changed, repaint our icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            PaintSearch(this.Handle);
        }

        private void PaintSearch(IntPtr HWnd)
        {
            using (Graphics g = Graphics.FromHwnd(HWnd))
            {
                if (String.IsNullOrEmpty(Text))
                    g.DrawString("Search for elements", this.Font, Brushes.Gray, 2, (DisplayRectangle.Height-this.Font.Height)/2);
                g.DrawImageUnscaled(Resources.Search, this.DisplayRectangle.Right - Resources.Search.Width - 2, (DisplayRectangle.Height - Resources.Search.Height) / 2);
            }
        }
        #endregion


        /// <summary>
        /// Update the mouse cursor depending on location
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_MouseMove(object Sender, MouseEventArgs e)
        {
            if (e.X > this.DisplayRectangle.Width - 16)
                this.Cursor = Cursors.Default;
            else
                this.Cursor = Cursors.IBeam;
        }
        
        /// <summary>
        /// Handle the search button click
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_MouseClick(object Sender, MouseEventArgs e)
        {
            if (e.X > this.DisplayRectangle.Width - 16 && SearchClicked != null)
                SearchClicked(Sender, EventArgs.Empty);
        }

    }
}
