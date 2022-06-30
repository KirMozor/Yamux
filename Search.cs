using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Gdk;
using Gtk;
using ManagedBass;
using Newtonsoft.Json.Linq;
using Pango;
using YandexMusicApi;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

namespace Yamux
{
    class Search : Window
    {
        public delegate void LenghtTrack();
        private static event LenghtTrack ChangeLengthTrack;
        private static int durationTrack = 1;
        public static string directLink;
        private static string titleTrack = "";
        private static string artistTrack = "";

        [UI] private Spinner spinnerProgress = null;
        [UI] private SearchEntry SearchMusic = null;
        [UI] private Box SearchBox = null;
        [UI] private Box ResultBox = null;
        [UI] private Label IfNoResult = null;
        [UI] private Image ImageSettings = null;

        [UI] private Box informTrackBox = null;
        [UI] private Box PlayerActionBox = null;
        [UI] private Box PlayerMoreActionBox = null;
        [UI] private Box PlayerBoxScale = null;
        [UI] private Label PlayerNameArtist = null;
        [UI] private Label PlayerTitleTrack = null;
        [UI] private Image PlayerImage = null;
        [UI] private Box SearchMusicBox = null;
        
        public static List<string> uidPlaylist = new List<string>();
        public static List<string> kindPlaylist = new List<string>();
        public static bool SearchOrNot = true;
        public static HScale PlayerScale = new HScale(0.0, 100, 1.0);
        
        public static VBox ResultSearchBox = new VBox();

        public Search() : this(YamuxWindow.YamuxWindowBuilder)
        {
        }

        public Search(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            SpyChangeDurationTrack();
            YamuxWindow.stopButton.Clicked += Player.StopTrack;
            YamuxWindow.playPauseButton.Clicked += ClickPauseOrPlay;
            YamuxWindow.lastTrackButton.Clicked += Player.LastTrack;
            YamuxWindow.nextTrackButton.Clicked += Player.NextTrack;
            YamuxWindow.downloadTrack.Clicked += (sender, args) => { PlayerDownloadTrackOnClicked(); };
            
            SearchMusic.SearchChanged += (object sender, EventArgs a) => { SearchChangedOutput(); };
        }
        public async void SearchChangedOutput()
        {
            string text = SearchMusic.Text;
            JToken root = "";
            await Task.Run(() =>
            {
                Thread.Sleep(2000);
                if (text == SearchMusic.Text && !string.IsNullOrEmpty(SearchMusic.Text) && !string.IsNullOrEmpty(text))
                {
                    spinnerProgress.Active = true;
                    JObject resultSearch = YandexMusicApi.Default.Search(text);
                    root = resultSearch.Last.Last.Root;
                    root = root.SelectToken("result");
                    spinnerProgress.Active = false;
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
                    spinnerProgress.Active = true;
                    SearchOrNot = false;
                    IfNoResult.Text = "";
                    PlayerNameArtist.Text = "";
                    PlayerTitleTrack.Text = "";
                    PlayerImage.Hide();
                    PlayerBoxScale.Hide();
                    PlayerActionBox.Hide();
                    
                    YamuxWindow.LandingBox.Destroy();
                    YamuxWindow.LandingBox = new VBox();
                    ResultSearchBox.Destroy();
                    ResultSearchBox = new VBox();
                    ResultBox.Add(ResultSearchBox);

                    Dictionary<string, string> best = Yamux.GetBest(root);
                    List<Dictionary<string, List<string>>> all = new List<Dictionary<string, List<string>>>();

                    try {all.Add(Yamux.GetArtist(root)); } catch (NullReferenceException) { } 
                    try { all.Add(Yamux.GetAlbums(root)); } catch (NullReferenceException) { } 
                    try { all.Add(Yamux.GetTracks(root)); } catch (NullReferenceException) { }
                    try { all.Add(Yamux.GetPodcasts(root)); } catch (NullReferenceException){ }
                    try { all.Add(Yamux.GetPlaylists(root)); } catch (NullReferenceException) { }

                    int index = -1;
                    foreach (var i in all)
                    {
                        index++;
                        if (i["type"][0] == best["type"])
                        {
                            all.Move(index, 0);
                            break;
                        } 
                    }

                    foreach (var i in all)
                    {
                        HBox box = new HBox();
                        ScrolledWindow scrolledWindow = new ScrolledWindow();
                        Viewport viewportWindow = new Viewport();
                        scrolledWindow.PropagateNaturalHeight = true;
                        
                        await Task.Run(() =>
                        {
                            Console.WriteLine(i["type"][0]);
                            if (i["type"][0] != "playlist")
                                box = Yamux.CreateBoxResultSearch(i["name"], i["coverUri"], i["id"], i["type"][0]);
                            else
                            {
                                kindPlaylist = i["kind"];
                                uidPlaylist = i["uid"];
                                box = Yamux.CreateBoxResultSearch(i["name"], i["coverUri"], new List<string>(),
                                    i["type"][0]);
                            }

                            box.Halign = Align.Start;
                        });
                        string typeResult = "";
                        switch (i["type"][0])
                        {
                            case "playlist": { typeResult = "Плейлисты"; break; }
                            case "album": { typeResult = "Альбомы"; break; }
                            case "podcast": { typeResult = "Выпуски подкастов"; break; }
                            case "track": { typeResult = "Треки"; break; }
                            case "artist": { typeResult = "Артисты"; break; }
                        }

                        Label trackLabel = new Label(typeResult);
                        FontDescription tpftrack = new FontDescription();
                        tpftrack.Size = 12288;
                        trackLabel.ModifyFont(tpftrack);

                        scrolledWindow.Add(viewportWindow);
                        viewportWindow.Add(box);
                        ResultSearchBox.Add(trackLabel);
                        ResultSearchBox.Add(scrolledWindow);
                        
                        SearchBox.ShowAll();
                        ResultBox.ShowAll();
                        ResultSearchBox.ShowAll();
                        PlayerBoxScale.Hide();
                        PlayerActionBox.Hide();
                    }

                    foreach (Button i in Yamux.ListButtonPlay)
                        i.Clicked += PlayButtonClick;
                    SearchOrNot = true;
                    spinnerProgress.Active = false;
                }
                else
                {
                    PlayerNameArtist.Text = "";
                    PlayerTitleTrack.Text = "";
                    PlayerImage = new Image();
                    PlayerBoxScale.Hide();
                    PlayerActionBox.Hide();
                    ResultSearchBox.Destroy();
                    IfNoResult.Text = "Нет результата😢";
                }
            }
        }
        private async void PlayButtonClick(object sender, EventArgs a)
        {
            Player.trackIds = new List<string>();
            Player.currentTrack = -1;
            Button buttonPlay = (Button) sender;
            JObject details = JObject.Parse(buttonPlay.Name);
            PlayerTitleTrack.MaxWidthChars = 17;
            PlayerNameArtist.MaxWidthChars = 17;

            PlayerImage.Show();
            Pixbuf imagePixbuf = new Pixbuf(System.IO.Path.GetFullPath("Svg/icons8_rock_music_100_negate50x50.png"));
            if (details["uri"].ToString() != "null")
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(details["uri"].ToString());
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    imagePixbuf = new Pixbuf(stream);
                }
                response.Close();
            }

            Player.ChangeCurrentTrack += () =>
            {
                try { Console.WriteLine(Player.trackIds[Player.currentTrack]); }
                catch (IndexOutOfRangeException) { Console.WriteLine("End playlist (event)"); }
            };
            switch (details["type"].ToString())
            {
                case "track":
                {
                    List<string> ids = new List<string>();
                    JToken informTrack = "{}";
                    
                    ids.Add(details["id"].ToString());
                    
                    await Task.Run(() => { informTrack = Track.GetInformTrack(ids)["result"]; });
                    
                    PlayerTitleTrack.Text = informTrack[0]["title"].ToString();
                    PlayerNameArtist.Text = informTrack[0]["artists"][0]["name"].ToString();
                    string directLinkToTrack = "";
                    await Task.Run(() => { directLinkToTrack = Player.GetDirectLinkWithTrack(details["id"].ToString()); });
                    Player.PlayUrlFile(directLinkToTrack);
                    break;
                }
                case "artist":
                {
                    JToken informArtist = "{}";
                    JToken trackArtist = "{}";
                    
                    await Task.Run(() => { informArtist = Artist.InformArtist(details["id"].ToString())["result"]; });
                    await Task.Run(() => { trackArtist = Artist.GetTrack(informArtist["artist"]["id"].ToString()); });
                    PlayerNameArtist.Text = informArtist["artist"]["name"].ToString();

                    foreach (var i in trackArtist["result"]["tracks"])
                    {
                        Player.trackIds.Add(i["id"].ToString());
                    }

                    Player.PlayPlaylist();
                    break;
                }
                case "playlist":
                {
                    JToken informPlaylist = "{}";
                    JToken trackPlaylist = "{}";

                    await Task.Run(() =>
                    {
                        informPlaylist =
                            Playlist.InformPlaylist(details["uid"].ToString(), details["kind"].ToString());
                    });
                    await Task.Run(() =>
                    {
                        trackPlaylist = Playlist.GetTrack(details["uid"].ToString(), details["kind"].ToString());
                    });
                    PlayerTitleTrack.Text = informPlaylist["result"]["title"].ToString();
                    PlayerNameArtist.Text = informPlaylist["result"]["description"].ToString();

                    foreach (var i in trackPlaylist["result"]["tracks"]) { Player.trackIds.Add(i["id"].ToString()); }

                    Player.PlayPlaylist();
                    break;
                }
                case "album":
                {
                    JToken informAlbum = "{}";
                    JToken trackAlbum = "{}";

                    await Task.Run(() => { informAlbum = Album.InformAlbum(details["id"].ToString()); });
                    await Task.Run(() => { trackAlbum = Album.GetTracks(details["id"].ToString()); });
                    
                    PlayerTitleTrack.Text = informAlbum["result"]["title"].ToString();
                    PlayerNameArtist.Text = informAlbum["result"]["artists"][0]["name"].ToString();
                    
                    foreach (var i in trackAlbum["result"]["volumes"][0]) { Player.trackIds.Add(i["id"].ToString()); Console.WriteLine(i["id"]);}
                    Player.PlayPlaylist();
                    
                    break;
                }
                case "podcast":
                {
                    List<string> ids = new List<string>();
                    JToken informPodcast = "{}";

                    ids.Add(details["id"].ToString());
                    await Task.Run(() => { informPodcast = Track.GetInformTrack(ids)["result"]; });
                    
                    PlayerTitleTrack.Text = informPodcast[0]["title"].ToString();
                    PlayerNameArtist.Text = informPodcast[0]["albums"][0]["title"].ToString();
                    Player.trackIds.Add(details["id"].ToString());
                    Player.PlayPlaylist();
                    break;
                }
            }

            PlayerScale.FillLevel = Player.GetLength();
            ChangeLengthTrack += () => { PlayerScale.Value = durationTrack; };
            PlayerImage.Pixbuf = imagePixbuf;
            
            PlayerBoxScale.ShowAll();
            PlayerActionBox.ShowAll();
        }
        private async void SpyChangeDurationTrack()
        {
            double oldDuration = durationTrack;
            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (Player.GetStatusPlayback() != PlaybackState.Stopped)
                        {
                            durationTrack = Player.GetPosition();
                            Thread.Sleep(1000);
                            if (oldDuration != durationTrack)
                            {
                                ChangeLengthTrack.Invoke();
                                oldDuration = durationTrack;
                            }
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    catch (NullReferenceException) { break; }
                }
            });
        }
        private void PlayerDownloadTrackOnClicked()
        {
            string pathToHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (Directory.Exists(pathToHome + "/YandexMusic") == false) { Directory.CreateDirectory("/home/kirill/YandexMusic/"); }

            string nameTrackFile = pathToHome + "/YandexMusic/" + artistTrack + " - " + titleTrack + ".mp3";
            Console.WriteLine(directLink);
            Player.DownloadUriWithThrottling(directLink, nameTrackFile);
        }
        private void ClickPauseOrPlay(object sender, EventArgs a)
        {
            if (Player.PlayTrackOrPause)
            {
                Pixbuf PlayerPausePixbuf = new Pixbuf("Svg/icons8-play.png");
                YamuxWindow.playPauseButton.Image = new Image(PlayerPausePixbuf);
            }
            else
            {
                Pixbuf playerPlayPixbuf = new Pixbuf("Svg/icons8-pause.png");
                YamuxWindow.playPauseButton.Image = new Image(playerPlayPixbuf);
            }
            Player.PauseOrStartPlay();
        }
    }
}