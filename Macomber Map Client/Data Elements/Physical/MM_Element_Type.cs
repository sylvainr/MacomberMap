using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Macomber_Map.Data_Connections;
using System.Data;
using System.Data.Common;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class stores information on an element type
    /// </summary>
    public class MM_Element_Type: MM_Serializable
    {
        #region Variable declarations
        /// <summary>The name of the element type</summary>
        public String Name;
        
        /// <summary>The acroym (short name) for the element type</summary>
        public string Acronym;

        /// <summary>The name used to represent the type in CIM</summary>
        public string CIMName;

        /// <summary>The parameter needed to identify this object from CIM</summary>
        public string CIMParameter;

        /// <summary>
        /// The XML configuration for this element
        /// </summary>
        public XmlElement Configuration;

        /// <summary>
        /// The integer value, used in the SQL versions of the CIM server
        /// </summary>
        public int Index;

        /// <summary>
        /// Handle the loading and saving of element parameters
        /// </summary>
        public XmlElement ElementParameters
        {
            get { return null; }
            set { }
        }
        #endregion


        #region Initialization
        /// <summary>        
        /// Initialize a new element type
        /// </summary>
        /// <param name="Configuration">The XML configuration element</param>        
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Element_Type(XmlElement Configuration, bool AddIfNew)
            : base(Configuration, AddIfNew)
        {
            this.Configuration = Configuration;            
        }

        /// <summary>
        /// Initialize a new element type
        /// </summary>
        /// <param name="dRd"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Element_Type(DbDataReader dRd, bool AddIfNew)
            : base(dRd, AddIfNew)
        { }

        /// <summary>
        /// Initialize a new element type
        /// </summary>
        /// <param name="Name">The name of the element type</param>
        /// <param name="Acronym">The acronym for the element type</param>
        /// <param name="CIMName">The CIM name of the type</param>
        public MM_Element_Type(String Name, String Acronym, String CIMName)
        {
            this.Name = Name;
            this.Acronym = Acronym;
            this.CIMName = CIMName;
        }
        #endregion


        /// <summary>
        /// Return the element type name as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
            
        }

    }
}
