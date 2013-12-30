using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using Macomber_Map.Data_Connections;
using System.Xml;
using System.Web;
using System.Reflection;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class handles a note on a particular object
    /// </summary>
    public class MM_Note:MM_Element
    {
        #region Variable declarations
        /// <summary>The unique identifier for this note</summary>
        public Int32 ID;

        /// <summary>When the note was created</summary>
        public DateTime CreatedOn;

        /// <summary>Who authored the note</summary>
        public String Author;

        /// <summary>The text of the note</summary>
        public String Note;

        /// <summary>The element associated with the note.</summary>
        public MM_Element AssociatedElement;

        /// <summary>If the note has been acknowledged</summary>
        public bool Acknowledged = false;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new note from the CIM server
        /// </summary>
        /// <param name="ID">The unique ID of the note</param>
        /// <param name="CreatedOn">When the note was created or updated</param>
        /// <param name="Author">The author of the note</param>
        /// <param name="Note">The text of the note</param>
        /// <param name="TEID">The TEID of the note</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Note(Int32 ID, DateTime CreatedOn, String Author, String Note, Int32 TEID, bool AddIfNew)
        {
            this.ID = ID;
            this.CreatedOn = CreatedOn;
            this.Author = Author;
            this.Note = Note;            
            if (MM_Repository.TEIDs.ContainsKey(TEID))
                AssociatedElement = MM_Repository.TEIDs[TEID];
            else
            {
                AssociatedElement = Data_Integration.MMServer.LoadElement(TEID,AddIfNew);
                MM_Repository.TEIDs.Add(TEID, AssociatedElement);
            }
        }

        /// <summary>
        /// Initialize a new note
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="CreatedOn"></param>
        /// <param name="Author"></param>
        /// <param name="Note"></param>
        /// <param name="Element"></param>
        public MM_Note(Int32 ID, DateTime CreatedOn, String Author, String Note, MM_Element Element)
        {
            this.ID = ID;
            this.CreatedOn = CreatedOn;
            this.Author = Author;
            this.Note = Note;
            this.AssociatedElement = Element;
        }

        /// <summary>
        /// Create a new note from XML configuration
        /// </summary>
        /// <param name="xBase">The base document</param>
        public MM_Note(XmlElement xBase)
        {
            this.ID = XmlConvert.ToInt32(xBase.Attributes["ID"].Value);
            this.CreatedOn = XmlConvert.ToDateTime(xBase.Attributes["CreatedOn"].Value, XmlDateTimeSerializationMode.Unspecified);
            this.Note = xBase.Attributes["Note"].Value;
            this.Author = xBase.Attributes["Author"].Value;
            this.AssociatedElement = MM_Repository.TEIDs[XmlConvert.ToInt32(xBase.Attributes["AssociatedElement"].Value)];
            this.Acknowledged = XmlConvert.ToBoolean(xBase.Attributes["Acknowledged"].Value.ToLower());

        }

        /// <summary>
        /// Create a new note from the database
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew"></param>
        public MM_Note(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        { }
        #endregion

        /// <summary>
        /// Get this note in XML format
        /// </summary>        
        public String XMLString
        {
            get
            {
                return "<Note UID=\"" + ID + "\" CreatedOn=\"" + XmlConvert.ToString(CreatedOn, XmlDateTimeSerializationMode.Local) + "\" Note=\"" + HttpUtility.HtmlEncode(Note) + "\" Author=\"" + this.Author + "\" TEID=\"" + this.AssociatedElement.TEID + "\" Acknowleged=\"" + XmlConvert.ToString(Acknowledged) + "\"/>";
            }
        }

        /// <summary>
        /// Provide the string definition of our note
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Note;
        }
    }
}
