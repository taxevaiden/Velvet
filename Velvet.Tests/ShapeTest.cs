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
    /// A test application for drawing shapes with Velvet's renderer, demonstrating rectangles, circles, textures, shaders, and text rendering.
    /// This test also demonstrates basic input handling for moving shapes and rotating them.
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
        public ShapeTest(GraphicsAPI graphicsAPI, int width = 1600, int height = 900, string title = "Hello, world!")
            : base(width, height, title, graphicsAPI) { }

        protected override void OnInit()
        {
            base.OnInit();
            usagi = new VelvetTexture(Renderer, "assets/usagi.jpg");

            // A render texture can be used to draw off-screen, then drawing the render texture with a shader. This allows for post-processing effects.
            renderTexture = new VelvetRenderTexture(Renderer, 1600, 900, SampleCount.Count1);

            shader = new VelvetShader(
                Renderer, 
                null, // No vertex shader, we'll use the default one
                "assets/shaders/jpeg.frag", // A simple shader
                [
                    new UniformDescription("Resolution", UniformType.Vector2, UniformStage.Fragment)
                ]
            );
            
            shader.Set("Resolution", new Vector2(1600, 900)); // Set the "Resolution" uniform to the size of the render texture
            shader.Flush(); // Each time you set a uniform, it is marked as dirty and won't be sent to the GPU until Flush is called.

            font = new VelvetFont(Renderer, "assets/sans.ttf", 32);

            rot = 22.5f;
            pos = Vector2.Zero;
            vel = Vector2.Zero;
            pos2 = Vector2.UnitX * 1580.0f;
        }

        protected override void Update()
        {
            // Basic movement
            if (InputManager.IsKeyDown(KeyCode.W)) { pos -= Vector2.UnitY * 500f * DeltaTime; }
            if (InputManager.IsKeyDown(KeyCode.S)) { pos += Vector2.UnitY * 500f * DeltaTime; }
            if (InputManager.IsKeyDown(KeyCode.A)) { pos -= Vector2.UnitX * 500f * DeltaTime; }
            if (InputManager.IsKeyDown(KeyCode.D)) { pos += Vector2.UnitX * 500f * DeltaTime; }

            // Rotate with mouse buttons
            if (InputManager.IsMouseButtonDown(MouseButton.Left)) { rot -= 500f * DeltaTime; } // Rotates counter-clockwise
            if (InputManager.IsMouseButtonDown(MouseButton.Middle)) { rot += 1000f * DeltaTime; } // Rotates clockwise but faster
            if (InputManager.IsMouseButtonDown(MouseButton.Right)) { rot += 500f * DeltaTime; } // Rotates clockwise

            if (InputManager.IsMouseButtonDown(MouseButton.Side1)) { rot += 250f * DeltaTime; } // Rotates clockwise but slower
            if (InputManager.IsMouseButtonDown(MouseButton.Side2)) { rot -= 250f * DeltaTime; } // Rotates counter-clockwise but slower

            // Scroll wheel adds to velocity
            InputManager.GetScrollDelta(out float x, out float y);
            vel += new Vector2(-x, -y);
            vel *= MathF.Max(0, 1 - DeltaTime * 5);
            pos2 += vel;

            fps = 1.0f / DeltaTime;
        }

        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.SetRenderTarget(renderTexture); // Set the render target to the off-screen render texture
            Renderer.ClearColor(Color.White);

            // Plain textured
            Renderer.ApplyTexture();
            Renderer.DrawRectangle(new Vector2(50.0f, 50.0f), new Vector2(200.0f, 200.0f), rot * (MathF.PI / 180.0f), AnchorPosition.Center, Color.Red);
            Renderer.DrawRectangle(new Vector2(50.0f, 350.0f), new Vector2(200.0f, 200.0f), Color.Black);
            Renderer.DrawCircle(new Vector2(800.0f, 450.0f), 200.0f, Color.Teal);
            
            // Usagi textured
            Renderer.ApplyTexture(usagi);
            Renderer.DrawRectangle(new Vector2(350.0f, 50.0f), new Vector2(200.0f, 200.0f), -rot * (MathF.PI / 180.0f), AnchorPosition.Top, Color.Red);
            Renderer.DrawRectangle(new Vector2(50.0f, 650.0f), new Vector2(200.0f, 200.0f), Color.White);
            Renderer.DrawCircle(new Vector2(450.0f, 750.0f), 100.0f, 32, Color.Green);

            // Plain textured
            Renderer.ApplyTexture();
            Renderer.DrawRectangle(new Vector2(350.0f, 350.0f), new Vector2(200.0f, 200.0f), Color.Lavender);

            InputManager.GetMousePosition(out float x, out float y);
            Renderer.DrawCircle(new Vector2(x, y), 10.0f, Color.Blue);
            Renderer.DrawCircle(pos, 20.0f, Color.Blue);
            Renderer.DrawRectangle(pos2, new Vector2(20.0f, 20.0f), Color.Blue);

            Renderer.DrawText(font, $"FPS: {fps:F3}", 32, new Vector2(50, 50), Color.Black);

            Renderer.SetRenderTargetToScreen(); // Set the render target back to the screen so we can draw the render texture with the shader
            Renderer.ClearColor(Color.White);

            Renderer.ApplyShader(shader);
            Renderer.ApplyTexture(renderTexture.Texture);
            Renderer.DrawRectangle(new Vector2(0, 0), new Vector2(1600, 900), Color.White);

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