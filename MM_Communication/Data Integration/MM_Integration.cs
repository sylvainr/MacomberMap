using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MM_Communication.Elements;
using System.Windows.Forms;

namespace MM_Communication.Data_Integration
{
    /// <summary>
    /// (C) 2012, Electric Reliability Council of Texas, Inc.
    /// This class provides the data integration layer for our components
    /// </summary>
    public static class MM_Integration
    {
        #region Enumerations
        /// <summary>Our collection of data types</summary>
        public enum DataTypes
        {
            /// <summary>The data is necessary to identify and locate our element (TEID)</summary>
            PrimaryLocator,
            /// <summary>The data is necessary to identify and locate our element (e.g., name, substation, etc.)
            SecondaryLocator,
            /// <summary>The information is static, and needs to be only loaded once</summary>
            Static,
            /// <summary>The information holds information on the current state of our object</summary>
            State,
            /// <summary>A data point that is only calculated</summary>
            Calculated
        }

        /// <summary>
        /// Our collection of possible query statuses
        /// </summary>
        public enum enumQueryStatus:byte
        {
            /// <summary>The query has never been executed</summary>
            NeverExecuted,
            /// <summary>The query is ready to be executed</summary>
            Ready,
            /// <summary>The query has been executed</summary>
            Executing,
            /// <summary>The data is being retrieved from the server</summary>
            Retrieving,
            /// <summary>The data from the query is being applied</summary>
            Processing,
            /// <summary>This query had an error, and should be skipped.</summary>
            Error,
            /// <summary>The query is paused, awaiting re-execution</summary>
            Paused
        };
        #endregion
        
        /// <summary>Our master list of locators</summary>
        public static List<MM_OneLine_TableLocator> MasterLocators = new List<MM_OneLine_TableLocator>();

        /// <summary>Our lookup of table locators</summary>
        public static Dictionary<String, List<MM_OneLine_TableLocator>> TableLocatorsByName = new Dictionary<string, List<MM_OneLine_TableLocator>>();

        /// <summary>Our collection of one-line table/record locators</summary>
         public static Dictionary<MM_Element.enumElementType, MM_OneLine_TableLocator[]> OneLineRecordLocators = new Dictionary<MM_Element.enumElementType, MM_OneLine_TableLocator[]>();

        #region Initialization
        /// <summary>
        /// Load in our collection of one-line record locators
        /// </summary>
         /// <param name="xDoc"></param>
         public static void InitializeOneLineRecordLocators(XmlElement xDoc)
         {
             foreach (XmlNode xElem in xDoc.ChildNodes)
                 try
                 {
                     if (xElem is XmlElement && xElem.Name == "ElementType")
                     {
                         List<MM_OneLine_TableLocator> TableLocators = new List<MM_OneLine_TableLocator>();
                         foreach (XmlElement xTableLocator in xElem.ChildNodes)
                             if (xTableLocator.Name == "DataLocator")
                             {
                                 MM_OneLine_TableLocator tLoc = new MM_OneLine_TableLocator(xTableLocator, MasterLocators.Count);
                                 MasterLocators.Add(tLoc);
                                 TableLocators.Add(tLoc);
                                 
                                 if (!String.IsNullOrEmpty(tLoc.Application))
                                    TryAdd(tLoc.Application + "_" + tLoc.Database + "_" + tLoc.TableName, tLoc);
                                 else if (tLoc.Database == "SCADAMOM")
                                     TryAdd("SCADA_" + tLoc.Database + "_" + tLoc.TableName, tLoc);
                                 else
                                 {
                                     TryAdd("StudySystem_" + tLoc.Database + "_" + tLoc.TableName, tLoc);
                                     TryAdd("StateEstimator_" + tLoc.Database + "_" + tLoc.TableName, tLoc);
                                     TryAdd("DTSPSM_" + tLoc.Database + "_" + tLoc.TableName, tLoc);
                                 }

                             }
                         OneLineRecordLocators.Add((MM_Element.enumElementType)Enum.Parse(typeof(MM_Element.enumElementType), xElem.Attributes["Name"].Value), TableLocators.ToArray());
                     }
                 }
                 catch (Exception ex)
                 {
                     Console.Error.WriteLine(ex.ToString());                     
                 }
         }

        /// <summary>
        /// Try and add a table locator to our element
        /// </summary>
        /// <param name="TitleName"></param>
        /// <param name="tLoc"></param>
         private static void TryAdd(String TitleName, MM_OneLine_TableLocator tLoc)
         {
             List<MM_OneLine_TableLocator> TLocs;
             if (!TableLocatorsByName.TryGetValue(TitleName, out TLocs))
                 TableLocatorsByName.Add(TitleName, TLocs = new List<MM_OneLine_TableLocator>());
             TLocs.Add(tLoc);
         }
        #endregion

        

       
    }

}