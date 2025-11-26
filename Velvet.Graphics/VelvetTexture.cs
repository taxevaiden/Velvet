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
        internal ResourceSet ResourceSet = null!;
        private Texture DeviceTexture = null!;
        private TextureView View = null!;
        private Sampler Sampler = null!;

        public uint Width { get; private set;}
        public uint Height { get; private set;}

        /// <summary>
        /// Creates a new VelvetTexture with the provided image path.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="imageFilePath"></param>
        public VelvetTexture(Renderer renderer, string imageFilePath)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(imageFilePath);
            Width = (uint)image.Width;
            Height = (uint)image.Height;

            byte[] pixelBytes = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelBytes);

            InitTexture(renderer._graphicsDevice, pixelBytes, (uint)image.Width, (uint)image.Height, renderer._window.windowID);
        }

        /// <summary>
        /// Creates a new VelvetTexture with the provided image data. 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="imageData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public VelvetTexture(Renderer renderer, byte[] imageData, uint width, uint height)
        {
            InitTexture(renderer._graphicsDevice, imageData, width, height, renderer._window.windowID);
        }

        // Use in Renderer class

        internal VelvetTexture(GraphicsDevice gd, string imageFilePath, uint windowID)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(imageFilePath);
            Width = (uint)image.Width;
            Height = (uint)image.Height;

            byte[] pixelBytes = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelBytes);

            InitTexture(gd, pixelBytes, (uint)image.Width, (uint)image.Height, windowID);
        }

        internal VelvetTexture(GraphicsDevice gd, byte[] imageData, uint width, uint height, uint windowID)
        {
            InitTexture(gd, imageData, width, height, windowID);
        }

        private void InitTexture(GraphicsDevice gd, byte[] imageData, uint width, uint height, uint windowID)
        {
            Width = width;
            Height = height;

            var desc = TextureDescription.Texture2D(
                width,
                height,
                mipLevels: 1,
                arrayLayers: 1,
                format: PixelFormat.R8G8B8A8UNorm,
                usage: TextureUsage.Sampled
            );

            DeviceTexture = gd.ResourceFactory.CreateTexture(ref desc);

            gd.UpdateTexture<byte>(
                DeviceTexture,
                imageData,
                0, 0, 0,
                DeviceTexture.Width, DeviceTexture.Height, DeviceTexture.Depth,
                0,
                0
            );

            View = gd.ResourceFactory.CreateTextureView(DeviceTexture);
            Sampler = gd.ResourceFactory.CreateSampler(
                new SamplerDescription(
                    SamplerAddressMode.Clamp,
                    SamplerAddressMode.Clamp,
                    SamplerAddressMode.Clamp,
                    SamplerFilter.MinLinearMagLinearMipLinear,
                    null,
                    1,
                    0,
                    0,
                    0,
                    SamplerBorderColor.TransparentBlack
                )
            );

            ResourceLayout resourceLayout = gd.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler0", ResourceKind.Sampler, ShaderStages.Fragment)
                )
            );

            ResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayout,
                [
                    View,
                    Sampler
                ]
            ));

            _logger.Information($"(Window-{windowID}): Texture loaded:");
            _logger.Information($"(Window-{windowID}): > Width: {width}");
            _logger.Information($"(Window-{windowID}): > Height: {height}");
            _logger.Information($"(Window-{windowID}): > Size In Bytes: {imageData.Length} bytes ({imageData.Length / 1024} KB, {imageData.Length / (1024 * 1024)} MB)");
        }

        public void Dispose()
        {
            ResourceSet.Dispose();
            View.Dispose();
            Sampler.Dispose();
            DeviceTexture.Dispose();
        }
    }
}