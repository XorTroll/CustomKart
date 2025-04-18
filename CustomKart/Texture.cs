using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSMBe4;

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

    public class LoadedTexture
    {
        public Texture Data { get; set; }

        public CARCEditor CARC { get; set; }

        public VirtualInlineFile SpriteFile { get; set; }

        public Image2D Sprite { get; set; }

        public VirtualInlineFile PaletteFile { get; set; }

        public Palette Palette { get; set; }

        public LoadedTexture(Texture tex_base)
        {
            Data = tex_base;
        }
    }
}
