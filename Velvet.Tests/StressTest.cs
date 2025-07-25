using System.Drawing;
using System.Numerics;
using Velvet.Graphics;
using Velvet.Input;

namespace Velvet.Tests
{
    class StressTest : BaseTest
    {
        List<Vector2> particles;
        List<Vector2> velocities;
        public StressTest()
        {
            particles = [];
            velocities = [];
        }

        public new void Run()
        {
            var win = new VelvetWindow("Stress test", 1600, 900);
            var renderer = new Renderer(RendererAPI.Vulkan, win);
            var random = new Random();

            while (win.IsRunning())
            {
                win.PollEvents();

                if (InputManager.IsMouseButtonDown(MouseButton.Left))
                {
                    for (int i = 0; i < 20; i++)
                    {
                        particles.Add(InputManager.GetMousePosition());
                        velocities.Add(new Vector2(random.Next() % 200 - 100, random.Next() % 200 - 100) / 100.0f);
                    }
                }

                for (int i = 0; i < particles.Count(); i++)
                {
                    particles[i] += velocities[i];
                    velocities[i] *= 0.99f;
                }

                renderer.Begin();
                renderer.ClearColor(Color.Black);

                for (int i = 0; i < particles.Count(); i++)
                {
                    renderer.DrawRectangle(particles[i], Vector2.One * 10.0f, i, Color.FromArgb(255, i * 325 % 255, i * 412 % 255, i * 176 % 255));
                }

                Console.WriteLine($"{1 / win.GetDeltaTime()} : {particles.Count()}");

                renderer.End();
            }

            renderer.Dispose();
            win.Dispose();
        }
    }
}