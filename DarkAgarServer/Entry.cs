using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgarServer
{
    /// <summary>
    /// The class that contains the entry point of the application.
    /// </summary>
    class Entry
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            ConnectionManager manager = new ConnectionManager();
            Console.WriteLine("Starting listening loop");
            var task = manager.StartListeningAsync();

            Console.WriteLine("Starting game loop");
            GameServer game = new GameServer(manager);
            game.Start();
            Console.WriteLine("Server started!");

            Console.WriteLine("Waiting for the connection manager task to complete.");
            if (!task.IsCompleted)
                task.Wait();

            Console.WriteLine("\n...Press any key to end!");
            Console.ReadLine();
        }
    }
}
