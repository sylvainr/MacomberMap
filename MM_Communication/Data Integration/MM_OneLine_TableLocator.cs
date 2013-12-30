using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MM_Communication.Elements;

namespace MM_Communication.Data_Integration
{
    /// <summary>
    /// This class holds information on a one-line table locator
    /// </summary>
    public class MM_OneLine_TableLocator
    {
        #region Variable declarations
        /// <summary>The element type associated with our record locator</summary>
        public MM_Element.enumElementType ElementType;

        /// <summary>Our requirement that must be false</summary>
        public String RequireFalse = "";

        /// <summary>Our requirement that must be true</summary>
        public String RequireTrue = "";

        /// <summary>The application to target</summary>
        public String Application;

        /// <summary>The database from which the data are retrieved</summary>
        public String Database;

        /// <summary>The value locator for an element in our table</summary>
        public String Values;

        /// <summary>The name of our table</summary>
        public String TableName;

        /// <summary>The linkage to our substation</summary>
        public MM_OneLine_RecordLocator SubstationLinkage;

        /// <summary>The linkage to our name</summary>
        public MM_OneLine_RecordLocator NameLinkage;

        /// <summary>The linkage to our type</summary>
        public MM_OneLine_RecordLocator TypeLinkage;

        /// <summary>Our collection of database linkages</summary>
        public List<MM_OneLine_RecordLocator> DatabaseLinkages = new List<MM_OneLine_RecordLocator>();

        /// <summary>Our collection of control panels, and their database linkages</summary>
        public Dictionary<String, MM_OneLine_RecordLocator[]> ControlPanels = new Dictionary<String, MM_OneLine_RecordLocator[]>();

        /// <summary>The ID of our table locator</summary>
        public int Id;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new table locator
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="Id"></param>
        public MM_OneLine_TableLocator(XmlElement xElem, int Id)
        {
            this.Id = Id;
            this.ElementType = (MM_Element.enumElementType)Enum.Parse(typeof(MM_Element.enumElementType), xElem.ParentNode.Attributes["Name"].Value);
            this.Database = xElem.Attributes["Database"].Value;
            this.Values = xElem.Attributes["Values"].Value;
            this.TableName = xElem.Attributes["TableName"].Value;
            if (xElem.HasAttribute("RequireFalse"))
                this.RequireFalse = xElem.Attributes["RequireFalse"].Value;
            if (xElem.HasAttribute("RequireTrue"))
                this.RequireTrue = xElem.Attributes["RequireTrue"].Value;

            if (xElem.HasAttribute("Application"))
                this.Application = xElem.Attributes["Application"].Value;
            //Now, add in all of our elements
            foreach (XmlElement xChild in xElem.ChildNodes)
                if (xChild.Name == "DatabaseLinkage")
                {
                    MM_OneLine_RecordLocator rLoc = new MM_OneLine_RecordLocator(xChild, (byte)DatabaseLinkages.Count);
                    DatabaseLinkages.Add( rLoc);
                    if (rLoc.Name == "Substation")
                        SubstationLinkage = rLoc;
                    else if (rLoc.Name == "Name")
                        NameLinkage = rLoc;
                    else if (rLoc.Name == "Type" || rLoc.Name == "NodeName")
                        TypeLinkage = rLoc;
                }
                else if (xChild.Name == "ControlPanel")
                {
                    List<MM_OneLine_RecordLocator> RecordLocators = new List<MM_OneLine_RecordLocator>();
                    int SourceNumber = 0;
                    foreach (XmlElement xDataSource in xChild.ChildNodes)
                        if (xDataSource.Name == "DataSource")
                            RecordLocators.Add(new MM_OneLine_RecordLocator(xDataSource, (byte)RecordLocators.Count));
                        else if (xDataSource.Name == "AdditionalDataSource")
                        {
                            MM_OneLine_AdditionalDataSource DataSource = new MM_OneLine_AdditionalDataSource(xDataSource, (byte)++SourceNumber, this);
                            foreach (XmlElement xAddlSource in xDataSource.ChildNodes)
                                RecordLocators.Add(new MM_OneLine_RecordLocator(xAddlSource, (byte)RecordLocators.Count, DataSource));                                
                        }
                    ControlPanels.Add(xChild.Attributes["Title"].Value, RecordLocators.ToArray());
                }

        }
        #endregion

        /// <summary>
        /// Determine whether an XML element fits into our table locator
        /// </summary>
        /// <param name="xElem"></param>
        /// <returns></returns>
        public bool CheckElement(XmlElement xElem)
        {
            if (!String.IsNullOrEmpty(RequireTrue) && xElem.HasAttribute(RequireTrue) && !XmlConvert.ToBoolean(xElem.Attributes[RequireTrue].Value))
                return false;
            else if (!String.IsNullOrEmpty(RequireFalse) && xElem.HasAttribute(RequireFalse) && XmlConvert.ToBoolean(xElem.Attributes[RequireFalse].Value))
                return false;
            else
                return true;
        }


        /// <summary>
        /// Build our query out
        /// </summary>
        /// <param name="Elements"></param>
        /// <param name="Fields"></param>
        /// <param name="Keys"></param>
        /// <param name="Application"></param>
        /// <param name="ElementLookups"></param>
        /// <param name="OutgoingElements"></param>
        /// <param name="AppPermissions"></param>
        public void BuildQuery(List<XmlElement> Elements, SortedDictionary<String, List<String>[]> OutgoingElements, String Application, Dictionary<String, byte[]> ElementLookups, Dictionary<String, bool[]> AppPermissions)
        {
            if (!AppPermissions.ContainsKey(Application))
                return;

            String ThisTitle = Application + "/" + Database + "/" + TableName;
            List<String>[] OutVals;
            if (!OutgoingElements.TryGetValue(ThisTitle, out OutVals))
                OutgoingElements.Add(ThisTitle, OutVals = new List<string>[] { new List<String>(), new List<String>() });

            //Write out all of our fields to be retrieved
            foreach (MM_OneLine_RecordLocator rLoc in DatabaseLinkages)
            {
                String[] splStr = rLoc.ColumnName.Split('>');
                StringBuilder OutXml = new StringBuilder();
                if (splStr.Length == 1)
                    OutXml.Append("\t\t\t\t<fld name=\"" + splStr[0] + "\"/>");
                else
                {
                    OutXml.AppendLine("\t\t\t\t<fld name=\"" + splStr[0] + "\">");
                    for (int a = 1; a < splStr.Length; a++)
                        OutXml.AppendLine("\t\t\t\t\t<indFld name=\"" + splStr[a] + "\"/>");
                    OutXml.Append("\t\t\t\t</fld>");
                }
                CheckAddLine(OutVals[0], OutXml.ToString());
            }

            //Now, write our key locators for all elements
            foreach (XmlElement xElem in Elements)
            {
                String[] OutKeyCollection;
                if (Values.Contains("{Measurement}") && Values.Contains("{Station}") && xElem.HasAttribute("BaseElement.Sub1Name"))
                    OutKeyCollection = new String[] { 
                        Values.Replace("{Station}", xElem.Attributes["BaseElement.Sub1Name"].Value).Replace("{Measurement}", "MW"), 
                        Values.Replace("{Station}", xElem.Attributes["BaseElement.Sub1Name"].Value).Replace("{Measurement}", "MVAR"), 
                        Values.Replace("{Station}", xElem.Attributes["BaseElement.Sub1Name"].Value).Replace("{Measurement}", "MVA"), 
                        Values.Replace("{Station}", xElem.Attributes["BaseElement.Sub2Name"].Value).Replace("{Measurement}", "MW"), 
                        Values.Replace("{Station}", xElem.Attributes["BaseElement.Sub2Name"].Value).Replace("{Measurement}", "MVAR"), 
                        Values.Replace("{Station}", xElem.Attributes["BaseElement.Sub2Name"].Value).Replace("{Measurement}", "MVA"), 
                    };
                else if (Values.Contains("{Measurement}"))
                    OutKeyCollection = new String[] { Values.Replace("{Measurement}", "MW"), Values.Replace("{Measurement}", "MVAR"), Values.Replace("{Measurement}", "MVA") };
                
                
                else if (Values.Contains("{Station}") && xElem.HasAttribute("BaseElement.Sub1Name"))
                    OutKeyCollection = new String[] { Values.Replace("{Station}", xElem.Attributes["BaseElement.Sub1Name"].Value), Values.Replace("{Station}", xElem.Attributes["BaseElement.Sub2Name"].Value) };
                else
                    OutKeyCollection = new String[] { Values };

                for (int a = 0; a < OutKeyCollection.Length; a++)
                {
                    String OutgoingKey = OutKeyCollection[a];
                    String OutKey = OutgoingKey;
                    while (OutKey.Contains("{"))
                    {
                        String ValueToHandle = OutKey.Substring(OutKey.IndexOf("{"), OutKey.IndexOf("}") + 1 - OutKey.IndexOf("{"));
                        if (ValueToHandle == "{Station}")
                        if (!xElem.HasAttribute("BaseElement.SubName"))
                            System.Windows.Forms.MessageBox.Show("Missing subname for " + xElem.OuterXml);
                        else
                            OutKey = OutKey.Replace(ValueToHandle, xElem.Attributes["BaseElement.SubName"].Value.ToUpper());                        
                        else if (ValueToHandle == "{Name}")
                            OutKey = OutKey.Replace(ValueToHandle, xElem.Attributes["BaseElement.Name"].Value.ToUpper());
                        else if (ValueToHandle == "{AssociatedNode}")
                            OutKey = OutKey.Replace("{AssociatedNode}", xElem.Attributes["BaseElement.AssociatedNodeName"].Value.ToUpper());
                        else if (ValueToHandle == "{XFName}")
                            OutKey = OutKey.Replace("{XFName}", xElem.ParentNode.Attributes["BaseElement.TransformerName"].Value.ToUpper());
                        else if (ValueToHandle == "{XFFullName}")
                            OutKey = OutKey.Replace("{XFFullName}", xElem.Attributes["BaseElement.TransformerName"].Value.ToUpper());

                    }

                    Dictionary<String, String> Lookups = new Dictionary<string, string>();
                    StringBuilder OutXml = new StringBuilder();
                    OutXml.AppendLine("\t\t\t\t<qns:key>");
                    foreach (String KeyPart in OutKey.Split(','))
                    {
                        String[] splStr = KeyPart.Split('=');
                        Lookups.Add(splStr[0].Trim(), splStr[1].Trim());
                        OutXml.AppendLine("\t\t\t\t\t<keyPart rec=\"" + splStr[0].Trim() + "\" val=\"" + splStr[1].Trim().Replace("&","&amp;") + "\"/>");
                    }
                    OutXml.Append("\t\t\t\t</qns:key>");


                    //Now, add in our record locator as appropriate.
                    if (Database == "SCADA" && TableName == "ANALOG")                                            
                            ElementLookups.Add(Application + "_" + Database + "_" + TableName + "_" + Lookups["SUBSTN"] + "_" + Lookups["DEVTYP"] + "_" + Lookups["DEVICE"] + "_" + Lookups["MEAS"] + "_" + Lookups["ANALOG"], BuildBytes(XmlConvert.ToInt32(xElem.Attributes["BaseElement.TEID"].Value), Id, Application, Lookups["ANALOG"],a));
                    else if (Database == "SCADA" && TableName == "POINT")
                        ElementLookups.Add(Application + "_" + Database + "_" + TableName + "_" + Lookups["SUBSTN"] + "_" + Lookups["DEVTYP"] + "_" + Lookups["DEVICE"] + "_" + Lookups["MEAS"] + "_" + Lookups["POINT"], BuildBytes(XmlConvert.ToInt32(xElem.Attributes["BaseElement.TEID"].Value), Id, Application, Lookups["POINT"], a));
                    else if (Database == "State Estimator" && TableName == "XF2")
                        //ElementLookups.Add(Application + "_" + Database + "_" + TableName + "_" + Lookups["ST"] + "_" + xElem.ParentNode.Attributes["BaseElement.Name"].Value + "_" + xElem.Attributes["BaseElement.AssociatedNodeName"].Value, BuildBytes(XmlConvert.ToInt32(xElem.Attributes["BaseElement.TEID"].Value), Id, Application, a));
                        ElementLookups.Add(Application + "_" + Database + "_" + TableName + "_" + Lookups["ST"] + "_" + xElem.ParentNode.Attributes["BaseElement.TransformerName"].Value + "_" + xElem.Attributes["BaseElement.AssociatedNodeName"].Value, BuildBytes(XmlConvert.ToInt32(xElem.Attributes["BaseElement.TEID"].Value), Id, Application, a));
                    else
                        ElementLookups.Add(Application + "_" + Database + "_" + TableName + "_" + Lookups["ST"] + "_" + xElem.Attributes["BaseElement.Name"].Value, BuildBytes(XmlConvert.ToInt32(xElem.Attributes["BaseElement.TEID"].Value), Id, Application, a));


                    //Depending on our data type, handle appropriately.
                    CheckAddLine(OutVals[1], OutXml.ToString());
                }
            }
        }


        /// <summary>
        /// Build a byte identifier: [TEID 0-4][Table Locator]
        /// </summary>
        /// <param name="TEID"></param>
        /// <param name="TableLocatorId"></param>
        /// <param name="Application"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        private byte[] BuildBytes(Int32 TEID, int TableLocatorId, String Application, int Index)
        {
            byte[] OutBytes = new byte[7];
            BitConverter.GetBytes(TEID).CopyTo(OutBytes, 0);
            OutBytes[4] = (byte)TableLocatorId;
            if (Application == "SCADA")
                OutBytes[5] = 0;
            else if (Application == "StateEstimator")
                OutBytes[5] = 1;
            else if (Application == "DTSPSM")
                OutBytes[5] = 2;
            else if (Application == "StudySystem")
                OutBytes[5] = 3;
            OutBytes[6] = (byte)Index;
            return OutBytes;
        }

        /// <summary>
        /// Build a byte identifier: [TEID 0-4][Table Locator][SCADA type]
        /// </summary>
        /// <param name="TEID"></param>
        /// <param name="TableLocatorId"></param>
        /// <param name="SCADAType"></param>
        /// <param name="Application"></param>
        /// <returns></returns>
        private byte[] BuildBytes(Int32 TEID, int TableLocatorId, String Application, String SCADAType, int Index)
        {
            byte[] OutBytes = new byte[8];
            BitConverter.GetBytes(TEID).CopyTo(OutBytes,0);
            OutBytes[4] = (byte)TableLocatorId;


            if (Application == "SCADA")
                OutBytes[5] = 0;
            else if (Application == "StateEstimator")
                OutBytes[5] = 1;
            else if (Application == "DTSPSM")
                OutBytes[5] = 2;
            else if (Application == "StudySystem")
                OutBytes[5] = 3;


            if (SCADAType == "MW")
                OutBytes[6] = 1;
            else if (SCADAType == "MVAR")
                OutBytes[6] = 2;
            else if (SCADAType == "MVA")
                OutBytes[6] = 3;
            else if (SCADAType == "KV")
                OutBytes[6] = 4;
            OutBytes[7] = (byte)Index;
            return OutBytes;
        }

        /// <summary>
        /// Determine whether to add in a new line
        /// </summary>
        /// <param name="OutList"></param>
        /// <param name="StringToAdd"></param>
        private void CheckAddLine(List<String> OutList, String StringToAdd)
        {
            if (!OutList.Contains(StringToAdd))
                OutList.Add(StringToAdd);
        }

        /// <summary>
        /// Write out the XML for our object
        /// </summary>
        /// <param name="OutXml"></param>
        /// <param name="Elements"></param>
        /// <param name="Application"></param>
        public void WriteXml(StringBuilder OutXml, List<XmlElement> Elements, String Application)
        {
            WriteXml(OutXml, Elements, Application, Database, TableName,DatabaseLinkages, Values);
        }

        /// <summary>
        /// Write our XML component for this table locator
        /// </summary>
        /// <param name="Elements"></param>
        /// <param name="Application"></param>
        /// <param name="OutXml"></param>
        public static void WriteXml(StringBuilder OutXml, List<XmlElement> Elements, String Application, String Database, String TableName, List<MM_OneLine_RecordLocator> DatabaseLinkages, String Values)
        {
            OutXml.AppendLine("\t<app name=\"" + Application + "\">");
            OutXml.AppendLine("\t\t<db name=\"" + Database + "\">");
            OutXml.AppendLine("\t\t\t<rec name=\"" + TableName + "\">");

            //Write out all of our fields to be retrieved
            foreach (MM_OneLine_RecordLocator rLoc in DatabaseLinkages)
            {
                String[] splStr = rLoc.ColumnName.Split('>');
                if (splStr.Length == 1)
                    OutXml.AppendLine("\t\t\t\t<fld name=\"" + splStr[0] + "\"/>");
                else
                {
                    OutXml.AppendLine("\t\t\t\t<fld name=\"" + splStr[0] + "\">");
                    for (int a = 1; a < splStr.Length; a++)
                        OutXml.AppendLine("\t\t\t\t\t<indFld name=\"" + splStr[a] + "\"/>");
                    OutXml.AppendLine("\t\t\t\t</fld>");
                }
            }

            //Now, write our key locators for all elements
            foreach (XmlElement xElem in Elements)
            {
                String[] OutKeyCollection;
                if (Values.Contains("{Measurement"))
                    OutKeyCollection = new String[] { Values.Replace("{Measurement}", "MW"), Values.Replace("{Measurement}", "MVAR"), Values.Replace("{Measurement}", "MVA") };
                else if (Values.Contains("{Station}") && xElem.HasAttribute("BaseElement.Sub1Name"))
                    OutKeyCollection = new String[] { Values.Replace("{Station}", xElem.Attributes["BaseElement.Sub1Name"].Value), Values.Replace("{Station}", xElem.Attributes["BaseElement.Sub2Name"].Value) };
                else
                    OutKeyCollection = new String[] { Values };

                foreach (String OutgoingKey in OutKeyCollection)
                {
                    String OutKey = OutgoingKey;
                    while (OutKey.Contains("{"))
                    {
                        String ValueToHandle = OutKey.Substring(OutKey.IndexOf("{"), OutKey.IndexOf("}") + 1 - OutKey.IndexOf("{"));
                        if (ValueToHandle == "{Station}")
                            OutKey = OutKey.Replace(ValueToHandle, xElem.Attributes["BaseElement.SubName"].Value.ToUpper());
                        else if (ValueToHandle == "{Name}")
                            OutKey = OutKey.Replace(ValueToHandle, xElem.Attributes["BaseElement.Name"].Value.ToUpper());
                    }
                    OutXml.AppendLine("\t\t\t\t<qns:key>");
                    foreach (String KeyPart in OutKey.Split(','))
                        OutXml.AppendLine("\t\t\t\t\t<keyPart rec=\"" + KeyPart.Split('=')[0].Trim() + "\" val=\"" + KeyPart.Split('=')[1].Trim() + "\"/>");
                    OutXml.AppendLine("\t\t\t\t</qns:key>");
                }
            }
            OutXml.AppendLine("\t\t\t</rec>");
            OutXml.AppendLine("\t\t</db>");
            OutXml.AppendLine("\t</app>");

        }

        /// <summary>
        /// Return a human-readable string 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ElementType.ToString() + " - " +  Application + "_" + Database + "_" + TableName;
        }
     }
}