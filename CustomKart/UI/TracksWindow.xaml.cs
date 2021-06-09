using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MKDS_Course_Modifier.G3D_Binary_File_Format;
using System.Windows.Media.Media3D;
using MarioKart.MKDS.NKM;
using NSMBe4;
using NDS.NitroSystem.FND;
using HelixToolkit.Wpf;
using Tao.OpenGl;

namespace CustomKart.UI
{
    /// <summary>
    /// Lógica de interacción para TracksWindow.xaml
    /// </summary>
    public partial class TracksWindow : Window
    {
        public TracksWindow()
        {
            InitializeComponent();
            foreach(var track in MKDS.InternalTrackNames)
            {
                var track_name = Utils.GetResource("tracks:trackName:" + track);
                TracksCombo.Items.Add(track_name);
            }
            TracksCombo.SelectedIndex = 0;
        }

        private TrackViewWindow TrackWindow = null;

        private void PropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if(TrackWindow != null && TrackWindow.IsLoaded)
            {
                return;
            }
            TrackWindow = new TrackViewWindow { Owner = this };
            TrackWindow.LoadTrack(TracksCombo.SelectedIndex);
            TrackWindow.Show();
        }
    }
}
