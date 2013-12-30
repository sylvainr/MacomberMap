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
    public partial class NoteWriter : Form
    {        
        /// <summary>
        /// Initialize the note writer
        /// </summary>
        public NoteWriter(MM_Element BaseElement)
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
        /// <param name="IntegrationLayer">The data integration layer</param>
        public static void CreateNote(MM_Element NoteElement, Data_Integration IntegrationLayer)
        {
            
            NoteWriter NewNote = new NoteWriter(NoteElement);
            if (NewNote.ShowDialog() == DialogResult.OK)
                IntegrationLayer.CIMServer.UploadNote(NoteElement.TEID, NewNote.txtAuthor.Text, NewNote.txtNote.Text);
            IntegrationLayer.CIMServer.LoadNotes();
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