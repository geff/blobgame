using System;

namespace BlobGame
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GameBlob game = new GameBlob())
            {
                game.Run();
            }
        }
    }
}

