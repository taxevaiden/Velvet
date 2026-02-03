using System.Drawing;
using System.Numerics;

using Velvet.Graphics;
using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;
using Velvet.Input;

namespace Velvet.Tests
{
    class ShaderTest : VelvetApplication
    {
        private VelvetTexture _usagi;
        private VelvetShader _testShader;

        public ShaderTest(GraphicsAPI graphicsAPI, int width = 1600, int height = 900, string title = "Hello, world!")
            : base(width, height, title, graphicsAPI)
        { }

        protected override void OnInit()
        {
            _usagi = new VelvetTexture(Renderer, "assets/usagi.jpg");
            _testShader = new VelvetShader(Renderer, null, "assets/shaders/test.frag");
        }


        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(Color.White);
            Renderer.ApplyTexture(_usagi);
            Renderer.DrawRectangle(
                new Vector2(50.0f, 50.0f),
                new Vector2(725.0f, 800.0f),
                Color.White
            );

            Renderer.ApplyShader(_testShader);
            Renderer.DrawRectangle(
                new Vector2(825.0f, 50.0f),
                new Vector2(725.0f, 800.0f),
                Color.White
            );

            Renderer.End();
        }

        protected override void OnShutdown()
        {
            _testShader.Dispose();
            _usagi.Dispose();
        }
    }
}
