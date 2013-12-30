using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Macomber_Map.User_Interface_Components.Overrides
{
    /// <summary>
    /// This class offers transparent backgrounds for a tab page
    /// </summary>
    public class MM_TabPage: TabPage
    {

        /// <summary>
        /// Initialize our tab page
        /// </summary>
        public MM_TabPage()
        { }

        /// <summary>
        /// Handle the background painting of our page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
        }

        /// <summary>
        /// Handle the painting of our page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
    }
}
