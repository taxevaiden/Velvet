using Velvet.Input;

using SDL3;

using Serilog;
using Serilog.Events;

namespace Velvet
{
    public partial class VelvetWindow
    {
        private static readonly ILogger _logger = Log.ForContext<VelvetWindow>();
        public IntPtr windowPtr { get; private set; } = IntPtr.Zero;
        private bool _running = false;
        private SDL.Event _e;

        /// <summary>
        /// Initializes a new VelvetWindow.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <exception cref="Exception"></exception>
        public VelvetWindow(string title, int width, int height)
        {
            Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

            _logger.Information("Initializing SDL3...");
            if (!SDL.Init(SDL.InitFlags.Video))
            {
                throw new Exception($"Unable to initialize SDL: {SDL.GetError()}");
            }

            _logger.Information("Creating window...");
            windowPtr = SDL.CreateWindow(title, width, height, SDL.WindowFlags.MouseFocus);
            if (windowPtr == IntPtr.Zero)
            {
                SDL.Quit();
                throw new Exception($"Window creation failed: {SDL.GetError()}");
            }

            _logger.Information("Running!");
            _running = true;
        }

        /// <summary>
        /// Poll for currently pending events.
        /// </summary>
        /// <returns></returns>
        public bool PollEvents()
        {
            InputManager.ClearEvents();
            while (SDL.PollEvent(out _e))
            {
                InputManager.PollEvent(_e);
                if (_e.Type == (uint)SDL.EventType.Quit)
                {
                    _running = false;
                    return false;
                }
            }

            SDL.Delay(1);

            return true;
        }

        /// <summary>
        /// Destroys the window and quits SDL.
        /// </summary>
        public void Dispose()
        {
            _logger.Information("Destroying window...");
            if (windowPtr != IntPtr.Zero)
            {
                SDL.DestroyWindow(windowPtr);
                windowPtr = IntPtr.Zero;
            }
            _logger.Information("Quitting SDL3...");
            SDL.Quit();
            _logger.Information("Session ended");
        }
    }
}