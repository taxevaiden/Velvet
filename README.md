# Velvet

Heavily unfinished suite of libraries for .NET 10 inspired by [raylib](https://github.com/raysan5/raylib) and [LOVE2D](https://github.com/love2d/love)

---

## Getting started

The examples below use the abstract `VelvetApplication` class, which manages a `VelvetWindow`, `VelvetRenderer` and the `InputManager` for you. You don't need to use it of course, as you can easily manage your resources. Just provided for convenience :)

Getting a window open is as easy as doing...

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

...and you have a window! `Window` and `Renderer` are exposed to whatever class you derive from `VelvetApplication`, so you can do whatever you want with them.

Here's a more complete example using a texture, a custom shader, and uniforms:

```csharp
// Game.cs
protected override void OnInit()
{
    base.OnInit();
    stopwatch = new();
    usagi = new VelvetTexture(Renderer, "assets/usagi.jpg");
    testShader = new VelvetShader(
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

More examples can be found in `Velvet.Tests`.

---

## What's done?

- Basic windowing
- Drawing rectangles, circles, arbitrary polygons, and text with color
- Textures
- Shaders with uniform support
- Render textures
  - Multisampling supported
- Keyboard and mouse input
- All four major graphics APIs
  - D3D11
  - Vulkan
  - Metal
  - OpenGL
- Audio (that can be 3D)

## Known issues

- (OpenGL) Drawing a rectangle (that has a `VelvetTexture` applied to it) on a multisampled `VelvetRenderTexture` results in the rectangle being invisible
- (Kinda fixed I haven't found a better solution for this) OpenGL has its texture coordinate origin in the bottom-left, whereas D3D11, Vulkan, and Metal have it in the top-left. This results in `VelvetRenderTexture`s being flipped across the X-axis, so to compensate, **Velvet flips the UVs of any rectangle rendered with a `VelvetRenderTexture` applied.** Keep this in mind when writing custom shader code!

---

![Stress Test](assets/image.png)

*~60 FPS with ~100,000 rectangles, Apple M1*

---

## Running a test locally

```sh
git clone https://github.com/taxevaiden/Velvet.git
cd Velvet
dotnet restore
dotnet run
```

In `Program.cs`, you can change this line to run another test:

```csharp
var test = new ShapeTest(resolvedAPI); // <-- You can change this to the other tests available
```
