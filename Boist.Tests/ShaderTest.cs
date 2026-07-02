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
    class ShaderTest : Application
    {
        private Texture _usagi;
        private Shader _testShader;

        public ShaderTest(GraphicsAPI graphicsAPI, int width = 1280, int height = 720, string title = "Hello, world!")
            : base(width, height, title, graphicsAPI)
        { }

        protected override void OnInit(int argc, string[]? argv)
        {
            _usagi = new Texture(Renderer, "assets/image.png");
            _testShader = new Shader(Renderer, null, "assets/shaders/test.frag");
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
