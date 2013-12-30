using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace Macomber_Map.User_Interface_Components.Overrides
{
    /// <summary>
    /// This class overrides the tabcontrol, allowing for custom backgrounds
    /// </summary>
    public class MM_TabControl: TabControl
    {

        /// <summary>
        /// Initialize our custom tab control
        /// </summary>
        public MM_TabControl()
        {
        }

        /// <summary>
        /// Override our paint background
        /// </summary>
        /// <param name="pevent"></param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
        }

        /// <summary>
        /// Override our painting
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
    }
}
