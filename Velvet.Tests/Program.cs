using System.Runtime.InteropServices;
using Velvet.Graphics;

namespace Velvet.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new ShapeTest();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                test.Run(RendererAPI.D3D11); // You can change this to either D3D11 or Vulkan
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                test.Run(RendererAPI.Metal);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                test.Run(RendererAPI.Vulkan);
        }
    }
}
