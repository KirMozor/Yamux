using ManagedBass;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Yamux
{
    public class Player
    {
        public static void PlayUrlFile()
        {
            // Init BASS using the default output device
            if (Bass.Init())
            {
                string fileName = "https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3";
                // Create a stream from a file
                var stream = Bass.CreateStream(fileName, 0, BassFlags.StreamDownloadBlocks, null, IntPtr.Zero);

                if (stream != 0)
                    Bass.ChannelPlay(stream); // Play the stream

                // Error creating the stream
                else Console.WriteLine("Error: {0}!", Bass.LastError);

                // Wait till user presses a key
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();

                // Free the stream
                Bass.StreamFree(stream);

                // Free current device.
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