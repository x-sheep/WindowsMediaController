using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;
using Windows.Media.Control;
using Windows.Storage.Streams;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;

namespace Sample.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly MediaManager mediaManager = new MediaManager();

        public MainWindow()
        {
            InitializeComponent();

            mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;

            mediaManager.Start();

            var menuItem = new NavigationViewItem
            {
                Content = "Automatic",
                Icon = new SymbolIcon() { Symbol = Symbol.Globe },
                Tag = null
            };
            SongList.MenuItems.Insert(0, menuItem);
            SongList.SelectedItem = menuItem;

            Application.Current.Dispatcher.Invoke(async () =>
            {
                foreach (var mediaSession in mediaManager.CurrentMediaSessions.Values)
                {
                    var playbackInfo = mediaSession.ControlSession.GetPlaybackInfo();
                    if (playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                    {
                        await UpdateUI(mediaSession);
                        break;
                    }
                }
            });
        }

        private void MediaManager_OnAnyPlaybackStateChanged(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
        {
            if (playbackInfo.PlaybackStatus != GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(async () => { 
                await UpdateUI(mediaSession);
            });
        }

        private async Task UpdateUI(MediaSession mediaSession)
        {
            var songInfo = await mediaSession.ControlSession.TryGetMediaPropertiesAsync();
            if(songInfo != null)
            {
                SongTitle.Content = songInfo.Title.ToUpper();
                SongAuthor.Content = songInfo.Artist.Replace(" - Topic", "", StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}
