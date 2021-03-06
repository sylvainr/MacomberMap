﻿using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Display
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds all of the parameters for a display view
    /// </summary>
    public class MM_Display_View
    {
        #region Variable declarations
        /// <summary>The name of the view</summary>
        public String Name;

        /// <summary>The base element for the view</summary>
        public XmlElement BaseElement;

        /// <summary>The collection of parameters for the display view</summary>
        public Dictionary<String, String> DisplayParameters = new Dictionary<string, string>();

        /// <summary>The full path of the view</summary>
        public String FullName;

        /// <summary>
        /// Return the depth of the current Xml element (so that tabs can properly be set).
        /// </summary>
        public int XmlDepth
        {
            get
            {
                int CurDepth = 0;
                XmlNode CurElem = BaseElement;
                while ((CurElem = CurElem.ParentNode) != null)
                    CurDepth++;
                return CurDepth;
            }
        }

        #endregion

        #region Initialization
        /// <summary>
        /// Create a display view for the default set of parameters
        /// </summary>
        /// <param name="Name">The name of the view</param>        
        public MM_Display_View(String Name)
        {
            //Set our existing parameters           
            this.Name = this.FullName = Name;
            BaseElement = MM_Repository.xConfiguration["Configuration"]["DisplayParameters"];
            this.Activate();
        }


        /// <summary>
        /// Create a new display view based on the XML parameters
        /// </summary>
        /// <param name="ViewElement">The XML configuration for this view</param> 
        public MM_Display_View(XmlElement ViewElement)
        {
            BaseElement = ViewElement;
            this.Name = ViewElement.Attributes["Name"].Value;

            StringBuilder OutFullName = new StringBuilder(this.Name);
            XmlElement Elem = ViewElement;
            while (((Elem = Elem.ParentNode as XmlElement) != null) && (Elem.HasAttribute("Name")))
                OutFullName.Insert(0, Elem.Attributes["Name"].Value + "\\");
            FullName = OutFullName.ToString();

            foreach (String ModifyCommand in ViewElement.InnerText.Trim('\r', '\n', '\t', ' ').Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                String[] splStr = ModifyCommand.Split('=');
                DisplayParameters.Add(splStr[0].Trim(), splStr[1].Trim());
            }
        }


        /// <summary>
        /// Activate this display view
        /// </summary>
        public void Activate()
        {
            //Create a list of our parameters
            List<Object> DisplayObjects = new List<object>();
            DisplayObjects.Add(MM_Repository.OverallDisplay);
            DisplayObjects.Add(MM_Repository.SubstationDisplay);
            foreach (MM_AlarmViolation_Type ViolType in MM_Repository.ViolationTypes.Values)
                DisplayObjects.Add(ViolType);
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                DisplayObjects.Add(KVLevel);

            //Now, retrieve our base view if we're the default
            if (this.BaseElement.Name == "DisplayParameters")
                MM_Repository.ReinitializeDisplayParameters();

            //Now, go through each command, and update.
            foreach (KeyValuePair<String, String> kvp in DisplayParameters)
            {
                String[] splStr = kvp.Key.Split('.');
                Object FoundObject = null;

                //Now, check the first element to find what we're looking for                                        
                foreach (Object obj in DisplayObjects)
                    if ((obj.GetType().GetProperty("Name") != null) && (obj.GetType().GetProperty("Name").GetValue(obj, null).ToString() == splStr[0]))
                        FoundObject = obj;
                    else if ((obj.GetType().GetField("Name") != null) && (obj.GetType().GetField("Name").GetValue(obj).ToString() == splStr[0]))
                        FoundObject = obj;

                //Now keep going through all sub-elements, and handle accordingly.
                for (int a = 1; a < splStr.Length - 1; a++)
                {
                    //First check through our properties
                    if (FoundObject.GetType().GetProperty(splStr[a]) != null)
                        FoundObject = FoundObject.GetType().GetProperty(splStr[a]).GetValue(FoundObject, null);

                    //Now check through our fields
                    if (FoundObject.GetType().GetField(splStr[a]) != null)
                        FoundObject = FoundObject.GetType().GetField(splStr[a]).GetValue(FoundObject);

                }

                //Now go the final step, and set our value.
                if (FoundObject != null)
                    MM_Repository.SetProperty(FoundObject, splStr[splStr.Length - 1], kvp.Value);
            }

            //Now tell the repository we're active
            MM_Repository.SetActiveView(this);
        }
        #endregion

        /// <summary>
        /// Extract all settings to config string
        /// </summary>
        /// <returns></returns>
        public string GetSettings()
        {
            StringBuilder sb = new StringBuilder(10);
            List<object> displayObjects = new List<object>();
            displayObjects.Add(MM_Repository.OverallDisplay);
            displayObjects.Add(MM_Repository.SubstationDisplay);
            foreach (MM_AlarmViolation_Type ViolType in MM_Repository.ViolationTypes.Values)
                displayObjects.Add(ViolType);
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                displayObjects.Add(KVLevel);

            foreach (var displayObject in displayObjects)
            {
                sb.Append("=====");

                
                Object InVal = null;
                PropertyInfo pI = displayObject.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
                if (pI != null)
                    InVal = pI.GetValue(displayObject,null);
                if (InVal==null)
                {
                    FieldInfo fI =displayObject.GetType().GetField("Name");
                    if (fI != null)
                        InVal=fI.GetValue(displayObject);
                }
                    
                string name = InVal==null?null:InVal.ToString();

                sb.Append(name);
                sb.AppendLine("=====");

                var publicProperties = displayObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (var property in publicProperties)
                {
                    if (property.Name == "Name") continue;

                    var value = property.GetValue(displayObject, null);
                    sb.AppendLine(new string('\t', XmlDepth) + property.Name + "=" + (value ?? string.Empty).ToString());
                }
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Save the configuration for this display view
        /// </summary>
        public void Save()
        {
            StringBuilder OutStr = new StringBuilder("\n");
            foreach (KeyValuePair<string, string> kvp in DisplayParameters)
                OutStr.AppendLine(new string('\t', XmlDepth) + kvp.Key + "=" + kvp.Value);
            BaseElement.InnerText = OutStr.ToString();
        }
    }
}
