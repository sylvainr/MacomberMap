using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Geographic
{
    /// <summary>
    /// This class holds zonal information (weather and load)
    /// </summary>
    public class MM_Zone : MM_Serializable
    {
        /// <summary>The integer locator for SQL CIM Servers</summary>
        public int Index;

        /// <summary>The name of the zone</summary>
        public string Name;

        /// <summary>The type of wone</summary>
        public MM_Element_Type ElemType;

        /// <summary>The collection of substations within this zone</summary>
        public List<MM_Substation> Substations = new List<MM_Substation>();

        /// <summary>The drawing color for the zone</summary>
        public Color DrawColor;

        /// <summary>Our random color generator</summary>
        private static Random RandomColor = new Random();

        /// <summary>
        /// Initialize a new weather or load zone.
        /// </summary>
        /// <param name="ElementSource">The data row containing the source</param>
        /// <param name="AddIfNew"></param>
        public MM_Zone(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
        }

        /// <summary>
        /// Create a new zone from XML
        /// </summary>
        /// <param name="ZoneSource">The XML configuration element for the zone</param>
        /// <param name="AddIfNew"></param>
        public MM_Zone(XmlElement ZoneSource, bool AddIfNew)
            : base(ZoneSource, AddIfNew)
        {
            DrawColor = Color.FromArgb(RandomColor.Next(0, 255), RandomColor.Next(0, 255), RandomColor.Next(0, 255));
        }

        /// <summary>
        /// Create a new zone with specified integer value and name
        /// </summary>
        /// <param name="Name">The name</param>
        /// <param name="Index">The integer value</param>
        public MM_Zone(string Name, int Index)
        {
            this.Index = Index;
            this.Name = Name;
        }


        /// <summary>
        /// Return an easy-to-read name for our zone
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Return the worst violation overall, regardless of its visibility
        /// </summary>
        public MM_AlarmViolation_Type WorstViolationOverall
        {
            get
            {
                MM_AlarmViolation_Type WorstViol = null;
                foreach (MM_Substation Sub in this.Substations)
                    WorstViol = Sub.WorstViolation(WorstViol);
                return WorstViol;
            }
        }

    }
}
