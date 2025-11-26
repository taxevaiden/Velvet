using System.Drawing;
using System.Numerics;
using Velvet.Graphics;
using Velvet.Input;

namespace Velvet.Tests
{
    class ShaderTest : BaseTest
    {
        public ShaderTest() {}
        public override void Run(RendererAPI rendererAPI)
        {
            var win = new VelvetWindow("Hello, world!", 1600, 900);
            var renderer = new Renderer(rendererAPI, win);

            VelvetTexture usagi = new VelvetTexture(renderer, "assets/usagi.jpg");
            VelvetShader testShader = new VelvetShader(renderer, null, "assets/shaders/test.frag");

            while (win.Running)
            {
                win.PollEvents();

                renderer.Begin();
                renderer.ClearColor(Color.White);

                renderer.ApplyTexture(usagi);
                renderer.DrawRectangle(new Vector2(50.0f, 50.0f), new Vector2(725.0f, 800.0f), Color.White);
                renderer.ApplyShader(testShader);
                renderer.DrawRectangle(new Vector2(825.0f, 50.0f), new Vector2(725.0f, 800.0f), Color.White);
            
                renderer.ApplyTexture();

                renderer.End();
            }

            renderer.Dispose();
            win.Dispose();
        }
    }
}