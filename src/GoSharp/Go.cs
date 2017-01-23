using System;
using System.Threading.Tasks;

namespace GoSharp
{
    public static class Go
    {
        public static void Run(Action action)
        {
            Task.Run(action);
        }
    }
}
