using Serilog;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Veldrid;

namespace Velvet.Graphics.Textures
{
    /// <summary>
    /// A texture is an image that can be drawn to the screen. It can be loaded from a file or created from raw pixel data. A texture can also be used as a render target (see <see cref="VelvetRenderTexture"/>).
    /// </summary>
    public class VelvetTexture : IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<VelvetTexture>();

        internal Texture DeviceTexture = null!;
        internal TextureView View = null!;
        internal Sampler Sampler = null!;

        /// <summary>
        /// The width of the texture in pixels. This is determined at initialization and cannot be changed. To use a different size, create a new instance of VelvetTexture with the desired dimensions.
        /// </summary>
        public uint Width { get; private set; }
        /// <summary>
        /// The height of the texture in pixels. This is determined at initialization and cannot be changed. To use a different size, create a new instance of VelvetTexture with the desired dimensions.
        /// </summary>
        public uint Height { get; private set; }
        /// <summary>
        /// Whether this texture is multi-sampled (MSAA). This is true for the backing texture of a multi-sampled render texture, and false for non-MSAA render textures and textures loaded from files or created from pixel data.
        /// </summary>
        public bool IsMultiSampled { get; private set; }
        /// <summary>
        /// Whether this texture was created from a render texture. This is true for the backing texture of a render texture, and false for textures loaded from files or created from pixel data. This can be used to determine whether the texture can be used as a render target (false) or is already a render target (true).
        /// </summary>
        public bool FromRenderTexture { get; private set; }

        /// <summary>
        /// Whether mipmaps have been generated for this texture this frame. This is used to prevent redundant mipmap generation calls. Mipmaps are generated on demand when <see cref="GenerateMipMapsIfNeeded"/> is called, and the renderer resets this flag each <c>End()</c> call.
        /// Mipmaps are only supported for non-multi-sampled textures, so this will always be false for MSAA render texture backing textures.
        /// </summary>
        public bool MipMapsGenerated { get; set; } = false;
        /// <summary>
        /// Whether this texture supports mipmaps. This is true for textures loaded from files or created from pixel data, and false for multi-sampled render texture backing textures (which cannot have mipmaps). This can be used to determine whether mipmaps can be generated for this texture.
        /// </summary>
        public bool SupportsMipMaps { get; private set; } = false;

        // Constructors

        /// <summary>Loads a texture from a file.</summary>
        public VelvetTexture(VelvetRenderer renderer, string imageFilePath)
        {
            using var image = Image.Load<Rgba32>(imageFilePath);
            Width = (uint)image.Width;
            Height = (uint)image.Height;

            byte[] pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            InitFromPixels(renderer, pixels, Width, Height);
        }

        /// <summary>Creates a texture from raw RGBA pixel data.</summary>
        public VelvetTexture(VelvetRenderer renderer, byte[] imageData, uint width, uint height)
        {
            InitFromPixels(renderer, imageData, width, height);
        }

        /// <summary>Internal constructor for render-texture backing textures.</summary>
        internal VelvetTexture(VelvetRenderer renderer, uint width, uint height, SampleCount sampleCount)
        {
            InitForRenderTexture(renderer, width, height, sampleCount);
        }

        // Initialization

        private void InitFromPixels(VelvetRenderer renderer, byte[] pixels, uint width, uint height)
        {
            Width = width;
            Height = height;
            IsMultiSampled = false;
            FromRenderTexture = false;
            SupportsMipMaps = true;

            uint mipLevels = CalcMipLevels(width, height);

            var desc = TextureDescription.Texture2D(
                width, height,
                mipLevels: mipLevels,
                arrayLayers: 1,
                format: PixelFormat.R8G8B8A8UNorm,
                usage: TextureUsage.RenderTarget | TextureUsage.Sampled | TextureUsage.GenerateMipmaps);

            var gd = renderer._graphicsDevice;
            DeviceTexture = gd.ResourceFactory.CreateTexture(ref desc);

            gd.UpdateTexture(
                DeviceTexture, pixels,
                x: 0, y: 0, z: 0,
                width: DeviceTexture.Width, height: DeviceTexture.Height, depth: DeviceTexture.Depth,
                mipLevel: 0, arrayLayer: 0);

            View = gd.ResourceFactory.CreateTextureView(DeviceTexture);
            Sampler = CreateSampler(gd, mipLevels);

            _logger.Information(
                "(Window-{WindowId}): Texture loaded: {W}x{H}, {Mips} mips, {Bytes} bytes",
                renderer._window.WindowID, width, height, mipLevels, pixels.Length);
        }

        private void InitForRenderTexture(VelvetRenderer renderer, uint width, uint height, SampleCount sampleCount)
        {
            Width = width;
            Height = height;
            FromRenderTexture = true;
            IsMultiSampled = sampleCount != SampleCount.Count1;

            // MSAA textures cannot have mipmaps
            bool multisampled = sampleCount != SampleCount.Count1;
            uint mipLevels = multisampled ? 1 : CalcMipLevels(width, height);
            SupportsMipMaps = !multisampled;

            TextureUsage usage = TextureUsage.RenderTarget | TextureUsage.Sampled;
            if (!multisampled) usage |= TextureUsage.GenerateMipmaps;

            var desc = TextureDescription.Texture2D(
                width, height,
                mipLevels: mipLevels,
                arrayLayers: 1,
                format: PixelFormat.R8G8B8A8UNorm,
                usage: usage,
                sampleCount: (TextureSampleCount)sampleCount);

            var gd = renderer._graphicsDevice;
            DeviceTexture = gd.ResourceFactory.CreateTexture(ref desc);
            View = gd.ResourceFactory.CreateTextureView(DeviceTexture);
            Sampler = CreateSampler(gd, mipLevels);

            _logger.Information(
                "(Window-{WindowId}): Render texture created: {W}x{H}, {Samples} samples, {Mips} mips",
                renderer._window.WindowID, width, height, sampleCount, mipLevels);
        }

        // Mip generation

        /// <summary>
        /// Generates mipmaps if supported and not yet done this frame.
        /// The renderer resets <see cref="MipMapsGenerated"/> each <c>End()</c> call.
        /// </summary>
        public void GenerateMipMapsIfNeeded(CommandList cl)
        {
            if (!SupportsMipMaps || MipMapsGenerated) return;
            cl.GenerateMipmaps(DeviceTexture);
            MipMapsGenerated = true;
        }

        // Helpers

        private static uint CalcMipLevels(uint width, uint height)
            => (uint)(MathF.Floor(MathF.Log2(Math.Max(width, height))) + 1);

        private static Sampler CreateSampler(GraphicsDevice gd, uint mipLevels)
            => gd.ResourceFactory.CreateSampler(new SamplerDescription(
                addressModeU: SamplerAddressMode.Clamp,
                addressModeV: SamplerAddressMode.Clamp,
                addressModeW: SamplerAddressMode.Clamp,
                filter: SamplerFilter.MinLinearMagLinearMipLinear,
                comparisonKind: null,
                maximumAnisotropy: 1,
                minimumLod: 0,
                maximumLod: mipLevels,
                lodBias: 0,
                borderColor: SamplerBorderColor.TransparentBlack));

        /// <summary>
        /// Disposes the texture and its resources.
        /// </summary>
        public void Dispose()
        {
            View?.Dispose();
            Sampler?.Dispose();
            DeviceTexture?.Dispose();
        }
    }
}