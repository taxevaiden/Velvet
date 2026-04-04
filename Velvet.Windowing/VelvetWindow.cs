using SDL3;

using Serilog;

namespace Velvet.Windowing
{   
    /// <summary>
    /// A window that can be drawn to and receive input. This is created by <c>VelvetApplication</c> and passed to the <c>VelvetRenderer</c> for drawing.
    /// </summary>
    public class VelvetWindow : IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<VelvetWindow>();

        /// <summary>The SDL window ID.</summary>
        public uint WindowID { get; private set; } = uint.MinValue;

        /// <summary>The underlying SDL window pointer.</summary>
        public IntPtr WindowPtr { get; private set; } = IntPtr.Zero;

        /// <summary>Whether the window has been successfully created and not yet destroyed.</summary>
        public bool Running { get; private set; } = false;

        // Size / position

        /// <summary>The width of the window's client area in pixels.</summary>
        public int Width
        {
            get { SDL.GetWindowSize(WindowPtr, out int w, out _); return w; }
            set => SetSize(value, Height);
        }

        /// <summary>The height of the window's client area in pixels.</summary>
        public int Height
        {
            get { SDL.GetWindowSize(WindowPtr, out _, out int h); return h; }
            set => SetSize(Width, value);
        }

        /// <summary>The position of the window's top-left corner on the desktop.</summary>
        public (int X, int Y) Position
        {
            get { SDL.GetWindowPosition(WindowPtr, out int x, out int y); return (x, y); }
            set => SDL.SetWindowPosition(WindowPtr, value.X, value.Y);
        }

        // Appearance

        /// <summary>The window title.</summary>
        public string Title
        {
            get => SDL.GetWindowTitle(WindowPtr);
            set => SDL.SetWindowTitle(WindowPtr, value);
        }

        /// <summary>
        /// The window opacity in the range [0.0, 1.0].
        /// Not supported on all platforms.
        /// </summary>
        public float Opacity
        {
            get => SDL.GetWindowOpacity(WindowPtr);
            set => SDL.SetWindowOpacity(WindowPtr, Math.Clamp(value, 0f, 1f));
        }

        /// <summary>Whether the window has a border and title bar.</summary>
        public bool Bordered
        {
            get => !HasFlag(SDL.WindowFlags.Borderless);
            set => SDL.SetWindowBordered(WindowPtr, value);
        }

        // State

        /// <summary>Whether the window is currently fullscreen.</summary>
        public bool IsFullscreen => HasFlag(SDL.WindowFlags.Fullscreen);

        /// <summary>Whether the window is currently minimized.</summary>
        public bool IsMinimized => HasFlag(SDL.WindowFlags.Minimized);

        /// <summary>Whether the window is currently maximized.</summary>
        public bool IsMaximized => HasFlag(SDL.WindowFlags.Maximized);

        /// <summary>Whether the window currently has input focus.</summary>
        public bool IsFocused => HasFlag(SDL.WindowFlags.InputFocus);

        // Construction

        /// <summary>Creates and shows a new window.</summary>
        /// <param name="title">Window title.</param>
        /// <param name="width">Client width in pixels.</param>
        /// <param name="height">Client height in pixels.</param>
        /// <exception cref="InvalidOperationException">Thrown if SDL or window creation fails.</exception>
        public VelvetWindow(string title, int width, int height)
        {
            _logger.Information("Initializing SDL3...");
            if (!SDL.Init(SDL.InitFlags.Video))
                throw new InvalidOperationException($"Unable to initialize SDL: {SDL.GetError()}");

            _logger.Information("Creating window...");
            WindowPtr = SDL.CreateWindow(
                title, width, height,
                SDL.WindowFlags.MouseFocus | SDL.WindowFlags.Resizable | SDL.WindowFlags.OpenGL);

            if (WindowPtr == IntPtr.Zero)
            {
                SDL.Quit();
                throw new InvalidOperationException($"Window creation failed: {SDL.GetError()}");
            }

            WindowID = SDL.GetWindowID(WindowPtr);

            _logger.Information("(Window-{WindowId}): Created: {W}x{H}, ptr={Ptr}",
                WindowID, width, height, WindowPtr);

            Running = true;
        }

        // Window actions

        /// <summary>Resizes the window. Values are clamped to a minimum of 1.</summary>
        public void SetSize(int width, int height)
        {
            if (WindowPtr == IntPtr.Zero) return;
            width  = Math.Max(1, width);
            height = Math.Max(1, height);
            SDL.SetWindowSize(WindowPtr, width, height);
            _logger.Debug("(Window-{WindowId}): Resized to {W}x{H}", WindowID, width, height);
        }

        /// <summary>Centers the window on its current display.</summary>
        public void Center()
            => SDL.SetWindowPosition(WindowPtr, (int)SDL.WindowPosCentered(), (int)SDL.WindowPosCentered());

        /// <summary>Minimizes the window.</summary>
        public void Minimize() => SDL.MinimizeWindow(WindowPtr);

        /// <summary>Maximizes the window.</summary>
        public void Maximize() => SDL.MaximizeWindow(WindowPtr);

        /// <summary>Restores the window from a minimized or maximized state.</summary>
        public void Restore() => SDL.RestoreWindow(WindowPtr);

        /// <summary>Raises the window to the front and gives it input focus.</summary>
        public void Focus() => SDL.RaiseWindow(WindowPtr);

        /// <summary>
        /// Toggles fullscreen mode.
        /// </summary>
        /// <param name="fullscreen">
        /// <c>true</c> for fullscreen, <c>false</c> for windowed.
        /// Omit to toggle from the current state.
        /// </param>
        public void SetFullscreen(bool? fullscreen = null)
        {
            bool enter = fullscreen ?? !IsFullscreen;
            SDL.SetWindowFullscreen(WindowPtr, enter);
            _logger.Debug("(Window-{WindowId}): Fullscreen = {Value}", WindowID, enter);
        }

        /// <summary>
        /// Sets the minimum allowed window size.
        /// </summary>
        public void SetMinimumSize(int width, int height)
            => SDL.SetWindowMinimumSize(WindowPtr, width, height);

        /// <summary>
        /// Sets the maximum allowed window size.
        /// </summary>
        public void SetMaximumSize(int width, int height)
            => SDL.SetWindowMaximumSize(WindowPtr, width, height);

        // Helpers

        private bool HasFlag(SDL.WindowFlags flag)
            => WindowPtr != IntPtr.Zero &&
               ((SDL.WindowFlags)SDL.GetWindowFlags(WindowPtr) & flag) != 0;

        // IDisposable

        /// <summary>
        /// Disposes the window and its resources.
        /// </summary>
        public void Dispose()
        {
            Running = false;

            if (WindowPtr != IntPtr.Zero)
            {
                _logger.Information("(Window-{WindowId}): Destroying window...", WindowID);
                SDL.DestroyWindow(WindowPtr);
                WindowPtr = IntPtr.Zero;
            }

            _logger.Information("(Window-{WindowId}): Quitting SDL3...", WindowID);
            SDL.Quit();
            _logger.Information("(Window-{WindowId}): Session ended.", WindowID);
        }
    }
}