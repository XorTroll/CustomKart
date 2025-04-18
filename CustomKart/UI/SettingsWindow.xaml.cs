using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace CustomKart.UI
{
    /// <summary>
    /// Lógica de interacción para SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        public void LoadSettings()
        {
            foreach(var tex in Settings.Textures)
            {
                var tex_list_item = new DockPanel
                {
                    Width = TexturesList.Width
                };
                tex_list_item.Children.Add(new Label { Content = "Texture name:" });
                tex_list_item.Children.Add(new TextBox { Text = tex.Name });
                tex_list_item.Children.Add(new Separator { Margin = new Thickness(5, 0, 0, 0), Width = 10 });
                tex_list_item.Children.Add(new Label { Content = "CARC archive (empty if not inside one):" });
                tex_list_item.Children.Add(new TextBox { Text = tex.CARC });
                tex_list_item.Children.Add(new Separator { Margin = new Thickness(5, 0, 0, 0), Width = 10 });
                tex_list_item.Children.Add(new Label { Content = "NGCR file:" });
                tex_list_item.Children.Add(new TextBox { Text = tex.NCGR});
                tex_list_item.Children.Add(new Separator { Margin = new Thickness(5, 0, 0, 0), Width = 10 });
                tex_list_item.Children.Add(new Label { Content = "NCLR file:" });
                tex_list_item.Children.Add(new TextBox { Text = tex.NCLR });
                TexturesList.Items.Add(tex_list_item);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            switch(Utils.ShowYesNoMessage(Utils.GetResource("common:saveChanges"), Utils.GetResource("common:warn")))
            {
                case MessageBoxResult.Yes:
                    {
                        var new_texs = new List<Texture>();
                        for(var i = 0; i < TexturesList.Items.Count; i++)
                        {
                            var list_item = TexturesList.Items[i] as DockPanel;
                            var name_box = list_item.Children[1] as TextBox;
                            var carc_box = list_item.Children[4] as TextBox;
                            var ncgr_box = list_item.Children[7] as TextBox;
                            var nclr_box = list_item.Children[10] as TextBox;

                            if(string.IsNullOrEmpty(name_box.Text))
                            {
                                Utils.ShowMessage("Empty name no...");
                                e.Cancel = true;
                                return;
                            }
                            if (string.IsNullOrEmpty(ncgr_box.Text))
                            {
                                Utils.ShowMessage("Empty ncgr no...");
                                e.Cancel = true;
                                return;
                            }
                            if (string.IsNullOrEmpty(nclr_box.Text))
                            {
                                Utils.ShowMessage("Empty nclr no...");
                                e.Cancel = true;
                                return;
                            }

                            new_texs.Add(new Texture(name_box.Text, carc_box.Text, ncgr_box.Text, nclr_box.Text));
                        }
                        Settings.Textures.Clear();
                        Settings.Textures.AddRange(new_texs);
                        Settings.Save();
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
    }
}
