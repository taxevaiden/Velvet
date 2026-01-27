using System.Runtime.InteropServices;

using Velvet.Input;

using SDL3;

using Serilog;
using Serilog.Events;

namespace Velvet.Windowing
{
    public class VelvetWindow : IDisposable
    {
        private readonly ILogger _logger;
        public uint windowID { get; private set; } = uint.MinValue;
        public IntPtr windowPtr { get; private set; } = IntPtr.Zero;
        public bool Running { get; private set; } = false;
        public int Width { get => GetWidth(); set => SetWidth(value); }
        public int Height { get => GetHeight(); set => SetHeight(value); }


        /// <summary>
        /// Initializes a new VelvetWindow.
        /// </summary>
        /// <param name="title">The title of the VelvetWindow.</param>
        /// <param name="width">The width of the VelvetWindow.</param>
        /// <param name="height">The height of the VelvetWindow.</param>
        /// <exception cref="Exception"></exception>
        public VelvetWindow(string title, int width, int height)
        {
            Width = width;
            Height = height;

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

        private void SetWidth(int width)
        {
            SetSize(width, Height);
        }

        private void SetHeight(int height)
        {
            SetSize(Width, height);
        }

        private void SetSize(int width, int height)
        {
            if (windowPtr == IntPtr.Zero)
                return;

            width = Math.Max(1, width);
            height = Math.Max(1, height);

            SDL.SetWindowSize(windowPtr, width, height);

            Width = width;
            Height = height;

            _logger.Debug("Requested window resize: {W}x{H}", width, height);
        }

        private int GetWidth()
        {
            GetSize(out int w, out _);
            return w;
        }

        private int GetHeight()
        {
            GetSize(out _, out int h);
            return h;
        }

        private void GetSize(out int width, out int height)
        {
            SDL.GetWindowSize(windowPtr, out width, out height);
        }

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