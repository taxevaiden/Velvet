using System.Security.Principal;
using SDL3;
using static SDL3.SDL;

namespace Velvet
{
    public class VelvetWindow
    {
        public IntPtr windowPtr { get; private set; } = IntPtr.Zero;
        private bool _running = false;
        private SDL_Event _e;

        public VelvetWindow(string title, int width, int height)
        {
            if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
            {
                throw new Exception ($"Unable to initialize SDL: {SDL_GetError()}");
            }

            windowPtr = SDL_CreateWindow(title, width, height, SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS);
            if (windowPtr == IntPtr.Zero)
            {
                SDL_Quit();
                throw new Exception($"Window creation failed: {SDL_GetError()}");
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

        public void Dispose()
        {
            if (windowPtr != IntPtr.Zero)
            {
                SDL_DestroyWindow(windowPtr);
                windowPtr = IntPtr.Zero;
            }
            SDL_Quit();
        }

    }
}