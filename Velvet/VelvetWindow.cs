using System.Security.Principal;
using SDL3;
using static SDL3.SDL;

namespace Velvet
{
    public class VelvetWindow
    {
        private IntPtr _window;
        private bool _running = false;
        private SDL_Event _e;

        public VelvetWindow(string title, int width, int height)
        {
            if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
            {
                Console.WriteLine($"Unable to initialize SDL: {SDL_GetError()}");
                return;
            }

            _window = SDL_CreateWindow(title, width, height, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            if (_window == IntPtr.Zero)
            {
                Console.WriteLine($"Window creation failed: {SDL_GetError()}");
                SDL_Quit();
                return;
            }

            _running = true;
        }

        public bool IsRunning() { return _running; }

        public bool PollEvents()
        {
            while (SDL_PollEvent(out _e))
            {
                if (_e.type == (uint)SDL_EventType.SDL_EVENT_QUIT)
                {
                    _running = false;
                    return false;
                }
            }

            SDL_Delay(16);

            return true;
        }

        public void Destroy()
        {
            if (_window != IntPtr.Zero)
            {
                SDL_DestroyWindow(_window);
                _window = IntPtr.Zero;
            }
            SDL_Quit();
        }

    }
}