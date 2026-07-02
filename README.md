# Boist

As in *boisterous*. Gracefully rough.

A suite of libraries for .NET 10 inspired by [raylib](https://github.com/raysan5/raylib) and [LOVE2D](https://github.com/love2d/love).

***This project is under active development and may introduce breaking changes!***

---

## Getting started

The examples below use the abstract `Application` class, which manages a `Window`, `Renderer` and the `Manager` for you. You don't need to use it of course, as you are able to manage your resources. Just provided for convenience :)

Getting a window open is as easy as doing...

```csharp
// Game.cs
namespace MyGame
{
    class Game : Application
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

...and you have a window! `Window` and `Renderer` are exposed to whatever class you derive from `Application`, so you can do whatever you want with them.

> [!IMPORTANT]
> `Boist.Windowing` depends on `SDL3-CS`, which requires native runtime libraries to function.  
> To install these libraries, simply install the package for your platform:  
>
> | Platform | Package         |
> | -        | -               |
> | Windows  | SDL3-CS.Windows |
> | macOS    | SDL3-CS.MacOS   |
> | Linux    | SDL3-CS.Linux   |
>
> You can also insert this into your .csproj if you're building an application for multiple platforms:
> ```xml
> <ItemGroup Condition="$([System.OperatingSystem]::IsWindows())">
>   <PackageReference Include="SDL3-CS.Windows" Version="3.4.10.5" />
> </ItemGroup>
>
> <ItemGroup Condition="$([System.OperatingSystem]::IsLinux())">
>   <PackageReference Include="SDL3-CS.Linux" Version="3.4.10.5" />
> </ItemGroup>
>
> <ItemGroup Condition="$([System.OperatingSystem]::IsMacOS())">
>   <PackageReference Include="SDL3-CS.MacOS" Version="3.4.10.5" />
> </ItemGroup>
> ```

Here's a more complete example using a texture, a custom shader, and uniforms:

```csharp
// Game.cs
protected override void OnInit()
{
    stopwatch = new();
    usagi = new Texture(Renderer, "assets/image.png");
    testShader = new Shader(
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

More examples can be found in `Boist.Tests`.

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

- (OpenGL) Drawing a rectangle (that has a `Texture` applied to it) on a multisampled `RenderTexture` results in the rectangle being invisible
- (Kinda fixed I haven't found a better solution for this) OpenGL has its texture coordinate origin in the bottom-left, whereas D3D11, Vulkan, and Metal have it in the top-left. This results in `RenderTexture`s being flipped across the X-axis, so to compensate, **Boist flips the UVs of any rectangle rendered with a `RenderTexture` applied.** Keep this in mind when writing custom shader code!

## Running a test locally

```sh
git clone https://github.com/taxevaiden/Boist.git
cd Boist
dotnet restore
dotnet run
```

In `Program.cs`, you can change this line to run another test:

```csharp
var test = new ShapeTest(resolvedAPI); // <-- You can change this to the other tests available
```
