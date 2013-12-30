using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using System.IO;
using Macomber_Map.Data_Elements;
using System.Windows.Forms;

namespace Macomber_Map
{
    /// <summary>
    /// (C) 2013, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the interfaces for randomizing a network model file for sharing, in order to support diagnostic and demonstration needs
    /// </summary>
    public static class ModelRandomizer
    {
        #region Variable declarations
        /// <summary>Our random buffer</summary>
        private static byte[] RndBuffer = new byte[4];

        /// <summary>Our collection of updated TEIDs and contingencies</summary>
        private static Dictionary<String, String> UpdatedTEIDsAndContingencies = new Dictionary<String, String>();

        /// <summary>The collection of original TEIDs and contingencies</summary>
        private static Dictionary<String, String> OriginalTEIDsAndContingencies = new Dictionary<String, String>();

        /// <summary>Our pseudorandom generator</summary>
        private static RNGCryptoServiceProvider csp = new System.Security.Cryptography.RNGCryptoServiceProvider();

        /// <summary>Our collection of assigned TEIDs to prevent duplication</summary>
        private static Dictionary<int, bool> AssignedTEIDs = new Dictionary<int,bool>();

        /// <summary>Our collection of dictionary words</summary>
        private static List<String> DictionaryWords = new List<string>();        
        #endregion


        /// <summary>
        /// NOTE: This subroutine takes a network model from a user and randomizes the information such 
        /// that it can be publically shared for demonstrating Macomber Map. This routine was ran against the 
        /// ERCOT MM_Model file (Macomber Map network model) in order to create random offsets of equipment, naming conventions, ownership and operatorship.
        /// </summary>
        /// <param name="XmlFileName"></param>
        /// <param name="DictionaryList"></param>
        public static void RandomizeNetworkModel(String XmlFileName, String DictionaryList)
        {

            //Load our dictionary in memory.
            //For the open source version demo, I used http://wordlist.sourceforge.net/ english-proper-names.95 file.
            String InLine;
           using (StreamReader sRd = new StreamReader(DictionaryList))
            while ((InLine = sRd.ReadLine()) != null)
                if (!InLine.Trim().EndsWith("'s"))
                    DictionaryWords.Add(InLine.Trim());


            //Load our XML file into memory
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(XmlFileName);

           
            //Go through all TEID attributes and create our updated list
            String BasePath = Path.Combine(Path.GetDirectoryName(XmlFileName), Path.GetFileNameWithoutExtension(XmlFileName));
            UpdatedTEIDsAndContingencies.Add("0", "0");
            OriginalTEIDsAndContingencies.Add("0", "0");
            foreach (XmlAttribute xAttr in xDoc.SelectNodes("//@TEID"))
                    if (xAttr.Value != "0" && !UpdatedTEIDsAndContingencies.ContainsKey(xAttr.Value))
                    {
                        String OldTEID = xAttr.Value;
                        String NewTEID = GenerateTEID();
                        UpdatedTEIDsAndContingencies.Add(OldTEID, NewTEID);
                        OriginalTEIDsAndContingencies.Add(NewTEID,OldTEID);
                    }

                foreach (XmlElement xElem in xDoc.SelectNodes("//Contingency"))
                {
                    String OldName = xElem.Attributes["Name"].Value;
                    RandomizeString(xElem.Attributes["Name"], 1, true);
                    UpdatedTEIDsAndContingencies.Add(OldName, xElem.Attributes["Name"].Value);
                    OriginalTEIDsAndContingencies.Add(xElem.Attributes["Name"].Value, OldName);
                }
            


            //Go through every element and update attributes as appropriate
            String[] TEIDsToUpdate = "PhysicalUnit,LogicalUnits,Elements,RASs,Substation,Substation1,Substation2,Substations,ConnectedStations,BusbarSection,Contingencies,Node,TEID,Loads,BusbarSections,Transformers,Units,Onwer,Owners,Operator,Operators,StaticVarCompensators,ShuntCompensators,ViolatedElement,Winding1,Winding2,Windings".Split(',');
            String[] TitlesToUpdate = "Name,LongName,Description".Split(',');
            List<String> SeenElements = new List<string>();


            //Go through each of our XML elements and update accordingly
            int Percentage= xDoc.DocumentElement.ChildNodes.Count/100;
            for (int CurNode=0; CurNode < xDoc.DocumentElement.ChildNodes.Count; CurNode++)
            {
                XmlNode xNode = xDoc.DocumentElement.ChildNodes[CurNode];
                if (CurNode % Percentage == 0)
                {
                    Console.WriteLine("Parsing nodes: " + (CurNode / Percentage).ToString() + "%");
                    Application.DoEvents();
                }
                if (!SeenElements.Contains(xNode.Name))
                {
                    Console.WriteLine("Encountered new node type: " + xNode.Name);
                    SeenElements.Add(xNode.Name);
                }

                UpdateNode(xNode, TEIDsToUpdate, TitlesToUpdate);
                foreach (XmlNode xChild in xNode.ChildNodes)
                    UpdateNode(xChild, TEIDsToUpdate, TitlesToUpdate);                              
            }

            //Now, save our updated XML file and translation dictionary
            using (StreamWriter sW = new StreamWriter(BasePath + "-TEIDAndContingencyDeltas.csv", false, new UTF8Encoding(false)))
            {
                sW.WriteLine("\"Original Value\",\"New Value\"");
                foreach (KeyValuePair<String, String> kvp in UpdatedTEIDsAndContingencies)
                    sW.WriteLine("\"{0}\",\"{1}\"", kvp.Key, kvp.Value);
            }

            using (XmlTextWriter xW = new XmlTextWriter(BasePath + "-Updated.MM_Model",new UTF8Encoding(false)))
            {
                xW.Formatting = Formatting.Indented;
                xDoc.WriteTo(xW);
            }
        }

        /// <summary>
        /// Update a node
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="TEIDsToUpdate"></param>
        /// <param name="TitlesToUpdate"></param>
        private static void UpdateNode(XmlNode xNode, String[] TEIDsToUpdate, String[] TitlesToUpdate)
        {
            foreach (String str in TEIDsToUpdate)
                UpdateTEIDs(xNode.Attributes[str]);

            foreach (String str in TitlesToUpdate)
                if (str != "Name" || (xNode.Name != "Contingency" && xNode.Name != "County" && xNode.Name != "State"))
                    RandomizeString(xNode.Attributes[str]);

            if (xNode.Name == "Company")
            {
                RandomizeNumber(xNode.Attributes["PrimaryPhone"]);
                RandomizeNumber(xNode.Attributes["DUNS"]);
                RandomizeString(xNode.Attributes["Alias"], 1, true);
            }
            else if (xNode.Name == "Substation")
            {
                RandomizePosition(xNode.Attributes["Longitude"]);
                RandomizePosition(xNode.Attributes["Latitude"]);
                xNode.Attributes["LatLong"].Value = xNode.Attributes["Longitude"].Value + "," + xNode.Attributes["Latitude"].Value;
            }
            else if (xNode.Name == "Line" || xNode.Name == "DCTie")
            {
                xNode.Attributes["LineTEID"].Value = GenerateTEID();
                XmlElement xSub1 = xNode.OwnerDocument.SelectSingleNode("//Substation[@TEID='" + xNode.Attributes["Substation1"].Value + "']") as XmlElement;
                XmlElement xSub2 = xNode.OwnerDocument.SelectSingleNode("//Substation[@TEID='" + xNode.Attributes["Substation1"].Value + "']") as XmlElement;
                xNode.Attributes["Coordinates"].Value = xSub1.Attributes["LatLong"].Value + "," + xSub2.Attributes["LatLong"].Value;
            }
        }

        /// <summary>
        /// Generate a new TEID
        /// </summary>
        /// <returns></returns>
        private static string GenerateTEID()
        {
            int NewValue;
            while (AssignedTEIDs.ContainsKey(NewValue = RandomInteger(100, 999999)))
            { }
            AssignedTEIDs.Add(NewValue, true);
            return NewValue.ToString();
        }

        /// <summary>
        /// Randomize a position
        /// </summary>
        /// <param name="xAttr"></param>
        private static void RandomizePosition(XmlAttribute xAttr)
        {
            if (xAttr == null)
                return;
            int Offset = RandomInteger(-500, 500);
            Double CurVal = XmlConvert.ToDouble(xAttr.Value);
            CurVal += (Double)Offset / 10000.0;
            xAttr.Value = CurVal.ToString();
        }

        /// <summary>
        /// Update our list of TEIDs
        /// </summary>
        /// <param name="xAttr"></param>
        private static void UpdateTEIDs(XmlAttribute xAttr)
        {
            if (xAttr == null)
                return;
            String[] InVal = xAttr.Value.Split(',');
            string FoundVal;
            for (int a = 0; a < InVal.Length; a++)
                if (UpdatedTEIDsAndContingencies.TryGetValue(InVal[a], out FoundVal))
                    InVal[a] = FoundVal;
                else
                {
                    Console.WriteLine(xAttr.OwnerElement.Name + ": Can't find " + InVal[a] + " for " + xAttr.Name);
                    Application.DoEvents();
                    FoundVal = RandomizeSubString(InVal[a], 1);
                    UpdatedTEIDsAndContingencies.Add(InVal[a], FoundVal);
                    OriginalTEIDsAndContingencies.Add(FoundVal, InVal[a]);
                    InVal[a] = FoundVal;
                }   
            xAttr.Value = String.Join(",", InVal);
        }

        /// <summary>
        /// Randomize a number
        /// </summary>
        /// <param name="xAttr"></param>
        private static void RandomizeNumber(XmlAttribute xAttr)
        {
            if (xAttr == null)
                return;
            char[] CurVal = xAttr.Value.ToCharArray();
            for (int a = 0; a < CurVal.Length; a++)
                if (Char.IsNumber(CurVal[a]))
                    CurVal[a] = RandomInteger(0, 10).ToString()[0];
            xAttr.Value = new string(CurVal);
        }

        /// <summary>
        /// Randomize a substring
        /// </summary>
        /// <param name="InString"></param>
        /// <param name="StartPos"></param>
        private static String RandomizeSubString(String InString, int StartPos)
        {
            char[] CurVal = InString.ToCharArray();
            do
            {
                for (int a = StartPos; a < CurVal.Length; a++)
                    if (Char.IsUpper(CurVal[a]))
                        CurVal[a] = (char)((int)'A' + RandomInteger(0, 26));
                    else if (Char.IsLower(CurVal[a]))
                        CurVal[a] = (char)((int)'a' + RandomInteger(0, 26));
            } while (OriginalTEIDsAndContingencies.ContainsKey(new string(CurVal)));
            return new string(CurVal);
        }
        /// <summary>
        /// Generate a random string based on our text
        /// </summary>
        /// <param name="xAttr"></param>
        /// <param name="StartPos"></param>
        /// <param name="PreserveLength"></param>
        private static void RandomizeString(XmlAttribute xAttr, int StartPos = 0, bool PreserveLength = false)
        {
            if (xAttr == null)
                return;
            else if (PreserveLength)
                xAttr.Value = RandomizeSubString(xAttr.Value, StartPos);
            else if (xAttr.Name != "Name")
                xAttr.Value= MM_Repository.TitleCase(DictionaryWords[RandomInteger(0, DictionaryWords.Count)]);
            else
            {
                String CurVal = xAttr.Value;
                do
                {
                    int CurWord = RandomInteger(0, DictionaryWords.Count);
                    CurVal = MM_Repository.TitleCase(DictionaryWords[CurWord]);
                    DictionaryWords.RemoveAt(CurWord);
                } while (OriginalTEIDsAndContingencies.ContainsKey(CurVal));
                OriginalTEIDsAndContingencies.Add(CurVal, xAttr.Value);
                xAttr.Value = CurVal;
            }
        }

        /// <summary>
        /// Use our random number generator to create an integer between an upper bound (exclusive) and lower bound (inclusive)
        /// Thanks to http://msdn.microsoft.com/en-us/magazine/cc163367.aspx
        /// </summary>
        /// <param name="Minimum"></param>
        /// <param name="Maximum"></param>
        /// <returns></returns>
        private static int RandomInteger(int Minimum, int Maximum)
        {

            if (Minimum > Maximum)
                throw new ArgumentOutOfRangeException("Minimum cannot be greater than maximum");
            else if (Minimum == Maximum)
                return Minimum;
            Int64 diff = Maximum - Minimum;
            while (true)
            {
                csp.GetBytes(RndBuffer);
                UInt32 rand = BitConverter.ToUInt32(RndBuffer, 0);
                Int64 max = (1 + (Int64)UInt32.MaxValue);
                Int64 remainder = max % diff;
                if (rand < max - remainder)
                    return (Int32)(Minimum + (rand % diff));
            }
        }

        /// <summary>
        /// Use our random number generator to create a double-precision number
        /// Thanks to http://msdn.microsoft.com/en-us/magazine/cc163367.aspx
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="Minimum"></param>
        /// <param name="Maximum"></param>
        /// <returns></returns>
        private static double RandomDouble()
        {
            byte[] RndBuffer = new byte[4];
            csp.GetBytes(RndBuffer);
            UInt32 rand = BitConverter.ToUInt32(RndBuffer, 0);
            return rand / (1.0 + UInt32.MaxValue);
        }

         
        
    }
}
