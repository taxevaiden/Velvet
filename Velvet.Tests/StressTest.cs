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

        public override void Run(RendererAPI rendererAPI)
        {
            var win = new VelvetWindow("Stress test", 1600, 900);
            var renderer = new Renderer(rendererAPI, win);
            var random = new Random();

            while (win.Running)
            {
                win.PollEvents();

                for (int i = 0; i < 200; i++)
                {
                    particles.Add(new Vector2(random.Next() % 1600, random.Next() % 900));
                    velocities.Add(new Vector2(random.Next() % 2000 - 1000, random.Next() % 2000 - 1000) / 100.0f);
                }

                // for (int i = 0; i < particles.Count(); i++)
                // {
                //     particles[i] += velocities[i];
                //     velocities[i] *= 0.99f;
                // }

                Parallel.For(0, particles.Count, i =>
                {
                    particles[i] += velocities[i];
                    // if (particles[i].X < 0) { particles[i] = new Vector2(0.0f, particles[i].Y); velocities[i] = new Vector2(-velocities[i].X, velocities[i].Y); };
                    // if (particles[i].X > 1600) { particles[i] = new Vector2(1600.0f, particles[i].Y); velocities[i] = new Vector2(-velocities[i].X, velocities[i].Y); };
                    // if (particles[i].Y < 0) { particles[i] = new Vector2(particles[i].X, 0.0f); velocities[i] = new Vector2(velocities[i].X, -velocities[i].Y); };
                    // if (particles[i].Y > 900) { particles[i] = new Vector2(particles[i].X, 900.0f); velocities[i] = new Vector2(velocities[i].X, -velocities[i].Y); };

                    //velocities[i] *= 0.99f;
                });

                renderer.Begin();
                renderer.ClearColor(Color.Black);

                for (int i = 0; i < particles.Count; i++)
                {
                    renderer.DrawRectangle(particles[i], Vector2.One * 20.0f, 0.0f, Color.FromArgb(255, i * 325 % 255, i * 412 % 255, i * 176 % 255));
                }

                Console.WriteLine($"{1 / win.DeltaTime} : {particles.Count()}");

                renderer.End();
            }

            renderer.Dispose();
            win.Dispose();
        }
    }
}