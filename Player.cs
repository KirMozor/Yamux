using ManagedBass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YandexMusicApi;

namespace Yamux
{
    public class Player
    {
        public delegate void PlayerTrack();

        public static event PlayerTrack TrackStop;
        public static event PlayerTrack TrackLast;
        public static event PlayerTrack TrackNext;
        public static event PlayerTrack TrackPause;
        public static event PlayerTrack TrackPlay;
        
        public static event PlayerTrack ChangeCurrentTrack;

        private static bool boolNextTrack = false;
        private static bool boolLastTrack = false;
        private static bool boolStopTrack = false;
        private static bool StopAwait = false;
        private static bool ClickButtonOrNo = false;
        
        private static bool PlayTrackOrNo;
        public static bool PlayTrackOrPause;
        public static int stream;
        public static int currentTrack = -1;
        public static List<string> trackIds;
        
        public static void PlayUrlFile(string url)
        {
            Search.directLink = url;
            if (!PlayTrackOrNo) { Bass.Init(); }
            else
            {
                Bass.StreamFree(stream);
                Bass.Free();
                Bass.Init();
            }
            currentTrack = -1;
            
            stream = Bass.CreateStream(url, 0, BassFlags.StreamDownloadBlocks, null, IntPtr.Zero);
            if (stream != 0)
            {
                Bass.ChannelPlay(stream);
                PlayTrackOrNo = true;
                PlayTrackOrPause = true;
            }
            else Console.WriteLine("Error: {0}!", Bass.LastError);
        }
        public static void NextTrack(object sender, EventArgs a) { TrackNext.Invoke(); }
        public static void LastTrack(object sender, EventArgs a) { TrackLast.Invoke(); }
        public static async void PlayPlaylist()
        {
            currentTrack = -1;
            while (true)
            {
                try
                {
                    ClickButtonOrNo = false;
                    if (boolStopTrack)
                    {
                        Bass.StreamFree(stream);
                        Bass.Free();
                        Bass.Init();
                        boolStopTrack = false; 
                    }
                    if (PlayTrackOrNo == false) { Bass.Init(); }
                    else
                    {
                        Bass.StreamFree(stream);
                        Bass.Free();
                        Bass.Init();
                    }
                    currentTrack++;
                    StopAwait = false;
                    
                    string directLinkToTrack = GetDirectLinkWithTrack(trackIds[currentTrack]);
                    Console.WriteLine(directLinkToTrack);
                    Search.directLink = directLinkToTrack;
                    Bass.Init();
                    stream = Bass.CreateStream(directLinkToTrack, 0, BassFlags.StreamDownloadBlocks, null, IntPtr.Zero);
                    if (stream != 0)
                    {
                        PlayTrackOrNo = true;
                        Bass.ChannelPlay(stream);
                    }
                    else Console.WriteLine("Error: {0}!", Bass.LastError);
                    
                    await Task.Run(() =>
                    {
                        TrackNext += () => { StopAwait = true; };
                        TrackStop += () => { boolStopTrack = true; StopAwait = true; };
                        TrackLast += () =>
                        {
                            if (ClickButtonOrNo == false)
                            {
                                currentTrack -= 2; 
                                ClickButtonOrNo = true;
                                StopAwait = true;
                            }
                        };

                        while (true)
                        {
                            
                            Thread.Sleep(1000);//Просто сон async метода, чтобы пк не офигел от проверки
                            if (Bass.ChannelGetPosition(stream) != -1)
                            {
                                if (Bass.ChannelGetLength(stream) == Bass.ChannelGetPosition(stream) || StopAwait) //Если трек закончился, начать следующию песню
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
                catch (ArgumentOutOfRangeException)
                {
                    currentTrack = -1;
                    PlayTrackOrNo = false;
                    Console.WriteLine("End playlist");
                    break;
                }
            }
        }
        public static void StopTrack(object sender, EventArgs a)
        {
            Bass.StreamFree(stream);
            Bass.Free();
            try
            { TrackStop.Invoke(); }catch (NullReferenceException){}
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
        public static async Task DownloadUriWithThrottling(string url, string path, double speedKbps = -0.0)
        {
            Uri uri = new Uri(url);
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