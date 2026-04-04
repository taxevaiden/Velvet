using SDL3;

using Serilog;

using Veldrid;
using Veldrid.OpenGL;

using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;
using Velvet.Windowing;

namespace Velvet.Graphics
{
    public partial class VelvetRenderer : IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<VelvetRenderer>();

        public const float DEG2RAD = MathF.PI / 180.0f;

        internal VelvetWindow _window = null!;
        internal GraphicsDevice _graphicsDevice = null!;

        private CommandList _commandList = null!;
        private DeviceBuffer _vertexBuffer = null!;
        private DeviceBuffer _indexBuffer = null!;

        private const uint VertexBufferSize = 1024 * 1024 * 32; // 32 MB
        private const uint IndexBufferSize  = 1024 * 1024 * 48; // 48 MB

        private Vertex[] _vertices = null!;
        private uint[]   _indices  = null!;

        private int _vertexCount            = 0;
        private int _indexCount             = 0;
        private int _lastFlushedVertexCount = 0;
        private int _lastFlushedIndexCount  = 0;
        private int _vertexCapacity         = 0;
        private int _indexCapacity          = 0;

        /// <summary>Initializes Veldrid for Windows.</summary>
        private void InitVeldrid_WIN(GraphicsAPI api, VelvetWindow window, bool vsync)
        {
            LogInit(window, "Windows", api, vsync);
            _window = window;

            _graphicsDevice = api switch
            {
                GraphicsAPI.D3D11  => CreateD3D11Device(window, vsync),
                GraphicsAPI.Vulkan => CreateVulkanDevice(window, vsync, GetWin32Source(window)),
                GraphicsAPI.OpenGL => CreateOpenGLDevice(window, vsync),
                GraphicsAPI.Metal  => throw PlatformUnsupported(api, "Windows", "D3D11, Vulkan, or OpenGL"),
                _                  => throw new ArgumentOutOfRangeException(nameof(api))
            };

            CreateResources();
        }

        /// <summary>Initializes Veldrid for Linux.</summary>
        private void InitVeldrid_LINUX(GraphicsAPI api, VelvetWindow window, bool vsync)
        {
            LogInit(window, "Linux", api, vsync);
            _window = window;

            _graphicsDevice = api switch
            {
                GraphicsAPI.Vulkan => CreateVulkanDevice(window, vsync, GetLinuxSource(window)),
                GraphicsAPI.OpenGL => CreateOpenGLDevice(window, vsync),
                GraphicsAPI.D3D11  => throw PlatformUnsupported(api, "Linux",   "Vulkan or OpenGL"),
                GraphicsAPI.Metal  => throw PlatformUnsupported(api, "Linux",   "Vulkan or OpenGL"),
                _                  => throw new ArgumentOutOfRangeException(nameof(api))
            };

            CreateResources();
        }

        /// <summary>Initializes Veldrid for macOS.</summary>
        private void InitVeldrid_OSX(GraphicsAPI api, VelvetWindow window, bool vsync)
        {
            LogInit(window, "macOS", api, vsync);
            _window = window;

            _graphicsDevice = api switch
            {
                GraphicsAPI.Metal  => CreateMetalDevice(window, vsync),
                GraphicsAPI.Vulkan => CreateVulkanDevice(window, vsync, GetCocoaSource(window)),
                GraphicsAPI.D3D11  => throw PlatformUnsupported(api, "macOS", "Metal or Vulkan"),
                GraphicsAPI.OpenGL => throw PlatformUnsupported(api, "macOS", "Metal or Vulkan"),
                _                  => throw new ArgumentOutOfRangeException(nameof(api))
            };

            CreateResources();
        }

        // GraphicsDevice factories

        private GraphicsDevice CreateD3D11Device(VelvetWindow window, bool vsync)
        {
            var options = MakeOptions(vsync, debug: true);
            var scDesc  = MakeSwapchain(window, vsync, GetWin32Source(window));
            return GraphicsDevice.CreateD3D11(options, scDesc);
        }

        private GraphicsDevice CreateVulkanDevice(VelvetWindow window, bool vsync, SwapchainSource source)
        {
            var options = MakeOptions(vsync);
            var scDesc  = MakeSwapchain(window, vsync, source);
            return GraphicsDevice.CreateVulkan(options, scDesc);
        }

        private GraphicsDevice CreateMetalDevice(VelvetWindow window, bool vsync)
        {
            var options = MakeOptions(vsync);
            var scDesc  = MakeSwapchain(window, vsync, GetCocoaSource(window));
            return GraphicsDevice.CreateMetal(options, scDesc);
        }

        private GraphicsDevice CreateOpenGLDevice(VelvetWindow window, bool vsync)
        {
            IntPtr glContext = SDL.GLCreateContext(window.windowPtr);

            var platformInfo = new OpenGLPlatformInfo(
                openGLContextHandle:    glContext,
                getProcAddress:         SDL.GLGetProcAddress,
                makeCurrent:            ctx =>
                {
                    if (!SDL.GLMakeCurrent(window.windowPtr, ctx))
                        throw new InvalidOperationException($"Failed to make OpenGL context current: {SDL.GetError()}");
                },
                getCurrentContext:      SDL.GLGetCurrentContext,
                clearCurrentContext:    () =>
                {
                    if (!SDL.GLMakeCurrent(IntPtr.Zero, IntPtr.Zero))
                        throw new InvalidOperationException($"Failed to clear OpenGL context: {SDL.GetError()}");
                },
                deleteContext:          ctx =>
                {
                    // SDL destroys the context by handle, not window — use ctx directly.
                    if (!SDL.GLDestroyContext(ctx))
                        throw new InvalidOperationException($"Failed to destroy OpenGL context: {SDL.GetError()}");
                },
                swapBuffers:            () =>
                {
                    if (!SDL.GLSwapWindow(window.windowPtr))
                        throw new InvalidOperationException($"Failed to swap buffers: {SDL.GetError()}");
                },
                setSyncToVerticalBlank: enabled =>
                {
                    // BUG FIX: was capturing outer `vsync` instead of using the `enabled` parameter.
                    if (!SDL.GLSetSwapInterval(enabled ? 1 : 0))
                        throw new InvalidOperationException($"Failed to set swap interval: {SDL.GetError()}");
                }
            );

            return GraphicsDevice.CreateOpenGL(MakeOptions(vsync), platformInfo, (uint)window.Width, (uint)window.Height);
        }

        // Swapchain source helpers

        private SwapchainSource GetWin32Source(VelvetWindow window)
        {
            var props    = SDL.GetWindowProperties(window.windowPtr);
            var hinstance = SDL.GetPointerProperty(props, SDL.Props.WindowWin32InstancePointer, IntPtr.Zero);
            var hwnd      = SDL.GetPointerProperty(props, SDL.Props.WindowWin32HWNDPointer,     IntPtr.Zero);
            return SwapchainSource.CreateWin32(hwnd, hinstance);
        }

        private SwapchainSource GetLinuxSource(VelvetWindow window)
        {
            var props      = SDL.GetWindowProperties(window.windowPtr);
            var wlDisplay  = SDL.GetPointerProperty(props, SDL.Props.WindowWaylandDisplayPointer, IntPtr.Zero);
            var wlSurface  = SDL.GetPointerProperty(props, SDL.Props.WindowWaylandSurfacePointer, IntPtr.Zero);

            if (wlDisplay != IntPtr.Zero && wlSurface != IntPtr.Zero)
            {
                _logger.Information("(Window-{WindowId}): Display protocol: Wayland", _window.windowID);
                return SwapchainSource.CreateWayland(wlDisplay, wlSurface);
            }

            _logger.Information("(Window-{WindowId}): Display protocol: X11", _window.windowID);
            var x11Display = SDL.GetPointerProperty(props, SDL.Props.WindowX11DisplayPointer, IntPtr.Zero);
            var x11Window  = (uint)SDL.GetNumberProperty(props, SDL.Props.WindowX11WindowNumber, 0);
            return SwapchainSource.CreateXlib(x11Display, (IntPtr)x11Window);
        }

        private SwapchainSource GetCocoaSource(VelvetWindow window)
        {
            var nsWindow = SDL.GetPointerProperty(
                SDL.GetWindowProperties(window.windowPtr),
                SDL.Props.WindowCocoaWindowPointer,
                IntPtr.Zero);
            return SwapchainSource.CreateNSWindow(nsWindow);
        }

        // Shared option / descriptor builders

        private static GraphicsDeviceOptions MakeOptions(bool vsync, bool debug = false) =>
            new(
                debug:                          debug,
                swapchainDepthFormat:           null,
                syncToVerticalBlank:            vsync,
                resourceBindingModel:           ResourceBindingModel.Improved,
                preferStandardClipSpaceYDirection: true,
                preferDepthRangeZeroToOne:      true);

        private static SwapchainDescription MakeSwapchain(
            VelvetWindow window, bool vsync, SwapchainSource source) =>
            new(
                source,
                (uint)window.Width,
                (uint)window.Height,
                PixelFormat.R32Float,
                vsync);

        // Resource creation

        private void CreateResources()
        {
            _logger.Information("(Window-{WindowId}): Creating resources...", _window.windowID);

            _vertexCapacity = (int)(VertexBufferSize / Vertex.SizeInBytes);
            _indexCapacity  = (int)(IndexBufferSize  / sizeof(uint));
            _vertices       = new Vertex[_vertexCapacity];
            _indices        = new uint[_indexCapacity];
            _vertexCount    = 0;
            _indexCount     = 0;
            _batches        = new List<Batch>();

            var factory = _graphicsDevice.ResourceFactory;

            _logger.Information("(Window-{WindowId}): Creating buffers...", _window.windowID);

            _vertexBuffer = factory.CreateBuffer(
                new BufferDescription(VertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            _indexBuffer = factory.CreateBuffer(
                new BufferDescription(IndexBufferSize,  BufferUsage.IndexBuffer  | BufferUsage.Dynamic));

            _logger.Information(
                "(Window-{WindowId}): Vertex buffer: {VBSize} MB  |  Index buffer: {IBSize} MB",
                _window.windowID,
                VertexBufferSize  / (1024 * 1024),
                IndexBufferSize   / (1024 * 1024));

            _commandList = factory.CreateCommandList();

            _logger.Information("(Window-{WindowId}): Creating default texture and shader...", _window.windowID);

            DefaultTexture = new VelvetTexture(this, [255, 255, 255, 255, 255, 255, 255, 255], 2, 1);
            CurrentTexture = DefaultTexture;

            DefaultShader = new VelvetShader(this, null, null);
            DefaultShader.SetTexture(DefaultTexture);
            CurrentShader = DefaultShader;

            _logger.Information("(Window-{WindowId}): Resources ready.", _window.windowID);
        }

        // Logging / error helpers

        private void LogInit(VelvetWindow window, string platform, GraphicsAPI api, bool vsync)
        {
            _logger.Information(
                "(Window-{WindowId}): Initialising Veldrid  |  Platform: {Platform}  |  API: {Api}  |  VSync: {VSync}",
                window.windowID, platform, api, vsync);
        }

        private static PlatformNotSupportedException PlatformUnsupported(
            GraphicsAPI api, string platform, string alternatives) =>
            new($"{api} is not supported on {platform}. Please use {alternatives}.");
    }
}