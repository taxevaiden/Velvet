using System.Diagnostics;
using System.Drawing;
using System.Numerics;

using Boist.Graphics;
using Boist.Graphics.Shaders;
using Boist.Graphics.Textures;
namespace Boist.Tests
{
    /// <summary>
    /// A shader test application for Boist, demonstrating basic shader usage by applying a custom shader to a textured rectangle.
    /// More shaders can be found in the assets/shaders folder.
    /// </summary>
    class ShaderTest2 : Application
    {
        Texture usagi;
        Shader testShader;
        Stopwatch stopwatch;
        public ShaderTest2(GraphicsAPI graphicsAPI, int width = 1280, int height = 720, string title = "Hello, world!")
            : base(width, height, title, graphicsAPI)
        { }

        protected override void OnInit(int argc, string[]? argv)
        {
            stopwatch = new();

            usagi = new Texture(Renderer, "assets/image.png");
            testShader = new Shader(Renderer, null, "assets/shaders/jpeg.frag", [new UniformDescription("Resolution", UniformType.Vector2)]);
            testShader.Set("Resolution", new Vector2(usagi.Width, usagi.Height));
            testShader.Flush();

            stopwatch.Start();
        }


        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(Color.AntiqueWhite);

            Renderer.ApplyTexture(usagi);
            Renderer.ApplyShader(testShader);
            Renderer.DrawRectangle(new Vector2(50.0f, 50.0f), new Vector2(Window.Width - 100.0f, Window.Height - 100.0f), Color.White);

            Renderer.End();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            stopwatch.Stop();
            usagi.Dispose();
            testShader.Dispose();
        }
    }
}