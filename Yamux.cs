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

        public static Dictionary<string, List<string>> GetPlaylists(JToken root)
        {
            Dictionary<string, List<string>> playlist = new Dictionary<string, List<string>>();
            List<string> type = new List<string>();
            List<string> playlistUid = new List<string>();
            List<string> playlistKind = new List<string>();
            List<string> playlistName = new List<string>();
            List<string> playlistCoverUri = new List<string>();

            type.Add("playlist");
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

            playlist.Add("type", type);
            playlist.Add("uid", playlistUid);
            playlist.Add("kind", playlistKind);
            playlist.Add("name", playlistName);
            playlist.Add("coverUri", playlistCoverUri);

            return playlist;
        }

        public static Dictionary<string, List<string>> GetPodcasts(JToken root)
        {
            Dictionary<string, List<string>> podcast = new Dictionary<string, List<string>>();
            List<string> type = new List<string>();
            List<string> podcastId = new List<string>();
            List<string> podcastName = new List<string>();
            List<string> podcastCoverUri = new List<string>();

            type.Add("podcast");
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

            podcast.Add("type", type);
            podcast.Add("id", podcastId);
            podcast.Add("name", podcastName);
            podcast.Add("coverUri", podcastCoverUri);

            return podcast;
        }

        public static Dictionary<string, List<string>> GetTracks(JToken root)
        {
            Dictionary<string, List<string>> tracks = new Dictionary<string, List<string>>();
            List<string> type = new List<string>();
            List<string> trackId = new List<string>();
            List<string> trackName = new List<string>();
            List<string> trackCoverUri = new List<string>();

            type.Add("track");
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

            tracks.Add("type", type);
            tracks.Add("id", trackId);
            tracks.Add("name", trackName);
            tracks.Add("coverUri", trackCoverUri);

            return tracks;
        }

        public static Dictionary<string, List<string>> GetArtist(JToken root)
        {
            Dictionary<string, List<string>> artist = new Dictionary<string, List<string>>();
            List<string> type = new List<string>();
            List<string> artistId = new List<string>();
            List<string> artistName = new List<string>();
            List<string> artistCoverUri = new List<string>();

            type.Add("artist");
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

            artist.Add("type", type);
            artist.Add("id", artistId);
            artist.Add("name", artistName);
            artist.Add("coverUri", artistCoverUri);

            return artist;
        }

        public static Dictionary<string, List<string>> GetAlbums(JToken root)
        {
            Dictionary<string, List<string>> albums = new Dictionary<string, List<string>>();
            List<string> type = new List<string>();
            List<string> albumsId = new List<string>();
            List<string> albumsName = new List<string>();
            List<string> albumsCoverUri = new List<string>();

            type.Add("album");
            foreach (JToken i in root["albums"]["results"])
            {
                albumsId.Add(i["id"].ToString());
                albumsName.Add(i["title"].ToString());

                try
                {
                    albumsCoverUri.Add(i["coverUri"].ToString());
                }
                catch (NullReferenceException)
                {
                    albumsCoverUri.Add("None");
                }
            }

            albums.Add("type", type);
            albums.Add("id", albumsId);
            albums.Add("name", albumsName);
            albums.Add("coverUri", albumsCoverUri);

            return albums;
        }

        public static Dictionary<string, string> GetBest(JToken root)
        {
            Dictionary<string, string> best = new Dictionary<string, string>();

            best.Add("type", root["best"]["type"].ToString());

            return best;
        }
        public static HBox CreateBoxResultSearch(List<string> name, List<string> coverUri, List<string> id, string typeResult)
        {
            HBox newBox = new HBox();
            newBox.Valign = Align.Start;

            int b = -1;
            foreach (string i in name)
            {
                b++;
                VBox coverImage = new VBox();
                Button buttonPlay = new Button();
                coverImage.Spacing = 4;
                coverImage.MarginTop = 20;
                coverImage.MarginBottom = 15;
                coverImage.Valign = Align.Start;
                coverImage.Halign = Align.Start;
                        
                newBox.Add(coverImage);
                newBox.Spacing = 8;
                        
                Label nameBestLabel = new Label(i);
                FontDescription tpfNameBest = new FontDescription();
                tpfNameBest.Size = 11264;
                nameBestLabel.ModifyFont(tpfNameBest);
                nameBestLabel.MaxWidthChars = 20;
                nameBestLabel.Ellipsize = EllipsizeMode.End;
                nameBestLabel.Halign = Align.Center;
                nameBestLabel.Valign = Align.Start;

                string uri;
                Pixbuf imagePixbuf;
                if (coverUri[b] != "None")
                {
                    uri = "https://" + coverUri[b].Replace("%%", "50x50");
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://" + coverUri[b].Replace("%%", "100x100"));
                    HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                    using (Stream stream = response.GetResponseStream())
                    {
                        imagePixbuf = new Pixbuf(stream);
                    }
                    response.Close();
                }
                else
                {
                    uri = "null";
                    imagePixbuf = new Pixbuf(Path.GetFullPath("Svg/icons8_rock_music_100_negate.png"));
                }
                
                if (typeResult != "playlist")
                {
                    buttonPlay.Name = "{'type': \"" + typeResult + "\",'id': \"" + id[b] + "\", 'uri': \""+ uri + "\" }";   
                }
                else
                {
                    buttonPlay.Name = "{ 'type': \"" + typeResult + "\", 'uid': \"" + Search.uidPlaylist[b] + "\", 'kind': \"" + Search.kindPlaylist[b] + "\", 'uri': \"" + uri + "\"}";
                }
                Console.WriteLine(buttonPlay.Name);
                
                Image image = new Image(imagePixbuf);
                image.Halign = Align.Fill;
                buttonPlay.Image = image;
                buttonPlay.Relief = ReliefStyle.None;
                
                ListButtonPlay.Add(buttonPlay);
                coverImage.Add(buttonPlay);
                coverImage.Add(nameBestLabel);
                newBox.Add(coverImage);
            }
                    
            return newBox;
        }
    }
}