using System.Drawing;
using System.Numerics;
using Velvet.Graphics;
using Velvet.Input;

namespace Velvet.Tests
{
    class ShapeTest : BaseTest
    {
        public ShapeTest() {}
        public override void Run(RendererAPI rendererAPI)
        {
            var win = new VelvetWindow("Hello, world!", 1600, 900);
            var renderer = new Renderer(rendererAPI, win);

            VelvetTexture usagi = new VelvetTexture(renderer, "assets/usagi.jpg");

            float rot = 22.5f;
            Vector2 pos = Vector2.Zero;

            while (win.Running)
            {
                win.PollEvents();

                if (InputManager.IsKeyDown(KeyCode.A)) { pos -= Vector2.UnitX * 5; }
                if (InputManager.IsKeyDown(KeyCode.D)) { pos += Vector2.UnitX * 5; }
                if (InputManager.IsKeyDown(KeyCode.W)) { pos -= Vector2.UnitY * 5; }
                if (InputManager.IsKeyDown(KeyCode.S)) { pos += Vector2.UnitY * 5; }
                if (InputManager.IsMouseButtonDown(MouseButton.Left)) { rot -= 5.0f; }
                if (InputManager.IsMouseButtonDown(MouseButton.Right)) { rot += 5.0f; }

                renderer.Begin();
                renderer.ClearColor(Color.White);

                renderer.DrawRectangle(new Vector2(50.0f, 50.0f), new Vector2(200.0f, 200.0f), rot * (MathF.PI / 180.0f), Color.Red);
                renderer.DrawRectangle(new Vector2(50.0f, 350.0f), new Vector2(200.0f, 200.0f), Color.Black);
                renderer.DrawCircle(new Vector2(800.0f, 450.0f), 200.0f, Color.Teal);

                renderer.ApplyTexture(usagi);
                renderer.DrawRectangle(new Vector2(50.0f, 650.0f), new Vector2(200.0f, 200.0f), Color.White);
                renderer.DrawCircle(new Vector2(450.0f, 750.0f), 100.0f, 32, Color.Green);
                renderer.DrawRectangle(new Vector2(350.0f, 50.0f), new Vector2(200.0f, 200.0f), -rot * (MathF.PI / 180.0f), Color.Red);
                renderer.ApplyTexture();

                renderer.DrawRectangle(new Vector2(350.0f, 350.0f), new Vector2(200.0f, 200.0f), Color.Lavender);
                renderer.DrawCircle(InputManager.GetMousePosition(), 10.0f, Color.Blue);
                renderer.DrawCircle(pos, 20.0f, Color.Blue);

                renderer.End();
            }

            renderer.Dispose();
            win.Dispose();
        }
    }
}