using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using NSMBe4;
using NDS.NitroSystem.FND;
using System.ComponentModel;

using NCGRFormat = MKDS_Course_Modifier.G2D_Binary_File_Format.NCGR;

namespace CustomKart.UI
{
    /// <summary>
    /// Lógica de interacción para TexturesWindow.xaml
    /// </summary>
    ///
    public partial class TexturesWindow : Window
    {
        public TexturesWindow()
        {
            InitializeComponent();
            foreach (var tex_data in Settings.Textures)
            {
                try
                {
                    var tex = LoadTexture(tex_data);
                    Textures.Add(tex);
                    TexturesCombo.Items.Add(tex_data.Name);
                }
                catch
                {
                    Utils.ShowMessage("Invalid texture: " + tex_data.Name);
                }
            }
            Load();
        }

        private List<LoadedTexture> Textures = new List<LoadedTexture>();

        private Bitmap CurrentTexture = null;

        public Image2D Sprite { get; set; }
        
        public Palette Palette { get; set; }

        public NARC CARC { get; set; }

        public string SpriteName { get; set; }

        public string PaletteName { get; set; }

        public string CARCName { get; set; }

        public VirtualInlineFile SpriteFile { get; set; }
        public VirtualInlineFile PaletteFile { get; set; }

        private LoadedTexture LoadTexture(Texture tex_data)
        {
            var tex = new LoadedTexture(tex_data);
            byte[] ncgr_data;
            byte[] nclr_data;
            if (tex_data.IsInCARC)
            {
                tex.CARC = new CARCEditor(tex_data.CARC);
                ncgr_data = tex.CARC.ReadFile(tex_data.NCGR);
                nclr_data = tex.CARC.ReadFile(tex_data.NCLR);
            }
            else
            {
                ncgr_data = ROMUtils.ROM.ReadFile(tex_data.NCGR);
                nclr_data = ROMUtils.ROM.ReadFile(tex_data.NCLR);
            }

            var ncgr_fmt = new NCGRFormat(ncgr_data);
            // Value 3 = 4bpp, 4 = 8bpp
            var bit_depth = (int)ncgr_fmt.CharacterData.pixelFmt;
            var is_4bpp = bit_depth == 3;

            var size = BitConverter.ToUInt32(ncgr_data, 16 + 4);
            var width = (ushort)(8 * BitConverter.ToUInt16(ncgr_data, 16 + 10));
            tex.SpriteFile = new VirtualInlineFile(ncgr_data, 16 + 32, (int)size - 32, "");
            tex.Sprite = new Image2D(tex.SpriteFile, width, is_4bpp, false);
            var palette_size = BitConverter.ToUInt32(nclr_data, 16 + 4);
            tex.PaletteFile = new VirtualInlineFile(nclr_data, 16 + 24, (int)palette_size - 24, "");
            tex.Palette = new FilePalette(tex.PaletteFile);
            return tex;
        }

        public void Save()
        {
            foreach(var tex in Textures)
            {
                tex.Sprite.save();
                var ncgr_data = tex.SpriteFile.GetData();
                tex.Palette.save();
                var nclr_data = tex.PaletteFile.GetData();
                if(tex.Data.IsInCARC)
                {
                    tex.CARC.WriteFile(tex.Data.NCGR, ncgr_data);
                    tex.CARC.WriteFile(tex.Data.NCLR, nclr_data);
                    tex.CARC.Save();
                }
                else
                {
                    ROMUtils.ROM.WriteFile(tex.Data.NCGR, ncgr_data);
                    ROMUtils.ROM.WriteFile(tex.Data.NCLR, nclr_data);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            switch(Utils.ShowYesNoMessage(Utils.GetResource("common:saveChanges"), Utils.GetResource("common:warn")))
            {
                case MessageBoxResult.Yes:
                    {
                        Save();
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

        public void Load()
        {
            TexturesCombo.SelectedIndex = 0;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: support more image formats
            var sd = new SaveFileDialog
            {
                Filter = Utils.GetResource("common:imageFile") + "|*.png",
                Title = Utils.GetResource("textures:textureExport")
            };
            if(sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CurrentTexture.Save(sd.FileName, ImageFormat.Png);
                Utils.ShowMessage("Nice m8");
            }
        }

        private void UpdateTexture()
        {
            var tex = Textures[TexturesCombo.SelectedIndex];
            var bitmap = tex.Sprite.render(tex.Palette);
            Utils.UpdateImageWithBitmap(ref IconImage, bitmap);
            CurrentTexture = bitmap;
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var od = new OpenFileDialog
            {
                Filter = Utils.GetResource("common:imageFile") + "|*.png;*.jpg;*.jpeg;*.bmp",
                Title = Utils.GetResource("textures:textureImport"),
                Multiselect = false,
            };
            if(od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var bitmap = new Bitmap(od.FileName);
                    var tex = Textures[TexturesCombo.SelectedIndex];
                    tex.Sprite.replaceImgAndPal(new Bitmap(od.FileName), tex.Palette);
                    UpdateTexture();
                }
                catch
                {
                    Utils.ShowMessage("Not good m8");
                }
            }
        }

        private void TexturesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTexture();
        }
    }
}
