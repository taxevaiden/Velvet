## Getting started

The examples below use the abstract `BoistApplication` class, which manages a `BoistWindow`, `BoistRenderer` and the `InputManager` for you. You don't need to use it of course, as you can easily manage your resources. Just provided for convenience :)

Getting a window open is as easy as doing...

```csharp
// Game.cs
namespace MyGame
{
    class Game : BoistApplication
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

...and you have a window! `Window` and `Renderer` are exposed to whatever class you derive from `BoistApplication`, so you can do whatever you want with them.

Here's a more complete example using a texture, a custom shader, and uniforms:

```csharp
// Game.cs
protected override void OnInit()
{
    base.OnInit();
    stopwatch = new();
    usagi = new BoistTexture(Renderer, "assets/image.png");
    testShader = new BoistShader(
        Renderer,
        "assets/shaders/shader.vert",
        null,
        [new UniformDescription("time", UniformType.Float)]
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
    mat4x4 Projection;
};

layout(set = 0, binding = 3) uniform Uniforms
{
    float time;
};

void main()
{
    vec2 pos = vec2(Position.x, Position.y + sin(Position.x * 100.0f + time) * 0.1f);
    gl_Position = Projection * vec4(pos, 0.0, 1.0);
    fsin_UV = UV;
    fsin_Color = Color;
}
```