using System;
using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using System.Threading.Tasks;
using Velvet.Graphics;
using Velvet.Windowing;
using Velvet.Input;

namespace Velvet.Tests
{
    public class StressTest : VelvetApplication
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

        /// <summary>
        /// Called once after the window and renderer are initialized.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            // Optionally prepopulate some particles
            for (int i = 0; i < 100; i++)
            {
                AddParticle();
            }
        }

        /// <summary>
        /// Called once per frame to update game logic.
        /// </summary>
        protected override void Update(float deltaTime)
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
                // Uncomment to add bounds handling
                // ClampParticle(i);
            });

            Console.WriteLine($"{1 / deltaTime:F1} FPS : {particles.Count}");
        }

        /// <summary>
        /// Called once per frame to draw everything.
        /// </summary>
        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(Color.Black);

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

        /// <summary>
        /// Called when the application is shutting down.
        /// </summary>
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

        private void ClampParticle(int i)
        {
            Vector2 pos = particles[i];
            Vector2 vel = velocities[i];

            if (pos.X < 0) { pos.X = 0; vel.X = -vel.X; }
            if (pos.X > Window.Width) { pos.X = Window.Width; vel.X = -vel.X; }
            if (pos.Y < 0) { pos.Y = 0; vel.Y = -vel.Y; }
            if (pos.Y > Window.Height) { pos.Y = Window.Height; vel.Y = -vel.Y; }

            particles[i] = pos;
            velocities[i] = vel;
        }
    }
}