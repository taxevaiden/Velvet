using System.Collections.Concurrent;
using System.Runtime.InteropServices;

using Serilog;

using Velvet.Graphics;
using Velvet.Input;
using Velvet.Windowing;

namespace Velvet
{
    /// <summary>
    /// An optional multi-threaded application layer that manages a <see cref="Renderer"/>, <see cref="Window"/>, and <see cref="InputManager"/> for you.
    /// </summary>
    /// <remarks>
    /// Multi-threading generally provides better performance than single-threading, however it can be disabled if desired. 
    /// When multi-threading is enabled, the main loop (update and rendering), event handling, and input processing are run in parallel.
    /// </remarks>
    public abstract class Application
    {
        private readonly ILogger _logger;

        /// <summary>The application window.</summary>
        protected Window Window { get; private set; } = null!;

        /// <summary>The renderer bound to <see cref="Window"/>.</summary>
        protected Renderer Renderer { get; private set; } = null!;
        /// <summary>
        /// The input manager for handling keyboard and mouse input.
        /// </summary>
        protected InputManager Input { get; private set; } = new();

        /// <summary>Seconds elapsed between the previous frame and the current one.</summary>
        public double DeltaTime { get; private set; }

        /// <summary>Total seconds elapsed since <see cref="Run"/> was called.</summary>
        public double TotalTime { get; private set; }

        /// <summary>Number of frames rendered since <see cref="Run"/> was called.</summary>
        public ulong FrameCount { get; private set; }

        private ulong _lastCounter;

        private readonly int _width;
        private readonly int _height;
        private readonly string _title;
        private readonly GraphicsAPI _graphicsAPI;
        private readonly bool _vsync;
        private ConcurrentQueue<InputEvent> _events = new();
        private readonly bool _multithreaded;
        private readonly Thread _iterThread;
        private readonly Thread _inputThread;

        // Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class with the specified window dimensions, and title.
        /// </summary>
        /// <param name="width">The width of the application window in pixels.</param>
        /// <param name="height">The height of the application window in pixels.</param>
        /// <param name="title">The title of the application window.</param>
        protected Application(int width, int height, string title)
            : this(width, height, title, GraphicsAPI.Default, false, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class with the specified window dimensions, title, and graphics API.
        /// </summary>
        /// <param name="width">The width of the application window in pixels.</param>
        /// <param name="height">The height of the application window in pixels.</param>
        /// <param name="title">The title of the application window.</param>
        /// <param name="graphicsAPI">The graphics API to use for rendering.</param>
        protected Application(int width, int height, string title, GraphicsAPI graphicsAPI)
            : this(width, height, title, graphicsAPI, false, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class with the specified window dimensions, title, graphics API, and VSync setting.
        /// </summary>
        /// <param name="width">The width of the application window in pixels.</param>
        /// <param name="height">The height of the application window in pixels.</param>
        /// <param name="title">The title of the application window.</param>
        /// <param name="graphicsAPI">The graphics API to use for rendering.</param>
        /// <param name="vsync">Whether to enable VSync.</param>
        protected Application(int width, int height, string title, GraphicsAPI graphicsAPI, bool vsync)
            : this(width, height, title, graphicsAPI, vsync, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class with the specified window dimensions, title, graphics API, VSync setting, and threading mode.
        /// </summary>
        /// <param name="width">The width of the application window in pixels.</param>
        /// <param name="height">The height of the application window in pixels.</param>
        /// <param name="title">The title of the application window.</param>
        /// <param name="graphicsAPI">The graphics API to use for rendering.</param>
        /// <param name="vsync">Whether to enable VSync.</param>
        /// <param name="multithreaded">Whether to enable multithreading.</param>
        protected Application(int width, int height, string title, GraphicsAPI graphicsAPI, bool vsync, bool multithreaded)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss.fff} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            _logger = Log.ForContext<Application>();

            _width = width;
            _height = height;
            _title = title;
            _graphicsAPI = graphicsAPI;
            _vsync = vsync;
            _multithreaded = multithreaded;

            if (multithreaded)
            {
                _iterThread = new Thread(() =>
                {
                    while (Window.Running)
                    {
                        Iterate();
                    }
                });

                _inputThread = new Thread(() =>
                {
                    while (Window.Running)
                    {
                        ProcessEvents(_events);
                    }
                });
            }
        }

        // Entry point

        /// <summary>Starts the application and blocks until it exits.</summary>
        public void Run(int argc, string[]? argv)
        {
            if (_multithreaded) RunMultiThreaded(argc, argv); else RunSingleThreaded(argc, argv);
        }

        private void RunSingleThreaded(int argc, string[]? argv)
        {
            Init(argc, argv);

            while (Window.Running)
            {
                PumpEvents();

                InputEvent @event;
                while (_events.TryDequeue(out @event))
                {
                    Input.ProcessEvent(@event);
                }
                Iterate();
                Input.Update();
            }
            Quit();
        }

        private void RunMultiThreaded(int argc, string[]? argv)
        {
            Init(argc, argv);

            _iterThread.Start(); _inputThread.Start();

            while (Window.Running)
            {
                PumpEvents();
            }

            _iterThread.Join();
            _inputThread.Join();

            Quit();
        }

        private void Init(int argc, string[]? argv)
        {
            switch (_graphicsAPI)
            {
                case GraphicsAPI.Vulkan:
                    {
                        Window = new Window(_title, _width, _height, WindowFlags.Vulkan);
                        break;
                    }
                case GraphicsAPI.OpenGL:
                    {
                        Window = new Window(_title, _width, _height, WindowFlags.OpenGL);
                        break;
                    }
                default:
                    {
                        Window = new Window(_title, _width, _height, 0);
                        break;
                    }
            }

            RendererEnvironment environment;

            RendererOpenGLPlatform? glPlatform = _graphicsAPI == GraphicsAPI.OpenGL ? new(
                Window.GetGLContext(),
                Window.GetGLProcAddress,
                Window.MakeGLContextCurrent,
                Window.GetCurrentGLContext,
                Window.ClearCurrentGLContext,
                Window.DestroyGLContext,
                Window.SwapGLBuffers,
                Window.SetGLVSync
            ) : null;

            // For some reason Veldrid seems to just disregards the vsync bool we give to it and passes true to this
            // So we have to do this
            if (_graphicsAPI == GraphicsAPI.OpenGL)
                Window.SetGLVSync(_vsync);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                environment = new(
                    _width,
                    _height,
                    glPlatform.GetValueOrDefault(),
                    hwnd: Window.GetHwnd(),
                    hInstance: Window.GetHInstance()
                );
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || OperatingSystem.IsMacOS())
            {
                environment = new(
                    _width,
                    _height,
                    glPlatform.GetValueOrDefault(),
                    cocoaWindow: Window.GetCocoaWindow()
                );
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                environment = new(
                    _width,
                    _height,
                    glPlatform.GetValueOrDefault(),
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

            Renderer = new Renderer(environment, _graphicsAPI, _vsync);

            _lastCounter = Window.PerformanceCounter;

            OnInit(argc, argv);
        }

        /// <summary>
        /// Intended to be used in a separate thread
        /// </summary>
        private void ProcessEvents(ConcurrentQueue<InputEvent> events)
        {
            InputEvent @event;
            while (events.TryDequeue(out @event))
            {
                Input.ProcessEvent(@event);
            }
            Thread.Sleep(1);
            Input.Update();
        }

        private void Iterate()
        {
            ulong currentCounter = Window.PerformanceCounter;
            DeltaTime = (currentCounter - _lastCounter) / (double)Window.PerformanceFrequency;
            TotalTime += DeltaTime;
            FrameCount++;
            _lastCounter = currentCounter;

            Update();
            Draw();
        }

        private void PumpEvents()
        {
            WindowEvent windowEvent = new();

            while (Window.PollEvent(ref windowEvent))
            {
                InputEvent inputEvent = new();
                switch (windowEvent.Type)
                {
                    case WindowEventType.WindowResized:
                        Renderer.Resize(windowEvent.WindowSize);
                        break;
                    case WindowEventType.MouseMotion:
                        inputEvent.MousePosition = windowEvent.MousePosition;
                        inputEvent.Type = InputEventType.MouseMotion;
                        break;
                    case WindowEventType.MouseWheel:
                        inputEvent.MouseScroll = windowEvent.MouseScroll;
                        inputEvent.Type = InputEventType.MouseWheel;
                        break;
                    case WindowEventType.MouseButtonDown:
                        inputEvent.MouseButton = windowEvent.MouseButton;
                        inputEvent.Type = InputEventType.MouseButtonDown;
                        break;
                    case WindowEventType.MouseButtonUp:
                        inputEvent.MouseButton = windowEvent.MouseButton;
                        inputEvent.Type = InputEventType.MouseButtonUp;
                        break;
                    case WindowEventType.KeyDown:
                        inputEvent.Key = windowEvent.Key;
                        inputEvent.Type = InputEventType.KeyDown;
                        break;
                    case WindowEventType.KeyUp:
                        inputEvent.Key = windowEvent.Key;
                        inputEvent.Type = InputEventType.KeyUp;
                        break;
                    default:
                        break;
                }
                _events.Enqueue(inputEvent);
            }
        }

        private void Quit()
        {
            OnShutdown();
            Renderer.Dispose();
            Window.Dispose();
        }

        // Virtual hooks

        /// <summary>Called once after the window and renderer are ready. Override to load resources.</summary>
        protected virtual void OnInit(int argc, string[]? argv) { }

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
