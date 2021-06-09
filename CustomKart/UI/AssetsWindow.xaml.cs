using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using NSMBe4;
using NSMBe4.DSFileSystem;

namespace CustomKart.UI
{
    /// <summary>
    /// Lógica de interacción para AssetsWindow.xaml
    /// </summary>
    public partial class AssetsWindow : Window
    {
        public AssetsWindow()
        {
            InitializeComponent();
            UpdateValues();
        }

        private Bitmap LoadedImage = null;

        public void UpdateValues()
        {
            GameNameText.Text = ROMUtils.ROM.ROM.Header.GameName;
            GameCodeText.Text = ROMUtils.ROM.ROM.Header.GameCode;
            MakerCodeText.Text = ROMUtils.ROM.ROM.Header.MakerCode;
            Utils.UpdateImageWithBitmap(ref IconImage, ROMUtils.ROM.Icon);
            TitleJaText.Text = ROMUtils.ROM.ROM.Banner.Banner.GameName[0];
            TitleEnText.Text = ROMUtils.ROM.ROM.Banner.Banner.GameName[1];
            TitleFrText.Text = ROMUtils.ROM.ROM.Banner.Banner.GameName[2];
            TitleDeText.Text = ROMUtils.ROM.ROM.Banner.Banner.GameName[3];
            TitleItText.Text = ROMUtils.ROM.ROM.Banner.Banner.GameName[4];
            TitleEsText.Text = ROMUtils.ROM.ROM.Banner.Banner.GameName[5];
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            switch(Utils.ShowYesNoMessage(Utils.GetResource("common:saveChanges"), Utils.GetResource("common:warn")))
            {
                case MessageBoxResult.Yes:
                    {
                        ROMUtils.ROM.ROM.Header.GameName = GameNameText.Text;
                        ROMUtils.ROM.ROM.Header.GameCode = GameCodeText.Text;
                        ROMUtils.ROM.ROM.Header.MakerCode = MakerCodeText.Text;
                        ROMUtils.ROM.ROM.Banner.Banner.GameName[0] = TitleJaText.Text;
                        ROMUtils.ROM.ROM.Banner.Banner.GameName[1] = TitleEnText.Text;
                        ROMUtils.ROM.ROM.Banner.Banner.GameName[2] = TitleFrText.Text;
                        ROMUtils.ROM.ROM.Banner.Banner.GameName[3] = TitleDeText.Text;
                        ROMUtils.ROM.ROM.Banner.Banner.GameName[4] = TitleItText.Text;
                        ROMUtils.ROM.ROM.Banner.Banner.GameName[5] = TitleEsText.Text;
                        if(LoadedImage != null) ROMUtils.ROM.Icon = LoadedImage;
                        break;
                    }
                case MessageBoxResult.Cancel:
                    {
                        e.Cancel = true;
                        break;
                    }
            }
            base.OnClosing(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            UpdateValues();
            base.OnGotFocus(e);
        }

        private void IconReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            var od = new OpenFileDialog
            {
                Title = Utils.GetResource("assets:dialogIconReplace"),
                Filter = Utils.GetResource("common:imageFile") + "|*.png;*.jpg;*.jpeg;*.bmp",
                Multiselect = false,
            };
            if(od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadedImage = new Bitmap(od.FileName);
                if(LoadedImage.Width != 32 || LoadedImage.Height != 32)
                {
                    Utils.ShowMessage(Utils.GetResource("assets:invalidIconSize"));
                    LoadedImage = null;
                }
                else
                {
                    try
                    {
                        Utils.UpdateImageWithBitmap(ref IconImage, LoadedImage);
                    }
                    catch
                    {
                        Utils.ShowMessage(Utils.GetResource("assets:errorIconChange"));
                    }
                }
            }
        }
    }
}
