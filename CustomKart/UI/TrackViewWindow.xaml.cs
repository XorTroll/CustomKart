using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;
using MKDS_Course_Modifier.G3D_Binary_File_Format;
using System.Windows.Media.Media3D;
using MarioKart.MKDS.NKM;
using HelixToolkit.Wpf;
using Tao.OpenGl;
using MarioKart.MKDS;
using Microsoft.VisualBasic.FileIO;
using CustomKart.Extras;
using System.IO;

using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using IOPath = System.IO.Path;

namespace CustomKart.UI
{
    public class ModelInfo
    {
        public string ModelFile { get; set; }

        public string TextureFile { get; set; }
    }

    public class Model
    {
        public CARCEditor ModelCARC { get; set; }

        public CARCEditor TextureCARC { get; set; }

        public byte[] NSBMD { get; set; }

        public byte[] NSBTX { get; set; }

        public string Path { get; set; }

        public ModelInfo Info { get; set; }

        public bool HasTexture => Info.TextureFile != null;

        private void LoadData()
        {
            NSBMD = (ModelCARC != null) ? ModelCARC.ReadFile(Info.ModelFile) : ROMUtils.ROM.ReadFile(Info.ModelFile);

            if(HasTexture) NSBTX = (TextureCARC != null) ? TextureCARC.ReadFile(Info.TextureFile) : ROMUtils.ROM.ReadFile(Info.TextureFile);
        }

        private void SaveData()
        {
            if(ModelCARC != null) ModelCARC.WriteFile(Info.ModelFile, NSBMD);
            else ROMUtils.ROM.WriteFile(Info.ModelFile, NSBMD);

            if (HasTexture)
            {
                if (TextureCARC != null) TextureCARC.WriteFile(Info.TextureFile, NSBTX);
                else ROMUtils.ROM.WriteFile(Info.TextureFile, NSBTX);
            }
        }

        public Model(ModelInfo info, CARCEditor model_carc, CARCEditor texture_carc)
        {
            ModelCARC = model_carc;
            TextureCARC = texture_carc;
            Path = null;
            Info = info;
            LoadData();
        }

        public bool Export()
        {
            var nsbmd = new NSBMD(NSBMD);
            if(nsbmd == null) return false;

            if(HasTexture)
            {
                var nsbtx = new NSBTX(NSBTX);
                if(nsbtx == null) return false;
                nsbmd.TexPlttSet = nsbtx.TexPlttSet;
            }

            var path = IOPath.Combine(Utils.PrepareTempDirectory(), "model.obj");
            GlNitro.glNitroBindTextures(nsbmd, 1);
            nsbmd.modelSet.models[0].ExportMesh(nsbmd.TexPlttSet, path, "PNG");
            Path = path;
            return true;
        }

        public void Import(string path, float scale)
        {
            MKDSCMExtras.GenerateNSBMD(path, scale, HasTexture, out var nsbmd_data, out var nsbtx_data);

            var new_path = IOPath.Combine(Utils.PrepareTempDirectory(), "model.obj");
            MKDSCMExtras.ScaleOBJ(path, new_path, scale);
            Path = new_path;
            NSBMD = nsbmd_data;
            NSBTX = nsbtx_data;
        }

        public void Save()
        {
            SaveData();
            if (ModelCARC != null) ModelCARC.Save();
            if (TextureCARC != null) TextureCARC.Save();
        }
    }

    /// <summary>
    /// Lógica de interacción para TrackViewWindow.xaml
    /// </summary>
    public partial class TrackViewWindow : Window
    {
        public TrackViewWindow()
        {
            InitializeComponent();
        }

        private ModelInfo TrackModelInfo = new ModelInfo { ModelFile = "course_model.nsbmd", TextureFile = "course_model.nsbtx" };
        private ModelInfo SkyboxModelInfo = new ModelInfo { ModelFile = "course_model_V.nsbmd", TextureFile = "course_model_V.nsbtx" };

        private CARCEditor MainCARC = null;
        private NKMD TrackNKM = null;
        private KCL TrackKCL = null;

        private List<Model> ItemModels = new List<Model>();
        private List<Model> MainModels = new List<Model>();
        private List<Vector3D> ItemModelTranslates = new List<Vector3D>();

        protected override void OnClosing(CancelEventArgs e)
        {
            switch(Utils.ShowYesNoMessage(Utils.GetResource("common:saveChanges"), Utils.GetResource("common:warn")))
            {
                case MessageBoxResult.Yes:
                    {
                        var ktps_pos = ItemModelTranslates[0];
                        var pos = TrackNKM.KartPointStart.Entries[0].Position;
                        pos.X = (float)(ktps_pos.X * 16);
                        pos.Y = (float)(ktps_pos.Y * 16);
                        pos.Z = (float)(ktps_pos.Z * 16);
                        TrackNKM.KartPointStart.Entries[0].Position = pos;

                        MainCARC.WriteFile("course_map.nkm", TrackNKM.Write());
                        MainCARC.Save();

                        foreach (var model in MainModels)
                        {
                            model.Save();
                        }
                        break;
                    }
                case MessageBoxResult.Cancel:
                    {
                        e.Cancel = true;
                        break;
                    }
            }

            base.OnClosed(e);
        }

        private void ExportPrepareTrack(int index)
        {
            var name = MKDS.InternalTrackNames[index];
            MainCARC = new CARCEditor(name + ".carc");
            var texture_carc = new CARCEditor(name + "Tex.carc");

            var track_model = new Model(TrackModelInfo, MainCARC, texture_carc);
            if(track_model.Export())
            {
                MainModelCombo.Items.Add("Track");
                MainModelCombo.SelectedIndex = 0;
                MainModels.Add(track_model);
                LoadModelIntoView(track_model);
            }
            else Utils.ShowMessage("Unable to load track model.");

            var skybox_model = new Model(SkyboxModelInfo, MainCARC, texture_carc);
            if (skybox_model.Export())
            {
                MainModelCombo.Items.Add("Track skybox");
                MainModelCombo.SelectedIndex = 0;
                MainModels.Add(skybox_model);
                LoadModelIntoView(skybox_model);
            }
            else Utils.ShowMessage("Unable to load track skybox model.");

            var nkm_data = MainCARC.ReadFile("course_map.nkm");
            TrackNKM = new NKMD(nkm_data);

            var kcl_data = MainCARC.ReadFile("course_collision.kcl");
            TrackKCL = new KCL(kcl_data);

            var dummy_kart_model_info = new ModelInfo { ModelFile = "kart_MR_a.nsbmd", TextureFile = "kart_MR_a.nsbtx" };
            var dummy_kart_model = new Model(dummy_kart_model_info, null, null);
            if (dummy_kart_model.Export())
            {
                ModelCombo.Items.Add("Start position");
                ModelCombo.SelectedIndex = 0;
                ItemModels.Add(dummy_kart_model);
                LoadModelIntoView(dummy_kart_model);

                var ktps = TrackNKM.KartPointStart.Entries[0];
                ItemModelTranslates.Add(new Vector3D(ktps.Position.X / 16, ktps.Position.Y / 16, ktps.Position.Z / 16));
                UpdateModelTranslate(0, ktps.Position.X / 16, ktps.Position.Y / 16, ktps.Position.Z / 16, true);
                UpdateModelRotation(0, ktps.Rotation.X, ktps.Rotation.Y, ktps.Rotation.Z);
            }
            else Utils.ShowMessage("Unable to load demo kart model.");
        }

        public Point3D GetCenter(Model3D model, double translate_x, double translate_y, double translate_z)
        {
            var bounds = model.Bounds;
            var center = new Point3D(bounds.X + translate_x + bounds.SizeX / 2, bounds.Y + translate_y + bounds.SizeY / 2, bounds.Z + translate_z + bounds.SizeZ / 2);
            return center;
        }

        private bool LoadModelIntoView(Model model)
        {
            try
            {
                // Change this so that ObjReader parses inf values properly
                var obj_data = File.ReadAllText(model.Path);
                var new_obj_data = obj_data.Replace("∞", "Infinity");
                File.WriteAllText(model.Path, new_obj_data);

                var model_group = new ObjReader().Read(model.Path);
                var model_visual = new ModelVisual3D { Content = model_group, Transform = new Transform3DGroup { Children = { new TranslateTransform3D(0, 0, 0), new RotateTransform3D(), new RotateTransform3D(), new RotateTransform3D() } } };
                Model.Children.Add(model_visual);
                return true;
            }
            catch(Exception e)
            {
                Utils.ShowMessage(e.Message);
            }
            return false;
        }

        private void UpdateModelTranslate(int idx, double x, double y, double z, bool absolute)
        {
            var index = idx + MainModelCombo.Items.Count;
            var translate = ((Model.Children[index] as ModelVisual3D).Transform as Transform3DGroup).Children[0] as TranslateTransform3D;
            var orig_translate = ItemModelTranslates[idx];
            if(absolute)
            {
                translate.OffsetX = x;
                translate.OffsetY = y;
                translate.OffsetZ = z;
            }
            else
            {
                translate.OffsetX += x;
                translate.OffsetY += y;
                translate.OffsetZ += z;
                orig_translate.X -= x;
                orig_translate.Y += y;
                orig_translate.Z -= z;
            }
            ItemModelTranslates[idx] = orig_translate;
            ((Model.Children[index] as ModelVisual3D).Transform as Transform3DGroup).Children[0] = translate;
        }

        private Point3D GetModelTranslate(int idx)
        {
            var index = idx + MainModelCombo.Items.Count;
            var translate = ((Model.Children[index] as ModelVisual3D).Transform as Transform3DGroup).Children[0] as TranslateTransform3D;
            return new Point3D(translate.OffsetX, translate.OffsetY, translate.OffsetZ);
        }

        private void UpdateModelRotation(int idx, float x, float y, float z)
        {
            var index = idx + MainModelCombo.Items.Count;
            var translate = ((Model.Children[index] as ModelVisual3D).Transform as Transform3DGroup).Children[0] as TranslateTransform3D;
            var rotate_x = ((Model.Children[index] as ModelVisual3D).Transform as Transform3DGroup).Children[1] as RotateTransform3D;
            var rotate_y = ((Model.Children[index] as ModelVisual3D).Transform as Transform3DGroup).Children[2] as RotateTransform3D;
            var rotate_z = ((Model.Children[index] as ModelVisual3D).Transform as Transform3DGroup).Children[3] as RotateTransform3D;

            var center = GetCenter((Model.Children[index] as ModelVisual3D).Content, translate.OffsetX, translate.OffsetY, translate.OffsetZ);

            rotate_x.CenterX = center.X;
            rotate_x.CenterY = center.Y;
            rotate_x.CenterZ = center.Z;
            rotate_x.Rotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), x);

            rotate_y.CenterX = center.X;
            rotate_y.CenterY = center.Y;
            rotate_y.CenterZ = center.Z;
            rotate_y.Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), y);

            rotate_z.CenterX = center.X;
            rotate_z.CenterY = center.Y;
            rotate_z.CenterZ = center.Z;
            rotate_z.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), z);

            ((Model.Children[index] as ModelVisual3D).Transform as Transform3DGroup).Children[1] = rotate_x;
            ((Model.Children[index] as ModelVisual3D).Transform as Transform3DGroup).Children[2] = rotate_y;
            ((Model.Children[index] as ModelVisual3D).Transform as Transform3DGroup).Children[3] = rotate_z;
        }


        private void SwapMainModel(int index, string path)
        {
            try
            {
                (Model.Children[index] as ModelVisual3D).Content = new ObjReader().Read(path);
            }
            catch
            {
                Utils.ShowMessage("Invalid model swap");
            }
        }

        public void LoadTrack(int index)
        {
            ExportPrepareTrack(index);
        }

        public const double Increment = 0.1;

        private void YIButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateModelTranslate(ModelCombo.SelectedIndex, 0, Increment, 0, false);
        }

        private void YDButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateModelTranslate(ModelCombo.SelectedIndex, 0, -Increment, 0, false);
        }

        private void ZDButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateModelTranslate(ModelCombo.SelectedIndex, 0, 0, -Increment, false);
        }

        private void ZIButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateModelTranslate(ModelCombo.SelectedIndex, 0, 0, Increment, false);
        }

        private void XIButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateModelTranslate(ModelCombo.SelectedIndex, Increment, 0, 0, false);
        }

        private void XDButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateModelTranslate(ModelCombo.SelectedIndex, -Increment, 0, 0, false);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            switch(SettingsGrid.Visibility)
            {
                case Visibility.Visible:
                    SettingsGrid.Visibility = Visibility.Hidden;
                    break;
                case Visibility.Hidden:
                    SettingsGrid.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = Utils.GetResource("tracks:modelExport"),
            };
            if(fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var model = MainModels[MainModelCombo.SelectedIndex];
                FileSystem.CopyDirectory(IOPath.GetDirectoryName(model.Path), fbd.SelectedPath);
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = Utils.GetResource("tracks:modelImport"),
                Filter = Utils.GetResource("common:objModel") + " |*.obj",
                Multiselect = false,
            };
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var model = MainModels[MainModelCombo.SelectedIndex];
                    float scale = 0.0625f;

                    model.Import(ofd.FileName, scale);
                    Utils.ShowMessage("New path: " + model.Path);

                    SwapMainModel(MainModelCombo.SelectedIndex, model.Path);
                }
                catch(Exception ex)
                {
                    Utils.ShowMessage("Invalid model: " + ex.GetType());
                    throw ex;
                }
            }
        }
    }
}
