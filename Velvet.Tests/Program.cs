using System.Drawing;
using System.Numerics;
using Velvet.Graphics;

namespace Velvet.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var win = new VelvetWindow("Hello, world!", 1280, 720);
            var renderer = new Renderer(RendererAPI.D3D11, win);

            float rot = 22.5f;

            while (win.IsRunning())
            {
                win.PollEvents();

                rot += 1.0f;

                renderer.Begin();
                renderer.ClearColor(Color.White);

                renderer.DrawRectangle(new Vector2(50.0f, 50.0f), new Vector2(200.0f, 200.0f), rot * (MathF.PI / 180.0f), Color.Red);
                renderer.DrawRectangle(new Vector2(50.0f, 300.0f), new Vector2(200.0f, 200.0f), Color.Black);
                renderer.DrawCircle(new Vector2(720.0f, 360.0f), 100.0f, Color.Teal);

                renderer.End();
            }

            renderer.Dispose();
            win.Dispose();
        }
    }
}
