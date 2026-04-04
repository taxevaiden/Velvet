using System.Drawing;
using System.Numerics;

using Velvet;
using Velvet.Graphics;
using Velvet.Input;
using Velvet.Windowing;

namespace Velvet.Tests
{
    class TextTest : VelvetApplication
    {
        VelvetFont font = null!;
        public TextTest(GraphicsAPI graphicsAPI, int width = 1600, int height = 900, string title = "Window")
            : base(width, height, title, graphicsAPI)
        { }

        protected override void OnInit()
        {
            base.OnInit();
            font = new VelvetFont(Renderer, "assets/sans.ttf", 48);
        }

        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(Color.White);

            Renderer.DrawText(font, "The quick brown fox jumps over the lazy dog.", 48, new Vector2(150, 150), Color.Black);
            Renderer.DrawText(font, "The quick brown fox jumps over the lazy dog.", 16, new Vector2(150, 300), Color.Black);
            Renderer.End();
        }

        protected override void OnShutdown()
        {
            font.Dispose();
        }
    }
}