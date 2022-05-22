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
        [UI] private Label IfNoResult = null;
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
                    //Console.WriteLine(root);
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
                    IfNoResult.Text = "";
                    foreach (KeyValuePair<string, string> i in GetBest(root))
                    {
                        Console.WriteLine(i.Key + ";" + i.Value);
                    }
                }
                else
                {
                    IfNoResult.Text = "Нет результата😢";
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