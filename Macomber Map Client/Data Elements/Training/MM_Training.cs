using System;
using System.Collections.Generic;using System.Linq;
using System.Text;
using System.Xml;
using System.Data.Common;
using System.Drawing;
using Macomber_Map.Data_Connections;
using Macomber_Map.User_Interface_Components.NetworkMap;

namespace Macomber_Map.Data_Elements.Training
{
    /// <summary>
    /// (C) 2013, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved
    /// This class provides the central training interface
    /// </summary>
    public class MM_Training
    {
        #region Variable declarations
        /// <summary>Our random number generator</summary>
        public Random rnd = new Random();

        /// <summary>The collection of training levels</summary>
        public SortedDictionary<int, MM_Level> Levels = new SortedDictionary<int, MM_Level>();

        /// <summary>The current level</summary>
        public MM_Level CurrentLevel = null;

        /// <summary>The prior level for training</summary>
        public MM_Level PriorLevel = null;

        /// <summary>The target element the user should be shown</summary>
        public MM_Element TargetElement = null;

        /// <summary>The instructions to be presented to the user</summary>
        public String QuestionText;

        /// <summary>The response to the user's click</summary>
        public String AnswerText;

        /// <summary>The user's current score</summary>
        public Double Score;

        /// <summary>The user's current score</summary>
        public Double LevelScore;

        /// <summary>When the user stared playing the game</summary>
        public DateTime TimePlayStart;

        /// <summary>The time the question was presented</summary>
        public DateTime TimeQuestionPresented;

        /// <summary>The network map</summary>
        public Network_Map nMap;

        /// <summary>Report how many seconds it's been since a question was presented</summary>
        public double TimeSincePresentation
        {
            get { return (DateTime.Now - TimeQuestionPresented).TotalSeconds; }
        }

        /// <summary>The current question being presented to the user</summary>
        public int QuestionsAnswered;

        /// <summary>The current number of questions the user has gotten right</summary>
        public int QuestionsRight;

        /// <summary>The current number of questions the user has gotten wrong</summary>
        public int QuestionsWrong;

        /// <summary>The font for questions</summary>
        public Font QuestionFont = new Font("Arial", 20);

        /// <summary>The font for questions</summary>
        public Font AnswerFont = new Font("Arial", 20);

        /// <summary>The color for the question text</summary>
        public Color QuestionTextColor = Color.White;

        /// <summary>The color for the question text</summary>
        public Color CorrectAnswerColor = Color.Green;

        /// <summary>The color for the question text</summary>
        public Color IncorrectAnswerColor = Color.Red;


        /// <summary>The current status of training</summary>
        public enum enumTrainingMode
        {
            /// <summary>Training is not currently happening</summary>
            NotTraning = 0,

            /// <summary>The question has been asked, and the countdown timer is going</summary>
            QuestionAsked = 1,

            /// <summary>The user answered correctly, and are being presented with the response</summary>
            AnswerCorrect = 2,

            /// <summary>The user answered incorrectly, and are being shown the correct response</summary>
            AnswerWrong = 3,

            /// <summary>The user failed the training</summary>
            UserFailed = 4,

            /// <summary>The user won the training</summary>
            UserWon = 5
        }

        /// <summary>The current training mode</summary>
        public enumTrainingMode TrainingMode = enumTrainingMode.NotTraning;

        /// <summary>The collection of available elements to be chosen from</summary>
        public MM_Element[] AvailableElements;

        /// <summary>The correct lat/long the user should see</summary>
        public PointF CorrectAnswer;

        /// <summary>The lat/long the user answered.</summary>
        public PointF UserAnswer;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our training application from a configuration
        /// </summary>
        /// <param name="xElem"></param>
        public MM_Training(XmlElement xElem)
        {
            foreach (XmlElement xChild in xElem.SelectNodes("Level").OfType<XmlElement>())
            {
                MM_Level NewLevel = new MM_Level(xChild);
                Levels.Add(NewLevel.ID, NewLevel);
            }
        }

        /// <summary>
        /// Initialize our traning from a database command that will retrieve all levels
        /// </summary>
        /// <param name="dCmd"></param>
        /// <param name="dRd"></param>
        public MM_Training(DbCommand dCmd, DbDataReader dRd)
        {
            using (DbDataReader dRd2 = dCmd.ExecuteReader())
                while (dRd2.Read())
                {
                    MM_Level NewLevel = new MM_Level(dRd2);
                    Levels.Add(NewLevel.ID, NewLevel);
                }
        }
        #endregion

        #region Training
        /// <summary>
        /// Start a training session
        /// </summary>
        /// <param name="nMap">The network map</param>
        public void InitiateTraining(Network_Map nMap)
        {
            this.nMap = nMap;
            CurrentLevel = null;
            NextQuestion();
            TimePlayStart = DateTime.Now;
        }

        /// <summary>
        /// Start a level by doing all the right things
        /// </summary>
        public void StartLevel()
        {
            if (CurrentLevel.TopLeftLatLong.IsEmpty)
                if (CurrentLevel.County == null)
                    nMap.ResetDisplayCoordinates();
                else 
                    nMap.SetDisplayCoordinates(CurrentLevel.County.Min, CurrentLevel.County.Max);
            else
                nMap.SetDisplayCoordinates(CurrentLevel.TopLeftLatLong, CurrentLevel.BottomRightLatLong);
        }

        /// <summary>
        /// Prepare our level for questions.
        /// </summary>
        private void PrepareLevel()
        {
            CurrentLevel.DeSerialize();
            List<MM_Element> Elements = new List<MM_Element>();
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
            {
                bool Include = true;
                if (CurrentLevel.ElemType != null && Elem.ElemType != null && !Elem.ElemType.Equals(CurrentLevel.ElemType))
                    Include = false;

                if (Include && CurrentLevel.KVLevel != null)
                {
                    if (Elem is MM_Substation)
                    {
                        if ((Elem as MM_Substation).KVLevels.IndexOf(CurrentLevel.KVLevel) != -1)
                            Include = false;
                    }
                    else if (Elem.KVLevel != CurrentLevel.KVLevel)
                        Include = false;
                }
                if (Include && CurrentLevel.County != null)
                {
                    if (Elem is MM_Substation && (Elem as MM_Substation).County != CurrentLevel.County)
                        Include = false;
                    else if (Elem is MM_Line && (Elem as MM_Line).Substation1.County != CurrentLevel.County && (Elem as MM_Line).Substation2.County != CurrentLevel.County)
                        Include = false;
                }
                if (Include && !string.IsNullOrEmpty(CurrentLevel.BooleanToCheck))
                    if (!(bool)Elem.GetType().GetField(CurrentLevel.BooleanToCheck).GetValue(Elem))
                        Include = false;
                if (Include)
                    Elements.Add(Elem);
            }
            QuestionsWrong = 0;
            QuestionsRight = 0;
            QuestionsAnswered = 0;            
            PriorLevel = CurrentLevel;
            AvailableElements = Elements.ToArray();
            StartLevel();
        }



        /// <summary>
        /// Determine our next question
        /// </summary>
        public void NextQuestion()
        {
            //First, determine what our current level should be, and by threshold move to the next level if needed
            if (CurrentLevel == null)
            {
                PriorLevel = null;
                if (!Levels.TryGetValue(0, out CurrentLevel))
                {
                    TrainingMode = enumTrainingMode.NotTraning;
                    return;
                }
            }
            else if (LevelScore >= CurrentLevel.ExitThresholdScore && QuestionsRight >= CurrentLevel.NumberOfQuestions && !Levels.TryGetValue(CurrentLevel.ID + 1, out CurrentLevel))
            {
                TrainingMode = enumTrainingMode.UserWon;
                return;
            }
            else if (QuestionsWrong >= CurrentLevel.FailureThreshold)
            {
                TrainingMode = enumTrainingMode.UserFailed;
                return;
            }




            //Based on our question, determine our next
            if (PriorLevel != CurrentLevel)
                PrepareLevel();


            //Pick random elements based on our needs
            TargetElement = AvailableElements[rnd.Next(0, AvailableElements.Length)];
            if (TargetElement is MM_Line || TargetElement is MM_Substation)
            { }
            else
                TargetElement = TargetElement.Substation;

            //Now, handle our target element
            if (TargetElement is MM_Substation)
            {
                MM_Substation TargetSub = (MM_Substation)TargetElement;
                QuestionText = "Where is substation " + TargetSub.LongName + (String.Equals(TargetSub.LongName, TargetSub.Name, StringComparison.CurrentCultureIgnoreCase) ? "?" : " (" + TargetSub.Name + ")?");
            }
            else if (TargetElement is MM_Line)
            {
                MM_Line TargetLine = (MM_Line)TargetElement;
                QuestionText = "Where is line " + TargetLine.Name + " (from " + TargetLine.Substation1.LongName + " to " + TargetLine.Substation2.LongName + ")?";
            }
            else
                QuestionText = "Where is " + TargetElement.ElemType.Name + " " + TargetElement.Name + "?";
            Data_Integration.ReportSystemLevelData(QuestionText);
            TrainingMode = enumTrainingMode.QuestionAsked;
            TimeQuestionPresented = DateTime.Now;
        }

        /// <summary>
        /// If we're currently training, check to see whether our time requires a notification, shift to next, etc.
        /// </summary>
        public void CheckTimes()
        {
            if (TrainingMode == enumTrainingMode.QuestionAsked && TimeSincePresentation > CurrentLevel.QuestionTimeout)
            {
                QuestionsWrong += 1;
                AnswerText = "Timed out! ";
                TimeQuestionPresented = DateTime.Now;
                UserAnswer = PointF.Empty;
                TrainingMode = enumTrainingMode.AnswerWrong;
            }
            else if ((TrainingMode == enumTrainingMode.AnswerCorrect || TrainingMode == enumTrainingMode.AnswerWrong) && (TimeSincePresentation > CurrentLevel.WaitBetweenQuestions))
                NextQuestion();
        }


        /// <summary>
        /// Handle the user's response
        /// </summary>
        /// <param name="LatLng"></param>
        public void HandleResponse(PointF LatLng)
        {
            //If we're a substation, find our position relative to the sub
            Double Dist;
            UserAnswer = LatLng;
            if (TargetElement is MM_Substation)
            {
                Dist = (TargetElement as MM_Substation).DistanceTo(LatLng);
                CorrectAnswer = (TargetElement as MM_Substation).LatLong;
            }
            else
            {
                MM_Line TargetLine = (MM_Line)TargetElement;
                Dist = MM_Substation.ComputeDistance(LatLng, CorrectAnswer = new PointF((TargetLine.Substation1.Longitude + TargetLine.Substation2.Longitude) / 2f, (TargetLine.Substation1.Latitude + TargetLine.Substation2.Latitude) / 2f));
            }

            //Now, check through our list of reponses, and see if we made it
            QuestionsAnswered++;
            for (int a = 0; a < CurrentLevel.PointsByDistance.Length; a += 2)
                if (Dist <= CurrentLevel.PointsByDistance[a])
                {
                    QuestionsRight += 1;
                    Score += CurrentLevel.PointsByDistance[a + 1];
                    LevelScore += CurrentLevel.PointsByDistance[a + 1];
                    AnswerText = "Correct! You were " + Dist.ToString("#,##0.0") + " miles away (" + CurrentLevel.PointsByDistance[a + 1].ToString() + " pts)";
                    TimeQuestionPresented = DateTime.Now;
                    TrainingMode = enumTrainingMode.AnswerCorrect;
                    return;
                }
            QuestionsWrong += 1;

            AnswerText = "Sorry! You were " + Dist.ToString("#,##0.0") + " miles away.";
            TimeQuestionPresented = DateTime.Now;
            TrainingMode = enumTrainingMode.AnswerWrong;
        }
        #endregion

    }
}