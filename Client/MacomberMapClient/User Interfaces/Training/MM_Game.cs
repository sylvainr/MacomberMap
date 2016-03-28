using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Menuing;
using MacomberMapClient.User_Interfaces.NetworkMap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Training
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc.
    /// This form offers the user a situation-awareness enhancing game
    /// </summary>
    public partial class MM_Game : MM_Form
    {
        #region Variable declarations
        /// <summary>The number of questions answered correctly</summary>
        public float CorrectQuestions = 0;

        /// <summary>The total number of questions asked</summary>
        public float TotalQuestions = 0;

        /// <summary>Whether the user answered the questions</summary>
        public bool IsAnswered = false;

        /// <summary>The right click menu for the game</summary>
        private MM_Popup_Menu RightClickMenu = new MM_Popup_Menu();

        /// <summary>The network map to handle mouse movements</summary>
        public MM_Network_Map_DX NetworkMap;

        private Random rnd = new Random();
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new game form
        /// </summary>
        public MM_Game(MM_Network_Map_DX NetworkMap)
        {
            this.NetworkMap = NetworkMap;
            InitializeComponent();
            RightClickMenu.ImageList = MM_Repository.ViolationImages;
            Data_Integration.RunningForms.Add(this);
            // Data_Integration.ViolationAdded += new Data_Integration.ViolationChangeDelegate(Data_Integration_ViolationAdded);
            AskNextQuestion();
        }

        /* /// <summary>
         /// When a violation is added, try and shut down
         /// </summary>
         /// <param name="Violation"></param>
         private void Data_Integration_ViolationAdded(MM_AlarmViolation Violation)
         {
             try
             {
                 this.Close();
             }
             catch (Exception ex)
             { }
         }*/

        #endregion

        #region Game playing
        /// <summary>
        /// Highlight our rectangle when the user moves the mouse into it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Answer_MouseEnter(object sender, EventArgs e)
        {
            if (IsAnswered)
                return;
            foreach (Label LabelToUpdate in new Label[] { lblAnswer1, lblAnswer2, lblAnswer3, lblAnswer4 })
            {
                bool IsSelected = LabelToUpdate.Equals(sender);
                LabelToUpdate.BackColor = IsSelected ? Color.LightBlue : LabelToUpdate.Parent.BackColor;
                LabelToUpdate.BorderStyle = IsSelected ? BorderStyle.FixedSingle : BorderStyle.None;
            }
        }

        /// <summary>
        /// Remove our rectangle's highlight when the mouse leaves it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Answer_MouseLeave(object sender, EventArgs e)
        {
            if (IsAnswered)
                return;
            Label Sender = sender as Label;
            Sender.BackColor = Sender.Parent.BackColor;
            Sender.BorderStyle = BorderStyle.None;
        }

        /// <summary>
        /// Ask our next question
        /// </summary>
        private void AskNextQuestion()
        {
            try
            {
                //Come up with a question
                int QuestionType = rnd.Next(1, 13);

                //1. Which of the following substations has a wind farm
                //2. Which of the following substations has a unit operated by X
                //3. Which of the following lines is highly congested?
                //4. Which of the following stations is in county X?
                //5. Which of the following lines is 345 KV?
                //6. Which of the following lines goes between county X and Y
                //7. Which county has the lowest bus voltages
                //8. Which county has the highest available reactive voltage support potential?
                //9: Which county has the highest available reactive capacitive support potential?                
                //10: Which substation is operated by the following TO:
                //11: Which line is operated by the following TO:
                //12: Which Unit is operated by the following QSE?

                String Question = null;
                MM_Element[] Elems = new MM_Element[4];
                if (QuestionType == 1)
                {
                    Question = "Which station has a wind gen?";
                    List<MM_Substation> Subs = new List<MM_Substation>(MM_Repository.Substations.Values);
                    for (int a = 0; a < 4; a++)
                        while (Elems[a] == null)
                        {
                            MM_Substation Sub = Subs[rnd.Next(Subs.Count)];
                            if (a == 0 && Sub.Units != null && Sub.Units.Count > 0 && Sub.Units[0].UnitType.Name == "Wind" && Sub.LongName.IndexOf("Wind", StringComparison.CurrentCultureIgnoreCase) == -1)
                                Elems[a] = Sub;
                            else if (a != 0 && Sub.Units != null && Sub.Units.Count > 0 && Sub.Units[0].UnitType.Name != "Wind")
                                Elems[a] = Sub;
                        }
                }
                else if (QuestionType == 10 || QuestionType == 11)
                {
                    for (int a = 0; a < 4; a++)
                        while (Elems[a] == null)
                            if (QuestionType == 10)
                            {
                                List<MM_Substation> Subs = new List<MM_Substation>(MM_Repository.Substations.Values);
                                MM_Substation Sub = Subs[rnd.Next(Subs.Count)];
                                if (a == 0)
                                    Elems[0] = Sub;
                                else if (!Sub.Operator.Equals(Elems[0].Operator))
                                    Elems[a] = Sub;
                                Question = "Which station is operated by " + Elems[0].Operator.Name + "?";
                            }
                            else if (QuestionType == 11)
                            {
                                List<MM_Line> Lines = new List<MM_Line>(MM_Repository.Lines.Values);
                                MM_Line Line = Lines[rnd.Next(Lines.Count)];
                                if (a == 0)
                                    Elems[0] = Line;
                                else if (!Line.Operator.Equals(Elems[0].Operator))
                                    Elems[a] = Line;
                                Question = "Which line is operated by " + Elems[0].Operator.Name + "?";
                            }
                }
                else if (QuestionType == 2 || QuestionType == 12)
                {
                    List<MM_Substation> Subs = new List<MM_Substation>(MM_Repository.Substations.Values);
                    MM_Company FoundQSE = null;
                    for (int a = 0; a < 4; a++)
                        while (Elems[a] == null)
                        {
                            MM_Substation Sub = Subs[rnd.Next(Subs.Count)];
                            if (a == 0 && Sub.Units != null && Sub.Units.Count > 0)
                            {
                                if (QuestionType == 2)
                                    Elems[a] = Sub;
                                else
                                    Elems[a] = Sub.Units[0];
                                FoundQSE = Sub.Units[0].Operator;
                            }
                            else if (a != 0 && Sub.Units != null && Sub.Units.Count > 0 && Sub.LongName.IndexOf(FoundQSE.Name, StringComparison.CurrentCultureIgnoreCase) == -1)
                            {
                                bool FoundOne = false;
                                foreach (MM_Unit Unit in Sub.Units)
                                    if (Unit.Operator.Equals(FoundQSE))
                                        FoundOne = true;
                                if (!FoundOne)
                                    if (QuestionType == 2)
                                        Elems[a] = Sub;
                                    else
                                        Elems[a] = Sub.Units[0];
                            }
                        }
                    if (QuestionType == 2)
                        Question = "Which substation has unit(s) operated by " + FoundQSE.Name + "?";
                    else
                        Question = "Which unit is operated by " + FoundQSE.Name + "?";

                }
                else if (QuestionType == 3)
                {
                    List<MM_Line> Lines = new List<MM_Line>(MM_Repository.Lines.Values);



                    MM_Line CongestedLine = null;
                    while (CongestedLine == null)
                    {
                        MM_Line TestLine = Lines[rnd.Next(Lines.Count)];
                        if (TestLine.LineEnergizationState.Name.EndsWith("Energized") && !TestLine.IsSeriesCompensator && TestLine.LinePercentage >= .7f)
                            CongestedLine = TestLine;
                    }
                    Elems[0] = CongestedLine;
                    Question = "Which line is highly congested (" + CongestedLine.LinePercentageText + ")?";
                    for (int a = 1; a < 4; a++)
                        while (Elems[a] == null)
                        {
                            MM_Line TestLine = Lines[rnd.Next(Lines.Count)];
                            if (TestLine.LineEnergizationState.Name.EndsWith("Energized") && !TestLine.IsSeriesCompensator && TestLine.LinePercentage <= .4f)
                                Elems[a] = TestLine;
                        }
                }
                else if (QuestionType == 4)
                {
                    List<MM_Boundary> Bounds = new List<MM_Boundary>(MM_Repository.Counties.Values);
                    List<MM_Substation> Subs = new List<MM_Substation>(MM_Repository.Substations.Values);

                    MM_Boundary FoundBound = null;
                    while (FoundBound == null || FoundBound.Substations.Count == 0)
                        FoundBound = Bounds[rnd.Next(Bounds.Count)];
                    Question = "Which substation is in " + FoundBound.Name + " county?";
                    Elems[0] = FoundBound.Substations[rnd.Next(FoundBound.Substations.Count)];
                    for (int a = 1; a < 4; a++)
                        while (Elems[a] == null)
                        {
                            MM_Substation Sub = Subs[rnd.Next(Subs.Count)];
                            if (!Sub.County.Equals(FoundBound))
                                Elems[a] = Sub;
                        }
                }
                else if (QuestionType == 5)
                {
                    MM_KVLevel VoltageToFind = new MM_KVLevel[] { MM_Repository.KVLevels["345 KV"], MM_Repository.KVLevels["138 KV"], MM_Repository.KVLevels["69 KV"] }[rnd.Next(3)];
                    List<MM_Line> Lines = new List<MM_Line>(MM_Repository.Lines.Values);
                    for (int a = 0; a < 4; a++)
                        while (Elems[a] == null)
                        {
                            MM_Line TestLine = Lines[rnd.Next(Lines.Count)];
                            if (!TestLine.IsSeriesCompensator)
                                if (a == 0 && TestLine.KVLevel.Equals(VoltageToFind))
                                    Elems[a] = TestLine;
                                else if (a != 0 && !TestLine.KVLevel.Equals(VoltageToFind))
                                    Elems[a] = TestLine;
                        }
                    Question = "Which line is " + VoltageToFind.Name + "?";
                }
                else if (QuestionType == 6)
                {
                    List<MM_Line> Lines = new List<MM_Line>(MM_Repository.Lines.Values);
                    MM_Boundary Bound1 = null, Bound2 = null;
                    for (int a = 0; a < 4; a++)
                        while (Elems[a] == null)
                        {
                            MM_Line TestLine = Lines[rnd.Next(Lines.Count)];
                            if (TestLine.IsSeriesCompensator)
                            { }
                            else if (a == 0 && !TestLine.Substation1.County.Equals(TestLine.Substation2.County))
                            {
                                Elems[a] = TestLine;
                                Bound1 = TestLine.Substation1.County;
                                Bound2 = TestLine.Substation2.County;
                            }
                            else if (a != 0 && !TestLine.Substation1.County.Equals(Bound1) && !TestLine.Substation1.County.Equals(Bound2) && !TestLine.Substation2.County.Equals(Bound1) && !TestLine.Substation2.County.Equals(Bound2))
                                Elems[a] = TestLine;
                        }
                    Question = "Which line travels between " + Bound1.Name + " and " + Bound2.Name + " (" + (Elems[0] as MM_Line).Length.ToString("#,##0.0") + " mi est.)?";
                }
                else if (QuestionType == 7)
                {
                    MM_Boundary LowestBound = null;
                    float LowestpU = float.NaN;
                    foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                        if (Bound.Substations.Count > 0)
                        {
                            float pU = Bound.Average_pU;
                            if (float.IsNaN(LowestpU) || LowestpU > pU)
                            {
                                LowestBound = Bound;
                                LowestpU = pU;
                            }
                        }

                    List<MM_Boundary> Bounds = new List<MM_Boundary>(MM_Repository.Counties.Values);
                    Elems[0] = LowestBound;
                    for (int a = 1; a < 4; a++)
                        while (Elems[a] == null)
                        {
                            MM_Boundary TestBound = Bounds[rnd.Next(0, Bounds.Count)];
                            if (TestBound.Substations.Count > 0 && TestBound.Average_pU > LowestpU + .1f)
                                Elems[a] = TestBound;
                        }
                    Question = "Which county has the lowest avg. bus voltage, at " + LowestpU.ToString("0.00%") + " pU?";
                }
                else if (QuestionType == 8 || QuestionType == 9)
                {
                    List<MM_Boundary> Bounds = new List<MM_Boundary>();
                    List<float> ReactivePotentials = new List<float>();

                    float MaxReac = float.NaN;
                    foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                        if (Bound.Substations.Count > 0)
                        {
                            float ReacAvail = 0;
                            int NumReacs = 0;
                            foreach (MM_Substation Sub in Bound.Substations)
                                if (Sub.ShuntCompensators != null)
                                    foreach (MM_ShuntCompensator SC in Sub.ShuntCompensators)
                                        if (SC.Open && ((QuestionType == 8 && SC.ElemType.Name == "Reactor") || (QuestionType == 9 && SC.ElemType.Name == "Capacitor")))
                                        {
                                            NumReacs++;
                                            ReacAvail += Math.Abs(SC.Nominal_MVAR);
                                        }
                            Bounds.Add(Bound);
                            ReactivePotentials.Add(ReacAvail);
                            if (float.IsNaN(MaxReac) || MaxReac < ReacAvail)
                            {
                                Elems[0] = Bound;
                                MaxReac = ReacAvail;
                            }
                        }
                    Question = "Which county has the largest available " + (QuestionType == 8 ? "reactive" : "capacitive") + " voltage support (" + MaxReac.ToString("#,##0.0") + " MVar)?";
                    for (int a = 1; a < 4; a++)
                        while (Elems[a] == null)
                            Elems[a] = Bounds[rnd.Next(0, Bounds.Count)];


                }

                lblQuestion.Text = Question;
                int[] OutVals = new int[] { -1, -1, -1, -1 };
                for (int a = 0; a < OutVals.Length; a++)
                    while (OutVals[a] == -1)
                    {
                        bool FoundIt = false;
                        int Next = rnd.Next(0, 4);
                        for (int b = 0; b < a; b++)
                            if (OutVals[b] == Next)
                                FoundIt = true;
                        if (!FoundIt)
                            OutVals[a] = Next;
                    }


                int MaxRight = Math.Max(lblQuestion.Right, lblScore.Right);
                for (int a = 0; a < 4; a++)
                {
                    Label lbl = Controls["lblAnswer" + (a + 1).ToString()] as Label;
                    if (Elems[OutVals[a]] is MM_Substation)
                    {
                        MM_Substation Sub = Elems[OutVals[a]] as MM_Substation;
                        if (Sub.LongName == Sub.Name)
                            lbl.Text = (a + 1).ToString() + ") " + Sub.LongName;
                        else
                            lbl.Text = (a + 1).ToString() + ") " + Sub.LongName + " (" + Sub.Name + ")";
                    }
                    else if (Elems[OutVals[a]] is MM_Boundary)
                        lbl.Text = (a + 1).ToString() + ") " + Elems[OutVals[a]].Name;
                    else
                        lbl.Text = (a + 1).ToString() + ") " + Elems[OutVals[a]].ElementDescription();
                    lbl.Tag = new KeyValuePair<MM_Element, bool>(Elems[OutVals[a]], OutVals[a] == 0);
                    lbl.BackColor = lbl.Parent.BackColor;
                    lbl.BorderStyle = BorderStyle.None;
                    lbl.AutoSize = true;
                    MaxRight = Math.Max(MaxRight, lbl.Right);
                }

                this.Width = MaxRight + 20;
                this.Refresh();


                pbTimeLeft.Value = 30;
                tmrQuestion.Enabled = true;
                tmrQuestion.Interval = 1000;
                IsAnswered = false;
            }
            catch (Exception)
            {
                AskNextQuestion();
            }
        }

        /// <summary>
        /// Answer our question
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnswerQuestion(object sender, MouseEventArgs e)
        {
            if (IsAnswered)
                return;

            tmrQuestion.Enabled = false;

            Label Sender = sender as Label;
            TotalQuestions++;

            int Selected = int.Parse(Sender.Name.Substring(Sender.Name.Length - 1, 1));
            KeyValuePair<MM_Element, bool> kvp = (KeyValuePair<MM_Element, bool>)Sender.Tag;

            if (e.Button == MouseButtons.Right)
            {
                RightClickMenu.Show(this, e.Location, kvp.Key, true);
                return;
            }
            else if (kvp.Value)
            {
                CorrectQuestions++;
                for (int a = 1; a <= 4; a++)
                    Controls["lblAnswer" + a.ToString()].BackColor = a == Selected ? Color.LightGreen : Sender.Parent.BackColor;
            }
            else
                for (int a = 1; a <= 4; a++)
                {
                    Control lbl = Controls["lblAnswer" + a.ToString()] as Control;
                    lbl.BackColor = a == Selected ? Color.Red : ((KeyValuePair<MM_Element, bool>)lbl.Tag).Value ? Color.LightGreen : Sender.Parent.BackColor;
                }
            lblScore.Text = "Score: " + CorrectQuestions.ToString("#,##0") + "/" + TotalQuestions.ToString("#,##0");
            IsAnswered = true;
            tmrQuestion.Interval = 3000;
            tmrQuestion.Enabled = true;


        }

        /// <summary>
        /// Handle a question being shown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrQuestion_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!IsAnswered)
                {
                    pbTimeLeft.Value--;
                    if (pbTimeLeft.Value == pbTimeLeft.Minimum)
                    {

                        for (int a = 1; a <= 4; a++)
                        {
                            Label lbl = Controls["lblAnswer" + a.ToString()] as Label;
                            lbl.BackColor = ((KeyValuePair<MM_Element, bool>)lbl.Tag).Value ? Color.Yellow : lbl.Parent.BackColor;
                            lbl.BorderStyle = ((KeyValuePair<MM_Element, bool>)lbl.Tag).Value ? BorderStyle.FixedSingle : BorderStyle.None;

                        }
                        IsAnswered = true;
                        tmrQuestion.Interval = 5000;
                    }
                }
                else
                    AskNextQuestion();
            }
            catch
            { }
        }

        #endregion

    }
}
