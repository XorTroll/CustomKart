using System.IO;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CustomKart
{
    public static class Utils
    {
        public const string ProgramDirectory = "ckart";

        public static string GetResource(string key)
        {
            return GetResourceAs<string>(key);
        }

        public static T GetResourceAs<T>(string key) where T : class
        {
            return Application.Current.FindResource(key) as T;
        }

        public static void ShowMessage(string msg)
        {
            (Application.Current.MainWindow as UI.MainWindow).ShowMessage(msg);
        }

        public static MessageBoxResult ShowYesNoMessage(string msg, string caption)
        {
            return MessageBox.Show(msg, GetResource("common:program") + " - " + caption, MessageBoxButton.YesNoCancel);
        }

        public static string NDSFilter => GetResource("common:ndsROM") + " (" + GetResource("common:commonFormat") + ")|*.nds|" + GetResource("common:ndsROM") + " (" + GetResource("common:officialFormat") + ")|*.srl";

        public static void UpdateImageWithBitmap(ref System.Windows.Controls.Image img, Bitmap bmp)
        {
            using(var memory = new MemoryStream())
            {
                bmp.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmap_img = new BitmapImage();
                bitmap_img.BeginInit();
                bitmap_img.StreamSource = memory;
                bitmap_img.CacheOption = BitmapCacheOption.OnLoad;
                bitmap_img.EndInit();
                img.Source = bitmap_img;
            }
        }

        public static string SelfPath => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public static string FullProgramDirectory => Path.Combine(SelfPath, ProgramDirectory);

        public static string TempDirectory => Path.Combine(SelfPath, "_temp");

        public static void CleanTempDirectories()
        {
            try
            {
                var dirs = Directory.GetDirectories(TempDirectory);
                foreach (var dir in dirs)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static void EnsureDirectories()
        {
            try
            {
                Directory.CreateDirectory(FullProgramDirectory);
            }
            catch { }
        }

        public static string MakeTempDirectory()
        {
            var new_dir = Path.Combine(TempDirectory, Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(new_dir);
            }
            catch { }
            return new_dir;
        }
    }
}
