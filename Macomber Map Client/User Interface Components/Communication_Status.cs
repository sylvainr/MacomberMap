using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using Macomber_Map.User_Interface_Components;
using Macomber_Map.Data_Elements;
using Macomber_Map.User_Interface_Components.OneLines;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class displays a communications status button that will update depending on the state of events
    /// </summary>
    public partial class Communication_Status : UserControl
    {
        #region Variable declarations                
        /// <summary>The connection between the EMS system and downstream systems</summary>
        public int EMSStatus;

        /// <summary>The connection between Macomber Map and its queries</summary>
        public int QueryStatus;

        /// <summary>The worst state of the connections</summary>
        public int ServerStatus;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new communications status box
        /// </summary>
        public Communication_Status()
        {
            InitializeComponent();
        }
        #endregion

        #region Updating
        /// <summary>
        /// Update the EMS status
        /// </summary>
        /// <param name="EMSStatus"></param>
        public void SetEMSStatus(int EMSStatus)
        {
            if (EMSStatus == this.EMSStatus)
                return;
            this.EMSStatus = EMSStatus;
            if (this.InvokeRequired)
                this.BeginInvoke(new SafeRefresh(Refresh));
            else
                this.Refresh();
        }

        /// <summary>
        /// Update the Server status
        /// </summary>
        /// <param name="ServerStatus"></param>
        public void SetServerStatus(int ServerStatus)
        {
            if (ServerStatus == this.ServerStatus)
                return;
            this.ServerStatus = ServerStatus;
            if (this.InvokeRequired)
                this.BeginInvoke(new SafeRefresh(Refresh));
            else
                this.Refresh();
        }

        /// <summary>
        /// Update the query status
        /// </summary>
        /// <param name="QueryStatus"></param>
        /// <param name="ServerStatus"></param>
        public void SetQueryAndServerStatus(int QueryStatus, int ServerStatus)
        {
            if (QueryStatus== this.QueryStatus && ServerStatus == this.ServerStatus)
                return;
            this.QueryStatus = QueryStatus;
            this.ServerStatus = ServerStatus;
            if (this.InvokeRequired)
                this.BeginInvoke(new SafeRefresh(Refresh));
            else
                this.Refresh();
        }

        /// <summary>Safely refresh the communication window</summary>
        private delegate void SafeRefresh();
        #endregion

        #region Rendering
        /// <summary>
        /// Draw a status letter
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Letter"></param>
        /// <param name="Left"></param>
        /// <param name="Top"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Status"></param>
        private void DrawLetter(Graphics g, String Letter, int Left, int Top, int Width, int Height, int Status)
        {
            Color ForeColor, BackColor;
            if (Status == 1)
            {
                ForeColor = MM_Repository.OverallDisplay.WarningColor;
                BackColor = MM_Repository.OverallDisplay.WarningBackground;
            }
            else if (Status == 2)
            {
                ForeColor = MM_Repository.OverallDisplay.ErrorColor;
                BackColor = MM_Repository.OverallDisplay.ErrorBackground;
            }
            else
            {
                ForeColor = MM_Repository.OverallDisplay.NormalColor;
                BackColor = MM_Repository.OverallDisplay.NormalBackground;
            }

            Rectangle DrawRect = new Rectangle(Left, Top, Width, Height);
            //g.DrawRectangle(Pens.White, DrawRect);
            using (SolidBrush Background = new SolidBrush(ForeColor))
                g.FillRectangle(Background, DrawRect);

            using (SolidBrush Foreground = new SolidBrush(BackColor))
                g.DrawString(Letter, this.Font, Foreground, DrawRect, MM_OneLine_Element.CenterFormat);
        }

        /// <summary>
        /// Handle painting of the comm status
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //Updated to handle rendering of our components
            int LetterLeft = 2;
            int LetterWidth = (this.Width-4) / 4;
            int LetterHeight = this.Height - 4;
            DrawLetter(e.Graphics, "S", LetterLeft, 2, LetterWidth, LetterHeight, ServerStatus);
            DrawLetter(e.Graphics, "Q", LetterLeft += LetterWidth + 2, 2, LetterWidth, LetterHeight, QueryStatus);
            DrawLetter(e.Graphics, "P", LetterLeft += LetterWidth + 2, 2, LetterWidth, LetterHeight, 0);
            DrawLetter(e.Graphics, "I", LetterLeft += LetterWidth + 2, 2, this.Width - LetterLeft-2, LetterHeight, EMSStatus);

            Communication_Viewer.Server_Status_Details_Level = ServerStatus;
            Communication_Viewer.Server_Status_Details_Text = "Server Status: " + (ServerStatus == 1 ? "Suspect" : ServerStatus == 2 ? "Not Connected" : "Online");
        }

        /// <summary>
        /// Handle the background painting
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent.BackColor);
            ControlPaint.DrawBorder3D(e.Graphics, e.ClipRectangle, Border3DStyle.SunkenOuter);
        }
        #endregion
    }
}
