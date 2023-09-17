using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Media.Control;
using Windows.Storage.Streams;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;
using System.Text.Json;
using System.Diagnostics;

namespace StreamThing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static App App => (App)Application.Current;

        private static readonly MediaManager mediaManager = new MediaManager();
        private string? lastMediaSession = null;

        public MainWindow()
        {
            InitializeComponent();

            mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
            mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;
            mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
            mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;

            UpdatePopupMenu();

            mediaManager.Start();

            Application.Current.Dispatcher.Invoke(async () =>
            {
                await LoadRandomSession();
            });
        }

        private void MediaManager_OnAnyMediaPropertyChanged(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                await UpdateUI(mediaSession);
            });
        }

        private void MediaManager_OnAnySessionOpened(MediaSession mediaSession)
        {
            var playbackInfo = mediaSession.ControlSession.GetPlaybackInfo();
            if (playbackInfo.PlaybackStatus != GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(async () =>
            {
                await UpdateUI(mediaSession);
            });
        }

        private void MediaManager_OnAnySessionClosed(MediaSession mediaSession)
        {
            if (lastMediaSession == mediaSession.Id)
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await LoadRandomSession();
                });
            }
        }

        private async Task LoadRandomSession()
        {
            foreach (var mediaSession in mediaManager.CurrentMediaSessions.Values)
            {
                var playbackInfo = mediaSession.ControlSession.GetPlaybackInfo();
                if (playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                {
                    await UpdateUI(mediaSession);
                    return;
                }
            }

            StatusText.Content = "No session opened";
            SongTitle.Content = "";
            SongAuthor.Content = "";
            StatusText.Visibility = App.Settings.MediaSourceVisibilty != MediaSourceVisibilty.Always ? Visibility.Hidden : Visibility.Visible;
        }

        private void MediaManager_OnAnyPlaybackStateChanged(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
        {
            var isPlaying = playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

            Application.Current.Dispatcher.Invoke(async () =>
            {
                if (isPlaying)
                    await UpdateUI(mediaSession);
                else if (lastMediaSession == mediaSession.Id)
                {
                    lastMediaSession = null;
                    await LoadRandomSession();
                }
            });
        }

        private async Task UpdateUI(MediaSession mediaSession)
        {
            lastMediaSession = mediaSession.Id;
            var songInfo = await mediaSession.ControlSession.TryGetMediaPropertiesAsync();
            if (songInfo != null)
            {
                StatusText.Content = "Playing from " + lastMediaSession;
                SongTitle.Content = songInfo.Title.ToUpper();
                SongAuthor.Content = songInfo.Artist.Replace(" - Topic", "", StringComparison.InvariantCultureIgnoreCase);
                StatusText.Visibility = App.Settings.MediaSourceVisibilty == MediaSourceVisibilty.Never ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private void UpdatePopupMenu()
        {
            var mediaSourceVisibilty = App.Settings.MediaSourceVisibilty;
            foreach (var subview in DisplayMediaSourceOption.Items)
            {
                var option = (MenuItem)subview!;
                var tag = (MediaSourceVisibilty)option.Tag;
                option.IsChecked = mediaSourceVisibilty == tag;
            }

            if (mediaSourceVisibilty == MediaSourceVisibilty.Never ||
                (mediaSourceVisibilty == MediaSourceVisibilty.WhenPlaying && !mediaManager.CurrentMediaSessions.Any())
               )
            {
                StatusText.Visibility = Visibility.Hidden;
            }
            else
            {
                StatusText.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Settings.Width.HasValue)
                App.MainWindow.Width = Math.Max(100, App.Settings.Width.Value);

            App.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        private async void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            App.Settings.Width = (int)e.NewSize.Width;
            await App.SaveSettings();
        }

        private async void DisplayMediaSourceOption_Click(object sender, RoutedEventArgs e)
        {
            var newValue = (MediaSourceVisibilty)((MenuItem)sender).Tag;
            App.Settings.MediaSourceVisibilty = newValue;
            UpdatePopupMenu();

            await App.SaveSettings();
        }
    }
}
