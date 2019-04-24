using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using System.IO;
using System.Reflection;

namespace CustomKart
{
    public enum MIDIConverter
    {
        Invalid,
        MIDI2SSEQ,
        SmfSeqConv
    }

    public static class ROMUtils
    {
        public static string GetExePath()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public static NDS.Nitro.NDS LoadROM(string Path)
        {
            NDS.Nitro.NDS rom;
            try
            {
                rom = new NDS.Nitro.NDS(File.ReadAllBytes(Path));
                var tmpfs = rom.ToFileSystem();
            }
            catch
            {
                rom = null;
            }
            return rom;
        }

        public static MIDIConverter DetermineConverter()
        {
            MIDIConverter cvtr = MIDIConverter.Invalid;
            string curdir = GetExePath();
            if(File.Exists(curdir + "\\midi2sseq.exe")) cvtr = MIDIConverter.MIDI2SSEQ;
            if(File.Exists(curdir + "\\smfconv.exe") && File.Exists(curdir + "\\seqconv.exe")) cvtr = MIDIConverter.SmfSeqConv;
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
                if (Dir.SubDirectories.Count > 0)
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

    }
}
