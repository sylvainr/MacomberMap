using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using Macomber_Map.Data_Elements;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D., Electrical Reliability Council of Texas, Inc. All Rights Reserved.
    /// This form allows for selection of a model from our Oracle back-end modeling database 
    /// </summary>
    public partial class MM_Model_Selector : Form
    {
        #region Variable declarations
        /// <summary>The ID of the selected model</summary>
        public int SelectedModelID
        {
            get { return SelectedModel.ID; }
        }

        /// <summary>Our collection of models</summary>
        private List<MM_Database_Model> Models = new List<MM_Database_Model>();

        /// <summary>Our collection of active models</summary>
        private List<MM_Database_Model> ActiveModels = new List<MM_Database_Model>();

        /// <summary>Whether our form is updating</summary>
        private bool Updating = false;

        /// <summary>Our selected model</summary>
        private MM_Database_Model SelectedModel = null;
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize our model selector
        /// </summary>
        /// <param name="oConn"></param>
        public MM_Model_Selector(OracleConnection oConn)
        {
            InitializeComponent();
            cmbModelCategory.Items.Add("All");

            //First, determine the list of categories
            using (OracleCommand oCmd = new OracleCommand("SELECT DISTINCT(MODELCATEGORY) FROM MM_DATABASE_MODEL", oConn))
            using (OracleDataReader oRd = oCmd.ExecuteReader())
                while (oRd.Read())
                    cmbModelCategory.Items.Add((string)oRd["ModelCategory"]);

            //Now, retrieve our collection of models
            using (OracleCommand oCmd = new OracleCommand("SELECT ID, ValidStart, FullClass, FullPath, ModelClass, ModelCategory, Name from MM_DATABASE_MODEL", oConn))
            using (OracleDataReader oRd = oCmd.ExecuteReader())
            while (oRd.Read())
                Models.Add(new MM_Database_Model(oRd,true));

        }
        #endregion

        #region Model storage internals
        /// <summary>
        /// (C) 2012, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc.
        /// This class provides an internal representation of a network model
        /// </summary>
        private class MM_Database_Model : MM_Serializable, IComparable<MM_Database_Model>
        {
            /// <summary>The ID of our model</summary>
            public int ID=-1;

            /// <summary>The valid start date of our model</summary>
            public DateTime ValidStart = DateTime.FromBinary(0);

            /// <summary>The full path to our model</summary>
            public String FullPath = "";
            
            /// <summary>The category of our model</summary>
            public String ModelCategory = "";

            /// <summary>The name of our model</summary>
            public String Name = "";

            /// <summary>The class of the model</summary>
            public String ModelClass = "";

            /// <summary>The full class of our model</summary>
            public String FullClass = "";

            /// <summary>
            /// Initialize our new model
            /// </summary>
            /// <param name="oRd"></param>
            /// <param name="AddIfNew"></param>
            public MM_Database_Model(OracleDataReader oRd, bool AddIfNew)
                : base(oRd, AddIfNew)
            {
            }

            public int CompareTo(MM_Database_Model other)
            {
                return -ID.CompareTo(ID);
            }
        }
        #endregion

        #region User interactions
        /// <summary>
        /// Update our list of models based on the category selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbModelCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActiveModels.Clear();
            cmbModelClass.Items.Clear();
            cmbModelClass.Text = "";
            cmbModel.Items.Clear();
            cmbModel.Text = "";

            SortedDictionary<int, MM_Database_Model> SortedModels = new SortedDictionary<int, MM_Database_Model>();
            foreach (MM_Database_Model Model in Models)
                if (Model.ModelCategory == (string)cmbModelCategory.SelectedItem || (string)cmbModelCategory.SelectedItem == "All")
                {
                    SortedModels.Add(-Model.ID, Model);
                    ActiveModels.Add(Model);
                }

            cmbModelClass.Items.Clear();
            foreach (MM_Database_Model Model in SortedModels.Values)
                if (!cmbModelClass.Items.Contains(Model.ModelClass))
                    cmbModelClass.Items.Add(Model.ModelClass);


            if (!Updating)
                UpdateDisplays();
        }
        /// <summary>
        /// Update our selection of models based on the selected category
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbModelClass_SelectedIndexChanged(object sender, EventArgs e)
        {

            ActiveModels.Clear();
            cmbModel.Items.Clear();
            cmbModel.Text = "";
            SortedDictionary<int, String> SortedModels = new SortedDictionary<int, String>();
            foreach (MM_Database_Model Mod in Models)
                if ((Mod.ModelCategory == (string)cmbModelCategory.SelectedItem || (string)cmbModelCategory.Text == "All") && Mod.ModelClass == (string)cmbModelClass.SelectedItem)
                {
                    ActiveModels.Add(Mod);
                    SortedModels.Add(-Mod.ID, Mod.FullClass);

                }
            cmbModel.Items.AddRange(SortedModels.Values.ToArray());


            if (!Updating)
                UpdateDisplays();
        }

        /// <summary>
        /// Handle the selection of our model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedModel = null;
            foreach (MM_Database_Model Mod in Models)
                if (Mod.FullClass == (string)cmbModel.SelectedItem)
                {
                    SelectedModel = Mod;
                    break;
                }
        }
        /// <summary>
        /// Update our displays based on our current collection of active models
        /// </summary>
        public void UpdateDisplays()
        {
            if (Updating)
                return;
            Updating = true;
            List<DateTime> SelDates = new List<DateTime>();
            foreach (MM_Database_Model Mod in ActiveModels)// Repository.Model_Collection.Values)
                if (!SelDates.Contains(Mod.ValidStart))
                    SelDates.Add(Mod.ValidStart);
            SelDates.Sort();

            //Adjust our date boundaries to fit
            if (SelDates.Count > 0)
            {
                if (calModel.MinDate > SelDates[SelDates.Count - 1])
                    calModel.MinDate = SelDates[SelDates.Count - 1];
                calModel.MaxDate = SelDates[SelDates.Count - 1];
                calModel.MinDate = SelDates[0];
                calModel.SelectionStart = calModel.SelectionEnd = SelDates[SelDates.Count - 1];
                calModel.BoldedDates = SelDates.ToArray();
            }

            Updating = false;
        }

        /// <summary>
        /// Update our list of options available when a user clicks on a date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calModel_DateChanged(object sender, DateRangeEventArgs e)
        {
            if (Updating)
                return;
            Updating = true;
            MM_Database_Model SelModel = null;
            DateTime MinDate = DateTime.MinValue;
            foreach (MM_Database_Model Mod in Models)
                if (Mod.ValidStart <= e.Start.Date && Mod.ValidStart > MinDate)
                {
                    SelModel = Mod;
                    MinDate = Mod.ValidStart;
                }
            if (SelModel != null)
            {
                cmbModelCategory.SelectedItem = SelModel.ModelCategory;
                cmbModelClass.SelectedItem = SelModel.ModelClass;
                cmbModel.SelectedItem = SelModel.FullClass;
                SelectedModel = SelModel;
            }
            Updating = false;
        }

        /// <summary>
        /// Handle the ok button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (SelectedModel == null)
                MessageBox.Show("Please select a model.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            else
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// Handle the cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            SelectedModel = null;
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        #endregion
    }
}