using Veldrid;

namespace Boist.Graphics.Textures
{
    /// <summary>
    /// A render texture is a texture that can be drawn to. This is useful for post-processing effects, dynamic textures, and more. A render texture can also be multi-sampled for anti-aliasing (MSAA).
    /// </summary>
    public class RenderTexture : IDisposable
    {
        // The texture that the framebuffer renders into.
        // For MSAA this is the multi-sampled texture; for non-MSAA it IS Texture.
        private readonly Texture _mainTexture;

        /// <summary>
        /// The resolved (single-sample) texture, safe to sample in a shader.
        /// For non-MSAA render textures this is the same object as the backing texture.
        /// </summary>
        public Texture Texture { get; }

        internal Framebuffer Framebuffer { get; }

        /// <summary>
        /// The width of the render texture in pixels. This is determined at initialization and cannot be changed. To use a different size, create a new instance of BoistRenderTexture with the desired dimensions.
        /// </summary>
        public uint Width => Texture.Width;
        /// <summary>
        /// The height of the render texture in pixels. This is determined at initialization and cannot be changed. To use a different size, create a new instance of BoistRenderTexture with the desired dimensions.
        /// </summary>
        public uint Height => Texture.Height;
        /// <summary>
        /// The MSAA sample count of the render texture. This is determined at initialization and cannot be changed. To use a different sample count, create a new instance of BoistRenderTexture with the desired sample count.
        /// </summary>
        public SampleCount SampleCount { get; }
        /// <summary>
        /// Whether this render texture is multi-sampled (MSAA) or not. This is determined by the SampleCount and is true if SampleCount is greater than Count1. Multi-sampled render textures require an explicit Resolve step after rendering and before sampling, but can produce smoother edges. Non-MSAA render textures can be sampled directly without resolving, but may have jagged edges.
        /// </summary>
        public bool IsMultiSampled => SampleCount != SampleCount.Count1;

        private readonly GraphicsDevice _gd;

        // Construction

        /// <summary>
        /// Creates a render texture.
        /// </summary>
        /// <param name="renderer">The owning renderer.</param>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        /// <param name="sampleCount">MSAA sample count (default: no MSAA).</param>
        public RenderTexture(
            Renderer renderer,
            uint width,
            uint height,
            SampleCount sampleCount = SampleCount.Count1)
        {
            _gd = renderer._graphicsDevice;
            SampleCount = sampleCount;

            // The framebuffer always draws into _mainTexture.
            _mainTexture = new Texture(renderer, width, height, sampleCount);

            var fbDesc = new FramebufferDescription(depthTarget: null, _mainTexture.DeviceTexture);
            Framebuffer = _gd.ResourceFactory.CreateFramebuffer(ref fbDesc);

            // For MSAA we need a separate single-sample texture to resolve into
            // before it can be sampled. For non-MSAA _mainTexture is already
            // single-sample so we reuse it directly (no extra allocation).
            Texture = IsMultiSampled
                ? new Texture(renderer, width, height, SampleCount.Count1)
                : _mainTexture;
        }

        // Internal API

        /// <summary>
        /// Resolves the MSAA framebuffer into the single-sample <see cref="Texture"/>.
        /// No-op for non-MSAA render textures.
        /// </summary>
        internal void Resolve(CommandList cl)
        {
            if (!IsMultiSampled) return;
            cl.ResolveTexture(_mainTexture.DeviceTexture, Texture.DeviceTexture);
        }

        // IDisposable


        /// <summary>
        /// Disposes the render texture and its resources.
        /// </summary>
        public void Dispose()
        {
            Framebuffer.Dispose();

            // Avoid double-dispose: for non-MSAA, Texture == _mainTexture.
            if (IsMultiSampled)
                Texture.Dispose();

            _mainTexture.Dispose();
        }
    }
}