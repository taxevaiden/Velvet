using System.Text;
using Veldrid;
using Veldrid.SPIRV;

using SDL3;
using Veldrid.Vk;
using Serilog;
using Veldrid.OpenGLBindings;
using Veldrid.OpenGL;
using System.Runtime.InteropServices;

namespace Velvet.Graphics
{
    public partial class Renderer
    {
        private readonly ILogger _logger = Log.ForContext<Renderer>();
        public const float DEG2RAD = MathF.PI / 180.0f;
        private VelvetWindow _window = null!;
        private GraphicsDevice _graphicsDevice = null!;
        private CommandList _commandList = null!;
        private DeviceBuffer _vertexBuffer = null!;
        private DeviceBuffer _uniformBuffer = null!;
        private ResourceSet _resourceSet = null!;
        private DeviceBuffer _indexBuffer = null!;
        private Shader[] _shaders = null!;
        private Pipeline _pipeline = null!;
        private List<Vertex> _vertices = null!;
        private List<uint> _indices = null!;


        // TODO: either move this somewhere else or clean it up
        private const string VertexCode = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 Anchor;
layout(location = 2) in float Rotation;
layout(location = 3) in uint Color;

layout(location = 0) out vec4 fsin_Color;

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
    fsin_Color = UnpackColor(Color);
}";

        private const string FragmentCode = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

        /// <summary>
        /// Initializes Veldrid, with the specifed RendererAPI and VelvetWindow on Windows.
        /// </summary>
        /// <param name="rendererAPI"></param>
        /// <param name="window"></param>
        /// <exception cref="PlatformNotSupportedException"></exception>
        private void InitVeldrid_WIN(RendererAPI rendererAPI, VelvetWindow window, bool vsync)
        {
            _logger.Information($"Window-{window.windowID}: Platform: Windows");
            _logger.Information($"Window-{window.windowID}: Initializing Veldrid...");
            switch (rendererAPI)
            {
                case RendererAPI.D3D11:
                    {
                        _logger.Information($"Window-{window.windowID}: Using D3D11");
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
                        _logger.Information($"Window-{window.windowID}: Complete!");
                        break;
                    }


                case RendererAPI.Vulkan:
                    {
                        _logger.Information($"Window-{window.windowID}: Using Vulkan");
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

            }

            CreateResources();
        }

        private void InitVeldrid_LINUX(RendererAPI rendererAPI, VelvetWindow window, bool vsync)
        {
            _logger.Information($"Window-{window.windowID}: Platform: Linux");
            _logger.Information($"Window-{window.windowID}: Initializing Veldrid...");
            switch (rendererAPI)
            {
                case RendererAPI.D3D11:
                    throw new PlatformNotSupportedException("D3D11 is not supported on Linux. Please use Vulkan.");


                case RendererAPI.Vulkan:
                    {
                        _logger.Information($"Window-{window.windowID}: Using Vulkan");
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
                            _logger.Information($"Window-{window.windowID}: Using Wayland");
                            source = SwapchainSource.CreateWayland(wlDisplay, wlSurface);
                        }
                        else
                        {
                            _logger.Information($"Window-{window.windowID}: Using X11");
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
            }

            CreateResources();
        }


        private void InitVeldrid_OSX(RendererAPI rendererAPI, VelvetWindow window, bool vsync)
        {
            _logger.Information($"Window-{window.windowID}: Platform: OSX");
            _logger.Information($"Window-{window.windowID}: Initializing Veldrid...");
            switch (rendererAPI)
            {
                case RendererAPI.D3D11:
                    throw new PlatformNotSupportedException("D3D11 is not supported on OSX. Please use Metal.");
                case RendererAPI.Vulkan:
                    throw new PlatformNotSupportedException("Vulkan is not supported on OSX. Please use Metal.");

                case RendererAPI.Metal:
                    {
                        _logger.Information($"Window-{window.windowID}: Using Metal");
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
            }

            CreateResources();
        }

        /// <summary>
        /// Creates the resources for Veldrid.
        /// </summary>
        private void CreateResources()
        {
            _vertices = [];
            _indices = [];

            _logger.Information($"Window-{_window.windowID}: Creating buffers...");
            _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(1024 * 1024 * 20, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            _uniformBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(ResolutionData.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _indexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(1024 * 1024 * 15, BufferUsage.IndexBuffer | BufferUsage.Dynamic));

            ResourceLayout resourceLayout = _graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Resolution", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
                )
            );

            _resourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayout,
                _uniformBuffer));

            _logger.Information($"Window-{_window.windowID}: Creating shaders...");
            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Anchor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
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
                ResourceLayouts = [resourceLayout],

                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: [vertexLayout],
                    shaders: _shaders),

                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
            };
            _pipeline = _graphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);

            _logger.Information($"Window-{_window.windowID}: Creating command list...");
            _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
            _logger.Information($"Window-{_window.windowID}: Complete!");
        }
    }
}