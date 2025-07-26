using System.Text;
using Veldrid;
using Veldrid.SPIRV;

using SDL3;
using Veldrid.Vk;
using Serilog;

namespace Velvet.Graphics
{
    public partial class Renderer
    {
        private static readonly ILogger _logger = Log.ForContext<Renderer>();
        public const float DEG2RAD = MathF.PI / 180.0f;
        private static VelvetWindow _window = null!;
        private static GraphicsDevice _graphicsDevice = null!;
        private static CommandList _commandList = null!;
        private static DeviceBuffer _vertexBuffer = null!;
        private static DeviceBuffer _uniformBuffer = null!;
        private static ResourceSet _resourceSet = null!;
        private static DeviceBuffer _indexBuffer = null!;
        private static Shader[] _shaders = null!;
        private static Pipeline _pipeline = null!;
        private List<VertexPositionColor> _vertices = null!;
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
        private void InitVeldrid_WIN(RendererAPI rendererAPI, VelvetWindow window)
        {
            _logger.Information("Platform: Windows");
            _logger.Information("Initializing Veldrid...");
            switch (rendererAPI)
            {
                case RendererAPI.D3D11:
                    {
                        _logger.Information("Using D3D11");
                        _window = window;
                        _vertices = [];
                        _indices = [];

                        _logger.Information("Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: true,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        IntPtr hwmd = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), "SDL.window.win32.hwnd", IntPtr.Zero);
                        _graphicsDevice = GraphicsDevice.CreateD3D11(options, hwmd, (uint)_window.GetWidth(), (uint)_window.GetHeight());
                        _logger.Information("Complete!");
                        break;
                    }


                case RendererAPI.Vulkan:
                    {
                        _logger.Information("Using Vulkan");
                        _window = window;
                        _vertices = new();
                        _indices = new();

                        _logger.Information("Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: true,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        var hinstance = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowWin32HWNDPointer, IntPtr.Zero);
                        var hwmd = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowWin32HWNDPointer, IntPtr.Zero);

                        VkSurfaceSource vkSurfaceSource = VkSurfaceSource.CreateWin32(hinstance, hwmd);

                        _graphicsDevice = GraphicsDevice.CreateVulkan(options, vkSurfaceSource, (uint)_window.GetWidth(), (uint)_window.GetHeight());
                        break;
                    }

                case RendererAPI.Metal:
                    throw new PlatformNotSupportedException("Metal is not supported on Windows. Please use either D3D11 or Vulkan.");
            }

            CreateResources();
        }

        private void InitVeldrid_LINUX(RendererAPI rendererAPI, VelvetWindow window)
        {
            _logger.Information("Platform: Linux");
            _logger.Information("Initializing Veldrid...");
            switch (rendererAPI)
            {
                case RendererAPI.D3D11:
                    {
                        throw new PlatformNotSupportedException("D3D11 is not supported on Linux. Please use Vulkan.");
                    }


                case RendererAPI.Vulkan:
                    {
                        _logger.Information("Using Vulkan");
                        _window = window;
                        _vertices = new();
                        _indices = new();

                        _logger.Information("Creating graphics device...");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: true,
                            resourceBindingModel: ResourceBindingModel.Improved,
                            preferStandardClipSpaceYDirection: true,
                            preferDepthRangeZeroToOne: true);

                        SwapchainSource source;

                        IntPtr wlDisplay = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowWaylandDisplayPointer, IntPtr.Zero);
                        IntPtr wlSurface = SDL.GetPointerProperty(SDL.GetWindowProperties(_window.windowPtr), SDL.Props.WindowWaylandSurfacePointer, IntPtr.Zero);

                        if (wlDisplay != IntPtr.Zero && wlSurface != IntPtr.Zero)
                        {
                            source = SwapchainSource.CreateWayland(wlDisplay, wlSurface);
                        }
                        else
                        {
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

                        break;
                    }

                case RendererAPI.Metal:
                    throw new PlatformNotSupportedException("Metal is not supported on Linux. Please use Vulkan.");
            }

            CreateResources();
        }

        /// <summary>
        /// Creates the resources for Veldrid.
        /// </summary>
        private void CreateResources()
        {
            _logger.Information("Creating buffers...");
            _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(1024 * 1024 * 20, BufferUsage.VertexBuffer));
            _uniformBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(ResolutionData.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _indexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(1024 * 1024 * 15, BufferUsage.IndexBuffer));

            ResourceLayout resourceLayout = _graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Resolution", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
                )
            );

            _resourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayout,
                _uniformBuffer));

            _logger.Information("Creating shaders...");
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

            _logger.Information("Creating pipeline...");
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

            _logger.Information("Creating command list...");
            _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
            _logger.Information("Complete!");
        }
    }
}