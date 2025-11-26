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
        internal ResourceSet ResourceSet;
        public Texture DeviceTexture { get; }
        public TextureView View { get; }
        public Sampler Sampler { get; }

        public uint Width { get; }
        public uint Height { get; }

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

            var desc = TextureDescription.Texture2D(
                (uint)image.Width,
                (uint)image.Height,
                mipLevels: 1,
                arrayLayers: 1,
                format: PixelFormat.R8G8B8A8UNorm,
                usage: TextureUsage.Sampled
            );

            DeviceTexture = renderer._graphicsDevice.ResourceFactory.CreateTexture(ref desc);

            renderer._graphicsDevice.UpdateTexture<byte>(
                DeviceTexture,
                pixelBytes,
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
                    0,
                    0,
                    SamplerBorderColor.TransparentBlack
                )
            );

            ResourceLayout resourceLayout = renderer._graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture2D", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )
            );

            ResourceSet = renderer._graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayout,
                [
                    View,
                    Sampler
                ]
            ));

            _logger.Information($"(Window-{renderer._window.windowID}): Texture Loaded:");
            _logger.Information($"(Window-{renderer._window.windowID}): > Width: {image.Width}");
            _logger.Information($"(Window-{renderer._window.windowID}): > Height: {image.Height}");
            _logger.Information($"(Window-{renderer._window.windowID}): > File Path: {imageFilePath}");
            _logger.Information($"(Window-{renderer._window.windowID}): > Size In Bytes: {pixelBytes.Length} bytes ({pixelBytes.Length / 1024} KB, {pixelBytes.Length / (1024 * 1024)} MB)");
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
                    0,
                    0,
                    SamplerBorderColor.TransparentBlack
                )
            );

            ResourceLayout resourceLayout = renderer._graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture2D", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )
            );

            ResourceSet = renderer._graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayout,
                [
                    View,
                    Sampler
                ]
            ));

            _logger.Information($"(Window-{renderer._window.windowID}): Texture Loaded:");
            _logger.Information($"(Window-{renderer._window.windowID}): > Width: {width}");
            _logger.Information($"(Window-{renderer._window.windowID}): > Height: {height}");
            _logger.Information($"(Window-{renderer._window.windowID}): > Size In Bytes: {imageData.Length} bytes ({imageData.Length / 1024} KB, {imageData.Length / (1024 * 1024)} MB)");
        }

        // Use in Renderer class

        internal VelvetTexture(GraphicsDevice gd, string imageFilePath)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(imageFilePath);
            Width = (uint)image.Width;
            Height = (uint)image.Height;

            byte[] pixelBytes = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelBytes);

            var desc = TextureDescription.Texture2D(
                (uint)image.Width,
                (uint)image.Height,
                mipLevels: 1,
                arrayLayers: 1,
                format: PixelFormat.R8G8B8A8UNorm,
                usage: TextureUsage.Sampled
            );

            DeviceTexture = gd.ResourceFactory.CreateTexture(ref desc);

            gd.UpdateTexture<byte>(
                DeviceTexture,
                pixelBytes,
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
                    new ResourceLayoutElementDescription("Texture2D", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )
            );

            ResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayout,
                [
                    View,
                    Sampler
                ]
            ));

            _logger.Information("(Within Renderer): Texture Loaded:");
            _logger.Information($"(Within Renderer): > Width: {image.Width}");
            _logger.Information($"(Within Renderer): > Height: {image.Height}");
            _logger.Information($"(Within Renderer): > File Path: {imageFilePath}");
            _logger.Information($"(Within Renderer): > Size In Bytes: {pixelBytes.Length} bytes ({pixelBytes.Length / 1024} KB, {pixelBytes.Length / (1024 * 1024)} MB)");
        }

        internal VelvetTexture(GraphicsDevice gd, byte[] imageData, uint width, uint height)
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
                    new ResourceLayoutElementDescription("Texture2D", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )
            );

            ResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayout,
                [
                    View,
                    Sampler
                ]
            ));


            _logger.Information("(Within Renderer): Texture Loaded:");
            _logger.Information($"(Within Renderer): > Width: {width}");
            _logger.Information($"(Within Renderer): > Height: {height}");
            _logger.Information($"(Within Renderer): > SizeInBytes: {imageData.Length} bytes ({imageData.Length / 1024} KB, {imageData.Length / (1024 * 1024)} MB)");
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