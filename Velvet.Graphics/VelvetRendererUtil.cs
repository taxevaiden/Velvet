using System.Drawing;
using System.Numerics;
using Veldrid;

namespace Velvet.Graphics
{
    public enum RendererAPI
    {
        D3D11,
        Vulkan,
        Metal
    }

    struct VertexPositionColor
    {
        public Vector2 Position;
        public Vector2 Anchor;
        public float Rotation;
        public uint Color;
        public VertexPositionColor(Vector2 position, Vector2 anchor, float rotation, uint color)
        {
            Position = position;
            Anchor = anchor;
            Rotation = rotation;
            Color = color;
        }
        public const uint SizeInBytes = 24;
    }

    struct ResolutionData
    {
        public uint w;
        public uint h;
        private uint p0;
        private uint p1;

        public ResolutionData(uint width, uint height)
        {
            w = width;
            h = height;
            p0 = 0;
            p1 = 0;
        }

        public const uint SizeInBytes = 16;
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