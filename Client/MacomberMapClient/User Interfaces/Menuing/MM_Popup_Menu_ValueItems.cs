using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacomberMapClient.User_Interfaces.Generic;

namespace MacomberMapClient.User_Interfaces.Menuing
{
    /// <summary>
    /// (C) 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved
    /// This class provides a holder for value items on an element, and autosizes them to fit in a standard menu
    /// </summary>
    public class MM_Popup_Menu_ValueItems
    {
        #region Variable declarations
        /// <summary>Whether to show estimated values</summary>
        public bool ShowEstimates = false;

        /// <summary>Whether to show telemetered values</summary>
        public bool ShowTelemetry = false;

        /// <summary>The name of our fields (e.g., MW, MVA)</summary>
        public Dictionary<String, Int32> Fields = new Dictionary<string, int>(5);

        /// <summary>The name of our headers (e.g., Stn1, Stn2)</summary>
        public Dictionary<String, Int32> Headers = new Dictionary<string, int>(3);

        /// <summary>Our collection of estimated values</summary>
        public Dictionary<Point, String> EstValues = new Dictionary<Point, string>(15);

        /// <summary>Our collection of telemetered values</summary>
        public Dictionary<Point, String> TelemValues = new Dictionary<Point, string>(15);

        /// <summary>Our MM parent menu instance</summary>
        public MM_Popup_Menu ParentMenu;

        /// <summary>The width of our space</summary>
        public int SpaceWidth;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new popup menu item collector
        /// </summary>
        /// <param name="ParentMenu"></param>
        public MM_Popup_Menu_ValueItems(MM_Popup_Menu ParentMenu)
        {
            this.ParentMenu = ParentMenu;
            this.SpaceWidth = TextRenderer.MeasureText(" ", ParentMenu.Font).Width;
        }

        /// <summary>
        /// Add our value in as necessary, and update our fields as present
        /// </summary>
        /// <param name="Field"></param>
        /// <param name="Header"></param>
        /// <param name="IsEst"></param>
        /// <param name="Value"></param>
        public void AddValue(String Field, String Header, bool IsEst, String Value)
        {
            try
            {
                int FieldID, HeaderID;
                if (!Fields.TryGetValue(Field, out FieldID))
                    Fields.Add(Field, FieldID = Fields.Count);
                if (!Headers.TryGetValue(Header, out HeaderID))
                    Headers.Add(Header, HeaderID = Headers.Count);
                if (IsEst)
                    EstValues.Add(new Point(HeaderID, FieldID), Value);
                else
                    TelemValues.Add(new Point(HeaderID, FieldID), Value);
            } catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
        }
        #endregion


        /// <summary>
        /// Add our menu items out to the display
        /// </summary>
        /// <param name="ItemCollection"></param>
        public void AddMenuItems(ToolStripItemCollection ItemCollection)
        {
            int EqualWidth = 0, FieldWidth = 0;

            //Measure the names of our substations
            Dictionary<String, String[]> NameReplacements = new Dictionary<string, string[]>();
            Random r = new Random();
            foreach (String str in Headers.Keys)
            {
                int ThisWidth = TextRenderer.MeasureText(" " + str + " ", ParentMenu.Font).Width;
                String OutStr = str;
                while (ThisWidth > 80)
                {
                    char[] OutChars = OutStr.ToCharArray();
                    List<int> Spaces = new List<int>();
                    for (int a = 0; a < OutChars.Length; a++)
                        if (OutChars[a] == ' ')
                            Spaces.Add(a);
                    if (Spaces.Count == 0)
                        break;
                    OutChars[Spaces[r.Next(0, Spaces.Count)]] = '\n';
                    OutStr = new string(OutChars);
                    ThisWidth = TextRenderer.MeasureText(OutStr, ParentMenu.Font).Width;
                }
                NameReplacements.Add(str, OutStr.Split('\n'));
                EqualWidth = Math.Max(EqualWidth, ThisWidth);
            }

            //Determine the names of our fields, and values
            foreach (String str in Fields.Keys)
                FieldWidth = Math.Max(FieldWidth, TextRenderer.MeasureText(str + ": ", ParentMenu.Font).Width);
            if (ShowEstimates) 
                foreach (String str in EstValues.Values)
                    EqualWidth = Math.Max(EqualWidth, TextRenderer.MeasureText(" " + str + " ", ParentMenu.Font).Width);
            if (ShowTelemetry)
                foreach (String str in TelemValues.Values)
                    EqualWidth = Math.Max(EqualWidth, TextRenderer.MeasureText(" " + str + " ", ParentMenu.Font).Width);

            //Write out our header first
            StringBuilder sB = new StringBuilder();



            //Pull in our strings for each header
            List<String[]> HeaderRows = new List<string[]>();
            int MaxLines = 0;
            foreach (string[] str in NameReplacements.Values)
            {
                HeaderRows.Add(str);
                MaxLines = Math.Max(MaxLines, str.Length);
            }

            //Now, add in our Est and Telem values
            sB.Append(GetString("", FieldWidth));
            if (ShowEstimates) // we might have estimates, but we just don't want the user to have a nice header unless some config is set somewhere.
                if (TextRenderer.MeasureText("Estimated", ParentMenu.Font).Width > EqualWidth * Headers.Count)
                    sB.Append(GetString("Est.", EqualWidth * Headers.Count));
                else
                    sB.Append(GetString("Estimated", EqualWidth * Headers.Count));
            if (ShowTelemetry)
                if (TextRenderer.MeasureText("Telemetered", ParentMenu.Font).Width > EqualWidth * Headers.Count)
                    sB.Append(GetString("Tel.", EqualWidth * Headers.Count));
                else
                    sB.Append(GetString("Telemetered", EqualWidth * Headers.Count));
            sB.AppendLine();


            //For each line, write out the space, then the text
            if (Headers.Count > 1)
                for (int CurRow = 0; CurRow < MaxLines; CurRow++)
                {
                    sB.Append(GetString("", FieldWidth));
                    foreach (String[] str in HeaderRows)
                        if (CurRow < str.Length)
                            sB.Append(GetString(str[CurRow], EqualWidth));
                        else
                            sB.Append(GetString("", EqualWidth));
                    foreach (String[] str in HeaderRows)
                        if (CurRow < str.Length)
                            sB.Append(GetString(str[CurRow], EqualWidth));
                        else
                            sB.Append(GetString("", EqualWidth));
                    sB.AppendLine();
                }

            //Now, write out our values
            foreach (KeyValuePair<String, int> kvp in Fields)
            {
                int LeftWidth = FieldWidth - TextRenderer.MeasureText(kvp.Key, ParentMenu.Font).Width;
                sB.Append(kvp.Key + ":" + new String(' ', GetSpaces(LeftWidth)));
                foreach (KeyValuePair<Point, String> kvp2 in EstValues)
                    if (kvp2.Key.Y == kvp.Value)
                        sB.Append(GetString(kvp2.Value, EqualWidth));
                foreach (KeyValuePair<Point, String> kvp2 in TelemValues)
                    if (kvp2.Key.Y == kvp.Value)
                        sB.Append(GetString(kvp2.Value, EqualWidth));
                sB.AppendLine();
            }

            String ThisLine;
            using (System.IO.StringReader sRd = new System.IO.StringReader(sB.ToString()))
                while (!String.IsNullOrEmpty(ThisLine = sRd.ReadLine()))
                    ItemCollection.Add(ThisLine).Enabled = false;
        }


        /// <summary>
        /// Determine the number of spaces needed to meet our target point
        /// </summary>
        /// <param name="TargetWidth"></param>
        /// <returns></returns>
        private int GetSpaces(Double TargetWidth)
        {
            int NumSpaces = 0;
            while (TextRenderer.MeasureText(new String(' ', NumSpaces), ParentMenu.Font).Width < Math.Ceiling(TargetWidth))
                NumSpaces++;
            return NumSpaces;
        }


        /// <summary>
        /// Return a string of the proper length
        /// </summary>
        /// <param name="InText"></param>
        /// <param name="TargetWidth"></param>
        private String GetString(String InText, int TargetWidth)
        {
            bool First = true;
            String OutText = InText;
            while (TextRenderer.MeasureText(OutText, ParentMenu.Font).Width < TargetWidth)
                if (First ^= true)
                    OutText = " " + OutText;
                else
                    OutText += " ";
            return OutText;
        }
    }
}
