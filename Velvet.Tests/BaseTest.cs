using System.Drawing;
using System.Numerics;
using Velvet.Graphics;
using Velvet.Input;

namespace Velvet.Tests
{
    class BaseTest
    {
        public BaseTest() {}
        public void Run()
        {
            var win = new VelvetWindow("Hello, world!", 1600, 900);

            float rot = 22.5f;

            while (win.IsRunning())
            {
                win.PollEvents();
            }

            win.Dispose();
        }
    }
}