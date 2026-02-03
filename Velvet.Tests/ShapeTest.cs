using System.Diagnostics;
using System.Drawing;
using System.Numerics;

using Velvet.Graphics;
using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;
using Velvet.Input;
using Velvet.Windowing;

namespace Velvet.Tests
{
    class ShapeTest : VelvetApplication
    {
        float rot;
        Vector2 pos;
        Vector2 vel;
        Vector2 pos2;
        VelvetTexture usagi;
        VelvetRenderTexture renderTexture;
        VelvetShader testShader;
        Stopwatch stopwatch;
        public ShapeTest(GraphicsAPI graphicsAPI, int width = 1600, int height = 900, string title = "Hello, world!")
            : base(width, height, title, graphicsAPI) { }

        protected override void OnInit()
        {
            base.OnInit();
            usagi = new VelvetTexture(Renderer, "assets/usagi.jpg");
            renderTexture = new(Renderer, 1600, 900);
            testShader = new VelvetShader(Renderer, null, "assets/shaders/jpeg.frag", [new UniformDescription("Resolution", UniformType.Vector2, UniformStage.Fragment)]);
            testShader.Set("Resolution", new Vector2(1600, 900));
            testShader.Flush();

            rot = 22.5f;
            pos = Vector2.Zero;
            vel = Vector2.Zero;
            pos2 = Vector2.UnitX * 1580.0f;

            stopwatch = new();
            stopwatch.Start();
        }

        protected override void Update(float deltaTime)
        {
            if (InputManager.IsKeyDown(KeyCode.A)) { pos -= Vector2.UnitX * 500f * deltaTime; }
            if (InputManager.IsKeyDown(KeyCode.D)) { pos += Vector2.UnitX * 500f * deltaTime; }
            if (InputManager.IsKeyDown(KeyCode.W)) { pos -= Vector2.UnitY * 500f * deltaTime; }
            if (InputManager.IsKeyDown(KeyCode.S)) { pos += Vector2.UnitY * 500f * deltaTime; }
            if (InputManager.IsMouseButtonDown(MouseButton.Left)) { rot -= 500f * deltaTime; }
            if (InputManager.IsMouseButtonDown(MouseButton.Middle)) { rot += 1000f * deltaTime; }
            if (InputManager.IsMouseButtonDown(MouseButton.Right)) { rot += 500f * deltaTime; }
            if (InputManager.IsMouseButtonDown(MouseButton.Side1)) { rot += 250f * deltaTime; }
            if (InputManager.IsMouseButtonDown(MouseButton.Side2)) { rot -= 250f * deltaTime; }

            InputManager.GetMouseScroll(out float x, out float y);
            vel += new Vector2(-x, -y);
            vel *= MathF.Max(0, 1 - deltaTime * 5);
            pos2 += vel;

            // testShader.Set("hehe", stopwatch.ElapsedMilliseconds / 100.0f);
            // testShader.Flush();
        }

        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.SetRenderTarget(renderTexture);
            Renderer.ClearColor(Color.White);

            Renderer.DrawRectangle(new Vector2(50.0f, 50.0f), new Vector2(200.0f, 200.0f), rot * (MathF.PI / 180.0f), AnchorPosition.Center, Color.Red);
            Renderer.DrawRectangle(new Vector2(50.0f, 350.0f), new Vector2(200.0f, 200.0f), Color.Black);
            Renderer.DrawCircle(new Vector2(800.0f, 450.0f), 200.0f, Color.Teal);

            Renderer.ApplyTexture(usagi);
            Renderer.DrawRectangle(new Vector2(50.0f, 650.0f), new Vector2(200.0f, 200.0f), Color.White);
            Renderer.DrawCircle(new Vector2(450.0f, 750.0f), 100.0f, 32, Color.Green);
            Renderer.DrawRectangle(new Vector2(350.0f, 50.0f), new Vector2(200.0f, 200.0f), -rot * (MathF.PI / 180.0f), AnchorPosition.Top, Color.Red);

            Renderer.ApplyTexture();
            Renderer.DrawRectangle(new Vector2(350.0f, 350.0f), new Vector2(200.0f, 200.0f), Color.Lavender);

            InputManager.GetMousePosition(out float x, out float y);
            Renderer.DrawCircle(new Vector2(x, y), 10.0f, Color.Blue);
            Renderer.DrawCircle(pos, 20.0f, Color.Blue);
            Renderer.DrawRectangle(pos2, new Vector2(20.0f, 20.0f), Color.Blue);

            Renderer.SetRenderTargetToScreen();
            Renderer.ApplyShader(testShader);
            Renderer.ClearColor(Color.White);

            Renderer.ApplyTexture(renderTexture.Texture);
            Renderer.DrawRectangle(new Vector2(0.0f, 0.0f), new Vector2(1600.0f, 900.0f), Color.White);

            Renderer.End();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            stopwatch.Stop();
            testShader.Dispose();
            renderTexture.Dispose();
            usagi.Dispose();
        }

    }
}