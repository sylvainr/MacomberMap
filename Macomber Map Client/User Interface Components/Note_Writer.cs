using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using Macomber_Map.Data_Connections;
using System.Threading;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class allows the operator to create a new note on an element
    /// </summary>
    public partial class Note_Writer : Form
    {        
        /// <summary>
        /// Initialize the note writer
        /// </summary>
        public Note_Writer(MM_Element BaseElement)
        {
            InitializeComponent();
            this.Text = " Note: " + BaseElement.ElemType.Name + " " + BaseElement.Name;
            txtAuthor.Text = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            txtNote.Focus();
        }


        /// <summary>
        /// Create a new note, and if selected, upload it.
        /// </summary>
        /// <param name="NoteElement">The element about which the note is being written</param>        
        public static void CreateNote(MM_Element NoteElement)
        {
            
            Note_Writer NewNote = new Note_Writer(NoteElement);
            if (NewNote.ShowDialog() == DialogResult.OK)
                if (Data_Integration.MMServer != null)
                    Data_Integration.MMServer.UploadNote(NoteElement, NewNote.txtAuthor.Text, NewNote.txtNote.Text);
                else
                    Data_Integration.HandleNoteEntry(new MM_Note(MM_Repository.Notes.Count, DateTime.Now, NewNote.txtAuthor.Text, NewNote.txtNote.Text, NoteElement));

            if (Data_Integration.MMServer != null)
                         Data_Integration.MMServer.LoadNotes();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }       
}
