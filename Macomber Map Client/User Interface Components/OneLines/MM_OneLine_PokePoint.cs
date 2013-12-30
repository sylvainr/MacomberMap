using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Macomber_Map.Data_Elements;
using System.Xml;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Macomber_Map.User_Interface_Components.OneLines
{
    /// <summary>
    /// This class holds the poke point information
    /// </summary>
    public class MM_OneLine_PokePoint : MM_OneLine_Node
    {
        #region Variable declarations
        /// <summary>Whether the poke point is a jumper</summary>
        private bool _IsJumper = false;

        /// <summary>Whether the poke point is a jumper</summary>
        [Category("Display"), Description("Whether the poke point is a jumper")]
        public bool IsJumper
        {
            get { return _IsJumper; }
            set { _IsJumper = value; IsVisible = _IsVisible; }
        }



        /// <summary>Whether the box should be drawn for this poke point</summary>
        private bool _IsVisible = true;

        /// <summary>Whether the box should be drawn for this poke point</summary>        
        [Category("Display"), Description("Whether the box should be drawn for this poke point")]
        public bool IsVisible
        {
            get { return _IsVisible; }
            set
            {
                _IsVisible = value;
                Point OldCenter = CenterRect(Bounds);

                if (this is MM_OneLine_PricingVector)
                { }
                else if (!value)
                    this.Size = new Size(4, 4);
                else if (!_IsJumper)
                    this.Size = new Size(8, 8);         

            }
        }


        #endregion

        /// <summary>
        /// Initialize a new poke point
        /// </summary>
        /// <param name="xE"></param>
        /// <param name="violViewer"></param>
        /// <param name="olViewer"></param>
        public MM_OneLine_PokePoint(XmlElement xE, Violation_Viewer violViewer, OneLine_Viewer olViewer)
            : base(xE, violViewer, olViewer)
        { }
        
       

        /// <summary>
        /// Initialize a poke point with a rectangle, for a jumper
        /// </summary>
        /// <param name="OwnerNode"></param>
        /// <param name="BaseNode">The node that owns this poke point</param>
        /// <param name="Orientation">The orientation of the poke point</param>
        /// <param name="IsJumper">Whether the poke point is a jumper</param>
        /// <param name="IsVisible">whether the poke point is visible</param>
        /// <param name="olView"></param>
        /// <param name="violView"></param>
        public MM_OneLine_PokePoint(MM_Element OwnerNode, MM_OneLine_Node BaseNode, MM_OneLine_Element.enumOrientations Orientation, bool IsJumper, bool IsVisible, Violation_Viewer  violView, OneLine_Viewer olView)
            : base(OwnerNode, Orientation, violView,olView)
        {
            this.ElemType = enumElemTypes.PokePoint;
            this.BaseNode = BaseNode;
            this.IsJumper = IsJumper;
            this.IsVisible = IsVisible;          
        }


        /// <summary>
        /// Report the information on the poke point
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String ThisBase = BaseElement == null ? "Unknown element" : BaseElement.ToString();
            if (this is MM_OneLine_PricingVector)
                return String.Format("{0} ({1},{2})-({3},{4}) to {5}", "PV ", Left, Top, Right, Bottom, ThisBase);
            else if (this is MM_OneLine_Node)
                if (this.BaseElement != null && (this.BaseElement as MM_Node).BusbarSection != null)
                    return String.Format("{0} ({1},{2})-({3},{4}) to {5}", "BusbarSection ", Left, Top, Right, Bottom, (BaseElement as MM_Node).BusbarSection.ToString() + " / " + ThisBase);
                else
                    return String.Format("{0} ({1},{2})-({3},{4}) to {5}", "Node ", Left, Top, Right, Bottom, ThisBase);
            else if (IsJumper)
                return String.Format("{0} ({1},{2})-({3},{4}) to {5}", "Jumper ", Left, Top, Right, Bottom, BaseElement.ToString());
            else if (!IsVisible)
                return String.Format("{0} ({1},{2})-({3},{4}) to {5}", "Invisible PokePoint ", Left, Top, Right, Bottom, BaseElement.ToString());
            else
                return String.Format("{0} ({1},{2})-({3},{4}) to {5}", "PokePoint ", Left, Top, Right, Bottom, BaseElement.ToString());
        }
       
    }

}
