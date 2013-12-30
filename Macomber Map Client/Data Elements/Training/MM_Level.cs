using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Xml;
using System.Drawing;

namespace Macomber_Map.Data_Elements.Training
{
    /// <summary>
    /// This class holds information on a level
    /// </summary>
    public class MM_Level
    {
        #region Variable declarations
        /// <summary>The title of the level</summary>
        public String Title { get; set; }

        /// <summary>The collection of radiuses and their associated points</summary>
        public Double[] PointsByDistance { get; set; }

        /// <summary>The point threshold to exit the level succesfully</summary>
        public int ExitThresholdScore { get; set; }

        /// <summary>The point threshold after which the # of questions has been exceeded and failed</summary>
        public int FailureThreshold { get; set; }

        /// <summary>The number of questions the user is presented with before the failure threshold is checked</summary>
        public int NumberOfQuestions { get; set; }

        /// <summary>The number of seconds after which the question is considered timed out</summary>
        public int QuestionTimeout { get; set; }

        /// <summary>The number of seconds after which the question is considered timed out</summary>
        public int WaitBetweenQuestions { get; set; }

        /// <summary>Whether the user is allowed to zoom at this level</summary>
        public bool AllowZoom { get; set; }

        /// <summary>The top left lat/long</summary>
        public PointF TopLeftLatLong { get; set; }

        /// <summary>The bottom right lat/long</summary>
        public PointF BottomRightLatLong { get; set; }

        /// <summary>The instructions that will be presented to the user (in HTML)</summary>
        public string InstructionText { get; set; }

        /// <summary>The ID of our level</summary>
        public int ID;

        /// <summary>The county to restrict to</summary>
        public MM_Boundary County { get; set; }

        /// <summary>The KV level to restrict to</summary>
        public MM_KVLevel KVLevel { get; set; }

        /// <summary>The element type to restrict to</summary>
        public MM_Element_Type ElemType { get; set; }

        /// <summary>The format of the question being asked (to use string.format)</summary>
        public String QuestionFormat { get; set; }

        /// <summary>The boolean that should checked, and included if true</summary>
        public String BooleanToCheck { get; set; }

        /// <summary>The XML configuration of our element</summary>
        public XmlElement xConfig;

        /// <summary>The collection of substations that can be selected from</summary>
        public MM_Substation[] Substations;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new level
        /// </summary>
        /// <param name="dRd"></param>
        public MM_Level(DbDataReader dRd)
        {
        }
        
        /// <summary>
        /// Initialize a new level
        /// </summary>
        /// <param name="xElem">The XML configuration for our level</param>
        public MM_Level(XmlElement xElem)
        {
            this.xConfig = xElem;
            if (xElem.HasAttribute("ID"))
                ID = XmlConvert.ToInt32(xElem.Attributes["ID"].Value);
        }

        /// <summary>
        /// Deserialize our component off XML.
        /// </summary>
        public void DeSerialize()
        {
            MM_Serializable.ReadXml(xConfig, this, false);
        }

        /// <summary>
        /// Initialize a new empty level 
        /// </summary>
        public MM_Level()
        {
        }

        #endregion
    }
}