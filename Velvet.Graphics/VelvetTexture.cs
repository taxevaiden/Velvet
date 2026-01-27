using System.Runtime.CompilerServices;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace Velvet.Graphics
{
    public class VelvetTexture : IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<VelvetTexture>();
        internal Texture DeviceTexture = null!;
        internal TextureView View = null!;
        internal Sampler Sampler = null!;

        public uint Width { get; private set;}
        public uint Height { get; private set;}
        public bool IsMultiSampled { get; private set; }
        public bool FromRenderTexture { get; private set; }

        /// <summary>
        /// Creates a new VelvetTexture.
        /// </summary>
        /// <param name="renderer">The VelvetRenderer to use.</param>
        /// <param name="imageFilePath">The file path to your image that will be used for the VelvetTexture.</param>
        public VelvetTexture(VelvetRenderer renderer, string imageFilePath)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(imageFilePath);
            Width = (uint)image.Width;
            Height = (uint)image.Height;

            byte[] pixelBytes = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelBytes);

            InitTexture(renderer, pixelBytes, (uint)image.Width, (uint)image.Height);
        }

        /// <summary>
        /// Creates a new VelvetTexture. 
        /// </summary>
        /// <param name="renderer">The VelvetRenderer to use.</param>
        /// <param name="imageData">The image data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        public VelvetTexture(VelvetRenderer renderer, byte[] imageData, uint width, uint height)
        {
            InitTexture(renderer, imageData, width, height);
        }

        internal VelvetTexture(VelvetRenderer renderer, uint width, uint height, SampleCount sampleCount)
        {
            InitTexture(renderer, width, height, sampleCount);
        }

        private void InitTexture(VelvetRenderer renderer, byte[] imageData, uint width, uint height)
        {
            Width = width;
            Height = height;

            IsMultiSampled = false;
            FromRenderTexture = false;

            uint mipLevels = (uint)(MathF.Floor(MathF.Log2(Math.Max(width, height))) + 1);

            var desc = TextureDescription.Texture2D(
                width,
                height,
                mipLevels: mipLevels,
                arrayLayers: 1,
                format: PixelFormat.R8G8B8A8UNorm,
                usage: TextureUsage.RenderTarget | TextureUsage.Sampled | TextureUsage.GenerateMipmaps
            );

            DeviceTexture = renderer._graphicsDevice.ResourceFactory.CreateTexture(ref desc);

            renderer._graphicsDevice.UpdateTexture<byte>(
                DeviceTexture,
                imageData,
                0, 0, 0,
                DeviceTexture.Width, DeviceTexture.Height, DeviceTexture.Depth,
                0,
                0
            );

            View = renderer._graphicsDevice.ResourceFactory.CreateTextureView(DeviceTexture);
            Sampler = renderer._graphicsDevice.ResourceFactory.CreateSampler(
                new SamplerDescription(
                    SamplerAddressMode.Clamp,
                    SamplerAddressMode.Clamp,
                    SamplerAddressMode.Clamp,
                    SamplerFilter.MinLinearMagLinearMipLinear,
                    null,
                    1,
                    0,
                    mipLevels,
                    0,
                    SamplerBorderColor.TransparentBlack
                )
            );

            _logger.Information($"(Window-{renderer._window.windowID}): Texture loaded:");
            _logger.Information($"(Window-{renderer._window.windowID}): > Width: {width}");
            _logger.Information($"(Window-{renderer._window.windowID}): > Height: {height}");
            _logger.Information($"(Window-{renderer._window.windowID}): > MipLevels: {mipLevels}");
            _logger.Information($"(Window-{renderer._window.windowID}): > Size In Bytes: {imageData.Length} bytes ({imageData.Length / 1024} KB, {imageData.Length / (1024 * 1024)} MB)");
        }

        // This method is used for Render Textures
        private void InitTexture(VelvetRenderer renderer, uint width, uint height, SampleCount sampleCount)
        {
            Width = width;
            Height = height;

            FromRenderTexture = true;

            switch (sampleCount) 
            {
                case SampleCount.Count1: IsMultiSampled = false; break; 
                default: IsMultiSampled = true; break; 
            }

            uint mipLevels = (uint)(MathF.Floor(MathF.Log2(Math.Max(width, height))) + 1);

            var desc = TextureDescription.Texture2D(
                width,
                height,
                mipLevels: mipLevels,
                arrayLayers: 1,
                format: PixelFormat.R8G8B8A8UNorm,
                usage: TextureUsage.RenderTarget | TextureUsage.Sampled | TextureUsage.GenerateMipmaps,
                (TextureSampleCount)sampleCount
            );

            DeviceTexture = renderer._graphicsDevice.ResourceFactory.CreateTexture(ref desc);

            View = renderer._graphicsDevice.ResourceFactory.CreateTextureView(DeviceTexture);
            Sampler = renderer._graphicsDevice.ResourceFactory.CreateSampler(
                new SamplerDescription(
                    SamplerAddressMode.Clamp,
                    SamplerAddressMode.Clamp,
                    SamplerAddressMode.Clamp,
                    SamplerFilter.MinLinearMagLinearMipLinear,
                    null,
                    1,
                    0,
                    mipLevels,
                    0,
                    SamplerBorderColor.TransparentBlack
                )
            );

            _logger.Information($"(Window-{renderer._window.windowID}): Render Texture loaded:");
            _logger.Information($"(Window-{renderer._window.windowID}): > Width: {width}");
            _logger.Information($"(Window-{renderer._window.windowID}): > Height: {height}");
            _logger.Information($"(Window-{renderer._window.windowID}): > MipLevels: {mipLevels}");
        }

        public void Dispose()
        {
            View.Dispose();
            Sampler.Dispose();
            DeviceTexture.Dispose();
        }
    }
}