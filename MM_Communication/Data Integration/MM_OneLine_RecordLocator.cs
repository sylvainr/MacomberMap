using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using MM_Communication.Elements;

namespace MM_Communication.Data_Integration
{
    /// <summary>
    /// This class holds information on a one-line record locator
    /// </summary>
    public class MM_OneLine_RecordLocator
    {
        #region Variable declarations
        /// <summary>The internal name of our record locator</summary>
        public String Name;

        /// <summary>The external name of our record locator</summary>
        public String ColumnName;

        /// <summary>The variable type of our element</summary>
        public Type VarType;

        /// <summary>The ID for our element, based on the order it's located</summary>
        public byte Id;

        /// <summary>Our collection of categories</summary>
        public enum enumCategory
        {
            /// <summary>Unknown category</summary>
            Unknown = 0,
            /// <summary>Identity category</summary>
            Identity = 1,
            /// <summary>Status category</summary>
            Status = 2,
        }

        /// <summary>The category for our record locator</summary>
        public enumCategory Category = enumCategory.Unknown;

        /// <summary>Our sub-data source, if any</summary>
        public MM_OneLine_AdditionalDataSource DataSource = null;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new one-line record locator
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="Id"></param>
        /// <param name="DataSource"></param>
        public MM_OneLine_RecordLocator(XmlElement xElem, byte Id, MM_OneLine_AdditionalDataSource DataSource):this(xElem,Id)
        {
            this.DataSource = DataSource;
            DataSource.DatabaseLinkages.Add(this);
        }


        /// <summary>
        /// Initialize a new one-line record locator
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="Id"></param>
        public MM_OneLine_RecordLocator(XmlElement xElem, byte Id)
        {
            this.Id = Id;
            this.Name = xElem.Attributes["Name"].Value;
            this.ColumnName = xElem.Attributes["ColumnName"].Value;
            if (!xElem.HasAttribute("Category"))
                this.Category = enumCategory.Status;
            else
                this.Category = (enumCategory)Enum.Parse(typeof(enumCategory), xElem.Attributes["Category"].Value);
            
            if (xElem.Attributes["VarType"].Value.StartsWith("MM_") || xElem.Attributes["VarType"].Value == "int")
                this.VarType = typeof(Int32);
            else if (xElem.Attributes["VarType"].Value == "float")
                this.VarType = typeof(Single);
            else if (xElem.Attributes["VarType"].Value == "string")
                this.VarType = typeof(String);
            else if (xElem.Attributes["VarType"].Value == "boolean")
                this.VarType = typeof(Boolean);            
        }

        /// <summary>
        /// Retrieve a value and convert to the appropriate type
        /// </summary>
        /// <param name="InValue"></param>
        public Object GetValue(Object InValue)
        {
            if (InValue is byte[])
                if (VarType == typeof(Int32))
                    return BitConverter.ToInt32((byte[])InValue, 0);
                else if (VarType == typeof(Single))
                    return BitConverter.ToSingle((byte[])InValue, 0);
                else if (VarType == typeof(bool))
                    return ((byte[])InValue)[0] == 1;
                else if (VarType == typeof(String))
                    return new UTF8Encoding(false).GetString((byte[])InValue);
                else
                    throw new InvalidOperationException("Unable to get a value from bytes for a " + VarType.Name);
            else if (VarType == typeof(Int32))
                return InValue == null || InValue == DBNull.Value ? 0 : Convert.ToInt32(InValue);
            else if (VarType == typeof(float))
                return InValue == null || InValue == DBNull.Value ? float.NaN : Convert.ToSingle(InValue);
            else if (VarType == typeof(bool))
                return InValue == null || InValue == DBNull.Value ? false : Convert.ToBoolean(InValue);
            else if (VarType == typeof(String))
                return InValue == null || InValue == DBNull.Value ? "" : (String)InValue;
            else
                throw new InvalidOperationException("Unable to convert EMS data type to " + VarType.Name);
        }
        #endregion

        /// <summary>
        /// Report an easy-to-read name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + "/" + ColumnName;
        }

        /// <summary>
        /// Write out our XML for our query
        /// </summary>
        /// <param name="OutXml"></param>
        public void WriteXml(StringBuilder OutXml)
        {
            String[] splStr = ColumnName.Split('>');
            if (splStr.Length == 1)
                if (splStr[0] == "[KeyIndex]" || splStr[0] == "__SUB")
                    OutXml.AppendLine("\t\t\t\t<fld name=\"__SUB\" fldType=\"sub\"/>");
                else
                OutXml.AppendLine("\t\t\t\t<fld name=\"" + splStr[0] + "\"/>");
            else
            {
                OutXml.AppendLine("\t\t\t\t<fld name=\"" + splStr[0] + "\">");
                for (int a = 1; a < splStr.Length; a++)
                    OutXml.AppendLine("\t\t\t\t\t<indFld name=\"" + splStr[a] + "\"/>");
                OutXml.AppendLine("\t\t\t\t</fld>");
            }
        }

    }
}
