using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using System.IO;
using System.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace CustomKart.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Utils.CleanTempDirectories();
            SetupLanguage();
            Title = Utils.GetResource("common:program") + " - " + Utils.GetResource("common:noROMOpened");
        }

        private void SetupLanguage()
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri("Resources\\Strings.en.xaml", UriKind.Relative)
            };

            // TODO: actual multi-language support
            /*
            var lang = Thread.CurrentThread.CurrentUICulture.Name;
            if(lang.StartsWith("es-"))
            {
                lang.Source = new Uri("Resources\\Strings.es.xaml");
            }
            */

            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        public void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
            MessageLog.Items.Insert(1, msg);
        }

        private AssetsWindow Assets = null;
        private SoundWindow Sound = null;
        private TextsWindow Texts = null;
        private TexturesWindow Textures = null;
        private TracksWindow Tracks = null;

        public void DoCloseWindow<T>(ref T window) where T : Window
        {
            if(window != null && window.IsLoaded) window.Close();
        }

        private void HandleClose<T>(ref T window) where T : Window
        {
            // TODO: ask to close the window
            MessageBox.Show("Opened!");
        }

        private void WindowClick<T>(ref T window) where T : Window, new()
        {
            if (window != null && window.IsLoaded) HandleClose(ref window);
            else
            {
                window = new T { Owner = this };
                window.Show();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(ROMUtils.HasLoadedROM())
            {
                DoCloseWindow(ref Assets);
                DoCloseWindow(ref Sound);
                DoCloseWindow(ref Texts);
                DoCloseWindow(ref Textures);
                DoCloseWindow(ref Tracks);
                switch(Utils.ShowYesNoMessage(Utils.GetResource("common:saveChanges"), Utils.GetResource("common:warn")))
                {
                    case MessageBoxResult.Yes:
                        {
                            var sf = new System.Windows.Forms.SaveFileDialog
                            {
                                Filter = Utils.GetNDSFilter(),
                                Title = Utils.GetResource("common:dialogROMSave"),
                            };
                            if(sf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                ROMUtils.ROM.Save(sf.FileName);
                            }
                            break;
                        }
                    case MessageBoxResult.Cancel:
                        {
                            e.Cancel = true;
                            break;
                        }
                }
            }
            base.OnClosing(e);
        }

        private void ROMOpenButton_Click(object sender, RoutedEventArgs e)
        {
            var od = new System.Windows.Forms.OpenFileDialog
            {
                Multiselect = false,
                Title = Utils.GetResource("common:dialogROMOpen"),
                Filter = Utils.GetNDSFilter(),
            };
            if(od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    ROMUtils.TryLoad(od.FileName);
                    Title = Utils.GetResource("common:program") + " - " + Path.GetFileName(od.FileName);
                    ShowMessage(Utils.GetResource("common:successROMLoad") + ": " + Path.GetFileName(od.FileName) + "\n" + Utils.GetResource("common:buildDateInfo") + " \"" + ROMUtils.GetBuildDate() + "\"");
                    AssetsButton.IsEnabled = true;
                    KartsButton.IsEnabled = true;
                    TexturesButton.IsEnabled = true;
                    SoundButton.IsEnabled = true;
                    TextsButton.IsEnabled = true;
                    TracksButton.IsEnabled = true;
                }
                catch(NotMKDSROMException)
                {
                    ShowMessage(Utils.GetResource("common:invalidMKDSROM"));
                }
                catch
                {
                    ShowMessage(Utils.GetResource("common:invalidNDSROM"));
                }
            }
        }

        private void AssetsButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref Assets);
        }

        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref Sound);
        }

        private void TextsButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref Texts);
        }

        private void TexturesButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref Textures);
        }

        private void KartsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void TracksButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref Tracks);
        }

        private void MessageClearButton_Click(object sender, RoutedEventArgs e)
        {
            var self = sender as Button;
            MessageLog.Items.Clear();
            MessageLog.Items.Add(self);
        }
    }
}
