using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Physical;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapClient.User_Interfaces.Summary
{

    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the bases for an underlying data set for a control, including its saving, loading, updating, etc. 
    /// In order to allow the controls to function across a wide variety of controls, this class is assigned to the Tag element as an object 
    /// </summary>
    public class MM_DataSet_Base : IDisposable
    {

        #region Variable declaration
        /// <summary>The dataset containing information for the display component</summary>
        public DataSet Data;

        /// <summary>The base element (if any) driving the display</summary>
        public MM_Element BaseElement;

        /// <summary>Our collection of elements by TEID</summary>
        public Dictionary<Int32, DataRow> ElementsByTEID = new Dictionary<int, DataRow>();
        #endregion

        /// <summary>
        /// Initialize a new dataset base using the specified name for the window
        /// </summary>
        /// <param name="DisplayName"></param>
        public MM_DataSet_Base(String DisplayName)
        {
            this.Data = new DataSet(DisplayName);
        }


        /// <summary>
        /// Add a data element to the appropriate table
        /// </summary>
        /// <param name="Elem">The element to be added</param>
        public DataRow AddDataElement(MM_Element Elem)
        {
            try
            {

                //If we don't have a table with the proper name, add it in.
                DataTable FoundTable = Data.Tables[Elem.ElemType.Name];

                if (FoundTable == null)
                {
                    FoundTable = Data.Tables.Add(Elem.ElemType.Name);
                    FoundTable.PrimaryKey = new DataColumn[] { FoundTable.Columns.Add("TEID", typeof(MM_Element)) };
                }

                DataRow NewRow;
                if (!ElementsByTEID.TryGetValue(Elem.TEID, out NewRow))
                {
                    NewRow = FoundTable.NewRow();
                    NewRow["TEID"] = Elem;
                    FoundTable.Rows.Add(NewRow);
                    ElementsByTEID.Add(Elem.TEID, NewRow);
                }


                //Add in the substation column
                if (Elem is MM_Line)
                {
                    MM_Line ThisLine = (Elem as MM_Line);
                    UpdateRowData(NewRow, "From", typeof(String), ThisLine.ConnectedStations[0].DisplayName());
                    UpdateRowData(NewRow, "To", typeof(String), ThisLine.ConnectedStations[1].DisplayName());
                }
                else if (Elem.Substation != null)
                    UpdateRowData(NewRow, "Substation", typeof(String), Elem.Substation.DisplayName());

                //Update the name column
                if (Elem is MM_Substation)
                    UpdateRowData(NewRow, "Name", typeof(String), (Elem as MM_Substation).DisplayName());
                else
                    UpdateRowData(NewRow, "Name", typeof(String), Elem.Name);

                //Update voltage, if possible
                if (Elem is MM_Substation)
                    UpdateRowData(NewRow, "KV Levels", typeof(String), (Elem as MM_Substation).KVLevelList);
                else if (Elem is MM_Transformer)
                    UpdateRowData(NewRow, "KV Levels", typeof(String), (Elem as MM_Transformer).KVLevelList);
                else if (Elem.KVLevel != null)
                    UpdateRowData(NewRow, "KV Level", typeof(String), Elem.KVLevel.Name.Split(' ')[0]);

                RefreshData(NewRow, Elem);
                return NewRow;
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        /// Refresh the data for a data row
        /// </summary>
        /// <param name="RowToUpdate">The row to be modified</param>
        /// <param name="Elem">The element associated with the row</param>
        private void RefreshData(DataRow RowToUpdate, MM_Element Elem)
        {
            //Now go through, and pull in the unique characteristics for each item            
            MemberInfo[] inMembers = Elem.GetType().GetMembers();//BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty);
            foreach (MemberInfo mI in inMembers)
                try
                {
                    //First, pull in our values
                    Type PropType = null;
                    Object PropValue = null;
                    if (mI is FieldInfo)
                    {
                        PropType = ((FieldInfo)mI).FieldType;
                        PropValue = ((FieldInfo)mI).GetValue(Elem);
                    }
                    else if (mI is PropertyInfo)
                    {
                        PropType = ((PropertyInfo)mI).PropertyType;
                        PropValue = ((PropertyInfo)mI).GetValue(Elem, null);
                    }


                    if (PropValue == null)
                    { }
                    else if (PropValue is PointF)
                    {
                        UpdateRowData(RowToUpdate, mI.Name + ".Latitude", typeof(Single), ((PointF)PropValue).Y);
                        UpdateRowData(RowToUpdate, mI.Name + ".Longitude", typeof(Single), ((PointF)PropValue).X);
                    }
                    else if (PropValue is MM_Substation)
                        UpdateRowData(RowToUpdate, mI.Name, typeof(String), (PropValue as MM_Substation).LongName);
                    else if (PropValue is MM_Element)
                        UpdateRowData(RowToUpdate, mI.Name, typeof(String), (PropValue as MM_Element).Name);
                    else if (PropValue is MM_DisplayParameter)
                        UpdateRowData(RowToUpdate, mI.Name, typeof(String), (PropValue as MM_DisplayParameter).Name);
                    else if (PropValue is Single[])
                    {
                        Single[] TSystemWiderse = (Single[])PropValue;
                        String[] Descriptor = new string[TSystemWiderse.Length];

                        if (TSystemWiderse.Length == 2)
                        {
                            Descriptor[0] = mI.Name + " [From]";
                            Descriptor[1] = mI.Name + " [To]";
                        }
                        else if (TSystemWiderse.Length == 3)
                        {
                            Descriptor[0] = mI.Name + " [Norm]";
                            Descriptor[1] = mI.Name + " [2 Hour]";
                            Descriptor[2] = mI.Name + " [15 Min]";
                        }
                        else
                            for (int a = 0; a < TSystemWiderse.Length; a++)
                                Descriptor[a] = mI.Name + " [" + a.ToString("#,##0") + "]";


                        for (int a = 0; a < TSystemWiderse.Length; a++)
                            UpdateRowData(RowToUpdate, Descriptor[a], typeof(Single), TSystemWiderse[a]);
                    }

                    else
                        UpdateRowData(RowToUpdate, mI.Name, PropType, PropValue);
                }
                catch { }
        }

        /// <summary>
        /// Update a row's data with specified parameters
        /// </summary>
        /// <param name="RowToUpdate">The data row to be updated</param>
        /// <param name="ColName">The name of the column</param>
        /// <param name="OutType">The outgoing type of the column</param>
        /// <param name="OutValue">The outgoing value of the column</param>
        private void UpdateRowData(DataRow RowToUpdate, String ColName, Type OutType, Object OutValue)
        {
            DataColumn Col = RowToUpdate.Table.Columns[ColName];
            if (Col == null)
            {                                    
                Col = RowToUpdate.Table.Columns.Add(ColName, OutType);
                if (ColName == "Open")
                    Col.SetOrdinal(3);
                else if (ColName=="Nominal_MVAR")
                    Col.SetOrdinal(3);
            }
            if (Col.DataType != typeof(MM_Element) && Col.DataType.BaseType != typeof(MM_Element))
                RowToUpdate[Col] = OutValue;

        }

        /// <summary>
        /// Clear our collection of elements
        /// </summary>
        public void Clear()
        {
            Data.Clear();
            ElementsByTEID.Clear();
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose of the data set base
        /// </summary>
        public void Dispose()
        {
            foreach (DataTable tbl in Data.Tables)
            {
                tbl.Clear();
                tbl.Dispose();
            }
            Data.Tables.Clear();
            Data.Dispose();
            BaseElement = null;
            ElementsByTEID.Clear();
        }

        #endregion
    }
}
