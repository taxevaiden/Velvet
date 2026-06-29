using System.Drawing;
using System.Numerics;

using Veldrid;

namespace Velvet.Graphics
{
    public partial class VelvetRenderer
    {
        /// <summary>
        /// Helper to convert degrees to radians for rotation parameters. Usage example: `float rotation = 45f * VelvetRenderer.DEG2RAD;`
        /// </summary>
        public const float DEG2RAD = MathF.PI / 180.0f;

        internal RgbaFloat ToRgbaFloat(RgbaColor c) => new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

        private Vector2 GetRenderSize()
        {
            if (CurrentRenderTarget != null)
                return new Vector2(CurrentRenderTarget.Width, CurrentRenderTarget.Height);

            return new Vector2(_width, _height);
        }

        private Rectangle GetFullUV()
        {
            return new Rectangle(0, 0, (int)CurrentTexture.Width, (int)CurrentTexture.Height);
        }

        /// <summary>
        /// Resizes the VelvetRenderer.
        /// </summary>
        /// <param name="width">The width that the VelvetRenderer will be resized to.</param>
        /// <param name="height">The height that the VelvetRenderer will be resized to.</param>
        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;
            _graphicsDevice.ResizeMainWindow((uint)width, (uint)height);
        }
    }
}