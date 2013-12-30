using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// (C) 2013, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides a keyboard-based search helper to assist with navigating through the map
    /// </summary>
    public class SearchResultsHelper: ListView
    {
        #region Variable declarations
        /// <summary>
        /// Our collection of search elements
        /// </summary>
        public SortedDictionary<String, MM_Element[]> Elements = new SortedDictionary<string, MM_Element[]>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Our event handler for the selection of a search option
        /// </summary>
        public event EventHandler<ListViewItemSelectionChangedEventArgs> SearchOptionSelected;

        /// <summary>Our main map</summary>
        public MacomberMap MainMap;

        /// <summary>Our popup menu</summary>
        private MM_Popup_Menu PopupMenu = new MM_Popup_Menu();
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our search results helper
        /// </summary>
        public SearchResultsHelper(MacomberMap MainMap)
        {
            Columns.Add("T");
            Columns.Add("Description");
            View = View.Details;
            FullRowSelect = true;
            HoverSelection = true;
            HideSelection = false;
            this.MainMap = MainMap;

            //Add in our voltage levels
            foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                AddElement(KVLevel.Name.Replace(" ", ""), KVLevel);

            //Add in our elements and all the keywords associated with them (at present, sub, line, unit, boundary, company)
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (!String.IsNullOrEmpty(Elem.Name))
                    if (Elem is MM_Substation)
                    {
                        AddElement((Elem as MM_Substation).LongName, Elem);
                        AddElement(Elem.Name, Elem);
                    }
                    else if (Elem is MM_Line)
                        AddElement(Elem.Name, Elem);
                    else if (Elem is MM_Unit)
                        AddElement(Elem.Substation.Name + "." + Elem.Name, Elem);
                    else if (Elem is MM_Boundary)
                        AddElement(Elem.Name, Elem);
        }

        /// <summary>
        /// Add an element to our collection list
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Elem"></param>
        private void AddElement(String Title, MM_Element Elem)
        {
            foreach (String Word in Title.Split(new char[] { ' ', '(', ')', '_', '#' }, StringSplitOptions.RemoveEmptyEntries))
                if (!String.IsNullOrEmpty(Word))
                    if (!Elements.ContainsKey(Word))
                        Elements.Add(Word, new MM_Element[] { Elem });
                    else if (!Elements[Word].Contains(Elem))
                    {
                        List<MM_Element> Elems = new List<MM_Element>(Elements[Word]);
                        Elems.Add(Elem);
                        Elements[Word] = Elems.ToArray();
                    }
        }
        #endregion

        #region Search text updating
        /// <summary>
        /// Update our search text
        /// </summary>
        /// <param name="CurrentText"></param>
        public void UpdateSearchText(String CurrentText)
        {
            Items.Clear();
            String[] Words = CurrentText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            //Go through and pull in our list of accepted TEIDs that match all of our words
            Dictionary<MM_Element, int> AcceptedTEIDs = new Dictionary<MM_Element, int>();
            int TargetValue = 0;
            for (int a = 0; a < Words.Length; a++)
            {
                TargetValue |= 2 ^ a;
                foreach (KeyValuePair<String, MM_Element[]> kvp in Elements)
                    if (kvp.Key.StartsWith(Words[a], StringComparison.CurrentCultureIgnoreCase))
                        foreach (MM_Element Elem in kvp.Value)
                            if (!AcceptedTEIDs.ContainsKey(Elem))
                                AcceptedTEIDs.Add(Elem, 2 ^ a);
                            else
                                AcceptedTEIDs[Elem] |= 2 ^ a;
            }

            //Now, based on all the elements that match our total number of words, add them
            SortedDictionary<String, ListViewItem> OutItems = new SortedDictionary<string, ListViewItem>();
            foreach (KeyValuePair<MM_Element, int> kvp in AcceptedTEIDs)
                if (kvp.Value == TargetValue)
                {
                    ListViewItem lvI = new ListViewItem(kvp.Key.GetType().Name.Substring(3,1));
                    if (kvp.Key is MM_Substation)
                        lvI.SubItems.Add((kvp.Key as MM_Substation).LongName + " (" + kvp.Key.Name + ")");
                    else if (kvp.Key is MM_Company)
                        lvI.SubItems.Add((kvp.Key as MM_Company).Alias + " - " + kvp.Key.Name);
                    else
                        lvI.SubItems.Add(kvp.Key.Name);
                    lvI.Tag = kvp.Key;
                    if (!OutItems.ContainsKey(lvI.SubItems[1].Text))
                        OutItems.Add(lvI.SubItems[1].Text, lvI);
                }

            Items.AddRange(OutItems.Values.ToArray());
            this.Scrollable = false;
            this.Height = 20 + ((this.Font.Height + 2) * (Items.Count+1));
        }
        
        #endregion

        /// <summary>
        /// Handle our mouse click event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            ListViewHitTestInfo hti = HitTest(e.Location);
            if (hti.Item != null)
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    PopupMenu.Show(this, e.Location, hti.Item.Tag as MM_Element, true);
                    this.Hide();
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Left && SearchOptionSelected != null)
                    SearchOptionSelected(this, new ListViewItemSelectionChangedEventArgs(hti.Item, hti.Item.Index, true));
            base.OnMouseClick(e);
        }
    }
}
