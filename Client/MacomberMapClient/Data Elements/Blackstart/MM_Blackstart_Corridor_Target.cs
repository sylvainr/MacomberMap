using MacomberMapClient.Data_Elements.Physical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Blackstart
{
    /// <summary>
    /// (C) 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a blackstart corridor path
    /// </summary>
    public class MM_Blackstart_Corridor_Target : MM_Element
    {

        #region Variable declarations
        /// <summary>The corridor associated with the target</summary>
        public MM_Blackstart_Corridor Blackstart_Corridor;

        /// <summary>Our collection of blackstart corridor elements for the primary path</summary>        
        public MM_Blackstart_Corridor_Element[] Primary = new MM_Blackstart_Corridor_Element[0];

        /// <summary>Our collection of blackstart corridor elements for the primary path</summary>        
        public MM_Blackstart_Corridor_Element[] Secondary = new MM_Blackstart_Corridor_Element[0];

        /// <summary>
        /// Our full list of elements
        /// </summary>
        public MM_Blackstart_Corridor_Element[] Elements
        {
            get
            {
                List<MM_Blackstart_Corridor_Element> OutList = new List<MM_Blackstart_Corridor_Element>();
                OutList.AddRange(Primary);
                OutList.AddRange(Secondary);
                return OutList.ToArray();
            }
        }

        /// <summary>The target of the blackstart path</summary>
        public String Target;

        /// <summary>The XML configuration for our element</summary>       
        public XmlElement xConfig;

        /// <summary>The description of our target</summary>
        public String Description;
        #endregion


        #region Initialization
        /// <summary>
        /// Initilaize a new blackstart corridor target
        /// </summary>
        public MM_Blackstart_Corridor_Target()
        {
        }

        /// <summary>
        /// Initialize a new blackstart corridor
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="Corridor"></param>
        public MM_Blackstart_Corridor_Target(XmlElement xElem, MM_Blackstart_Corridor Corridor)
            : base(xElem, false)
        {
            this.xConfig = xElem;
            this.Blackstart_Corridor = Corridor;
            this.Target = xElem.Attributes["Target"].Value;
            this.Description = xElem.Attributes["Description"].Value;
            foreach (string str in "Primary,Secondary".Split(','))
                if (xElem[str] != null)
                {
                    List<MM_Blackstart_Corridor_Element> Corridors = new List<MM_Blackstart_Corridor_Element>();
                    foreach (XmlNode xNode in xElem[str].ChildNodes)
                        if (xNode is XmlElement)
                            Corridors.Add(new MM_Blackstart_Corridor_Element(xNode as XmlElement, this));
                    if (str == "Primary")
                        Primary = Corridors.ToArray();
                    else
                        Secondary = Corridors.ToArray();
                }
        }


        #endregion
        /// <summary>
        /// Report the target name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Blackstart_Corridor.Name + "/" + Target;
        }
    }
}
