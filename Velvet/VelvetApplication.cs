using Velvet.Windowing;
using Velvet.Graphics;
using Velvet.Input;
using SDL3;
using Serilog;

namespace Velvet
{
    public abstract class VelvetApplication
    {
        private readonly ILogger _logger = Log.ForContext<VelvetApplication>();
        protected VelvetWindow Window { get; private set; }
        protected VelvetRenderer Renderer { get; private set; }

        public float DeltaTime { get; private set; }
        private ulong lastCounter;

        private SDL.MainFunc? _runCallback;
        private SDL.AppInitFunc? _initCallback;
        private SDL.AppIterateFunc? _iterateCallback;
        private SDL.AppEventFunc? _eventCallback;
        private SDL.AppQuitFunc? _quitCallback;
        private int _width;
        private int _height;
        private string _title;
        private GraphicsAPI _graphicsAPI;
        private bool _vsync;

        protected VelvetApplication(int width, int height, string title)
        {
            InitApp(width, height, title, GraphicsAPI.Default, true);
        }

        protected VelvetApplication(int width, int height, string title, GraphicsAPI graphicsAPI)
        {
            InitApp(width, height, title, graphicsAPI, true);
        }

        protected VelvetApplication(int width, int height, string title, GraphicsAPI graphicsAPI, bool vsync)
        {
            InitApp(width, height, title, graphicsAPI, vsync);
        }

        private void InitApp(int width, int height, string title, GraphicsAPI graphicsAPI, bool vsync)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}")
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

        public int Run(int argc, string[] argv)
        {
            return SDL.RunApp(argc, argv, _runCallback, (nint)null);
        }

        private int RunCallback(int argc, string[] argv)
        {
            return SDL.EnterAppMainCallbacks(
                argc, argv,
                _initCallback,
                _iterateCallback,
                _eventCallback,
                _quitCallback
            );
        }

        SDL.AppResult InitCallback(nint appstate, int argc, string[] argv)
        {
            Window = new VelvetWindow(_title, _width, _height); // This already initializes SDL3 so it should be fine to use everything else here

            Renderer = new VelvetRenderer(Window, _graphicsAPI, _vsync);

            lastCounter = SDL.GetPerformanceCounter();

            OnInit();

            return SDL.AppResult.Continue;
        }

        SDL.AppResult IterateCallback(nint appstate)
        {
            ulong currentCounter = SDL.GetPerformanceCounter();
            DeltaTime = (currentCounter - lastCounter) / (float)SDL.GetPerformanceFrequency();
            lastCounter = currentCounter;

            InputManager.Update();
            Update(DeltaTime);
            Draw();
            InputManager.EndFrame();
            return SDL.AppResult.Continue;
        }

        SDL.AppResult EventCallback(nint appstate, ref SDL.Event @event)
        {
            InputManager.ProcessEvent(@event);

            switch ((SDL.EventType)@event.Type)
            {
                case SDL.EventType.WindowExposed:
                    {
                        _logger.Information("Window exposed!");
                        break;
                    }
                case SDL.EventType.WindowResized:
                    {
                        if (@event.Window.WindowID == Window.windowID)
                        {
                            Window.SetSize(@event.Window.Data1, @event.Window.Data2);
                            Renderer.Resize(Window.Width, Window.Height);
                        }
                        break;
                    }

                case SDL.EventType.WindowCloseRequested:
                    {
                        if (@event.Window.WindowID == Window.windowID)
                        {
                            _logger.Information("Window close requested");
                            return SDL.AppResult.Success;

                        }
                        break;
                    }

                case SDL.EventType.Quit:
                    {
                        _logger.Information("SDL quit requested");
                        return SDL.AppResult.Success;
                    }
            }

            return SDL.AppResult.Continue;
        }

        void QuitCallback(nint appstate, SDL.AppResult result)
        {
            OnShutdown();
            Renderer.Dispose();
            Window.Dispose();
        }

        protected virtual void OnInit() { }
        protected virtual void Update(float deltaTime) { }
        protected virtual void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(System.Drawing.Color.Black);
            Renderer.End();
        }
        protected virtual void OnShutdown() { }
    }

}

