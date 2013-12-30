using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Data.OleDb;
using System.Diagnostics;
using Macomber_Map.Data_Elements;
using System.Reflection;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
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
                    Program.MessageBox( "The Excel file has been exported, but could not be opened: " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            OleDbConnection oConn;
            if (Path.GetExtension(FileName) == ".xls")
                oConn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FileName + ";Extended Properties=\"Excel 8.0;HDR=YES;\"");
            else
                oConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=\"Excel 12.0;HDR=YES;\"");
            using (oConn)
            {
                oConn.Open();
                List<String> Tables = new List<string>();
                foreach (DataTable tbl in OutSet.Tables)
                    Tables.Add(tbl.TableName);
                Tables.Sort();

                foreach (String TblName in Tables)
                {
                    //Add our new table in with its columns
                    DataTable tbl = OutSet.Tables[TblName];
                    StringBuilder TableCreator = new StringBuilder("CREATE TABLE [" + tbl.TableName + "]");
                    foreach (DataColumn col in tbl.Columns)
                    {
                        if (col.Ordinal == 0)
                            TableCreator.Append("(");
                        else
                            TableCreator.Append(", ");

                        if (col.DataType == typeof(int))
                            TableCreator.Append("[" + col.ColumnName + "] int");
                        else if (col.DataType == typeof(float))
                            TableCreator.Append("[" + col.ColumnName.Replace('.', '-').Replace('[', '(').Replace(']', ')') + "] float");
                        else if (col.DataType == typeof(double))
                            TableCreator.Append("[" + col.ColumnName.Replace('.', '-').Replace('[', '(').Replace(']', ')') + "] double");
                        else if (col.DataType == typeof(decimal))
                            TableCreator.Append("[" + col.ColumnName.Replace('.', '-').Replace('[', '(').Replace(']', ')') + "] decimal");
                        else if (col.DataType == typeof(Int32))
                            TableCreator.Append("[" + col.ColumnName.Replace('.', '-').Replace('[', '(').Replace(']', ')') + "] long");
                        else if (col.DataType == typeof(DateTime))
                            TableCreator.Append("[" + col.ColumnName.Replace('.', '-').Replace('[', '(').Replace(']', ')') + "] DateTime");
                        else if (col.DataType == typeof(bool))
                            TableCreator.Append("[" + col.ColumnName.Replace('.', '-').Replace('[', '(').Replace(']', ')') + "] String");
                        else
                            TableCreator.Append("[" + col.ColumnName.Replace('.', '-').Replace('[', '(').Replace(']', ')') + "] String");
                    }
                    OleDbCommand oCmd = new OleDbCommand(TableCreator.ToString() + ")", oConn);
                    oCmd.ExecuteNonQuery();


                    //Now, build our dictionary of outgoing values                    
                    List<OleDbParameter> SystemWideram = new List<OleDbParameter>();
                    foreach (DataRow dr in tbl.Rows)                    
                    {
                        CurrentRow++;
                        SystemWideram.Clear();
                        int CurParam = 1;
                        StringBuilder OutStr1 = new StringBuilder();
                        StringBuilder OutStr2 = new StringBuilder();
                        foreach (DataColumn dCol in tbl.Columns)
                            if (dr[dCol] is DBNull == false)
                            {      
                                OutStr1.Append((CurParam == 1 ? "INSERT INTO [" + tbl.TableName + "] ([" : ", [") + dCol.ColumnName.Replace('.', '-').Replace('[', '(').Replace(']', ')') + "]");
                                OutStr2.Append((CurParam == 1 ? ") VALUES (" : ", ") + ":" + CurParam.ToString());
                                if (dr[dCol] is float && float.IsNaN((float)dr[dCol]))
                                    SystemWideram.Add(new OleDbParameter(":" + CurParam.ToString(), DBNull.Value));
                                else if (dr[dCol] == null || dr[dCol] is DBNull)
                                    SystemWideram.Add(new OleDbParameter(":" + CurParam.ToString(), DBNull.Value));
                                else if (dr[dCol] is bool)
                                    SystemWideram.Add(new OleDbParameter(":" + CurParam.ToString(), dr[dCol].ToString()));
                                else if (dr[dCol] is MM_Serializable)
                                {
                                    MemberInfo[] mI = dr[dCol].GetType().GetMember("Name");
                                    Object InObj = null;
                                    if (mI[0] is FieldInfo)
                                         InObj = (mI[0] as FieldInfo).GetValue(dr[dCol]);
                                    else if (mI[0] is PropertyInfo)
                                         InObj = (mI[0] as PropertyInfo).GetValue(dr[dCol],null);
                                    if (InObj != null)
                                        SystemWideram.Add(new OleDbParameter(":" + CurParam.ToString(),InObj));
                                    else
                                        SystemWideram.Add(new OleDbParameter(":" + CurParam.ToString(),"?"));
                                }
                                else if (dr[dCol] is String == false && dr[dCol] is System.Collections.IEnumerable)
                                {
                                    StringBuilder OutStr = new StringBuilder();
                                    foreach (Object obj in (System.Collections.IEnumerable)dr[dCol])
                                        if (obj is PointF)
                                            OutStr.Append((OutStr.Length == 0 ? "" : ",") + String.Format("{0},{1}", ((PointF)obj).X, ((PointF)obj).Y));
                                        else if (obj == null)
                                            OutStr.Append(OutStr.Length == 0 ? "?" : ",?");
                                        else if (obj is MM_Serializable)
                                        {
                                            Object InObj = null;
                                            MemberInfo[] mI = obj.GetType().GetMember("Name");
                                            if (mI[0] is FieldInfo)
                                                InObj = (mI[0] as FieldInfo).GetValue(obj);
                                            else if (mI[0] is PropertyInfo)
                                                InObj = (mI[0] as PropertyInfo).GetValue(obj, null);
                                            if (InObj == null)
                                                OutStr.Append(OutStr.Length == 0 ? "?" : ",?");
                                            else
                                                OutStr.Append(OutStr.Length == 0 ? InObj.ToString() : "," + InObj.ToString());                                            
                                        }
                                        else
                                            OutStr.Append((OutStr.Length == 0 ? "" : ",") + obj.ToString());
                                    SystemWideram.Add(new OleDbParameter(":" + CurParam.ToString(), OutStr.ToString()));
                                }
                                else
                                    SystemWideram.Add(new OleDbParameter(":" + CurParam.ToString(), dr[dCol]));
                                CurParam++;
                            }

                        //Now, execute our query                        
                        using (OleDbCommand oCmd2 = new OleDbCommand(OutStr1.ToString() + OutStr2.ToString() + ")", oConn))
                        {
                            oCmd2.Prepare();
                            oCmd2.Parameters.AddRange(SystemWideram.ToArray());
                            try
                            {                              
                                oCmd2.ExecuteNonQuery();

                            }
                            catch (Exception ex)
                            {
                                Program.LogError("Error exporting {0} row", ex, tbl.TableName);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}