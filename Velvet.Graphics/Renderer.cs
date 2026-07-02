using System.Runtime.InteropServices;

namespace Velvet.Graphics
{
    /// <summary>
    /// Allows you to draw simple 2D primitives with <see cref="Textures.Texture"/>s, <see cref="Textures.RenderTexture"/>, and <see cref="Shaders.Shader"/>s.  
    /// </summary>
    public partial class Renderer : IDisposable
    {
        private int _width;
        private int _height;

        // Constructors

        /// <summary>
        /// Initializes a VelvetRenderer with a VelvetWindow.
        /// </summary>
        /// <remarks>This chooses the GraphicsAPI depending on the platform you're on. If you're on Windows, the GraphicsAPI is D3D11. If you're on OSX, the GraphicsAPI is Metal. If you're on Linux, the GraphicsAPI is Vulkan.</remarks>
        /// <param name="environment">The VelvetRendererEnvironment required to initialize a <see cref="Renderer"/></param>
        public Renderer(RendererEnvironment environment) { InitRenderer(environment, GraphicsAPI.Default, false); }

        /// <summary>
        /// Initializes a VelvetRenderer with a VelvetWindow.
        /// </summary>
        /// <param name="environment">The VelvetRendererEnvironment required to initialize a <see cref="Renderer"/></param>
        /// <param name="graphicsAPI">The GraphicsAPI to use.</param>
        public Renderer(RendererEnvironment environment, GraphicsAPI graphicsAPI) { InitRenderer(environment, graphicsAPI, false); }

        /// <summary>
        /// Initializes a VelvetRenderer with the specified GraphicsAPI with a VelvetWindow.
        /// </summary>
        /// <param name="environment">The VelvetRendererEnvironment required to initialize a <see cref="Renderer"/></param>
        /// <param name="graphicsAPI">The GraphicsAPI to use.</param>
        /// <param name="vsync">Whether VSync will be enabled or not.</param>
        public Renderer(RendererEnvironment environment, GraphicsAPI graphicsAPI, bool vsync) { InitRenderer(environment, graphicsAPI, vsync); }

        private void InitRenderer(RendererEnvironment environment, GraphicsAPI graphicsAPI, bool vsync)
        {
            _width = environment.WindowWidth;
            _height = environment.WindowHeight;
            GraphicsAPI resolvedAPI = graphicsAPI;

            if (graphicsAPI == GraphicsAPI.Default)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    resolvedAPI = GraphicsAPI.D3D11;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || OperatingSystem.IsMacOS())
                    resolvedAPI = GraphicsAPI.Metal;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    resolvedAPI = GraphicsAPI.Vulkan;
                else
                    throw new PlatformNotSupportedException();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                InitVeldrid_WIN(resolvedAPI, environment, vsync);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || OperatingSystem.IsMacOS())
                InitVeldrid_OSX(resolvedAPI, environment, vsync);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                InitVeldrid_LINUX(resolvedAPI, environment, vsync);
        }

        // IDisposable

        /// <summary>
        /// Clears up the resources used for the VelvetRenderer.
        /// </summary>
        public void Dispose()
        {
            _logger.Information("Disposing default texture...");
            DefaultTexture.Dispose();
            _logger.Information("Disposing shaders...");
            DefaultShader.Dispose();
            _logger.Information("Disposing command list...");
            _commandList.Dispose();
            _logger.Information("Disposing buffers...");
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _logger.Information("Disposing graphics device...");
            _graphicsDevice.Dispose();

        }
    }
}
