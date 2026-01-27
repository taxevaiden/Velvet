using System.Runtime.CompilerServices;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace Velvet.Graphics
{
    public enum SampleCount
    {
        Count1 = TextureSampleCount.Count1,
        Count2 = TextureSampleCount.Count2,
        Count4 = TextureSampleCount.Count4,
        Count8 = TextureSampleCount.Count8,
        Count16 = TextureSampleCount.Count16,
        Count32 = TextureSampleCount.Count32
    }
    public class VelvetRenderTexture : IDisposable
    {
        public VelvetTexture MainTexture;
        public VelvetTexture Texture;
        internal Framebuffer Framebuffer;

        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public SampleCount SampleCount { get; private set; }
        public bool IsMultiSampled => SampleCount != SampleCount.Count1;

        private readonly GraphicsDevice _gd;

        /// <summary>
        /// Creates a new VelvetRenderTexture
        /// </summary>
        /// <param name="renderer">The VelvetRenderer to use.</param>
        /// <param name="width">The width of the VelvetRenderTexture.</param>
        /// <param name="height">The height of the VelvetRenderTexture.</param>
        /// <param name="sampleCount">The amount of samples for the VelvetRenderTexture.</param>
        public VelvetRenderTexture(VelvetRenderer renderer, uint width, uint height, SampleCount sampleCount = SampleCount.Count1)
        {
            _gd = renderer._graphicsDevice;
            Width = width;
            Height = height;
            SampleCount = sampleCount;

            MainTexture = new VelvetTexture(renderer, width, height, sampleCount);

            FramebufferDescription fbDesc = new FramebufferDescription(null, MainTexture.DeviceTexture);
            Framebuffer = _gd.ResourceFactory.CreateFramebuffer(ref fbDesc);

            if (IsMultiSampled)
            {
                Texture = new VelvetTexture(renderer, width, height, SampleCount.Count1);
            }
            else
            {
                Texture = MainTexture;
            }
        }

        /// <summary>
        /// Resolves MSAA framebuffer into single-sample texture.
        /// No-op if not multisampled.
        /// </summary>
        internal void Resolve(CommandList cl)
        {
            if (!IsMultiSampled) return;
            cl.ResolveTexture(MainTexture.DeviceTexture, Texture.DeviceTexture);
        }
        
        public void Dispose()
        {
            Framebuffer.Dispose();
            Texture.Dispose();
            MainTexture?.Dispose();
        }
    }
}