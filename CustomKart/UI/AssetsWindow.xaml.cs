using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

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

        public void UpdateValues()
        {
            GameNameText.Text = MainWindow.LoadedROM.Header.GameName;
            GameCodeText.Text = MainWindow.LoadedROM.Header.GameCode;
            MakerCodeText.Text = MainWindow.LoadedROM.Header.MakerCode;
            var bitmap = MainWindow.LoadedROM.Banner.Banner.GetIcon();
            using(MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                IconImage.Source = bitmapimage;
            }
            TitleJaText.Text = MainWindow.LoadedROM.Banner.Banner.GameName[0];
            TitleEnText.Text = MainWindow.LoadedROM.Banner.Banner.GameName[1];
            TitleFrText.Text = MainWindow.LoadedROM.Banner.Banner.GameName[2];
            TitleDeText.Text = MainWindow.LoadedROM.Banner.Banner.GameName[3];
            TitleItText.Text = MainWindow.LoadedROM.Banner.Banner.GameName[4];
            TitleEsText.Text = MainWindow.LoadedROM.Banner.Banner.GameName[5];
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            MainWindow.LoadedROM.Header.GameName = GameNameText.Text;
            MainWindow.LoadedROM.Header.GameCode = GameCodeText.Text;
            MainWindow.LoadedROM.Header.MakerCode = MakerCodeText.Text;
            MainWindow.LoadedROM.Banner.Banner.GameName[0] = TitleJaText.Text;
            MainWindow.LoadedROM.Banner.Banner.GameName[1] = TitleEnText.Text;
            MainWindow.LoadedROM.Banner.Banner.GameName[2] = TitleFrText.Text;
            MainWindow.LoadedROM.Banner.Banner.GameName[3] = TitleDeText.Text;
            MainWindow.LoadedROM.Banner.Banner.GameName[4] = TitleItText.Text;
            MainWindow.LoadedROM.Banner.Banner.GameName[5] = TitleEsText.Text;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            UpdateValues();
            base.OnGotFocus(e);
        }

        private void IconReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog
            {
                Title = "Select image to replace icon",
                Filter = "Portable Network Graphics image|*.png|JPEG image|*.jpg|All file types|*.*",
                Multiselect = false,
            };
            if(od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    NSMBe4.ROM.load(new NSMBe4.DSFileSystem.NitroROMFilesystem(MainWindow.ROMPath));
                    var f = NSMBe4.ROM.getLevelFile("banner");
                    var imgf = new NSMBe4.DSFileSystem.InlineFile(f, 32, 512, f.name);
                    var palf = new NSMBe4.DSFileSystem.InlineFile(f, 544, 32, f.name);
                    var i2d = new NSMBe4.Image2D(imgf, 32, true, false) as NSMBe4.PalettedImage;
                    var ipal = new NSMBe4.FilePalette(palf);
                    i2d.replaceImgAndPal(new Bitmap(od.FileName), ipal as NSMBe4.Palette);
                    Array.Copy((i2d as NSMBe4.Image2D).getData(), MainWindow.LoadedROM.Banner.Banner.Image, 512);
                    Array.Copy(ipal.getData(), MainWindow.LoadedROM.Banner.Banner.Pltt, 32);
                    NSMBe4.ROM.close();
                    UpdateValues();
                }
                catch
                {
                    Utils.ShowMessage("An error ocurred trying to change the game's icon.");
                }
            }
        }
    }
}
