using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MKDS_Course_Modifier.Sound;
using MaterialDesignThemes.Wpf;
using NAudio.Midi;

using IOPath = System.IO.Path;
using System.ComponentModel;

namespace CustomKart.UI
{
    /// <summary>
    /// Lógica de interacción para AudioWindow.xaml
    /// </summary>
    public partial class AudioWindow : Window
    {
        public AudioWindow()
        {
            InitializeComponent();
            for (int i = 0; i < MKDS.SequenceCount; i++)
            {
                SequencesCombo.Items.Add("Sequence no. " + i.ToString());
            }
            Load();
            SequencesCombo.SelectedIndex = 0;
            Reload();
        }

        public static SDAT SoundData { get; set; }

        public static SDAT.InfoBlock.SEQInfo SequenceInfo { get; set; }

        public static SSEQ Sequence { get; set; }

        public static string TempExportedMidi = IOPath.Combine(Utils.TempDirectory, "exported_seq.mid");

        public static SDAT.InfoBlock.BANKInfo BankInfo { get; set; }

        public static SBNK Bank { get; set; }

        public static string TempExportedDLS = IOPath.Combine(Utils.TempDirectory, "exported_bnk.dls");

        public static MusicPlayer Player { get; set; }

        protected override void OnClosing(CancelEventArgs e)
        {
            switch(Utils.ShowYesNoMessage(Utils.GetResource("common:saveChanges"), Utils.GetResource("common:warn")))
            {
                case MessageBoxResult.Yes:
                    {
                        Save();
                        break;
                    }
                case MessageBoxResult.Cancel:
                    {
                        e.Cancel = true;
                        break;
                    }
            }
            base.OnClosing(e);
        }

        public void ResetPlayer()
        {
            if(Player != null)
            {
                Player.UnloadDLS();
                Player.Unload();
            }
            Player = new MusicPlayer();
            Player.SetMidi(TempExportedMidi);
            Player.SetDLS(TempExportedDLS);
        }

        public void Load()
        {
            SoundData = new SDAT(ROMUtils.ROM.ReadFile("sound_data.sdat"));
            ResetPlayer();
        }

        public void Save()
        {
            var sel_idx = SequencesCombo.SelectedIndex;

            ROMUtils.ROM.WriteFile("sound_data.sdat", SoundData.Write());
        }

        private void Reload()
        {
            var sel_idx = SequencesCombo.SelectedIndex;
            SequenceInfo = SoundData.INFO.SEQRecord[sel_idx];
            Sequence = new SSEQ(SoundData.FAT.Records[SequenceInfo.fileID].Data);
            BankInfo = SoundData.INFO.BANKRecord[SequenceInfo.bank];
            Bank = new SBNK(SoundData.FAT.Records[BankInfo.fileID].Data);

            var wave_arcs = new List<SWAR>();
            for (int i = 0; i < 4; i++)
            {
                if (BankInfo.wavearc[i] == ushort.MaxValue)
                {
                    wave_arcs.Add(null);
                }
                else
                {
                    var wave_arc_info = SoundData.INFO.WAVEARCRecord[BankInfo.wavearc[i]];
                    wave_arcs.Add(new SWAR(SoundData.FAT.Records[wave_arc_info.fileID].Data));
                }
            }
            SBNK.InitDLS(Bank, wave_arcs.ToArray());
            File.WriteAllBytes(TempExportedDLS, SBNK.ToDLS(Bank));
        }

        private void SequencesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Store current sequence and bank

            try
            {
                var sseq_data = TestMidiToSseq.Save(new MidiFile(TempExportedMidi), Sequence);
                File.WriteAllBytes("E:\\sample.sseq", sseq_data);
                Utils.ShowMessage("Done");
            }
            catch
            {
                Utils.ShowMessage("Meh... bad");
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var midi = Sequence.Data.Midi;
            midi.PrepareForExport();
            MidiFile.Export(TempExportedMidi, midi);
            
            Player.Play();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop();
        }

        private void SequenceImportButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = Utils.GetResource("sound:sequenceReplace"),
                Filter = Utils.GetResource("sound:midiFile") + " |*.mid|" + Utils.GetResource("sound:sseqFile") + " (SSEQ)|*.sseq",
                Multiselect = false,
            };
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var midi_f = new MidiFile(ofd.FileName);
                Sequence.Data.Midi = midi_f.Events;
                File.Delete(TempExportedMidi);
                File.Copy(ofd.FileName, TempExportedMidi);
                ResetPlayer();
                Utils.ShowMessage("Done");
            }
        }
    }
}
