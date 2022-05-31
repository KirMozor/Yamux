using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Gdk;
using Gtk;
using Newtonsoft.Json.Linq;
using Pango;

namespace Yamux
{
    public class Yamux
    {
        public static List<Button> ListButtonPlay = new List<Button>();

        public static void OpenLinkToWebBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        public static Dictionary<string, List<string>> GetPlaylist(JToken root)
        {
            Dictionary<string, List<string>> playlist = new Dictionary<string, List<string>>();
            List<string> playlistUid = new List<string>();
            List<string> playlistKind = new List<string>();
            List<string> playlistName = new List<string>();
            List<string> playlistCoverUri = new List<string>();

            foreach (JToken i in root["playlists"]["results"])
            {
                playlistUid.Add(i["uid"].ToString());
                playlistKind.Add(i["kind"].ToString());
                playlistName.Add(i["title"].ToString());
                try
                {
                    playlistCoverUri.Add(i["cover"]["uri"].ToString());
                }
                catch (NullReferenceException)
                {
                    playlistCoverUri.Add("None");
                }
            }

            playlist.Add("uid", playlistUid);
            playlist.Add("kind", playlistKind);
            playlist.Add("name", playlistName);
            playlist.Add("coverUri", playlistCoverUri);

            return playlist;
        }

        public static Dictionary<string, List<string>> GetPodcast(JToken root)
        {
            Dictionary<string, List<string>> podcast = new Dictionary<string, List<string>>();
            List<string> podcastId = new List<string>();
            List<string> podcastName = new List<string>();
            List<string> podcastCoverUri = new List<string>();

            foreach (JToken i in root["podcast_episodes"]["results"])
            {
                podcastId.Add(i["id"].ToString());
                podcastName.Add(i["title"].ToString());
                try
                {
                    podcastCoverUri.Add(i["coverUri"].ToString());
                }
                catch (NullReferenceException)
                {
                    podcastCoverUri.Add("None");
                }
            }

            podcast.Add("id", podcastId);
            podcast.Add("name", podcastName);
            podcast.Add("coverUri", podcastCoverUri);

            return podcast;
        }

        public static Dictionary<string, List<string>> GetTrack(JToken root)
        {
            Dictionary<string, List<string>> tracks = new Dictionary<string, List<string>>();
            List<string> trackId = new List<string>();
            List<string> trackName = new List<string>();
            List<string> trackCoverUri = new List<string>();

            foreach (JToken i in root["tracks"]["results"])
            {
                trackId.Add(i["id"].ToString());
                trackName.Add(i["title"].ToString());

                try
                {
                    trackCoverUri.Add(i["coverUri"].ToString());
                }
                catch (NullReferenceException)
                {
                    trackCoverUri.Add("None");
                }
            }

            tracks.Add("id", trackId);
            tracks.Add("name", trackName);
            tracks.Add("coverUri", trackCoverUri);

            return tracks;
        }

        public static Dictionary<string, List<string>> GetArtist(JToken root)
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

        public static HBox CreateBoxResultSearch(List<string> name, List<string> coverUri, List<string> id, string typeResult = "dsad")
        {
            HBox newBox = new HBox();

            int b = -1;
            foreach (string i in name)
            {
                b++;
                VBox coverImage = new VBox();
                coverImage.Spacing = 4;
                coverImage.MarginTop = 20;
                coverImage.MarginBottom = 15;
                coverImage.Valign = Align.Fill;
                        
                newBox.Add(coverImage);
                newBox.Spacing = 8;
                        
                Label nameBestLabel = new Label(i);
                FontDescription tpfNameBest = new FontDescription();
                tpfNameBest.Size = 11264;
                nameBestLabel.ModifyFont(tpfNameBest);
                nameBestLabel.Halign = Align.Fill;
        
                if (coverUri[b] != "None")
                {
                    File.Delete("s.jpg");
                    string url = coverUri[b];
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
                    image.Halign = Align.Fill;
                    Button buttonPlay = new Button();
                    buttonPlay.Image = image;
                    buttonPlay.Relief = ReliefStyle.None;
                    if (typeResult != "playlist")
                    {
                        buttonPlay.Name = "{'type': \"" + typeResult + "\",'id': \"" + id[b] + "\" }";
                    }

                    ListButtonPlay.Add(buttonPlay);
                    coverImage.Add(buttonPlay);
                }
                else
                {
                    Pixbuf imagePixbuf;
                    imagePixbuf = new Pixbuf("Svg/icons8_rock_music_100_negate.png");
                    Image image = new Image(imagePixbuf);
                    image.Halign = Align.Fill;
                    Button buttonPlay = new Button();
                    buttonPlay.Image = image;
                    buttonPlay.Relief = ReliefStyle.None;
                    if (typeResult != "playlist")
                    {
                        buttonPlay.Name = "{'type': \"" + typeResult + "\",'id': \"" + id[b] + "\" }";
                    }

                    ListButtonPlay.Add(buttonPlay);
                    coverImage.Add(buttonPlay);
                }
                coverImage.Add(nameBestLabel);
                newBox.Add(coverImage);
            }
                    
            return newBox;
        }
    }
}