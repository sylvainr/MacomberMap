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
    /// Store information on generation types
    /// </summary>
    public class MM_Unit_Type : MM_Element
    {
        #region Variable declarations

        /// <summary>The EMS name for the type</summary>
        public String EMSName;

        /// <summary>The collection of units of this generation type</summary>
        public List<MM_Unit> Units = new List<MM_Unit>(100);

        /// <summary>The MW rating for this generation type</summary>
        public Single MW
        {
            get { return _MW; }
            set { _MW = value; }
        }
        private Single _MW;

        /// <summary>The MW remaining for this generation type</summary>
        public Single Remaining
        {
            get { return _Remaining; }
            set { _Remaining = value; }
        }
        private Single _Remaining;

        /// <summary>The capacity for the generation type</summary>
        public Single Capacity
        {
            get { return _MW + _Remaining; }
        }


        /// <summary>
        /// Return the total MW output of all units of this class
        /// </summary>
        public float ReadMW
        {
            get
            {
                float OutMW = 0f;
                foreach (MM_Unit Unit in Units)
                    if (Integration.Data_Integration.UseEstimates)
                        OutMW += Unit.Estimated_MW;
                    else
                        OutMW += float.IsNaN(Unit.Telemetered_MW) || Unit.Telemetered_MW == 0f ? Unit.Estimated_MW : Unit.Telemetered_MW;
                return OutMW;
            }
        }

        /// <summary>Whether the generation type is physical</summary>
        public bool Physical;

        /// <summary>Whether the generation type is referenced in EMS</summary>
        public bool EMS;
        #endregion

        #region Initialization
        /// <summary>
        /// Create a new generation type
        /// </summary>
        /// <param name="Name">The name of the generation type</param>    
        public MM_Unit_Type(String Name)
        {
            this.Name = Name;
        }

        /// <summary>
        /// Initialize a new generation type
        /// </summary>
        public MM_Unit_Type()
        {

        }

        /// <summary>
        /// Initialize a new generation type from an XML element
        /// </summary>
        /// <param name="xConfig"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Unit_Type(XmlElement xConfig, bool AddIfNew)
            : base(xConfig, AddIfNew)
        {
        }

        /// <summary>
        /// Initialize a new generation type from an XML element
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Unit_Type(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
        }
        #endregion

        /// <summary>
        /// Report the name of the generation type
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
