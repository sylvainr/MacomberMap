using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Macomber_Map.Data_Elements;
using System.Xml;
using System.ComponentModel;
using System.Windows.Forms;

namespace Macomber_Map.User_Interface_Components.OneLines
{
    /// <summary>
    /// This class holds information on a pricing vector
    /// </summary>
    public class MM_OneLine_PricingVector : MM_OneLine_PokePoint
    {
        #region Variable declarations

        /// <summary>The EPS meter associated with the vector</summary>
        [Category("Display"), Description("The EPS meter associated with the pricing vector")]
        public MM_EPSMeter EPSMeter
        {
            get { return (BaseElement as MM_PricingVector).EPSMeter; }
            set { (BaseElement as MM_PricingVector).EPSMeter = value; }
        }


        /// <summary>
        /// The resource ID of the pricing vector
        /// </summary>
        [Category("Display"), Description("The resource ID of the pricing vector")]
        public string RID
        {
            get { return (BaseElement as MM_PricingVector).EPSMeter.RID; }
        }


        /// <summary>Whether the flow towards the associated node is positive</summary>
        [Category("Display"), Description("Whether the flow towards the associated node is positive")]
        public bool IsPositive
        {
            get { return (BaseElement as MM_PricingVector).IsPositive; }
            set { (BaseElement as MM_PricingVector).IsPositive = value; }
        }
        #endregion
        /// <summary>
        /// Initialize a new pricing vector
        /// </summary>
        /// <param name="xE"></param>
        /// <param name="violViewer"></param>
        /// <param name="olView"></param>
        public MM_OneLine_PricingVector(XmlElement xE, Violation_Viewer violViewer, OneLine_Viewer olView)
            : base(xE, violViewer, olView)
        {
            if (xE.HasAttribute("EPSMeter.TEID"))
            {
                MM_Element FoundEPS;
                if (MM_Repository.TEIDs.TryGetValue(Int32.Parse(xE.Attributes["EPSMeter.TEID"].Value), out FoundEPS))
                {
                    this.EPSMeter = FoundEPS as MM_EPSMeter;                    
                    foreach (XmlAttribute xAttr in xE.Attributes)
                        if (xAttr.Name.StartsWith("EPSMeter."))
                            MM_Serializable.AssignValue(xAttr.Name.Substring(xAttr.Name.IndexOf('.') + 1), xAttr.Value, this.EPSMeter,false);
                }
                else
                    this.EPSMeter = MM_Element.CreateElement(xE, "EPSMeter",false) as MM_EPSMeter;
            }
        }
    }
}
