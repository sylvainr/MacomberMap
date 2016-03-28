using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages
{

    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a training level
    /// </summary>
    [RetrievalCommand("LoadTrainingLevels")]
    public class MM_Training_Level
    {
        /// <summary>The title of the level</summary>
        public String Title { get; set; }

        /// <summary>The upper score value (at precisely the bulls-eye threshold distance or less</summary>
        public double UpperScoreValue { get; set; }

        /// <summary>The lower score value (at precisely the no point threshold</summary>
        public double LowerScoreValue { get; set; }

        /// <summary>The threshold for distance between bulls eye</summary>
        public double BullsEyeThresholdDistance { get; set; }

        /// <summary>The threshold distance for no score</summary>
        public double NoScoreThreshold { get; set; }

        /// <summary>The maximum time score bonus</summary>
        public double MaximumTimeScore { get; set; }

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
        public double[] TopLeftLngLat { get; set; }

        /// <summary>The bottom right lat/long</summary>
        public double[] BottomRightLngLat { get; set; }

        /// <summary>The instructions that will be presented to the user (in HTML)</summary>
        public string InstructionText { get; set; }

        /// <summary>The ID of our level</summary>
        public int ID { get; set; }

        /// <summary>The county to restrict to</summary>
        public String County { get; set; }

        /// <summary>The KV level to restrict to</summary>
        public String KVLevel { get; set; }

        /// <summary>The element type to restrict to</summary>
        public String ElemType { get; set; }

        /// <summary>The format of the question being asked (to use string.format)</summary>
        public String QuestionFormat { get; set; }

        /// <summary>The boolean that should checked, and included if true</summary>
        public String BooleanToCheck { get; set; }

        /// <summary>The collection of substations that can be selected from</summary>
        public int[] Substations;
    }
}
