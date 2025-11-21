using System.Text;
using static System.Action;
using Veldrid;
using Veldrid.SPIRV;

using SDL3;
using Veldrid.Vk;
using Serilog;
using Veldrid.OpenGLBindings;
using Veldrid.OpenGL;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Reflection;
using System.Data.SqlTypes;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;
using System.Drawing;

namespace Velvet.Graphics
{
    public partial class Renderer : IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<Renderer>();
        public const float DEG2RAD = MathF.PI / 180.0f;
        private VelvetWindow _window = null!;
        internal GraphicsDevice _graphicsDevice = null!;
        private CommandList _commandList = null!;
        private DeviceBuffer _vertexBuffer = null!;
        private DeviceBuffer _indexBuffer = null!;
        private DeviceBuffer _uniformBuffer = null!;
        private uint _vertexBufferSize = 1024 * 1024 * 8; // 8 MB
        private uint _indexBufferSize = 1024 * 1024 * 12; // 12 MB
        private ResourceSet _resourceSetV = null!;
        private Shader[] _shaders = null!;
        private Pipeline _pipeline = null!;
        private List<Vertex> _vertices = null!;
        private List<uint> _indices = null!;


        // TODO: either move this somewhere else or clean it up
        private const string VertexCode = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 Anchor;
layout(location = 2) in vec2 UV;
layout(location = 3) in float Rotation;
layout(location = 4) in uint Color;

layout(location = 0) out vec2 fsin_UV;
layout(location = 1) out vec4 fsin_Color;

layout(std140, binding = 0) uniform Resolution {
    uvec2 windowResolution;
};

vec4 UnpackColor(uint packed)
{
    float r = float((packed >>  0) & 0xFF) / 255.0;
    float g = float((packed >>  8) & 0xFF) / 255.0;
    float b = float((packed >> 16) & 0xFF) / 255.0;
    float a = float((packed >> 24) & 0xFF) / 255.0;
    return vec4(r, g, b, a);
}

void main()
{
    vec2 pos = Position - Anchor;
    float c = cos(Rotation);
    float s = sin(Rotation);
    mat2 rot = mat2(c, -s, s, c);
    pos *= rot;
    pos += Anchor;

    vec2 ndc = (pos / vec2(windowResolution)) * 2.0 - 1.0;
    gl_Position = vec4(ndc.x, -ndc.y, 0.0, 1.0);
    gl_PointSize = 5.0;
    fsin_UV = UV;
    fsin_Color = UnpackColor(Color);
}";

        private const string FragmentCode = @"
#version 450

layout(location = 0) in vec2 fsin_UV;
layout(location = 1) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

layout(set = 1, binding = 0) uniform texture2D Texture2D;
layout(set = 1, binding = 1) uniform sampler Sampler;

void main()
{
    vec4 color = texture(sampler2D(Texture2D, Sampler), fsin_UV);
    fsout_Color = color * fsin_Color;
}";

        /// <summary>
        /// Initializes Veldrid, with the specifed RendererAPI and VelvetWindow on Windows.
        /// </summary>
        /// <param name="rendererAPI"></param>
        /// <param name="window"></param>
        /// <exception cref="PlatformNotSupportedException"></exception>
        private void InitVeldrid_WIN(RendererAPI rendererAPI, VelvetWindow window, bool vsync)
        {
            _logger.Information($"Window-{window.windowID}: Initializing Veldrid...");
            _logger.Information($"Window-{window.windowID}: > Platform: Windows");
            switch (rendererAPI)
            {
                case RendererAPI.D3D11:
                    {

                        _logger.Information($"Window-{window.windowID}: > GraphicsAPI: D3D11");
                        _logger.Information($"Window-{window.windowID}: > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"Window-{window.windowID}: Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: vsync,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        IntPtr hwmd = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowWin32HWNDPointer, IntPtr.Zero);
                        _graphicsDevice = GraphicsDevice.CreateD3D11(options, hwmd, (uint)_window.GetWidth(), (uint)_window.GetHeight());
                        break;
                    }


                case RendererAPI.Vulkan:
                    {
                        _logger.Information($"Window-{window.windowID}: > GraphicsAPI: Vulkan");
                        _logger.Information($"Window-{window.windowID}: > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"Window-{window.windowID}: Creating graphics device...");
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

                        _graphicsDevice = GraphicsDevice.CreateVulkan(options, vkSurfaceSource, (uint)_window.GetWidth(), (uint)_window.GetHeight());
                        _logger.Information($"Window-{window.windowID}: Complete!");
                        break;
                    }

                case RendererAPI.Metal:
                    throw new PlatformNotSupportedException("Metal is not supported on Windows. Please use either D3D11 or Vulkan.");
                case RendererAPI.OpenGL:
                    {
                        _logger.Information($"Window-{window.windowID}: > GraphicsAPI: OpenGL");
                        _logger.Information($"Window-{window.windowID}: > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"Window-{window.windowID}: Creating graphics device...");
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

                        _graphicsDevice = GraphicsDevice.CreateOpenGL(options, glPlatformInfo, (uint)window.GetWidth(), (uint)window.GetHeight());
                        _logger.Information($"Window-{window.windowID}: Complete! (im praying thatit works)");
                        break;
                    }


            }

            CreateResources();
        }

        private void InitVeldrid_LINUX(RendererAPI rendererAPI, VelvetWindow window, bool vsync)
        {
            _logger.Information($"Window-{window.windowID}: Initializing Veldrid...");
            _logger.Information($"Window-{window.windowID}: > Platform: Linux");
            switch (rendererAPI)
            {
                case RendererAPI.D3D11:
                    throw new PlatformNotSupportedException("D3D11 is not supported on Linux. Please use Vulkan.");


                case RendererAPI.Vulkan:
                    {
                        _logger.Information($"Window-{window.windowID}: > GraphicsAPI: Vulkan");
                        _logger.Information($"Window-{window.windowID}: > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"Window-{window.windowID}: Creating graphics device...");
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
                            _logger.Information($"Window-{window.windowID}: > Display Protocol: Wayland");
                            source = SwapchainSource.CreateWayland(wlDisplay, wlSurface);
                        }
                        else
                        {
                            _logger.Information($"Window-{window.windowID}: > Display Protocol: X11");
                            IntPtr x11Display = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowX11DisplayPointer, IntPtr.Zero);
                            uint x11Window = (uint)SDL.GetNumberProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowX11WindowNumber, 0);
                            source = SwapchainSource.CreateXlib(x11Display, (IntPtr)x11Window);
                        }

                        SwapchainDescription scDesc = new SwapchainDescription(
                            source,
                            (uint)_window.GetWidth(),
                            (uint)_window.GetHeight(),
                            PixelFormat.R32Float,
                            true);

                        _graphicsDevice = GraphicsDevice.CreateVulkan(options, scDesc);
                        _logger.Information($"Window-{window.windowID}: Complete!");
                        break;
                    }

                case RendererAPI.Metal:
                    throw new PlatformNotSupportedException("Metal is not supported on Linux. Please use Vulkan.");
                case RendererAPI.OpenGL:
                    {
                        _logger.Information($"Window-{window.windowID}: > GraphicsAPI: OpenGL (This has not been confirmed to run and work correctly yet!)");
                        _logger.Information($"Window-{window.windowID}: > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"Window-{window.windowID}: Creating graphics device...");
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
                            bool fp = SDL.GLMakeCurrent(window.windowPtr, pointer);
                        };

                        Func<IntPtr> getCurrentContext = SDL.GLGetCurrentContext;

                        Action clearCurrentContext = () => { SDL.GLMakeCurrent(IntPtr.Zero, IntPtr.Zero); };

                        Action<IntPtr> deleteContext = pointer => { SDL.GLDestroyContext(window.windowPtr); };

                        Action swapBuffers = () => { SDL.GLSwapWindow(window.windowPtr); };

                        Action<bool> setSyncToVerticalBlank = vSync =>
                        {
                            SDL.GLSetSwapInterval(vsync ? 1 : 0);
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

                        _graphicsDevice = GraphicsDevice.CreateOpenGL(options, glPlatformInfo, (uint)window.GetWidth(), (uint)window.GetHeight());
                        _logger.Information($"Window-{window.windowID}: Complete! (im praying thatit works)");
                        break;
                    }
            }

            CreateResources();
        }


        private void InitVeldrid_OSX(RendererAPI rendererAPI, VelvetWindow window, bool vsync)
        {
            _logger.Information($"Window-{window.windowID}: Initializing Veldrid...");
            _logger.Information($"Window-{window.windowID}: > Platform: OSX");
            switch (rendererAPI)
            {
                case RendererAPI.D3D11:
                    throw new PlatformNotSupportedException("D3D11 is not supported on OSX. Please use Metal.");
                case RendererAPI.Vulkan:
                    throw new PlatformNotSupportedException("Vulkan is not supported on OSX. Please use Metal.");

                case RendererAPI.Metal:
                    {
                        _logger.Information($"Window-{window.windowID}: > GraphicsAPI: Metal");
                        _logger.Information($"Window-{window.windowID}: > VSync: {vsync}");
                        _window = window;

                        _logger.Information($"Window-{window.windowID}: Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: vsync,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        IntPtr nsWindow = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowCocoaWindowPointer, IntPtr.Zero);

                        _graphicsDevice = GraphicsDevice.CreateMetal(options, nsWindow);
                        _logger.Information($"Window-{window.windowID}: Complete!");
                        break;
                    }
                case RendererAPI.OpenGL:
                    throw new PlatformNotSupportedException("OpenGL is not supported on OSX. Please use Metal. (OpenGL was deprecated in macOS 10.14)");
            }

            CreateResources();
        }

        /// <summary>
        /// Creates the resources for Veldrid.
        /// </summary>
        private void CreateResources()
        {
            _logger.Information($"Window-{_window.windowID}: Creating resources");

            

            _vertices = [];
            _indices = [];

            _logger.Information($"Window-{_window.windowID}: Creating buffers...");
            _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(_vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            _indexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(_indexBufferSize, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
            _uniformBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(ResolutionData.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            _logger.Information($"Window-{_window.windowID}: Buffers created");
            _logger.Information($"Window-{_window.windowID}: > Vertex Buffer Size: {_vertexBufferSize} bytes ({_vertexBufferSize / 1024} KB, {_vertexBufferSize / (1024 * 1024)} MB)");
            _logger.Information($"Window-{_window.windowID}: > Index Buffer Size: {_indexBufferSize} bytes ({_indexBufferSize / 1024} KB, {_indexBufferSize / (1024 * 1024)} MB)");
            _logger.Information($"Window-{_window.windowID}: > Uniform Buffer Size: {ResolutionData.SizeInBytes} bytes ({ResolutionData.SizeInBytes / 1024} KB, {ResolutionData.SizeInBytes / (1024 * 1024)} MB)");

            ResourceLayout resourceLayoutV = _graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Resolution", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
                )
            );

            _resourceSetV = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayoutV,
                _uniformBuffer
            ));

            ResourceLayout resourceLayoutF = _graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture2D", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )
            );

            _logger.Information($"Window-{_window.windowID}: Creating shaders...");
            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Anchor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Rotation", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1));

            ShaderDescription vertexShaderDesc = new(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(VertexCode),
                "main");
            ShaderDescription fragmentShaderDesc = new(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(FragmentCode),
                "main");

            _logger.Information($"Window-{_window.windowID}: > Compiling shaders...");
            _shaders = _graphicsDevice.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            _logger.Information($"Window-{_window.windowID}: Creating pipeline...");
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SINGLE_OVERRIDE_BLEND,

                DepthStencilState = DepthStencilStateDescription.DISABLED,

                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.CounterClockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false),

                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = [resourceLayoutV, resourceLayoutF],

                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: [vertexLayout],
                    shaders: _shaders),

                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
            };
            _pipeline = _graphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);

            byte[] whitePixelData = [255, 255, 255, 255];
            _defaultTexture = new VelvetTexture(_graphicsDevice, whitePixelData, 1, 1);
            _currentTexture = _defaultTexture;

            _logger.Information($"Window-{_window.windowID}: Creating command list...");
            _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
            _logger.Information($"Window-{_window.windowID}: Finished creating resources");
        }
    }
}