using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LibEveryFileExplorer.Files.SimpleFileSystem;

namespace CustomKart
{
    public static class NDSExtras
    {
        public static string GetTitleNameByLanguage(this NDS.Nitro.NDS ROM)
        {
            int idx = 1;
            string lang = Thread.CurrentThread.CurrentUICulture.Name;
            if (lang.StartsWith("ja-")) idx = 0;
            else if (lang.StartsWith("fr-")) idx = 2;
            else if (lang.StartsWith("de-")) idx = 3;
            else if (lang.StartsWith("it-")) idx = 4;
            else if (lang.StartsWith("es-")) idx = 5;
            return ROM.Banner.Banner.GameName[idx];
        }

        public static bool ExistsFile(this NDS.Nitro.NDS ROM, string Name, SFSDirectory Dir)
        {
            bool ex = false;
            if(Dir.Files.Count > 0)
            {
                foreach(var file in Dir.Files)
                {
                    if(file.FileName == Name)
                    {
                        ex = true;
                        break;
                    }
                }
            }
            if(!ex)
            {
                if(Dir.SubDirectories.Count > 0)
                {
                    foreach(var dir in Dir.SubDirectories)
                    {
                        bool ex2 = ROM.ExistsFile(Name, dir);
                        if(ex2)
                        {
                            ex = true;
                            break;
                        }
                    }
                }
            }
            return ex;
        }

        public static string[] SomeMKDSFiles = new string[]
        {
            "Main2D.carc",
            "KartModelMain.carc",
            "KartModelMenu.carc",
            "GeneralMenu.carc",
            "CharacterKartSelect.carc",
            "KartModelMainA.carc",
            "KartModelMainB.carc",
            "KartModelSub.carc",
            "MainRace.carc",
            "Static2D.carc",
            "MainEffect.carc",
            "charmenuparam.bin",
            "kartmenuparam.bin",
            "kartoffsetdata.bin",
            "kartphysicalparam.bin",
            "sound_data.sdat",
            "MapObj.carc"
        };

        public static bool IsMKDS(this NDS.Nitro.NDS ROM)
        {
            bool ismk = true;
            SFSDirectory root = ROM.ToFileSystem();
            foreach(var file in SomeMKDSFiles)
            {
                if(!ROM.ExistsFile(file, root))
                {
                    ismk = false;
                    break;
                }
            }
            return ismk;
        }
    }
}
