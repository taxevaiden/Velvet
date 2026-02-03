using System.Drawing;
using System.Numerics;

using Veldrid;

using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;

namespace Velvet.Graphics
{
    public enum GraphicsAPI
    {
        /// <summary>
        /// The default GraphicsAPI for your operating systen.
        /// </summary>
        Default,
        /// <summary>
        /// Direct3D 11 (Supports Windows only)
        /// </summary>
        D3D11,
        /// <summary>
        /// Vulkan (Supports all operating systems, however for macOS you need MoltenVK)
        /// </summary>
        Vulkan,
        /// <summary>
        /// Metal (Supports macOS only)
        /// </summary>
        Metal,
        /// <summary>
        /// OpenGL (Supports Windows and Linux)
        /// </summary>
        OpenGL
    }

    /// <summary>
    /// Where a shape (e.g. rectangles) will be rotated around.
    /// </summary>
    public enum AnchorPosition
    {
        /// <summary>
        /// Rotated around the top-left.
        /// </summary>
        TopLeft,
        /// <summary>
        /// Rotated around the top.
        /// </summary>
        Top,
        /// <summary>
        /// Rotated around the top right.
        /// </summary>
        TopRight,
        /// <summary>
        /// Rotated around the left.
        /// </summary>
        Left,
        /// <summary>
        /// Rotated around the center.
        /// </summary>
        Center,
        /// <summary>
        /// Rotated around the right.
        /// </summary>
        Right,
        /// <summary>
        /// Rotated around the bottom-left.
        /// </summary>
        BottomLeft,
        /// <summary>
        /// Rotated around the bottom.
        /// </summary>
        Bottom,
        /// <summary>
        /// Rotated around the botton-right.
        /// </summary>
        BottomRight
    }

    struct Vertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public RgbaFloat Color;
        public Vertex(Vector2 position, Vector2 uv, RgbaFloat color)
        {
            Position = position;
            UV = uv;
            Color = color;
        }
        public const uint SizeInBytes = 32;
    }

    struct Batch
    {
        public int VertexStart;
        public int VertexCount;
        public int IndexStart;
        public int IndexCount;
        public VelvetTexture Texture;
        public VelvetRenderTexture? RenderTarget;
        public VelvetShader Shader;

        public Batch(int vertexStart, int vertexCount, int indexStart, int indexCount, VelvetTexture texture, VelvetShader shader, VelvetRenderTexture? renderTarget = null)
        {
            VertexStart = vertexStart;
            VertexCount = vertexCount;
            IndexStart = indexStart;
            IndexCount = indexCount;
            Texture = texture;
            RenderTarget = renderTarget;
            Shader = shader;
        }
    }

    public partial class VelvetRenderer
    {
        /// <summary>
        /// Converts a System.Drawing.Color to an RgbaFloat.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>The converted RgbaFloat.</returns>
        private static RgbaFloat ToRgbaFloat(Color c)
        {
            return new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }

        private Vector2 GetRenderSize()
        {
            if (CurrentRenderTarget != null)
                return new Vector2(CurrentRenderTarget.Width, CurrentRenderTarget.Height);

            return new Vector2(_window.Width, _window.Height);
        }

        private static Rectangle Rect(Vector2 pos, Vector2 size)
        {
            return new Rectangle(
                (int)pos.X,
                (int)pos.Y,
                (int)size.X,
                (int)size.Y);
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
        { _graphicsDevice.ResizeMainWindow((uint)width, (uint)height); }
    }
}