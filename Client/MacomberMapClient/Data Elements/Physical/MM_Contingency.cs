using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class contains contingency definitions, and the collection of violations associated with it.
    /// </summary>
    public class MM_Contingency : MM_Element
    {
        #region Variable declarations
        /// <summary>
        /// The long name of the contingency definition
        /// </summary>
        public String Description;

        /// <summary>
        /// Contingency Type (FG or CTG)
        /// </summary>
        public String Type;

        /// <summary>
        /// The collection of KV levels found within the contingency definition
        /// </summary>
        public MM_KVLevel[] KVLevels = null;

        /// <summary>
        /// The counties which are traversed by elements within the contingency definition
        /// </summary>
        public MM_Boundary[] Counties = null;

        /// <summary>
        /// The substations that are included in the contingency definition
        /// </summary>
        public MM_Substation[] Substations = null;

        /// <summary>
        /// Contingent elements
        /// </summary>
        public List<int> ConElements = new List<int>(5);

        /// <summary>
        /// The position of the contingency definition
        /// </summary>
        public int Position;

        /// <summary>Whether the contingency is active</summary>
        public bool Active;
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize a new CIM Contingency Definition
        /// </summary>
        public MM_Contingency()
        {
            this.ElemType = MM_Repository.FindElementType("Contingency");
        }

        /// <summary>
        /// Initialize a new CIM Contingency Definition
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Contingency(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Contingency");
            RollUpCountiesAndVoltages();
        }

        /// <summary>
        /// Roll up our counties and voltages
        /// </summary>
        public void RollUpCountiesAndVoltages()
        {
            //Go through our substations to pull in county and kv information
            HashSet<MM_KVLevel> KVs = new HashSet<MM_KVLevel>();
            HashSet<MM_Boundary> Boundaries = new HashSet<MM_Boundary>();

            if (Substations != null)
            {

                foreach (MM_Substation Sub in Substations)
                {
                    foreach (MM_KVLevel KVLevel in Sub.KVLevels)
                        if (!KVs.Contains(KVLevel))
                            KVs.Add(KVLevel);
                    if (!Boundaries.Contains(Sub.County))
                        Boundaries.Add(Sub.County);
                }
            }

            if (ConElements != null && ConElements.Count > 0)
            {
                foreach (var teid in ConElements)
                {
                    MM_Element con = null;
                    MM_Repository.TEIDs.TryGetValue(teid, out con);
                    if (con != null && con.Contingencies.All(c => c.Name != con.Name))
                        con.Contingencies.Add(this);
                }
            } 

            KVLevels = KVs.ToArray();
            Counties = Boundaries.ToArray();
        }


        /// <summary>
        /// Initialize a new CIM Substation
        /// </summary>
        /// <param name="ElementSource">The data source for this substation</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Contingency(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            //The name, TEID, owner and operator are taken care of by the Element overload - define the rest
            this.Description = (String)ElementSource["LongName"];
            List<MM_Boundary> Boundaries = new List<MM_Boundary>();
            foreach (String County in ((String)ElementSource["Counties"]).Split(','))
                Boundaries.Add(MM_Repository.Counties[County]);
            Counties = Boundaries.ToArray();

            //Make sure we have this element type
            this.ElemType = MM_Repository.FindElementType("Contingency");
        }

        /// <summary>
        /// Initialize a new contiengency
        /// </summary>
        /// <param name="ContingencyName">The name of the contingency</param>
        public MM_Contingency(String ContingencyName)
        {
            this.Name = ContingencyName;
            this.Description = ContingencyName;
            this.ElemType = MM_Repository.FindElementType("Contingency");
        }

        /// <summary>
        /// Initialize a new contiengency
        /// </summary>
        /// <param name="ContingencyName">The name of the contingency</param>
        /// <param name="ContingencyDescription">The description of the contingency</param>
        /// <param name="ContingencyPosition">The contingency position</param>
        /// <param name="Active">Whether the contingency is active</param>
        public MM_Contingency(String ContingencyName, String ContingencyDescription, int ContingencyPosition, bool Active)
        {
            this.Name = ContingencyName;
            this.Description = ContingencyDescription;
            this.ElemType = MM_Repository.FindElementType("Contingency");
            this.Position = ContingencyPosition;
        }

        /// <summary>
        /// Report the name of the contingency
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + " / " + Description;
        }

        #endregion
    }
}