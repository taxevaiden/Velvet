using System.Drawing;
using System.Numerics;
using Veldrid;

namespace Velvet.Graphics
{
    public enum RendererAPI
    {
        D3D11,
        Vulkan,
        Metal,
        OpenGL
    }

    struct Vertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public uint Color;
        public Vertex(Vector2 position, Vector2 uv, uint color)
        {
            Position = position;
            UV = uv;
            Color = color;
        }
        public const uint SizeInBytes = 20;
    }

    struct Batch
    {
        public Vertex[] Vertices;
        public uint[] Indices;
        public VelvetTexture Texture;
        public VelvetShader Shader;
        public Batch(Vertex[] vertices, uint[] indices, VelvetTexture texture, VelvetShader shader)
        {
            Vertices = vertices;
            Indices = indices;
            Texture = texture;
            Shader = shader;
        }
    }

    public partial class Renderer
    {
        /// <summary>
        /// Packs a System.Drawing.Color into a single uint.
        /// </summary>
        /// <param name="color"></param>
        /// <returns>The color packed into a uint</returns>
        private static uint PackColor(Color color)
        {
            return (uint)(color.R | (color.G << 8) | (color.B << 16) | (color.A << 24));
        }

        /// <summary>
        /// Converts a System.Drawing.Color to an RgbaFloat.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>The converted RgbaFloat.</returns>
        private static RgbaFloat ToRgbaFloat(Color c)
        {
            return new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }


    }
}