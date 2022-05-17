using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gtk;
using YandexMusicApi;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

namespace Yamux
{
    class YamuxWindow : Window
    {
        [UI] private SearchEntry SearchMusic = null;
        [UI] private Box ResultBox = null;
        [UI] private Box asd = null;
        [UI] private Box asd1 = null;
        [UI] private Box asd2 = null;
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
                ResultBox.Add(TypeBest);

                Label NameBest = new Label(nameBest);
                asd.Add(NameBest);

                ResultBox.ShowAll();
                asd.ShowAll();
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