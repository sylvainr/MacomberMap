using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.User_Interfaces.OneLines
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a one-line node, which is deserialized from the MM_OneLine XML.
    /// </summary>
    public class MM_OneLine_Node: MM_OneLine_Element
    {
        #region Variable declarations
        /// <summary>If a poke point is visible</summary>
        public bool IsVisible;

        /// <summary>If a poke point is a jumper</summary>
        public bool IsJumper;

        /// <summary>Whether a pricing vector is pointing in the posive direction</summary>
        public bool IsPositive;

        /// <summary>The paths from a node through poke points to the target element</summary>
        public Dictionary<MM_OneLine_Element, GraphicsPath> NodePaths;

        /// <summary>The paths from a node through poke points to the target element</summary>
        public Dictionary<MM_OneLine_Element, MM_OneLine_Node[]> NodeTargets;

        /// <summary>The resource node information, if availalbe</summary>
        public string ResourceNode;

        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new one-line node
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="DisplayElementsByTEID"></param>
        /// <param name="ParentElement"></param>
        public MM_OneLine_Node(XmlElement xElem, Dictionary<int, MM_OneLine_Element> DisplayElementsByTEID, MM_OneLine_Element ParentElement = null)
            : base(xElem, ParentElement)
        {
            MM_OneLine_Element TargetElem;

            NodeTargets = new Dictionary<MM_OneLine_Element, MM_OneLine_Node[]>();
            NodePaths = new Dictionary<MM_OneLine_Element, GraphicsPath>();
            //Pull in our node targets and paths
            foreach (XmlElement xChild in xElem.ChildNodes)
                if (xChild.Name != "Descriptor" && xChild.Name != "SecondaryDescriptor" && xChild.HasAttribute("TEID"))
                    if (DisplayElementsByTEID.TryGetValue(XmlConvert.ToInt32(xChild.Attributes["TEID"].Value), out TargetElem))
                    {
                        List<MM_OneLine_Node> SubElements = new List<MM_OneLine_Node>();
                        foreach (XmlElement xChild2 in xChild.ChildNodes)
                            SubElements.Add(new MM_OneLine_Node(xChild2, DisplayElementsByTEID, xChild2.Name == "PricingVector" ? null : this));
                        NodeTargets.Add(TargetElem, SubElements.ToArray());
                        NodePaths.Add(TargetElem, MM_OneLine_Element.BuildNodePath(this, TargetElem, SubElements.ToArray()));

                    }
        }
        #endregion

        /// <summary>
        /// Determine our bus associated with this node
        /// </summary>
        public void DetermineAssociatedBus()
        {
            MM_Bus FoundBus = null;
            if (BaseElement.NearBus != null)
                FoundBus = BaseElement.NearBus;
            else
            {
                int MaxBuses = 0;
                Dictionary<MM_Bus, int> FoundBuses = new Dictionary<MM_Bus, int>();
                foreach (MM_OneLine_Element Elem in NodeTargets.Keys)
                {
                    bool Include = true;
                    if (Elem.BaseElement is MM_Line && ((MM_Line)Elem.BaseElement).MVAFlow <= MM_Repository.OverallDisplay.EnergizationThreshold)
                        Include = false;
                    else if (Elem.BaseElement is MM_Breaker_Switch && ((MM_Breaker_Switch)Elem.BaseElement).BreakerState == MM_Breaker_Switch.BreakerStateEnum.Open)
                        Include = false;
                    if (Include)
                    {
                        if (Elem.BaseElement.NearBus != null && !Elem.BaseElement.NearBus.Dead)
                            if (FoundBuses.ContainsKey(Elem.BaseElement.NearBus))
                                FoundBuses[Elem.BaseElement.NearBus]++;
                            else
                                FoundBuses.Add(Elem.BaseElement.NearBus, 1);
                        if (Elem.BaseElement.FarBus != null && !Elem.BaseElement.FarBus.Dead)
                            if (FoundBuses.ContainsKey(Elem.BaseElement.FarBus))
                                FoundBuses[Elem.BaseElement.FarBus]++;
                            else
                                FoundBuses.Add(Elem.BaseElement.FarBus, 1);
                    }

                    foreach (KeyValuePair<MM_Bus, int> kvp in FoundBuses)
                        if (kvp.Value > MaxBuses)
                        {
                            FoundBus = kvp.Key;
                            MaxBuses = kvp.Value;
                        }
                }
            }
            
            ((MM_Node)BaseElement).AssociatedBus = FoundBus;
        }
    }
}
