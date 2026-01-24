using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Serilog;
using Veldrid;
using Veldrid.SPIRV;

namespace Velvet.Graphics
{
    public enum UniformType
    {
        Float,
        Int,
        UInt,
        Vector2,
        Vector3,
        Vector4,
        Matrix4x4
    }

    public enum UniformStage
    {
        Vertex = ShaderStages.Vertex,
        Fragment = ShaderStages.Fragment,
        Both = ShaderStages.Vertex | ShaderStages.Fragment
    }

    public struct UniformDescription
    {
        public string Name;
        public UniformType Type;
        public UniformStage Stage;

        public UniformDescription(string name, UniformType type, UniformStage stage)
        {
            Name = name;
            Type = type;
            Stage = stage;
        }
    }

    internal struct PackedUniform
    {
        public string Name;
        public UniformType Type;
        public uint Offset;
        public uint Size;
    }

    public sealed class VelvetShader : IDisposable
    {
        private const string DefaultVertexCode = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 UV;
layout(location = 2) in vec4 Color;

layout(location = 0) out vec2 fsin_UV;
layout(location = 1) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0.0, 1.0);
    fsin_UV = UV;
    fsin_Color = Color;
}";

        private const string DefaultFragmentCode = @"
#version 450

layout(location = 0) in vec2 fsin_UV;
layout(location = 1) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

layout(set = 0, binding = 0) uniform texture2D Texture0;
layout(set = 0, binding = 1) uniform sampler Sampler0;

void main()
{
    vec4 color = texture(sampler2D(Texture0, Sampler0), fsin_UV);
    fsout_Color = color * fsin_Color;
}";

        internal Pipeline Pipeline = null!;
        internal Shader[] Shaders = null!;
        internal ResourceSet ResourceSet = null!;
        internal DeviceBuffer UniformBuffer = null!;

        private readonly ILogger _logger = Log.ForContext<VelvetShader>();
        private readonly Dictionary<string, PackedUniform> _uniforms = new();

        private byte[] _cpuUniformBuffer = Array.Empty<byte>();
        private bool _uniformsDirty;
        private bool _resourceSetDirty;

        private VelvetTexture _texture = null!;
        private ResourceLayout _resourceLayout = null!;

        private GraphicsDevice _gd = null!;
        private VelvetRenderer _renderer = null!;

        public VelvetShader(
            VelvetRenderer renderer,
            string? vertPath,
            string? fragPath,
            UniformDescription[]? uniforms = null)
        {
            _renderer = renderer;
            _gd = renderer._graphicsDevice;
            _texture = renderer.CurrentTexture;

            Init(vertPath, fragPath, uniforms);
        }

        /* ================= UNIFORM PACKING ================= */

        private static uint Align(uint value, uint alignment)
            => (value + alignment - 1) & ~(alignment - 1);

        private static uint GetUniformSize(UniformType type) => type switch
        {
            UniformType.Float => 4,
            UniformType.Int => 4,
            UniformType.UInt => 4,
            UniformType.Vector2 => 8,
            UniformType.Vector3 => 16,
            UniformType.Vector4 => 16,
            UniformType.Matrix4x4 => 64,
            _ => throw new ArgumentOutOfRangeException()
        };

        /* ================= INIT ================= */

        private void Init(
            string? vertPath,
            string? fragPath,
            UniformDescription[]? uniformDescriptions)
        {
            var layoutElements = new List<ResourceLayoutElementDescription>
            {
                new("Texture0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new("Sampler0", ResourceKind.Sampler, ShaderStages.Fragment)
            };

            if (uniformDescriptions != null && uniformDescriptions.Length > 0)
            {
                uint offset = 0;

                foreach (var u in uniformDescriptions)
                {
                    uint size = GetUniformSize(u.Type);
                    offset = Align(offset, size);

                    _uniforms[u.Name] = new PackedUniform
                    {
                        Name = u.Name,
                        Type = u.Type,
                        Offset = offset,
                        Size = size
                    };

                    offset += size;
                }

                uint totalSize = Align(offset, 16);
                _cpuUniformBuffer = new byte[totalSize];

                UniformBuffer = _gd.ResourceFactory.CreateBuffer(
                    new BufferDescription(
                        totalSize,
                        BufferUsage.UniformBuffer | BufferUsage.Dynamic
                    )
                );

                layoutElements.Add(
                    new ResourceLayoutElementDescription(
                        "Globals",
                        ResourceKind.UniformBuffer,
                        ShaderStages.Vertex | ShaderStages.Fragment
                    )
                );
            }

            _resourceLayout = _gd.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(layoutElements.ToArray())
            );

            RebuildResourceSet();

            var vsDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(vertPath == null ? DefaultVertexCode : File.ReadAllText(vertPath)),
                "main");

            var fsDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(fragPath == null ? DefaultFragmentCode : File.ReadAllText(fragPath)),
                "main");

            Shaders = _gd.ResourceFactory.CreateFromSpirv(vsDesc, fsDesc);

            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
            );

            Pipeline = _gd.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription
                {
                    BlendState = BlendStateDescription.SINGLE_OVERRIDE_BLEND,
                    DepthStencilState = DepthStencilStateDescription.DISABLED,
                    RasterizerState = new RasterizerStateDescription(
                        FaceCullMode.Back,
                        PolygonFillMode.Solid,
                        FrontFace.CounterClockwise,
                        true,
                        false
                    ),
                    PrimitiveTopology = PrimitiveTopology.TriangleList,
                    ResourceLayouts = new[] { _resourceLayout },
                    ShaderSet = new ShaderSetDescription(
                        new[] { vertexLayout },
                        Shaders),
                    Outputs = _renderer._graphicsDevice.SwapchainFramebuffer.OutputDescription
                }
            );
        }

        private void RebuildResourceSet()
        {
            ResourceSet?.Dispose();

            if (UniformBuffer != null)
            {
                ResourceSet = _gd.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        _resourceLayout,
                        _texture.View,
                        _texture.Sampler,
                        UniformBuffer
                    )
                );
            }
            else
            {
                ResourceSet = _gd.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        _resourceLayout,
                        _texture.View,
                        _texture.Sampler
                    )
                );
            }

            _resourceSetDirty = false;
        }

        /* ================= SET API ================= */

        public void Set(string name, float value) => Write(name, value);
        public void Set(string name, int value) => Write(name, value);
        public void Set(string name, uint value) => Write(name, value);
        public void Set(string name, Vector2 value) => Write(name, value);
        public void Set(string name, Vector3 value) => Write(name, value);
        public void Set(string name, Vector4 value) => Write(name, value);
        public void Set(string name, Matrix4x4 value) => Write(name, value);

        internal void SetTexture(VelvetTexture texture)
        {
            if (_texture == texture)
                return;

            _texture = texture;
            _resourceSetDirty = true;
        }

        private unsafe void Write<T>(string name, T value) where T : unmanaged
        {
            if (!_uniforms.TryGetValue(name, out var u))
                throw new InvalidOperationException($"Uniform '{name}' not defined.");

            fixed (byte* dst = &_cpuUniformBuffer[u.Offset])
            {
                *(T*)dst = value;
            }

            _uniformsDirty = true;
        }

        public void Flush()
        {
            if (_uniformsDirty)
            {
                _gd.UpdateBuffer(UniformBuffer, 0, _cpuUniformBuffer);
                _uniformsDirty = false;
            }

            if (_resourceSetDirty)
                RebuildResourceSet();
        }

        public void Dispose()
        {
            UniformBuffer?.Dispose();
            ResourceSet?.Dispose();
            Pipeline?.Dispose();

            if (Shaders != null)
            {
                foreach (var s in Shaders)
                    s.Dispose();
            }
        }
    }
}
