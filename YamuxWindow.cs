using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Gdk;
using Gtk;
using ManagedBass;
using Newtonsoft.Json.Linq;
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
        [UI] private Dialog DonateWindow = null;
        [UI] private Window AboutWindow = null;
        [UI] private ApplicationWindow LoginYamux = null;
        [UI] private Window PreStartWindow = null;

        [UI] private Window HowToYourTheme = null;
        [UI] private Button DarkButton = null;
        [UI] private Button LightButton = null;

        [UI] private Image YandexImage = null;
        [UI] private Entry SetLogin = null;
        [UI] private Entry SetPassword = null;
        [UI] private Button ResetPassword = null;
        [UI] private Label ResultLogin = null;
        [UI] private Button LoginYamuxButton = null;

        [UI] private Button AboutGitHubProject = null;
        [UI] private Button AboutGitHubAuthor = null;
        [UI] private Button AboutTelegramChannel = null;
        [UI] private Button AboutDonateMe = null;
        [UI] private Button KofiDonate = null;
        [UI] private Button CloseAboutWindow = null;
        [UI] private Button CloseDonateWindow = null;

        [UI] private Button RotorPageButton = null;
        [UI] private Button LandingPageButton = null;
        [UI] private Button AboutProgram = null;
        [UI] private Box SearchBox = null;
        [UI] private Box ResultBox = null;
        [UI] private Label IfNoResult = null;
        [UI] private Image AboutImage = null;
        [UI] private Image ImageSettings = null;

        [UI] private Box PlayerActionBox = null;
        [UI] private Box PlayerBoxScale = null;
        [UI] private  Label PlayerNameArtist = null;
        [UI] private Label PlayerTitleTrack = null;
        [UI] private Image PlayerImage = null;
        
        public static Button playPauseButton = new Button();
        private static Button nextTrackButton = new Button();
        private static Button lastTrackButton = new Button();
        private static Button stopButton = new Button();
        private static Button downloadTrack = new Button();

        private static Button myWaveButton = new Button();
        public static VBox LandingBox = new VBox();
        private static HScale PlayerScale = new HScale(0.0, 100, 1.0);
        
        private VBox ResultSearchBox = new VBox();
        public static Builder YamuxWindowBuilder = new Builder("Yamux.glade");
        public YamuxWindow() : this(YamuxWindowBuilder)
        {
            stopButton.Clicked += Player.StopTrack;
            playPauseButton.Clicked += Search.ClickPauseOrPlay;
            lastTrackButton.Clicked += Player.LastTrack;
            nextTrackButton.Clicked += Player.NextTrack;
            downloadTrack.Clicked += PlayerDownloadTrackOnClicked;
        }
        private YamuxWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += (o, args) => { Application.Quit(); };
            if (Directory.Exists("Svg")) {SetDefaultIconFromFile("Svg/yandex_en_icon-icons.com_61632(1).png");}
            
            /*
            AboutProgram.Relief = ReliefStyle.None;
            AboutProgram.Clicked += ShowAboutWindow;
            AboutDonateMe.Clicked += ShowDonateWindow;
            Player.ChangeCurrentTrack += () => { ShowCurrentTrack(); };
            LandingPageButton.Clicked += (sender, args) => { GenerateLanding(); };
            RotorPageButton.Clicked += (sender, args) => { GenerateRotor(); };
            
            CreatePlayer();
            SetDefaultIconFromFile("Svg/icon.svg");
            AboutImage.Pixbuf = new Pixbuf("Svg/about_icon.svg");
            ImageSettings.Pixbuf = new Pixbuf("Svg/icons8-settings-20.png");
            
            GenerateLanding();
            */
            PreStart();
        }

        private async void PreStart()
        {
            if (!Directory.Exists("Svg"))
            {
                HowToYourTheme.ShowAll();
                DarkButton.Clicked += (sender, args) => { DownloadIcons("dark"); };
                LightButton.Clicked += (sender, args) => { DownloadIcons("light"); };
            }
            else if (!File.Exists("config.toml")) { GetTokenGui(); }
            else
            {
                PreStartWindow.ShowAll();
                using (FileStream fstream = File.OpenRead("config.toml"))
                {
                    byte[] buffer = new byte[fstream.Length];
                    fstream.Read(buffer, 0, buffer.Length);
                    string textFromFile = Encoding.Default.GetString(buffer);

                    var model = Toml.ToModel(textFromFile);
                    Token.token = (string) model["yandex_token"]!;
                }

                try
                {
                    await Task.Run(() => { Account.ShowSettings(); });
                    PreStartWindow.Hide();
                    LoginYamux.Hide();
                    
                    AboutProgram.Relief = ReliefStyle.None;
                    AboutProgram.Clicked += ShowAboutWindow;
                    AboutDonateMe.Clicked += ShowDonateWindow;
                    Player.ChangeCurrentTrack += () => { ShowCurrentTrack(); };
                    LandingPageButton.Clicked += (sender, args) => { GenerateLanding(); };
                    RotorPageButton.Clicked += (sender, args) => { GenerateRotor(); };
            
                    CreatePlayer();
                    SetDefaultIconFromFile("Svg/yandex_en_icon-icons.com_61632(1).png");
                    AboutImage.Pixbuf = new Pixbuf("Svg/yandex_en_icon-icons.com_61632(1).png");
                    ImageSettings.Pixbuf = new Pixbuf("Svg/icons8-settings-20.png");
            
                    GenerateLanding();
                }
                catch (WebException)
                {
                    PreStartWindow.Hide();
                    LoginYamux.Hide();
                    GetTokenGui();
                }
            }
        }
        private void GenerateRotor()
        {
            IfNoResult.Text = "";
            if (Player.currentTrack != -1)
            {
                PlayerNameArtist.Text = "";
                PlayerTitleTrack.Text = "";
                PlayerImage.Hide();
                PlayerBoxScale.Hide();
                PlayerActionBox.Hide();   
            }
            LandingBox.Destroy();
            Search.ResultSearchBox.Destroy();
            Search.ResultSearchBox = new VBox();
            
            PlayerBoxScale.Hide();
            PlayerActionBox.Hide();
            new RotorWindow();
        }
        private void GenerateLanding()
        {
            IfNoResult.Text = "";
            if (Player.stream == 0)
            {
                PlayerNameArtist.Text = "";
                PlayerTitleTrack.Text = "";
                PlayerImage.Hide();
                PlayerBoxScale.Hide();
                PlayerActionBox.Hide();   
            }
            LandingBox.Destroy();
            Search.ResultSearchBox.Destroy();
            Search.ResultSearchBox = new VBox();
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
            if (Player.stream == 0)
            {
                PlayerBoxScale.Hide();
                PlayerActionBox.Hide();   
            }
            new Search();
        }
        private async void PlayMyWave(object sender, EventArgs a)
        {
            bool StopAwait = false;
            bool StopWave = false;
            stopButton.Clicked += (o, args) => { StopWave = true; };
            while (true)
            {
                Player.trackIds = new List<string>();
                if (StopWave)
                {
                    StopWave = false;
                    Bass.Free();
                    Bass.Init();
                    break;
                }
                JObject myWaveReturn = new JObject();
                await Task.Run(() => { myWaveReturn = Rotor.GetTrack("user:onyourwave"); });
                Console.WriteLine(Rotor.GetInfo("user:onyourwave"));
                string ids = myWaveReturn["result"]["sequence"][0]["track"]["id"].ToString();

                PlayerScale.FillLevel = Player.GetLength();
                ChangeLengthTrack += () => { PlayerScale.Value = durationTrack; };
                Player.TrackNext += () => { StopAwait = true; };
                Console.WriteLine(ids);
                Player.trackIds.Add(ids);
                Console.WriteLine(Player.trackIds.Count);
                Player.PlayUrlFile();
                
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

            SearchBox.Add(PlayerActionBox);
            PlayerBoxScale.Hide();
            PlayerActionBox.Hide();
            SearchBox.Hide();
        }
        private async void ShowCurrentTrack()
        {
            Pixbuf playerPlayPixbuf = new Pixbuf("Svg/icons8-pause.png");
            playPauseButton.Image = new Image(playerPlayPixbuf);
            if (Player.currentTrack != -1 || Player.trackIds.Count == 1)
            {
                JToken trackInform = "";
                if (Player.currentTrack != -1)
                {
                    await Task.Run(() => { trackInform = Track.GetInformTrack(new List<string>() { Player.trackIds[Player.currentTrack] })["result"][0]; });
                }
                else
                {
                    await Task.Run(() => { trackInform = Track.GetInformTrack(new List<string>() { Player.trackIds[0] })["result"][0]; });
                }
                Console.WriteLine(trackInform);
                
                Pixbuf imagePixbuf = new Pixbuf(System.IO.Path.GetFullPath("Svg/icons8_rock_music_100_negate50x50.png"));
                PlayerImage.Pixbuf = imagePixbuf;
                PlayerTitleTrack.Text = trackInform["title"].ToString();
                PlayerNameArtist.Text = trackInform["artists"][0]["name"].ToString();
                PlayerScale.Show();
                PlayerBoxScale.ShowAll();
                PlayerActionBox.ShowAll();
                await Task.Run(() =>
                {
                    string url = "https://" + trackInform["albums"][0]["coverUri"].ToString().Replace("%%", "50x50");
                    Console.WriteLine(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                    using (Stream stream = response.GetResponseStream()) 
                    { 
                        imagePixbuf = new Pixbuf(stream);
                    }
                    response.Close();
                });
                PlayerImage.Pixbuf = imagePixbuf;
                PlayerImage.ShowAll();
            }
        }
        private void GetTokenGui()
        {
            if (File.Exists("config.toml")) { File.Delete("config.toml"); }
            LoginYamux.ShowAll();
            ResetPassword.Clicked += (sender, args) => { Login.ResetPasswordOpen(); }; 
            LoginYamuxButton.Clicked += (sender, args) =>
            {
                ResultLogin.Text = Login.LogInButton(SetLogin.Text, SetPassword.Text);
                if (ResultLogin.Text == "ok")
                {
                    PreStart();
                }
            };
        }
        private void PlayerDownloadTrackOnClicked(object sender, EventArgs a)
        {
            string pathToHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (Directory.Exists(pathToHome + "/YandexMusic") == false) { Directory.CreateDirectory("/home/kirill/YandexMusic/"); }

            string nameTrackFile = pathToHome + "/YandexMusic/" + PlayerNameArtist.Text + " - " + PlayerTitleTrack.Text + ".mp3";
            Console.WriteLine(Search.directLink);
            Player.DownloadUriWithThrottling(Search.directLink, nameTrackFile);
        }
        private void DownloadIcons(string theme)
        {
            Directory.CreateDirectory("Svg");
            List<Uri> IconList = new List<Uri>();
            if (theme == "dark")
            {
                IconList = new List<Uri>()
                {
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/about_icon.svg"), //about_icon.svg
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/icon.svg"), //icon.svg
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/icons8_rock_music_100_negate.png"), //icons8_rock_music_100_negate.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/icons8-download.png"), //icons8-download.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/icons8-next.png"), //icons8-next.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/icons8-pause.png"), //icons8-pause.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/icons8-play.png"), //icons8-play.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/icons8-previous.png"), //icons8-previous.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/icons8-settings-20.png"), //icons8-settings-20.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/icons8-stop.png"), //icons8-stop.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/yandex_en_icon-icons.com_61632(1).png"), //yandex_en_icon-icons.com_61632(1).png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/dark/icons8_rock_music_100_negate50x50.png"), //icons8_rock_music_100_negate50x50.png
                };
            }
            else
            {
                IconList = new List<Uri>()
                {
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/about_icon.svg"), //about_icon.svg
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/icon.svg"), //icon.svg
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/icons8_rock_music_100_negate.png"), //icons8_rock_music_100_negate.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/icons8_download.png"), //icons8-download.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/icons8_next.png"), //icons8-next.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/icons8_pause.png"), //icons8-pause.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/icons8_play.png"), //icons8-play.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/icons8_previous.png"), //icons8-previous.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/icons8_settings_20.png"), //icons8-settings-20.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/icons8_stop.png"), //icons8-stop.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/yandex_en_icon-icons.com_61632(1).png"), //yandex_en_icon-icons.com_61632(1).png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/light/icons8_rock_music_100_negate50x50.png"), //icons8_rock_music_100_negate50x50.png
                };
            }
            Player.DownloadWithSync(IconList[0], "Svg/about_icon.svg");
            Player.DownloadWithSync(IconList[1], "Svg/icon.svg");
            Player.DownloadWithSync(IconList[2], "Svg/icons8_rock_music_100_negate.png");
            Player.DownloadWithSync(IconList[3], "Svg/icons8-download.png");
            Player.DownloadWithSync(IconList[4], "Svg/icons8-next.png");
            Player.DownloadWithSync(IconList[5], "Svg/icons8-pause.png");
            Player.DownloadWithSync(IconList[6], "Svg/icons8-play.png");
            Player.DownloadWithSync(IconList[7], "Svg/icons8-previous.png");
            Player.DownloadWithSync(IconList[8], "Svg/icons8-settings-20.png");
            Player.DownloadWithSync(IconList[9], "Svg/icons8-stop.png");
            Player.DownloadWithSync(IconList[10], "Svg/yandex_en_icon-icons.com_61632(1).png");
            Player.DownloadWithSync(IconList[11], "Svg/icons8_rock_music_100_negate50x50.png");
            
            HowToYourTheme.Hide();
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