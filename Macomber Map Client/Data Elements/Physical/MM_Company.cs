using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.Common;
using Macomber_Map.Data_Connections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class contains information on companies (QSEs and TDSPs)
    /// </summary>
    public class MM_Company : MM_Element
    {
        #region Variable Declarations
        /// <summary>
        /// The telephone number for the QSE/TDSP
        /// </summary>
        public String PrimaryPhone;

        /// <summary>
        /// The DUNS for the company
        /// </summary>
        public String DUNS;

        /// <summary>The alias/acronym of the operator</summary>
        public String Alias;

        /// <summary>Whether the company has operatorship of an element</summary>
        public bool OperatesEquipment = false;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a blank CIM company
        /// </summary>
        public MM_Company()
        {
        }

        /// <summary>
        /// Initialize a new CIM Transformer
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew"></param>
        public MM_Company(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Company");
        }

        /// <summary>
        /// Initialize a new CIM Substation
        /// </summary>
        /// <param name="ElementSource">The data source for this substation</param>
        /// <param name="AddIfNew"></param>
        public MM_Company(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Company");
        }

        #endregion
    }
}