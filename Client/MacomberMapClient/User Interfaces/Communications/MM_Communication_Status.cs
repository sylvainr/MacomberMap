using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.OneLines;

namespace MacomberMapClient.User_Interfaces.Communications
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class displays a communications status button that will update depending on the state of events
    /// </summary>
    public partial class MM_Communication_Status : UserControl
    {
        #region Variable declarations
        /// <summary>The connection between the EMS system and downstream systems</summary>
        public static int EMSStatus;

        /// <summary>The connection between Macomber Map and its queries</summary>
        public static int QueryStatus;

        /// <summary>The worst state of the connections</summary>
        public static int ServerStatus;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new communications status box
        /// </summary>
        public MM_Communication_Status()
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
            if (EMSStatus == MM_Communication_Status.EMSStatus)
                return;
            MM_Communication_Status.EMSStatus = EMSStatus;
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
            if (ServerStatus == MM_Communication_Status.ServerStatus)
                return;
            MM_Communication_Status.ServerStatus = ServerStatus;
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
            if (QueryStatus == MM_Communication_Status.QueryStatus && ServerStatus == MM_Communication_Status.ServerStatus)
                return;
            MM_Communication_Status.QueryStatus = QueryStatus;
            MM_Communication_Status.ServerStatus = ServerStatus;
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
        /// <param name="DrawFont"></param>
        private static void DrawLetter(Graphics g, String Letter, int Left, int Top, int Width, int Height, int Status, Font DrawFont)
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
                g.DrawString(Letter, DrawFont, Foreground, DrawRect, MM_OneLine_Element.CenterFormat);
        }

        /// <summary>
        /// Handle painting of the comm status
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //Updated to handle rendering of our components
            try
            {
                DrawStatus(e.Graphics, 2, (this.Width - 8) / 4, this.Height - 4, this.Width, this.Font);
            }
            catch { }
        }

        /// <summary>
        /// Draw our communication status
        /// </summary>
        /// <param name="g"></param>
        /// <param name="LetterLeft"></param>
        /// <param name="LetterWidth"></param>
        /// <param name="LetterHeight"></param>
        /// <param name="Width"></param>
        /// <param name="DrawFont"></param>
        public static void DrawStatus(Graphics g, int LetterLeft, int LetterWidth, int LetterHeight, int Width, Font DrawFont)
        {
            DrawLetter(g, "S", LetterLeft, 2, LetterWidth, LetterHeight, ServerStatus, DrawFont);
            DrawLetter(g, "Q", LetterLeft += LetterWidth + 2, 2, LetterWidth, LetterHeight, QueryStatus, DrawFont);
            DrawLetter(g, "P", LetterLeft += LetterWidth + 2, 2, LetterWidth, LetterHeight, 0, DrawFont);
            DrawLetter(g, "I", LetterLeft += LetterWidth + 2, 2, LetterWidth, LetterHeight, EMSStatus, DrawFont);
        }

        /// <summary>
        /// Handle the background painting
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Parent.BackColor);
                ControlPaint.DrawBorder3D(e.Graphics, e.ClipRectangle, Border3DStyle.SunkenOuter);
            }
            catch { }
        }
        #endregion
    }
}
