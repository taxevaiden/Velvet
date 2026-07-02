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
                resolvedAPI = GraphicsAPI.Vulkan; // Vulkan/OpenGL is far more stable than D3D11
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                resolvedAPI = GraphicsAPI.Metal;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                resolvedAPI = GraphicsAPI.Vulkan;
            else
                throw new PlatformNotSupportedException();

            var test = new ShapeTest(resolvedAPI); // <-- You can change this to the other tests available
            test.Run(args.Length, args);
        }
    }
}
