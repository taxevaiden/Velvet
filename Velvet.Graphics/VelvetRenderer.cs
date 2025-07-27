using Veldrid;
using System.Runtime.InteropServices;

namespace Velvet.Graphics
{
    public partial class Renderer
    {
        /// <summary>
        /// Initializes a Renderer with a VelvetWindow.
        /// </summary>
        /// <remarks>This chooses the RendererAPI depending on the platform you're on. If you're on Windows, the RendererAPI is D3D11. If you're on OSX, the RendererAPI is Metal, and so on.</remarks>
        /// <param name="window"></param>
        public Renderer(VelvetWindow window)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                InitVeldrid_WIN(RendererAPI.D3D11, window);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                InitVeldrid_OSX(RendererAPI.Metal, window);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                InitVeldrid_LINUX(RendererAPI.Vulkan, window);
        }

        /// <summary>
        /// Initializes a Renderer with the specified RendererAPI with a VelvetWindow.
        /// </summary>
        /// <param name="rendererAPI"></param>
        /// <param name="window"></param>
        public Renderer(RendererAPI rendererAPI, VelvetWindow window)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                InitVeldrid_WIN(rendererAPI, window);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                InitVeldrid_OSX(rendererAPI, window);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                InitVeldrid_LINUX(rendererAPI, window);
        }

        /// <summary>
        /// Clears up the resources used for the Renderer.
        /// </summary>
        public void Dispose()
        {
            _logger.Information("Disposing pipeline...");
            _pipeline.Dispose();
            _logger.Information("Disposing shaders...");
            foreach (Shader shader in _shaders)
            {
                shader.Dispose();
            }
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
