using ManagedBass;
using System;
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
        private static bool PlayTrackOrNo = false;
        private static int stream;
        public static void PlayUrlFile(string url)
        {
                if (PlayTrackOrNo == false)
                {
                    Bass.Init();
                    stream = Bass.CreateStream(url, 0, BassFlags.StreamDownloadBlocks, null, IntPtr.Zero);
                    if (stream != 0)
                    {
                        Bass.ChannelPlay(stream);
                        PlayTrackOrNo = true;
                    }
                    else Console.WriteLine("Error: {0}!", Bass.LastError);
                }
                else
                {
                    Bass.StreamFree(stream);
                    Bass.Free();
                    Bass.Init();
                    
                    stream = Bass.CreateStream(url, 0, BassFlags.StreamDownloadBlocks, null, IntPtr.Zero);
                    if (stream != 0)
                    {
                        Bass.ChannelPlay(stream);
                        PlayTrackOrNo = true;
                    }
                    else Console.WriteLine("Error: {0}!", Bass.LastError);
                }
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
        
        public static async Task DownloadUriWithThrottling(Uri uri, string path, double speedKbps)
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
                    if (actuallyRead == 0) // end of stream
                        return;
                    await outfile.WriteAsync(buffer, 0, actuallyRead);
                    totalDownloaded += actuallyRead;

                    // recalc speed and wait
                    var expectedTime = totalDownloaded / 1024.0 / speedKbps;
                    var actualTime = (DateTime.Now - startTime).TotalSeconds;
                    if (expectedTime > actualTime)
                        await Task.Delay(TimeSpan.FromSeconds(expectedTime - actualTime));
                }
            }
        }
    }
}