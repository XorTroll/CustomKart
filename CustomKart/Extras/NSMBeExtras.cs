using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomKart
{
    public static class NSMBeExtras
    {
        public static byte[] getData(this NSMBe4.FilePalette Palette)
        {
            var byteArrayOutputStream = new NSMBe4.ByteArrayOutputStream();
            for(int i = 0; i < Palette.pal.Length; i++)
            {
                byteArrayOutputStream.writeUShort(NSMBe4.NSMBTileset.toRGB15(Palette.pal[i]));
            }
            return byteArrayOutputStream.getArray();
        }

        public static byte[] getData(this NSMBe4.Image2D Img)
        {
            byte[] idata;
            byte[] innerdata = Img.getRawData();
            if(Img.is4bpp)
            {
                idata = new byte[innerdata.Length / 2];
                for(int i = 0; i < idata.Length; i++)
                {
                    idata[i] = (byte)((innerdata[i * 2] & 15) | (innerdata[i * 2 + 1] & 15) << 4);
                }
            }
            else
            {
                idata = (byte[])innerdata.Clone();
            }
            return idata;
        }
    }
}
