using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// This class holds the information for our CSC/IROL limits
    /// </summary>
    public class MM_Limit : MM_Element
    {
        #region Variable delarations
        /// <summary>The current MW of the limit</summary>
        public float Current;

        /// <summary>The maximum MW of the limit</summary>
        public float Max;
        #endregion


        #region Initialization/updating
        /// <summary>
        /// Initialize a new limit
        /// </summary>
        public MM_Limit()
        {
        }

        /// <summary>
        /// Create a new IROL/CSC limit
        /// </summary>
        /// <param name="dr"></param>
        public MM_Limit(DataRow dr)
        {
            this.Name = MM_Repository.TitleCase((string)dr["ID"]);
            this.Current = MM_Converter.ToSingle(dr["RTOTMW"]);
            this.Max = MM_Converter.ToSingle(dr["LTOTMW"]);
        }

        /// <summary>
        /// Create a new IROL/CSC limit
        /// </summary>
        /// <param name="ElementSource">The element source</param>
        /// <param name="AddIfNew"></param>
        public MM_Limit(XmlElement ElementSource, bool AddIfNew) : base(ElementSource, AddIfNew) { }

      
        #endregion
    }
}
