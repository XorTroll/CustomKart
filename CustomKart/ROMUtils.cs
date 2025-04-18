using System;
using System.Collections.Generic;
using System.Drawing;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using System.IO;
using System.Text;
using NSMBe4;
using NSMBe4.DSFileSystem;

using EFEROM = NDS.Nitro.NDS;
using NSMBeROM = NSMBe4.ROM;
using NSMBeROMFs = NSMBe4.DSFileSystem.NitroROMFilesystem;
using SystemFile = System.IO.File;
using NARCFormat = NDS.NitroSystem.FND.NARC;

namespace CustomKart
{
    public enum MIDIConverter
    {
        Invalid,
        MIDI2SSEQ,
        SmfSeqConv
    }

    public class ROMObject
    {
        public string Path { get; set; }

        public EFEROM ROM { get; set; }

        public SFSDirectory ROMFs { get; set; }

        public NDS.UtilityBin UtilityBin { get; set; }

        public SFSDirectory UtilityBinFs { get; set; }

        public Bitmap Icon { get; set; }

        private void WriteROMIcon()
        {
            const int ImageSize = 512;
            const int PaletteSize = 32;
            // Virtual banner file used in NSMBe simplifies this a lot
            var banner_f = NSMBeROM.getLevelFile("banner");
            var image_f = new InlineFile(banner_f, PaletteSize, ImageSize, banner_f.name);
            var palette_f = new InlineFile(banner_f, ImageSize + PaletteSize, PaletteSize, banner_f.name);
            var image = new Image2D(image_f, 32, true, false);
            var palette = new FilePalette(palette_f);
            image.replaceImgAndPal(Icon, palette);
            Array.Copy(image.GetData(), ROM.Banner.Banner.Image, ImageSize);
            Array.Copy(palette.GetData(), ROM.Banner.Banner.Pltt, PaletteSize);
        }

        private void DoWithNSMBeROM(string path, System.Action callback)
        {
            NSMBeROM.load(new NSMBeROMFs(path));
            callback();
            NSMBeROM.close();
        }

        private void Load()
        {
            // Load ROM icon
            Icon = ROM.Banner.Banner.GetIcon();

            // Load filesystem
            ROMFs = ROM.ToFileSystem();

            // Load utility.bin filesystem
            UtilityBin = new NDS.UtilityBin(ReadFile("utility.bin"));
            UtilityBinFs = UtilityBin.ToFileSystem();
        }

        public byte[] ReadFile(string name)
        {
            var file = ROMUtils.GetSFSFile(name, ROMFs);
            return file.Data;
        }

        public void WriteFile(string name, byte[] data)
        {
            var file = ROMUtils.GetSFSFile(name, ROMFs);
            file.Data = data;
        }

        public byte[] ReadUtilityBinFile(string name)
        {
            var file = ROMUtils.GetSFSFile(name, UtilityBinFs);
            return file.Data;
        }

        public void WriteUtilityBinFile(string name, byte[] data)
        {
            var file = ROMUtils.GetSFSFile(name, UtilityBinFs);
            file.Data = data;
        }

        public byte[] ReadDecompressUtilityBinFile(string name)
        {
            var file = ROMUtils.GetSFSFile(name, UtilityBinFs);
            return NSMBeROM.LZ77_Decompress(file.Data);
        }

        public void WriteCompressUtilityBinFile(string name, byte[] data)
        {
            var file = ROMUtils.GetSFSFile(name, UtilityBinFs);
            file.Data = NSMBeROM.LZ77_Compress(data);
        }

        public ROMObject(string path, EFEROM rom)
        {
            Path = path;
            ROM = rom;
            Load();
        }

        public void Save(string path)
        {
            // Save utility.bin filesystem
            UtilityBin.FromFileSystem(UtilityBinFs);
            WriteFile("utility.bin", UtilityBin.Write());

            // Write ROM icon (uses NSMBe to generate icon data)
            DoWithNSMBeROM(Path, WriteROMIcon);

            // Write filesystem
            ROM.FromFileSystem(ROMFs);

            // Save the ROM
            var data = ROM.Write();
            SystemFile.WriteAllBytes(path, data);
        }
    }

    public static class ROMUtils
    {
        public static ROMObject ROM = null;

        public static void TryLoad(string path)
        {
            var rom = new EFEROM(SystemFile.ReadAllBytes(path));
            // Ensure the filesystem is valid
            rom.ToFileSystem();
            // Check its a valid MKDS ROM
            if(!rom.IsMKDS()) throw new NotMKDSROMException();
            ROM = new ROMObject(path, rom);
        }

        public static bool HasLoadedROM()
        {
            return ROM != null;
        }
        public static MIDIConverter DetermineConverter()
        {
            var cvtr = MIDIConverter.Invalid;
            if(SystemFile.Exists(Path.Combine(Utils.SelfPath, "midi2sseq.exe"))) cvtr = MIDIConverter.MIDI2SSEQ;
            if(SystemFile.Exists(Path.Combine(Utils.SelfPath, "smfconv.exe")) && SystemFile.Exists(Path.Combine(Utils.SelfPath, "seqconv.exe"))) cvtr = MIDIConverter.SmfSeqConv;
            return cvtr;
        }
        public static byte[] GetFile(string Name, SFSDirectory Dir)
        {
            var f = GetSFSFile(Name, Dir);
            byte[] data = null;
            if (f != null) data = f.Data;
            return data;
        }

        public static SFSFile GetSFSFile(string Name, SFSDirectory Dir)
        {
            SFSFile data = null;
            if(Dir.Files.Count > 0)
            {
                foreach(var file in Dir.Files)
                {
                    if(file.FileName == Name)
                    {
                        data = file;
                        break;
                    }
                }
            }
            if(data == null)
            {
                if(Dir.SubDirectories.Count > 0)
                {
                    foreach (var dir in Dir.SubDirectories)
                    {
                        SFSFile data2 = GetSFSFile(Name, dir);
                        if (data2 != null)
                        {
                            data = data2;
                            break;
                        }
                    }
                }
            }
            return data;
        }

        public static string GetBuildDate()
        {
            return Encoding.ASCII.GetString(ROM.ReadFile("builddate.bin"));
        }

        public static Bitmap ConvertCharPaletteIcon(byte[] chr, byte[] plt)
        {
            var bannerv1 = new EFEROM.RomBanner.BannerV1
            {
                Image = chr,
                Pltt = plt
            };
            return bannerv1.GetIcon();
        }

    }
}
