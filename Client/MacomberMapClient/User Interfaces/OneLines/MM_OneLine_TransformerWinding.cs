using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.User_Interfaces.OneLines
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a transformer winding
    /// </summary>
    public class MM_OneLine_TransformerWinding : MM_OneLine_Element
    {
        /// <summary>Whether the winding is the primary one</summary>
        public bool IsPrimary { get; set; }

        /// <summary>Whether the winding has a phase shifter</summary>
        public bool IsPhaseShifter { get; set; }

        /// <summary>
        /// Initialize a new transformer winding
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="ParentElement"></param>
        public MM_OneLine_TransformerWinding(XmlElement xElem, MM_OneLine_Element ParentElement = null)
            : base(xElem, ParentElement)
        {
            if (xElem.HasAttribute("BaseElement.IsPrimary"))
                IsPrimary = XmlConvert.ToBoolean(xElem.Attributes["BaseElement.IsPrimary"].Value);

            if (xElem.ParentNode.Attributes["BaseElement.PhaseShifter"] != null && XmlConvert.ToBoolean(xElem.ParentNode.Attributes["BaseElement.PhaseShifter"].Value))
                IsPhaseShifter = true;
        }
        /// <summary>
        /// Initialize a new transformer winding by cloning its parent
        /// </summary>
        /// <param name="BaseWinding"></param>
        public MM_OneLine_TransformerWinding(MM_OneLine_TransformerWinding BaseWinding, MM_OneLine_Element ParentElement)
            : base(null)
        {
            if (BaseWinding != null)
                foreach (FieldInfo fI in typeof(MM_OneLine_TransformerWinding).GetFields())
                    if (fI.Name == "ParentElement")
                        fI.SetValue(this, ParentElement);
                    else
                        try
                        {
                            fI.SetValue(this, fI.GetValue(BaseWinding));
                        }
                        catch (Exception ex)
                        { }
        }
    }
}
