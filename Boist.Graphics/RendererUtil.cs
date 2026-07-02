using System.Drawing;
using System.Numerics;

using Veldrid;

using Boist.Graphics.Textures;

namespace Boist.Graphics
{
    public partial class Renderer
    {
        /// <summary>
        /// Helper to convert degrees to radians for rotation parameters. Usage example: `float rotation = 45f * BoistRenderer.DEG2RAD;`
        /// </summary>
        public const float DEG2RAD = MathF.PI / 180.0f;

        internal RgbaFloat ToRgbaFloat(RgbaColor c) => new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

        private Vector2 GetRenderSize()
        {
            if (CurrentRenderTarget != null)
                return new Vector2(CurrentRenderTarget.Width, CurrentRenderTarget.Height);

            return new Vector2(_width, _height);
        }

        private Rectangle GetFullUV() => GetTextureFullUV(CurrentTexture);

        private Rectangle GetTextureFullUV(Textures.Texture texture) => new Rectangle(0, 0, (int)texture.Width, (int)texture.Height);

        /// <summary>
        /// Resizes the <see cref="Renderer"/>.
        /// </summary>
        /// <param name="width">The width that the <see cref="Renderer"/> will be resized to.</param>
        /// <param name="height">The height that the <see cref="Renderer"/> will be resized to.</param>
        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;
            _graphicsDevice.ResizeMainWindow((uint)width, (uint)height);
        }

        /// <summary>
        /// Resizes the <see cref="Renderer"/>.
        /// </summary>
        /// <param name="size">The <see cref="Vector2"/> that the <see cref="Renderer"/> will be resized to.</param>
        public void Resize(Vector2 size)
        {
            _width = (int)size.X;
            _height = (int)size.Y;
            _graphicsDevice.ResizeMainWindow((uint)_width, (uint)_height);
        }
    }
}