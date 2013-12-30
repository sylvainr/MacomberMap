using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Macomber_Map.Data_Elements;

namespace Macomber_Map.User_Interface_Components.OneLines
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// </summary>
    public class MM_OneLine_Transformer: MM_OneLine_Element
    {
        #region Variable declarations
        /// <summary>Our collection of transformer windings</summary>
        public MM_OneLine_TransformerWinding[] TransformerWindings;

        /// <summary>The nodes associated with this transformer</summary>
        public MM_OneLine_Node[] AssociatedNodes = new MM_OneLine_Node[2];
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new transformer
        /// </summary>
        /// <param name="xCh"></param>
        /// <param name="violViewer"></param>
        /// <param name="olView"></param>
        public MM_OneLine_Transformer(XmlElement xCh, Violation_Viewer violViewer, OneLine_Viewer olView)
            : base(xCh, violViewer, olView)
        { 
            List<MM_OneLine_TransformerWinding> Windings = new List<MM_OneLine_TransformerWinding>();
            foreach (XmlElement xChild in xCh.ChildNodes)
                if (xChild.Name == "Winding")
                    Windings.Add(new MM_OneLine_TransformerWinding(xChild,false, (this.BaseElement as MM_Transformer).Windings));
            TransformerWindings = Windings.ToArray();
        }

        /// <summary>
        /// Assign a node to our transformer
        /// </summary>
        /// <param name="Node"></param>
        public void AssignNode(MM_OneLine_Node Node)
        {
            if (Node.BaseElement.KVLevel.Equals(TransformerWindings[0].BaseWinding.KVLevel) && Node.BaseElement.KVLevel.Equals(TransformerWindings[1].BaseWinding.KVLevel))
                if (AssociatedNodes[0] == null)
                    AssociatedNodes[0] = Node;
                else
                    AssociatedNodes[1] = Node;
            else if (Node.BaseElement.KVLevel.Equals(TransformerWindings[0].BaseWinding.KVLevel))
                AssociatedNodes[0] = Node;
            else
                AssociatedNodes[1] = Node;
        }
        #endregion
    }
}
