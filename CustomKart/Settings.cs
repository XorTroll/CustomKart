using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CustomKart
{
    public static class Settings
    {
        public const string SettingsFileName = "settings.json";

        public static List<Texture> Textures { get; set; }

        public static void Load()
        {
            Textures = new List<Texture>();
            var settings_path = Path.Combine(Utils.FullProgramDirectory, SettingsFileName);

#if DEBUG
            CreateDefaultSettings();
            var settings_data = File.ReadAllText(settings_path);
            var settings_json = JObject.Parse(settings_data);
#else
            if (!File.Exists(settings_path))
            {
                CreateDefaultSettings();
            }
            var settings_data = File.ReadAllText(settings_path);
            var settings_json = JObject.Parse(settings_data);
            if (!settings_json.ContainsKey("textures"))
            {
                CreateDefaultSettings();
                settings_data = File.ReadAllText(settings_path);
                settings_json = JObject.Parse(settings_data);
            }
#endif

            foreach (var texture_obj in settings_json["textures"])
            {
                if (texture_obj["name"] == null) continue;
                var name = texture_obj["name"].ToString();
                if (texture_obj["ncgr"] == null) continue;
                var ncgr = texture_obj["ncgr"].ToString();
                if (texture_obj["nclr"] == null) continue;
                var nclr = texture_obj["nclr"].ToString();
                var carc = "";
                if (texture_obj["carc"] != null) carc = texture_obj["carc"].ToString();

                Textures.Add(new Texture(name, carc, ncgr, nclr));
            }
        }

        public static void Save()
        {
            var settings_json = new JObject();
            var textures_array = new JArray();
            foreach (var tex in Textures)
            {
                var tex_json = new JObject
                {
                    ["name"] = tex.Name,
                    ["ncgr"] = tex.NCGR,
                    ["nclr"] = tex.NCLR
                };
                if (tex.IsInCARC) tex_json["carc"] = tex.CARC;
                textures_array.Add(tex_json);
            }
            settings_json["textures"] = textures_array;
            var settings_path = Path.Combine(Utils.FullProgramDirectory, SettingsFileName);
            File.WriteAllText(settings_path, settings_json.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public static void CreateDefaultSettings()
        {
            var settings_path = Path.Combine(Utils.FullProgramDirectory, SettingsFileName);
            try
            {
                File.Delete(settings_path);
            }
            catch { }
            File.WriteAllBytes(settings_path, Properties.Resources.default_settings);
        }
    }
}
