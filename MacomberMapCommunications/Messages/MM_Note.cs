using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapCommunications.Messages
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a note, placed on a piece of equipment
    /// </summary>
    [RetrievalCommand("LoadNotes")]
    public class MM_Note
    {
        /// <summary>The unique identifier for this note</summary>
        public Int32 ID { get; set;}

        /// <summary>When the note was created</summary>
        public DateTime CreatedOn{ get; set;}

        /// <summary>Who authored the note</summary>
        public String Author{ get; set;}

        /// <summary>The text of the note</summary>
        public String Note{ get; set;}

        /// <summary>The element associated with the note.</summary>
        public int AssociatedElement { get; set;}

        /// <summary>If the note has been acknowledged</summary>
        public bool Acknowledged { get; set;}

        /// <summary>
        /// Initialize an empty note
        /// </summary>
        public MM_Note() { }

        /// <summary>
        /// Initialize a note off an XML element
        /// </summary>
        /// <param name="xElem"></param>
        public MM_Note(XmlElement xElem)
        {
            PropertyInfo pI;
            foreach (XmlAttribute xAttr in xElem.Attributes)
                if ((pI = this.GetType().GetProperty(xAttr.Name)) != null)
                    pI.SetValue(this, Convert.ChangeType(xAttr.Value, pI.PropertyType));
        }

        /// <summary>
        /// Initialize a note off a data reader
        /// </summary>
        /// <param name="dRd"></param>
        public MM_Note(DbDataReader dRd)
        {
            PropertyInfo pI;
            for (int a=0; a < dRd.FieldCount; a++)
                 if ((pI = this.GetType().GetProperty(dRd.GetName(a), BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public)) != null)
                    pI.SetValue(this, Convert.ChangeType(dRd[a], pI.PropertyType));
        }

        /// <summary>
        /// Write out our note's information for the note, so that it can be written to a savecase
        /// </summary>
        /// <param name="xW"></param>
        public void WriteXML(XmlTextWriter xW)
        {
            xW.WriteStartElement("Note");
            foreach (PropertyInfo pI in this.GetType().GetProperties())
                xW.WriteAttributeString(pI.Name, pI.GetValue(this).ToString());
            xW.WriteEndElement();
        }
    }
}
