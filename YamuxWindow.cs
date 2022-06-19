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

                    Dictionary<string, string> best = Yamux.GetBest(root);;
                    List<Dictionary<string, List<string>>> all = new List<Dictionary<string, List<string>>>();
                    all.Add(Yamux.GetAlbums(root));
                    all.Add(Yamux.GetPlaylists(root));
                    all.Add(Yamux.GetPodcasts(root));
                    all.Add(Yamux.GetTracks(root));
                    all.Add(Yamux.GetArtist(root));

                    int index = -1;
                    foreach (var i in all)
                    {
                        index++;
                        if (i["type"][0] == best["type"])
                        {
                            break;
                        }
                    }
                    all.Move(index, 0);
                    
                    foreach (var i in all)
                    {
                        HBox box = new HBox();
                        ScrolledWindow scrolledWindow = new ScrolledWindow();
                        Viewport viewportWindow = new Viewport();
                        scrolledWindow.PropagateNaturalHeight = true;
                        scrolledWindow.PropagateNaturalWidth = true;
                        await Task.Run(() =>
                        {
                            if (i["type"][0] != "playlist")
                            {
                                box = Yamux.CreateBoxResultSearch(i["name"], i["coverUri"], i["id"], i["type"][0]);   
                            }
                        });
                        scrolledWindow.Add(viewportWindow);
                        viewportWindow.Add(box);
                        ResultSearchBox.Add(scrolledWindow);
                        
                        SearchBox.ShowAll();
                        ResultBox.ShowAll();
                        ResultSearchBox.ShowAll();
                    }
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
    static class Ext
    {
        public static void Move<T>(this List<T> list, int i, int j)
        {
            var elem = list[i];
            list.RemoveAt(i);
            list.Insert(j, elem);
        }
 
        public static void Swap<T>(this List<T> list, int i, int j)
        {
            var elem1 = list[i];
            var elem2 = list[j];
 
            list[i] = elem2;
            list[j] = elem1;
        }
    }
}