using System.Numerics;

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

        /// <summary>The size of the window's client area in pixels.</summary>
        public Vector2 Size
        {
            get { SDL.GetWindowSize(WindowPtr, out int w, out int h); return new Vector2(w, h); }
            set => SetSize((int)value.X, (int)value.Y);
        }

        /// <summary>
        /// The minimum allowed width of the window's client area in pixels.
        /// </summary>
        public int MinWidth
        {
            get { SDL.GetWindowMinimumSize(WindowPtr, out int w, out _); return w; }
            set => SDL.SetWindowMinimumSize(WindowPtr, value, MinHeight);
        }

        /// <summary>
        /// The minimum allowed height of the window's client area in pixels.
        /// </summary>
        public int MinHeight
        {
            get { SDL.GetWindowMinimumSize(WindowPtr, out _, out int h); return h; }
            set => SDL.SetWindowMinimumSize(WindowPtr, MinWidth, value);
        }

        /// <summary>
        /// The minimum allowed size of the window's client area in pixels.
        /// </summary>
        public Vector2 MinSize
        {
            get { SDL.GetWindowMinimumSize(WindowPtr, out int w, out int h); return new Vector2(w, h); }
            set => SDL.SetWindowMinimumSize(WindowPtr, (int)value.X, (int)value.Y);
        }

        /// <summary>
        /// The maximum allowed width of the window's client area in pixels.
        /// </summary>
        public int MaxWidth
        {
            get { SDL.GetWindowMaximumSize(WindowPtr, out int w, out _); return w; }
            set => SDL.SetWindowMaximumSize(WindowPtr, value, MaxHeight);
        }

        /// <summary>
        /// The maximum allowed height of the window's client area in pixels.
        /// </summary>
        public int MaxHeight
        {
            get { SDL.GetWindowMaximumSize(WindowPtr, out _, out int h); return h; }
            set => SDL.SetWindowMaximumSize(WindowPtr, MaxWidth, value);
        }

        /// <summary>
        /// The maximum allowed size of the window's client area in pixels.
        /// </summary>
        public Vector2 MaxSize
        {
            get { SDL.GetWindowMaximumSize(WindowPtr, out int w, out int h); return new Vector2(w, h); }
            set => SDL.SetWindowMaximumSize(WindowPtr, (int)value.X, (int)value.Y);
        }

        /// <summary>The position of the window's top-left corner on the desktop.</summary>
        public (int X, int Y) Position
        {
            get { SDL.GetWindowPosition(WindowPtr, out int x, out int y); return (x, y); }
            set => SDL.SetWindowPosition(WindowPtr, value.X, value.Y);
        }

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

        /// <summary>Whether the window is currently fullscreen.</summary>
        public bool Fullscreen
        {
            get => HasFlag(SDL.WindowFlags.Fullscreen);
            set => SDL.SetWindowFullscreen(WindowPtr, value);
        }

        /// <summary>
        /// Whether the window is currently minimized.
        /// </summary>
        public bool Minimized
        {
            get => HasFlag(SDL.WindowFlags.Minimized);
            set { if (value) SDL.MinimizeWindow(WindowPtr); else Restore(); }
        }

        /// <summary>
        /// Whether the window is currently maximized.
        /// </summary>
        public bool Maximized
        {
            get => HasFlag(SDL.WindowFlags.Maximized);
            set { if (value) SDL.MaximizeWindow(WindowPtr); else Restore(); }
        }

        /// <summary>
        /// Whether the window currently has input focus.
        /// </summary>
        public bool Focused
        {
            get => HasFlag(SDL.WindowFlags.InputFocus);
        }

        /// <summary>
        /// Whether the window can be resized by the user.
        /// </summary>
        public bool Resizable
        {
            get => HasFlag(SDL.WindowFlags.Resizable);
            set => SDL.SetWindowResizable(WindowPtr, value);
        }

        /// <summary>
        /// Whether the window is currently hidden.
        /// </summary>
        public bool Hidden
        {
            get => HasFlag(SDL.WindowFlags.Hidden);
            set
            {
                if (value) SDL.HideWindow(WindowPtr);
                else SDL.ShowWindow(WindowPtr);
            }
        }

        // Construction

        /// <summary>Creates and shows a new window.</summary>
        /// <exception cref="InvalidOperationException">Thrown if window creation fails.</exception>
        public VelvetWindow(string title, int width, int height)
        {
            _logger.Information("Initializing SDL3...");
            if (!SDL.Init(SDL.InitFlags.Video))
                throw new InvalidOperationException($"Unable to initialize SDL: {SDL.GetError()}");

            _logger.Information("Creating window...");
            WindowPtr = SDL.CreateWindow(
                title, width, height,
                SDL.WindowFlags.MouseFocus | SDL.WindowFlags.OpenGL);

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

        /// <summary>Centers the window on its current display.</summary>
        public void Center()
            => SDL.SetWindowPosition(WindowPtr, (int)SDL.WindowPosCentered(), (int)SDL.WindowPosCentered());

        /// <summary>Restores the window from a minimized or maximized state.</summary>
        public void Restore() => SDL.RestoreWindow(WindowPtr);

        /// <summary>
        /// Brings the window to the front and gives it input focus.
        /// </summary>
        public void Focus() => SDL.RaiseWindow(WindowPtr);

        /// <summary>
        /// Shows or hides the mouse cursor.
        /// </summary>
        public static void ShowCursor(bool show)
        { if (show) SDL.ShowCursor(); else SDL.HideCursor(); }

        // Helpers

        private bool HasFlag(SDL.WindowFlags flag)
            => WindowPtr != IntPtr.Zero &&
               ((SDL.WindowFlags)SDL.GetWindowFlags(WindowPtr) & flag) != 0;

        // Tbh I could make this public but I think making this a property is more convenient
        private void SetSize(int width, int height)
        {
            if (WindowPtr == IntPtr.Zero) return;
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            SDL.SetWindowSize(WindowPtr, width, height);
            _logger.Debug("(Window-{WindowId}): Resized to {W}x{H}", WindowID, width, height);
        }

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