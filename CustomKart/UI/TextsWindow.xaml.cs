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
using Newtonsoft.Json.Linq;

namespace CustomKart.UI
{
    public static class BMGExtensions
    {
        public static void LoadFromList(this BMG bmg, ListView list)
        {
            var i = 0;
            foreach (TextBox box in list.Items)
            {
                bmg.DAT1.Strings[i] = box.Text;
                i++;
            }
        }
    }

    /// <summary>
    /// Lógica de interacción para TextsWindow.xaml
    /// </summary>
    public partial class TextsWindow : Window
    {
        public TextsWindow()
        {
            InitializeComponent();

            Static2D = new CARCEditor("Static2D.carc");

            for(var i = 0; i < CARCLanguages.Length; i++)
            {
                var main2d = new CARCEditor(FormatLanguageFile("Main2D", "carc", i));
                Main2D.Add(main2d);
                var common = new BMG(main2d.ReadFile("common.bmg"));
                Common.Add(common);

                var mbchild = new BMG(Static2D.ReadFile(FormatLanguageFile("MBChild", "bmg", i)));
                MBChild.Add(mbchild);

                var chksel = new CARCEditor(FormatLanguageFile("CharacterKartSelect", "carc", i));
                CharacterKartSelect.Add(chksel);
                var kartsel = new BMG(chksel.ReadFile("kart_select.bmg"));
                KartSelect.Add(kartsel);

                var wlmenu = new CARCEditor(FormatLanguageFile("WLMenu", "carc", i));
                WLMenu.Add(wlmenu);
                var banner = new BMG(wlmenu.ReadFile("banner.bmg"));
                Banner.Add(banner);

                var menu = new CARCEditor(FormatLanguageFile("Menu", "carc", i));
                Menu.Add(menu);
                var menu_bmg = new BMG(menu.ReadFile("menu.bmg"));
                MenuBMG.Add(menu_bmg);
                var mission = new BMG(menu.ReadFile("mission.bmg"));
                Mission.Add(mission);
                var rule = new BMG(menu.ReadFile("rule.bmg"));
                Rule.Add(rule);
            }

            Load();
        }

        private void LoadBMGOnList(ref ListView list, BMG bmg)
        {
            list.Items.Clear();
            foreach (var str in bmg.DAT1.Strings)
            {
                list.Items.Add(new TextBox
                {
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    Text = str,
                });
            }
        }

        private List<CARCEditor> Main2D = new List<CARCEditor>();
        private List<BMG> Common = new List<BMG>();

        private CARCEditor Static2D = null;
        private List<BMG> MBChild = new List<BMG>();

        private List<CARCEditor> CharacterKartSelect = new List<CARCEditor>();
        private List<BMG> KartSelect = new List<BMG>();

        private List<CARCEditor> WLMenu = new List<CARCEditor>();
        private List<BMG> Banner = new List<BMG>();

        private List<CARCEditor> Menu = new List<CARCEditor>();
        private List<BMG> MenuBMG = new List<BMG>();
        private List<BMG> Mission = new List<BMG>();
        private List<BMG> Rule = new List<BMG>();

        private int LastLanguageIndex = -1;

        private readonly string[] CARCLanguages = new string[]
        {
            "us",
            "es",
            "fr",
            "ge",
            "it"
        };

        private string FormatLanguageFile(string name, string ext, int lang_idx)
        {
            return name + "_" + CARCLanguages[lang_idx] + "." + ext;
        }

        private void UpdateBMGs()
        {
            if(LastLanguageIndex > -1)
            {
                var lang_idx = LastLanguageIndex;

                Common[lang_idx].LoadFromList(CommonTextsList);
                MBChild[lang_idx].LoadFromList(MBChildTextsList);
                KartSelect[lang_idx].LoadFromList(KartSelectTextsList);
                Banner[lang_idx].LoadFromList(DlPlayTextsList);
                MenuBMG[lang_idx].LoadFromList(BattleTextsList);
                Mission[lang_idx].LoadFromList(MissionTextsList);
                Rule[lang_idx].LoadFromList(RuleTextsList);
            }
        }

        private void Save()
        {
            UpdateBMGs();
            for(var i = 0; i < CARCLanguages.Length; i++)
            {
                Main2D[i].WriteFile("common.bmg", Common[i].Save());
                Main2D[i].Save();

                Static2D.WriteFile(FormatLanguageFile("MBChild", "bmg", i), MBChild[i].Save());

                CharacterKartSelect[i].WriteFile("kart_select.bmg", KartSelect[i].Save());
                CharacterKartSelect[i].Save();

                WLMenu[i].WriteFile("banner.bmg", Banner[i].Save());
                WLMenu[i].Save();

                Menu[i].WriteFile("menu.bmg", MenuBMG[i].Save());
                Menu[i].WriteFile("mission.bmg", Mission[i].Save());
                Menu[i].WriteFile("rule.bmg", Rule[i].Save());
                Menu[i].Save();
            }
            Static2D.Save();
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
            LanguageCombo.SelectedIndex = 0;
        }

        private void LoadBMGs()
        {
            var lang_idx = LanguageCombo.SelectedIndex;

            LoadBMGOnList(ref CommonTextsList, Common[lang_idx]);
            LoadBMGOnList(ref MBChildTextsList, MBChild[lang_idx]);
            LoadBMGOnList(ref KartSelectTextsList, KartSelect[lang_idx]);
            LoadBMGOnList(ref DlPlayTextsList, Banner[lang_idx]);
            LoadBMGOnList(ref BattleTextsList, MenuBMG[lang_idx]);
            LoadBMGOnList(ref MissionTextsList, Mission[lang_idx]);
            LoadBMGOnList(ref RuleTextsList, Rule[lang_idx]);
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBMGs();
            LoadBMGs();
            LastLanguageIndex = LanguageCombo.SelectedIndex;
        }

        private void ExportListJSON(string out_dir, string json_name, ref ListView list)
        {
            var json_array = new JArray();
            foreach (TextBox box in list.Items)
            {
                json_array.Add(box.Text);
            }
            var path = System.IO.Path.Combine(out_dir, json_name + ".json");
            MessageBox.Show(json_array.ToString());
            File.WriteAllText(path, json_array.ToString());
        }

        private void ImportListJSON(string json_dir, string json_name, ref ListView list)
        {
            var path = System.IO.Path.Combine(json_dir, json_name + ".json");
            var json_data = File.ReadAllText(path);
            var json_array = JArray.Parse(json_data);
            for(var i = 0; i < json_array.Count; i++)
            {
                (list.Items[i] as TextBox).Text = json_array[i].ToString();
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select folder",
                ShowNewFolderButton = true
            };
            if(fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExportListJSON(fbd.SelectedPath, "common", ref CommonTextsList);
                ExportListJSON(fbd.SelectedPath, "mbchild", ref MBChildTextsList);
                ExportListJSON(fbd.SelectedPath, "kart_select", ref KartSelectTextsList);
                ExportListJSON(fbd.SelectedPath, "dl_play", ref DlPlayTextsList);
                ExportListJSON(fbd.SelectedPath, "battle", ref BattleTextsList);
                ExportListJSON(fbd.SelectedPath, "mission", ref MissionTextsList);
                ExportListJSON(fbd.SelectedPath, "rule", ref RuleTextsList);
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select folder",
                ShowNewFolderButton = true
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportListJSON(fbd.SelectedPath, "common", ref CommonTextsList);
                ImportListJSON(fbd.SelectedPath, "mbchild", ref MBChildTextsList);
                ImportListJSON(fbd.SelectedPath, "kart_select", ref KartSelectTextsList);
                ImportListJSON(fbd.SelectedPath, "dl_play", ref DlPlayTextsList);
                ImportListJSON(fbd.SelectedPath, "battle", ref BattleTextsList);
                ImportListJSON(fbd.SelectedPath, "mission", ref MissionTextsList);
                ImportListJSON(fbd.SelectedPath, "rule", ref RuleTextsList);
            }
        }
    }
}
