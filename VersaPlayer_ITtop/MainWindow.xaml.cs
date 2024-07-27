using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using NAudio.Wave;

namespace VersaPlayer_ITtop
{
    public partial class MainWindow : Window
    {
        private List<string> musicFiles;
        private List<string> filteredMusicFiles;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private DispatcherTimer timer;
        private bool isDraggingProgressBar = false;
        private bool isMuted = false;

        public MainWindow()
        {
            InitializeComponent();
            InitalizeTools();
            LoadMusicFiles();
        }

        private void InitalizeTools()
        {
            this.ResizeMode = ResizeMode.NoResize;
            musicFiles = new List<string>();
            filteredMusicFiles = new List<string>();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private async void LoadMusicFiles()
        {
            musicList.Visibility = Visibility.Hidden;
            loadingProgress.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                    SearchMusicFiles(drive.RootDirectory);
            });

            filteredMusicFiles = new List<string>(musicFiles);
            musicList.ItemsSource = filteredMusicFiles.Select(Path.GetFileName).ToList();
            musicList.Visibility = Visibility.Visible;
            loadingProgress.Visibility = Visibility.Hidden;
        }

        private void SearchMusicFiles(DirectoryInfo directory)
        {
            try
            {
                if (directory.Name.Equals("Windows", StringComparison.OrdinalIgnoreCase) ||
                    directory.Name.Equals("Program Files", StringComparison.OrdinalIgnoreCase) ||
                    directory.Name.Equals("Program Files (x86)", StringComparison.OrdinalIgnoreCase))
                    return;

                musicFiles.AddRange(Directory.GetFiles(directory.FullName, "*.mp3"));

                foreach (var subDirectory in directory.GetDirectories())
                    SearchMusicFiles(subDirectory);
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore directory to which there is no access
            }
            catch (Exception ex)
            {
                // Ignore directory to which there is no access
            }
        }

        private void MusicList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (musicList.SelectedItem != null)
                PlayMusic(filteredMusicFiles[musicList.SelectedIndex]);
        }

        private void PlayMusic(string filePath)
        {
            if (outputDevice != null)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
            }

            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

            audioFile = new AudioFileReader(filePath);
            outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            outputDevice.Play();

            nameLabel.Content = Path.GetFileName(filePath);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (audioFile != null)
            {
                timeLabel.Content = audioFile.CurrentTime.ToString(@"hh\:mm\:ss");
                progressBar.Value = audioFile.CurrentTime.TotalSeconds / audioFile.TotalTime.TotalSeconds * 100;
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (outputDevice != null)
            {
                if (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    outputDevice.Pause();
                    stopBtn.Content = ">";
                }
                else
                {
                    outputDevice.Play();
                    stopBtn.Content = "||";
                }
            }
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            if (musicList.SelectedIndex < musicList.Items.Count - 1)
            {
                musicList.SelectedIndex++;
                PlayMusic(filteredMusicFiles[musicList.SelectedIndex]);
            }
        }

        private void LeftBtn_Click(object sender, RoutedEventArgs e)
        {
            if (musicList.SelectedIndex > 0)
            {
                musicList.SelectedIndex--;
                PlayMusic(filteredMusicFiles[musicList.SelectedIndex]);
            }
        }

        private void SereachTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = sereachTextBox.Text.ToLower();
            filteredMusicFiles = musicFiles.Where(file => Path.GetFileName(file).ToLower().Contains(searchText)).ToList();
            musicList.ItemsSource = filteredMusicFiles.Select(Path.GetFileName).ToList();
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (audioFile != null && isDraggingProgressBar)
                audioFile.CurrentTime = TimeSpan.FromSeconds(audioFile.TotalTime.TotalSeconds * (progressBar.Value / 100));
        }

        private void ProgressBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => isDraggingProgressBar = true;

        private void ProgressBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingProgressBar = false;
            if (audioFile != null)
                audioFile.CurrentTime = TimeSpan.FromSeconds(audioFile.TotalTime.TotalSeconds * (progressBar.Value / 100));
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();

        private void UprageBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void OffVolumeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (outputDevice != null)
            {
                if (isMuted)
                {
                    outputDevice.Volume = 1.0f;
                    offVolumeBtn.Content = "OFF VOLUME";
                }
                else
                {
                    outputDevice.Volume = 0.0f;
                    offVolumeBtn.Content = "ON VOLUME";
                }
                isMuted = !isMuted;
            }
        }
    }
}
