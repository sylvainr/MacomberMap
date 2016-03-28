using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapCommunications.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Information
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class allows the operator to create a new note on an element
    /// </summary>
    public partial class MM_Note_Writer : Form
    {
        #region Variable declarations
        /// <summary>The element associated with the note writer </summary>
        private MM_Element BaseElement = null;

        /// <summary>
        /// Return a note corresponding to our created information
        /// </summary>
        public MM_Note CreatedNote
        {
            get { return new MM_Note() { Author = txtAuthor.Text, Note = txtNote.Text, CreatedOn = DateTime.Now, Acknowledged = false, AssociatedElement = BaseElement.TEID }; }
        }
        #endregion

        /// <summary>
        /// Initialize the note writer
        /// </summary>
        public MM_Note_Writer(MM_Element BaseElement)
        {
            InitializeComponent();
            this.BaseElement = BaseElement;
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
            using (MM_Note_Writer NewNote = new MM_Note_Writer(NoteElement))
                if (NewNote.ShowDialog() == DialogResult.OK)
                {
                    MM_Note OutNote = NewNote.CreatedNote;
                    if (MM_Server_Interface.Client != null)
                         OutNote.ID = MM_Server_Interface.Client.UploadNote(OutNote);
                    Data_Integration.HandleNoteEntry(OutNote);
                }            
        }

        /// <summary>
        /// Handle the OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Handle the cancel button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }



    }
}