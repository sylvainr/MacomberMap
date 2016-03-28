using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.User_Interfaces.Blackstart
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
            this.Title = this.Text;
            this.cmbVideoRecorder.SelectedIndex = 0;
        }

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        /// <summary>
        /// Every interval, grab a screen shot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrCaptureScreenshot_Tick(object sender, EventArgs e)
        {
            if (!chkCapturePaused.Checked && Data_Integration.SimulatorStatus != MacomberMapCommunications.Messages.EMS.MM_Simulation_Time.enumSimulationStatus.Running)
                return;

            MacomberMap_Form NetworkMap = Program.MM;
            using (Bitmap OutBmp = new Bitmap(NetworkMap.DisplayRectangle.Width, NetworkMap.DisplayRectangle.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))           
            {
                //Note, due to the DirectX layer, now a printscreen core library must be used
                using (Graphics g = Graphics.FromImage(OutBmp))
                using (Font DrawFont = new Font("Arial", 16, FontStyle.Bold))
                using (StringFormat sF = new StringFormat() { Alignment = StringAlignment.Center })
                {
                    IntPtr hDC = g.GetHdc();
                    try
                    {


                        PrintWindow(NetworkMap.Handle, hDC, (uint)0);
                    }
                    catch
                    {
                        NetworkMap.DrawToBitmap(OutBmp, NetworkMap.DisplayRectangle);
                    }
                    finally
                    {
                        g.ReleaseHdc(hDC);
                    }
                    g.DrawString(DateTime.Now.ToString(), DrawFont, Brushes.White, OutBmp.Width / 2, 20, sF);
                }
                OutBmp.Save("img"+ImageCount.ToString("00000") + ".png", ImageFormat.Png);
                lblFramesCaptured.Text = "Frames captured: " + (++CapturedFrames).ToString("#,##0");

                using (XmlTextWriter xW = new XmlTextWriter("snp" + ImageCount.ToString("00000") + ".xml", new UTF8Encoding(false)))
                {
                    xW.WriteStartDocument();
                    xW.WriteStartElement("Savecase");
                    xW.WriteAttributeString("Realtime", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Unspecified));
                    xW.WriteAttributeString("SavecaseTime", XmlConvert.ToString(Data_Integration.CurrentTime, XmlDateTimeSerializationMode.Unspecified));
                    xW.WriteAttributeString("SimulatorState", Data_Integration.SimulatorStatus.ToString());
                    xW.WriteAttributeString("Commands", Data_Integration.EMSCommands.Count.ToString());
                    foreach (Data_Integration.OverallIndicatorEnum Indicator in Enum.GetValues(typeof(Data_Integration.OverallIndicatorEnum)))
                        xW.WriteAttributeString(Indicator.ToString(), Data_Integration.OverallIndicators[(int)Indicator].ToString());
                    xW.WriteEndElement();
                    xW.WriteEndDocument();
                }

                ImageCount++;
            }
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
                ImageCount = 0;
                while (System.IO.File.Exists("img" + ImageCount.ToString("00000") + ".png"))
                    ImageCount++;  
                btnRecord.BackColor = Color.LightGreen;
                btnRecord.Text = "STOP";
                tmrCaptureScreenshot_Tick(tmrCaptureScreenshot, EventArgs.Empty);
                if (cmbVideoRecorder.SelectedItem.ToString()=="minutes")
                    tmrCaptureScreenshot.Interval = (int)txtCaptureEvery.Value * 60000 / (int)txtFPS.Value;
                else if (cmbVideoRecorder.SelectedItem.ToString()=="seconds")
                    tmrCaptureScreenshot.Interval = (int)txtCaptureEvery.Value * 1000 / (int)txtFPS.Value;
                else if (cmbVideoRecorder.SelectedItem.ToString()=="hours")
                    tmrCaptureScreenshot.Interval = (int)txtCaptureEvery.Value * 3600000 / (int)txtFPS.Value;

                tmrCaptureScreenshot.Start();
            }
            else if (btnRecord.Text=="STOP")
            {
                tmrCaptureScreenshot.Stop();
                btnRecord.BackColor = SystemColors.ButtonFace;
                btnRecord.Text = "RECORD";
            }
        }

        /// <summary>
        /// Handle our screen lock button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnScreenLock_Click(object sender, EventArgs e)
        {
            MM_Repository.DisplayPositionLocked ^= true;
            btnScreenLock.Text = "Screen Lock:" + Environment.NewLine + (MM_Repository.DisplayPositionLocked ? "ON" : "OFF");
            btnScreenLock.BackColor = MM_Repository.DisplayPositionLocked ? Color.LightGreen : SystemColors.ButtonFace;
        }

        /// <summary>
        /// Update the starting position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPositionScreen_Click(object sender, EventArgs e)
        {
            MacomberMap_Form NetworkMap = Program.MM;
            MacomberMapClient.Data_Elements.Geographic.MM_Boundary Bound = MM_Repository.Counties["STATE"];

            //NetworkMap.ctlNetworkMap.SetDisplayCoordinates(Bound.Min, Bound.Max);
            NetworkMap.ctlNetworkMap.SetDisplayCoordinates(new PointF(-109.544678f, 35.5947838f), new PointF(-90.60425f, 25.37381f));
        }

        /// <summary>
        /// Produce our video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProduceVideo_Click(object sender, EventArgs e)
        {
            Thread ProduceThread = new Thread(ProduceVideo);
            ProduceThread.SetApartmentState(ApartmentState.STA);
            ProduceThread.Start();
        }

        [STAThread]
        private void ProduceVideo(object state)
        {
            using (SaveFileDialog sFd = new SaveFileDialog() {Title = "Save Macomber Map Video...",Filter = "Video file (*.mp4)|*.mp4"})
            if (sFd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProcessStartInfo psi = new ProcessStartInfo("ffmpeg.exe", "-framerate " + txtFPS.Value.ToString() + " -i img%05d.png -r " + txtFPS.Value.ToString() + " \"" + sFd.FileName + "\"");
                Process.Start(psi).WaitForExit();

                ProcessStartInfo psi2 = new ProcessStartInfo("chrome.exe", "\"" + sFd.FileName + "\"");
                Process.Start(psi2);
            }
        }
    }
}