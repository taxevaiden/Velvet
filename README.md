# Velvet

heavily unfinished framework for .net 10

you can make games, tools, whatever

there are better frameworks/libraries out there that can do a lot more than this library can, like [Bliss](https://github.com/MrScautHD/Bliss). this was only made for fun and to learn more about how graphics and rendering work.

getting a window open is as easy as doing:

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
            test.Run(args.Length, args);
        }
    }
}
```

and you have a window!

`Window` and `Renderer` will be exposed to the `Game` class you create, so you can do anything you want!

```csharp
// Game.cs

protected override void OnInit()
{
    base.OnInit();
    stopwatch = new();
    usagi = new VelvetTexture(Renderer, "assets/usagi.jpg");
    testShader = new VelvetShader(Renderer, "assets/shaders/shader.vert", null, [ new UniformDescription("time", UniformType.Float, UniformStage.Vertex) ]);
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
    Renderer.DrawRectangle(new Vector2(50.0f, 50.0f), new Vector2(Window.Width - 100.0f, Window.Height - 100.0f), Color.White);
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
    vec2 new = vec2(Position.x, Position.y + sin(Position.x * 100.0f + hehe) * 0.1f);
    gl_Position = vec4(new, 0.0, 1.0);
    fsin_UV = UV;
    fsin_Color = Color;
}
```

there are also many examples within Velvet.Tests

## things i'm done with

- basic window
- drawing rectangles, circles, any polygon you could think of, with color!
- shaders
  - uniforms supported
- render textures
  - multisampling supported (however, drawing a VelvetTexture to a multisampled VelvetRenderTexture does not do anything on the OpenGL backend. the Vulkan backend also doesn't render the multisampled VelvetRenderTexture smoothly for some reason. D3D11 has both of these working)
  - OpenGL behaves differently from all the other GraphicsAPIs when drawing to a VelvetRenderTexture. all texture samples are flipped across the x-axis. VelvetTextures will still work fine on OpenGL and behave like all the other GraphicsAPIs, however VelvetRenderTextures don't. **in order to compensate, Velvet flips the UVs of anything drawn across the x-axis when using OpenGL to render a VelvetRenderTexture. keep this in mind when writing shader code!**
- textures
- input from keyboard and mouse
- support for all four major graphics APIs (if OpenGL counts)
  - D3D11
  - Vulkan
  - Metal
  - OpenGL

![Stress Test](assets/image.png)

Stress Test, ~60 FPS with ~100,000 rectangles

## how to test

first, clone the repo:

    git clone https://github.com/taxevaiden/Velvet.git

cd into the cloned repo and do

    dotnet restore

it should just work,,

    dotnet run
