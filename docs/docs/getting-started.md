# Getting started

Getting a window open is as easy as doing:

```csharp
// Game.cs
namespace MyGame
{
    class Game : VelvetApplication
    {
        public Game(int width = 1600, int height = 900, string title = "Hello, world!")
            : base(width, height, title)
        {}
    }
}
```

```csharp
// Program.cs
namespace MyGame
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            game.Run(args.Length, args);
        }
    }
}
```

And you have a window!  `Window` and `Renderer` are exposed to whatever class you derive from `VelvetApplication`, so you can do whatever you want with them.

Here's a more complete example using a texture, a custom shader, and uniforms:

```csharp
// Game.cs
protected override void OnInit()
{
    base.OnInit();
    stopwatch = new();
    usagi     = new VelvetTexture(Renderer, "assets/usagi.jpg");
    testShader = new VelvetShader(
        Renderer,
        "assets/shaders/shader.vert",
        null,
        [new UniformDescription("time", UniformType.Float, UniformStage.Vertex)]
    );
    testShader.Set("time", 0.0f);
    testShader.Flush();
    stopwatch.Start();
}

protected override void Update(float deltaTime)
{
    testShader.Set("time", stopwatch.ElapsedMilliseconds / 100.0f);
    testShader.Flush();
}

protected override void Draw()
{
    Renderer.Begin();
    Renderer.ClearColor(Color.White);
    Renderer.ApplyTexture(usagi);
    Renderer.ApplyShader(testShader);
    Renderer.DrawRectangle(
        new Vector2(50.0f, 50.0f),
        new Vector2(Window.Width - 100.0f, Window.Height - 100.0f),
        Color.White
    );
    Renderer.End();
}

protected override void OnShutdown()
{
    base.OnShutdown();
    stopwatch.Stop();
    usagi.Dispose();
    testShader.Dispose();
}
```

```glsl
// shader.vert
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 UV;
layout(location = 2) in vec4 Color;

layout(location = 0) out vec2 fsin_UV;
layout(location = 1) out vec4 fsin_Color;

layout(set = 0, binding = 2) uniform Globals
{
    float time;
};

void main()
{
    vec2 pos = vec2(Position.x, Position.y + sin(Position.x * 100.0f + time) * 0.1f);
    gl_Position = vec4(pos, 0.0, 1.0);
    fsin_UV    = UV;
    fsin_Color = Color;
}
```