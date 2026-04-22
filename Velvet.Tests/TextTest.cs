using System.Drawing;
using System.Numerics;

using Velvet;
using Velvet.Graphics;
using Velvet.Input;
using Velvet.Windowing;

namespace Velvet.Tests
{
    /// <summary>
    /// A text rendering test application for Velvet, demonstrating the ability to render text using a custom font. It loads a TTF font and renders a sample sentence at different sizes on the screen.
    /// </summary>
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

            // Draw text at different sizes
            int size = 48;
            Renderer.DrawText(font, "The quick brown fox jumps over the lazy dog.", size, new Vector2(150, 150), Color.Black); size -= 8;
            Renderer.DrawText(font, "The quick brown fox jumps over the lazy dog.", size, new Vector2(150, 200), Color.Black); size -= 8;
            Renderer.DrawText(font, "The quick brown fox jumps over the lazy dog.", size, new Vector2(150, 250), Color.Black); size -= 8;
            Renderer.DrawText(font, "The quick brown fox jumps over the lazy dog.", size, new Vector2(150, 300), Color.Black); size -= 8;
            Renderer.DrawText(font, "The quick brown fox jumps over the lazy dog.", size, new Vector2(150, 350), Color.Black); size -= 8;

            Renderer.End();
        }

        protected override void OnShutdown()
        {
            font.Dispose();
        }
    }
}