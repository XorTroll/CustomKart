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
        public static string GetResource(string key)
        {
            return Application.Current.FindResource(key) as string;
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

        public static string GetNDSFilter()
        {
            return GetResource("common:ndsROM") + " (" + GetResource("common:commonFormat") + ")|*.nds|" + GetResource("common:ndsROM") + " (" + GetResource("common:officialFormat") + ")|*.srl";
        }

        public static void UpdateImageWithBitmap(ref System.Windows.Controls.Image img, Bitmap bmp)
        {
            using(MemoryStream memory = new MemoryStream())
            {
                bmp.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                img.Source = bitmapimage;
            }
        }

        public static string GetSelfPath()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public static string GetTempDirectory()
        {
            return Path.Combine(GetSelfPath(), "_temp");
        }

        public static void CleanTempDirectories()
        {
            try
            {
                var dirs = Directory.GetDirectories(GetTempDirectory());
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

        public static string PrepareTempDirectory()
        {
            var tmp = GetTempDirectory();
            var new_dir = Path.Combine(tmp, Path.GetRandomFileName());
            Directory.CreateDirectory(new_dir);
            return new_dir;
        }
    }
}
