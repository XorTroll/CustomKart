using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSMBe4.DSFileSystem;

namespace CustomKart
{
    public static class NSMBeExtras
    {
        public static byte[] GetData(this NSMBe4.FilePalette Palette)
        {
            var byteArrayOutputStream = new NSMBe4.ByteArrayOutputStream();
            for(int i = 0; i < Palette.pal.Length; i++)
            {
                byteArrayOutputStream.writeUShort(NSMBe4.NSMBTileset.toRGB15(Palette.pal[i]));
            }
            return byteArrayOutputStream.getArray();
        }

        public static byte[] GetData(this NSMBe4.Image2D Img)
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

    public class VirtualInlineFile : FileWithLock
    {
        private int inlineOffs;

        private int inlineLen;

        private byte[] vdata;

        public VirtualInlineFile(byte[] data, int offs, int len, string name)
        {
            nameP = name;
            fileSizeP = len;
            vdata = data;
            inlineOffs = offs;
            inlineLen = len;
        }

        public override byte[] getContents()
        {
            return vdata.Skip(inlineOffs).Take(inlineLen).ToArray();
        }

        public override void replace(byte[] newFile, object editor)
        {
            if (newFile.Length != inlineLen)
            {
                throw new Exception("Trying to resize an InlineFile: " + base.name);
            }
            replaceInterval(newFile, 0);
        }

        public override byte[] getInterval(int start, int end)
        {
            validateInterval(start, end);
            return vdata.Skip(inlineOffs + start).Take(end - (inlineOffs + start)).ToArray();
        }

        public override void replaceInterval(byte[] newFile, int start)
        {
            validateInterval(start, start + newFile.Length);
            Array.Copy(newFile, 0, vdata, inlineOffs + start, newFile.Length);
        }

        public byte[] GetData()
        {
            return vdata;
        }
    }
}
