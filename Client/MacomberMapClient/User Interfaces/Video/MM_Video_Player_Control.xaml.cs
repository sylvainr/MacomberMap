using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MacomberMapClient.User_Interfaces.Video
{
    /// <summary>
    /// Interaction logic for MM_Video_Player_Control.xaml
    /// </summary>
    public partial class MM_Video_Player_Control : UserControl
    {
        public MM_Video_Player_Control()
        {
            InitializeComponent();
        }

        public void Play()
        {
            mediaElement.Play();
        }

        public void Pause()
        {
            if (mediaElement.CanPause)
                mediaElement.Pause();
        }

        public void Stop()
        {
            mediaElement.Stop();
        }

        public void LoadFile(Uri Source, out System.Drawing.Size VideoSize)
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.Source = Source;
            VideoSize = new System.Drawing.Size(mediaElement.NaturalVideoHeight, mediaElement.NaturalVideoWidth);
        }
    }
}
