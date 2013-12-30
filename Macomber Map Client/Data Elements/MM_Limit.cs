using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;
using Macomber_Map.Data_Connections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class holds the information for our CSC/IROL limits
    /// </summary>
    public class MM_Limit : MM_Serializable
    {
        #region Variable delarations
        /// <summary>The name of the limit</summary>
        public string Name;

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
        public MM_Limit(XmlElement ElementSource, bool AddIfNew) : base(ElementSource,  AddIfNew) { }

        /// <summary>
        /// Update an existing CSC/IROL limit
        /// </summary>
        /// <param name="dr"></param>
        public void Update(DataRow dr)
        {
            this.Current = MM_Converter.ToSingle(dr["RTOTMW"]);
            this.Max = MM_Converter.ToSingle(dr["LTOTMW"]);
        }

        /// <summary>
        /// Update our value from teh server
        /// </summary>
        /// <param name="Attribute"></param>
        /// <param name="InValue"></param>
        public void Update(MM_Communication.Elements.MM_Element.enumAttributes Attribute, Object InValue)
        {
            if (Attribute == MM_Communication.Elements.MM_Element.enumAttributes.ID)
                this.Name = MM_Repository.TitleCase(InValue.ToString());
            else if (Attribute == MM_Communication.Elements.MM_Element.enumAttributes.RTOTMW)
                this.Current = (float)InValue;
            else if (Attribute == MM_Communication.Elements.MM_Element.enumAttributes.LTOTMW)
                this.Max = (float)InValue;

            MM_DCTie FoundTie;
            if (this.Name.StartsWith("DC_", StringComparison.CurrentCultureIgnoreCase) && MM_Repository.DCTies.TryGetValue(this.Name.Substring(0, 4), out FoundTie))
            {
                int Index = Name.EndsWith("_IMPORT", StringComparison.CurrentCultureIgnoreCase) ? 0 : 1;
                if (Attribute == MM_Communication.Elements.MM_Element.enumAttributes.RTOTMW)
                    FoundTie.MW_Monitored[Index] = (float)InValue;
                else if (Attribute == MM_Communication.Elements.MM_Element.enumAttributes.LTOTMW)
                    FoundTie.Limits[Index] = (float)InValue;
            }             
        }
        #endregion        
    }
}
