using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Blackstart
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
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
        public enumBlackstartElementActivity Action;

        /// <summary>The collection of possible element activities</summary>
        public enum enumBlackstartElementActivity : byte
        {
            /// <summary>Energize a unit</summary>
            EnergizeUnit,
            /// <summary>Close a breaker</summary>
            Close_Breaker,
            /// <summary>Energize a line</summary>
            EnergizeLine,
            /// <summary>Reach a synchronization point</summary>
            ReachSynchPoint,
            /// <summary>Close a switch</summary>
            Close_Switch
        }
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
            if (xElem.Name == "Breaker" || xElem.Name == "Switch")
                this.AssociatedElement = Integration.Data_Integration.LocateElement(xElem, typeof(MM_Breaker_Switch), true);
            else
                this.AssociatedElement = Integration.Data_Integration.LocateElement(xElem, Type.GetType("MacomberMapClient.Data_Elements.Physical.MM_" + xElem.Name), true);

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
