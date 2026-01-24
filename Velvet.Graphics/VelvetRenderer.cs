using Veldrid;
using Velvet.Windowing;
using System.Runtime.InteropServices;

namespace Velvet.Graphics
{
    public partial class VelvetRenderer : IDisposable
    {
        /// <summary>
        /// Initializes a Renderer with a VelvetWindow.
        /// </summary>
        /// <remarks>This chooses the GraphicsAPI depending on the platform you're on. If you're on Windows, the GraphicsAPI is D3D11. If you're on OSX, the GraphicsAPI is Metal. If you're on Linux, the GraphicsAPI is Vulkan.</remarks>
        /// <param name="window"></param>
        public VelvetRenderer(VelvetWindow window)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                InitVeldrid_WIN(GraphicsAPI.D3D11, window, true);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                InitVeldrid_OSX(GraphicsAPI.Metal, window, true);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                InitVeldrid_LINUX(GraphicsAPI.Vulkan, window, true);
        }

        /// <summary>
        /// Initializes a Renderer with the specified GraphicsAPI with a VelvetWindow.
        /// </summary>
        /// <param name="rendererAPI"></param>
        /// <param name="window"></param>
        public VelvetRenderer(VelvetWindow window, GraphicsAPI rendererAPI)
        {
            GraphicsAPI resolvedAPI = rendererAPI;

            if (rendererAPI == GraphicsAPI.Default)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    resolvedAPI = GraphicsAPI.D3D11;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    resolvedAPI = GraphicsAPI.Metal;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    resolvedAPI = GraphicsAPI.Vulkan;
                else
                    throw new PlatformNotSupportedException();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                InitVeldrid_WIN(resolvedAPI, window, true);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                InitVeldrid_OSX(resolvedAPI, window, true);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                InitVeldrid_LINUX(resolvedAPI, window, true);
        }

        /// <summary>
        /// Initializes a Renderer with the specified GraphicsAPI with a VelvetWindow.
        /// </summary>
        /// <param name="rendererAPI"></param>
        /// <param name="window"></param>
        public VelvetRenderer(VelvetWindow window, GraphicsAPI rendererAPI, bool vsync)
        {
            GraphicsAPI resolvedAPI = rendererAPI;

            if (rendererAPI == GraphicsAPI.Default)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    resolvedAPI = GraphicsAPI.D3D11;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    resolvedAPI = GraphicsAPI.Metal;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    resolvedAPI = GraphicsAPI.Vulkan;
                else
                    throw new PlatformNotSupportedException();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                InitVeldrid_WIN(resolvedAPI, window, vsync);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                InitVeldrid_OSX(resolvedAPI, window, vsync);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                InitVeldrid_LINUX(resolvedAPI, window, vsync);
        }

        /// <summary>
        /// Clears up the resources used for the Renderer.
        /// </summary>
        public void Dispose()
        {
            _logger.Information($"(Window-{_window.windowID}): Disposing default texture...");
            DefaultTexture.Dispose();
            _logger.Information($"(Window-{_window.windowID}): Disposing shaders...");
            DefaultShader.Dispose();
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
