using SDL3;

namespace Velvet
{
    public partial class VelvetWindow
    {
        public IntPtr windowPtr { get; private set; } = IntPtr.Zero;
        private bool _running = false;
        private SDL.Event _e;

        public VelvetWindow(string title, int width, int height)
        {
            Console.WriteLine("Initializing SDL3...");
            if (!SDL.Init(SDL.InitFlags.Video))
            {
                throw new Exception($"Unable to initialize SDL: {SDL.GetError()}");
            }

            Console.WriteLine("Creating window...");
            windowPtr = SDL.CreateWindow(title, width, height, SDL.WindowFlags.MouseFocus);
            if (windowPtr == IntPtr.Zero)
            {
                SDL.Quit();
                throw new Exception($"Window creation failed: {SDL.GetError()}");
            }

            Console.WriteLine("Running!");
            _running = true;
        }

        public bool PollEvents()
        {
            while (SDL.PollEvent(out _e))
            {
                if (_e.Type == (uint)SDL.EventType.Quit)
                {
                    _running = false;
                    return false;
                }
            }

            SDL.Delay(1);

            return true;
        }

        public void Dispose()
        {
            Console.WriteLine("Destroying window...");
            if (windowPtr != IntPtr.Zero)
            {
                SDL.DestroyWindow(windowPtr);
                windowPtr = IntPtr.Zero;
            }
            Console.WriteLine("Quitting SDL3...");
            SDL.Quit();
            Console.WriteLine("Session ended");
        }
    }
}