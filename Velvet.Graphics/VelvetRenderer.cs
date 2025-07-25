using Veldrid;

namespace Velvet.Graphics
{
    public partial class Renderer
    {
        public Renderer(RendererAPI rendererAPI, VelvetWindow window)
        {
            InitVeldrid_WIN(rendererAPI, window);
        }

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
