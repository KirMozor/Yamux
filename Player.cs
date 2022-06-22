using ManagedBass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Gdk;
using Gtk;
using Newtonsoft.Json.Linq;
using YandexMusicApi;

namespace Yamux
{
    public class Player
    {
        public static bool PlayTrackOrNo;
        public static bool PlayTrackOrPause;
        private static int stream;
        public static int currentTrack = -1;
        private static string nextOrLastTrack;
        public static List<string> trackIds;
        public static void PlayUrlFile(string url)
        {
            if (!PlayTrackOrNo)
            {
                Bass.Init();
            }
            else
            {
                Bass.StreamFree(stream);
                Bass.Free();
                Bass.Init();
            }
            stream = Bass.CreateStream(url, 0, BassFlags.StreamDownloadBlocks, null, IntPtr.Zero);
            if (stream != 0)
            {
                Bass.ChannelPlay(stream);
                PlayTrackOrNo = true;
                PlayTrackOrPause = true;
            }
            else Console.WriteLine("Error: {0}!", Bass.LastError);
        }

        public static void NextTrack(object sender, EventArgs a)
        {
            nextOrLastTrack = "next";
            if (trackIds.Count - 1 > currentTrack)
            {
                currentTrack++;
            }
        }

        public static void LastTrack(object sender, EventArgs a)
        {
            nextOrLastTrack = "last";
            if (currentTrack > 0)
            {
                currentTrack--;
            }
        }
        public async static void PlayPlaylist()
        {
            while (true)
            {
                try
                {
                    if (nextOrLastTrack != "last" && nextOrLastTrack != "next")
                    {
                        currentTrack++;
                    }

                    if (!PlayTrackOrNo)
                    {
                        Bass.Init();
                    }
                    else
                    {
                        Bass.StreamFree(stream);
                        Bass.Free();
                        Bass.Init();
                    }

                    string directLinkToTrack = GetDirectLinkWithTrack(trackIds[currentTrack]);
                    stream = Bass.CreateStream(directLinkToTrack, 0, BassFlags.StreamDownloadBlocks, null, IntPtr.Zero);
                    if (stream != 0)
                    {
                        Bass.ChannelPlay(stream);
                        PlayTrackOrNo = true;
                        PlayTrackOrPause = true;
                    }
                    else Console.WriteLine("Error: {0}!", Bass.LastError);

                    await Task.Run(() =>
                    {
                        while (true)
                        {
                            int saveCurrentTrack = currentTrack;
                            Thread.Sleep(1000);
                            long length = Bass.ChannelGetLength(stream);
                            long position = Bass.ChannelGetPosition(stream);

                            if (length == position)
                            {
                                Bass.Free();
                                Bass.Init();
                                break;
                            }

                            if (currentTrack > saveCurrentTrack || currentTrack < saveCurrentTrack)
                            {
                                Bass.Free();
                                Bass.Init();
                            }
                        }
                    });
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("End playlist");
                    break;
                }
            }
        }

        public static void StopTrack(object sender, EventArgs a)
        {
            Bass.StreamFree(stream);
            Bass.Free();
            PlayTrackOrNo = false;
        }

        public static void PauseOrStartPlay()
        {
            if (PlayTrackOrPause)
            {
                Bass.ChannelPause(stream);
                PlayTrackOrNo = false;
                PlayTrackOrPause = false;
            }
            else
            {
                Bass.PauseNoPlay = 1;
                Bass.ChannelPlay(stream);
                PlayTrackOrNo = true;
                PlayTrackOrPause = true;
            }
        }

        public static PlaybackState GetStatusPlayback()
        {
            return Bass.ChannelIsActive(stream);
        }

        public static int GetLength()
        {
            if (stream == 0) { return stream; }
            return Convert.ToInt32(Bass.ChannelBytes2Seconds(stream, Bass.ChannelGetLength(stream)));
        }

        public static int GetPosition()
        {
            if (stream == 0) { return stream; }
            return Convert.ToInt32(Bass.ChannelBytes2Seconds(stream, Bass.ChannelGetPosition(stream)));
        }

        public static string GetDirectLinkWithTrack(string trackId)
        {
            JObject result = Track.GetDownloadInfoWithToken(trackId);
            Console.WriteLine(result);
            
            var resultData = result.GetValue("result");
            resultData = resultData[0];

            string url = resultData.ToList()[3].ToList()[0].ToString();
            return Track.GetDirectLink(url);
        }
        
        public static async Task DownloadUriWithThrottling(Uri uri, string path, double speedKbps = -0.0)
        {
            var req = WebRequest.CreateHttp(uri);
            using (var resp = await req.GetResponseAsync())
            using (var stream = resp.GetResponseStream())
            using (var outfile = File.OpenWrite(path))
            {
                var startTime = DateTime.Now;
                long totalDownloaded = 0;
                var buffer = new byte[0x10000];
                while (true)
                {
                    var actuallyRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (actuallyRead == 0)
                        return;
                    await outfile.WriteAsync(buffer, 0, actuallyRead);
                    totalDownloaded += actuallyRead;

                    if (speedKbps != -0.0)
                    {
                        var expectedTime = totalDownloaded / 1024.0 / speedKbps;
                        var actualTime = (DateTime.Now - startTime).TotalSeconds;
                        if (expectedTime > actualTime)
                            await Task.Delay(TimeSpan.FromSeconds(expectedTime - actualTime));   
                    }
                }
            }
        }
        public static void DownloadWithSync(Uri uri, string path, double speedKbps = -0.0)
        {
            var req = WebRequest.CreateHttp(uri);
            using (var resp = req.GetResponse())
            using (var stream = resp.GetResponseStream())
            using (var outfile = File.OpenWrite(path))
            {
                long totalDownloaded = 0;
                var buffer = new byte[0x10000];
                while (true)
                {
                    var actuallyRead = stream.Read(buffer, 0, buffer.Length);
                    if (actuallyRead == 0)
                        return;
                    outfile.Write(buffer, 0, actuallyRead);
                }
            }
        }
    }
}