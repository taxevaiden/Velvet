using Veldrid;

namespace Velvet.Graphics.Textures
{
    public enum SampleCount
    {
        Count1  = TextureSampleCount.Count1,
        Count2  = TextureSampleCount.Count2,
        Count4  = TextureSampleCount.Count4,
        Count8  = TextureSampleCount.Count8,
        Count16 = TextureSampleCount.Count16,
        Count32 = TextureSampleCount.Count32,
    }

    public class VelvetRenderTexture : IDisposable
    {
        // The texture that the framebuffer renders into.
        // For MSAA this is the multi-sampled texture; for non-MSAA it IS Texture.
        private readonly VelvetTexture _mainTexture;

        /// <summary>
        /// The resolved (single-sample) texture, safe to sample in a shader.
        /// For non-MSAA render textures this is the same object as the backing texture.
        /// </summary>
        public VelvetTexture Texture { get; }

        internal Framebuffer Framebuffer { get; }

        public uint        Width         => Texture.Width;
        public uint        Height        => Texture.Height;
        public SampleCount SampleCount   { get; }
        public bool        IsMultiSampled => SampleCount != SampleCount.Count1;

        private readonly GraphicsDevice _gd;

        // Construction

        /// <summary>
        /// Creates a render texture.
        /// </summary>
        /// <param name="renderer">The owning renderer.</param>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        /// <param name="sampleCount">MSAA sample count (default: no MSAA).</param>
        public VelvetRenderTexture(
            VelvetRenderer renderer,
            uint           width,
            uint           height,
            SampleCount    sampleCount = SampleCount.Count1)
        {
            _gd         = renderer._graphicsDevice;
            SampleCount = sampleCount;

            // The framebuffer always draws into _mainTexture.
            _mainTexture = new VelvetTexture(renderer, width, height, sampleCount);

            var fbDesc = new FramebufferDescription(depthTarget: null, _mainTexture.DeviceTexture);
            Framebuffer = _gd.ResourceFactory.CreateFramebuffer(ref fbDesc);

            // For MSAA we need a separate single-sample texture to resolve into
            // before it can be sampled. For non-MSAA _mainTexture is already
            // single-sample so we reuse it directly (no extra allocation).
            Texture = IsMultiSampled
                ? new VelvetTexture(renderer, width, height, SampleCount.Count1)
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