using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;

namespace MacomberMapClient.Integration
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides information on data retrieval
    /// </summary>
    public class  MM_DataRetrieval_Information
    {
        #region Variable declarations
        /// <summary>The type associated with our data retrieval class</summary>
        public Type BaseType;

        /// <summary>The time of our last update</summary>
        public DateTime LastUpdate;

        /// <summary>Our warning time (in seconds)</summary>
        public int WarningTime = -1;

        /// <summary>Our error time (in seconds)</summary>
        public int ErrorTime = -1;

        /// <summary>How often to refresh our item</summary>
        public int FullRefreshTime = 300;

        /// <summary>Report the full refresh command if we can</summary>
        public String RefreshCommand;

        /// <summary>The data source for our element</summary>
        public String DataSource;

        /// <summary>The time of our last full refresh. Presume this will be set to current on startup.</summary>
        public DateTime LastFullRefresh;
        #endregion

        #region Initialization
        /// <summary>
        /// Initilaize a new data retrieval information point
        /// </summary>
        /// <param name="BaseType"></param>
        public MM_DataRetrieval_Information(Type BaseType)
        {
            this.BaseType = BaseType;
            this.LastFullRefresh = DateTime.Now;

            foreach (object CustomAttribute in BaseType.GetCustomAttributes(false))
                if (CustomAttribute is UpdateParametersAttribute)
                { 
                    UpdateParametersAttribute Attr = (UpdateParametersAttribute)CustomAttribute;
                    ErrorTime = Attr.ErrorTime;
                    WarningTime = Attr.WarningTime;
                    RefreshCommand=Attr.FullRefreshCommand;
                    FullRefreshTime = Attr.FullRefreshTime;
                }

            //Since we only initialize right now on a new point, 
            LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Initialize a new data retrieval information point
        /// </summary>
        /// <param name="xElem"></param>
        public MM_DataRetrieval_Information(XmlElement xElem)
        {
            foreach (XmlAttribute xAttr in xElem.Attributes)            
            {
                System.Reflection.FieldInfo fI = this.GetType().GetField(xAttr.Name);
                if (fI.FieldType == typeof(Type))
                    fI.SetValue(this,typeof(MacomberMapCommunications.Messages.EMS.MM_Analog_Measurement).Assembly.GetType("MacomberMapCommunications.Messages.EMS." + xAttr.Value));
                else if (fI.FieldType==typeof(String))
                    fI.SetValue(this,xAttr.Value);
                else if (fI.FieldType==typeof(int))
                    fI.SetValue(this,XmlConvert.ToInt32(xAttr.Value));
            }
        }
        #endregion

        /// <summary>
        /// Report the proper color based on the time differential
        /// </summary>
        public Color ProperColor()
        {
            double Diff = (DateTime.Now - LastUpdate).TotalSeconds;
            if (ErrorTime != -1 && Diff >= ErrorTime)
                return MM_Repository.OverallDisplay.ErrorColor;
            else if (WarningTime != -1 && Diff >= WarningTime)
                return MM_Repository.OverallDisplay.WarningColor;
            else
                return MM_Repository.OverallDisplay.NormalColor;            
        }
    }
}
