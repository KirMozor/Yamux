using System;
using Gtk;
using Task = System.Threading.Tasks.Task;
using YandexMusicApi;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;
using Tomlyn;
using System.Text;
using System.IO;

namespace Yamux;
public class RotorWindow : Window
{
    [UI] private Spinner spinnerProgress = null;
    [UI] private Box SearchMusicBox = null;
    public static Box RotorBox = null;
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
        await Task.Run(() => { Console.WriteLine(Rotor.StationList()); });
        spinnerProgress.Active = false;
    }
}