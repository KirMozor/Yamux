using System;
using System.Collections.Generic;
using System.Linq;
using Cairo;
using Gtk;
using YandexMusicApi;
using Gdk;
using GLib;
using Newtonsoft.Json.Linq;
using Pango;
using Application = Gtk.Application;
using Rectangle = Gdk.Rectangle;
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
                    //Console.WriteLine(resultSearch.Last.Last);
                    root = resultSearch.Last.Last.Root;
                    root = root.SelectToken("result");
                    Console.WriteLine(root);
                }
            });
            ShowResultSearch(root, text);
        }

        private void ShowResultSearch(JToken root, string text)
        {
            if (text == SearchMusic.Text && !string.IsNullOrEmpty(SearchMusic.Text) && !string.IsNullOrEmpty(text))
            {
                Label NoResult = new Label("Нет результата😢");
                FontDescription tpfNoResult = new FontDescription();
                tpfNoResult.Size = 18432;
                NoResult.ModifyFont(tpfNoResult);
                ResultBox.Add(NoResult);
                
                if (root.Count() > 6)
                {
                    string typeBest = root.SelectToken("best").SelectToken("type").ToString();
                    string nameBest = root.SelectToken("best").SelectToken("result").SelectToken("name").ToString();
                    List<JToken> genresBest = root.SelectToken("best").SelectToken("result").SelectToken("genres").ToList();

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
                    }
                
                    Label TypeBest = new Label(typeBest);
                    FontDescription tpfTypeBest = new FontDescription();
                    tpfTypeBest.Size = 15360;
                    TypeBest.ModifyFont(tpfTypeBest);
                    ResultBox.Add(TypeBest);

                    Label NameBest = new Label(nameBest);
                    FontDescription tpfNameBest = new FontDescription();
                    tpfNameBest.Size = 11264;
                    NameBest.ModifyFont(tpfNameBest);

                    Button Asd = new Button(Stock.MediaPlay);
                    Box BestResultName = new HBox();
                    Box BestResultButtonPlay = new HBox();
                
                    ResultBox.Add(BestResultName);
                    ResultBox.Add(BestResultButtonPlay);
                    BestResultName.Add(NameBest);
                    BestResultButtonPlay.Add(Asd);

                    ResultBox.ShowAll();
                    BestResultName.ShowAll();
                    BestResultButtonPlay.ShowAll();
                }
                else
                {
                    ResultBox.ShowAll();
                }
            }
        }
        
        private Dictionary<string, string> GetBest(JToken root)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            root = root.SelectToken("best");
            result.Add("type", root.SelectToken("type").ToString());
            result.Add("id", root.SelectToken("result").SelectToken("id").ToString());

            return result;
        }
        
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}