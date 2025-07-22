using System.Security.Principal;
using Velvet;
using Velvet.Graphics;

namespace Velvet.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var win = new VelvetWindow("Hello, world!", 1280, 720);
            var renderer = new Renderer(win);

            while (win.IsRunning())
            {
                win.PollEvents();
                renderer.Go();
            }

            renderer.Dispose();
            win.Dispose();
        }
    }
}
