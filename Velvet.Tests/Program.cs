using System.Numerics;
using Veldrid;
using Velvet.Graphics;

namespace Velvet.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var win = new VelvetWindow("Hello, world!", 1280, 720);
            var renderer = new Renderer(win);

            float rot = 22.5f;

            while (win.IsRunning())
            {
                win.PollEvents();

                rot += 0.1f;

                renderer.Begin();
                renderer.ClearColor(RgbaFloat.White);

                renderer.DrawRectangle(new Vector2(50.0f, 50.0f), new Vector2(200.0f, 200.0f), rot * (MathF.PI / 180.0f), RgbaFloat.Black);
                renderer.DrawRectangle(new Vector2(50.0f, 300.0f), new Vector2(200.0f, 200.0f), 0.0f * (MathF.PI / 180.0f), RgbaFloat.Black);

                renderer.End();
            }

            renderer.Dispose();
            win.Dispose();
        }
    }
}
