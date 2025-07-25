using Veldrid;

namespace Velvet.Graphics
{
    public partial class Renderer
    {
        /// <summary>
        /// Initializes a Renderer with the specified RendererAPI with a VelvetWindow.
        /// </summary>
        /// <param name="rendererAPI"></param>
        /// <param name="window"></param>
        public Renderer(RendererAPI rendererAPI, VelvetWindow window)
        {
            InitVeldrid_WIN(rendererAPI, window);
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
