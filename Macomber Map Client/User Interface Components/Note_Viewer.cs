using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using Macomber_Map.Data_Elements;
using Macomber_Map.User_Interface_Components.NetworkMap;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class contains the viewer for intra-operator/engineer notes
    /// </summary>
    public partial class Note_Viewer : Form
    {
        #region Variable declaration
        /// <summary>The key indicators for the system</summary>
        public Key_Indicators KeyIndicators;

        /// <summary>The menu for handling right-click events</summary>
        private MM_Popup_Menu RightClickMenu = new MM_Popup_Menu();

        /// <summary>The associated network map (for zooming/panning)</summary>
        public Network_Map nMap;

        /// <summary>Whether any new violations are in place</summary>
        public string NoteText
        {
            get
            {
                int Unacknowledged = 0;
                foreach (ListViewItem i in lvNotes.Items)
                    if (!(i.Tag as MM_Note).Acknowledged)
                        Unacknowledged++;
                if (Unacknowledged > 0)
                    return "   *" + Unacknowledged.ToString("#,##0");
                else
                    return "   " + MM_Repository.Notes.Count.ToString("#,##0");
                
            }
        }


        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new note viewer
        /// </summary>
        /// <param name="KeyIndicators">The key indicators (for notifications of new notes)</param>
        /// <param name="nMap">The associated network map</param>
        public Note_Viewer(Key_Indicators KeyIndicators, Network_Map nMap)
        {
            
            this.KeyIndicators = KeyIndicators;
            this.nMap = nMap;
            InitializeComponent();            
            //Add in all of our notes
            foreach (MM_Note Note in MM_Repository.Notes.Values)
                NoteAdded(Note);

            //Set our hooks for future activities
            Data_Integration.NoteAdded += new Data_Integration.NoteChangeDelegate(NoteAdded);
            Data_Integration.NoteModified += new Data_Integration.NoteChangeDelegate(NoteModified);
            Data_Integration.NoteRemoved += new Data_Integration.NoteChangeDelegate(NoteRemoved);

            //Also, add all current notes in 
            lock (this)
                foreach (MM_Note NewNote in MM_Repository.Notes.Values)
                    NoteAdded(NewNote);
        }
        #endregion

        #region UI events
        /// <summary>
        /// Override the form's closing to handle hiding if the user clicks on the 'X'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Note_Viewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }
        #endregion

        #region Note event handling
        /// <summary>
        /// A thread-safe delegate for handling a note
        /// </summary>
        /// <param name="Note">The note to be handled</param>
        private delegate void SafeNoteHandler(MM_Note Note);

        /// <summary>
        /// Remove a note from the display
        /// </summary>
        /// <param name="Note">The note to be handled</param>
        private void NoteRemoved(Macomber_Map.Data_Elements.MM_Note Note)
        {
            if (InvokeRequired)
                Invoke(new SafeNoteHandler(NoteRemoved), Note);
            else
                lvNotes.Items.RemoveByKey(Note.ID.ToString());
        }

        /// <summary>
        /// Handle the modification of a note
        /// </summary>
        /// <param name="Note">The note to be handled</param>
        private void NoteModified(Macomber_Map.Data_Elements.MM_Note Note)
        {
            if (InvokeRequired)
                Invoke(new SafeNoteHandler(NoteModified), Note);
            else
            {
                ListViewItem FoundItem = lvNotes.Items[Note.ID.ToString()];
                FoundItem.SubItems[0].Text = "*";
                FoundItem.SubItems[1].Text = Note.CreatedOn.ToString();
                FoundItem.SubItems[2].Text = Note.AssociatedElement.ElemType.Name;
                FoundItem.SubItems[3].Text = Note.Author;
                FoundItem.SubItems[4].Text = Note.Note;                
            }
        }

        /// <summary>
        /// Add a new note to our display
        /// </summary>
        /// <param name="Note">The note to be handled</param>
        private void NoteAdded(Macomber_Map.Data_Elements.MM_Note Note)
        {
            if (InvokeRequired)
                Invoke(new SafeNoteHandler(NoteAdded), Note);
            else
            {
                ListViewItem NewItem = lvNotes.Items.Add(Note.ID.ToString(), "*", "");
                NewItem.SubItems.Add(Note.CreatedOn.ToString());
                NewItem.SubItems.Add(Note.AssociatedElement.Name);
                NewItem.SubItems.Add(Note.AssociatedElement.ElemType.Name);
                NewItem.SubItems.Add(Note.Author);
                NewItem.SubItems.Add(Note.Note);
                NewItem.Tag = Note;                
            }
        }
        #endregion

        /// <summary>
        /// Handle the right-click option on a note
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvNotes_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ListViewHitTestInfo ht = lvNotes.HitTest(e.Location);
            if (ht.Item != null && ht.Item.Tag is MM_Note && (ht.Item.Tag as MM_Note).AssociatedElement != null)
                RightClickMenu.Show(this, e.Location, (ht.Item.Tag as MM_Note), true);            


        }



        /// <summary>
        /// Every timer interval, if we're visible, update everything.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            //If we're showing, update all columns
            if (this.Visible)
                foreach (ColumnHeader col in lvNotes.Columns)
                    col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        /// <summary>
        /// Handle the clicking on an item, by marking it as "acknowledged" or seen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvNotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvNotes.SelectedItems.Count == 1)
            {
                MM_Note Note = lvNotes.SelectedItems[0].Tag as MM_Note;
                Note.Acknowledged = true;
                lvNotes.SelectedItems[0].Text = "";
            }
        }

        private void lvNotes_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo ht = lvNotes.HitTest(e.Location);
            if (ht != null)
            {
                MM_Note Note = (ht.Item.Tag as MM_Note);
                Note.Acknowledged = true;
                ht.Item.Text = "";                
                MessageBox.Show(Note.Note, Note.Author + " on " + Note.CreatedOn, MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }
    }
}
