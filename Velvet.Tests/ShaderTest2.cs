using System.Drawing;
using System.Numerics;
using Velvet.Graphics;
using Velvet.Windowing;
using Velvet.Input;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;

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
            testShader = new VelvetShader(Renderer, "assets/shaders/test.vert", null, [ new UniformDescription("hehe", UniformType.Float, UniformStage.Vertex) ]);
            testShader.Set("hehe", 0.0f);
            testShader.Flush();

            stopwatch.Start();
        }

        protected override void Update(float deltaTime)
        {
            testShader.Set("hehe", stopwatch.ElapsedMilliseconds / 100.0f);
            testShader.Flush();
        }


        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(Color.White);

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