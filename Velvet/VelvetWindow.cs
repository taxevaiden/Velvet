using Velvet.Input;

using SDL3;

using Serilog;
using Serilog.Events;

namespace Velvet
{
    public class VelvetWindow : IDisposable
    {
        private readonly ILogger _logger;
        public uint windowID { get; private set; } = uint.MinValue;
        public IntPtr windowPtr { get; private set; } = IntPtr.Zero;
        public bool Running { get; private set; } = false;
        private SDL.Event _e;

        private ulong lastCounter = SDL.GetPerformanceCounter();
        private ulong freq = SDL.GetPerformanceFrequency();
        public float DeltaTime { get; private set; } = 1 / 1000.0f;
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// Initializes a new VelvetWindow.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <exception cref="Exception"></exception>
        public VelvetWindow(string title, int width, int height)
        {
            Width = width;
            Height = height;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            _logger = Log.ForContext<VelvetWindow>();

            _logger.Information("Initializing SDL3...");
            if (!SDL.Init(SDL.InitFlags.Video))
            {
                throw new Exception($"Unable to initialize SDL: {SDL.GetError()}");
            }

            _logger.Information("Creating window...");
            windowPtr = SDL.CreateWindow(title, width, height, SDL.WindowFlags.MouseFocus | SDL.WindowFlags.Resizable | SDL.WindowFlags.OpenGL);
            windowID = SDL.GetWindowID(windowPtr);
            _logger.Information($"> Window Pointer: {windowPtr}");
            _logger.Information($"> Window ID: {windowID}");
            if (windowPtr == IntPtr.Zero)
            {
                SDL.Quit();
                throw new Exception($"Window creation failed: {SDL.GetError()}");
            }

            _logger.Information($"(Window-{windowID}): Running!");
            Running = true;
        }

        /// <summary>
        /// Poll for currently pending events.
        /// </summary>
        /// <returns></returns>
        public bool PollEvents()
        {
            ulong currentCounter = SDL.GetPerformanceCounter();
            DeltaTime = (currentCounter - lastCounter) / (float)freq;
            lastCounter = currentCounter;

            InputManager.ClearEvents();
            while (SDL.PollEvent(out _e))
            {
                InputManager.PollEvent(_e);

                switch (_e.Type)
                {
                    case (uint)SDL.EventType.WindowResized:
                        {
                            Width = _e.Window.Data1;
                            Height = _e.Window.Data2;
                            return true;
                        }
                    case (uint)SDL.EventType.WindowCloseRequested:
                        {
                            uint eventWindowID = _e.Window.WindowID;

                            if (eventWindowID == windowID)
                            {
                                Running = false;
                                return true;
                            } else
                            {
                                return true;
                            }
                        }

                    case (uint)SDL.EventType.Quit:
                        {
                            Running = false;
                            return true;
                        }
                }
            }

            SDL.Delay(1);

            return false;
        }

        /// <summary>
        /// Destroys the window and quits SDL.
        /// </summary>
        public void Dispose()
        {
            Running = false;
            _logger.Information($"(Window-{windowID}): Destroying window...");
            if (windowPtr != IntPtr.Zero)
            {
                SDL.DestroyWindow(windowPtr);
                windowPtr = IntPtr.Zero;
            }
            _logger.Information($"(Window-{windowID}): Quitting SDL3...");
            SDL.Quit();
            _logger.Information($"(Window-{windowID}): Session ended");
        }
    }
}