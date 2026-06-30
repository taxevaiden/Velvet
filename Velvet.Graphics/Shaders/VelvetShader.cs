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
    /// A description of a shader uniform, including its name and type.
    /// </summary>
    /// <param name="Name">The name of the uniform.</param>
    /// <param name="Type">The type of the uniform.</param>
    public readonly record struct UniformDescription(
        string Name,
        UniformType Type);

    // Internal layout type

    internal readonly record struct PackedUniform(
        string Name,
        UniformType Type,
        uint Offset,
        uint Size);

    // VelvetShader

    /// <summary>
    /// A shader that can be used for rendering.
    /// </summary>
    public sealed class VelvetShader : IDisposable
    {
        // Pipeline cache key: record struct gives structural Equals/GetHashCode for free.
        private readonly record struct PipelineKey(OutputDescription Outputs);

        // Default GLSL sources

        private const string DefaultVertexCode = """
            #version 450

            layout(location = 0) in vec3 Position;
            layout(location = 1) in vec2 UV;
            layout(location = 2) in vec4 Color;

            layout(location = 0) out vec2 fsin_UV;
            layout(location = 1) out vec4 fsin_Color;

            layout(set = 0, binding = 2) uniform Globals
            {
                mat4 Projection;
            };

            void main()
            {
                gl_Position = Projection * vec4(Position, 1.0);
                fsin_UV = UV;
                fsin_Color = Color;
            }
            """;

        private const string DefaultFragmentCode = """
            #version 450

            layout(location = 0) in vec2 fsin_UV;
            layout(location = 1) in vec4 fsin_Color;

            layout(location = 0) out vec4 fsout_Color;

            layout(set = 0, binding = 0) uniform texture2D Texture0;
            layout(set = 0, binding = 1) uniform sampler Sampler0;

            void main()
            {
                fsout_Color = texture(sampler2D(Texture0, Sampler0), fsin_UV) * fsin_Color;
            }
            """;

        // Veldrid objects

        internal Pipeline Pipeline = null!;
        internal Shader[] Shaders = null!;
        internal ResourceSet ResourceSet = null!;
        internal DeviceBuffer? GlobalsBuffer;
        internal DeviceBuffer? UniformBuffer;

        private readonly GraphicsDevice _gd;
        private readonly Dictionary<PipelineKey, Pipeline> _pipelineCache = new();
        private readonly Dictionary<string, PackedUniform> _uniforms = new();

        private ResourceLayout _resourceLayout = null!;
        private VelvetTexture _texture = null!;

        private byte[] _cpuGlobalsBuffer = Array.Empty<byte>();
        private byte[] _cpuUniformBuffer = Array.Empty<byte>();
        private OutputDescription _currentOutputs;
        private VelvetRenderTexture? _currentRenderTarget;

        private bool _globalsDirty;
        private bool _uniformsDirty;
        private bool _resourceSetDirty;
        private bool _pipelineDirty;

        // Construction

        /// <summary>
        /// Creates a new <see cref="VelvetShader"/>. In your shader, uniforms will be defined in a uniform block.
        /// </summary>
        /// <param name="renderer">The owning renderer.</param>
        /// <param name="vertPath">Path to a SPIR-V vertex shader, or <c>null</c> for the built-in default.</param>
        /// <param name="fragPath">Path to a SPIR-V fragment shader, or <c>null</c> for the built-in default.</param>
        /// <param name="uniforms">Optional uniform layout. Pass <c>null</c> if the shader has no custom uniforms.</param>
        public VelvetShader(
            VelvetRenderer renderer,
            string? vertPath,
            string? fragPath,
            UniformDescription[]? uniforms = null)
        {
            _gd = renderer._graphicsDevice;
            _texture = renderer.CurrentTexture;
            Init(vertPath, fragPath, uniforms);
        }

        // Uniform size / alignment helpers

        private static uint Align(uint value, uint alignment)
            => (value + alignment - 1) & ~(alignment - 1);

        private static uint GetUniformSize(UniformType type) => type switch
        {
            UniformType.Float => 4,
            UniformType.Int => 4,
            UniformType.UInt => 4,
            UniformType.Vector2 => 8,
            UniformType.Vector3 => 16,  // std140 vec3 = 16 bytes
            UniformType.Vector4 => 16,
            UniformType.Matrix4x4 => 64,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        private static string GetGlslType(UniformType type) => type switch
        {
            UniformType.Float => "float",
            UniformType.Int => "int",
            UniformType.UInt => "uint",
            UniformType.Vector2 => "vec2",
            UniformType.Vector3 => "vec3",
            UniformType.Vector4 => "vec4",
            UniformType.Matrix4x4 => "mat4",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        private static string BuildUniformBlock(string blockName, uint binding, IEnumerable<PackedUniform> uniforms)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"layout(set = 0, binding = {binding}) uniform {blockName}");
            builder.AppendLine("{");
            bool hasMembers = false;
            foreach (var u in uniforms)
            {
                builder.AppendLine($"    {GetGlslType(u.Type)} {u.Name};");
                hasMembers = true;
            }

            if (!hasMembers)
            {
                builder.AppendLine("    vec4 _Dummy;");
            }

            builder.AppendLine("};");
            return builder.ToString();
        }

        private static string BuildDefaultVertexCode(string vertexUniformBlock)
        {
            return $$"""
            #version 450

            layout(location = 0) in vec3 Position;
            layout(location = 1) in vec2 UV;
            layout(location = 2) in vec4 Color;

            layout(location = 0) out vec2 fsin_UV;
            layout(location = 1) out vec4 fsin_Color;

            layout(set = 0, binding = 2) uniform Globals
            {
                mat4 Projection;
            };

            {{vertexUniformBlock}}

            void main()
            {
                gl_Position = Projection * vec4(Position, 1.0);
                fsin_UV = UV;
                fsin_Color = Color;
            }
            """;
        }

        private static string BuildDefaultFragmentCode(string fragmentUniformBlock)
        {
            return $$"""
            #version 450

            layout(location = 0) in vec2 fsin_UV;
            layout(location = 1) in vec4 fsin_Color;

            layout(location = 0) out vec4 fsout_Color;

            layout(set = 0, binding = 0) uniform texture2D Texture0;
            layout(set = 0, binding = 1) uniform sampler Sampler0;

            layout(set = 0, binding = 2) uniform Globals
            {
                mat4 Projection;
            };

            {{fragmentUniformBlock}}

            void main()
            {
                fsout_Color = texture(sampler2D(Texture0, Sampler0), fsin_UV) * fsin_Color;
            }
            """;
        }

        // Initialization

        private void Init(
            string? vertPath,
            string? fragPath,
            UniformDescription[]? uniformDescriptions)
        {
            // Build resource layout elements (texture + sampler always present)
            var layoutElements = new List<ResourceLayoutElementDescription>
            {
                new("Texture0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new("Sampler0", ResourceKind.Sampler,         ShaderStages.Fragment),
            };

            // Reserve the first 64 bytes for the built-in Globals.Projection matrix
            uint globalsSize = 64;
            uint uniformTotalSize = 0;
            var userUniforms = new List<PackedUniform>();

            foreach (var u in uniformDescriptions ?? Array.Empty<UniformDescription>())
            {
                uint size = GetUniformSize(u.Type);
                uint offset = Align(uniformTotalSize, size);
                var packed = new PackedUniform(u.Name, u.Type, offset, size);
                _uniforms[u.Name] = packed;
                userUniforms.Add(packed);
                uniformTotalSize = offset + size;
            }

            uint uniformBufferSize = Math.Max(Align(uniformTotalSize, 16), 16u);

            _cpuGlobalsBuffer = new byte[globalsSize];
            _cpuUniformBuffer = new byte[uniformBufferSize];

            GlobalsBuffer = _gd.ResourceFactory.CreateBuffer(
                new BufferDescription(globalsSize, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            if (userUniforms.Count > 0)
            {
                UniformBuffer = _gd.ResourceFactory.CreateBuffer(
                    new BufferDescription(uniformBufferSize, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            }

            layoutElements.Add(new ResourceLayoutElementDescription(
                "Globals",
                ResourceKind.UniformBuffer,
                ShaderStages.Vertex | ShaderStages.Fragment));

            if (userUniforms.Count > 0)
            {
                layoutElements.Add(new ResourceLayoutElementDescription(
                    "Uniforms",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex | ShaderStages.Fragment));
            }

            _resourceLayout = _gd.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(layoutElements.ToArray()));

            // Compile shaders
            byte[] vertBytes;
            byte[] fragBytes;

            if (vertPath is null)
            {
                string uniformBlock = userUniforms.Count > 0
                    ? BuildUniformBlock("Uniforms", 3, userUniforms)
                    : string.Empty;
                vertBytes = Encoding.UTF8.GetBytes(BuildDefaultVertexCode(uniformBlock));
            }
            else
            {
                vertBytes = File.ReadAllBytes(vertPath);
            }

            if (fragPath is null)
            {
                string uniformBlock = userUniforms.Count > 0
                    ? BuildUniformBlock("Uniforms", 3, userUniforms)
                    : string.Empty;
                fragBytes = Encoding.UTF8.GetBytes(BuildDefaultFragmentCode(uniformBlock));
            }
            else
            {
                fragBytes = File.ReadAllBytes(fragPath);
            }

            Shaders = _gd.ResourceFactory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, vertBytes, "main"),
                new ShaderDescription(ShaderStages.Fragment, fragBytes, "main"));

            _currentOutputs = _gd.SwapchainFramebuffer.OutputDescription;

            RebuildResourceSet();
            RebuildPipeline();
        }

        // Rebuild helpers

        private void RebuildResourceSet()
        {
            ResourceSet?.Dispose();

            var resources = new List<IBindableResource> { _texture.View, _texture.Sampler, GlobalsBuffer! };
            if (UniformBuffer is not null)
                resources.Add(UniformBuffer);

            ResourceSet = _gd.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(_resourceLayout, resources.ToArray()));

            _resourceSetDirty = false;
        }

        private void RebuildPipeline()
        {
            var key = new PipelineKey(_currentOutputs);
            if (_pipelineCache.TryGetValue(key, out var cached))
            {
                Pipeline = cached;
                _pipelineDirty = false;
                return;
            }

            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4Norm));

            var pipeline = _gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SINGLE_ALPHA_BLEND,
                DepthStencilState = DepthStencilStateDescription.DISABLED,
                RasterizerState = new RasterizerStateDescription(
                    FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.CounterClockwise,
                    depthClipEnabled: true, scissorTestEnabled: false),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new[] { _resourceLayout },
                ShaderSet = new ShaderSetDescription(new[] { vertexLayout }, Shaders),
                Outputs = _currentOutputs
            });

            _pipelineCache[key] = pipeline;
            Pipeline = pipeline;
            _pipelineDirty = false;
        }

        // Public uniform setters

        /// <summary>Sets a uniform value by name.</summary>
        public void Set(string name, float value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, int value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, uint value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, Vector2 value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, Vector3 value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, Vector4 value) => Write(name, value);
        /// <inheritdoc cref="Set(string,float)"/>
        public void Set(string name, Matrix4x4 value) => Write(name, value);

        // Internal state setters (called by the renderer)

        internal void SetTexture(VelvetTexture texture)
        {
            if (_texture == texture) return;
            _texture = texture;
            _resourceSetDirty = true;
        }

        internal void SetRenderTexture(VelvetRenderTexture? rt)
        {
            var newOutputs = rt?.Framebuffer.OutputDescription
                             ?? _gd.SwapchainFramebuffer.OutputDescription;

            if (_currentRenderTarget == rt && _currentOutputs.Equals(newOutputs)) return;

            _currentRenderTarget = rt;
            _currentOutputs = newOutputs;
            _pipelineDirty = true;
        }

        internal unsafe void SetProjection(Matrix4x4 projection)
        {
            if (GlobalsBuffer is null)
                return;

            fixed (byte* dst = &_cpuGlobalsBuffer[0])
                *(Matrix4x4*)dst = projection;

            _globalsDirty = true;
        }

        // Write / flush

        private unsafe void Write<T>(string name, T value) where T : unmanaged
        {
            if (!_uniforms.TryGetValue(name, out var u))
                throw new InvalidOperationException($"Uniform '{name}' is not defined in this shader.");

            if (UniformBuffer is null)
                throw new InvalidOperationException("Uniform buffer is not available for this shader.");

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

            if (_globalsDirty && GlobalsBuffer is not null)
            {
                _gd.UpdateBuffer(GlobalsBuffer, 0, _cpuGlobalsBuffer);
                _globalsDirty = false;
            }

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
            GlobalsBuffer?.Dispose();
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