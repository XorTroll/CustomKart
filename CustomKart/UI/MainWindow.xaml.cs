using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
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
            Utils.EnsureDirectories();
            Utils.CleanTempDirectories();
            Settings.Load();

            SetupLanguage();
            Title = Utils.GetResource("common:program") + " - " + Utils.GetResource("common:notOpened");
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

        private SettingsWindow WSettings = null;
        private AssetsWindow WAssets = null;
        //  private SoundWindow WSound = null;
        private AudioWindow WAudio = null;
        private TextsWindow WTexts = null;
        private TexturesWindow WTextures = null;
        private TracksWindow WTracks = null;

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
            if (window != null && window.IsLoaded) HandleClose(ref window); // TODO: do nothing instead?
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
                DoCloseWindow(ref WSettings);
                DoCloseWindow(ref WAssets);
                DoCloseWindow(ref WAudio);
                DoCloseWindow(ref WTexts);
                DoCloseWindow(ref WTextures);
                DoCloseWindow(ref WTracks);
                switch(Utils.ShowYesNoMessage(Utils.GetResource("common:saveChanges"), Utils.GetResource("common:warn")))
                {
                    case MessageBoxResult.Yes:
                        {
                            var sf = new System.Windows.Forms.SaveFileDialog
                            {
                                Filter = Utils.NDSFilter,
                                Title = Utils.GetResource("common:dialogSave"),
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

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var od = new System.Windows.Forms.OpenFileDialog
            {
                Multiselect = false,
                Title = Utils.GetResource("common:dialogOpen"),
                Filter = Utils.NDSFilter,
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref WSettings);
        }

        private void AssetsButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref WAssets);
        }

        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref WAudio);
        }

        private void TextsButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref WTexts);
        }

        private void TexturesButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref WTextures);
        }

        private static System.Drawing.Color[] ConvertXBGR1555(byte[] Data)
        {
            System.Drawing.Color[] array = new System.Drawing.Color[Data.Length / 2];
            for (int i = 0; i < Data.Length; i += 2)
            {
                array[i / 2] = System.Drawing.Color.FromArgb((int)LibEveryFileExplorer.GFX.GFXUtil.ConvertColorFormat(LibEveryFileExplorer.IO.IOUtil.ReadU16LE(Data, i), LibEveryFileExplorer.GFX.ColorFormat.XBGR1555, LibEveryFileExplorer.GFX.ColorFormat.ARGB8888));
            }
            return array;
        }

        private static uint ConvertARGB(uint argb)
        {
            return LibEveryFileExplorer.GFX.GFXUtil.ConvertColorFormat(argb, LibEveryFileExplorer.GFX.ColorFormat.ARGB8888, LibEveryFileExplorer.GFX.ColorFormat.XBGR1555);
        }

        private void KartsButton_Click(object sender, RoutedEventArgs e)
        {
            var data = File.ReadAllBytes(@"I:\NitroEdit\ext_fs\\1311482822\data\Sound\sound_data.sdat");
            var sdat = new MKDS_Course_Modifier.Sound.SDAT(data);
            var swar_data = sdat.FAT.Records[sdat.INFO.WAVEARCRecord.Entries[7].fileID].Data;
            File.WriteAllBytes(@"E:\demo.swar", swar_data);
            var swar = new MKDS_Course_Modifier.Sound.SWAR(swar_data);
            MessageBox.Show("Doen");
        }

        private void TracksButton_Click(object sender, RoutedEventArgs e)
        {
            WindowClick(ref WTracks);
        }

        private void MessageClearButton_Click(object sender, RoutedEventArgs e)
        {
            var self = sender as Button;
            MessageLog.Items.Clear();
            MessageLog.Items.Add(self);
        }
    }
}
