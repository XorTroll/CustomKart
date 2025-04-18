using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace CustomKart
{

    public class KartOffsetData
    {
        public static int KartCount = 37;
        public static int CharacterCount = 13;

        public class Entry
        {
            public string WheelsName { get; set; }

            public int WheelsSize { get; set; }

            public Point3D FrontLeftWheelOffset { get; set; }

            public Point3D FrontRightWheelOffset { get; set; }

            public Point3D BackLeftWheelOffset { get; set; }

            public Point3D BackRightWheelOffset { get; set; }

            public List<Point3D> CharacterOffsets { get; set; }

            public Entry()
            {
                CharacterOffsets = new List<Point3D>();
            }
        }

        public List<Entry> Entries { get; set; }

        public KartOffsetData()
        {
            Entries = new List<Entry>();
        }

        public KartOffsetData(byte[] data)
        {
            Entries = new List<Entry>();
            var bin_reader = new BinaryReader(new MemoryStream(data));
            for(var i = 0; i < KartCount; i++)
            {
                var kart_entry = new Entry();
                
                kart_entry.WheelsName = Encoding.ASCII.GetString(bin_reader.ReadBytes(0x10));
                kart_entry.WheelsSize = bin_reader.ReadInt32();

                var fl_x = bin_reader.ReadInt32();
                var fl_y = bin_reader.ReadInt32();
                var fl_z = bin_reader.ReadInt32();
                kart_entry.FrontLeftWheelOffset = new Point3D(fl_x, fl_y, fl_z);

                var fr_x = bin_reader.ReadInt32();
                var fr_y = bin_reader.ReadInt32();
                var fr_z = bin_reader.ReadInt32();
                kart_entry.FrontRightWheelOffset = new Point3D(fr_x, fr_y, fr_z);

                var bl_x = bin_reader.ReadInt32();
                var bl_y = bin_reader.ReadInt32();
                var bl_z = bin_reader.ReadInt32();
                kart_entry.BackLeftWheelOffset = new Point3D(bl_x, bl_y, bl_z);

                var br_x = bin_reader.ReadInt32();
                var br_y = bin_reader.ReadInt32();
                var br_z = bin_reader.ReadInt32();
                kart_entry.BackRightWheelOffset = new Point3D(br_x, br_y, br_z);

                for(var j = 0; j < CharacterCount; j++)
                {
                    var ch_x = bin_reader.ReadInt32();
                    var ch_y = bin_reader.ReadInt32();
                    var ch_z = bin_reader.ReadInt32();
                    kart_entry.CharacterOffsets.Add(new Point3D(ch_x, ch_y, ch_z));
                }

                Entries.Add(kart_entry);
            }
        }

        public byte[] Save()
        {
            var out_data = new List<byte>();
            foreach(var kart_entry in Entries)
            {
                out_data.AddRange(Encoding.ASCII.GetBytes(kart_entry.WheelsName.Substring(0, 0x10)));
                if(kart_entry.WheelsName.Length < 0x10)
                {
                    for(var i = 0; i < (0x10 - kart_entry.WheelsName.Length); i++)
                    {
                        out_data.Add(0);
                    }
                }
                out_data.AddRange(BitConverter.GetBytes(kart_entry.WheelsSize));

                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.FrontLeftWheelOffset.X));
                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.FrontLeftWheelOffset.Y));
                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.FrontLeftWheelOffset.Z));

                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.FrontRightWheelOffset.X));
                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.FrontRightWheelOffset.Y));
                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.FrontRightWheelOffset.Z));

                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.BackLeftWheelOffset.X));
                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.BackLeftWheelOffset.Y));
                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.BackLeftWheelOffset.Z));

                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.BackRightWheelOffset.X));
                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.BackRightWheelOffset.Y));
                out_data.AddRange(BitConverter.GetBytes((int)kart_entry.BackRightWheelOffset.Z));

                foreach(var char_offset in kart_entry.CharacterOffsets)
                {
                    out_data.AddRange(BitConverter.GetBytes((int)char_offset.X));
                    out_data.AddRange(BitConverter.GetBytes((int)char_offset.Y));
                    out_data.AddRange(BitConverter.GetBytes((int)char_offset.Z));
                }
            }
            return out_data.ToArray();
        }
    }
}
