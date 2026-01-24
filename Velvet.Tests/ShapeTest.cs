using System.Drawing;
using System.Numerics;
using Velvet.Graphics;
using Velvet.Windowing;
using Velvet.Input;

namespace Velvet.Tests
{
    class ShapeTest : VelvetApplication
    {
        float rot;
        Vector2 pos;
        VelvetTexture usagi;
        public ShapeTest(GraphicsAPI graphicsAPI, int width = 1600, int height = 900, string title = "Hello, world!")
            : base(width, height, title, graphicsAPI) { }

        protected override void OnInit()
        {
            base.OnInit();
            usagi = new VelvetTexture(Renderer, "assets/usagi.jpg");

            rot = 22.5f;
            pos = Vector2.Zero;
        }

        protected override void Update(float deltaTime)
        {
            if (InputManager.IsKeyDown(KeyCode.A)) { pos -= Vector2.UnitX * 5; }
            if (InputManager.IsKeyDown(KeyCode.D)) { pos += Vector2.UnitX * 5; }
            if (InputManager.IsKeyDown(KeyCode.W)) { pos -= Vector2.UnitY * 5; }
            if (InputManager.IsKeyDown(KeyCode.S)) { pos += Vector2.UnitY * 5; }
            if (InputManager.IsMouseButtonDown(MouseButton.Left)) { rot -= 5.0f; }
            if (InputManager.IsMouseButtonDown(MouseButton.Right)) { rot += 5.0f; }
        }

        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(Color.White);

            Renderer.DrawRectangle(new Vector2(50.0f, 50.0f), new Vector2(200.0f, 200.0f), rot * (MathF.PI / 180.0f), Color.Red);
            Renderer.DrawRectangle(new Vector2(50.0f, 350.0f), new Vector2(200.0f, 200.0f), Color.Black);
            Renderer.DrawCircle(new Vector2(800.0f, 450.0f), 200.0f, Color.Teal);

            Renderer.ApplyTexture(usagi);
            Renderer.DrawRectangle(new Vector2(50.0f, 650.0f), new Vector2(200.0f, 200.0f), Color.White);
            Renderer.DrawCircle(new Vector2(450.0f, 750.0f), 100.0f, 32, Color.Green);
            Renderer.DrawRectangle(new Vector2(350.0f, 50.0f), new Vector2(200.0f, 200.0f), -rot * (MathF.PI / 180.0f), Color.Red);

            Renderer.ApplyTexture();
            Renderer.DrawRectangle(new Vector2(350.0f, 350.0f), new Vector2(200.0f, 200.0f), Color.Lavender);
            Renderer.DrawCircle(InputManager.GetMousePosition(), 10.0f, Color.Blue);
            Renderer.DrawCircle(pos, 20.0f, Color.Blue);

            Renderer.End();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            usagi.Dispose();
        }

    }
}