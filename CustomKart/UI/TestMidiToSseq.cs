using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NAudio.Midi;
using MKDS_Course_Modifier.Sound;
using MKDS_Course_Modifier.Sound.SSEQEvents;

namespace CustomKart.UI
{
    public static class TestMidiToSseq
    {
		public static byte[] Save(MidiFile midiFile, SSEQ seq_base)
		{
			var Events = new List<SSEQEvent>();
			Events.Add(new SSEQAllocTrackEvent(midiFile.Tracks));
			for (int i = 0; i < midiFile.Tracks - 1; i++)
			{
				Events.Add(new SSEQTrackEvent(i + 1));
			}
			foreach (var m_event in midiFile.Events)
			{
				List<SSEQEvent> list = new List<SSEQEvent>();
				foreach (MidiEvent item in m_event)
				{
					if (item is NoteOnEvent n_item)
					{
						if (n_item.OffEvent == null) n_item.OffEvent = new NoteEvent(n_item.AbsoluteTime + 50, n_item.Channel, MidiCommandCode.NoteOff, n_item.NoteNumber, 64);
						list.Add(new SSEQNoteEvent(n_item));
					}
				}
				Events.AddRange(list);
			}

			var mem = new MemoryStream();
			var ebw = new EndianBinaryWriter(mem, Endianness.LittleEndian);

			seq_base.Header.Write(ebw);

			var mem_2= new MemoryStream();
			var ebw_2 = new EndianBinaryWriter(mem_2, Endianness.LittleEndian);
			var base_off = ebw.BaseStream.Position;
			foreach(var evt in Events)
            {
				if(evt is SSEQAllocTrackEvent alloc_t_evt)
                {
					alloc_t_evt.Write(ebw_2);
                }
				else if (evt is SSEQTrackEvent t_evt)
				{
					t_evt.Write(ebw_2);
				}
				else if (evt is SSEQNoteEvent n_evt)
				{
					n_evt.Write(ebw_2);
				}
				// TODO: more events
			}
			seq_base.Data.Header.Write(ebw, (int)ebw_2.BaseStream.Position);
			foreach(var b in mem_2.ToArray())
            {
				ebw.Write(b);
            }
			ebw_2.Close();

			var data = mem.ToArray();
			ebw.Close();
			return data;
		}

		public static void Write(this SSEQAllocTrackEvent evt, EndianBinaryWriter ebw)
        {
			ebw.Write(evt.EventID);
			ebw.Write(evt.TracksUsed);
		}

		public static void WriteLEB128Unsigned(this EndianBinaryWriter ebw, ulong value, out int bytes)
		{
			bytes = 0;
			bool more = true;

			while (more)
			{
				byte chunk = (byte)(value & 0x7fUL); // extract a 7-bit chunk
				value >>= 7;

				more = value != 0;
				if (more) { chunk |= 0x80; } // set msb marker that more bytes are coming

				ebw.Write(chunk);
				bytes += 1;
			};
		}

		public static void Write(this SSEQTrackEvent evt, EndianBinaryWriter ebw)
        {
			ebw.Write(evt.EventID);
			ebw.Write(evt.TrackNr);
			var u32_bytes = BitConverter.GetBytes(evt.Offset);
			var bytes = new byte[3];
			Array.Copy(u32_bytes, bytes, bytes.Length);
			foreach(var b in bytes)
            {
				ebw.Write(b);
            }
        }

		public static void Write(this SSEQNoteEvent evt, EndianBinaryWriter ebw)
        {
			ebw.Write(evt.EventID);
			ebw.Write(evt.Velocity);
			ebw.WriteLEB128Unsigned((uint)evt.Duration, out _);
        }
	}
}
