using ManagedBass;
using System;

namespace Yamux
{
    public class Player
    {
        public static void PlayUrlFile()
        {
            // Init BASS using the default output device
            if (Bass.Init())
            {
                // Create a stream from a file
                var stream = Bass.CreateStream("/home/kirill/Downloads/2.mp3");

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
    }
}