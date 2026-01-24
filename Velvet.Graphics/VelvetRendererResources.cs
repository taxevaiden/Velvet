using System.Text;
using static System.Action;
using Veldrid;
using Veldrid.SPIRV;

using SDL3;
using Veldrid.Vk;
using Serilog;
using Veldrid.OpenGL;
using System.Runtime.InteropServices;

using Velvet.Windowing;
using Vulkan.Xlib;

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
        private uint _vertexBufferSize = 1024 * 1024 * 32; // 32 MB
        private uint _indexBufferSize = 1024 * 1024 * 48; // 48 MB
        private List<Vertex> _vertices = null!;
        private List<uint> _indices = null!;

        /// <summary>
        /// Initializes Veldrid, with the specifed RendererAPI and VelvetWindow on Windows.
        /// </summary>
        /// <param name="rendererAPI"></param>
        /// <param name="window"></param>
        /// <exception cref="PlatformNotSupportedException"></exception>
        private void InitVeldrid_WIN(GraphicsAPI rendererAPI, VelvetWindow window, bool vsync)
        {
            _logger.Information($"(Window-{window.windowID}): Initializing Veldrid...");
            _logger.Information($"(Window-{window.windowID}): > Platform: Windows");
            switch (rendererAPI)
            {
                case GraphicsAPI.D3D11:
                    {

                        _logger.Information($"(Window-{window.windowID}): > GraphicsAPI: D3D11");
                        _logger.Information($"(Window-{window.windowID}): > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"(Window-{window.windowID}): Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: true,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: vsync,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        IntPtr hwmd = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowWin32HWNDPointer, IntPtr.Zero);
                        _graphicsDevice = GraphicsDevice.CreateD3D11(options, hwmd, (uint)window.Width, (uint)window.Height);
                        break;
                    }


                case GraphicsAPI.Vulkan:
                    {
                        _logger.Information($"(Window-{window.windowID}): > GraphicsAPI: Vulkan");
                        _logger.Information($"(Window-{window.windowID}): > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"(Window-{window.windowID}): Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: vsync,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        var hinstance = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowWin32HWNDPointer, IntPtr.Zero);
                        var hwmd = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowWin32HWNDPointer, IntPtr.Zero);

                        VkSurfaceSource vkSurfaceSource = VkSurfaceSource.CreateWin32(hinstance, hwmd);

                        _graphicsDevice = GraphicsDevice.CreateVulkan(options, vkSurfaceSource, (uint)window.Width, (uint)window.Height);
                        _logger.Information($"(Window-{window.windowID}): Complete!");
                        break;
                    }

                case GraphicsAPI.Metal:
                    throw new PlatformNotSupportedException("Metal is not supported on Windows. Please use either D3D11 or Vulkan.");
                case GraphicsAPI.OpenGL:
                    {
                        _logger.Information($"(Window-{window.windowID}): > GraphicsAPI: OpenGL");
                        _logger.Information($"(Window-{window.windowID}): > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"(Window-{window.windowID}): Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: vsync,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        IntPtr glContextHandle = SDL.GLCreateContext(window.windowPtr);
                        Func<string, IntPtr> getProc = name => Marshal.GetFunctionPointerForDelegate<SDL.FunctionPointer>(SDL.GLGetProcAddress(name));
                        Action<IntPtr> makeCurrent = pointer => SDL.GLMakeCurrent(window.windowPtr, pointer); ;
                        Func<IntPtr> getCurrentContext = SDL.GLGetCurrentContext;
                        Action clearCurrentContext = () => SDL.GLMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                        Action<IntPtr> deleteContext = pointer => SDL.GLDestroyContext(window.windowPtr);
                        Action swapBuffers = () => SDL.GLSwapWindow(window.windowPtr); ;
                        Action<bool> setSyncToVerticalBlank = vSync => SDL.GLSetSwapInterval(vsync ? 1 : 0);

                        var glPlatformInfo = new OpenGLPlatformInfo(
                            glContextHandle,
                            getProc,
                            makeCurrent,
                            getCurrentContext,
                            clearCurrentContext,
                            deleteContext,
                            swapBuffers,
                            setSyncToVerticalBlank
                        );

                        _graphicsDevice = GraphicsDevice.CreateOpenGL(options, glPlatformInfo, (uint)window.Width, (uint)window.Height);
                        _logger.Information($"(Window-{window.windowID}): Complete!");
                        break;
                    }


            }

            CreateResources();
        }

        private void InitVeldrid_LINUX(GraphicsAPI rendererAPI, VelvetWindow window, bool vsync)
        {
            _logger.Information($"(Window-{window.windowID}): Initializing Veldrid...");
            _logger.Information($"(Window-{window.windowID}): > Platform: Linux");
            switch (rendererAPI)
            {
                case GraphicsAPI.D3D11:
                    throw new PlatformNotSupportedException("D3D11 is not supported on Linux. Please use Vulkan.");


                case GraphicsAPI.Vulkan:
                    {
                        _logger.Information($"(Window-{window.windowID}): > GraphicsAPI: Vulkan");
                        _logger.Information($"(Window-{window.windowID}): > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"(Window-{window.windowID}): Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: vsync,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        SwapchainSource source;

                        IntPtr wlDisplay = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowWaylandDisplayPointer, IntPtr.Zero);
                        IntPtr wlSurface = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowWaylandSurfacePointer, IntPtr.Zero);

                        if (wlDisplay != IntPtr.Zero && wlSurface != IntPtr.Zero)
                        {
                            _logger.Information($"(Window-{window.windowID}): > Display Protocol: Wayland");
                            source = SwapchainSource.CreateWayland(wlDisplay, wlSurface);
                        }
                        else
                        {
                            _logger.Information($"(Window-{window.windowID}): > Display Protocol: X11");
                            IntPtr x11Display = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowX11DisplayPointer, IntPtr.Zero);
                            uint x11Window = (uint)SDL.GetNumberProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowX11WindowNumber, 0);
                            source = SwapchainSource.CreateXlib(x11Display, (IntPtr)x11Window);
                        }

                        SwapchainDescription scDesc = new SwapchainDescription(
                            source,
                            (uint)window.Width,
                            (uint)window.Height,
                            PixelFormat.R32Float,
                            true);

                        _graphicsDevice = GraphicsDevice.CreateVulkan(options, scDesc);
                        _logger.Information($"(Window-{window.windowID}): Complete!");
                        break;
                    }

                case GraphicsAPI.Metal:
                    throw new PlatformNotSupportedException("Metal is not supported on Linux. Please use Vulkan.");
                case GraphicsAPI.OpenGL:
                    {
                        _logger.Information($"(Window-{window.windowID}): > GraphicsAPI: OpenGL (This has not been confirmed to run and work correctly yet!)");
                        _logger.Information($"(Window-{window.windowID}): > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"(Window-{window.windowID}): Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: vsync,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        IntPtr glContextHandle = SDL.GLCreateContext(window.windowPtr);

                        Func<string, IntPtr> getProc = name =>
                        {
                            SDL.FunctionPointer fp = SDL.GLGetProcAddress(name);
                            return Marshal.GetFunctionPointerForDelegate<SDL.FunctionPointer>(fp);
                        };

                        Action<IntPtr> makeCurrent = pointer =>
                        {
                            bool err = SDL.GLMakeCurrent(window.windowPtr, pointer);
                            if (!err)
                                throw new Exception($"Failed to set up OpenGL context: {SDL.GetError()}");
                        };

                        Func<IntPtr> getCurrentContext = SDL.GLGetCurrentContext;

                        Action clearCurrentContext = () => {
                            bool err = SDL.GLMakeCurrent(IntPtr.Zero, IntPtr.Zero); 
                            if (!err)
                                throw new Exception($"Failed to clear OpenGL context: {SDL.GetError()}");
                        };

                        Action<IntPtr> deleteContext = pointer => {
                            bool err = SDL.GLDestroyContext(window.windowPtr); 
                            if (!err)
                                throw new Exception($"Failed to destroy OpenGL context: {SDL.GetError()}");
                        };

                        Action swapBuffers = () => {
                            bool err = SDL.GLSwapWindow(window.windowPtr);
                            if (!err)
                                throw new Exception($"Failed to swap buffers: {SDL.GetError()}");
                        };

                        Action<bool> setSyncToVerticalBlank = vSync =>
                        {
                            bool err = SDL.GLSetSwapInterval(vsync ? 1 : 0);
                            if (!err)
                                throw new Exception($"Failed to set swap interval for OpenGL context: {SDL.GetError()}");
                        };

                        var glPlatformInfo = new OpenGLPlatformInfo(
                            glContextHandle,
                            getProc,
                            makeCurrent,
                            getCurrentContext,
                            clearCurrentContext,
                            deleteContext,
                            swapBuffers,
                            setSyncToVerticalBlank
                        );

                        _graphicsDevice = GraphicsDevice.CreateOpenGL(options, glPlatformInfo, (uint)window.Width, (uint)window.Height);
                        _logger.Information($"(Window-{window.windowID}): Complete!");
                        break;
                    }
            }

            CreateResources();
        }


        private void InitVeldrid_OSX(GraphicsAPI rendererAPI, VelvetWindow window, bool vsync)
        {
            _logger.Information($"(Window-{window.windowID}): Initializing Veldrid...");
            _logger.Information($"(Window-{window.windowID}): > Platform: OSX");
            switch (rendererAPI)
            {
                case GraphicsAPI.D3D11:
                    throw new PlatformNotSupportedException("D3D11 is not supported on OSX. Please use Metal.");
                case GraphicsAPI.Vulkan:
                    throw new PlatformNotSupportedException("Vulkan is not supported on OSX. Please use Metal.");

                case GraphicsAPI.Metal:
                    {
                        _logger.Information($"(Window-{window.windowID}): > GraphicsAPI: Metal");
                        _logger.Information($"(Window-{window.windowID}): > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"(Window-{window.windowID}): Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: vsync,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        IntPtr nsWindow = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowCocoaWindowPointer, IntPtr.Zero);

                        _graphicsDevice = GraphicsDevice.CreateMetal(options, nsWindow);
                        _logger.Information($"(Window-{window.windowID}): Complete!");
                        break;
                    }
                case GraphicsAPI.OpenGL:
                    throw new PlatformNotSupportedException("OpenGL is not supported on OSX. Please use Metal. (OpenGL was deprecated in macOS 10.14 in favor of Metal. While it could work if OpenGL support was implemented for macOS, it remains unsupported by Apple.)");
            }

            CreateResources();
        }

        /// <summary>
        /// Creates the resources for Veldrid.
        /// </summary>
        private void CreateResources()
        {
            _logger.Information($"(Window-{_window.windowID}): Creating resources...");

            _vertices = [];
            _indices = [];

            _batches = [];

            _logger.Information($"(Window-{_window.windowID}): > Creating buffers...");
            _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(_vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            _indexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(_indexBufferSize, BufferUsage.IndexBuffer | BufferUsage.Dynamic));

            _logger.Information($"(Window-{_window.windowID}): > Buffers created");
            _logger.Information($"(Window-{_window.windowID}):   > Vertex Buffer Size: {_vertexBufferSize} bytes ({_vertexBufferSize / 1024} KB, {_vertexBufferSize / (1024 * 1024)} MB)");
            _logger.Information($"(Window-{_window.windowID}):   > Index Buffer Size: {_indexBufferSize} bytes ({_indexBufferSize / 1024} KB, {_indexBufferSize / (1024 * 1024)} MB)");


            _logger.Information($"(Window-{_window.windowID}): > Creating command list...");
            _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

            _logger.Information($"(Window-{_window.windowID}): > Creating default texture...");
            byte[] whitePixelData = [255, 255, 255, 255];
            DefaultTexture = new VelvetTexture(this, whitePixelData, 1, 1);
            CurrentTexture = DefaultTexture;

            DefaultShader = new VelvetShader(this, null, null);
            DefaultShader.SetTexture(DefaultTexture);
            CurrentShader = DefaultShader;

            _logger.Information($"(Window-{_window.windowID}): Finished creating resources");
        }
    }
}