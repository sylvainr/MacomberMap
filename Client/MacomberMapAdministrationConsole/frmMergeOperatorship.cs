using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapAdministrationConsole
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the handling of merging operatorship information
    /// </summary>
    public partial class frmMergeOperatorship : Form
    {
        #region Variable declarations
        /// <summary>Our collection of areas and their names</summary>
        Dictionary<int, String> Areas = new Dictionary<int, string>();

        /// <summary>Our collection of users and their names</summary>
        Dictionary<int, String> UserTypeNames = new Dictionary<int, string>();

        /// <summary>Our collection of users and their names</summary>
        Dictionary<int, String> UserNames = new Dictionary<int, string>();

        /// <summary>Our collection of modes and their names</summary>
        Dictionary<int, String> Modes = new Dictionary<int, string>();


        /// <summary>Our collection of permissions</summary>
        Dictionary<int[], bool> Permissions = new Dictionary<int[], bool>();

        /// <summary>Our collection of current permissions</summary>
        SortedDictionary<String, String[]> OldPermissions = new SortedDictionary<string, string[]>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>Our collection of new permissions</summary>
        SortedDictionary<String, String[]> NewPermissions = new SortedDictionary<string, string[]>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>Our user type to mode linkages</summary>
        Dictionary<String, List<int>> UserTypeToModeLinkages = new Dictionary<String, List<int>>();

        #endregion

        #region Initiailzation
        /// <summary>
        /// Initialize our form
        /// </summary>
        public frmMergeOperatorship()
        {
            InitializeComponent();
        }
        #endregion

        #region Drag/drop handling
        /// <summary>
        /// Handle the drag over event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbl_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) && ((string[])e.Data.GetData(DataFormats.FileDrop)).Length==1? DragDropEffects.All : DragDropEffects.None;
        }

        /// <summary>
        /// Handle the drag/drop event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbl_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop) || ((string[])e.Data.GetData(DataFormats.FileDrop)).Length != 1)
                return;
            string FileName="";
            try
            {
                FileName = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (sender == lblDropMode)
                    ProcessFile<int, String>(Modes, FileName, "ID_MODE(", ")", "= ", "", lblCountModes, "modes");
                else if (sender == lblDropUserTypeFile)
                    ProcessFile<int, String>(UserTypeNames, FileName, "USER(", ")", "= ", "", lblCountUserType, "usertypes");
                else if (sender == lblDropUserName)
                    ProcessFile<int, String>(UserNames, FileName, "USER(", ")", "= ", "", lblCountUserNames, "usernames");
                else if (sender == lblDropArea)
                    ProcessFile<int, String>(Areas, FileName, "ID_AREA(", ")", "= ", "", lblCountAreas, "areas");
                else if (sender == lblDropPermission)
                    ProcessFile<int[], bool>(Permissions, FileName, "_AREA_MODE(", ")", "= ", "", lblCountPermissions, "permissions");
                else if (sender == lblDropOldOperatorships)
                    ProcessFile(OldPermissions, FileName, lblCountAreas, "operatorships");
                else if (sender == lblDropUserTypeToModeLinkages)
                {
                    String InLine;
                    List<int> MappedTypes = null;
                    UserTypeToModeLinkages.Clear();
                    using (StreamReader sRd = new StreamReader(FileName))
                        while ((InLine = sRd.ReadLine()) != null)
                            if (InLine.StartsWith("USRTYP,"))
                            {
                                int StartStr = InLine.IndexOf("ID='") + 4;
                                UserTypeToModeLinkages.Add(InLine.Substring(StartStr, InLine.IndexOf("'", StartStr + 1) - StartStr), MappedTypes = new List<int>());
                            }
                            else if (InLine.StartsWith("UMODE,"))
                                MappedTypes.Add(int.Parse(InLine.Substring(InLine.LastIndexOf('=')+1)));                                    

                    foreach (List<int> MappedType in UserTypeToModeLinkages.Values)
                        MappedType.TrimExcess();
                    lblCountUserTypeToModeLinkages.Text = UserTypeToModeLinkages.Count.ToString() + " user->mode";
                }

                //Now, if we have enough data, produce our list
                if (Permissions.Count > 0 && Areas.Count > 0 && UserNames.Count > 0 && UserTypeNames.Count > 0 && Modes.Count > 0 && UserTypeToModeLinkages.Count > 0)
                {
                    NewPermissions.Clear();

                    //Now, perform our merging
                    List<String> MissingUsers = new List<String>();
                    List<int> MissingAreas = new List<int>();
                    Dictionary<String, String> UserNameToUserType = new Dictionary<string, string>();

                    List<int> UserTypeToModeLinkage;
                    foreach (KeyValuePair<int, string> User in UserNames)
                        if (!UserTypeToModeLinkages.TryGetValue(UserTypeNames[User.Key], out UserTypeToModeLinkage))
                            MissingUsers.Add(UserTypeNames[User.Key]);
                        else
                        {
                            UserNameToUserType.Add(User.Value, UserTypeNames[User.Key]);
                            List<String> OutAreas = new List<string>();
                            String FoundArea;
                            foreach (KeyValuePair<int[], bool> kvp in Permissions)
                                if (kvp.Value && UserTypeToModeLinkage.Contains(kvp.Key[1]))
                                    if (!Areas.TryGetValue(kvp.Key[0], out FoundArea))
                                        MissingAreas.Add(kvp.Key[0]);
                                    else if (!OutAreas.Contains(FoundArea))
                                        OutAreas.Add(FoundArea);
                            OutAreas.Sort(StringComparer.CurrentCultureIgnoreCase);
                            if (OutAreas.Count > 0)
                                NewPermissions.Add(User.Value, OutAreas.ToArray());

                        }


                    //Now, clear our ListView
                    lvOutputs.Items.Clear();
                    if (OldPermissions.Count > 0)
                    {
                        String[] FoundPermissions;
                        foreach (KeyValuePair<String, String[]> kvp in NewPermissions)
                            if (!OldPermissions.TryGetValue(kvp.Key, out FoundPermissions))
                            {
                                ListViewItem lvI = lvOutputs.Items.Add(kvp.Key);
                                lvI.SubItems.Add(UserNameToUserType[kvp.Key]);
                                lvI.SubItems.Add("Yes");
                                lvI.SubItems.Add(String.Join(",", kvp.Value));
                                lvI.SubItems.Add("");
                            }
                            else
                            {


                                List<String> Additions = new List<string>();
                                List<String> Removals = new List<string>();
                                foreach (String str in FoundPermissions)
                                    if (Array.IndexOf(kvp.Value, str) == -1)
                                        Removals.Add(str);
                                foreach (String str in kvp.Value)
                                    if (Array.IndexOf(FoundPermissions, str) == -1)
                                        Additions.Add(str);
                                if (Additions.Count + Removals.Count > 0)
                                {
                                    ListViewItem lvI = lvOutputs.Items.Add(kvp.Key);
                                    lvI.SubItems.Add(UserNameToUserType[kvp.Key]);
                                    lvI.SubItems.Add("");
                                    lvI.SubItems.Add(String.Join(",", Additions.ToArray()));
                                    lvI.SubItems.Add(String.Join(",", Removals.ToArray()));
                                }
                            }

                        //Now, pull in our old ones that are missing
                        foreach (KeyValuePair<String, String[]> kvp in OldPermissions)
                            if (!NewPermissions.ContainsKey(kvp.Key))
                            {
                                ListViewItem lvI = lvOutputs.Items.Add(kvp.Key);
                                lvI.SubItems.Add("Old");
                                lvI.SubItems.Add(String.Join(",", kvp.Value.ToArray()));
                                lvI.SubItems.Add("");

                            }
                    }
                    else
                        foreach (KeyValuePair<String, String[]> kvp in NewPermissions)
                        {
                            ListViewItem lvI = lvOutputs.Items.Add(kvp.Key);
                            lvI.SubItems.Add(UserNameToUserType[kvp.Key]);

                            lvI.SubItems.Add("Yes");
                            lvI.SubItems.Add(String.Join(",", kvp.Value));
                            lvI.SubItems.Add("");
                        }

                    lvOutputs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    lvOutputs.Sort();
                    if (MissingAreas.Count > 0)
                        MessageBox.Show("Missing " + MissingAreas.Count.ToString() + " areas: " + String.Join(", ", MissingAreas.ToArray()));


                    if (MissingUsers.Count > 0)
                        MessageBox.Show("Missing " + MissingUsers.Count.ToString() + " users: " + String.Join(", ", MissingUsers.ToArray()));

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading file " + FileName + ":" + ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Process a file for operatorships
        /// </summary>
        /// <param name="OutDictionary"></param>
        /// <param name="FileName"></param>
        /// <param name="CountLabel"></param>
        /// <param name="DataType"></param>
        private void ProcessFile(SortedDictionary<String, String[]> OutDictionary, String FileName, Label CountLabel, String DataType)
        {
            String InLine;
            OutDictionary.Clear();
            using (StreamReader sRd = new StreamReader(FileName))
                while ((InLine = sRd.ReadLine()) != null)
                    if (InLine.Contains(','))
                    {
                        String Key = InLine.Substring(0, InLine.IndexOf(','));
                        List<String> Values = new List<String>(InLine.Substring(InLine.IndexOf(',')+1).Split(','));
                        Values.Sort(StringComparer.CurrentCultureIgnoreCase);                        
                        OutDictionary.Add(Key, Values.ToArray());
                    }
            CountLabel.Text = OutDictionary.Count.ToString("#,##0") + " " + DataType;
        }


        /// <summary>
        /// Process a file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="OutDictionary"></param>
        /// <param name="FileName"></param>
        /// <param name="KeyStartFlag"></param>
        /// <param name="KeyStopFlag"></param>
        /// <param name="ValueStartFlag"></param>
        /// <param name="ValueStopFlag"></param>
        /// <param name="CountLabel"></param>
        /// <param name="DataType"></param>        
        private void ProcessFile<T,U>(Dictionary<T,U> OutDictionary, String FileName, String KeyStartFlag, String KeyStopFlag, String ValueStartFlag,String ValueStopFlag, Label CountLabel, String DataType)
        {
            OutDictionary.Clear();
            String InLine;
            using (StreamReader sRd = new StreamReader(FileName))
                while ((InLine = sRd.ReadLine()) != null)
                {
                    int StartPoint = InLine.IndexOf(KeyStartFlag);
                    if (StartPoint != -1)
                    {
                        int EndPoint = InLine.IndexOf(KeyStopFlag, StartPoint + KeyStartFlag.Length);
                        String Key = InLine.Substring(StartPoint + KeyStartFlag.Length, EndPoint - StartPoint - KeyStartFlag.Length);
                        Object OutKey = typeof(T) == typeof(int[]) ? new int[] { int.Parse(Key.Split(',')[0]), int.Parse(Key.Split(',')[1]) } : Convert.ChangeType(Key, typeof(T));

                        StartPoint = InLine.IndexOf(ValueStartFlag);
                        EndPoint = String.IsNullOrEmpty(ValueStopFlag) ? InLine.Length : InLine.IndexOf(ValueStopFlag, StartPoint + ValueStartFlag.Length);
                        String Value = InLine.Substring(StartPoint + ValueStartFlag.Length, EndPoint - StartPoint - ValueStartFlag.Length);
                        U OutValue = (U)Convert.ChangeType(Value, typeof(U));

                        OutDictionary.Add((T)OutKey, OutValue);
                    }
                }
            CountLabel.Text = OutDictionary.Count.ToString("#,##0") + " " + DataType;
        }

        /// <summary>
        /// Handle our export button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportOperatorship_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sFd = new SaveFileDialog() { Title = "Save the operatorship list", Filter = "Login operatorships (LoginOperatorships.csv)|LoginOperatorships.csv", FileName = "LoginOperatorships.csv" })
                if (sFd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    using (StreamWriter sW = new StreamWriter(sFd.FileName, false, new UTF8Encoding(false)))
                    {
                        foreach (KeyValuePair<String, string[]> kvp in NewPermissions)
                            sW.WriteLine(kvp.Key + "," + String.Join(",", kvp.Value));
                        foreach (KeyValuePair<String, String[]> kvp in OldPermissions)
                            if (!NewPermissions.ContainsKey(kvp.Key))
                                sW.WriteLine(kvp.Key + "," + String.Join(",", kvp.Value));

                    }
        }
        #endregion
    }
}
