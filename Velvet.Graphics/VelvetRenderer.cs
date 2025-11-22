using Veldrid;
using System.Runtime.InteropServices;

namespace Velvet.Graphics
{
    public partial class Renderer : IDisposable
    {
        /// <summary>
        /// Initializes a Renderer with a VelvetWindow.
        /// </summary>
        /// <remarks>This chooses the RendererAPI depending on the platform you're on. If you're on Windows, the RendererAPI is D3D11. If you're on OSX, the RendererAPI is Metal, and so on.</remarks>
        /// <param name="window"></param>
        public Renderer(VelvetWindow window)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                InitVeldrid_WIN(RendererAPI.D3D11, window, true);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                InitVeldrid_OSX(RendererAPI.Metal, window, true);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                InitVeldrid_LINUX(RendererAPI.Vulkan, window, true);
        }

        /// <summary>
        /// Initializes a Renderer with the specified RendererAPI with a VelvetWindow.
        /// </summary>
        /// <param name="rendererAPI"></param>
        /// <param name="window"></param>
        public Renderer(RendererAPI rendererAPI, VelvetWindow window)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                InitVeldrid_WIN(rendererAPI, window, true);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                InitVeldrid_OSX(rendererAPI, window, true);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                InitVeldrid_LINUX(rendererAPI, window, true);
        }

        /// <summary>
        /// Initializes a Renderer with the specified RendererAPI with a VelvetWindow.
        /// </summary>
        /// <param name="rendererAPI"></param>
        /// <param name="window"></param>
        public Renderer(RendererAPI rendererAPI, VelvetWindow window, bool vsync)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                InitVeldrid_WIN(rendererAPI, window, vsync);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                InitVeldrid_OSX(rendererAPI, window, vsync);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                InitVeldrid_LINUX(rendererAPI, window, vsync);
        }

        /// <summary>
        /// Clears up the resources used for the Renderer.
        /// </summary>
        public void Dispose()
        {
            _logger.Information($"(Window-{_window.windowID}): Disposing pipeline...");
            _pipeline.Dispose();
            _logger.Information($"(Window-{_window.windowID}): Disposing shaders...");
            foreach (Shader shader in _shaders)
            {
                shader.Dispose();
            }
            _logger.Information($"(Window-{_window.windowID}): Disposing command list...");
            _commandList.Dispose();
            _logger.Information($"(Window-{_window.windowID}): Disposing buffers...");
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _logger.Information($"(Window-{_window.windowID}): Disposing graphics device...");
            _graphicsDevice.Dispose();

        }
    }
}
