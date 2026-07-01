using System.Numerics;
using System.Runtime.InteropServices;

using SDL3;

using Serilog;

using Velvet.Graphics;
using Velvet.Input;
using Velvet.Windowing;

namespace Velvet
{
    /// <summary>
    /// Base class for all Velvet applications.
    /// Handles window creation, rendering, input, and the main loop.
    /// </summary>
    public abstract class VelvetApplication
    {
        private readonly ILogger _logger = Log.ForContext<VelvetApplication>();

        /// <summary>The application window.</summary>
        protected VelvetWindow Window { get; private set; } = null!;

        /// <summary>The renderer bound to <see cref="Window"/>.</summary>
        protected VelvetRenderer Renderer { get; private set; } = null!;
        /// <summary>
        /// The input manager for handling keyboard and mouse input.
        /// </summary>
        protected InputManager Input { get; private set; } = new();

        /// <summary>Seconds elapsed between the previous frame and the current one.</summary>
        public float DeltaTime { get; private set; }

        /// <summary>Total seconds elapsed since <see cref="Run"/> was called.</summary>
        public float TotalTime { get; private set; }

        /// <summary>Number of frames rendered since <see cref="Run"/> was called.</summary>
        public ulong FrameCount { get; private set; }

        private ulong _lastCounter;

        // SDL lifecycle callbacks
        private readonly SDL.MainFunc _runCallback;
        private readonly SDL.AppInitFunc _initCallback;
        private readonly SDL.AppIterateFunc _iterateCallback;
        private readonly SDL.AppEventFunc _eventCallback;
        private readonly SDL.AppQuitFunc _quitCallback;

        private readonly int _width;
        private readonly int _height;
        private readonly string _title;
        private readonly GraphicsAPI _graphicsAPI;
        private readonly bool _vsync;

        // Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VelvetApplication"/> class with the specified window dimensions, and title.
        /// </summary>
        /// <param name="width">The width of the application window in pixels.</param>
        /// <param name="height">The height of the application window in pixels.</param>
        /// <param name="title">The title of the application window.</param>
        protected VelvetApplication(int width, int height, string title)
            : this(width, height, title, GraphicsAPI.Default, vsync: false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VelvetApplication"/> class with the specified window dimensions, title, and graphics API.
        /// </summary>
        /// <param name="width">The width of the application window in pixels.</param>
        /// <param name="height">The height of the application window in pixels.</param>
        /// <param name="title">The title of the application window.</param>
        /// <param name="graphicsAPI">The graphics API to use for rendering.</param>
        protected VelvetApplication(int width, int height, string title, GraphicsAPI graphicsAPI)
            : this(width, height, title, graphicsAPI, vsync: false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VelvetApplication"/> class with the specified window dimensions, title, graphics API, and VSync setting.
        /// </summary>
        /// <param name="width">The width of the application window in pixels.</param>
        /// <param name="height">The height of the application window in pixels.</param>
        /// <param name="title">The title of the application window.</param>
        /// <param name="graphicsAPI">The graphics API to use for rendering.</param>
        /// <param name="vsync">Whether to enable VSync.</param>
        protected VelvetApplication(int width, int height, string title, GraphicsAPI graphicsAPI, bool vsync)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss.fff} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            _width = width;
            _height = height;
            _title = title;
            _graphicsAPI = graphicsAPI;
            _vsync = vsync;

            _runCallback = RunCallback;
            _initCallback = InitCallback;
            _iterateCallback = IterateCallback;
            _eventCallback = EventCallback;
            _quitCallback = QuitCallback;
        }

        // Entry point

        /// <summary>Starts the application and blocks until it exits.</summary>
        public int Run(int argc, string[]? argv)
            => SDL.RunApp(argc, argv, _runCallback, nint.Zero);

        // SDL callbacks

        private int RunCallback(int argc, string[]? argv)
            => SDL.EnterAppMainCallbacks(argc, argv,
                _initCallback, _iterateCallback, _eventCallback, _quitCallback);

        private SDL.AppResult InitCallback(ref nint appstate, int argc, string[]? argv)
        {
            switch (_graphicsAPI)
            {
                case GraphicsAPI.Vulkan:
                    {
                        Window = new VelvetWindow(_title, _width, _height, VelvetWindowFlags.Vulkan);
                        break;
                    }
                case GraphicsAPI.OpenGL:
                    {
                        Window = new VelvetWindow(_title, _width, _height, VelvetWindowFlags.OpenGL);
                        break;
                    }
                default:
                    {
                        Window = new VelvetWindow(_title, _width, _height, 0);
                        break;
                    }
            }

            VelvetRendererEnvironment environment;

            VelvetOpenGLPlatform glPlatform = new(
                Window.GetGLContext(),
                Window.GetGLProcAddress,
                Window.MakeGLContextCurrent,
                Window.GetCurrentGLContext,
                Window.ClearCurrentGLContext,
                Window.DestroyGLContext,
                Window.SwapGLBuffers,
                Window.SetGLVSync
            );

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                environment = new(
                    _width,
                    _height,
                    glPlatform,
                    hwnd: Window.GetHwnd(),
                    hInstance: Window.GetHInstance()
                );
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || OperatingSystem.IsMacOS())
            {
                environment = new(
                    _width,
                    _height,
                    glPlatform,
                    cocoaWindow: Window.GetCocoaWindow()
                );
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                environment = new(
                    _width,
                    _height,
                    glPlatform,
                    waylandDisplay: Window.GetWaylandDisplay(),
                    waylandSurface: Window.GetWaylandSurface(),
                    x11Display: Window.GetX11Display(),
                    x11Window: Window.GetX11Window()
                );
            }
            else
            {
                throw new PlatformNotSupportedException("This platform is not supported.");
            }

            Renderer = new VelvetRenderer(environment, _graphicsAPI, _vsync);

            _lastCounter = SDL.GetPerformanceCounter();

            OnInit();
            return SDL.AppResult.Continue;
        }

        private SDL.AppResult IterateCallback(nint appstate)
        {
            ulong currentCounter = SDL.GetPerformanceCounter();
            DeltaTime = (currentCounter - _lastCounter) / (float)SDL.GetPerformanceFrequency();
            TotalTime += DeltaTime;
            FrameCount++;
            _lastCounter = currentCounter;

            Input.Update();
            Update();
            Draw();
            Input.EndFrame();

            return SDL.AppResult.Continue;
        }

        private SDL.AppResult EventCallback(nint appstate, ref SDL.Event @event)
        {
            InputEvent inputEvent = new();

            switch ((SDL.EventType)@event.Type)
            {
                case SDL.EventType.WindowResized:
                    if (@event.Window.WindowID == Window.WindowID)
                        Renderer.Resize(Window.Width, Window.Height);
                    break;

                case SDL.EventType.WindowCloseRequested:
                    if (@event.Window.WindowID == Window.WindowID)
                    {
                        _logger.Information("(Window-{WindowId}): Close requested.", Window.WindowID);
                        return SDL.AppResult.Success;
                    }
                    break;

                case SDL.EventType.Quit:
                    _logger.Information("SDL quit event received.");
                    return SDL.AppResult.Success;
                case SDL.EventType.MouseMotion:
                    inputEvent.MousePosition = new Vector2(@event.Motion.X, @event.Motion.Y);
                    inputEvent.Type = InputEventType.MouseMotion;
                    break;

                case SDL.EventType.MouseWheel:
                    inputEvent.MouseScroll = new Vector2(@event.Wheel.X, @event.Wheel.Y);
                    if (@event.Wheel.Direction == SDL.MouseWheelDirection.Flipped)
                    {
                        inputEvent.MouseScroll = -inputEvent.MouseScroll;
                    }
                    inputEvent.Type = InputEventType.MouseWheel;
                    break;

                case SDL.EventType.MouseButtonDown:
                    inputEvent.MouseButton = (uint)@event.Button.Button - 1;
                    inputEvent.Type = InputEventType.MouseButtonDown;
                    break;

                case SDL.EventType.MouseButtonUp:
                    inputEvent.MouseButton = (uint)@event.Button.Button - 1;
                    inputEvent.Type = InputEventType.MouseButtonUp;
                    break;
                case SDL.EventType.KeyDown:
                    inputEvent.Key = (uint)@event.Key.Scancode;
                    inputEvent.Type = InputEventType.KeyDown;
                    break;
                case SDL.EventType.KeyUp:
                    inputEvent.Key = (uint)@event.Key.Scancode;
                    inputEvent.Type = InputEventType.KeyUp;
                    break;
            }

            Input.ProcessEvent(inputEvent);

            return SDL.AppResult.Continue;
        }

        private void QuitCallback(nint appstate, SDL.AppResult result)
        {
            _logger.Information("Shutting down (result = {Result})...", result);
            OnShutdown();
            Renderer.Dispose();
            Window.Dispose();
        }

        // Virtual hooks

        /// <summary>Called once after the window and renderer are ready. Override to load resources.</summary>
        protected virtual void OnInit() { }

        /// <summary>Called every frame before <see cref="Draw"/>. Override for game logic.</summary>
        protected virtual void Update() { }

        /// <summary>Called every frame after <see cref="Update"/>. Override to issue draw calls.</summary>
        protected virtual void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(System.Drawing.Color.Black);
            Renderer.End();
        }

        /// <summary>Called just before the window and renderer are destroyed. Override to release resources.</summary>
        protected virtual void OnShutdown() { }
    }
}
