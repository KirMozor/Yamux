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