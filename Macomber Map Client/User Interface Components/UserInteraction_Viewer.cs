using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Macomber_Map.Data_Elements;
using Macomber_Map.Data_Connections;
using System.Xml;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class holds information on a series of events associated with a user switch
    /// </summary>
    public partial class UserInteraction_Viewer : Form
    {
        #region Variable declarations
        /// <summary>The name of the user engaging in switching actions</summary>
        public String UserName;

        /// <summary>The event associated with our user</summary>
        public MM_AlarmViolation AssociatedEvent;

        /// <summary>Track a prior event being triggered</summary>
        public bool PriorEvent = false;

        /// <summary>Our popup menu</summary>
        public MM_Popup_Menu PopupMenu = new MM_Popup_Menu();
        
        #endregion


        /// <summary>
        /// Initialize a new user event tracking window
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="EventTime"></param>
        public UserInteraction_Viewer(String UserName, DateTime EventTime)
        {
            InitializeComponent();
            this.Text = "Event log: " + UserName;
            lvHistory.ListViewItemSorter = new UserInteraction_Sorter(0,true);
            ShowInTaskbar = true;
            lvHistory.FullRowSelect = true;
            AssociatedEvent = new MM_AlarmViolation();           
            AssociatedEvent.EventTime = EventTime;
            AssociatedEvent.New = true;
            AssociatedEvent.Type = MM_Repository.ViolationTypes["UserAction"];
            AssociatedEvent.ViolatedElement = new MM_Element();
            AssociatedEvent.ViolatedElement.ElemType = MM_Repository.FindElementType("User");
            AssociatedEvent.ViolatedElement.Name = UserName;
            AssociatedEvent.ViolatedElement.TEID = Data_Connections.Data_Integration.GetTEID();
        }

        /// <summary>
        /// Handle the closing of our form to hide instead
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            //base.OnClosing(e);
        }

        private delegate void SafeShow();
        /// <summary>
        /// Safely show our display
        /// </summary>
        public void ShowForm()
        {
            if (lvHistory.InvokeRequired) //was InvokeRequired - nataros
                lvHistory.Invoke(new SafeShow(ShowForm));
            else
                try
                {
                    {
                        //Make our form so it's under our mouse cursor
                        this.Location = new Point(Cursor.Position.X + 9, Cursor.Position.Y+17);
                        if (!Visible)
                        {
                            Visible = true;
                            Activate();
                        }
                        else
                            Visible = false;
                    }
                }
                catch (Exception)
                { }
        }

        /// <summary>
        /// Handle the UI viewer losing focus
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
        }

        private delegate void SafeAddEvent(DateTime EventTime, String Station, String Type, String Name, String Action, String OldValue, String NewValue);

        /// <summary>
        /// Add an event without updating the core system (just paste in display)
        /// </summary>
        /// <param name="EventTime"></param>
        /// <param name="Station"></param>
        /// <param name="ElemType"></param>
        /// <param name="Name"></param>
        /// <param name="Action"></param>
        /// <param name="OldValue"></param>
        /// <param name="NewValue"></param>
        public void AddEventWithoutNotification(DateTime EventTime, String Station, String ElemType, String Name, String Action, String OldValue, String NewValue)
        {
            if (lvHistory.InvokeRequired)
                lvHistory.Invoke(new SafeAddEvent(AddEvent), EventTime, Station, ElemType, Name, Action, OldValue, NewValue);
            else            
                lvHistory.Items.Add(new UserInteraction(AssociatedEvent.EventTime = EventTime, Station, ElemType, Name, Action, OldValue, NewValue));            
        }

        /// <summary>
        /// Add an event to our user space
        /// </summary>
        /// <param name="EventTime"></param>
        /// <param name="Station"></param>
        /// <param name="ElemType"></param>
        /// <param name="Name"></param>
        /// <param name="Action"></param>
        /// <param name="NewValue"></param>
        /// <param name="OldValue"></param>
        public void AddEvent(DateTime EventTime, String Station, String ElemType, String Name, String Action, String OldValue, String NewValue)
        {
            if (lvHistory.InvokeRequired)
                lvHistory.Invoke(new SafeAddEvent(AddEvent), EventTime, Station, ElemType, Name, Action, OldValue, NewValue);
            else
            {
                lvHistory.Items.Add(new UserInteraction(AssociatedEvent.EventTime = EventTime, Station, ElemType, Name, Action, OldValue, NewValue));
                lvHistory.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                AssociatedEvent.Text = Station + " " + ElemType + " " + Name;
                AssociatedEvent.ViolatedElement.Substation = MM_Repository.Substations[Station];
                if (PriorEvent)
                    Data_Integration.UpdateViolation(AssociatedEvent, AssociatedEvent.Text, EventTime);
                //else

                 //   Data_Integration.RemoveViolation(AssociatedEvent);
                //AssociatedEvent.ViolatedElement.Substation = MM_Repository.Substations[Station];
                //AssociatedEvent.ViolatedElement.Operator = AssociatedEvent.ViolatedElement.Substation.Operator;
                //AssociatedEvent.ViolatedElement.Owner= AssociatedEvent.ViolatedElement.Substation.Owner;
                Data_Integration.CheckAddViolation(AssociatedEvent);
                PriorEvent = true;
            }
        }

        /// <summary>
        /// Handle the resizing of our user interaction viewer display
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            try
            {
                lvHistory.TileSize = new Size(DisplayRectangle.Width - 24, lvHistory.Font.Height * lvHistory.Columns.Count);            
            }
            catch (Exception)
            { }
            base.OnResize(e);
        }


        /// <summary>
        /// This class holds infromation on a comparer for user interaction items
        /// </summary>
        public class UserInteraction_Sorter : IComparer
        {
            /// <summary>Our column used for sorting</summary>
            public int SortColumn = 0;

            /// <summary>Ascending/descending</summary>
            public bool Ascending = true;

            /// <summary>
            /// Initialize a new user interaction sorter
            /// </summary>
            /// <param name="SortColumn"></param>
            /// <param name="Ascending"></param>
            public UserInteraction_Sorter(int SortColumn, bool Ascending)
            {
                this.SortColumn = SortColumn;
                this.Ascending = Ascending;
            }

            /// <summary>
            /// Handle a column click event
            /// </summary>
            /// <param name="ColumnNumber"></param>
            public void ColumnClick(int ColumnNumber)
            {
                if (ColumnNumber == SortColumn)
                    Ascending ^= true;
                else
                    SortColumn = ColumnNumber;
            }

            /// <summary>
            /// Compare two columns
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(Object x, Object y)
            {
                UserInteraction X = (UserInteraction)x;
                UserInteraction Y = (UserInteraction)y;
                if (SortColumn == 0)
                    return X.EventTime.CompareTo(Y.EventTime) * (Ascending ? 1 : -1);
                else if (SortColumn == 1)
                    return X.Station.CompareTo(Y.Station) * (Ascending ? 1 : -1);
                else if (SortColumn == 2)
                    return X.ElemType.CompareTo(Y.ElemType) * (Ascending ? 1 : -1);
                else if (SortColumn == 3)
                    return X.ElemName.CompareTo(Y.ElemName) * (Ascending ? 1 : -1);
                else if (SortColumn == 4)
                    return X.Action.CompareTo(Y.Action) * (Ascending ? 1 : -1);
                else if (SortColumn == 5)
                    return X.NewValue.CompareTo(Y.NewValue) * (Ascending ? 1 : -1);
                else 
                    return X.OldValue.CompareTo(Y.OldValue) * (Ascending ? 1 : -1);
                
            }


        }

        /// <summary>
        /// This class holds information on the user interaction
        /// </summary>
        private class UserInteraction : ListViewItem
        {
            public DateTime EventTime;
            public String Station;
            public String ElemName;
            public MM_Communication.Elements.MM_Element.enumElementType ElemType;
            public String Action;
            public String OldValue;
            public String NewValue;


            public UserInteraction(DateTime EventTime, string Station, String ElemType, String Name, String Action, String OldValue, String NewValue)
            {

                //If requested, write out our value
                if (Data_Integration.ValueChangeLog != null)
                    Data_Integration.ValueChangeLog.WriteLine("<UserControl TimeStamp=\"" + XmlConvert.ToString(EventTime, XmlDateTimeSerializationMode.Unspecified) + "\" Action=\"" + Action + "\" Station=\"" + Station + "\" ElemType=\"" + ElemType + "\" Name=\"" + Name + "\" OldValue=\"" + OldValue + "\" NewValue=\"" + NewValue + "\" />");
                this.EventTime = EventTime;
                this.Station = Station;
                this.ElemName = Name;
                this.Action = Action;
                this.ElemType = (MM_Communication.Elements.MM_Element.enumElementType)Enum.Parse(typeof(MM_Communication.Elements.MM_Element.enumElementType), ElemType, true);
                this.OldValue = OldValue;
                this.NewValue = NewValue;
                this.Text = EventTime.ToString();
                SubItems.Add(Station);
                SubItems.Add(ElemType);
                SubItems.Add(Name);
                SubItems.Add(Action);
                SubItems.Add(OldValue);
                SubItems.Add(NewValue);                
            }
        }

        /// <summary>
        /// Handle the mouse click on our component
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvHistory_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hti = lvHistory.HitTest(e.Location);
            if (hti.Item != null && e.Button == MouseButtons.Right)
            {
                UserInteraction FoundInteraction = hti.Item as UserInteraction;
                MM_Substation FoundSub;
                if (!MM_Repository.Substations.TryGetValue(FoundInteraction.Station, out FoundSub))
                    return;                
                PopupMenu.Show(this, e.Location, FoundSub, true);
            }
        }

        /// <summary>
        /// Handle the left mouse double-click by opening up our one-line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvHistory_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hti = lvHistory.HitTest(e.Location);
            if (hti.Item == null || e.Button != MouseButtons.Left)
                return;
            UserInteraction FoundInteraction = hti.Item as UserInteraction;
            MM_Substation FoundSub;
            if (!MM_Repository.Substations.TryGetValue(FoundInteraction.Station, out FoundSub))
                return;
            Form_Builder.OneLine_Display(FoundSub, Program.MM.ctlNetworkMap);
        }
    }
}