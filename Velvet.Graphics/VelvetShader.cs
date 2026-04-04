using System.IO;
using System.Numerics;
using System.Text;

using Veldrid;
using Veldrid.SPIRV;

using Velvet.Graphics.Textures;

namespace Velvet.Graphics.Shaders
{
    // Public types

    /// <summary>
    /// The type of a shader uniform. This is used to determine the size and alignment of the uniform in the shader's uniform buffer.
    /// </summary>
    public enum UniformType
    {
        /// <summary>
        /// A single float value.
        /// </summary>
        Float,
        /// <summary>
        /// A single int value.
        /// </summary>
        Int,
        /// <summary>
        /// A single uint value.
        /// </summary>
        UInt,
        /// <summary>
        /// A 2D vector of floats.
        /// </summary>
        Vector2,
        /// <summary>
        /// A 3D vector of floats. Note that in std140 layout, a vec3 takes up 16 bytes of space (the same as a vec4) to ensure proper alignment.
        /// </summary>
        Vector3,
        /// <summary>
        /// A 4D vector of floats.
        /// </summary>
        Vector4,
        /// <summary>
        /// A 4x4 matrix of floats, stored in column-major order. This takes up 64 bytes of space in the uniform buffer.
        /// </summary>
        Matrix4x4
    }

    /// <summary>
    /// The shader stage(s) that a uniform is used in. This is used to determine which shader stages the uniform buffer will be bound to.
    /// </summary>
    public enum UniformStage
    {
        /// <summary>
        /// The uniform is used in the vertex shader stage.
        /// </summary>
        Vertex   = ShaderStages.Vertex,
        /// <summary>
        /// The uniform is used in the fragment shader stage.
        /// </summary>
        Fragment = ShaderStages.Fragment,
        /// <summary>
        /// The uniform is used in both the vertex and fragment shader stages.
        /// </summary>
        Both     = ShaderStages.Vertex | ShaderStages.Fragment
    }

    /// <summary>
    /// A description of a shader uniform, including its name, type, and the shader stage(s) it is used in. This is used to define the layout of the shader's uniform buffer.
    /// </summary>
    /// <param name="Name">The name of the uniform.</param>
    /// <param name="Type">The type of the uniform.</param>
    /// <param name="Stage">The shader stage(s) the uniform is used in.</param>
    public readonly record struct UniformDescription(
        string      Name,
        UniformType Type,
        UniformStage Stage);

    // Internal layout type

    internal readonly record struct PackedUniform(
        string      Name,
        UniformType Type,
        uint        Offset,
        uint        Size);

    // VelvetShader

    /// <summary>
    /// A shader that can be used for rendering.
    /// </summary>
    public sealed class VelvetShader : IDisposable
    {
        // Pipeline cache key — record struct gives structural Equals/GetHashCode for free.
        private readonly record struct PipelineKey(OutputDescription Outputs);

        // Default GLSL sources

        private const string DefaultVertexCode = """
            #version 450

            layout(location = 0) in vec2 Position;
            layout(location = 1) in vec2 UV;
            layout(location = 2) in vec4 Color;

            layout(location = 0) out vec2 fsin_UV;
            layout(location = 1) out vec4 fsin_Color;

            void main()
            {
                gl_Position = vec4(Position, 0.0, 1.0);
                fsin_UV     = UV;
                fsin_Color  = Color;
            }
            """;

        private const string DefaultFragmentCode = """
            #version 450

            layout(location = 0) in  vec2 fsin_UV;
            layout(location = 1) in  vec4 fsin_Color;
            layout(location = 0) out vec4 fsout_Color;

            layout(set = 0, binding = 0) uniform texture2D Texture0;
            layout(set = 0, binding = 1) uniform sampler   Sampler0;

            void main()
            {
                fsout_Color = texture(sampler2D(Texture0, Sampler0), fsin_UV) * fsin_Color;
            }
            """;

        // Veldrid objects

        internal Pipeline    Pipeline    = null!;
        internal Shader[]    Shaders     = null!;
        internal ResourceSet ResourceSet = null!;
        internal DeviceBuffer? UniformBuffer;

        private readonly GraphicsDevice _gd;
        private readonly Dictionary<PipelineKey, Pipeline> _pipelineCache = new();
        private readonly Dictionary<string, PackedUniform> _uniforms      = new();

        private ResourceLayout _resourceLayout = null!;
        private VelvetTexture  _texture        = null!;

        private byte[]               _cpuUniformBuffer = Array.Empty<byte>();
        private OutputDescription    _currentOutputs;
        private VelvetRenderTexture? _currentRenderTarget;

        private bool _uniformsDirty;
        private bool _resourceSetDirty;
        private bool _pipelineDirty;

        // Construction

        /// <summary>
        /// Creates a new <see cref="VelvetShader"/>.
        /// </summary>
        /// <param name="renderer">The owning renderer.</param>
        /// <param name="vertPath">Path to a SPIR-V vertex shader, or <c>null</c> for the built-in default.</param>
        /// <param name="fragPath">Path to a SPIR-V fragment shader, or <c>null</c> for the built-in default.</param>
        /// <param name="uniforms">Optional uniform layout. Pass <c>null</c> if the shader has no custom uniforms.</param>
        public VelvetShader(
            VelvetRenderer       renderer,
            string?              vertPath,
            string?              fragPath,
            UniformDescription[]? uniforms = null)
        {
            _gd      = renderer._graphicsDevice;
            _texture = renderer.CurrentTexture;
            Init(vertPath, fragPath, uniforms);
        }

        // Uniform size / alignment helpers

        private static uint Align(uint value, uint alignment)
            => (value + alignment - 1) & ~(alignment - 1);

        private static uint GetUniformSize(UniformType type) => type switch
        {
            UniformType.Float    => 4,
            UniformType.Int      => 4,
            UniformType.UInt     => 4,
            UniformType.Vector2  => 8,
            UniformType.Vector3  => 16,  // std140 vec3 = 16 bytes
            UniformType.Vector4  => 16,
            UniformType.Matrix4x4 => 64,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        // Initialization

        private void Init(
            string?               vertPath,
            string?               fragPath,
            UniformDescription[]? uniformDescriptions)
        {
            // Build resource layout elements (texture + sampler always present).
            var layoutElements = new List<ResourceLayoutElementDescription>
            {
                new("Texture0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new("Sampler0", ResourceKind.Sampler,         ShaderStages.Fragment),
            };

            // Pack uniforms into a single UBO if any were supplied.
            if (uniformDescriptions is { Length: > 0 })
            {
                uint offset = 0;
                foreach (var u in uniformDescriptions)
                {
                    uint size = GetUniformSize(u.Type);
                    offset = Align(offset, size);

                    _uniforms[u.Name] = new PackedUniform(u.Name, u.Type, offset, size);
                    offset += size;
                }

                uint totalSize    = Align(offset, 16);
                _cpuUniformBuffer = new byte[totalSize];

                UniformBuffer = _gd.ResourceFactory.CreateBuffer(
                    new BufferDescription(totalSize, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

                layoutElements.Add(new ResourceLayoutElementDescription(
                    "Globals",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex | ShaderStages.Fragment));
            }

            _resourceLayout = _gd.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(layoutElements.ToArray()));

            // Compile shaders
            byte[] vertBytes = vertPath is null
                ? Encoding.UTF8.GetBytes(DefaultVertexCode)
                : File.ReadAllBytes(vertPath);

            byte[] fragBytes = fragPath is null
                ? Encoding.UTF8.GetBytes(DefaultFragmentCode)
                : File.ReadAllBytes(fragPath);

            Shaders = _gd.ResourceFactory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex,   vertBytes, "main"),
                new ShaderDescription(ShaderStages.Fragment, fragBytes, "main"));

            _currentOutputs = _gd.SwapchainFramebuffer.OutputDescription;

            RebuildResourceSet();
            RebuildPipeline();
        }

        // Rebuild helpers

        private void RebuildResourceSet()
        {
            ResourceSet?.Dispose();
 
            ResourceSet = UniformBuffer is not null
                ? _gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                    _resourceLayout, _texture.View, _texture.Sampler, UniformBuffer))
                : _gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                    _resourceLayout, _texture.View, _texture.Sampler));
 
            _resourceSetDirty = false;
        }

        private void RebuildPipeline()
        {
            var key = new PipelineKey(_currentOutputs);
            if (_pipelineCache.TryGetValue(key, out var cached))
            {
                Pipeline      = cached;
                _pipelineDirty = false;
                return;
            }

            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("UV",       VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color",    VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            var pipeline = _gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription
            {
                BlendState        = BlendStateDescription.SINGLE_ALPHA_BLEND,
                DepthStencilState = DepthStencilStateDescription.DISABLED,
                RasterizerState   = new RasterizerStateDescription(
                    FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.CounterClockwise,
                    depthClipEnabled: true, scissorTestEnabled: false),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts   = new[] { _resourceLayout },
                ShaderSet         = new ShaderSetDescription(new[] { vertexLayout }, Shaders),
                Outputs           = _currentOutputs
            });

            _pipelineCache[key] = pipeline;
            Pipeline             = pipeline;
            _pipelineDirty       = false;
        }

        // Public uniform setters

        /// <summary>Sets a uniform value by name.</summary>
        public void Set(string name, float    value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, int      value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, uint     value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, Vector2  value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, Vector3  value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, Vector4  value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, Matrix4x4 value) => Write(name, value);

        // Internal state setters (called by the renderer)

        internal void SetTexture(VelvetTexture texture)
        {
            if (_texture == texture) return;
            _texture          = texture;
            _resourceSetDirty = true;
        }

        internal void SetRenderTexture(VelvetRenderTexture? rt)
        {
            var newOutputs = rt?.Framebuffer.OutputDescription
                             ?? _gd.SwapchainFramebuffer.OutputDescription;

            if (_currentRenderTarget == rt && _currentOutputs.Equals(newOutputs)) return;

            _currentRenderTarget = rt;
            _currentOutputs      = newOutputs;
            _pipelineDirty       = true;
        }

        // Write / flush

        private unsafe void Write<T>(string name, T value) where T : unmanaged
        {
            if (!_uniforms.TryGetValue(name, out var u))
                throw new InvalidOperationException($"Uniform '{name}' is not defined in this shader.");

            fixed (byte* dst = &_cpuUniformBuffer[u.Offset])
                *(T*)dst = value;

            _uniformsDirty = true;
        }

        /// <summary>
        /// Uploads any pending uniform data and rebuilds stale pipeline / resource-set objects.
        /// Called automatically by the renderer before each draw call.
        /// </summary>
        public void Flush()
        {
            if (_pipelineDirty)
                RebuildPipeline();

            if (_uniformsDirty && UniformBuffer is not null)
            {
                _gd.UpdateBuffer(UniformBuffer, 0, _cpuUniformBuffer);
                _uniformsDirty = false;
            }

            if (_resourceSetDirty)
                RebuildResourceSet();
        }

        // IDisposable

        /// <summary>
        /// Disposes the shader and its resources.
        /// </summary>
        public void Dispose()
        {
            ResourceSet?.Dispose();
            UniformBuffer?.Dispose();
            _resourceLayout?.Dispose();

            foreach (var pipeline in _pipelineCache.Values)
                pipeline.Dispose();
            _pipelineCache.Clear();

            if (Shaders is not null)
                foreach (var shader in Shaders)
                    shader.Dispose();
        }
    }
}