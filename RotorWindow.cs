using System;
using Gtk;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;
using YandexMusicApi;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

namespace Yamux;
public class RotorWindow : Window
{
    [UI] private Spinner spinnerProgress = null;
    [UI] private Box ResultBox = null;
    public RotorWindow() : this(YamuxWindow.YamuxWindowBuilder)
    {
    }

    private RotorWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
    {
        builder.Autoconnect(this);
        GenerateUI();
    }

    private async void GenerateUI()
    {
        spinnerProgress.Active = true;
        Grid sdasd = new Grid();
        sdasd.RowSpacing = 100;
        sdasd.ColumnSpacing = 100;
        JToken stationList = "";
        await Task.Run(() => { stationList = Rotor.StationList()["result"]; });

        /*
        Label a = new Label(stationList[0]["station"]["name"].ToString());
        a.MarginStart = 9;
        sdasd.Attach(a, 0, 0, 1, 1);
        Label b = new Label(stationList[1]["station"]["name"].ToString());
        b.MarginStart = 9;
        sdasd.Attach(b, 0, 0, 6, 1);
        
        Label c = new Label(stationList[2]["station"]["name"].ToString());
        c.MarginStart = 9;
        sdasd.Attach(c, 0, 0, 6, 2);
        Label d = new Label(stationList[3]["station"]["name"].ToString());
        d.MarginStart = 9;
        sdasd.Attach(d, 0, 0, 1, 2);
        */
        
        int labelPositionHeight = 1;
        bool labelPositonWidth = false;
        foreach (var i in stationList)
        {
            Label k = new Label(i["station"]["name"].ToString());
            k.MarginStart = 9;
            if (labelPositonWidth)
            {
                sdasd.Attach(k, 0, 0, 6, labelPositionHeight);
                labelPositonWidth = false;
                labelPositionHeight++;
            }
            else
            {
                sdasd.Attach(k, 0, 0, 1, labelPositionHeight);
                labelPositonWidth = true;
            }
        }
        ResultBox.Add(sdasd);
        ResultBox.ShowAll();
        spinnerProgress.Active = false;
    }
}