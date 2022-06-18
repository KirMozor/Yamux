using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Gdk;
using Gtk;
using ManagedBass;
using Newtonsoft.Json.Linq;
using Pango;
using Tomlyn;
using YandexMusicApi;
using Application = Gtk.Application;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

namespace Yamux
{
    class YamuxWindow : Window
    {
        public delegate void LenghtTrack();
        private static event LenghtTrack ChangeLengthTrack;
        private static int durationTrack = 1;
        private static string directLink;
        private static string titleTrack = "";
        private static string artistTrack = "";
        [UI] private Dialog DonateWindow = null;
        [UI] private Window AboutWindow = null;

        [UI] private Button AboutGitHubProject = null;
        [UI] private Button AboutGitHubAuthor = null;
        [UI] private Button AboutTelegramChannel = null;
        [UI] private Button AboutDonateMe = null;
        [UI] private Button KofiDonate = null;
        [UI] private Button CloseAboutWindow = null;
        [UI] private Button CloseDonateWindow = null;

        [UI] private Button AboutProgram = null;
        [UI] private SearchEntry SearchMusic = null;
        [UI] private Box SearchBox = null;
        [UI] private Box ResultBox = null;
        [UI] private Label IfNoResult = null;
        [UI] private Image AboutImage = null;
        [UI] private Image ImageSettings = null;

        [UI] private Box informTrackBox = null;
        [UI] private Box PlayerBoxScale = null;
        [UI] private Box PlayerActionBox = null;
        [UI] private Box PlayerMoreActionBox = null;
        [UI] private Label PlayerNameArtist = null;
        [UI] private Label PlayerTitleTrack = null;
        [UI] private Image PlayerImage = null;
        
        public static List<string> uidPlaylist = new List<string>();
        public static List<string> kindPlaylist = new List<string>();
        public static bool SearchOrNot = true;
        
        private VBox ResultSearchBox = new VBox();

        public YamuxWindow() : this(new Builder("Yamux.glade"))
        {
        }

        private YamuxWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            using (FileStream fstream = File.OpenRead("config.toml"))
            {
                byte[] buffer = new byte[fstream.Length];
                fstream.Read(buffer, 0, buffer.Length);
                string textFromFile = Encoding.Default.GetString(buffer);

                var model = Toml.ToModel(textFromFile);
                Token.token = (string) model["yandex_token"]!;
            }
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            AboutProgram.Clicked += ShowAboutWindow;
            AboutDonateMe.Clicked += ShowDonateWindow;
            SearchMusic.SearchChanged += SearchChangedOutput;

            SetDefaultIconFromFile("Svg/icon.svg");
            AboutImage.Pixbuf = new Pixbuf("Svg/about_icon.svg");
            ImageSettings.Pixbuf = new Pixbuf("Svg/icons8-settings-20.png");
        }

        private void ShowAboutWindow(object sender, EventArgs a)
        {
            AboutWindow.ShowAll();
            AboutWindow.Deletable = false;
            
            CloseAboutWindow.Clicked += HideAboutWindow;
            AboutGitHubProject.Clicked += ClickAboutGitHubProject;
            AboutGitHubAuthor.Clicked += ClickAboutGitHubAuthor;
            AboutTelegramChannel.Clicked += ClickTelegramChannel;
        }

        private void ShowDonateWindow(object sender, EventArgs a)
        {
            DonateWindow.ShowAll();
            DonateWindow.Deletable = false;
            CloseDonateWindow.Clicked += HideDonateWindow;
            KofiDonate.Clicked += ClickKofiDonate;
        }
        private async void SearchChangedOutput(object sender, EventArgs a)
        {
            string text = SearchMusic.Text;
            JToken root = "";
            await Task.Run(() =>
            {
                Thread.Sleep(2000);
                if (text == SearchMusic.Text && !string.IsNullOrEmpty(SearchMusic.Text) && !string.IsNullOrEmpty(text))
                {
                    Console.WriteLine(text);
                    JObject resultSearch = YandexMusicApi.Default.Search(text);
                    root = resultSearch.Last.Last.Root;
                    root = root.SelectToken("result");
                }
            });
            ShowResultSearch(root, text);
        }
        private async void ShowResultSearch(JToken root, string text)
        {
            if (text == SearchMusic.Text && !string.IsNullOrEmpty(SearchMusic.Text) && !string.IsNullOrEmpty(text))
            {
                if (root.Count() > 6 && SearchOrNot)
                {
                    SearchOrNot = false;
                    ResultSearchBox.Destroy();
                    ResultSearchBox = new VBox();
                    ResultBox.Add(ResultSearchBox);

                    HBox albumsBox = new HBox();
                    HBox playlistsBox = new HBox();
                    HBox podcastsBox = new HBox();
                    HBox tracksBox = new HBox();
                    HBox artistsBox = new HBox();
                    ScrolledWindow scrolledAlbums = new ScrolledWindow(); scrolledAlbums.PropagateNaturalHeight = true; scrolledAlbums.PropagateNaturalWidth = true;
                    ScrolledWindow scrolledArtists = new ScrolledWindow(); scrolledArtists.PropagateNaturalHeight = true; scrolledArtists.PropagateNaturalWidth = true;
                    ScrolledWindow scrolledTracks = new ScrolledWindow(); scrolledTracks.PropagateNaturalHeight = true; scrolledTracks.PropagateNaturalWidth = true;
                    ScrolledWindow scrolledPodcasts = new ScrolledWindow(); scrolledPodcasts.PropagateNaturalHeight = true; scrolledPodcasts.PropagateNaturalWidth = true;
                    ScrolledWindow scrolledPlaylists = new ScrolledWindow(); scrolledPlaylists.PropagateNaturalHeight = true; scrolledPlaylists.PropagateNaturalWidth = true;
                    Viewport viewportAlbums = new Viewport();
                    Viewport viewportArtists = new Viewport();
                    Viewport viewportTracks = new Viewport();
                    Viewport viewportPodcasts = new Viewport();
                    Viewport viewportPlaylists = new Viewport();
                    Dictionary<string, List<string>> albums;
                    Dictionary<string, List<string>> playlists;
                    Dictionary<string, List<string>> podcasts;
                    Dictionary<string, List<string>> tracks;
                    Dictionary<string, List<string>> artists;

                    Dictionary<string, string> best = Yamux.GetBest(root);

                    try
                    {
                        albums = Yamux.GetAlbums(root);
                        await Task.Run(() =>
                        {
                            albumsBox = Yamux.CreateBoxResultSearch(albums["name"], albums["coverUri"], albums["id"], "albums");
                        });
                    }
                    catch (NullReferenceException) {}
                    try
                    {
                        podcasts = Yamux.GetPodcasts(root);
                        await Task.Run(() =>
                        {
                            podcastsBox = Yamux.CreateBoxResultSearch(podcasts["name"], podcasts["coverUri"], podcasts["id"], "podcasts");
                        });
                    }
                    catch (NullReferenceException) {}
                    try
                    {
                        tracks = Yamux.GetTracks(root);
                        await Task.Run(() =>
                        {
                            tracksBox = Yamux.CreateBoxResultSearch(tracks["name"], tracks["coverUri"], tracks["id"],
                                "tracks");
                        });
                    }
                    catch (NullReferenceException) {}
                    try
                    {
                        artists = Yamux.GetArtist(root);
                        await Task.Run(() =>
                        {
                            artistsBox = Yamux.CreateBoxResultSearch(artists["name"], artists["coverUri"],
                                artists["id"], "artists");
                        });
                    }
                    catch (NullReferenceException) {}
                    /*
                    try
                    {
                        playlists = Yamux.GetPlaylists(root); 
                        List<string> playlistsName = playlists["name"];
                        List<string> playlistsCoverUri = playlists["coverUri"];
                        kindPlaylist = playlists["kind"];
                        uidPlaylist = playlists["uid"];
                        await Task.Run(() =>
                        {
                            playlistsBox = Yamux.CreateBoxResultSearch(playlistsName, playlistsCoverUri, new List<string>(), "playlist");
                        });
                    }
                    catch (NullReferenceException) {}
                    */
                    
                    switch (best["type"])
                    {
                        case "artist":
                        {
                            scrolledArtists.Add(viewportArtists);
                            viewportArtists.Add(artistsBox);
                            ResultSearchBox.Add(scrolledArtists); 
                            break;
                        }
                        case "track":
                        {
                            scrolledTracks.Add(viewportTracks);
                            viewportTracks.Add(tracksBox);
                            ResultSearchBox.Add(scrolledTracks); 
                            break;
                        }
                        case "podcast":
                        {
                            scrolledPodcasts.Add(viewportPodcasts);
                            viewportPodcasts.Add(playlistsBox);
                            ResultSearchBox.Add(scrolledPodcasts); 
                            break;
                        }
                        case "album":
                        {
                            scrolledAlbums.Add(viewportAlbums);
                            viewportAlbums.Add(albumsBox);
                            ResultSearchBox.Add(scrolledAlbums); 
                            break;
                        }
                        case "playlist":
                        {
                            scrolledPlaylists.Add(viewportPlaylists);
                            viewportPlaylists.Add(playlistsBox);
                            ResultSearchBox.Add(scrolledPlaylists); 
                            break;
                        }
                    }
                    
                    SearchBox.ShowAll();
                    ResultBox.ShowAll();
                    SearchOrNot = true;
                }
                else
                {
                    ResultSearchBox.Destroy();
                    IfNoResult.Text = "Нет результата😢";
                }
            }
        }
        private async static void SpyChangeDurationTrack()
        {
            double oldDuration = durationTrack;
            await Task.Run(() =>
            {
                while (true)
                {
                    if (Player.GetStatusPlayback() != PlaybackState.Stopped)
                    {
                        Thread.Sleep(1000);
                        if (oldDuration != durationTrack)
                        {
                            ChangeLengthTrack.Invoke();
                            oldDuration = durationTrack;
                        }
                    }
                    else { break; }
                }
            });
        }
        private void PlayerDownloadTrackOnClicked()
        {
            string pathToHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (Directory.Exists(pathToHome + "/YandexMusic") == false) { Directory.CreateDirectory("/home/kirill/YandexMusic/"); }

            string nameTrackFile = pathToHome + "/YandexMusic/" + artistTrack + " - " + titleTrack + ".mp3";
            Player.DownloadUriWithThrottling(new Uri(directLink), nameTrackFile);
        }
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
        private void HideAboutWindow(object sender, EventArgs a)
        {
            AboutWindow.Hide();
        }
        private void HideDonateWindow(object sender, EventArgs a)
        {
            DonateWindow.Hide();
        }
        private void ClickAboutGitHubProject(object sender, EventArgs a)
        {
            Yamux.OpenLinkToWebBrowser("https://github.com/KirMozor/Yamux");
        }
        private void ClickAboutGitHubAuthor(object sender, EventArgs a)
        {
            Yamux.OpenLinkToWebBrowser("https://github.com/KirMozor");
        }
        private void ClickTelegramChannel(object sender, EventArgs a)
        {
            Yamux.OpenLinkToWebBrowser("https://t.me/kirmozor");
        }

        private void ClickKofiDonate(object sender, EventArgs a)
        {
            Yamux.OpenLinkToWebBrowser("https://ko-fi.com/kirmozor");
        }
    }
}