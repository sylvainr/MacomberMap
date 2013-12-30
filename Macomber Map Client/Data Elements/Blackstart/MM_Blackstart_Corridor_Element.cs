using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Macomber_Map.Data_Connections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class holds information on a blackstart element
    /// </summary>
    public class MM_Blackstart_Corridor_Element : MM_Element
    {
       

        #region Variable declarations
        /// <summary>The blackstart corridor target associated with our element</summary>
        public MM_Blackstart_Corridor_Target Blackstart_Target;

        /// <summary>The element associated with the action</summary>
        public MM_Element AssociatedElement = null;

        /// <summary>The action to be carried out</summary>
        public MM_Communication.Elements.MM_Element.enumBlackstartElementActivity Action;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new blackstart corridor target
        /// </summary>
        public MM_Blackstart_Corridor_Element()
        {
            ElemType = MM_Repository.FindElementType("Blackstart_Corridor");
        }


        /// <summary>
        /// Initialize a new blacstart corridor element
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="Target"></param>
        public MM_Blackstart_Corridor_Element(XmlElement xElem, MM_Blackstart_Corridor_Target Target)
            : base(xElem, false)
        {
            this.Blackstart_Target = Target;
            if (xElem.HasAttribute("Substation"))
                this.Substation = MM_Repository.TEIDs[Convert.ToInt32(xElem.Attributes["Substation"].Value)] as MM_Substation;
            if (xElem.Name == "Breaker")
                this.AssociatedElement = Data_Integration.LocateElement(xElem, typeof(MM_Breaker_Switch), true);
            else
                this.AssociatedElement = Data_Integration.LocateElement(xElem, Type.GetType("Macomber_Map.Data_Elements.MM_" + xElem.Name), true);
            
            if (this.AssociatedElement is MM_Line)
                (this.AssociatedElement as MM_Line).IsBlackstart = true;
            else
                this.AssociatedElement.Substation.IsBlackstart = true;
            ElemType = MM_Repository.FindElementType("Blackstart_Corridor");

        }
        #endregion

        /// <summary>
        /// Report the name of our string 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Blackstart_Target.Blackstart_Corridor.Name + "/" + Blackstart_Target.Target + "/" + Action.ToString() + " " + Substation.Name + " " + AssociatedElement.ToString();
        }
    }
}
