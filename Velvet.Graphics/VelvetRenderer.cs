using Velvet;
using SDL3;
using static SDL3.SDL;

using Veldrid;
using Veldrid.SPIRV;
using SharpGen.Runtime.Win32;
using System.Numerics;
using System.Text;
using Vulkan;
using Vulkan.Xlib;

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
        uint p0;
        uint p1;

        public ResolutionData(uint width, uint height)
        {
            w = width;
            h = height;
            p0 = 0;
            p1 = 0;
        }

        public const uint SizeInBytes = 16;

    }

    public class Renderer
    {
        public const float DEG2RAD = MathF.PI / 180.0f;
        private static VelvetWindow _window;
        private static GraphicsDevice _graphicsDevice;
        private static CommandList _commandList;
        private static DeviceBuffer _vertexBuffer;
        private static DeviceBuffer _uniformBuffer;
        private static ResourceSet _resourceSet;
        private static DeviceBuffer _indexBuffer;
        private static Shader[] _shaders;
        private static Pipeline _pipeline;
        private List<VertexPositionColor> _vertices;
        private List<ushort> _indices;


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
    mat2 rot = mat2(c, s, -s, c);
    pos *= rot;
    pos += Anchor;

    vec2 ndc = (pos / vec2(windowResolution)) * 2.0 - 1.0;
    gl_Position = vec4(ndc, 0.0, 1.0);
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

        private static uint PackColor(RgbaFloat color)
        {
            byte r = (byte)(Math.Clamp(color.R, 0f, 1f) * 255f);
            byte g = (byte)(Math.Clamp(color.G, 0f, 1f) * 255f);
            byte b = (byte)(Math.Clamp(color.B, 0f, 1f) * 255f);
            byte a = (byte)(Math.Clamp(color.A, 0f, 1f) * 255f);

            return (uint)(r | (g << 8) | (b << 16) | (a << 24));
        }

        public Renderer(VelvetWindow window)
        {
            _window = window;
            _vertices = new();
            _indices = new();

            var options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: null,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferStandardClipSpaceYDirection: true,
                preferDepthRangeZeroToOne: true);

            IntPtr hwmd = SDL_GetPointerProperty(SDL_GetWindowProperties(_window.windowPtr), SDL_PROP_WINDOW_WIN32_HWND_POINTER, IntPtr.Zero);
            _graphicsDevice = GraphicsDevice.CreateD3D11(options, hwmd, (uint)_window.GetWidth(), (uint)_window.GetHeight());

            _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(1024 * 1024, BufferUsage.VertexBuffer));
            _uniformBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(ResolutionData.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _indexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(512 * 1024, BufferUsage.IndexBuffer));

            ResourceLayout resourceLayout = _graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Resolution", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
                )
            );

            _resourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayout,
                _uniformBuffer));



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

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,

                DepthStencilState = DepthStencilStateDescription.Disabled,

                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
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

            _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

        }

        public void DrawRectangle(Vector2 pos, Vector2 size, float rotation, RgbaFloat color)
        {
            _vertices.Add(new VertexPositionColor(pos, pos + size / 2, rotation, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size * Vector2.UnitY, pos + size / 2, rotation, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size, pos + size / 2, rotation, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size * Vector2.UnitX, pos + size / 2, rotation, PackColor(color)));

            int baseIndex = _vertices.Count - 4;

            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 0));
        }


        public void Begin()
        {
            _commandList.Begin();

            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);

            ResolutionData resolutionData = new(
                (uint)_window.GetWidth(),
                (uint)_window.GetHeight()
            );

            _graphicsDevice.UpdateBuffer(_uniformBuffer, 0, ref resolutionData);
        }

        public void End()
        {
            if (_vertices.Count > 0)
                _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertices.ToArray());

            if (_indices.Count > 0)
                _graphicsDevice.UpdateBuffer(_indexBuffer, 0, _indices.ToArray());

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetGraphicsResourceSet(0, _resourceSet);
            _commandList.DrawIndexed(
                indexCount: (uint)_indices.Count,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);

            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);

            _graphicsDevice.SwapBuffers();

            _vertices.Clear();
            _indices.Clear();
        }

        public void ClearColor(RgbaFloat color)
        {
            _commandList.ClearColorTarget(0, color);
        }

        public void Dispose()
        {
            _pipeline.Dispose();
            foreach (Shader shader in _shaders)
            {
                shader.Dispose();
            }
            _commandList.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _graphicsDevice.Dispose();
        }
    }
}
