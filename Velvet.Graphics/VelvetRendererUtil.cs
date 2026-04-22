using System.Drawing;
using System.Numerics;

using Veldrid;

using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;

namespace Velvet.Graphics
{
    /// <summary>
    /// The GraphicsAPI to use for rendering. The default GraphicsAPI for each operating system is as follows:  
    /// 
    /// Windows: D3D11  
    /// 
    /// macOS: Metal  
    /// 
    /// Linux: Vulkan  
    /// 
    /// </summary>
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
        /// A struct representing a color with 4 bytes (R, G, B, A). This is used for vertex colors in the renderer. 
        /// The implicit conversion from System.Drawing.Color allows you to easily use System.Drawing.Color with the VelvetRenderer's drawing functions.
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct RgbaColor
        {
            /// <summary>The red component of the color, from 0 to 255.</summary>
            public byte R;
            /// <summary>The green component of the color, from 0 to 255.</summary>
            public byte G;
            /// <summary>The blue component of the color, from 0 to 255.</summary>
            public byte B;
            /// <summary>The alpha component of the color, from 0 to 255.</summary>
            public byte A;

            /// <summary>
            /// Initializes a new instance of the RgbaColor struct with the specified red, green, blue, and alpha components.
            /// </summary>
            /// <param name="r">The red component of the color, from 0 to 255.</param>
            /// <param name="g">The green component of the color, from 0 to 255.</param>
            /// <param name="b">The blue component of the color, from 0 to 255.</param>
            /// <param name="a">The alpha component of the color, from 0 to 255.</param>
            public RgbaColor(byte r, byte g, byte b, byte a) { R = r; G = g; B = b; A = a; }

            /// <summary>
            /// Defines an implicit conversion from System.Drawing.Color to RgbaColor, allowing you to use System.Drawing.Color with the VelvetRenderer's drawing functions without needing to manually convert them to RgbaColor.
            /// </summary>
            /// <param name="c">The System.Drawing.Color to convert.</param>
            /// <returns>The RgbaColor equivalent.</returns>
            public static implicit operator RgbaColor(System.Drawing.Color c) => new(c.R, c.G, c.B, c.A);
        }

        internal RgbaFloat ToRgbaFloat(RgbaColor c) => new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        
        /// <summary>
        /// A struct representing a vertex with a position, UV coordinates, and a color.
        /// </summary>
        public struct Vertex
        {
            /// <summary>
            /// The position of the vertex in 3D space.
            /// </summary>
            public Vector3 Position;
            /// <summary>
            /// The UV coordinates of the vertex, used for texturing.
            /// </summary>
            public Vector2 UV;
            /// <summary>
            /// The color of the vertex.
            /// </summary>
            public RgbaColor Color;

            /// <summary>
            /// Initializes a new instance of the Vertex struct with the specified position, UV coordinates, and color.
            /// </summary>
            /// <param name="position">The position of the vertex in 3D space.</param>
            /// <param name="uv">The UV coordinates of the vertex, used for texturing.</param>
            /// <param name="color">The color of the vertex.</param>
            public Vertex(Vector3 position, Vector2 uv, RgbaColor color)
            {
                Position = position;
                UV = uv;
                Color = color;
            }

            /// <summary>
            /// The size of the Vertex struct in bytes.
            /// </summary>
            public const uint SizeInBytes = 24;
        }

        private Vector2 GetRenderSize()
        {
            if (CurrentRenderTarget != null)
                return new Vector2(CurrentRenderTarget.Width, CurrentRenderTarget.Height);

            return new Vector2(_window.Width, _window.Height);
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