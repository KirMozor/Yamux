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
        private static event LenghtTrack ChangeLegthTrack;
        private static int durationTrack = 1;
        private static string directLink;
        private static string titleTrack = "";
        private static string artistTrack = "";
        private static HScale PlayerScale = new HScale(0.0, 100.0, 0.1);
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

        public static Button PlayerStopTrack = new Button();
        public static Button PlayerPreviousTrack = new Button();
        public static Button PlayerPlayTrack = new Button();
        public static Button PlayerNextTrack = new Button();
        public static Button PlayerDownloadTrack = new Button();

        public static List<string> uidPlaylist = new List<string>();
        public static List<string> kindPlaylist = new List<string>();

        private VBox _bestBox = new VBox();

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
            PlayerPlayTrack.Clicked += ClickPauseOrPlay;
            PlayerPreviousTrack.Clicked += Player.LastTrack;
            PlayerNextTrack.Clicked += Player.NextTrack;
            PlayerDownloadTrack.Clicked += PlayerDownloadTrackOnClicked;
            
            SetDefaultIconFromFile("Svg/icon.svg");
            AboutImage.Pixbuf = new Pixbuf("Svg/about_icon.svg");
            ImageSettings.Pixbuf = new Pixbuf("Svg/icons8-settings-20.png");
            CreatePlayerBox();
            PlayerBoxScale.Hide();
            PlayerActionBox.Hide();
            PlayerImage.Hide();
        }

        private async void LandingLoad()
        {
            await Task.Run(() =>
            {
            });
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
            JToken root = "{}";
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
                if (root.Count() > 6)
                {
                    _bestBox.Destroy();
                    _bestBox = new VBox();
                    ResultBox.Add(_bestBox);
                    await Task.Run(() =>
                    {
                        IfNoResult.Text = "";
                        string typeBest = root["best"]["type"].ToString();

                        switch (typeBest)
                        {
                            case "artist":
                                typeBest = "Артист";
                                break;
                            case "track":
                                typeBest = "Трек";
                                break;
                            case "playlist":
                                typeBest = "Плейлист";
                                break;
                            case "podcast":
                                typeBest = "Выпуски подкастов";
                                break;
                            case "album":
                                typeBest = "Альбом";
                                break;
                        }

                        Dictionary<string, List<string>> artist = Yamux.GetArtist(root);
                        Dictionary<string, List<string>> track = Yamux.GetTrack(root);
                        Dictionary<string, List<string>> podcast = Yamux.GetPodcast(root);
                        Dictionary<string, List<string>> playlist = Yamux.GetPlaylist(root);
                        List<string> artistName = artist["name"];
                        List<string> artistCoverUri = artist["coverUri"];
                        List<string> artistId = artist["id"];
                        List<string> trackName = track["name"];
                        List<string> trackCoverUri = track["coverUri"];
                        List<string> trackId = track["id"];
                        List<string> podcastName = podcast["name"];
                        List<string> podcastCoverUri = podcast["coverUri"];
                        List<string> podcastId = podcast["id"];
                        List<string> playlistName = playlist["name"];
                        List<string> playlistCoverUri = playlist["coverUri"];
                        List<string> playlistId = new List<string>();
                        kindPlaylist = playlist["kind"];
                        uidPlaylist = playlist["uid"];
                        
                        HBox artistBox = Yamux.CreateBoxResultSearch(artistName, artistCoverUri, artistId, "artist");
                        HBox trackBox = Yamux.CreateBoxResultSearch(trackName, trackCoverUri, trackId, "track");
                        HBox podcastBox = Yamux.CreateBoxResultSearch(podcastName, podcastCoverUri, podcastId, "podcast");
                        HBox playlistBox = Yamux.CreateBoxResultSearch(playlistName, playlistCoverUri, playlistId, "playlist");

                        ScrolledWindow scrolledArtist = new ScrolledWindow();
                        ScrolledWindow scrolledTrack = new ScrolledWindow();
                        ScrolledWindow scrolledPodcast = new ScrolledWindow();
                        ScrolledWindow scrolledPlaylist = new ScrolledWindow();
                        scrolledArtist.PropagateNaturalHeight = true;
                        scrolledArtist.PropagateNaturalWidth = true;
                        scrolledTrack.PropagateNaturalHeight = true;
                        scrolledTrack.PropagateNaturalWidth = true;
                        scrolledPodcast.PropagateNaturalHeight = true;
                        scrolledPodcast.PropagateNaturalWidth = true;
                        scrolledPlaylist.PropagateNaturalHeight = true;
                        scrolledPlaylist.PropagateNaturalWidth = true;

                        Viewport viewportArtist = new Viewport();
                        Viewport viewportTrack = new Viewport();
                        Viewport viewportPodcast = new Viewport();
                        Viewport viewportPlaylist = new Viewport();

                        Label artistLabel = new Label(typeBest);
                        FontDescription tpfartist = new FontDescription();
                        tpfartist.Size = 12288;
                        artistLabel.ModifyFont(tpfartist);

                        Label trackLabel = new Label("Треки");
                        FontDescription tpftrack = new FontDescription();
                        tpftrack.Size = 12288;
                        trackLabel.ModifyFont(tpftrack);

                        Label podcastLabel = new Label("Выпуски подкастов");
                        FontDescription tpfpodcast = new FontDescription();
                        tpfpodcast.Size = 12288;
                        podcastLabel.ModifyFont(tpfpodcast);

                        Label playlistLabel = new Label("Плейлисты");
                        FontDescription tpfplaylist = new FontDescription();
                        tpfplaylist.Size = 12288;
                        playlistLabel.ModifyFont(tpfplaylist);

                        scrolledArtist.Add(viewportArtist);
                        viewportArtist.Add(artistBox);
                        scrolledTrack.Add(viewportTrack); 
                        viewportTrack.Add(trackBox);
                        scrolledPodcast.Add(viewportPodcast);
                        viewportPodcast.Add(podcastBox);
                        scrolledPlaylist.Add(viewportPlaylist);
                        viewportPlaylist.Add(playlistBox);

                        _bestBox.Add(artistLabel);
                        _bestBox.Add(scrolledArtist);
                        _bestBox.Add(trackLabel);
                        _bestBox.Add(scrolledTrack);
                        _bestBox.Add(podcastLabel);
                        _bestBox.Add(scrolledPodcast);
                        _bestBox.Add(playlistLabel);
                        _bestBox.Add(scrolledPlaylist);
                    });
                    ResultBox.ShowAll();
                    _bestBox.ShowAll();
                    foreach (Button i in Yamux.ListButtonPlay)
                    {
                        i.Clicked += PlayButtonClick;
                    }
                }
                else
                {
                    _bestBox.Destroy();
                    IfNoResult.Text = "Нет результата😢";
                }
            }
        }

        private async void PlayButtonClick(object sender, EventArgs a)
        {
            Button buttonPlay = (Button) sender;
            try
            {
                JObject details = JObject.Parse(buttonPlay.Name);
                string id = details["id"].ToString();

                await Task.Run(() =>
                {
                    try
                    {
                        File.Delete("s.jpg");
                        string url = details["uri"].ToString();
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(new Uri(url), "s.jpg");
                        }
                        Pixbuf imagePixbuf;
                        imagePixbuf = new Pixbuf("s.jpg");
                        PlayerImage.Pixbuf = imagePixbuf;
                    }
                    catch (Newtonsoft.Json.JsonReaderException)
                    {
                        Pixbuf imagePixbuf;
                        imagePixbuf = new Pixbuf("Svg/icons8_rock_music_100_negate.png");
                        PlayerImage.Pixbuf = imagePixbuf;
                    }
                    catch (NullReferenceException)
                    {
                        Pixbuf imagePixbuf;
                        imagePixbuf = new Pixbuf("Svg/icons8_rock_music_100_negate.png");
                        PlayerImage.Pixbuf = imagePixbuf;
                    }
                    catch (GLib.GException)
                    {
                    }
                });
                
                if (Convert.ToString(details["type"]) == "track")
                {
                    List<string> track = new List<string>();
                    track.Add(id);

                    await Task.Run(() =>
                    {
                        JObject informTrack = Track.GetInformTrack(track); 
                        titleTrack = informTrack["result"][0]["title"].ToString();
                        artistTrack = informTrack["result"][0]["artists"][0]["name"].ToString();
                    });
                    Console.WriteLine(titleTrack + ";" + artistTrack);
                    await Task.Run(() =>
                    {
                        directLink = Player.GetDirectLinkWithTrack(details["id"].ToString());
                        Player.PlayUrlFile(directLink);
                    });
                    PlayerTitleTrack.Text = titleTrack;
                    PlayerNameArtist.Text = artistTrack;
                }

                if (Convert.ToString(details["type"]) == "artist")
                {
                    List<string> track = new List<string>();

                    await Task.Run(() =>
                    {
                        JObject artist = Artist.InformArtist(id);
                        JToken popularTracks = artist["result"]["popularTracks"];
                        artistTrack = artist["result"]["artist"]["name"].ToString();

                        foreach (var i in popularTracks)
                        {
                            track.Add(i["id"].ToString());
                        }
                        Player.trackIds = track;
                        Player.PlayPlaylist();
                    });
                    PlayerNameArtist.Text = artistTrack; 
                }
                Pixbuf playerPlayPixbuf = new Pixbuf("Svg/icons8-pause.png");
                PlayerPlayTrack.Image = new Image(playerPlayPixbuf);
                PlayerScale.SetRange(0.0, Player.GetLength());
                PlayerBoxScale.ShowAll();
                PlayerActionBox.ShowAll();
                PlayerImage.ShowAll();
                SpyChangeDurationTrack();

                ChangeLegthTrack += () => { PlayerScale.Value = durationTrack; };

                await Task.Run(() =>
                {
                    while (true)
                    {
                        if (Player.GetStatusPlayback() != PlaybackState.Stopped)
                        {
                            Thread.Sleep(1000);
                            durationTrack = Player.GetPosition();
                        }
                        else
                        {
                            PlayerTitleTrack.Text = "";
                            PlayerNameArtist.Text = "";
                            PlayerBoxScale.Hide();
                            PlayerActionBox.Hide();
                            PlayerImage.Hide();
                            break;
                        }
                    }
                });
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
            }
        }

        private void CreatePlayerBox()
        {
            PlayerTitleTrack.MaxWidthChars = 17;
            PlayerNameArtist.MaxWidthChars = 17;
            PlayerStopTrack.Relief = ReliefStyle.None;
            PlayerPreviousTrack.Relief = ReliefStyle.None;
            PlayerPlayTrack.Relief = ReliefStyle.None;
            PlayerNextTrack.Relief = ReliefStyle.None;
            PlayerDownloadTrack.Relief = ReliefStyle.None;
                        
            Pixbuf playerStopPixbuf;
            Pixbuf playerPreviousPixbuf;
            Pixbuf playerPlayPixbuf;
            Pixbuf playerNextPixbuf;
            Pixbuf playerDownloadPixbuf;
                    
            playerStopPixbuf = new Pixbuf("Svg/icons8-stop.png");
            playerPreviousPixbuf = new Pixbuf("Svg/icons8-previous.png");
            playerPlayPixbuf = new Pixbuf("Svg/icons8-pause.png");
            playerNextPixbuf = new Pixbuf("Svg/icons8-next.png");
            playerDownloadPixbuf = new Pixbuf("Svg/icons8-download.png");

            PlayerStopTrack.Image = new Image(playerStopPixbuf);
            PlayerPlayTrack.Image = new Image(playerPlayPixbuf);
            PlayerPreviousTrack.Image = new Image(playerPreviousPixbuf);
            PlayerNextTrack.Image = new Image(playerNextPixbuf);
            PlayerDownloadTrack.Image = new Image(playerDownloadPixbuf);

            PlayerActionBox.Add(PlayerStopTrack);
            PlayerActionBox.Add(PlayerPreviousTrack);
            PlayerActionBox.Add(PlayerPlayTrack);
            PlayerActionBox.Add(PlayerNextTrack);
            PlayerActionBox.Add(PlayerDownloadTrack);

            PlayerScale = new HScale(0.0, 100, 1.0);
            PlayerScale.Hexpand = true;
            PlayerScale.Valign = Align.Center;
            PlayerScale.DrawValue = false;
            PlayerBoxScale.Add(PlayerScale);

            PlayerBoxScale.ShowAll();
            PlayerActionBox.ShowAll();
            PlayerImage.ShowAll();
        }
        private void ClickPauseOrPlay(object sender, EventArgs a)
        {
            if (Player.PlayTrackOrPause)
            {
                Pixbuf PlayerPausePixbuf = new Pixbuf("Svg/icons8-play.png");
                PlayerPlayTrack.Image = new Image(PlayerPausePixbuf);
            }
            else
            {
                Pixbuf playerPlayPixbuf = new Pixbuf("Svg/icons8-pause.png");
                PlayerPlayTrack.Image = new Image(playerPlayPixbuf);
            }
            Player.PauseOrStartPlay();
        }
        async static void SpyChangeDurationTrack()
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
                            ChangeLegthTrack.Invoke();
                            oldDuration = durationTrack;
                        }
                    }
                    else { break; }
                }
            });
        }
        private void PlayerDownloadTrackOnClicked(object sender, EventArgs e)
        {
            Console.WriteLine(1231231);
            if (!Directory.Exists("/home/kirill/YandexMusic/")) { Directory.CreateDirectory("/home/kirill/YandexMusic/"); }

            string nameTrackFile = "/home/kirill/YandexMusic/" + artistTrack + " - " + titleTrack + ".mp3";
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