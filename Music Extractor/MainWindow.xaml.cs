using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Microsoft.WindowsAPICodePack.Dialogs;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using IStreamInfo = YoutubeExplode.Videos.Streams.AudioOnlyStreamInfo;


namespace Music_Extractor
{
    public partial class MainWindow : Window
    {
        internal YoutubeClient youtube;
        public string LoadDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        public MainWindow()
        {
            InitializeComponent();
            PathTextBox.Text = LoadDirectory;
            DoneMsgGrid.Visibility = Visibility.Hidden;


            youtube = new YoutubeClient();


            //Optional Previewed Clean
            DescriptionLabel.Content = "";
            TitleLabel.Content = "";
        }

        /* Future Update
        private void btnSetTime_Click(object sender, RoutedEventArgs e)
        {
            new SetTimeWindow().Show();
        }*/



        private void CallPathPickingWindow()
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog
            {
                Title = "Select downloading directory:",
                IsFolderPicker = true,
                InitialDirectory = LoadDirectory,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = LoadDirectory,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                PathTextBox.Text = dlg.FileName;
        }

        private async void DownloadAudio()
        {
            btnDownload.IsEnabled = false;
            try
            {
                YoutubeExplode.Videos.Video video = await youtube.Videos.GetAsync(LinkTextBox.Text);

                string fileName = video.Title;
                foreach (char i in Path.GetInvalidFileNameChars())
                {
                    fileName = fileName.Replace(i, '-');
                }
                fileName = Path.Combine(PathTextBox.Text, fileName + ".opus");

                StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(LinkTextBox.Text.ToString());

                IStreamInfo streamInfo = (IStreamInfo)streamManifest
                    .GetAudioOnlyStreams().GetWithHighestBitrate();

                if (streamInfo != null)
                {
                    Stream stream = await youtube.Videos.Streams.GetAsync((IStreamInfo)streamInfo);

                    await youtube.Videos.Streams.DownloadAsync((IStreamInfo)streamInfo, fileName);
                }
                
                DoneMsgGrid.Visibility = Visibility.Visible;
            }
            catch (ArgumentException)
            {
                LinkTextBox.Text = "Enter valid video link here";
            }
            catch (DirectoryNotFoundException)
            {
                PathTextBox.Text =  "Enter valid download directory here";
            }

            btnDownload.IsEnabled = true;
        }

        private async void TryFindVideoAsync()
        {
            try
            {
                YoutubeExplode.Videos.Video video;
                BitmapImage bi3 = new BitmapImage();

                video = await youtube.Videos.GetAsync(LinkTextBox.Text);

                string title = video.Title;
                string author = video.Author.Title;
                string thumbnail = video.Thumbnails[4].Url;
                string duration = video.Duration.ToString();

                
                bi3.BeginInit();
                bi3.UriSource = new Uri(thumbnail, UriKind.Absolute);
                bi3.CacheOption = BitmapCacheOption.None;
                bi3.EndInit();

                ThumbnailImage.Source = bi3;

                TitleLabel.Content = title;
                DescriptionLabel.Content = $"{(duration.StartsWith("00") ? duration.Substring(3, 5) : duration)} | {author}";
            }
            catch { ClearPreviewed(); }
        }

        private void ClearPreviewed()
        {
            //TitleLabel.Content = "";
            DescriptionLabel.Content = "";
            ThumbnailImage.Source = new BitmapImage();
            DoneMsgGrid.Visibility = Visibility.Hidden;
        }

        //Buttons
        private void LinkTextBox_DataContextChanged(object sender, TextChangedEventArgs args)
        {
            TryFindVideoAsync();
        }
        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            DownloadAudio();
        }
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            CallPathPickingWindow();
        }
        //Standart
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void btn__Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnX_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
