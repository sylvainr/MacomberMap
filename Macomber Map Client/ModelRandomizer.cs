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
        private static Dictionary<int, bool> AssignedTEIDs = new Dictionary<int, bool>();

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
            //Insert your own list of forbidden words below to strip out any words that should be excluded from the random name selection process
            DictionaryWords.Clear();
            String InLine;
            String[] ForbiddenWords = "".ToLower().Split(',');
            using (StreamReader sRd = new StreamReader(DictionaryList))
                while ((InLine = sRd.ReadLine()) != null)
                {
                    bool WordOk = !InLine.Trim().EndsWith("'s");
                    foreach (String ForbiddenWord in ForbiddenWords)
                        if (InLine.ToLower().Contains(ForbiddenWord))
                            WordOk = false;
                    if (WordOk)
                        DictionaryWords.Add(InLine.Trim());
                }

            //Load our XML file into memory
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(XmlFileName);


            //Go through all TEID attributes and create our updated list
            String BasePath = Path.Combine(Path.GetDirectoryName(XmlFileName), Path.GetFileNameWithoutExtension(XmlFileName));
            UpdatedTEIDsAndContingencies.Clear();
            OriginalTEIDsAndContingencies.Clear();
            AssignedTEIDs.Clear();
            UpdatedTEIDsAndContingencies.Add("0", "0");
            String FoundValue;
            XmlNodeList NodesToParse;

            String[] TEIDsToUpdate = "TEID,LineTEID,PhysicalUnit,LogicalUnits,Unit,Load,Elements,RASs,Substation,Substation1,Substation2,Substations,ConnectedStations,BusbarSection,Node,Loads,BusbarSections,Transformers,Transformer,Units,Owner,Operator,StaticVarCompensators,ShuntCompensators,ViolatedElement,Winding1,Winding2,Windings,AssociatedLine".Split(',');
            Dictionary<String, int> UpdateCount = new Dictionary<string, int>();
            foreach (String AttributeToUpdate in TEIDsToUpdate)
            {
                NodesToParse = xDoc.SelectNodes("//@" + AttributeToUpdate);
                Console.WriteLine("Updating TEIDs in " + AttributeToUpdate+": " + NodesToParse.Count.ToString("#,##0"));
                UpdateCount.Add(AttributeToUpdate, NodesToParse.Count);
                foreach (XmlAttribute xAttr in NodesToParse)
                {
                    String[] TEIDs = xAttr.Value.Split(',');
                    for (int a = 0; a < TEIDs.Length; a++)
                    {
                        if (!UpdatedTEIDsAndContingencies.TryGetValue(TEIDs[a], out FoundValue))
                        {
                            UpdatedTEIDsAndContingencies.Add(TEIDs[a], FoundValue = GenerateTEID());
                            OriginalTEIDsAndContingencies.Add(FoundValue, TEIDs[a]);
                        }
                        TEIDs[a] = FoundValue;
                    }
                    xAttr.Value = String.Join(",", TEIDs);
                }
            }

            
            //Now, go through our names and update accordingly.
            String[] TitlesToUpdate = "Alias,Name,Contingencies,LongName,Description,Target".Split(',');
            foreach (String AttributeToUpdate in TitlesToUpdate)
            {
                NodesToParse = xDoc.SelectNodes("//@" + AttributeToUpdate);
                UpdateCount.Add(AttributeToUpdate, NodesToParse.Count);
                Console.WriteLine("Updating Names in " + AttributeToUpdate + ": " + NodesToParse.Count.ToString("#,##0"));
                foreach (XmlAttribute xAttr in NodesToParse)
                    if (!xAttr.OwnerElement.Name.StartsWith("MM_"))
                    {
                        String[] Names = xAttr.Value.Split(',');
                        for (int a = 0; a < Names.Length; a++)
                        {
                            if (xAttr.OwnerElement.Name == "County" || xAttr.OwnerElement.Name == "State")
                            { }
                            else if ((xAttr.OwnerElement.Name == "Contingency" && xAttr.Name == "Name") || (xAttr.OwnerElement.Name == "Company" && xAttr.Name == "Alias"))
                            {
                                if (!UpdatedTEIDsAndContingencies.TryGetValue(Names[a], out FoundValue))
                                    UpdatedTEIDsAndContingencies.Add(Names[a], FoundValue = RandomizeSubString(Names[a], 1));
                                Names[a] = FoundValue;

                            }
                            else
                            {
                                if (!UpdatedTEIDsAndContingencies.TryGetValue(Names[a], out FoundValue))
                                    UpdatedTEIDsAndContingencies.Add(Names[a], FoundValue = GenerateRandomWord(Names[a]));
                                Names[a] = FoundValue;
                            }
                        }
                        xAttr.Value = String.Join(",", Names);
                    }
            }


            //Update our companies
            NodesToParse = xDoc.SelectNodes("/NetworkModel/Company");
            UpdateCount.Add("<Company>", NodesToParse.Count);

            Console.WriteLine("Updating company phone number and DUNs: " + NodesToParse.Count.ToString("#,##0"));
            foreach (XmlElement xNode in NodesToParse)
            {
                RandomizeNumber(xNode.Attributes["PrimaryPhone"]);
                RandomizeNumber(xNode.Attributes["DUNS"]);
            }


            //Update our substation coordinates
            Dictionary<String, String> SubstationCoordinates = new Dictionary<string, string>();
            NodesToParse = xDoc.SelectNodes("/NetworkModel/Substation");
            UpdateCount.Add("<Substation>", NodesToParse.Count);
            Console.WriteLine("Updating substation coordinates: " + NodesToParse.Count.ToString("#,##0"));
            foreach (XmlElement xNode in NodesToParse)
            {
                RandomizePosition(xNode.Attributes["Longitude"]);
                RandomizePosition(xNode.Attributes["Latitude"]);
                xNode.Attributes["LatLong"].Value = xNode.Attributes["Longitude"].Value + "," + xNode.Attributes["Latitude"].Value;
                SubstationCoordinates.Add(xNode.Attributes["TEID"].Value, xNode.Attributes["LatLong"].Value);
            }


            //Update our lines and DC ties
            foreach (String LineAttribute in "Line,DCTie".Split(','))
            {
                NodesToParse = xDoc.SelectNodes("/NetworkModel/" + LineAttribute);
                UpdateCount.Add("<" + LineAttribute + ">", NodesToParse.Count);
                Console.WriteLine("Updating substation coordinates for " + LineAttribute + ": " + NodesToParse.Count.ToString("#,##0"));
                foreach (XmlNode xNode in NodesToParse)
                {
                    String LatLong1 = SubstationCoordinates[xNode.Attributes["Substation1"].Value];
                    String LatLong2 = SubstationCoordinates[xNode.Attributes["Substation2"].Value];
                    xNode.Attributes["Coordinates"].Value = LatLong1 + "," + LatLong2;
                }
            }


            //Now, save our updated XML file and translation dictionary
            using (StreamWriter sW = new StreamWriter(BasePath + "-TEIDAndContingencyDeltas.csv", false, new UTF8Encoding(false)))
            {
                sW.WriteLine("\"Original Value\",\"New Value\"");
                foreach (KeyValuePair<String, String> kvp in UpdatedTEIDsAndContingencies)
                    sW.WriteLine("\"{0}\",\"{1}\"", kvp.Key, kvp.Value);
            }

            using (XmlTextWriter xW = new XmlTextWriter(BasePath + "-Updated.MM_Model", new UTF8Encoding(false)))
            {
                xW.Formatting = Formatting.Indented;
                xDoc.WriteTo(xW);
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
            OriginalTEIDsAndContingencies.Add(new string(CurVal), InString);
            return new string(CurVal);
        }

        /// <summary>
        /// Generate a random word from our dictionary
        /// </summary>
        /// <param name="InString"></param>
        /// <returns></returns>
        private static String GenerateRandomWord(String InString)
        {
            String CurVal = InString;
            do
            {
                int CurWord = RandomInteger(0, DictionaryWords.Count);
                CurVal = MM_Repository.TitleCase(DictionaryWords[CurWord]);
                DictionaryWords.RemoveAt(CurWord);
            } while (OriginalTEIDsAndContingencies.ContainsKey(CurVal));
            OriginalTEIDsAndContingencies.Add(CurVal, InString);
            return CurVal;
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