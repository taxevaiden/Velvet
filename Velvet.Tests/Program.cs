using System.Runtime.InteropServices;

using Velvet.Graphics;

namespace Velvet.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            GraphicsAPI resolvedAPI;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                                                 // You can change this to either D3D11, Vulkan, or OpenGL
                resolvedAPI = GraphicsAPI.D3D11; // For some reason Vulkan/OpenGL on Windows is way smoother than D3D11, even with VSync on
                                                 // I have no idea why but if you want smoothness use Vulkan
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                resolvedAPI = GraphicsAPI.Metal;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                resolvedAPI = GraphicsAPI.Vulkan;  // You can change this to either Vulkan or OpenGL
            else
                throw new PlatformNotSupportedException();

            var test = new TextTest(resolvedAPI); // <-- You can change this to the other tests available
            test.Run(args.Length, args);
        }
    }
}
