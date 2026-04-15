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
                resolvedAPI = GraphicsAPI.D3D11;  // You can change this to either D3D11, Vulkan, or OpenGL
                                                   // For some reason Vulkan/OpenGL is much smoother than D3D11. I have no idea why but if you want smoothness use Vulkan
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                resolvedAPI = GraphicsAPI.Metal;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                resolvedAPI = GraphicsAPI.Vulkan;  // You can change this to either Vulkan or OpenGL
            else
                throw new PlatformNotSupportedException();

            var test = new AudioTest(resolvedAPI);
            test.Run(args.Length, args);
        }
    }
}
