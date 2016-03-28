using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.NetworkMap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Blackstart
{
    public partial class MM_LoadGen_Tracking : MM_Form
    {
        #region Variable declarations
        /// <summary>The network map driving our display</summary>
        public MM_Network_Map_DX NetworkMap;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new load/gen tracker
        /// </summary>
        /// <param name="NetworkMap"></param>
        public MM_LoadGen_Tracking(MM_Network_Map_DX NetworkMap): this()
        {
            this.NetworkMap = NetworkMap;
            this.Title = Text;
        }

        /// <summary>
        /// Initialize a new empty load tracker
        /// </summary>
        public MM_LoadGen_Tracking()
        {
            InitializeComponent();
            flwMain.AutoScroll = true;

            //Initilaize our types
            AddType(MM_Repository.FindElementType("Load"), typeof(MM_Load));
            AddType(MM_Repository.FindElementType("LaaR"), typeof(MM_Load));
            AddType(MM_Repository.FindElementType("Capacitor"), typeof(MM_ShuntCompensator));
            AddType(MM_Repository.FindElementType("Reactor"), typeof(MM_ShuntCompensator));
            AddType(MM_Repository.FindElementType("SVC"), typeof(MM_StaticVarCompensator));
            AddType(MM_Repository.FindElementType("Line"), typeof(MM_Line));
            AddType(MM_Repository.FindElementType("Unit"), typeof(MM_Unit));

        }

        /// <summary>
        /// Add in our target types
        /// </summary>
        /// <param name="ElemType"></param>
        /// <param name="TargetType"></param>
        private void AddType(MM_Element_Type ElemType, Type TargetType)
        {
            ToolStripMenuItem OpMenu = (ToolStripMenuItem)byOperatorToolStripMenuItem.DropDownItems.Add(ElemType.Name);
            ToolStripMenuItem CtyMenu = (ToolStripMenuItem)byCountyToolStripMenuItem.DropDownItems.Add(ElemType.Name);
            OpMenu.DropDownItemClicked += btnTrendHistory_DropDownItemClicked;
            CtyMenu.DropDownItemClicked += btnTrendHistory_DropDownItemClicked;
            OpMenu.Tag = ElemType;
            CtyMenu.Tag = ElemType;

            SortedDictionary<String, MemberInfo> Members = new SortedDictionary<string, MemberInfo>();
            foreach (MemberInfo mI in TargetType.GetMembers(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public))
                if ((mI is FieldInfo && ((FieldInfo)mI).FieldType == typeof(float)) || (mI is PropertyInfo && ((PropertyInfo)mI).PropertyType == typeof(float)))
                     Members.Add(mI.Name, mI);
            foreach (KeyValuePair<String, MemberInfo> kvp in Members)
            {
                OpMenu.DropDownItems.Add(kvp.Key).Tag = kvp.Value;
                CtyMenu.DropDownItems.Add(kvp.Key).Tag = kvp.Value;
            }
        }
        #endregion

        /// <summary>
        /// Handle our drop-down item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTrendHistory_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Tag == null || e.ClickedItem.Tag is MemberInfo == false)
                return;
            MM_Element_Type ElemType = (MM_Element_Type)(((ToolStripItem)sender).Tag);
            MemberInfo mI = (MemberInfo)e.ClickedItem.Tag;

            btnTrendHistory.Text = ElemType.Name + " " + mI.Name + " " + e.ClickedItem.OwnerItem.OwnerItem.Text;


            flwMain.Controls.Clear();
            MM_LoadGen_Tracking_Operator FoundOperator;
            SortedDictionary<String, MM_LoadGen_Tracking_Operator> Trackers = new SortedDictionary<string, MM_LoadGen_Tracking_Operator>();
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem.ElemType==ElemType)
                {
                    //Determine our key
                    String Key="";
                    if (e.ClickedItem.OwnerItem.OwnerItem == byOperatorToolStripMenuItem)
                        Key = Elem.Operator.Alias;
                    else if (e.ClickedItem.OwnerItem.OwnerItem == byCountyToolStripMenuItem)
                        if (Elem.Substation == null || Elem.Substation.County == null)
                            Key = "Unknown";
                        else
                            Key = Elem.Substation.County.Name;


                    if (!Trackers.TryGetValue(Key, out FoundOperator))
                        Trackers.Add(Key, FoundOperator = new MM_LoadGen_Tracking_Operator(Key, ElemType, mI));
                    FoundOperator.Elements.Add(Elem);
                }
            foreach (MM_LoadGen_Tracking_Operator Oper in Trackers.Values)
                Oper.BeginTracking();
            flwMain.Controls.AddRange(Trackers.Values.ToArray());
        }

        /// <summary>
        /// Hide our trend display when it comes up
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            this.Hide();
            base.OnShown(e);
        }

        /// <summary>
        /// Create a seperate thread to run the communications viewer, and run it.
        /// </summary>
        /// <param name="nMap"></param>
        /// <param name="MenuItem"></param>
        /// <returns></returns>
        public static void CreateInstanceInSeparateThread(ToolStripMenuItem MenuItem, MM_Network_Map_DX nMap)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(InstantiateForm), new object[] { MenuItem, nMap });
        }

        /// <summary>
        /// Instantiate a comm viewer
        /// </summary>
        /// <param name="state">The state of the form</param>
        private static void InstantiateForm(object state)
        {
            object[] State = (object[])state;
            using (MM_LoadGen_Tracking oDisp = new MM_LoadGen_Tracking(State[1] as MM_Network_Map_DX))
            {
                (State[0] as ToolStripMenuItem).Tag = oDisp;
                Data_Integration.DisplayForm(oDisp, Thread.CurrentThread);
            }
        }

       

    }
}
