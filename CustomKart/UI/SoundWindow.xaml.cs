using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using LibDSSound.Software;
using LibDSSound.IO;
using NAudio.Wave;
using MaterialDesignThemes.Wpf;
using System.ComponentModel;

namespace CustomKart.UI
{
    /// <summary>
    /// Lógica de interacción para SoundWindow.xaml
    /// </summary>
    public partial class SoundWindow : Window
    {
        public SoundWindow()
        {
            InitializeComponent();
            for(int i = 0; i < MKDS.SequenceCount; i++)
            {
                SequencesCombo.Items.Add("Sequence no. " + i.ToString());
            }
            Load();
        }

        public static SDAT Data { get; set; }

        public static SSEQ Sequence { get; set; }

        public static SBNK Bank { get; set; }

        public static SDAT.INFO.SequenceInfo SequenceInfo { get; set; }

        public static SDAT.INFO.BankInfo BankInfo { get; set; }

        public static bool Stop { get; set; }

        public static bool Playing { get; set; }

        protected override void OnClosing(CancelEventArgs e)
        {
            Playing = false;
            Stop = true;

            // TODO: ask whether to save/close
            ROMUtils.ROM.WriteFile("sound_data.sdat", Data.Write());

            base.OnClosing(e);
        }

        private void SoundThread()
        {
            BufferedWaveProvider bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(65456, 16, 2));
            bufferedWaveProvider.DiscardOnBufferOverflow = true;
            bufferedWaveProvider.BufferLength = 21824;
            WaveOut waveOut = new WaveOut();
            waveOut.DesiredLatency = 150;
            waveOut.Init(bufferedWaveProvider);
            waveOut.Play();
            DSSoundContext dssoundContext = new DSSoundContext();
            dssoundContext.ExChannelInit();
            dssoundContext.SeqInit();
            dssoundContext.StartSeq(0, Sequence.Data, 0, Bank);
            Player player = dssoundContext.Work.Players[0];
            player.Volume = SequenceInfo.Volume;
            while(!Stop)
            {
                if(Playing && (bufferedWaveProvider.BufferedBytes < bufferedWaveProvider.BufferLength) && ((bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes) > 1364))
                {
                    dssoundContext.UpdateExChannel();
                    dssoundContext.SeqMain(true);
                    dssoundContext.ExChannelMain(true);
                    Util.CalcRandom();
                    for (int i = 0; i < 341; i++)
                    {
                        dssoundContext.Hardware.Evaluate(256, out short num, out short num2);
                        bufferedWaveProvider.AddSamples(new byte[]
                        {
                            (byte)(num & 255),
                            (byte)(num >> 8 & 255),
                            (byte)(num2 & 255),
                            (byte)(num2 >> 8 & 255)
                        }, 0, 4);
                    }
                }
            }
            waveOut.Stop();
            waveOut.Dispose();
        }

        public void Load()
        {
            Playing = false;
            Stop = true;
            Data = new SDAT(ROMUtils.ROM.ReadFile("sound_data.sdat"));
            SequencesCombo.SelectedIndex = 0;
        }

        private void Reload()
        {
            Playing = false;
            Stop = true;
            PlayButton.Content = new PackIcon { Kind = PackIconKind.Play };
            int sidx = SequencesCombo.SelectedIndex;
            SequenceInfo = Data.InfoBlock.SequenceInfos[sidx];
            Sequence = new SSEQ(Data.GetFileData(SequenceInfo.FileId));
            BankInfo = Data.InfoBlock.BankInfos[SequenceInfo.Bank];
            Bank = new SBNK(Data.GetFileData(BankInfo.FileId));
            for(int i = 0; i < 4; i++)
            {
                if (BankInfo.WaveArchives[i] != 65535)
                {
                    Bank.AssignWaveArc(i, new SWAR(Data.GetFileData(Data.InfoBlock.WaveArchiveInfos[(int)BankInfo.WaveArchives[i]].FileId)));
                }
            }
        }

        private void SequencesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Reload();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if(!Playing)
            {
                if(Stop)
                {
                    Stop = false;
                    new Thread(new ThreadStart(SoundThread))
                    {
                        IsBackground = true,
                    }.Start();
                }
                PlayButton.Content = new PackIcon
                {
                    Kind = PackIconKind.Pause
                };
            }
            else
            {
                PlayButton.Content = new PackIcon
                {
                    Kind = PackIconKind.Play
                };
            }
            Playing = !Playing;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Playing = false;
            Stop = true;
            PlayButton.Content = new PackIcon
            {
                Kind = PackIconKind.Play
            };
        }

        private void SequenceImportButton_Click(object sender, RoutedEventArgs e)
        {
            bool chplay = false;
            if(!Stop && Playing)
            {
                chplay = true;
                PlayButton.Content = new PackIcon
                {
                    Kind = PackIconKind.Play
                };
                Playing = !Playing;
            }
            var od = new OpenFileDialog
            {
                Title = Utils.GetResource("sound:sequenceReplace"),
                Filter = Utils.GetResource("sound:midiFile") + " |*.mid|" + Utils.GetResource("sound:sseqFile") + " (SSEQ)|*.sseq",
                Multiselect = false,
            };
            if(od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ext = Path.GetExtension(od.FileName);
                string sseq = "";
                if(ext.EndsWith("sseq")) sseq = od.FileName;
                else if(ext.EndsWith("mid"))
                {
                    string fname = Path.GetFileNameWithoutExtension(od.FileName);
                    MIDIConverter cvtr = ROMUtils.DetermineConverter();
                    if(cvtr == MIDIConverter.Invalid)
                    {
                        Utils.ShowMessage(Utils.GetResource("sound:errorMidiConversion"));
                        return;
                    }
                    else if(cvtr == MIDIConverter.MIDI2SSEQ)
                    {
                        string midi2sseq = Utils.GetSelfPath() + "\\midi2sseq.exe";
                        var p = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = midi2sseq,
                                Arguments = "\"" + od.FileName + "\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            }
                        };
                        p.Start();
                        p.WaitForExit();
                        if(!File.Exists(Utils.GetSelfPath() + "\\" + fname + ".sseq"))
                        {
                            Utils.ShowMessage(Utils.GetResource("sound:errorSequenceConversion"));
                            if (chplay)
                            {
                                PlayButton.Content = new PackIcon
                                {
                                    Kind = PackIconKind.Pause
                                };
                                Playing = !Playing;
                            }
                            return;
                        }
                        sseq = Utils.GetSelfPath() + "\\" + fname + ".sseq";
                    }
                    else if(cvtr == MIDIConverter.SmfSeqConv)
                    {
                        string smfconv = Utils.GetSelfPath() + "\\smfconv.exe";
                        string seqconv = Utils.GetSelfPath() + "\\seqconv.exe";
                        var p = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = smfconv,
                                Arguments = "\"" + od.FileName + "\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            }
                        };
                        p.Start();
                        p.WaitForExit();
                        string basenoext = Path.GetDirectoryName(od.FileName) + "\\" + fname;
                        if(!File.Exists(basenoext + ".smft"))
                        {
                            Utils.ShowMessage(Utils.GetResource("sound:errorSequenceConversion"));
                            if (chplay)
                            {
                                PlayButton.Content = new PackIcon
                                {
                                    Kind = PackIconKind.Pause
                                };
                                Playing = !Playing;
                            }
                            return;
                        }
                        var p2 = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = seqconv,
                                Arguments = "\"" + basenoext + ".smft\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            }
                        };
                        p2.Start();
                        p2.WaitForExit();
                        if(!File.Exists(basenoext + ".sseq"))
                        {
                            Utils.ShowMessage(Utils.GetResource("sound:errorSequenceConversion"));
                            if(chplay)
                            {
                                PlayButton.Content = new PackIcon
                                {
                                    Kind = PackIconKind.Play
                                };
                                Playing = false;
                                Stop = true;
                            }
                            return;
                        }
                        File.Delete(basenoext + ".smft");
                        sseq = basenoext + ".sseq";
                    }
                }
                Data.FileAllocationTable.Entries[(int)Data.InfoBlock.SequenceInfos[SequencesCombo.SelectedIndex].FileId].Data = File.ReadAllBytes(sseq);
                Reload();
                if(ext.EndsWith("mid")) File.Delete(sseq);
            }
            if(chplay)
            {
                PlayButton.Content = new PackIcon
                {
                    Kind = PackIconKind.Play
                };
                Playing = false;
                Stop = true;
            }
        }
    }
}
