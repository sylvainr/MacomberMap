using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Drawing;
using Macomber_Map.Data_Elements.Weather;
using System.Reflection;
using Macomber_Map.Data_Connections;
using System.Data.OracleClient;
using System.Data.Common;
namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class offers the interface to saving and loading an element's XML.
    /// </summary>
    public class MM_Serializable
    {
        /// <summary>The collection of unmatched types for debugging</summary>
        public static Dictionary<String, XmlAttribute> UnmatchedTypes = new Dictionary<string, XmlAttribute>();


        /// <summary>
        /// Create a new serializable element
        /// </summary>
        public MM_Serializable()
        { }

        /// <summary>
        /// Create a new serializable element
        /// </summary>
        /// <param name="ElementSource">The element source for the data</param>        
        /// <param name="AddIfNew">Whether to add new elements to the main repository</param>
        public MM_Serializable(XmlElement ElementSource, bool AddIfNew)
        {
            ReadXML(ElementSource, AddIfNew);
        }

        /// <summary>
        /// Initialize an element from a data reader
        /// </summary>
        /// <param name="dRd"></param>
        /// <param name="AddIfNew"></param>
        public MM_Serializable(DbDataReader dRd, bool AddIfNew)
        {
            ReadDatabase(dRd,AddIfNew);            
        }


      /*  /// <summary>
        /// Handle a LOB value
        /// </summary>
        /// <param name="ar"></param>
        private void AssignLobValue(IAsyncResult ar)
        {
            object[] inObj = (object[])ar.AsyncState;
            int Results = (inObj[2] as OracleLob).EndRead(ar);

            if ((inObj[2] as OracleLob).LobType == OracleType.Clob)
                AssignValue((string)inObj[0], Encoding.Unicode.GetString((byte[])inObj[1]), this);
            else if ((inObj[2] as OracleLob).IsNull)
            { }
            else
                AssignValue((string)inObj[0], (byte[])inObj[1], this);                
        }*/

        /// <summary>
        /// Assign a value to a specific element
        /// </summary>
        /// <param name="ColName">The column name</param>
        /// <param name="ColValue">The column value</param>
        /// <param name="Target">The target</param>
        /// <param name="AddIfNew"></param>
        public static void AssignValue(String ColName, Object ColValue, Object Target, bool AddIfNew)
        {
            BindingFlags OutFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy;
            if (Target is Type)
                OutFlags |= BindingFlags.Static;
            else
                OutFlags |= BindingFlags.Instance;


            Type TargetType = Target is Type ? Target as Type : Target.GetType();
            MemberInfo mI = null;
            if (ColValue is DBNull == false)
                try
                {
                    if (ColName.Equals("Winding1", StringComparison.CurrentCultureIgnoreCase))
                        (Target as MM_Transformer).Winding1.TEID = Convert.ToInt32(ColValue);
                    else if (ColName.Equals("Winding2", StringComparison.CurrentCultureIgnoreCase))
                        (Target as MM_Transformer).Winding2.TEID = Convert.ToInt32(ColValue);
                    else if ((mI = TargetType.GetField(ColName, OutFlags)) != null)
                        AssignValue(mI, RetrieveConvertedValue((mI as FieldInfo).FieldType, ColValue, Target, AddIfNew), Target);
                    else if ((mI = TargetType.GetField("_" + ColName, OutFlags)) != null)
                        AssignValue(mI, RetrieveConvertedValue((mI as FieldInfo).FieldType, ColValue, Target, AddIfNew), Target);
                    else if ((mI = TargetType.GetProperty(ColName, OutFlags)) != null)
                        AssignValue(mI, RetrieveConvertedValue((mI as PropertyInfo).PropertyType, ColValue, Target, AddIfNew), Target);
                    //else
                      //  Program.WriteConsoleLine("Unable to locate attribute " + ColName + "!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error deserializing: " + ex.Message);
                }
        }


        /// <summary>
        /// Write the boundary information to the XML Text writer
        /// </summary>
        /// <param name="xW">The outgoing text writer</param>
        public void WriteXML(XmlTextWriter xW)
        {
            if (this is MM_AlarmViolation)
                xW.WriteStartElement("Violation");
            else if (this is MM_Element && (this as MM_Element).ElemType != null)
                xW.WriteStartElement((this as MM_Element).ElemType.Name);
            else
                xW.WriteStartElement(this.GetType().Name);

            foreach (MemberInfo mI in this.GetType().GetMembers())
                if (mI is MemberInfo || (mI is PropertyInfo && (mI as PropertyInfo).CanWrite))
                {
                    Object InValue = null;
                    if (mI is FieldInfo)
                        InValue = (mI as FieldInfo).GetValue(this);
                    else if (mI is PropertyInfo && (mI as PropertyInfo).CanWrite)
                        InValue = (mI as PropertyInfo).GetValue(this, null);


                    if (InValue is System.Collections.IDictionary)
                    {
                        if ((InValue as System.Collections.IDictionary).Count > 0)
                            WriteXMLData(mI.Name, InValue, xW);
                    }
                    else if (InValue is System.Collections.IList)
                    {
                        if ((InValue as System.Collections.IList).Count > 0)
                            WriteXMLData(mI.Name, InValue, xW);
                    }
                    else if (InValue != null)
                            WriteXMLData(mI.Name, InValue, xW);
                }
            xW.WriteEndElement();
        }

        /// <summary>
        /// Go through all columns and load in the values
        /// </summary>
        /// <param name="dRd"></param>
        /// <param name="AddIfNew"></param>
        public void ReadDatabase(DbDataReader dRd, bool AddIfNew)
        {
            for (int Col = 0; Col < dRd.FieldCount; Col++)
                AssignValue(dRd.GetName(Col), dRd.GetValue(Col), this, AddIfNew);
        }

        
        /// <summary>
        /// Go through all attributes and sub-elements within this element, and load in the values
        /// </summary>
        /// <param name="XmlToRead">The Xml Element to be read</param>
        /// <param name="AddIfNew">Whether to add a new element to our master repository</param>
        public void ReadXML(XmlElement XmlToRead, bool AddIfNew)
        {
      
            foreach (XmlAttribute xAttr in XmlToRead.Attributes)
            {
                FieldInfo fI = this.GetType().GetField(xAttr.Name);
                FieldInfo fI2 = this.GetType().GetField("_" + xAttr.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                PropertyInfo pI = this.GetType().GetProperty(xAttr.Name);
                if (fI != null)
                    ConvertAndStoreValue(fI, fI.FieldType, xAttr.Value, AddIfNew);
                else if (fI2 != null)
                    ConvertAndStoreValue(fI2, fI2.FieldType, xAttr.Value, AddIfNew);
                else if (pI != null)
                    ConvertAndStoreValue(pI, pI.PropertyType, xAttr.Value, AddIfNew);
                else if (!UnmatchedTypes.ContainsKey(this.GetType().Name + "." + xAttr.Name))
                    UnmatchedTypes.Add(this.GetType().Name + "." + xAttr.Name, xAttr);                
            }
        }

        /// <summary>
        /// Go through all attributes and sub-elements within the target object, and load in the values
        /// </summary>
        /// <param name="XmlToRead">The XML to be read</param>
        /// <param name="TargetObject">The target object</param>
        /// <param name="AddIfNew">Whether to add the element into our master repository</param>        
        public static void ReadXml(XmlElement XmlToRead, object TargetObject, bool AddIfNew)
        {
           /* foreach (XmlAttribute xAttr in XmlToRead.Attributes)
            {
                object ThisObject = TargetObject;
                String[] splStr = xAttr.Name.Split('.');
                for (int a = 0; a < splStr.Length; a++)
                    try
                    {
                        MemberInfo[] mI = ThisObject.GetType().GetMember(splStr[a], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.IgnoreCase);
                        if (mI.Length == 0)
                            return;
                        else if (a == splStr.Length - 1 && mI[0] is FieldInfo)
                            (mI[0] as FieldInfo).SetValue(ThisObject, RetrieveConvertedValue((mI[0] as FieldInfo).FieldType, xAttr.Value, ThisObject));
                        else if (a == splStr.Length - 1 && mI[0] is PropertyInfo && (mI[0] as PropertyInfo).CanWrite)
                            (mI[0] as PropertyInfo).SetValue(ThisObject, RetrieveConvertedValue((mI[0] as PropertyInfo).PropertyType, xAttr.Value, ThisObject), null);
                        else if (mI[0] is FieldInfo)
                        {
                            Object TryObject = (mI[0] as FieldInfo).GetValue(ThisObject);
                            if (TryObject != null)
                                ThisObject = TryObject;
                            else if (typeof(MM_Element).IsAssignableFrom((mI[0] as FieldInfo).FieldType))
                                (mI[0] as FieldInfo).SetValue(ThisObject, ThisObject = MM_Element.CreateElement(XmlToRead, mI[0].Name));
                        }

                        else if (mI[0] is PropertyInfo)
                        {
                            Object TryObject = (mI[0] as PropertyInfo).GetValue(ThisObject, null);
                            if (TryObject != null)
                                ThisObject = TryObject;
                            else if (typeof(MM_Element).IsAssignableFrom((mI[0] as PropertyInfo).PropertyType))
                                (mI[0] as PropertyInfo).SetValue(ThisObject, ThisObject = MM_Element.CreateElement(XmlToRead, mI[0].Name), null);

                        }
                    }
                    catch (Exception ex)
                    {
                    }
            }
           /*/ foreach (XmlAttribute xAttr in XmlToRead.Attributes)
            {
                FieldInfo fI = TargetObject.GetType().GetField(xAttr.Name);
                FieldInfo fI2 = TargetObject.GetType().GetField("_" + xAttr.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                PropertyInfo pI = TargetObject.GetType().GetProperty(xAttr.Name);
                if (fI != null)
                    fI.SetValue(TargetObject, RetrieveConvertedValue(fI.FieldType, xAttr.Value, TargetObject,AddIfNew));                    
                else if (fI2 != null)
                    fI2.SetValue(TargetObject, RetrieveConvertedValue(fI2.FieldType, xAttr.Value, TargetObject,AddIfNew));
                else if (pI != null && pI.CanWrite)
                {
                    // ***DELETE THE TRY/CATCH BITS and Leave the call to 
                    // pI.SetValue() - temporary only.  MBY ***
                    try
                    {
                        pI.SetValue(TargetObject, RetrieveConvertedValue(pI.PropertyType, xAttr.Value, TargetObject,AddIfNew), null);
                    }
                    catch (Exception e)
                    {
                        string s = e.Message;
                    }
                }
               //  else
                //    Program.WriteConsoleLine("Unable to locate attribute " + xAttr.Name + " for " + TargetObject.ToString() + "!");
            }
        }

        /// <summary>
        /// Assign a particular value to a field/property
        /// </summary>
        /// <param name="fpI">The field/property's information</param>
        /// <param name="OutgoingValue">The outgoing value</param>
        /// <param name="Target">The target to receive the new value</param>
        private static void AssignValue(MemberInfo fpI, Object OutgoingValue, Object Target)
        {
            if (fpI is FieldInfo)
                (fpI as FieldInfo).SetValue(Target, OutgoingValue);
            else if (fpI is PropertyInfo && (fpI as PropertyInfo).CanWrite)
                (fpI as PropertyInfo).SetValue(Target,OutgoingValue,null);
        }

        /// <summary>
        /// Convert a value into a specified type
        /// </summary>
        /// <param name="OutgoingType">The outgoing value's type</param>
        /// <param name="OutgoingValue">The value in string form</param>
        /// <param name="Target">The recipient of the data</param>
        /// <param name="AddIfNew">Whether to add a new element to our master repository</param>        
        public static object RetrieveConvertedValue(Type OutgoingType, Object OutgoingValue, object Target, bool AddIfNew)
        {
            try
            {
                if (OutgoingType == typeof(MM_Transformer) && OutgoingValue is Int32)
                    return MM_Repository.TEIDs[(Int32)OutgoingValue];
                else if (OutgoingType == typeof(MM_TransformerWinding) && Target is MM_Transformer)
                {
                    MM_TransformerWinding OutWinding = MM_Repository.TEIDs[Convert.ToInt32(OutgoingValue)] as MM_TransformerWinding;
                    OutWinding.Transformer = Target as MM_Transformer;
                    return OutWinding;
                }
                else if (OutgoingType == typeof(MM_Boundary))
                {
                    foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                        if (Bound.Index == Convert.ToInt32(OutgoingValue))
                            return Bound;
                    return MM_Repository.Counties["STATE"];
                }
                else if ((OutgoingType == typeof(MM_Element) || OutgoingType.BaseType == typeof(MM_Element)) && IsNumeric(OutgoingValue))
                    return Data_Integration.LocateElement(Convert.ToInt32(OutgoingValue), OutgoingType, AddIfNew);                        
                else if (OutgoingType == typeof(List<MM_KVLevel>))
                {
                    List<MM_KVLevel> KVLevels = new List<MM_KVLevel>();
                    foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                        if ((Convert.ToInt32(OutgoingValue) & KVLevel.Index) == KVLevel.Index)
                            KVLevels.Add(KVLevel);
                    KVLevels.TrimExcess();
                    return KVLevels;
                }
                else if (OutgoingType == typeof(MM_KVLevel) && IsNumeric(OutgoingValue))
                {
                    foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                        if (KVLevel.Index == Convert.ToInt32(OutgoingValue))
                            return KVLevel;
                    return MM_Repository.KVLevels["Other KV"];
                }
                else if (OutgoingType == typeof(MM_KVLevel) && OutgoingValue is string)
                {
                    foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                        if (KVLevel.Name == (string)OutgoingValue)
                            return KVLevel;
                    return MM_Repository.KVLevels["Other KV"];
                }
                else if (OutgoingType == typeof(MM_Element_Type) && IsNumeric(OutgoingValue))
                {
                    foreach (MM_Element_Type ElemType in MM_Repository.ElemTypes.Values)
                        if (ElemType.Index == Convert.ToInt32(OutgoingValue))
                            return ElemType;
                    return null;
                }
                else if (OutgoingType == typeof(MM_Element_Type) && OutgoingValue is string)
                {
                    foreach (MM_Element_Type ElemType in MM_Repository.ElemTypes.Values)
                        if (ElemType.Name.Equals((string)OutgoingValue, StringComparison.CurrentCultureIgnoreCase))
                            return ElemType;
                    return null;
                }
                else if (OutgoingType == typeof(MM_Element[]) && OutgoingValue is string)
                {
                    List<MM_Element> OutElems = new List<MM_Element>();
                    foreach (String str in ((string)OutgoingValue).Split(','))
                        if (MM_Repository.TEIDs.ContainsKey(Int32.Parse(str)))
                            OutElems.Add(MM_Repository.TEIDs[Int32.Parse(str)]);
                    return OutElems.ToArray();
                }
                 
                else if (OutgoingType == typeof(List<MM_Element_Type>))
                {
                    String OutgoingString = (string)OutgoingValue;
                    List<MM_Element_Type> ElemTypes = new List<MM_Element_Type>();
                    foreach (MM_Element_Type ElemType in MM_Repository.ElemTypes.Values)
                        if (ElemType.Index < OutgoingString.Length && OutgoingString[ElemType.Index] == '1')
                            ElemTypes.Add(ElemType);
                    ElemTypes.TrimExcess();
                    return ElemTypes;
                }
                else if (OutgoingType == typeof(MM_Element_Type[]))
                {
                    String OutgoingString = (string)OutgoingValue;
                    List<MM_Element_Type> ElemTypes = new List<MM_Element_Type>();
                    foreach (MM_Element_Type ElemType in MM_Repository.ElemTypes.Values)
                        if (ElemType.Index < OutgoingString.Length && OutgoingString[ElemType.Index] == '1')
                            ElemTypes.Add(ElemType);
                    ElemTypes.TrimExcess();
                    return ElemTypes.ToArray();
                }    
                else if (OutgoingType == typeof(MM_Zone) && OutgoingValue is Int32)
                {                    
                        foreach (MM_Zone Zone in MM_Repository.Zones.Values)
                            if (Zone.Index == Convert.ToInt32(OutgoingValue))
                                return Zone;
                    return null;
                }
                else if (OutgoingType == typeof(MM_Zone) && OutgoingValue is String)
                {
                    MM_Zone FoundZone;
                    if (!MM_Repository.Zones.TryGetValue((string)OutgoingValue, out FoundZone))
                        MM_Repository.Zones.Add((String)OutgoingValue, FoundZone = new MM_Zone((string)OutgoingValue, MM_Repository.Zones.Count));
                    return FoundZone;                    
                }
                else if (OutgoingType == typeof(PointF) && OutgoingValue is String)
                {
                    String[] splStr = ((string)OutgoingValue).Split(',');
                    return new PointF(float.Parse(splStr[0]), float.Parse(splStr[1]));
                }
                else if (OutgoingType == typeof(Point) && OutgoingValue is String)
                {
                    String[] splStr = ((string)OutgoingValue).Split(',');
                    return new Point(int.Parse(splStr[0]), int.Parse(splStr[1]));
                }
                else if (OutgoingType == typeof(Size) && OutgoingValue is String)
                {
                    String[] splStr = ((string)OutgoingValue).Split(',');
                    return new Size(int.Parse(splStr[0]), int.Parse(splStr[1]));
                }
                else if (OutgoingType == typeof(PointF[]) && OutgoingValue is String)
                {
                    String[] splStr = ((string)OutgoingValue).Split(',');
                    PointF[] OutPoints = new PointF[splStr.Length / 2];
                    for (int a = 0; a < splStr.Length; a += 2)
                        OutPoints[a / 2] = new PointF(Convert.ToSingle(splStr[a]), Convert.ToSingle(splStr[a + 1]));
                    return OutPoints;
                }
                else if (OutgoingType == typeof(Single[]) && OutgoingValue is String)
                {
                    String[] splStr = ((string)OutgoingValue).Split(',');
                    Single[] OutValues = new Single[splStr.Length];
                    for (int a = 0; a < splStr.Length; a++)
                        OutValues[a] = Convert.ToSingle(splStr[a]);
                    return OutValues;
                }
                else if (OutgoingType == typeof(Rectangle))
                {
                    string[] splStr = ((string)OutgoingValue).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new Rectangle(int.Parse(splStr[0]), int.Parse(splStr[1]), int.Parse(splStr[2]), int.Parse(splStr[3]));
                }
                else if (OutgoingType == typeof(RectangleF))
                {
                    string[] splStr = ((string)OutgoingValue).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new RectangleF(float.Parse(splStr[0]), float.Parse(splStr[1]), float.Parse(splStr[2]), float.Parse(splStr[3]));
                }
                else if (OutgoingType == typeof(PointF[]) && OutgoingValue is Byte[])
                {
                    byte[] inBytes = (byte[])OutgoingValue;
                    PointF[] OutPoints = new PointF[inBytes.Length / 8];
                    for (int a = 0; a < inBytes.Length; a += 8)
                        OutPoints[a / 8] = new PointF(BitConverter.ToSingle(inBytes, a + 4), BitConverter.ToSingle(inBytes, a));
                    return OutPoints;
                }
                else if (OutgoingType == typeof(XmlElement) && OutgoingValue is String)
                {
                    XmlDocument OutDoc = new XmlDocument();
                    OutDoc.LoadXml((string)OutgoingValue);
                    return OutDoc.DocumentElement;
                }
                else if (OutgoingType == typeof(MM_Generation_Type) && OutgoingValue is String)
                {
                    foreach (MM_Generation_Type GenType in MM_Repository.GenerationTypes.Values)
                        if (GenType.Name == (string)OutgoingValue)
                            return GenType;
                    return null;
                }
                else if (OutgoingType.IsEnum && OutgoingValue is string)
                    return Enum.Parse(OutgoingType, (string)OutgoingValue);
                else if (OutgoingType == typeof(Color) && OutgoingValue is string)
                    return ColorTranslator.FromHtml((string)OutgoingValue);
                else if (OutgoingType == typeof(Color) && OutgoingValue is int)
                    return Color.FromArgb((int)OutgoingValue);
                else if (OutgoingType == typeof(Double[]) && OutgoingValue is String)
                {
                    List<Double> OutDbl = new List<double>();
                    foreach (String str in ((String)OutgoingValue).Split(','))
                        OutDbl.Add(Double.Parse(str));
                    return OutDbl.ToArray();
                }
                else if (OutgoingType == typeof(Font) && OutgoingValue is string)
                    return new FontConverter().ConvertFromString((string)OutgoingValue);
                else if (OutgoingType == typeof(Bitmap) && OutgoingValue is string)
                    using (System.IO.MemoryStream mS = new System.IO.MemoryStream(Convert.FromBase64String(OutgoingValue.ToString())))
                        return Bitmap.FromStream(mS);
                else if (OutgoingType == typeof(MM_Substation[]) && OutgoingValue is string)
                {
                    String[] splStr = OutgoingValue.ToString().Split(',');
                    MM_Substation[] OutSub = new MM_Substation[splStr.Length];
                    for (int a = 0; a < splStr.Length; a++)
                        OutSub[a] = Data_Integration.LocateElement(Convert.ToInt32(splStr[a]), typeof(MM_Substation), AddIfNew) as MM_Substation;
                    return OutSub;
                }

                else
                    return Convert.ChangeType(OutgoingValue, OutgoingType);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving " + OutgoingType.Name + " " +  OutgoingValue.ToString() + ": " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Determine whether an object is numeric (e.g., int, decimal)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNumeric(Object obj)
        {
            Int32 test;
            if (obj is Int16 || obj is Int32 || obj is Int64 || obj is UInt16 || obj is UInt32 || obj is Int32 || obj is float || obj is double || obj is decimal)
                return true;
            else if (obj is string && Int32.TryParse((string)obj, out test))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Convert and store a value from XML into its appropriate format
        /// </summary>
        /// <param name="fpI">The field or property info</param>
        /// <param name="OutgoingType">The outgoing type of the variable</param>
        /// <param name="OutgoingValue">The outgoing value (in string format)</param>
        /// <param name="AddIfNew">Whether to add a new element to our master repository</param>        
        private void ConvertAndStoreValue(MemberInfo fpI, Type OutgoingType, String OutgoingValue, bool AddIfNew)
        {
            if (OutgoingType == typeof(PointF))
            {
                String[] splStr = OutgoingValue.Split(',');
                AssignValue(fpI, new PointF(MM_Converter.ToSingle(splStr[0]), MM_Converter.ToSingle(splStr[1])), this);
            }
            else if (OutgoingType == typeof(MM_Boundary))
                AssignValue(fpI, MM_Repository.Counties[OutgoingValue], this);

            else if (OutgoingType == typeof(MM_AlarmViolation))
                AssignValue(fpI, OutgoingValue, this);
            else if (OutgoingType == typeof(MM_KVLevel) && OutgoingValue is string)
                AssignValue(fpI, MM_Repository.FindKVLevel((string)OutgoingValue), this);
            else if (OutgoingType == typeof(MM_Generation_Type) && OutgoingValue is string)
                AssignValue(fpI, MM_Repository.FindGenerationType((string)OutgoingValue), this);

            else if (OutgoingType == typeof(MM_Element) || OutgoingType.BaseType == typeof(MM_Element))
                AssignValue(fpI, Data_Integration.LocateElement(Int32.Parse(OutgoingValue), OutgoingType, AddIfNew), this);
            else if (OutgoingType == typeof(MM_Contingency[]))
            {
                List<MM_Contingency> OutCtg = new List<MM_Contingency>();
                MM_Contingency FoundCtg;
                foreach (String str in OutgoingValue.Split(','))
                    if (MM_Repository.Contingencies.TryGetValue(str, out FoundCtg))
                        OutCtg.Add(FoundCtg);
                AssignValue(fpI, OutCtg.ToArray(), this);
            }
            else if (OutgoingType.IsArray && (OutgoingType.GetElementType() == typeof(MM_Element) || OutgoingType.GetElementType().BaseType == typeof(MM_Element)))
            {
                String[] splStr = ((string)OutgoingValue).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Array OutArray = Array.CreateInstance(OutgoingType.GetElementType(), splStr.Length);
                for (int a = 0; a < splStr.Length; a++)
                    OutArray.SetValue(Data_Integration.LocateElement(Convert.ToInt32(splStr[a]), OutgoingType.GetElementType(), AddIfNew), a);
                AssignValue(fpI, OutArray, this);
            }
            else if (OutgoingType == typeof(String))
                AssignValue(fpI, OutgoingValue, this);
            else if (OutgoingType == typeof(RectangleF))
            {
                string[] splStr = ((string)OutgoingValue).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                AssignValue(fpI, new RectangleF(float.Parse(splStr[0]), float.Parse(splStr[1]), float.Parse(splStr[2]), float.Parse(splStr[3])), this);
            }
            else if (OutgoingType == typeof(Rectangle))
            {
                string[] splStr = ((string)OutgoingValue).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                AssignValue(fpI, new Rectangle(int.Parse(splStr[0]), int.Parse(splStr[1]), int.Parse(splStr[2]), int.Parse(splStr[3])), this);
            }
            else if (OutgoingType == typeof(Int32))
                AssignValue(fpI, XmlConvert.ToInt32(OutgoingValue), this);
            else if (OutgoingType == typeof(Int32))
                AssignValue(fpI, XmlConvert.ToInt32(OutgoingValue), this);
            else if (OutgoingType == typeof(Single))
                AssignValue(fpI, MM_Converter.ToSingle(OutgoingValue), this);
            else if (OutgoingType == typeof(Color))
                AssignValue(fpI, System.Drawing.ColorTranslator.FromHtml(OutgoingValue), this);
            else if (OutgoingType == typeof(bool))
                AssignValue(fpI, Convert.ToBoolean(OutgoingValue), this);
            else if (OutgoingType == typeof(MM_KVLevel))
                AssignValue(fpI, MM_Repository.FindKVLevel((string)OutgoingValue), this);
            else if (OutgoingType == typeof(MM_Generation_Type))
                AssignValue(fpI, MM_Repository.FindGenerationType(OutgoingValue), this);
            else if (OutgoingType == typeof(Single[]))
            {
                String[] splStr = OutgoingValue.Split(',');
                Single[] OutPts = new Single[splStr.Length];
                for (int a = 0; a < splStr.Length; a++)
                    OutPts[a] = MM_Converter.ToSingle(splStr[a]);
                AssignValue(fpI, OutPts, this);
            }
            else if (OutgoingType == typeof(PointF[]))
            {
                String[] splStr = OutgoingValue.Split(',');
                PointF[] OutPts = new PointF[splStr.Length / 2];
                for (int a = 0; a < splStr.Length; a += 2)
                    OutPts[a / 2] = new PointF(MM_Converter.ToSingle(splStr[a]), MM_Converter.ToSingle(splStr[a + 1]));
                AssignValue(fpI, OutPts, this);
            }
            else if (OutgoingType == typeof(List<PointF>))
            {
                String[] splStr = OutgoingValue.Split(',');
                List<PointF> OutStr = new List<PointF>(splStr.Length / 2);
                for (int a = 0; a < splStr.Length; a += 2)
                    OutStr.Add(new PointF(MM_Converter.ToSingle(splStr[a]), MM_Converter.ToSingle(splStr[a + 1])));
                AssignValue(fpI, OutStr, this);
            }
            else if (OutgoingType == typeof(List<MM_WeatherStation>))
            {
                String[] splStr = OutgoingValue.Split(',');
                List<MM_WeatherStation> OutStr = new List<MM_WeatherStation>(splStr.Length);
                for (int a = 0; a < splStr.Length; a++)
                    OutStr.Add(Data_Integration.Weather.WeatherStations[splStr[a]]);
                AssignValue(fpI, OutStr, this);
            }
            else if (OutgoingType == typeof(Dictionary<MM_AlarmViolation, Int32>))
            {/*
                String[] splStr = OutgoingValue.Split(',');
                Dictionary<MM_AlarmViolation,Int32> OutDic = new Dictionary<MM_AlarmViolation, Int32>(splStr.Length);
                for (int a = 0; a < splStr.Length; a++)
                    OutDic.Add(Data_Integration.LocateViolation(Int32.Parse(splStr[a])), Int32.Parse(splStr[a]));
                AssignValue(fpI, OutDic, this);*/
            }
            else if (OutgoingType == typeof(MM_Boundary[]))
            {
                List<MM_Boundary> OutBounds = new List<MM_Boundary>();
                foreach (String str in ((string)OutgoingValue).Split(','))
                    if (!String.IsNullOrEmpty(str))
                        OutBounds.Add(MM_Repository.Counties[str]);
                AssignValue(fpI, OutBounds.ToArray(), this);
            }
            else if (OutgoingType == typeof(Dictionary<Int32, MM_Element>))
            {
                String[] splStr = OutgoingValue.Split(',');
                Dictionary<Int32, MM_Element> OutDic = new Dictionary<Int32, MM_Element>(splStr.Length);
                for (int a = 0; a < splStr.Length; a++)
                    OutDic.Add(Int32.Parse(splStr[a]), Data_Integration.LocateElement(Int32.Parse(splStr[a]), AddIfNew));
                AssignValue(fpI, OutDic, this);
            }
            else if (OutgoingType == typeof(List<MM_KVLevel>))
            {
                String[] splStr = OutgoingValue.Split(',');
                List<MM_KVLevel> OutLevels = new List<MM_KVLevel>(splStr.Length);
                for (int a = 0; a < splStr.Length; a++)
                    OutLevels.Add(MM_Repository.FindKVLevel(splStr[a]));
                AssignValue(fpI, OutLevels, this);
            }
            else if (OutgoingType == typeof(MM_KVLevel[]))
            {
                String[] splStr = OutgoingValue.Split(',');
                MM_KVLevel[] OutLevels = new MM_KVLevel[splStr.Length];
                for (int a = 0; a < splStr.Length; a++)
                    OutLevels[a] = MM_Repository.KVLevels[splStr[a]];
                AssignValue(fpI, OutLevels, this);
            }
            else if (OutgoingType == typeof(List<MM_Substation>))
            {
                String[] splStr = OutgoingValue.Split(',');
                List<MM_Substation> OutSubs = new List<MM_Substation>(splStr.Length);
                for (int a = 0; a < splStr.Length; a++)
                    OutSubs.Add(Data_Integration.LocateElement(Convert.ToInt32(splStr[a]), typeof(MM_Substation), AddIfNew) as MM_Substation);
                AssignValue(fpI, OutSubs, this);
            }
            else if (OutgoingType == typeof(MM_Substation[]))
            {
                String[] splStr = OutgoingValue.Split(',');
                MM_Substation[] OutSubs = new MM_Substation[splStr.Length];
                for (int a = 0; a < splStr.Length; a++)
                    OutSubs[a] = Data_Integration.LocateElement(Convert.ToInt32(splStr[a]), typeof(MM_Substation), AddIfNew) as MM_Substation;
                AssignValue(fpI, OutSubs, this);
            }
            else if (OutgoingType == typeof(List<MM_Element_Type>))
            {
                String[] splStr = OutgoingValue.Split(',');
                List<MM_Element_Type> OutElems = new List<MM_Element_Type>(splStr.Length);
                for (int a = 0; a < splStr.Length; a++)
                    if (!string.IsNullOrEmpty(splStr[a]))
                        OutElems.Add(MM_Repository.FindElementType(splStr[a]));
                AssignValue(fpI, OutElems, this);
            }
            else if (OutgoingType == typeof(MM_Load[]))
            {
                String[] splStr = OutgoingValue.Split(',');
                MM_Load[] OutLoads = new MM_Load[splStr.Length];
                for (int a = 0; a < splStr.Length; a++)
                    OutLoads[a] = Data_Integration.LocateElement(Convert.ToInt32(splStr[a]), typeof(MM_Load), AddIfNew) as MM_Load;
                AssignValue(fpI, OutLoads, this);
            }
            else if (OutgoingType == typeof(MM_Unit[]))
            {
                String[] splStr = OutgoingValue.Split(',');
                MM_Unit[] OutUnits = new MM_Unit[splStr.Length];
                for (int a = 0; a < splStr.Length; a++)
                    OutUnits[a] = Data_Integration.LocateElement(Convert.ToInt32(splStr[a]), typeof(MM_Unit), AddIfNew) as MM_Unit;
                AssignValue(fpI, OutUnits, this);
            }
            else if (OutgoingType == typeof(MM_LogicalUnit[]))
            {
                String[] splStr = OutgoingValue.Split(',');
                MM_Unit[] OutUnits = new MM_Unit[splStr.Length];
                for (int a = 0; a < splStr.Length; a++)
                    OutUnits[a] = Data_Integration.LocateElement(Convert.ToInt32(splStr[a]), typeof(MM_LogicalUnit), AddIfNew) as MM_Unit;
                AssignValue(fpI, OutUnits, this);
            }
            else if (OutgoingType == typeof(MM_Transformer[]))
            {
                String[] splStr = OutgoingValue.Split(',');
                MM_Transformer[] OutTransformers = new MM_Transformer[splStr.Length];
                for (int a = 0; a < splStr.Length; a++)
                    OutTransformers[a] = Data_Integration.LocateElement(Convert.ToInt32(splStr[a]), typeof(MM_Transformer), AddIfNew) as MM_Transformer;
                AssignValue(fpI, OutTransformers, this);
            }
            else if (OutgoingType == typeof(MM_Substation))
                AssignValue(fpI, Data_Integration.LocateElement(Convert.ToInt32(OutgoingValue), typeof(MM_Substation), AddIfNew), this);
            else if (OutgoingType == typeof(MM_Electrical_Bus[]))
            {
                String[] splStr = OutgoingValue.Split(',');
                MM_Electrical_Bus[] OutElectricalBus = new MM_Electrical_Bus[splStr.Length];
                for (int a = 0; a < splStr.Length; a++)
                    OutElectricalBus[a] = Data_Integration.LocateElement(Convert.ToInt32(splStr[a]), typeof(MM_Electrical_Bus), AddIfNew) as MM_Electrical_Bus;
                AssignValue(fpI, OutElectricalBus, this);
            }
            else if (OutgoingType == typeof(MM_Contingency))
                AssignValue(fpI, Data_Integration.LocateElement(Convert.ToInt32(OutgoingValue), typeof(MM_Contingency), AddIfNew), this);
            else if (OutgoingType == typeof(MM_ShuntCompensator[]))
            {
                String[] splStr = OutgoingValue.Split(',');
                MM_ShuntCompensator[] OutSC = new MM_ShuntCompensator[splStr.Length];
                for (int a = 0; a < splStr.Length; a++)
                    OutSC[a] = Data_Integration.LocateElement(Convert.ToInt32(splStr[a]), typeof(MM_ShuntCompensator), AddIfNew) as MM_ShuntCompensator;
                AssignValue(fpI, OutSC, this);
            }
            else if (OutgoingType == typeof(MM_Element_Type))
                AssignValue(fpI, MM_Repository.FindElementType(OutgoingValue), this);
            else if (OutgoingType == typeof(MM_Company))
                AssignValue(fpI, Data_Integration.LocateElement(Convert.ToInt32(OutgoingValue), typeof(MM_Company), AddIfNew), this);
            else if (OutgoingType == typeof(MM_AlarmViolation_Type))
                AssignValue(fpI, MM_Repository.ViolationTypes[OutgoingValue], this);
            else if (OutgoingType == typeof(DateTime))
                AssignValue(fpI, XmlConvert.ToDateTime(OutgoingValue, XmlDateTimeSerializationMode.Unspecified), this);
            else if (OutgoingType == typeof(MM_Communication_Linkage.ConnectionStateEnum))
                AssignValue(fpI, Enum.Parse(typeof(MM_Communication_Linkage.ConnectionStateEnum), OutgoingValue), this);
            else if (OutgoingType == typeof(System.Windows.Forms.Label))
            { }
            else if (OutgoingType == typeof(MM_Zone))
                AssignValue(fpI, MM_Repository.Zones[OutgoingValue], this);
            else if (OutgoingType == typeof(MM_Substation_Display.SubstationViewEnum))
                AssignValue(fpI, Enum.Parse(typeof(MM_Substation_Display.SubstationViewEnum), OutgoingValue), this);
            else if (OutgoingType == typeof(MM_Display.MM_Contour_Enum))
                AssignValue(fpI, Enum.Parse(typeof(MM_Display.MM_Contour_Enum), OutgoingValue), this);
            else if (OutgoingType == typeof(MM_AlarmViolation_Type.DisplayModeEnum))
                AssignValue(fpI, Enum.Parse(typeof(MM_AlarmViolation_Type.DisplayModeEnum), OutgoingValue), this);
            else if (OutgoingType == typeof(Type))
                AssignValue(fpI, MM_Type_Finder.LocateType(OutgoingValue, null), this);
            //     else if (OutgoingType == typeof(MM_Element) || OutgoingType.BaseType == typeof(MM_Element))
            //       AssignValue(fpI, Data_Integration.LocateElement(Int32.Parse(OutgoingValue), OutgoingType, AddIfNew), this);
            else if (OutgoingType == typeof(MM_TransformerWinding[]))
            {
                String[] splStr = (OutgoingValue as string).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                MM_TransformerWinding[] OutWinding = new MM_TransformerWinding[splStr.Length];
                for (int a = 0; a < splStr.Length; a++)
                {
                    OutWinding[a] = new MM_TransformerWinding();
                    OutWinding[a].TEID = Int32.Parse(splStr[a]);
                    OutWinding[a].ElemType = MM_Repository.FindElementType("TransformerWinding");
                }
                AssignValue(fpI, OutWinding, this);
            }
            else if (OutgoingType == typeof(MM_TransformerWinding))
            {
                MM_TransformerWinding OutWinding;
                Int32 TEID = Convert.ToInt32(OutgoingValue);
                if (MM_Repository.TEIDs.ContainsKey(TEID) && MM_Repository.TEIDs[TEID] is MM_TransformerWinding)
                    OutWinding = MM_Repository.TEIDs[TEID] as MM_TransformerWinding;
                else
                    OutWinding = new MM_TransformerWinding();
                OutWinding.TEID = TEID;
                OutWinding.ElemType = MM_Repository.FindElementType("TransformerWinding");
                MM_Repository.TEIDs.Add(TEID, OutWinding);
                AssignValue(fpI, OutWinding, this);
            }
            else if (OutgoingType == typeof(int[]))
            {
                List<int> OutInt = new List<int>();
                foreach (string splStr in OutgoingValue.ToString().Split(','))
                    OutInt.Add(int.Parse(splStr));
                AssignValue(fpI, OutInt.ToArray(), this);
            }
            else if (OutgoingType == typeof(double[]))
            {
                List<double> OutDouble = new List<double>();
                foreach (string splStr in OutgoingValue.ToString().Split(','))
                    OutDouble.Add(double.Parse(splStr));
                AssignValue(fpI, OutDouble.ToArray(), this);
            }
            else if (OutgoingType.IsEnum)
                AssignValue(fpI, Enum.Parse(OutgoingType, OutgoingValue, true), this);
            else if (OutgoingType == typeof(System.Windows.Forms.ListViewItem))
            { }
            else if (fpI is FieldInfo)
                Program.WriteConsoleLine("Unable to handle type " + (fpI as FieldInfo).FieldType.Name + " for " + (fpI as FieldInfo).Name);
            else if (fpI is PropertyInfo)
                Program.WriteConsoleLine("Unable to handle type " + (fpI as PropertyInfo).PropertyType.Name + " for " + (fpI as PropertyInfo).Name);
        }

        /// <summary>
        /// Write an XML snippet to the text writer
        /// </summary>
        /// <param name="Name">The name of the attribute</param>
        /// <param name="Value">The value of the attribute</param>
        /// <param name="xW">The output XML text writer</param>
        public static void WriteXMLData(String Name, Object Value, XmlTextWriter xW)
        {
            if (Value == null)
            { }
            else if (Value is PointF)
                xW.WriteAttributeString(Name, ((PointF)Value).X.ToString() + "," + ((PointF)Value).Y.ToString());
            else if (Value is RectangleF)
                xW.WriteAttributeString(Name, ((RectangleF)Value).Left.ToString() + "," + ((RectangleF)Value).Top.ToString() + "," + ((RectangleF)Value).Width.ToString() + "," + ((RectangleF)Value).Height.ToString() + ",");
            else if (Value is Rectangle)
                xW.WriteAttributeString(Name, ((Rectangle)Value).Left.ToString() + "," + ((Rectangle)Value).Top.ToString() + "," + ((Rectangle)Value).Width.ToString() + "," + ((Rectangle)Value).Height.ToString() + ",");
            else if (Value is MM_Boundary)
                xW.WriteAttributeString(Name, (Value as MM_Boundary).Name);
            else if (Value is String)
                xW.WriteAttributeString(Name, (String)Value);
            else if (Value is Int32)
                xW.WriteAttributeString(Name, Value.ToString());
            else if (Value is Single)
                xW.WriteAttributeString(Name, Value.ToString());
            else if (Value is bool)
                xW.WriteAttributeString(Name, Value.ToString());
            else if (Value is DateTime)
                xW.WriteAttributeString(Name, XmlConvert.ToString((DateTime)Value, XmlDateTimeSerializationMode.Unspecified));
            else if (Value is MM_KVLevel)
                xW.WriteAttributeString(Name, (Value as MM_KVLevel).Name);
            else if (Value is MM_AlarmViolation_Type)
                xW.WriteAttributeString(Name, (Value as MM_AlarmViolation_Type).Name);
            else if (Value is MM_Element_Type)
                xW.WriteAttributeString(Name, (Value as MM_Element_Type).Name);
            else if (Value is MM_Element)
                xW.WriteAttributeString(Name, (Value as MM_Element).TEID.ToString());
            else if (Value is MM_DisplayParameter)
            { }
            else if (Value is Data_Integration)
            { }
            else if (Value is System.Windows.Forms.ListViewItem)
                xW.WriteAttributeString(Name, (Value as System.Windows.Forms.ListViewItem).Text);
            else if (Value is MM_Generation_Type)
                xW.WriteAttributeString(Name, (Value as MM_Generation_Type).Name);
            else if (Value is System.Windows.Forms.Label)
                xW.WriteAttributeString(Name, (Value as System.Windows.Forms.Label).Text);
            else if (Value is MM_Element_Type)
            { }
            else if (Value is IEnumerable<PointF>)
            {
                StringBuilder sB = new StringBuilder();
                foreach (PointF Pt in (IEnumerable<PointF>)Value)
                    sB.Append(sB.Length != 0 ? "," + Pt.X.ToString() + "," + Pt.Y.ToString() : Pt.X.ToString() + "," + Pt.Y.ToString());
                xW.WriteAttributeString(Name, sB.ToString());
            }
            else if (Value is Macomber_Map.Data_Elements.MM_Communication_Linkage.ConnectionStateEnum)
                xW.WriteAttributeString(Name, Value.ToString());
            else if (Value is List<MM_WeatherStation>)
            {
                StringBuilder sB = new StringBuilder();
                foreach (MM_WeatherStation WX in (List<MM_WeatherStation>)Value)
                    sB.Append(sB.Length != 0 ? "," + WX.StationID : WX.StationID);
                xW.WriteAttributeString(Name, sB.ToString());
            }
            else if (Value is System.Collections.IDictionary)
            {
                StringBuilder sB = new StringBuilder();
                foreach (System.Collections.DictionaryEntry Entry in (System.Collections.IDictionary)Value)
                    sB.Append(sB.Length != 0 ? "," + Entry.Key.ToString() : Entry.Key.ToString());
                xW.WriteAttributeString(Name, sB.ToString());
            }
            else if (Value is System.Collections.IEnumerable)
            {
                StringBuilder sB = new StringBuilder();
                foreach (Object inStr in (System.Collections.IEnumerable)Value)
                    if (inStr != null)
                    {
                        String OutVal = null;
                        if (inStr is MM_Contingency)
                            OutVal = (inStr as MM_Element).Name;
                        else if (inStr is MM_Element)
                            OutVal = (inStr as MM_Element).TEID.ToString();
                        else
                            OutVal = inStr.ToString();
                        sB.Append(sB.Length != 0 ? "," + OutVal.ToString() : OutVal.ToString()); ;
                    }
                xW.WriteAttributeString(Name, sB.ToString());
            }
            else if (Value is MM_Zone)
                xW.WriteAttributeString(Name, (Value as MM_Zone).Name);
            else if (Value is Bitmap)
            { }
            else if (Value is Color)
                xW.WriteAttributeString(Name, ColorTranslator.ToHtml((Color)Value));
            else if (Value.GetType().IsEnum)
                xW.WriteAttributeString(Name, Value.ToString());
            else if (Value is TimeSpan)
                xW.WriteAttributeString(Name, XmlConvert.ToString((TimeSpan)Value));
            else
                Console.WriteLine("Unable to handle type " + Value.GetType().Name + " for parameter " + Name);                
        }
    }
}
