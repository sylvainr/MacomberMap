using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using Macomber_Map.Data_Elements;

namespace Macomber_Map.User_Interface_Components.NetworkMap.GDIPlus
{
    /// <summary>
    /// This class holds the information for a county, including its path and whether it is visible
    /// </summary>
    public class MM_GDI_County
    {
        /// <summary>
        /// The path for the county
        /// </summary>
        public GraphicsPath CountyPath;

        /// <summary>
        /// The pen for drawing the county
        /// </summary>
        public Pen CountyPen;

        /// <summary>
        /// Whether the county is visible
        /// </summary>
        public bool Visible;

        /// <summary>
        /// Initialize a new county
        /// </summary>
        /// <param name="CountyPath">The graphics path for hte county</param>
        /// <param name="PenColor"></param>
        /// <param name="PenWidth"></param>
        public MM_GDI_County(GraphicsPath CountyPath, Color PenColor, float PenWidth)
        {
            this.CountyPath = CountyPath;
            this.CountyPen = new Pen(PenColor, PenWidth);
        }


    }

}
