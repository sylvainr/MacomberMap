using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.NetworkMap;
using MacomberMapCommunications.Messages;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Training
{
    /// <summary>
    /// (C) 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved
    /// This class provides the central training interface
    /// </summary>
    public class MM_Training
    {
        #region Variable declarations
        /// <summary>Our random number generator</summary>
        public Random rnd = new Random();

        /// <summary>The collection of training levels</summary>
        public SortedDictionary<int, MM_Training_Level> Levels = new SortedDictionary<int, MM_Training_Level>();

        private double _Score = 0, _LevelScore = 0;
        private int _QuestionsAnswered = 0, _QuestionsRight = 0, _QuestionsWrong = 0;
        private MM_Training_Level _CurrentLevel = null, _PriorLevel = null;
        private enumTrainingMode _TrainingMode = enumTrainingMode.NotTraning;

        /// <summary>The current level</summary>
        public MM_Training_Level CurrentLevel
        {
            get { return _CurrentLevel; }
            set { _CurrentLevel = value; UpdateValue("CurrentLevel", value); }
        }

        /// <summary>The prior level for training</summary>
        public MM_Training_Level PriorLevel
        {
            get { return _PriorLevel; }
            set { _PriorLevel = value; UpdateValue("PriorLevel", value); }
        }

        /// <summary>The target element the user should be shown</summary>
        public MM_Element TargetElement = null;

        /// <summary>The instructions to be presented to the user</summary>
        public String QuestionText;

        /// <summary>The response to the user's click</summary>
        public String AnswerText;

        /// <summary>The user's current score</summary>
        public Double Score
        {
            get { return _Score; }
            set { _Score = value; UpdateValue("Score", value); }
        }

        /// <summary>The user's current score</summary>
        public Double LevelScore
        {
            get { return _LevelScore; }
            set { _LevelScore = value; UpdateValue("LevelScore", value); }
        }

        /// <summary>When the user stared playing the game</summary>
        public DateTime TimePlayStart;

        /// <summary>The time the question was presented</summary>
        public DateTime TimeQuestionPresented;

        /// <summary>The network map</summary>
        public MM_Network_Map_DX nMap;

        /// <summary>Report how many seconds it's been since a question was presented</summary>
        public double TimeSincePresentation
        {
            get { return (DateTime.Now - TimeQuestionPresented).TotalSeconds; }
        }

        /// <summary>The current question being presented to the user</summary>
        public int QuestionsAnswered
        {
            get { return _QuestionsAnswered; }
            set { _QuestionsAnswered = value; UpdateValue("QuestionsAnswered", value); }
        }

        /// <summary>The current number of questions the user has gotten right</summary>
        public int QuestionsRight
        {
            get { return _QuestionsRight; }
            set { _QuestionsRight = value; UpdateValue("QuestionsRight", value); }
        }

        /// <summary>The current number of questions the user has gotten wrong</summary>
        public int QuestionsWrong
        {
            get { return _QuestionsWrong; }
            set { _QuestionsWrong = value; UpdateValue("QuestionsWrong", value); }
        }

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

        /// <summary>The ID of our session</summary>
        public int SessionId = -1;

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
        public enumTrainingMode TrainingMode
        {
            get { return _TrainingMode; }
            set { _TrainingMode = value; UpdateValue("TrainingMode", (int)value); }
        }

        /// <summary>The collection of available elements to be chosen from</summary>
        public MM_Element[] AvailableElements;

        /// <summary>The correct lat/long the user should see</summary>
        public PointF CorrectAnswer;

        /// <summary>The lat/long the user answered.</summary>
        public PointF UserAnswer;

        /// <summary>The timestamp of when our message was sent</summary>
        public DateTime MessageTime;
        #endregion

        #region Value Updates
        /// <summary>
        /// Update a value
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="value"></param>
        public void UpdateValue(String Title, object value)
        {
            if (SessionId == -1 || value==null)
                return;
            else
                if (value is MM_Training_Level)
                    MM_Server_Interface.Client.UpdateTrainingInformation(Title, ((MM_Training_Level)value).ID, SessionId);
                else
                    MM_Server_Interface.Client.UpdateTrainingInformation(Title, Convert.ToSingle(value), SessionId);

        }
        #endregion


        #region Training
        /// <summary>
        /// Start a training session
        /// </summary>
        /// <param name="nMap">The network map</param>
        public void InitiateTraining(MM_Network_Map_DX nMap)
        {
            this.nMap = nMap;
            CurrentLevel = null;
            this.SessionId = MM_Server_Interface.Client.StartTrainingSession();
            NextQuestion();
            TimePlayStart = DateTime.Now;


        }

        /// <summary>
        /// Start a level by doing all the right things
        /// </summary>
        public void StartLevel()
        {
            if (CurrentLevel.TopLeftLngLat==null)
                if (CurrentLevel.County == null)
                    nMap.ResetDisplayCoordinates();
                else
                {
                    MM_Boundary County = MM_Repository.Counties[CurrentLevel.County];
                    nMap.SetDisplayCoordinates(County.Min, County.Max);
                }
            else
                nMap.SetDisplayCoordinates(CurrentLevel.TopLeftLngLat, CurrentLevel.BottomRightLngLat);
        }

        /// <summary>
        /// Prepare our level for questions.
        /// </summary>
        private void PrepareLevel()
        {
            MM_KVLevel KVLevel = null;
            MM_Boundary County = null;
            MM_Element_Type ElemType = null;
            if (CurrentLevel.KVLevel != null)
                MM_Repository.KVLevels.TryGetValue(CurrentLevel.KVLevel, out KVLevel);
            if (CurrentLevel.County != null)
                MM_Repository.Counties.TryGetValue(CurrentLevel.County, out County);
            if (CurrentLevel.ElemType != null)
                MM_Repository.ElemTypes.TryGetValue(CurrentLevel.ElemType, out ElemType);

            List<MM_Element> Elements = new List<MM_Element>();
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
            {
                bool Include = true;                
                if (ElemType != null && Elem.ElemType != null && !Elem.ElemType.Equals(ElemType))
                    Include = false;

                if (Include && CurrentLevel.KVLevel != null)
                {
                    if (Elem is MM_Substation)
                    {

                        if (((MM_Substation)Elem).KVLevels.IndexOf(KVLevel) == -1)
                            Include = false;
                    }
                    else if (Elem.KVLevel != KVLevel)
                        Include = false;
                }
                if (Include && CurrentLevel.County != null)
                {
                    if (Elem is MM_Substation && (Elem as MM_Substation).County != County)
                        Include = false;
                    else if (Elem is MM_Line && (Elem as MM_Line).Substation1.County != County && (Elem as MM_Line).Substation2.County != County)
                        Include = false;
                }
                if (Include && !string.IsNullOrEmpty(CurrentLevel.BooleanToCheck))
                {
                    MemberInfo[] mI = Elem.GetType().GetMember(CurrentLevel.BooleanToCheck);
                    if (mI.Length != 1)
                        Include = false;//throw new InvalidOperationException("Unable to locate member " + CurrentLevel.BooleanToCheck);
                    else if (mI[0] is PropertyInfo)
                    {
                        if (!(bool)((PropertyInfo)mI[0]).GetValue(Elem, null))
                            Include = false;
                    }
                    else if (mI[0] is FieldInfo)
                    {
                        if (!(bool)((FieldInfo)mI[0]).GetValue(Elem))
                            Include = false;
                    }
                }
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
            MM_Training_Level FoundLevel;
            if (CurrentLevel == null)
            {
                PriorLevel = null;
                if (!Levels.TryGetValue(0, out FoundLevel))
                {
                    TrainingMode = enumTrainingMode.NotTraning;
                    return;
                }
                else
                    CurrentLevel = FoundLevel;
            }
            else if (LevelScore >= CurrentLevel.ExitThresholdScore && QuestionsRight >= CurrentLevel.NumberOfQuestions)
                if (!Levels.TryGetValue(CurrentLevel.ID + 1, out FoundLevel))
                {
                    MessageTime = DateTime.Now;
                    TrainingMode = enumTrainingMode.UserWon;
                    return;
                }
                else
                    CurrentLevel = FoundLevel;

            else if (QuestionsWrong >= CurrentLevel.FailureThreshold)
            {
                    MessageTime = DateTime.Now;
                TrainingMode = enumTrainingMode.UserFailed;
                return;
            }




            //Based on our question, determine our next
            if (PriorLevel != CurrentLevel)
                PrepareLevel();

            //If we have no elements, assume we won
            if (AvailableElements.Length==0)
            {
                TrainingMode = enumTrainingMode.UserWon;
                return;
            }

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
        /// <param name="LngLat"></param>
        public void HandleResponse(PointF LngLat)
        {
            //If we're a substation, find our position relative to the sub
            Double Dist;
            UserAnswer = LngLat;
            if (TargetElement is MM_Substation)
            {
                Dist = (TargetElement as MM_Substation).DistanceTo(LngLat);
                CorrectAnswer = (TargetElement as MM_Substation).LngLat;
            }
            else
            {
                MM_Line TargetLine = (MM_Line)TargetElement;
                Dist = MM_Substation.ComputeDistance(LngLat, CorrectAnswer = new PointF((TargetLine.Substation1.Longitude + TargetLine.Substation2.Longitude) / 2f, (TargetLine.Substation1.Latitude + TargetLine.Substation2.Latitude) / 2f));
            }

            //Now, check through our list of reponses, and see if we made it
            QuestionsAnswered++;

            if (Dist > CurrentLevel.NoScoreThreshold)
            {
                AnswerText = "Sorry! You were " + Dist.ToString("#,##0.0") + " miles away.";
                TimeQuestionPresented = DateTime.Now;
                TrainingMode = enumTrainingMode.AnswerWrong;
                QuestionsWrong++;
            }
            else
            {
                double TotalScore;
                if (Dist <= CurrentLevel.BullsEyeThresholdDistance)
                    TotalScore = CurrentLevel.UpperScoreValue;
                else
                {
                    double XPercentile = (Dist - CurrentLevel.BullsEyeThresholdDistance) / (CurrentLevel.NoScoreThreshold - CurrentLevel.BullsEyeThresholdDistance);
                    TotalScore = ((CurrentLevel.LowerScoreValue - CurrentLevel.UpperScoreValue) * XPercentile) + CurrentLevel.UpperScoreValue;
                }
                double TimeScore = CurrentLevel.MaximumTimeScore * ((CurrentLevel.QuestionTimeout - TimeSincePresentation) / CurrentLevel.QuestionTimeout);
                Score += TimeScore + TotalScore;
                LevelScore += TimeScore + TotalScore;
                QuestionsRight += 1;
                AnswerText = "Correct! You were " + Dist.ToString("#,##0.0") + " miles away (" + TotalScore.ToString("0") + " pts; bonus " + TimeScore.ToString("0") + " pts)";
                TimeQuestionPresented = DateTime.Now;
                TrainingMode = enumTrainingMode.AnswerCorrect;
            }




        }
        #endregion

    }
}