using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;

namespace MacomberMapClient.User_Interfaces.External
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides a user interface for exporting to Excel
    /// </summary>
    public partial class MM_Excel_Exporter : MM_Form
    {
        #region Variable declarations
        /// <summary>The thread to be used for data export</summary>
        private Thread ExportThread;

        /// <summary>The data set to be exported</summary>
        private DataSet OutSet;

        /// <summary>The output file name</summary>
        private String FileName;

        /// <summary>Tracking of the current row</summary>
        private int CurrentRow = 0;

        /// <summary>The update timer</summary>
        private System.Windows.Forms.Timer UpdateTimer = new System.Windows.Forms.Timer();
        #endregion

        #region Initialization
        /// <summary>
        /// Create a new Excel exporter window
        /// </summary>
        /// <param name="OutSet">The outgoing data set</param>
        /// <param name="FileName">The outgoing filename</param>
        public MM_Excel_Exporter(DataSet OutSet, String FileName)
        {
            this.OutSet = OutSet;
            this.FileName = FileName;
            InitializeComponent();

            //Set our progress bar with the # of rows
            foreach (DataTable tbl in OutSet.Tables)
                pbToExcel.Maximum += tbl.Rows.Count;
        }

        /// <summary>
        /// When the progress bar is shown, kick off the process.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {

            //Now, also set up our timer to show our progress
            UpdateTimer = new System.Windows.Forms.Timer();
            UpdateTimer.Interval = 1000;
            UpdateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            UpdateTimer.Start();

            ExportThread = new Thread(CompleteExcelExport);
            ExportThread.Name = "Excel export: " + Path.GetFileName(FileName);
            ExportThread.SetApartmentState(ApartmentState.STA);
            ExportThread.Start();
        }

        /// <summary>
        /// Handle the timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            pbToExcel.Value = CurrentRow;
            if (ExportThread.ThreadState == System.Threading.ThreadState.Stopped)
            {
                UpdateTimer.Stop();
                try
                {
                    Process.Start(FileName);
                }
                catch (Exception ex)
                {
                    MM_System_Interfaces.MessageBox("The Excel file has been exported, but could not be opened: " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                this.Close();
            }
        }

        /// <summary>
        /// This is the full export function.
        /// </summary>
        public void CompleteExcelExport()
        {
            if (File.Exists(FileName))
                File.Delete(FileName);


            try
            {
                using (ExcelPackage pck = new ExcelPackage(new FileInfo(FileName)))
                {
                    int tableIx = 0;
                    foreach (DataTable dataTable in OutSet.Tables)
                    {
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add(!string.IsNullOrWhiteSpace(dataTable.TableName) ? dataTable.TableName : "Sheet" + tableIx++);
                        ws.Cells["A1"].LoadFromDataTable(dataTable, true);

                    }
                    pck.Save();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Problem writing Excel file.\n" + ex.Message);
                return;
            }
        }
        #endregion
    }
}