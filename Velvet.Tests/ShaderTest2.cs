using System.Diagnostics;
using System.Drawing;
using System.Numerics;

using SixLabors.ImageSharp.Processing;

using Velvet.Graphics;
using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;
using Velvet.Input;
using Velvet.Windowing;

namespace Velvet.Tests
{
    class ShaderTest2 : VelvetApplication
    {
        VelvetTexture usagi;
        VelvetShader testShader;
        Stopwatch stopwatch;
        public ShaderTest2(GraphicsAPI graphicsAPI, int width = 1600, int height = 900, string title = "Hello, world!")
            : base(width, height, title, graphicsAPI)
        { }

        protected override void OnInit()
        {
            base.OnInit();

            stopwatch = new();

            usagi = new VelvetTexture(Renderer, "assets/usagi.jpg");
            testShader = new VelvetShader(Renderer, null, "assets/shaders/jpeg.frag", [new UniformDescription("Resolution", UniformType.Vector2, UniformStage.Fragment)]);
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