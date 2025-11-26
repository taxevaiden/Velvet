using System.Text;
using System.IO;
using Veldrid;
using Veldrid.SPIRV;
using Serilog;

namespace Velvet.Graphics
{
    public class VelvetShader : IDisposable
    {
        private const string DefaultVertexCode = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 UV;
layout(location = 2) in uint Color;

layout(location = 0) out vec2 fsin_UV;
layout(location = 1) out vec4 fsin_Color;

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
    gl_Position = vec4(Position, 0.0, 1.0);
    gl_PointSize = 5.0;
    fsin_UV = UV;
    fsin_Color = UnpackColor(Color);
}";

        private const string DefaultFragmentCode = @"
#version 450

layout(location = 0) in vec2 fsin_UV;
layout(location = 1) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

layout(set = 0, binding = 0) uniform texture2D Texture2D;
layout(set = 0, binding = 1) uniform sampler Sampler;

void main()
{
    vec4 color = texture(sampler2D(Texture2D, Sampler), fsin_UV);
    fsout_Color = color * fsin_Color;
}";

        internal Pipeline Pipeline;
        internal Shader[] Shaders;

        private readonly ILogger _logger = Log.ForContext<VelvetShader>();

        public VelvetShader(Renderer renderer, string? vertShaderFilePath, string? fragShaderFilePath)
        {
            _logger.Information($"(Window-{renderer._window.windowID}): Creating shaders");
            ResourceLayout resourceLayoutF = renderer._graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture2D", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )
            );

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1));

            _logger.Information($"(Window-{renderer._window.windowID}): > {(vertShaderFilePath == null ? "Vertex shader file path not set, using default vertex shader" : $"Vertex shader file path: {vertShaderFilePath}")}");
            _logger.Information($"(Window-{renderer._window.windowID}): > {(fragShaderFilePath == null ? "Fragment shader file path not set, using default fragment shader" : $"Fragment shader file path: {fragShaderFilePath}")}");

            ShaderDescription vertexShaderDesc = new(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(vertShaderFilePath == null ? DefaultVertexCode : File.ReadAllText(vertShaderFilePath)),
                "main");
            ShaderDescription fragmentShaderDesc = new(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(fragShaderFilePath == null ? DefaultFragmentCode : File.ReadAllText(fragShaderFilePath)),
                "main");

            _logger.Information($"(Window-{renderer._window.windowID}): > Compiling shaders");
            Shaders = renderer._graphicsDevice.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            _logger.Information($"(Window-{renderer._window.windowID}): > Creating pipeline for shaders");
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
                ResourceLayouts = [resourceLayoutF],

                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: [vertexLayout],
                    shaders: Shaders),

                Outputs = renderer._graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            Pipeline = renderer._graphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
        }

        // Use within Renderer Class

        internal VelvetShader(GraphicsDevice gd, string? vertShaderFilePath, string? fragShaderFilePath)
        {
            _logger.Information($"(Within Renderer): Creating shaders");

            ResourceLayout resourceLayoutF = gd.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture2D", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )
            );

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1));

            _logger.Information($"(Within Renderer): > {(vertShaderFilePath == null ? "Vertex shader file path not set, using default vertex shader" : $"Vertex shader file path: {vertShaderFilePath}")}");
            _logger.Information($"(Within Renderer): > {(fragShaderFilePath == null ? "Fragment shader file path not set, using default fragment shader" : $"Fragment shader file path: {fragShaderFilePath}")}");

            ShaderDescription vertexShaderDesc = new(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(vertShaderFilePath == null ? DefaultVertexCode : File.ReadAllText(vertShaderFilePath)),
                "main");
            ShaderDescription fragmentShaderDesc = new(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(fragShaderFilePath == null ? DefaultFragmentCode : File.ReadAllText(fragShaderFilePath)),
                "main");

            _logger.Information($"(Within Renderer): > Compiling shaders");
            Shaders = gd.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            _logger.Information($"(Within Renderer): > Creating pipeline for shaders");
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
                ResourceLayouts = [resourceLayoutF],

                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: [vertexLayout],
                    shaders: Shaders),

                Outputs = gd.SwapchainFramebuffer.OutputDescription
            };

            Pipeline = gd.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
        }

        public void Dispose()
        {
            foreach (Shader shader in Shaders)
            {
                shader.Dispose();
            }
            Pipeline.Dispose();
        }
    }
}