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
            foreach(var elem in MKDS.Sequences) SequencesCombo.Items.Add(elem.ToString());
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
            var fs = MainWindow.LoadedROM.ToFileSystem();
            var sdatfile = ROMUtils.GetSFSFile("sound_data.sdat", fs);
            sdatfile.Data = Data.Write();
            MainWindow.LoadedROM.FromFileSystem(fs);
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
                if(Playing && bufferedWaveProvider.BufferedBytes < bufferedWaveProvider.BufferLength && bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes > 1364)
                {
                    dssoundContext.UpdateExChannel();
                    dssoundContext.SeqMain(true);
                    dssoundContext.ExChannelMain(true);
                    Util.CalcRandom();
                    for (int i = 0; i < 341; i++)
                    {
                        short num;
                        short num2;
                        dssoundContext.Hardware.Evaluate(256, out num, out num2);
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

        public void Load(SDAT SData)
        {
            Playing = false;
            Stop = true;
            Data = SData;
            SequencesCombo.SelectedIndex = 0;
        }

        private void Reload()
        {
            Playing = false;
            Stop = true;
            PlayButton.Content = new PackIcon
            {
                Kind = PackIconKind.Play
            };
            int sidx = MKDS.Sequences[SequencesCombo.SelectedIndex];
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
            OpenFileDialog od = new OpenFileDialog
            {
                Title = "Replace sound sequence",
                Filter = "MIDI audio|*.mid|Nitro Sound Sequence (SSEQ)|*.sseq",
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
                        Utils.ShowMessage("No MIDI conversion tool was found.");
                        return;
                    }
                    else if(cvtr == MIDIConverter.MIDI2SSEQ)
                    {
                        string midi2sseq = ROMUtils.GetExePath() + "\\midi2sseq.exe";
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
                        if(!File.Exists(ROMUtils.GetExePath() + "\\" + fname + ".sseq"))
                        {
                            Utils.ShowMessage("Unable to convert MIDI audio to SSEQ.");
                            if(chplay)
                            {
                                PlayButton.Content = new PackIcon
                                {
                                    Kind = PackIconKind.Pause
                                };
                                Playing = !Playing;
                            }
                            return;
                        }
                        sseq = ROMUtils.GetExePath() + "\\" + fname + ".sseq";
                    }
                    else if(cvtr == MIDIConverter.SmfSeqConv)
                    {
                        string smfconv = ROMUtils.GetExePath() + "\\smfconv.exe";
                        string seqconv = ROMUtils.GetExePath() + "\\seqconv.exe";
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
                            Utils.ShowMessage("Unable to generate SMFT from MIDI audio.");
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
                            Utils.ShowMessage("Unable to generate SSEQ from SMFT data.");
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
                Data.FileAllocationTable.Entries[(int)Data.InfoBlock.SequenceInfos[MKDS.Sequences[SequencesCombo.SelectedIndex]].FileId].Data = File.ReadAllBytes(sseq);
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
