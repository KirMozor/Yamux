using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Gdk;
using Gtk;
using Newtonsoft.Json.Linq;
using Pango;
using Application = Gtk.Application;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

namespace Yamux
{
    class YamuxWindow : Window
    {
        [UI] private SearchEntry SearchMusic = null;
        [UI] private Box ResultBox = null;
        [UI] private Label IfNoResult = null;
        private VBox BestBox = new VBox();
        public YamuxWindow() : this(new Builder("Yamux.glade"))
        {
        }

        private YamuxWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
            SearchMusic.SearchChanged += SearchChangedOutput;
            SetDefaultIconFromFile("Svg/icon.svg");
        }

        async void SearchChangedOutput(object sender, EventArgs a)
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

        private void ShowResultSearch(JToken root, string text)
        {
            if (text == SearchMusic.Text && !string.IsNullOrEmpty(SearchMusic.Text) && !string.IsNullOrEmpty(text))
            {
                Dictionary<string, List<string>> result = GetTrack(root);
                if (root.Count() > 6)
                {
                    BestBox.Destroy();
                    BestBox = new VBox();
                    ResultBox.Add(BestBox);
                    
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
                            typeBest = "Подкасты";
                            break;
                        case "album":
                            typeBest = "Альбом";
                            break;
                    }
                    
                    Dictionary<string, List<string>> Artist = GetArtist(root);
                    Dictionary<string, List<string>> Track = GetTrack(root);
                    List<string> ArtistName = Artist["name"];
                    List<string> ArtistCoverUri = Artist["coverUri"];
                    List<string> TrackName = Track["name"];
                    List<string> TrackCoverUri = Track["coverUri"];

                    HBox ArtistBox = CreateBoxResultSearch(ArtistName, ArtistCoverUri);
                    HBox TrackBox = CreateBoxResultSearch(TrackName, TrackCoverUri);

                    ScrolledWindow ScrolledArtist = new ScrolledWindow();
                    ScrolledWindow ScrolledTrack = new ScrolledWindow();
                    ScrolledArtist.PropagateNaturalHeight = true;
                    ScrolledArtist.PropagateNaturalWidth = true;
                    ScrolledTrack.PropagateNaturalHeight = true;
                    ScrolledTrack.PropagateNaturalWidth = true;
                    
                    Viewport ViewportArtist = new Viewport();
                    Viewport ViewportTrack = new Viewport();
                    
                    Label ArtistLabel = new Label(typeBest);
                    FontDescription tpfArtist = new FontDescription();
                    tpfArtist.Size = 12288;
                    ArtistLabel.ModifyFont(tpfArtist);
                    Label TrackLabel = new Label("Треки");
                    FontDescription tpfTrack = new FontDescription();
                    tpfTrack.Size = 12288;
                    ArtistLabel.ModifyFont(tpfTrack);
                    
                    ScrolledArtist.Add(ViewportArtist);
                    ViewportArtist.Add(ArtistBox);
                    ScrolledTrack.Add(ViewportTrack);
                    ViewportTrack.Add(TrackBox);

                    BestBox.Add(ArtistLabel);
                    BestBox.Add(ScrolledArtist);
                    BestBox.Add(TrackLabel);
                    BestBox.Add(ScrolledTrack);
                    ResultBox.ShowAll();
                    BestBox.ShowAll();
                    Console.WriteLine("dasdasd");
                }
                else
                {
                    BestBox.Destroy();
                    IfNoResult.Text = "Нет результата😢";
                }
            }
        }

        private Dictionary<string, List<string>> GetTrack(JToken root)
        {
            Dictionary<string, List<string>> tracks = new Dictionary<string, List<string>>();
            List<string> trackId = new List<string>();
            List<string> trackName = new List<string>();
            List<string> trackCoverUri = new List<string>();

            foreach (JToken i in root["tracks"]["results"])
            {
                trackId.Add(i["id"].ToString());
                trackName.Add(i["title"].ToString());

                try
                {
                    trackCoverUri.Add(i["coverUri"].ToString());
                }
                catch (NullReferenceException)
                {
                    trackCoverUri.Add("None");
                }
            }
            tracks.Add("id", trackId);
            tracks.Add("name", trackName);
            tracks.Add("coverUri", trackCoverUri);

            return tracks;
        }

        private Dictionary<string, List<string>> GetArtist(JToken root)
        {
            Dictionary<string, List<string>> artist = new Dictionary<string, List<string>>();
            List<string> artistId = new List<string>();
            List<string> artistName = new List<string>();
            List<string> artistCoverUri = new List<string>();

            foreach (JToken i in root["artists"]["results"])
            {
                artistId.Add(i["id"].ToString());
                artistName.Add(i["name"].ToString());

                try
                {
                    artistCoverUri.Add(i["cover"]["uri"].ToString());
                }
                catch (NullReferenceException)
                {
                    artistCoverUri.Add("None");
                }
            }
            artist.Add("id", artistId);
            artist.Add("name", artistName);
            artist.Add("coverUri", artistCoverUri);
            
            return artist;
        }
        
        private HBox CreateBoxResultSearch(List<string> Name, List<string> CoverUri)
        {
            HBox NewBox = new HBox();

            int b = -1;
            foreach (string i in Name)
            {
                b++;
                VBox CoverImage = new VBox();
                CoverImage.Spacing = 4;
                CoverImage.MarginTop = 20;
                CoverImage.MarginBottom = 15;
                CoverImage.Valign = Align.Fill;
                
                NewBox.Add(CoverImage);
                NewBox.Spacing = 8;
                
                Label NameBestLabel = new Label(i);
                FontDescription tpfNameBest = new FontDescription();
                tpfNameBest.Size = 11264;
                NameBestLabel.ModifyFont(tpfNameBest);
                NameBestLabel.Halign = Align.Fill;

                if (CoverUri[b] != "None")
                {
                    File.Delete("s.jpg");
                    string url = CoverUri[b];
                    using (WebClient client = new WebClient())
                    {
                        url = url.Replace("%%", "100x100");
                        url = "https://" + url;
                        Console.WriteLine(url);
                        client.DownloadFile(new Uri(url), "s.jpg");
                    }

                    Pixbuf imagePixbuf;
                    imagePixbuf = new Pixbuf("s.jpg");
                    Image image = new Image(imagePixbuf);
                    image.Halign = Align.Fill;
                    CoverImage.Add(image);
                }
                else
                {
                    Pixbuf imagePixbuf;
                    imagePixbuf = new Pixbuf("Svg/icons8_rock_music_100_negate.png");
                    Image image = new Image(imagePixbuf);
                    image.Halign = Align.Fill;
                    CoverImage.Add(image);
                }

                Button PlayButton0 = new Button(Stock.MediaPlay);
                PlayButton0.Halign = Align.Fill;
                
                CoverImage.Add(NameBestLabel);
                CoverImage.Add(PlayButton0);
                NewBox.Add(CoverImage);
            }
            
            return NewBox;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}