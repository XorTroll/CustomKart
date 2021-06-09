using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomKart
{
    public class Texture
    {
        public string Name { get; set; }

        public string CARC { get; set; }

        public string NCGR { get; set; }

        public string NCLR { get; set; }

        public bool IsInCARC => !string.IsNullOrEmpty(CARC);

        public Texture(string name, string carc, string ncgr, string nclr)
        {
            Name = name;
            CARC = carc;
            NCGR = ncgr;
            NCLR = nclr;
        }
    };

    public static class MKDS
    {
        public static int SequenceCount = 76;

        public static List<string> InternalTrackNames = new List<string>
        {
            "cross_course",
            "mansion_course",
            "beach_course",
            "town_course",
        };

        public static List<Texture> Textures = new List<Texture>();

        static MKDS()
        {
            // Misc
            Textures.Add(new Texture("title", "Title_es.carc", "title_m_b.NCGR", "title_m_b.NCLR"));
            Textures.Add(new Texture("select_menu_etc", "CharacterKartSelect.carc", "select_menu_etc_m_b.NCGR", "select_menu_etc_m_b.NCLR"));
            Textures.Add(new Texture("thanksYou_0", "Ending.carc", "thanksYou_m_b_00.NCGR", "thanksYou_m_b_00.NCLR"));
            Textures.Add(new Texture("thanksYou_1", "Ending.carc", "thanksYou_m_b_01.NCGR", "thanksYou_m_b_01.NCLR"));
            Textures.Add(new Texture("logo_end_marioKart_0", "Ending.carc", "logo_end_marioKart_m_b_00.NCGR", "logo_end_marioKart_m_b.NCLR"));
            Textures.Add(new Texture("logo_end_marioKart_1", "Ending.carc", "logo_end_marioKart_m_b_01.NCGR", "logo_end_marioKart_m_b.NCLR"));
            Textures.Add(new Texture("logo_end_marioKart_2", "Ending.carc", "logo_end_marioKart_m_b_02.NCGR", "logo_end_marioKart_m_b.NCLR"));

            // Cup pictures
            for(var i = 0; i < 16; i++)
            {
                var base_name = "select_cup_";
                var base_nitro_name = base_name + "nitro" + (i + 1).ToString("00");
                var base_retro_name = base_name + "retro" + (i + 1).ToString("00");
                Textures.Add(new Texture(base_nitro_name, "", base_nitro_name + "_m_b.NCGR", base_nitro_name + "_m_b.NCLR"));
                Textures.Add(new Texture(base_retro_name, "", base_retro_name + "_m_b.NCGR", base_retro_name + "_m_b.NCLR"));
            }
        }
    }
}
