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
                if (root.Count() > 6)
                {
                    BestBox.Destroy();
                    BestBox = new VBox();
                    ResultBox.Add(BestBox);
                    
                    IfNoResult.Text = "";
                    Dictionary<string, string> Best = GetBest(root);
                    string typeBest = Best["type"];
                    string nameBest = Best["name"];
                    
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
                    List<string> ArtistId = Artist["id"];
                    List<string> ArtistName = Artist["name"];
                    List<string> ArtistCoverUri = Artist["coverUri"];
                    

                    HBox d = CreateBox(ArtistId, ArtistName, ArtistCoverUri, Best);
                    ScrolledWindow b = new ScrolledWindow();
                    b.PropagateNaturalHeight = true;
                    b.PropagateNaturalWidth = true;
                    Viewport s = new Viewport();
                    Label TypeBestLabel = new Label(typeBest);
                    FontDescription tpfTypeBest = new FontDescription();
                    tpfTypeBest.Size = 12288;
                    TypeBestLabel.ModifyFont(tpfTypeBest);
                    
                    b.Add(s);
                    s.Add(d);

                    BestBox.Add(TypeBestLabel);
                    BestBox.Add(b);
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
        
        private Dictionary<string, string> GetBest(JToken root)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            root = root["best"];
            result.Add("type", root["type"].ToString());

            try
            {
                result.Add("uriCover", root["result"]["cover"]["uri"].ToString());
            }
            catch (NullReferenceException)
            {
                result.Add("uriCover", root["result"]["coverUri"].ToString());
            }
            
            try
            {
                result.Add("name", root["result"]["name"].ToString());
            }
            catch (NullReferenceException)
            {
                result.Add("name", root["result"]["title"].ToString());
            }
            
            try
            {
                result.Add("id", root["result"]["id"].ToString());
            }
            catch (NullReferenceException)
            {
                result.Add("uid", root["result"]["uid"].ToString());
                result.Add("kind", root["result"]["kind"].ToString());
            }

            return result;
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

        private HBox CreateBox(List<string> ArtistId, List<string> ArtistName, List<string> ArtistCoverUri, Dictionary<string, string> Best)
        {
            HBox NewBox = new HBox();

            int b = -1;
            foreach (string i in ArtistName)
            {
                b++;
                //Console.WriteLine(i + ";" + ArtistId[b] + ";" + ArtistCoverUri[b]);
                VBox BestCoverImage = new VBox();
                BestCoverImage.Spacing = 4;
                BestCoverImage.Valign = Align.Fill;
                
                NewBox.Add(BestCoverImage);
                NewBox.Spacing = 8;
                
                Label NameBestLabel = new Label(i);
                FontDescription tpfNameBest = new FontDescription();
                tpfNameBest.Size = 11264;
                NameBestLabel.ModifyFont(tpfNameBest);
                NameBestLabel.Halign = Align.Start;

                if (ArtistCoverUri[b] != "None")
                {
                    File.Delete("s.jpg");
                    string url = Best["uriCover"];
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
                    image.Halign = Align.Start;
                    BestCoverImage.Add(image);
                }
                else
                {
                    Pixbuf imagePixbuf;
                    imagePixbuf = new Pixbuf("/home/kirill/Downloads/icons8_rock_music_100_negate.png");
                    Image image = new Image(imagePixbuf);
                    image.Halign = Align.Start;
                    BestCoverImage.Add(image);
                }

                Button PlayButton0 = new Button(Stock.MediaPlay);
                PlayButton0.Halign = Align.Start;
                
                BestCoverImage.Add(NameBestLabel);
                BestCoverImage.Add(PlayButton0);
                NewBox.Add(BestCoverImage);
            }
            
            return NewBox;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}