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
            Console.WriteLine("Disposing pipeline...");
            _pipeline.Dispose();
            Console.WriteLine("Disposing shaders...");
            foreach (Shader shader in _shaders)
            {
                shader.Dispose();
            }
            Console.WriteLine("Disposing command list...");
            _commandList.Dispose();
            Console.WriteLine("Disposing buffers...");
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            Console.WriteLine("Disposing graphics device...");
            _graphicsDevice.Dispose();
            
        }
    }
}
