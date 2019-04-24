using System;
using System.Collections.Generic;
using System.IO;
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
using NDS.NitroSystem.FND;
using MKDS_Course_Modifier.Misc;
using NSMBe4;
using System.ComponentModel;

namespace CustomKart.UI
{
    /// <summary>
    /// Lógica de interacción para TextsWindow.xaml
    /// </summary>
    public partial class TextsWindow : Window
    {
        public TextsWindow()
        {
            InitializeComponent();
            ComboIndex = -1;
        }

        public NARC Main2D { get; set; }

        public BMG Common { get; set; }

        public NARC Static2D { get; set; }

        public BMG MBChild { get; set; }

        public NARC CharacterKartSelect { get; set; }

        public BMG KartSelect { get; set; }

        private int ComboIndex { get; set; }

        public string GetCARCExtension(int Lang)
        {
            string carc = "_";
            if(Lang == 0) carc += "us";
            else if(Lang == 1) carc += "es";
            else if(Lang == 2) carc += "fr";
            else if(Lang == 3) carc += "ge";
            else if(Lang == 4) carc += "it";
            return carc;
        }

        protected override void OnInitialized(EventArgs e)
        {
            // LanguageCombo.SelectedIndex = 0;
            base.OnInitialized(e);
        }

        private void Save(int Lang)
        {
            string ext = GetCARCExtension(Lang);
            var rfs = MainWindow.LoadedROM.ToFileSystem();
            int idx = 0;
            foreach(TextBox elem in CommonTextsList.Items)
            {
                Common.DAT1.Strings[idx] = elem.Text;
                idx++;
            }
            var common = Common.Save();
            var dmain2d = Main2D.ToFileSystem();
            var fcommon = ROMUtils.GetSFSFile("common.bmg", dmain2d);
            fcommon.Data = common;
            Main2D.FromFileSystem(dmain2d);
            var fmain2d = ROMUtils.GetSFSFile("Main2D" + ext + ".carc", rfs);
            fmain2d.Data = ROM.LZ77_Compress(Main2D.Write());
            idx = 0;
            foreach(TextBox elem in MBChildTextsList.Items)
            {
                MBChild.DAT1.Strings[idx] = elem.Text;
                idx++;
            }
            var mbchild = MBChild.Save();
            var dstatic2d = Static2D.ToFileSystem();
            var fmbchild = ROMUtils.GetSFSFile("MBChild" + ext + ".bmg", dstatic2d);
            fmbchild.Data = mbchild;
            Static2D.FromFileSystem(dstatic2d);
            var fstatic2d = ROMUtils.GetSFSFile("Static2D.carc", rfs);
            fstatic2d.Data = ROM.LZ77_Compress(Static2D.Write());
            idx = 0;
            foreach(TextBox elem in KartSelectTextsList.Items)
            {
                KartSelect.DAT1.Strings[idx] = elem.Text;
                idx++;
            }
            var ksel = KartSelect.Save();
            var dchksel = CharacterKartSelect.ToFileSystem();
            var fksel = ROMUtils.GetSFSFile("kart_select.bmg", dchksel);
            fksel.Data = ksel;
            CharacterKartSelect.FromFileSystem(dchksel);
            var fchksel = ROMUtils.GetSFSFile("CharacterKartSelect" + ext + ".carc", rfs);
            fchksel.Data = ROM.LZ77_Compress(CharacterKartSelect.Write());
            MainWindow.LoadedROM.FromFileSystem(rfs);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var res = MessageBox.Show("Would you like to save current language's texts before changing to another one?\nAny changes will be lost otherwise.", "Save edited texts", MessageBoxButton.YesNoCancel);
            if(res == MessageBoxResult.Yes)
            {
                Save(LanguageCombo.SelectedIndex);
            }
            else if(res == MessageBoxResult.Cancel) e.Cancel = true;
            base.OnClosing(e);
        }

        public void Load()
        {
            if(ComboIndex < 0) LanguageCombo.SelectedIndex = 0;
            var rfs = MainWindow.LoadedROM.ToFileSystem();
            string ext = GetCARCExtension(LanguageCombo.SelectedIndex);
            var cbmain2d = ROMUtils.GetFile("Main2D" + ext + ".carc", rfs);
            var bmain2d = ROM.LZ77_Decompress(cbmain2d);
            Main2D = new NARC(bmain2d);
            var dmain2d = Main2D.ToFileSystem();
            var common = ROMUtils.GetFile("common.bmg", dmain2d);
            Common = new BMG(common);
            var cbstatic2d = ROMUtils.GetFile("Static2D.carc", rfs);
            var bstatic2d = ROM.LZ77_Decompress(cbstatic2d);
            Static2D = new NARC(bstatic2d);
            var dstatic2d = Static2D.ToFileSystem();
            var mbchild = ROMUtils.GetFile("MBChild" + ext + ".bmg", dstatic2d);
            MBChild = new BMG(mbchild);
            var cbchksel = ROMUtils.GetFile("CharacterKartSelect" + ext + ".carc", rfs);
            var bchksel = ROM.LZ77_Decompress(cbchksel);
            CharacterKartSelect = new NARC(bchksel);
            var dchksel = CharacterKartSelect.ToFileSystem();
            var ksel = ROMUtils.GetFile("kart_select.bmg", dchksel);
            KartSelect = new BMG(ksel);
            CommonTextsList.Items.Clear();
            MBChildTextsList.Items.Clear();
            KartSelectTextsList.Items.Clear();
            foreach(var str in Common.DAT1.Strings) CommonTextsList.Items.Add(new TextBox()
            {
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Text = str,
            });
            foreach(var str in MBChild.DAT1.Strings) MBChildTextsList.Items.Add(new TextBox()
            {
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Text = str,
            });
            foreach(var str in KartSelect.DAT1.Strings) KartSelectTextsList.Items.Add(new TextBox()
            {
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Text = str,
            });
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ComboIndex < 0)
            {
                ComboIndex = LanguageCombo.SelectedIndex;
                return;
            }
            var res = MessageBox.Show("Would you like to save current language's texts before changing to another one?\nAny changes will be lost otherwise.", "Save edited texts", MessageBoxButton.YesNoCancel);
            if(res == MessageBoxResult.Yes)
            {
                Save(ComboIndex);
            }
            else if(res == MessageBoxResult.Cancel)
            {
                ComboIndex = -1;
                LanguageCombo.SelectedItem = e.RemovedItems[0];
            }
            Load();
            ComboIndex = LanguageCombo.SelectedIndex;
        }
    }
}
