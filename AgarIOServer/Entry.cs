using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer
{
    class Entry
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server started!");
            ConnectionManager manager = new ConnectionManager();
            Console.WriteLine("Starting listening loop");
            var task = manager.StartListeningAsync();
            

            Console.WriteLine("Starting game loop");
            GameServer game = new GameServer(manager);
            game.Start();


            Console.WriteLine("Waiting for Manager task to complete.");
            if (!task.IsCompleted)
                task.Wait();

            Console.WriteLine("\n...Press any key to end!");
            Console.ReadLine();
        }
    }
}
