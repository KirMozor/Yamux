using ManagedBass;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Yamux
{
    public class Player
    {
        public static void PlayUrlFile(string url)
        {
            if (Bass.Init())
            {
                var stream = Bass.CreateStream(url, 0, BassFlags.StreamDownloadBlocks, null, IntPtr.Zero);

                if (stream != 0)
                    Bass.ChannelPlay(stream);

                else Console.WriteLine("Error: {0}!", Bass.LastError);

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();

                Bass.StreamFree(stream);
                Bass.Free();
            }
            else Console.WriteLine("BASS could not be initialized!");
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