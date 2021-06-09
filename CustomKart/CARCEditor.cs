using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibEveryFileExplorer.Files.SimpleFileSystem;

using NSMBeROM = NSMBe4.ROM;
using NARCFormat = NDS.NitroSystem.FND.NARC;

namespace CustomKart
{
    public class CARCEditor
    {
        // Note: CARC = compressed NARC (LZ77)

        public string Name { get; set; }

        public NARCFormat NARC { get; set; }

        public SFSDirectory Fs { get; set; }

        private void LoadNARC()
        {
            var data = ROMUtils.ROM.ReadFile(Name);
            var data_dec = NSMBeROM.LZ77_Decompress(data);
            NARC = new NARCFormat(data_dec);
            Fs = NARC.ToFileSystem();
        }

        public CARCEditor(string name)
        {
            Name = name;
            LoadNARC();
        }

        public byte[] ReadFile(string name)
        {
            var file = ROMUtils.GetSFSFile(name, Fs);
            return file.Data;
        }

        public void WriteFile(string name, byte[] data)
        {
            var file = ROMUtils.GetSFSFile(name, Fs);
            file.Data = data;
        }

        public void Save()
        {
            NARC.FromFileSystem(Fs);
            var dec_data = NARC.Write();
            var data = NSMBeROM.LZ77_Compress(dec_data);
            ROMUtils.ROM.WriteFile(Name, data);
        }
    }
}
