using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

using Velvet.Graphics;
using Velvet.Input;
using Velvet.Windowing;

namespace Velvet.Tests
{
    /// <summary>
    /// A stress test application for Velvet, designed to test the performance of the renderer under heavy load by simulating a large number of moving particles on the screen.
    /// </summary>
    class StressTest : VelvetApplication
    {
        private List<Vector2> particles;
        private List<Vector2> velocities;
        private Random random;

        public StressTest(GraphicsAPI graphicsAPI, int width = 1600, int height = 900, string title = "Stress Test")
            : base(width, height, title, graphicsAPI)
        {
            particles = new List<Vector2>();
            velocities = new List<Vector2>();
            random = new Random();
        }

        protected override void OnInit()
        {
            base.OnInit();
            // Prepopulate some particles
            for (int i = 0; i < 100; i++)
            {
                AddParticle();
            }
        }

        protected override void Update()
        {
            // Add new particles
            for (int i = 0; i < 25; i++)
            {
                AddParticle();
            }

            // Move particles in parallel
            Parallel.For(0, particles.Count, i =>
            {
                particles[i] += velocities[i];
                // Bounds handling
                ClampParticle(i);
            });

            Console.WriteLine($"{1 / DeltaTime:F1} FPS : {particles.Count}");
        }

        private void ClampParticle(int i)
        {
            if (particles[i].X < 0 || particles[i].X > Window.Width)
            {
                velocities[i] = new Vector2(-velocities[i].X, velocities[i].Y);
                particles[i] = new Vector2(Math.Clamp(particles[i].X, 0, Window.Width), particles[i].Y);
            }
            if (particles[i].Y < 0 || particles[i].Y > Window.Height)
            {
                velocities[i] = new Vector2(velocities[i].X, -velocities[i].Y);
                particles[i] = new Vector2(particles[i].X, Math.Clamp(particles[i].Y, 0, Window.Height));
            }
        }

        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(Color.Black);

            // Draw particles
            for (int i = 0; i < particles.Count; i++)
            {
                Renderer.DrawRectangle(
                    particles[i],
                    Vector2.One * 20.0f,
                    0.0f,
                    Color.FromArgb(255, i * 325 % 255, i * 412 % 255, i * 176 % 255)
                );
            }

            Renderer.End();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            particles.Clear();
            velocities.Clear();
        }

        private void AddParticle()
        {
            particles.Add(new Vector2(random.Next(0, Window.Width), random.Next(0, Window.Height)));
            velocities.Add(new Vector2((float)(random.NextDouble() * 20.0 - 10.0),
                                       (float)(random.NextDouble() * 20.0 - 10.0)));
        }
    }
}