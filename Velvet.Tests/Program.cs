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
                test.Run(RendererAPI.Vulkan); // You can change this to either D3D11, Vulkan, or OpenGL
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                test.Run(RendererAPI.Metal);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                test.Run(RendererAPI.OpenGL); // You can change this to either Vulkan or OpenGL
        }
    }
}
