using Serilog;

using Veldrid;
using Veldrid.OpenGL;

using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;

namespace Velvet.Graphics
{
    public partial class VelvetRenderer : IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<VelvetRenderer>();
        internal GraphicsDevice _graphicsDevice = null!;

        private CommandList _commandList = null!;
        private DeviceBuffer _vertexBuffer = null!;
        private DeviceBuffer _indexBuffer = null!;

        private const uint VertexBufferSize = 1024 * 1024 * 4; // 4 MB
        private const uint IndexBufferSize = 1024 * 1024 * 6; // 6 MB

        private int _vertexCount = 0;
        private int _indexCount = 0;
        private uint _vertexCapacity = VertexBufferSize / Vertex.SizeInBytes;
        private uint _indexCapacity = IndexBufferSize / sizeof(uint);

        /// <summary>Initializes Veldrid for Windows.</summary>
        private void InitVeldrid_WIN(GraphicsAPI api, VelvetRendererEnvironment environment, bool vsync)
        {
            LogInit("Windows", api, vsync);

            _graphicsDevice = api switch
            {
                GraphicsAPI.D3D11 => CreateD3D11Device(environment, vsync),
                GraphicsAPI.Vulkan => CreateVulkanDevice(environment, vsync, GetWin32Source(environment.Hwnd, environment.HInstance)),
                GraphicsAPI.OpenGL => CreateOpenGLDevice(environment, vsync),
                GraphicsAPI.Metal => throw PlatformUnsupported(api, "Windows", "D3D11, Vulkan, or OpenGL"),
                _ => throw new ArgumentOutOfRangeException(nameof(api))
            };

            CreateResources();
        }

        /// <summary>Initializes Veldrid for Linux.</summary>
        private void InitVeldrid_LINUX(GraphicsAPI api, VelvetRendererEnvironment environment, bool vsync)
        {
            LogInit("Linux", api, vsync);

            _graphicsDevice = api switch
            {
                GraphicsAPI.Vulkan => CreateVulkanDevice(environment, vsync, GetLinuxSource(environment.WaylandDisplay, environment.WaylandSurface, environment.X11Display, environment.X11Window)),
                GraphicsAPI.OpenGL => CreateOpenGLDevice(environment, vsync),
                GraphicsAPI.D3D11 => throw PlatformUnsupported(api, "Linux", "Vulkan or OpenGL"),
                GraphicsAPI.Metal => throw PlatformUnsupported(api, "Linux", "Vulkan or OpenGL"),
                _ => throw new ArgumentOutOfRangeException(nameof(api))
            };

            CreateResources();
        }

        /// <summary>Initializes Veldrid for macOS.</summary>
        private void InitVeldrid_OSX(GraphicsAPI api, VelvetRendererEnvironment environment, bool vsync)
        {
            LogInit("macOS", api, vsync);

            _graphicsDevice = api switch
            {
                GraphicsAPI.Metal => CreateMetalDevice(environment, vsync),
                GraphicsAPI.Vulkan => CreateVulkanDevice(environment, vsync, GetCocoaSource(environment.CocoaWindow)),
                GraphicsAPI.D3D11 => throw PlatformUnsupported(api, "macOS", "Metal or Vulkan"),
                GraphicsAPI.OpenGL => throw PlatformUnsupported(api, "macOS", "Metal or Vulkan"),
                _ => throw new ArgumentOutOfRangeException(nameof(api))
            };

            CreateResources();
        }

        // GraphicsDevice factories

        private GraphicsDevice CreateD3D11Device(VelvetRendererEnvironment environment, bool vsync)
        {
            var options = MakeOptions(vsync, debug: true);
            var scDesc = MakeSwapchain(environment.WindowWidth, environment.WindowHeight, vsync, GetWin32Source(environment.Hwnd, environment.HInstance));
            return GraphicsDevice.CreateD3D11(options, scDesc);
        }

        private GraphicsDevice CreateVulkanDevice(VelvetRendererEnvironment environment, bool vsync, SwapchainSource source)
        {
            var options = MakeOptions(vsync);
            var scDesc = MakeSwapchain(environment.WindowWidth, environment.WindowHeight, vsync, source);
            return GraphicsDevice.CreateVulkan(options, scDesc);
        }

        private GraphicsDevice CreateMetalDevice(VelvetRendererEnvironment environment, bool vsync)
        {
            var options = MakeOptions(vsync);
            var scDesc = MakeSwapchain(environment.WindowWidth, environment.WindowHeight, vsync, GetCocoaSource(environment.CocoaWindow));
            return GraphicsDevice.CreateMetal(options, scDesc);
        }

        private GraphicsDevice CreateOpenGLDevice(VelvetRendererEnvironment environment, bool vsync)
        {
            var platformInfo = new OpenGLPlatformInfo(
                openGLContextHandle: environment.OpenGLPlatform.Context,
                getProcAddress: environment.OpenGLPlatform.GetProcAddress,
                makeCurrent: environment.OpenGLPlatform.MakeCurrent,
                getCurrentContext: environment.OpenGLPlatform.GetCurrentContext,
                clearCurrentContext: environment.OpenGLPlatform.ClearCurrentContext,
                deleteContext: environment.OpenGLPlatform.DestroyContext,
                swapBuffers: environment.OpenGLPlatform.SwapBuffers,
                setSyncToVerticalBlank: environment.OpenGLPlatform.SetVSync
            );

            return GraphicsDevice.CreateOpenGL(MakeOptions(vsync), platformInfo, (uint)environment.WindowWidth, (uint)environment.WindowHeight);
        }

        // Swapchain source helpers

        private SwapchainSource GetWin32Source(nint hwnd, nint hinstance) => SwapchainSource.CreateWin32(hwnd, hinstance);

        // TODO: Maybe make it so You don't have to pass in like four pointers cause that just feels wasteful 
        // if someone's on wayland and they have to get x11 pointers that dont exist anyway? 
        // I don't know tho maybe this is fine
        private SwapchainSource GetLinuxSource(nint wlDisplay, nint wlSurface, nint x11Display, nint x11Window)
        {
            if (wlDisplay != IntPtr.Zero && wlSurface != IntPtr.Zero)
            {
                _logger.Information("Display protocol: Wayland");
                return SwapchainSource.CreateWayland(wlDisplay, wlSurface);
            }

            _logger.Information("Display protocol: X11");
            return SwapchainSource.CreateXlib(x11Display, x11Window);
        }

        // nsWindow is the WindowCocoaWindowPointer
        private SwapchainSource GetCocoaSource(nint nsWindow) => SwapchainSource.CreateNSWindow(nsWindow);

        // Shared option / descriptor builders

        private static GraphicsDeviceOptions MakeOptions(bool vsync, bool debug = false) =>
            new(
                debug: debug,
                swapchainDepthFormat: null,
                syncToVerticalBlank: vsync,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferStandardClipSpaceYDirection: true,
                preferDepthRangeZeroToOne: true);

        private static SwapchainDescription MakeSwapchain(
            int width, int height, bool vsync, SwapchainSource source) =>
            new(
                source,
                (uint)width,
                (uint)height,
                PixelFormat.R32Float,
                vsync);

        // Resource creation

        private void CreateResources()
        {
            _logger.Information("Creating resources...");

            _vertexCount = 0;
            _indexCount = 0;
            _batches = new List<Batch>();

            var factory = _graphicsDevice.ResourceFactory;

            _logger.Information("Creating buffers...");

            _vertexBuffer = factory.CreateBuffer(
                new BufferDescription(VertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            _indexBuffer = factory.CreateBuffer(
                new BufferDescription(IndexBufferSize, BufferUsage.IndexBuffer | BufferUsage.Dynamic));

            _logger.Information(
                "Vertex buffer: {VBSize} MB, Index buffer: {IBSize} MB",
                VertexBufferSize / (1024 * 1024),
                IndexBufferSize / (1024 * 1024));

            _commandList = factory.CreateCommandList();

            _logger.Information("Creating default texture and shader...");

            DefaultTexture = new VelvetTexture(this, [255, 255, 255, 255], 1, 1);
            CurrentTexture = DefaultTexture;

            DefaultShader = new VelvetShader(this, null, null);
            DefaultShader.SetTexture(DefaultTexture);
            CurrentShader = DefaultShader;

            _logger.Information("Resources ready.");
        }

        // Logging / error helpers

        private void LogInit(string platform, GraphicsAPI api, bool vsync)
        {
            _logger.Information(
                "Initializing Veldrid: {Platform}, {Api}, VSync: {VSync}",
                platform, api, vsync);
        }

        private static PlatformNotSupportedException PlatformUnsupported(
            GraphicsAPI api, string platform, string alternatives) =>
            new($"{api} is not supported on {platform}. Please use {alternatives}.");
    }
}