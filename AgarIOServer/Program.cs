using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server started!");
            ConnectionManager manager = new ConnectionManager();
            Console.WriteLine("Starting listening loop");
            var task = manager.StartListeningAsync();
            

            Console.WriteLine("Starting server loop");

            Console.WriteLine("Waiting for Manager task to complete.");
            if (!task.IsCompleted)
                task.Wait();

            Console.WriteLine("\n...Press any key to end!");
            Console.ReadLine();
        }
    }
}
