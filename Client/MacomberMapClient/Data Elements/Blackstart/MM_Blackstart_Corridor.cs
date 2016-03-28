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
    /// This class holds the information needed for a blackstart corridor.
    /// </summary>
    public class MM_Blackstart_Corridor : MM_Element
    {
        #region Variable declarations
        /// <summary>The description of the blackstart corridor</summary>
        public String Description;

        /// <summary>Our collection of blackstart targets</summary>
        public MM_Blackstart_Corridor_Target[] Blackstart_Targets = new MM_Blackstart_Corridor_Target[0];
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize a new blackstart corridor
        /// </summary>
        public MM_Blackstart_Corridor()
        {
        }

        /// <summary>
        /// Initialize a new blackstart corridor
        /// </summary>
        /// <param name="xElem"></param>
        public MM_Blackstart_Corridor(XmlElement xElem)
        {
            //Initialize from our XML data
            this.Name = xElem.Attributes["Name"].Value;
            this.Name = xElem.Attributes["Name"].Value;
            this.Description = xElem.Attributes["Description"].Value;
            List<MM_Blackstart_Corridor_Target> Targets = new List<MM_Blackstart_Corridor_Target>();
            foreach (XmlElement xTarget in xElem.ChildNodes)
                if (xTarget.Name == "Target")
                    Targets.Add(new MM_Blackstart_Corridor_Target(xTarget, this));
            this.Blackstart_Targets = Targets.ToArray();
        }

        /// <summary>
        /// Report the name of the blackstart corridor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
