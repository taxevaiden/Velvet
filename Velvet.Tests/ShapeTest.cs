using System.Diagnostics;
using System.Drawing;
using System.Numerics;

using Velvet.Graphics;
using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;
using Velvet.Input;
using Velvet.Windowing;

namespace Velvet.Tests
{
    /// <summary>
    /// A test application for drawing with VelvetRenderer, along with input handling
    /// </summary>
    class ShapeTest : VelvetApplication
    {
        float rot;
        Vector2 pos;
        Vector2 vel;
        Vector2 pos2;
        VelvetTexture usagi;
        VelvetRenderTexture renderTexture;
        VelvetShader shader;
        VelvetFont font;
        float fps = 60;
        List<float> fpsSamples = new();
        public ShapeTest(GraphicsAPI graphicsAPI, int width = 1280, int height = 720, string title = "Hello, world!")
            : base(width, height, title, graphicsAPI) { }

        protected override void OnInit()
        {
            base.OnInit();
            usagi = new VelvetTexture(Renderer, "assets/image.png");

            // A render texture can be rendered to, and then drawn on-screen with a shader applied. This allows for post-processing effects.
            renderTexture = new VelvetRenderTexture(Renderer, 1280, 720, SampleCount.Count1);

            shader = new VelvetShader(
                Renderer,
                null, // No vertex shader, we'll use the default one
                "assets/shaders/tax-dither.frag", // A simple shader
                [
                    new UniformDescription("Resolution", UniformType.Vector2)
                ]
            );

            shader.Set("Resolution", new Vector2(1280, 720)); // Set the "Resolution" uniform to the size of the render texture
            shader.Flush(); // Each time you set a uniform, it is marked as dirty and won't be sent to the GPU until Flush is called.

            font = new VelvetFont(Renderer, "assets/sans.ttf", 16);

            rot = 22.5f;
            pos = Vector2.Zero;
            vel = Vector2.Zero;
            pos2 = Vector2.UnitX * 1260.0f;
        }

        protected override void Update()
        {
            // Basic movement
            if (Input.IsKeyDown(KeyCode.W)) { pos -= Vector2.UnitY * 500f * DeltaTime; }
            if (Input.IsKeyDown(KeyCode.S)) { pos += Vector2.UnitY * 500f * DeltaTime; }
            if (Input.IsKeyDown(KeyCode.A)) { pos -= Vector2.UnitX * 500f * DeltaTime; }
            if (Input.IsKeyDown(KeyCode.D)) { pos += Vector2.UnitX * 500f * DeltaTime; }

            // Rotate with mouse buttons
            if (Input.IsMouseButtonDown(MouseButton.Left)) { rot -= 500f * DeltaTime; } // Rotates counter-clockwise
            if (Input.IsMouseButtonDown(MouseButton.Middle)) { rot += 1000f * DeltaTime; } // Rotates clockwise but faster
            if (Input.IsMouseButtonDown(MouseButton.Right)) { rot += 500f * DeltaTime; } // Rotates clockwise

            if (Input.IsMouseButtonDown(MouseButton.Side1)) { rot += 250f * DeltaTime; } // Rotates clockwise but slower
            if (Input.IsMouseButtonDown(MouseButton.Side2)) { rot -= 250f * DeltaTime; } // Rotates counter-clockwise but slower

            // Scroll wheel adds to velocity
            Input.GetScrollDelta(out float x, out float y);
            vel += new Vector2(-x, -y);
            vel *= MathF.Max(0, 1 - DeltaTime * 5);
            pos2 += vel;

            fps = 1.0f / DeltaTime;
            fpsSamples.Add(fps);

            while (fpsSamples.Count > fps*2)
                fpsSamples.RemoveAt(0);
        }

        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.SetRenderTarget(renderTexture); // Set the render target to the off-screen render texture
            Renderer.ClearColor(Color.White); // Clears the RENDER TEXTURE to white

            // Plain textured
            Renderer.ApplyTexture();
            Renderer.DrawRectangle(new Vector2(50.0f, 50.0f), new Vector2(100.0f, 100.0f), rot * VelvetRenderer.DEG2RAD, AnchorPosition.Center, Color.Red);
            Renderer.DrawRectangle(new Vector2(50.0f, 200.0f), new Vector2(100.0f, 100.0f), Color.Black);
            Renderer.DrawCircle(new Vector2(800.0f, 360.0f), 200.0f, Color.Teal);

            // Usagi textured
            Renderer.ApplyTexture(usagi);
            Renderer.DrawRectangle(new Vector2(200.0f, 50.0f), new Vector2(100.0f, 100.0f), -rot * VelvetRenderer.DEG2RAD, AnchorPosition.Top, Color.Red);
            Renderer.DrawRectangle(new Vector2(50.0f, 350.0f), new Vector2(300.0f, 300.0f), Color.White);
            Renderer.DrawCircle(new Vector2(250.0f, 250.0f), 50.0f, 32, Color.Green);

            // Plain textured
            Renderer.ApplyTexture();

            Input.GetMousePosition(out float x, out float y);
            Renderer.DrawCircle(new Vector2(x, y), 10.0f, Color.Blue);
            Renderer.DrawCircle(pos, 20.0f, Color.Blue);
            Renderer.DrawRectangle(pos2, new Vector2(20.0f, 20.0f), Color.Blue);

            Renderer.SetRenderTargetToScreen(); // Set the render target back to the screen
            Renderer.ClearColor(Color.White); // Clear the SCREEN to white

            Renderer.ApplyShader(shader);
            Renderer.ApplyTexture(renderTexture.Texture);
            Renderer.DrawRectangle(new Vector2(0, 0), new Vector2(1280, 720), Color.White); // Draw the render texture with a shader applied
            Renderer.ApplyShader(); // Reset the shader to the default shader
            Renderer.DrawText(font, $"FPS: {fps:F3} ", 16, new Vector2(16, 16), Color.Black);
            Renderer.DrawText(font, $"Average FPS: {fpsSamples.Average():F3}", 16, new Vector2(16, 32), Color.Black);

            Renderer.End();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            font.Dispose();
            usagi.Dispose();
            renderTexture.Dispose();
            shader.Dispose();
        }
    }
}
