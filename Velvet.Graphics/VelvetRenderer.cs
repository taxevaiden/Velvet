using Velvet;
using SDL3;
using static SDL3.SDL;

using Veldrid;
using Veldrid.StartupUtilities;
using SharpGen.Runtime.Win32;

namespace Velvet.Graphics
{
    public class Renderer
    {
        IntPtr hwmd;
        GraphicsDevice graphicsDevice;
        public Renderer(VelvetWindow window)
        {
            hwmd = SDL_GetPointerProperty(SDL_GetWindowProperties(window.windowPtr), SDL_PROP_WINDOW_WIN32_HWND_POINTER, IntPtr.Zero);

            int w, h;

            if (SDL_GetWindowSizeInPixels(window.windowPtr, out w, out h))
            {
                graphicsDevice = GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions(), hwmd, (uint)w, (uint)h);
            }
            else
            {
                SDL_Quit();
                throw new Exception($"Failed to create Renderer: {SDL_GetError()}");
            }
        }

        //  TODO: actually clean the Go() method up and make it proper

        public void Go()
        {
            CommandList cmd = graphicsDevice.ResourceFactory.CreateCommandList();
            cmd.Begin();

            // Clear screen to cornflower blue
            cmd.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
            cmd.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

            // Your rendering commands here

            cmd.End();
            graphicsDevice.SubmitCommands(cmd);
            graphicsDevice.SwapBuffers();

            cmd.Dispose();
        }

        public void Dispose()
        {
            graphicsDevice.Dispose();
        }
    }
}
