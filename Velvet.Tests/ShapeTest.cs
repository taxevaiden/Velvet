using System.Drawing;
using System.Numerics;
using SDL3;
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

            float rot = 22.5f;
            Vector2 pos = Vector2.Zero;

            while (win.IsRunning())
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
                renderer.DrawRectangle(new Vector2(50.0f, 300.0f), new Vector2(200.0f, 200.0f), Color.Black);
                renderer.DrawCircle(new Vector2(720.0f, 450.0f), 200.0f, Color.Teal);
                renderer.DrawCircle(InputManager.GetMousePosition(), 10.0f, Color.Blue);
                renderer.DrawCircle(pos, 20.0f, Color.Blue);
                renderer.DrawPolygon(new Vector2(1000.0f, 450.0f), [new Vector2(25.0f, 0.0f), new Vector2(0.0f, 50.0f), new Vector2(50.0f, 50.0f), new Vector2(100.0f, -50.0f)], [0, 1, 2, 2, 3, 0], Color.Black);
                Console.WriteLine(1 / win.GetDeltaTime());

                renderer.End();
            }

            renderer.Dispose();
            win.Dispose();
        }
    }
}