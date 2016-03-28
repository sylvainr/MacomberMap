using MacomberMapClient.User_Interfaces.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.NetworkMap
{
    /// <summary>
    /// © 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides a video interface for writing timelines and the states of equipment
    /// </summary>
    public partial class MM_Video_Recorder : MM_Form
    {
        int ImageCount = 0, CapturedFrames=0;

        /// <summary>
        /// Initialize a new video recorder
        /// </summary>
        public MM_Video_Recorder()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Every interval, grab a screen shot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrCaptureScreenshot_Tick(object sender, EventArgs e)
        {
            MacomberMap NetworkMap = Program.MM;
            using (Bitmap OutBmp = new Bitmap(NetworkMap.DisplayRectangle.Width, NetworkMap.DisplayRectangle.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                NetworkMap.DrawToBitmap(OutBmp, NetworkMap.DisplayRectangle);
                using (Graphics g = Graphics.FromImage(OutBmp))
                using (StringFormat sF = new StringFormat() { Alignment = StringAlignment.Center })
                    g.DrawString(DateTime.Now.ToShortDateString(), this.Font, Brushes.White, OutBmp.Width / 2, 5, sF);
                OutBmp.Save("img"+ImageCount.ToString("00000") + ".png", ImageFormat.Png);
                lblFramesCaptured.Text = "Frames captured: " + (++CapturedFrames).ToString("#,##0");
                ImageCount++;
            }
        }

        /// <summary>
        /// Select our target path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectPath_Click(object sender, EventArgs e)
        {          
                btnRecord.Text = "RECORD";
                ImageCount = 0;
                while (System.IO.File.Exists("img" + ImageCount.ToString("00000") + ".png"))
                    ImageCount++;         
        }

        /// <summary>
        /// Handle our record button clicking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRecord_Click(object sender, EventArgs e)
        {         
            if (btnRecord.Text == "RECORD")
            {
                btnRecord.BackColor = Color.LightGreen;
                btnRecord.Text = "STOP";
                tmrCaptureScreenshot_Tick(tmrCaptureScreenshot, EventArgs.Empty);
                tmrCaptureScreenshot.Interval = (int)txtCaptureEvery.Value * 60000 / (int)txtFPS.Value;
                tmrCaptureScreenshot.Start();
            }
            else if (btnRecord.Text=="STOP")
            {
                tmrCaptureScreenshot.Stop();
                btnRecord.BackColor = SystemColors.ButtonFace;
                btnRecord.Text = "RECORD";
            }
        }
    }
}