using System;
using System.Numerics;

using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;

namespace Velvet.Graphics
{
    /// <summary>
    /// Represents all native platform state required to initialize a <see cref="VelvetRenderer"/>.
    ///
    /// The <c>Width</c> and <c>Height</c> fields **must** be populated.
    ///
    /// Native window handles appropriate for the target platform must also be populated.
    ///
    /// To use the OpenGL backend, the <c>VelvetOpenGLPlatform</c> field must be populated
    ///
    /// ### Platform-specific fields
    ///
    /// Windows
    /// - <c>Hwnd</c>
    /// - <c>HInstance</c>
    ///
    /// Linux (Wayland)
    /// - <c>WaylandDisplay</c>
    /// - <c>WaylandSurface</c>
    ///
    /// Linux (X11)
    /// - <c>X11Display</c>
    /// - <c>X11Window</c>
    ///
    /// macOS
    /// - <c>CocoaWindow</c>
    /// </summary>
    public struct VelvetRendererEnvironment
    {
        /// <summary>
        /// The native Win32 window handle (HWMD) for the window.
        /// </summary>
        public nint Hwnd;
        /// <summary>
        /// The native Win32 application instance handle (HINSTANCE) associated with the window.
        /// </summary>
        public nint HInstance;
        /// <summary>
        /// The native Wayland display (<c>wl_display</c>) associated with the window.
        /// </summary>
        public nint WaylandDisplay;
        /// <summary>
        /// The native Wayland surface (<c>wl_surface</c>) associated with the window.
        /// </summary>
        public nint WaylandSurface;
        /// <summary>
        /// The native X11 display (<c>Display*</c>) associated with the window.
        /// </summary>
        public nint X11Display;
        /// <summary>
        /// The native X11 window identifier (<c>Window</c>) for the window.
        /// </summary>
        public nint X11Window;
        /// <summary>
        /// The native Cocoa <c>NSWindow</c> associated with the window.
        /// </summary>
        public nint CocoaWindow;

        /// <summary>
        /// The width of the window.
        /// </summary>
        public int WindowWidth;
        /// <summary>
        /// The height of the window.
        /// </summary>
        public int WindowHeight;

        /// <summary>
        /// Various pieces of OpenGL context, required to initialize a <see cref="VelvetRenderer"/> with OpenGL.
        /// </summary>
        public VelvetOpenGLPlatform OpenGLPlatform;

        /// <summary>
        /// Initializes a new instance of the <see cref="VelvetRendererEnvironment"/> struct
        /// with platform-specific native window handles and rendering configuration.
        /// </summary>
        /// <param name="windowWidth">The width of the window in pixels.</param>
        /// <param name="windowHeight">The height of the window in pixels.</param>
        /// <param name="openGLPlatform">
        /// Platform-specific OpenGL context and function loading configuration required
        /// when using the OpenGL backend.
        /// </param>
        /// <param name="hwnd">The native Win32 window handle (HWND). Defaults to <c>default</c> if not used.</param>
        /// <param name="hInstance">The native Win32 application instance handle (HINSTANCE). Defaults to <c>default</c> if not used.</param>
        /// <param name="waylandDisplay">The Wayland display (<c>wl_display</c>) handle. Defaults to <c>default</c> if not used.</param>
        /// <param name="waylandSurface">The Wayland surface (<c>wl_surface</c>) handle. Defaults to <c>default</c> if not used.</param>
        /// <param name="x11Display">The X11 display (<c>Display*</c>) handle. Defaults to <c>default</c> if not used.</param>
        /// <param name="x11Window">The X11 window identifier (<c>Window</c>). Defaults to <c>default</c> if not used.</param>
        /// <param name="cocoaWindow">The Cocoa <c>NSWindow</c> handle. Defaults to <c>default</c> if not used.</param>
        public VelvetRendererEnvironment(
            int windowWidth,
            int windowHeight,
            VelvetOpenGLPlatform openGLPlatform,
            nint hwnd = default,
            nint hInstance = default,
            nint waylandDisplay = default,
            nint waylandSurface = default,
            nint x11Display = default,
            nint x11Window = default,
            nint cocoaWindow = default)
        {
            Hwnd = hwnd;
            HInstance = hInstance;

            WaylandDisplay = waylandDisplay;
            WaylandSurface = waylandSurface;
            X11Display = x11Display;
            X11Window = x11Window;

            CocoaWindow = cocoaWindow;

            WindowWidth = windowWidth;
            WindowHeight = windowHeight;

            OpenGLPlatform = openGLPlatform;
        }
    }

    /// <summary>
    /// Represents platform-specific OpenGL function bindings and context handlers. Required to initialize a <see cref="VelvetRenderer"/> with OpenGL.
    /// </summary>
    public struct VelvetOpenGLPlatform
    {
        /// <summary>The native OpenGL context handle.</summary>
        public nint Context;

        /// <summary>
        /// Retrieves an OpenGL function pointer for the specified function name.
        /// </summary>
        public Func<string, nint> GetProcAddress;

        /// <summary>Makes the specified OpenGL context current on the calling thread.</summary>
        public Action<nint> MakeCurrent;

        /// <summary>Gets the currently active OpenGL context for the calling thread.</summary>
        public Func<nint> GetCurrentContext;

        /// <summary>Clears the current OpenGL context from the calling thread.</summary>
        public Action ClearCurrentContext;

        /// <summary>Destroys the specified OpenGL context and releases associated resources.</summary>
        public Action<nint> DestroyContext;

        /// <summary>Swaps the front and back buffers, presenting the rendered frame to the screen.</summary>
        public Action SwapBuffers;

        /// <summary>Enables or disables vertical synchronization (VSync).</summary>
        public Action<bool> SetVSync;

        /// <summary>
        /// Initializes a new instance of the <see cref="VelvetOpenGLPlatform"/> struct
        /// with all required platform-specific OpenGL function bindings and context handlers.
        /// </summary>
        /// <param name="glContext">The native OpenGL context handle.</param>
        /// <param name="glGetProcAddress">Function used to retrieve OpenGL procedure addresses.</param>
        /// <param name="glMakeCurrent">Makes the specified OpenGL context current on the calling thread.</param>
        /// <param name="glGetCurrentContext">Retrieves the currently active OpenGL context.</param>
        /// <param name="glClearCurrentContext">Clears the current OpenGL context from the thread.</param>
        /// <param name="glDestroyContext">Destroys the specified OpenGL context.</param>
        /// <param name="glSwapBuffers">Swaps the front and back buffers to present the frame.</param>
        /// <param name="glSetVSync">Enables or disables vertical synchronization.</param>
        public VelvetOpenGLPlatform(
            nint glContext,
            Func<string, nint> glGetProcAddress,
            Action<nint> glMakeCurrent,
            Func<nint> glGetCurrentContext,
            Action glClearCurrentContext,
            Action<nint> glDestroyContext,
            Action glSwapBuffers,
            Action<bool> glSetVSync)
        {
            Context = glContext;
            GetProcAddress = glGetProcAddress;
            MakeCurrent = glMakeCurrent;
            GetCurrentContext = glGetCurrentContext;
            ClearCurrentContext = glClearCurrentContext;
            DestroyContext = glDestroyContext;
            SwapBuffers = glSwapBuffers;
            SetVSync = glSetVSync;
        }
    }

    /// <summary>
    /// The GraphicsAPI to use for rendering.
    /// <para>
    /// The default GraphicsAPI for each operating system is:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Windows: D3D11</description></item>
    /// <item><description>macOS: Metal</description></item>
    /// <item><description>Linux: Vulkan</description></item>
    /// </list>
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

    /// <summary>
    /// Represents a batch to be submitted for rendering. This class stores its own vertex and index data.
    /// </summary>
    sealed class Batch
    {
        public Vertex[] Vertices;
        public uint[] Indices;
        public int VertexCount;
        public int IndexCount;
        public VelvetTexture Texture;
        public VelvetRenderTexture? RenderTarget;
        public VelvetShader Shader;

        public Batch(VelvetTexture texture, VelvetShader shader, VelvetRenderTexture? renderTarget = null, int vertexCapacity = 256, int indexCapacity = 384)
        {
            Texture = texture;
            Shader = shader;
            RenderTarget = renderTarget;
            VertexCount = 0;
            IndexCount = 0;
            Vertices = new Vertex[vertexCapacity];
            Indices = new uint[indexCapacity];
        }

        public void EnsureCapacity(int verticesNeeded, int indicesNeeded)
        {
            if (verticesNeeded > 0)
            {
                int requiredVertexCapacity = VertexCount + verticesNeeded;
                if (requiredVertexCapacity > Vertices.Length)
                {
                    int newVertexCapacity = Math.Max(Vertices.Length * 2, Math.Max(requiredVertexCapacity, 16));
                    Array.Resize(ref Vertices, newVertexCapacity);
                }
            }

            if (indicesNeeded > 0)
            {
                int requiredIndexCapacity = IndexCount + indicesNeeded;
                if (requiredIndexCapacity > Indices.Length)
                {
                    int newIndexCapacity = Math.Max(Indices.Length * 2, Math.Max(requiredIndexCapacity, 16));
                    Array.Resize(ref Indices, newIndexCapacity);
                }
            }
        }
    }

    /// <summary>
    /// Represents a color with 4 bytes (R, G, B, A). Used for vertex colors in the <see cref="VelvetRenderer"/>. 
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

    /// <summary>
    /// Represents a vertex with a position, UV coordinates, and a color.
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
}