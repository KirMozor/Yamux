using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        [UI] private Button LandingPageButton = null;
        [UI] private Button AboutProgram = null;
        [UI] private Box SearchBox = null;
        [UI] private Box ResultBox = null;
        [UI] private Label IfNoResult = null;
        [UI] private Image AboutImage = null;
        [UI] private Image ImageSettings = null;

        [UI] private Box informTrackBox = null;
        [UI] private Box PlayerActionBox = null;
        [UI] private Box PlayerMoreActionBox = null;
        [UI] private Box PlayerBoxScale = null;
        [UI] private Label PlayerNameArtist = null;
        [UI] private Label PlayerTitleTrack = null;
        [UI] private Image PlayerImage = null;
        
        public static Button playPauseButton = new Button();
        public static Button nextTrackButton = new Button();
        public static Button lastTrackButton = new Button();
        public static Button stopButton = new Button();
        public static Button downloadTrack = new Button();

        public static Button myWaveButton = new Button();
        public static VBox LandingBox = new VBox();
        public static bool SearchOrNot = true;
        public static HScale PlayerScale = new HScale(0.0, 100, 1.0);
        
        private VBox ResultSearchBox = new VBox();
        public static Builder YamuxWindowBuilder = new Builder("Yamux.glade");
        public YamuxWindow() : this(YamuxWindowBuilder)
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

            DeleteEvent += (o, args) => { Application.Quit(); };
            AboutProgram.Relief = ReliefStyle.None;
            AboutProgram.Clicked += ShowAboutWindow;
            AboutDonateMe.Clicked += ShowDonateWindow;
            LandingPageButton.Clicked += (sender, args) => { GenerateLanding(); };

            CreatePlayer();
            SetDefaultIconFromFile("Svg/icon.svg");
            AboutImage.Pixbuf = new Pixbuf("Svg/about_icon.svg");
            ImageSettings.Pixbuf = new Pixbuf("Svg/icons8-settings-20.png");
            
            new Search();
            GenerateLanding();
        }

        private void GenerateLanding()
        {
            IfNoResult.Text = "";
            PlayerNameArtist.Text = "";
            PlayerTitleTrack.Text = "";
            PlayerImage.Hide();
            PlayerBoxScale.Hide();
            PlayerActionBox.Hide();
                    
            LandingBox.Destroy();
            ResultSearchBox.Destroy();
            ResultSearchBox = new VBox();
            LandingBox = new VBox();
            ResultBox.Add(LandingBox);

            myWaveButton = new Button();
            JToken myWaveImage = Rotor.GetInfo("user:onyourwave")["result"][0]["station"]["fullImageUrl"];
            
            myWaveImage = myWaveImage.ToString().Replace("%%", "100x100");
            Pixbuf imagePixbuf = new Pixbuf(System.IO.Path.GetFullPath("Svg/icons8_rock_music_100_negate50x50.png"));
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://" + myWaveImage);
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                imagePixbuf = new Pixbuf(stream);
            }

            Image waveButtonImage = new Image();
            waveButtonImage.Pixbuf = imagePixbuf;
            response.Close();

            myWaveButton.Relief = ReliefStyle.None;
            myWaveButton.Image = waveButtonImage;
            LandingBox.Add(myWaveButton);
            myWaveButton.Clicked += PlayMyWave;
            
            SearchBox.ShowAll();
            LandingBox.ShowAll();
            ResultBox.ShowAll();
            PlayerBoxScale.Hide();
            PlayerActionBox.Hide();
        }

        private async void PlayMyWave(object sender, EventArgs a)
        {
            bool StopAwait = false;
            bool StopWave = false;
            stopButton.Clicked += (o, args) => { StopWave = true; };
            while (true)
            {
                if (StopWave)
                {
                    StopWave = false;
                    Bass.Free();
                    Bass.Init();
                    break;
                }
                JObject myWaveReturn = new JObject();
                await Task.Run(() => { myWaveReturn = Rotor.GetTrack("user:onyourwave"); });
            
                JToken informTrack = "{}";
                string ids = myWaveReturn["result"]["sequence"][0]["track"]["id"].ToString();
                await Task.Run(() => { informTrack = Track.GetInformTrack(new List<string> {ids})["result"]; });
                    
                PlayerTitleTrack.Text = informTrack[0]["title"].ToString();
                PlayerNameArtist.Text = informTrack[0]["artists"][0]["name"].ToString();
                string directLinkToTrack = Player.GetDirectLinkWithTrack(ids);
                Player.PlayUrlFile(directLinkToTrack);
            
                PlayerScale.FillLevel = Player.GetLength();
                ChangeLengthTrack += () => { PlayerScale.Value = durationTrack; };
                PlayerBoxScale.ShowAll();
                PlayerActionBox.ShowAll();
                
                await Task.Run(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(1000);//Просто сон async метода, чтобы пк не офигел от проверки
                        if (Bass.ChannelGetPosition(Player.stream) != -1)
                        {
                            if (Bass.ChannelGetLength(Player.stream) == Bass.ChannelGetPosition(Player.stream) || StopAwait) //Если трек закончился, начать следующию песню
                            {
                                StopAwait = false;
                                Bass.Free();
                                Bass.Init();
                                break;
                            }   
                        }
                    }
                });
            }
        }
        private void ShowAboutWindow(object sender, EventArgs a)
        {
            AboutWindow.ShowAll();
            AboutWindow.Deletable = false;
            
            CloseAboutWindow.Clicked += (o, args) => { AboutWindow.Hide(); };
            AboutGitHubProject.Clicked += (o, args) => { Yamux.OpenLinkToWebBrowser("https://github.com/KirMozor/Yamux"); };
            AboutGitHubAuthor.Clicked += (o, args) => { Yamux.OpenLinkToWebBrowser("https://github.com/KirMozor"); };
            AboutTelegramChannel.Clicked += (o, args) => { Yamux.OpenLinkToWebBrowser("https://t.me/kirmozor"); };
        }
        private void ShowDonateWindow(object sender, EventArgs a)
        {
            DonateWindow.ShowAll();
            DonateWindow.Deletable = false;
            CloseDonateWindow.Clicked += (o, args) => { DonateWindow.Hide(); };
            KofiDonate.Clicked += (o, args) => { Yamux.OpenLinkToWebBrowser("https://ko-fi.com/kirmozor"); };
        }
        private void CreatePlayer()
        {
            PlayerNameArtist.Text = "";
            PlayerTitleTrack.Text = "";
            PlayerBoxScale = new HBox();
            PlayerBoxScale.Valign = Align.Start;
            PlayerBoxScale.Vexpand = true;

            PlayerScale.Vexpand = true;
            PlayerScale.Valign = Align.Start;
            PlayerScale.DrawValue = false;
            PlayerBoxScale.Add(PlayerScale);
            
            playPauseButton.Relief = ReliefStyle.None;
            nextTrackButton.Relief = ReliefStyle.None;
            lastTrackButton.Relief = ReliefStyle.None;
            stopButton.Relief = ReliefStyle.None;
            downloadTrack.Relief = ReliefStyle.None;
            
            playPauseButton.Image = new Image(new Pixbuf(System.IO.Path.GetFullPath("Svg/icons8-pause.png")));
            nextTrackButton.Image = new Image(new Pixbuf(System.IO.Path.GetFullPath("Svg/icons8-next.png")));
            lastTrackButton.Image = new Image(new Pixbuf(System.IO.Path.GetFullPath("Svg/icons8-previous.png")));
            stopButton.Image = new Image(new Pixbuf(System.IO.Path.GetFullPath("Svg/icons8-stop.png")));
            downloadTrack.Image = new Image(new Pixbuf(System.IO.Path.GetFullPath("Svg/icons8-download.png")));
            
            PlayerActionBox.Add(stopButton);
            PlayerActionBox.Add(lastTrackButton);
            PlayerActionBox.Add(playPauseButton);
            PlayerActionBox.Add(nextTrackButton);
            PlayerActionBox.Add(downloadTrack);

            SearchBox.Add(PlayerBoxScale);
            SearchBox.Add(PlayerActionBox);
            PlayerBoxScale.Hide();
            PlayerActionBox.Hide();
            SearchBox.Hide();
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