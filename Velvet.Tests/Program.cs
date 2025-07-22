using System.Security.Principal;
using Velvet;

namespace Velvet.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var win = new VelvetWindow("Hello, world!", 720, 720);

            while (win.IsRunning())
            {
                win.PollEvents();
            }

            win.Dispose();
        }
    }
}
