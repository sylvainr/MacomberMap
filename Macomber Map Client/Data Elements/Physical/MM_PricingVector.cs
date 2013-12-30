using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.Common;

namespace Macomber_Map.Data_Elements
{
   /// <summary>
    /// This class holds information on a pricing vector and its orientation
    /// </summary>
    public class MM_PricingVector : MM_Element
    {
        #region Variable declarations

        /// <summary>The node to which the vector is connected</summary>
        public MM_Node NodeElement
        {
            get { return _NodeElement; }
            set { _NodeElement = value; }
        }
        private MM_Node _NodeElement;


        /// <summary>The element to which the vector is connected</summary>
        public MM_Element OtherElement
        {
            get { return _OtherElement; }
            set { _OtherElement = value; }
        }
        private MM_Element _OtherElement;

        /// <summary>Whether the flow towards the associated node is positive</summary>
        public bool IsPositive
        {
            get { return _IsPositive; }
            set { _IsPositive = value; }
        }
        private bool _IsPositive;

        /// <summary>The EPS meter associated with the vector</summary>
        public MM_EPSMeter EPSMeter
        {
            get { return _EPSMeter; }
            set { _EPSMeter = value; }
        }
        private MM_EPSMeter _EPSMeter;

        /// <summary>
        /// The resource ID of the pricing vector
        /// </summary>
        public string RID
        {
            get
            {
                if (_EPSMeter == null)
                    return null;
                else
                    return _EPSMeter.RID;
            }            
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new node from a data reader
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_PricingVector(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
        }

        /// <summary>
        /// Initialize a new node from an XML document
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_PricingVector(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource,AddIfNew)
        {
        }

        /// <summary>
        /// Initialize a parameterless node
        /// </summary>
        public MM_PricingVector()
            : base()
        { }
        #endregion
    }
}
