using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Macomber_Map.Data_Elements;
using System.Xml;
using System.Data;

namespace Macomber_Map.User_Interface_Components.OneLines
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a transformer winding
    /// </summary>    
    public class MM_OneLine_TransformerWinding: MM_Serializable
    {
        #region Variable declarations
        /// <summary>The orientation of the winding</summary>
        public MM_OneLine_Element.enumOrientations Orientation = MM_OneLine_Element.enumOrientations.Unknown;

        /// <summary>The bounds of the transformer winding</summary>
        public Rectangle Bounds;

        /// <summary>The winding associated with our view</summary>
        public MM_TransformerWinding BaseWinding;

        /// <summary>The data row for this line.</summary>
        public DataRow BaseRow;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our new element
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        /// <param name="Windings"></param>
        public MM_OneLine_TransformerWinding(XmlElement xElem, bool AddIfNew, MM_TransformerWinding[] Windings)
            : base(xElem, AddIfNew)
        {
            //Now, if we have one, set our base element
            if (xElem.HasAttribute("BaseElement.TEID"))
                foreach (MM_TransformerWinding Winding in Windings)
                    if (Winding.TEID == Int32.Parse(xElem.Attributes["BaseElement.TEID"].Value))
                        BaseWinding = Winding;

            if (BaseWinding == null)
                BaseWinding = MM_Element.CreateElement(xElem,"BaseElement",AddIfNew) as MM_TransformerWinding;
        }
        #endregion
    }
}
