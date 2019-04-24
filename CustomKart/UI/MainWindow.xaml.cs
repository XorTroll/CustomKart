using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Threading;
using System.IO;
using System.ComponentModel;
using LibDSSound.IO;

namespace CustomKart.UI
{
    public partial class MainWindow : Window
    {
        public static NDS.Nitro.NDS LoadedROM { get; private set; }

        public static string ROMPath { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            SetupLanguage();
            Title = Utils.GetResource("program") + " - " + Utils.GetResource("noROM");
            Notifications.MessageQueue = new MaterialDesignThemes.Wpf.SnackbarMessageQueue(TimeSpan.FromMilliseconds(1000));
        }

        private void SetupLanguage()
        {
            ResourceDictionary langd = new ResourceDictionary();
            string lang = Thread.CurrentThread.CurrentUICulture.Name;
            langd.Source = new Uri("Resources\\Strings.en.xaml", UriKind.Relative);
            // if(lang.StartsWith("es-")) langd.Source = new Uri("Resources\\Strings.es.xaml");
            Resources.MergedDictionaries.Add(langd);
        }

        public void ShowNotification(string Message)
        {
            Notifications.MessageQueue.Enqueue(Message);
        }

        public void ShowNotificationWithOption(string Message, string Option, Action ToCall)
        {
            Notifications.MessageQueue.Enqueue(Message, Option, ToCall);
        }

        private AssetsWindow assets = null;
        private SoundWindow sound = null;
        private TextsWindow texts = null;

        protected override void OnClosing(CancelEventArgs e)
        {
            if(LoadedROM != null)
            {
                if(assets != null)
                {
                    assets.Close();
                    if(assets.IsVisible)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                if(sound != null)
                {
                    sound.Close();
                    if(sound.IsVisible)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                if(texts != null)
                {
                    texts.Close();
                    if(texts.IsVisible)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                var res = System.Windows.MessageBox.Show("Would you like to save the edited changes?", "Edited changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if(res == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if(res == MessageBoxResult.Yes)
                {
                    var data = LoadedROM.Write();
                    SaveFileDialog sf = new SaveFileDialog
                    {
                        Filter = Utils.GetNDSFilter(),
                        Title = Utils.GetResource("ROMSave"),
                    };
                    if(sf.ShowDialog() == System.Windows.Forms.DialogResult.OK) File.WriteAllBytes(sf.FileName, data);
                }
            }
            base.OnClosing(e);
        }

        private void ROMOpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog
            {
                Multiselect = false,
                Title = Utils.GetResource("ROMOpen"),
                Filter = Utils.GetNDSFilter(),
            };
            if(od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadedROM = ROMUtils.LoadROM(od.FileName);
                if(LoadedROM == null)
                {
                    ShowNotification("The selected file is not a valid Nintendo DS ROM.");
                    return;
                }
                if(!LoadedROM.IsMKDS())
                {
                    ShowNotification("The selected file is not a valid Mario Kart DS ROM.");
                    LoadedROM = null;
                    return;
                }
                Title = Utils.GetResource("program") + " - " + Path.GetFileName(od.FileName);
                ShowNotification("MKDS ROM loaded: " + Path.GetFileName(od.FileName));
                ROMPath = od.FileName;
                AssetsCard.IsEnabled = true;
                KartsCard.IsEnabled = true;
                TexturesCard.IsEnabled = true;
                SoundCard.IsEnabled = true;
                TextsCard.IsEnabled = true;
            }
        }

        private void AssetsButton_Click(object sender, RoutedEventArgs e)
        {
            if(assets == null)
            {
                assets = new AssetsWindow
                {
                    Owner = this
                };
                assets.Closing += new CancelEventHandler((object s, CancelEventArgs e2) =>
                {
                    Focus();
                    assets = null;
                });
                assets.Show();
            }
            else
            {
                assets.Focus();
            }
        }

        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            if(sound == null)
            {
                sound = new SoundWindow
                {
                    Owner = this
                };
                sound.Closing += new CancelEventHandler((object s, CancelEventArgs e2) =>
                {
                    Focus();
                    sound = null;
                });
                sound.Load(new SDAT(ROMUtils.GetFile("sound_data.sdat", LoadedROM.ToFileSystem())));
                sound.Show();
            }
            else
            {
                sound.Focus();
            }
        }

        private void TextsButton_Click(object sender, RoutedEventArgs e)
        {
            if(texts == null)
            {
                texts = new TextsWindow
                {
                    Owner = this
                };
                texts.Closing += new CancelEventHandler((object s, CancelEventArgs e2) =>
                {
                    Focus();
                    assets = null;
                });
                texts.Load();
                texts.Show();
            }
            else
            {
                texts.Focus();
            }
        }
    }
}
