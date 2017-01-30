using System;

namespace GoSharp.PerfTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Select one of the options:");
            Console.WriteLine("    1. Examples");
            Console.WriteLine("    2. Threaded runner");
            Console.WriteLine("    3. Task-based runner");
            var c = Console.ReadKey();

            switch (c.Key)
            {
                case ConsoleKey.D1:
                    new Examples().Run();
                    break;
                case ConsoleKey.D2:
                    new ThreadedRunner().Run();
                    break;
                case ConsoleKey.D3:
                    new TaskRunner().Run();
                    break;
                default:
                    Console.WriteLine("Select 1, 2, or 3");
                    break;
            }
        }
    }
}
