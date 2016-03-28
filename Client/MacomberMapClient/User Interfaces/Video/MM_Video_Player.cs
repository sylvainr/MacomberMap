using MacomberMapClient.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Video
{
    /// <summary>
    /// © 2016, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This video player allows for quick video playing or live stream navigation - to any needed video. Currently it's set 
    /// to pop up when the user presses 'z' on the main map, but could also be configured to run on another stream based on element selection.
    /// Currently it uses the WPF MediaElement control to render the video, but should be upgraded to use SharpDx media classes.
    /// </summary>
    public partial class MM_Video_Player : Form
    {
        #region Variable declarations
        /// <summary>Our video player control</summary>
        MM_Video_Player_Control VideoPlayer;


        #endregion

        #region Initiailzation
        /// <summary>
        /// Show our player in a separate thread
        /// </summary>
        public static void ShowPlayer()
        {
            if (String.IsNullOrEmpty(Settings.Default.VideoURL))
                return;
            Thread RunThread = new Thread(ShowPlayerPrivate);
            RunThread.SetApartmentState(ApartmentState.STA);
            RunThread.Start();
        }

        [STAThread]
        private static void ShowPlayerPrivate()
        {
            MM_Video_Player VideoPlayer = new MM_Video_Player() { StartPosition = FormStartPosition.Manual, Location = new Point(Cursor.Position.X - 400, Cursor.Position.Y - 300), Size = new Size(800, 600), BackColor = Color.Black };
          //  VideoPlayer.Size = VideoPlayer.VideoDimensions;
          //  VideoPlayer.Location = new Point(Cursor.Position.X - (VideoPlayer.VideoDimensions.Width / 2), Cursor.Position.Y - (VideoPlayer.VideoDimensions.Height / 2));
            Application.Run(VideoPlayer);
        }

        /// <summary>The dimensions of our video</summary>
        public Size VideoDimensions;

        /// <summary>
        /// Initialize our video player
        /// </summary>
        public MM_Video_Player()
        {
            InitializeComponent();
            VideoPlayer = new MM_Video_Player_Control();
            wpfHost.Child = VideoPlayer;
            VideoPlayer.MouseDown += VideoPlayer_MouseDown;
            VideoPlayer.mediaElement.MediaEnded += MediaElement_MediaEnded;
            VideoPlayer.LoadFile(new Uri(Settings.Default.VideoURL), out VideoDimensions);
            
        }

        /// <summary>
        /// When our video ends, shut down the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaElement_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region User Interactions
        /// <summary>
        /// When the user clicks on a video, shut down the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoPlayer_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// When our form is shown, begin playing the video
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            VideoPlayer.Play();            
        }

        
        /// <summary>
        /// When our video window loses focus, shut down.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.Close();
        }
        #endregion
    }
}
